using MiniEngine.Components;
using MiniEngine.Windowing;

namespace MiniEngine
{
    [HandlesComponent<BoxColliderDebugOverlay>]
    public class ColliderDebugOverlaySystem : System
    {
        private static GameWindow? _gameWindow => GameContext.GetGameEngine().WindowManager.GameWindow;
        
        public void OnComponentRegister(BoxColliderDebugOverlay overlay)
        {
            var entity = overlay.Owner!;
            var drawable = entity.GetComponent<Drawable>();
            drawable.ExternalDrawCall += DrawRectangleDebugOverlay;
        }

        public void OnComponentRemove(BoxColliderDebugOverlay overlay)
        {
            var entity = overlay.Owner!;
            var drawable = entity.GetComponent<Drawable>();
            drawable.ExternalDrawCall -= DrawRectangleDebugOverlay;
        }

        public void HandleComponent(BoxColliderDebugOverlay overlay)
        {

        }

        private static void DrawRectangleDebugOverlay(Drawable drawable)
        {
            var entity = drawable.Owner!;
            var transform = entity.GetComponent<Transform>();
            var boxCollider = entity.GetComponent<BoxCollider>();

            var viewportOffset = (_gameWindow!.WindowSize.Width / 2f, _gameWindow!.WindowSize.Height / 2f)
                - entity.ParentScene!.ViewPosition;

            var topLeft = (transform.Translate.X - boxCollider.Left, transform.Translate.Y - boxCollider.Top) + viewportOffset;
            var bottomRight = (transform.Translate.X + boxCollider.Right, transform.Translate.Y + boxCollider.Bottom) + viewportOffset;
            GameContext.GetGameEngine().Graphics.DrawRectangle(topLeft, bottomRight, Color.Blue, null);
        }
    }
}
