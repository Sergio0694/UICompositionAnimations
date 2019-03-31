using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimationsLegacy.Behaviours;
using UICompositionAnimationsLegacy.Behaviours.Effects;
using UICompositionAnimationsLegacy.Brushes.Cache;
using UICompositionAnimationsLegacy.Enums;
using UICompositionAnimationsLegacy.Helpers;

namespace UICompositionAnimationsLegacy.Brushes
{
    /// <summary>
    /// A custom XAML brush that includes an acrylic effect that blurs the in-app content
    /// </summary>
    public sealed class CustomAcrylicBrush : XamlCompositionBrushBase
    {
        #region Constants

        // The name of the animatable blur amount property
        private const String BlurAmountParameterName = "Blur.BlurAmount";

        // The name of the animatable source 1 property (the brush) of the tint effect
        private const String TintColor1ParameterName = "Tint.Source1Amount";

        // The name of the animatable source 2 property (the tint color) of the tint effect
        private const String TintColor2ParameterName = "Tint.Source2Amount";

        // The name of the animatable color property of the color effect
        private const String ColorSourceParameterName = "ColorSource.Color";

        // The name of the animatable color property of the fallback color effect
        private const String FallbackColorParameterName = "FallbackColor.Color";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the source mode for the custom acrylic effect
        /// </summary>
        public AcrylicEffectMode Mode
        {
            get => GetValue(ModeProperty).To<AcrylicEffectMode>();
            set => SetValue(ModeProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Mode"/> property
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(AcrylicEffectMode), typeof(CustomAcrylicBrush), new PropertyMetadata(AcrylicEffectMode.InAppBlur, OnModePropertyChanged));

        private static async void OnModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.CompositionBrush != null)
            {
                // Rebuild the effects pipeline when needed
                @this.CompositionBrush.Dispose();
                @this.CompositionBrush = null;
                @this._State = AcrylicBrushEffectState.Default;
                await @this.SetupEffectAsync();
            }
            @this.ConnectedSemaphore.Release();
        }

        /// <summary>
        /// Gets or sets the blur amount for the effect
        /// </summary>
        /// <remarks>This property is ignored when the active mode is <see cref="AcrylicEffectMode.HostBackdrop"/></remarks>
        public double BlurAmount
        {
            get => GetValue(BlurAmountProperty).To<double>();
            set => SetValue(BlurAmountProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="BlurAmount"/> property
        /// </summary>
        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(CustomAcrylicBrush), new PropertyMetadata(8d, OnBlurAmountPropertyChanged));

        private static async void OnBlurAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.Mode == AcrylicEffectMode.InAppBlur)
            {
                // Update the blur value with or without an animation
                if (@this.BlurAnimationDuration == 0)
                {
                    @this._EffectBrush?.Properties.InsertScalar(BlurAmountParameterName, (float)e.NewValue.To<double>());
                }
                else @this._EffectBrush?.Properties.StartAnimationAsync(BlurAmountParameterName, (float)e.NewValue, TimeSpan.FromMilliseconds(@this.BlurAnimationDuration));
            }
            @this.ConnectedSemaphore.Release();
        }

        /// <summary>
        /// Gets or sets the duration of the optional animation played when changing the <see cref="BlurAmount"/> property
        /// </summary>
        /// <remarks>This property is ignored when the active mode is <see cref="AcrylicEffectMode.HostBackdrop"/></remarks>
        public int BlurAnimationDuration
        {
            get => GetValue(BlurAnimationDurationProperty).To<int>();
            set => SetValue(BlurAnimationDurationProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="BlurAnimationDuration"/> property
        /// </summary>
        public static readonly DependencyProperty BlurAnimationDurationProperty =
            DependencyProperty.Register(nameof(BlurAnimationDuration), typeof(int), typeof(CustomAcrylicBrush), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the color for the tint effect
        /// </summary>
        public Color Tint
        {
            get => GetValue(TintProperty).To<Color>();
            set => SetValue(TintProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Tint"/> property
        /// </summary>
        public static readonly DependencyProperty TintProperty =
            DependencyProperty.Register(nameof(Tint), typeof(Color), typeof(CustomAcrylicBrush), new PropertyMetadata(Colors.Transparent, OnTintPropertyChanged));

        private static async void OnTintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.CompositionBrush != null && @this._State != AcrylicBrushEffectState.FallbackMode)
            {
                @this._EffectBrush?.Properties.InsertColor(ColorSourceParameterName, e.NewValue.To<Color>());
            }
            @this.ConnectedSemaphore.Release();
        }

        /// <summary>
        /// Gets or sets the color for the tint effect (NOTE: this value must be in the [0..1) range)
        /// </summary>
        public double TintMix
        {
            get => GetValue(TintMixProperty).To<double>();
            set => SetValue(TintMixProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="TintMix"/> property
        /// </summary>
        public static readonly DependencyProperty TintMixProperty =
            DependencyProperty.Register(nameof(TintMix), typeof(double), typeof(CustomAcrylicBrush), new PropertyMetadata(0d, OnTintMixPropertyChanged));

        private static async void OnTintMixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double value = e.NewValue.To<double>();
            float fvalue = (float)value;
            if (value < 0 || value >= 1) throw new ArgumentOutOfRangeException(nameof(value), "The tint mix must be in the [0..1) range");
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.CompositionBrush != null && @this._State != AcrylicBrushEffectState.FallbackMode)
            {
                @this._EffectBrush.Properties.InsertScalar(TintColor1ParameterName, 1 - fvalue);
                @this._EffectBrush.Properties.InsertScalar(TintColor2ParameterName, fvalue);
            }
            @this.ConnectedSemaphore.Release();
        }

        /// <summary>
        /// Gets or sets the optional color to use for the brush, if the effect can't be loaded
        /// </summary>
        public Color UnsupportedEffectFallbackColor
        {
            get => GetValue(UnsupportedEffectFallbackColorProperty).To<Color>();
            set => SetValue(UnsupportedEffectFallbackColorProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="UnsupportedEffectFallbackColor"/> property
        /// </summary>
        public static readonly DependencyProperty UnsupportedEffectFallbackColorProperty =
            DependencyProperty.Register(nameof(UnsupportedEffectFallbackColor), typeof(Color), typeof(CustomAcrylicBrush), 
                new PropertyMetadata(Colors.Transparent, OnUnsupportedEffectFallbackColorPropertyChanged));

        private static async void OnUnsupportedEffectFallbackColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Unpack and lock
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            Color value = e.NewValue.To<Color>();

            // The fallback mode is currently enabled
            if (@this._State == AcrylicBrushEffectState.FallbackMode)
            {
                // New value is null, reset the current effect
                if (value.Equals(Colors.Transparent))
                {
                    @this.CompositionBrush.Dispose();
                    @this.CompositionBrush = null;
                    @this._State = AcrylicBrushEffectState.Default;
                }
                else
                {
                    // Otherwise, update the fallback color effect
                    @this._EffectBrush?.Properties.InsertColor(FallbackColorParameterName, value);
                }
            }
            else if (@this._State == AcrylicBrushEffectState.Default && // The effect is currently disabled
                     @this.Mode == AcrylicEffectMode.HostBackdrop &&    // The current settings are not valid
                     ApiInformationHelper.IsMobileDevice &&
                     !value.Equals(Colors.Transparent))                 // The new value allows the fallback mode to be enabled
            {
                await @this.SetupEffectAsync();
            }
            @this.ConnectedSemaphore.Release();
        }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> to the noise texture to use
        /// </summary>
        /// <remarks>This property must be initialized before using the brush</remarks>
        public Uri NoiseTextureUri { get; set; }

        /// <summary>
        /// Indicates whether or not to enable an additional safety procedure when loading the acrylic brush.
        /// This property should be used for brushes being rendered on secondary windows that use a given acrylic mode for the first
        /// time in the application, to prevent the internal cache from storing a brush instance that would cause the app to crash if
        /// reused in the app primary window, due to the different <see cref="Windows.UI.Core.CoreDispatcher"/> associated with that window.
        /// This property can stay disabled in most cases and should only be turned on when dealing with particular edge cases or issues
        /// with secondary app windows.
        /// </summary>
        /// <remarks>Turning this property on disables the internal cache system for <see cref="CompositionBackdropBrush"/> instances.
        /// This property, like the <see cref="NoiseTextureUri"/> property, must be initialized before using the brush</remarks>
        public bool DispatchProtectionEnabled { get; set; }

        #endregion

        // Initialization semaphore (due to the Win2D image loading being asynchronous)
        private readonly SemaphoreSlim ConnectedSemaphore = new SemaphoreSlim(1);

        // The composition brush used to render the effect
        private CompositionEffectBrush _EffectBrush;

        // Gets the current brush shate
        private AcrylicBrushEffectState _State = AcrylicBrushEffectState.Default;

        /// <inheritdoc/>
        protected override async void OnConnected()
        {
            if (CompositionBrush == null)
            {
                await ConnectedSemaphore.WaitAsync();
                if (CompositionBrush == null) // It could have been initialized while waiting on the semaphore
                {
                    await SetupEffectAsync();
                }
                ConnectedSemaphore.Release();
            }
            base.OnConnected();
        }

        /// <inheritdoc/>
        protected override async void OnDisconnected()
        {
            if (CompositionBrush != null)
            {
                await ConnectedSemaphore.WaitAsync();
                if (CompositionBrush != null)
                {
                    CompositionBrush.Dispose();
                    CompositionBrush = null;
                    _State = AcrylicBrushEffectState.Default;
                }
                ConnectedSemaphore.Release();
            }
            base.OnDisconnected();
        }

        #region CompositionBackdropBrush cache

        // The synchronization semaphore for the in-app backdrop brush
        private static readonly SemaphoreSlim BackdropSemaphore = new SemaphoreSlim(1);

        // The cached in-app backdrop brush
        private static CompositionBackdropBrush _BackdropInstance;

        // The name to use for the in-app backdrop reference parameter
        private const String BackdropReferenceParameterName = "BackdropBrush";

        // The synchronization semaphore for the host backdrop brush
        private static readonly SemaphoreSlim HostBackdropSemaphore = new SemaphoreSlim(1);

        // The cached host backdrop effect and partial pipeline to reuse
        private static HostBackdropInstanceWrapper _HostBackdropCache;

        // The name to use for the host backdrop reference parameter
        private const String HostBackdropReferenceParameterName = "HostBackdropBrush";

        /// <summary>
        /// Clears the internal cache of <see cref="CompositionBackdropBrush"/> instances
        /// </summary>
        [PublicAPI]
        public static async Task ClearCacheAsync(AcrylicEffectMode targets)
        {
            // In-app backdrop brush
            if (targets.HasFlag(AcrylicEffectMode.InAppBlur))
            {
                await BackdropSemaphore.WaitAsync();
                _BackdropInstance = null;
                BackdropSemaphore.Release();
            }

            // Host backdrop brush
            if (targets.HasFlag(AcrylicEffectMode.HostBackdrop))
            {
                await HostBackdropSemaphore.WaitAsync();
                _HostBackdropCache = null;
                HostBackdropSemaphore.Release();
            }
        }

        #endregion

        /// <summary>
        /// Initializes the appropriate acrylic effect for the current instance
        /// </summary>
        private async Task SetupEffectAsync()
        {
            // Designer check
            if (DesignMode.DesignModeEnabled) return;

            // Platform check
            if (ApiInformationHelper.IsMobileDevice && Mode == AcrylicEffectMode.HostBackdrop)
            {
                // Create the fallback brush effect when needed
                if (!UnsupportedEffectFallbackColor.Equals(Colors.Transparent))
                {
                    ColorSourceEffect fallback = new ColorSourceEffect
                    {
                        Name = "FallbackColor",
                        Color = UnsupportedEffectFallbackColor
                    };
                    CompositionEffectFactory fallbackFactory = Window.Current.Compositor.CreateEffectFactory(fallback);
                    _EffectBrush = fallbackFactory.CreateBrush();
                    CompositionBrush = _EffectBrush;
                    _State = AcrylicBrushEffectState.FallbackMode;
                }
                return;
            }

            // Dictionary to track the reference and animatable parameters
            IDictionary<String, CompositionBrush> sourceParameters = new Dictionary<String, CompositionBrush>();
            List<String> animatableParameters = new List<String>
            {
                TintColor1ParameterName,
                TintColor2ParameterName,
                ColorSourceParameterName
            };

            // Setup the base effect
            IGraphicsEffectSource baseEffect;
            if (Mode == AcrylicEffectMode.InAppBlur)
            {
                // Manage the cache
                await BackdropSemaphore.WaitAsync();
                if (_BackdropInstance == null && !DispatchProtectionEnabled)
                {
                    _BackdropInstance = Window.Current.Compositor.CreateBackdropBrush();
                }

                // Prepare the blur effect for the backdrop brush
                baseEffect = new GaussianBlurEffect
                {
                    Name = "Blur",
                    BlurAmount = 0f, // The blur value is inserted later on as it isn't applied correctly when set from here
                    BorderMode = EffectBorderMode.Hard,
                    Optimization = EffectOptimization.Balanced,
                    Source = new CompositionEffectSourceParameter(nameof(BackdropReferenceParameterName))
                };
                animatableParameters.Add(BlurAmountParameterName);
                sourceParameters.Add(nameof(BackdropReferenceParameterName),
                    _BackdropInstance?.Dispatcher.HasThreadAccess == true
                    ? _BackdropInstance
                    : Window.Current.Compositor.CreateBackdropBrush()); // Create a new instance when on a secondary window
                BackdropSemaphore.Release();
            }
            else
            {
                // Manage the cache
                await HostBackdropSemaphore.WaitAsync();
                if (_HostBackdropCache == null ||                           // Cache not initialized yet
                    !_HostBackdropCache.Brush.Dispatcher.HasThreadAccess)   // Cache initialized on another UI thread (different window)
                {
                    // Prepare a luminosity to alpha effect to adjust the background contrast
                    CompositionBackdropBrush hostBackdropBrush = Window.Current.Compositor.CreateHostBackdropBrush();
                    CompositionEffectSourceParameter backgroundParameter = new CompositionEffectSourceParameter(nameof(hostBackdropBrush));
                    LuminanceToAlphaEffect alphaEffect = new LuminanceToAlphaEffect { Source = backgroundParameter };
                    OpacityEffect opacityEffect = new OpacityEffect
                    {
                        Source = alphaEffect,
                        Opacity = 0.4f // Reduce the amount of the effect to avoid making bright areas completely black
                    };

                    // Layer [0,1,3] - Desktop background with blur and opacity mask
                    baseEffect = new BlendEffect
                    {
                        Background = backgroundParameter,
                        Foreground = opacityEffect,
                        Mode = BlendEffectMode.Multiply
                    };
                    sourceParameters.Add(HostBackdropReferenceParameterName, hostBackdropBrush);

                    // Update the cache when needed
                    if (_HostBackdropCache == null && !DispatchProtectionEnabled)
                    {
                        _HostBackdropCache = new HostBackdropInstanceWrapper(baseEffect, hostBackdropBrush);
                    }
                }
                else
                {
                    // Reuse the cached pipeline and effect
                    baseEffect = _HostBackdropCache.Pipeline;
                    sourceParameters.Add(HostBackdropReferenceParameterName, _HostBackdropCache.Brush);
                }
                HostBackdropSemaphore.Release();
            }

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(Window.Current.Compositor,
                baseEffect, sourceParameters, Tint, (float)TintMix, null, NoiseTextureUri);

            // Extract and setup the tint and color effects
            ArithmeticCompositeEffect tint = source as ArithmeticCompositeEffect ?? source.To<BlendEffect>().Background as ArithmeticCompositeEffect;
            if (tint == null) throw new InvalidOperationException("Error while retrieving the tint effect");
            tint.Name = "Tint";
            if (!(tint.Source2 is ColorSourceEffect color)) throw new InvalidOperationException("Error while retrieving the color effect");
            color.Name = "ColorSource";

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory factory = Window.Current.Compositor.CreateEffectFactory(source, animatableParameters);

            // Create the effect factory and apply the final effect
            _EffectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
            {
                _EffectBrush.SetSourceParameter(pair.Key, pair.Value);
            }

            // Update the blur amount and store the effect
            if (Mode == AcrylicEffectMode.InAppBlur)
                _EffectBrush.Properties.InsertScalar(BlurAmountParameterName, (float)BlurAmount);
            CompositionBrush = _EffectBrush;
            _State = AcrylicBrushEffectState.EffectEnabled;
        }
    }
}
