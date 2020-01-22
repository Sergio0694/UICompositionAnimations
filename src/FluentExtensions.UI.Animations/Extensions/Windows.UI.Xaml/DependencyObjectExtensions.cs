using System;
using System.Diagnostics.Contracts;
using Windows.UI.Xaml.Media.Animation;
using FluentExtensions.UI.Animations.Enums;

#nullable enable

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="DependencyObject"/> type
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="to">The final property value</param>
        /// <param name="duration">The duration of the <see cref="DoubleAnimation"/></param>
        [Pure]
        public static DoubleAnimation CreateDoubleAnimation(
            this DependencyObject target,
            string property,
            double to,
            TimeSpan duration)
        {
            return target.CreateDoubleAnimation(property, null, to, duration, Easing.Linear, false);
        }

        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="to">The final property value</param>
        /// <param name="duration">The duration of the <see cref="DoubleAnimation"/></param>
        /// <param name="easing">The easing function to use inside the <see cref="DoubleAnimation"/></param>
        [Pure]
        public static DoubleAnimation CreateDoubleAnimation(
            this DependencyObject target,
            string property,
            double to,
            TimeSpan duration,
            Easing easing)
        {
            return target.CreateDoubleAnimation(property, null, to, duration, easing, false);
        }

        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="to">The final property value</param>
        /// <param name="duration">The duration of the <see cref="DoubleAnimation"/></param>
        /// <param name="easing">The easing function to use inside the <see cref="DoubleAnimation"/></param>
        /// <param name="enableDependecyAnimations">Indicates whether or not to apply this animation to elements that need the visual tree to be rearranged</param>
        [Pure]
        public static DoubleAnimation CreateDoubleAnimation(
            this DependencyObject target,
            string property,
            double to,
            TimeSpan duration,
            Easing easing,
            bool enableDependecyAnimations)
        {
            return target.CreateDoubleAnimation(property, null, to, duration, easing, enableDependecyAnimations);
        }

        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="from">The optional initial property value</param>
        /// <param name="to">The final property value</param>
        /// <param name="duration">The duration of the <see cref="DoubleAnimation"/></param>
        [Pure]
        public static DoubleAnimation CreateDoubleAnimation(
            this DependencyObject target,
            string property,
            double? from,
            double to,
            TimeSpan duration)
        {
            return target.CreateDoubleAnimation(property, from, to, duration, Easing.Linear, false);
        }

        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="from">The optional initial property value</param>
        /// <param name="to">The final property value</param>
        /// <param name="duration">The duration of the <see cref="DoubleAnimation"/></param>
        /// <param name="easing">The easing function to use inside the <see cref="DoubleAnimation"/></param>
        [Pure]
        public static DoubleAnimation CreateDoubleAnimation(
            this DependencyObject target,
            string property,
            double? from,
            double to,
            TimeSpan duration,
            Easing easing)
        {
            return target.CreateDoubleAnimation(property, from, to, duration, easing, false);
        }

        /// <summary>
        /// Prepares a <see cref="DoubleAnimation"/> with the given info
        /// </summary>
        /// <param name="target">The target <see cref="DependencyObject"/> to animate</param>
        /// <param name="property">The property to animate inside the target <see cref="DependencyObject"/></param>
        /// <param name="from">The optional initial property value</param>
        /// <param name="to">The final property value</param>
        /// <param name="duration">The duration of the <see cref="DoubleAnimation"/></param>
        /// <param name="easing">The easing function to use inside the <see cref="DoubleAnimation"/></param>
        /// <param name="enableDependecyAnimations">Indicates whether or not to apply this animation to elements that need the visual tree to be rearranged</param>
        [Pure]
        public static DoubleAnimation CreateDoubleAnimation(
            this DependencyObject target,
            string property,
            double? from,
            double to,
            TimeSpan duration,
            Easing easing,
            bool enableDependecyAnimations)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                EnableDependentAnimation = enableDependecyAnimations,
                EasingFunction = easing.ToEasingFunction()
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);

            return animation;
        }
    }
}
