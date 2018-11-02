﻿using Windows.Graphics.Effects;
using JetBrains.Annotations;

namespace UICompositionAnimations.Enums
{
    /// <summary>
    /// An <see langword="enum"/> used to modify the default placement of the input <see cref="IGraphicsEffectSource"/> instance in a blend operation
    /// </summary>
    [PublicAPI]
    public enum EffectPlacement
    {
        /// <summary>
        /// The instance used to call the blend method is placed on top of the other
        /// </summary>
        Foreground,

        /// <summary>
        /// The instance used to call the blend method is placed behind the other
        /// </summary>
        Background
    }
}