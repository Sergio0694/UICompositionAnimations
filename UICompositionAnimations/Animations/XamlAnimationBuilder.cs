using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using JetBrains.Annotations;
using UICompositionAnimations.Animations.Abstract;
using UICompositionAnimations.Animations.Interfaces;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations
{
    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IAnimationBuilder"/> <see langword="interface"/> using composition APIs
    /// </summary>
    internal sealed class XamlAnimationBuilder : AnimationBuilderBase<Func<TimeSpan, Timeline>>
    {
        /// <summary>
        /// The target <see cref="CompositeTransform"/> to animate
        /// </summary>
        [NotNull]
        private readonly CompositeTransform TargetTransform;

        public XamlAnimationBuilder([NotNull] UIElement target) : base(target)
        {
            TargetTransform = target.GetTransform<CompositeTransform>(false);
            target.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Opacity(double to, Easing ease = Easing.Linear)
        {
            double from = TargetElement.Opacity;
            return Opacity(from, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Opacity(double from, double to, Easing ease = Easing.Linear)
        {
            AnimationFactories.Add(duration => TargetElement.CreateDoubleAnimation(nameof(UIElement.Opacity), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override Vector2 CurrentTranslation => new Vector2((float)TargetTransform.TranslateX, (float)TargetTransform.TranslateY);

        /// <inheritdoc/>
        public override IAnimationBuilder Translation(Vector2 from, Vector2 to, Easing ease = Easing.Linear)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(Axis.X.ToString(), from.X, to.X, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(Axis.Y.ToString(), from.Y, to.Y, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override Vector2 CurrentOffset => throw new NotSupportedException("Can't animate the offset property from XAML");

        /// <inheritdoc/>
        public override IAnimationBuilder Offset(Vector2 from, Vector2 to, Easing ease = Easing.Linear)
        {
            throw new NotSupportedException("Can't animate the offset property from XAML");
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Scale(double to, Easing ease = Easing.Linear)
        {
            double scale = TargetTransform.ScaleX;
            return Scale((float)scale, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Scale(double from, double to, Easing ease = Easing.Linear)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleX), from, to, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleY), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Rotate(double to, Easing ease = Easing.Linear)
        {
            double angle = TargetTransform.Rotation;
            return Rotate(angle, to, ease);
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Rotate(double from, double to, Easing ease = Easing.Linear)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.Rotation), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Clip(Side side, double to, Easing ease = Easing.Linear)
        {
            throw new NotSupportedException("Can't animate the clip property from XAML");
        }

        /// <inheritdoc/>
        public override IAnimationBuilder Clip(Side side, double from, double to, Easing ease = Easing.Linear)
        {
            throw new NotSupportedException("Can't animate the clip property from XAML");
        }

        /// <summary>
        /// Gets the <see cref="Windows.UI.Xaml.Media.Animation.Storyboard"/> represented by the current instance
        /// </summary>
        [NotNull]
        private Storyboard Storyboard => AnimationFactories.Select(f => f(DurationInterval)).ToStoryboard();

        /// <inheritdoc/>
        protected override void OnStart() => Storyboard.Begin();

        /// <inheritdoc/>
        protected override Task OnStartAsync() => Storyboard.BeginAsync();
    }
}
