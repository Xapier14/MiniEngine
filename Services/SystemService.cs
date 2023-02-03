using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniEngine.Utility;
using MiniEngine.Collections;

namespace MiniEngine
{
    public static class SystemService
    {
        private static readonly SystemList _systemList = new();
        private static readonly Dictionary<Type, List<Component>> _components = new();
        private static readonly Dictionary<Type, Type> _associatedSystemFromComponent = new();

        internal static bool InitializeWithDefaultSystems()
        {
            try
            {
                LoggingService.Debug("Initializing default ECS systems...");
                _systemList.Clear();
                _components.Clear();
                RegisterAfter<System>(typeof(InputSystem));
                RegisterAfter<InputSystem>(typeof(ScriptSystem));
                RegisterAfter<InputSystem>(typeof(TransformSystem));
                RegisterAfter<TransformSystem>(typeof(PhysicsSystem));
                RegisterAfter<PhysicsSystem>(typeof(MotionSystem));
                RegisterAfter<MotionSystem>(typeof(SpriteSystem));
                LoggingService.Debug("All default ECS system OK.");
            }
            catch (Exception ex)
            {
                LoggingService.Fatal("Couldn't initialize default ECS systems.", ex);
                return true;
            }

            return false;
        }

        internal static T? Get<T>() where T : System => (T?)_systemList.FindSystemBySystemType<T>();

        public static void Register(Type systemType) => RegisterBefore<SpriteSystem>(systemType);

        public static void RegisterBefore<T>(Type systemType) where T : System
        {
            if (_components.ContainsKey(systemType))
            {
                LoggingService.Error("Error registering {0}, System of the same type already exists.", systemType.Name);
                return;
            }

            if (systemType.IsAssignableFrom(typeof(System)))
            {
                LoggingService.Error("Error registering {0}, type is not assignable from System.", systemType.Name);
                return;
            }

            var constructor = systemType.GetConstructor(Array.Empty<Type>());
            if (constructor is null)
            {
                LoggingService.Error("Error registering {0}, System does not have a default constructor.", systemType.Name);
                return;
            }
            var system = (System)constructor.Invoke(null);
            _components.Add(systemType, new List<Component>());
            if (typeof(T) == typeof(System))
            {
                _systemList.Add(system);
                return;
            }
            _systemList.FindSystemNodeBySystemType<T>()?.InsertBefore(new SystemNode
            {
                Value = system
            });
        }

        public static void RegisterAfter<T>(Type systemType) where T : System
        {
            if (_components.ContainsKey(systemType))
            {
                LoggingService.Error("Error registering {0}, System of the same type already exists.", systemType.Name);
                return;
            }

            if (systemType.IsAssignableFrom(typeof(System)))
            {
                LoggingService.Error("Error registering {0}, type is not assignable to System.", systemType.Name);
                return;
            }

            var constructor = systemType.GetConstructor(Array.Empty<Type>());
            if (constructor is null)
            {
                LoggingService.Error("Error registering {0}, System does not have a default constructor.", systemType.Name);
                return;
            }
            var system = (System)constructor.Invoke(null);
            _components.Add(systemType, new List<Component>());
            if (typeof(T) == typeof(System))
            {
                _systemList.Add(system);
                return;
            }
            _systemList.FindSystemNodeBySystemType<T>()?.InsertAfter(new SystemNode
            {
                Value = system
            });
        }

        public static void RegisterComponentInstance<T>(Component component) where T : System
        {
            if (!_components.TryGetValue(typeof(T), out var list))
                return;
            list.Add(component);
        }

        public static void RemoveComponentInstance<T>(Component component) where T : System
        {
            if (!_components.TryGetValue(typeof(T), out var list))
                return;
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
                list.Clear();
            }
        }
    }
}
