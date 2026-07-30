using System;

namespace MiniEngine.Components
{
    /// <summary>
    /// An axis-aligned box. The four values are distances from the entity's
    /// <see cref="Transform.Translate"/> out to each edge, not edge coordinates, so a box
    /// can sit off-centre from its origin.
    /// <para>No <see cref="Motion"/> sibling means static: it collides, but nothing moves
    /// it.</para>
    /// </summary>
    [RequiresComponent<Transform>]
    public class BoxCollider : Collider
    {
        public float Top { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        /// <summary>
        /// The box in world space, given the owning entity's position. The one place the
        /// extent convention is written down, so the collision system and the debug overlay
        /// cannot drift apart.
        /// </summary>
        public (Vector2F Min, Vector2F Max) Bounds(Vector2F origin)
            => ((origin.X - Left, origin.Y - Top), (origin.X + Right, origin.Y + Bottom));

        internal override float SmallestExtent
            => MathF.Min(MathF.Min(Left, Right), MathF.Min(Top, Bottom));
    }
}
