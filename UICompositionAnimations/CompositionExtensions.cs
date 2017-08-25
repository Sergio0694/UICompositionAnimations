using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using JetBrains.Annotations;
using UICompositionAnimations.Composition;
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations
{
    /// <summary>
    /// A static class that wraps the animation methods in the Windows.UI.Composition namespace
    /// </summary>
    public static class CompositionExtensions
    {
        #region Internal tools

        /// <summary>
        /// Sets the <see cref="Visual.CenterPoint"/> property of a <see cref="Visual"/> object to the center of a given <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The source element</param>
        /// <param name="visual">The Visual object for the source <see cref="FrameworkElement"/></param>
        private static void SetFixedCenterPoint([NotNull] this FrameworkElement element, [NotNull] Visual visual)
        {
            if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
                throw new InvalidOperationException("The target element must have a fixed size");
            visual.CenterPoint = new Vector3((float)element.Width / 2, (float)element.Height / 2, 0);
        }

        /// <summary>
        /// Sets the <see cref="Visual.CenterPoint"/> property of a <see cref="Visual"/> object to the center of a given <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The source element</param>
        /// <param name="visual">The Visual object for the source <see cref="FrameworkElement"/></param>
        private static async Task SetCenterPointAsync([NotNull] this FrameworkElement element, [NotNull] Visual visual)
        {
            // Check if the control hasn't already been loaded
            bool CheckLoadingPending() => element.ActualWidth + element.ActualHeight < 0.1;
            if (CheckLoadingPending())
            {
                // Wait for the loaded event and set the CenterPoint
                TaskCompletionSource<object> loadedTcs = new TaskCompletionSource<object>();
                void Handler(object s, RoutedEventArgs e)
                {
                    loadedTcs.SetResult(null);
                    element.Loaded -= Handler;
                }

                // Wait for the loaded event for a given time threshold
                element.Loaded += Handler;
                await Task.WhenAny(loadedTcs.Task, Task.Delay(500));
                element.Loaded -= Handler;

                // If the control still hasn't been loaded, approximate the center point with its desired size
                if (CheckLoadingPending())
                {
                    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    visual.CenterPoint = new Vector3((float)element.DesiredSize.Width / 2, (float)element.DesiredSize.Height / 2, 0);
                    return;
                }
            }

            // Update the center point
            visual.CenterPoint = new Vector3((float)element.ActualWidth / 2, (float)element.ActualHeight / 2, 0);
        }

        #endregion

        #region Fade

        // Manages the fade animation
        private static Task ManageCompositionFadeAnimationAsync([NotNull] Visual visual,
            float? startOp, float endOp,
            int ms, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the default values
            visual.StopAnimation("Opacity");
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Get the opacity animation
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, duration, delay, easingFunction);

            // Close the batch and manage its event
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            batch.End();
            return tcs.Task;
        }

        /// <summary>
        /// Starts a fade animation on the target <see cref="UIElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        public static async void StartCompositionFadeAnimation([NotNull] this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await StartCompositionFadeAnimationAsync(element, startOp, endOp, ms, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade animation on the target <see cref="UIElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        public static async void StartCompositionFadeAnimation([NotNull] this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, float x1, float y1, float x2, float y2, Action callback = null)
        {
            await StartCompositionFadeAnimationAsync(element, startOp, endOp, ms, msDelay, x1,y1, x2, y2);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade animation on the target <see cref="UIElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeAnimationAsync([NotNull] this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            return ManageCompositionFadeAnimationAsync(visual, startOp, endOp, ms, msDelay, ease);
        }

        /// <summary>
        /// Starts a fade animation on the target <see cref="UIElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        public static Task StartCompositionFadeAnimationAsync([NotNull] this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, float x1, float y1, float x2, float y2)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            return ManageCompositionFadeAnimationAsync(visual, startOp, endOp, ms, msDelay, ease);
        }

        // Sets an implicit fade animation on the target element
        private static void SetCompositionFadeImplicitAnimation([NotNull] UIElement element, [NotNull] Compositor compositor, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the easing function, the duration and delay
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Get the opacity animation
            CompositionAnimationGroup group = compositor.CreateAnimationGroup();
            ScalarKeyFrameAnimation opacityAnimation = compositor.CreateScalarKeyFrameAnimation(start, end, duration, delay, easingFunction);
            opacityAnimation.Target = "Opacity";
            group.Add(opacityAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        /// <summary>
        /// Sets an implicit fade animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="start">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="end">The final opacity value</param>
        /// <param name="ms">The duration of the fade animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static void SetCompositionFadeImplicitAnimation([NotNull] this UIElement element, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            SetCompositionFadeImplicitAnimation(element, visual.Compositor, type, start, end, ms, msDelay, ease);
        }

        /// <summary>
        /// Sets an implicit fade animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="start">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="end">The final opacity value</param>
        /// <param name="ms">The duration of the fade animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        public static void SetCompositionFadeImplicitAnimation([NotNull] this UIElement element, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, float x1, float y1, float x2, float y2)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            SetCompositionFadeImplicitAnimation(element, visual.Compositor, type, start, end, ms, msDelay, ease);

        }

        #endregion

        #region Fade and slide

        // Manages the fade and slide animation
        private static Task ManageCompositionFadeSlideAnimationAsync([NotNull] UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Offset");
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            TimeSpan durationOp = TimeSpan.FromMilliseconds(msOp);
            TimeSpan durationSlide = TimeSpan.FromMilliseconds(msSlide ?? msOp);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final offset values
            Vector3 initialOffset = visual.Offset;
            Vector3 endOffset = visual.Offset;
            if (axis == TranslationAxis.X)
            {
                if (startXY.HasValue) initialOffset.X = startXY.Value;
                endOffset.X = endXY;
            }
            else
            {
                if (startXY.HasValue) initialOffset.Y = startXY.Value;
                endOffset.Y = endXY;
            }

            // Get the batch
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            // Get the opacity the animation
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, durationOp, delay, easingFunction);

            // Offset animation
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialOffset, endOffset, durationSlide, delay, easingFunction);

            // Close the batch and manage its event
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            visual.StartAnimation("Offset", offsetAnimation);
            batch.End();
            return tcs.Task;
        }

        /// <summary>
        /// Starts a fade and slide animation on the target <see cref="UIElement"/> and optionally runs a callback <see cref="Action"/> when the animations finish
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        public static async void StartCompositionFadeSlideAnimation([NotNull] this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await StartCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target <see cref="UIElement"/> and optionally runs a callback <see cref="Action"/> when the animations finish
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        public static async void StartCompositionFadeSlideAnimation([NotNull] this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, float x1, float y1, float x2, float y2, Action callback = null)
        {
            await StartCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, x1, y1, x2, y2);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target <see cref="UIElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeSlideAnimationAsync([NotNull] this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            return ManageCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, ease);
        }

        /// <summary>
        /// Starts a fade and slide animation on the target <see cref="UIElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        public static Task StartCompositionFadeSlideAnimationAsync([NotNull] this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, float x1, float y1, float x2, float y2)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            return ManageCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, ease);
        }

        // Sets an implicit fade and slide animation on the target element
        private static void SetCompositionFadeSlideImplicitAnimation([NotNull] UIElement element, [NotNull] Visual visual, ImplicitAnimationType type,
            float startOp, float endOp,
            TranslationAxis axis, float startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the easing function, the duration and delay
            TimeSpan durationOp = TimeSpan.FromMilliseconds(msOp);
            TimeSpan durationSlide = TimeSpan.FromMilliseconds(msSlide ?? msOp);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final offset values
            Vector3 initialOffset = visual.Offset;
            Vector3 endOffset = visual.Offset;
            if (axis == TranslationAxis.X)
            {
                initialOffset.X = startXY;
                endOffset.X = endXY;
            }
            else
            {
                initialOffset.Y = startXY;
                endOffset.Y = endXY;
            }

            // Create and return the animations
            CompositionAnimationGroup group = visual.Compositor.CreateAnimationGroup();
            ScalarKeyFrameAnimation fade = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, durationOp, delay, easingFunction);
            fade.Target = "Opacity";
            group.Add(fade);
            Vector3KeyFrameAnimation slide = visual.Compositor.CreateVector3KeyFrameAnimation(initialOffset, endOffset, durationSlide, delay, easingFunction);
            slide.Target = "Offset";
            group.Add(slide);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        /// <summary>
        /// Sets an implicit fade and slide animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static void SetCompositionFadeSlideImplicitAnimation([NotNull] this UIElement element, ImplicitAnimationType type,
            float startOp, float endOp,
            TranslationAxis axis, float startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            SetCompositionFadeSlideImplicitAnimation(element, visual, type, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, ease);
        }

        /// <summary>
        /// Sets an implicit fade and slide animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        public static void SetCompositionFadeSlideImplicitAnimation([NotNull] this UIElement element, ImplicitAnimationType type,
            float startOp, float endOp,
            TranslationAxis axis, float startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, float x1, float y1, float x2, float y2)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            SetCompositionFadeSlideImplicitAnimation(element, visual, type, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, ease);
        }

        #endregion

        #region Fade and scale

        // Manages the fade and scale animation
        private static async Task ManageCompositionFadeScaleAnimationAsync([NotNull] FrameworkElement element, Visual visual,
            float? startOp, float endOp,
            float? startXY, float endXY,
            int msOp, int? msScale, int? msDelay, [NotNull] CompositionEasingFunction easingFunction, bool useFixedScale)
        {
            // Get the default values and set the CenterPoint
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Scale");
            if (useFixedScale) element.SetFixedCenterPoint(visual);
            else await element.SetCenterPointAsync(visual);
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            TimeSpan durationOp = TimeSpan.FromMilliseconds(msOp);
            TimeSpan durationScale = TimeSpan.FromMilliseconds(msScale ?? msOp);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final scale values
            Vector3 initialScale = visual.Scale;
            if (startXY.HasValue)
            {
                initialScale.X = startXY.Value;
                initialScale.Y = startXY.Value;
            }
            Vector3 endScale = new Vector3
            {
                X = endXY,
                Y = endXY,
                Z = visual.Scale.Z
            };

            // Get the opacity the animation
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, durationOp, delay, easingFunction);

            // Scale animation
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialScale, endScale, durationScale, delay, easingFunction);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            visual.StartAnimation("Scale", scaleAnimation);
            batch.End();
            await tcs.Task;
        }

        /// <summary>
        /// Starts a fade and scale animation on the target <see cref="FrameworkElement"/> and optionally runs a callback <see cref="Action"/> when the animations finish
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async void StartCompositionFadeScaleAnimation([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction, Action callback = null, bool useFixedSize = false)
        {
            await StartCompositionFadeScaleAnimationAsync(element, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, easingFunction, useFixedSize);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and scale animation on the target <see cref="FrameworkElement"/> and optionally runs a callback <see cref="Action"/> when the animations finish
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async void StartCompositionFadeScaleAnimation([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int msOp, int? msScale, int? msDelay, float x1, float y1, float x2, float y2, Action callback = null, bool useFixedSize = false)
        {
            await StartCompositionFadeScaleAnimationAsync(element, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, x1, y1, x2, y2, useFixedSize);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and scale animation on the target <see cref="FrameworkElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static Task StartCompositionFadeScaleAnimationAsync([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction, bool useFixedSize = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            return ManageCompositionFadeScaleAnimationAsync(element, visual, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, ease, useFixedSize);
        }

        /// <summary>
        /// Starts a fade and scale animation on the target <see cref="FrameworkElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static Task StartCompositionFadeScaleAnimationAsync([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int msOp, int? msScale, int? msDelay, float x1, float y1, float x2, float y2, bool useFixedSize = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            return ManageCompositionFadeScaleAnimationAsync(element, visual, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, ease, useFixedSize);
        }

        // Sets an implicit fade and scale animation on the target element
        private static async Task SetCompositionFadeScaleImplicitAnimationAsync([NotNull] FrameworkElement element, [NotNull] Visual visual, ImplicitAnimationType type,
            float startOp, float endOp,
            float startScale, float endScale,
            int msOp, int? msScale, int? msDelay, [NotNull] CompositionEasingFunction easingFunction, bool useFixedSize)
        {
            // Get the default values and set the CenterPoint
            if (useFixedSize) element.SetFixedCenterPoint(visual);
            else await element.SetCenterPointAsync(visual);

            // Get the easing function, the duration and delay
            TimeSpan durationOp = TimeSpan.FromMilliseconds(msOp);
            TimeSpan durationScale = TimeSpan.FromMilliseconds(msScale ?? msOp);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final scale values
            Vector3 initialScale = new Vector3(startScale, startScale, visual.Scale.Z);
            Vector3 endScale3 = new Vector3(endScale, endScale, visual.Scale.Z);

            // Get the animations
            CompositionAnimationGroup group = visual.Compositor.CreateAnimationGroup();
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, durationOp, delay, easingFunction);
            opacityAnimation.Target = "Opacity";
            group.Add(opacityAnimation);
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialScale, endScale3, durationScale, delay, easingFunction);
            scaleAnimation.Target = "Scale";
            group.Add(scaleAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        /// <summary>
        /// Sets an implicit fade and scale animation on the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static Task SetCompositionFadeScaleImplicitAnimationAsync([NotNull] this FrameworkElement element, ImplicitAnimationType type,
            float startOp, float endOp,
            float startScale, float endScale,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction, bool useFixedSize = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            return SetCompositionFadeScaleImplicitAnimationAsync(element, visual, type, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, ease, useFixedSize);
        }

        /// <summary>
        /// Sets an implicit fade and scale animation on the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static Task SetCompositionFadeScaleImplicitAnimationAsync([NotNull] this FrameworkElement element, ImplicitAnimationType type,
            float startOp, float endOp,
            float startScale, float endScale,
            int msOp, int? msScale, int? msDelay, float x1, float y1, float x2, float y2, bool useFixedSize = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            return SetCompositionFadeScaleImplicitAnimationAsync(element, visual, type, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, ease, useFixedSize);
        }

        #endregion

        #region Scale only

        // Manages the scale animation
        private static async Task<float> ManageCompositionScaleAnimationAsync([NotNull] FrameworkElement element, [NotNull] Visual visual,
            float? startXY, float endXY,
            int ms, int? msDelay, [NotNull] CompositionEasingFunction easingFunction, bool useFixedSize)
        {
            // Get the default values and set the CenterPoint
            visual.StopAnimation("Scale");
            if (useFixedSize) element.SetFixedCenterPoint(visual);
            else await element.SetCenterPointAsync(visual);

            // Get the easing function, the duration and delay
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final scale values
            Vector3 initialScale = visual.Scale;
            if (startXY.HasValue)
            {
                initialScale.X = startXY.Value;
                initialScale.Y = startXY.Value;
            }
            Vector3 endScale = new Vector3
            {
                X = endXY,
                Y = endXY,
                Z = visual.Scale.Z
            };

            // Scale animation
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialScale, endScale, duration, delay, easingFunction);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Scale", scaleAnimation);
            batch.End();
            await tcs.Task;
            return initialScale.X;
        }

        /// <summary>
        /// Starts a scale animation on the target <see cref="FrameworkElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async void StartCompositionScaleAnimation([NotNull] this FrameworkElement element,
            float? startScale, float endScale,
            int ms, int? msDelay, EasingFunctionNames easingFunction, 
            bool reverse = false, Action callback = null, bool useFixedSize = false)
        {
            await element.StartCompositionScaleAnimationAsync(startScale, endScale, ms, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a scale animation on the target <see cref="FrameworkElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async void StartCompositionScaleAnimation([NotNull] this FrameworkElement element,
            float? startScale, float endScale,
            int ms, int? msDelay, float x1, float y1, float x2, float y2, bool reverse = false, Action callback = null, bool useFixedSize = false)
        {
            await element.StartCompositionScaleAnimationAsync(startScale, endScale, ms, msDelay, x1, y1, x2, y2, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a scale animation on the target <see cref="FrameworkElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async Task StartCompositionScaleAnimationAsync([NotNull] this FrameworkElement element,
            float? startScale, float endScale,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, bool useFixedSize = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            startScale = await ManageCompositionScaleAnimationAsync(element, visual, startScale, endScale, ms, msDelay, ease, useFixedSize);
            if (reverse) await ManageCompositionScaleAnimationAsync(element, visual, endScale, startScale.Value, ms, null, ease, useFixedSize);
        }

        /// <summary>
        /// Starts a scale animation on the target <see cref="FrameworkElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async Task StartCompositionScaleAnimationAsync([NotNull] this FrameworkElement element,
            float? startScale, float endScale,
            int ms, int? msDelay, float x1, float y1, float x2, float y2, bool reverse = false, bool useFixedSize = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            startScale = await ManageCompositionScaleAnimationAsync(element, visual, startScale, endScale, ms, msDelay, ease, useFixedSize);
            if (reverse) await ManageCompositionScaleAnimationAsync(element, visual, endScale, startScale.Value, ms, null, ease, useFixedSize);
        }

        // Sets an implicit scale animation on the target element
        private static async Task SetCompositionScaleImplicitAnimationAsync([NotNull] FrameworkElement element, [NotNull] Visual visual, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the default values and set the CenterPoint
            await element.SetCenterPointAsync(visual);

            // Get the easing function, the duration and delay
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final scale values
            Vector3 initialScale = new Vector3(start, start, visual.Scale.Z);
            Vector3 endScale = new Vector3(end, end, visual.Scale.Z);

            // Get the animations
            CompositionAnimationGroup group = visual.Compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialScale, endScale, duration, delay, easingFunction);
            scaleAnimation.Target = "Scale";
            group.Add(scaleAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        /// <summary>
        /// Sets an implicit scale animation on the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="start">The initial scale value</param>
        /// <param name="end">The final value</param>
        /// <param name="ms">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task SetCompositionScaleImplicitAnimationAsync([NotNull] this FrameworkElement element, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            return SetCompositionScaleImplicitAnimationAsync(element, visual, type, start, end, ms, msDelay, ease);
        }

        /// <summary>
        /// Sets an implicit scale animation on the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="start">The initial scale value</param>
        /// <param name="end">The final value</param>
        /// <param name="ms">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        public static Task SetCompositionScaleImplicitAnimationAsync([NotNull] this FrameworkElement element, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, float x1, float y1, float x2, float y2)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            return SetCompositionScaleImplicitAnimationAsync(element, visual, type, start, end, ms, msDelay, ease);
        }

        #endregion

        #region Slide only

        // Manages the scale animation
        private static async Task<float> ManageCompositionSlideAnimationAsync([NotNull] Visual visual,
            TranslationAxis axis, float? startXY, float endXY,
            int ms, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the default values
            visual.StopAnimation("Offset");

            // Get the easing function, the duration and delay
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final offset values
            Vector3 initialOffset = visual.Offset;
            Vector3 endOffset = visual.Offset;
            if (axis == TranslationAxis.X)
            {
                if (startXY.HasValue) initialOffset.X = startXY.Value;
                endOffset.X = endXY;
            }
            else
            {
                if (startXY.HasValue) initialOffset.Y = startXY.Value;
                endOffset.Y = endXY;
            }

            // Scale animation
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialOffset, endOffset, duration, delay, easingFunction);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Offset", offsetAnimation);
            batch.End();
            await tcs.Task;
            return initialOffset.X;
        }

        /// <summary>
        /// Starts an offset animation on the target <see cref="UIElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startOffset">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endOffset">The final offset X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        public static async void StartCompositionSlideAnimation([NotNull] this UIElement element,
            TranslationAxis axis, float? startOffset, float endOffset,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, Action callback = null)
        {
            await element.StartCompositionSlideAnimationAsync(axis, startOffset, endOffset, ms, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts an offset animation on the target <see cref="UIElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startOffset">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endOffset">The final offset X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        public static async void StartCompositionSlideAnimation([NotNull] this UIElement element,
            TranslationAxis axis, float? startOffset, float endOffset,
            int ms, int? msDelay, float x1, float y1, float x2, float y2, bool reverse = false, Action callback = null)
        {
            await element.StartCompositionSlideAnimationAsync(axis, startOffset, endOffset, ms, msDelay, x1, y1, x2, y2, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts an offset animation on the target <see cref="UIElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIEl<see cref="UIElement"/>ement to animate</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startOffset">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endOffset">The final offset X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async Task StartCompositionSlideAnimationAsync([NotNull] this UIElement element,
            TranslationAxis axis, float? startOffset, float endOffset,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            startOffset = await ManageCompositionSlideAnimationAsync(visual, axis, startOffset, endOffset, ms, msDelay, ease);
            if (reverse) await ManageCompositionSlideAnimationAsync(visual, axis, endOffset, startOffset.Value, ms, msDelay, ease);
        }

        /// <summary>
        /// Starts an offset animation on the target <see cref="UIElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startOffset">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endOffset">The final offset X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async Task StartCompositionSlideAnimationAsync([NotNull] this UIElement element,
            TranslationAxis axis, float? startOffset, float endOffset,
            int ms, int? msDelay, float x1, float y1, float x2, float y2, bool reverse = false)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            startOffset = await ManageCompositionSlideAnimationAsync(visual, axis, startOffset, endOffset, ms, msDelay, ease);
            if (reverse) await ManageCompositionSlideAnimationAsync(visual, axis, endOffset, startOffset.Value, ms, msDelay, ease);
        }

        // Sets an implicit slide animation on the target element
        private static void SetCompositionSlideImplicitAnimation([NotNull] UIElement element, [NotNull] Visual visual, ImplicitAnimationType type,
            TranslationAxis axis, float start, float end,
            int ms, int? msDelay, [NotNull] CompositionEasingFunction easingFunction)
        {
            // Get the easing function, the duration and delay
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final offset values
            Vector3 initialOffset = visual.Offset;
            Vector3 endOffset = visual.Offset;
            if (axis == TranslationAxis.X)
            {
                initialOffset.X = start;
                endOffset.X = end;
            }
            else
            {
                initialOffset.Y = start;
                endOffset.Y = end;
            }

            // Get the animations
            CompositionAnimationGroup group = visual.Compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialOffset, endOffset, duration, delay, easingFunction);
            offsetAnimation.Target = "Offset";
            group.Add(offsetAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        /// <summary>
        /// Sets an implicit slide animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="start">The initial scale value</param>
        /// <param name="end">The final value</param>
        /// <param name="ms">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static void SetCompositionSlideImplicitAnimation([NotNull] this UIElement element, ImplicitAnimationType type,
            TranslationAxis axis, float start, float end,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            SetCompositionSlideImplicitAnimation(element, visual, type, axis, start, end, ms, msDelay, ease);
        }

        /// <summary>
        /// Sets an implicit slide animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="start">The initial scale value</param>
        /// <param name="end">The final value</param>
        /// <param name="ms">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="x1">The X coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="y1">The Y coordinate of the first control point of the cubic beizer easing function</param>
        /// <param name="x2">The X coordinate of the second control point of the cubic beizer easing function</param>
        /// <param name="y2">The Y coordinate of the second control point of the cubic beizer easing function</param>
        public static void SetCompositionSlideImplicitAnimation([NotNull] this UIElement element, ImplicitAnimationType type,
            TranslationAxis axis, float start, float end,
            int ms, int? msDelay, float x1, float y1, float x2, float y2)
        {
            Visual visual = element.GetVisual();
            CompositionEasingFunction ease = visual.GetEasingFunction(x1, y1, x2, y2);
            SetCompositionSlideImplicitAnimation(element, visual, type, axis, start, end, ms, msDelay, ease);
        }

        #endregion

        #region Roll

        // Manages the roll animation
        private static async Task<(float, float, float)> ManageCompositionRollAnimationAsync([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool useFixedSize)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Offset");
            visual.StopAnimation("RotationAngle");
            if (useFixedSize) element.SetFixedCenterPoint(visual);
            else await element.SetCenterPointAsync(visual);

            // Get the current opacity
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final offset values
            Vector3 initialOffset = visual.Offset;
            Vector3 endOffset = visual.Offset;
            if (axis == TranslationAxis.X)
            {
                if (startXY.HasValue) initialOffset.X = startXY.Value;
                endOffset.X = endXY;
            }
            else
            {
                if (startXY.HasValue) initialOffset.Y = startXY.Value;
                endOffset.Y = endXY;
            }

            // Calculate the initial and final rotation angle
            if (startDegrees == null) startDegrees = visual.RotationAngle.ToDegrees();

            // Get the opacity the animation
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, duration, delay, ease);

            // Scale animation
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialOffset, endOffset, duration, delay, ease);

            // Rotate animation
            ScalarKeyFrameAnimation rotateAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startDegrees.Value.ToRadians(), endDegrees.ToRadians(), duration, delay, ease);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            visual.StartAnimation("Offset", offsetAnimation);
            visual.StartAnimation("RotationAngle", rotateAnimation);
            batch.End();
            await tcs.Task;
            return (startOp.Value, initialOffset.X, startDegrees.Value);
        }

        /// <summary>
        /// Starts a roll animation on the target <see cref="FrameworkElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startXY">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset X and Y value</param>
        /// <param name="startDegrees">The initial angle in degrees</param>
        /// <param name="endDegrees">The target angle in degrees</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async void StartCompositionRollAnimation([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, Action callback = null, bool useFixedSize = false)
        {
            await element.ManageCompositionRollAnimationAsync(startOp, endOp, axis, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction, useFixedSize);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a roll animation on the target <see cref="FrameworkElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startXY">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset X and Y value</param>
        /// <param name="startDegrees">The initial angle in degrees</param>
        /// <param name="endDegrees">The target angle in degrees</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async Task StartCompositionRollAnimationAsync([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, bool useFixedSize = false)
        {
            (float opacity, float offset, float degrees) = await element.ManageCompositionRollAnimationAsync(startOp, endOp, axis, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction, useFixedSize);
            if (reverse) await element.ManageCompositionRollAnimationAsync(endOp, opacity, axis, endXY, offset, endDegrees, degrees, ms, msDelay, easingFunction, useFixedSize);
        }

        #endregion

        #region Rotation + fade + slide

        // Manages the composite animation
        private static async Task<(float, float, float)> ManageCompositionRotationFadeSlideAnimationAsync([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool useFixedSize)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Scale");
            visual.StopAnimation("RotationAngle");
            if (useFixedSize) element.SetFixedCenterPoint(visual);
            else await element.SetCenterPointAsync(visual);

            // Get the current opacity
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final scale values
            Vector3 initialScale = visual.Scale;
            if (startXY.HasValue)
            {
                initialScale.X = startXY.Value;
                initialScale.Y = startXY.Value;
            }
            Vector3 endScale = new Vector3
            {
                X = endXY,
                Y = endXY,
                Z = visual.Scale.Z
            };

            // Scale animation
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(initialScale, endScale, duration, delay, ease);

            // Calculate the initial and final rotation angle
            if (startDegrees == null) startDegrees = visual.RotationAngle.ToDegrees();

            // Get the opacity the animation
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startOp, endOp, duration, delay, ease);

            // Rotate animation
            ScalarKeyFrameAnimation rotateAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(startDegrees.Value.ToRadians(), endDegrees.ToRadians(), duration, delay, ease);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            visual.StartAnimation("Scale", scaleAnimation);
            visual.StartAnimation("RotationAngle", rotateAnimation);
            batch.End();
            await tcs.Task;
            return (startOp.Value, initialScale.X, startDegrees.Value);
        }

        /// <summary>
        /// Starts a rotation, fade and slide animation on the target <see cref="FrameworkElement"/> and optionally runs a callback <see cref="Action"/> when the animation finishes
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startXY">The initial scale value. If null, the current scale will be used</param>
        /// <param name="endXY">The final scale value</param>
        /// <param name="startDegrees">The initial angle in degrees</param>
        /// <param name="endDegrees">The target angle in degrees</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An <see cref="Action"/> to execute when the new animations end</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async void StartCompositionRotationFadeSlideAnimation([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, Action callback = null, bool useFixedSize = false)
        {
            await element.ManageCompositionRotationFadeSlideAnimationAsync(startOp, endOp, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction, useFixedSize);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a rotation, fade and slide animation on the target <see cref="FrameworkElement"/> and returns a <see cref="Task"/> that completes when the animation ends
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startXY">The initial scale value. If null, the current scale will be used</param>
        /// <param name="endXY">The final scale value</param>
        /// <param name="startDegrees">The initial angle in degrees</param>
        /// <param name="endDegrees">The target angle in degrees</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="useFixedSize">If true, the fixed <see cref="FrameworkElement.Height"/> and <see cref="FrameworkElement.Width"/> properties
        /// will be used, otherwise the center point will be calculated using the <see cref="FrameworkElement.ActualHeight"/> and width</param>
        public static async Task StartCompositionRotationFadeSlideAnimationAsync([NotNull] this FrameworkElement element,
            float? startOp, float endOp,
            float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, bool useFixedSize = false)
        {
            (float opacity, float degrees, float scale) = await element.ManageCompositionRotationFadeSlideAnimationAsync(startOp, endOp, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction, useFixedSize);
            if (reverse) await element.ManageCompositionRotationFadeSlideAnimationAsync(endOp, opacity, endXY, scale, endDegrees, degrees, ms, msDelay, easingFunction, useFixedSize);
        }

        #endregion

        #region Expression animations

        /// <summary>
        /// Creates and starts an animation on the target element that binds either the X or Y axis of the source <see cref="ScrollViewer"/>
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/> that will be animated</param>
        /// <param name="scroller">The source <see cref="ScrollViewer"/> control to use</param>
        /// <param name="sourceXY">The scrolling axis of the source <see cref="ScrollViewer"/></param>
        /// <param name="targetXY">The optional scrolling axis of the target element, if null the source axis will be used</param>
        /// <param name="invertSourceAxis">Indicates whether or not to invert the animation from the source <see cref="CompositionPropertySet"/></param>
        [NotNull]
        public static ExpressionAnimation StartExpressionAnimation(
            [NotNull] this UIElement element, [NotNull] ScrollViewer scroller,
            TranslationAxis sourceXY, TranslationAxis? targetXY = null, bool invertSourceAxis = false)
        {
            CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);
            String sign = invertSourceAxis ? "-" : String.Empty;
            ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"{sign}scroll.Translation.{sourceXY}");
            animation.SetReferenceParameter("scroll", scrollSet);
            element.GetVisual().StartAnimation($"Offset.{targetXY ?? sourceXY}", animation);
            return animation;
        }

        /// <summary>
        /// Creates and starts an animation on the target element, with the addition of a scalar parameter in the resulting <see cref="ExpressionAnimation"/>
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/> that will be animated</param>
        /// <param name="scroller">The source <see cref="ScrollViewer"/> control to use</param>
        /// <param name="sourceXY">The scrolling axis of the source <see cref="ScrollViewer"/></param>
        /// <param name="parameter">An additional parameter that will be included in the expression animation</param>
        /// <param name="targetXY">The optional scrolling axis of the target element, if null the source axis will be used</param>
        /// <param name="invertSourceAxis">Indicates whether or not to invert the animation from the source <see cref="CompositionPropertySet"/></param>
        [NotNull]
        public static ExpressionAnimationWithScalarParameter StartExpressionAnimation(
            [NotNull] this UIElement element, [NotNull] ScrollViewer scroller,
            TranslationAxis sourceXY, float parameter,
            TranslationAxis? targetXY = null, bool invertSourceAxis = false)
        {
            // Get the property set and setup the scroller offset sign
            CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);
            String sign = invertSourceAxis ? "-" : "+";

            // Prepare the second property set to insert the additional parameter
            CompositionPropertySet properties = scroller.GetVisual().Compositor.CreatePropertySet();
            properties.InsertScalar(nameof(parameter), parameter);

            // Create and start the animation
            ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"{nameof(properties)}.{nameof(parameter)} {sign} scroll.Translation.{sourceXY}");
            animation.SetReferenceParameter("scroll", scrollSet);
            animation.SetReferenceParameter(nameof(properties), properties);
            element.GetVisual().StartAnimation($"Offset.{targetXY ?? sourceXY}", animation);
            return new ExpressionAnimationWithScalarParameter(animation, properties, nameof(parameter));
        }

        #endregion

        #region Shadows

        /// <summary>
        /// Creates a <see cref="DropShadow"/> object from the given <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The source element for the shadow</param>
        /// <param name="target">The optional target element to apply the shadow to (it can be the same as the source element)</param>
        /// <param name="apply">Indicates whether or not to immediately add the shadow to the visual tree</param>
        /// <param name="width">The optional width of the shadow (if null, the element width will be used)</param>
        /// <param name="height">The optional height of the shadow (if null, the element height will be used)</param>
        /// <param name="color">The shadow color (the default is <see cref="Colors.Black"/></param>
        /// <param name="opacity">The opacity of the shadow</param>
        /// <param name="offsetX">The optional horizontal offset of the shadow</param>
        /// <param name="offsetY">The optional vertical offset of the shadow</param>
        /// <param name="clipMargin">The optional margin of the clip area of the shadow</param>
        /// <param name="clipOffsetX">The optional horizontal offset of the clip area of the shadow</param>
        /// <param name="clipOffsetY">The optional vertical offset of the clip area of the shadow</param>
        /// <returns>The <see cref="SpriteVisual"/> object that hosts the shadow</returns>
        public static SpriteVisual AttachVisualShadow(
            [NotNull] this FrameworkElement element, [NotNull] UIElement target, bool apply,
            float? width, float? height,
            Color color, float opacity,
            float offsetX = 0, float offsetY = 0, 
            Thickness? clipMargin = null, float clipOffsetX = 0, float clipOffsetY = 0)
        {
            // Setup the shadow
            Visual elementVisual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = elementVisual.Compositor;
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            DropShadow shadow = compositor.CreateDropShadow();
            shadow.Color = color;
            shadow.Opacity = opacity;
            sprite.Shadow = shadow;
            sprite.Size = new Vector2(width ?? (float)element.Width, height ?? (float)element.Height);
            sprite.Offset = new Vector3(offsetX, offsetY, 0);

            // Clip it and add it to the visual tree
            InsetClip clip = compositor.CreateInsetClip(
                (float)(clipMargin?.Left ?? 0), (float)(clipMargin?.Top ?? 0),
                (float)(clipMargin?.Right ?? 0), (float)(clipMargin?.Bottom ?? 0));
            clip.Offset = new Vector2(clipOffsetX, clipOffsetY);
            sprite.Clip = clip;
            if (apply) ElementCompositionPreview.SetElementChildVisual(target, sprite);
            return sprite;
        }

        #endregion

        #region Utility extensions

        /// <summary>
        /// Sets a target property to the given value
        /// </summary>
        /// <param name="compObject">The target object</param>
        /// <param name="property">The name of the property to animate</param>
        /// <param name="value">The final value of the property</param>
        public static void SetInstantValue(this CompositionObject compObject, String property, float value)
        {
            // Stop previous animations
            compObject.StopAnimation(property);

            // Setup the animation
            ScalarKeyFrameAnimation animation = compObject.Compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = TimeSpan.FromMilliseconds(1);
            compObject.StartAnimation(property, animation);
        }

        /// <summary>
        /// Starts an animation on the given property of a composition object
        /// </summary>
        /// <param name="compObject">The target object</param>
        /// <param name="property">The name of the property to animate</param>
        /// <param name="value">The final value of the property</param>
        /// <param name="duration">The animation duration</param>
        public static Task StartAnimationAsync(this CompositionObject compObject, String property, float value, TimeSpan duration)
        {
            // Stop previous animations
            compObject.StopAnimation(property);

            // Setup the animation
            ScalarKeyFrameAnimation animation = compObject.Compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = duration;

            // Get the batch and start the animations
            CompositionScopedBatch batch = compObject.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            compObject.StartAnimation(property, animation);
            batch.End();
            return tcs.Task;
        }

        /// <summary>
        /// Stops the animations with the target names on the given element
        /// </summary>
        /// <param name="element">The target element</param>
        /// <param name="properties">The names of the animations to stop</param>
        public static void StopAnimations(this UIElement element, params String[] properties)
        {
            if (properties == null || properties.Length == 0) return;
            Visual visual = element.GetVisual();
            foreach (String property in properties) visual.StopAnimation(property);
        }

        /// <summary>
        /// Sets the scale property of the visual object for a given <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target element</param>
        /// <param name="x">The X value of the scale property</param>
        /// <param name="y">The Y value of the scale property</param>
        /// <param name="z">The Z value of the scale property</param>
        public static void SetVisualScale(this UIElement element, float? x, float? y, float? z)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();

            // Set the scale property
            if (x == null && y == null && z == null) return;
            Vector3 targetScale = new Vector3
            {
                X = x ?? visual.Scale.X,
                Y = y ?? visual.Scale.Y,
                Z = z ?? visual.Scale.Z
            };
            visual.Scale = targetScale;
        }

        /// <summary>
        /// Sets the scale property of the visual object for a given <see cref="FrameworkElement"/> and sets the center point to the center of the element
        /// </summary>
        /// <param name="element">The target element</param>
        /// <param name="x">The X value of the scale property</param>
        /// <param name="y">The Y value of the scale property</param>
        /// <param name="z">The Z value of the scale property</param>
        public static async Task SetVisualScaleAsync(this FrameworkElement element, float? x, float? y, float? z)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            await element.SetCenterPointAsync(visual);

            // Set the scale property
            if (x == null && y == null && z == null) return;
            Vector3 targetScale = new Vector3
            {
                X = x ?? visual.Scale.X,
                Y = y ?? visual.Scale.Y,
                Z = z ?? visual.Scale.Z
            };
            visual.Scale = targetScale;
        }

        /// <summary>
        /// Sets the offset property of the visual object for a given <see cref="UIElement"/> object
        /// </summary>
        /// <param name="element">The target element</param>
        /// <param name="axis">The offset axis to edit</param>
        /// <param name="offset">The final offset value to set for that axis</param>
        public static void SetVisualOffset(this UIElement element, TranslationAxis axis, float offset)
        {
            // Get the element visual and stop the animation
            Visual visual = element.GetVisual();

            // Set the desired offset
            Vector3 endOffset = visual.Offset;
            if (axis == TranslationAxis.X) endOffset.X = offset;
            else endOffset.Y = offset;
            visual.Offset = endOffset;
        }

        /// <summary>
        /// Sets the offset value of a given <see cref="UIElement"/> object
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to edit</param>
        /// <param name="axis">The offset axis to set</param>
        /// <param name="value">The new value for the axis to set</param>
        public static Task SetVisualOffsetAsync(this UIElement element, TranslationAxis axis, float value)
        {
            Visual visual = element.GetVisual();
            return Task.Run(() =>
            {
                Vector3 offset = visual.Offset;
                if (axis == TranslationAxis.X) offset.X = value;
                else offset.Y = value;
                visual.Offset = offset;
            });
        }

        /// <summary>
        /// Returns the visual offset for a target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The input element</param>
        public static Vector3 GetVisualOffset(this UIElement element) => element.GetVisual().Offset;

        /// <summary>
        /// Resets the scale, offset and opacity properties for a framework element
        /// </summary>
        /// <param name="element">The element to edit</param>
        public static async Task ResetCompositionVisualPropertiesAsync(this FrameworkElement element)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            visual.StopAnimation("Scale");
            visual.StopAnimation("Offset");
            visual.StopAnimation("Opacity");
            await element.SetCenterPointAsync(visual);

            // Reset the visual properties
            visual.Scale = Vector3.One;
            visual.Offset = Vector3.Zero;
            visual.Opacity = 1.0f;
        }

        /// <summary>
        /// Gets the opacity for the Visual object behind a given UIElement
        /// </summary>
        /// <param name="element">The source UIElement</param>
        public static float GetVisualOpacity(this UIElement element) => element.GetVisual().Opacity;

        /// <summary>
        /// Sets the opacity for the Visual object behind a given UIElement
        /// </summary>
        /// <param name="element">The source UIElement</param>
        /// <param name="value">The new opacity value</param>
        public static void SetVisualOpacity(this UIElement element, float value) => element.GetVisual().Opacity = value;

        /// <summary>
        /// Returns the Visual object for a given UIElement
        /// </summary>
        /// <param name="element">The source UIElement</param>
        public static Visual GetVisual(this UIElement element) => ElementCompositionPreview.GetElementVisual(element);

        #endregion
    }
}