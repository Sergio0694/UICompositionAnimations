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
        /// Sets the CenterPoint of a visual to the center of a given FrameworkElement
        /// </summary>
        /// <param name="element">The source element</param>
        /// <param name="visual">The Visual object for the source FrameworkElement</param>
        private static async Task SetCenterPoint(this FrameworkElement element, Visual visual)
        {
            bool LoadedTester() => element.ActualWidth + element.ActualHeight < 0.1;
            if (LoadedTester())
            {
                // Wait for the loaded event and set the CenterPoint
                TaskCompletionSource<object> loadedTcs = new TaskCompletionSource<object>();
                void Handler(object s, RoutedEventArgs e)
                {
                    loadedTcs.SetResult(null);
                    element.Loaded -= Handler;
                }
                element.Loaded += Handler;
                await Task.WhenAny(loadedTcs.Task, Task.Delay(500));
                element.Loaded -= Handler;
                if (LoadedTester())
                {
                    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    visual.CenterPoint = new Vector3((float)element.DesiredSize.Width / 2, (float)element.DesiredSize.Height / 2, 0);
                    return;
                }
            }
            visual.CenterPoint = new Vector3((float)element.ActualWidth / 2, (float)element.ActualHeight / 2, 0);
        }

        #endregion

        #region Fade

        // Manages the fade animation
        private static Task ManageCompositionFadeAnimationAsync(this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Get the opacity animation
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, duration, delay, ease);

            // Close the batch and manage its event
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            batch.End();
            return tcs.Task;
        }

        /// <summary>
        /// Starts a fade animation on the target UIElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionFadeAnimation(this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await ManageCompositionFadeAnimationAsync(element, startOp, endOp, ms, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade animation on the target UIElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeAnimationAsync(this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            return ManageCompositionFadeAnimationAsync(element, startOp, endOp, ms, msDelay, easingFunction);
        }

        /// <summary>
        /// Sets an implicit fade animation on the target FrameworkElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="start">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="end">The final opacity value</param>
        /// <param name="ms">The duration of the fade animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static void SetCompositionFadeImplicitAnimation(this FrameworkElement element, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Get the opacity animation
            CompositionAnimationGroup group = visual.Compositor.CreateAnimationGroup();
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(end, start, duration, delay, ease);
            opacityAnimation.Target = "Opacity";
            group.Add(opacityAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        #endregion

        #region Fade and slide

        // Manages the fade and slide animation
        private static Task ManageCompositionFadeSlideAnimationAsync(this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Offset");
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
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
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, durationOp, delay, ease);

            // Offset animation
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endOffset, initialOffset, durationSlide, delay, ease);

            // Close the batch and manage its event
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            visual.StartAnimation("Offset", offsetAnimation);
            batch.End();
            return tcs.Task;
        }

        /// <summary>
        /// Starts a fade and slide animation on the target UIElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionFadeSlideAnimation(this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await ManageCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target UIElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the slide animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeSlideAnimationAsync(this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction)
        {
            return ManageCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, easingFunction);
        }

        /// <summary>
        /// Sets an implicit fade and slide animation on the target FrameworkElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
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
        public static void SetCompositionFadeSlideImplicitAnimation(this FrameworkElement element, ImplicitAnimationType type,
            float startOp, float endOp,
            TranslationAxis axis, float startXY, float endXY,
            int msOp, int? msSlide, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
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
            ScalarKeyFrameAnimation fade = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, durationOp, delay, ease);
            fade.Target = "Opacity";
            group.Add(fade);
            Vector3KeyFrameAnimation slide = visual.Compositor.CreateVector3KeyFrameAnimation(endOffset, initialOffset, durationSlide, delay, ease);
            slide.Target = "Offset";
            group.Add(slide);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        #endregion

        #region Fade and scale

        // Manages the fade and scale animation
        private static async Task ManageCompositionFadeScaleAnimationAsync(this FrameworkElement element,
            float? startOp, float endOp,
            float? startXY, float endXY,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Scale");
            await element.SetCenterPoint(visual);
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
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
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, durationOp, delay, ease);

            // Scale animation
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endScale, initialScale, durationScale, delay, ease);

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
        /// Starts a fade and scale animation on the target FrameworkElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionFadeScaleAnimation(this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await ManageCompositionFadeScaleAnimationAsync(element, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and scale animation on the target FrameworkElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeScaleAnimationAsync(this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction)
        {
            return ManageCompositionFadeScaleAnimationAsync(element, startOp, endOp, startScale, endScale, msOp, msScale, msDelay, easingFunction);
        }

        /// <summary>
        /// Sets an implicit fade and scale animation on the target FrameworkElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="msOp">The duration of the fade animation, in milliseconds</param>
        /// <param name="msScale">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static async Task SetCompositionFadeScaleImplicitAnimationAsync(this FrameworkElement element, ImplicitAnimationType type,
            float startOp, float endOp,
            float startScale, float endScale,
            int msOp, int? msScale, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            await element.SetCenterPoint(visual);

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
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
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, durationOp, delay, ease);
            opacityAnimation.Target = "Opacity";
            group.Add(opacityAnimation);
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endScale3, initialScale, durationScale, delay, ease);
            scaleAnimation.Target = "Scale";
            group.Add(scaleAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        #endregion

        #region Scale only

        // Manages the scale animation
        private static async Task<float> ManageCompositionScaleAnimationAsync(this FrameworkElement element,
            float? startXY, float endXY,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            visual.StopAnimation("Scale");
            await element.SetCenterPoint(visual);

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
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endScale, initialScale, duration, delay, ease);

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
        /// Starts a scale animation on the target FrameworkElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionScaleAnimation(this FrameworkElement element,
            float? startScale, float endScale,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, Action callback = null)
        {
            await element.StartCompositionScaleAnimationAsync(startScale, endScale, ms, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a scale animation on the target FrameworkElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async Task StartCompositionScaleAnimationAsync(this FrameworkElement element,
            float? startScale, float endScale,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false)
        {
            startScale = await ManageCompositionScaleAnimationAsync(element, startScale, endScale, ms, msDelay, easingFunction);
            if (reverse) await ManageCompositionScaleAnimationAsync(element, endScale, startScale.Value, ms, null, easingFunction);
        }

        /// <summary>
        /// Sets an implicit scale animation on the target FrameworkElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="start">The initial scale value</param>
        /// <param name="end">The final value</param>
        /// <param name="ms">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static async Task SetCompositionScaleImplicitAnimationAsync(this FrameworkElement element, ImplicitAnimationType type,
            float start, float end,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            await element.SetCenterPoint(visual);

            // Get the easing function, the duration and delay
            CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
            TimeSpan duration = TimeSpan.FromMilliseconds(ms);
            TimeSpan? delay;
            if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
            else delay = null;

            // Calculate the initial and final scale values
            Vector3 initialScale = new Vector3(start, start, visual.Scale.Z);
            Vector3 endScale = new Vector3(end, end, visual.Scale.Z);

            // Get the animations
            CompositionAnimationGroup group = visual.Compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation scaleAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endScale, initialScale, duration, delay, ease);
            scaleAnimation.Target = "Scale";
            group.Add(scaleAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        #endregion

        #region Slide only

        // Manages the scale animation
        private static async Task<float> ManageCompositionSlideAnimationAsync(this FrameworkElement element,
            TranslationAxis axis, float? startXY, float endXY,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Offset");

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

            // Scale animation
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endOffset, initialOffset, duration, delay, ease);

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
        /// Starts an offset animation on the target FrameworkElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startOffset">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endOffset">The final offset X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionSlideAnimation(this FrameworkElement element,
            TranslationAxis axis, float? startOffset, float endOffset,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, Action callback = null)
        {
            await element.ManageCompositionSlideAnimationAsync(axis, startOffset, endOffset, ms, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts an offset animation on the target FrameworkElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="startOffset">The initial offset X and Y value. If null, the current offset will be used</param>
        /// <param name="endOffset">The final offset X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async Task StartCompositionSlideAnimationAsync(this FrameworkElement element,
            TranslationAxis axis, float? startOffset, float endOffset,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false)
        {
            startOffset = await element.ManageCompositionSlideAnimationAsync(axis, startOffset, endOffset, ms, msDelay, easingFunction);
            if (reverse) await element.ManageCompositionSlideAnimationAsync(axis, endOffset, startOffset.Value, ms, msDelay, easingFunction);
        }

        /// <summary>
        /// Sets an implicit slide animation on the target FrameworkElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="type">The type of implicit animation to set</param>
        /// <param name="axis">The offset axis</param>
        /// <param name="start">The initial scale value</param>
        /// <param name="end">The final value</param>
        /// <param name="ms">The duration of the scale animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static void SetCompositionSlideImplicitAnimation(this FrameworkElement element, ImplicitAnimationType type,
            TranslationAxis axis, float start, float end,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();

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
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endOffset, initialOffset, duration, delay, ease);
            offsetAnimation.Target = "Offset";
            group.Add(offsetAnimation);

            // Set the implicit animation
            if (type == ImplicitAnimationType.Show) ElementCompositionPreview.SetImplicitShowAnimation(element, group);
            else ElementCompositionPreview.SetImplicitHideAnimation(element, group);
        }

        #endregion

        #region Roll

        // Manages the roll animation
        private static async Task<float> ManageCompositionRollAnimationAsync(this FrameworkElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            visual.StopAnimation("Opacity");
            visual.StopAnimation("Offset");
            visual.StopAnimation("RotationAngle");
            await element.SetCenterPoint(visual);

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
            ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, duration, delay, ease);

            // Scale animation
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endOffset, initialOffset, duration, delay, ease);

            // Rotate animation
            ScalarKeyFrameAnimation rotateAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endDegrees.ToRadians(), startDegrees.Value.ToRadians(), duration, delay, ease);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Opacity", opacityAnimation);
            visual.StartAnimation("Offset", offsetAnimation);
            visual.StartAnimation("RotationAngle", rotateAnimation);
            batch.End();
            await tcs.Task;
            return initialOffset.X;
        }

        /// <summary>
        /// Starts a roll animation on the target FrameworkElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
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
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionRollAnimation(this FrameworkElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false, Action callback = null)
        {
            await element.ManageCompositionRollAnimationAsync(startOp, endOp, axis, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a roll animation on the target FrameworkElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
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
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>>
        public static async Task StartCompositionRollAnimationAsync(this FrameworkElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            float? startDegrees, float endDegrees,
            int ms, int? msDelay, EasingFunctionNames easingFunction, bool reverse = false)
        {
            startXY = await element.ManageCompositionRollAnimationAsync(startOp, endOp, axis, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction);
            if (reverse) await element.ManageCompositionRollAnimationAsync(startOp, endOp, axis, startXY, endXY, startDegrees, endDegrees, ms, msDelay, easingFunction);
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
        public static ExpressionAnimation StartExpressionAnimation(
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
            return animation;
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
        /// <returns>The <see cref="SpriteVisual"/> object that hosts the shadow, and the <see cref="DropShadow"/> itself</returns>
        public static (SpriteVisual, DropShadow) AttachVisualShadow(
            [NotNull] this FrameworkElement element, [NotNull] UIElement target, bool apply,
            float? width, float? height,
            Color color, float opacity,
            float offsetX = 0, float offsetY = 0, 
            Thickness? clipMargin = null, float clipOffsetX = 0, float clipOffsetY = 0)
        {
            // Setup the shadow
            Visual elementVisual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = elementVisual.Compositor;
            SpriteVisual visual = compositor.CreateSpriteVisual();
            DropShadow shadow = compositor.CreateDropShadow();
            shadow.Color = color;
            shadow.Opacity = opacity;
            visual.Shadow = shadow;
            visual.Size = new Vector2(width ?? (float)element.Width, height ?? (float)element.Height);
            visual.Offset = new Vector3(-0.5f, -0.5f, 0);

            // Clip it and add it to the visual tree
            InsetClip clip = compositor.CreateInsetClip(
                (float)(clipMargin?.Left ?? 0), (float)(clipMargin?.Top ?? 0),
                (float)(clipMargin?.Right ?? 0), (float)(clipMargin?.Bottom ?? 0));
            clip.Offset = new Vector2(clipOffsetX, clipOffsetY);
            visual.Clip = clip;
            if (apply) ElementCompositionPreview.SetElementChildVisual(target, visual);
            return (visual, shadow);
        }

        #endregion

        #region Utility extensions

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
            compObject.StartAnimation(property, animation);

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
            await element.SetCenterPoint(visual);

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
            await element.SetCenterPoint(visual);

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