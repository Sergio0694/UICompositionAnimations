using Windows.Graphics.Effects;
using Windows.UI.Composition;
using JetBrains.Annotations;

namespace UICompositionAnimations.Brushes.Cache
{
    /// <summary>
    /// A simple class that holds information on a <see cref="CompositionBackdropBrush"/> instance and its effects pipeline
    /// </summary>
    internal sealed class HostBackdropInstanceWrapper
    {
        /// <summary>
        /// Gets the partial pipeline with the host backdrop effect
        /// </summary>
        [NotNull]
        public IGraphicsEffectSource Pipeline { get; }

        /// <summary>
        /// Gets the host backdrop effect brush instance
        /// </summary>
        [NotNull]
        public CompositionBackdropBrush Brush { get; }

        /// <summary>
        /// Creates a new wrapper instance with the given parameters
        /// </summary>
        /// <param name="pipeline">The current effects pipeline</param>
        /// <param name="brush">The host backdrop brush instance</param>
        public HostBackdropInstanceWrapper([NotNull] IGraphicsEffectSource pipeline, [NotNull] CompositionBackdropBrush brush)
        {
            Pipeline = pipeline;
            Brush = brush;
        }
    }
}
