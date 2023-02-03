using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    public class SpriteSystem : System
    {
        public SpriteSystem()
        {
            LoggingService.Debug("Sprite system initialized");
        }

        public void HandleComponent(Sprite spriteComponent)
        {
        }
    }
}
