using MiniEngine.Utility;

namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// <para>The agreement between a server and a client about what an entity type id means.</para>
/// <para>The server owns the numbering and sends the table at connect; the client resolves
/// each name against its own build. Sending names rather than assuming a shared order
/// means a client built from stale sources fails loudly at connect, instead of spawning
/// the wrong entity halfway through a session.</para>
/// </summary>
public sealed class NetworkTypeTable
{
    private readonly Dictionary<ushort, Type> _byId = [];
    private readonly Dictionary<Type, ushort> _byType = [];
    private ushort _nextId = 1;

    public IReadOnlyDictionary<ushort, Type> Entries => _byId;

    /// <summary>
    /// Adds an entity type that may be spawned over the network, returning its id. Called
    /// on the server. Registering the same type twice returns the id it already has.
    /// </summary>
    public ushort Register<T>() where T : Entity
        => Register(typeof(T));

    /// <inheritdoc cref="Register{T}"/>
    public ushort Register(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        if (!entityType.IsAssignableTo(typeof(Entity)))
            throw new ArgumentException($"{entityType.Name} is not an Entity.", nameof(entityType));

        if (_byType.TryGetValue(entityType, out var existing))
            return existing;

        var id = _nextId++;
        _byId[id] = entityType;
        _byType[entityType] = id;
        return id;
    }

    public bool TryGetId(Type entityType, out ushort id)
        => _byType.TryGetValue(entityType, out id);

    public bool TryGetType(ushort id, out Type? entityType)
        => _byId.TryGetValue(id, out entityType);

    /// <summary>
    /// The form sent to a connecting client: id to fully qualified type name.
    /// </summary>
    public IReadOnlyDictionary<ushort, string> ToWireFormat()
        => _byId.ToDictionary(entry => entry.Key, entry => FullNameOf(entry.Value));

    /// <summary>
    /// Rebuilds a table from what a server sent, resolving each name against the types
    /// this process actually has.
    /// </summary>
    /// <param name="unresolved">
    /// Names the server knows and this build does not. A non-empty list means the two
    /// sides disagree about the game and the connection should be rejected.
    /// </param>
    public static NetworkTypeTable FromWireFormat(
        IReadOnlyDictionary<ushort, string> wire,
        out IReadOnlyList<string> unresolved)
    {
        var table = new NetworkTypeTable();
        var missing = new List<string>();

        // TypeResolver matches on the short type name and filters by a list of known
        // namespaces, so each namespace has to be registered before its types resolve.
        // SceneBuilder seeds it the same way when resolving entities out of scene XML.
        var resolver = new TypeResolver();
        foreach (var fullName in wire.Values)
        {
            var namespaceName = NamespaceOf(fullName);
            if (namespaceName != null)
                resolver.AddReference(namespaceName);
        }

        foreach (var (id, fullName) in wire)
        {
            var resolved = resolver.ResolveType(ShortNameOf(fullName));
            if (resolved == null || !resolved.IsAssignableTo(typeof(Entity)))
            {
                LoggingService.Error("[NetworkTypeTable] Could not resolve entity type {0} sent by the server.", fullName);
                missing.Add(fullName);
                continue;
            }

            table._byId[id] = resolved;
            table._byType[resolved] = id;
            if (id >= table._nextId)
                table._nextId = (ushort)(id + 1);
        }

        unresolved = missing;
        return table;
    }

    private static string FullNameOf(Type type)
        => type.Namespace != null ? $"{type.Namespace}.{type.Name}" : type.Name;

    private static string? NamespaceOf(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot < 0 ? null : fullName[..lastDot];
    }

    private static string ShortNameOf(string fullName)
    {
        var lastDot = fullName.LastIndexOf('.');
        return lastDot < 0 ? fullName : fullName[(lastDot + 1)..];
    }
}
