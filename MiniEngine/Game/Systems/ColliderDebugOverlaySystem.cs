using MiniEngine.Components;
using MiniEngine.Windowing;

namespace MiniEngine
{
    [HandlesComponent<BoxColliderDebugOverlay>]
    public class ColliderDebugOverlaySystem(GameEngine gameEngine) : System
    {
        private GameWindow? _gameWindow => gameEngine.WindowManager.GameWindow;

        public void OnComponentRegister(BoxColliderDebugOverlay overlay)
        {
            var drawable = Ecs.GetSibling<Drawable>(overlay);
            if (drawable != null)
                drawable.ExternalDrawCall += DrawRectangleDebugOverlay;
        }

        public void OnComponentRemove(BoxColliderDebugOverlay overlay)
        {
            var drawable = Ecs.GetSibling<Drawable>(overlay);
            if (drawable != null)
                drawable.ExternalDrawCall -= DrawRectangleDebugOverlay;
        }

        public void HandleComponent(BoxColliderDebugOverlay overlay)
        {

        }

        private void DrawRectangleDebugOverlay(Drawable drawable)
        {
            var transform = Ecs.GetSibling<Transform>(drawable);
            var boxCollider = Ecs.GetSibling<BoxCollider>(drawable);
            if (transform == null || boxCollider == null)
                return;

            var viewPosition = Ecs.ActiveScene?.ViewPosition ?? Vector2F.Zero;
            var viewportOffset = (_gameWindow!.WindowSize.Width / 2f, _gameWindow!.WindowSize.Height / 2f)
                - viewPosition;

            var (min, max) = boxCollider.Bounds(transform.Translate);
            gameEngine.Graphics.DrawRectangle(min + viewportOffset, max + viewportOffset, Color.Blue, null);
        }
    }
}
