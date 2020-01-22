using System;
using System.Numerics;
using Windows.UI.Xaml;
using FluentExtensions.UI.Animations.Enums;

#nullable enable

namespace Windows.UI.Composition
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Visual"/> type
    /// </summary>
    public static class VisualExtensions
    {
        /// <summary>
        /// Stops the animations with the target names on the given <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="properties">The names of the animations to stop</param>
        public static void StopAnimations(this Visual visual, params string[] properties)
        {
            if (properties.Length == 0) return;

            foreach (string property in properties)
            {
                visual.StopAnimation(property);
            }
        }

        /// <summary>
        /// Gets the translation property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="axis">The <see cref="Visual.Offset"/> axis to retrieve the value for</param>
        /// <returns>The translation value for the specified axis</returns>
        public static float GetTranslation(this Visual visual, Axis axis)
        {
            return axis switch
            {
                Axis.X => visual.TransformMatrix.Translation.X,
                Axis.Y => visual.TransformMatrix.Translation.Y,
                Axis.Z => visual.TransformMatrix.Translation.Z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis: {axis}")
            };
        }

        /// <summary>
        /// Sets the translation property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="axis">The <see cref="Visual.Offset"/> axis to edit</param>
        /// <param name="translation">The translation value to set for that axis</param>
        public static void SetTranslation(this Visual visual, Axis axis, float translation)
        {
            Vector3 translation3 = visual.TransformMatrix.Translation;

            switch (axis)
            {
                case Axis.X: translation3.X = translation; break;
                case Axis.Y: translation3.Y = translation; break;
                case Axis.Z: translation3.Z = translation; break;
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis: {axis}");
            }

            Matrix4x4 translation4x4 = visual.TransformMatrix;
            translation4x4.Translation = translation3;

            visual.TransformMatrix = translation4x4;
        }

        /// <summary>
        /// Sets the translation property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="x">The horizontal translation value</param>
        /// <param name="y">The vertical translation value</param>
        public static void SetTranslation(this Visual visual, float x, float y)
        {
            Vector3 translation3 = visual.TransformMatrix.Translation;

            translation3.X = x;
            translation3.Y = y;

            Matrix4x4 translation4x4 = visual.TransformMatrix;
            translation4x4.Translation = translation3;

            visual.TransformMatrix = translation4x4;
        }

        /// <summary>
        /// Sets the translation property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="x">The horizontal translation value</param>
        /// <param name="y">The vertical translation value</param>
        /// <param name="z">The depth translation value</param>
        public static void SetTranslation(this Visual visual, float x, float y, float z)
        {
            Vector3 translation3 = visual.TransformMatrix.Translation;

            translation3.X = x;
            translation3.Y = y;
            translation3.Z = z;

            Matrix4x4 translation4x4 = visual.TransformMatrix;
            translation4x4.Translation = translation3;

            visual.TransformMatrix = translation4x4;
        }

        /// <summary>
        /// Gets the <see cref="Visual.Offset"/> property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="axis">The <see cref="Visual.Offset"/> axis to retrieve the value for</param>
        public static float GetOffset(this Visual visual, Axis axis)
        {
            return axis switch
            {
                Axis.X => visual.Offset.X,
                Axis.Y => visual.Offset.Y,
                Axis.Z => visual.Offset.Z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis: {axis}")
            };
        }

        /// <summary>
        /// Sets the <see cref="Visual.Offset"/> property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="axis">The <see cref="Visual.Offset"/> axis to edit</param>
        /// <param name="offset">The <see cref="Visual.Offset"/> value to set for that axis</param>
        public static void SetOffset(this Visual visual, Axis axis, float offset)
        {
            Vector3 offset3 = visual.Offset;

            switch (axis)
            {
                case Axis.X: offset3.X = offset; break;
                case Axis.Y: offset3.Y = offset; break;
                case Axis.Z: offset3.Z = offset; break;
                default: throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis: {axis}");
            }

            visual.Offset = offset3;
        }

        /// <summary>
        /// Sets the <see cref="Visual.Scale"/> property of a target <see cref="Visual"/> instance
        /// </summary>
        /// <param name="visual">The target <see cref="Visual"/> instance</param>
        /// <param name="x">The X value of the <see cref="Visual.Scale"/> property</param>
        /// <param name="y">The Y value of the <see cref="Visual.Scale"/> property</param>
        /// <param name="z">The Z value of the <see cref="Visual.Scale"/> property</param>
        public static void SetScale(this Visual visual, float? x, float? y, float? z)
        {
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
        public static void SetVisualClip(this UIElement element, Thickness clip)
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
        public static void SetVisualClip(this UIElement element, Side side, float clip)
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
    }
}
