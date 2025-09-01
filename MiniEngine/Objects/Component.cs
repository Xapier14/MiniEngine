using System;

namespace MiniEngine
{
    public abstract class Component : IComponent
    {
        public Entity? Owner { get; private set; }
        public string Id { get; } = Guid.NewGuid().ToString();

        public void SetOwnerIfNotNull(Entity? owner)
        {
            Owner ??= owner;
        }
    }
}
