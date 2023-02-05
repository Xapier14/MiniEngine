using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniEngine.Utility;

namespace MiniEngine
{
    public abstract class System
    {
        private readonly Dictionary<Type, MethodInfo> _methodCache = new();
        private long? _lastTick;
        public double DeltaTime { get; private set; }

        internal void PreCacheHandlers()
        {
            var systemType = GetType();
            var methods = systemType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(methodInfo => methodInfo.Name == "HandleComponent" &&
                                     methodInfo.GetParameters().Length == 1);
            foreach (var method in methods)
            {
                var handledComponentType = method.GetParameters()[0].ParameterType;
                _methodCache.TryAdd(handledComponentType, method);
            }
        }

        internal void HandleComponents(IEnumerable<Component> components)
        {
            if (_lastTick is { } lastTick)
            {
                var thisTick = DateTime.Now.Ticks;
                DeltaTime = TimeSpan.FromTicks(thisTick - lastTick).TotalMilliseconds / 1000.0;
                _lastTick = thisTick;
            }
            else
            {
                _lastTick = DateTime.Now.Ticks;
            }

            foreach (var component in components)
            {
                if (!_methodCache.TryGetValue(component.GetType(), out var methodInfo))
                {
                    var systemType = GetType();
                    var componentType = component.GetType();
                    methodInfo = systemType.GetMethod(
                        "HandleComponent",
                        BindingFlags.Public | BindingFlags.Instance,
                        new []
                        {
                            componentType
                        });
                    if (methodInfo == null)
                    {
                        LoggingService.Error(
                            "System {0} does not handle component {1}.",
                            systemType.Name,
                            componentType.Name);
                        continue;
                    }
                }
                methodInfo.Invoke(this, new object?[] { component });
            }
        }
    }
}
