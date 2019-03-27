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
            if (axis == TranslationAxis.X) translation.X = to;
            else translation.Y = to;
            return Translation(new Vector2(translation.X, translation.Y), ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(TranslationAxis axis, float from, float to, EasingFunctionNames ease)
        {
            Vector3 translation = TargetVisual.TransformMatrix.Translation;
            return axis == TranslationAxis.X
                ? Translation(new Vector2(from, translation.Y), new Vector2(to, translation.Y), ease)
                : Translation(new Vector2(translation.X, from), new Vector2(translation.X, to), ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Scale(float to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            Vector3 scale = TargetVisual.Scale;
            return Scale(scale.X, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Scale(float from, float to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The scale animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the scale animation
            AnimationProducers.Add(duration =>
            {
                // Stop the animation and get the easing function
                TargetVisual.StopAnimation(nameof(Visual.Scale));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Get the starting and target vectors
                Vector3
                    scale = TargetVisual.Scale,
                    from3 = new Vector3(from, from, scale.Z),
                    to3 = new Vector3(to, to, scale.Z);

                // Create and return the animation
                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from3, to3, duration, null, easingFunction);
                return (nameof(Visual.Scale), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            Vector3 translation = TargetVisual.TransformMatrix.Translation;
            return Translation(new Vector2(translation.X, translation.Y), to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Vector2 from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            AnimationProducers.Add(duration =>
            {
                // Stop the animation and get the easing function
                TargetVisual.StopAnimation("Translation");
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Get the starting and target vectors
                Vector3
                    translation = TargetVisual.TransformMatrix.Translation,
                    from3 = new Vector3(from.X, from.Y, translation.Z),
                    to3 = new Vector3(to.X, to.Y, translation.Z);

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
                var (property, animation) = producer(DurationInterval);
                TargetVisual.StartAnimation(property, animation);
            }
        }
    }
}
