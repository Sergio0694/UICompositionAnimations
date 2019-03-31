using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using JetBrains.Annotations;

namespace UICompositionAnimationsLegacy.Helpers.PointerEvents
{
    /// <summary>
    /// A small class that holds the necessary information to remove a single pointer handler from a target control
    /// </summary>
    internal sealed class PointerHandlerInfo
    {
        /// <summary>
        /// Gets the target <see cref="RoutedEvent"/> for the handler
        /// </summary>
        [NotNull]
        public RoutedEvent Event { get; }

        /// <summary>
        /// Gets the <see cref="PointerEventHandler"/> added to the target event
        /// </summary>
        [NotNull]
        public PointerEventHandler Handler { get; }

        /// <summary>
        /// Creates a new instance to wrap a custom pointer handler
        /// </summary>
        /// <param name="target">The target event for the handler</param>
        /// <param name="handler">The handler that has just been added to the target event</param>
        public PointerHandlerInfo([NotNull] RoutedEvent target, [NotNull] PointerEventHandler handler)
        {
            Event = target;
            Handler = handler;
        }
    }
}
