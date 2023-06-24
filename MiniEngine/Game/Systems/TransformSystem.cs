using MiniEngine.Utility;

namespace MiniEngine
{
    public class TransformSystem : System
    {
        public TransformSystem()
        {
            LoggingService.Debug("Transform system initialized");
        }

        //public void HandleComponent(Motion motionComponent)
        //{
        //    var transformComponent = motionComponent.Owner?.GetComponent<Transform>();
        //    if (transformComponent is null)
        //        return;
        //    var translate = transformComponent.Translate;
        //    var velocity = motionComponent.Velocity;
        //    var withRelationToAngularVelocity =
        //        Vector2F.From(
        //            Convert.ToSingle(velocity.Angle + motionComponent.AngularVelocity * DeltaTime),
        //            Convert.ToSingle(velocity.Magnitude * DeltaTime)
        //            );
        //    translate += withRelationToAngularVelocity;
        //    transformComponent.Translate = translate;
        //    //LoggingService.Debug("Handled! {0}x{1} Delta: {2}s", 
        //    //    transformComponent.TranslateX,
        //    //    transformComponent.TranslateY,
        //    //    DeltaTime);
        //}

        protected override void Step(object? arg)
        {

        }
    }
}
