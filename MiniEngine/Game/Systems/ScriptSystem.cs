﻿using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    [HandlesComponent<Script>]
    public class ScriptSystem : System
    {
        public ScriptSystem()
        {
            LoggingService.Debug("Script system initialized.");
        }

        public static void OnComponentRegister(Script scriptComponent)
        {
            scriptComponent.Create?.Invoke(scriptComponent);
        }

        public static void OnComponentRemove(Script scriptComponent)
        {
            scriptComponent.Destroy?.Invoke(scriptComponent);
        }

        public void HandleComponent(Script scriptComponent, object type)
        {
            var eventType = (ScriptEvent)type;
            scriptComponent.Delta = DeltaTime;
            switch (eventType)
            {
                case ScriptEvent.BeforeUpdate:
                    scriptComponent.BeforeUpdate?.Invoke(scriptComponent);
                    break;
                case ScriptEvent.Update:
                    scriptComponent.Update?.Invoke(scriptComponent);
                    break;
                case ScriptEvent.AfterUpdate:
                    scriptComponent.AfterUpdate?.Invoke(scriptComponent);
                    break;
                case ScriptEvent.BeforeDraw:
                    scriptComponent.BeforeDraw?.Invoke(scriptComponent);
                    break;
                case ScriptEvent.AfterDraw:
                    scriptComponent.AfterDraw?.Invoke(scriptComponent);
                    break;
                default:
                    return;
            }
        }

        protected override void Step(object? arg)
        {

        }
    }
}
