using System;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    /// <summary>
    /// Advances every <see cref="Motion"/> by one step of simulated time.
    /// <para>Semi-implicit Euler: velocity is integrated first and the new velocity is what
    /// moves the position. Register at <see cref="Cadence.Fixed"/> — position accumulates
    /// acceleration, so it is only reproducible when every step is the same length.</para>
    /// </summary>
    [HandlesComponent<Motion>]
    public class PhysicsSystem : System
    {
        private readonly IReadOnlyEngineSetup? _setup;

        /// <summary>
        /// Headless constructor, for a server or a test with no engine present.
        /// </summary>
        public PhysicsSystem() : this(null)
        {
        }

        public PhysicsSystem(IReadOnlyEngineSetup? setup)
        {
            _setup = setup;
            LoggingService.Debug("Physics system initialized");
        }

        public void HandleComponent(Motion motion)
        {
            var transform = Ecs.GetSibling<Transform>(motion);
            if (transform == null)
                return;

            // Optional. Without one, a body has unit mass, no drag and no limits.
            var behavior = Ecs.GetSibling<PhysicsBehavior>(motion);

            // No game speed factor here: it is a property of time itself, applied once by
            // the WorldClock, so a system reapplying it would scale twice.
            var delta = DeltaTimeF;

            var acceleration = behavior == null
                ? motion.Acceleration
                : Limit(Vector2F.Clamp(motion.Acceleration, behavior.AccelerationMin, behavior.AccelerationMax),
                    behavior.AccelerationDirectionalLimit);

            motion.Velocity += acceleration * delta;

            if (behavior is { Resistance: > 0f })
            {
                // Linear damping. Safe only because the step is fixed: at a varying delta
                // this form overshoots into negative velocity for large steps, which is
                // why the clamp at zero is here as well.
                motion.Velocity *= MathF.Max(1f - behavior.Resistance * delta, 0f);
            }

            if (behavior != null)
            {
                motion.Velocity = Limit(
                    Vector2F.Clamp(motion.Velocity, behavior.VelocityMin, behavior.VelocityMax),
                    behavior.VelocityDirectionalLimit);
            }

            transform.Translate += motion.Velocity * delta;
        }

        /// <summary>
        /// Caps a vector's magnitude without turning it. The component-wise clamps bound a
        /// box; this bounds a circle, so diagonal motion is not quietly faster.
        /// </summary>
        private static Vector2F Limit(Vector2F value, float maxMagnitude)
        {
            if (maxMagnitude >= float.MaxValue || maxMagnitude < 0f)
                return value;

            var magnitude = value.Magnitude;
            return magnitude <= maxMagnitude || magnitude <= 0f
                ? value
                : value * (maxMagnitude / magnitude);
        }
    }
}
