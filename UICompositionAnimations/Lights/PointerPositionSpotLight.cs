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
    [PublicAPI]
    public class PointerPositionSpotLight : XamlLight
    {
        #region Properties

        /// <summary>
        /// Gets or sets the intensity of the light. A higher value will result in a brighter light
        /// </summary>
        public byte Shade
        {
            get => GetValue(ShadeProperty).To<byte>();
            set => SetValue(ShadeProperty, value);
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
                    A = byte.MaxValue,
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
            get => (float)GetValue(OuterConeAngleProperty);
            set => SetValue(OuterConeAngleProperty, value);
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

        // The constant attenuation value to use when the light is inactive
        private const int InactiveAttenuationValue = 50;

        /// <summary>
        /// Gets or sets whether or not the light is active
        /// </summary>
        public bool Active
        {
            get => GetValue(ActiveProperty).To<bool>();
            set => SetValue(ActiveProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Active"/> property
        /// </summary>
        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register(nameof(Active), typeof(bool), typeof(PointerPositionSpotLight), new PropertyMetadata(true, OnActivePropertyChanged));

        private static void OnActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PointerPositionSpotLight l = d as PointerPositionSpotLight;
            l?._Light?.StartAnimationAsync("ConstantAttenuation", e.NewValue.To<bool>() ? 0 : InactiveAttenuationValue, TimeSpan.FromMilliseconds(250));
        }

        #endregion

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
        private SpotLight _Light;

        // The source compositor
        private Compositor _Compositor;

        // The expression animation for the light position
        private ExpressionAnimation _Animation;

        // The properties for the animation
        private CompositionPropertySet _Properties;

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
                _Light.InnerConeColor = _Light.OuterConeColor = Color.FromArgb(byte.MaxValue, Shade, Shade, Shade);
                _Light.InnerConeAngleInDegrees = 0;
                _Light.OuterConeAngleInDegrees = OuterConeAngle;
                _Light.ConstantAttenuation = Active ? 0 : InactiveAttenuationValue;
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
