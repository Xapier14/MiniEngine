using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiniEngine
{
    public abstract class System
    {
        private readonly Dictionary<(Type, bool), MethodInfo> _methodCache = new();

        /// <summary>
        /// Seconds elapsed for this step. Assigned by <see cref="EcsManager"/> before each
        /// dispatch: exactly <see cref="WorldClock.FixedDelta"/> for a
        /// <see cref="Cadence.Fixed"/> node, and the measured frame time for a
        /// <see cref="Cadence.Frame"/> one.
        /// </summary>
        public double DeltaTime { get; internal set; }
        public float DeltaTimeF => (float)DeltaTime;

        /// <summary>
        /// The manager this system is registered with. Assigned by <see cref="Attach"/>
        /// during registration, so it is not available in a system's constructor.
        /// </summary>
        public EcsManager Ecs { get; private set; } = null!;

        protected virtual void BeforeStep(object? arg) { }
        protected virtual void Step(object? arg) { }
        protected virtual void AfterStep(object? arg) { }

        internal void Attach(EcsManager ecs)
        {
            Ecs = ecs;
            PreCacheHandlers();
        }

        internal void PreCacheHandlers()
        {
            var systemType = GetType();
            var methods = systemType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(methodInfo => methodInfo.Name == "HandleComponent" &&
                                     methodInfo.GetParameters().Length == 1 ||
                                     methodInfo.GetParameters().Length == 2);
            _methodCache.Clear();
            foreach (var method in methods)
            {
                var handledComponentType = method.GetParameters()[0].ParameterType;
                var hasOtherArg = method.GetParameters().Length > 1;
                _methodCache.TryAdd((handledComponentType, hasOtherArg), method);
            }
        }

        internal void HandleComponents(EcsManager ecsManager, IEnumerable<Component> components, object? arg = null)
        {
            BeforeStep(arg);
            Step(arg);
            var componentsSnapshot = components.ToArray();
            foreach (var component in componentsSnapshot)
            {
                // skip if component is in component diff (flagged for removal)
                if (ecsManager.ComponentDiff.TryGetValue(GetType(), out var componentDiff))
                    if (componentDiff.Contains(component))
                        continue;

                if (!_methodCache.TryGetValue((component.GetType(), arg != null), out var methodInfo))
                {
                    var systemType = GetType();
                    var componentType = component.GetType();
                    methodInfo = arg != null ?
                        systemType.GetMethod(
                            "HandleComponent",
                            BindingFlags.Public | BindingFlags.Instance, new[]
                            {
                                componentType,
                                typeof(object)
                            }) :
                        systemType.GetMethod(
                            "HandleComponent",
                            BindingFlags.Public | BindingFlags.Instance, new[]
                            {
                                componentType
                            });

                    if (methodInfo == null && arg != null)
                    {
                        LoggingService.Error(
                            // arg == null ?
                            //     "System {0} does not handle component {1}" :
                                "System {0} does not handle component {1} with additional parameter of type object.",
                            systemType.Name,
                            componentType.Name);
                        continue;
                    }
                }

                try
                {
                    var args = new object?[] { component };
                    if ((methodInfo?.GetParameters().Length ?? 0) > 1)
                        args = args.Concat(new[] { arg }).ToArray();
                    methodInfo?.Invoke(this, args);
                }
                catch (Exception e)
                {
                    LoggingService.Trace("Error executing handler for component {0} via system {1}.",
                        e,
                        component.GetType().Name,
                        GetType().Name);
                    // A swallowed handler exception is invisible at Trace level, which is
                    // fine for a game that should keep drawing but wrong for a server,
                    // where it would silently corrupt state instead of failing.
                    if (ecsManager.ThrowOnHandlerException)
                        throw;
                }
            }
            AfterStep(arg);
        }
    }
}
