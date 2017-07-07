using System;
using UICompositionAnimations.Helpers;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using JetBrains.Annotations;

namespace UICompositionAnimations.Lights
{
    /// <summary>
    /// An attached XAML property to enable the <see cref="Brushes.LightingBrush"/> XAML brush
    /// </summary>
    public class PointerPositionSpotLight : XamlLight
    {
        /// <summary>
        /// Gets or sets the alpha channel value for the light to display
        /// </summary>
        public byte Alpha
        {
            get { return GetValue(AlphaProperty).To<byte>(); }
            set { SetValue(AlphaProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Alpha"/> property
        /// </summary>
        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(PointerPositionSpotLight), new PropertyMetadata(byte.MaxValue, OnAlphaPropertyChanged));

        private static void OnAlphaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointerPositionSpotLight l = d as PointerPositionSpotLight;
            if (l?._Light != null)
            {
                byte alpha = e.NewValue.To<byte>();
                Color color = l._Light.InnerConeColor;
                color.A = alpha;
                l._Light.InnerConeColor = l._Light.OuterConeColor = color;
            }
        }

        /// <summary>
        /// Gets or sets the intensity of the light. A higher value will result in a brighter light
        /// </summary>
        public byte Shade
        {
            get { return GetValue(ShadeProperty).To<byte>(); }
            set { SetValue(ShadeProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Shade"/> property
        /// </summary>
        public static readonly DependencyProperty ShadeProperty =
            DependencyProperty.Register(nameof(Shade), typeof(byte), typeof(PointerPositionSpotLight), new PropertyMetadata(byte.MaxValue, OnShadePropertyChanged));

        private static void OnShadePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointerPositionSpotLight l = d as PointerPositionSpotLight;
            if (l?._Light != null)
            {
                byte shade = e.NewValue.To<byte>();
                Color color = new Color
                {
                    A = l.Alpha,
                    R = shade,
                    G = shade,
                    B = shade
                };
                l._Light.InnerConeColor = l._Light.OuterConeColor = color;
            }
        }

        float _Z = 20;

        /// <summary>
        /// Gets or sets the Z axis of the light
        /// </summary>
        public float Z
        {
            get => _Z;
            set
            {
                _Z = value;
                _Properties?.InsertScalar("Z", value);
            }
        }

        /// <summary>
        /// Gets the <see cref="CompositionPropertySet"/> object for the current instance
        /// </summary>
        public CompositionPropertySet Properties => _Properties;

        /// <summary>
        /// Gets or sets the cone angle of the light
        /// </summary>
        public float OuterConeAngle
        {
            get { return (float)GetValue(OuterConeAngleProperty); }
            set { SetValue(OuterConeAngleProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="OuterConeAngle"/> property
        /// </summary>
        public static readonly DependencyProperty OuterConeAngleProperty =
            DependencyProperty.Register(nameof(OuterConeAngle), typeof(float), typeof(PointerPositionSpotLight), new PropertyMetadata(90f, OuterConeAngleChanged));

        private static void OuterConeAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointerPositionSpotLight l = d as PointerPositionSpotLight;
            if (l?._Light != null)
            {
                l._Light.OuterConeAngleInDegrees = (float)e.NewValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="IsTargetProperty"/> value for the target object
        /// </summary>
        /// <param name="obj">The target onject to inspect</param>
        public static bool GetIsTarget(DependencyObject obj) => (bool)obj.GetValue(IsTargetProperty);

        /// <summary>
        /// Sets the <see cref="IsTargetProperty"/> value for the target object
        /// </summary>
        /// <param name="obj">The target object</param>
        /// <param name="value">The value of the property to set</param>
        public static void SetIsTarget(DependencyObject obj, bool value) => obj.SetValue(IsTargetProperty, value);

        /// <summary>
        /// Gets or sets the value of the attached property
        /// </summary>
        public static readonly DependencyProperty IsTargetProperty =
            DependencyProperty.RegisterAttached("IsTarget", typeof(bool), typeof(PointerPositionSpotLight), new PropertyMetadata(false, OnIsTargetChanged));

        private static void OnIsTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // API test
            if (!ApiInformationHelper.AreXamlLightsSupported) return;

            // Apply the changes
            if ((bool)e.NewValue)
            {
                // Add
                if (d is Brush b)
                {
                    AddTargetBrush(GetIdStatic(), b);
                }
                else if (d is UIElement el)
                {
                    AddTargetElement(GetIdStatic(), el);
                }
            }
            else
            {
                // Remove
                if (d is Brush b)
                {
                    RemoveTargetBrush(GetIdStatic(), b);
                }
                else if (d is UIElement el)
                {
                    RemoveTargetElement(GetIdStatic(), el);
                }
            }
        }

        // The light to use
        SpotLight _Light;

        // The source compositor
        Compositor _Compositor;

        // The expression animation for the light position
        ExpressionAnimation _Animation;

        // The properties for the animation
        CompositionPropertySet _Properties;

        /// <inheritdoc cref="XamlLight"/>
        protected override void OnConnected(UIElement newElement)
        {
            if (CompositionLight == null)
            {
                // Initialize the fields
                _Compositor = Window.Current.Compositor;
                _Properties = _Compositor.CreatePropertySet();
                _Properties.InsertScalar("Z", Z);
                _Light = Window.Current.Compositor.CreateSpotLight();

                // Setup the light
                CompositionPropertySet pointer = ElementCompositionPreview.GetPointerPositionPropertySet(newElement);
                _Animation = _Compositor.CreateExpressionAnimation("Vector3(pointer.Position.X, pointer.Position.Y, props.Z)");
                _Animation.SetReferenceParameter("pointer", pointer);
                _Animation.SetReferenceParameter("props", _Properties);
                _Light.StartAnimation("Offset", _Animation);
                _Light.InnerConeColor = _Light.OuterConeColor = Color.FromArgb(Alpha, Shade, Shade, Shade);
                _Light.InnerConeAngleInDegrees = 0;
                _Light.OuterConeAngleInDegrees = OuterConeAngle;
                CompositionLight = _Light;
            }
            base.OnConnected(newElement);
        }

        /// <summary>
        /// Gets or sets a custom appendage for the <see cref="GetIdStatic"/> method
        /// </summary>
        [NotNull]
        public String IdAppendage { get; set; } = String.Empty;

        /// <inheritdoc cref="XamlLight"/>
        protected override String GetId() => GetIdStatic() + IdAppendage;

        /// <summary>
        /// Gets a static Id for the class
        /// </summary>
        public static String GetIdStatic() => typeof(PointerPositionSpotLight).FullName;
    }
}
