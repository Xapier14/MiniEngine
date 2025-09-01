namespace MiniEngine.Networking.Common;

public class LoopbackConnectionStrategy : IConnectionStrategy
{
    private readonly LoopbackMediator _loopbackMediator = new();

    public IConnection ExposedConnection => _loopbackMediator.ServerConnection;
    
    public IConnection CreateConnection(IDictionary<string, object> data) => _loopbackMediator.ClientConnection;
}