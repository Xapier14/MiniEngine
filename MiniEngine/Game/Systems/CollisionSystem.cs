using System;
using System.Collections.Generic;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    /// <summary>
    /// Finds overlapping colliders, separates them, and exchanges momentum.
    /// <para>Register at <see cref="Cadence.Fixed"/>, immediately after
    /// <see cref="PhysicsSystem"/>: it resolves the positions physics has just written.</para>
    /// <para>Translation only. <see cref="Transform"/> has no rotation, so there is no
    /// angular velocity, no torque and no rotational inertia to account for.</para>
    /// </summary>
    [HandlesComponent<BoxCollider>]
    [HandlesComponent<CircleCollider>]
    public class CollisionSystem : System
    {
        /// <summary>
        /// Overlap left unresolved. Without it, a body resting under gravity is separated
        /// every step, re-penetrates the next, and visibly buzzes.
        /// </summary>
        public float Slop { get; set; } = 0.01f;

        private readonly List<Body> _bodies = [];
        private readonly HashSet<Collider> _tunnelWarned = [];
        private readonly HashSet<Collider> _massWarned = [];

        /// <summary>
        /// A collider with its siblings resolved once, up front. Looking them up inside the
        /// pairwise loop would be O(n^2) lookups instead of O(n).
        /// </summary>
        private readonly struct Body(
            Collider collider,
            Transform transform,
            Motion? motion,
            PhysicsBehavior? behavior,
            Vector2F min,
            Vector2F max,
            float inverseMass)
        {
            public Collider Collider { get; } = collider;
            public Transform Transform { get; } = transform;
            public Motion? Motion { get; } = motion;
            public Vector2F Min { get; } = min;
            public Vector2F Max { get; } = max;

            /// <summary>Zero for anything immovable, so the resolver can treat it as infinitely heavy.</summary>
            public float InverseMass { get; } = inverseMass;

            public float Restitution { get; } = behavior?.Restitution ?? 0f;
            public bool IsTrigger { get; } = collider.IsTrigger;
        }

        protected override void BeforeStep(object? arg)
            => _bodies.Clear();

        public void HandleComponent(BoxCollider collider)
            => Track(collider);

        public void HandleComponent(CircleCollider collider)
            => Track(collider);

        private void Track(Collider collider)
        {
            var transform = Ecs.GetSibling<Transform>(collider);
            if (transform == null)
                return;

            // No Motion means static. Presence of the component is the flag, rather than a
            // separate one that could disagree with it.
            var motion = Ecs.GetSibling<Motion>(collider);
            var behavior = Ecs.GetSibling<PhysicsBehavior>(collider);

            var inverseMass = 0f;
            if (motion != null)
            {
                var mass = behavior?.Mass ?? 1f;
                if (mass > 0f)
                {
                    inverseMass = 1f / mass;
                }
                else if (_massWarned.Add(collider))
                {
                    LoggingService.Warn(
                        "[Collision] {0} has a mass of {1} and will be treated as immovable. Remove its Motion component to say so directly.",
                        collider.GetType().Name, mass);
                }
            }

            var (min, max) = BoundsOf(collider, transform.Translate);
            _bodies.Add(new Body(collider, transform, motion, behavior, min, max, inverseMass));

            WarnIfTunnelling(collider, motion);
        }

        protected override void AfterStep(object? arg)
        {
            // O(n^2) with an AABB reject. Matches the scale the engine works at; a uniform
            // grid is the upgrade when body counts make this show up in a profile.
            for (var i = 0; i < _bodies.Count; i++)
            {
                for (var j = i + 1; j < _bodies.Count; j++)
                {
                    var a = _bodies[i];
                    var b = _bodies[j];

                    // Two immovable things cannot resolve against each other, and reporting
                    // it every step would be noise about level layout.
                    if (a.InverseMass <= 0f && b.InverseMass <= 0f)
                        continue;

                    if (!BoundsOverlap(a, b))
                        continue;

                    if (!TryCollide(a, b, out var normal, out var penetration))
                        continue;

                    if (!a.IsTrigger && !b.IsTrigger)
                    {
                        Separate(a, b, normal, penetration);
                        ApplyImpulse(a, b, normal);
                    }

                    Report(a, b, normal, penetration);
                }
            }
        }

        private static bool BoundsOverlap(Body a, Body b)
            => a.Min.X <= b.Max.X && a.Max.X >= b.Min.X
            && a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y;

        private static (Vector2F Min, Vector2F Max) BoundsOf(Collider collider, Vector2F origin)
            => collider switch
            {
                BoxCollider box => box.Bounds(origin),
                CircleCollider circle => (
                    circle.Center(origin) - (circle.Radius, circle.Radius),
                    circle.Center(origin) + (circle.Radius, circle.Radius)),
                _ => (origin, origin),
            };

        /// <summary>
        /// Narrowphase. <paramref name="normal"/> points from <paramref name="b"/> towards
        /// <paramref name="a"/>, so moving <paramref name="a"/> along it separates them.
        /// </summary>
        private static bool TryCollide(Body a, Body b, out Vector2F normal, out float penetration)
        {
            normal = Vector2F.Zero;
            penetration = 0f;

            return (a.Collider, b.Collider) switch
            {
                (BoxCollider, BoxCollider) => BoxBox(a, b, out normal, out penetration),
                (CircleCollider ca, CircleCollider cb) =>
                    CircleCircle(ca.Center(a.Transform.Translate), ca.Radius,
                        cb.Center(b.Transform.Translate), cb.Radius, out normal, out penetration),
                (BoxCollider, CircleCollider cb) =>
                    BoxCircle(a, cb.Center(b.Transform.Translate), cb.Radius, out normal, out penetration),
                (CircleCollider ca, BoxCollider) =>
                    // Same test with the roles swapped, so the normal has to be flipped to
                    // keep pointing from b towards a.
                    FlipNormal(BoxCircle(b, ca.Center(a.Transform.Translate), ca.Radius,
                        out normal, out penetration), ref normal),
                _ => false,
            };
        }

        private static bool FlipNormal(bool hit, ref Vector2F normal)
        {
            if (hit)
                normal = Vector2F.Zero - normal;
            return hit;
        }

        private static bool BoxBox(Body a, Body b, out Vector2F normal, out float penetration)
        {
            normal = Vector2F.Zero;
            penetration = 0f;

            var overlapX = MathF.Min(a.Max.X, b.Max.X) - MathF.Max(a.Min.X, b.Min.X);
            var overlapY = MathF.Min(a.Max.Y, b.Max.Y) - MathF.Max(a.Min.Y, b.Min.Y);
            if (overlapX <= 0f || overlapY <= 0f)
                return false;

            var centerA = (a.Min + a.Max) / 2f;
            var centerB = (b.Min + b.Max) / 2f;

            // Push out along whichever axis needs the least movement.
            if (overlapX < overlapY)
            {
                penetration = overlapX;
                normal = centerA.X < centerB.X ? (-1f, 0f) : (1f, 0f);
            }
            else
            {
                penetration = overlapY;
                normal = centerA.Y < centerB.Y ? (0f, -1f) : (0f, 1f);
            }

            return true;
        }

        private static bool CircleCircle(
            Vector2F centerA, float radiusA,
            Vector2F centerB, float radiusB,
            out Vector2F normal, out float penetration)
        {
            normal = Vector2F.Zero;
            penetration = 0f;

            var offset = centerA - centerB;
            var distance = offset.Magnitude;
            var touching = radiusA + radiusB;
            if (distance >= touching)
                return false;

            penetration = touching - distance;
            // Exactly concentric has no separating direction; pick one so they still part.
            normal = distance > 0f ? offset / distance : (1f, 0f);
            return true;
        }

        private static bool BoxCircle(
            Body box, Vector2F center, float radius,
            out Vector2F normal, out float penetration)
        {
            normal = Vector2F.Zero;
            penetration = 0f;

            var closest = Vector2F.Clamp(center, box.Min, box.Max);
            var offset = center - closest;
            var distance = offset.Magnitude;

            if (distance > 0f)
            {
                if (distance >= radius)
                    return false;
                penetration = radius - distance;
                // From the circle towards the box, which is b towards a.
                normal = (Vector2F.Zero - offset) / distance;
                return true;
            }

            // Centre is inside the box: leave by the nearest face.
            var toLeft = center.X - box.Min.X;
            var toRight = box.Max.X - center.X;
            var toTop = center.Y - box.Min.Y;
            var toBottom = box.Max.Y - center.Y;
            var nearest = MathF.Min(MathF.Min(toLeft, toRight), MathF.Min(toTop, toBottom));

            if (nearest == toLeft) normal = (1f, 0f);
            else if (nearest == toRight) normal = (-1f, 0f);
            else if (nearest == toTop) normal = (0f, 1f);
            else normal = (0f, -1f);

            penetration = radius + nearest;
            return true;
        }

        /// <summary>
        /// Moves the pair apart along the normal, sharing the correction in proportion to
        /// inverse mass so a light body climbs out of a heavy one rather than shoving it.
        /// </summary>
        private void Separate(Body a, Body b, Vector2F normal, float penetration)
        {
            var inverseSum = a.InverseMass + b.InverseMass;
            if (inverseSum <= 0f)
                return;

            var correction = MathF.Max(penetration - Slop, 0f) / inverseSum;
            if (correction <= 0f)
                return;

            a.Transform.Translate += normal * (correction * a.InverseMass);
            b.Transform.Translate -= normal * (correction * b.InverseMass);
        }

        private static void ApplyImpulse(Body a, Body b, Vector2F normal)
        {
            var inverseSum = a.InverseMass + b.InverseMass;
            if (inverseSum <= 0f)
                return;

            var velocityA = a.Motion?.Velocity ?? Vector2F.Zero;
            var velocityB = b.Motion?.Velocity ?? Vector2F.Zero;
            var approach = (velocityA - velocityB).Dot(normal);

            // Already moving apart. Resolving again would add energy from nothing.
            if (approach >= 0f)
                return;

            // Bouncier of the two wins, as Box2D does. Taking the lower would mean any
            // surface without a PhysicsBehavior -- which is all static geometry by default
            // -- silently killed every bounce, making restitution unusable until every wall
            // was annotated.
            var restitution = MathF.Max(a.Restitution, b.Restitution);
            var impulse = normal * (-(1f + restitution) * approach / inverseSum);

            if (a.Motion != null)
                a.Motion.Velocity += impulse * a.InverseMass;
            if (b.Motion != null)
                b.Motion.Velocity -= impulse * b.InverseMass;
        }

        private void Report(Body a, Body b, Vector2F normal, float penetration)
        {
            a.Collider.RaiseCollided(new CollisionInfo(
                a.Collider, b.Collider, Ecs.GetOwner(b.Collider), normal, penetration));
            b.Collider.RaiseCollided(new CollisionInfo(
                b.Collider, a.Collider, Ecs.GetOwner(a.Collider), Vector2F.Zero - normal, penetration));
        }

        /// <summary>
        /// A fixed step bounds how far a body moves per step but does not stop it skipping
        /// over something thinner than that. Say so once, with the numbers, rather than
        /// leaving it to be found as an intermittent bug.
        /// </summary>
        private void WarnIfTunnelling(Collider collider, Motion? motion)
        {
            if (motion == null)
                return;

            var travel = motion.Velocity.Magnitude * DeltaTimeF;
            var extent = collider.SmallestExtent;
            if (extent <= 0f || travel <= extent || !_tunnelWarned.Add(collider))
                return;

            LoggingService.Warn(
                "[Collision] {0} moves {1:F2} per step but is only {2:F2} across, so it can pass through thin geometry. Cap its speed near {3:F2} or raise the tick rate.",
                collider.GetType().Name, travel, extent, extent / MathF.Max(DeltaTimeF, 0.0001f));
        }
    }
}
