using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Behaviours.Effects
{
    /// <summary>
    /// An base class for an attached composition effect
    /// </summary>
    /// <typeparam name="T">Tye type of the target element</typeparam>
    public class AttachedStaticCompositionEffect<T> : IDisposable where T : FrameworkElement
    {
        /// <summary>
        /// Gets the element used to apply the blur effect
        /// </summary>
        [NotNull]
        public T Element { get; }

        /// <summary>
        /// Gets the actual blur sprite shown above the visual element
        /// </summary>
        [NotNull]
        public SpriteVisual Sprite { get; }

        /// <summary>
        /// Gets the composition effect brush applied to the visual element
        /// </summary>
        [NotNull]
        public CompositionEffectBrush EffectBrush => Sprite.Brush.To<CompositionEffectBrush>();

        // Internal constructor
        internal AttachedStaticCompositionEffect([NotNull] T element, [NotNull] SpriteVisual sprite, bool disposeOnUnload)
        {
            // Store the parameters
            Element = element;
            Sprite = sprite;
            if (disposeOnUnload) element.Unloaded += (s, e) => Dispose();
        }

        /// <summary>
        /// Disposes the resources in the current instance
        /// </summary>
        protected virtual void DisposeCore()
        {
            Sprite.StopAnimation("Size");
            ElementCompositionPreview.SetElementChildVisual(Element, null);
            EffectBrush.Dispose();
            Sprite.Dispose();
        }

        // Indicates whether or not the wrapped effect has already been disposed
        private bool _Disposed;

        /// <summary>
        /// Stops the size animation, removes the effect from the visual tree and disposes it
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;
            try
            {
                DisposeCore();
            }
            catch
            {
                // Never trust the framework
            }
        }
    }
}
