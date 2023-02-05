using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SDL2.SDL;

namespace MiniEngine
{
    public static class GraphicsService
    {
        private static IntPtr? RendererPtr => WindowService.GameWindow?.RendererPtr;
        public static void RenderClear()
        {
            if (RendererPtr is null)
                return;
            _ = SDL_RenderClear(RendererPtr.Value);
        }
        public static void RenderPresent()
        {
            if (RendererPtr is null)
                return;
            SDL_RenderPresent(RendererPtr.Value);
        }
    }
}
