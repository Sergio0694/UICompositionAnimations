using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Composition;
using Windows.UI.Core;
using JetBrains.Annotations;

namespace UICompositionAnimations.Brushes.Cache
{
    /// <summary>
    /// A <see langword="class"/> used to cache reusable <see cref="CompositionObject"/> instances with an associated key
    /// </summary>
    /// <typeparam name="TKey">The type of key to classify the items in the cache</typeparam>
    /// <typeparam name="TValue">The type of items stored in the cache</typeparam>
    internal sealed class ThreadSafeCompositionMapCache<TKey, TValue> where TValue : CompositionObject
    {
        /// <summary>
        /// The cache of weak references, to avoid memory leaks
        /// </summary>
        [NotNull]
        private readonly Dictionary<TKey, List<WeakReference<TValue>>> Cache = new Dictionary<TKey, List<WeakReference<TValue>>>();

        /// <summary>
        /// Tries to retrieve a valid instance from the cache, and uses the provided factory if an existing item is not found
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <param name="result">The resulting value, if existing</param>
        [MustUseReturnValue]
        public bool TryGetInstance(TKey key, out TValue result)
        {
            // Try to retrieve an valid instance from the cache
            if (Cache.TryGetValue(key, out List<WeakReference<TValue>> values))
                foreach (WeakReference<TValue> value in values)
                    if (value.TryGetTarget(out TValue instance) && instance.TryGetDispatcher(out CoreDispatcher dispatcher) && dispatcher.HasThreadAccess)
                    {
                        result = instance;
                        return true;
                    }

            // Not found
            result = null;
            return false;
        }

        /// <summary>
        /// Adds a new value with the specified key to the cache
        /// </summary>
        /// <param name="key">The key of the item to add</param>
        /// <param name="value">The value to add</param>
        public void Add(TKey key, TValue value)
        {
            if (Cache.TryGetValue(key, out List<WeakReference<TValue>> list)) list.Add(new WeakReference<TValue>(value));
            else Cache.Add(key, new List<WeakReference<TValue>> { new WeakReference<TValue>(value) });
        }

        /// <summary>
        /// Adds a new value and removes previous values with the same key, if any
        /// </summary>
        /// <param name="key">The key of the item to add</param>
        /// <param name="value">The value to add</param>
        public void Overwrite(TKey key, TValue value)
        {
            Cache.Remove(key);
            Cache.Add(key, new List<WeakReference<TValue>> { new WeakReference<TValue>(value) });
        }

        /// <summary>
        /// Performs a cleanup of the cache by removing invalid references
        /// </summary>
        public void Cleanup()
        {
            foreach (List<WeakReference<TValue>> list in Cache.Values)
                list.RemoveAll(reference => !reference.TryGetTarget(out TValue value) || !value.TryGetDispatcher(out _));
            foreach (TKey key in Cache.Keys.ToArray())
                if (Cache[key].Count == 0)
                    Cache.Remove(key);
        }

        /// <summary>
        /// Clears the cache by removing all the stored items
        /// </summary>
        public IReadOnlyList<TValue> Clear()
        {
            List<TValue> values = new List<TValue>();
            foreach (WeakReference<TValue> reference in Cache.Values.SelectMany(list => list))
                if (reference.TryGetTarget(out TValue value))
                    values.Add(value);
            Cache.Clear();
            return values;
        }
    }
}