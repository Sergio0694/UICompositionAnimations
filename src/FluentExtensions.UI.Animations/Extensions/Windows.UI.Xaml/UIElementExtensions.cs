using System;
using System.Diagnostics.Contracts;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using FluentExtensions.UI.Animations;
using FluentExtensions.UI.Animations.Enums;
using FluentExtensions.UI.Animations.Interfaces;

#nullable enable

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="UIElement"/> type
    /// </summary>
    public static class UIElementExtensions
    {
        /// <summary>
        /// Returns the desired <see cref="Transform"/> instance after assigning it to the <see cref="UIElement.RenderTransform"/> property of the target <see cref="UIElement"/>
        /// </summary>
        /// <typeparam name="T">The desired <see cref="Transform"/> type</typeparam>
        /// <param name="element">The target <see cref="UIElement"/> to modify</param>
        public static T GetTransform<T>(this UIElement element) where T : Transform, new()
        {
            return element.GetTransform<T>(TransformOption.ReuseIfExisting);
        }

        /// <summary>
        /// Returns the desired <see cref="Transform"/> instance after assigning it to the <see cref="UIElement.RenderTransform"/> property of the target <see cref="UIElement"/>
        /// </summary>
        /// <typeparam name="T">The desired <see cref="Transform"/> type</typeparam>
        /// <param name="element">The target <see cref="UIElement"/> to modify</param>
        /// <param name="option">If <see langword="true"/>, a new <see cref="Transform"/> instance will always be created and assigned to the <see cref="UIElement"/></param>
        public static T GetTransform<T>(this UIElement element, TransformOption option) where T : Transform, new()
        {
            // Return the existing transform object, if it exists
            if (element.RenderTransform is T transform &&
                option == TransformOption.ReuseIfExisting)
            {
                return transform;
            }

            return (T)(element.RenderTransform = new T());
        }

        /// <summary>
        /// Returns the <see cref="Visual"/> object for a given <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The source UIElement</param>
        /// <returns>The <see cref="Visual"/> object associated with <paramref name="element"/></returns>
        [Pure]
        public static Visual GetVisual(this UIElement element) => ElementCompositionPreview.GetElementVisual(element);

        /// <summary>
        /// Initializes an <see cref="IAnimationBuilder"/> instance that targets the input <see cref="UIElement"/>
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        /// <returns>An <see cref="IAnimationBuilder"/> instance that targets <paramref name="target"/></returns>
        [Pure]
        public static IAnimationBuilder Animation(this UIElement target)
        {
            return target.Animation(FrameworkLayer.Composition);
        }

        /// <summary>
        /// Initializes an <see cref="IAnimationBuilder"/> instance that targets the input <see cref="UIElement"/>
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        /// <param name="layer">The target layer to animate</param>
        /// <returns>An <see cref="IAnimationBuilder"/> instance that targets <paramref name="target"/></returns>
        [Pure]
        public static IAnimationBuilder Animation(this UIElement target, FrameworkLayer layer)
        {
            return layer switch
            {
                FrameworkLayer.Composition => new CompositionAnimationBuilder(target),
                FrameworkLayer.Xaml => new XamlAnimationBuilder(target),
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, $"The {layer} value isn't valid"),
            };
        }
    }
}
