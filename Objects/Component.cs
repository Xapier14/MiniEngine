namespace MiniEngine
{
    public abstract class Component
    {
        public Entity? Owner { get; private set; }

        public void SetOwnerIfNotNull(Entity? owner)
        {
            Owner ??= owner;
        }
    }
}
