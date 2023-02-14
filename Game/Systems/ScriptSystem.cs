using MiniEngine.Components;
using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    [HandlesComponent<Script>]
    public class ScriptSystem : System
    {
        public ScriptSystem()
        {
            LoggingService.Debug("Script system initialized.");    
        }

        public void HandleComponent(Script scriptComponent, object type)
        {
            var eventType = (ScriptEventType)type;
            scriptComponent.Delta = DeltaTime;
            switch (eventType)
            {
                case ScriptEventType.Create:
                    scriptComponent.Create?.Invoke(scriptComponent);
                    break;
                case ScriptEventType.BeforeUpdate:
                    scriptComponent.BeforeUpdate?.Invoke(scriptComponent);
                    break;
                case ScriptEventType.Update:
                    scriptComponent.Update?.Invoke(scriptComponent);
                    break;
                case ScriptEventType.AfterUpdate:
                    scriptComponent.AfterUpdate?.Invoke(scriptComponent);
                    break;
                case ScriptEventType.BeforeDraw:
                    scriptComponent.BeforeDraw?.Invoke(scriptComponent);
                    break;
                case ScriptEventType.AfterDraw:
                    scriptComponent.AfterDraw?.Invoke(scriptComponent);
                    break;
                case ScriptEventType.Destroy:
                    scriptComponent.Destroy?.Invoke(scriptComponent);
                    break;
                default:
                    return;
            }
        }
    }
}
