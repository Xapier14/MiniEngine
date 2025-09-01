using System;
using System.Collections.Generic;

namespace MiniEngine.Components
{
    public class AnimationEventArgs : EventArgs
    {
        public IAnimation Current { get; set; }
        public IAnimation? Next { get; set; }

        public AnimationEventArgs(IAnimation current)
        {
            Current = current;
        }
    }

    /// <summary>
    /// <para>Provides a queue for animations as well as setting up an animation cycle.</para>
    /// <para>Overrides the <c>Sprite</c> set by the <c>Drawable</c> component.</para>
    /// <para>Requires a <c>Drawable</c> component.</para>
    /// </summary>
    [OnlyOneOfType]
    [RequiresComponent<Drawable>]
    public class Animator : Component
    {
        internal readonly Queue<AnimationSystem.AnimationTask> Tasks = [];
        public IAnimation? CurrentAnimation { get; set; }
        public event Action<Animator>? FinishedCurrentAnimation;
        public event Func<Animator, AnimationEventArgs, bool>? NextAnimation;

        public void EnqueueAnimation(IAnimation animation, bool requeue = false, int priority = 0)
        {
            Tasks.Enqueue(new AnimationSystem.AnimationTask(AnimationSystem.Task.Add, animation, requeue, priority));
        }

        /// <summary>
        /// Ends the current animation.
        /// If the current animation has the requeue flag, the animation will be requeued.
        /// </summary>
        public void EndCurrentAnimation()
        {
            Tasks.Enqueue(new AnimationSystem.AnimationTask(AnimationSystem.Task.Remove, null, true));
        }

        /// <summary>
        /// Removes the first instance of the specified animation from the animation queue.
        /// If the animation is not specified, the current animation is removed.
        /// </summary>
        /// <param name="animation">The animation to remove from the animation queue.</param>
        public void ClearAnimation(IAnimation? animation)
        {
            Tasks.Enqueue(new AnimationSystem.AnimationTask(AnimationSystem.Task.Remove, animation));
        }

        /// <summary>
        /// Clears the entire animation queue.
        /// </summary>
        public void ClearAnimationQueue()
        {
            Tasks.Enqueue(new AnimationSystem.AnimationTask(AnimationSystem.Task.Clear));
        }
    }
}
