namespace MiniEngine.Networking.Client
{
    public class RemoteScene: Scene
    {
        // we want the server to dynamically generate the scene for us
        public ClientStateController Client =>
            GetEntity("ClientEntity")!.GetComponent<ClientStateController>()!;
        public RemoteScene()
        {
            var clientEntity = EntityFactory.TryCreateEntity<ClientEntity>()
                ?? throw new EntityCreationFailException("Failed creating instance of entity ClientEntity.");
            AddEntity(clientEntity);
        }
    }
}
