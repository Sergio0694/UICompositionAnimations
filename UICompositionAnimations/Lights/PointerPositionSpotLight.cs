using UICompositionAnimations.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace UICompositionAnimations.Lights
{
    public class PointerPositionSpotLight : XamlLight
    {


        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(PointerPositionSpotLight), new PropertyMetadata(Colors.White, OnColorChanged));


        float z = 20;
        public float Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
                props?.InsertScalar("Z", z);
            }
        }

        public CompositionPropertySet Properties => props;


        public float OuterConeAngle
        {
            get { return (float)GetValue(OuterConeAngleProperty); }
            set { SetValue(OuterConeAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OuterConeAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OuterConeAngleProperty =
            DependencyProperty.Register("OuterConeAngle", typeof(float), typeof(PointerPositionSpotLight), new PropertyMetadata(90f, OuterConeAngleChanged));

        private static void OuterConeAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var l = d as PointerPositionSpotLight;
            if (l.light != null)
            {
                l.light.OuterConeAngleInDegrees = (float)e.NewValue;
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var l = d as PointerPositionSpotLight;
            if (l.light != null)
            {
                l.light.InnerConeColor = l.light.OuterConeColor = (Color)e.NewValue;
            }
        }

        public static bool GetIsTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTargetProperty);
        }

        public static void SetIsTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTargetProperty, value);
        }

        public string IdAppendage = "";

        // Using a DependencyProperty as the backing store for IsTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTargetProperty =
            DependencyProperty.RegisterAttached("IsTarget", typeof(bool), typeof(PointerPositionSpotLight), new PropertyMetadata(false, OnIsTargetChanged));

        private static void OnIsTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
            if (!ApiInformationHelper.AreXamlLightsSupported)
                return;
            if ((bool)e.NewValue)
            {
                if (d is Brush b)
                {
                    XamlLight.AddTargetBrush(GetIdStatic(), b);
                }
                else if (d is UIElement el)
                {
                    AddTargetElement(GetIdStatic(), el);
                }
            }
            else
            {
                if (d is Brush b)
                {
                    XamlLight.RemoveTargetBrush(GetIdStatic(), b);
                }
                else if (d is UIElement el)
                {
                    RemoveTargetElement(GetIdStatic(), el);
                }
            }
        }
        SpotLight light;
        Compositor c;
        ExpressionAnimation ani;
        CompositionPropertySet props;
        protected override void OnConnected(UIElement newElement)
        {
            if (CompositionLight == null)
            {
                c = Window.Current.Compositor;
                props = c.CreatePropertySet();
                props.InsertScalar("Z", (float)Z);
                light = Window.Current.Compositor.CreateSpotLight();
                var pointer = ElementCompositionPreview.GetPointerPositionPropertySet(newElement);
                ani = c.CreateExpressionAnimation("Vector3(pointer.Position.X, pointer.Position.Y, props.Z)");
                //light.Offset = new Vector3(150, 150, 200);
                ani.SetReferenceParameter("pointer", pointer);
                ani.SetReferenceParameter("props", props);
                light.StartAnimation("Offset", ani);
                
                light.InnerConeColor = Color;
                light.OuterConeColor = Color;

                light.InnerConeAngleInDegrees = 00;
                light.OuterConeAngleInDegrees = OuterConeAngle;
                //light.LinearAttenuation = 10;
                CompositionLight = light;
                if (newElement is FrameworkElement fe)
                {
                    //fe.SizeChanged += Fe_SizeChanged;
                }
            }
            base.OnConnected(newElement);
        }

        private async void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //await Task.Delay(100);
            //light.StartAnimation("Offset", ani);
        }

        protected override void OnDisconnected(UIElement oldElement)
        {
            if (CompositionLight != null)
            {
                //CompositionLight.Dispose();
                //CompositionLight = null;
            }
            base.OnDisconnected(oldElement);
        }

        protected override string GetId()
        {
            return GetIdStatic() + IdAppendage;
        }

        public static string GetIdStatic()
        {
            return typeof(PointerPositionSpotLight).FullName;
        }
    }
}
