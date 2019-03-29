using System;
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
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Animations
{
    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IAnimationBuilder"/> <see langword="interface"/> using composition APIs
    /// </summary>
    internal sealed class CompositionAnimationBuilder : AnimationBuilderBase<Action<TimeSpan>>
    {
        /// <summary>
        /// The target <see cref="Visual"/> to animate
        /// </summary>
        [NotNull]
        private readonly Visual TargetVisual;

        public CompositionAnimationBuilder([NotNull] UIElement target) : base(target)
        {
            TargetVisual = target.GetVisual();
            ElementCompositionPreview.SetIsTranslationEnabled(TargetElement, true);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOpacity(double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration =>
            {
                TargetVisual.StopAnimation(nameof(Visual.Opacity));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.Opacity), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(TranslationAxis axis, double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the EasingFunctionNames function
                string property = $"Translation.{axis}";
                TargetVisual.StopAnimation(property);
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Create and return the animation
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                TargetVisual.StartAnimation(property, animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the EasingFunctionNames function
                TargetVisual.StopAnimation("Translation");
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Create and return the animation
                Vector3? from3 = from == null ? default : new Vector3(from.Value, 0);
                Vector3 to3 = new Vector3(to, 0);
                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from3, to3, duration, null, easingFunction);
                TargetVisual.StartAnimation("Translation", animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(TranslationAxis axis, double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the EasingFunctionNames function
                TargetVisual.StopAnimation(nameof(Visual.Offset));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Create and return the animation
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.Offset), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the EasingFunctionNames function
                TargetVisual.StopAnimation(nameof(Visual.Offset));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Create and return the animation
                Vector3? from3 = from == null ? default : new Vector3(from.Value, 0);
                Vector3 to3 = new Vector3(to, 0);
                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from3, to3, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.Offset), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(double? from, double to, EasingFunctionNames ease)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The scale animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the scale animation
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the EasingFunctionNames function
                TargetVisual.StopAnimation(nameof(Visual.Scale));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Create and return the animation
                Vector3? from3 = from == null ? default : new Vector3((float)from.Value, (float)from.Value, 0);
                Vector3 to3 = new Vector3((float)to, (float)to, 0);
                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from3, to3, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.Scale), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnRotate(double? from, double to, EasingFunctionNames ease)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The rotation animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the rotation animation
            AnimationFactories.Add(duration =>
            {
                TargetVisual.StopAnimation(nameof(Visual.RotationAngleInDegrees));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.RotationAngleInDegrees), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnClip(MarginSide side, double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the EasingFunctionNames function
                string property;
                switch (side)
                {
                    case MarginSide.Top: property = nameof(InsetClip.TopInset); break;
                    case MarginSide.Bottom: property = nameof(InsetClip.BottomInset); break;
                    case MarginSide.Right: property = nameof(InsetClip.RightInset); break;
                    case MarginSide.Left: property = nameof(InsetClip.LeftInset); break;
                    default: throw new ArgumentException("Invalid side", nameof(side));
                }
                InsetClip clip = TargetVisual.Clip as InsetClip ?? (TargetVisual.Clip = TargetVisual.Compositor.CreateInsetClip()).To<InsetClip>();
                clip.StopAnimation(property);
                CompositionEasingFunction easingFunction = clip.GetEasingFunction(ease);

                // Create and return the animation
                ScalarKeyFrameAnimation animation = clip.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                clip.StartAnimation(property, animation);
            });

            return this;
        }

        /// <summary>
        /// Creates and starts the pending animations
        /// </summary>
        private void StartAnimations()
        {
            foreach (var animation in AnimationFactories)
                animation(DurationInterval);
        }

        /// <inheritdoc/>
        protected override void OnStart() => StartAnimations();

        /// <inheritdoc/>
        protected override Task OnStartAsync()
        {
            CompositionScopedBatch batch = TargetVisual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            batch.Completed += (s, e) => tcs.SetResult(null);
            StartAnimations();
            batch.End();
            return tcs.Task;
        }
    }
}
