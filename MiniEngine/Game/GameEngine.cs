using MiniEngine.Utility;
using MiniEngine.Utility.Logging;
using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static SDL2.SDL;
using static SDL2.SDL_image;
using static SDL2.SDL_ttf;
using static SDL2.SDL_mixer;

namespace MiniEngine
{
    public class GameEngine
    {
        private bool _isInitialized;
        private bool _isReleased;
        private bool _sdlInitialized;
        private bool _canAddInitializers = true;
        private bool _canAddReleasers = true;
        private bool _requestedHalt;
        private EngineSetup _setup = EngineSetup.Default;
        private readonly List<Func<bool>> _initializers = [];
        private readonly List<Func<bool>> _releasers = [];
        private readonly List<Func<bool>> _externalInitializers = [];
        private readonly List<Func<bool>> _externalReleasers = [];

        // services
        private TaskScheduler? _taskScheduler;
        public TaskScheduler TaskScheduler
        {
            get
            {
                if (_taskScheduler == null)
                    throw new EngineNotInitializedException();
                return _taskScheduler;
            }
            private set => _taskScheduler = value;
        }

        private EcsManager? _ecsManager;
        public EcsManager EcsManager
        {
            get
            {
                if (_ecsManager == null)
                    throw new EngineNotInitializedException();
                return _ecsManager;
            }
            private set => _ecsManager = value;
        }
        private AudioService? _audioService;
        public AudioService AudioService
        {
            get
            {
                if (_audioService == null)
                    throw new EngineNotInitializedException();
                return _audioService;
            }
            private set => _audioService = value;
        }

        private SceneManager? _sceneManager;
        public SceneManager SceneManager
        {
            get
            {
                if (_sceneManager == null)
                    throw new EngineNotInitializedException();
                return _sceneManager;
            }
            private set => _sceneManager = value;
        }

        private InputManager? _inputManager;
        public InputManager InputManager
        {
            get
            {
                if (_inputManager == null)
                    throw new EngineNotInitializedException();
                return _inputManager;
            }
            private set => _inputManager = value;
        }

        private WindowManager? _windowManager;
        public WindowManager WindowManager
        {
            get
            {
                if (_windowManager == null)
                    throw new EngineNotInitializedException();
                return _windowManager;
            }
            private set => _windowManager = value;
        }

        private Graphics? _graphics;
        public Graphics Graphics
        {
            get
            {
                if (_graphics == null)
                    throw new EngineNotInitializedException();
                return _graphics;
            }
            private set => _graphics = value;
        }

        public IReadOnlyEngineSetup Setup => _setup;
        public bool IsRunning { get; private set; }

        public bool Initialize()
        {
            if (_isInitialized)
                return false;

            LoggingService.Use(new ConsoleLogger());

            DllMap.RegisterDllMap();

            // AudioService.InitializeDevice below calls into SDL_mixer, so SDL must already
            // be up regardless of whether Initialize() was reached via Run() or called directly.
            if (InitializeSDL())
            {
                LoggingService.Fatal("GameEngine could not be initialized (SDL initialization error).");
                return true;
            }

            TaskScheduler = new TaskScheduler();
            EcsManager = new EcsManager(new WorldClock(1.0 / _setup.FixedTickRate.GetValueOrDefault(30f)));
            EcsManager.Clock.TimeScale = _setup.GameSpeed.GetValueOrDefault(1f);
            AudioService = new AudioService(TaskScheduler);
            SceneManager = new SceneManager(EcsManager);
            InputManager = new InputManager(this, SceneManager);
            WindowManager = new WindowManager(this, InputManager);
            Graphics = new Graphics(this);

            _initializers.AddRange([
                AudioService.InitializeDevice,
                InitializeDefaultSystems,
                InitializeGameAssets
            ]);

            _releasers.AddRange([
                ReleaseComponents,
                AudioService.CleanupCache,
                ReleaseGameAssets
            ]);

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

        private bool InitializeExternal()
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

        private bool Release()
        {
            if (_isReleased)
                return false;

            // SDL teardown runs in the finally so a failing releaser cannot leak this
            // engine's reference to the process-wide SDL init.
            try
            {
                var coreReleasersResult = _releasers.Any(releaser => releaser());
                if (coreReleasersResult)
                {
                    LoggingService.Fatal("An internal releaser returned an error!");
                    return true;
                }

                if (!_externalReleasers.Any())
                    return false;

                LoggingService.Debug("{0} external releaser delegate(s) queued.", _externalReleasers.Count);
                return _externalReleasers.Any(releaser => releaser());
            }
            finally
            {
                _isReleased = true;
                ReleaseSDL();
            }
        }

        // SDL's init/quit calls are process-global, so several engine instances in one
        // process have to share them. First one in initialises, last one out tears down.
        private static readonly object _sdlLock = new();
        private static int _sdlRefCount;

        private bool InitializeSDL()
        {
            if (_sdlInitialized)
                return false;

            lock (_sdlLock)
            {
                if (_sdlRefCount > 0)
                {
                    _sdlRefCount++;
                    _sdlInitialized = true;
                    return false;
                }

                if (InitializeSDLSubsystems())
                    return true;

                _sdlRefCount++;
                _sdlInitialized = true;
                return false;
            }
        }

        private void ReleaseSDL()
        {
            if (!_sdlInitialized)
                return;
            _sdlInitialized = false;

            lock (_sdlLock)
            {
                _sdlRefCount--;
                if (_sdlRefCount > 0)
                    return;

                Mix_CloseAudio();
                Mix_Quit();
                TTF_Quit();
                IMG_Quit();
                SDL_Quit();
                LoggingService.Info("Released SDL2.");
            }
        }

        private bool InitializeSDLSubsystems()
        {
            if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
            {
                LoggingService.Fatal("Error initializing SDL2.");
                LoggingService.Fatal(SDL_GetError());
                return true;
            }

            const IMG_InitFlags imgFlags = IMG_InitFlags.IMG_INIT_PNG | IMG_InitFlags.IMG_INIT_JPG;
            if ((IMG_InitFlags)IMG_Init(imgFlags) != imgFlags)
            {
                LoggingService.Fatal("Error initializing SDL_image.");
                LoggingService.Fatal(IMG_GetError());
                return true;
            }

            if (TTF_Init() != 0)
            {
                LoggingService.Fatal("Error initializing SDL_ttf.");
                LoggingService.Fatal(TTF_GetError());
                return true;
            }

            const MIX_InitFlags mixFlags = MIX_InitFlags.MIX_INIT_OGG | MIX_InitFlags.MIX_INIT_OPUS | MIX_InitFlags.MIX_INIT_MP3;
            if (Mix_Init(mixFlags) == 0)
            {
                LoggingService.Fatal("Error initializing SDL_mixer");
                return true;
            }

            LoggingService.Info("Initialized SDL2.");
            return false;
        }

        /// <summary>
        /// Builds the engine's default system pipeline. Systems needing engine services are
        /// constructed here and registered by instance; the rest the ECS can build itself.
        /// </summary>
        private bool InitializeDefaultSystems()
        {
            try
            {
                LoggingService.Debug("Initializing default ECS systems...");
                EcsManager.Reset();

                // Update events and physics are simulation: they step at a fixed rate so a
                // tick covers the same amount of time everywhere. Draw events, animation
                // and drawing are presentation and run once per frame.
                EcsManager.RegisterAfter<MiniEngine.System>(typeof(ScriptSystem), ScriptEvent.BeforeUpdate, Cadence.Fixed);
                EcsManager.RegisterAfter<ScriptSystem>(ScriptEvent.BeforeUpdate, typeof(ScriptSystem), ScriptEvent.Update, Cadence.Fixed);
                EcsManager.RegisterAfter<ScriptSystem>(ScriptEvent.Update, typeof(ScriptSystem), ScriptEvent.AfterUpdate, Cadence.Fixed);
                EcsManager.RegisterAfter<ScriptSystem>(new PhysicsSystem(Setup), null, Cadence.Fixed);
                EcsManager.RegisterAfter<PhysicsSystem>(typeof(AnimationSystem));
                EcsManager.RegisterAfter<AnimationSystem>(typeof(ScriptSystem), ScriptEvent.BeforeDraw);
                EcsManager.RegisterAfter<ScriptSystem>(new DrawSystem(this));
                EcsManager.RegisterAfter<DrawSystem>(ScriptEvent.BeforeDraw, typeof(ScriptSystem), ScriptEvent.AfterDraw);
                LoggingService.Debug("All default ECS OK.");
            }
            catch (Exception ex)
            {
                LoggingService.Fatal("Couldn't initialize default ECS systems.", ex);
                return true;
            }

            return false;
        }

        private bool InitializeGameAssets()
        {
            if (Setup.AssetsFile != null)
                Resources.UsePack(Setup.AssetsFile);
            return false;
        }

        private bool ReleaseGameAssets()
        {
            Resources.ClosePacks();
            Graphics.ReleaseResources();
            return false;
        }

        private bool ReleaseComponents()
        {
            EcsManager.PurgeComponents();
            return false;
        }

        private void GracefulExit()
        {
            _canAddReleasers = false;
            if (Release())
                FatalExit(-1);
        }

        /// <summary>
        /// Stops and releases this engine, then throws. Does not end the process: another
        /// engine instance, or a host application such as an editor, may still be running.
        /// Catch <see cref="EngineFatalException"/> at your entry point if you want the
        /// process to exit.
        /// </summary>
        /// <exception cref="EngineFatalException">Always.</exception>
        public void FatalExit(int exitCode)
        {
            LoggingService.Fatal("GameEngine has run into a fatal error and will shut down.");
            IsRunning = false;
            _canAddReleasers = false;
            Release();
            throw new EngineFatalException($"GameEngine exited with code {exitCode}.", exitCode);
        }

        public void FatalExit(ExitCode exitCode)
            => FatalExit((int)exitCode);

        public void RequestHalt()
        {
            if (_requestedHalt)
                return;
            _requestedHalt = true;
            LoggingService.Debug("Requested GameEngine halt.");
        }

        public void UseSetup(params EngineSetup[] additionalSetups)
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

        public void AddInitializer(Func<bool> initializerDelegate)
        {
            if (!_canAddInitializers)
            {
                LoggingService.Error("Cannot add initializer delegate. Engine is already initializing!");
                return;
            }
            _externalInitializers.Add(initializerDelegate);
        }

        public void AddInitializer(Action initializerDelegate)
        {
            if (!_canAddInitializers)
            {
                LoggingService.Error("Cannot add initializer delegate. Engine is already initializing!");
                return;
            }
            _externalInitializers.Add(() =>
            {
                initializerDelegate();

                return false;
            });
        }

        public void AddReleaser(Func<bool> releaserDelegate)
        {
            if (!_canAddReleasers)
            {
                LoggingService.Error("Cannot add releaser delegate. Engine is already releasing!");
                return;
            }
            _externalReleasers.Add(releaserDelegate);
        }

        public bool Run()
        {
            _canAddInitializers = false;
            if (InitializeSDL())
            {
                LoggingService.Fatal("GameEngine could not be initialized (SDL initialization error).");
                return true;
            }
            IsRunning = true;

            if (_setup.DisableCursor == true)
                _ = SDL_ShowCursor(SDL_DISABLE);
            if (Initialize() || InitializeExternal())
            {
                LoggingService.Fatal("GameEngine could not be initialized.");
                return true;
            }
            var initialWindowTitle = _setup.InitialWindowTitle ?? "MiniEngine Game Window";
            WindowManager.CreateWindow(initialWindowTitle);

            var frameStopwatch = new Stopwatch();
            while (IsRunning)
            {
                frameStopwatch.Restart();

                Graphics.Tidy();
                InputManager.UpdateState();
                WindowManager.PumpEvents();
                EcsManager.Update();
                TaskScheduler.UpdateAndExecute();

                LimitFrameRate(frameStopwatch);

                if (!_requestedHalt)
                    continue;
                IsRunning = false;
                LoggingService.Debug("GameEngine halted.");
            }

            GracefulExit();
            return false;
        }

        /// <summary>
        /// Gives the rest of the frame back to the operating system when the setup asks for
        /// an fps limit. Sleeps only whole milliseconds, so the loop runs a shade fast
        /// rather than overshooting the frame it was asked to fit in.
        /// </summary>
        private void LimitFrameRate(Stopwatch frameStopwatch)
        {
            var limit = _setup.FpsLimit.GetValueOrDefault(0f);
            if (limit <= 0f)
                return;

            var remainingMs = 1000.0 / limit - frameStopwatch.Elapsed.TotalMilliseconds;
            if (remainingMs >= 1.0)
                Thread.Sleep((int)remainingMs);
        }
    }
}
