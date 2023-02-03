using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    public class MotionSystem : System
    {
        public MotionSystem()
        {
            LoggingService.Debug("Motion system initialized");
        }

        public void HandleComponent(Motion motionComponent)
        {
        }
    }
}
