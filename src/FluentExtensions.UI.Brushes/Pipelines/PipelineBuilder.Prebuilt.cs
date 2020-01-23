using System;
using System.Diagnostics.Contracts;
using Windows.UI;
using FluentExtensions.UI.Brushes.Enums;
using Microsoft.Graphics.Canvas.Effects;

#nullable enable

namespace FluentExtensions.UI.Brushes.Pipelines
{
    public sealed partial class PipelineBuilder
    {
        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the host backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="noiseRelativePath">The relative path for the noise texture to load (eg. "/Assets/noise.png")</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromHostBackdropAcrylic(Color tint, float mix, string noiseRelativePath, CacheMode cache = CacheMode.Default)
        {
            return FromHostBackdropAcrylic(tint, mix, noiseRelativePath.ToAppxUri(), cache);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the host backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromHostBackdropAcrylic(Color tint, float mix, Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return
                FromHostBackdropBrush()
                .Effect(source => new LuminanceToAlphaEffect { Source = source })
                .Opacity(0.4f)
                .Blend(FromHostBackdropBrush(), BlendEffectMode.Multiply)
                .Tint(tint, mix)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the host backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="noiseRelativePath">The relative path for the noise texture to load (eg. "/Assets/noise.png")</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromHostBackdropAcrylic(Color tint, float mix, out EffectAnimation tintAnimation, string noiseRelativePath, CacheMode cache = CacheMode.Default)
        {
            return FromHostBackdropAcrylic(tint, mix, out tintAnimation, noiseRelativePath.ToAppxUri(), cache);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the host backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromHostBackdropAcrylic(Color tint, float mix, out EffectAnimation tintAnimation, Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return
                FromHostBackdropBrush()
                .Effect(source => new LuminanceToAlphaEffect { Source = source })
                .Opacity(0.4f)
                .Blend(FromHostBackdropBrush(), BlendEffectMode.Multiply)
                .Tint(tint, mix, out tintAnimation)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="noiseRelativePath">The relative path for the noise texture to load (eg. "/Assets/noise.png")</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(Color tint, float mix, float blur, string noiseRelativePath, CacheMode cache = CacheMode.Default)
        {
            return FromBackdropAcrylic(tint, mix, blur, noiseRelativePath.ToAppxUri(), cache);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(Color tint, float mix, float blur, Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return
                FromBackdropBrush()
                .Tint(tint, mix)
                .Blur(blur)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="noiseRelativePath">The relative path for the noise texture to load (eg. "/Assets/noise.png")</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint,
            float mix,
            out EffectAnimation tintAnimation,
            float blur,
            string noiseRelativePath,
            CacheMode cache = CacheMode.Default)
        {
            return FromBackdropAcrylic(tint, mix, out tintAnimation, blur, noiseRelativePath.ToAppxUri(), cache);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint,
            float mix,
            out EffectAnimation tintAnimation,
            float blur,
            Uri noiseUri,
            CacheMode cache = CacheMode.Default)
        {
            return
                FromBackdropBrush()
                .Tint(tint, mix, out tintAnimation)
                .Blur(blur)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="blurAnimation">The animation to apply on the blur effect in the pipeline</param>
        /// <param name="noiseRelativePath">The relative path for the noise texture to load (eg. "/Assets/noise.png")</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint,
            float mix,
            float blur,
            out EffectAnimation blurAnimation,
            string noiseRelativePath,
            CacheMode cache = CacheMode.Default)
        {
            return FromBackdropAcrylic(tint, mix, blur, out blurAnimation, noiseRelativePath.ToAppxUri(), cache);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="blurAnimation">The animation to apply on the blur effect in the pipeline</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint,
            float mix,
            float blur,
            out EffectAnimation blurAnimation,
            Uri noiseUri,
            CacheMode cache = CacheMode.Default)
        {
            return
                FromBackdropBrush()
                .Tint(tint, mix)
                .Blur(blur, out blurAnimation)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="blurAnimation">The animation to apply on the blur effect in the pipeline</param>
        /// <param name="noiseRelativePath">The relative path for the noise texture to load (eg. "/Assets/noise.png")</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint,
            float mix,
            out EffectAnimation tintAnimation,
            float blur,
            out EffectAnimation blurAnimation,
            string noiseRelativePath,
            CacheMode cache = CacheMode.Default)
        {
            return FromBackdropAcrylic(tint, mix, out tintAnimation, blur, out blurAnimation, noiseRelativePath.ToAppxUri(), cache);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="blurAnimation">The animation to apply on the blur effect in the pipeline</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint,
            float mix,
            out EffectAnimation tintAnimation,
            float blur,
            out EffectAnimation blurAnimation,
            Uri noiseUri,
            CacheMode cache = CacheMode.Default)
        {
            return
                FromBackdropBrush()
                .Tint(tint, mix, out tintAnimation)
                .Blur(blur, out blurAnimation)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }
    }
}
