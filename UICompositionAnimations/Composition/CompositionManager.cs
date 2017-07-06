using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace UICompositionAnimations.Composition
{
    /// <summary>
    /// Create composition animations using this class
    /// </summary>
    internal static class CompositionManager
    {
        // UIElement scalar animation
        public static void BeginScalarAnimation(UIElement element, String propertyPath, float to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            element.GetVisual().BeginScalarAnimation(propertyPath, to, from, duration, delay, ease);
        }

        // UIElement Vector2 animation
        public static void BeginVector2Animation(UIElement element, String propertyPath, Vector2 to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            element.GetVisual().BeginVector2Animation(propertyPath, to, from, duration, delay, ease);
        }

        // UIElement Vector3 animation
        public static void BeginVector3Animation(UIElement element, String propertyPath, Vector3 to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            element.GetVisual().BeginVector3Animation(propertyPath, to, from, duration, delay, ease);
        }

        // CompositionObject scalar animation
        public static void BeginScalarAnimation(this CompositionObject compObj, String propertyPath, float to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            compObj.StartAnimation(propertyPath, compObj.Compositor.CreateScalarKeyFrameAnimation(to, from, duration, delay, ease));
        }

        // CompositionObject Vector2 animation
        public static void BeginVector2Animation(this CompositionObject compObj, String propertyPath, Vector2 to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            compObj.StartAnimation(propertyPath, compObj.Compositor.CreateVector2KeyFrameAnimation(to, from, duration, delay, ease));
        }

        // CompositionObject Vector3 animation
        public static void BeginVector3Animation(this CompositionObject compObj, String propertyPath, Vector3 to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            compObj.StartAnimation(propertyPath, compObj.Compositor.CreateVector3KeyFrameAnimation(to, from, duration, delay, ease));
        }

        // Create scalar animation from compositor
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(this Compositor compositor, float to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
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

        // Create expression scalar animation from compositor
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(this Compositor compositor, String to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
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

        // Create Vector2 animation from compositor
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(this Compositor compositor, Vector2 to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
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

        // Create Vector2 expression animation from compositor
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(this Compositor compositor, String to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
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

        // Create Vector3 animation from compositor
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(this Compositor compositor, Vector3 to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
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

        // Create Vector3 expression animation from compositor
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(this Compositor compositor, String to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
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
