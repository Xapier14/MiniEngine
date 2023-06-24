using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    [HandlesComponent<Sprite>]
    public class DrawSystem : System
    {
        public DrawSystem()
        {
            LoggingService.Debug("Sprite system initialized");
        }

        public void HandleComponent(Sprite spriteComponent)
        {
            var entity = spriteComponent.Owner!;
            var transformComponent = entity.GetComponent<Transform>()!;

            var position = transformComponent.Translate + spriteComponent.Offset;
            var size = spriteComponent.Size;

            Graphics.DrawTexture(spriteComponent.SpriteResource, position, size);
        }

        protected override void Step(object? arg)
        {

        }
    }
}
