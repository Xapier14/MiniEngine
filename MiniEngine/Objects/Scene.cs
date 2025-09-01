using System;
using MiniEngine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiniEngine
{
    public class Scene : IEnumerable<Entity>
    {
        private readonly List<Entity> _entities = new();

        public Vector2F ViewPosition = Vector2F.Zero;
        public float ViewRotation = 0f;
        public Color BackgroundColor = Color.Black;
        public Entity? FollowEntity { get; set; }

        public bool ContainsEntity(Entity entity)
            => entity.ParentScene == this && _entities.Contains(entity);

        public T CreateEntity<T>(params object[] args) where T : Entity
        {
            var entity = EntityFactory.TryCreateEntity<T>(args);
            if (entity == null)
            {
                GameContext.GetGameEngine().FatalExit(100);
                return default(T)!;
            }
            AddEntity(entity);
            return entity;
        }

        public void AddEntity(Entity entity, GameEngine? gameEngine = null)
        {
            if (entity.Destroyed)
            {
                LoggingService.Error("Cannot add destroyed entity {0} to scene.", entity.Id);
                return;
            }
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

            if (GameContext.GetGameEngine().SceneManager.CurrentScene == this)
                GameContext.GetGameEngine().EcsManager.RegisterEntity(entity);

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

            if (GameContext.GetGameEngine().SceneManager.CurrentScene == this)
                GameContext.GetGameEngine().EcsManager.RemoveEntity(entity);

            entity.ParentScene = null;
            _entities.Remove(entity);
        }

        public IEnumerable<Entity> GetEntity<T>() where T : Entity
            => GetEntity(typeof(T));

        public IEnumerable<Entity> GetEntity(Type entityType)
            => _entities.Where(e => e.GetType() == entityType);

        public Entity? GetEntity(string name)
            => _entities.FirstOrDefault(entity => entity.Name == name);

        public T? GetEntity<T>(string name) where T : Entity
            => (T?)_entities.Where(entity => entity.GetType() == typeof(T))
                .FirstOrDefault(entity => entity.Name == name);

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
