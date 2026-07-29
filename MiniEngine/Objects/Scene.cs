using System;
using MiniEngine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MiniEngine
{
    public class Scene : IEnumerable<Entity>, IFormatProvider
    {
        private readonly List<Entity> _entities = new();

        public Vector2F ViewPosition = Vector2F.Zero;
        public float ViewRotation = 0f;
        public Color BackgroundColor = Color.Black;
        public Entity? FollowEntity { get; set; }

        /// <summary>
        /// Raised when an entity joins the scene. The <see cref="SceneManager"/> subscribes
        /// while this scene is current, so the scene never has to know about the ECS.
        /// </summary>
        public event Action<Entity>? EntityAdded;

        /// <summary>
        /// Raised when an entity leaves the scene.
        /// </summary>
        public event Action<Entity>? EntityRemoved;

        public bool ContainsEntity(Entity entity)
            => _entities.Contains(entity);

        public T CreateEntity<T>(params object[] args) where T : Entity
        {
            var entity = EntityFactory.TryCreateEntity<T>(args)
                ?? throw new EntityCreationFailException($"Failed creating instance of entity {typeof(T).Name}.");
            AddEntity(entity);
            return entity;
        }

        public void AddEntity(Entity entity)
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

            if (entity.IsAttached)
            {
                LoggingService.Error("Entity {0} is already part of another scene.", entity.Id);
                return;
            }

            entity.IsAttached = true;
            entity.Destroying+= OnEntityDestroyed;
            _entities.Add(entity);
            EntityAdded?.Invoke(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (!ContainsEntity(entity))
            {
                LoggingService.Error("Entity {0} is not in scene.", entity.Id);
                return;
            }

            entity.Destroying-= OnEntityDestroyed;
            entity.IsAttached = false;
            _entities.Remove(entity);
            if (FollowEntity == entity)
                FollowEntity = null;
            EntityRemoved?.Invoke(entity);
        }

        private void OnEntityDestroyed(Entity entity)
            => RemoveEntity(entity);

        /// <summary>
        /// Hands this scene to <see cref="Entity.TryParse"/> so entity references in scene
        /// XML resolve against the scene being built.
        /// </summary>
        public object? GetFormat(Type? formatType)
            => formatType == typeof(Scene) ? this : null;

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
