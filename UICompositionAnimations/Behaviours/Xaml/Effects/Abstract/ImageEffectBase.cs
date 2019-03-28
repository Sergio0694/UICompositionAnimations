using System;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Behaviours.Xaml.Effects.Abstract
{
    /// <summary>
    /// An image based effect that loads an image at the specified location
    /// </summary>
    public abstract class ImageEffectBase : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Uri"/> for the image to load
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the DPI mode used to render the image (the default is <see cref="DpiMode.DisplayDpiWith96AsLowerBound"/>)
        /// </summary>
        public DpiMode DPIMode { get; set; } = DpiMode.DisplayDpiWith96AsLowerBound;

        /// <summary>
        /// Gets or sets the cache mode to use when loading the image (the default is <see cref="Enums.CacheMode.Default"/>)
        /// </summary>
        public CacheMode CacheMode { get; set; } = CacheMode.Default;
    }
}
