// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>An <see cref="IList{T}"/> implementation designed to be shared across threads.</summary>
    /// <typeparam name="T">The type of value in the list.</typeparam>
    public class SharedList<T> : IList<T>, IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly List<T> list = new List<T>();
        private ILockStrategy lockStrategy;

        private bool isDisposed;

        /// <summary>Initializes a new instance of the <see cref="SharedList{T}"/> class.</summary>
        public SharedList()
            : this(LockingStrategy.ReaderWriter)
        {
		Logger.LogDebug(">>> Entering SharedList()");
		Logger.LogDebug("<<< Exiting SharedList()");
		Logger.LogDebug(">>> Entering SharedList(lockStrategy={LockStrategy})", lockStrategy);
		Logger.LogDebug("<<< Exiting SharedList(lockStrategy={LockStrategy})", lockStrategy);
        }

        /// <summary>Initializes a new instance of the <see cref="SharedList{T}"/> class.</summary>
        /// <param name="lockStrategy">The locking strategy to use.</param>
        public SharedList(ILockStrategy lockStrategy)
        {
            this.lockStrategy = lockStrategy;
			Logger.LogDebug("SharedList(lockStrategy={LockStrategy}): lockStrategy → {LockStrategy1}", lockStrategy, lockStrategy);
			Logger.LogDebug("SharedList(lockStrategy={LockStrategy}): this.lockStrategy → {Arg1}", lockStrategy, this.lockStrategy);
        }

        /// <summary>Initializes a new instance of the <see cref="SharedList{T}"/> class.</summary>
        /// <param name="strategy">The locking strategy to use.</param>
        public SharedList(LockingStrategy strategy)
            : this(LockingStrategyFactory.Create(strategy))
        {
        }

        /// <summary>Finalizes an instance of the <see cref="SharedList{T}"/> class.</summary>
        ~SharedList()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this.list.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return ((ICollection<T>)this.list).IsReadOnly;
            }
        }

        /// <summary>Gets the backing list to use.</summary>
        internal IList<T> BackingList
        {
            get
            {
                return this.list;
            }
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this.list[index];
            }

            set
            {
                this.EnsureNotDisposed();
                this.EnsureWriteAccess();
                this.list[index] = value;
            }
        }

        /// <inheritdoc />
        public void Add(T item)
        {
			Logger.LogDebug(">>> Entering Add(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.list.Add(item);
			Logger.LogDebug("<<< Exiting Add(item={Item})", item);
        }

        /// <inheritdoc />
        public void Clear()
        {
			Logger.LogDebug(">>> Entering Clear()");
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.list.Clear();
			Logger.LogDebug("<<< Exiting Clear()");
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
			Logger.LogDebug(">>> Entering Contains(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogDebug("<<< Exiting Contains(item={Item})", item);
            return this.list.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
			Logger.LogDebug(">>> Entering CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            this.list.CopyTo(array, arrayIndex);
			Logger.LogDebug("<<< Exiting CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
			Logger.LogDebug(">>> Entering Remove(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
			Logger.LogDebug("<<< Exiting Remove(item={Item})", item);
            return this.list.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
			Logger.LogDebug(">>> Entering GetEnumerator()");
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogDebug("<<< Exiting GetEnumerator()");
            return this.list.GetEnumerator();
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
			Logger.LogDebug(">>> Entering IndexOf(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogDebug("<<< Exiting IndexOf(item={Item})", item);
            return this.list.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
			Logger.LogDebug(">>> Entering Insert(index={Index},item={Item})", index, item);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.list.Insert(index, item);
			Logger.LogDebug("<<< Exiting Insert(index={Index},item={Item})", index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
			Logger.LogDebug(">>> Entering RemoveAt(index={Index})", index);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.list.RemoveAt(index);
			Logger.LogDebug("<<< Exiting RemoveAt(index={Index})", index);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <summary>Ensures the instance is not disposed.</summary>
        public void EnsureNotDisposed()
        {
			Logger.LogDebug(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("SharedList");
            }
			Logger.LogDebug("<<< Exiting EnsureNotDisposed()");
        }

        /// <summary>Gets a read lock on the resource.</summary>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock()
        {
			Logger.LogDebug(">>> Entering GetReadLock()");
			Logger.LogDebug("<<< Exiting GetReadLock()");
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>Gets a read lock on the resource for the specified amount of time.</summary>
        /// <param name="timeOut">The amount of time to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
			Logger.LogDebug("<<< Exiting GetReadLock(timeOut={TimeOut})", timeOut);
            return this.lockStrategy.GetReadLock(timeOut);
        }

        /// <summary>Gets a read lock on the resource for the specified amount of time.</summary>
        /// <param name="millisecondTimeout">The number of milliseconds to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock(int millisecondTimeout)
        {
			Logger.LogDebug("<<< Exiting GetReadLock(millisecondTimeout={MillisecondTimeout})", millisecondTimeout);
            return this.GetReadLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        /// <summary>Gets a write lock on the resource for the specified amount of time.</summary>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock()
        {
			Logger.LogDebug(">>> Entering GetWriteLock()");
			Logger.LogDebug("<<< Exiting GetWriteLock()");
            return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>Gets a write lock on the resource for the specified amount of time.</summary>
        /// <param name="timeOut">The amount of time to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
			Logger.LogDebug("<<< Exiting GetWriteLock(timeOut={TimeOut})", timeOut);
            return this.lockStrategy.GetWriteLock(timeOut);
        }

        /// <summary>Gets a write lock on the resource for the specified amount of time.</summary>
        /// <param name="millisecondTimeout">The number of milliseconds to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
			Logger.LogDebug("<<< Exiting GetWriteLock(millisecondTimeout={MillisecondTimeout})", millisecondTimeout);
            return this.GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        /// <summary>Gets an enumerator to iterate through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator1()
        {
			Logger.LogDebug(">>> Entering GetEnumerator1()");
			Logger.LogDebug("<<< Exiting GetEnumerator1()");
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator1();
        }

        /// <summary>Disposes this instance resources.</summary>
        /// <param name="disposing">Indicates if this instance is currently disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                this.lockStrategy.Dispose();
                this.lockStrategy = null;
				Logger.LogDebug("Dispose(disposing={Disposing}): lockStrategy → {LockStrategy}", disposing, lockStrategy);
				Logger.LogDebug("Dispose(disposing={Disposing}): this.lockStrategy → {Arg1}", disposing, this.lockStrategy);
            }

            this.isDisposed = true;
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
			Logger.LogDebug("Dispose(disposing={Disposing}): this.isDisposed → {Arg1}", disposing, this.isDisposed);
        }

        private void EnsureReadAccess()
        {
			Logger.LogDebug(">>> Entering EnsureReadAccess()");
            if (!this.lockStrategy.ThreadCanRead)
            {
                throw new ReadLockRequiredException();
            }
			Logger.LogDebug("<<< Exiting EnsureReadAccess()");
        }

        private void EnsureWriteAccess()
        {
			Logger.LogDebug(">>> Entering EnsureWriteAccess()");
            if (!this.lockStrategy.ThreadCanWrite)
            {
                throw new WriteLockRequiredException();
            }
			Logger.LogDebug("<<< Exiting EnsureWriteAccess()");
        }
    }
}
