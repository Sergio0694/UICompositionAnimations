using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using UICompositionAnimations.Animations.Abstract;
using UICompositionAnimations.Animations.Interfaces;
using UICompositionAnimations.Composition;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations
{
    /// <summary>
    /// A <see langword="delegate"/> that prepares composition animations with a specified duration
    /// </summary>
    /// <param name="duration">The duration of the animation returned by the <see langword="delegate"/></param>
    internal delegate (string Property, CompositionAnimation Animation) CompositionAnimationProducer(TimeSpan duration);

    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IAnimationBuilder"/> <see langword="interface"/> using composition APIs
    /// </summary>
    internal sealed class CompositionAnimationBuilder : AnimationBuilderBase
    {
        /// <summary>
        /// The target <see cref="Visual"/> to animate
        /// </summary>
        [NotNull]
        private readonly Visual TargetVisual;

        /// <summary>
        /// The list of <see cref="CompositionAnimationProducer"/> instances used to create the animations to run
        /// </summary>
        [NotNull, ItemNotNull]
        private readonly IList<CompositionAnimationProducer> AnimationProducers = new List<CompositionAnimationProducer>();

        public CompositionAnimationBuilder([NotNull] UIElement target) : base(target)
        {
            TargetVisual = target.GetVisual();
            ElementCompositionPreview.SetIsTranslationEnabled(TargetElement, true);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Opacity(float to, EasingFunctionNames ease)
        {
            float from = TargetVisual.Opacity;
            return Opacity(from, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Opacity(float from, float to, EasingFunctionNames ease)
        {
            AnimationProducers.Add(duration =>
            {
                TargetVisual.StopAnimation(nameof(Visual.Opacity));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation(from, to, duration, null, easingFunction);
                return (nameof(Visual.Opacity), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(TranslationAxis axis, float to, EasingFunctionNames ease)
        {
            Vector3 translation = TargetVisual.TransformMatrix.Translation;
            float from = axis == TranslationAxis.X ? translation.X : translation.Y;
            return Translation(axis, from, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(TranslationAxis axis, float from, float to, EasingFunctionNames ease)
        {
            AnimationProducers.Add(duration =>
            {
                // Stop the animation and get the easing function
                TargetVisual.StopAnimation("Translation");
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Get the starting and target vectors
                Vector3
                    from3 = TargetVisual.TransformMatrix.Translation,
                    to3 = from3;
                if (axis == TranslationAxis.X) to3.X = to;
                else to3.Y = to;

                // Create and return the animation
                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from3, to3, duration, null, easingFunction);
                return ("Translation", animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override void OnStart() => StartAnimations();

        /// <inheritdoc/>
        protected override Task OnStartAsync()
        {
            CompositionScopedBatch batch = TargetVisual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource tcs = new TaskCompletionSource();
            batch.Completed += (s, e) => tcs.SetResult(null);
            StartAnimations();
            batch.End();
            return tcs.Task;
        }

        /// <summary>
        /// Creates and starts the pending animations
        /// </summary>
        private void StartAnimations()
        {
            foreach (var producer in AnimationProducers)
            {
                var info = producer(DurationInterval);
                TargetVisual.StartAnimation(info.Property, info.Animation);
            }
        }
    }
}
