using System;
using UICompositionAnimationsLegacy.Enums;

namespace UICompositionAnimationsLegacy.Behaviours.Xaml.Effects.Abstract
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
        /// Gets or sets the DPI mode used to render the image (the default is <see cref="BitmapDPIMode.CopyDisplayDPISettingsWith96AsLowerBound"/>)
        /// </summary>
        public BitmapDPIMode DPIMode { get; set; } = BitmapDPIMode.CopyDisplayDPISettingsWith96AsLowerBound;

        /// <summary>
        /// Gets or sets the cache mode to use when loading the image (the default is <see cref="BitmapCacheMode.Default"/>)
        /// </summary>
        public BitmapCacheMode CacheMode { get; set; } = BitmapCacheMode.Default;
    }
}
