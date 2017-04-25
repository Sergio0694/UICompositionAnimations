using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours.Misc;

namespace UICompositionAnimations.Behaviours.Effects.Base
{
    /// <summary>
    /// An base class for an attached composition effect
    /// </summary>
    /// <typeparam name="T">Tye type of the target element</typeparam>
    public class AttachedStaticCompositionEffect<T> : IDisposable, IAttachedCompositionEffect where T : FrameworkElement
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
        public CompositionBrush EffectBrush { get; }

        // Internal constructor
        internal AttachedStaticCompositionEffect([NotNull] T element, [NotNull] SpriteVisual sprite, [NotNull] CompositionBrush effectBrush)
        {
            // Store the parameters
            Element = element;
            Sprite = sprite;
            EffectBrush = effectBrush;
        }

        /// <summary>
        /// Adjusts the effect size with the actual size of the parent UI element
        /// </summary>
        public void AdjustSize() => Sprite.Size = new Vector2((float)Element.ActualWidth, (float)Element.ActualHeight);

        /// <summary>
        /// Adjusts the effect size
        /// </summary>
        /// <param name="width">The desired width for the effect sprite</param>
        /// <param name="height">The desired height for the effect sprite</param>
        public void AdjustSize(double width, double height) => Sprite.Size = new Vector2((float)width, (float)height);

        /// <summary>
        /// Helps the GC by freeing up some resources that are no longer necessary
        /// </summary>
        public virtual void Dispose() => ElementCompositionPreview.SetElementChildVisual(Element, null);
    }
}
