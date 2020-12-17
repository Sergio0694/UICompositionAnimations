using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using FluentExtensions.UI.Animations.Enums;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI.Xaml.Media;

#nullable enable

namespace FluentExtensions.UI.Animations
{
    /// <summary>
    /// A <see langword="class"/> that allows to build custom animations targeting both the XAML and composition layers.
    /// </summary>
    public sealed class AnimationBuilder
    {
        /// <summary>
        /// The list of <see cref="ICompositionAnimation"/> instances representing composition animations to run.
        /// </summary>
        private readonly List<ICompositionAnimation> compositionAnimations = new();

        /// <summary>
        /// The list of <see cref="IXamlAnimationFactory"/> instances representing factories for XAML animations to run.
        /// </summary>
        private readonly List<IXamlAnimationFactory> xamlAnimationFactories = new();

        /// <summary>
        /// Adds a new opacity animation to the current schedule.
        /// </summary>
        /// <param name="from">The optional starting value for the animation.</param>
        /// <param name="to">The final value for the animation.</param>
        /// <param name="delay">The optional initial delay for the animation.</param>
        /// <param name="duration">The animation duration.</param>
        /// <param name="easing">The optional easing function for the animation.</param>
        /// <param name="layer">The target framework layer to animate.</param>
        /// <returns>The current <see cref="AnimationBuilder"/> instance.</returns>
        public AnimationBuilder Opacity(
            double? from,
            double to,
            TimeSpan? delay,
            TimeSpan duration,
            Easing easing,
            FrameworkLayer layer)
        {
            if (layer == FrameworkLayer.Composition)
            {
                return OnCompositionScalarAnimation(nameof(Visual.Opacity), (float?)from, (float)to, delay, duration, easing);
            }
            else
            {
                return OnXamlDoubleAnimation(nameof(UIElement.Opacity), from, to, delay, duration, easing, false);
            }
        }

        /// <summary>
        /// Adds a new translation animation for a single axis to the current schedule.
        /// </summary>
        /// <param name="axis">The target translation axis to animate.</param>
        /// <param name="from">The optional starting value for the animation.</param>
        /// <param name="to">The final value for the animation.</param>
        /// <param name="delay">The optional initial delay for the animation.</param>
        /// <param name="duration">The animation duration.</param>
        /// <param name="easing">The optional easing function for the animation.</param>
        /// <param name="layer">The target framework layer to animate.</param>
        /// <returns>The current <see cref="AnimationBuilder"/> instance.</returns>
        public AnimationBuilder Translation(
            Axis axis,
            double? from,
            double to,
            TimeSpan? delay,
            TimeSpan duration,
            Easing easing,
            FrameworkLayer layer)
        {
            if (layer == FrameworkLayer.Composition)
            {
                return OnCompositionScalarAnimation($"Translation.{axis}", (float?)from, (float)to, delay, duration, easing);
            }
            else
            {
                return OnXamlTransformDoubleAnimation($"Translate{axis}", from, to, delay, duration, easing);
            }
        }

        /// <summary>
        /// Starts the animations present in the current <see cref="AnimationBuilder"/> instance.
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/> to animate.</param>
        public void Start(UIElement element)
        {
            if (this.compositionAnimations.Count > 0)
            {
                Visual visual = ElementCompositionPreview.GetElementVisual(element);

                foreach (var animation in this.compositionAnimations)
                {
                    animation.StartAnimation(visual);
                }
            }

            if (this.xamlAnimationFactories.Count > 0)
            {
                Storyboard storyboard = new();

                foreach (var factory in this.xamlAnimationFactories)
                {
                    storyboard.Children.Add(factory.GetAnimation(element));
                }

                storyboard.Begin();
            }
        }

        /// <summary>
        /// Starts the animations present in the current <see cref="AnimationBuilder"/> instance.
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/> to animate.</param>
        /// <returns>A <see cref="Task"/> that completes when all animations have completed.</returns>
        public Task StartAsync(UIElement element)
        {
            Task
                compositionTask = Task.CompletedTask,
                xamlTask = Task.CompletedTask;

            if (this.compositionAnimations.Count > 0)
            {
                Visual visual = ElementCompositionPreview.GetElementVisual(element);
                CompositionScopedBatch batch = visual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
                TaskCompletionSource<object?> taskCompletionSource = new();

                batch.Completed += (_, _) => taskCompletionSource.SetResult(null);

                foreach (var animation in this.compositionAnimations)
                {
                    animation.StartAnimation(visual);
                }

                batch.End();

                compositionTask = taskCompletionSource.Task;
            }

            if (this.xamlAnimationFactories.Count > 0)
            {
                Storyboard storyboard = new();
                TaskCompletionSource<object?> taskCompletionSource = new();

                foreach (var factory in this.xamlAnimationFactories)
                {
                    storyboard.Children.Add(factory.GetAnimation(element));
                }

                storyboard.Completed += (_, _) => taskCompletionSource.SetResult(null);
                storyboard.Begin();

                xamlTask = taskCompletionSource.Task;
            }

            return Task.WhenAll(compositionTask, xamlTask);
        }

        /// <summary>
        /// Adds a new composition scalar animation to the current schedule.
        /// </summary>
        /// <param name="property">The target property to animate.</param>
        /// <param name="from">The optional starting value for the animation.</param>
        /// <param name="to">The final value for the animation.</param>
        /// <param name="delay">The optional initial delay for the animation.</param>
        /// <param name="duration">The animation duration.</param>
        /// <param name="easing">The optional easing function for the animation.</param>
        /// <returns>The current <see cref="AnimationBuilder"/> instance.</returns>
        private AnimationBuilder OnCompositionScalarAnimation(
            string property,
            float? from,
            float to,
            TimeSpan? delay,
            TimeSpan duration,
            Easing easing)
        {
            CompositionScalarAnimation animation = new(property, from, to, delay, duration, easing);

            this.compositionAnimations.Add(animation);

            return this;
        }

        /// <summary>
        /// Adds a new composition <see cref="Vector3"/> animation to the current schedule.
        /// </summary>
        /// <param name="property">The target property to animate.</param>
        /// <param name="from">The optional starting value for the animation.</param>
        /// <param name="to">The final value for the animation.</param>
        /// <param name="delay">The optional initial delay for the animation.</param>
        /// <param name="duration">The animation duration.</param>
        /// <param name="easing">The optional easing function for the animation.</param>
        /// <returns>The current <see cref="AnimationBuilder"/> instance.</returns>
        private AnimationBuilder OnCompositionVector3Animation(
            string property,
            Vector3? from,
            Vector3 to,
            TimeSpan? delay,
            TimeSpan duration,
            Easing easing)
        {
            CompositionVector3Animation animation = new(property, from, to, delay, duration, easing);

            this.compositionAnimations.Add(animation);

            return this;
        }

        /// <summary>
        /// Adds a new XAML <see cref="double"/> animation to the current schedule.
        /// </summary>
        /// <param name="property">The target property to animate.</param>
        /// <param name="from">The optional starting value for the animation.</param>
        /// <param name="to">The final value for the animation.</param>
        /// <param name="delay">The optional initial delay for the animation.</param>
        /// <param name="duration">The animation duration.</param>
        /// <param name="easing">The optional easing function for the animation.</param>
        /// <param name="enableDependentAnimation">Whether to set <see cref="DoubleAnimation.EnableDependentAnimation"/>.</param>
        /// <returns>The current <see cref="AnimationBuilder"/> instance.</returns>
        private AnimationBuilder OnXamlDoubleAnimation(
            string property,
            double? from,
            double to,
            TimeSpan? delay,
            TimeSpan duration,
            Easing easing,
            bool enableDependentAnimation)
        {
            XamlDoubleAnimation animation = new(property, from, to, delay, duration, easing, enableDependentAnimation);

            this.xamlAnimationFactories.Add(animation);

            return this;
        }

        /// <summary>
        /// Adds a new XAML transform <see cref="double"/> animation to the current schedule.
        /// </summary>
        /// <param name="property">The target property to animate.</param>
        /// <param name="from">The optional starting value for the animation.</param>
        /// <param name="to">The final value for the animation.</param>
        /// <param name="delay">The optional initial delay for the animation.</param>
        /// <param name="duration">The animation duration.</param>
        /// <param name="easing">The optional easing function for the animation.</param>
        /// <returns>The current <see cref="AnimationBuilder"/> instance.</returns>
        private AnimationBuilder OnXamlTransformDoubleAnimation(
            string property,
            double? from,
            double to,
            TimeSpan? delay,
            TimeSpan duration,
            Easing easing)
        {
            XamlTransformDoubleAnimation animation = new(property, from, to, delay, duration, easing);

            this.xamlAnimationFactories.Add(animation);

            return this;
        }

        /// <summary>
        /// A model representing a specified composition scalar animation.
        /// </summary>
        private sealed record CompositionScalarAnimation(
            string Property,
            float? From,
            float To,
            TimeSpan? Delay,
            TimeSpan Duration,
            Easing Easing)
            : ICompositionAnimation
        {
            /// <inheritdoc/>
            public void StartAnimation(Visual visual)
            {
                visual.StopAnimation(Property);

                CompositionEasingFunction easingFunction = visual.Compositor.GetEasingFunction(Easing);
                ScalarKeyFrameAnimation animation = visual.Compositor.CreateScalarKeyFrameAnimation(From, To, Duration, Delay, easingFunction);

                visual.StartAnimation(Property, animation);
            }
        }

        /// <summary>
        /// A model representing a specified composition <see cref="Vector3"/> animation.
        /// </summary>
        private sealed record CompositionVector3Animation(
            string Property,
            Vector3? From,
            Vector3 To,
            TimeSpan? Delay,
            TimeSpan Duration,
            Easing Easing)
            : ICompositionAnimation
        {
            /// <inheritdoc/>
            public void StartAnimation(Visual visual)
            {
                visual.StopAnimation(Property);

                CompositionEasingFunction easingFunction = visual.Compositor.GetEasingFunction(Easing);
                Vector3KeyFrameAnimation animation = visual.Compositor.CreateVector3KeyFrameAnimation(From, To, Duration, Delay, easingFunction);

                visual.StartAnimation(Property, animation);
            }
        }

        /// <summary>
        /// A model representing a specified XAML <see cref="double"/> animation.
        /// </summary>
        private sealed record XamlDoubleAnimation(
            string Property,
            double? From,
            double To,
            TimeSpan? Delay,
            TimeSpan Duration,
            Easing Easing,
            bool EnableDependentAnimation)
            : IXamlAnimationFactory
        {
            /// <inheritdoc/>
            public Timeline GetAnimation(UIElement element)
            {
                DoubleAnimation animation = new()
                {
                    From = From,
                    To = To,
                    BeginTime = Delay,
                    Duration = Duration,
                    EasingFunction = Easing.ToEasingFunction(),
                    EnableDependentAnimation = EnableDependentAnimation
                };

                Storyboard.SetTarget(animation, element);
                Storyboard.SetTargetProperty(animation, Property);

                return animation;
            }
        }

        /// <summary>
        /// A model representing a specified XAML <see cref="double"/> animation targeting a transform.
        /// </summary>
        private sealed record XamlTransformDoubleAnimation(
            string Property,
            double? From,
            double To,
            TimeSpan? Delay,
            TimeSpan Duration,
            Easing Easing)
            : IXamlAnimationFactory
        {
            /// <inheritdoc/>
            public Timeline GetAnimation(UIElement element)
            {
                DoubleAnimation animation = new()
                {
                    From = From,
                    To = To,
                    BeginTime = Delay,
                    Duration = Duration,
                    EasingFunction = Easing.ToEasingFunction()
                };
                CompositeTransform transform = element.GetTransform<CompositeTransform>();

                Storyboard.SetTarget(animation, transform);
                Storyboard.SetTargetProperty(animation, Property);

                return animation;
            }
        }

        /// <summary>
        /// An interface for factories of XAML animations.
        /// </summary>
        private interface IXamlAnimationFactory
        {
            /// <summary>
            /// Gets a <see cref="Timeline"/> instance representing the animation to start.
            /// </summary>
            /// <param name="element">The target <see cref="UIElement"/> instance to animate.</param>
            /// <returns>A <see cref="Timeline"/> instance with the specified animation.</returns>
            Timeline GetAnimation(UIElement element);
        }

        /// <summary>
        /// An interface for animations targeting the composition layer.
        /// </summary>
        private interface ICompositionAnimation
        {
            /// <summary>
            /// Starts the current animation.
            /// </summary>
            /// <param name="visual">The target <see cref="Visual"/> instance to animate.</param>
            void StartAnimation(Visual visual);
        }
    }
}