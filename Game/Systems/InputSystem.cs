using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    public class InputSystem : System
    {
        public InputSystem()
        {
            LoggingService.Debug("Input system initialized.");    
        }

        public void HandleComponent(InputHandler inputComponent)
        {
        }
    }
}
