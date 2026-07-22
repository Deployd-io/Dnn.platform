// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>An <see cref="IDictionary{TKey,TValue}"/> implementation designed to be shared across threads.</summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [Serializable]
    public class SharedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IDictionary<TKey, TValue> dict;

        private bool isDisposed;
        private ILockStrategy lockController;

        /// <summary>Initializes a new instance of the <see cref="SharedDictionary{TKey, TValue}"/> class.</summary>
        public SharedDictionary()
            : this(LockingStrategy.ReaderWriter)
        {
		Logger.LogDebug(">>> Entering SharedDictionary()");
		Logger.LogDebug("<<< Exiting SharedDictionary()");
		Logger.LogDebug(">>> Entering SharedDictionary(lockStrategy={LockStrategy})", lockStrategy);
		Logger.LogDebug("<<< Exiting SharedDictionary(lockStrategy={LockStrategy})", lockStrategy);
        }

        /// <summary>Initializes a new instance of the <see cref="SharedDictionary{TKey, TValue}"/> class.</summary>
        /// <param name="lockStrategy">The locking strategy to use.</param>
        public SharedDictionary(ILockStrategy lockStrategy)
        {
            this.dict = new Dictionary<TKey, TValue>();
            this.lockController = lockStrategy;
			Logger.LogDebug("SharedDictionary(lockStrategy={LockStrategy}): lockController → {LockController}", lockStrategy, lockController);
			Logger.LogDebug("SharedDictionary(lockStrategy={LockStrategy}): this.lockController → {Arg1}", lockStrategy, this.lockController);
			Logger.LogDebug("SharedDictionary(lockStrategy={LockStrategy}): dict → {Dict}", lockStrategy, dict);
        }

        /// <summary>Initializes a new instance of the <see cref="SharedDictionary{TKey, TValue}"/> class.</summary>
        /// <param name="strategy">The locking strategy to use.</param>
        public SharedDictionary(LockingStrategy strategy)
            : this(LockingStrategyFactory.Create(strategy))
        {
        }

        /// <summary>Finalizes an instance of the <see cref="SharedDictionary{TKey, TValue}"/> class.</summary>
        ~SharedDictionary()
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
                return this.dict.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this.dict.IsReadOnly;
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this.dict.Keys;
            }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this.dict.Values;
            }
        }

        /// <summary>Gets the backing dictionnary to use.</summary>
        internal IDictionary<TKey, TValue> BackingDictionary
        {
            get
            {
                return this.dict;
            }
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                this.EnsureNotDisposed();
                this.EnsureReadAccess();
                return this.dict[key];
            }

            set
            {
                this.EnsureNotDisposed();
                this.EnsureWriteAccess();
                this.dict[key] = value;
            }
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
			Logger.LogDebug(">>> Entering GetEnumerator()");
			Logger.LogDebug("<<< Exiting GetEnumerator()");
            return this.IEnumerable_GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.dict.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
			Logger.LogDebug(">>> Entering Clear()");
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.dict.Clear();
			Logger.LogDebug("<<< Exiting Clear()");
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this.dict.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            this.dict.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            return this.dict.Remove(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
			Logger.LogDebug(">>> Entering ContainsKey(key={Key})", key);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogDebug("<<< Exiting ContainsKey(key={Key})", key);
            return this.dict.ContainsKey(key);
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
			Logger.LogDebug(">>> Entering Add(key={Key},value={Value})", key, value);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.dict.Add(key, value);
			Logger.LogDebug("<<< Exiting Add(key={Key},value={Value})", key, value);
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
			Logger.LogDebug("<<< Exiting Remove(key={Key})", key);
            return this.dict.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
			Logger.LogDebug(">>> Entering TryGetValue(key={Key})", key);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogDebug("<<< Exiting TryGetValue(key={Key})", key);
            return this.dict.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);

            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        /// <summary>Gets a read lock.</summary>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock()
        {
			Logger.LogDebug(">>> Entering GetReadLock()");
			Logger.LogDebug("<<< Exiting GetReadLock()");
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>Gets a read lock for a specified amount of time.</summary>
        /// <param name="timeOut">The amount of time to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
			Logger.LogDebug("<<< Exiting GetReadLock(timeOut={TimeOut})", timeOut);
            return this.lockController.GetReadLock(timeOut);
        }

        /// <summary>Gets a read lock for a specified amount of time.</summary>
        /// <param name="millisecondTimeout">For how many milliseconds to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock(int millisecondTimeout)
        {
			Logger.LogDebug("<<< Exiting GetReadLock(millisecondTimeout={MillisecondTimeout})", millisecondTimeout);
            return this.GetReadLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        /// <summary>Gets a write lock.</summary>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock()
        {
			Logger.LogDebug(">>> Entering GetWriteLock()");
			Logger.LogDebug("<<< Exiting GetWriteLock()");
            return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>Gets a write lock for the specified amount of time.</summary>
        /// <param name="timeOut">The amount of time to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
			Logger.LogDebug("<<< Exiting GetWriteLock(timeOut={TimeOut})", timeOut);
            return this.lockController.GetWriteLock(timeOut);
        }

        /// <summary>Gets a write lock for the specified amount of time.</summary>
        /// <param name="millisecondTimeout">The amount of milliseconds to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
			Logger.LogDebug("<<< Exiting GetWriteLock(millisecondTimeout={MillisecondTimeout})", millisecondTimeout);
            return this.GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        /// <summary>Returns an enumerator to iterate through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable_GetEnumerator()
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();

            // todo nothing ensures read lock is held for life of enumerator
            return this.dict.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.IEnumerable_GetEnumerator();
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
                    // dispose managed resources here
                    this.dict = null;
					Logger.LogDebug("Dispose(disposing={Disposing}): dict → {Dict}", disposing, dict);
					Logger.LogDebug("Dispose(disposing={Disposing}): this.dict → {Arg1}", disposing, this.dict);
                }

                // dispose unmanaged resources here
                this.lockController.Dispose();
                this.lockController = null;
				Logger.LogDebug("Dispose(disposing={Disposing}): lockController → {LockController}", disposing, lockController);
				Logger.LogDebug("Dispose(disposing={Disposing}): this.lockController → {Arg1}", disposing, this.lockController);
                this.isDisposed = true;
            }
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
			Logger.LogDebug("Dispose(disposing={Disposing}): this.isDisposed → {Arg1}", disposing, this.isDisposed);
        }

        private void EnsureReadAccess()
        {
			Logger.LogDebug(">>> Entering EnsureReadAccess()");
            if (!this.lockController.ThreadCanRead)
            {
                throw new ReadLockRequiredException();
            }
			Logger.LogDebug("<<< Exiting EnsureReadAccess()");
        }

        private void EnsureWriteAccess()
        {
			Logger.LogDebug(">>> Entering EnsureWriteAccess()");
            if (!this.lockController.ThreadCanWrite)
            {
                throw new WriteLockRequiredException();
            }
			Logger.LogDebug("<<< Exiting EnsureWriteAccess()");
        }

        private void EnsureNotDisposed()
        {
			Logger.LogDebug(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("SharedDictionary");
            }
			Logger.LogDebug("<<< Exiting EnsureNotDisposed()");
        }
    }
}
