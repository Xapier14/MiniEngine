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

            var topLeft = (transform.Translate.X - boxCollider.Left, transform.Translate.Y - boxCollider.Top) + viewportOffset;
            var bottomRight = (transform.Translate.X + boxCollider.Right, transform.Translate.Y + boxCollider.Bottom) + viewportOffset;
            gameEngine.Graphics.DrawRectangle(topLeft, bottomRight, Color.Blue, null);
        }
    }
}
