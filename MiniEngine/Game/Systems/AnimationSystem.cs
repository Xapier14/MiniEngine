using System;
using MiniEngine.Components;
using MiniEngine.Utility;
using System.Collections.Generic;
using System.Linq;

namespace MiniEngine
{
    [HandlesComponent<Animator>]
    public class AnimationSystem : System
    {
        public enum Task
        {
            Add,
            Remove,
            Clear,
        }

        public record AnimationTask(Task Task, IAnimation? Animation = null, bool Requeue = false, int? Priority = 0);

        private record QueuedAnimation(IAnimation Animation)
        {
            public bool Requeue { get; set; } = false;
        }

        private class AnimationState(Animator animator)
        {
            public Animator Animator { get; } = animator;
            public int CurrentFrameIndex = 0;
            public long AnimationStartTick { get; set; } = DateTime.Now.Ticks;
            public PriorityQueue<QueuedAnimation, int> AnimationQueue { get; } = new();
        }

        private readonly Dictionary<Animator, AnimationState> _states = [];

        public AnimationSystem()
        {
            LoggingService.Debug("Animation system initialized");
        }

        public void OnComponentRegister(Animator animator)
        {
            if (!_states.ContainsKey(animator))
            {
                LoggingService.Debug("Registered animator-{0}'s state to animation system.", animator.Id);
                _states[animator] = new AnimationState(animator);
            }
        }

        public void OnComponentRemove(Animator animator)
        {
            LoggingService.Debug("Removed animator-{0}'s state from animation system.", animator.Id);
            _states.Remove(animator);
        }

        public void HandleComponent(Animator animator)
        {
            var entity = animator.Owner!;
            var drawableComponent = entity.TryGetComponent<Drawable>();
            if (drawableComponent == null) return;

            if (!_states.TryGetValue(animator, out var state))
            {
                LoggingService.Warn("State was missing from animation system.", animator.Id);
                LoggingService.Warn("Registered animator-{0}'s state to animation system.", animator.Id);
                LoggingService.Warn("This is a bug, state should've been added during OnComponentRegister.");
                state = new AnimationState(animator);
                _states[animator] = state;
            }
            
            // process animator tasks
            while (animator.Tasks.TryDequeue(out var task))
            {
                switch (task.Task)
                {
                    case Task.Add:
                        if (task.Animation != null)
                        {
                            state.AnimationQueue.Enqueue(new QueuedAnimation(task.Animation)
                            {
                                Requeue = task.Requeue,
                            }, task.Priority ?? 0);
                        }
                        break;
                    case Task.Remove:
                        if (task.Animation != null)
                        {
                            var copies = state.AnimationQueue.UnorderedItems.Where(anim => anim.Element.Animation != task.Animation).ToArray();
                            state.AnimationQueue.Clear();
                            foreach (var (element, priority) in copies)
                            {
                                state.AnimationQueue.Enqueue(element, priority);
                            }
                        }
                        break;
                    case Task.Clear:
                        state.AnimationQueue.Clear();
                        break;
                }
            }

            // Update State
            if (animator.CurrentAnimation == null)
            {
                state.AnimationQueue.TryDequeue(out var animation, out var priority);
                if (animation != null)
                {
                    animator.CurrentAnimation = animation.Animation;
                    state.AnimationStartTick = DateTime.Now.Ticks;
                    if (animation.Requeue)
                    {
                        state.AnimationQueue.Enqueue(animation, priority);
                    }
                }
            }

            var currentAnimation = animator.CurrentAnimation;
            if (currentAnimation == null)
                return;

            var elapsedSinceAnimationStart = new TimeSpan(DateTime.Now.Ticks - state.AnimationStartTick);
            var animationDuration = 0f;
            if (currentAnimation.AnimationSpeed == 0)
            {
                state.CurrentFrameIndex = 0;
            }
            else
            {
                var frameDuration = Math.Abs(1000 / currentAnimation.AnimationSpeed);
                var elapsedFrames = elapsedSinceAnimationStart.TotalMilliseconds / frameDuration;
                animationDuration = frameDuration * currentAnimation.FrameCount;
                state.CurrentFrameIndex = (int)Math.Round(elapsedFrames % currentAnimation.FrameCount);
            }

            var currentFrame = currentAnimation.GetFrame(state.CurrentFrameIndex);

            // end animation if duration ended
            if (elapsedSinceAnimationStart.TotalMilliseconds > animationDuration)
            {
                animator.CurrentAnimation = null;
            }

            // Replace Drawable's image resource with state's image resource
            drawableComponent.Sprite = currentFrame;
        }
    }
}
