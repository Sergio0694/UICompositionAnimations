using System;
using System.Numerics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for some composition types
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Adds a <see cref="CompositionBrush"/> instance on top of the target <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="brush">The <see cref="CompositionBrush"/> instance to display</param>
        /// <param name="target">The target <see cref="FrameworkElement"/> that will host the effect</param>
        public static void AttachToElement(this CompositionBrush brush, FrameworkElement target)
        {
            // Add the brush to a sprite and attach it to the target element
            SpriteVisual sprite = Window.Current.Compositor.CreateSpriteVisual();
            sprite.Brush = brush;
            sprite.Size = new Vector2((float)target.ActualWidth, (float)target.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(target, sprite);
        }

        /// <summary>
        /// Starts an <see cref="ExpressionAnimation"/> to keep the size of the source <see cref="CompositionObject"/> in sync with the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="source">The <see cref="CompositionObject"/> to start the animation on</param>
        /// <param name="target">The target <see cref="UIElement"/> to read the size updates from</param>
        public static void BindSize(this CompositionObject source, UIElement target)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(target);
            ExpressionAnimation bindSizeAnimation = Window.Current.Compositor.CreateExpressionAnimation($"{nameof(visual)}.Size");
            bindSizeAnimation.SetReferenceParameter(nameof(visual), visual);

            // Start the animation
            source.StartAnimation("Size", bindSizeAnimation);
        }

        /// <summary>
        /// Tries to retrieve the <see cref="CoreDispatcher"/> instance of the input <see cref="CompositionObject"/>
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> instance</param>
        /// <param name="dispatcher">The resulting <see cref="CoreDispatcher"/>, if existing</param>
        public static bool TryGetDispatcher(this CompositionObject source, out CoreDispatcher dispatcher)
        {
            try
            {
                dispatcher = source.Dispatcher;
                return true;
            }
            catch (ObjectDisposedException)
            {
                // I'm sorry Jack, I was too late! :'(
                dispatcher = null;
                return false;
            }
        }
    }
}
