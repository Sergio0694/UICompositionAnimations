using System;
using System.Numerics;
using Windows.UI.Composition;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Composition
{
    /// <summary>
    /// A <see langword="static"/> <see langword="class"/> that generates a cubic beizer curve from an input easing function name
    /// </summary>
    internal static class CubicBeizerEasingProvider
    {
        /// <summary>
        /// Creates a <see cref="CubicBezierEasingFunction"/> from the input control points
        /// </summary>
        /// <param name="compObject">The source object used to create the easing function</param>
        /// <param name="x1">The X coordinate of the first control point</param>
        /// <param name="y1">The Y coordinate of the first control point</param>
        /// <param name="x2">The X coordinate of the second control point</param>
        /// <param name="y2">The Y coordinate of the second control point</param>
        [Pure, NotNull]
        public static CubicBezierEasingFunction GetEasingFunction([NotNull] this CompositionObject compObject, float x1, float y1, float x2, float y2)
        {
            return compObject.Compositor.CreateCubicBezierEasingFunction(new Vector2 { X = x1, Y = y1 }, new Vector2 { X = x2, Y = y2 });
        }

        /// <summary>
        /// Creates the appropriate <see cref="CubicBezierEasingFunction"/> from the given easing function name
        /// </summary>
        /// <param name="compObject">The source object used to create the easing function</param>
        /// <param name="ease">The target easing function to create</param>
        [Pure, NotNull]
        public static CubicBezierEasingFunction GetEasingFunction([NotNull] this CompositionObject compObject, EasingFunctionNames ease)
        {
            switch (ease)
            {
                case EasingFunctionNames.Linear: return compObject.GetEasingFunction(0, 0, 1, 1);
                case EasingFunctionNames.SineEaseIn: return compObject.GetEasingFunction(0.4f, 0, 1, 1);
                case EasingFunctionNames.SineEaseOut: return compObject.GetEasingFunction(0, 0, 0.6f, 1);
                case EasingFunctionNames.SineEaseInOut: return compObject.GetEasingFunction(0.4f, 0, 0.6f, 1);
                case EasingFunctionNames.QuadraticEaseIn: return compObject.GetEasingFunction(0.8f, 0, 1, 1);
                case EasingFunctionNames.QuadraticEaseOut: return compObject.GetEasingFunction(0, 0, 0.2f, 1);
                case EasingFunctionNames.QuadraticEaseInOut: return compObject.GetEasingFunction(0.8f, 0, 0.2f, 1);
                case EasingFunctionNames.CircleEaseIn: return compObject.GetEasingFunction(1, 0, 1, 0.8f);
                case EasingFunctionNames.CircleEaseOut: return compObject.GetEasingFunction(0, 0.3f, 0, 1);
                case EasingFunctionNames.CircleEaseInOut: return compObject.GetEasingFunction(0.9f, 0, 0.1f, 1);
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }
    }
}