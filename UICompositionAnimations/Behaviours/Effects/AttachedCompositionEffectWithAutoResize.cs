using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours.Effects.Base;

namespace UICompositionAnimations.Behaviours.Effects
{
    /// <summary>
    /// An attached effect that automatically resizes to follow its target visual element
    /// </summary>
    /// <typeparam name="T">The type of the element the effect will be applied to</typeparam>
    public sealed class AttachedCompositionEffectWithAutoResize<T> : AttachedStaticCompositionEffect<T> where T : FrameworkElement
    {
        // Size changed event to resize the blur sprite
        [NotNull]
        private readonly SizeChangedEventHandler SizeHandler;

        // Internal constructor that also sets the event handler up
        internal AttachedCompositionEffectWithAutoResize([NotNull] T element, [NotNull] SpriteVisual sprite, [NotNull] CompositionEffectBrush effectBrush)
            : base(element, sprite, effectBrush)
        {
            // Handle the events
            SizeHandler = (s, e) =>
            {
                Sprite.Size = new Vector2((float)Element.ActualWidth, (float)Element.ActualHeight);
            };
            element.SizeChanged += SizeHandler;
        }

        /// <summary>
        /// Disposes the control and removes the resize handler
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            try
            {
                Element.SizeChanged -= SizeHandler;

            }
            catch
            {
                // Why would you call this twice?
            }
        }
    }
}
