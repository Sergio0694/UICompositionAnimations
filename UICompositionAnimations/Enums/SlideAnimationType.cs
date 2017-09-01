namespace UICompositionAnimations.Enums
{
    /// <summary>
    /// Indicates the type of slide animation to apply to a <see cref="Windows.UI.Composition.Visual"/> element
    /// </summary>
    public enum SlideAnimationType
    {
        /// <summary>
        /// The element will be animated using its offset property
        /// </summary>
        Offset,

        /// <summary>
        /// The element will be animated through its render transform property
        /// </summary>
        RenderTransform
    }
}