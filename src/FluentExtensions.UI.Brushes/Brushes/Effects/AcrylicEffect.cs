using System;
using Windows.UI;
using Windows.UI.Xaml.Media;
using FluentExtensions.UI.Brushes.Brushes.Effects.Interfaces;

namespace FluentExtensions.UI.Brushes.Brushes.Effects
{
    /// <summary>
    /// A custom acrylic effect that can be inserted into a pipeline
    /// </summary>
    public sealed class AcrylicEffect : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the source mode for the effect
        /// </summary>
        public AcrylicBackgroundSource Source { get; set; }

        /// <summary>
        /// Gets or sets the blur amount for the effect
        /// </summary>
        /// <remarks>This property is ignored when the active mode is <see cref="AcrylicBackgroundSource.HostBackdrop"/></remarks>
        public double BlurAmount { get; set; }

        /// <summary>
        /// Gets or sets the tint for the effect
        /// </summary>
        public Color Tint { get; set; }

        /// <summary>
        /// Gets or sets the color for the tint effect
        /// </summary>
        public double TintMix { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> to the texture to use
        /// </summary>
        public Uri TextureUri { get; set; }
    }
}
