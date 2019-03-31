using JetBrains.Annotations;

namespace UICompositionAnimationsLegacy.Enums
{
    /// <summary>
    /// An <see langword="enum"/> that indicates the framework layer to target in a specific animation
    /// </summary>
    [PublicAPI]
    public enum FrameworkLayer
    {
        /// <summary>
        /// Indicates the <see cref="Windows.UI.Composition"/> APIs
        /// </summary>
        Composition,

        /// <summary>
        /// Indicates the <see cref="Windows.UI.Xaml.Media.Animation"/> APIs
        /// </summary>
        Xaml
    }
}