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

        public void AddComponent<T>() where T : Component, new()
        {
            if (_components.Any(component => component.GetType() == typeof(T)))
            {
                LoggingService.Error("Tried to add component {0} to entity {1} but the same component already exists.",
                    typeof(T),
                    Id);
                return;
            }
            var newComponent = new T
            {
                Owner = this
            };
            var requiredComponents = newComponent.GetRequiredComponents().ToArray();
            var isMissingRequiredComponent = requiredComponents
                .Any(type => !_components.Any(ownedComponent => ownedComponent.GetType() == type));
            if (isMissingRequiredComponent)
            {
                LoggingService.Error("Tried to add component {0} to entity {1} but does not have the required component(s).",
                    typeof(T),
                    Id);
                LoggingService.Error("Requires: {0}.",
                    string.Join(';', requiredComponents.Select(type => type.Name)));
                LoggingService.Error("Has: {0}.",
                    _components.Any() ? 
                        string.Join(';', _components.Select(component => component.GetType().Name))
                    : "<empty>");
                return;
            }
            _components.Add(newComponent);
        }

        public void RemoveComponent<T>()
        {
            var component = _components.FirstOrDefault(c => c is T);
            if (component == null)
            {
                LoggingService.Error("Component of type '{0}' does not exist in {1} of {2}.", typeof(T), Id, this);
                return;
            }

            if (!_components.Remove(component))
                LoggingService.Error("Error removing component of type '{0}' from {1} of {2}.", typeof(T), Id, this);
        }

        public T? GetComponent<T>() where T : Component
        {
            var component = _components.FirstOrDefault(c => c is T);
            return (T?)component;
        }

        public bool HasComponent<T>()
         => _components.Any(c => c is T);
    }
}
