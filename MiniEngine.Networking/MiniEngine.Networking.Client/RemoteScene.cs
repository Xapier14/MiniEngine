namespace MiniEngine.Networking.Client
{
    public class RemoteScene: Scene
    {
        // we want the server to dynamically generate the scene for us
        public ClientStateController Client =>
            GetEntity("ClientEntity")!.GetComponent<ClientStateController>()!;
        public RemoteScene()
        {
            var clientEntity = EntityFactory.TryCreateEntity<ClientEntity>();
            if (clientEntity == null)
            {
                GameContext.GameEngine?.FatalExit(-1);
                return;
            }
            AddEntity(clientEntity, GameContext.GameEngine);
        }
    }
}
