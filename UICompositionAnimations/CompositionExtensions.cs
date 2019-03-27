using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using JetBrains.Annotations;
using UICompositionAnimations.Composition;
using UICompositionAnimations.Composition.Misc;
using UICompositionAnimations.Enums;
using Windows.UI.Xaml.Shapes;

namespace UICompositionAnimations
{
    /// <summary>
    /// A static class that wraps the animation methods in the Windows.UI.Composition namespace
    /// </summary>
    [PublicAPI]
    public static class CompositionExtensions
    {
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
            string sign = invertSourceAxis ? "-" : string.Empty;
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
            string sign = invertSourceAxis ? "-" : "+";

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
        /// <param name="blurRadius">The optional explicit shadow blur radius</param>
        /// <param name="maskElement">The optional <see cref="UIElement"/> to use to create an alpha mask for the shadow</param>
        /// <returns>The <see cref="SpriteVisual"/> object that hosts the shadow</returns>
        public static SpriteVisual AttachVisualShadow(
            [NotNull] this FrameworkElement element, [NotNull] UIElement target, bool apply,
            float? width, float? height,
            Color color, float opacity,
            float offsetX = 0, float offsetY = 0, 
            Thickness? clipMargin = null, float clipOffsetX = 0, float clipOffsetY = 0,
            float? blurRadius = null, [CanBeNull] UIElement maskElement = null)
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

        #endregion

        #region Utility extensions

        /// <summary>
        /// Starts an animation on the given property of a composition object
        /// </summary>
        /// <param name="compObject">The target object</param>
        /// <param name="property">The name of the property to animate</param>
        /// <param name="value">The final value of the property</param>
        /// <param name="duration">The animation duration</param>
        public static Task StartAnimationAsync([NotNull] this CompositionObject compObject, string property, float value, TimeSpan duration)
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
        /// Adds a <see cref="CompositionBrush"/> instance on top of the target <see cref="FrameworkElement"/> and binds the size of the two items with an expression animation
        /// </summary>
        /// <param name="brush">The <see cref="CompositionBrush"/> instance to display</param>
        /// <param name="target">The target <see cref="FrameworkElement"/> that will host the effect</param>
        public static void AttachToElement([NotNull] this CompositionBrush brush, [NotNull] FrameworkElement target)
        {
            // Add the brush to a sprite and attach it to the target element
            SpriteVisual sprite = Window.Current.Compositor.CreateSpriteVisual();
            sprite.Brush = brush;
            sprite.Size = new Vector2((float)target.ActualWidth, (float)target.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(target, sprite);

            // Keep the sprite size in sync
            sprite.BindSize(target);
        }

        /// <summary>
        /// Starts an expression animation to keep the size of the source <see cref="CompositionObject"/> in sync with the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="source">The composition object to start the animation on</param>
        /// <param name="target">The target <see cref="UIElement"/> to read the size updates from</param>
        public static void BindSize([NotNull] this CompositionObject source, [NotNull] UIElement target)
        {
            Visual visual = target.GetVisual();
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
        [MustUseReturnValue]
        public static bool TryGetDispatcher([NotNull] this CompositionObject source, out CoreDispatcher dispatcher)
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

        #endregion
    }
}
 