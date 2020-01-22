using System;
using System.Numerics;
using System.Threading.Tasks;
using FluentExtensions.UI.Animations.Enums;

#nullable enable

namespace FluentExtensions.UI.Animations.Interfaces
{
    /// <summary>
    /// An <see langword="interface"/> that represents an animation builder that uses a specific set of APIs
    /// </summary>
    public interface IAnimationBuilder
    {
        /// <summary>
        /// Schedules an opacity animation
        /// </summary>
        /// <param name="to">The target opacity value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Opacity(double to);

        /// <summary>
        /// Schedules an opacity animation
        /// </summary>
        /// <param name="to">The target opacity value to animate to</param>
        /// <param name="ease">The easing function to use for the opacity animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Opacity(double to, Easing ease);

        /// <summary>
        /// Schedules an opacity animation
        /// </summary>
        /// <param name="from">The opacity value to animate from</param>
        /// <param name="to">The target opacity value to animate to</param>
        /// <param name="ease">The easing function to use for the opacity animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Opacity(double from, double to, Easing ease);

        /// <summary>
        /// Schedules a translation animation on a specified axis
        /// </summary>
        /// <param name="axis">The translation axis to animate</param>
        /// <param name="to">The target translation value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Axis axis, double to);

        /// <summary>
        /// Schedules a translation animation on a specified axis
        /// </summary>
        /// <param name="axis">The translation axis to animate</param>
        /// <param name="to">The target translation value to animate to</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Axis axis, double to, Easing ease);

        /// <summary>
        /// Schedules a translation animation on a specified axis
        /// </summary>
        /// <param name="axis">The translation axis to animate</param>
        /// <param name="from">The initial translation value to animate from</param>
        /// <param name="to">The target translation value to animate to</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Axis axis, double from, double to, Easing ease);

        /// <summary>
        /// Schedules a translation animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target translation position to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Vector2 to);

        /// <summary>
        /// Schedules a translation animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target translation position to animate to</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Vector2 to, Easing ease);

        /// <summary>
        /// Schedules a translation animation on the X and Y axes
        /// </summary>
        /// <param name="from">The initial translation position to animate from</param>
        /// <param name="to">The target translation position to animate to</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Vector2 from, Vector2 to, Easing ease);

        /// <summary>
        /// Schedules a translation animation on all axes
        /// </summary>
        /// <param name="to">The target translation position to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Vector3 to);

        /// <summary>
        /// Schedules a translation animation on all axes
        /// </summary>
        /// <param name="to">The target translation position to animate to</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Vector3 to, Easing ease);

        /// <summary>
        /// Schedules a translation animation on all axes
        /// </summary>
        /// <param name="from">The initial translation position to animate from</param>
        /// <param name="to">The target translation position to animate to</param>
        /// <param name="ease">The easing function to use for the translation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Translation(Vector3 from, Vector3 to, Easing ease);

        /// <summary>
        /// Schedules an offset animation on a specified axis
        /// </summary>
        /// <param name="axis">The offset axis to animate</param>
        /// <param name="to">The target offset value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Axis axis, double to);

        /// <summary>
        /// Schedules an offset animation on a specified axis
        /// </summary>
        /// <param name="axis">The offset axis to animate</param>
        /// <param name="to">The target offset value to animate to</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Axis axis, double to, Easing ease);

        /// <summary>
        /// Schedules an offset animation on a specified axis
        /// </summary>
        /// <param name="axis">The offset axis to animate</param>
        /// <param name="from">The initial offset value to animate from</param>
        /// <param name="to">The target offset value to animate to</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Axis axis, double from, double to, Easing ease);

        /// <summary>
        /// Schedules an offset animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target offset vector to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Vector2 to);

        /// <summary>
        /// Schedules an offset animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target offset vector to animate to</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Vector2 to, Easing ease);

        /// <summary>
        /// Schedules an offset animation on the X and Y axes
        /// </summary>
        /// <param name="from">The initial offset vector to animate from</param>
        /// <param name="to">The target offset vector to animate to</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Vector2 from, Vector2 to, Easing ease);

        /// <summary>
        /// Schedules an offset animation on all axes
        /// </summary>
        /// <param name="to">The target offset vector to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Vector3 to);

        /// <summary>
        /// Schedules an offset animation on all axes
        /// </summary>
        /// <param name="to">The target offset vector to animate to</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Vector3 to, Easing ease);

        /// <summary>
        /// Schedules an offset animation on all axes
        /// </summary>
        /// <param name="from">The initial offset vector to animate from</param>
        /// <param name="to">The target offset vector to animate to</param>
        /// <param name="ease">The easing function to use for the offset animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Offset(Vector3 from, Vector3 to, Easing ease);

        /// <summary>
        /// Schedules a uniform scale animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target scale value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(double to);

        /// <summary>
        /// Schedules a uniform scale animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(double to, Easing ease);

        /// <summary>
        /// Schedules a uniform scale animation on the X and Y axes
        /// </summary>
        /// <param name="from">The scale value to animate from</param>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(double from, double to, Easing ease);

        /// <summary>
        /// Schedules a scale animation on a specified axis
        /// </summary>
        /// <param name="axis">The scale axis to animate</param>
        /// <param name="to">The target scale value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Axis axis, double to);

        /// <summary>
        /// Schedules a scale animation on a specified axis
        /// </summary>
        /// <param name="axis">The scale axis to animate</param>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Axis axis, double to, Easing ease);

        /// <summary>
        /// Schedules a scale animation on a specified axis
        /// </summary>
        /// <param name="axis">The scale axis to animate</param>
        /// <param name="from">The scale value to animate from</param>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Axis axis, double from, double to, Easing ease);

        /// <summary>
        /// Schedules a scale animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target scale value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Vector2 to);

        /// <summary>
        /// Schedules a scale animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Vector2 to, Easing ease);

        /// <summary>
        /// Schedules a scale animation on the X and Y axes
        /// </summary>
        /// <param name="from">The scale value to animate from</param>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Vector2 from, Vector2 to, Easing ease);

        /// <summary>
        /// Schedules a scale animation on all axes
        /// </summary>
        /// <param name="to">The target scale value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Vector3 to);

        /// <summary>
        /// Schedules a scale animation on all axes
        /// </summary>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Vector3 to, Easing ease);

        /// <summary>
        /// Schedules a scale animation on all axes
        /// </summary>
        /// <param name="from">The scale value to animate from</param>
        /// <param name="to">The target scale value to animate to</param>
        /// <param name="ease">The easing function to use for the scale animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Scale(Vector3 from, Vector3 to, Easing ease);

        /// <summary>
        /// Schedules a rotation animation
        /// </summary>
        /// <param name="to">The target rotation value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Rotate(double to);

        /// <summary>
        /// Schedules a rotation animation
        /// </summary>
        /// <param name="to">The target rotation value to animate to</param>
        /// <param name="ease">The easing function to use for the rotation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Rotate(double to, Easing ease);

        /// <summary>
        /// Schedules a rotation animation
        /// </summary>
        /// <param name="from">The rotation value to animate from</param>
        /// <param name="to">The target rotation value to animate to</param>
        /// <param name="ease">The easing function to use for the rotation animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Rotate(double from, double to, Easing ease);

        /// <summary>
        /// Schedules a clip animation
        /// </summary>
        /// <param name="side">The clip side to animate</param>
        /// <param name="to">The target clip value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Clip(Side side, double to);

        /// <summary>
        /// Schedules a clip animation
        /// </summary>
        /// <param name="side">The clip side to animate</param>
        /// <param name="to">The target clip value to animate to</param>
        /// <param name="ease">The easing function to use for the clip animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Clip(Side side, double to, Easing ease);

        /// <summary>
        /// Schedules a clip animation
        /// </summary>
        /// <param name="side">The clip side to animate</param>
        /// <param name="from">The clip value to animate from</param>
        /// <param name="to">The target clip value to animate to</param>
        /// <param name="ease">The easing function to use for the clip animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Clip(Side side, double from, double to, Easing ease);

        /// <summary>
        /// Schedules a size animation on a specified axis
        /// </summary>
        /// <param name="axis">The size axis to animate</param>
        /// <param name="to">The target size value to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Axis axis, double to);

        /// <summary>
        /// Schedules a size animation on a specified axis
        /// </summary>
        /// <param name="axis">The size axis to animate</param>
        /// <param name="to">The target size value to animate to</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Axis axis, double to, Easing ease);

        /// <summary>
        /// Schedules a size animation on a specified axis
        /// </summary>
        /// <param name="axis">The size axis to animate</param>
        /// <param name="from">The initial size value to animate from</param>
        /// <param name="to">The target size value to animate to</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Axis axis, double from, double to, Easing ease);

        /// <summary>
        /// Schedules a size animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target size vector to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Vector2 to);

        /// <summary>
        /// Schedules a size animation on the X and Y axes
        /// </summary>
        /// <param name="to">The target size vector to animate to</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Vector2 to, Easing ease);

        /// <summary>
        /// Schedules a size animation on the X and Y axes
        /// </summary>
        /// <param name="from">The initial size vector to animate from</param>
        /// <param name="to">The target size vector to animate to</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Vector2 from, Vector2 to, Easing ease);

        /// <summary>
        /// Schedules a size animation on all axes
        /// </summary>
        /// <param name="to">The target size vector to animate to</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Vector3 to);

        /// <summary>
        /// Schedules a size animation on all axes
        /// </summary>
        /// <param name="to">The target size vector to animate to</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Vector3 to, Easing ease);

        /// <summary>
        /// Schedules a size animation on all axes
        /// </summary>
        /// <param name="from">The initial size vector to animate from</param>
        /// <param name="to">The target size vector to animate to</param>
        /// <param name="ease">The easing function to use for the size animation</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Size(Vector3 from, Vector3 to, Easing ease);

        /// <summary>
        /// Sets the duration of the animation
        /// </summary>
        /// <param name="ms">The animation duration, in milliseconds</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Duration(int ms);

        /// <summary>
        /// Sets the duration of the animation
        /// </summary>
        /// <param name="duration">The <see cref="TimeSpan"/> value indicating the animation duration</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Duration(TimeSpan duration);

        /// <summary>
        /// Sets the optional initial delay for the animation
        /// </summary>
        /// <param name="ms">The delay duration, in milliseconds</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Delay(int ms);

        /// <summary>
        /// Sets the optional initial delay for the animation
        /// </summary>
        /// <param name="duration">The <see cref="TimeSpan"/> value indicating the animation delay duration</param>
        /// <returns>The current <see cref="IAnimationBuilder"/> instance in use</returns>
        IAnimationBuilder Delay(TimeSpan duration);

        /// <summary>
        /// Starts the animation
        /// </summary>
        void Start();

        /// <summary>
        /// Starts the animation and executes a callback when it completes
        /// </summary>
        /// <param name="callback">The callback to execute when the animation completes</param>
        void Start(Action callback);

        /// <summary>
        /// Starts the animation and returns a <see cref="Task"/> to track its completion
        /// </summary>
        Task StartAsync();
    }
}
