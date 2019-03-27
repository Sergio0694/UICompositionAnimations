using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
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
    internal sealed class CompositionAnimationBuilder : IAnimationBuilder
    {
        /// <summary>
        /// The target <see cref="UIElement"/> to animate
        /// </summary>
        [NotNull]
        private readonly UIElement TargetElement;

        /// <summary>
        /// The target <see cref="Visual"/> to animate
        /// </summary>
        [NotNull]
        private readonly Visual TargetVisual;

        public CompositionAnimationBuilder([NotNull] UIElement target)
        {
            TargetElement = target;
            TargetVisual = target.GetVisual();
        }

        private readonly IList<CompositionAnimationProducer> AnimationProducers = new List<CompositionAnimationProducer>();

        /// <summary>
        /// The <see cref="TimeSpan"/> that defines the duration of the animation
        /// </summary>
        private TimeSpan _Duration;

        /// <summary>
        /// The <see cref="TimeSpan"/> that defines the initial delay of the animation
        /// </summary>
        private TimeSpan? _Delay;

        public IAnimationBuilder Opacity(float to, EasingFunctionNames ease)
        {
            float from = TargetVisual.Opacity;
            return Opacity(from, to, ease);
        }

        public IAnimationBuilder Opacity(float from, float to, EasingFunctionNames ease)
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

        public IAnimationBuilder Translation(float to, EasingFunctionNames ease)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Translation(float from, float to, EasingFunctionNames ease)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAnimationBuilder Duration(int ms) => Duration(TimeSpan.FromMilliseconds(ms));

        /// <inheritdoc/>
        public IAnimationBuilder Duration(TimeSpan duration)
        {
            _Duration = duration;
            return this;
        }

        /// <inheritdoc/>
        public IAnimationBuilder Delay(int ms) => Delay(TimeSpan.FromMilliseconds(ms));

        /// <inheritdoc/>
        public IAnimationBuilder Delay(TimeSpan interval)
        {
            _Delay = interval;
            return this;
        }

        /// <inheritdoc/>
        public async void Start()
        {
            if (_Delay != null) await Task.Delay(_Delay.Value);
            StartAnimations();
        }

        /// <inheritdoc/>
        public async void Start(Action callback)
        {
            await StartAsync();
            callback();
        }

        /// <inheritdoc/>
        public async Task StartAsync()
        {
            if (_Delay != null) await Task.Delay(_Delay.Value);
            CompositionScopedBatch batch = TargetVisual.Compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            TaskCompletionSource tcs = new TaskCompletionSource();
            batch.Completed += (s, e) => tcs.SetResult(null);
            StartAnimations();
            batch.End();
            await tcs.Task;
        }

        /// <summary>
        /// Creates and starts the pending animations
        /// </summary>
        private void StartAnimations()
        {
            foreach (var producer in AnimationProducers)
            {
                var info = producer(_Duration);
                TargetVisual.StartAnimation(info.Property, info.Animation);
            }
        }
    }
}
