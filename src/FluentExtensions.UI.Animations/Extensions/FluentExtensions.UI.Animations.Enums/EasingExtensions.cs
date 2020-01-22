using System;
using System.Diagnostics.Contracts;
using Windows.UI.Xaml.Media.Animation;

#nullable enable

namespace FluentExtensions.UI.Animations.Enums
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Easing"/> type
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Converts an easing value to the right easing function
        /// </summary>
        /// <param name="ease">The desired easing function</param>
        [Pure]
        public static EasingFunctionBase? ToEasingFunction(this Easing ease)
        {
            return ease switch
            {
                Easing.Linear => null,
                Easing.SineEaseIn => new SineEase { EasingMode = EasingMode.EaseIn },
                Easing.SineEaseOut => new SineEase { EasingMode = EasingMode.EaseOut },
                Easing.SineEaseInOut => new SineEase { EasingMode = EasingMode.EaseInOut },
                Easing.QuadraticEaseIn => new QuadraticEase { EasingMode = EasingMode.EaseIn },
                Easing.QuadraticEaseOut => new QuadraticEase { EasingMode = EasingMode.EaseOut },
                Easing.QuadraticEaseInOut => new QuadraticEase { EasingMode = EasingMode.EaseInOut },
                Easing.CircleEaseIn => new CircleEase { EasingMode = EasingMode.EaseIn },
                Easing.CircleEaseOut => new CircleEase { EasingMode = EasingMode.EaseOut },
                Easing.CircleEaseInOut => new CircleEase { EasingMode = EasingMode.EaseInOut },
                _ => throw new ArgumentOutOfRangeException(nameof(ease), ease, "Invalid easing value")
            };
        }
    }
}
