using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using UICompositionAnimations.Composition;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Extensions.Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="ScrollViewer"/> control
    /// </summary>
    [PublicAPI]
    public static class ScrollViewerExtensions
    {
        /// <summary>
        /// Creates and starts an animation on the target element that binds either the X or Y axis of the source <see cref="ScrollViewer"/>
        /// </summary>
        /// <param name="scroller">The source <see cref="ScrollViewer"/> control to use</param>
        /// <param name="target">The target <see cref="UIElement"/> that will be animated</param>
        /// <param name="sourceXY">The scrolling axis of the source <see cref="ScrollViewer"/></param>
        /// <param name="targetXY">The optional scrolling axis of the target element, if <see langword="null"/> the source axis will be used</param>
        /// <param name="invertSourceAxis">Indicates whether or not to invert the animation from the source <see cref="CompositionPropertySet"/></param>
        [NotNull]
        public static ExpressionAnimation StartExpressionAnimation(
            [NotNull] this ScrollViewer scroller, [NotNull] UIElement target,
            TranslationAxis sourceXY, TranslationAxis? targetXY = null, bool invertSourceAxis = false)
        {
            CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);
            string sign = invertSourceAxis ? "-" : string.Empty;
            ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"{sign}scroll.Translation.{sourceXY}");
            animation.SetReferenceParameter("scroll", scrollSet);
            target.GetVisual().StartAnimation($"Offset.{targetXY ?? sourceXY}", animation);
            return animation;
        }

        /// <summary>
        /// Creates and starts an animation on the target element, with the addition of a scalar parameter in the resulting <see cref="ExpressionAnimation"/>
        /// </summary>
        /// <param name="scroller">The source <see cref="ScrollViewer"/> control to use</param>
        /// <param name="target">The target <see cref="UIElement"/> that will be animated</param>
        /// <param name="sourceXY">The scrolling axis of the source <see cref="ScrollViewer"/></param>
        /// <param name="parameter">An additional parameter that will be included in the expression animation</param>
        /// <param name="targetXY">The optional scrolling axis of the target element, if <see langword="null"/> the source axis will be used</param>
        /// <param name="invertSourceAxis">Indicates whether or not to invert the animation from the source <see cref="CompositionPropertySet"/></param>
        [NotNull]
        public static ExpressionAnimationWithScalarParameter StartExpressionAnimation(
            [NotNull] this ScrollViewer scroller, [NotNull] UIElement target,
            TranslationAxis sourceXY, float parameter,
            TranslationAxis? targetXY = null, bool invertSourceAxis = false)
        {
            // Get the property set and setup the scroller offset sign
            CompositionPropertySet scrollSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroller);
            string sign = invertSourceAxis ? "-" : "+";

            // Prepare the second property set to insert the additional parameter
            CompositionPropertySet properties = scroller.GetVisual().Compositor.CreatePropertySet();
            properties.InsertScalar(nameof(parameter), parameter);

            // Create and start the animation
            ExpressionAnimation animation = scrollSet.Compositor.CreateExpressionAnimation($"{nameof(properties)}.{nameof(parameter)} {sign} scroll.Translation.{sourceXY}");
            animation.SetReferenceParameter("scroll", scrollSet);
            animation.SetReferenceParameter(nameof(properties), properties);
            target.GetVisual().StartAnimation($"Offset.{targetXY ?? sourceXY}", animation);
            return new ExpressionAnimationWithScalarParameter(animation, properties, nameof(parameter));
        }
    }
}
