using System;
using Windows.UI.Xaml;
using JetBrains.Annotations;

namespace UICompositionAnimations.Helpers.PointerEvents
{
    /// <summary>
    /// A small class that contains all the needed information on a series of pointer handlers added to a target <see cref="UIElement"/>
    /// </summary>
    public sealed class ControlAttachedHandlersInfo
    {
        // Holds a weak reference to the target element
        [NotNull]
        private readonly WeakReference<UIElement> ElementReference;

        // The list of added handlers
        [NotNull]
        private readonly PointerHandlerInfo[] Info;

        /// <summary>
        /// Creates a new instance to dispose the handlers when requested
        /// </summary>
        /// <param name="element">The target element used to add the handlers</param>
        /// <param name="info">The sequence of info on the added handlers</param>
        internal ControlAttachedHandlersInfo([NotNull] UIElement element, [NotNull] params PointerHandlerInfo[] info)
        {
            ElementReference = new WeakReference<UIElement>(element);
            Info = info;
        }

        // Indicates whether the current handlers have already been removed
        private bool _Disposed;

        /// <summary>
        /// Tries to remove the custom pointer handlers added to a target element
        /// </summary>
        /// <returns>A value indicating the result of the operation</returns>
        /// <remarks>The class holds a <see cref="WeakReference{T}"/> to the target element, so the target
        /// control will still be garbage collected even if this method isn't called</remarks>
        public HandlersRemovalResult TryRemoveHandlers()
        {
            if (_Disposed) return HandlersRemovalResult.AlreadyRemoved;
            _Disposed = true;
            if (Info.Length == 0) return HandlersRemovalResult.NoHandlersRegistered;
            bool active = ElementReference.TryGetTarget(out UIElement element);
            if (!active) return HandlersRemovalResult.ObjectFinalized;
            try
            {
                foreach (PointerHandlerInfo info in Info)
                    element.RemoveHandler(info.Event, info.Handler);
                return HandlersRemovalResult.Success;
            }
            catch
            {
                // Whops!
                return HandlersRemovalResult.UnknownError;
            }
        }
    }
}
