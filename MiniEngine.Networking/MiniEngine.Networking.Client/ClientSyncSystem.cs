using MiniEngine.Networking.Common;
using MiniEngine.Utility;

namespace MiniEngine.Networking.Client;

[HandlesComponent<ClientStateController>]
public class ClientSyncSystem : System
{
    private ConnectionState _connectionState = new ConnectionState();
    private IConnection? _connection;
    private IConnectionStrategy _connectionStrategy;
    public enum Task
    {
        Connect,
        Disconnect,
        Ping,
    }

    public record ClientSyncTask(Task Task, IDictionary<string, object>? TaskParameters = null);

    public ClientSyncSystem()
    {
        object? strategyObject = null;
        GameContext.GameEngine?.Setup.AdditionalConfiguration?.TryGetValue("connection_strategy",
            out strategyObject);
        var connectionStrategy = (IConnectionStrategy?)strategyObject;
        _connectionStrategy = connectionStrategy ?? new RemoteConnectionStrategy();
    }

    public void OnComponentRegister(ClientStateController controller)
    {
        // update ClientStateController with latest connection state
        controller.ConnectionState = _connectionState;
    }

    public void HandleComponent(ClientStateController controller)
    {
        // update ClientStateController with latest connection state
        controller.ConnectionState = _connectionState;

        // handle each client state task
        while (controller.Tasks.TryDequeue(out var syncTask))
        {
            switch (syncTask.Task)
            {
                case Task.Connect:
                    LoggingService.Info("[ClientStateController] Connecting via \"{0}\".", _connectionStrategy.GetType().Name);
                    _connection = _connectionStrategy.CreateConnection(syncTask.TaskParameters ?? (Dictionary<string, object>)[]);
                    _connection!.OnMessageReceive += (message) => Console.WriteLine("received from server: {0}", message);
                    LoggingService.Info("[ClientStateController] Established connection.");
                    break;
                case Task.Disconnect:

                    break;
                case Task.Ping:

                    break;
            }
            // after task completion, update with latest connection state
            controller.ConnectionState = _connectionState;
        }
    }
}
