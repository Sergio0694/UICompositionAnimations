using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours.Effects.Base;
using UICompositionAnimations.Behaviours.Misc;

namespace UICompositionAnimations.Behaviours.Effects
{
    /// <summary>
    /// An attached composition effect that supports multiple in/out animations
    /// </summary>
    /// <typeparam name="T">The type of the visual element the effects will be applied to</typeparam>
    public sealed class AttachedCompositeAnimatableCompositionEffect<T> : AttachedAnimatableCompositionEffectBase<T> where T : FrameworkElement
    {
        // Private animations parameters
        [NotNull]
        private readonly IDictionary<String, Tuple<float, float>> PropertiesAnimationValues;

        // Internal constructor
        internal AttachedCompositeAnimatableCompositionEffect(
            [NotNull] T element, [NotNull] SpriteVisual sprite, [NotNull] CompositionEffectBrush effectBrush,
            [NotNull] IDictionary<String, Tuple<float, float>> propertyValues, bool disposeOnUnload) : base(element, sprite, effectBrush, disposeOnUnload)
        {
            PropertiesAnimationValues = propertyValues;
        }

        /// <summary>
        /// Executes the animation to the desired destination status and returns a task that completes when the animation ends
        /// </summary>
        /// <param name="animationType">The target animation status</param>
        /// <param name="duration">The animation duration</param>
        public override Task AnimateAsync(FixedAnimationType animationType, TimeSpan duration)
        {
            // Apply all the animations in parallel and wait for their completion
            return Task.WhenAll(
                from pair in PropertiesAnimationValues
                let target = animationType == FixedAnimationType.In ? pair.Value.Item1 : pair.Value.Item2
                select EffectBrush.StartAnimationAsync(pair.Key, target, duration));
        }
    }
}