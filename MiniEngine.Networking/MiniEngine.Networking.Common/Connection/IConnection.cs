namespace MiniEngine.Networking.Common;

public interface IConnection
{
    public event Action<object> OnMessageReceive;
    public void SendMessage(object message);
}
