using System.Text;
using MiniEngine.Networking.Common.Serialization;

namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// One replicated field: how to get it off a component, how to put it back, and how it is
/// encoded. Non-generic so a schema can hold a mixed list of them.
/// </summary>
internal abstract class FieldAccessor
{
    public abstract string Name { get; }
    public abstract string WireTypeName { get; }
    public abstract void Write(Component component, ref NetWriter writer);
    public abstract void Read(Component component, ref NetReader reader);
}

internal sealed class FieldAccessor<TComponent, TField>(
    string name,
    Func<TComponent, TField> get,
    Action<TComponent, TField> set,
    IFieldCodec<TField> codec) : FieldAccessor
    where TComponent : Component
{
    public override string Name => name;
    public override string WireTypeName => codec.WireTypeName;

    public override void Write(Component component, ref NetWriter writer)
        => codec.Write(ref writer, get((TComponent)component));

    public override void Read(Component component, ref NetReader reader)
        => set((TComponent)component, codec.Read(ref reader));
}

/// <summary>
/// <para>The replicated shape of one component type: which fields travel, in what order,
/// in what encoding.</para>
/// <para>Field order is the order they were registered in, not the order reflection
/// happens to report members, which the runtime does not guarantee and which can differ
/// between builds. Getting that wrong shifts every field by one and produces a world that
/// is subtly and silently wrong, so the order is something the author states rather than
/// something the process discovers.</para>
/// </summary>
public sealed class ComponentSchema
{
    private readonly List<FieldAccessor> _fields = [];
    private uint? _hash;

    public Type ComponentType { get; }

    /// <summary>Names of the replicated fields, in wire order.</summary>
    public IReadOnlyList<string> FieldNames => _fields.Select(field => field.Name).ToArray();

    internal ComponentSchema(Type componentType)
        => ComponentType = componentType;

    /// <summary>
    /// Identifies this component's wire layout. Both ends compare it at connect, so a
    /// mismatched build is rejected up front instead of misreading every snapshot.
    /// </summary>
    public uint SchemaHash => _hash ??= ComputeHash();

    internal void AddField(FieldAccessor accessor)
    {
        if (_fields.Any(existing => existing.Name == accessor.Name))
            throw new InvalidOperationException(
                $"{ComponentType.Name}.{accessor.Name} is already registered for replication.");
        _fields.Add(accessor);
        _hash = null;
    }

    /// <summary>Writes every replicated field of <paramref name="component"/>, in order.</summary>
    public void Write(Component component, ref NetWriter writer)
    {
        foreach (var field in _fields)
            field.Write(component, ref writer);
    }

    /// <summary>Reads every replicated field back onto <paramref name="component"/>, in order.</summary>
    public void Read(Component component, ref NetReader reader)
    {
        foreach (var field in _fields)
            field.Read(component, ref reader);
    }

    private uint ComputeHash()
    {
        var canonical = new StringBuilder(ComponentType.FullName ?? ComponentType.Name);
        foreach (var field in _fields)
            canonical.Append(';').Append(field.Name).Append(':').Append(field.WireTypeName);
        return Fnv1A(canonical.ToString());
    }

    /// <summary>
    /// FNV-1a over UTF-8. Deliberately not string.GetHashCode, which is randomised per
    /// process in .NET and so would differ between the two machines being compared.
    /// </summary>
    internal static uint Fnv1A(string value)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;

        var hash = offsetBasis;
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            hash ^= b;
            hash *= prime;
        }
        return hash;
    }
}
