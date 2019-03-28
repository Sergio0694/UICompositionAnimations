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
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="x1">The X coordinate of the first control point</param>
        /// <param name="y1">The Y coordinate of the first control point</param>
        /// <param name="x2">The X coordinate of the second control point</param>
        /// <param name="y2">The Y coordinate of the second control point</param>
        [Pure, NotNull]
        public static CubicBezierEasingFunction GetEasingFunction([NotNull] this CompositionObject source, float x1, float y1, float x2, float y2)
        {
            return source.Compositor.CreateCubicBezierEasingFunction(new Vector2 { X = x1, Y = y1 }, new Vector2 { X = x2, Y = y2 });
        }

        /// <summary>
        /// Creates the appropriate <see cref="CubicBezierEasingFunction"/> from the given easing function name
        /// </summary>
        /// <param name="source">The source <see cref="CompositionObject"/> used to create the easing function</param>
        /// <param name="ease">The target easing function to create</param>
        [Pure, NotNull]
        public static CubicBezierEasingFunction GetEasingFunction([NotNull] this CompositionObject source, Easing ease)
        {
            switch (ease)
            {
                case Easing.Linear: return source.GetEasingFunction(0, 0, 1, 1);
                case Easing.SineEaseIn: return source.GetEasingFunction(0.4f, 0, 1, 1);
                case Easing.SineEaseOut: return source.GetEasingFunction(0, 0, 0.6f, 1);
                case Easing.SineEaseInOut: return source.GetEasingFunction(0.4f, 0, 0.6f, 1);
                case Easing.QuadraticEaseIn: return source.GetEasingFunction(0.8f, 0, 1, 1);
                case Easing.QuadraticEaseOut: return source.GetEasingFunction(0, 0, 0.2f, 1);
                case Easing.QuadraticEaseInOut: return source.GetEasingFunction(0.8f, 0, 0.2f, 1);
                case Easing.CircleEaseIn: return source.GetEasingFunction(1, 0, 1, 0.8f);
                case Easing.CircleEaseOut: return source.GetEasingFunction(0, 0.3f, 0, 1);
                case Easing.CircleEaseInOut: return source.GetEasingFunction(0.9f, 0, 0.1f, 1);
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }
    }
}