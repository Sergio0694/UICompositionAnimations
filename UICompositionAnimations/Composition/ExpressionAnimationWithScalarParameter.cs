using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;

namespace UICompositionAnimations.Composition
{
    /// <summary>
    /// A simple wrapper <see langword="class"/> around an <see cref="ExpressionAnimation"/> instance with a custom parameter
    /// </summary>
    public sealed class ExpressionAnimationWithScalarParameter : DependencyObject
    {
        /// <summary>
        /// Gets the actual <see cref="ExpressionAnimation"/> instance that's being played
        /// </summary>
        [NotNull]
        public ExpressionAnimation Animation { get; }

        // Variable parameter name
        private readonly string ParameterName;

        // Target property set with the variable parameter
        private readonly CompositionPropertySet PropertySet;

        /// <summary>
        /// Internal constructor for a target animation
        /// </summary>
        /// <param name="animation">The new animation that will be played</param>
        /// <param name="propertySet">The property set with the custom parameter</param>
        /// <param name="parameterName">The name of the custom parameter that will be updated when requested</param>
        internal ExpressionAnimationWithScalarParameter([NotNull] ExpressionAnimation animation, 
            [NotNull] CompositionPropertySet propertySet, [NotNull] string parameterName)
        {
            Animation = animation;
            PropertySet = propertySet;
            ParameterName = parameterName;
        }

        /// <summary>
        /// Gets or sets the custom parameter that's being used in the <see cref="ExpressionAnimation"/>
        /// </summary>
        public float Parameter
        {
            get => GetValue(ParameterProperty).To<float>();
            set => SetValue(ParameterProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DependencyProperty"/> for the <see cref="Parameter"/> property
        /// </summary>
        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
            nameof(Parameter), typeof(float), typeof(ExpressionAnimationWithScalarParameter), 
            new PropertyMetadata(default(float), OnParameterPropertyChanged));

        private static void OnParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Unpack
            ExpressionAnimationWithScalarParameter @this = d.To<ExpressionAnimationWithScalarParameter>();
            float value = e.NewValue.To<float>();

            // Update the parameter when needed
            CompositionGetValueStatus status = @this.PropertySet.TryGetScalar(@this.ParameterName, out float old);
            if (status != CompositionGetValueStatus.Succeeded) throw new InvalidOperationException("The target parameter has not been found");
            float delta = value - old,
                abs = delta >= 0 ? delta : -delta;
            if (abs > 0.1) @this.PropertySet.InsertScalar(@this.ParameterName, value);
        }
    }
}
