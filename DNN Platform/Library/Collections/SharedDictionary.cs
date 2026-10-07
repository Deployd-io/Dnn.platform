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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SharedDictionary));
        private IDictionary<TKey, TValue> dict;

        private bool isDisposed;
        private ILockStrategy lockController;

        /// <summary>Initializes a new instance of the <see cref="SharedDictionary{TKey, TValue}"/> class.</summary>
        public SharedDictionary()
            : this(LockingStrategy.ReaderWriter)
        {
		Logger.LogWarning("<<< Exiting SharedDictionary(lockStrategy={LockStrategy})", lockStrategy);
        }

        /// <summary>Initializes a new instance of the <see cref="SharedDictionary{TKey, TValue}"/> class.</summary>
        /// <param name="lockStrategy">The locking strategy to use.</param>
        public SharedDictionary(ILockStrategy lockStrategy)
        {
			Logger.LogError(">>> Entering SharedDictionary(lockStrategy={LockStrategy})", lockStrategy);
            this.dict = new Dictionary<TKey, TValue>();
			Logger.LogWarning("<<< Exiting SharedDictionary(lockStrategy={LockStrategy})", lockStrategy);
            this.lockController = lockStrategy;
			Logger.LogDebug("SharedDictionary(lockStrategy={LockStrategy}): lockController → {LockController}", lockStrategy, lockController);
			Logger.LogError(">>> Entering SharedDictionary(strategy={Strategy})", strategy);
        Logger.LogWarning("<<< Exiting SharedDictionary(strategy={Strategy})", strategy);
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
			Logger.LogError(">>> Entering GetEnumerator()");
            Logger.LogWarning("<<< Exiting GetEnumerator()");
            return this.IEnumerable_GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
			Logger.LogError(">>> Entering Add(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.dict.Add(item);
			Logger.LogWarning("<<< Exiting Add(item={Item})", item);
        }

        /// <inheritdoc />
        public void Clear()
        {
			Logger.LogError(">>> Entering Clear()");
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.dict.Clear();
			Logger.LogWarning("<<< Exiting Clear()");
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
			Logger.LogError(">>> Entering Contains(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogWarning("<<< Exiting Contains(item={Item})", item);
            return this.dict.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
			Logger.LogError(">>> Entering CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            this.dict.CopyTo(array, arrayIndex);
			Logger.LogWarning("<<< Exiting CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
			Logger.LogError(">>> Entering Remove(item={Item})", item);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
			Logger.LogWarning("<<< Exiting Remove(item={Item})", item);
			Logger.LogWarning("<<< Exiting Remove(key={Key})", key);
            return this.dict.Remove(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
			Logger.LogError(">>> Entering ContainsKey(key={Key})", key);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogWarning("<<< Exiting ContainsKey(key={Key})", key);
            return this.dict.ContainsKey(key);
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
			Logger.LogError(">>> Entering Add(key={Key},value={Value})", key, value);
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.dict.Add(key, value);
			Logger.LogWarning("<<< Exiting Add(key={Key},value={Value})", key, value);
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            return this.dict.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
			Logger.LogError(">>> Entering TryGetValue(key={Key})", key);
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
			Logger.LogWarning("<<< Exiting TryGetValue(key={Key})", key);
            return this.dict.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);

			Logger.LogWarning("<<< Exiting Dispose()");
            GC.SuppressFinalize(this);
        }

        /// <summary>Gets a read lock.</summary>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock()
        {
			Logger.LogError(">>> Entering GetReadLock()");
            Logger.LogWarning("<<< Exiting GetReadLock()");
			Logger.LogWarning("<<< Exiting GetReadLock(timeOut={TimeOut})", timeOut);
            return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>Gets a read lock for a specified amount of time.</summary>
        /// <param name="timeOut">The amount of time to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
            return this.lockController.GetReadLock(timeOut);
        }

        /// <summary>Gets a read lock for a specified amount of time.</summary>
        /// <param name="millisecondTimeout">For how many milliseconds to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetReadLock(int millisecondTimeout)
        {
            return this.GetReadLock(TimeSpan.FromMilliseconds(millisecondTimeout));
        }

        /// <summary>Gets a write lock.</summary>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock()
        {
			Logger.LogError(">>> Entering GetWriteLock()");
            Logger.LogWarning("<<< Exiting GetWriteLock()");
			Logger.LogWarning("<<< Exiting GetWriteLock(timeOut={TimeOut})", timeOut);
            return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>Gets a write lock for the specified amount of time.</summary>
        /// <param name="timeOut">The amount of time to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock(TimeSpan timeOut)
        {
            this.EnsureNotDisposed();
            return this.lockController.GetWriteLock(timeOut);
        }

        /// <summary>Gets a write lock for the specified amount of time.</summary>
        /// <param name="millisecondTimeout">The amount of milliseconds to lock for.</param>
        /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
        public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
        {
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
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
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
				Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
                this.lockController = null;
				Logger.LogDebug("Dispose(disposing={Disposing}): lockController → {LockController}", disposing, lockController);
				Logger.LogDebug("Dispose(disposing={Disposing}): this.lockController → {Arg1}", disposing, this.lockController);
                this.isDisposed = true;
            }
        }

        private void EnsureReadAccess()
        {
			Logger.LogError(">>> Entering EnsureReadAccess()");
            if (!this.lockController.ThreadCanRead)
            {
				Logger.LogError("Throwing ReadLockRequiredException from EnsureReadAccess()");
                throw new ReadLockRequiredException();
            }
			Logger.LogWarning("<<< Exiting EnsureReadAccess()");
        }

        private void EnsureWriteAccess()
        {
			Logger.LogError(">>> Entering EnsureWriteAccess()");
            if (!this.lockController.ThreadCanWrite)
            {
				Logger.LogError("Throwing WriteLockRequiredException from EnsureWriteAccess()");
                throw new WriteLockRequiredException();
            }
			Logger.LogWarning("<<< Exiting EnsureWriteAccess()");
        }

        private void EnsureNotDisposed()
        {
			Logger.LogError(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
				Logger.LogError("Throwing ObjectDisposedException from EnsureNotDisposed()");
                throw new ObjectDisposedException("SharedDictionary");
            }
			Logger.LogWarning("<<< Exiting EnsureNotDisposed()");
        }
    }
}
