using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;

namespace UICompositionAnimations.Composition
{
    /// <summary>
    /// Create composition animations using this class
    /// </summary>
    public static class CompositionManager
    {
        #region Animations initialization

        /// <summary>
        /// Creates and starts a scalar animation on the target element
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        public static void BeginScalarAnimation([NotNull] UIElement element, [NotNull] String propertyPath, 
            float? from, float to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            element.GetVisual().BeginScalarAnimation(propertyPath, from, to, duration, delay, ease);
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector2"/> animation on the target element
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        public static void BeginVector2Animation([NotNull] UIElement element, [NotNull] String propertyPath, 
            Vector2? from, Vector2 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            element.GetVisual().BeginVector2Animation(propertyPath, from, to, duration, delay, ease);
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector3"/> animation on the target element
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginVector3Animation([NotNull] UIElement element, [NotNull] String propertyPath, 
            Vector3? from, Vector3 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            element.GetVisual().BeginVector3Animation(propertyPath, from, to, duration, delay, ease);
        }

        /// <summary>
        /// Creates and starts a scalar animation on the current <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="compObj">The target to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        public static void BeginScalarAnimation([NotNull] this CompositionObject compObj, [NotNull] String propertyPath, 
            float? from, float to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            compObj.StartAnimation(propertyPath, compObj.Compositor.CreateScalarKeyFrameAnimation(from, to, duration, delay, ease));
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector2"/> animation on the current <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="compObj">The target to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        public static void BeginVector2Animation([NotNull]this CompositionObject compObj, [NotNull] String propertyPath, 
            Vector2? from, Vector2 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            compObj.StartAnimation(propertyPath, compObj.Compositor.CreateVector2KeyFrameAnimation(from, to, duration, delay, ease));
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector3"/> animation on the current <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="compObj">The target to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        public static void BeginVector3Animation([NotNull] this CompositionObject compObj, [NotNull] String propertyPath, 
            Vector3? from, Vector3 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            compObj.StartAnimation(propertyPath, compObj.Compositor.CreateVector3KeyFrameAnimation(from, to, duration, delay, ease));
        }

        #endregion

        #region KeyFrame animations

        /// <summary>
        /// Creates a <see cref="ScalarKeyFrameAnimation"/> instance with the given parameters to on a target element
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        [Pure, NotNull]
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation([NotNull] this Compositor compositor, 
            float? from, float to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
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
        [PublicAPI]
        [Pure, NotNull]
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation([NotNull] this Compositor compositor, 
            float? from, [NotNull] String to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
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
        [PublicAPI]
        [Pure, NotNull]
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation([NotNull] this Compositor compositor, 
            Vector2? from, Vector2 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
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
        [PublicAPI]
        [Pure, NotNull]
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation([NotNull] this Compositor compositor, 
            Vector2? from, [NotNull] String to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
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
        [PublicAPI]
        [Pure, NotNull]
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation([NotNull] this Compositor compositor, 
            Vector3? from, Vector3 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
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
        [PublicAPI]
        [Pure, NotNull]
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation([NotNull] this Compositor compositor, 
            Vector3? from, [NotNull] String to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
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

        /// <summary>
        /// Creates a <see cref="CompositionAnimation"/> instance with the given parameters to on a target element, using an expression animation
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> instance used to create the animation</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        [PublicAPI]
        [Pure, NotNull]
        public static CompositionAnimation CreateMatrix4x4KeyFrameAnimation([NotNull] this Compositor compositor,
            Matrix4x4? from, Matrix4x4 to, TimeSpan duration, TimeSpan? delay, [CanBeNull] CompositionEasingFunction ease = null)
        {
            throw new NotImplementedException(); // TODO
        }

        #endregion
    }
}
