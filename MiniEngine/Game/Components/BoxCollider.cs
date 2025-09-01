namespace MiniEngine.Components
{
    [RequiresComponent<Motion>]
    public class BoxCollider : Component, ICollider
    {
        public float Top { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }
    }
}
