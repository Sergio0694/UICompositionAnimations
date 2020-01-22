using System.Runtime.CompilerServices;

#nullable enable

namespace System.Threading.Tasks
{
    /// <summary>
    /// An <see langword="async"/> <see cref="AsyncMutex"/> implementation that can be easily used inside a <see langword="using"/> block
    /// </summary>
    public sealed class AsyncMutex
    {
        /// <summary>
        /// The underlying <see cref="SemaphoreSlim"/> instance in use
        /// </summary>
        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Acquires a lock for the current instance, that is automatically released outside the <see langword="using"/> block
        /// </summary>
        public async Task<IDisposable> LockAsync()
        {
            await Semaphore.WaitAsync().ConfigureAwait(false);

            return new _Lock(Semaphore);
        }

        /// <summary>
        /// Private class that implements the automatic release of the semaphore
        /// </summary>
        private sealed class _Lock : IDisposable
        {
            /// <summary>
            /// The <see cref="SemaphoreSlim"/> instance of the parent class
            /// </summary>
            private readonly SemaphoreSlim Semaphore;

            /// <summary>
            /// Creates a new <see cref="_Lock"/> instance with the specified parameters
            /// </summary>
            /// <param name="semaphore">The <see cref="SemaphoreSlim"/> instance of the parent class</param>
            public _Lock(SemaphoreSlim semaphore)
            {
                Semaphore = semaphore;
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDisposable.Dispose()
            {
                Semaphore.Release();
            }
        }
    }
}