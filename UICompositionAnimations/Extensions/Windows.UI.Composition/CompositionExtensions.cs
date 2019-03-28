using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for some composition types
    /// </summary>
    [PublicAPI]
    public static class CompositionExtensions
    {
        #region Easing functions

        /// <summary>
        /// Creates a <see cref="CubicBezierEasingFunction"/> from the input control points
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="x1">The X coordinate of the first control point</param>
        /// <param name="y1">The Y coordinate of the first control point</param>
        /// <param name="x2">The X coordinate of the second control point</param>
        /// <param name="y2">The Y coordinate of the second control point</param>
        [Pure, NotNull]
        public static CubicBezierEasingFunction GetEasingFunction([NotNull] this CompositionObject source, float x1, float y1, float x2, float y2)
        {
            return source.Compositor.CreateCubicBezierEasingFunction(new Vector2 { X = x1, Y = y1 }, new Vector2 { X = x2, Y = y2 });
        }

        /// <summary>
        /// Creates the appropriate <see cref="CubicBezierEasingFunction"/> from the given easing function name
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="ease">The target easing function to create</param>
        [Pure, NotNull]
        public static CubicBezierEasingFunction GetEasingFunction([NotNull] this CompositionObject source, Easing ease)
        {
            switch (ease)
            {
                case Easing.Linear: return source.GetEasingFunction(0, 0, 1, 1);
                case Easing.SineEaseIn: return source.GetEasingFunction(0.4f, 0, 1, 1);
                case Easing.SineEaseOut: return source.GetEasingFunction(0, 0, 0.6f, 1);
                case Easing.SineEaseInOut: return source.GetEasingFunction(0.4f, 0, 0.6f, 1);
                case Easing.QuadraticEaseIn: return source.GetEasingFunction(0.8f, 0, 1, 1);
                case Easing.QuadraticEaseOut: return source.GetEasingFunction(0, 0, 0.2f, 1);
                case Easing.QuadraticEaseInOut: return source.GetEasingFunction(0.8f, 0, 0.2f, 1);
                case Easing.CircleEaseIn: return source.GetEasingFunction(1, 0, 1, 0.8f);
                case Easing.CircleEaseOut: return source.GetEasingFunction(0, 0.3f, 0, 1);
                case Easing.CircleEaseInOut: return source.GetEasingFunction(0.9f, 0, 0.1f, 1);
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }

        #endregion

        #region Animations

        /// <summary>
        /// Creates and starts a scalar animation on the current <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="target">The target to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginScalarAnimation(
            [NotNull] this CompositionObject target,
            [NotNull] string propertyPath,
            float? from, float to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            target.StartAnimation(propertyPath, target.Compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, ease));
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector2"/> animation on the current <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="target">The target to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginVector2Animation(
            [NotNull]this CompositionObject target,
            [NotNull] string propertyPath,
            Vector2? from, Vector2 to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            target.StartAnimation(propertyPath, target.Compositor.CreateVector2KeyFrameAnimation(from, to, duration, delay, ease));
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector3"/> animation on the current <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="target">The target to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginVector3Animation(
            [NotNull] this CompositionObject target,
            [NotNull] string propertyPath,
            Vector3? from, Vector3 to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            target.StartAnimation(propertyPath, target.Compositor.CreateVector3KeyFrameAnimation(from, to, duration, delay, ease));
        }

        /// <summary>
        /// Starts an animation on the given property of a <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="target">The target <see cref="CompositionObject"/></param>
        /// <param name="property">The name of the property to animate</param>
        /// <param name="value">The final value of the property</param>
        /// <param name="duration">The animation duration</param>
        public static Task StartAnimationAsync([NotNull] this CompositionObject target, string property, float value, TimeSpan duration)
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

        #endregion

        #region Misc

        /// <summary>
        /// Adds a <see cref="CompositionBrush"/> instance on top of the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="brush">The <see cref="CompositionBrush"/> instance to display</param>
        /// <param name="target">The target <see cref="FrameworkElement"/> that will host the effect</param>
        public static void AttachToElement([NotNull] this CompositionBrush brush, [NotNull] FrameworkElement target)
        {
            // Add the brush to a sprite and attach it to the target element
            SpriteVisual sprite = Window.Current.Compositor.CreateSpriteVisual();
            sprite.Brush = brush;
            sprite.Size = new Vector2((float)target.ActualWidth, (float)target.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(target, sprite);
        }

        /// <summary>
        /// Starts an <see cref="ExpressionAnimation"/> to keep the size of the source <see cref="CompositionObject"/> in sync with the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="source">The <see cref="CompositionObject"/> to start the animation on</param>
        /// <param name="target">The target <see cref="UIElement"/> to read the size updates from</param>
        public static void BindSize([NotNull] this CompositionObject source, [NotNull] UIElement target)
        {
            Visual visual = target.GetVisual();
            ExpressionAnimation bindSizeAnimation = Window.Current.Compositor.CreateExpressionAnimation($"{nameof(visual)}.Size");
            bindSizeAnimation.SetReferenceParameter(nameof(visual), visual);

            // Start the animation
            source.StartAnimation("Size", bindSizeAnimation);
        }

        /// <summary>
        /// Tries to retrieve the <see cref="CoreDispatcher"/> instance of the input <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> instance</param>
        /// <param name="dispatcher">The resulting <see cref="CoreDispatcher"/>, if existing</param>
        [MustUseReturnValue]
        public static bool TryGetDispatcher([NotNull] this CompositionObject source, out CoreDispatcher dispatcher)
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

        #endregion
    }
}
