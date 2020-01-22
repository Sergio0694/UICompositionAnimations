using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

#nullable enable

namespace FluentExtensions.AttachedProperties
{
    /// <summary>
    /// A <see langword="class"/> with an attached property to set a composition clip to <see cref="UIElement"/> instances
    /// </summary>
    public sealed class ClipHelper
    {
        /// <summary>
        /// Gets the value of <see cref="ClipToBoundsProperty"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to read the property value from</param>
        public static bool GetClipToBounds(UIElement element)
        {
            return (bool)element.GetValue(ClipToBoundsProperty);
        }

        /// <summary>
        /// Sets the value of <see cref="ClipToBoundsProperty"/>
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
            typeof(ClipHelper),
            new PropertyMetadata(DependencyProperty.UnsetValue, OnClipToBoundsPropertyChanged));

        private static void OnClipToBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement @this = (UIElement)d;
            bool value = (bool)e.NewValue;

            Visual visual = ElementCompositionPreview.GetElementVisual(@this);

            visual.Clip = value ? visual.Compositor.CreateInsetClip() : null;
        }
    }
}