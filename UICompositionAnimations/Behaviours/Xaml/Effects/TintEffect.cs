using Windows.UI;

namespace UICompositionAnimations.Behaviours.Xaml.Effects
{
    /// <summary>
    /// A tint effect with a customizable opacity
    /// </summary>
    public sealed class TintEffect : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the tint color to use
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the opacity of the tint effect
        /// </summary>
        public float Opacity { get; set; }
    }
}
