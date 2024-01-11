using System.Collections.Generic;
using MiniEngine.Components;
using MiniEngine.Utility;
using MiniEngine.Windowing;

namespace MiniEngine
{
    [HandlesComponent<Sprite>]
    public class DrawSystem : System
    {
        private readonly GameWindow _window;

        private readonly
            PriorityQueue<(MemoryResource? Sprite, Vector2F Position, Size Size, float ImageAngle, Vector2F Center), int>
            _drawQueue = new();

        public double Fps => 1.0 / DeltaTime;

        public DrawSystem()
        {
            _window = WindowManager.GameWindow!;
            LoggingService.Debug("Sprite system initialized");
        }

        public void HandleComponent(Sprite spriteComponent)
        {
            var entity = spriteComponent.Owner!;
            var scene = entity.ParentScene;
            var transformComponent = entity.GetComponent<Transform>()!;

            var cameraHalfWidth = _window.WindowSize.Width / 2f;
            var cameraHalfHeight = _window.WindowSize.Height / 2f;
            var cameraOrigin = scene?.ViewPosition ?? (0, 0);
            var cameraAngle = scene?.ViewRotation ?? 0f;

            Vector2F drawOrigin = (cameraHalfWidth, cameraHalfHeight);

            // get object vector relative to drawOrigin
            // TODO: make object offset respect camera angle
            var position = transformComponent.Translate + spriteComponent.Offset;
            var relativePosition = position - cameraOrigin;
            var size = spriteComponent.Size;
            var imageAngle = spriteComponent.Rotation - cameraAngle;

            var distanceToDrawOrigin = relativePosition.Magnitude;
            var angleToDrawOrigin = relativePosition.Angle;

            var drawPosition = Vector2F.From(angleToDrawOrigin - cameraAngle, distanceToDrawOrigin) + drawOrigin;
            // LoggingService.Debug("[Mario] drawOrigin: {0}, drawPosition: {1}, realPosition: {2}", drawOrigin, drawPosition, position);

            _drawQueue.Enqueue((spriteComponent.SpriteResource, drawPosition, size, imageAngle, (0, 0)), spriteComponent.Depth);
        }

        protected override void AfterStep(object? arg)
        {
            var newColor = SceneManager.CurrentScene?.BackgroundColor ?? Color.Black;
            var oldColor = Graphics.GetDrawColor();
            Graphics.SetDrawColor(newColor);
            Graphics.RenderClear();
            Graphics.SetDrawColor(oldColor);
            while (_drawQueue.TryDequeue(out var drawCall, out _))
            {
                Graphics.DrawTexture(drawCall.Sprite, drawCall.Position, drawCall.Size, drawCall.ImageAngle, drawCall.Center);
            }
            Graphics.RenderPresent();
        }
    }
}
