using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;

namespace UICompositionAnimations.Helpers
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
    }
}
