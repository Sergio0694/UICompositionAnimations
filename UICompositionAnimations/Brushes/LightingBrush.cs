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

        // The color transformation effect to convert luminance to opacity
        ColorMatrixEffect _ColorMatrixEffect;

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

                _ColorMatrixEffect = new ColorMatrixEffect()
                {
                    Source = sceneLightingEffect,
                    ColorMatrix = new Matrix5x4()
                    {
                        M11 = 0, M21 = 0, M31 = 0, M41 = 0, M51 = 1,
                        M12 = 0, M22 = 0, M32 = 0, M42 = 0, M52 = 1,
                        M13 = 0, M23 = 0, M33 = 0, M43 = 0, M53 = 1,
                        M14 = 0.2125f, M24 = 0.7154f, M34 = 0.0721f, M44 = 0, M54 = 0
                    }
                };

                _Factory = Window.Current.Compositor.CreateEffectFactory(_ColorMatrixEffect, new[] { "Light.DiffuseAmount", "Light.SpecularShine", "Light.SpecularAmount" });

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
                _ColorMatrixEffect?.Dispose();
                CompositionBrush = null;
            }
            base.OnDisconnected();
        }
    }
}
