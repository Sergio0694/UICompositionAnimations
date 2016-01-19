using System;
using System.Numerics;
using Windows.UI.Xaml;

namespace Windows.UI.Composition
{
    /// <summary>
    /// Create composition animations using this class
    /// </summary>
    public static class CompositionManager
    {
        // UIElement extensions
        public static void BeginScalarAnimation(UIElement element, string propertyPath, float to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            element.GetVisual().BeginScalarAnimation(propertyPath, to, from, duration, delay, ease);
        }
        public static void BeginVector2Animation(UIElement element, string propertyPath, Vector2 to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            element.GetVisual().BeginVector2Animation(propertyPath, to, from, duration, delay, ease);
        }
        public static void BeginVector3Animation(UIElement element, string propertyPath, Vector3 to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            element.GetVisual().BeginVector3Animation(propertyPath, to, from, duration, delay, ease);
        }

        // CompositionObject extensions
        public static void BeginScalarAnimation(this CompositionObject compObj, string propertyPath, float to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            compObj.StartAnimation(propertyPath,
                compObj.Compositor.CreateScalarKeyFrameAnimation(to, from, duration, delay, ease));
        }
        public static void BeginVector2Animation(this CompositionObject compObj, string propertyPath, Vector2 to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            compObj.StartAnimation(propertyPath,
                compObj.Compositor.CreateVector2KeyFrameAnimation(to, from, duration, delay, ease));
        }
        public static void BeginVector3Animation(this CompositionObject compObj, string propertyPath, Vector3 to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            compObj.StartAnimation(propertyPath,
                compObj.Compositor.CreateVector3KeyFrameAnimation(to, from, duration, delay, ease));
        }

        // CompositorExtensions
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(this Compositor compositor, float to, float? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            var ani = compositor.CreateScalarKeyFrameAnimation();
            // Set duration and delay time
            ani.Duration = duration;
            if (delay.HasValue)
                ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue)
            {
                ani.InsertKeyFrame(0, from.Value);
            }
            return ani;
        }
        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(this Compositor compositor, Vector2 to, Vector2? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            var ani = compositor.CreateVector2KeyFrameAnimation();

            // Set duration and delay time
            ani.Duration = duration;
            if (delay.HasValue)
                ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue)
            {
                ani.InsertKeyFrame(0, from.Value);
            }
            return ani;
        }
        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(this Compositor compositor, Vector3 to, Vector3? from, TimeSpan duration, TimeSpan? delay, CompositionEasingFunction ease)
        {
            var ani = compositor.CreateVector3KeyFrameAnimation();

            // Set duration and delay time
            ani.Duration = duration;
            if (delay.HasValue)
                ani.DelayTime = delay.Value;

            // Insert "to" and "from" keyframes
            ani.InsertKeyFrame(1, to, ease ?? compositor.CreateLinearEasingFunction());
            if (from.HasValue)
            {
                ani.InsertKeyFrame(0, from.Value);
            }
            return ani;
        }
    }
}
