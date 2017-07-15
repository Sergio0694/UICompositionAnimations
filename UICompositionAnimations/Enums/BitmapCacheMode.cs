namespace UICompositionAnimations.Enums
{
    /// <summary>
    /// Indicates the cache mode to use when loading an item
    /// </summary>
    public enum BitmapCacheMode
    {
        /// <summary>
        /// The new item will be either loaded from the cache when possible, or saved in the cache for future use
        /// </summary>
        EnableCaching,

        /// <summary>
        /// The item will not be loaded from the cache, but it will be stored in the cache for future use if possible
        /// </summary>
        DisableCachingOnRead,

        /// <summary>
        /// The new item will not be loaded from the cache and it will not be saved into the cache either
        /// </summary>
        DisableCaching
    }
}