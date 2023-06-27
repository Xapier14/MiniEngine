using MiniEngine.Utility;
using MiniEngine.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Silk.NET.Windowing;

namespace MiniEngine
{
    public static class GameEngine
    {
        private static bool _isInitialized = false;
        private static bool _isReleased = false;
        private static bool _canAddInitializers = true;
        private static bool _canAddReleasers = true;
        private static bool _requestedHalt = false;
        private static bool _stopThreads = false;
        private static EngineSetup _setup = EngineSetup.Default;
        private static IWindow? _window;
        private static Thread? _renderThread;
        private static readonly List<Func<bool>> _initializers = new();
        private static readonly List<Func<bool>> _releasers = new();
        private static readonly List<Func<bool>> _externalInitializers = new();
        private static readonly List<Func<bool>> _externalReleasers = new();

        public static IReadOnlyEngineSetup Setup => _setup;
        public static bool IsRunning { get; private set; }

        static GameEngine()
        {
            LoggingService.Use(new ConsoleLogger());

            _initializers.AddRange(new Func<bool>[]
            {
                Graphics.Initialize,
                SystemManager.InitializeWithDefaultSystems,
                InitializeGameAssets
            });

            _releasers.AddRange(new Func<bool>[]
            {
                ReleaseThreads,
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
                LoggingService.Fatal("An internal initializer returned an error!");
                return true;
            }
            LoggingService.Debug("Finished core initializers.", _externalInitializers.Count);

            if (!_externalInitializers.Any())
            {
                _isInitialized = true;
                return false;
            }

            LoggingService.Debug("{0} external initializer delegate(s) queued.", _externalInitializers.Count);
            var externalInitializersResult = _externalInitializers.Any(initializer => initializer());
            _isInitialized = true;
            AppDomain.CurrentDomain.UnhandledException += LoggingService.OnUnhandledException;
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

        private static bool InitializeGameAssets()
        {
            Resources.UsePack(Setup.AssetsFile);
            return false;
        }

        private static bool ReleaseGameAssets()
        {
            Resources.ClosePack();
            return false;
        }

        private static bool ReleaseThreads()
        {
            _stopThreads = true;
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
            if (_requestedHalt && !IsRunning)
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

        public static void Run()
        {
            var windowOptions = WindowOptions.Default;
            windowOptions.Size = _setup.WindowSize!.Value;
            windowOptions.Title = _setup.InitialWindowTitle ?? "MiniEngine Game";
            windowOptions.FramesPerSecond = 0;
            windowOptions.VSync = false;
            windowOptions.ShouldSwapAutomatically = true;
            _window = Window.Create(windowOptions);
            _window.Update += WindowOnUpdate;
            _window.Render += WindowOnRender;
            _window.Load += WindowOnLoad;
            _window.Closing += WindowOnClosing;
            _renderThread = new Thread(() =>
            {
                _window.Initialize();
                while (!_stopThreads)
                {
                    _window.DoEvents();
                    if (!_window.IsClosing)
                        _window.DoRender();
                }
            });
            Graphics.SetWindow(_window);
            _renderThread.Start();
            IsRunning = true;

            while (IsRunning)
            {
                if (!_window!.IsInitialized)
                    continue;
                _window.DoUpdate();
                if (_requestedHalt)
                {
                    if (!_window!.IsClosing)
                    {
                        _window?.Close();
                        _window?.Reset();
                    }

                    IsRunning = false;
                }
            }
            GracefulExit();
        }

        private static void WindowOnClosing()
        {
            RequestHalt();
        }

        private static void WindowOnLoad()
        {
            _canAddInitializers = false;
            if (Initialize())
            {
                LoggingService.Fatal("GameEngine could not be initialized.");
                FatalExit(1);
            }
            IsRunning = true;
        }

        private static void WindowOnRender(double obj)
        {
            Graphics.Clear();
            LoggingService.Debug("Render");
        }

        private static void WindowOnUpdate(double obj)
        {
            LoggingService.Debug("Update");
        }
    }
}
