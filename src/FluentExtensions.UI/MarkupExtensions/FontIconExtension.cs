using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

#nullable enable

namespace FluentExtensions.MarkupExtensions
{
    /// <summary>
    /// A markup extension that can be used to create a <see cref="FontIcon"/> from a <see cref="string"/> that specifies a Segoe MDL2 Assets icon
    /// </summary>
    [MarkupExtensionReturnType(ReturnType = typeof(FontIcon))]
    public sealed class FontIconExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the <see cref="string"/> representing the icon to display
        /// </summary>
        public string? Glyph { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            return new FontIcon
            {
                Glyph = Glyph!,
                FontFamily = new FontFamily("Segoe MDL2 Assets")
            };
        }
    }
}