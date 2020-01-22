using System.Collections.Generic;
using System.Diagnostics.Contracts;

#nullable enable

namespace Windows.UI.Xaml.Media.Animation
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Timeline"/> type
    /// </summary>
    public static class TimelineExtensions
    {
        /// <summary>
        /// Creates a new <see cref="Storyboard"/> with the given animation
        /// </summary>
        /// <param name="animation">The single animation to insert into the returned <see cref="Storyboard"/></param>
        /// <returns>A <see cref="Storyboard"/> instance with <paramref name="animation"/></returns>
        [Pure]
        public static Storyboard ToStoryboard(this Timeline animation)
        {
            return new Storyboard { Children = { animation } };
        }

        /// <summary>
        /// Creates a <see cref="Storyboard"/> with the given animations
        /// </summary>
        /// <param name="animations">The animations to run inside the <see cref="Storyboard"/></param>
        /// <returns>A <see cref="Storyboard"/> instance with all the animations in <paramref name="animations"/></returns>
        [Pure]
        public static Storyboard ToStoryboard(this IEnumerable<Timeline> animations)
        {
            Storyboard storyboard = new Storyboard();

            foreach (Timeline animation in animations)
            {
                storyboard.Children.Add(animation);
            }

            return storyboard;
        }
    }
}
