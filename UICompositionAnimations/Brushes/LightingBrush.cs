using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Effects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace UICompositionAnimations.Brushes
{
    public class LightingBrush : XamlCompositionBrushBase
    {


        public double DiffuseAmount
        {
            get { return (double)GetValue(DiffuseAmountProperty); }
            set { SetValue(DiffuseAmountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Diffuseamount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DiffuseAmountProperty =
            DependencyProperty.Register("DiffuseAmount", typeof(double), typeof(LightingBrush), new PropertyMetadata(1d, OnDiffuseAmountChanged));

        private static void OnDiffuseAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as LightingBrush;
            b.CompositionBrush?.Properties.InsertScalar("Light.DiffuseAmount", (float)(double)e.NewValue);
        }


        /// <summary>
        /// Specular shine of the light. Default value is 16. Must be between 1 and 128.
        /// </summary>
        public double SpecularShine
        {
            get { return (double)GetValue(SpecularShineProperty); }
            set { SetValue(SpecularShineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpecularShine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecularShineProperty =
            DependencyProperty.Register("SpecularShine", typeof(double), typeof(LightingBrush), new PropertyMetadata(16d, OnSpecularShineChanged));

        private static void OnSpecularShineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as LightingBrush;
            b.CompositionBrush?.Properties.InsertScalar("Light.SpecularShine", (float)(double)e.NewValue);
        }



        public double SpecularAmount
        {
            get { return (double)GetValue(SpecularAmountProperty); }
            set { SetValue(SpecularAmountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpecularAmount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpecularAmountProperty =
            DependencyProperty.Register("SpecularAmount", typeof(double), typeof(LightingBrush), new PropertyMetadata(1d, OnSpecularAmountChanged));

        private static void OnSpecularAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = d as LightingBrush;
            b.CompositionBrush?.Properties.InsertScalar("Light.SpecularAmount", (float)(double)e.NewValue);
        }

        CompositionEffectBrush brush;
        CompositionEffectFactory factory;
        CompositionColorBrush color;
        InvertEffect inverse;
        LuminanceToAlphaEffect luminance;
        protected override void OnConnected()
        {
            if (CompositionBrush == null)
            {
                SceneLightingEffect sceneLightingEffect = new SceneLightingEffect()
                {
                    Name = "Light",
                    SpecularShine = (float)SpecularShine,
                    SpecularAmount = (float)SpecularAmount,
                    DiffuseAmount = (float)DiffuseAmount,
                    AmbientAmount = 0
                };
                luminance = new LuminanceToAlphaEffect() { Source = sceneLightingEffect };
                inverse = new InvertEffect() { Source = luminance };
                factory = Window.Current.Compositor.CreateEffectFactory(inverse, new String[] { "Light.DiffuseAmount", "Light.SpecularShine", "Light.SpecularAmount" });

                brush = factory.CreateBrush();
                CompositionBrush = brush;
            }
            base.OnConnected();
        }
        protected override void OnDisconnected()
        {
            if (CompositionBrush != null)
            {
                brush?.Dispose();
                factory?.Dispose();
                luminance?.Dispose();
                inverse?.Dispose();
                CompositionBrush = null;
            }
            base.OnDisconnected();
        }
    }
}
