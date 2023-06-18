using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    [HandlesComponent<Motion>]
    public class MotionSystem : System
    {
        public MotionSystem()
        {
            LoggingService.Debug("Motion system initialized");
        }

        public void HandleComponent(Motion motionComponent)
        {
            var entity = motionComponent.Owner;
            if (entity is null)
                return;
            var transformComponent = entity.GetComponent<Transform>();

            var acceleration = motionComponent.Acceleration;
            motionComponent.Velocity += acceleration * (float)DeltaTime;

            if (transformComponent != null)
            {
                transformComponent.Translate += motionComponent.Velocity * (float)DeltaTime;
            }
        }
    }
}
