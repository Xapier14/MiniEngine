using System.Collections.Generic;
using MiniEngine.Components;
using MiniEngine.Utility;

namespace MiniEngine
{
    [HandlesComponent<Motion>]
    public class PhysicsSystem : System
    {
        public struct MovementVector(Motion MotionComponent, Vector2F MotionVector);
        private readonly List<MovementVector> _movementVectors = [];

        public PhysicsSystem()
        {
            LoggingService.Debug("Physics system initialized");
        }

        public void HandleComponent(Motion motionComponent)
        {
            var delta = GameContext.GetGameEngine().Setup.GameSpeed.GetValueOrDefault(1f) * DeltaTimeF;

            var entity = motionComponent.Owner;
            if (entity is null)
                return;
            var colliders = entity.GetComponents<ICollider>();
            var behavior = entity.TryGetComponent<PhysicsBehavior>();

            var acceleration = motionComponent.Acceleration;
            motionComponent.Velocity += acceleration * delta;
        }

        protected override void AfterStep(object? arg)
        {
            // processes & solve movement vectors
            foreach (var vector in _movementVectors)
            {

            }
            _movementVectors.Clear();
            base.AfterStep(arg);
        }
    }
}
