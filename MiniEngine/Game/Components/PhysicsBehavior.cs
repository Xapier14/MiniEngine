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
        public float Resistance { get; set; } = 0f;
    }
}
