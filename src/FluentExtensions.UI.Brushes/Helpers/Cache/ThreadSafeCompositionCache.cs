using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Core;

namespace UICompositionAnimations.Helpers.Cache
{
    /// <summary>
    /// A <see langword="class"/> used to cache reusable <see cref="CompositionObject"/> instances
    /// </summary>
    /// <typeparam name="T">The type of <see cref="CompositionObject"/> instances to cache</typeparam>
    internal sealed class ThreadSafeCompositionCache<T> where T : CompositionObject
    {
        /// <summary>
        /// The cache of weak references, to avoid memory leaks
        /// </summary>
        private readonly List<WeakReference<T>> Cache = new List<WeakReference<T>>();

        /// <summary>
        /// The <see cref="AsyncMutex"/> instance used to synchronize concurrent operations on the cache
        /// </summary>
        private readonly AsyncMutex Mutex = new AsyncMutex();

        /// <summary>
        /// Tries to retrieve a valid instance from the cache, and uses the provided factory if an existing item is not found
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> used when the requested value is not present in the cache</param>
        public async Task<T> TryGetInstanceAsync(Func<T> factory)
        {
            using (await Mutex.LockAsync())
            {
                // Try to retrieve an valid instance from the cache
                foreach (WeakReference<T> value in Cache)
                    if (value.TryGetTarget(out T instance) && instance.TryGetDispatcher(out CoreDispatcher dispatcher) && dispatcher.HasThreadAccess)
                        return instance;

                // Create a new instance when needed
                T fallback = factory();
                Cache.Add(new WeakReference<T>(fallback));
                return fallback;
            }
        }

        /// <summary>
        /// Performs a cleanup of the cache by removing invalid references
        /// </summary>
        public async void Cleanup()
        {
            using (await Mutex.LockAsync())
            {
                Cache.RemoveAll(reference => !reference.TryGetTarget(out T target) || !target.TryGetDispatcher(out _));
            }
        }
    }
}
