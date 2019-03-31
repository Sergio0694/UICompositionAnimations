namespace UICompositionAnimationsLegacy.Composition.Misc
{
    /// <summary>
    /// A simple struct that keeps track of the initial values for a rotation animation
    /// </summary>
    internal struct CompositeRotationAnimationStartInfo
    {
        /// <summary>
        /// Gets the initial animation opacity
        /// </summary>
        public float Opacity { get; }

        /// <summary>
        /// Gets the initial value of the secondary animation parameter (either offset or scale)
        /// </summary>
        public float SecondaryProperty { get; }

        /// <summary>
        /// Gets the initial animation degrees property
        /// </summary>
        public float Degrees { get; }
        
        public CompositeRotationAnimationStartInfo(float opacity, float property, float degrees)
        {
            Opacity = opacity;
            SecondaryProperty = property;
            Degrees = degrees;
        }
    }
}