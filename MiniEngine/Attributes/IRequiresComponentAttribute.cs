using System;

namespace MiniEngine
{
    public interface IRequiresComponentAttribute
    {
        public Type RequiredComponentType { get; }
    }
}
