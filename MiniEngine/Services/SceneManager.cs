namespace MiniEngine
{
    public static class SceneManager
    {
        private static Scene? _currentScene;

        public static Scene? CurrentScene
        {
            get => _currentScene;
            set
            {
                _currentScene = value;
                SystemManager.PurgeComponents();
                if (_currentScene == null)
                    return;
                foreach (var entity in _currentScene)
                {
                    SystemManager.RegisterEntity(entity);
                }
            }
        }
    }
}
