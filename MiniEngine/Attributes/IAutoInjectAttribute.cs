using System;

namespace MiniEngine
{
    public interface IAutoInjectAttribute
    {
        public Type InjectType { get; }
    }
}
