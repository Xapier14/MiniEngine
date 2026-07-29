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
        private readonly IReadOnlyEngineSetup? _setup;

        /// <summary>
        /// Headless constructor. Simulation runs at a game speed of 1 with no engine present.
        /// </summary>
        public PhysicsSystem() : this(null)
        {
        }

        public PhysicsSystem(IReadOnlyEngineSetup? setup)
        {
            _setup = setup;
            LoggingService.Debug("Physics system initialized");
        }

        public void HandleComponent(Motion motionComponent)
        {
            var delta = (_setup?.GameSpeed).GetValueOrDefault(1f) * DeltaTimeF;

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
