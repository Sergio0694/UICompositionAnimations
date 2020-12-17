using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using FluentExtensions.UI.Animations.Enums;

#nullable enable

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="CompositionObject"/> type
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Creates a <see cref="CubicBezierEasingFunction"/> from the input control points
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="x1">The X coordinate of the first control point</param>
        /// <param name="y1">The Y coordinate of the first control point</param>
        /// <param name="x2">The X coordinate of the second control point</param>
        /// <param name="y2">The Y coordinate of the second control point</param>
        /// <returns>A <see cref="CubicBezierEasingFunction"/> instance with the given control points</returns>
        [Pure]
        public static CubicBezierEasingFunction GetEasingFunction(this Compositor compositor, float x1, float y1, float x2, float y2)
        {
            return compositor.CreateCubicBezierEasingFunction(new Vector2 { X = x1, Y = y1 }, new Vector2 { X = x2, Y = y2 });
        }

        /// <summary>
        /// Creates a <see cref="CubicBezierEasingFunction"/> from the input control points
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="x1">The X coordinate of the first control point</param>
        /// <param name="y1">The Y coordinate of the first control point</param>
        /// <param name="x2">The X coordinate of the second control point</param>
        /// <param name="y2">The Y coordinate of the second control point</param>
        /// <returns>A <see cref="CubicBezierEasingFunction"/> instance with the given control points</returns>
        [Pure]
        public static CubicBezierEasingFunction GetEasingFunction(this CompositionObject source, float x1, float y1, float x2, float y2)
        {
            return source.Compositor.CreateCubicBezierEasingFunction(new Vector2 { X = x1, Y = y1 }, new Vector2 { X = x2, Y = y2 });
        }

        /// <summary>
        /// Creates the appropriate <see cref="CubicBezierEasingFunction"/> from the given easing function name
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="ease">The target easing function to create</param>
        /// <returns>A <see cref="CubicBezierEasingFunction"/> instance with the specified easing value</returns>
        [Pure]
        public static CubicBezierEasingFunction GetEasingFunction(this CompositionObject source, Easing ease)
        {
            return ease switch
            {
                Easing.Linear => source.GetEasingFunction(0, 0, 1, 1),
                Easing.SineEaseIn => source.GetEasingFunction(0.4f, 0, 1, 1),
                Easing.SineEaseOut => source.GetEasingFunction(0, 0, 0.6f, 1),
                Easing.SineEaseInOut => source.GetEasingFunction(0.4f, 0, 0.6f, 1),
                Easing.QuadraticEaseIn => source.GetEasingFunction(0.8f, 0, 1, 1),
                Easing.QuadraticEaseOut => source.GetEasingFunction(0, 0, 0.2f, 1),
                Easing.QuadraticEaseInOut => source.GetEasingFunction(0.8f, 0, 0.2f, 1),
                Easing.CircleEaseIn => source.GetEasingFunction(1, 0, 1, 0.8f),
                Easing.CircleEaseOut => source.GetEasingFunction(0, 0.3f, 0, 1),
                Easing.CircleEaseInOut => source.GetEasingFunction(0.9f, 0, 0.1f, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(ease), ease, $"Invalid easing value: {ease}")
            };
        }

        /// <summary>
        /// Creates the appropriate <see cref="CubicBezierEasingFunction"/> from the given easing function name
        /// </summary>
        /// <param name="source">The source <see cref="Compositor"/> used to create the easing function</param>
        /// <param name="ease">The target easing function to create</param>
        /// <returns>A <see cref="CubicBezierEasingFunction"/> instance with the specified easing value</returns>
        [Pure]
        public static CubicBezierEasingFunction GetEasingFunction(this Compositor compositor, Easing ease)
        {
            return ease switch
            {
                Easing.Linear => compositor.GetEasingFunction(0, 0, 1, 1),
                Easing.SineEaseIn => compositor.GetEasingFunction(0.4f, 0, 1, 1),
                Easing.SineEaseOut => compositor.GetEasingFunction(0, 0, 0.6f, 1),
                Easing.SineEaseInOut => compositor.GetEasingFunction(0.4f, 0, 0.6f, 1),
                Easing.QuadraticEaseIn => compositor.GetEasingFunction(0.8f, 0, 1, 1),
                Easing.QuadraticEaseOut => compositor.GetEasingFunction(0, 0, 0.2f, 1),
                Easing.QuadraticEaseInOut => compositor.GetEasingFunction(0.8f, 0, 0.2f, 1),
                Easing.CircleEaseIn => compositor.GetEasingFunction(1, 0, 1, 0.8f),
                Easing.CircleEaseOut => compositor.GetEasingFunction(0, 0.3f, 0, 1),
                Easing.CircleEaseInOut => compositor.GetEasingFunction(0.9f, 0, 0.1f, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(ease), ease, $"Invalid easing value: {ease}")
            };
        }

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
            this CompositionObject target,
            string propertyPath,
            float? from,
            float to,
            TimeSpan duration,
            TimeSpan? delay,
            CompositionEasingFunction? ease = null)
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
            this CompositionObject target,
            string propertyPath,
            Vector2? from,
            Vector2 to,
            TimeSpan duration,
            TimeSpan? delay,
            CompositionEasingFunction? ease = null)
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
            this CompositionObject target,
            string propertyPath,
            Vector3? from,
            Vector3 to,
            TimeSpan duration,
            TimeSpan? delay,
            CompositionEasingFunction? ease = null)
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
        /// <returns>A <see cref="Task"/> that completes when the specified animation completes</returns>
        public static Task StartAnimationAsync(
            this CompositionObject target,
            string property,
            float value,
            TimeSpan duration)
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
