using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    [HandlesComponent<Scriptable>]
    public class ScriptSystem : System
    {
        public ScriptSystem()
        {
            LoggingService.Debug("Script system initialized.");
        }

        public void OnComponentRegister(Scriptable scriptableComponent)
        {
            scriptableComponent.Create?.Invoke(scriptableComponent);
        }

        public void OnComponentRemove(Scriptable scriptableComponent)
        {
            scriptableComponent.Destroy?.Invoke(scriptableComponent);
        }

        public void HandleComponent(Scriptable scriptableComponent, object type)
        {
            var eventType = (ScriptEvent)type;
            scriptableComponent.Delta = DeltaTime;
            switch (eventType)
            {
                case ScriptEvent.BeforeUpdate:
                    scriptableComponent.BeforeUpdate?.Invoke(scriptableComponent);
                    break;
                case ScriptEvent.Update:
                    scriptableComponent.Update?.Invoke(scriptableComponent);
                    break;
                case ScriptEvent.AfterUpdate:
                    scriptableComponent.AfterUpdate?.Invoke(scriptableComponent);
                    break;
                case ScriptEvent.BeforeDraw:
                    scriptableComponent.BeforeDraw?.Invoke(scriptableComponent);
                    break;
                case ScriptEvent.AfterDraw:
                    scriptableComponent.AfterDraw?.Invoke(scriptableComponent);
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
