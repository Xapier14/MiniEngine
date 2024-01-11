using System;
using System.Runtime.InteropServices;
using MiniEngine.Utility;
using MiniEngine.Windowing;

using static SDL2.SDL;

namespace MiniEngine
{
    public static class WindowManager
    {
        public static GameWindow? GameWindow { get; private set; }

        public static void CreateWindow(string initialWindowTitle)
        {
            if (!GameEngine.IsRunning)
            {
                var exception =
                    new EngineNotRunningException(
                        "Call and wait for GameEngine.Run() to finish before using WindowManager.");
                LoggingService.Fatal("GameWindow could not be created. GameEngine is not running!", exception);
                throw exception;
            }

            if (GameWindow != null)
            {
                LoggingService.Error("GameWindow is already created! Cannot create window.");
                return;
            }

            var setup = GameEngine.Setup;
            var windowSize = setup.WindowSize!.Value;
            var windowPtr = SDL_CreateWindow(initialWindowTitle, 
                SDL_WINDOWPOS_UNDEFINED, 
                SDL_WINDOWPOS_UNDEFINED, 
                windowSize.Width,
                windowSize.Height,
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);

            if (windowPtr == IntPtr.Zero)
            {
                var error = SDL_GetError();
                var exception =
                    new SDLErrorException(
                        $"SDL threw an error whilst creating window.\n{error}");
                LoggingService.Fatal("SDL threw an error whilst creating window. Error: {0}", exception, error);
                throw exception;
            }
            LoggingService.Info("Created window {0}.", windowSize);

            var rendererPtr = SDL_CreateRenderer(windowPtr, -1, SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            if (rendererPtr == IntPtr.Zero)
            {
                var error = SDL_GetError();
                var exception =
                    new SDLErrorException(
                        $"SDL threw an error whilst creating renderer.\n{error}");
                LoggingService.Fatal("SDL threw an error whilst creating renderer. Error: {0}", exception, error);
                throw exception;
            }

            _ = SDL_GetRendererInfo(rendererPtr, out var rendererInfo);
            var rendererName = Marshal.PtrToStringUTF8(rendererInfo.name);
            Size maxTextureSize = (rendererInfo.max_texture_width, rendererInfo.max_texture_height);
            LoggingService.Info("Created renderer \"{0}\", max texture size: {1}.", rendererName ?? "n/a", maxTextureSize);

            SDL_SetWindowTitle(windowPtr, initialWindowTitle);
            GameWindow = new GameWindow(windowSize, windowPtr, rendererPtr);
        }

        internal static void PumpEvents()
        {
            _ = SDL_PollEvent(out var sdlEvent);
            if (sdlEvent.type == SDL_EventType.SDL_WINDOWEVENT)
            {
                if (sdlEvent.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                    GameEngine.RequestHalt();
            }
        }
    }
}
