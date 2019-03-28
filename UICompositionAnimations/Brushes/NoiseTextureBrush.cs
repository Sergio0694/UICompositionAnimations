using System;
using UICompositionAnimations.Behaviours;
using UICompositionAnimations.Brushes.Base;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A <see cref="XamlCompositionBrush"/> that displays a tiled noise texture
    /// </summary>
    public sealed class NoiseTextureBrush : XamlCompositionEffectBrushBase
    {
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> to the texture to use
        /// </summary>
        /// <remarks>This property must be initialized before using the brush</remarks>
        public Uri TextureUri { get; set; }

        /// <inheritdoc/>
        protected override CompositionBrushBuilder OnBrushRequested() => CompositionBrushBuilder.FromTiles(TextureUri);
    }
}
