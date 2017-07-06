using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition;
using Windows.UI.Composition.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A custom XAML brush that includes a lighting effect
    /// </summary>
    public sealed class LightingBrush : XamlCompositionBrushBase
    {
        /// <summary>
        /// Gets or sets the diffuse property for the brush
        /// </summary>
        public double DiffuseAmount
        {
            get { return (double)GetValue(DiffuseAmountProperty); }
            set { SetValue(DiffuseAmountProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="DiffuseAmount"/> property
        /// </summary>
        public static readonly DependencyProperty DiffuseAmountProperty =
            DependencyProperty.Register(nameof(DiffuseAmount), typeof(double), typeof(LightingBrush), new PropertyMetadata(1d, OnDiffuseAmountChanged));

        private static void OnDiffuseAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.To<LightingBrush>()?.CompositionBrush?.Properties.InsertScalar("Light.DiffuseAmount", (float)(double)e.NewValue);
        }


        /// <summary>
        /// Gets or sets the specular shine of the light. The default value is 16 and it must be between 1 and 128
        /// </summary>
        public double SpecularShine
        {
            get { return (double)GetValue(SpecularShineProperty); }
            set { SetValue(SpecularShineProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="SpecularShine"/> property
        /// </summary>
        public static readonly DependencyProperty SpecularShineProperty =
            DependencyProperty.Register(nameof(SpecularShine), typeof(double), typeof(LightingBrush), new PropertyMetadata(16d, OnSpecularShineChanged));

        private static void OnSpecularShineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.To<LightingBrush>()?.CompositionBrush?.Properties.InsertScalar("Light.SpecularShine", (float)(double)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the specular amount for the effect
        /// </summary>
        public double SpecularAmount
        {
            get { return (double)GetValue(SpecularAmountProperty); }
            set { SetValue(SpecularAmountProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="SpecularAmount"/> property
        /// </summary>
        public static readonly DependencyProperty SpecularAmountProperty =
            DependencyProperty.Register(nameof(SpecularAmount), typeof(double), typeof(LightingBrush), new PropertyMetadata(1d, OnSpecularAmountChanged));

        private static void OnSpecularAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.To<LightingBrush>()?.CompositionBrush?.Properties.InsertScalar("Light.SpecularAmount", (float)(double)e.NewValue);
        }

        // The effect brush to use
        CompositionEffectBrush _Brush;

        // The factory to create the brush
        CompositionEffectFactory _Factory;

        // The invert effect for the opacity mask
        InvertEffect _InverseEffect;

        // The luminance effect to generate the opacity mask
        LuminanceToAlphaEffect _LuminanceEffect;

        /// <inheritdoc cref="XamlCompositionBrushBase"/>
        protected override void OnConnected()
        {
            if (CompositionBrush == null)
            {
                // Effects setup
                SceneLightingEffect sceneLightingEffect = new SceneLightingEffect // Base lighting effect
                {
                    Name = "Light",
                    SpecularShine = (float)SpecularShine,
                    SpecularAmount = (float)SpecularAmount,
                    DiffuseAmount = (float)DiffuseAmount,
                    AmbientAmount = 0
                };
                _LuminanceEffect = new LuminanceToAlphaEffect { Source = sceneLightingEffect }; // Map the bright areas of the light to an opacity mask
                _InverseEffect = new InvertEffect { Source = _LuminanceEffect }; // Invert the colors to make the brighter areas white
                _Factory = Window.Current.Compositor.CreateEffectFactory(_InverseEffect, new[] { "Light.DiffuseAmount", "Light.SpecularShine", "Light.SpecularAmount" });

                // Create and store the brush
                _Brush = _Factory.CreateBrush();
                CompositionBrush = _Brush;
            }
            base.OnConnected();
        }

        /// <inheritdoc cref="XamlCompositionBrushBase"/>
        protected override void OnDisconnected()
        {
            if (CompositionBrush != null)
            {
                _Brush?.Dispose();
                _Factory?.Dispose();
                _LuminanceEffect?.Dispose();
                _InverseEffect?.Dispose();
                CompositionBrush = null;
            }
            base.OnDisconnected();
        }
    }
}
