// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Threading;

    /// <summary>Provides read/write slimlock functionality.</summary>
    internal class ReaderWriterSlimLock : ISharedCollectionLock
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private bool disposed;
        private ReaderWriterLockSlim @lock;

        /// <summary>Initializes a new instance of the <see cref="ReaderWriterSlimLock"/> class.</summary>
        /// <param name="lock">The reference to <see cref="ReaderWriterLockSlim"/>.</param>
        public ReaderWriterSlimLock(ReaderWriterLockSlim @lock)
        {
			Logger.LogDebug(">>> Entering ReaderWriterSlimLock()");
            this.@lock = @lock;
			Logger.LogDebug("<<< Exiting ReaderWriterSlimLock()");
        }

        /// <summary>Finalizes an instance of the <see cref="ReaderWriterSlimLock"/> class.</summary>
        ~ReaderWriterSlimLock()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);

            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <summary>Disposes this instance resources.</summary>
        /// <param name="disposing">A value indicating whether this instance is currently disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (!this.disposed)
            {
                if (disposing)
                {
                    // free managed resources here
                }

                // free unmanaged resources here
                if (this.@lock.IsReadLockHeld)
                {
                    this.@lock.ExitReadLock();
                }
                else if (this.@lock.IsWriteLockHeld)
                {
                    this.@lock.ExitWriteLock();
                }
                else if (this.@lock.IsUpgradeableReadLockHeld)
                {
                    this.@lock.ExitUpgradeableReadLock();
                }

                this.@lock = null;
                this.disposed = true;
				Logger.LogDebug("Dispose(disposing={Disposing}): this.disposed → {Arg1}", disposing, this.disposed);
            }
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }

        private void EnsureNotDisposed()
        {
			Logger.LogDebug(">>> Entering EnsureNotDisposed()");
            if (this.disposed)
            {
                throw new ObjectDisposedException("ReaderWriterSlimLock");
            }
			Logger.LogDebug("<<< Exiting EnsureNotDisposed()");
        }
    }
}
