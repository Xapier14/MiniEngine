using System.Collections.Generic;
using MiniEngine.Utility;

namespace MiniEngine
{
    public sealed class SequenceAnimation : Animation
    {
        public IList<Sprite> Frames { get; set; } = [];
        public override int FrameCount => Frames.Count;

        public override Sprite GetFrame(int frameIndex)
        {
            return Frames[Formulas.Mod(frameIndex, FrameCount)];
        }

    }
}
