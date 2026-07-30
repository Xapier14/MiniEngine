namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// <para>Marks an entity as replicated, and carries the one name for it that both ends of
/// a connection agree on.</para>
/// <para>Identity is a component rather than a field on <see cref="Entity"/> because
/// networking is an optional package: an entity gains a network presence by having one
/// attached, and the engine never has to know the concept exists. It also means the
/// <see cref="ReplicationSystem"/> is handed exactly the set of replicated entities by the
/// ECS, with no scan and no lookup.</para>
/// </summary>
[OnlyOneOfType]
public sealed class NetworkIdentity : Component
{
    /// <summary>
    /// Server-assigned handle for this entity, unique for the life of a session. Zero
    /// means unassigned. Ids are never recycled, so a stale reference cannot be mistaken
    /// for a live entity.
    /// </summary>
    public uint NetworkId { get; internal set; }

    /// <summary>
    /// The client this entity belongs to, or zero for one the server owns outright. Used
    /// later to decide whose input drives it and who may predict it.
    /// </summary>
    public uint OwnerClientId { get; internal set; }
}
