using MiniEngine.Components;
using MiniEngine.Networking.Common;
using MiniEngine.Utility;
using Timer = MiniEngine.Utility.Bits.Timer;

namespace MiniEngine.Networking.Client;

public class ClientEntity : Entity
{
    private Timer _timer = new Timer(1000);
    
    [Inject]
    public ClientEntity(ClientStateController clientStateController, Scriptable scriptable)
    {
        Name = "ClientEntity";
        scriptable.Update += Update;
        AddComponent(scriptable);
        AddComponent(clientStateController);
    }

    private void Update(Scriptable obj)
    {
        if (_timer.UpdateAndCheckElapsed())
        {
            LoggingService.Info("Simulating server send...");
            var strategy = GameContext.GameEngine!.Setup.AdditionalConfiguration!["connection_strategy"] as LoopbackConnectionStrategy;
            strategy?.ExposedConnection.SendMessage("test");
        }
    }
}