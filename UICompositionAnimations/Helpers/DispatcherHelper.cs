using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A static class that manages the UI dispatching from background threads
    /// </summary>
    public static class DispatcherHelper
    {
        /// <summary>
        /// Gets the current CoreDispatcher instance
        /// </summary>
        private static CoreDispatcher _CoreDispatcher;

        /// <summary>
        /// Checks whether or not the current thread has access to the UI
        /// </summary>
        public static bool HasUIThreadAccess => (_CoreDispatcher ?? (_CoreDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher)).HasThreadAccess;

        /// <summary>
        /// Executes a given action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static void RunOnUIThread(Action callback)
        {
            if (HasUIThreadAccess) callback();
            else
            {
                _CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    callback();
                }).Forget();
            }
        }

        /// <summary>
        /// Executes a given action on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static async Task RunOnUIThreadAsync(Action callback)
        {
            if (HasUIThreadAccess) callback();
            else
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                _CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    callback();
                    tcs.SetResult(null);
                }).Forget();
                await tcs.Task;
            }
        }

        /// <summary>
        /// Executes a given function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="function">The function to execute on the UI thread</param>
        public static async ValueTask<T> GetFromUIThreadAsync<T>(Func<T> function)
        {
            if (HasUIThreadAccess) return function();
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            _CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                T result = function();
                tcs.SetResult(result);
            }).Forget();
            return await tcs.Task;
        }
    }
}
