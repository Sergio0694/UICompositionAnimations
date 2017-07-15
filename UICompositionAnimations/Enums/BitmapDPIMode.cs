namespace UICompositionAnimations.Enums
{
    /// <summary>
    /// Indicates the DPI mode to use to load an image
    /// </summary>
    public enum BitmapDPIMode
    {
        /// <summary>
        /// Uses the original DPI settings of the loaded image
        /// </summary>
        UseSourceDPI,

        /// <summary>
        /// Uses the default value of 96 DPI
        /// </summary>
        Default96DPI,

        /// <summary>
        /// Overrides the image DPI settings with the current screen DPI value
        /// </summary>
        CopyDisplayDPISetting,

        /// <summary>
        /// Overrides the image DPI settings with the current screen DPI value and ensures the resulting value is at least 96
        /// </summary>
        CopyDisplayDPISettingsWith96AsLowerBound
    }
}