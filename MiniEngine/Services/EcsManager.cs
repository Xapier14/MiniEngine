using MiniEngine.Collections;
using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiniEngine
{
    public class EcsManager
    {
        private readonly SystemList _systemList = [];
        private readonly Dictionary<Type, List<Component>> _components = [];
        private readonly Dictionary<Type, List<Type>> _systems = [];
        private readonly Dictionary<Type, System> _systemInstanceCache = [];
        private readonly Dictionary<(Type system, string eventType), MethodInfo> _systemEventsCache = [];
        private readonly Dictionary<Type, List<Component>> _componentDiff = [];

        // Entity association. _components above is keyed by *system* type and so loses
        // track of which entity a component came from; these two carry that instead of
        // making components hold a reference back to their owner.
        private readonly Dictionary<Entity, List<Component>> _entityComponents = [];
        private readonly Dictionary<Component, Entity> _componentOwner = [];

        private Type? _currentlyExecutingSystem;

        /// <summary>
        /// The world currently being processed. Set by <see cref="SceneManager"/>. Systems
        /// read world state (camera, background) from here rather than walking up from a
        /// component to its entity's scene.
        /// </summary>
        public Scene? ActiveScene { get; internal set; }

        internal IReadOnlyDictionary<Type, List<Component>> ComponentDiff => _componentDiff;

        /// <summary>
        /// Drops every registered system, component and association. Hosts call this before
        /// building their pipeline; the pipeline itself is the host's concern, not the ECS's.
        /// </summary>
        public void Reset()
        {
            _systemList.Clear();
            _components.Clear();
            _systems.Clear();
            _systemInstanceCache.Clear();
            _systemEventsCache.Clear();
            _componentDiff.Clear();
            _entityComponents.Clear();
            _componentOwner.Clear();
            _currentlyExecutingSystem = null;
        }

        private void AssociateComponentWithSystem(Type component, Type system)
        {
            if (!_systems.TryGetValue(component, out var systemList))
            {
                systemList = new List<Type>();
                _systems[component] = systemList;
            }
            if (!systemList.Contains(system))
                systemList.Add(system);
        }

        internal T? Get<T>(object? data = null) where T : System => (T?)_systemList.FindSystemBySystemType<T>(data)?.Item1;

        /// <summary>
        /// Registers a system at the default position: just before <see cref="DrawSystem"/>
        /// when there is one, otherwise appended. A headless pipeline has no draw system,
        /// so anchoring on it unconditionally would drop every registration.
        /// </summary>
        public void Register(Type systemType)
        {
            if (_systemList.ContainsSystemType<DrawSystem>())
                RegisterBefore<DrawSystem>(systemType);
            else
                RegisterAfter<System>(systemType);
        }

        /// <summary>
        /// Registers a host-constructed system, for systems needing dependencies the ECS
        /// knows nothing about (a <see cref="GameEngine"/>, a connection strategy, ...).
        /// </summary>
        public void Register(System system)
        {
            if (_systemList.ContainsSystemType<DrawSystem>())
                RegisterBefore<DrawSystem>(system);
            else
                RegisterAfter<System>(system);
        }

        private void UpdateSystemAssociation(Type systemType)
        {
            var attributes = systemType.GetCustomAttributes()
                .Where(attribute => attribute.GetType().IsAssignableTo(typeof(IHandlesComponentAttribute)));
            foreach (var attribute in attributes)
            {
                var handlesAttribute = (IHandlesComponentAttribute)attribute;
                var componentType = handlesAttribute.ComponentType;
                AssociateComponentWithSystem(componentType, systemType);
            }
        }

        private void RaiseSystemEvent(Type systemType, string eventName, params object?[] args)
        {
            if (!_systemInstanceCache.TryGetValue(systemType, out var system))
            {
                LoggingService.Fatal("Failed to raise event {0}. System does not have an instance.", eventName);
                return;
            }
            if (!_systemEventsCache.TryGetValue((systemType, eventName), out var eventMethod))
            {
                eventMethod = systemType.GetMethod(eventName, BindingFlags.Public | BindingFlags.Instance);
                if (eventMethod != null)
                    _systemEventsCache.Add((systemType, eventName), eventMethod);
            }
            eventMethod?.Invoke(system, args);
        }

        /// <summary>
        /// Resolves the single instance for <paramref name="systemType"/>, constructing it
        /// via its parameterless constructor on first use. Systems needing host services
        /// should be constructed by the host and passed to the instance overloads instead.
        /// </summary>
        private System? ResolveSystem(Type systemType)
        {
            if (systemType.IsAssignableFrom(typeof(System)))
            {
                LoggingService.Error("Error registering {0}, type is not assignable to System.", systemType.Name);
                return null;
            }

            if (_systemInstanceCache.TryGetValue(systemType, out var system))
                return system;

            var constructor = systemType.GetConstructor(Array.Empty<Type>());
            if (constructor is null)
            {
                LoggingService.Error("Error registering {0}, System does not have a default constructor.", systemType.Name);
                return null;
            }
            system = (System)constructor.Invoke(null);
            _systemInstanceCache.Add(systemType, system);
            return system;
        }

        /// <summary>
        /// Takes ownership of a host-constructed system. One instance per system type, so a
        /// type already registered keeps the instance it was first registered with.
        /// </summary>
        private System AdoptSystem(System system)
        {
            var systemType = system.GetType();
            if (_systemInstanceCache.TryGetValue(systemType, out var existing))
            {
                if (!ReferenceEquals(existing, system))
                    LoggingService.Warn("System {0} is already registered; keeping the existing instance.", systemType.Name);
                return existing;
            }
            _systemInstanceCache.Add(systemType, system);
            return system;
        }

        private void Link(System system, SystemNode? target, bool before, object? argument)
        {
            var systemType = system.GetType();
            system.Attach(this);
            _components.TryAdd(systemType, new List<Component>());

            if (target == null)
            {
                _systemList.Add(system, argument);
            }
            else
            {
                var node = new SystemNode { Value = system, Argument = argument };
                if (before)
                    _systemList.InsertBefore(target, node);
                else
                    _systemList.InsertAfter(target, node);
            }

            UpdateSystemAssociation(systemType);
        }

        public void RegisterBefore<T>(Type systemType, object? argument = null) where T : System
            => RegisterBefore<T>(null, systemType, argument);

        public void RegisterBefore<T>(object? systemArg, Type systemType, object? argument = null) where T : System
        {
            if (ResolveSystem(systemType) is { } system)
                LinkBefore<T>(systemArg, system, argument);
        }

        /// <summary>
        /// Registers a system the host constructed itself, for systems that need
        /// dependencies the ECS knows nothing about.
        /// </summary>
        public void RegisterBefore<T>(System system, object? argument = null) where T : System
            => LinkBefore<T>(null, AdoptSystem(system), argument);

        public void RegisterBefore<T>(object? systemArg, System system, object? argument = null) where T : System
            => LinkBefore<T>(systemArg, AdoptSystem(system), argument);

        private void LinkBefore<T>(object? systemArg, System system, object? argument) where T : System
        {
            // T == System means "append to the end of the pipeline", which is not the same
            // as an anchor that could not be found.
            if (typeof(T) == typeof(System))
            {
                Link(system, null, before: true, argument);
                return;
            }

            var target = _systemList.FindFirstSystemNodeBySystemType<T>(systemArg);
            if (target == null)
            {
                LoggingService.Error("Error registering {0}, anchor system {1} is not in the pipeline.",
                    system.GetType().Name, typeof(T).Name);
                return;
            }
            Link(system, target, before: true, argument);
        }

        public void RegisterAfter<T>(Type systemType, object? argument = null) where T : System
            => RegisterAfter<T>(null, systemType, argument);

        public void RegisterAfter<T>(object? systemArg, Type systemType, object? argument = null) where T : System
        {
            if (ResolveSystem(systemType) is { } system)
                LinkAfter<T>(systemArg, system, argument);
        }

        /// <summary>
        /// Registers a system the host constructed itself, for systems that need
        /// dependencies the ECS knows nothing about.
        /// </summary>
        public void RegisterAfter<T>(System system, object? argument = null) where T : System
            => LinkAfter<T>(null, AdoptSystem(system), argument);

        public void RegisterAfter<T>(object? systemArg, System system, object? argument = null) where T : System
            => LinkAfter<T>(systemArg, AdoptSystem(system), argument);

        private void LinkAfter<T>(object? systemArg, System system, object? argument) where T : System
        {
            // T == System means "append to the end of the pipeline", which is not the same
            // as an anchor that could not be found.
            if (typeof(T) == typeof(System))
            {
                Link(system, null, before: false, argument);
                return;
            }

            var target = _systemList.FindLastSystemNodeBySystemType<T>(systemArg);
            if (target == null)
            {
                LoggingService.Error("Error registering {0}, anchor system {1} is not in the pipeline.",
                    system.GetType().Name, typeof(T).Name);
                return;
            }
            Link(system, target, before: false, argument);
        }

        public void RegisterEntity(Entity entity)
        {
            foreach (var component in entity.GetComponents())
            {
                RegisterComponent(entity, component);
            }
        }

        /// <summary>
        /// Registers <paramref name="component"/> and records that it belongs to
        /// <paramref name="entity"/>. The association is what lets systems reach a
        /// component's siblings without the component holding a reference to its owner.
        /// </summary>
        public void RegisterComponent(Entity entity, Component component)
        {
            _componentOwner[component] = entity;
            if (!_entityComponents.TryGetValue(entity, out var owned))
            {
                owned = new List<Component>();
                _entityComponents.Add(entity, owned);
            }
            if (!owned.Contains(component))
                owned.Add(component);

            DispatchRegister(component);
        }

        /// <summary>
        /// Returns the entity <paramref name="component"/> was registered under, or null
        /// if it is not registered with this manager.
        /// </summary>
        public Entity? GetOwner(Component component)
            => _componentOwner.TryGetValue(component, out var entity) ? entity : null;

        /// <summary>
        /// Returns the component of type <typeparamref name="T"/> belonging to the same
        /// entity as <paramref name="component"/>, or null if there is none.
        /// </summary>
        public T? GetSibling<T>(Component component) where T : class, IComponent
            => GetOwner(component)?.TryGetComponent(typeof(T)) as T;

        /// <summary>
        /// Returns every component of type <typeparamref name="T"/> belonging to the same
        /// entity as <paramref name="component"/>.
        /// </summary>
        public IEnumerable<T> GetSiblings<T>(Component component) where T : IComponent
            => GetOwner(component)?.GetComponents<T>() ?? Enumerable.Empty<T>();

        /// <summary>
        /// Returns every registered component belonging to <paramref name="entity"/>.
        /// </summary>
        public IReadOnlyList<Component> GetEntityComponents(Entity entity)
            => _entityComponents.TryGetValue(entity, out var owned)
                ? owned
                : Array.Empty<Component>();

        private void DispatchRegister(Component component)
        {
            if (!_systems.TryGetValue(component.GetType(), out var systemList))
                return;
            systemList.ForEach(systemType =>
            {
                if (!_components.TryGetValue(systemType, out var list))
                    return;

                if (_currentlyExecutingSystem == systemType)
                {
                    if (!_componentDiff.TryGetValue(_currentlyExecutingSystem, out var diffList))
                    {
                        diffList = new List<Component>();
                        _componentDiff.Add(_currentlyExecutingSystem, diffList);
                    }
                    // add if not included in diff list
                    if (!diffList.Contains(component))
                    {
                        diffList.Add(component);
                        RaiseSystemEvent(systemType, "OnComponentRegister", component);
                    }

                    return;
                }
                if (list.Contains(component))
                    return;

                list.Add(component);
                RaiseSystemEvent(systemType, "OnComponentRegister", component);
            });
        }

        public void RemoveEntity(Entity entity)
        {
            foreach (var component in entity.GetComponents())
            {
                RemoveComponent(component);
            }
        }

        /// <summary>
        /// Removes <paramref name="component"/> and forgets its entity association. The
        /// owner is not a parameter because the manager already knows it from registration.
        /// </summary>
        public void RemoveComponent(Component component)
        {
            // Dispatch first: OnComponentRemove handlers may still query siblings.
            DispatchRemove(component);
            Disassociate(component);
        }

        private void Disassociate(Component component)
        {
            if (!_componentOwner.TryGetValue(component, out var entity))
                return;
            _componentOwner.Remove(component);
            if (!_entityComponents.TryGetValue(entity, out var owned))
                return;
            owned.Remove(component);
            if (owned.Count == 0)
                _entityComponents.Remove(entity);
        }

        private void DispatchRemove(Component component)
        {
            if (!_systems.TryGetValue(component.GetType(), out var systemList))
                return;
            systemList.ForEach(systemType =>
            {
                if (!_components.TryGetValue(systemType, out var list))
                    return;

                if (_currentlyExecutingSystem == systemType)
                {
                    if (!_componentDiff.TryGetValue(_currentlyExecutingSystem, out var diffList))
                    {
                        diffList = new List<Component>();
                        _componentDiff.Add(_currentlyExecutingSystem, diffList);
                    }

                    // add if not included in diff list
                    if (!diffList.Contains(component))
                    {
                        RaiseSystemEvent(systemType, "OnComponentRemove", component);
                        diffList.Add(component);
                    }

                    return;
                }
                if (!list.Contains(component))
                    return;

                RaiseSystemEvent(systemType, "OnComponentRemove", component);
                list.Remove(component);
            });
        }

        public IEnumerable<T> GetComponents<T>() where T : IComponent
        {
            // _components is keyed by system type, so a component handled by more than one
            // system appears in several buckets. Distinct keeps it to one result.
            return _components.Values
                .SelectMany(components => components)
                .Where(component => component.GetType().IsAssignableTo(typeof(T)))
                .Distinct()
                .Cast<T>()
                .ToList();
        }

        public int CountComponents()
        {
            return _components.Sum((kp) => kp.Value.Count);
        }

        public int CountSystems()
        {
            return _systemList.Count();
        }

        public void PurgeComponents()
        {
            foreach (var (_, list) in _components)
            {
                var snapshot = list.ToArray();
                foreach (var component in snapshot)
                    RemoveComponent(component);
            }
            _entityComponents.Clear();
            _componentOwner.Clear();
        }

        public T? GetSystemInstance<T>() where T : System
            => (T?)GetSystemInstance(typeof(T));

        public System? GetSystemInstance(Type systemType)
        {
            _systemInstanceCache.TryGetValue(systemType, out var system);
            return system;
        }

        public void ProcessSystems()
        {
            IEnumerable<SystemNode>? systemList = _systemList;
            var tickList = new List<System>();
            foreach (var system in systemList)
            {
                if (system.Value == null)
                    continue;
                if (!tickList.Contains(system.Value))
                {
                    system.Value.UpdateTick();
                    tickList.Add(system.Value);
                }
                if (!_components.TryGetValue(system.Value.GetType(), out var componentList))
                    continue;
                _currentlyExecutingSystem = system.Value.GetType();
                system.Value?.HandleComponents(this, componentList, system.Argument);
                _currentlyExecutingSystem = null;
                // process diff list
                foreach (var (systemType, diffList) in ComponentDiff)
                {
                    var realComponentList = _components[systemType];
                    foreach (var diff in diffList)
                    {
                        if (realComponentList.Contains(diff))
                        {
                            realComponentList.Remove(diff);
                        }
                        else
                        {
                            realComponentList.Add(diff);
                        }
                    }
                }
                _componentDiff.Clear();
            }
        }
    }
}
