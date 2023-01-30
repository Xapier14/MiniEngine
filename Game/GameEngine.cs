using System;
using System.Collections.Generic;
using System.Linq;
using MiniEngine.Utility;
using MiniEngine.Utility.Logging;

using SDL2;
using static SDL2.SDL;

namespace MiniEngine
{
    public static class GameEngine
    {
        private static bool _isInitialized = false;
        private static bool _isReleased = false;
        private static bool _canAddInitializers = true;
        private static bool _canAddReleasers = true;
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

            _initializers.AddRange(new Func<bool>[]
            {
                InitializeSDL,
                InitializeGameAssets
            });
        }

        private static bool Initialize()
        {
            if (_isInitialized)
                return false;
            var coreInitializersResult = _initializers.Any(initializer => initializer());
            if (coreInitializersResult)
            {
                LoggingService.Fatal("An internal initializer returned an error!");
                return true;
            }

            if (!_externalInitializers.Any())
            {
                _isInitialized = true;
                return false;
            }

            LoggingService.Debug("{0} external initializer delegate(s) queued.", _externalInitializers.Count);
            var externalInitializersResult = _externalInitializers.Any(initializer => initializer());
            _isInitialized = true;
            return externalInitializersResult;
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
            
            LoggingService.Info("Initialized SDL2.");
            return false;
        }

        private static bool InitializeGameAssets()
        {
            SystemService.LoadSystems();
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
            if (Initialize())
            {
                LoggingService.Fatal("GameEngine could not be initialized.");
                return true;
            }
            IsRunning = true;

            // loop here

            GracefulExit();
            return false;
        }
    }
}
