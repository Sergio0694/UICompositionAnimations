using System;
using System.Numerics;
using Windows.UI.Composition;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Composition
{
    /// <summary>
    /// A static class that generates a CubicBeizer curve from an input easing function name
    /// </summary>
    internal static class CubicBeizerEasingProvider
    {
        public static CubicBezierEasingFunction GetCubicBeizerFunction(CompositionObject compObject, float x1, float y1, float x2, float y2)
        {
            return compObject.Compositor.CreateCubicBezierEasingFunction(new Vector2 { X = x1, Y = y1 }, new Vector2 { X = x2, Y = y2 });
        }

        public static CubicBezierEasingFunction GetEasingFunction(this CompositionObject compObject, EasingFunctionNames ease)
        {
            switch (ease)
            {
                case EasingFunctionNames.Linear: return GetCubicBeizerFunction(compObject, 0, 0, 1, 1);
                case EasingFunctionNames.SineEaseIn: return GetCubicBeizerFunction(compObject, 0.4f, 0, 1, 1);
                case EasingFunctionNames.SineEaseOut: return GetCubicBeizerFunction(compObject, 0, 0, 0.6f, 1);
                case EasingFunctionNames.SineEaseInOut: return GetCubicBeizerFunction(compObject, 0.4f, 0, 0.6f, 1);
                case EasingFunctionNames.QuadraticEaseIn: return GetCubicBeizerFunction(compObject, 0.8f, 0, 1, 1);
                case EasingFunctionNames.QuadraticEaseOut: return GetCubicBeizerFunction(compObject, 0, 0, 0.2f, 1);
                case EasingFunctionNames.QuadraticEaseInOut: return GetCubicBeizerFunction(compObject, 0.8f, 0, 0.2f, 1);
                case EasingFunctionNames.CircleEaseIn: return GetCubicBeizerFunction(compObject, 1, 0, 1, 0.8f);
                case EasingFunctionNames.CircleEaseOut: return GetCubicBeizerFunction(compObject, 0, 0.3f, 0, 1);
                case EasingFunctionNames.CircleEaseInOut: return GetCubicBeizerFunction(compObject, 0.9f, 0, 0.1f, 1);
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }
    }
}