namespace FluentExtensions.UI.Animations.Enums
{
    /// <summary>
    /// An <see langword="enum"/> that indicates the preference when retrieving a render transform instance
    /// </summary>
    public enum TransformOption
    {
        /// <summary>
        /// Reuses the current render transform instance, if already existing
        /// </summary>
        ReuseIfExisting,

        /// <summary>
        /// Always creates a new render transform instance of the requested type
        /// </summary>
        CreateNewInstance
    }
}
