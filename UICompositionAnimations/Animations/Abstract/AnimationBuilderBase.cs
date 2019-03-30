using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Animations.Interfaces;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations.Abstract
{
    /// <summary>
    /// An <see langword="abstract"/> <see langword="class"/> used as base by all the animation builder classes
    /// </summary>
    internal abstract class AnimationBuilderBase<T> : IAnimationBuilder
    {
        /// <summary>
        /// A <see langword="protected"/> constructor that initializes the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        protected AnimationBuilderBase([NotNull] UIElement target) => TargetElement = target;

        /// <summary>
        /// The target <see cref="UIElement"/> to animate
        /// </summary>
        [NotNull]
        protected UIElement TargetElement { get; }

        /// <summary>
        /// Gets the list of <typeparamref name="T"/> instances used to create the animations to run
        /// </summary>
        [NotNull, ItemNotNull]
        protected IList<T> AnimationFactories { get; } = new List<T>();

        /// <summary>
        /// Gets the <see cref="System.TimeSpan"/> that defines the duration of the animation
        /// </summary>
        protected TimeSpan DurationInterval { get; private set; }

        /// <summary>
        /// The <see cref="TimeSpan"/> that defines the initial delay of the animation
        /// </summary>
        private TimeSpan? _Delay;

        #region Animations

        /// <inheritdoc/>
        public IAnimationBuilder Opacity(double to, Easing ease = Easing.Linear) => OnOpacity(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Opacity(double from, double to, Easing ease = Easing.Linear) => OnOpacity(from, to, ease);

        /// <summary>
        /// Schedules an opacity animation on a single axis
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnOpacity(double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, double to, Easing ease = Easing.Linear) => OnTranslation(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, double from, double to, Easing ease = Easing.Linear) => OnTranslation(axis, from, to, ease);

        /// <summary>
        /// Schedules a translation animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnTranslation(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 to, Easing ease = Easing.Linear) => OnTranslation(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 from, Vector2 to, Easing ease = Easing.Linear) => OnTranslation(from, to, ease);

        /// <summary>
        /// Schedules a 2D translation animation
        /// </summary>
        /// <param name="from">The optional starting position</param>
        /// <param name="to">The target position</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, double to, Easing ease = Easing.Linear) => OnOffset(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, double from, double to, Easing ease = Easing.Linear) => OnOffset(axis, from, to, ease);

        /// <summary>
        /// Schedules an offset animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnOffset(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 to, Easing ease = Easing.Linear) => OnOffset(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 from, Vector2 to, Easing ease = Easing.Linear) => OnOffset(from, to, ease);

        /// <summary>
        /// Schedules a 2D offset animation
        /// </summary>
        /// <param name="from">The optional starting position</param>
        /// <param name="to">The target position</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnOffset(Vector2? from, Vector2 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double to, Easing ease = Easing.Linear) => OnScale(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double from, double to, Easing ease = Easing.Linear) => OnScale(from, to, ease);

        /// <summary>
        /// Schedules a scale animation
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnScale(double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double to, Easing ease = Easing.Linear) => OnRotate(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double from, double to, Easing ease = Easing.Linear) => OnRotate(from, to, ease);

        /// <summary>
        /// Schedules a rotation animation
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the rotation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnRotate(double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(Side side, double to, Easing ease = Easing.Linear) => OnClip(side, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(Side side, double from, double to, Easing ease = Easing.Linear) => OnClip(side, from, to, ease);

        /// <summary>
        /// Schedules a clip animation
        /// </summary>
        /// <param name="side">The clip side to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the clip animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnClip(Side side, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Axis axis, double to, Easing ease = Easing.Linear) => OnSize(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Axis axis, double from, double to, Easing ease = Easing.Linear) => OnSize(axis, from, to, ease);

        /// <summary>
        /// Schedules a size animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnSize(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 to, Easing ease = Easing.Linear) => OnSize(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 from, Vector2 to, Easing ease = Easing.Linear) => OnSize(from, to, ease);

        /// <summary>
        /// Schedules a 2D size animation
        /// </summary>
        /// <param name="from">The optional starting position</param>
        /// <param name="to">The target position</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnSize(Vector2? from, Vector2 to, Easing ease = Easing.Linear);

        #endregion

        /// <inheritdoc/>
        public IAnimationBuilder Duration(int ms) => Duration(TimeSpan.FromMilliseconds(ms));

        /// <inheritdoc/>
        public IAnimationBuilder Duration(TimeSpan duration)
        {
            DurationInterval = duration;
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
            OnStart();
        }

        /// <summary>
        /// Starts the animation represented by the current instance
        /// </summary>
        protected abstract void OnStart();

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
            await OnStartAsync();
        }

        /// <summary>
        /// Starts the animation represented by the current instance, and returns a <see cref="Task"/> to track it
        /// </summary>
        protected abstract Task OnStartAsync();
    }
}
