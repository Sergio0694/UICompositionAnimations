namespace System
{
    /// <summary>
    /// An empty <see langword="class"/> that represents the F# unit type
    /// </summary>
    public sealed class Unit : IEquatable<Unit>, IComparable<Unit>
    {
        // Private constructor
        private Unit() { }

        /// <summary>
        /// Gets the default <see cref="Unit"/> value
        /// </summary>
        public static Unit Value { get; } = new Unit();

        /// <inheritdoc/>
        public bool Equals(Unit other) => other != null;

        /// <inheritdoc/>
        public override int GetHashCode() => 0;

        /// <inheritdoc/>
        public int CompareTo(Unit other) => 0;
    }
}
