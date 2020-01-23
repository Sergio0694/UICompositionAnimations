using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Windows.UI.Xaml.Media;
using FluentExtensions.UI.Brushes.Behaviours;
using FluentExtensions.UI.Brushes.Brushes.Base;
using FluentExtensions.UI.Brushes.Brushes.Effects;
using FluentExtensions.UI.Brushes.Brushes.Effects.Interfaces;
using Microsoft.Graphics.Canvas.Effects;
using BlendEffect = FluentExtensions.UI.Brushes.Brushes.Effects.BlendEffect;
using OpacityEffect = FluentExtensions.UI.Brushes.Brushes.Effects.OpacityEffect;
using SaturationEffect = FluentExtensions.UI.Brushes.Brushes.Effects.SaturationEffect;
using TileEffect = FluentExtensions.UI.Brushes.Brushes.Effects.TileEffect;
using TintEffect = FluentExtensions.UI.Brushes.Brushes.Effects.TintEffect;

namespace FluentExtensions.UI.Brushes.Brushes
{
    /// <summary>
    /// A <see cref="Brush"/> that renders a customizable Composition/Win2D effects pipeline
    /// </summary>
    public sealed class PipelineBrush : XamlCompositionEffectBrushBase
    {
        /// <summary>
        /// Builds a new effects pipeline from the input effects sequence
        /// </summary>
        /// <param name="effects">The input collection of <see cref="IPipelineEffect"/> instance</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance with the items in <paramref name="effects"/></returns>
        [Pure]
        private static PipelineBuilder Build(IList<IPipelineEffect> effects)
        {
            if (effects.Count == 0) throw new ArgumentException("An effects pipeline can't be empty");

            return effects.Skip(1).Aggregate(Start(effects[0]), (b, e) => Append(e, b));
        }

        /// <summary>
        /// Starts a new composition pipeline from the given effect
        /// </summary>
        /// <param name="effect">The initial <see cref="IPipelineEffect"/> instance</param>
        /// <returns>A new <see cref="PipelineBuilder"/> instance starting from <paramref name="effect"/></returns>
        [Pure]
        private static PipelineBuilder Start(IPipelineEffect effect)
        {
            return effect switch
            {
                BackdropEffect backdrop when backdrop.Source == AcrylicBackgroundSource.Backdrop => PipelineBuilder.FromBackdropBrush(),
                BackdropEffect backdrop when backdrop.Source == AcrylicBackgroundSource.HostBackdrop => PipelineBuilder.FromHostBackdropBrush(),
                SolidColorEffect color => PipelineBuilder.FromColor(color.Color),
                ImageEffect image => PipelineBuilder.FromImage(image.Uri, image.DPIMode, image.CacheMode),
                TileEffect tile => PipelineBuilder.FromTiles(tile.Uri, tile.DPIMode, tile.CacheMode),
                AcrylicEffect acrylic => acrylic.Source switch
                {
                    AcrylicBackgroundSource.Backdrop => PipelineBuilder.FromBackdropAcrylic(acrylic.Tint, (float)acrylic.TintMix, (float)acrylic.BlurAmount, acrylic.TextureUri),
                    AcrylicBackgroundSource.HostBackdrop => PipelineBuilder.FromHostBackdropAcrylic(acrylic.Tint, (float)acrylic.TintMix, acrylic.TextureUri),
                    _ => throw new ArgumentOutOfRangeException(nameof(acrylic.Source), $"Invalid acrylic source: {acrylic.Source}")
                },
                _ => throw new ArgumentException($"Invalid initial pipeline effect: {effect.GetType()}")
            };
        }

        /// <summary>
        /// Appends an effect to an existing composition pipeline
        /// </summary>
        /// <param name="effect">The <see cref="IPipelineEffect"/> instance to append to the current pipeline</param>
        /// <param name="builder">The target <see cref="PipelineBuilder"/> instance to modify</param>
        /// <returns>The target <see cref="PipelineBuilder"/> instance in use</returns>
        private static PipelineBuilder Append(IPipelineEffect effect, PipelineBuilder builder)
        {
            return effect switch
            {
                OpacityEffect opacity => builder.Opacity((float)opacity.Value),
                LuminanceEffect _ => builder.Effect(source => new LuminanceToAlphaEffect { Source = source }),
                TintEffect tint => builder.Tint(tint.Color, (float)tint.Opacity),
                BlurEffect blur => builder.Blur((float)blur.Value),
                SaturationEffect saturation => builder.Saturation((float)saturation.Value),
                BlendEffect blend => builder.Blend(Build(blend.Input), blend.Mode, blend.Placement),
                _ => throw new ArgumentException($"Invalid pipeline effect: {effect.GetType()}")
            };
        }

        /// <inheritdoc/>
        protected override PipelineBuilder OnBrushRequested()
        {
            return Build(Effects);
        }

        /// <summary>
        /// Gets or sets the collection of effects to use in the current pipeline
        /// </summary>
        public IList<IPipelineEffect> Effects { get; set; } = new List<IPipelineEffect>();
    }
}
