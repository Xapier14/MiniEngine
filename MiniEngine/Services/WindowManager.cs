using System;
using System.Runtime.InteropServices;
using MiniEngine.Utility;
using MiniEngine.Windowing;

using static SDL2.SDL;

namespace MiniEngine
{
    public class WindowManager(GameEngine gameEngine, InputManager inputManager)
    {
        public GameWindow? GameWindow { get; private set; }

        public void CreateWindow(string initialWindowTitle)
        {
            if (!gameEngine.IsRunning)
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

            var setup = gameEngine.Setup;
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

        internal void PumpEvents()
        {
            _ = SDL_PollEvent(out var sdlEvent);
            switch (sdlEvent.type)
            {
                case SDL_EventType.SDL_WINDOWEVENT:
                    ProcessWindowEvent(sdlEvent);
                    break;
                case SDL_EventType.SDL_KEYDOWN:
                    ProcessKeyDownEvent(sdlEvent);
                    break;
                case SDL_EventType.SDL_KEYUP:
                    ProcessKeyUpEvent(sdlEvent);
                    break;
            }
        }

        private void ProcessWindowEvent(SDL_Event sdlEvent)
        {
            switch (sdlEvent.window.windowEvent)
            {
                case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                    gameEngine.RequestHalt();
                    break;
            }
        }

        private void ProcessKeyDownEvent(SDL_Event sdlEvent)
        {
            var keycode = sdlEvent.key.keysym.sym;
            inputManager.UpdateKeyState((Key)keycode, true);
        }

        private void ProcessKeyUpEvent(SDL_Event sdlEvent)
        {
            var keycode = sdlEvent.key.keysym.sym;
            inputManager.UpdateKeyState((Key)keycode, false);
        }
    }
}
