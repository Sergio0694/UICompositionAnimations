namespace UICompositionAnimationsLegacy.Enums
{
    /// <summary>
    /// Indicates the internal state of the acrylic brush
    /// </summary>
    internal enum AcrylicBrushEffectState
    {
        /// <summary>
        /// The composition effect is enabled with the current settings
        /// </summary>
        EffectEnabled,

        /// <summary>
        /// The current effect is not supported and a fallback color is being used
        /// </summary>
        FallbackMode,

        /// <summary>
        /// Default state, no visible effects
        /// </summary>
        Default
    }
}