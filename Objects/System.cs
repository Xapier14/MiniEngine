using System;
using System.Collections.Generic;
using System.Reflection;
using MiniEngine.Utility;

namespace MiniEngine
{
    public abstract class System
    {
        private long? _lastTick;
        public double DeltaTime { get; private set; }

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
                var systemType = GetType();
                var componentType = component.GetType();
                var handler = systemType.GetMethod(
                    "HandleComponent",
                    BindingFlags.Public | BindingFlags.Instance,
                    new []
                    {
                        componentType
                    });
                if (handler == null)
                {
                    LoggingService.Error(
                        "System {0} does not handle component {1}.",
                        systemType.Name,
                        componentType.Name);
                    continue;
                }
                handler.Invoke(this, new object?[] { component });
            }
        }
    }
}
