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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering GetReadLock()");
			Logger.LogDebug("<<< Exiting GetReadLock()");
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
			Logger.LogDebug("<<< Exiting GetReadLock(timeout={Timeout})", timeout);
            return this.GetLock(timeout);
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetWriteLock()
        {
			Logger.LogDebug(">>> Entering GetWriteLock()");
			Logger.LogDebug("<<< Exiting GetWriteLock()");
            return this.GetLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
			Logger.LogDebug("<<< Exiting GetWriteLock(timeout={Timeout})", timeout);
            return this.GetLock(timeout);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <summary>Releases the exclusive lock.</summary>
        public void Exit()
        {
			Logger.LogDebug(">>> Entering Exit()");
            this.EnsureNotDisposed();
            Monitor.Exit(this.@lock);
            this.lockedThread = null;
			Logger.LogDebug("<<< Exiting Exit()");
			Logger.LogDebug("Exit(): lockedThread → {LockedThread}", lockedThread);
			Logger.LogDebug("Exit(): this.lockedThread → {Arg0}", this.lockedThread);
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            // todo remove disposable from interface?
            this.isDisposed = true;
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
			Logger.LogDebug("Dispose(disposing={Disposing}): this.isDisposed → {Arg1}", disposing, this.isDisposed);
        }

        private MonitorLock GetLock(TimeSpan timeout)
        {
			Logger.LogDebug(">>> Entering GetLock(timeout={Timeout})", timeout);
            this.EnsureNotDisposed();
            if (this.IsThreadLocked())
            {
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
                throw new LockTimeoutException("ExclusiveLockStrategy.GetLock timed out");
            }
			Logger.LogDebug("<<< Exiting GetLock(timeout={Timeout})", timeout);
        }

        private bool IsThreadLocked()
        {
			Logger.LogDebug(">>> Entering IsThreadLocked()");
			Logger.LogDebug("<<< Exiting IsThreadLocked()");
            return Thread.CurrentThread.Equals(this.lockedThread);
        }

        private void EnsureNotDisposed()
        {
			Logger.LogDebug(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("ExclusiveLockStrategy");
            }
			Logger.LogDebug("<<< Exiting EnsureNotDisposed()");
        }
    }
}
