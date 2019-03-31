using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;

namespace UICompositionAnimationsLegacy.Behaviours.Effects
{
    /// <summary>
    /// A custom acrylic brush effect that can quickly toggle between different sources (in-app blur or host backdrop effect)
    /// </summary>
    /// <typeparam name="T">The the host <see cref="UIElement"/> that will display the effect visual</typeparam>
    public sealed class AttachedToggleAcrylicEffect<T> : AttachedStaticCompositionEffect<T> where T : FrameworkElement
    {
        /// <summary>
        /// Gets the action that edits the effects pipeline to apply the requested change
        /// </summary>
        [NotNull]
        private readonly Action<AcrylicEffectMode> Toggle;

        // An optional fade in animation for the blurred in-app acrylic effect
        [CanBeNull]
        private readonly Action InitialInAppEffectFadeIn;

        // Internal constructor
        internal AttachedToggleAcrylicEffect([NotNull] T element, AcrylicEffectMode mode, [CanBeNull] Action fadeIn,
            [NotNull] Action<AcrylicEffectMode> toggle, [NotNull] SpriteVisual sprite, bool disposeOnUnload)
            : base(element, sprite, disposeOnUnload)
        {
            _AcrylicMode = mode;
            _FadeInPending = mode == AcrylicEffectMode.HostBackdrop;
            InitialInAppEffectFadeIn = fadeIn;
            Toggle = toggle;
        }

        // Indicates whether or not the fade in animation should be run
        private bool _FadeInPending;

        private AcrylicEffectMode _AcrylicMode;

        /// <summary>
        /// Gets or sets the current effect mode in use
        /// </summary>
        public AcrylicEffectMode AcrylicMode
        {
            get => _AcrylicMode;
            set
            {
                if (_AcrylicMode != value)
                {
                    Toggle(value);
                    if (_FadeInPending && InitialInAppEffectFadeIn != null)
                    {
                        _FadeInPending = false;
                        InitialInAppEffectFadeIn();
                    }
                    _AcrylicMode = value;
                }
            }
        }
    }
}
