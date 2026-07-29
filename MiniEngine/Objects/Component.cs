using System;

namespace MiniEngine
{
    public abstract class Component : IComponent
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }
}
