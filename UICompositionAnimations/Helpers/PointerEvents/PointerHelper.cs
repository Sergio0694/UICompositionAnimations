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
        /// Adds an event handler to all the pointer events of the target control
        /// </summary>
        /// <param name="control">The control to monitor</param>
        /// <param name="action">An action to call every time a pointer event is raised. The bool parameter
        /// indicates whether the pointer is moving to or from the control</param>
        [NotNull]
        public static ControlAttachedHandlersInfo ManageControlPointerStates(this UIElement control, Action<PointerDeviceType, bool> action)
        {
            // Nested functions that adds the actual handlers
            PointerHandlerInfo AddHandler(RoutedEvent @event, bool state, Func<PointerDeviceType, bool> predicate)
            {
                void Handler(object _, PointerRoutedEventArgs e)
                {
                    if (predicate == null || predicate(e.Pointer.PointerDeviceType))
                        action(e.Pointer.PointerDeviceType, state);
                }

                control.AddHandler(@event, (PointerEventHandler) Handler, true);
                return new PointerHandlerInfo(@event, Handler);
            }

            // Add handlers
            return new ControlAttachedHandlersInfo(control,
                AddHandler(UIElement.PointerExitedEvent, false, null),
                AddHandler(UIElement.PointerCaptureLostEvent, false, null),
                AddHandler(UIElement.PointerCanceledEvent, false, null),
                AddHandler(UIElement.PointerEnteredEvent, true, p => p != PointerDeviceType.Touch),
                AddHandler(UIElement.PointerReleasedEvent, false, p => p == PointerDeviceType.Touch));
        }

        /// <summary>
        /// Adds an event handler to all the pointer events of the target element
        /// </summary>
        /// <param name="host">The element to monitor</param>
        /// <param name="action">An action to call every time a pointer event is raised. The bool parameter
        /// indicates whether the pointer is moving to or from the control</param>
        [NotNull]
        public static ControlAttachedHandlersInfo ManageHostPointerStates(this UIElement host, Action<PointerDeviceType, bool> action)
        {
            // Nested functions that adds the actual handlers
            PointerHandlerInfo AddHandler(RoutedEvent @event, bool state, Func<PointerDeviceType, bool> predicate)
            {
                void Handler(object _, PointerRoutedEventArgs e)
                {
                    if (predicate == null || predicate(e.Pointer.PointerDeviceType))
                        action(e.Pointer.PointerDeviceType, state);
                }

                host.AddHandler(@event, (PointerEventHandler) Handler, true);
                return new PointerHandlerInfo(@event, Handler);
            }

            // Add handlers
            return new ControlAttachedHandlersInfo(host,
                AddHandler(UIElement.PointerExitedEvent, false, null),
                AddHandler(UIElement.PointerMovedEvent, true, p => p != PointerDeviceType.Touch));
        }

        /// <summary>
        /// Adds the appropriate handlers to a control to help setup the light effects (skipped when on a mobile phone)
        /// </summary>
        /// <param name="element">The element to monitor</param>
        /// <param name="action">An action to call every time the light effects state should be changed</param>
        [CanBeNull]
        public static ControlAttachedHandlersInfo ManageLightsPointerStates(this UIElement element, Action<bool> action)
        {
            // Nested functions that adds the actual handlers
            return element.ManageHostPointerStates((pointer, value) =>
            {
                if (pointer != PointerDeviceType.Mouse) return;
                action(value);
            });
        }
    }
}
