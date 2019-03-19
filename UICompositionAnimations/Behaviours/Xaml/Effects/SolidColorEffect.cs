using Windows.UI;

namespace UICompositionAnimations.Behaviours.Xaml.Effects
{
    /// <summary>
    /// A simple effect that renders a solid color on the available surface
    /// </summary>
    public sealed class SolidColorEffect : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the color to display
        /// </summary>
        public Color Color { get; set; }
    }
}
