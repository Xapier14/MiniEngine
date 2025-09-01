namespace MiniEngine
{
    public interface IAnimation
    {
        /// <summary>
        /// A negative value denotes an infinite number of frames.
        /// </summary>
        public int FrameCount { get; }

        public Sprite GetFrame(int frameIndex);

        /// <summary>
        /// The animation speed in FPS.
        /// </summary>
        public float AnimationSpeed { get; set; }
    }

    public abstract class Animation : IAnimation
    {
        public abstract Sprite GetFrame(int frameIndex);
        public abstract int FrameCount { get; }
        public float AnimationSpeed { get; set; }
    }
}
