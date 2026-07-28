// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;

    /// <summary>A collection of PageHierarchyData objects.</summary>
    public class NavDataPageHierarchicalEnumerable : ArrayList, IHierarchicalEnumerable, IList<NavDataPageHierarchyData>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        NavDataPageHierarchyData IList<NavDataPageHierarchyData>.this[int index]
        {
            get => (NavDataPageHierarchyData)this[index];
            set => this[index] = value;
        }

        /// <summary>Handles enumeration.</summary>
        /// <param name="enumeratedItem">THe <see cref="IHierarchyData"/> item.</param>
        /// <returns><paramref name="enumeratedItem"/>.</returns>
        public virtual IHierarchyData GetHierarchyData(object enumeratedItem)
        {
			Logger.LogDebug(">>> Entering GetHierarchyData(enumeratedItem={EnumeratedItem})", enumeratedItem);
			Logger.LogDebug("<<< Exiting GetHierarchyData(enumeratedItem={EnumeratedItem})", enumeratedItem);
            return (IHierarchyData)enumeratedItem;
        }

        /// <inheritdoc />
        public void Add(NavDataPageHierarchyData item) => base.Add(item);

        /// <inheritdoc />
        public bool Contains(NavDataPageHierarchyData item) => base.Contains(item);

        /// <inheritdoc />
        public void CopyTo(NavDataPageHierarchyData[] array, int arrayIndex) => base.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(NavDataPageHierarchyData item)
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
        public int IndexOf(NavDataPageHierarchyData item) => base.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, NavDataPageHierarchyData item) => base.Insert(index, item);

        /// <inheritdoc />
        IEnumerator<NavDataPageHierarchyData> IEnumerable<NavDataPageHierarchyData>.GetEnumerator()
        {
            return new NavDataPageHierarchyDataEnumerator(this.GetEnumerator());
        }

        private class NavDataPageHierarchyDataEnumerator(IEnumerator enumerator) : IEnumerator<NavDataPageHierarchyData>
        {
			Logger.LogDebug(">>> Entering NavDataPageHierarchyDataEnumerator(enumerator={Enumerator})", enumerator);
            /// <inheritdoc />
            public NavDataPageHierarchyData Current => (NavDataPageHierarchyData)enumerator.Current;

			Logger.LogDebug("NavDataPageHierarchyDataEnumerator(enumerator={Enumerator}): Current → {Current}", enumerator, Current);
            /// <inheritdoc />
            object IEnumerator.Current => this.Current;

			Logger.LogDebug("<<< Exiting NavDataPageHierarchyDataEnumerator(enumerator={Enumerator})", enumerator);
            /// <inheritdoc />
            public void Dispose() => (enumerator as IDisposable)?.Dispose();

            /// <inheritdoc />
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => enumerator.Reset();
        }
    }
}
