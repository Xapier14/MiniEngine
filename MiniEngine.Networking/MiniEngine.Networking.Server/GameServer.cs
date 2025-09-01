namespace MiniEngine.Networking.Server;

public class GameServer
{
    private readonly EcsManager _ecs = new();
    private readonly TaskScheduler _scheduler = new();
    public event Action<IConnection> OnClientConnect;

    private void SetupTasks()
    {
        // 31.25 ticks / s
        _scheduler.ScheduleNow(ExecuteServerTick, 32);
    }
    
    public GameServer()
    {
        SetupTasks();
        
        _ecs.Register(typeof(ScriptSystem));
        _ecs.Register(typeof(PhysicsSystem));
    }

    public void AcceptConnection(IConnection connection)
    {

    }

    public void Start()
    {
        while (true)
        {
            _scheduler.UpdateAndExecute();

            // read client states
            _ecs.ProcessSystems();
            // update client states
        }
    }

    private void ExecuteServerTick()
    {
        
    }
}