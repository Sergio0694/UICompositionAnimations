using System;
using System.Numerics;

namespace Windows.UI.Composition
{
    public static class CubicEaseProvider
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
                case EasingFunctionNames.SineEaseInOut: return GetCubicBeizerFunction(compObject, 0, 0.4f, 0.6f, 1);
                case EasingFunctionNames.QuadraticEaseIn: return GetCubicBeizerFunction(compObject, 0, 0.8f, 1, 1);
                case EasingFunctionNames.QuadraticEaseOut: return GetCubicBeizerFunction(compObject, 0, 0.4f, 0.8f, 1);
                case EasingFunctionNames.QuadraticEaseInOut: return GetCubicBeizerFunction(compObject, 0, 0.6f, 0.3f, 1);
                default: throw new ArgumentOutOfRangeException(nameof(ease), ease, "This shouldn't happen");
            }
        }
    }
}