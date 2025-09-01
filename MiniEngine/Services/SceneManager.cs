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
                _currentScene = value;
                ecsManager.PurgeComponents();
                if (_currentScene == null)
                    return;
                foreach (var entity in _currentScene)
                {
                    ecsManager.RegisterEntity(entity);
                }
            }
        }

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
