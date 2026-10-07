// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>Provides naive locking for generic lists.</summary>
    /// <typeparam name="T">The type of value in the list.</typeparam>
    public class NaiveLockingList<T> : IList<T>, IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NaiveLockingList));
        /// <summary>Gets access to the underlying SharedList.</summary>
        /// <remarks> Allows locking to be explicitly managed for the sake of efficiency.</remarks>
        public SharedList<T> SharedList { get; } = new SharedList<T>();

        /// <inheritdoc />
        public int Count => this.DoInReadLock(() => this.SharedList.Count);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public T this[int index]
        {
            get => this.DoInReadLock(() => this.SharedList[index]);
            set => this.DoInWriteLock(() => this.SharedList[index] = value);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
			Logger.LogError(">>> Entering GetEnumerator()");
            // disposal of enumerator will release read lock
            // TODO is there a need for some sort of timed release?  the timer must release from the correct thread
            // if using RWLS
            var readLock = this.SharedList.GetReadLock();
			Logger.LogError("GetEnumerator(): readLock → {ReadLock}", readLock);
			Logger.LogWarning("<<< Exiting GetEnumerator()");
            return new NaiveLockingEnumerator(this.SharedList.GetEnumerator(), readLock);
        }

        /// <inheritdoc />
        public void Add(T item)
        {
			Logger.LogError(">>> Entering Add(item={Item})", item);
            this.DoInWriteLock(() => this.SharedList.Add(item));
			Logger.LogWarning("<<< Exiting Add(item={Item})", item);
        }

        /// <inheritdoc />
        public void Clear()
        {
			Logger.LogError(">>> Entering Clear()");
            this.DoInWriteLock(() => this.SharedList.Clear());
			Logger.LogWarning("<<< Exiting Clear()");
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
			Logger.LogError(">>> Entering Contains(item={Item})", item);
			Logger.LogWarning("<<< Exiting Contains(item={Item})", item);
            return this.DoInReadLock(() => this.SharedList.Contains(item));
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
			Logger.LogError(">>> Entering CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
            this.DoInReadLock(() => this.SharedList.CopyTo(array, arrayIndex));
			Logger.LogWarning("<<< Exiting CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
			Logger.LogError(">>> Entering Remove(item={Item})", item);
			Logger.LogWarning("<<< Exiting Remove(item={Item})", item);
            return this.DoInWriteLock(() => this.SharedList.Remove(item));
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
			Logger.LogError(">>> Entering IndexOf(item={Item})", item);
			Logger.LogWarning("<<< Exiting IndexOf(item={Item})", item);
            return this.DoInReadLock(() => this.SharedList.IndexOf(item));
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
			Logger.LogError(">>> Entering Insert(index={Index},item={Item})", index, item);
            this.DoInWriteLock(() => this.SharedList.Insert(index, item));
			Logger.LogWarning("<<< Exiting Insert(index={Index},item={Item})", index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
			Logger.LogError(">>> Entering RemoveAt(index={Index})", index);
            this.DoInWriteLock(() => this.SharedList.RemoveAt(index));
			Logger.LogWarning("<<< Exiting RemoveAt(index={Index})", index);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.SharedList?.Dispose();
            }
			Logger.LogError(">>> Entering Dispose()");
        }

        // TODO is no recursion the correct policy
        private void DoInReadLock(Action action)
        {
			Logger.LogError(">>> Entering DoInReadLock(action={Action})", action);
            this.DoInReadLock(() =>
            {
                action.Invoke();
			Logger.LogWarning("<<< Exiting DoInReadLock(action={Action})", action);
                return true;
            });
        }

        private TRet DoInReadLock<TRet>(Func<TRet> func)
        {
            using (this.SharedList.GetReadLock())
            {
                return func.Invoke();
            }
        }

        private void DoInWriteLock(Action action)
        {
			Logger.LogError(">>> Entering DoInWriteLock(action={Action})", action);
            this.DoInWriteLock(() =>
            {
                action.Invoke();
			Logger.LogWarning("<<< Exiting DoInWriteLock(action={Action})", action);
                return true;
            });
        }

        private TRet DoInWriteLock<TRet>(Func<TRet> func)
        {
            using (this.SharedList.GetWriteLock())
            {
                return func.Invoke();
            }
        }

        /// <summary>An <see cref="IEnumerator{T}"/> implementation for <see cref="NaiveLockingList{T}"/>.</summary>
        public class NaiveLockingEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<T> enumerator;
            private readonly ISharedCollectionLock readLock;
            private bool isDisposed;

            /// <summary>Initializes a new instance of the <see cref="NaiveLockingEnumerator"/> class.</summary>
            /// <param name="enumerator">The enumerator to implement locking on.</param>
            /// <param name="readLock">An <see cref="ISharedCollectionLock"/> instance to implement the read lock.</param>
            public NaiveLockingEnumerator(IEnumerator<T> enumerator, ISharedCollectionLock readLock)
            {
				Logger.LogError(">>> Entering NaiveLockingEnumerator(enumerator={Enumerator},readLock={ReadLock})", enumerator, readLock);
				Logger.LogDebug("NaiveLockingEnumerator(enumerator={Enumerator},readLock={ReadLock}): readLock → {ReadLock1}", enumerator, readLock, readLock);
                this.enumerator = enumerator;
                this.readLock = readLock;
				Logger.LogDebug("NaiveLockingEnumerator(enumerator={Enumerator},readLock={ReadLock}): this.enumerator → {Arg2}", enumerator, readLock, this.enumerator);
            Logger.LogWarning("<<< Exiting NaiveLockingEnumerator(enumerator={Enumerator},readLock={ReadLock})", enumerator, readLock);
            }

            /// <summary>Finalizes an instance of the <see cref="NaiveLockingEnumerator"/> class.</summary>
            ~NaiveLockingEnumerator()
            {
                this.Dispose(false);
            }

            /// <inheritdoc />
            public T Current
            {
                get
                {
                    return this.enumerator.Current;
                }
            }

            /// <inheritdoc />
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
				Logger.LogError(">>> Entering MoveNext()");
                Logger.LogWarning("<<< Exiting MoveNext()");
                return this.enumerator.MoveNext();
            }

            /// <inheritdoc />
            public void Reset()
            {
				Logger.LogError(">>> Entering Reset()");
                this.enumerator.Reset();
				Logger.LogWarning("<<< Exiting Reset()");
            }

            /// <inheritdoc />
            public void Dispose()
            {
				Logger.LogError(">>> Entering Dispose()");
                this.Dispose(true);

                GC.SuppressFinalize(this);
				Logger.LogWarning("<<< Exiting Dispose()");
            }

            /// <summary>Disposes this instance resources.</summary>
            /// <param name="disposing">A value indicating whether this instance is currently disposing.</param>
            protected virtual void Dispose(bool disposing)
            {
				Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
                if (!this.isDisposed)
                {
					Logger.LogWarning("<<< Exiting Dispose()");
                    if (disposing)
                    {
                        // dispose managed resources here
                        this.enumerator.Dispose();
                        this.readLock.Dispose();
                    }

                    // dispose unmanaged resources here
					Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
                    this.isDisposed = true;
					Logger.LogDebug("Dispose(disposing={Disposing}): this.isDisposed → {Arg1}", disposing, this.isDisposed);
                }
            }
        }
    }
}
