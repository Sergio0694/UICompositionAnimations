using System;
using FluentExtensions.UI.Brushes.Behaviours;
using FluentExtensions.UI.Brushes.Brushes.Base;

namespace FluentExtensions.UI.Brushes.Brushes
{
    /// <summary>
    /// A <see cref="XamlCompositionBrush"/> that displays a tiled noise texture
    /// </summary>
    public sealed class TilesBrush : XamlCompositionEffectBrushBase
    {
        /// <summary>
        /// Gets or sets the <see cref="Uri"/> to the texture to use
        /// </summary>
        /// <remarks>This property must be initialized before using the brush</remarks>
        public Uri TextureUri { get; set; }

        /// <inheritdoc/>
        protected override PipelineBuilder OnBrushRequested() => PipelineBuilder.FromTiles(TextureUri);
    }
}
