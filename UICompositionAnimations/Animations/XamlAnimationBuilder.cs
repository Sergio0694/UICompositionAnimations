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
using UICompositionAnimations.XAMLTransform;

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
            TargetTransform = target.GetRenderTransform<CompositeTransform>(false);
            target.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOpacity(double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetElement, nameof(UIElement.Opacity), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(TranslationAxis axis, double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, $"Translate{axis}", from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(CompositeTransform.TranslateX), from?.X, to.X, duration, ease));
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(CompositeTransform.TranslateY), from?.Y, to.Y, duration, ease));

            return this;
        }

        protected override IAnimationBuilder OnOffset(TranslationAxis axis, double? from, double to, EasingFunctionNames ease) => throw new NotSupportedException("Can't animate the offset property from XAML");

        protected override IAnimationBuilder OnOffset(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => throw new NotSupportedException("Can't animate the offset property from XAML");

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(CompositeTransform.ScaleX), from, to, duration, ease));
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(CompositeTransform.ScaleY), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnRotate(double? from, double to, EasingFunctionNames ease)
        {
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(CompositeTransform.Rotation), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnClip(MarginSide side, double? from, double to, EasingFunctionNames ease) => throw new NotSupportedException("Can't animate the clip property from XAML");

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(TranslationAxis axis, double? from, double to, EasingFunctionNames ease)
        {
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("Can't animate the size of an item that's not a framework element");
            AnimationFactories.Add(duration =>
            {
                switch (axis)
                {
                    case TranslationAxis.X: return XAMLTransformToolkit.CreateDoubleAnimation(TargetElement, nameof(FrameworkElement.Width), double.IsNaN(element.Width) ? element.ActualWidth : from, to, duration, ease, true);
                    case TranslationAxis.Y: return XAMLTransformToolkit.CreateDoubleAnimation(TargetElement, nameof(FrameworkElement.Height), double.IsNaN(element.Height) ? element.ActualHeight : from, to, duration, ease, true);
                    default: throw new ArgumentException($"Invalid axis: {axis}", nameof(axis));
                }
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear)
        {
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("Can't animate the size of an item that's not a framework element");
            double?
                fromX = double.IsNaN(element.Width) ? (double?)element.ActualWidth : from?.X,
                fromY = double.IsNaN(element.Height) ? (double?)element.ActualHeight : from?.Y;
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(FrameworkElement.Width), fromX, to.X, duration, ease, true));
            AnimationFactories.Add(duration => XAMLTransformToolkit.CreateDoubleAnimation(TargetTransform, nameof(FrameworkElement.Height), fromY, to.Y, duration, ease, true));

            return this;
        }

        /// <summary>
        /// Gets the <see cref="Windows.UI.Xaml.Media.Animation.Storyboard"/> represented by the current instance
        /// </summary>
        [NotNull]
        private Storyboard Storyboard => XAMLTransformToolkit.PrepareStoryboard(AnimationFactories.Select(f => f(DurationInterval)).ToArray());

        /// <inheritdoc/>
        protected override void OnStart() => Storyboard.Begin();

        /// <inheritdoc/>
        protected override Task OnStartAsync() => Storyboard.WaitAsync();
    }
}
