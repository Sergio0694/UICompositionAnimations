using System;
using Windows.UI.Xaml.Media.Animation;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.XAMLTransform
{
    /// <summary>
    /// A simple class that creates the desired XAML transform easing function
    /// </summary>
    internal static class EasingFunctionBaseProvider
    {
        /// <summary>
        /// Converts an easing function name to the right easing function
        /// </summary>
        /// <param name="ease">The desired easing function</param>
        public static EasingFunctionBase ToEasingFunction(this EasingFunctionNames ease)
        {
            switch (ease)
            {
                case EasingFunctionNames.Linear: return null;
                case EasingFunctionNames.SineEaseIn: return new SineEase { EasingMode = EasingMode.EaseIn };
                case EasingFunctionNames.SineEaseOut: return new SineEase { EasingMode = EasingMode.EaseOut };
                case EasingFunctionNames.SineEaseInOut: return new SineEase { EasingMode = EasingMode.EaseInOut };
                case EasingFunctionNames.QuadraticEaseIn: return new QuadraticEase { EasingMode = EasingMode.EaseIn };
                case EasingFunctionNames.QuadraticEaseOut: return new QuadraticEase { EasingMode = EasingMode.EaseOut };
                case EasingFunctionNames.QuadraticEaseInOut: return new QuadraticEase { EasingMode = EasingMode.EaseInOut };
                case EasingFunctionNames.CircleEaseIn: return new CircleEase { EasingMode = EasingMode.EaseIn };
                case EasingFunctionNames.CircleEaseOut: return new CircleEase { EasingMode = EasingMode.EaseOut };
                case EasingFunctionNames.CircleEaseInOut: return new CircleEase { EasingMode = EasingMode.EaseInOut };
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }
    }
}
