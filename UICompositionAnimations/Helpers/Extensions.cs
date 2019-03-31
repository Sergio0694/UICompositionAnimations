using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using JetBrains.Annotations;

namespace UICompositionAnimationsLegacy.Helpers
{
    /// <summary>
    /// A misc extensions class
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Safely calls the Equals method on a given object, returning false if the object is null
        /// </summary>
        /// <typeparam name="T">The Type of the two object</typeparam>
        /// <param name="value">The first object to test</param>
        /// <param name="test">The comparison value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SafeEquals<T>(this T value, T test) => value?.Equals(test) == true;

        /// <summary>
        /// Performs a direct cast on the given object
        /// </summary>
        public static T To<T>(this object o) => (T)o;

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="radians">The value to convert</param>
        public static float ToDegrees(this float radians) => (float)(Math.PI * radians / 180.0);

        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="degrees">The value to convert</param>
        public static float ToRadians(this float degrees) => (float)(Math.PI / 180 * degrees);

        /// <summary>
        /// Suppresses the warnings when calling an async method without awaiting it
        /// </summary>
        /// <param name="action">The IAsyncAction returned by the async call</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this IAsyncAction action) { }

        /// <summary>
        /// Suppresses the warnings when calling an async method without awaiting it
        /// </summary>
        /// <param name="task">The task returned by the async call</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this Task task) { }

        /// <summary>
        /// Merges the two input <see cref="IReadOnlyDictionary{TKey,TValue}"/> instances and makes sure no duplicate keys are present
        /// </summary>
        /// <param name="a">The first <see cref="IReadOnlyDictionary{TKey,TValue}"/> to merge</param>
        /// <param name="b">The second <see cref="IReadOnlyDictionary{TKey,TValue}"/> to merge</param>
        [Pure, NotNull]
        public static IReadOnlyDictionary<TKey, TValue> Merge<TKey, TValue>(
            [NotNull] this IReadOnlyDictionary<TKey, TValue> a,
            [NotNull] IReadOnlyDictionary<TKey, TValue> b)
        {
            if (a.Keys.FirstOrDefault(b.ContainsKey) is TKey key)
                throw new InvalidOperationException($"The key {key} already exists in the current pipeline");
            return a.Concat(b).ToDictionary(item => item.Key, item => item.Value);
        }

        /// <summary>
        /// Merges the two input <see cref="IReadOnlyCollection{T}"/> instances and makes sure no duplicate items are present
        /// </summary>
        /// <param name="a">The first <see cref="IReadOnlyCollection{T}"/> to merge</param>
        /// <param name="b">The second <see cref="IReadOnlyCollection{T}"/> to merge</param>
        [Pure, NotNull, ItemNotNull]
        public static IReadOnlyCollection<T> Merge<T>([NotNull, ItemNotNull] this IReadOnlyCollection<T> a, [NotNull, ItemNotNull] IReadOnlyCollection<T> b)
        {
            if (a.FirstOrDefault(b.Contains) is T animation)
                throw new InvalidOperationException($"The animation {animation} already exists in the current pipeline");
            return a.Concat(b).ToArray();
        }
    }
}
