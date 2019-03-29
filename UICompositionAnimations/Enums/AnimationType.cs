using JetBrains.Annotations;

namespace UICompositionAnimations.Enums
{
    /// <summary>
    /// Indicates the type of an implicit composition animation
    /// </summary>
    [PublicAPI]
    public enum AnimationType
    {
        /// <summary>
        /// The animation plays when the item becomes visible
        /// </summary>
        Show,

        /// <summary>
        /// The animation plays when the item is collapsed
        /// </summary>
        Hide
    }
}