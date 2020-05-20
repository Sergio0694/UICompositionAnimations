using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using JetBrains.Annotations;
using UICompositionAnimations.Animations;
using UICompositionAnimations.Animations.Interfaces;
using UICompositionAnimations.Enums;

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="UIElement"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class UIElementExtensions
    {
        /// <summary>
        /// Returns the Visual object for a given UIElement
        /// </summary>
        /// <param name="element">The source UIElement</param>
        [Pure, NotNull]
        public static Visual GetVisual([NotNull] this UIElement element) => ElementCompositionPreview.GetElementVisual(element);

        #region Animations

        /// <summary>
        /// Initializes an <see cref="IAnimationBuilder"/> instance that targets the input <see cref="UIElement"/>
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to animate</param>
        /// <param name="layer">The target layer to animate</param>
        [Pure, NotNull]
        public static IAnimationBuilder Animation([NotNull] this UIElement target, FrameworkLayer layer = FrameworkLayer.Composition)
        {
            switch (layer)
            {
                case FrameworkLayer.Composition: return new CompositionAnimationBuilder(target);
                case FrameworkLayer.Xaml: return new XamlAnimationBuilder(target);
                default: throw new ArgumentOutOfRangeException(nameof(layer), layer, $"The {layer} value isn't valid");
            }
        }

        /// <summary>
        /// Creates and starts a scalar animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginScalarAnimation(
            [NotNull] this UIElement element,
            [NotNull] string propertyPath,
            float? from, float to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            element.GetVisual().BeginScalarAnimation(propertyPath, from, to, duration, delay, ease);
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector2"/> animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginVector2Animation(
            [NotNull] this UIElement element,
            [NotNull] string propertyPath,
            Vector2? from, Vector2 to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            element.GetVisual().BeginVector2Animation(propertyPath, from, to, duration, delay, ease);
        }

        /// <summary>
        /// Creates and starts a <see cref="Vector3"/> animation on the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to animate</param>
        /// <param name="propertyPath">The path that identifies the property to animate</param>
        /// <param name="from">The optional starting value for the animation</param>
        /// <param name="to">The final value for the animation</param>
        /// <param name="duration">The animation duration</param>
        /// <param name="delay">The optional initial delay for the animation</param>
        /// <param name="ease">The optional easing function for the animation</param>
        public static void BeginVector3Animation(
            [NotNull] this UIElement element,
            [NotNull] string propertyPath,
            Vector3? from, Vector3 to,
            TimeSpan duration, TimeSpan? delay,
            [CanBeNull] CompositionEasingFunction ease = null)
        {
            element.GetVisual().BeginVector3Animation(propertyPath, from, to, duration, delay, ease);
        }

        #endregion

        #region Property setters

        /// <summary>
        /// Returns the desired <see cref="Transform"/> instance after assigning it to the <see cref="UIElement.RenderTransform"/> property of the target <see cref="UIElement"/>
        /// </summary>
        /// <typeparam name="T">The desired <see cref="Transform"/> type</typeparam>
        /// <param name="element">The target <see cref="UIElement"/> to modify</param>
        /// <param name="reset">If <see langword="true"/>, a new <see cref="Transform"/> instance will always be created and assigned to the <see cref="UIElement"/></param>
        [MustUseReturnValue, NotNull]
        public static T GetTransform<T>([NotNull] this UIElement element, bool reset = true) where T : Transform, new()
        {
            // Return the existing transform object, if it exists
            if (element.RenderTransform is T && !reset) return (T)element.RenderTransform;

            // Create a new transform
            T transform = new T();
            element.RenderTransform = transform;
            return transform;
        }

        /// <summary>
        /// Stops the animations with the target names on the given <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="properties">The names of the animations to stop</param>
        public static void StopVisualAnimations([NotNull] this UIElement element, [NotNull, ItemNotNull] params string[] properties)
        {
            if (properties.Length == 0) return;
            Visual visual = element.GetVisual();
            foreach (string property in properties)
                visual.StopAnimation(property);
        }

        /// <summary>
        /// Sets the translation property of the <see cref="Visual"/> instance for a given <see cref="UIElement"/> object
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="axis">The <see cref="Visual.Offset"/> axis to edit</param>
        /// <param name="translation">The translation value to set for that axis</param>
        public static void SetVisualTranslation([NotNull] this UIElement element, Axis axis, float translation)
        {
            // Get the element visual and stop the animation
            Visual visual = element.GetVisual();
            ElementCompositionPreview.SetIsTranslationEnabled(element, true);

            // Set the desired offset
            Matrix4x4 transform = visual.TransformMatrix;
            Vector3 translation3 = visual.TransformMatrix.Translation;
            if (axis == Axis.X) translation3.X = translation;
            else translation3.Y = translation;
            transform.Translation = translation3;
            visual.TransformMatrix = transform;
        }

        /// <summary>
        /// Sets the translation property of the <see cref="Visual"/> instance for a given <see cref="UIElement"/> object
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="x">The horizontal translation value</param>
        /// <param name="y">The vertical translation value</param>
        public static void SetVisualTranslation([NotNull] this UIElement element, float x, float y)
        {
            // Get the element visual and stop the animation
            Visual visual = element.GetVisual();
            ElementCompositionPreview.SetIsTranslationEnabled(element, true);

            // Set the desired offset
            Matrix4x4 transform = visual.TransformMatrix;
            Vector3 translation3 = visual.TransformMatrix.Translation;
            translation3.X = x;
            translation3.Y = y;
            transform.Translation = translation3;
            visual.TransformMatrix = transform;
        }

        /// <summary>
        /// Sets the <see cref="Visual.Offset"/> property of the <see cref="Visual"/> instance for a given <see cref="UIElement"/> object
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="axis">The <see cref="Visual.Offset"/> axis to edit</param>
        /// <param name="offset">The <see cref="Visual.Offset"/> value to set for that axis</param>
        public static void SetVisualOffset([NotNull] this UIElement element, Axis axis, float offset)
        {
            // Get the element visual and stop the animation
            Visual visual = element.GetVisual();

            // Set the desired offset
            Vector3 offset3 = visual.Offset;
            if (axis == Axis.X) offset3.X = offset;
            else offset3.Y = offset;
            visual.Offset = offset3;
        }

        /// <summary>
        /// Sets the <see cref="Visual.Scale"/> property of the <see cref="Visual"/> instance for a given <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="x">The X value of the <see cref="Visual.Scale"/> property</param>
        /// <param name="y">The Y value of the <see cref="Visual.Scale"/> property</param>
        /// <param name="z">The Z value of the <see cref="Visual.Scale"/> property</param>
        public static void SetVisualScale([NotNull] this UIElement element, float? x, float? y, float? z)
        {
            // Get the default values and set the CenterPoint
            Visual visual = element.GetVisual();

            // Set the scale property
            if (x == null && y == null && z == null) return;
            Vector3 targetScale = new Vector3
            {
                X = x ?? visual.Scale.X,
                Y = y ?? visual.Scale.Y,
                Z = z ?? visual.Scale.Z
            };
            visual.Scale = targetScale;
        }

        /// <summary>
        /// Sets the clip property of the visual instance for a given <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="clip">The desired clip margins to set</param>
        public static void SetVisualClip([NotNull] this UIElement element, Thickness clip)
        {
            // Get the element visual
            Visual visual = element.GetVisual();

            // Set the desired clip
            InsetClip inset = visual.Clip as InsetClip ?? (InsetClip)(visual.Clip = visual.Compositor.CreateInsetClip());
            inset.TopInset = (float)clip.Top;
            inset.BottomInset = (float)clip.Bottom;
            inset.LeftInset = (float)clip.Left;
            inset.RightInset = (float)clip.Right;
            visual.Clip = inset;
        }

        /// <summary>
        /// Sets the clip property of the visual instance for a given <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target <see cref="UIElement"/></param>
        /// <param name="side">The target clip side to update</param>
        /// <param name="clip">The desired clip value to set</param>
        public static void SetVisualClip([NotNull] this UIElement element, Side side, float clip)
        {
            // Get the element visual
            Visual visual = element.GetVisual();

            // Set the desired clip
            InsetClip inset = visual.Clip as InsetClip ?? (InsetClip)(visual.Clip = visual.Compositor.CreateInsetClip());
            switch (side)
            {
                case Side.Top: inset.TopInset = clip; break;
                case Side.Bottom: inset.BottomInset = clip; break;
                case Side.Right: inset.RightInset = clip; break;
                case Side.Left: inset.LeftInset = clip; break;
                default: throw new ArgumentException("Invalid side", nameof(side));
            }
        }

        #endregion
    }
}
