using System;
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
    internal abstract class AnimationBuilderBase : IAnimationBuilder
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
        /// Gets the <see cref="System.TimeSpan"/> that defines the duration of the animation
        /// </summary>
        protected TimeSpan DurationInterval { get; private set; }

        /// <summary>
        /// The <see cref="System.TimeSpan"/> that defines the initial delay of the animation
        /// </summary>
        private TimeSpan? _Delay;

        #region Animations

        /// <inheritdoc/>
        public abstract IAnimationBuilder Opacity(float to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Opacity(float from, float to, Easing ease = Easing.Linear);

        /// <summary>
        /// Gets a <see cref="Vector2"/> value that represents the current translation for the target <see cref="UIElement"/>
        /// </summary>
        protected abstract Vector2 CurrentTranslation { get; }

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, float to, Easing ease = Easing.Linear)
        {
            Vector2 translation = CurrentTranslation;
            if (axis == Axis.X) translation.X = to;
            else translation.Y = to;
            return Translation(CurrentOffset, translation, ease);
        }

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Axis axis, float from, float to, Easing ease = Easing.Linear)
        {
            Vector2 translation = CurrentTranslation;
            return axis == Axis.X
                ? Translation(new Vector2(from, translation.Y), new Vector2(to, translation.Y), ease)
                : Translation(new Vector2(translation.X, from), new Vector2(translation.X, to), ease);
        }

        /// <inheritdoc/>
        public IAnimationBuilder Translation(Vector2 to, Easing ease = Easing.Linear)
        {
            return Translation(CurrentTranslation, to, ease);
        }

        /// <inheritdoc/>
        public abstract IAnimationBuilder Translation(Vector2 from, Vector2 to, Easing ease = Easing.Linear);

        /// <summary>
        /// Gets a <see cref="Vector2"/> value that represents the current offset for the target <see cref="UIElement"/>
        /// </summary>
        protected abstract Vector2 CurrentOffset { get; }

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, float to, Easing ease = Easing.Linear)
        {
            Vector2 offset = CurrentOffset;
            if (axis == Axis.X) offset.X = to;
            else offset.Y = to;
            return Offset(CurrentOffset, offset, ease);
        }

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Axis axis, float from, float to, Easing ease = Easing.Linear)
        {
            Vector2 offset = CurrentOffset;
            return axis == Axis.X
                ? Offset(new Vector2(from, offset.Y), new Vector2(to, offset.Y), ease)
                : Offset(new Vector2(offset.X, from), new Vector2(offset.X, to), ease);
        }

        /// <inheritdoc/>
        public IAnimationBuilder Offset(Vector2 to, Easing ease = Easing.Linear)
        {
            return Offset(CurrentOffset, to, ease);
        }

        /// <inheritdoc/>
        public abstract IAnimationBuilder Offset(Vector2 from, Vector2 to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Scale(float to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Scale(float from, float to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Rotate(float to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Rotate(float from, float to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Clip(Side side, float to, Easing ease = Easing.Linear);

        /// <inheritdoc/>
        public abstract IAnimationBuilder Clip(Side side, float from, float to, Easing ease = Easing.Linear);

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
