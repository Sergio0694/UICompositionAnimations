using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using FluentExtensions.UI.Animations.Abstract;
using FluentExtensions.UI.Animations.Enums;
using FluentExtensions.UI.Animations.Interfaces;

#nullable enable

namespace FluentExtensions.UI.Animations
{
    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IAnimationBuilder"/> <see langword="interface"/> using composition APIs
    /// </summary>
    internal sealed class CompositionAnimationBuilder : AnimationBuilderBase<Action<TimeSpan>>
    {
        /// <summary>
        /// The target <see cref="Visual"/> to animate
        /// </summary>
        private readonly Visual TargetVisual;

        /// <summary>
        /// Creates a new <see cref="CompositionAnimationBuilder"/> with the specified parameters
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        public CompositionAnimationBuilder(UIElement target) : base(target)
        {
            TargetVisual = target.GetVisual();

            ElementCompositionPreview.SetIsTranslationEnabled(TargetElement, true);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOpacity(double? from, double to, Easing ease)
        {
            return OnScalarAnimation(nameof(Visual.Opacity), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Axis axis, double? from, double to, Easing ease)
        {
            return OnScalarAnimation($"Translation.{axis}", from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, Easing ease)
        {
            return OnVector3Animation("Translation", from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector3? from, Vector3 to, Easing ease)
        {
            return OnVector3Animation("Translation", from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Axis axis, double? from, double to, Easing ease)
        {
            return OnScalarAnimation($"{nameof(Visual.Offset)}.{axis}", from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Vector2? from, Vector2 to, Easing ease)
        {
            return OnVector3Animation(nameof(Visual.Offset), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Vector3? from, Vector3 to, Easing ease)
        {
            return OnVector3Animation(nameof(Visual.Offset), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(double? from, double to, Easing ease)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The scale animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the scale animation
            Vector2? from2 = from == null ? default : new Vector2((float)from.Value, (float)from.Value);
            Vector2 to2 = new Vector2((float)to, (float)to);
            return OnVector3Animation(nameof(Visual.Scale), from2, to2, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(Axis axis, double? from, double to, Easing ease)
        {
            // Get the target vector
            Vector3 to3 = TargetVisual.Scale;
            switch (axis)
            {
                case Axis.X: to3.X = (float)to; break;
                case Axis.Y: to3.Y = (float)to; break;
                case Axis.Z: to3.Z = (float)to; break;
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis: {axis}");
            }

            if (from is null) return OnVector3Animation(nameof(Visual.Scale), null, to3, ease);

            // Get the starting vector
            Vector3 from3 = TargetVisual.Scale;
            switch (axis)
            {
                case Axis.X: from3.X = (float)from.Value; break;
                case Axis.Y: from3.Y = (float)from.Value; break;
                case Axis.Z: from3.Z = (float)from.Value; break;
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis: {axis}");
            }

            return OnVector3Animation(nameof(Visual.Scale), from3, to3, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(Vector2? from, Vector2 to, Easing ease)
        {
            return OnVector3Animation(nameof(Visual.Scale), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(Vector3? from, Vector3 to, Easing ease)
        {
            return OnVector3Animation(nameof(Visual.Scale), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnRotate(double? from, double to, Easing ease)
        {
            // Center the visual center point
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("The rotation animation needs a framework element");
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);

            // Add the rotation animation
            return OnScalarAnimation(nameof(Visual.RotationAngleInDegrees), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnClip(Side side, double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the easing function
                string property = side switch
                {
                    Side.Top => nameof(InsetClip.TopInset),
                    Side.Bottom => nameof(InsetClip.BottomInset),
                    Side.Right => nameof(InsetClip.RightInset),
                    Side.Left => nameof(InsetClip.LeftInset),
                    _ => throw new ArgumentException($"Invalid side: {side}", nameof(side))
                };

                InsetClip clip = TargetVisual.Clip as InsetClip ?? (InsetClip)(TargetVisual.Clip = TargetVisual.Compositor.CreateInsetClip());
                clip.StopAnimation(property);
                CompositionEasingFunction easingFunction = clip.GetEasingFunction(ease);

                // Create and return the animation
                ScalarKeyFrameAnimation animation = clip.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                clip.StartAnimation(property, animation);
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Axis axis, double? from, double to, Easing ease)
        {
            return OnScalarAnimation($"{nameof(Visual.Size)}.{axis}", from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Vector2? from, Vector2 to, Easing ease)
        {
            return OnVector3Animation(nameof(Visual.Size), from, to, ease);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Vector3? from, Vector3 to, Easing ease)
        {
            return OnVector3Animation(nameof(Visual.Size), from, to, ease);
        }

        /// <summary>
        /// Schedules a scalar animation targeting the current <see cref="Visual"/> item
        /// </summary>
        /// <param name="property">The target <see cref="Visual"/> property to animate</param>
        /// <param name="from">The optional starting value for the scalar animation</param>
        /// <param name="to">The target value for the scalar animation</param>
        /// <param name="ease">The easing function to use for the scalar animation</param>
        private IAnimationBuilder OnScalarAnimation(string property, double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the easing function
                TargetVisual.StopAnimation(property);
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                // Create and return the animation
                ScalarKeyFrameAnimation animation = TargetVisual.Compositor.CreateScalarKeyFrameAnimation((float?)from, (float)to, duration, null, easingFunction);
                TargetVisual.StartAnimation(property, animation);
            });

            return this;
        }

        /// <summary>
        /// Schedules a vector animation targeting the current <see cref="Visual"/> item
        /// </summary>
        /// <param name="property">The target <see cref="Visual"/> property to animate</param>
        /// <param name="from">The optional starting value for the vector animation</param>
        /// <param name="to">The target value for the vector animation</param>
        /// <param name="ease">The easing function to use for the vector animation</param>
        private IAnimationBuilder OnVector3Animation(string property, Vector2? from, Vector2 to, Easing ease)
        {
            Vector3? from3 = from == null ? default : new Vector3(from.Value, 0);
            Vector3 to3 = new Vector3(to, 0);

            return OnVector3Animation(property, from3, to3, ease);
        }

        /// <summary>
        /// Schedules a vector animation targeting the current <see cref="Visual"/> item
        /// </summary>
        /// <param name="property">The target <see cref="Visual"/> property to animate</param>
        /// <param name="from">The optional starting value for the vector animation</param>
        /// <param name="to">The target value for the vector animation</param>
        /// <param name="ease">The easing function to use for the vector animation</param>
        private IAnimationBuilder OnVector3Animation(string property, Vector3? from, Vector3 to, Easing ease)
        {
            AnimationFactories.Add(duration =>
            {
                // Stop the animation and get the easing function
                TargetVisual.StopAnimation(property);
                CompositionEasingFunction easingFunction = TargetVisual.GetEasingFunction(ease);

                Vector3KeyFrameAnimation animation = TargetVisual.Compositor.CreateVector3KeyFrameAnimation(from, to, duration, null, easingFunction);

                TargetVisual.StartAnimation(property, animation);
            });

            return this;
        }

        /// <summary>
        /// Creates and starts the pending animations
        /// </summary>
        private void StartAnimations()
        {
            foreach (var animation in AnimationFactories)
            {
                animation(DurationInterval);
            }
        }

        /// <inheritdoc/>
        protected override void OnStart() => StartAnimations();

        /// <inheritdoc/>
        protected override Task OnStartAsync()
        {
            CompositionScopedBatch batch = TargetVisual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            TaskCompletionSource<object?> tcs = new TaskCompletionSource<object?>();

            batch.Completed += (s, e) => tcs.SetResult(null);

            StartAnimations();

            batch.End();

            return tcs.Task;
        }
    }
}
