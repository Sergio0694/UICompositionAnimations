using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using System.Numerics;

namespace Windows.UI.Composition
{
    /// <summary>
    /// A static class that wraps the animation methods in the Windows.UI.Composition namespace
    /// </summary>
    public static class CompositionExtensions
    {
        #region Fade

        // Manages the fade animation
        private static Task ManageCompositionFadeSlidenimationAsync(this UIElement element,
            float? startOp, float endOp,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Move to a background thread
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            return Task.Run(() =>
            {
                // Get the easing function, the duration and delay
                CompositionEasingFunction ease = visual.GetEasingFunction(easingFunction);
                TimeSpan duration = TimeSpan.FromMilliseconds(ms);
                TimeSpan? delay;
                if (msDelay.HasValue) delay = TimeSpan.FromMilliseconds(msDelay.Value);
                else delay = null;

                // Get the batch
                CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                // Get the opacity the animation
                ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, duration, delay, ease);

                // Close the batch and manage its event
                batch.End();
                batch.Completed += (s, e) => tcs.SetResult(null);
                visual.StartAnimation("Opacity", opacityAnimation);
                return tcs.Task;
            });
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
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values
            Visual visual = element.GetVisual();
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Move to a background thread
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            return Task.Run(() =>
            {
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

                // Get the batch
                CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                // Get the opacity the animation
                ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, duration, delay, ease);

                // Offset animation
                Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endOffset, initialOffset, duration, delay, ease);

                // Close the batch and manage its event
                batch.End();
                batch.Completed += (s, e) => tcs.SetResult(null);
                visual.StartAnimation("Opacity", opacityAnimation);
                visual.StartAnimation("Offset", offsetAnimation);
                return tcs.Task;
            });
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
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionFadeSlideAnimation(this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int ms, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await ManageCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, ms, msDelay, easingFunction);
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
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeSlideAnimationAsync(this UIElement element,
            float? startOp, float endOp,
            TranslationAxis axis, float? startXY, float endXY,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            return ManageCompositionFadeSlideAnimationAsync(element, startOp, endOp, axis, startXY, endXY, ms, msDelay, easingFunction);
        }

        #endregion

        #region Fade and scale

        // Manages the fade and scale animation
        private static async Task ManageCompositionFadeScaleAnimationAsync(this FrameworkElement element,
            float? startOp, float endOp,
            float? startXY, float endXY,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();
            if (element.ActualWidth + element.ActualHeight < 0.1)
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
                await loadedTcs.Task;
            }
            visual.CenterPoint = new Vector3((float)element.ActualWidth / 2, (float)element.ActualHeight / 2, 0);
            if (!startOp.HasValue) startOp = visual.Opacity;

            // Move to a background thread
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            await Task.Run(() =>
            {
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

                // Get the batch
                CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                // Get the opacity the animation
                ScalarKeyFrameAnimation opacityAnimation = visual.Compositor.CreateScalarKeyFrameAnimation(endOp, startOp, duration, delay, ease);

                // Offset animation
                Vector3KeyFrameAnimation offsetAnimation = visual.Compositor.CreateVector3KeyFrameAnimation(endScale, initialScale, duration, delay, ease);

                // Close the batch and manage its event
                batch.End();
                batch.Completed += (s, e) => tcs.SetResult(null);
                visual.StartAnimation("Opacity", opacityAnimation);
                visual.StartAnimation("Scale", offsetAnimation);
                return tcs.Task;
            });
        }

        /// <summary>
        /// Starts a fade and slide animation on the target FrameworkElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        public static async void StartCompositionFadeScaleAnimation(this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int ms, int? msDelay, EasingFunctionNames easingFunction, Action callback = null)
        {
            await ManageCompositionFadeScaleAnimationAsync(element, startOp, endOp, startScale, endScale, ms, msDelay, easingFunction);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target FrameworkElement and returns a Task that completes when the animation ends
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale X and Y value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale X and Y value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        public static Task StartCompositionFadeScaleAnimationAsync(this FrameworkElement element,
            float? startOp, float endOp,
            float? startScale, float endScale,
            int ms, int? msDelay, EasingFunctionNames easingFunction)
        {
            return ManageCompositionFadeScaleAnimationAsync(element, startOp, endOp, startScale, endScale, ms, msDelay, easingFunction);
        }

        #endregion

        #region Utility extensions

        /// <summary>
        /// Sets the opacity for the Visual object behing a given UIElement
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