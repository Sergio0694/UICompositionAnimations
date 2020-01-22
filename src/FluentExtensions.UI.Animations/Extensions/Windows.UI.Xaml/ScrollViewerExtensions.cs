using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using FluentExtensions.UI.Animations.Enums;

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="ScrollViewer"/> control
    /// </summary>
    public static class ScrollViewerExtensions
    {
        /// <summary>
        /// Creates and starts an animation on the target element that binds either the X or Y axis of the source <see cref="ScrollViewer"/>
        /// </summary>
        /// <param name="scroller">The source <see cref="ScrollViewer"/> control to use</param>
        /// <param name="target">The target <see cref="UIElement"/> that will be animated</param>
        /// <param name="axis">The scrolling axis of the source <see cref="ScrollViewer"/></param>
        public static ExpressionAnimation StartExpressionAnimation(
            this ScrollViewer scroller,
            UIElement target,
            Axis axis)
        {
            return scroller.StartExpressionAnimation(target, axis, axis);
        }

        /// <summary>
        /// Creates and starts an animation on the target element that binds either the X or Y axis of the source <see cref="ScrollViewer"/>
        /// </summary>
        /// <param name="scroller">The source <see cref="ScrollViewer"/> control to use</param>
        /// <param name="target">The target <see cref="UIElement"/> that will be animated</param>
        /// <param name="sourceAxis">The scrolling axis of the source <see cref="ScrollViewer"/></param>
        /// <param name="targetAxis">The optional scrolling axis of the target element, if <see langword="null"/> the source axis will be used</param>
        public static ExpressionAnimation StartExpressionAnimation(
            this ScrollViewer scroller,
            UIElement target,
            Axis sourceAxis,
            Axis targetAxis)
        {
            CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);

            ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"scroll.Translation.{sourceAxis}");

            animation.SetReferenceParameter("scroll", scrollSet);

            target.GetVisual().StartAnimation($"Offset.{targetAxis}", animation);

            return animation;
        }
    }
}
