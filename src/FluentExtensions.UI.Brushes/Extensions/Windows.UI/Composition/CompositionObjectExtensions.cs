using System;
using System.Threading.Tasks;

#nullable enable

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="CompositionObject"/> type
    /// </summary>
    public static class CompositionObjectExtensions
    {
        /// <summary>
        /// Starts an animation on the given property of a <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="target">The target <see cref="CompositionObject"/></param>
        /// <param name="property">The name of the property to animate</param>
        /// <param name="value">The final value of the property</param>
        /// <param name="duration">The animation duration</param>
        public static Task StartAnimationAsync(this CompositionObject target, string property, float value, TimeSpan duration)
        {
            // Stop previous animations
            target.StopAnimation(property);

            // Setup the animation
            ScalarKeyFrameAnimation animation = target.Compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = duration;

            // Get the batch and start the animations
            CompositionScopedBatch batch = target.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            target.StartAnimation(property, animation);
            batch.End();
            return tcs.Task;
        }
    }
}
