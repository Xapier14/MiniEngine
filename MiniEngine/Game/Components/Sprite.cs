namespace MiniEngine.Components
{
    [RequiresComponent<Transform>]
    public class Sprite : Component
    {
        public Vector2F Offset = Vector2F.Zero;
        public Size Size = Vector2.Zero;
        public Vector2 RotationOrigin = Vector2.Zero;
        public float Rotation = 0f;
        public int Depth = 0;

        public MemoryResource? SpriteResource { get; set; }
    }
}
