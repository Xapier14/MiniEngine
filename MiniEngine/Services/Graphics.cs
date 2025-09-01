using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MiniEngine.Windowing;
using static SDL2.SDL;
using static SDL2.SDL_image;
using static SDL2.SDL_ttf;

namespace MiniEngine
{
    public class Graphics(GameEngine gameEngine)
    {
        private readonly Dictionary<MemoryResource, IntPtr> _textureCache = [];
        private readonly Dictionary<(MemoryResource, int), IntPtr> _fontCache = [];
        private readonly Dictionary<(IntPtr, string, Color, bool, uint), (IntPtr Texture, Size Size)> _renderedTextCache = [];
        private GameWindow? _gameWindow => gameEngine.WindowManager.GameWindow;
        public IntPtr? RendererPtr => _gameWindow?.RendererPtr;
        private int? WindowWidth => _gameWindow?.WindowSize.Width;
        private int? WindowHeight => _gameWindow?.WindowSize.Height;
        private IntPtr? _missingTexture;
        private bool _inDrawTime;

        private void AutoCorrectVector(ref Vector2 vector)
        {
            if (gameEngine.Setup.InvertYAxis != true)
                return;
            vector.Y = (_gameWindow?.WindowSize.Height ?? 0) - vector.Y;
        }

        private void AutoCorrectVector(ref Vector2F vector)
        {
            if (gameEngine.Setup.InvertYAxis != true)
                return;
            vector.Y = (_gameWindow?.WindowSize.Height ?? 0f) - vector.Y;
        }

        private bool WindowNullSoftCheck(string? from = null)
        {
            if (_gameWindow is not null)
                return false;
            LoggingService.Error($"[GraphicsService{(from != null ? $".{from}" : "")}] GameWindow is null!");
            return true;

        }

        private bool RendererNullSoftCheck(string? from = null)
        {
            if (RendererPtr is not null)
                return false;
            LoggingService.Error($"[GraphicsService{(from != null ? $".{from}" : "")}] Renderer is null!");
            return true;
        }

        private bool RenderDrawCycleFatalCheck(string? from = null)
        {
            if (_inDrawTime)
                return false;
            LoggingService.Fatal($"[GraphicsService{(from != null ? $".{from}" : "")}] Attempted to draw outside of cycle!");
            gameEngine.FatalExit(ExitCode.GameError);
            return true;
        }

        public void RenderClear()
        {
            if (RendererNullSoftCheck("RenderClear"))
                return;
            _ = SDL_RenderClear(RendererPtr!.Value);
            _inDrawTime = true;
        }

        public void RenderPresent()
        {
            if (RendererNullSoftCheck("RenderPresent"))
                return;
            SDL_RenderPresent(RendererPtr!.Value);
            _inDrawTime = false;
        }

        public Color GetDrawColor()
        {
            if (RendererNullSoftCheck("GetDrawColor"))
                throw new NoWindowInstanceException("No window instance to get draw color from.");
            _ = SDL_GetRenderDrawColor(RendererPtr!.Value, out var r, out var g, out var b, out var a);

            return (r, g, b, a);
        }

        public void SetDrawColor(Color color)
        {
            if (RendererNullSoftCheck("SetDrawColor"))
                throw new NoWindowInstanceException("No window instance to set draw color to.");
            _ = SDL_SetRenderDrawColor(RendererPtr!.Value, color.R, color.G, color.B, color.A);
        }

        public void DrawPixel(Vector2 windowVector, Color? color = null)
        {
            if (RendererNullSoftCheck("DrawPixel"))
                return;
            if (RenderDrawCycleFatalCheck("DrawPixel"))
                return;

            Color oldColor = default;

            if (color is not null)
            {
                oldColor = GetDrawColor();
                SetDrawColor(color.Value);
            }

            AutoCorrectVector(ref windowVector);
            _ = SDL_RenderDrawPoint(RendererPtr!.Value, windowVector.X, windowVector.Y);

            if (color is not null)
            {
                SetDrawColor(oldColor);
            }
        }

        public void DrawRectangle(Vector2 point1, Vector2 point2, Color? borderColor,
            Color? fillColor)
        {
            if (WindowNullSoftCheck("DrawRectangle"))
                return;
            if (RenderDrawCycleFatalCheck("DrawRectangle"))
                return;

            if (borderColor is null && fillColor is null)
                return;

            AutoCorrectVector(ref point1);
            AutoCorrectVector(ref point2);

            var rect = new SDL_Rect
            {
                x = point1.X,
                y = point1.Y,
                w = point2.X - point1.X,
                h = point2.Y - point1.Y
            };

            var oldColor = GetDrawColor();
            if (fillColor is not null)
            {
                SetDrawColor(fillColor.Value);
                _ = SDL_RenderFillRect(RendererPtr!.Value, ref rect);
            }

            if (borderColor is not null)
            {
                SetDrawColor(borderColor.Value);
                _ = SDL_RenderDrawRect(RendererPtr!.Value, ref rect);
            }

            SetDrawColor(oldColor);
        }

        public void DrawLine(Vector2 point1, Vector2 point2, Color lineColor)
        {
            if (RendererNullSoftCheck("DrawLine"))
                return;
            if (RenderDrawCycleFatalCheck("DrawLine"))
                return;

            var oldColor = GetDrawColor();
            SetDrawColor(lineColor);

            AutoCorrectVector(ref point1);
            AutoCorrectVector(ref point2);
            _ = SDL_RenderDrawLine(RendererPtr!.Value, point1.X, point1.Y, point2.X, point2.Y);

            SetDrawColor(oldColor);
        }

        public void DrawSprite(Sprite? sprite, Vector2F position, Size size, double angle = 0.0, Vector2F? rotationPoint = null, bool FlipX = false, bool FlipY = false)
        {
            if (RendererNullSoftCheck("DrawSprite"))
                return;
            if (RenderDrawCycleFatalCheck("DrawSprite"))
                return;

            AutoCorrectVector(ref position);
            if (gameEngine.Setup.InvertYAxis == true)
            {
                position.Y -= size.Height;
            }
            var rect = new SDL_FRect
            {
                x = position.X,
                y = position.Y,
                w = size.Width,
                h = size.Height
            };
            var point = new SDL_FPoint
            {
                x = size.Width / 2f,
                y = size.Height / 2f
            };
            if (rotationPoint.HasValue)
            {
                var rPoint = rotationPoint.Value;
                point.x = rPoint.X;
                point.y = rPoint.Y;
            }
            var srcPtr = IntPtr.Zero;
            
            var texture = GetTexture(sprite?.TextureResource);
            unsafe
            {
                var srcRect = new SDL_Rect();
                if (sprite != null && (sprite.Size.Width >= 0 || sprite.Size.Height >= 0))
                {
                    srcRect.x = Math.Max(0, sprite.Offset.X);
                    srcRect.y = Math.Max(0, sprite.Offset.Y);
                    srcRect.w = sprite.Size.Width;
                    srcRect.h = sprite.Size.Height;
                    srcPtr = (IntPtr)(&srcRect);
                }

                var flip = SDL_RendererFlip.SDL_FLIP_NONE;
                if (FlipX)
                    flip |= SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
                if (FlipY)
                    flip |= SDL_RendererFlip.SDL_FLIP_VERTICAL;

                _ = Vector2.Zero.Equals(size)
                    ? SDL_RenderCopyExF(RendererPtr!.Value, texture, srcPtr, IntPtr.Zero, angle, ref point, flip)
                    : SDL_RenderCopyExF(RendererPtr!.Value, texture, srcPtr, ref rect, angle, (IntPtr)(&point), flip);
            }
        }

        internal IntPtr GetTexture(MemoryResource? textureResource)
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
                    gameEngine.FatalExit(-1);
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
                gameEngine.FatalExit(-200);
            }
            texture = SDL_CreateTextureFromSurface(RendererPtr.Value, surface);
            if (texture == IntPtr.Zero)
            {
                LoggingService.Fatal(SDL_GetError());
                gameEngine.FatalExit(-201);
            }
            SDL_FreeSurface(surface);

            _textureCache.Add(textureResource, texture);

            return texture;
        }

        public void DrawText(MemoryResource fontResource, int ptSize, Vector2 position, string text, Color foreground, bool wrap = false, uint wrapLength = 0)
        {
            if (RendererNullSoftCheck("DrawText"))
                return;
            if (RenderDrawCycleFatalCheck("DrawText"))
                return;

            if (!_fontCache.TryGetValue((fontResource, ptSize), out var font))
            {
                font = TTF_OpenFontRW(fontResource.RwHandle, 0, ptSize);
                _fontCache.Add((fontResource, ptSize), font);
            }

            if (!_renderedTextCache.TryGetValue((font, text, foreground, wrap, wrap ? wrapLength : 0),
                    out var tuple))
            {
                var fg = new SDL_Color
                {
                    r = foreground.R,
                    g = foreground.G,
                    b = foreground.B,
                    a = foreground.A,
                };

                var surface = wrap
                    ? TTF_RenderText_Blended_Wrapped(font, text, fg, wrapLength)
                    : TTF_RenderText_Blended(font, text, fg);
                if (surface == IntPtr.Zero)
                {
                    LoggingService.Fatal(SDL_GetError());
                    gameEngine.FatalExit(-200);
                }

                tuple.Texture = SDL_CreateTextureFromSurface(RendererPtr!.Value, surface);
                if (tuple.Texture == IntPtr.Zero)
                {
                    LoggingService.Fatal(SDL_GetError());
                    gameEngine.FatalExit(-201);
                }

                tuple.Size = new Size();
                unsafe
                {
                    var surfacePtr = (SDL_Surface*)surface;
                    tuple.Size.Width = surfacePtr->w;
                    tuple.Size.Height = surfacePtr->h;
                }
                SDL_FreeSurface(surface);

                _renderedTextCache.Add((font, text, foreground, wrap, wrap ? wrapLength : 0), tuple);
            }

            AutoCorrectVector(ref position);
            if (gameEngine.Setup.InvertYAxis == true)
            {
                position.Y -= tuple.Size.Height;
            }

            var dstRect = new SDL_Rect
            {
                x = position.X,
                y = position.Y,
                w = tuple.Size.Width,
                h = tuple.Size.Height
            };

            _ = SDL_RenderCopy(RendererPtr!.Value, tuple.Texture, IntPtr.Zero, ref dstRect);

            _renderedTextCache.Remove((font, text, foreground, wrap, wrap ? wrapLength : 0));
            SDL_DestroyTexture(tuple.Texture);
        }

        internal void Tidy()
        {

        }

        internal void ReleaseResources()
        {
            foreach (var (_, texture) in _textureCache)
            {
                SDL_DestroyTexture(texture);
            }
            _textureCache.Clear();
            foreach (var (_, (texture, _)) in _renderedTextCache)
            {
                SDL_DestroyTexture(texture);
            }
            _renderedTextCache.Clear();
            foreach (var (_, font) in _fontCache)
            {
                TTF_CloseFont(font);
            }
        }
    }
}
