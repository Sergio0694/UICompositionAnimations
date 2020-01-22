using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using FluentExtensions.UI.Animations.Abstract;
using FluentExtensions.UI.Animations.Enums;
using FluentExtensions.UI.Animations.Interfaces;

#nullable enable

namespace FluentExtensions.UI.Animations
{
    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IAnimationBuilder"/> <see langword="interface"/> using composition APIs
    /// </summary>
    internal sealed class XamlAnimationBuilder : AnimationBuilderBase<Func<TimeSpan, Timeline>>
    {
        /// <summary>
        /// The target <see cref="CompositeTransform"/> to animate
        /// </summary>
        private readonly CompositeTransform TargetTransform;

        /// <summary>
        /// Creates a new <see cref="XamlAnimationBuilder"/> with the specified parameters
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        public XamlAnimationBuilder(UIElement target) : base(target)
        {
            TargetTransform = target.GetTransform<CompositeTransform>();

            target.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOpacity(double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetElement.CreateDoubleAnimation(nameof(UIElement.Opacity), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Axis axis, double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation($"Translate{axis}", from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.TranslateX), from?.X, to.X, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.TranslateY), from?.Y, to.Y, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector3? from, Vector3 to, Easing ease)
        {
            throw new NotSupportedException("Animations on 3 axes are not supported on the XAML layer");
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Axis axis, double? from, double to, Easing ease)
        {
            throw new NotSupportedException("Can't animate the offset property from XAML");
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Vector2? from, Vector2 to, Easing ease)
        {
            throw new NotSupportedException("Can't animate the offset property from XAML");
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOffset(Vector3? from, Vector3 to, Easing ease)
        {
            throw new NotSupportedException("Animations on 3 axes are not supported on the XAML layer");
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleX), from, to, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleY), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(Axis axis, double? from, double to, Easing ease)
        {
            string property = axis switch
            {
                Axis.X => nameof(CompositeTransform.ScaleX),
                Axis.Y => nameof(CompositeTransform.ScaleY),
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, $"This axis is not supported on the XAML layer: {axis}")
            };

            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(property, from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(Vector2? from, Vector2 to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleX), from?.X, to.X, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleY), from?.Y, to.Y, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(Vector3? from, Vector3 to, Easing ease)
        {
            throw new NotSupportedException("Animations on 3 axes are not supported on the XAML layer");
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnRotate(double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.Rotation), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnClip(Side side, double? from, double to, Easing ease)
        {
            throw new NotSupportedException("Can't animate the clip property from XAML");
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Axis axis, double? from, double to, Easing ease)
        {
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("Can't animate the size of an item that's not a framework element");

            AnimationFactories.Add(duration =>
            {
                return axis switch
                {
                    Axis.X => TargetElement.CreateDoubleAnimation(nameof(FrameworkElement.Width), double.IsNaN(element.Width) ? element.ActualWidth : from, to, duration, ease, true),
                    Axis.Y => TargetElement.CreateDoubleAnimation(nameof(FrameworkElement.Height), double.IsNaN(element.Height) ? element.ActualHeight : from, to, duration, ease, true),
                    _ => throw new ArgumentException($"Invalid axis: {axis}", nameof(axis)),
                };
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Vector2? from, Vector2 to, Easing ease)
        {
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("Can't animate the size of an item that's not a framework element");

            double?
                fromX = double.IsNaN(element.Width) ? (double?)element.ActualWidth : from?.X,
                fromY = double.IsNaN(element.Height) ? (double?)element.ActualHeight : from?.Y;

            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(FrameworkElement.Width), fromX, to.X, duration, ease, true));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(FrameworkElement.Height), fromY, to.Y, duration, ease, true));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Vector3? from, Vector3 to, Easing ease)
        {
            throw new NotSupportedException("Animations on 3 axes are not supported on the XAML layer");
        }

        /// <summary>
        /// Gets the <see cref="Windows.UI.Xaml.Media.Animation.Storyboard"/> represented by the current instance
        /// </summary>
        private Storyboard Storyboard => AnimationFactories.Select(f => f(DurationInterval)).ToStoryboard();

        /// <inheritdoc/>
        protected override void OnStart() => Storyboard.Begin();

        /// <inheritdoc/>
        protected override Task OnStartAsync() => Storyboard.BeginAsync();
    }
}
