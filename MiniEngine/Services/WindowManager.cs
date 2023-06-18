using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Utility;
using MiniEngine.Windowing;

using static SDL2.SDL;

namespace MiniEngine
{
    public static class WindowManager
    {
        public static GameWindow? GameWindow { get; private set; }

        public static void CreateWindow()
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
            var result = SDL_CreateWindowAndRenderer(
                windowSize.Width,
                windowSize.Height,
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI,
                out var windowPtr,
                out var rendererPtr);

            if (result != 0)
            {
                var error = SDL_GetError();
                var exception =
                    new SDLErrorException(
                        $"SDL threw an error whilst creating window.\n{error}");
                LoggingService.Fatal("SDL threw an error whilst creating window. Error: {0}", exception, error);
                throw exception;
            }

            GameWindow = new GameWindow(windowSize, windowPtr, rendererPtr);
            LoggingService.Info("Created window {0}.", windowSize);
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
