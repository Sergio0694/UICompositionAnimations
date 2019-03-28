using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Windows.UI.Xaml.Media.Animation
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Timeline"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class TimelineExtensions
    {
        /// <summary>
        /// Creates a new <see cref="Storyboard"/> with the given animation
        /// </summary>
        /// <param name="animation">The single animation to insert into the returned <see cref="Storyboard"/></param>
        [NotNull]
        public static Storyboard ToStoryboard([NotNull] this Timeline animation)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            return storyboard;
        }

        /// <summary>
        /// Prepares a <see cref="Storyboard"/> with the given animations
        /// </summary>
        /// <param name="animations">The animations to run inside the <see cref="Storyboard"/></param>
        public static Storyboard ToStoryboard([NotNull, ItemNotNull] this IEnumerable<Timeline> animations)
        {
            Storyboard storyboard = new Storyboard();
            foreach (Timeline animation in animations)
                storyboard.Children.Add(animation);
            return storyboard;
        }

        /// <summary>
        /// Starts an animation and waits for it to be completed
        /// </summary>
        /// <param name="storyboard">The target storyboard</param>
        public static Task BeginAsync(this Storyboard storyboard)
        {
            if (storyboard == null) throw new ArgumentNullException();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            storyboard.Completed += (s, e) => tcs.SetResult(null);
            storyboard.Begin();
            return tcs.Task;
        }
    }
}
