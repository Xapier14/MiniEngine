using MiniEngine.Collections;
using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MiniEngine
{
    public static class SystemManager
    {
        private static readonly SystemList _systemList = new();
        private static readonly Dictionary<Type, List<Component>> _components = new();
        private static readonly Dictionary<Type, List<Type>> _systems = new();
        private static readonly Dictionary<Type, System> _systemInstanceCache = new();
        private static readonly Dictionary<(Type system, string eventType), MethodInfo> _systemEventsCache = new ();
        private static readonly Dictionary<Type, List<Component>> _componentDiff = new();
        private static Type? _currentlyExecutingSystem;

        internal static IReadOnlyDictionary<Type, List<Component>> ComponentDiff => _componentDiff;

        internal static bool InitializeWithDefaultSystems()
        {
            try
            {
                LoggingService.Debug("Initializing default ECS systems...");
                _systemList.Clear();
                _components.Clear();
                RegisterAfter<System>(typeof(ScriptSystem), ScriptEvent.BeforeUpdate);
                RegisterAfter<ScriptSystem>(ScriptEvent.BeforeUpdate, typeof(ScriptSystem), ScriptEvent.Update);
                RegisterAfter<ScriptSystem>(ScriptEvent.Update, typeof(ScriptSystem), ScriptEvent.AfterUpdate);
                RegisterAfter<ScriptSystem>(typeof(PhysicsSystem));
                RegisterAfter<PhysicsSystem>(typeof(MotionSystem));
                RegisterAfter<MotionSystem>(typeof(TransformSystem));
                RegisterAfter<TransformSystem>(typeof(ScriptSystem), ScriptEvent.BeforeDraw);
                RegisterAfter<ScriptSystem>(typeof(DrawSystem));
                RegisterAfter<DrawSystem>(ScriptEvent.BeforeDraw, typeof(ScriptSystem), ScriptEvent.AfterDraw);
                LoggingService.Debug("All default ECS system OK.");
            }
            catch (Exception ex)
            {
                LoggingService.Fatal("Couldn't initialize default ECS systems.", ex);
                return true;
            }

            return false;
        }

        private static void AssociateComponentWithSystem(Type component, Type system)
        {
            if (!_systems.TryGetValue(component, out var systemList))
            {
                systemList = new List<Type>();
                _systems[component] = systemList;
            }
            if (!systemList.Contains(system))
                systemList.Add(system);
        }

        internal static T? Get<T>(object? data = null) where T : System => (T?)_systemList.FindSystemBySystemType<T>(data)?.Item1;

        public static void Register(Type systemType) => RegisterBefore<DrawSystem>(systemType);

        private static void UpdateSystemAssociation(Type systemType)
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

        private static void RaiseSystemEvent(Type systemType, string eventName, params object?[] args)
        {
            if (!_systemEventsCache.TryGetValue((systemType, eventName), out var eventMethod))
            {
                eventMethod = systemType.GetMethod(eventName, BindingFlags.Public | BindingFlags.Static);
                if (eventMethod != null)
                    _systemEventsCache.Add((systemType, eventName), eventMethod);
            }
            eventMethod?.Invoke(null, args);
        }

        public static void RegisterBefore<T>(Type systemType, object? argument = null) where T : System
            => RegisterBefore<T>(null, systemType, argument);

        public static void RegisterBefore<T>(object? systemArg, Type systemType, object? argument = null) where T : System
        {
            if (systemType.IsAssignableFrom(typeof(System)))
            {
                LoggingService.Error("Error registering {0}, type is not assignable from System.", systemType.Name);
                return;
            }

            if (!_systemInstanceCache.TryGetValue(systemType, out var system))
            {
                var constructor = systemType.GetConstructor(Array.Empty<Type>());
                if (constructor is null)
                {
                    LoggingService.Error("Error registering {0}, System does not have a default constructor.", systemType.Name);
                    return;
                }
                system = (System)constructor.Invoke(null);
                _systemInstanceCache.Add(systemType, system);
            }
            system.PreCacheHandlers();
            _components.TryAdd(systemType, new List<Component>());
            if (typeof(T) == typeof(System))
            {
                _systemList.Add(system, argument);
                UpdateSystemAssociation(systemType);
                return;
            }
            _systemList.FindFirstSystemNodeBySystemType<T>(systemArg)?.InsertBefore(new SystemNode
            {
                Value = system,
                Argument = argument
            });
            UpdateSystemAssociation(systemType);
        }

        public static void RegisterAfter<T>(Type systemType, object? argument = null) where T : System
            => RegisterAfter<T>(null, systemType, argument);

        public static void RegisterAfter<T>(object? systemArg, Type systemType, object? argument = null) where T : System
        {
            if (systemType.IsAssignableFrom(typeof(System)))
            {
                LoggingService.Error("Error registering {0}, type is not assignable to System.", systemType.Name);
                return;
            }

            if (!_systemInstanceCache.TryGetValue(systemType, out var system))
            {
                var constructor = systemType.GetConstructor(Array.Empty<Type>());
                if (constructor is null)
                {
                    LoggingService.Error("Error registering {0}, System does not have a default constructor.", systemType.Name);
                    return;
                }
                system = (System)constructor.Invoke(null);
                _systemInstanceCache.Add(systemType, system);
            }
            system.PreCacheHandlers();
            _components.TryAdd(systemType, new List<Component>());
            if (typeof(T) == typeof(System))
            {
                _systemList.Add(system, argument);
                UpdateSystemAssociation(systemType);
                return;
            }

            var test = _systemList.FindLastSystemNodeBySystemType<T>(systemArg);
            test?.InsertAfter(new SystemNode
            {
                Value = system,
                Argument = argument
            });
            UpdateSystemAssociation(systemType);
        }

        public static void RegisterEntity(Entity entity)
        {
            foreach (var component in entity.GetComponents())
            {
                RegisterComponent(component);
            }
        }

        public static void RegisterComponent(Component component)
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

        public static void RegisterComponent<T>(Component component) where T : System
        {
            if (!_components.TryGetValue(typeof(T), out var list))
                return;

            if (_currentlyExecutingSystem == typeof(T))
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
                    RaiseSystemEvent(typeof(T), "OnComponentRegister", component);
                }

                return;
            }
            if (list.Contains(component))
                return;

            list.Add(component);
            RaiseSystemEvent(typeof(T), "OnComponentRegister", component);
        }

        public static void RemoveEntity(Entity entity)
        {
            foreach (var component in entity.GetComponents())
            {
                RemoveComponent(component);
            }
        }

        public static void RemoveComponent(Component component)
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

        public static void RemoveComponent<T>(Component component) where T : System
        {
            if (!_components.TryGetValue(typeof(T), out var list))
                return;

            if (_currentlyExecutingSystem == typeof(T))
            {
                if (!_componentDiff.TryGetValue(_currentlyExecutingSystem, out var diffList))
                {
                    diffList = new List<Component>();
                    _componentDiff.Add(_currentlyExecutingSystem, diffList);
                }
                // add if not included in diff list
                if (!diffList.Contains(component))
                {
                    RaiseSystemEvent(typeof(T), "OnComponentRemove", component);
                    diffList.Add(component);
                }

                return;
            }
            if (!list.Contains(component))
                return;

            RaiseSystemEvent(typeof(T), "OnComponentRemove", component);
            list.Remove(component);
        }

        public static int CountComponents()
        {
            return _components.Sum((kp) => kp.Value.Count);
        }

        public static int CountSystems()
        {
            return _systemList.Count();
        }

        public static void PurgeComponents()
        {
            foreach (var (_, list) in _components)
            {
                var snapshot = list.ToArray();
                foreach (var component in snapshot)
                    RemoveComponent(component);
            }
        }

        internal static void ProcessSystems()
        {
            var systemList = (IEnumerable<SystemNode>)_systemList;
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
                system.Value?.HandleComponents(componentList, system.Argument);
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
