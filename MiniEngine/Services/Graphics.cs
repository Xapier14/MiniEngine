using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace MiniEngine
{
    public static class Graphics
    {
        private static readonly Dictionary<MemoryResource, IntPtr> _textureCache = new();
        public static IntPtr? RendererPtr => WindowManager.GameWindow?.RendererPtr;
        private static int? WindowWidth => WindowManager.GameWindow?.WindowSize.Width;
        private static int? WindowHeight => WindowManager.GameWindow?.WindowSize.Height;
        private static IntPtr? _missingTexture;

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

        public static void DrawTexture(MemoryResource? textureResource, Vector2F position, Size size, double angle = 0.0, Vector2F? rotationPoint = null)
        {
            if (RendererNullSoftCheck("DrawTexture"))
                return;

            var rect = new SDL_FRect
            {
                x = position.X,
                y = position.Y,
                w = size.Width,
                h = size.Height
            };
            var point = new SDL_FPoint()
            {
                x = rotationPoint?.X ?? size.Width / 2f,
                y = rotationPoint?.Y ?? size.Height / 2f
            };
            var texture = GetTexture(textureResource);
            unsafe
            {
                _ = Vector2.Zero.Equals(size)
                    ? SDL_RenderCopyExF(RendererPtr!.Value, texture, IntPtr.Zero, IntPtr.Zero, angle, ref point,
                        SDL_RendererFlip.SDL_FLIP_NONE)
                    : SDL_RenderCopyExF(RendererPtr!.Value, texture, IntPtr.Zero, ref rect, angle, (IntPtr)Unsafe.AsPointer(ref point), SDL_RendererFlip.SDL_FLIP_NONE);
            }
        }

        internal static IntPtr GetTexture(MemoryResource? textureResource)
        {
            if (RendererPtr == null)
                throw new NoWindowInstanceException();
            if (!_missingTexture.HasValue)
            {
                var missingTexture = SDL_CreateTexture(RendererPtr.Value, SDL_PIXELFORMAT_RGBA8888,
                    (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, 2, 2);
                var perPixelSize = SDL_BYTESPERPIXEL(SDL_PIXELFORMAT_RGBA8888);
                var pixelData = new byte[]
                {
                    255, 200, 0, 255, 255, 0, 0, 0,
                    255, 0, 0, 0, 255, 200, 0, 255
                };
                var handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);

                var result = SDL_UpdateTexture(missingTexture, IntPtr.Zero, handle.AddrOfPinnedObject(), perPixelSize * 2);
                if (result != 0)
                {
                    LoggingService.Fatal("Error creating missing texture. {0}", SDL_GetError());
                    GameEngine.FatalExit(-1);
                }
                handle.Free();
                _missingTexture = missingTexture;
            }

            if (textureResource is null)
                return _missingTexture.Value;

            if (_textureCache.TryGetValue(textureResource, out var texture))
                return texture;

            var surface = IMG_Load_RW(textureResource.RwHandle, 0);
            if (surface == IntPtr.Zero)
            {
                LoggingService.Fatal(SDL_GetError());
                GameEngine.FatalExit(-200);
            }
            texture = SDL_CreateTextureFromSurface(RendererPtr.Value, surface);
            if (texture == IntPtr.Zero)
            {
                LoggingService.Fatal(SDL_GetError());
                GameEngine.FatalExit(-201);
            }

            _textureCache.Add(textureResource, texture);

            return texture;
        }
    }
}
