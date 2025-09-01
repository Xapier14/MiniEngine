using System;

namespace MiniEngine.Components
{
    [RequiresComponent<Transform>]
    public sealed class Drawable : Component
    {
        public Vector2F Offset = Vector2F.Zero;
        public Size Size = Vector2.Zero;
        public float Rotation = 0f;
        public int Depth = 0;
        public bool FlipX = false;
        public bool FlipY = false;
        public bool DisableManagedDraw = false;

        public Sprite? Sprite { get; set; }
        public event Action<Drawable>? ExternalDrawCall;

        internal void RaiseExternalDrawCall()
            => ExternalDrawCall?.Invoke(this);
    }
}
