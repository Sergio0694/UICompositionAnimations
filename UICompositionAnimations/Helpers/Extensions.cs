using System.Runtime.CompilerServices;
using Windows.Foundation;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A misc extensions class
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Suppresses the warnings when calling an async method without awaiting it
        /// </summary>
        /// <param name="action">The IAsyncAction returned by the async call</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Forget(this IAsyncAction action) { }
    }
}
