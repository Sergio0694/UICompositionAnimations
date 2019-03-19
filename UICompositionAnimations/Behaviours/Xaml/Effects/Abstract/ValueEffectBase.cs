namespace UICompositionAnimations.Behaviours.Xaml.Effects.Abstract
{
    /// <summary>
    /// A base <see langword="class"/> for an effect that exposes a single <see cref="float"/> parameter
    /// </summary>
    public abstract class ValueEffectBase : IPipelineEffect
    {
        /// <summary>
        /// Gets or sets the value of the parameter for the current effect
        /// </summary>
        public double Value { get; set; }
    }
}
