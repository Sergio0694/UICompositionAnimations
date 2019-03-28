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
        /// The list of <see cref="Action"/> instances used to create the animations to run
        /// </summary>
        [NotNull, ItemNotNull]
        private readonly IList<Action<TimeSpan>> Animations = new List<Action<TimeSpan>>();

        public CompositionAnimationBuilder([NotNull] UIElement target) : base(target)
        {
            TargetVisual = target.GetVisual();
            ElementCompositionPreview.SetIsTranslationEnabled(TargetElement, true);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Opacity(float to, Easing ease = Easing.Linear)
        {
            float from = TargetVisual.Opacity;
            return Opacity(from, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Opacity(float from, float to, Easing ease = Easing.Linear)
        {
            Animations.Add(duration =>
            {
                TargetVisual.StopAnimation(nameof(Visual.Opacity));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation(from, to, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.Opacity), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Axis axis, float to, Easing ease = Easing.Linear)
        {
            Vector3 translation = TargetVisual.TransformMatrix.Translation;
            if (axis == Axis.X) translation.X = to;
            else translation.Y = to;
            return Translation(new Vector2(translation.X, translation.Y), ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Axis axis, float from, float to, Easing ease = Easing.Linear)
        {
            Vector3 translation = TargetVisual.TransformMatrix.Translation;
            return axis == Axis.X
                ? Translation(new Vector2(from, translation.Y), new Vector2(to, translation.Y), ease)
                : Translation(new Vector2(translation.X, from), new Vector2(translation.X, to), ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Vector2 to, Easing ease = Easing.Linear)
        {
            Vector3 translation = TargetVisual.TransformMatrix.Translation;
            return Translation(new Vector2(translation.X, translation.Y), to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Vector2 from, Vector2 to, Easing ease = Easing.Linear)
        {
            Animations.Add(duration =>
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
                TargetVisual.StartAnimation("Translation", animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Offset(Axis axis, float to, Easing ease = Easing.Linear)
        {
            Vector3 offset = TargetVisual.Offset;
            if (axis == Axis.X) offset.X = to;
            else offset.Y = to;
            return Offset(new Vector2(offset.X, offset.Y), ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Offset(Axis axis, float from, float to, Easing ease = Easing.Linear)
        {
            Vector3 offset = TargetVisual.Offset;
            return axis == Axis.X
                ? Offset(new Vector2(from, offset.Y), new Vector2(to, offset.Y), ease)
                : Offset(new Vector2(offset.X, from), new Vector2(offset.X, to), ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Offset(Vector2 to, Easing ease = Easing.Linear)
        {
            Vector3 offset = TargetVisual.Offset;
            return Offset(new Vector2(offset.X, offset.Y), to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Offset(Vector2 from, Vector2 to, Easing ease = Easing.Linear)
        {
            Animations.Add(duration =>
            {
                // Stop the animation and get the easing function
                TargetVisual.StopAnimation(nameof(Visual.Offset));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Get the starting and target vectors
                Vector3
                    offset = TargetVisual.Offset,
                    from3 = new Vector3(from.X, from.Y, offset.Z),
                    to3 = new Vector3(to.X, to.Y, offset.Z);

                // Create and return the animation
                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from3, to3, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.Offset), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Scale(float to, Easing ease = Easing.Linear)
        {
            Vector3 scale = TargetVisual.Scale;
            return Scale(scale.X, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Scale(float from, float to, Easing ease = Easing.Linear)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The scale animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the scale animation
            Animations.Add(duration =>
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
                TargetVisual.StartAnimation(nameof(Visual.Scale), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Rotate(float to, Easing ease = Easing.Linear)
        {
            return Rotate(TargetVisual.RotationAngleInDegrees, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Rotate(float from, float to, Easing ease = Easing.Linear)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The rotation animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the rotation animation
            Animations.Add(duration =>
            {
                TargetVisual.StopAnimation(nameof(Visual.RotationAngleInDegrees));
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation(from, to, duration, null, easingFunction);
                TargetVisual.StartAnimation(nameof(Visual.RotationAngleInDegrees), animation);
            });

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Clip(Side side, float to, Easing ease = Easing.Linear)
        {
            InsetClip clip = TargetVisual.Clip as InsetClip ?? (TargetVisual.Clip = TargetVisual.Compositor.CreateInsetClip()).To<InsetClip>();
            float from;
            switch (side)
            {
                case Side.Top: from = clip.TopInset; break;
                case Side.Bottom: from = clip.BottomInset; break;
                case Side.Right: from = clip.RightInset; break;
                case Side.Left: from = clip.LeftInset; break;
                default: throw new ArgumentException("Invalid side", nameof(side));
            }

            return Clip(side, from, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Clip(Side side, float from, float to, Easing ease = Easing.Linear)
        {
            Animations.Add(duration =>
            {
                // Stop the animation and get the easing function
                string property;
                switch (side)
                {
                    case Side.Top: property = nameof(InsetClip.TopInset); break;
                    case Side.Bottom: property = nameof(InsetClip.BottomInset); break;
                    case Side.Right: property = nameof(InsetClip.RightInset); break;
                    case Side.Left: property = nameof(InsetClip.LeftInset); break;
                    default: throw new ArgumentException("Invalid side", nameof(side));
                }
                InsetClip clip = TargetVisual.Clip as InsetClip ?? (TargetVisual.Clip = TargetVisual.Compositor.CreateInsetClip()).To<InsetClip>();
                clip.StopAnimation(property);
                CompositionEasingFunction easingFunction = clip.GetEasingFunction(ease);

                // Create and return the animation
                ScalarKeyFrameAnimation animation = clip.Compositor.CreateScalarKeyFrameAnimation(from, to, duration, null, easingFunction);
                clip.StartAnimation(property, animation);
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
            foreach (var animation in Animations)
                animation(DurationInterval);
        }
    }
}
