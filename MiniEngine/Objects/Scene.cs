using MiniEngine.Utility;
using System.Collections;
using System.Collections.Generic;

namespace MiniEngine
{
    public class Scene : IEnumerable<Entity>
    {
        private readonly List<Entity> _entities = new();

        public Vector2F ViewPosition = Vector2F.Zero;

        public bool ContainsEntity(Entity entity)
            => entity.ParentScene == this && _entities.Contains(entity);

        public T CreateEntity<T>(params object[] args) where T : Entity
        {
            var entity = Factory.TryCreateEntity<T>(args);
            if (entity == null)
            {
                GameEngine.FatalExit(100);
                return default(T)!;
            }
            AddEntity(entity);
            return entity;
        }

        public void AddEntity(Entity entity)
        {
            if (ContainsEntity(entity))
            {
                LoggingService.Error("Entity {0} already exists in scene.", entity.Id);
                return;
            }

            if (entity.ParentScene != null)
            {
                LoggingService.Error("Entity {0} is already part of another scene.", entity.Id);
                return;
            }

            if (SceneManager.CurrentScene == this)
                SystemManager.RegisterEntity(entity);

            entity.ParentScene = this;
            _entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (!ContainsEntity(entity))
            {
                LoggingService.Error("Entity {0} is not in scene.", entity.Id);
                return;
            }

            if (SceneManager.CurrentScene == this)
                SystemManager.RemoveEntity(entity);

            entity.ParentScene = null;
            _entities.Remove(entity);
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
