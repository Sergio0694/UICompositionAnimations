using System;
using Windows.UI.Xaml.Media.Animation;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Extensions
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Enum"/> types in the library
    /// </summary>
    internal static class EnumExtensions
    {
        /// <summary>
        /// Converts an easing value to the right easing function
        /// </summary>
        /// <param name="ease">The desired easing function</param>
        public static EasingFunctionBase ToEasingFunction(this Easing ease)
        {
            switch (ease)
            {
                case Easing.Linear: return null;
                case Easing.SineEaseIn: return new SineEase { EasingMode = EasingMode.EaseIn };
                case Easing.SineEaseOut: return new SineEase { EasingMode = EasingMode.EaseOut };
                case Easing.SineEaseInOut: return new SineEase { EasingMode = EasingMode.EaseInOut };
                case Easing.QuadraticEaseIn: return new QuadraticEase { EasingMode = EasingMode.EaseIn };
                case Easing.QuadraticEaseOut: return new QuadraticEase { EasingMode = EasingMode.EaseOut };
                case Easing.QuadraticEaseInOut: return new QuadraticEase { EasingMode = EasingMode.EaseInOut };
                case Easing.CircleEaseIn: return new CircleEase { EasingMode = EasingMode.EaseIn };
                case Easing.CircleEaseOut: return new CircleEase { EasingMode = EasingMode.EaseOut };
                case Easing.CircleEaseInOut: return new CircleEase { EasingMode = EasingMode.EaseInOut };
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }
    }
}
