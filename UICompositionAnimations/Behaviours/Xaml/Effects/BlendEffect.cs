using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Behaviours.Xaml.Effects
{
    /// <summary>
    /// A blend effect that merges the current pipeline with an input one
    /// </summary>
    public sealed class BlendEffect : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the input pipeline to merge with the current instance
        /// </summary>
        [ItemNotNull]
        public List<IPipelineEffect> Input { get; set; } = new List<IPipelineEffect>();

        /// <summary>
        /// Gets or sets the blending mode to use (the default mode is <see cref="BlendEffectMode.Multiply"/>)
        /// </summary>
        public BlendEffectMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the placement of the input pipeline with respect to the current one (the default is <see cref="EffectPlacement.Foreground"/>)
        /// </summary>
        public EffectPlacement Placement { get; set; } = EffectPlacement.Foreground;
    }
}
