using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

#nullable enable

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="CompositionObject"/> type
    /// </summary>
    public static class CompositionObjectExtensions
    {
        /// <summary>
        /// Starts an <see cref="ExpressionAnimation"/> to keep the size of the source <see cref="CompositionObject"/> in sync with the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="source">The <see cref="CompositionObject"/> to start the animation on</param>
        /// <param name="target">The target <see cref="UIElement"/> to read the size updates from</param>
        public static void BindSize(this CompositionObject source, UIElement target)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(target);
            ExpressionAnimation bindSizeAnimation = source.Compositor.CreateExpressionAnimation($"{nameof(visual)}.Size");
            bindSizeAnimation.SetReferenceParameter(nameof(visual), visual);

            // Start the animation
            source.StartAnimation("Size", bindSizeAnimation);
        }

        /// <summary>
        /// Tries to retrieve the <see cref="CoreDispatcher"/> instance of the input <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> instance</param>
        /// <param name="dispatcher">The resulting <see cref="CoreDispatcher"/>, if existing</param>
        public static bool TryGetDispatcher(this CompositionObject source, out CoreDispatcher? dispatcher)
        {
            try
            {
                dispatcher = source.Dispatcher;

                return true;
            }
            catch (ObjectDisposedException)
            {
                // I'm sorry Jack, I was too late! :'(
                dispatcher = null;

                return false;
            }
        }

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

            TaskCompletionSource<object?> tcs = new TaskCompletionSource<object?>();

            batch.Completed += (s, e) => tcs.SetResult(null);

            target.StartAnimation(property, animation);

            batch.End();

            return tcs.Task;
        }
    }
}
