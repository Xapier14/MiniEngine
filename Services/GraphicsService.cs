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
        private static int? WindowWidth => WindowService.GameWindow?.WindowSize.Width;
        private static int? WindowHeight => WindowService.GameWindow?.WindowSize.Height;

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

        public static Color GetDrawColor()
        {
            if (RendererPtr is null)
                throw new NoWindowInstanceException("No window instance to get draw color from.");
            _ = SDL_GetRenderDrawColor(RendererPtr.Value, out var r, out var g, out var b, out var a);

            return (r, g, b, a);
        }

        public static void SetDrawColor(Color color)
        {
            if (RendererPtr is null)
                throw new NoWindowInstanceException("No window instance to set draw color to.");
            _ = SDL_SetRenderDrawColor(RendererPtr.Value, color.R, color.G, color.B, color.A);
        }

        public static void DrawPixel(Vector2 windowVector, Color? color = null)
        {
            if (RendererPtr is null)
                return;

            Color oldColor = default;

            if (color is not null)
            {
                oldColor = GetDrawColor();
                SetDrawColor(color.Value);
            }

            _ = SDL_RenderDrawPoint(RendererPtr.Value, windowVector.X, windowVector.Y);

            if (color is not null)
            {
                SetDrawColor(oldColor);
            }
        }

        public static void DrawPixel(Vector2F normalizedVector, Color? color = null)
        {
            if (WindowWidth is null ||
                WindowHeight is null)
                return;

            var clampedVector = Vector2F.Clamp(normalizedVector, -1, 1);
            var windowVector = clampedVector.Denormalize((WindowWidth.Value, WindowHeight.Value));

            DrawPixel(windowVector, color);
        }
    }
}
