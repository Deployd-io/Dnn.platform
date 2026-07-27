// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;

    /// <summary>Provides read/write locking.</summary>
    [Serializable]
    public class ReaderWriterLockStrategy : IDisposable, ILockStrategy
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [NonSerialized]
        private ReaderWriterLockSlim @lock;

        private LockRecursionPolicy lockRecursionPolicy;

        private bool isDisposed;

        /// <summary>Initializes a new instance of the <see cref="ReaderWriterLockStrategy"/> class.</summary>
        public ReaderWriterLockStrategy()
            : this(LockRecursionPolicy.NoRecursion)
        {
		Logger.LogDebug(">>> Entering ReaderWriterLockStrategy()");
		Logger.LogDebug("<<< Exiting ReaderWriterLockStrategy()");
		Logger.LogDebug(">>> Entering ReaderWriterLockStrategy(recursionPolicy={RecursionPolicy})", recursionPolicy);
		Logger.LogDebug("<<< Exiting ReaderWriterLockStrategy(recursionPolicy={RecursionPolicy})", recursionPolicy);
        }

        /// <summary>Initializes a new instance of the <see cref="ReaderWriterLockStrategy"/> class.</summary>
        /// <param name="recursionPolicy">An instance of a <see cref="LockRecursionPolicy"/> to use.</param>
        public ReaderWriterLockStrategy(LockRecursionPolicy recursionPolicy)
        {
			Logger.LogDebug(">>> Entering ReaderWriterLockStrategy(recursionPolicy={RecursionPolicy})", recursionPolicy);
            this.lockRecursionPolicy = recursionPolicy;
            this.@lock = new ReaderWriterLockSlim(recursionPolicy);
			Logger.LogDebug("<<< Exiting ReaderWriterLockStrategy(recursionPolicy={RecursionPolicy})", recursionPolicy);
			Logger.LogDebug("ReaderWriterLockStrategy(recursionPolicy={RecursionPolicy}): this.lockRecursionPolicy → {Arg1}", recursionPolicy, this.lockRecursionPolicy);
			Logger.LogDebug("ReaderWriterLockStrategy(recursionPolicy={RecursionPolicy}): lockRecursionPolicy → {LockRecursionPolicy}", recursionPolicy, lockRecursionPolicy);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderWriterLockStrategy"/> class.
        /// The special constructor is used to deserialize values.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> to use.</param>
        /// <param name="context">The streaming context to use.</param>
        public ReaderWriterLockStrategy(SerializationInfo info, StreamingContext context)
        {
            this.lockRecursionPolicy = (LockRecursionPolicy)info.GetValue("_lockRecursionPolicy", typeof(LockRecursionPolicy));
            this.@lock = new ReaderWriterLockSlim(this.lockRecursionPolicy);
			Logger.LogDebug("ReaderWriterLockStrategy(info={Info},context={Context}): lockRecursionPolicy → {LockRecursionPolicy}", info, context, lockRecursionPolicy);
        }

        /// <summary>Finalizes an instance of the <see cref="ReaderWriterLockStrategy"/> class.</summary>
        ~ReaderWriterLockStrategy()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            this.Dispose(false);
        }

        /// <inheritdoc />
        public bool ThreadCanRead
        {
            get
            {
                this.EnsureNotDisposed();
                return this.Lock.IsReadLockHeld || this.Lock.IsWriteLockHeld;
            }
        }

        /// <inheritdoc />
        public bool ThreadCanWrite
        {
            get
            {
                this.EnsureNotDisposed();
                return this.Lock.IsWriteLockHeld;
            }
        }

        /// <inheritdoc />
        public bool SupportsConcurrentReads
        {
            get
            {
                return true;
            }
        }

        private ReaderWriterLockSlim Lock
        {
            get
            {
                return this.@lock ?? (this.@lock = new ReaderWriterLockSlim(this.lockRecursionPolicy));
            }
        }

        /// <summary>
        /// Implement this method to serialize data. The method is called
        /// on serialization.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to use.</param>
        /// <param name="context">The <see cref="StreamingContext"/> to use.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
			Logger.LogDebug(">>> Entering GetObjectData(info={Info},context={Context})", info, context);
            // Use the AddValue method to specify serialized values.
            info.AddValue("_lockRecursionPolicy", this.lockRecursionPolicy, typeof(LockRecursionPolicy));
			Logger.LogDebug("<<< Exiting GetObjectData(info={Info},context={Context})", info, context);
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetReadLock()
        {
			Logger.LogDebug(">>> Entering GetReadLock()");
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetReadLock(TimeSpan timeout)
        {
            this.EnsureNotDisposed();
            if ((this.Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && this.Lock.IsReadLockHeld) ||
                this.Lock.TryEnterReadLock(timeout))
            {
                return new ReaderWriterSlimLock(this.Lock);
            }
            else
            {
                throw new LockTimeoutException("ReaderWriterLockStrategy.GetReadLock timed out");
            }
			Logger.LogDebug("<<< Exiting GetReadLock()");
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetWriteLock()
        {
			Logger.LogDebug(">>> Entering GetWriteLock()");
            return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <inheritdoc />
        public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
        {
            this.EnsureNotDisposed();
            if ((this.Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && this.Lock.IsWriteLockHeld) ||
                this.Lock.TryEnterWriteLock(timeout))
            {
                return new ReaderWriterSlimLock(this.Lock);
            }
            else
            {
                throw new LockTimeoutException("ReaderWriterLockStrategy.GetWriteLock timed out");
            }
			Logger.LogDebug("<<< Exiting GetWriteLock()");
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <summary>Disposes this instance resources.</summary>
        /// <param name="disposing">A value indicating whether this instance is currently disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                if (this.@lock != null)
                {
                    this.@lock.Dispose();
                    this.@lock = null;
                }
            }

            this.isDisposed = true;
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
			Logger.LogDebug("Dispose(disposing={Disposing}): this.isDisposed → {Arg1}", disposing, this.isDisposed);
        }

        private void EnsureNotDisposed()
        {
			Logger.LogDebug(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("ReaderWriterLockStrategy");
            }
			Logger.LogDebug("<<< Exiting EnsureNotDisposed()");
        }
    }
}
