using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.Foundation;

namespace Windows.UI.Composition
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
            Func<bool> loadedTester = () => element.ActualWidth + element.ActualHeight < 0.1;
            if (loadedTester())
            {
                // Wait for the loaded event and set the CenterPoint
                TaskCompletionSource<object> loadedTcs = new TaskCompletionSource<object>();
                RoutedEventHandler handler = null;
                handler = (s, e) =>
                {
                    loadedTcs.SetResult(null);
                    element.Loaded -= handler;
                };
                element.Loaded += handler;
                await Task.WhenAny(loadedTcs.Task, Task.Delay(500));
                element.Loaded -= handler;
                if (loadedTester())
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
        private static Task ManageCompositionFadeSlidenimationAsync(this UIElement element,
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

            // Get the opacity the animation
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
            await ManageCompositionFadeSlidenimationAsync(element, startOp, endOp, ms, msDelay, easingFunction);
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
            return ManageCompositionFadeSlidenimationAsync(element, startOp, endOp, ms, msDelay, easingFunction);
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
            Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endScale, initialScale, duration, delay, ease);

            // Get the batch and start the animations
            CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            visual.StartAnimation("Scale", offsetAnimation);
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
            if (reverse) await ManageCompositionScaleAnimationAsync(element, endScale, startScale.Value, ms, msDelay, easingFunction);
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

        #endregion

        #region Utility extensions

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
        /// Sets the offset value of a given UIElement
        /// </summary>
        /// <param name="element">The UIElement to edit</param>
        /// <param name="axis">The offset axis to set</param>
        /// <param name="value">The new value for the axis to set</param>
        public static void SetVisualOffset(this UIElement element, TranslationAxis axis, float value)
        {
            Visual visual = element.GetVisual();
            Task.Run(() =>
            {
                Vector3 offset = visual.Offset;
                if (axis == TranslationAxis.X) offset.X = value;
                else offset.Y = value;
                visual.Offset = offset;
            });
        }

        /// <summary>
        /// Returns the Visual object for a given UIElement
        /// </summary>
        /// <param name="element">The source UIElement</param>
        public static Visual GetVisual(this UIElement element) => ElementCompositionPreview.GetElementVisual(element);

        #endregion
    }
}