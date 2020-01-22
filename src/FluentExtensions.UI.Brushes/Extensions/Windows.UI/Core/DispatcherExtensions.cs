using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Windows.UI.Core
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="CoreDispatcher"/> <see langword="class"/>
    /// </summary>
    public static class DispatcherExtensions
    {
        /// <summary>
        /// Executes a given action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="callback">The action to execute on the UI thread</param>
        public static void Run([NotNull] this CoreDispatcher dispatcher, [NotNull] Action callback)
        {
            if (dispatcher.HasThreadAccess) callback();
            else _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => callback());
        }

        /// <summary>
        /// Executes a given async action on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="dispatcher">The target dispatcher to use to schedule the callback execution</param>
        /// <param name="asyncCallback">The action to execute on the UI thread</param>
        public static void Run([NotNull] this CoreDispatcher dispatcher, [NotNull] Func<Task> asyncCallback)
        {
            if (dispatcher.HasThreadAccess) asyncCallback();
            else _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => asyncCallback());
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
                _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    callback();
                    tcs.SetResult(null);
                });
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
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => tcs.SetResult(asyncCallback()));
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
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                T result = function();
                tcs.SetResult(result);
            });
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
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => tcs.SetResult(function()));
            return tcs.Task.Unwrap();
        }
    }
}
