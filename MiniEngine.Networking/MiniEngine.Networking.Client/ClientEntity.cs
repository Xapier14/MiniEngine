using MiniEngine.Components;
using MiniEngine.Networking.Common;
using MiniEngine.Utility;
using Timer = MiniEngine.Utility.Bits.Timer;

namespace MiniEngine.Networking.Client;

public class ClientEntity : Entity
{
    private Timer _timer = new Timer(1000);
    private readonly ClientStateController _clientStateController;

    [Inject]
    public ClientEntity(ClientStateController clientStateController, Scriptable scriptable)
    {
        Name = "ClientEntity";
        _clientStateController = clientStateController;
        scriptable.Update += Update;
        AddComponent(scriptable);
        AddComponent(clientStateController);
    }

    private void Update(Scriptable obj)
    {
        if (_timer.UpdateAndCheckElapsed())
        {
            LoggingService.Info("Sending periodic client message...");
            _clientStateController.Send("test");
        }
    }
}