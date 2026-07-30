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
            // No game speed factor here any more: it is a property of time itself, applied
            // once by the WorldClock, so a system that reapplied it would scale twice.
            motionComponent.Velocity += motionComponent.Acceleration * DeltaTimeF;
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
