using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Windows.UI.Xaml.Media.Animation
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Storyboard"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class StoryboardExtensions
    {
        /// <summary>
        /// Schedules a callback <see cref="Action"/> to run when the animation completes
        /// </summary>
        /// <param name="target">The <see cref="Storyboard"/> to start</param>
        /// <param name="action">The callback <see cref="Action"/> to execute when the animation ends</param>
        public static void Then([NotNull] this Storyboard target, Action action)
        {
            // Set up the token
            TaskCompletionSource tcs = new TaskCompletionSource();

            // Prepare the handler
            void Handler(object sender, object e)
            {
                action();
                target.Completed -= Handler;
                tcs.SetResult();
            }

            // Assign the handler
            target.Completed += Handler;
        }
    }
}
