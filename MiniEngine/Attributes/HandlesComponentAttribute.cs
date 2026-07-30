using System;

namespace MiniEngine
{
    // AllowMultiple, because a system can legitimately act on several component types:
    // CollisionSystem handles both collider shapes. Generic attributes count as one
    // attribute for this purpose regardless of their type argument, so without it the
    // second declaration is a duplicate.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HandlesComponentAttribute<T> : Attribute, IHandlesComponentAttribute where T : Component
    {
        public Type ComponentType { get; init; } = typeof(T);
    }
}
