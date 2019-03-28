using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;

namespace UICompositionAnimations.AttachedProperties
{
    /// <summary>
    /// A <see langword="class"/> with an attached property to set a composition clip to <see cref="UIElement"/> instances
    /// </summary>
    [PublicAPI]
    public sealed class UIElementProperties
    {
        /// <summary>
        /// Gets the value of the attached property
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to read the property value from</param>
        public static bool GetClipToBounds(UIElement element)
        {
            return element.GetValue(ClipToBoundsProperty).To<bool>();
        }

        /// <summary>
        /// Sets the value of the attached property
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to set the property to</param>
        /// <param name="value">The new value of the attached property</param>
        public static void SetClipToBounds(UIElement element, bool value)
        {
            element.SetValue(ClipToBoundsProperty, value);
        }

        /// <summary>
        /// A property that indicates whether or not the contents of the target <see cref="UIElement"/> should always be clipped to their parent's bounds
        /// </summary>
        public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.RegisterAttached(
            "ClipToBounds",
            typeof(bool),
            typeof(UIElementProperties),
            new PropertyMetadata(false, OnClipToBoundsPropertyChanged));

        private static void OnClipToBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(d.To<UIElement>());
            visual.Clip = e.NewValue.To<bool>() ? visual.Compositor.CreateInsetClip() : null;
        }
    }
}
