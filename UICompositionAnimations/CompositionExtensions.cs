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
    }
}
 