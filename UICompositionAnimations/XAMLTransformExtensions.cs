using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;
using UICompositionAnimations.XAMLTransform;

namespace UICompositionAnimations
{
    /// <summary>
    /// A static class that wraps the animation methods in the Windows.UI.Xaml.Media.Animation namespace
    /// </summary>
    [PublicAPI]
    public static class XAMLTransformExtensions
    {
        #region Fade

        // Manages the fade animation
        private static async Task ManageXAMLTransformFadeAnimationAsync(this UIElement element,
            double? startOp, double? endOp,
            int ms, int? msDelay, Easing easingFunction, bool reverse)
        {
            // Delay if necessary
            if (msDelay.HasValue) await Task.Delay(msDelay.Value);

            // Start and wait the animation
            DoubleAnimation animation = XAMLTransformToolkit.CreateDoubleAnimation(element, "Opacity", startOp ?? element.Opacity, endOp, ms, easingFunction);
            Storyboard storyboard = XAMLTransformToolkit.PrepareStoryboard(animation);
            storyboard.AutoReverse = reverse;
            await storyboard.WaitAsync();
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
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async void StartXAMLTransformFadeAnimation(this UIElement element,
            double? startOp, double? endOp,
            int ms, int? msDelay, Easing easingFunction, Action callback = null, bool reverse = false)
        {
            await ManageXAMLTransformFadeAnimationAsync(element, startOp, endOp, ms, msDelay, easingFunction, reverse);
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
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static Task StartXAMLTransformFadeAnimationAsync(this UIElement element,
            double? startOp, double? endOp,
            int ms, int? msDelay, Easing easingFunction, bool reverse = false)
        {
            return ManageXAMLTransformFadeAnimationAsync(element, startOp, endOp, ms, msDelay, easingFunction, reverse);
        }

        #endregion

        #region Fade and slide

        // Manages the fade and slide animation
        private static async Task ManageXAMLTransformFadeSlideAnimationAsync(this UIElement element,
            double? startOp, double? endOp,
            Axis axis, double? startXY, double? endXY,
            int msOp, int? msSlide, int? msDelay, Easing easingFunction, bool reverse)
        {
            // Delay if necessary
            if (msDelay.HasValue) await Task.Delay(msDelay.Value);

            // Try to get the original starting value if necessary
            if (startXY == null && element.RenderTransform is TranslateTransform)
            {
                startXY = axis == Axis.X ? element.RenderTransform.To<TranslateTransform>().X : element.RenderTransform.To<TranslateTransform>().Y;
            }

            // Start and wait the animation
            DoubleAnimation opacity = XAMLTransformToolkit.CreateDoubleAnimation(element, "Opacity", startOp ?? element.Opacity, endOp, msOp, easingFunction);
            DoubleAnimation slide = XAMLTransformToolkit.CreateDoubleAnimation(element.GetRenderTransform<TranslateTransform>(),
                axis.ToString(), startXY, endXY,
                msSlide ?? msOp, easingFunction);
            Storyboard storyboard = XAMLTransformToolkit.PrepareStoryboard(opacity, slide);
            storyboard.AutoReverse = reverse;
            await storyboard.WaitAsync();
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
        /// <param name="msOp">The duration of the animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async void StartXAMLTransformFadeSlideAnimation(this UIElement element,
            double? startOp, double? endOp,
            Axis axis, double? startXY, double? endXY,
            int msOp, int? msSlide, int? msDelay, Easing easingFunction, Action callback = null, bool reverse = false)
        {
            await element.ManageXAMLTransformFadeSlideAnimationAsync(startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts and wait a fade and slide animation on the target UIElement
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
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static Task StartXAMLTransformFadeSlideAnimationAsync(this UIElement element,
            double? startOp, double? endOp,
            Axis axis, double? startXY, double? endXY,
            int msOp, int? msSlide, int? msDelay, Easing easingFunction, bool reverse = false)
        {
            return element.ManageXAMLTransformFadeSlideAnimationAsync(startOp, endOp, axis, startXY, endXY, msOp, msSlide, msDelay, easingFunction, reverse);
        }

        /// <summary>
        /// Fades the opacity of a target element and slides it over a given axis
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="startOp">The initial opacity</param>
        /// <param name="endOp">The end opacity</param>
        /// <param name="axis">A string that indicates which axis to use with the TranslateTransform animation</param>
        /// <param name="startXY">The initial axis value</param>
        /// <param name="endXY">The final axis value</param>
        /// <param name="ms">The duration of the animation in milliseconds</param>
        /// <param name="easing">The easing function to use in the animation</param>
        public static Storyboard GetXAMLTransformFadeSlideStoryboard(this UIElement element, double? startOp, double? endOp,
            Axis axis, double? startXY, double? endXY, int ms, Easing easing)
        {
            // Try to get the original starting value if necessary
            TranslateTransform translate = element.RenderTransform as TranslateTransform;
            bool cleanAnimation = startXY != null;
            if (startXY == null && translate != null)
            {
                startXY = axis == Axis.X ? translate.X : translate.Y;
            }

            // Prepare and run the animation
            if (translate == null || cleanAnimation)
            {
                translate = new TranslateTransform();
                element.RenderTransform = translate;
            }
            return XAMLTransformToolkit.PrepareStoryboard(
                XAMLTransformToolkit.CreateDoubleAnimation(element, "Opacity", startOp ?? element.Opacity, endOp, ms, easing),
                XAMLTransformToolkit.CreateDoubleAnimation(translate, axis.ToString(), startXY, endXY, ms, easing));
        }

        #endregion

        #region Fade and scale

        // Manages the scale animation
        private static async Task ManageXAMLTransformFadeScaleAnimationAsync(this UIElement element,
            double? startOp, double? endOp,
            double? startScale, double? endScale,
            int msOp, int? msScale, int? msDelay, Easing easingFunction, bool reverse)
        {
            // Delay if necessary
            if (msDelay.HasValue) await Task.Delay(msDelay.Value);

            // Try to get the original starting values if necessary
            if (startScale == null && element.RenderTransform is ScaleTransform)
            {
                ScaleTransform scale = element.RenderTransform.To<ScaleTransform>();
                startScale = (scale.ScaleX + scale.ScaleY) / 2;
            }

            // Start and wait the animation
            DoubleAnimation opacity = XAMLTransformToolkit.CreateDoubleAnimation(element, "Opacity", startOp ?? element.Opacity, endOp, msOp, easingFunction);
            DoubleAnimation scaleX = XAMLTransformToolkit.CreateDoubleAnimation(element.GetRenderTransform<ScaleTransform>(), "ScaleX",
                startScale, endScale, msScale ?? msOp, easingFunction);
            DoubleAnimation scaleY = XAMLTransformToolkit.CreateDoubleAnimation(element.GetRenderTransform<ScaleTransform>(), "ScaleY",
                startScale, endScale, msScale ?? msOp, easingFunction);
            Storyboard storyboard = XAMLTransformToolkit.PrepareStoryboard(opacity, scaleX, scaleY);
            storyboard.AutoReverse = reverse;
            await storyboard.WaitAsync();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target UIElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial scale value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale value</param>
        /// <param name="msOp">The duration of the animation, in milliseconds</param>
        /// <param name="msScale">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async void StartXAMLTransformFadeScaleAnimation(this UIElement element,
            double? startOp, double? endOp,
            double? startScale, double? endScale,
            int msOp, int? msScale, int? msDelay, Easing easingFunction, Action callback = null, bool reverse = false)
        {
            await element.ManageXAMLTransformFadeScaleAnimationAsync(startOp, endOp, startScale, endScale, msOp, msScale, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts and wait a fade and slide animation on the target UIElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startOp">The initial opacity value. If null, the current opacity will be used</param>
        /// <param name="endOp">The final opacity value</param>
        /// <param name="startScale">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endScale">The final offset value</param>
        /// <param name="msOp">The duration of the animation, in milliseconds</param>
        /// <param name="msSlide">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static Task StartXAMLTransformFadeScaleAnimationAsync(this UIElement element,
            double? startOp, double? endOp,
            double? startScale, double? endScale,
            int msOp, int? msSlide, int? msDelay, Easing easingFunction, bool reverse = false)
        {
            return element.ManageXAMLTransformFadeScaleAnimationAsync(startOp, endOp, startScale, endScale, msOp, msSlide, msDelay, easingFunction, reverse);
        }

        #endregion

        #region Scale only

        // Manages the scale animation
        private static async Task ManageXAMLTransformFadeScaleAnimationAsync(this UIElement element,
            double? startScale, double? endScale,
            int ms, int? msDelay, Easing easingFunction, bool reverse)
        {
            // Delay if necessary
            if (msDelay.HasValue) await Task.Delay(msDelay.Value);

            // Try to get the original starting values if necessary
            if (startScale == null && element.RenderTransform is ScaleTransform)
            {
                ScaleTransform scale = element.RenderTransform.To<ScaleTransform>();
                startScale = (scale.ScaleX + scale.ScaleY) / 2;
            }

            // Start and wait the animation
            DoubleAnimation scaleX = XAMLTransformToolkit.CreateDoubleAnimation(element.GetRenderTransform<ScaleTransform>(), "ScaleX",
                startScale, endScale, ms, easingFunction);
            DoubleAnimation scaleY = XAMLTransformToolkit.CreateDoubleAnimation(element.GetRenderTransform<ScaleTransform>(), "ScaleY",
                startScale, endScale, ms, easingFunction);
            Storyboard storyboard = XAMLTransformToolkit.PrepareStoryboard(scaleX, scaleY);
            storyboard.AutoReverse = reverse;
            await storyboard.WaitAsync();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target UIElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startScale">The initial scale value. If null, the current scale will be used</param>
        /// <param name="endScale">The final scale value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async void StartXAMLTransformScaleAnimation(this UIElement element,
            double? startScale, double? endScale,
            int ms, int? msDelay, Easing easingFunction, Action callback = null, bool reverse = false)
        {
            await element.ManageXAMLTransformFadeScaleAnimationAsync(startScale, endScale, ms, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts and wait a fade and slide animation on the target UIElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="startScale">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endScale">The final offset value</param>
        /// <param name="ms">The duration of the fade animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static Task StartXAMLTransformScaleAnimationAsync(this UIElement element,
            double? startScale, double? endScale,
            int ms, int? msDelay, Easing easingFunction, bool reverse = false)
        {
            return element.ManageXAMLTransformFadeScaleAnimationAsync(startScale, endScale, ms, msDelay, easingFunction, reverse);
        }

        #endregion

        #region Slide only

        // Manages the fade and slide animation
        private static async Task ManageXAMLTransformSlideAnimationAsync(this UIElement element,
            Axis axis, double? startXY, double? endXY,
            int ms, int? msDelay, Easing easingFunction, bool reverse)
        {
            // Delay if necessary
            if (msDelay.HasValue) await Task.Delay(msDelay.Value);

            // Try to get the original starting value if necessary
            if (startXY == null && element.RenderTransform is TranslateTransform)
            {
                startXY = axis == Axis.X ? element.RenderTransform.To<TranslateTransform>().X : element.RenderTransform.To<TranslateTransform>().Y;
            }

            // Start and wait the animation
            DoubleAnimation slide = XAMLTransformToolkit.CreateDoubleAnimation(element.GetRenderTransform<TranslateTransform>(),
                axis.ToString(), startXY, endXY, ms, easingFunction);
            Storyboard storyboard = XAMLTransformToolkit.PrepareStoryboard(slide);
            storyboard.AutoReverse = reverse;
            await storyboard.WaitAsync();
        }

        /// <summary>
        /// Starts a fade and slide animation on the target UIElement and optionally runs a callback Action when the animations finish
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="ms">The duration of the animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="callback">An Action to execute when the new animations end</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static async void StartXAMLTransformSlideAnimation(this UIElement element,
            Axis axis, double? startXY, double? endXY,
            int ms, int? msDelay, Easing easingFunction, Action callback = null, bool reverse = false)
        {
            await element.ManageXAMLTransformSlideAnimationAsync(axis, startXY, endXY, ms, msDelay, easingFunction, reverse);
            callback?.Invoke();
        }

        /// <summary>
        /// Starts and wait a fade and slide animation on the target UIElement
        /// </summary>
        /// <param name="element">The UIElement to animate</param>
        /// <param name="axis">The offset axis to use on the translation animation</param>
        /// <param name="startXY">The initial offset value. If null, the current offset will be used</param>
        /// <param name="endXY">The final offset value</param>
        /// <param name="ms">The duration of the fade animation, in milliseconds</param>
        /// <param name="msDelay">The delay before the animation starts, in milliseconds. If null, there will be no delay</param>
        /// <param name="easingFunction">The easing function to use with the new animations</param>
        /// <param name="reverse">If true, the animation will be played in reverse mode when it finishes for the first time</param>
        public static Task StartXAMLTransformSlideAnimationAsync(this UIElement element,
            Axis axis, double? startXY, double? endXY,
            int ms, int? msDelay, Easing easingFunction, bool reverse = false)
        {
            return element.ManageXAMLTransformSlideAnimationAsync(axis, startXY, endXY, ms, msDelay, easingFunction, reverse);
        }

        /// <summary>
        /// Slides a target element over a given axis
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="axis">A string that indicates which axis to use with the TranslateTransform animation</param>
        /// <param name="startXY">The initial axis value</param>
        /// <param name="endXY">The final axis value</param>
        /// <param name="ms">The duration of the animation in milliseconds</param>
        /// <param name="easing">The easing function to use in the animation</param>
        public static Storyboard GetXAMLTransformSlideStoryboard(this UIElement element,
            Axis axis, double? startXY, double? endXY, int ms, Easing easing)
        {
            // Try to get the original starting value if necessary
            TranslateTransform translate = element.RenderTransform as TranslateTransform;
            bool cleanAnimation = startXY != null;
            if (startXY == null && translate != null)
            {
                startXY = axis == Axis.X ? element.RenderTransform.To<TranslateTransform>().X : element.RenderTransform.To<TranslateTransform>().Y;
            }

            // Prepare and run the animation
            if (translate == null || cleanAnimation)
            {
                translate = new TranslateTransform();
                element.RenderTransform = translate;
            }
            return XAMLTransformToolkit.PrepareStoryboard(XAMLTransformToolkit.CreateDoubleAnimation(translate, axis.ToString(), startXY, endXY, ms, easing));
        }

        #endregion

        #region Thickness animations

        /// <summary>
        /// Animates a side of the margin of the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="start">The initial value for the animation</param>
        /// <param name="end">The final value for the animation</param>
        /// <param name="side">The margin side to animate</param>
        /// <param name="duration">The duration of the animation to create</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")] // Margin updates at each animation timestep
        public static async Task StartXAMLMarginAnimation([NotNull] this FrameworkElement element, double? start, double end, Side side, int duration, int? delay = null)
        {
            // Delay if needed, and calculate the start offset
            if (delay != null) await Task.Delay(delay.Value);
            if (start == null)
            {
                switch (side)
                {
                    case Side.Top:
                        start = element.Margin.Top;
                        break;
                    case Side.Bottom:
                        start = element.Margin.Bottom;
                        break;
                    case Side.Left:
                        start = element.Margin.Left;
                        break;
                    case Side.Right:
                        start = element.Margin.Right;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(side), "Invalid margin side");
                }
            }

            // Calculate the animation steps
            int
                frames = (int)Math.Ceiling(duration * 4d / 100d),
                elapsed = 0;
            double
                delta = end - start.Value,
                step = delta / frames;

            // Execute the animation
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) }; // 40fps
            timer.Tick += TickHandler;
            Thickness margin = element.Margin;

            void TickHandler(object sender, object e)
            {
                elapsed++;
                switch (side)
                {
                    case Side.Top:
                        margin.Top += step;
                        break;
                    case Side.Bottom:
                        margin.Bottom += step;
                        break;
                    case Side.Left:
                        margin.Left += step;
                        break;
                    case Side.Right:
                        margin.Right += step;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(side), "Invalid margin side");
                }
                element.Margin = margin;
                if (elapsed >= frames)
                {
                    timer.Tick -= TickHandler;
                    timer.Stop();
                    tcs.SetResult(true);
                }
            }
            timer.Start();
            await tcs.Task;

            // Wait for completion and adjust the final margin (just to be sure)
            switch (side)
            {
                case Side.Top:
                    margin.Top = end;
                    break;
                case Side.Bottom:
                    margin.Bottom = end;
                    break;
                case Side.Left:
                    margin.Left = end;
                    break;
                case Side.Right:
                    margin.Right = end;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(side), "Invalid margin side");
            }
            element.Margin = margin;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Animates the target color brush to a given color
        /// </summary>
        /// <param name="solidColorBrush">The <see cref="SolidColorBrush"/> to animate</param>
        /// <param name="color">The target value to set</param>
        /// <param name="ms">The duration of the animation</param>
        /// <param name="easing">The easing function to use</param>
        public static void AnimateColor(this SolidColorBrush solidColorBrush, Color color, int ms, Easing easing)
        {
            // Get the target color
            if (solidColorBrush.Color.Equals(color)) return;

            // Prepare the animation
            ColorAnimation animation = new ColorAnimation
            {
                From = solidColorBrush.Color,
                To = color,
                Duration = new Duration(TimeSpan.FromMilliseconds(ms)),
                EasingFunction = easing.ToEasingFunction()
            };
            Storyboard.SetTarget(animation, solidColorBrush);
            Storyboard.SetTargetProperty(animation, "Color");
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        /// <summary>
        /// Gets a looped storyboard that makes the opacity of the element go from to 0 to 1 and vice versa
        /// </summary>
        /// <param name="element">The element to animate</param>
        /// <param name="ms">The loop duration</param>
        public static Storyboard GetLoopedFadeStoryboard(this UIElement element, int ms)
        {
            DoubleAnimation backgroundAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(ms)),
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut },
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            Storyboard.SetTarget(backgroundAnimation, element);
            Storyboard.SetTargetProperty(backgroundAnimation, "Opacity");
            return XAMLTransformToolkit.PrepareStoryboard(backgroundAnimation);
        }

        #endregion
    }
}