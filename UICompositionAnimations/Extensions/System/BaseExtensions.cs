using System.Runtime.CompilerServices;
using JetBrains.Annotations;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static T To<T>(this object o) => (T)o;

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
