namespace MiniEngine
{
    public sealed class StaticAnimation : Animation
    {
        private Sprite _sprite;
        public new float AnimationSpeed => 1000;
        public override int FrameCount => 1;

        public StaticAnimation(Sprite sprite)
        {
            _sprite = sprite;
        }

        public StaticAnimation(MemoryResource? memoryResource)
        {
            _sprite = memoryResource;
        }

        public override Sprite GetFrame(int frameIndex)
        {
            return _sprite;
        }
        
    }
}
