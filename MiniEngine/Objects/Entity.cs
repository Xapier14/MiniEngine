using System;
using System.Collections.Generic;
using System.Linq;
using MiniEngine.Utility;

namespace MiniEngine
{
    public abstract class Entity
    {
        private readonly List<Component> _components = new();
        private readonly Guid _guid = Guid.NewGuid();
        public string Id => $"{GetType()}#{_guid}";

        internal IReadOnlyList<Component> GetComponents()
            => _components;

        protected void AddComponent(Component component)
        {
            var requiredComponents = component.GetRequiredComponents().ToArray();
            var isMissingRequiredComponent = requiredComponents
                .Any(type => !_components.Any(ownedComponent => ownedComponent.GetType() == type));
            if (isMissingRequiredComponent)
            {
                LoggingService.Error("Tried to add component {0} to entity {1} but does not have the required component(s).",
                    component.GetType(),
                    Id);
                LoggingService.Error("Requires: {0}.",
                    string.Join(';', requiredComponents.Select(type => type.Name)));
                LoggingService.Error("Has: {0}.",
                    _components.Any() ? 
                        string.Join(';', _components.Select(hasComponent => hasComponent.GetType().Name))
                        : "<empty>");
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

        public T? GetComponent<T>() where T : Component
        {
            var component = _components.FirstOrDefault(c => c is T);
            return (T?)component;
        }

        public bool HasComponent<T>()
         => _components.Any(c => c is T);

        public override string ToString()
        {
            return Id;
        }
    }
}
