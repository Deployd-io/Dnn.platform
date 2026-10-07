// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Threading;

    /// <summary>Represents an exclusive locking strategy.</summary>
    public class ExclusiveLockStrategy : ILockStrategy
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExclusiveLockStrategy));
        private readonly object @lock = new object();

        private bool isDisposed;
        private Thread lockedThread;

        /// <inheritdoc />
        public bool ThreadCanRead
        {
            get
            {
                this.EnsureNotDisposed();
                return this.IsThreadLocked();
            }
        }

        /// <inheritdoc />
        public bool ThreadCanWrite
        {
            get
            {
                this.EnsureNotDisposed();
                return this.IsThreadLocked();
            }
        }

        /// <inheritdoc />
        public bool SupportsConcurrentReads
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetReadLock()
        {
			Logger.LogError(">>> Entering GetReadLock()");
            Logger.LogWarning("<<< Exiting GetReadLock()");
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            return this.GetLock(timeout);
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetWriteLock()
        {
			Logger.LogError(">>> Entering GetWriteLock()");
            Logger.LogWarning("<<< Exiting GetWriteLock()");
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            return this.GetLock(timeout);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);
			Logger.LogWarning("<<< Exiting Dispose()");
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the exclusive lock.</summary>
        public void Exit()
        {
			Logger.LogError(">>> Entering Exit()");
            this.EnsureNotDisposed();
			Logger.LogWarning("<<< Exiting Exit()");
            Monitor.Exit(this.@lock);
            this.lockedThread = null;
			Logger.LogDebug("Exit(): lockedThread → {LockedThread}", lockedThread);
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            // todo remove disposable from interface?
			Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
            this.isDisposed = true;
        }

        private MonitorLock GetLock(TimeSpan timeout)
        {
			Logger.LogError(">>> Entering GetLock(timeout={Timeout})", timeout);
            this.EnsureNotDisposed();
            if (this.IsThreadLocked())
            {
				Logger.LogError("Throwing LockRecursionException from GetLock(timeout={Timeout})", timeout);
                throw new LockRecursionException();
            }

            if (Monitor.TryEnter(this.@lock, timeout))
            {
                this.lockedThread = Thread.CurrentThread;
				Logger.LogDebug("GetLock(timeout={Timeout}): lockedThread → {LockedThread}", timeout, lockedThread);
				Logger.LogDebug("GetLock(timeout={Timeout}): this.lockedThread → {Arg1}", timeout, this.lockedThread);
                return new MonitorLock(this);
            }
            else
            {
				Logger.LogError("Throwing LockTimeoutException from GetLock(timeout={Timeout})", timeout);
			Logger.LogWarning("<<< Exiting GetLock(timeout={Timeout})", timeout);
                throw new LockTimeoutException("ExclusiveLockStrategy.GetLock timed out");
            }
        }

        private bool IsThreadLocked()
        {
			Logger.LogError(">>> Entering IsThreadLocked()");
            Logger.LogWarning("<<< Exiting IsThreadLocked()");
            return Thread.CurrentThread.Equals(this.lockedThread);
        }

        private void EnsureNotDisposed()
        {
			Logger.LogError(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
				Logger.LogError("Throwing ObjectDisposedException from EnsureNotDisposed()");
                throw new ObjectDisposedException("ExclusiveLockStrategy");
            }
			Logger.LogWarning("<<< Exiting EnsureNotDisposed()");
        }
    }
}
