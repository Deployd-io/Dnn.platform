// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;

    /// <summary>Restricts the client to add only controls of specific type into the control collection.</summary>
    /// <typeparam name="T">The type of control.</typeparam>
    public class TypedControlCollection<T> : ControlCollection, IList<T>
        where T : Control
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TypedControlCollection{T}"/> class.</summary>
        /// <param name="owner">The owner control.</param>
        public TypedControlCollection(Control owner)
            : base(owner)
        {
		Logger.LogDebug(">>> Entering TypedControlCollection(owner={Owner})", owner);
		Logger.LogDebug("<<< Exiting TypedControlCollection(owner={Owner})", owner);
        }

        /// <inheritdoc />
        public new T this[int index]
        {
            get => (T)base[index];
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public int IndexOf(T item)
            => base.IndexOf(item);
			Logger.LogDebug(">>> Entering IndexOf(item={Item})", item);

		Logger.LogDebug("<<< Exiting IndexOf(item={Item})", item);
        /// <inheritdoc />
        public void Insert(int index, T item)
            => throw new NotSupportedException();
			Logger.LogDebug(">>> Entering Insert(index={Index},item={Item})", index, item);

		Logger.LogDebug("<<< Exiting Insert(index={Index},item={Item})", index, item);
        /// <inheritdoc />
        public override void Add(Control child)
        {
			Logger.LogDebug(">>> Entering Add(child={Child})", child);
            if (child is not T item)
            {
                this.ThrowOnInvalidControlType();
            }

            base.Add(child);
			Logger.LogDebug("<<< Exiting Add(child={Child})", child);
        }

        /// <inheritdoc />
        public override void AddAt(int index, Control child)
        {
			Logger.LogDebug(">>> Entering AddAt(index={Index},child={Child})", index, child);
            if (child is not T)
            {
                this.ThrowOnInvalidControlType();
            }

            base.AddAt(index, child);
			Logger.LogDebug("<<< Exiting AddAt(index={Index},child={Child})", index, child);
        }

        /// <inheritdoc />
        public void Add(T item)
            => base.Add(item);

        /// <inheritdoc />
        public bool Contains(T item)
            => base.Contains(item);
			Logger.LogDebug(">>> Entering Contains(item={Item})", item);

		Logger.LogDebug("<<< Exiting Contains(item={Item})", item);
        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
            => base.CopyTo(array, arrayIndex);
			Logger.LogDebug(">>> Entering CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);

		Logger.LogDebug("<<< Exiting CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
        /// <inheritdoc />
        public bool Remove(T item)
        {
			Logger.LogDebug(">>> Entering Remove(item={Item})", item);
            if (!this.Contains(item))
            {
                return false;
            }

            base.Remove(item);
			Logger.LogDebug("<<< Exiting Remove(item={Item})", item);
            return true;
        }

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => new TypedEnumerator(this.GetEnumerator());

        /// <summary>Throws an <see cref="InvalidOperationException"/>.</summary>
        /// <exception cref="InvalidOperationException">Always.</exception>
        protected virtual void ThrowOnInvalidControlType()
        {
			Logger.LogDebug(">>> Entering ThrowOnInvalidControlType()");
            throw new InvalidOperationException("Not supported");
        }

        private sealed class TypedEnumerator(IEnumerator enumerator) : IEnumerator<T>
        {
			Logger.LogDebug(">>> Entering TypedEnumerator(enumerator={Enumerator})", enumerator);
            /// <inheritdoc />
            public T Current => (T)enumerator.Current;

			Logger.LogDebug("TypedEnumerator(enumerator={Enumerator}): Current → {Current}", enumerator, Current);
            /// <inheritdoc />
            object IEnumerator.Current => this.Current;

			Logger.LogDebug("<<< Exiting TypedEnumerator(enumerator={Enumerator})", enumerator);
            /// <inheritdoc />
            public void Dispose() => (enumerator as IDisposable)?.Dispose();

            /// <inheritdoc />
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => enumerator.Reset();
        }
    }
}
