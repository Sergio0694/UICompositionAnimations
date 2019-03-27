using System.Linq;
using JetBrains.Annotations;

namespace System.Collections.Generic
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="System.Collections.Generic"/> <see langword="namespace"/>
    /// </summary>
    internal static class GenericExtensions
    {
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
            if (a.Keys.FirstOrDefault(b.ContainsKey) is TKey key) throw new InvalidOperationException($"The key {key} already exists in the current pipeline");
            return new Dictionary<TKey, TValue>(a.Concat(b));
        }

        /// <summary>
        /// Merges the two input <see cref="IReadOnlyCollection{T}"/> instances and makes sure no duplicate items are present
        /// </summary>
        /// <param name="a">The first <see cref="IReadOnlyCollection{T}"/> to merge</param>
        /// <param name="b">The second <see cref="IReadOnlyCollection{T}"/> to merge</param>
        [Pure, NotNull, ItemNotNull]
        public static IReadOnlyCollection<T> Merge<T>([NotNull, ItemNotNull] this IReadOnlyCollection<T> a, [NotNull, ItemNotNull] IReadOnlyCollection<T> b)
        {
            if (a.Any(b.Contains)) throw new InvalidOperationException("The input collection has at least an item already present in the second collection");
            return a.Concat(b).ToArray();
        }
    }
}
