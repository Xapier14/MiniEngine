using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Utility;
using static SDL2.SDL;

namespace MiniEngine
{
    public static class Graphics
    {
        private static IntPtr? RendererPtr => WindowManager.GameWindow?.RendererPtr;
        private static int? WindowWidth => WindowManager.GameWindow?.WindowSize.Width;
        private static int? WindowHeight => WindowManager.GameWindow?.WindowSize.Height;

        private static bool WindowNullSoftCheck(string? from = null)
        {
            if (WindowManager.GameWindow is not null)
                return false;
            LoggingService.Error($"[GraphicsService{(from != null ? $".{from}" : "")}] GameWindow is null!");
            return true;

        }

        private static bool RendererNullSoftCheck(string? from = null)
        {
            if (RendererPtr is not null)
                return false;
            LoggingService.Error($"[GraphicsService{(from != null ? $".{from}" : "")}] Renderer is null!");
            return true;

        }

        public static void RenderClear()
        {
            if (RendererNullSoftCheck("RenderClear"))
                return;
            _ = SDL_RenderClear(RendererPtr!.Value);
        }

        public static void RenderPresent()
        {
            if (RendererNullSoftCheck("RenderPresent"))
                return;
            SDL_RenderPresent(RendererPtr!.Value);
        }

        public static Color GetDrawColor()
        {
            if (RendererNullSoftCheck("GetDrawColor"))
                throw new NoWindowInstanceException("No window instance to get draw color from.");
            _ = SDL_GetRenderDrawColor(RendererPtr!.Value, out var r, out var g, out var b, out var a);

            return (r, g, b, a);
        }

        public static void SetDrawColor(Color color)
        {
            if (RendererNullSoftCheck("SetDrawColor"))
                throw new NoWindowInstanceException("No window instance to set draw color to.");
            _ = SDL_SetRenderDrawColor(RendererPtr!.Value, color.R, color.G, color.B, color.A);
        }

        public static void DrawPixel(Vector2 windowVector, Color? color = null)
        {
            if (RendererNullSoftCheck("DrawPixel"))
                return;

            Color oldColor = default;

            if (color is not null)
            {
                oldColor = GetDrawColor();
                SetDrawColor(color.Value);
            }

            _ = SDL_RenderDrawPoint(RendererPtr!.Value, windowVector.X, windowVector.Y);

            if (color is not null)
            {
                SetDrawColor(oldColor);
            }
        }

        public static void DrawPixel(Vector2F normalizedVector, Color? color = null)
        {
            if (WindowNullSoftCheck("DrawPixel"))
                return;

            var clampedVector = Vector2F.Clamp(normalizedVector, -1, 1);
            var windowVector = clampedVector.Denormalize((WindowWidth!.Value, WindowHeight!.Value));

            DrawPixel(windowVector, color);
        }
    }
}
