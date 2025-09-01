namespace MiniEngine
{
    public class Sprite(MemoryResource? textureResource, Vector2 offset, Size size)
    {
        public MemoryResource? TextureResource => textureResource;
        public IReadOnlyVector2 Offset => offset;
        public IReadOnlySize Size => size;

        public static implicit operator Sprite(MemoryResource? textureResource) => new(textureResource, Vector2.Zero, MiniEngine.Size.NoSize);

        public static implicit operator Sprite(string resourcePath) =>
            new(resourcePath, Vector2.Zero, MiniEngine.Size.NoSize);
    }
}
