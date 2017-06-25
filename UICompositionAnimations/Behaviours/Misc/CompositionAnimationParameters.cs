using System;
using JetBrains.Annotations;

namespace UICompositionAnimations.Behaviours.Misc
{
    /// <summary>
    /// A class that stores the parameters for two different states for a pre-defined composition animation
    /// </summary>
    internal class CompositionAnimationValueParameters
    {
        /// <summary>
        /// Gets the target value when the effect is enabled
        /// </summary>
        public float On { get; }

        /// <summary>
        /// Gets the target value when the effect is disabled
        /// </summary>
        public float Off { get; }

        /// <summary>
        /// Creates a new instance for the given states
        /// </summary>
        /// <param name="on">The on parameter</param>
        /// <param name="off">The off parameter</param>
        public CompositionAnimationValueParameters(float on, float off)
        {
            On = on;
            Off = off;
        }
    }

    /// <summary>
    /// A class that stores the parameters for two different states for a pre-defined composition animation, along with the target property to animate
    /// </summary>
    internal sealed class CompositionAnimationParameters : CompositionAnimationValueParameters
    {
        /// <summary>
        /// Gets the property to animate
        /// </summary>
        [NotNull]
        public String Property { get; }

        /// <summary>
        /// Creates a new instance for the target property and states
        /// </summary>
        /// <param name="property">The target property to animate</param>
        /// <param name="on">The on parameter</param>
        /// <param name="off">The off parameter</param>
        public CompositionAnimationParameters([NotNull] String property, float on, float off) : base(on, off)
        {
            Property = property;
        }
    }
}
