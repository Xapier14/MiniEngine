namespace MiniEngine.Components
{
    [OnlyOneOfType]
    [RequiresComponent<Transform>]
    public class Motion : Component
    {
        public Vector2F Velocity = Vector2F.Zero;
        public Vector2F Acceleration = Vector2F.Zero;
    }
}
