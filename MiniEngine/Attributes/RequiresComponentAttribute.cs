using System;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiresComponentAttribute<T> : Attribute, IRequiresComponentAttribute
    {
        public Type RequiredComponentType { get; init; }
        public RequiresComponentAttribute()
        {
            if (!typeof(T).IsAssignableTo(typeof(Component)))
                throw new ArgumentException("Invalid component type.");
            RequiredComponentType = typeof(T);
        }
    }
}
