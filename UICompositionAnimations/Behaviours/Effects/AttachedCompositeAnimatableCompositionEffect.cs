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
        private readonly IDictionary<String, CompositionAnimationValueParameters> PropertiesAnimationValues;

        // Internal constructor
        internal AttachedCompositeAnimatableCompositionEffect(
            [NotNull] T element, [NotNull] SpriteVisual sprite,
            [NotNull] IDictionary<String, CompositionAnimationValueParameters> propertyValues, bool disposeOnUnload) : base(element, sprite, disposeOnUnload)
        {
            PropertiesAnimationValues = propertyValues;
        }

        /// <inheritdoc />
        protected override IEnumerable<String> GetAnimatedProperties()
        {
            return base.GetAnimatedProperties().Concat(PropertiesAnimationValues.Keys);
        }

        /// <inheritdoc />
        public override Task AnimateAsync(FixedAnimationType animationType, TimeSpan duration)
        {
            // Apply all the animations in parallel and wait for their completion
            return Task.WhenAll(
                from pair in PropertiesAnimationValues
                let target = animationType == FixedAnimationType.In ? pair.Value.On : pair.Value.Off
                select EffectBrush.StartAnimationAsync(pair.Key, target, duration));
        }
    }
}