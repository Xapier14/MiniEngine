namespace MiniEngine
{
    public interface IComponent
    {
        public Entity? Owner { get; }

        public void SetOwnerIfNotNull(Entity? owner);
    }
}
