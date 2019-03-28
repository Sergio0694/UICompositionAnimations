using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.XAMLTransform
{
    /// <summary>
    /// A toolkit with some extensions for XAML animations and some useful methods
    /// </summary>
    [PublicAPI]
    public static class XAMLTransformToolkit
    {
        #region Extensions

        

        

        /// <summary>
        /// Starts an animation and runs an action when it completes
        /// </summary>
        /// <param name="target">The storyboard to start</param>
        /// <param name="action">The callback action to execute when the animation ends</param>
        public static Task RunDelegateOnAnimationEndedAsync(this Storyboard target, Action action)
        {
            // Set up the token
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            // Prepare the handler
            void Handler(object sender, object e)
            {
                action();
                target.Completed -= Handler;
                tcs.SetResult(null);
            }

            // Assign the handler, start the animation and return the Task to wait for
            target.Completed += Handler;
            target.Begin();
            return tcs.Task;
        }

        /// <summary>
        /// Checks whether or not the target value of the two animations is the same
        /// </summary>
        /// <param name="storyboard">The input animation</param>
        /// <param name="test">The animation to compare to the first one</param>
        public static bool CompareTargetValue(this Storyboard storyboard, Storyboard test)
        {
            if (storyboard == null || storyboard.Children.Count != 1 || test == null || test.Children.Count != 1) return false;
            return storyboard.Children.First().To<DoubleAnimation>().To?.Equals(test.Children.First().To<DoubleAnimation>().To) == true;
        }

        #endregion

        #region Tools

        /// <summary>
        /// Prepares a an animation with the given info
        /// </summary>
        /// <param name="target">The target object to animate</param>
        /// <param name="property">The property to animate inside the target object</param>
        /// <param name="from">The initial property value</param>
        /// <param name="to">The final property value</param>
        /// <param name="ms">The duration of the animation</param>
        /// <param name="easing">The easing function to use inside the animation</param>
        /// <param name="enableDependecyAnimations">Indicates whether or not to apply this animation to elements that need the visual tree to be rearranged</param>
        public static DoubleAnimation CreateDoubleAnimation(DependencyObject target, string property,
            double? from, double? to, int ms, Easing easing = Easing.Linear, bool enableDependecyAnimations = false)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(ms)),
                EnableDependentAnimation = enableDependecyAnimations
            };
            if (easing != Easing.Linear) animation.EasingFunction = easing.ToEasingFunction();
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, property);
            return animation;
        }

        

        #endregion
    }
}
