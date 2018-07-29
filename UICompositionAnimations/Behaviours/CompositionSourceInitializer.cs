using System;
using System.Threading.Tasks;
using Windows.UI.Composition;
using JetBrains.Annotations;

namespace UICompositionAnimations.Behaviours
{
    /// <summary>
    /// A simple container <see langword="class"/> used to store info on a custom composition effect to create
    /// </summary>
    [PublicAPI]
    public sealed class CompositionSourceInitializer
    {
        /// <summary>
        /// Gets the stored effect initializer
        /// </summary>
        [NotNull]
        internal Func<Task<CompositionBrush>> Initializer { get; }

        /// <summary>
        /// Gets the name of the target <see cref="CompositionEffectSourceParameter"/>
        /// </summary>
        [NotNull]
        internal string Name { get; }

        private CompositionSourceInitializer([NotNull] string name, [NotNull] Func<Task<CompositionBrush>> initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        /// <summary>
        /// Creates a new instance with the info on a given <see cref="CompositionEffectSourceParameter"/> to initialize
        /// </summary>
        /// <param name="name">The target effect name</param>
        /// <param name="initializer">A <see cref="Func{TResult}"/> instance that will produce the <see cref="CompositionBrush"/> to use to initialize the effect</param>
        [Pure, NotNull]
        public static CompositionSourceInitializer New([NotNull] string name, [NotNull] Func<CompositionBrush> initializer) => new CompositionSourceInitializer(name, () => Task.FromResult(initializer()));

        /// <summary>
        /// Creates a new instance with the info on a given <see cref="CompositionEffectSourceParameter"/> to initialize
        /// </summary>
        /// <param name="name">The target effect name</param>
        /// <param name="initializer">An asynchronous <see cref="Func{TResult}"/> instance that will produce the <see cref="CompositionBrush"/> to use to initialize the effect</param>
        [Pure, NotNull]
        public static CompositionSourceInitializer New([NotNull] string name, [NotNull] Func<Task<CompositionBrush>> initializer) => new CompositionSourceInitializer(name, initializer);
    }
}
