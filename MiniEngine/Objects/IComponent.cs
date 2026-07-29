namespace MiniEngine
{
    /// <summary>
    /// Marker for component types. Carries no members: a component is inert data and does
    /// not know which entity holds it. Ask <see cref="EcsManager.GetOwner"/> or
    /// <see cref="EcsManager.GetSibling{T}"/> for that.
    /// </summary>
    /// <remarks>
    /// Exists so component interfaces (for example <c>ICollider</c>) satisfy the
    /// <c>where T : IComponent</c> constraints on the component query methods, which a
    /// <c>where T : Component</c> constraint could not express.
    /// </remarks>
    public interface IComponent
    {
    }
}
