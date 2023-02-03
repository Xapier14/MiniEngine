using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    public class TransformSystem : System
    {
        public TransformSystem()
        {
            LoggingService.Debug("Transform system initialized");
        }

        public void HandleComponent(Transform transformComponent)
        {
            LoggingService.Debug("Handling...");
        }
    }
}
