namespace UICompositionAnimations.Behaviours.Misc
{
    /// <summary>
    /// An interface for a ready to use composition effect applied to a <see cref="Windows.UI.Xaml.FrameworkElement"/>
    /// </summary>
    public interface IAttachedCompositionEffect
    {
        /// <summary>
        /// Adjusts the effect size with the actual size of the parent UI element
        /// </summary>
        void AdjustSize();

        /// <summary>
        /// Adjusts the effect size
        /// </summary>
        /// <param name="width">The desired width for the effect sprite</param>
        /// <param name="height">The desired height for the effect sprite</param>
        void AdjustSize(double width, double height);
    }
}