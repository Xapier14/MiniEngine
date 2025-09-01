using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniEngine
{
    public abstract class Entity : IParsable<Entity?>
    {
        public Scene? ParentScene { get; internal set; }
        private readonly List<Component> _components = new();
        private bool _destroyed;
        private readonly Guid _guid = Guid.NewGuid();

        public string Id => $"{GetType()}#{Name ?? _guid.ToString()}";
        public string? Name { get; set; }
        public bool Destroyed => _destroyed;

        internal IReadOnlyList<Component> GetComponents()
            => _components;

        protected internal void AddComponent(Component component)
        {
            var requiredComponents = component.GetRequiredComponents().ToArray();
            var isMissingRequiredComponent = requiredComponents
                .Any(type => !_components.Any(ownedComponent => ownedComponent.GetType() == type));
            if (isMissingRequiredComponent)
            {
                LoggingService.Fatal("Tried to add component {0} to entity {1} but does not have the required component(s).",
                    component.GetType(),
                    Id);
                LoggingService.Fatal("Requires: {0}.",
                    string.Join(';', requiredComponents.Select(type => type.Name)));
                LoggingService.Fatal("Has: {0}.",
                    _components.Any() ?
                        string.Join(';', _components.Select(hasComponent => hasComponent.GetType().Name))
                        : "<empty>");
                GameContext.GetGameEngine().FatalExit(ExitCode.EntityCreationError);
                return;
            }
            component.SetOwnerIfNotNull(this);
            _components.Add(component);
        }

        public void AddComponent<T>() where T : Component, new()
        {
            if (_components.Any(component => component.GetType() == typeof(T)))
            {
                LoggingService.Error("Tried to add component {0} to entity {1} but the same component already exists.",
                    typeof(T),
                    Id);
                return;
            }

            var newComponent = new T();
            AddComponent(newComponent);
        }

        public T GetComponent<T>() where T : IComponent
            => (T)GetComponent(typeof(T));

        public T? TryGetComponent<T>() where T : IComponent
            => (T?)TryGetComponent(typeof(T));

        public IEnumerable<T> GetComponents<T>() where T : IComponent
            => GetComponents(typeof(T)).Cast<T>();

        public IComponent GetComponent(Type componentType)
        {
            var component = TryGetComponent(componentType);
            return component ?? throw new ComponentNotFoundException(
                $"Component of type {componentType.Name} was not found on {GetType().Name}.");
        }

        public IComponent? TryGetComponent(Type componentType)
        {
            var component = componentType.IsInterface
                ? _components.FirstOrDefault(c => c.GetType().IsAssignableTo(componentType))
                : _components.FirstOrDefault(c => c.GetType() == componentType);
            return component;
        }

        public IEnumerable<IComponent> GetComponents(Type componentType)
        {
            var components = componentType.IsInterface
                ? _components.Where(c => c.GetType().IsAssignableTo(componentType))
                : _components.Where(c => c.GetType() == componentType);
            return components;
        }

        public void Destroy()
        {
            ParentScene?.RemoveEntity(this);
            _components.Clear();
            _destroyed = true;
        }

        public bool HasComponent<T>()
         => _components.Any(c => c is T);

        public override string ToString()
        {
            return Id;
        }

        public static Entity Parse(string s, IFormatProvider? provider)
        {
            var parsedSuccessfully = TryParse(s, provider, out var result);
            if (!parsedSuccessfully)
            {
                throw new InvalidOperationException("Input was not in a valid format.");
            }

            return result!;
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out Entity? result)
        {
            result = null!;
            if (GameContext.GetGameEngine().SceneManager.CurrentScene == null || s == null)
                return false;
            result = GameContext.GetGameEngine().SceneManager.CurrentScene?.GetEntity(s);

            return result != null;
        }
    }
}
