using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using JetBrains.Annotations;
using UICompositionAnimations;

namespace Windows.UI.Xaml
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="FrameworkElement"/> <see langword="class"/>
    /// </summary>
    [PublicAPI]
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Sets the <see cref="Visual.CenterPoint"/> property of the <see cref="Visual"/> behind a given <see cref="FrameworkElement"/>
        /// </summary>
        /// <param name="element">The source <see cref="FrameworkElement"/></param>
        public static void SetVisualCenterPoint([NotNull] this FrameworkElement element)
        {
            if (double.IsNaN(element.Width) || double.IsNaN(element.Height))
                throw new InvalidOperationException("The target element must have a fixed size");
            element.GetVisual().CenterPoint = new Vector3((float)(element.Width / 2), (float)(element.Height / 2), 0);
        }

        /// <summary>
        /// Sets the <see cref="Visual.CenterPoint"/> property of the <see cref="Visual"/> behind a given <see cref="FrameworkElement"/> with no fixed size
        /// </summary>
        /// <param name="element">The source element</param>
        public static async Task SetVisualCenterPointAsync([NotNull] this FrameworkElement element)
        {
            // Check if the control hasn't already been loaded
            bool CheckLoadingPending() => element.ActualWidth + element.ActualHeight < 0.1;
            if (CheckLoadingPending())
            {
                // Wait for the loaded event and set the CenterPoint
                TaskCompletionSource tcs = new TaskCompletionSource();
                void Handler(object s, RoutedEventArgs e)
                {
                    tcs.SetResult();
                    element.Loaded -= Handler;
                }

                // Wait for the loaded event for a given time threshold
                element.Loaded += Handler;
                await Task.WhenAny(tcs.Task, Task.Delay(500));
                element.Loaded -= Handler;

                // If the control still hasn't been loaded, approximate the center point with its desired size
                if (CheckLoadingPending())
                {
                    element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    element.GetVisual().CenterPoint = new Vector3((float)(element.DesiredSize.Width / 2), (float)(element.DesiredSize.Height / 2), 0);
                    return;
                }
            }

            // Update the center point
            element.GetVisual().CenterPoint = new Vector3((float)(element.ActualWidth / 2), (float)(element.ActualHeight / 2), 0);
        }
    }
}
