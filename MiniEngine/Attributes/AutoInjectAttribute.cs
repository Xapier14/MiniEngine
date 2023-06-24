using System;

namespace MiniEngine
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class AutoInjectAttribute<T> : Attribute, IAutoInjectAttribute
    {
        public Type InjectType { get; }

        public AutoInjectAttribute()
        {
            InjectType = typeof(T);
            if (InjectType.IsEquivalentTo(typeof(Component)))
                throw new ArgumentException("Auto inject type must not be 'Component'.");
        }
    }
}
