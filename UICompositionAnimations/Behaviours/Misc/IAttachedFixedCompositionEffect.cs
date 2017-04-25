using System;
using System.Threading.Tasks;

namespace UICompositionAnimations.Behaviours.Misc
{
    /// <summary>
    /// An interface for a ready to use composition effect applied to a <see cref="Windows.UI.Xaml.FrameworkElement"/> that supports animations
    /// </summary>
    public interface IAttachedFixedCompositionEffect : IAttachedCompositionEffect
    {
        /// <summary>
        /// Executes the animation to the desired destination status and returns a task that completes when the animation ends
        /// </summary>
        /// <param name="animationType">The target animation status</param>
        /// <param name="duration">The animation duration</param>
        Task AnimateAsync(FixedAnimationType animationType, TimeSpan duration);
    }
}