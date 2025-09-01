namespace MiniEngine.Networking.Common;

public class LoopbackMediator
{
    private readonly InternalConnection _clientConnection;
    private readonly InternalConnection _serverConnection;

    public IConnection ClientConnection => _clientConnection;
    public IConnection ServerConnection => _serverConnection;

    public LoopbackMediator()
    {
        _clientConnection = new InternalConnection();
        _serverConnection = new InternalConnection();
        _clientConnection.TargetConnection = _serverConnection;
        _serverConnection.TargetConnection = _clientConnection;
    }
}
