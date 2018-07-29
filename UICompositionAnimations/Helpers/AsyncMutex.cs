using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace System.Threading.Tasks
{
    /// <summary>
    /// An <see langword="async"/> <see cref="AsyncMutex"/> implementation that can be easily used inside a <see langword="using"/> block
    /// </summary>
    internal sealed class AsyncMutex
    {
        // The underlying semaphore used by this instance
        [NotNull]
        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Acquires a lock for the current instance, that is automatically released outside the <see langword="using"/> block
        /// </summary>
        [NotNull, ItemNotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            // Reference to the semaphore instance of the parent class
            [NotNull]
            private readonly SemaphoreSlim Semaphore;

            public _Lock([NotNull] SemaphoreSlim semaphore) => Semaphore = semaphore;

            // Releases the lock when the instance is disposed
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDisposable.Dispose() => Semaphore.Release();
        }
    }
}