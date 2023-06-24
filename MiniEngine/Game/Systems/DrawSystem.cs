using MiniEngine.Components;
using MiniEngine.Utility;
using System;
using static SDL2.SDL;

namespace MiniEngine
{
    [HandlesComponent<Sprite>]
    public class DrawSystem : System
    {
        public DrawSystem()
        {
            LoggingService.Debug("Sprite system initialized");
        }

        public void HandleComponent(Sprite spriteComponent)
        {
            if (Graphics.RendererPtr == null)
                return;
            var entity = spriteComponent.Owner!;
            var transformComponent = entity.GetComponent<Transform>()!;

            var position = transformComponent.Translate + spriteComponent.Offset;
            var size = spriteComponent.Size;

            var rect = new SDL_FRect
            {
                x = position.X,
                y = position.Y,
                w = size.Width,
                h = size.Height
            };
            var texture = Resources.GetTexture(spriteComponent.SpriteResource!);
            _ = Vector2.Zero.Equals(size)
                ? SDL_RenderCopyF(Graphics.RendererPtr.Value, texture, IntPtr.Zero, IntPtr.Zero)
                : SDL_RenderCopyF(Graphics.RendererPtr.Value, texture, IntPtr.Zero, ref rect);
        }

        protected override void Step(object? arg)
        {

        }
    }
}
