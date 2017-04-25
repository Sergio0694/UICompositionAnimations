using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
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
        /// Executes a given action on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="asyncCallback">The action to execute on the UI thread</param>
        public static Task RunOnUIThreadAsync(Func<Task> asyncCallback)
        {
            // Check the current thread
            if (HasUIThreadAccess) return asyncCallback();

            // Schedule on the UI thread if necessary
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            _CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tcs.SetResult(asyncCallback());
            }).Forget();
            return tcs.Task.Unwrap();
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

        /// <summary>
        /// Executes a given async function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="function">The async function to execute on the UI thread</param>
        public static Task<T> GetFromUIThreadAsync<T>(Func<Task<T>> function)
        {
            // Check the current thread
            if (HasUIThreadAccess) return function();

            // Schedule on the UI thread if necessary
            TaskCompletionSource<Task<T>> tcs = new TaskCompletionSource<Task<T>>();
            _CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tcs.SetResult(function());
            }).Forget();
            return tcs.Task.Unwrap();
        }

        /// <summary>
        /// Executes a given async function that returns an async operation on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="function">The async function to execute on the UI thread</param>
        public static Task<T> GetFromUIThreadAsync<T>(Func<IAsyncOperation<T>> function)
        {
            // Check the current thread
            if (HasUIThreadAccess) return function().AsTask();

            // Schedule on the UI thread if necessary
            TaskCompletionSource<Task<T>> tcs = new TaskCompletionSource<Task<T>>();
            _CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tcs.SetResult(function().AsTask());
            }).Forget();
            return tcs.Task.Unwrap();
        }
    }
}
