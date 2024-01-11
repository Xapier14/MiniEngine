using MiniEngine.Utility;
using MiniEngine.Utility.Logging;
using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace MiniEngine
{
    public static class GameEngine
    {
        private static bool _isInitialized;
        private static bool _isReleased;
        private static bool _canAddInitializers = true;
        private static bool _canAddReleasers = true;
        private static bool _requestedHalt;
        private static EngineSetup _setup = EngineSetup.Default;
        private static readonly List<Func<bool>> _initializers = new();
        private static readonly List<Func<bool>> _releasers = new();
        private static readonly List<Func<bool>> _externalInitializers = new();
        private static readonly List<Func<bool>> _externalReleasers = new();

        public static IReadOnlyEngineSetup Setup => _setup;
        public static bool IsRunning { get; private set; }

        static GameEngine()
        {
            LoggingService.Use(new ConsoleLogger());

            DllMap.RegisterDllMap();

            _initializers.AddRange(new []
            {
                SystemManager.InitializeWithDefaultSystems,
                InitializeGameAssets
            });

            _releasers.AddRange(new []
            {
                ReleaseComponents,
                ReleaseGameAssets
            });
        }

        private static bool Initialize()
        {
            if (_isInitialized)
                return false;
            var coreInitializersResult = _initializers.Any(initializer => initializer());
            if (coreInitializersResult)
            {
                LoggingService.Fatal("A core initializer returned an error!");
                return true;
            }
            LoggingService.Debug("Finished core initializers.", _externalInitializers.Count);

            _isInitialized = true;
            return false;
        }

        private static bool InitializeExternal()
        {
            if (!_externalInitializers.Any())
            {
                return false;
            }

            LoggingService.Debug("{0} external initializer delegate(s) queued.", _externalInitializers.Count);
            var externalInitializersResult = _externalInitializers.Any(initializer => initializer());
            if (externalInitializersResult)
            {
                LoggingService.Fatal("An external initializer returned an error!");
                return true;
            }
            LoggingService.Debug("Finished external initializers.", _externalInitializers.Count);

            return false;
        }

        private static bool Release()
        {
            if (_isReleased)
                return false;
            var coreReleasersResult = _releasers.Any(releaser => releaser());
            if (coreReleasersResult)
            {
                LoggingService.Fatal("An internal releaser returned an error!");
                return true;
            }

            if (!_externalReleasers.Any())
            {
                _isReleased = true;
                return false;
            }

            LoggingService.Debug("{0} external releaser delegate(s) queued.", _externalReleasers.Count);
            var externalReleasersResult = _externalReleasers.Any(releaser => releaser());
            _isReleased = true;
            return externalReleasersResult;
        }

        private static bool InitializeSDL()
        {
            if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
            {
                LoggingService.Fatal("Error initializing SDL2.");
                LoggingService.Fatal(SDL_GetError());
                return true;
            }

            var imgFlags = IMG_InitFlags.IMG_INIT_PNG | IMG_InitFlags.IMG_INIT_JPG;
            if ((IMG_InitFlags)IMG_Init(imgFlags) != imgFlags)
            {
                LoggingService.Fatal("Error initializing SDL_image.");
                LoggingService.Fatal(IMG_GetError());
                return true;
            }

            LoggingService.Info("Initialized SDL2.");
            return false;
        }

        private static bool InitializeGameAssets()
        {
            if (Setup.AssetsFile != null)
                Resources.UsePack(Setup.AssetsFile);
            return false;
        }

        private static bool ReleaseGameAssets()
        {
            Resources.ClosePacks();
            return false;
        }

        private static bool ReleaseComponents()
        {
            SystemManager.PurgeComponents();
            return false;
        }

        private static void GracefulExit()
        {
            _canAddReleasers = false;
            if (Release())
                FatalExit(-1);
        }

        internal static void FatalExit(int exitCode)
        {
            LoggingService.Fatal("GameEngine has run into a fatal error and will forcibly exit.");
            if (exitCode == 0)
                GracefulExit();
            Environment.Exit(exitCode);
        }

        public static void RequestHalt()
        {
            if (_requestedHalt)
                return;
            _requestedHalt = true;
            LoggingService.Debug("Requested GameEngine halt.");
        }

        public static void UseSetup(params EngineSetup[] additionalSetups)
        {
            if (IsRunning)
            {
                LoggingService.Error("Attempted to apply another setup configuration while running!");
                return;
            }
            if (!_canAddInitializers)
            {
                LoggingService.Error("Attempted to apply another setup configuration while initializing!");
                return;
            }

            foreach (var additionalSetup in additionalSetups)
            {
                _setup.Apply(additionalSetup);
            }
        }

        public static void AddInitializer(Func<bool> initializerDelegate)
        {
            if (!_canAddInitializers)
            {
                LoggingService.Error("Cannot add initializer delegate. Engine is already initializing!");
                return;
            }
            _externalInitializers.Add(initializerDelegate);
        }

        public static void AddReleaser(Func<bool> releaserDelegate)
        {
            if (!_canAddReleasers)
            {
                LoggingService.Error("Cannot add releaser delegate. Engine is already releasing!");
                return;
            }
            _externalReleasers.Add(releaserDelegate);
        }

        public static bool Run()
        {
            _canAddInitializers = false;
            if (InitializeSDL())
            {
                LoggingService.Fatal("GameEngine could not be initialized (SDL initialization error).");
                return true;
            }
            IsRunning = true;

            var initialWindowTitle = _setup.InitialWindowTitle ?? "MiniEngine Game Window";
            WindowManager.CreateWindow(initialWindowTitle);
            if (Initialize() || InitializeExternal())
            {
                LoggingService.Fatal("GameEngine could not be initialized.");
                return true;
            }

            while (IsRunning)
            {
                WindowManager.PumpEvents();
                InputManager.UpdateState();
                SystemManager.ProcessSystems();

                if (!_requestedHalt)
                    continue;
                IsRunning = false;
                LoggingService.Debug("GameEngine halted.");
            }

            GracefulExit();
            return false;
        }
    }
}
