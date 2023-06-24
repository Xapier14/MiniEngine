using System;

namespace MiniEngine
{
    public interface IHandlesComponentAttribute
    {
        public Type ComponentType { get; }
    }
}
