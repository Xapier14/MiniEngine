using MiniEngine.Networking.Common.Replication;
using MiniEngine.Utility;

namespace MiniEngine.Networking.Server;

/// <summary>
/// A headless world that owns the simulation. No <see cref="GameEngine"/> and no SDL: the
/// server runs the same systems a client does, minus everything that draws.
/// </summary>
public class GameServer
{
    private readonly EcsManager _ecs;
    private readonly TaskScheduler _scheduler = new();
    private readonly ReplicationSystem _replication;
    private volatile bool _running;

    /// <summary>Raised when a client's connection has been accepted.</summary>
    public event Action<IConnection>? OnClientConnect;

    public EcsManager Ecs => _ecs;
    public ReplicationSystem Replication => _replication;
    public SceneManager Scenes { get; }

    /// <param name="tickRate">Simulation steps per second.</param>
    public GameServer(float tickRate = 30f)
    {
        _ecs = new EcsManager(new WorldClock(1.0 / tickRate));

        // A swallowed handler exception on a client costs one bad frame. Here it would
        // corrupt the state every client is being told to trust, so fail loudly instead.
        _ecs.ThrowOnHandlerException = true;

        _replication = new ReplicationSystem(ReplicationMode.Server);
        EngineSchemas.RegisterAll();

        // Simulation only. Nothing here draws, so nothing runs at frame cadence.
        _ecs.RegisterAfter<MiniEngine.System>(typeof(ScriptSystem), null, Cadence.Fixed);
        _ecs.RegisterAfter<ScriptSystem>(new PhysicsSystem(), null, Cadence.Fixed);
        _ecs.RegisterAfter<PhysicsSystem>(_replication, null, Cadence.Fixed);

        Scenes = new SceneManager(_ecs);
    }

    public void AcceptConnection(IConnection connection)
    {
        OnClientConnect?.Invoke(connection);
    }

    /// <summary>
    /// Runs the simulation until <see cref="Stop"/>. Blocks the calling thread.
    /// </summary>
    public void Start()
    {
        _running = true;
        LoggingService.Info("[GameServer] Starting at {0} ticks/s.", 1.0 / _ecs.Clock.FixedDelta);

        while (_running)
        {
            var tickBefore = _ecs.Clock.Tick;

            _ecs.Update();
            _scheduler.UpdateAndExecute();

            if (_ecs.Clock.Tick != tickBefore)
                ExecuteServerTick();

            // Nothing left to simulate until more time has passed. Without this the loop
            // spins a core flat out to discover that repeatedly.
            SleepUntilNextTick();
        }

        LoggingService.Info("[GameServer] Stopped after {0} tick(s).", _ecs.Clock.Tick);
    }

    public void Stop()
        => _running = false;

    /// <summary>
    /// Runs a fixed number of steps and returns. For tests and for hosting the server
    /// inside another loop rather than giving it a thread of its own.
    /// </summary>
    public void StepOnce()
    {
        var tickBefore = _ecs.Clock.Tick;
        _ecs.Update();
        _scheduler.UpdateAndExecute();
        if (_ecs.Clock.Tick != tickBefore)
            ExecuteServerTick();
    }

    private void SleepUntilNextTick()
    {
        // Alpha is how far into the current step the accumulator has got, so the rest of
        // the step is what is left to wait. Whole milliseconds only, which wakes fractionally
        // early rather than overshooting the tick.
        var remainingMs = (1.0 - _ecs.Clock.Alpha) * _ecs.Clock.FixedDelta * 1000.0;
        if (remainingMs >= 1.0)
            Thread.Sleep((int)remainingMs);
    }

    private void ExecuteServerTick()
    {
        // Where a snapshot is built and sent. The spawns and despawns accumulated by the
        // replication system during this tick are drained here in Phase 2.
    }
}
