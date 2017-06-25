using System;
using System.Collections.Generic;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;

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
        public CompositionBrush EffectBrush => Sprite.Brush;

        // Internal constructor
        internal AttachedStaticCompositionEffect([NotNull] T element, [NotNull] SpriteVisual sprite, bool disposeOnUnload)
        {
            // Store the parameters
            Element = element;
            Sprite = sprite;
            if (disposeOnUnload) element.Unloaded += (s, e) => Dispose();
        }

        /// <summary>
        /// Gets a sequence of all the animated properties for the current instance
        /// </summary>
        protected virtual IEnumerable<String> GetAnimatedProperties() => new[] { "Size" };

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
                foreach (String property in GetAnimatedProperties()) Sprite.StopAnimation(property);
                ElementCompositionPreview.SetElementChildVisual(Element, null);
                EffectBrush.Dispose();
                Sprite.Dispose();
            }
            catch
            {
                // Never trust the framework
            }
        }
    }
}
