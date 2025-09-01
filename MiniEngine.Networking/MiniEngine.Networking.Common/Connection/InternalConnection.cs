namespace MiniEngine.Networking.Common
{
    internal class InternalConnection : IConnection
    {
        public InternalConnection? TargetConnection { get; set; }

        public event Action<object>? OnMessageReceive;
        public void SendMessage(object message)
        {
            TargetConnection?.OnMessageReceive?.Invoke(message);
        }
    }
}
