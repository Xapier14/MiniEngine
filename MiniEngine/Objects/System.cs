using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniEngine.Utility;

namespace MiniEngine
{
    public abstract class System
    {
        private readonly Dictionary<(Type, bool), MethodInfo> _methodCache = new();
        private long? _lastTick;
        public double DeltaTime { get; private set; }

        internal void PreCacheHandlers()
        {
            var systemType = GetType();
            var methods = systemType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(methodInfo => methodInfo.Name == "HandleComponent" &&
                                     methodInfo.GetParameters().Length == 1 ||
                                     methodInfo.GetParameters().Length == 2);
            foreach (var method in methods)
            {
                var handledComponentType = method.GetParameters()[0].ParameterType;
                var hasOtherArg = method.GetParameters().Length > 1;
                _methodCache.TryAdd((handledComponentType, hasOtherArg), method);
            }
        }

        internal void UpdateTick()
        {
            if (_lastTick is { } lastTick)
            {
                var thisTick = DateTime.Now.Ticks;
                DeltaTime = Math.Min(TimeSpan.FromTicks(thisTick - lastTick).TotalMilliseconds / 1000.0, 0.25);
                _lastTick = thisTick;
            }
            else
            {
                _lastTick = DateTime.Now.Ticks;
            }
        }

        internal void HandleComponents(IEnumerable<Component> components, object? arg = null)
        {
            foreach (var component in components)
            {
                if (!_methodCache.TryGetValue((component.GetType(), arg != null), out var methodInfo))
                {
                    var systemType = GetType();
                    var componentType = component.GetType();
                    methodInfo = arg != null ? 
                        systemType.GetMethod(
                            "HandleComponent",
                            BindingFlags.Public | BindingFlags.Instance, new []
                            {
                                componentType,
                                typeof(object)
                            }) :
                        systemType.GetMethod(
                            "HandleComponent",
                            BindingFlags.Public | BindingFlags.Instance, new []
                            {
                                componentType
                            });

                    if (methodInfo == null)
                    {
                        LoggingService.Error(
                            arg == null ?
                                "System {0} does not handle component {1}" :
                                "System {0} does not handle component {1} with additional parameter of type object.",
                            systemType.Name,
                            componentType.Name);
                        continue;
                    }
                }

                try
                {
                    var args = new object?[] { component };
                    if (methodInfo.GetParameters().Length > 1)
                        args = args.Concat(new[] { arg }).ToArray();
                    methodInfo.Invoke(this, args);
                }
                catch (Exception e)
                {
                    LoggingService.Trace("Error executing handler for component {0} via system {1}.",
                        e,
                        component.GetType().Name,
                        GetType().Name);
                }
            }
        }
    }
}
