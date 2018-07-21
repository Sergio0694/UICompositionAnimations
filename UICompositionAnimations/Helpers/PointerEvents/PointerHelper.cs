using System;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using JetBrains.Annotations;

namespace UICompositionAnimations.Helpers.PointerEvents
{
    /// <summary>
    /// A static class with some extension methods to manage different pointer states
    /// </summary>
    [PublicAPI]
    public static class PointerHelper
    {
        /// <summary>
        /// Adds an event handler to the following <see cref="RoutedEvent"/> targets of the input <see cref="UIElement"/>:
        /// <para/>UIElement.PointerExitedEvent, UIElement.PointerCaptureLostEvent, UIElement.PointerCanceledEvent, UIElement.PointerEnteredEvent, UIElement.PointerReleasedEvent.
        /// <para/>Some additional handling of the input device is also performed automatically to filter out some useful input combinations (eg. disable touch input for the <see cref="UIElement.PointerEnteredEvent"/> case).
        /// </summary>
        /// <param name="control">The <see cref="UIElement"/> to monitor</param>
        /// <param name="action">An <see cref="Action"/> to call every time a pointer event is raised. The <see cref="bool"/> parameter indicates whether the pointer is moving to or from the control</param>
        [NotNull]
        public static ControlAttachedHandlersInfo ManageControlPointerStates([NotNull] this UIElement control, [NotNull] Action<PointerDeviceType, bool> action)
        {
            return new ControlAttachedHandlersInfo(control,
                AddHandler(control, UIElement.PointerExitedEvent, false, null, action),
                AddHandler(control, UIElement.PointerCaptureLostEvent, false, null, action),
                AddHandler(control, UIElement.PointerCanceledEvent, false, null, action),
                AddHandler(control, UIElement.PointerEnteredEvent, true, p => p != PointerDeviceType.Touch, action),
                AddHandler(control, UIElement.PointerReleasedEvent, false, p => p == PointerDeviceType.Touch, action));
        }

        /// <summary>
        /// Adds an event handler to the following <see cref="RoutedEvent"/> targets of the input <see cref="UIElement"/>: UIElement.PointerExitedEvent, UIElement.PointerMovedEvent.
        /// <para/>This is especially useful to check whether or not the user is currently moving the input cursor above a host control, as this method will correctly handle scenarios
        /// where the cursor would be considered as exited from the target control, while still being inside its area.
        /// <para/>For example, this could happen if the user clicked on a nested control inside the target host, without actually moving the cursor away.
        /// </summary>
        /// <param name="host">The <see cref="UIElement"/> to monitor</param>
        /// <param name="action">An <see cref="Action"/> to call every time a pointer event is raised. The <see cref="bool"/> parameter indicates whether the pointer is moving to or from the control</param>
        [NotNull]
        public static ControlAttachedHandlersInfo ManageHostPointerStates([NotNull] this UIElement host, [NotNull] Action<PointerDeviceType, bool> action)
        {
            return new ControlAttachedHandlersInfo(host,
                AddHandler(host, UIElement.PointerExitedEvent, false, null, action),
                AddHandler(host, UIElement.PointerMovedEvent, true, p => p != PointerDeviceType.Touch, action));
        }

        /// <summary>
        /// Adds the appropriate handlers to a control to help setup the light effects (skipped when using a touch screen)
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to monitor</param>
        /// <param name="action">An <see cref="Action"/> to call every time the light effects state should be changed</param>
        [NotNull]
        public static ControlAttachedHandlersInfo ManageLightsPointerStates([NotNull] this UIElement element, [NotNull] Action<bool> action)
        {
            bool state = false;
            return element.ManageHostPointerStates((pointer, value) =>
            {
                if (pointer != PointerDeviceType.Mouse || state == value) return;
                state = value;
                action(value);
            });
        }

        // Private method that adds a custom handler to the target event and control
        [NotNull]
        private static PointerHandlerInfo AddHandler([NotNull] UIElement element, [NotNull] RoutedEvent @event, bool state, [CanBeNull] Func<PointerDeviceType, bool> predicate, [NotNull] Action<PointerDeviceType, bool> action)
        {
            // Callback with additional pointer type check
            void Handler(object _, PointerRoutedEventArgs e)
            {
                if (predicate == null || predicate(e.Pointer.PointerDeviceType))
                    action(e.Pointer.PointerDeviceType, state);
            }

            // Handler setup
            element.AddHandler(@event, (PointerEventHandler)Handler, true);
            return new PointerHandlerInfo(@event, Handler);
        }
    }
}
