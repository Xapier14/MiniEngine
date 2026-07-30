using MiniEngine.Networking.Common;
using MiniEngine.Utility;

namespace MiniEngine.Networking.Client;

[HandlesComponent<ClientStateController>]
public class ClientSyncSystem : System
{
    private ConnectionState _connectionState = new ConnectionState();
    private IConnection? _connection;
    private readonly IConnectionStrategy _connectionStrategy;

    public enum Task
    {
        Connect,
        Disconnect,
        Ping,
        Send,
    }

    public record ClientSyncTask(Task Task, IDictionary<string, object>? TaskParameters = null);

    /// <summary>
    /// Default constructor, used when the system is registered by type. Talks to a remote host.
    /// </summary>
    public ClientSyncSystem() : this(null)
    {
    }

    /// <summary>
    /// Register with an explicit strategy via
    /// <c>ecs.Register(new ClientSyncSystem(strategy))</c> to override how connections
    /// are made, for example with a loopback for in-process client/server.
    /// </summary>
    public ClientSyncSystem(IConnectionStrategy? connectionStrategy)
    {
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
                case Task.Send:
                    // The system owns the connection, so sending stays here rather than in
                    // an entity reaching out for a strategy.
                    if (syncTask.TaskParameters?.TryGetValue("message", out var message) == true)
                        _connection?.SendMessage(message);
                    break;
            }
            // after task completion, update with latest connection state
            controller.ConnectionState = _connectionState;
        }
    }
}
