namespace UICompositionAnimations.Helpers.PointerEvents
{
    /// <summary>
    /// Indicates a result when trying to remove some pointer handlers from a target control
    /// </summary>
    public enum HandlersRemovalResult
    {
        /// <summary>
        /// The handlers were successfully removed
        /// </summary>
        Success,

        /// <summary>
        /// The handlers had already been removed correctly
        /// </summary>
        AlreadyRemoved,

        /// <summary>
        /// The target object has already been garbage collected
        /// </summary>
        ObjectFinalized,

        /// <summary>
        /// The target object doesn't have any known pointer handlers
        /// </summary>
        NoHandlersRegistered,

        /// <summary>
        /// There was an error while removing the pointer handlers
        /// </summary>
        UnknownError
    }
}