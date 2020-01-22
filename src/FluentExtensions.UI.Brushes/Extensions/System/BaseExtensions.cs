using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

#nullable enable

namespace System
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="System"/> <see langword="namespace"/>
    /// </summary>
    public static class BaseExtensions
    {
        /// <summary>
        /// Performs a direct cast on the given <see cref="object"/>
        /// </summary>
        /// <typeparam name="T">The target type to return</typeparam>
        /// <param name="o">The input <see cref="object"/> to cast</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static T To<T>(this object o) => (T)o;

        /// <summary>
        /// Performs a safe cast to the specified type
        /// </summary>
        /// <typeparam name="TTo">The target type to return, if possible</typeparam>
        /// <param name="o">The input <see cref="object"/> to try to convert</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static TTo? As<TTo>(this object? o) where TTo : class => o as TTo;

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="radians">The value to convert</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static float ToDegrees(this float radians) => (float)(Math.PI * radians / 180.0);

        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="degrees">The value to convert</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static float ToRadians(this float degrees) => (float)(Math.PI / 180 * degrees);
    }
}
