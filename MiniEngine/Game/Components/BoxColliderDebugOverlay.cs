namespace MiniEngine.Components
{
    [OnlyOneOfType]
    [RequiresComponent<BoxCollider>, RequiresComponent<Drawable>]
    public class BoxColliderDebugOverlay : Component
    {

    }
}
