using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

using static SDL2.SDL;

namespace MiniEngine
{
    [HandlesComponent<Sprite>]
    public class SpriteSystem : System
    {
        public SpriteSystem()
        {
            LoggingService.Debug("Sprite system initialized");
        }

        public void HandleComponent(Sprite spriteComponent)
        {
            if (spriteComponent.SpriteResource == null || Graphics.RendererPtr == null)
                return;
            var entity = spriteComponent.Owner!;
            var transformComponent = entity.GetComponent<Transform>()!;

            var position = transformComponent.Translate + spriteComponent.Offset;
            var texture = Resources.GetTexture(spriteComponent.SpriteResource!);
            _ = SDL_RenderCopy(Graphics.RendererPtr.Value, texture, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
