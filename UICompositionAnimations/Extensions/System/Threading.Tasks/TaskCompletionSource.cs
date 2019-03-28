namespace System.Threading.Tasks
{
    /// <summary>
    /// A <see cref="TaskCompletionSource{TResult}"/> implementation without the generic argument
    /// </summary>
    public sealed class TaskCompletionSource : TaskCompletionSource<Unit>
    {
        /// <summary>
        /// Transitions the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.RanToCompletion"/> state
        /// </summary>
        public void SetResult() => SetResult(Unit.Value);

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.RanToCompletion"/> state
        /// </summary>
        public void TrySetResult() => TrySetResult(Unit.Value);
    }
}
