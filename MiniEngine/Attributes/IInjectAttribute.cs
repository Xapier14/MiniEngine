using System;

namespace MiniEngine
{
    public interface IInjectAttribute
    {
        public Type InjectType { get; }
    }
}
