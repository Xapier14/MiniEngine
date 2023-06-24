using System;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class InjectAttribute<T> : Attribute, IInjectAttribute
    {
        public Type InjectType { get; protected set; }

        public InjectAttribute()
        {
            InjectType = typeof(T);
        }
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class InjectAttribute : InjectAttribute<Component>
    {
        public InjectAttribute()
        {
            InjectType = typeof(Component);
        }
    }
}
