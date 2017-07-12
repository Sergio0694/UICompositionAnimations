using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Graphics.Effects;
using Windows.UI;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using UICompositionAnimations.Behaviours;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A custom XAML brush that includes an acrylic effect that blurs the in-app content
    /// </summary>
    public sealed class CustomInAppAcrylicBrush : XamlCompositionBrushBase
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
        private const String ColorSourceColorParameterName = "ColorSource.Color";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the blur amount for the effect
        /// </summary>
        public float BlurAmount
        {
            get { return GetValue(BlurAmountProperty).To<float>(); }
            set { SetValue(BlurAmountProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="BlurAmount"/> property
        /// </summary>
        public static readonly DependencyProperty BlurAmountProperty =
            DependencyProperty.Register(nameof(BlurAmount), typeof(double), typeof(LightingBrush), new PropertyMetadata(8, OnBlurAmountPropertyChanged));

        private static void OnBlurAmountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.To<CustomInAppAcrylicBrush>()?._EffectBrush?.StartAnimationAsync(
                BlurAmountParameterName, e.NewValue.To<float>(), TimeSpan.FromMilliseconds(BlurAnimationDuration)).Forget();
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

        private static void OnTintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.To<CustomInAppAcrylicBrush>()?._EffectBrush.Properties.InsertColor(ColorSourceColorParameterName, e.NewValue.To<Color>());
        }

        /// <summary>
        /// Gets or sets the color for the tint effect
        /// </summary>
        public float TintMix
        {
            get { return GetValue(TintMixProperty).To<float>(); }
            set { SetValue(TintMixProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="TintMix"/> property
        /// </summary>
        public static readonly DependencyProperty TintMixProperty =
            DependencyProperty.Register(nameof(Tint), typeof(float), typeof(LightingBrush), new PropertyMetadata(0, OnTintMixPropertyChanged));

        private static void OnTintMixPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            float value = e.NewValue.To<float>();
            if (value < 0 || value >= 1) throw new ArgumentOutOfRangeException("The tint mix must be in the [0..1) range");
            CustomInAppAcrylicBrush @this = d.To<CustomInAppAcrylicBrush>();
            if (@this._EffectBrush != null)
            {
                @this._EffectBrush.Properties.InsertScalar(TintColor1ParameterName, 1 - value);
                @this._EffectBrush.Properties.InsertScalar(TintColor2ParameterName, value);
            }
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> to the noise texture to use (NOTE: this must be initialized before using the brush)
        /// </summary>
        public Uri NoiseTextureUri { get; set; }

        #endregion

        // Initialization semaphore (due to the Win2D image loading being asynchronous)
        private readonly SemaphoreSlim ConnectedSemaphore = new SemaphoreSlim(1);

        // The composition brush used to render the effect
        private CompositionEffectBrush _EffectBrush;

        /// <inheritdoc cref="XamlCompositionBrushBase"/>
        protected override async void OnConnected()
        {
            if (CompositionBrush == null)
            {
                await ConnectedSemaphore.WaitAsync();
                if (CompositionBrush == null) // It could have been initialized while waiting on the semaphore
                {
                    // Prepare a luminosity to alpha effect to adjust the background contrast
                    CompositionBackdropBrush backdropBrush = Window.Current.Compositor.CreateBackdropBrush();
                    GaussianBlurEffect blurEffect = new GaussianBlurEffect
                    {
                        Name = "Blur",
                        BlurAmount = 0f,
                        BorderMode = EffectBorderMode.Hard,
                        Optimization = EffectOptimization.Balanced,
                        Source = new CompositionEffectSourceParameter(nameof(backdropBrush))
                    };

                    // Background with blur and tint overlay
                    IDictionary<String, CompositionBrush> sourceParameters = new Dictionary<String, CompositionBrush>
                    {
                        { nameof(backdropBrush), backdropBrush }
                    };

                    // Get the noise brush using Win2D
                    IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(Window.Current.Compositor,
                        blurEffect, sourceParameters, Tint, TintMix, null, NoiseTextureUri);

                    // Extract and setup the tint and color effects
                    ArithmeticCompositeEffect tint = source as ArithmeticCompositeEffect ?? source.To<BlendEffect>().Background as ArithmeticCompositeEffect;
                    if (tint == null) throw new InvalidOperationException("Error while retrieving the tint effect");
                    tint.Name = "Tint";
                    ColorSourceEffect color = tint.Source2 as ColorSourceEffect;
                    if (color == null) throw new InvalidOperationException("Error while retrieving the color effect");
                    color.Name = "ColorSource";

                    // Make sure the Win2D brush was loaded correctly
                    CompositionEffectFactory factory = Window.Current.Compositor.CreateEffectFactory(source, new[]
                    {
                        BlurAmountParameterName,
                        TintColor1ParameterName,
                        TintColor2ParameterName,
                        ColorSourceColorParameterName
                    });

                    // Create the effect factory and apply the final effect
                    _EffectBrush = factory.CreateBrush();
                    foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
                    {
                        _EffectBrush.SetSourceParameter(pair.Key, pair.Value);
                    }

                    // Animate the blur and store the effect
                    _EffectBrush.StartAnimationAsync(BlurAmountParameterName, BlurAmount, TimeSpan.FromMilliseconds(BlurAnimationDuration)).Forget();
                    CompositionBrush = _EffectBrush;
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
                }
                ConnectedSemaphore.Release();
            }
            base.OnDisconnected();
        }
    }
}
