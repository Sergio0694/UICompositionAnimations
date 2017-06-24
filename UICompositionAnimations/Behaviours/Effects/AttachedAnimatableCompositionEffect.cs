using System;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours.Effects.Base;
using UICompositionAnimations.Behaviours.Misc;

namespace UICompositionAnimations.Behaviours.Effects
{
    /// <summary>
    /// An attached composition effect that supports a single in/out animation
    /// </summary>
    /// <typeparam name="T">The type of the visual element the effect will be applied to</typeparam>
    public sealed class AttachedAnimatableCompositionEffect<T> : AttachedAnimatableCompositionEffectBase<T> where T : FrameworkElement
    {
        // The animation parameters
        [NotNull]
        private readonly Tuple<String, float, float> AnimationValues;

        // Internal constructor
        internal AttachedAnimatableCompositionEffect(
            [NotNull] T element, [NotNull] SpriteVisual sprite, [NotNull] CompositionEffectBrush effectBrush,
            [NotNull] Tuple<String, float, float> animationValues, bool disposeOnUnload) : base(element, sprite, effectBrush, disposeOnUnload)
        {
            AnimationValues = animationValues;
        }

        /// <summary>
        /// Executes the animation to the desired destination status and returns a task that completes when the animation ends
        /// </summary>
        /// <param name="animationType">The target animation status</param>
        /// <param name="duration">The animation duration</param>
        public override Task AnimateAsync(FixedAnimationType animationType, TimeSpan duration)
        {
            return EffectBrush.StartAnimationAsync(AnimationValues.Item1,
                animationType == FixedAnimationType.In ? AnimationValues.Item2 : AnimationValues.Item3, duration);
        }
    }
}
