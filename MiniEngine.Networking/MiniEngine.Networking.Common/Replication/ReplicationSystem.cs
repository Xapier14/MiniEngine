using MiniEngine.Utility;

namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// Which end of a connection a <see cref="ReplicationSystem"/> is running on. Stated
/// rather than inferred: a client that quietly minted its own ids would diverge from the
/// server without anything appearing to go wrong.
/// </summary>
public enum ReplicationMode
{
    /// <summary>Owns the simulation and hands out network ids.</summary>
    Server,

    /// <summary>Applies what the server sends; every id arrives already assigned.</summary>
    Client,
}

/// <summary>
/// Tracks which entities are replicated and what each one is called on the wire.
/// <para>The ECS hands this system the <see cref="NetworkIdentity"/> component list
/// directly, so "every replicated entity" costs nothing to enumerate. Allocation and
/// teardown hang off the ECS's own registration events, which fire at exactly the points
/// an entity joins and leaves the world.</para>
/// </summary>
[HandlesComponent<NetworkIdentity>]
public class ReplicationSystem : System
{
    /// <summary>An entity that has appeared and needs announcing to clients.</summary>
    public readonly record struct SpawnRecord(uint NetworkId, ushort TypeId, uint OwnerClientId);

    private readonly Dictionary<uint, NetworkIdentity> _byId = [];
    private readonly Queue<SpawnRecord> _spawns = [];
    private readonly Queue<uint> _despawns = [];
    private uint _nextNetworkId = 1;

    public ReplicationMode Mode { get; }
    public NetworkTypeTable TypeTable { get; }

    /// <summary>Every replicated entity currently in the world, by network id.</summary>
    public IReadOnlyDictionary<uint, NetworkIdentity> Replicated => _byId;

    public ReplicationSystem(ReplicationMode mode, NetworkTypeTable? typeTable = null)
    {
        Mode = mode;
        TypeTable = typeTable ?? new NetworkTypeTable();
    }

    /// <summary>
    /// Spawns a replicated entity the server announced.
    /// <para>Exists because the order matters and is easy to get wrong: a component added
    /// after its entity has joined a scene is never registered with the ECS, since
    /// registration happens once, on the <see cref="Scene.EntityAdded"/> event. The
    /// identity therefore has to be attached and filled in before the entity is added.</para>
    /// </summary>
    public Entity? SpawnFromServer(Scene scene, Type entityType, uint networkId, uint ownerClientId = 0)
    {
        if (Mode != ReplicationMode.Client)
        {
            LoggingService.Error("[Replication] SpawnFromServer is for clients; the server spawns entities itself.");
            return null;
        }
        if (networkId == 0)
        {
            LoggingService.Error("[Replication] Refusing to spawn {0} with an unassigned network id.", entityType.Name);
            return null;
        }

        var entity = EntityFactory.TryCreateEntity(entityType);
        if (entity == null)
        {
            LoggingService.Error("[Replication] Could not construct entity type {0}.", entityType.Name);
            return null;
        }

        // An entity type may already declare its own identity, or may leave it to be
        // attached here. Both are fine; adding a second would be rejected and logged.
        var identity = entity.TryGetComponent<NetworkIdentity>();
        if (identity == null)
        {
            entity.AddComponent<NetworkIdentity>();
            identity = entity.GetComponent<NetworkIdentity>();
        }
        identity.NetworkId = networkId;
        identity.OwnerClientId = ownerClientId;

        scene.AddEntity(entity);
        return entity;
    }

    /// <summary>
    /// Looks up a replicated entity's identity by the id the server gave it.
    /// </summary>
    public NetworkIdentity? Find(uint networkId)
        => _byId.TryGetValue(networkId, out var identity) ? identity : null;

    /// <summary>
    /// Takes the spawns accumulated since the last call. Server side.
    /// </summary>
    public IReadOnlyList<SpawnRecord> DrainSpawns()
    {
        if (_spawns.Count == 0)
            return [];
        var drained = _spawns.ToArray();
        _spawns.Clear();
        return drained;
    }

    /// <summary>
    /// Takes the despawns accumulated since the last call. Server side.
    /// </summary>
    public IReadOnlyList<uint> DrainDespawns()
    {
        if (_despawns.Count == 0)
            return [];
        var drained = _despawns.ToArray();
        _despawns.Clear();
        return drained;
    }

    public void OnComponentRegister(NetworkIdentity identity)
    {
        if (Mode == ReplicationMode.Server)
        {
            if (identity.NetworkId == 0)
                identity.NetworkId = _nextNetworkId++;
            QueueSpawn(identity);
        }
        else if (identity.NetworkId == 0)
        {
            // Almost always the ordering trap: the identity was attached after the entity
            // joined the scene, so nothing filled it in. Use SpawnFromServer instead.
            LoggingService.Error(
                "[Replication] A client-side NetworkIdentity was registered with no id and will not replicate.");
            return;
        }

        if (_byId.TryGetValue(identity.NetworkId, out var clash) && !ReferenceEquals(clash, identity))
        {
            LoggingService.Error("[Replication] Network id {0} is already in use by another entity.", identity.NetworkId);
            return;
        }

        _byId[identity.NetworkId] = identity;
    }

    public void OnComponentRemove(NetworkIdentity identity)
    {
        if (identity.NetworkId == 0)
            return;
        if (!_byId.Remove(identity.NetworkId))
            return;

        if (Mode == ReplicationMode.Server)
            _despawns.Enqueue(identity.NetworkId);
    }

    public void HandleComponent(NetworkIdentity identity)
    {
        // Where per-tick snapshot capture goes. Declared now so the ECS keeps the component
        // list for this system maintained, and so the reflection lookup in
        // System.HandleComponents caches a hit rather than retrying every tick.
    }

    private void QueueSpawn(NetworkIdentity identity)
    {
        var owner = Ecs.GetOwner(identity);
        if (owner == null)
        {
            LoggingService.Error("[Replication] Identity {0} has no owning entity to announce.", identity.NetworkId);
            return;
        }

        var entityType = owner.GetType();
        if (!TypeTable.TryGetId(entityType, out var typeId))
        {
            // Registering on demand keeps a server from silently dropping an entity the
            // host forgot to declare. The client is sent the table at connect either way.
            typeId = TypeTable.Register(entityType);
        }

        _spawns.Enqueue(new SpawnRecord(identity.NetworkId, typeId, identity.OwnerClientId));
    }
}
