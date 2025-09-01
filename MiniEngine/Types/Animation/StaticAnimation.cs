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

        public override Sprite GetFrame(int frameIndex)
        {
            return _sprite;
        }

        public static implicit operator StaticAnimation(Sprite sprite) => new(sprite);
        public static implicit operator StaticAnimation(string sprite) => new(sprite);
    }
}
