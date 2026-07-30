using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// <para>Declares which component fields replicate.</para>
/// <para>Stated here rather than by an attribute on the components themselves, because
/// networking is an optional package and the engine should not carry markers for it.
/// Registration is also opt-in per field for a practical reason: components hold things
/// that cannot cross a wire at all, such as <c>Scriptable</c>'s delegates,
/// <c>Drawable</c>'s sprite and event, and <c>Animator</c>'s animations. Naming what
/// travels fails safe; naming what does not would need updating every time a component
/// grows a field.</para>
/// </summary>
public static class ReplicationSchema
{
    private static readonly Dictionary<Type, ComponentSchema> _schemas = [];

    /// <summary>Every registered component schema, keyed by component type.</summary>
    public static IReadOnlyDictionary<Type, ComponentSchema> All => _schemas;

    /// <summary>
    /// Begins declaring the replicated fields of <typeparamref name="T"/>. Calling it
    /// again for the same component adds to what is already there.
    /// </summary>
    public static ComponentSchemaBuilder<T> Register<T>() where T : Component
    {
        if (!_schemas.TryGetValue(typeof(T), out var schema))
        {
            schema = new ComponentSchema(typeof(T));
            _schemas.Add(typeof(T), schema);
        }
        return new ComponentSchemaBuilder<T>(schema);
    }

    public static ComponentSchema? For<T>() where T : Component
        => For(typeof(T));

    public static ComponentSchema? For(Type componentType)
        => _schemas.TryGetValue(componentType, out var schema) ? schema : null;

    /// <summary>
    /// Covers every registered schema at once. Exchanged at connect: if the two ends
    /// disagree the builds differ, and no snapshot between them would mean the same thing.
    /// </summary>
    public static uint GlobalHash
    {
        get
        {
            var canonical = new StringBuilder();
            // Sorted by name so the value does not depend on which order the host happened
            // to register components in.
            foreach (var schema in _schemas.Values.OrderBy(s => s.ComponentType.FullName, StringComparer.Ordinal))
                canonical.Append(schema.ComponentType.FullName).Append('=').Append(schema.SchemaHash).Append(';');
            return ComponentSchema.Fnv1A(canonical.ToString());
        }
    }

    /// <summary>
    /// Forgets every registration. For tests that need to build a schema set from scratch.
    /// </summary>
    public static void Clear()
        => _schemas.Clear();
}

/// <summary>
/// Fluent declaration of one component's replicated fields. Field order is the order these
/// calls are made in, and that order is part of the wire format.
/// </summary>
public sealed class ComponentSchemaBuilder<TComponent>(ComponentSchema schema)
    where TComponent : Component
{
    public ComponentSchema Schema => schema;

    /// <summary>
    /// Adds a field or settable property to the wire format.
    /// </summary>
    /// <param name="selector">
    /// A direct member access, such as <c>t =&gt; t.Translate</c>. Anything else is
    /// rejected: there would be nothing to assign back to when reading.
    /// </param>
    /// <param name="quantize">
    /// Rounds floating point components to this step before sending, trading precision for
    /// bytes once delta compression arrives. Zero, the default, sends full precision.
    /// </param>
    public ComponentSchemaBuilder<TComponent> Field<TField>(
        Expression<Func<TComponent, TField>> selector,
        float quantize = 0f)
    {
        var member = UnwrapMember(selector);
        var instance = Expression.Parameter(typeof(TComponent), "component");
        var access = Expression.MakeMemberAccess(instance, member);

        var get = Expression.Lambda<Func<TComponent, TField>>(access, instance).Compile();

        var value = Expression.Parameter(typeof(TField), "value");
        var set = Expression.Lambda<Action<TComponent, TField>>(
            Expression.Assign(access, value), instance, value).Compile();

        schema.AddField(new FieldAccessor<TComponent, TField>(
            member.Name, get, set, FieldCodecs.For<TField>(quantize)));

        return this;
    }

    private static MemberInfo UnwrapMember<TField>(Expression<Func<TComponent, TField>> selector)
    {
        // A value-typed member is sometimes wrapped in a conversion by the compiler.
        var body = selector.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert
            ? convert.Operand
            : selector.Body;

        if (body is not MemberExpression member)
            throw new ArgumentException(
                "A replicated field must be a direct member access, such as t => t.Translate.",
                nameof(selector));

        if (member.Member is PropertyInfo { CanWrite: false })
            throw new ArgumentException(
                $"{member.Member.Name} has no setter, so it could not be restored when read.",
                nameof(selector));

        return member.Member;
    }
}
