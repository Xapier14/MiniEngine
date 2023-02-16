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
            var component = motionComponent.Owner;
            if (component is null)
                return;
            var acceleration = motionComponent.Acceleration;
            var velocity = motionComponent.Velocity;

            velocity.X += acceleration.X * 1f;
            velocity.Y += acceleration.X * 1f;
            motionComponent.Velocity = velocity;
        }
    }
}
