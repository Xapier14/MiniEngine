using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine.Components
{
    [RequiresComponent<Transform>]
    public class Sprite : Component
    {
        public Vector2F Offset = Vector2F.Zero;

        public MemoryResource? SpriteResource { get; set; }
    }
}
