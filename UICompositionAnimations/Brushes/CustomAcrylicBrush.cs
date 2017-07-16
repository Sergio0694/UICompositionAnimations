using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using UICompositionAnimations.Behaviours;
using UICompositionAnimations.Behaviours.Effects;
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A custom XAML brush that includes an acrylic effect that blurs the in-app content
    /// </summary>
    public sealed class CustomAcrylicBrush : XamlCompositionBrushBase
    {
        #region Constants

        // The animation of the blur transition animation
        private const int BlurAnimationDuration = 250;

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
            get { return GetValue(ModeProperty).To<AcrylicEffectMode>(); }
            set { SetValue(ModeProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Mode"/> property
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(AcrylicEffectMode), typeof(LightingBrush), new PropertyMetadata(AcrylicEffectMode.InAppBlur, OnModePropertyChanged));

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
        /// Gets or sets the blur amount for the effect (NOTE: this property is ignored when the active mode is <see cref="AcrylicEffectMode.HostBackdrop"/>)
        /// </summary>
        public double BlurAmount
        {
            get { return GetValue(BlurAmountProperty).To<double>(); }
            set { SetValue(BlurAmountProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="BlurAmount"/> property
        /// </summary>
        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(LightingBrush), new PropertyMetadata(8d, OnBlurAmountPropertyChanged));

        private static async void OnBlurAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.Mode == AcrylicEffectMode.InAppBlur)
                @this._EffectBrush?.StartAnimationAsync(BlurAmountParameterName, (float)e.NewValue.To<double>(), TimeSpan.FromMilliseconds(BlurAnimationDuration)).Forget();
            @this.ConnectedSemaphore.Release();
                
        }

        /// <summary>
        /// Gets or sets the color for the tint effect
        /// </summary>
        public Color Tint
        {
            get { return GetValue(TintProperty).To<Color>(); }
            set { SetValue(TintProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Tint"/> property
        /// </summary>
        public static readonly DependencyProperty TintProperty =
            DependencyProperty.Register(nameof(Tint), typeof(Color), typeof(LightingBrush), new PropertyMetadata(Colors.Transparent, OnTintPropertyChanged));

        private static async void OnTintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.CompositionBrush != null)
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
            get { return GetValue(TintMixProperty).To<double>(); }
            set { SetValue(TintMixProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="TintMix"/> property
        /// </summary>
        public static readonly DependencyProperty TintMixProperty =
            DependencyProperty.Register(nameof(Tint), typeof(double), typeof(LightingBrush), new PropertyMetadata(0d, OnTintMixPropertyChanged));

        private static async void OnTintMixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double value = e.NewValue.To<double>();
            float fvalue = (float)value;
            if (value < 0 || value >= 1) throw new ArgumentOutOfRangeException("The tint mix must be in the [0..1) range");
            CustomAcrylicBrush @this = d.To<CustomAcrylicBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.CompositionBrush != null)
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
            get { return GetValue(UnsupportedEffectFallbackColorProperty).To<Color>(); }
            set { SetValue(UnsupportedEffectFallbackColorProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="UnsupportedEffectFallbackColor"/> property
        /// </summary>
        public static readonly DependencyProperty UnsupportedEffectFallbackColorProperty =
            DependencyProperty.Register(nameof(UnsupportedEffectFallbackColor), typeof(Color), typeof(LightingBrush), 
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
        /// Gets or sets the <see cref="Uri"/> to the noise texture to use (NOTE: this must be initialized before using the brush)
        /// </summary>
        public Uri NoiseTextureUri { get; set; }

        /// <summary>
        /// Gets or sets the caching setting for the acrylic brush
        /// </summary>
        public BitmapCacheMode CacheMode { get; set; } = BitmapCacheMode.EnableCaching;

        #endregion

        // Initialization semaphore (due to the Win2D image loading being asynchronous)
        private readonly SemaphoreSlim ConnectedSemaphore = new SemaphoreSlim(1);

        // The composition brush used to render the effect
        private CompositionEffectBrush _EffectBrush;

        // Gets the current brush shate
        private AcrylicBrushEffectState _State = AcrylicBrushEffectState.Default;

        /// <inheritdoc cref="XamlCompositionBrushBase"/>
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

        /// <inheritdoc cref="XamlCompositionBrushBase"/>
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

        /// <summary>
        /// Initializes the appropriate acrylic effect for the current instance
        /// </summary>
        private async Task SetupEffectAsync()
        {
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
                // Prepare a luminosity to alpha effect to adjust the background contrast
                CompositionBackdropBrush backdropBrush = Window.Current.Compositor.CreateBackdropBrush();
                baseEffect = new GaussianBlurEffect
                {
                    Name = "Blur",
                    BlurAmount = 0f,
                    BorderMode = EffectBorderMode.Hard,
                    Optimization = EffectOptimization.Balanced,
                    Source = new CompositionEffectSourceParameter(nameof(backdropBrush))
                };
                animatableParameters.Add(BlurAmountParameterName);
                sourceParameters.Add(nameof(backdropBrush), backdropBrush);
            }
            else
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

                // Layer [0,1,3] - Desktop background with blur and tint overlay
                baseEffect = new BlendEffect
                {
                    Background = backgroundParameter,
                    Foreground = opacityEffect,
                    Mode = BlendEffectMode.Overlay
                };
                sourceParameters.Add(nameof(hostBackdropBrush), hostBackdropBrush);
            }

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(Window.Current.Compositor,
                baseEffect, sourceParameters, Tint, (float)TintMix, null, NoiseTextureUri, CacheMode);

            // Extract and setup the tint and color effects
            ArithmeticCompositeEffect tint = source as ArithmeticCompositeEffect ?? source.To<BlendEffect>().Background as ArithmeticCompositeEffect;
            if (tint == null) throw new InvalidOperationException("Error while retrieving the tint effect");
            tint.Name = "Tint";
            ColorSourceEffect color = tint.Source2 as ColorSourceEffect;
            if (color == null) throw new InvalidOperationException("Error while retrieving the color effect");
            color.Name = "ColorSource";

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory factory = Window.Current.Compositor.CreateEffectFactory(source, animatableParameters);

            // Create the effect factory and apply the final effect
            _EffectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
            {
                _EffectBrush.SetSourceParameter(pair.Key, pair.Value);
            }

            // Animate the blur and store the effect
            if (Mode == AcrylicEffectMode.InAppBlur)
                _EffectBrush.StartAnimationAsync(BlurAmountParameterName, (float)BlurAmount, TimeSpan.FromMilliseconds(BlurAnimationDuration)).Forget();
            CompositionBrush = _EffectBrush;
            _State = AcrylicBrushEffectState.EffectEnabled;
        }
    }
}
