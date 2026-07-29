using System.IO;

namespace MiniEngine
{
    public class SceneManager(EcsManager ecsManager)
    {
        private Scene? _currentScene;

        public Scene? CurrentScene
        {
            get => _currentScene;
            set
            {
                if (_currentScene != null)
                {
                    _currentScene.EntityAdded -= OnEntityAdded;
                    _currentScene.EntityRemoved -= OnEntityRemoved;
                }

                _currentScene = value;
                ecsManager.ActiveScene = value;
                ecsManager.PurgeComponents();
                if (_currentScene == null)
                    return;

                _currentScene.EntityAdded += OnEntityAdded;
                _currentScene.EntityRemoved += OnEntityRemoved;
                foreach (var entity in _currentScene)
                {
                    ecsManager.RegisterEntity(entity);
                }
            }
        }

        private void OnEntityAdded(Entity entity)
            => ecsManager.RegisterEntity(entity);

        private void OnEntityRemoved(Entity entity)
            => ecsManager.RemoveEntity(entity);

        public Scene BuildScene(MemoryResource xmlMemoryResource)
        {
            using var stream = xmlMemoryResource.CreateStream();
            return SceneBuilder.BuildScene(stream);
        }

        public Scene BuildSceneXml(string xmlFilePath)
        {
            using var stream = File.OpenRead(xmlFilePath);
            return SceneBuilder.BuildScene(stream);
        }
    }
}
