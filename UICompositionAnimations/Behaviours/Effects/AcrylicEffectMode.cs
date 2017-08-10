using System;

namespace UICompositionAnimations.Behaviours.Effects
{
    /// <summary>
    /// Indicates the UI mode for an acrylic effect brush
    /// </summary>
    [Flags]
    public enum AcrylicEffectMode
    {
        /// <summary>
        /// The source content is the blurred UI of the application window
        /// </summary>
        InAppBlur = 1,

        /// <summary>
        /// The source content is the host screen
        /// </summary>
        HostBackdrop = 1 << 1
    }
}