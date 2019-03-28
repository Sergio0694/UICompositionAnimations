using System;
using Windows.UI.Xaml;
using JetBrains.Annotations;

namespace Windows.UI.Composition
{
    /// <summary>
    /// A simple wrapper <see langword="class"/> around an <see cref="ExpressionAnimation"/> instance with a custom parameter
    /// </summary>
    public sealed class ScalarExpressionAnimation : DependencyObject
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
        /// Internal constructor for a target <see cref="ExpressionAnimation"/>
        /// </summary>
        /// <param name="animation">The <see cref="ExpressionAnimation"/> that will be played</param>
        /// <param name="propertySet">The <see cref="CompositionPropertySet"/> with the custom parameter</param>
        /// <param name="parameterName">The name of the custom parameter that will be updated when requested</param>
        internal ScalarExpressionAnimation([NotNull] ExpressionAnimation animation, [NotNull] CompositionPropertySet propertySet, [NotNull] string parameterName)
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
            nameof(Parameter),
            typeof(float),
            typeof(ScalarExpressionAnimation),
            new PropertyMetadata(default(float), OnParameterPropertyChanged));

        private static void OnParameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Unpack
            ScalarExpressionAnimation @this = d.To<ScalarExpressionAnimation>();
            float value = e.NewValue.To<float>();

            // Update the parameter when needed
            CompositionGetValueStatus status = @this.PropertySet.TryGetScalar(@this.ParameterName, out float old);
            if (status != CompositionGetValueStatus.Succeeded) throw new InvalidOperationException("The target parameter has not been found");
            float
                delta = value - old,
                abs = delta >= 0 ? delta : -delta;
            if (abs > 0.1) @this.PropertySet.InsertScalar(@this.ParameterName, value);
        }
    }
}
