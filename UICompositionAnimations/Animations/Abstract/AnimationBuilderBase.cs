using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimationsLegacy.Animations.Interfaces;
using UICompositionAnimationsLegacy.Enums;

namespace UICompositionAnimationsLegacy.Animations.Abstract
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
        public IAnimationBuilder Opacity(double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnOpacity(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Opacity(double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnOpacity(from, to, ease);

        /// <summary>
        /// Schedules an opacity animation on a single axis
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The EasingFunctionNames function to use for the translation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnOpacity(double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(TranslationAxis axis, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnTranslation(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(TranslationAxis axis, double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnTranslation(axis, from, to, ease);

        /// <summary>
        /// Schedules a translation animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The EasingFunctionNames function to use for the translation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnTranslation(TranslationAxis axis, double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnTranslation(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnTranslation(from, to, ease);

        /// <summary>
        /// Schedules a 2D translation animation
        /// </summary>
        /// <param name="from">The optional starting position</param>
        /// <param name="to">The target position</param>
        /// <param name="ease">The EasingFunctionNames function to use for the translation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(TranslationAxis axis, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnOffset(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(TranslationAxis axis, double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnOffset(axis, from, to, ease);

        /// <summary>
        /// Schedules an offset animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The EasingFunctionNames function to use for the offset animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnOffset(TranslationAxis axis, double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnOffset(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnOffset(from, to, ease);

        /// <summary>
        /// Schedules a 2D offset animation
        /// </summary>
        /// <param name="from">The optional starting position</param>
        /// <param name="to">The target position</param>
        /// <param name="ease">The EasingFunctionNames function to use for the offset animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnOffset(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnScale(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnScale(from, to, ease);

        /// <summary>
        /// Schedules a scale animation
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The EasingFunctionNames function to use for the scale animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnScale(double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnRotate(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnRotate(from, to, ease);

        /// <summary>
        /// Schedules a rotation animation
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The EasingFunctionNames function to use for the rotation animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnRotate(double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(MarginSide side, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnClip(side, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(MarginSide side, double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnClip(side, from, to, ease);

        /// <summary>
        /// Schedules a clip animation
        /// </summary>
        /// <param name="side">The clip side to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The EasingFunctionNames function to use for the clip animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnClip(MarginSide side, double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(TranslationAxis axis, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnSize(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(TranslationAxis axis, double from, double to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnSize(axis, from, to, ease);

        /// <summary>
        /// Schedules a size animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnSize(TranslationAxis axis, double? from, double to, EasingFunctionNames ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnSize(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear) => OnSize(from, to, ease);

        /// <summary>
        /// Schedules a 2D size animation
        /// </summary>
        /// <param name="from">The optional starting position</param>
        /// <param name="to">The target position</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        [MustUseReturnValue, NotNull]
        protected abstract IAnimationBuilder OnSize(Vector2? from, Vector2 to, EasingFunctionNames ease = EasingFunctionNames.Linear);

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
