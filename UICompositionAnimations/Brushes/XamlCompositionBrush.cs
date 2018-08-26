using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours;
using UICompositionAnimations.Brushes.Base;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A <see langword="delegate"/> that represents a custom effect animation that can be applied to a <see cref="XamlCompositionBrush"/> instance
    /// </summary>
    /// <param name="value">The animation target value</param>
    /// <param name="ms">The animation duration, in milliseconds</param>
    public delegate Task XamlEffectAnimation(float value, int ms);

    /// <summary>
    /// A simple <see langword="class"/> that can be used to quickly create XAML brushes from arbitrary <see cref="CompositionBrushBuilder"/> pipelines
    /// </summary>
    public sealed class XamlCompositionBrush : XamlCompositionEffectBrushBase
    {
        /// <summary>
        /// Gets the <see cref="CompositionBrushBuilder"/> pipeline for the current instance
        /// </summary>
        [NotNull]
        public CompositionBrushBuilder Pipeline { get; }

        /// <summary>
        /// Creates a new XAML brush from the input effects pipeline
        /// </summary>
        /// <param name="pipeline">The <see cref="CompositionBrushBuilder"/> instance to create the effect</param>
        public XamlCompositionBrush([NotNull] CompositionBrushBuilder pipeline) => Pipeline = pipeline;

        /// <summary>
        /// Binds an <see cref="EffectAnimation"/> to the composition brush in the current instance
        /// </summary>
        /// <param name="animation">The input animation</param>
        /// <param name="bound">The resulting animation</param>
        [Pure, NotNull]
        public XamlCompositionBrush Bind([NotNull] EffectAnimation animation, out XamlEffectAnimation bound)
        {
            bound = (value, ms) => animation(CompositionBrush, value, ms);
            return this;
        }

        /// <inheritdoc cref="XamlCompositionEffectBrushBase"/>
        protected override CompositionBrushBuilder OnBrushRequested() => Pipeline;

        /// <summary>
        /// Clones the current instance by rebuilding the source <see cref="Windows.UI.Xaml.Media.Brush"/>. Use this method to reuse the same effects pipeline on a different <see cref="Windows.UI.Core.CoreDispatcher"/>
        /// </summary>
        [PublicAPI, Pure, NotNull]
        public XamlCompositionBrush Clone()
        {
            if (Dispatcher.HasThreadAccess)
            {
                throw new InvalidOperationException("The current thread already has access to the brush dispatcher, so a clone operation is not necessary. " +
                                                    "You can just assign this brush to an arbitrary number of controls and it will still work correctly. " +
                                                    "This method is only meant to be used to create a new instance of this brush using the same pipeline, " +
                                                    "on threads that can't access the current instance, for example in secondary app windows.");
            }
            return new XamlCompositionBrush(Pipeline);
        }
    }
}