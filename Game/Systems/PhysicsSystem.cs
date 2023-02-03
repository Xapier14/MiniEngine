using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    public class PhysicsSystem : System
    {
        public PhysicsSystem()
        {
            LoggingService.Debug("Physics system initialized");
        }

        public void HandleComponent(PhysicsBehavior physicsComponent)
        {
            var entity = physicsComponent.Owner!;
            var collider = entity.GetComponent<Collider>();
        }
    }
}
