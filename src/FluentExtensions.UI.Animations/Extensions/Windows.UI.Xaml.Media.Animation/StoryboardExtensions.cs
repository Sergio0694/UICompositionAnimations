using System;
using System.Threading.Tasks;

#nullable enable

namespace Windows.UI.Xaml.Media.Animation
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Storyboard"/> type
    /// </summary>
    public static class StoryboardExtensions
    {
        /// <summary>
        /// Schedules a callback <see cref="Action"/> to run when the animation completes
        /// </summary>
        /// <param name="target">The <see cref="Storyboard"/> to start</param>
        /// <param name="action">The callback <see cref="Action"/> to execute when the animation ends</param>
        /// <returns>The same <see cref="Storyboard"/> passed as input</returns>
        public static Storyboard Then(this Storyboard target, Action action)
        {
            // Prepare the handler
            void Handler(object sender, object e)
            {
                target.Completed -= Handler;

                action();
            }

            // Assign the handler
            target.Completed += Handler;

            return target;
        }

        /// <summary>
        /// Starts an animation and waits for it to be completed
        /// </summary>
        /// <param name="storyboard">The target storyboard</param>
        /// <returns>A <see cref="Task"/> that completes when <paramref name="storyboard"/> completes</returns>
        public static Task BeginAsync(this Storyboard storyboard)
        {
            TaskCompletionSource<object?> tcs = new TaskCompletionSource<object?>();

            storyboard.Completed += (s, e) => tcs.SetResult(null);

            storyboard.Begin();

            return tcs.Task;
        }
    }
}
