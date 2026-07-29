// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// 
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;
using System.Collections;

namespace log4net.Appender
{
    /// <summary>
    /// A strongly-typed collection of <see cref="IAppender"/> objects.
    /// </summary>
    /// <author>Nicko Cadell</author>
    public class AppenderCollection : ICollection, IList, IEnumerable
#if !NETSTANDARD1_3
        , ICloneable
#endif
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Supports type-safe iteration over a <see cref="AppenderCollection"/>.
        /// </summary>
        /// <exclude/>
        public interface IAppenderCollectionEnumerator
        {
            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            IAppender Current { get; }

            /// <summary>
            /// Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element; 
            /// <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            bool MoveNext();

            /// <summary>
            /// Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            void Reset();
        }

        private const int DEFAULT_CAPACITY = 16;

        private IAppender[] m_array;
        private int m_count = 0;
        private int m_version = 0;

        /// <summary>
        /// Creates a read-only wrapper for a <c>AppenderCollection</c> instance.
        /// </summary>
        /// <param name="list">list to create a readonly wrapper arround</param>
        /// <returns>
        /// An <c>AppenderCollection</c> wrapper that is read-only.
        /// </returns>
        public static AppenderCollection ReadOnly(AppenderCollection list)
        {
			Logger.LogDebug(">>> Entering ReadOnly(list={List})", list);
            if(list == null)
            {
                throw new ArgumentNullException("list");
            }

			Logger.LogDebug("<<< Exiting ReadOnly(list={List})", list);
            return new ReadOnlyAppenderCollection(list);
        }

        /// <summary>
        /// An empty readonly static AppenderCollection
        /// </summary>
        public static readonly AppenderCollection EmptyCollection = ReadOnly(new AppenderCollection(0));

        /// <summary>
        /// Initializes a new instance of the <c>AppenderCollection</c> class
        /// that is empty and has the default initial capacity.
        /// </summary>
        public AppenderCollection()
        {
			Logger.LogDebug(">>> Entering AppenderCollection()");
            this.m_array = new IAppender[DEFAULT_CAPACITY];
			Logger.LogDebug("<<< Exiting AppenderCollection()");
			Logger.LogDebug("AppenderCollection(): this.m_array → {Arg0}", this.m_array);
        }
        
        /// <summary>
        /// Initializes a new instance of the <c>AppenderCollection</c> class
        /// that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The number of elements that the new <c>AppenderCollection</c> is initially capable of storing.
        /// </param>
        public AppenderCollection(int capacity)
        {
			Logger.LogDebug(">>> Entering AppenderCollection(capacity={Capacity})", capacity);
            this.m_array = new IAppender[capacity];
			Logger.LogDebug("<<< Exiting AppenderCollection(capacity={Capacity})", capacity);
			Logger.LogDebug("AppenderCollection(capacity={Capacity}): this.m_array → {Arg1}", capacity, this.m_array);
        }

        /// <summary>
        /// Initializes a new instance of the <c>AppenderCollection</c> class
        /// that contains elements copied from the specified <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="c">The <c>AppenderCollection</c> whose elements are copied to the new collection.</param>
        public AppenderCollection(AppenderCollection c)
        {
			Logger.LogDebug(">>> Entering AppenderCollection(c={C})", c);
            this.m_array = new IAppender[c.Count];
            this.AddRange(c);
			Logger.LogDebug("<<< Exiting AppenderCollection(c={C})", c);
			Logger.LogDebug("AppenderCollection(c={C}): this.m_array → {Arg1}", c, this.m_array);
        }

        /// <summary>
        /// Initializes a new instance of the <c>AppenderCollection</c> class
        /// that contains elements copied from the specified <see cref="IAppender"/> array.
        /// </summary>
        /// <param name="a">The <see cref="IAppender"/> array whose elements are copied to the new list.</param>
        public AppenderCollection(IAppender[] a)
        {
			Logger.LogDebug(">>> Entering AppenderCollection(a={A})", a);
            this.m_array = new IAppender[a.Length];
            this.AddRange(a);
			Logger.LogDebug("<<< Exiting AppenderCollection(a={A})", a);
			Logger.LogDebug("AppenderCollection(a={A}): this.m_array → {Arg1}", a, this.m_array);
        }
        
        /// <summary>
        /// Initializes a new instance of the <c>AppenderCollection</c> class
        /// that contains elements copied from the specified <see cref="IAppender"/> collection.
        /// </summary>
        /// <param name="col">The <see cref="IAppender"/> collection whose elements are copied to the new list.</param>
        public AppenderCollection(ICollection col)
        {
			Logger.LogDebug(">>> Entering AppenderCollection(col={Col})", col);
            this.m_array = new IAppender[col.Count];
            this.AddRange(col);
			Logger.LogDebug("<<< Exiting AppenderCollection(col={Col})", col);
			Logger.LogDebug("AppenderCollection(col={Col}): this.m_array → {Arg1}", col, this.m_array);
        }

        /// <summary>
        /// Type visible only to our subclasses
        /// Used to access protected constructor
        /// </summary>
        /// <exclude/>
        protected internal enum Tag 
        {
            /// <summary>
            /// A value
            /// </summary>
            Default
        }

        /// <summary>
        /// Allow subclasses to avoid our default constructors
        /// </summary>
        /// <param name="tag"></param>
        /// <exclude/>
        protected internal AppenderCollection(Tag tag)
        {
			Logger.LogDebug(">>> Entering AppenderCollection(tag={Tag})", tag);
            this.m_array = null;
			Logger.LogDebug("<<< Exiting AppenderCollection(tag={Tag})", tag);
			Logger.LogDebug("AppenderCollection(tag={Tag}): this.m_array → {Arg1}", tag, this.m_array);
        }

        /// <summary>
        /// Gets the number of elements actually contained in the <c>AppenderCollection</c>.
        /// </summary>
        public virtual int Count
        {
            get { return this.m_count; }
        }

        /// <summary>
        /// Copies the entire <c>AppenderCollection</c> to a one-dimensional
        /// <see cref="IAppender"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="IAppender"/> array to copy to.</param>
        public virtual void CopyTo(IAppender[] array)
        {
            this.CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the entire <c>AppenderCollection</c> to a one-dimensional
        /// <see cref="IAppender"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="IAppender"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(IAppender[] array, int start)
        {
			Logger.LogDebug(">>> Entering CopyTo(array={Array},start={Start})", array, start);
            if (this.m_count > array.GetUpperBound(0) + 1 - start)
            {
                throw new ArgumentException("Destination array was not long enough.");
            }
            
            Array.Copy(this.m_array, 0, array, start, this.m_count); 
			Logger.LogDebug("<<< Exiting CopyTo(array={Array},start={Start})", array, start);
			Logger.LogDebug(">>> Entering CopyTo(array={Array})", array);
			Logger.LogDebug("<<< Exiting CopyTo(array={Array})", array);
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        /// <returns>false, because the backing type is an array, which is never thread-safe.</returns>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return this.m_array; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAppender"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///        <para><paramref name="index"/> is less than zero</para>
        ///        <para>-or-</para>
        ///        <para><paramref name="index"/> is equal to or greater than <see cref="AppenderCollection.Count"/>.</para>
        /// </exception>
        public virtual IAppender this[int index]
        {
            get
            {
                this.ValidateIndex(index); // throws
                return this.m_array[index]; 
            }
            set
            {
                this.ValidateIndex(index); // throws
                ++this.m_version;
                this.m_array[index] = value; 
            }
        }

        /// <summary>
        /// Adds a <see cref="IAppender"/> to the end of the <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="IAppender"/> to be added to the end of the <c>AppenderCollection</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(IAppender item)
        {
			Logger.LogDebug(">>> Entering Add(item={Item})", item);
            if (this.m_count == this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + 1);
            }

            this.m_array[this.m_count] = item;
            this.m_version++;

			Logger.LogDebug("<<< Exiting Add(item={Item})", item);
            return this.m_count++;
        }
        
        /// <summary>
        /// Removes all elements from the <c>AppenderCollection</c>.
        /// </summary>
        public virtual void Clear()
        {
			Logger.LogDebug(">>> Entering Clear()");
            ++this.m_version;
            this.m_array = new IAppender[DEFAULT_CAPACITY];
			Logger.LogDebug("Clear(): this.m_array → {Arg0}", this.m_array);
            this.m_count = 0;
			Logger.LogDebug("<<< Exiting Clear()");
			Logger.LogDebug("Clear(): m_count → {M_count}", m_count);
			Logger.LogDebug("Clear(): this.m_count → {Arg0}", this.m_count);
        }
        
        /// <summary>
        /// Creates a shallow copy of the <see cref="AppenderCollection"/>.
        /// </summary>
        /// <returns>A new <see cref="AppenderCollection"/> with a shallow copy of the collection data.</returns>
        public virtual object Clone()
        {
			Logger.LogDebug(">>> Entering Clone()");
            AppenderCollection newCol = new AppenderCollection(this.m_count);
            Array.Copy(this.m_array, 0, newCol.m_array, 0, this.m_count);
			Logger.LogDebug("Clone(): newCol → {NewCol}", newCol);
			Logger.LogDebug("Clone(): newCol.m_count → {Arg0}", newCol.m_count);
			Logger.LogDebug("Clone(): newCol.m_version → {Arg0}", newCol.m_version);
            newCol.m_count = this.m_count;
            newCol.m_version = this.m_version;

			Logger.LogDebug("<<< Exiting Clone()");
            return newCol;
        }

        /// <summary>
        /// Determines whether a given <see cref="IAppender"/> is in the <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="IAppender"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>AppenderCollection</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(IAppender item)
        {
			Logger.LogDebug(">>> Entering Contains(item={Item})", item);
            for (int i = 0; i != this.m_count; ++i)
            {
                if (this.m_array[i].Equals(item))
                {
                    return true;
                }
            }
			Logger.LogDebug("<<< Exiting Contains(item={Item})", item);
            return false;
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a <see cref="IAppender"/>
        /// in the <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="IAppender"/> to locate in the <c>AppenderCollection</c>.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item"/> 
        /// in the entire <c>AppenderCollection</c>, if found; otherwise, -1.
        /// </returns>
        public virtual int IndexOf(IAppender item)
        {
			Logger.LogDebug(">>> Entering IndexOf(item={Item})", item);
            for (int i = 0; i != this.m_count; ++i)
            {
                if (this.m_array[i].Equals(item))
                {
                    return i;
                }
            }
			Logger.LogDebug("<<< Exiting IndexOf(item={Item})", item);
            return -1;
        }

        /// <summary>
        /// Inserts an element into the <c>AppenderCollection</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="IAppender"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="AppenderCollection.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, IAppender item)
        {
			Logger.LogDebug(">>> Entering Insert(index={Index},item={Item})", index, item);
            this.ValidateIndex(index, true); // throws
            
            if (this.m_count == this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + 1);
            }

            if (index < this.m_count)
            {
                Array.Copy(this.m_array, index, this.m_array, index + 1, this.m_count - index);
            }

            this.m_array[index] = item;
            this.m_count++;
            this.m_version++;
			Logger.LogDebug("<<< Exiting Insert(index={Index},item={Item})", index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific <see cref="IAppender"/> from the <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="IAppender"/> to remove from the <c>AppenderCollection</c>.</param>
        /// <exception cref="ArgumentException">
        /// The specified <see cref="IAppender"/> was not found in the <c>AppenderCollection</c>.
        /// </exception>
        public virtual void Remove(IAppender item)
        {		   
			Logger.LogDebug(">>> Entering Remove(item={Item})", item);
            int i = this.IndexOf(item);
            if (i < 0)
            {
                throw new ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
            }
            
            ++this.m_version;
            this.RemoveAt(i);
			Logger.LogDebug("<<< Exiting Remove(item={Item})", item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="AppenderCollection.Count"/>.</para>
        /// </exception>
        public virtual void RemoveAt(int index)
        {
			Logger.LogDebug(">>> Entering RemoveAt(index={Index})", index);
            this.ValidateIndex(index); // throws

            this.m_count--;

            if (index < this.m_count)
            {
                Array.Copy(this.m_array, index + 1, this.m_array, index, this.m_count - index);
            }
            
            // We can't set the deleted entry equal to null, because it might be a value type.
            // Instead, we'll create an empty single-element array of the right type and copy it 
            // over the entry we want to erase.
            IAppender[] temp = new IAppender[1];
            Array.Copy(temp, 0, this.m_array, this.m_count, 1);
            this.m_version++;
			Logger.LogDebug("<<< Exiting RemoveAt(index={Index})", index);
        }

        /// <summary>
        /// Gets a value indicating whether the collection has a fixed size.
        /// </summary>
        /// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the IList is read-only.
        /// </summary>
        /// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the <c>AppenderCollection</c>.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <c>AppenderCollection</c>.</returns>
        public virtual IAppenderCollectionEnumerator GetEnumerator()
        {
			Logger.LogDebug("<<< Exiting GetEnumerator()");
            return new Enumerator(this);
        }

        /// <summary>
        /// Gets or sets the number of elements the <c>AppenderCollection</c> can contain.
        /// </summary>
        public virtual int Capacity
        {
            get 
            { 
                return this.m_array.Length; 
            }
            set
            {
                if (value < this.m_count)
                {
                    value = this.m_count;
                }

                if (value != this.m_array.Length)
                {
                    if (value > 0)
                    {
                        IAppender[] temp = new IAppender[value];
                        Array.Copy(this.m_array, 0, temp, 0, this.m_count);
                        this.m_array = temp;
                    }
                    else
                    {
                        this.m_array = new IAppender[DEFAULT_CAPACITY];
                    }
                }
            }
        }

        /// <summary>
        /// Adds the elements of another <c>AppenderCollection</c> to the current <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="x">The <c>AppenderCollection</c> whose elements should be added to the end of the current <c>AppenderCollection</c>.</param>
        /// <returns>The new <see cref="AppenderCollection.Count"/> of the <c>AppenderCollection</c>.</returns>
        public virtual int AddRange(AppenderCollection x)
        {
			Logger.LogDebug(">>> Entering AddRange(x={X})", x);
            if (this.m_count + x.Count >= this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + x.Count);
            }
            
            Array.Copy(x.m_array, 0, this.m_array, this.m_count, x.Count);
            this.m_count += x.Count;
            this.m_version++;

			Logger.LogDebug("<<< Exiting AddRange(x={X})", x);
            return this.m_count;
        }

        /// <summary>
        /// Adds the elements of a <see cref="IAppender"/> array to the current <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="x">The <see cref="IAppender"/> array whose elements should be added to the end of the <c>AppenderCollection</c>.</param>
        /// <returns>The new <see cref="AppenderCollection.Count"/> of the <c>AppenderCollection</c>.</returns>
        public virtual int AddRange(IAppender[] x)
        {
            if (this.m_count + x.Length >= this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + x.Length);
            }

            Array.Copy(x, 0, this.m_array, this.m_count, x.Length);
            this.m_count += x.Length;
            this.m_version++;

            return this.m_count;
        }
        
        /// <summary>
        /// Adds the elements of a <see cref="IAppender"/> collection to the current <c>AppenderCollection</c>.
        /// </summary>
        /// <param name="col">The <see cref="IAppender"/> collection whose elements should be added to the end of the <c>AppenderCollection</c>.</param>
        /// <returns>The new <see cref="AppenderCollection.Count"/> of the <c>AppenderCollection</c>.</returns>
        public virtual int AddRange(ICollection col)
        {
            if (this.m_count + col.Count >= this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + col.Count);
            }

            foreach(object item in col)
            {
                this.Add((IAppender)item);
            }

			Logger.LogDebug("<<< Exiting AddRange(col={Col})", col);
            return this.m_count;
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements.
        /// </summary>
        public virtual void TrimToSize()
        {
            this.Capacity = this.m_count;
			Logger.LogDebug(">>> Entering TrimToSize()");
			Logger.LogDebug("<<< Exiting TrimToSize()");
        }

        /// <summary>
        /// Return the collection elements as an array
        /// </summary>
        /// <returns>the array</returns>
        public virtual IAppender[] ToArray()
        {
			Logger.LogDebug(">>> Entering ToArray()");
            IAppender[] resultArray = new IAppender[this.m_count];
            if (this.m_count > 0)
            {
                Array.Copy(this.m_array, 0, resultArray, 0, this.m_count);
            }
			Logger.LogDebug("<<< Exiting ToArray()");
            return resultArray;
        }

        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="i"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="i"/> is equal to or greater than <see cref="AppenderCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i)
        {
            this.ValidateIndex(i, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="i"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="i"/> is equal to or greater than <see cref="AppenderCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i, bool allowEqualEnd)
        {
            int max = (allowEqualEnd) ? (this.m_count) : (this.m_count-1);
            if (i < 0 || i > max)
            {
                throw Util.SystemInfo.CreateArgumentOutOfRangeException("i", (object)i, "Index was out of range. Must be non-negative and less than the size of the collection. [" + (object)i + "] Specified argument was out of the range of valid values.");
            }
        }

        private void EnsureCapacity(int min)
        {
			Logger.LogDebug(">>> Entering EnsureCapacity(min={Min})", min);
            int newCapacity = ((this.m_array.Length == 0) ? DEFAULT_CAPACITY : this.m_array.Length * 2);
            if (newCapacity < min)
			Logger.LogDebug("EnsureCapacity(min={Min}): newCapacity → {NewCapacity}", min, newCapacity);
            {
                newCapacity = min;
            }

            this.Capacity = newCapacity;
			Logger.LogDebug("<<< Exiting EnsureCapacity(min={Min})", min);
			Logger.LogDebug("EnsureCapacity(min={Min}): this.Capacity → {Arg1}", min, this.Capacity);
        }

        void ICollection.CopyTo(Array array, int start)
        {
            if (this.m_count > 0)
            {
                Array.Copy(this.m_array, 0, array, start, this.m_count);
            }
        }

        object IList.this[int i]
        {
            get { return (object)this[i]; }
            set { this[i] = (IAppender)value; }
        }

        int IList.Add(object x)
        {
			Logger.LogDebug(">>> Entering Add(x={X})", x);
			Logger.LogDebug("<<< Exiting Add(x={X})", x);
            return this.Add((IAppender)x);
        }

        bool IList.Contains(object x)
        {
			Logger.LogDebug(">>> Entering Contains(x={X})", x);
			Logger.LogDebug("<<< Exiting Contains(x={X})", x);
            return this.Contains((IAppender)x);
        }

        int IList.IndexOf(object x)
        {
			Logger.LogDebug(">>> Entering IndexOf(x={X})", x);
			Logger.LogDebug("<<< Exiting IndexOf(x={X})", x);
            return this.IndexOf((IAppender)x);
        }

        void IList.Insert(int pos, object x)
        {
            this.Insert(pos, (IAppender)x);
        }

        void IList.Remove(object x)
        {
            this.Remove((IAppender)x);
        }

        void IList.RemoveAt(int pos)
        {
            this.RemoveAt(pos);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
			Logger.LogDebug(">>> Entering GetEnumerator()");
            return (IEnumerator)(this.GetEnumerator());
        }

        /// <summary>
        /// Supports simple iteration over a <see cref="AppenderCollection"/>.
        /// </summary>
        /// <exclude/>
        private sealed class Enumerator : IEnumerator, IAppenderCollectionEnumerator
        {
            private readonly AppenderCollection m_collection;
            private int m_index;
            private int m_version;

            /// <summary>
            /// Initializes a new instance of the <c>Enumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal Enumerator(AppenderCollection tc)
            {
				Logger.LogDebug(">>> Entering Enumerator(tc={Tc})", tc);
                this.m_collection = tc;
                this.m_index = -1;
				Logger.LogDebug("Enumerator(tc={Tc}): this.m_collection → {Arg1}", tc, this.m_collection);
                this.m_version = tc.m_version;
				Logger.LogDebug("Enumerator(tc={Tc}): m_index → {M_index}", tc, m_index);
				Logger.LogDebug("Enumerator(tc={Tc}): this.m_index → {Arg1}", tc, this.m_index);
				Logger.LogDebug("Enumerator(tc={Tc}): m_version → {M_version}", tc, m_version);
				Logger.LogDebug("Enumerator(tc={Tc}): this.m_version → {Arg1}", tc, this.m_version);
				Logger.LogDebug("Enumerator(tc={Tc}): m_collection → {M_collection}", tc, m_collection);
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public IAppender Current
            {
                get { return this.m_collection[this.m_index]; }
            }

            /// <summary>
            /// Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element; 
            /// <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
				Logger.LogDebug(">>> Entering MoveNext()");
                if (this.m_version != this.m_collection.m_version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                ++this.m_index;
				Logger.LogDebug("<<< Exiting MoveNext()");
                return (this.m_index < this.m_collection.Count);
            }

            /// <summary>
            /// Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            public void Reset()
            {
				Logger.LogDebug(">>> Entering Reset()");
                this.m_index = -1;
				Logger.LogDebug("<<< Exiting Reset()");
				Logger.LogDebug("Reset(): m_index → {M_index}", m_index);
				Logger.LogDebug("Reset(): this.m_index → {Arg0}", this.m_index);
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }
        }

        /// <exclude/>
        private sealed class ReadOnlyAppenderCollection : AppenderCollection, ICollection
        {
            private readonly AppenderCollection m_collection;

            internal ReadOnlyAppenderCollection(AppenderCollection list) : base(Tag.Default)
            {
				Logger.LogDebug(">>> Entering ReadOnlyAppenderCollection(list={List})", list);
                this.m_collection = list;
				Logger.LogDebug("<<< Exiting ReadOnlyAppenderCollection(list={List})", list);
				Logger.LogDebug("ReadOnlyAppenderCollection(list={List}): m_collection → {M_collection}", list, m_collection);
				Logger.LogDebug("ReadOnlyAppenderCollection(list={List}): this.m_collection → {Arg1}", list, this.m_collection);
            }

            public override void CopyTo(IAppender[] array)
            {
				Logger.LogDebug(">>> Entering CopyTo(array={Array})", array);
                this.m_collection.CopyTo(array);
				Logger.LogDebug("<<< Exiting CopyTo(array={Array})", array);
            }

            public override void CopyTo(IAppender[] array, int start)
            {
                this.m_collection.CopyTo(array, start);
            }

            void ICollection.CopyTo(Array array, int start)
            {
                ((ICollection)this.m_collection).CopyTo(array, start);
            }

            public override int Count
            {
                get { return this.m_collection.Count; }
            }

            public override bool IsSynchronized
            {
                get { return this.m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }

            public override IAppender this[int i]
            {
                get { return this.m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(IAppender x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(IAppender x)
            {
                return this.m_collection.Contains(x);
            }

            public override int IndexOf(IAppender x)
            {
                return this.m_collection.IndexOf(x);
            }

            public override void Insert(int pos, IAppender x)
            {
				Logger.LogDebug(">>> Entering Insert(pos={Pos},x={X})", pos, x);
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(IAppender x)
            {
				Logger.LogDebug(">>> Entering Remove(x={X})", x);
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
				Logger.LogDebug(">>> Entering RemoveAt(pos={Pos})", pos);
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool IsFixedSize
            {
                get { return true; }
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override IAppenderCollectionEnumerator GetEnumerator()
            {
				Logger.LogDebug("<<< Exiting Enumerator(tc={Tc})", tc);
                return this.m_collection.GetEnumerator();
            }

            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return this.m_collection.Capacity; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(AppenderCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(IAppender[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override IAppender[] ToArray()
            {
                return this.m_collection.ToArray();
            }

            public override void TrimToSize()
            {
				Logger.LogDebug(">>> Entering TrimToSize()");
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
        }
    }

}
