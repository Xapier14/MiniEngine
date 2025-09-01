using System;

namespace MiniEngine.Components
{
    [OnlyOneOfType]
    public class Transform : Component
    {
        public Vector2F Translate = Vector2F.Zero;
        public int TranslateX => (int)Math.Round(Translate.X);
        public int TranslateY => (int)Math.Round(Translate.Y);
    }
}
