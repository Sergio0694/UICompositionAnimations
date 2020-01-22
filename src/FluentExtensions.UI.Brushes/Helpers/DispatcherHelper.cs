using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using JetBrains.Annotations;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A <see langword="static"/> <see langword="class"/> that manages the UI dispatching from background threads
    /// </summary>
    [PublicAPI]
    public static class DispatcherHelper
    {
        /// <summary>
        /// Gets the current <see cref="CoreDispatcher"/> instance
        /// </summary>
        [NotNull]
        private static CoreDispatcher CoreDispatcher { get; } = CoreApplication.MainView.CoreWindow.Dispatcher;

        /// <summary>
        /// Checks whether or not the current thread has access to the UI
        /// </summary>
        public static bool HasThreadAccess => CoreDispatcher.HasThreadAccess;

        /// <summary>
        /// Executes a given <see cref="Action"/> on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="callback">The <see cref="Action"/> to execute on the UI thread</param>
        public static void Run([NotNull] Action callback) => CoreDispatcher.Run(callback);

        /// <summary>
        /// Executes a given <see langword="async"/> <see cref="Func{TResult}"/> that returns a <see cref="Task"/> on the UI thread without awaiting the operation
        /// </summary>
        /// <param name="asyncCallback">The <see cref="Func{TResult}"/> to execute on the UI thread</param>
        public static void Run([NotNull] Func<Task> asyncCallback) => CoreDispatcher.Run(asyncCallback);

        /// <summary>
        /// Executes a given <see cref="Action"/> on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="callback">The <see cref="Action"/> to execute on the UI thread</param>
        public static Task RunAsync([NotNull] Action callback) => CoreDispatcher.RunAsync(callback);

        /// <summary>
        /// Executes a given <see cref="Func{TResult}"/> that returns a <see cref="Task"/> on the UI thread and waits for it to be completed
        /// </summary>
        /// <param name="asyncCallback">The <see cref="Func{TResult}"/> to execute on the UI thread</param>
        public static Task RunAsync([NotNull] Func<Task> asyncCallback) => CoreDispatcher.RunAsync(asyncCallback);

        /// <summary>
        /// Executes a given <see cref="Func{TResult}"/> on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="function">The <see cref="Func{TResult}"/> to execute on the UI thread</param>
        public static Task<T> GetAsync<T>([NotNull] Func<T> function) => CoreDispatcher.GetAsync(function);

        /// <summary>
        /// Executes a given <see langword="async"/> <see cref="Func{TResult}"/> that returns a <see cref="Task{TResult}"/> on the UI thread and returns its result
        /// </summary>
        /// <typeparam name="T">The return type for the <see cref="Task{TResult}"/></typeparam>
        /// <param name="function">The <see langword="async"/> <see cref="Func{TResult}"/> to execute on the UI thread</param>
        public static Task<T> GetAsync<T>([NotNull] Func<Task<T>> function) => CoreDispatcher.GetAsync(function);
    }
}
