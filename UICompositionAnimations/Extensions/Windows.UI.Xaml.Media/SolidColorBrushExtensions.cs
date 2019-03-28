using System;
using Windows.UI.Xaml.Media.Animation;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace Windows.UI.Xaml.Media
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="SolidColorBrushExtensions"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class SolidColorBrushExtensions
    {
        /// <summary>
        /// Animates the target <see cref="SolidColorBrush"/> to a given color
        /// </summary>
        /// <param name="solidColorBrush">The <see cref="SolidColorBrush"/> to animate</param>
        /// <param name="to">The target value to set</param>
        /// <param name="duration">The duration of the animation</param>
        /// <param name="easing">The easing function to use</param>
        [Pure, NotNull]
        public static ColorAnimation CreateColorAnimation([NotNull] this SolidColorBrush solidColorBrush, Color to, TimeSpan duration, Easing easing)
        {
            ColorAnimation animation = new ColorAnimation
            {
                From = solidColorBrush.Color,
                To = to,
                Duration = duration,
                EasingFunction = easing.ToEasingFunction()
            };
            Storyboard.SetTarget(animation, solidColorBrush);
            Storyboard.SetTargetProperty(animation, nameof(SolidColorBrush.Color));
            return animation;
        }
    }
}
