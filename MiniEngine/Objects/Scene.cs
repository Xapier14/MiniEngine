using System.Collections.Generic;

namespace MiniEngine
{
    public abstract class Scene
    {
        private readonly List<Entity> _entities = new();
        public IReadOnlyCollection<Entity> Entities => _entities;
    }
}
