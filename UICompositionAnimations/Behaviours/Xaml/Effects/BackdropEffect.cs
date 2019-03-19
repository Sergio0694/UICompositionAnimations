using Windows.UI.Xaml.Media;

namespace UICompositionAnimations.Behaviours.Xaml.Effects
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
