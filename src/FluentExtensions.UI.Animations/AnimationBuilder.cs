using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using FluentExtensions.UI.Animations.Enums;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Hosting;

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
        /// A model representing a specified composition scalar animation.
        /// </summary>
        private sealed record CompositionScalarAnimation(
            string Property,
            float? From,
            float To,
            TimeSpan Duration,
            TimeSpan? Delay,
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