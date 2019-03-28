﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Shapes;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="FrameworkElement"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Sets the <see cref="Visual.CenterPoint"/> property of the <see cref="Visual"/> behind a given <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The source <see cref="FrameworkElement"/></param>
        public static void SetVisualCenterPoint([NotNull] this FrameworkElement element)
        {
            if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
                throw new InvalidOperationException("The target element must have a fixed size");
            element.GetVisual().CenterPoint = new Vector3((float)(element.Width / 2), (float)(element.Height / 2), 0);
        }

        /// <summary>
        /// Sets the <see cref="Visual.CenterPoint"/> property of the <see cref="Visual"/> behind a given <see cref="FrameworkElement"/> with no fixed size
        /// </summary>
        /// <param name="element">The source element</param>
        public static async Task SetVisualCenterPointAsync([NotNull] this FrameworkElement element)
        {
            // Check if the control hasn't already been loaded
            bool CheckLoadingPending() => element.ActualWidth + element.ActualHeight < 0.1;
            if (CheckLoadingPending())
            {
                // Wait for the loaded event and set the CenterPoint
                TaskCompletionSource tcs = new TaskCompletionSource();
                void Handler(object s, RoutedEventArgs e)
                {
                    tcs.SetResult();
                    element.Loaded -= Handler;
                }

                // Wait for the loaded event for a given time threshold
                element.Loaded += Handler;
                await Task.WhenAny(tcs.Task, Task.Delay(500));
                element.Loaded -= Handler;

                // If the control still hasn't been loaded, approximate the center point with its desired size
                if (CheckLoadingPending())
                {
                    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    element.GetVisual().CenterPoint = new Vector3((float)(element.DesiredSize.Width / 2), (float)(element.DesiredSize.Height / 2), 0);
                    return;
                }
            }

            // Update the center point
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);
        }

        /// <summary>
        /// Creates a <see cref="DropShadow"/> from the given <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The source <see cref="FrameworkElement"/> for the <see cref="DropShadow"/></param>
        /// <param name="target">The optional target <see cref="FrameworkElement"/> to apply the <see cref="DropShadow"/> to (it can be the same as the source <see cref="FrameworkElement"/>)</param>
        /// <param name="apply">Indicates whether or not to immediately add the <see cref="DropShadow"/> to the visual tree</param>
        /// <param name="width">The optional width of the <see cref="DropShadow"/> (if null, the element width will be used)</param>
        /// <param name="height">The optional height of the <see cref="DropShadow"/> (if null, the element height will be used)</param>
        /// <param name="color">The <see cref="DropShadow"/> color (the default is <see cref="Colors.Black"/></param>
        /// <param name="opacity">The opacity of the <see cref="DropShadow"/></param>
        /// <param name="offsetX">The optional horizontal offset of the <see cref="DropShadow"/></param>
        /// <param name="offsetY">The optional vertical offset of the <see cref="DropShadow"/></param>
        /// <param name="clipMargin">The optional margin of the clip area of the <see cref="DropShadow"/></param>
        /// <param name="clipOffsetX">The optional horizontal offset of the clip area of the <see cref="DropShadow"/></param>
        /// <param name="clipOffsetY">The optional vertical offset of the clip area of the <see cref="DropShadow"/></param>
        /// <param name="blurRadius">The optional explicit <see cref="DropShadow"/> blur radius</param>
        /// <param name="maskElement">The optional <see cref="UIElement"/> to use to create an alpha mask for the <see cref="DropShadow"/></param>
        /// <returns>The <see cref="SpriteVisual"/> that hosts the <see cref="DropShadow"/></returns>
        public static SpriteVisual AttachVisualShadow(
            [NotNull] this FrameworkElement element, [NotNull] UIElement target,
            bool apply,
            float? width, float? height,
            Color color, float opacity,
            float offsetX = 0, float offsetY = 0,
            Thickness? clipMargin = null, float clipOffsetX = 0, float clipOffsetY = 0,
            float? blurRadius = null,
            [CanBeNull] UIElement maskElement = null)
        {
            // Setup the shadow
            Visual elementVisual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = elementVisual.Compositor;
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            DropShadow shadow = compositor.CreateDropShadow();
            shadow.Color = color;
            shadow.Opacity = opacity;
            shadow.Offset = new Vector3(offsetX, offsetY, 0);
            if (blurRadius != null) shadow.BlurRadius = blurRadius.Value;
            sprite.Shadow = shadow;
            sprite.Size = new Vector2(width ?? (float)element.Width, height ?? (float)element.Height);

            // Clip it (if needed) and add it to the visual tree
            if (clipMargin != null || clipOffsetX > 0 || clipOffsetY > 0)
            {
                InsetClip clip = compositor.CreateInsetClip(
                (float)(clipMargin?.Left ?? 0), (float)(clipMargin?.Top ?? 0),
                (float)(clipMargin?.Right ?? 0), (float)(clipMargin?.Bottom ?? 0));
                clip.Offset = new Vector2(clipOffsetX, clipOffsetY);
                sprite.Clip = clip;
            }

            // Alpha mask
            switch (maskElement)
            {
                case null: break;
                case Shape shape: shadow.Mask = shape.GetAlphaMask(); break;
                case Image image: shadow.Mask = image.GetAlphaMask(); break;
                case TextBlock textBlock: shadow.Mask = textBlock.GetAlphaMask(); break;
            }
            if (apply) ElementCompositionPreview.SetElementChildVisual(target, sprite);
            return sprite;
        }

        /// <summary>
        /// Animates a side of the margin of the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The <see cref="FrameworkElement"/> to animate</param>
        /// <param name="start">The initial value for the animation</param>
        /// <param name="end">The final value for the animation</param>
        /// <param name="side">The margin side to animate</param>
        /// <param name="duration">The duration of the animation to create</param>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")] // Margin updated at each animation timestep
        public static async Task AnimateMarginAsync([NotNull] this FrameworkElement element, double? start, double end, Side side, int duration)
        {
            // Calculate the start offset
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
    }
}