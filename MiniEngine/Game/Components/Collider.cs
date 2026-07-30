using System;

namespace MiniEngine.Components
{
    /// <summary>
    /// One contact, reported to both colliders involved. <see cref="Normal"/> always points
    /// away from <see cref="Self"/>, so a handler can use it without working out which side
    /// of the pair it was.
    /// </summary>
    /// <param name="Self">The collider being notified.</param>
    /// <param name="Other">What it touched.</param>
    /// <param name="OtherEntity">The entity <paramref name="Other"/> belongs to, if it is registered.</param>
    /// <param name="Normal">Unit vector pointing from <paramref name="Other"/> towards <paramref name="Self"/>.</param>
    /// <param name="Penetration">How deeply the two overlap along <paramref name="Normal"/>.</param>
    public readonly record struct CollisionInfo(
        Collider Self,
        Collider Other,
        Entity? OtherEntity,
        Vector2F Normal,
        float Penetration);

    /// <summary>
    /// Shared base for collision shapes. Exists so the contact event has one implementation
    /// rather than one per shape.
    /// </summary>
    public abstract class Collider : Component, ICollider
    {
        /// <summary>
        /// Reports contacts but is never separated and never changes anyone's velocity.
        /// For volumes that detect rather than block: pickups, checkpoints, damage zones.
        /// </summary>
        public bool IsTrigger { get; set; }

        /// <summary>
        /// Raised once per contact per step. Raised on both colliders involved.
        /// </summary>
        public event Action<CollisionInfo>? Collided;

        internal void RaiseCollided(CollisionInfo info)
            => Collided?.Invoke(info);

        /// <summary>
        /// Smallest distance from the shape's origin to its edge. The collision system uses
        /// it to warn when a body is moving fast enough per step to pass through.
        /// </summary>
        internal abstract float SmallestExtent { get; }
    }
}
