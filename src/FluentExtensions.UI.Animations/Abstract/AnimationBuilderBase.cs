using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using FluentExtensions.UI.Animations.Enums;
using FluentExtensions.UI.Animations.Interfaces;

#nullable enable

namespace FluentExtensions.UI.Animations.Abstract
{
    /// <summary>
    /// An <see langword="abstract"/> <see langword="class"/> used as base by all the animation builder classes
    /// </summary>
    /// <typeparam name="T">The <see cref="Delegate"/> type used to add new animations when requested</typeparam>
    internal abstract class AnimationBuilderBase<T> : IAnimationBuilder where T : Delegate
    {
        /// <summary>
        /// The <see cref="TimeSpan"/> that defines the initial delay of the animation
        /// </summary>
        private TimeSpan? _Delay;

        /// <summary>
        /// A <see langword="protected"/> constructor that initializes the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        protected AnimationBuilderBase(UIElement target) => TargetElement = target;

        /// <summary>
        /// The target <see cref="UIElement"/> to animate
        /// </summary>
        protected UIElement TargetElement { get; }

        /// <summary>
        /// Gets the list of <typeparamref name="T"/> instances used to create the animations to run
        /// </summary>
        protected IList<T> AnimationFactories { get; } = new List<T>();

        /// <summary>
        /// Gets the <see cref="System.TimeSpan"/> that defines the duration of the animation
        /// </summary>
        protected TimeSpan DurationInterval { get; private set; }

        /// <inheritdoc/>
        public IAnimationBuilder Opacity(double to) => OnOpacity(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Opacity(double to, Easing ease) => OnOpacity(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Opacity(double from, double to, Easing ease) => OnOpacity(from, to, ease);

        /// <summary>
        /// Schedules an opacity animation on a single axis
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        protected abstract IAnimationBuilder OnOpacity(double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, double to) => OnTranslation(axis, null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, double to, Easing ease) => OnTranslation(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, double from, double to, Easing ease) => OnTranslation(axis, from, to, ease);

        /// <summary>
        /// Schedules a translation animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        protected abstract IAnimationBuilder OnTranslation(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 to) => OnTranslation(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 to, Easing ease) => OnTranslation(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 from, Vector2 to, Easing ease) => OnTranslation(from, to, ease);

        /// <summary>
        /// Schedules a 2D translation animation
        /// </summary>
        /// <param name="from">The optional starting vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        protected abstract IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector3 to) => OnTranslation(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector3 to, Easing ease) => OnTranslation(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector3 from, Vector3 to, Easing ease) => OnTranslation(from, to, ease);

        /// <summary>
        /// Schedules a 3D translation animation
        /// </summary>
        /// <param name="from">The optional starting vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        protected abstract IAnimationBuilder OnTranslation(Vector3? from, Vector3 to, Easing ease);

        // <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, double to) => OnOffset(axis, null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, double to, Easing ease) => OnOffset(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, double from, double to, Easing ease) => OnOffset(axis, from, to, ease);

        /// <summary>
        /// Schedules an offset animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        protected abstract IAnimationBuilder OnOffset(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 to) => OnOffset(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 to, Easing ease) => OnOffset(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 from, Vector2 to, Easing ease) => OnOffset(from, to, ease);

        /// <summary>
        /// Schedules a 2D offset animation
        /// </summary>
        /// <param name="from">The optional starting vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        protected abstract IAnimationBuilder OnOffset(Vector2? from, Vector2 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector3 to) => OnOffset(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector3 to, Easing ease) => OnOffset(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector3 from, Vector3 to, Easing ease) => OnOffset(from, to, ease);

        /// <summary>
        /// Schedules a 3D offset animation
        /// </summary>
        /// <param name="from">The optional starting vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        protected abstract IAnimationBuilder OnOffset(Vector3? from, Vector3 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double to) => OnScale(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double to, Easing ease) => OnScale(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(double from, double to, Easing ease) => OnScale(from, to, ease);

        /// <summary>
        /// Schedules a uniform scale animation on the X and Y axes
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        protected abstract IAnimationBuilder OnScale(double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Axis axis, double to) => OnScale(axis, null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Axis axis, double to, Easing ease) => OnScale(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Axis axis, double from, double to, Easing ease) => OnScale(axis, from, to, ease);

        /// <summary>
        /// Schedules a scale animation on a specified axis
        /// </summary>
        /// <param name="axis">The scale axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        protected abstract IAnimationBuilder OnScale(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Vector2 to) => OnScale(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Vector2 to, Easing ease) => OnScale(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Vector2 from, Vector2 to, Easing ease) => OnScale(from, to, ease);

        /// <summary>
        /// Schedules a scale animation on the X and Y axes
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        protected abstract IAnimationBuilder OnScale(Vector2? from, Vector2 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Vector3 to) => OnScale(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Vector3 to, Easing ease) => OnScale(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Scale(Vector3 from, Vector3 to, Easing ease) => OnScale(from, to, ease);

        /// <summary>
        /// Schedules a scale animation on all axes
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        protected abstract IAnimationBuilder OnScale(Vector3? from, Vector3 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double to) => OnRotate(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double to, Easing ease) => OnRotate(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Rotate(double from, double to, Easing ease) => OnRotate(from, to, ease);

        /// <summary>
        /// Schedules a rotation animation
        /// </summary>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the rotation animation</param>
        protected abstract IAnimationBuilder OnRotate(double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(Side side, double to) => OnClip(side, null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(Side side, double to, Easing ease) => OnClip(side, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Clip(Side side, double from, double to, Easing ease) => OnClip(side, from, to, ease);

        /// <summary>
        /// Schedules a clip animation
        /// </summary>
        /// <param name="side">The clip side to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the clip animation</param>
        protected abstract IAnimationBuilder OnClip(Side side, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Axis axis, double to) => OnSize(axis, null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Axis axis, double to, Easing ease) => OnSize(axis, null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Axis axis, double from, double to, Easing ease) => OnSize(axis, from, to, ease);

        /// <summary>
        /// Schedules a size animation on a single axis
        /// </summary>
        /// <param name="axis">The target axis to animate</param>
        /// <param name="from">The optional starting value</param>
        /// <param name="to">The target value</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        protected abstract IAnimationBuilder OnSize(Axis axis, double? from, double to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 to) => OnSize(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 to, Easing ease) => OnSize(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector2 from, Vector2 to, Easing ease) => OnSize(from, to, ease);

        /// <summary>
        /// Schedules a 2D size animation
        /// </summary>
        /// <param name="from">The optional starting vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        protected abstract IAnimationBuilder OnSize(Vector2? from, Vector2 to, Easing ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector3 to) => OnSize(null, to, Easing.Linear);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector3 to, Easing ease) => OnSize(null, to, ease);

        /// <inheritdoc/>
        public IAnimationBuilder Size(Vector3 from, Vector3 to, Easing ease) => OnSize(from, to, ease);

        /// <summary>
        /// Schedules a 3D size animation
        /// </summary>
        /// <param name="from">The optional starting vector</param>
        /// <param name="to">The target vector</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        protected abstract IAnimationBuilder OnSize(Vector3? from, Vector3 to, Easing ease);

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
