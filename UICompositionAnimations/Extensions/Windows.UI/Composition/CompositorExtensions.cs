using System;
using System.Numerics;
using JetBrains.Annotations;

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Compositor"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class CompositorExtensions
    {
        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [Pure, NotNull]
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(
            [NotNull] this Compositor compositor,
            float? from, float to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            // Set duration and delay time
            ScalarKeyFrameAnimation ani = compositor.CreateScalarKeyFrameAnimation();
            ani.Duration = duration;
            if (delay.HasValue) ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue) ani.InsertKeyFrame(0, from.Value);
            return ani;
        }

        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element, using an expression animation
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">A string that indicates the final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [Pure, NotNull]
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(
            [NotNull] this Compositor compositor,
            float? from, [NotNull] string to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            // Set duration and delay time
            ScalarKeyFrameAnimation ani = compositor.CreateScalarKeyFrameAnimation();
            ani.Duration = duration;
            if (delay.HasValue) ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertExpressionKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue) ani.InsertKeyFrame(0, from.Value);
            return ani;
        }

        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [Pure, NotNull]
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(
            [NotNull] this Compositor compositor,
            Vector2? from, Vector2 to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            // Set duration and delay time
            Vector2KeyFrameAnimation ani = compositor.CreateVector2KeyFrameAnimation();
            ani.Duration = duration;
            if (delay.HasValue) ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue) ani.InsertKeyFrame(0, from.Value);
            return ani;
        }

        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element, using an expression animation
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">A string that indicates the final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [Pure, NotNull]
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(
            [NotNull] this Compositor compositor,
            Vector2? from, [NotNull] string to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            // Set duration and delay time
            Vector2KeyFrameAnimation ani = compositor.CreateVector2KeyFrameAnimation();
            ani.Duration = duration;
            if (delay.HasValue) ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertExpressionKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue) ani.InsertKeyFrame(0, from.Value);
            return ani;
        }

        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [Pure, NotNull]
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(
            [NotNull] this Compositor compositor,
            Vector3? from, Vector3 to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            // Set duration and delay time
            Vector3KeyFrameAnimation ani = compositor.CreateVector3KeyFrameAnimation();
            ani.Duration = duration;
            if (delay.HasValue) ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue) ani.InsertKeyFrame(0, from.Value);
            return ani;
        }

        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element, using an expression animation
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">A string that indicates the final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [Pure, NotNull]
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(
            [NotNull] this Compositor compositor,
            Vector3? from, [NotNull] string to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            // Set duration and delay time
            Vector3KeyFrameAnimation ani = compositor.CreateVector3KeyFrameAnimation();
            ani.Duration = duration;
            if (delay.HasValue) ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertExpressionKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue) ani.InsertKeyFrame(0, from.Value);
            return ani;
        }
    }
}
