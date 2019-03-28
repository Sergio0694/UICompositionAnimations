using System;
using Windows.UI.Xaml.Media.Animation;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="DependencyObject"/> type
    /// </summary>
    [PublicAPI]
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="from">The initial property value</param>
        /// <param name="to">The final property value</param>
        /// <param name="ms">The duration of the <see cref="DoubleAnimation"/></param>
        /// <param name="easing">The easing function to use inside the <see cref="DoubleAnimation"/></param>
        /// <param name="enableDependecyAnimations">Indicates whether or not to apply this animation to elements that need the visual tree to be rearranged</param>
        public static DoubleAnimation CreateDoubleAnimation(
            [NotNull] this DependencyObject target, string property,
            double? from, double? to,
            int ms,
            Easing easing = Easing.Linear,
            bool enableDependecyAnimations = false)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(ms)),
                EnableDependentAnimation = enableDependecyAnimations
            };
            if (easing != Easing.Linear) animation.EasingFunction = easing.ToEasingFunction();
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);
            return animation;
        }
    }
}
