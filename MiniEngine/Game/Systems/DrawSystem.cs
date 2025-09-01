using System;
using System.Collections.Generic;
using System.Linq;
using MiniEngine.Components;
using MiniEngine.Utility;
using MiniEngine.Windowing;

namespace MiniEngine
{
    [HandlesComponent<Components.Drawable>]
    public class DrawSystem : System
    {
        private readonly GameEngine _gameEngine;

        private readonly
            PriorityQueue<(Sprite? Sprite, Vector2F Position, Size Size, float ImageAngle, Vector2F Center, (bool X, bool Y) Flip), int>
            _drawQueue = new();

        private readonly PriorityQueue<Action, int> _afterDrawQueue = new();

        private LinkedList<double> _fpsCache = [];

        public double Fps => 1.0 / DeltaTime;

        public DrawSystem()
        {
            _gameEngine = GameContext.GetGameEngine();
            LoggingService.Debug("Drawable system initialized");
        }

        public void HandleComponent(Drawable drawableComponent)
        {
            if (!drawableComponent.DisableManagedDraw)
            {
                var window = _gameEngine.WindowManager.GameWindow!;
                var entity = drawableComponent.Owner!;
                var scene = entity.ParentScene;
                var transformComponent = entity.GetComponent<Transform>();
                var cameraHalfWidth = window.WindowSize.Width / 2f;
                var cameraHalfHeight = window.WindowSize.Height / 2f;
                var cameraOrigin = scene?.FollowEntity?.GetComponent<Transform>().Translate ?? scene?.ViewPosition ?? (0, 0);
                var cameraAngle = scene?.FollowEntity?.GetComponent<Drawable>().Rotation ?? scene?.ViewRotation ?? 0f;

                Vector2F drawOrigin = (cameraHalfWidth, cameraHalfHeight);

                // get object vector relative to drawOrigin
                // TODO: make object offset respect camera angle
                var objectRelative = transformComponent.Translate - cameraOrigin;
                var drawPosition = Vector2F.From(objectRelative.Angle + cameraAngle, objectRelative.Magnitude) + drawOrigin - drawableComponent.Offset;
                //_afterDrawQueue.Enqueue(() =>
                //{
                //    Graphics.DrawRectangle(drawPosition - (1, 1), drawPosition + (1, 1), Color.Red, Color.Black);
                //}, 0);
                _drawQueue.Enqueue(
                    (drawableComponent.Sprite, drawPosition, drawableComponent.Size,
                        drawableComponent.Rotation + (_gameEngine.Setup.InvertYAxis == true ? -cameraAngle : cameraAngle),
                        drawableComponent.Offset, (drawableComponent.FlipX, drawableComponent.FlipY)), drawableComponent.Depth);
            }

            _afterDrawQueue.Enqueue(drawableComponent.RaiseExternalDrawCall, drawableComponent.Depth);

            // var drawPosition = Vector2F.From(angleToDrawOrigin - cameraAngle, distanceToDrawOrigin) + drawOrigin;
            // LoggingService.Debug("[Mario] drawOrigin: {0}, drawPosition: {1}, realPosition: {2}", drawOrigin, drawPosition, position);

            // _drawQueue.Enqueue((drawableComponent.Sprite, drawPosition, size, imageAngle, (0f,0f)-drawableComponent.Offset), drawableComponent.Depth);
        }

        protected override void Step(object? arg)
        {
            //_afterDrawQueue.Enqueue(() =>
            //{
            //    if (_fpsCache.Count >= 100) _fpsCache.RemoveFirst();
            //    _fpsCache.AddLast(Fps);
            //    Graphics.DrawText("fonts/font-ui.ttf"!, 12, (12, 12), $"FPS: {_fpsCache.Average():F1}", Color.Black);
            //}, -1);
            //_afterDrawQueue.Enqueue(() =>
            //{
            //    Graphics.DrawPixel((_window.WindowSize.Width / 2, _window.WindowSize.Height / 2), Color.Green);
            //}, -1);
        }

        protected override void AfterStep(object? arg)
        {
            var newColor = _gameEngine.SceneManager.CurrentScene?.BackgroundColor ?? Color.Black;
            var oldColor = _gameEngine.Graphics.GetDrawColor();
            _gameEngine.Graphics.SetDrawColor(newColor);
            _gameEngine.Graphics.RenderClear();
            _gameEngine.Graphics.SetDrawColor(oldColor);
            while (_drawQueue.TryDequeue(out var drawCall, out _))
            {
                _gameEngine.Graphics.DrawSprite(drawCall.Sprite, drawCall.Position, drawCall.Size, drawCall.ImageAngle, drawCall.Center, drawCall.Flip.X, drawCall.Flip.Y);
            }
            while (_afterDrawQueue.TryDequeue(out var action, out _))
                action.Invoke();
            _gameEngine.Graphics.RenderPresent();
        }
    }
}
