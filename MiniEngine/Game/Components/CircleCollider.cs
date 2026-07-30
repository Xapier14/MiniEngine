namespace MiniEngine.Components
{
    /// <summary>
    /// A circle, centred on the entity's <see cref="Transform.Translate"/> plus
    /// <see cref="Offset"/>.
    /// <para>No <see cref="Motion"/> sibling means static: it collides, but nothing moves
    /// it.</para>
    /// </summary>
    [RequiresComponent<Transform>]
    public class CircleCollider : Collider
    {
        public float Radius { get; set; } = 8f;

        /// <summary>
        /// Displacement of the circle's centre from the entity's position. A box expresses
        /// the same thing through asymmetric extents; a circle needs it explicitly.
        /// </summary>
        public Vector2F Offset = Vector2F.Zero;

        /// <summary>The circle's centre in world space, given the entity's position.</summary>
        public Vector2F Center(Vector2F origin)
            => origin + Offset;

        internal override float SmallestExtent => Radius;
    }
}
