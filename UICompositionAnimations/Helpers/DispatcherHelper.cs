using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using JetBrains.Annotations;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A static class that manages the UI dispatching from background threads
    /// </summary>
    [PublicAPI]
    public static class DispatcherHelper
    {
        #region Target dispatcher helpers

        /// <summary>
        /// Executes a given action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static void Run([NotNull] this CoreDispatcher dispatcher, [NotNull] Action callback)
        {
            if (dispatcher.HasThreadAccess) callback();
            else dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => callback()).Forget();
        }

        /// <summary>
        /// Executes a given async action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="asyncCallback">The action to execute on the UI thread</param>
        public static void Run([NotNull] this CoreDispatcher dispatcher, [NotNull] Func<Task> asyncCallback)
        {
            if (dispatcher.HasThreadAccess) asyncCallback();
            else dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => asyncCallback()).Forget();
        }

        /// <summary>
        /// Executes a given action on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static async Task RunAsync([NotNull] this CoreDispatcher dispatcher, [NotNull] Action callback)
        {
            if (dispatcher.HasThreadAccess) callback();
            else
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="asyncCallback">The action to execute on the UI thread</param>
        public static Task RunAsync([NotNull] this CoreDispatcher dispatcher, [NotNull] Func<Task> asyncCallback)
        {
            if (dispatcher.HasThreadAccess) return asyncCallback();
            TaskCompletionSource<Task> tcs = new TaskCompletionSource<Task>();
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tcs.SetResult(asyncCallback());
            }).Forget();
            return tcs.Task.Unwrap();
        }

        /// <summary>
        /// Executes a given function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="function">The function to execute on the UI thread</param>
        public static Task<T> GetAsync<T>([NotNull] this CoreDispatcher dispatcher, [NotNull] Func<T> function)
        {
            if (dispatcher.HasThreadAccess) return Task.FromResult(function());
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                T result = function();
                tcs.SetResult(result);
            }).Forget();
            return tcs.Task;
        }

        /// <summary>
        /// Executes a given async function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="function">The async function to execute on the UI thread</param>
        public static Task<T> GetAsync<T>([NotNull] this CoreDispatcher dispatcher, [NotNull] Func<Task<T>> function)
        {
            if (dispatcher.HasThreadAccess) return function();
            TaskCompletionSource<Task<T>> tcs = new TaskCompletionSource<Task<T>>();
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tcs.SetResult(function());
            }).Forget();
            return tcs.Task.Unwrap();
        }

        #endregion

        #region Standalone helpers

        private static CoreDispatcher _CoreDispatcher;

        /// <summary>
        /// Gets the current CoreDispatcher instance
        /// </summary>
        [NotNull]
        private static CoreDispatcher CoreDispatcher => _CoreDispatcher ?? (_CoreDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher);

        /// <summary>
        /// Checks whether or not the current thread has access to the UI
        /// </summary>
        public static bool HasUIThreadAccess => CoreDispatcher.HasThreadAccess;

        /// <summary>
        /// Executes a given action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static void RunOnUIThread([NotNull] Action callback) => CoreDispatcher.Run(callback);

        /// <summary>
        /// Executes a given async action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="asyncCallback">The action to execute on the UI thread</param>
        public static void RunOnUIThread([NotNull] Func<Task> asyncCallback) => CoreDispatcher.Run(asyncCallback);

        /// <summary>
        /// Executes a given action on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static Task RunOnUIThreadAsync([NotNull] Action callback) => CoreDispatcher.RunAsync(callback);

        /// <summary>
        /// Executes a given action on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="asyncCallback">The action to execute on the UI thread</param>
        public static Task RunOnUIThreadAsync([NotNull] Func<Task> asyncCallback) => CoreDispatcher.RunAsync(asyncCallback);

        /// <summary>
        /// Executes a given function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="function">The function to execute on the UI thread</param>
        public static Task<T> GetFromUIThreadAsync<T>([NotNull] Func<T> function) => CoreDispatcher.GetAsync(function);

        /// <summary>
        /// Executes a given async function on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="function">The async function to execute on the UI thread</param>
        public static Task<T> GetFromUIThreadAsync<T>([NotNull] Func<Task<T>> function) => CoreDispatcher.GetAsync(function);

        #endregion
    }
}
