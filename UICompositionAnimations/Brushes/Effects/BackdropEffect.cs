using Windows.UI.Xaml.Media;
using UICompositionAnimations.Brushes.Effects.Interfaces;

namespace UICompositionAnimations.Brushes.Effects
{
    /// <summary>
    /// A backdrop effect that can sample from a specified source
    /// </summary>
    public sealed class BackdropEffect : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the backdrop source to use to render the effect
        /// </summary>
        public AcrylicBackgroundSource Source { get; set; }
    }
}
