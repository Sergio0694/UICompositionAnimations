using System;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours.Misc;

namespace UICompositionAnimations.Behaviours.Effects.Base
{
    /// <summary>
    /// An base class for an attached composition effect that supports a ready to use animation with fixed states
    /// </summary>
    /// <typeparam name="T">Tye type of the target element the animation will be applied to</typeparam>
    public abstract class AttachedAnimatableCompositionEffectBase<T> : AttachedStaticCompositionEffect<T> where T : FrameworkElement
    {
        // Protected constructor for the implementations
        internal AttachedAnimatableCompositionEffectBase([NotNull] T element, [NotNull] SpriteVisual sprite, bool disposeOnUnload)
            : base(element, sprite, disposeOnUnload) { }

        /// <summary>
        /// Executes the animation to the desired destination status and returns a task that completes when the animation ends
        /// </summary>
        /// <param name="animationType">The target animation status</param>
        /// <param name="duration">The animation duration</param>
        public abstract Task AnimateAsync(FixedAnimationType animationType, TimeSpan duration);
    }
}