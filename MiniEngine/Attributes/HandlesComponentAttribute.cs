using System;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlesComponentAttribute<T> : Attribute, IHandlesComponentAttribute where T : Component
    {
        public Type ComponentType { get; init; } = typeof(T);
    }
}
