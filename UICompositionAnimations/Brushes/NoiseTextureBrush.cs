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
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers;
using Windows.ApplicationModel;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A custom XAML brush that overlays a noise texture over a tint color
    /// </summary>
    public sealed class NoiseTextureBrush : XamlCompositionBrushBase
    {
        // The name of the animatable color property of the color effect
        private const string ColorSourceParameterName = "ColorSource.Color";

        #region Properties

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
            DependencyProperty.Register(nameof(Tint), typeof(Color), typeof(NoiseTextureBrush), new PropertyMetadata(Colors.Transparent, OnTintPropertyChanged));

        private static async void OnTintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NoiseTextureBrush @this = d.To<NoiseTextureBrush>();
            await @this.ConnectedSemaphore.WaitAsync();
            if (@this.CompositionBrush != null)
            {
                @this._EffectBrush?.Properties.InsertColor(ColorSourceParameterName, e.NewValue.To<Color>());
            }
            @this.ConnectedSemaphore.Release();
        }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> to the texture to use
        /// </summary>
        /// <remarks>This property must be initialized before using the brush</remarks>
        public Uri TextureUri { get; set; }

        /// <summary>
        /// Gets or sets the caching setting for the acrylic brush
        /// </summary>
        public BitmapCacheMode CacheMode { get; set; } = BitmapCacheMode.EnableCaching;

        #endregion

        // Initialization semaphore (due to the Win2D image loading being asynchronous)
        private readonly SemaphoreSlim ConnectedSemaphore = new SemaphoreSlim(1);

        // The composition brush used to render the effect
        private CompositionEffectBrush _EffectBrush;

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
                }
                ConnectedSemaphore.Release();
            }
            base.OnDisconnected();
        }

        /// <summary>
        /// Initializes the appropriate effect for the current instance
        /// </summary>
        private async Task SetupEffectAsync()
        {
            // Designer check
            if (DesignMode.DesignMode2Enabled) return;

            // Dictionary to track the reference and animatable parameters
            IDictionary<string, CompositionBrush> sourceParameters = new Dictionary<string, CompositionBrush>();
            List<string> animatableParameters = new List<string> { ColorSourceParameterName };

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.LoadTextureEffectWithTintAsync(Window.Current.Compositor, sourceParameters, Tint, TextureUri, CacheMode);

            // Extract and setup the tint and color effects
            ColorSourceEffect color = source as ColorSourceEffect ?? source.To<BlendEffect>().Background as ColorSourceEffect;
            if (color == null) throw new InvalidOperationException("Error while retrieving the color effect");
            color.Name = "ColorSource";

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory factory = Window.Current.Compositor.CreateEffectFactory(source, animatableParameters);

            // Create the effect factory and apply the final effect
            _EffectBrush = factory.CreateBrush();
            foreach (KeyValuePair<string, CompositionBrush> pair in sourceParameters)
            {
                _EffectBrush.SetSourceParameter(pair.Key, pair.Value);
            }
            CompositionBrush = _EffectBrush;
        }
    }
}
