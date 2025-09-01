namespace MiniEngine.Networking.Common;

public interface IConnectionStrategy
{
    public IConnection? CreateConnection(IDictionary<string, object> data);
}