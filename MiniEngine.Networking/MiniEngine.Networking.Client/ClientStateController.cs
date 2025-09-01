namespace MiniEngine.Networking.Client
{
    public struct ConnectionState
    {
        public bool IsConnected;
        public string? Host;
    }
    public sealed class ClientStateController: Component
    {
        public ConnectionState ConnectionState;
        internal readonly Queue<ClientSyncSystem.ClientSyncTask> Tasks = [];

        public event Action<ClientStateController, bool>? OnConnect;
        public event Action<ClientStateController, bool>? OnDisconnect;
        public event Action<ClientStateController, bool>? OnPingReceived;

        public void StartConnect()
        {
            Tasks.Enqueue(
                new ClientSyncSystem.ClientSyncTask(ClientSyncSystem.Task.Connect)
            );
        }
        public void StartConnect(string hostname, ushort port)
        {
            Tasks.Enqueue(
                new ClientSyncSystem.ClientSyncTask(ClientSyncSystem.Task.Connect,
                new Dictionary<string, object>()
                    {
                        ["hostname"] = hostname,
                        ["port"] = port,
                    }
                )
            );
        }
        public void StartConnect(IDictionary<string, object> connectionParameters)
        {
            Tasks.Enqueue(
                new ClientSyncSystem.ClientSyncTask(ClientSyncSystem.Task.Connect, connectionParameters)
            );
        }

        public void StartDisconnect()
        {
            Tasks.Enqueue(
                new ClientSyncSystem.ClientSyncTask(ClientSyncSystem.Task.Disconnect)
            );
        }

        public void StartPing()
        {
            Tasks.Enqueue(
                new ClientSyncSystem.ClientSyncTask(ClientSyncSystem.Task.Ping)
            );
        }
    }
}
