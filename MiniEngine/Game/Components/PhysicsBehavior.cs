namespace MiniEngine.Components
{
    [OnlyOneOfType]
    [RequiresComponent<Motion>]
    public class PhysicsBehavior : Component
    {
        public float VelocityDirectionalLimit { get; set; } = float.MaxValue;
        public Vector2F VelocityMin = Vector2F.NegativeInf;
        public Vector2F VelocityMax = Vector2F.PositiveInf;

        public float AccelerationDirectionalLimit { get; set; } = float.MaxValue;
        public Vector2F AccelerationMin = Vector2F.NegativeInf;
        public Vector2F AccelerationMax = Vector2F.PositiveInf;

        public float Mass { get; set; } = 1f;

        /// <summary>
        /// Velocity lost per second, as a fraction. 0 coasts forever; 1 stops within a
        /// second. Applied linearly per step, which is stable because the step is fixed.
        /// </summary>
        public float Resistance { get; set; } = 0f;

        /// <summary>
        /// Bounciness on impact. 0 keeps none of the approach speed, 1 keeps all of it.
        /// A contact uses the higher of the two bodies' values, so a bouncy thing bounces
        /// off ordinary geometry without that geometry having to opt in.
        /// </summary>
        public float Restitution { get; set; } = 0f;
    }
}
