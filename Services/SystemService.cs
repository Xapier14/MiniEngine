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
        private static readonly List<System> _systems = new();
        private static readonly SystemList _systemList = new();
        private static readonly Dictionary<Type, (System, MethodInfo)> _associatedHandlers = new();

        internal static void LoadSystems()
        {
            if (_associatedHandlers.Any())
                return;

            // load systems components via reflection
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();

            var systems = new List<Type>();
            var components = new List<Type>();
            foreach (var type in types)
            {
                if (type.IsAssignableTo(typeof(System)) &&
                    type != typeof(System))
                    systems.Add(type);
                if (type.IsAssignableTo(typeof(Component)) &&
                    type != typeof(Component))
                    components.Add(type);
            }

            systems.ForEach(HandleSystem);
            LoggingService.Debug("Processed {0} system(s).", systems.Count);
            components.ForEach(HandleComponent);
            LoggingService.Debug("Processed {0} component(s).", components.Count);
        }

        private static void HandleComponent(Type type)
        {
            var attributes = type.GetCustomAttributes();
            foreach (var attribute in attributes)
            {
                if (!attribute.GetType().IsOfGenericType(typeof(HandledByAttribute<>)))
                    continue;
                var systemType = ((IHandledByAttribute)attribute).SystemType;
                var systemInstance = _systems.First(system => system.GetType() == systemType);
                var systemMethods = systemType.GetMethods();
                var method = systemMethods.FirstOrDefault(methodInfo =>
                {
                    if (methodInfo is null ||
                        methodInfo.ReturnType != typeof(void) ||
                        methodInfo.GetParameters().Length != 1 ||
                        methodInfo.Name != "HandleComponent")
                        return false;
                    var firstParameter = methodInfo.GetParameters()[0];
                    return firstParameter.ParameterType == type;
                }, null);
                if (method is null)
                {
                    LoggingService.Warn("{0} is handled by {1} but does not have a method that matches \"{2}\".",
                        type,
                        systemType,
                        $"void HandleComponent({type})");
                    continue;
                }
                _associatedHandlers.Add(type, (systemInstance, method));
            }
        }

        private static void HandleSystem(Type type)
        {
            var system = (System?)type.GetConstructor(Array.Empty<Type>())?.Invoke(null);
            if (system != null)
                _systems.Add(system);
        }
    }
}
