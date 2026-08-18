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

namespace log4net.Core
{
    /// <summary>A strongly-typed collection of <see cref="Level"/> objects.</summary>
    /// <author>Nicko Cadell</author>
    public class LevelCollection : ICollection, IList, IEnumerable
#if !NETSTANDARD1_3
        , ICloneable
#endif
    {
        /// <summary>Supports type-safe iteration over a <see cref="LevelCollection"/>.</summary>
        public interface ILevelCollectionEnumerator
        {
            /// <summary>Gets the current element in the collection.</summary>
            Level Current { get; }

            /// <summary>Advances the enumerator to the next element in the collection.</summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element; 
            /// <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            bool MoveNext();

            /// <summary>Sets the enumerator to its initial position, before the first element in the collection.</summary>
            void Reset();
        }

        private const int DEFAULT_CAPACITY = 16;

        private Level[] m_array;
        private int m_count = 0;
        private int m_version = 0;

        /// <summary>Creates a read-only wrapper for a <c>LevelCollection</c> instance.</summary>
        /// <param name="list">list to create a readonly wrapper arround</param>
        /// <returns>
        /// A <c>LevelCollection</c> wrapper that is read-only.
        /// </returns>
        public static LevelCollection ReadOnly(LevelCollection list)
        {
			value.LogDebug(">>> Entering ReadOnly(list={List})", list);
            if(list == null)
            {
				value.LogWarning("Throwing ArgumentNullException from ReadOnly(list={List})", list);
			value.LogDebug("<<< Exiting ReadOnly(list={List})", list);
                throw new ArgumentNullException("list");
            }

			value.LogDebug(">>> Entering Reset()");
			value.LogDebug("<<< Exiting Reset()");
            return new ReadOnlyLevelCollection(list);
        }

        /// <summary>
        /// Initializes a new instance of the <c>LevelCollection</c> class
        /// that is empty and has the default initial capacity.
        /// </summary>
        public LevelCollection()
        {
			value.LogDebug(">>> Entering LevelCollection()");
            this.m_array = new Level[DEFAULT_CAPACITY];
			value.LogDebug("LevelCollection(tag={Tag}): this.m_array → {Arg1}", tag, this.m_array);
			value.LogDebug("LevelCollection(): this.m_array → {Arg0}", this.m_array);
			value.LogDebug("LevelCollection(capacity={Capacity}): this.m_array → {Arg1}", capacity, this.m_array);
			value.LogDebug("<<< Exiting LevelCollection()");
        }
        
        /// <summary>
        /// Initializes a new instance of the <c>LevelCollection</c> class
        /// that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
		value.LogDebug("<<< Exiting LevelCollection()");
        /// The number of elements that the new <c>LevelCollection</c> is initially capable of storing.
        /// </param>
        public LevelCollection(int capacity)
        {
			value.LogDebug(">>> Entering LevelCollection(capacity={Capacity})", capacity);
            this.m_array = new Level[capacity];
			value.LogDebug("<<< Exiting LevelCollection(capacity={Capacity})", capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <c>LevelCollection</c> class
        /// that contains elements copied from the specified <c>LevelCollection</c>.
        /// </summary>
        /// <param name="c">The <c>LevelCollection</c> whose elements are copied to the new collection.</param>
        public LevelCollection(LevelCollection c)
        {
			value.LogDebug(">>> Entering LevelCollection(c={C})", c);
            this.m_array = new Level[c.Count];
            this.AddRange(c);
			value.LogDebug("LevelCollection(c={C}): this.m_array → {Arg1}", c, this.m_array);
			value.LogDebug("<<< Exiting LevelCollection(c={C})", c);
        }

        /// <summary>
        /// Initializes a new instance of the <c>LevelCollection</c> class
        /// that contains elements copied from the specified <see cref="Level"/> array.
        /// </summary>
        /// <param name="a">The <see cref="Level"/> array whose elements are copied to the new list.</param>
        public LevelCollection(Level[] a)
        {
			value.LogDebug(">>> Entering LevelCollection(a={A})", a);
            this.m_array = new Level[a.Length];
            this.AddRange(a);
			value.LogDebug("LevelCollection(a={A}): this.m_array → {Arg1}", a, this.m_array);
			value.LogDebug("<<< Exiting LevelCollection(a={A})", a);
        }

        /// <summary>
        /// Initializes a new instance of the <c>LevelCollection</c> class
        /// that contains elements copied from the specified <see cref="Level"/> collection.
        /// </summary>
        /// <param name="col">The <see cref="Level"/> collection whose elements are copied to the new list.</param>
        public LevelCollection(ICollection col)
        {
			value.LogDebug(">>> Entering LevelCollection(col={Col})", col);
            this.m_array = new Level[col.Count];
            this.AddRange(col);
			value.LogDebug("LevelCollection(col={Col}): this.m_array → {Arg1}", col, this.m_array);
			value.LogDebug("<<< Exiting LevelCollection(col={Col})", col);
        }
        
        /// <summary>
        /// Type visible only to our subclasses
        /// Used to access protected constructor
        /// </summary>
        protected internal enum Tag 
        {
            /// <summary>A value</summary>
            Default
        }

        /// <summary>Allow subclasses to avoid our default constructors</summary>
        /// <param name="tag"></param>
        protected internal LevelCollection(Tag tag)
        {
			value.LogDebug(">>> Entering LevelCollection(tag={Tag})", tag);
            this.m_array = null;
        }

        /// <summary>Gets the number of elements actually contained in the <c>LevelCollection</c>.</summary>
        public virtual int Count
        {
            get { return this.m_count; }
        }

        /// <summary>
        /// Copies the entire <c>LevelCollection</c> to a one-dimensional
        /// <see cref="Level"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Level"/> array to copy to.</param>
        public virtual void CopyTo(Level[] array)
        {
            this.CopyTo(array, 0);
			value.LogDebug("<<< Exiting CopyTo(array={Array},start={Start})", array, start);
        }

        /// <summary>
        /// Copies the entire <c>LevelCollection</c> to a one-dimensional
        /// <see cref="Level"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Level"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(Level[] array, int start)
        {
			value.LogDebug(">>> Entering CopyTo(array={Array},start={Start})", array, start);
            if (this.m_count > array.GetUpperBound(0) + 1 - start)
            {
				value.LogWarning("Throwing ArgumentException from CopyTo(array={Array},start={Start})", array, start);
                throw new ArgumentException("Destination array was not long enough.");
            }
            
            Array.Copy(this.m_array, 0, array, start, this.m_count); 
			value.LogDebug("<<< Exiting CopyTo(array={Array},start={Start})", array, start);
			value.LogDebug(">>> Entering CopyTo(array={Array})", array);
			value.LogDebug("<<< Exiting CopyTo(array={Array})", array);
        }

        /// <summary>Gets a value indicating whether access to the collection is synchronized (thread-safe).</summary>
        /// <returns>false, because the backing type is an array, which is never thread-safe.</returns>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
        public virtual object SyncRoot
        {
            get { return this.m_array; }
        }

        /// <summary>Gets or sets the <see cref="Level"/> at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
        /// </exception>
        public virtual Level this[int index]
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

        /// <summary>Adds a <see cref="Level"/> to the end of the <c>LevelCollection</c>.</summary>
        /// <param name="item">The <see cref="Level"/> to be added to the end of the <c>LevelCollection</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(Level item)
        {
			value.LogDebug(">>> Entering Add(item={Item})", item);
            if (this.m_count == this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + 1);
            }

            this.m_array[this.m_count] = item;
            this.m_version++;

			value.LogDebug("<<< Exiting Add(item={Item})", item);
            return this.m_count++;
        }
        
        /// <summary>Removes all elements from the <c>LevelCollection</c>.</summary>
        public virtual void Clear()
        {
            ++this.m_version;
            this.m_array = new Level[DEFAULT_CAPACITY];
            this.m_count = 0;
			value.LogDebug(">>> Entering Clear()");
			value.LogWarning("Throwing NotSupportedException from Clear()");
			value.LogDebug("<<< Exiting Clear()");
        }
        
        /// <summary>Creates a shallow copy of the <see cref="LevelCollection"/>.</summary>
        /// <returns>A new <see cref="LevelCollection"/> with a shallow copy of the collection data.</returns>
        public virtual object Clone()
        {
			value.LogDebug(">>> Entering Clone()");
            LevelCollection newCol = new LevelCollection(this.m_count);
            Array.Copy(this.m_array, 0, newCol.m_array, 0, this.m_count);
			value.LogDebug("Clone(): newCol → {NewCol}", newCol);
			value.LogDebug("Clone(): newCol.m_count → {Arg0}", newCol.m_count);
			value.LogDebug("Clone(): newCol.m_version → {Arg0}", newCol.m_version);
            newCol.m_count = this.m_count;
            newCol.m_version = this.m_version;

			value.LogDebug("<<< Exiting Clone()");
            return newCol;
        }

        /// <summary>Determines whether a given <see cref="Level"/> is in the <c>LevelCollection</c>.</summary>
        /// <param name="item">The <see cref="Level"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>LevelCollection</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(Level item)
        {
			value.LogDebug(">>> Entering Contains(item={Item})", item);
            for (int i = 0; i != this.m_count; ++i)
            {
                if (this.m_array[i].Equals(item))
                {
                    return true;
                }
            }
			value.LogDebug("<<< Exiting Contains(item={Item})", item);
            return false;
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of a <see cref="Level"/>
        /// in the <c>LevelCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="Level"/> to locate in the <c>LevelCollection</c>.</param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="item"/> 
        /// in the entire <c>LevelCollection</c>, if found; otherwise, -1.
        ///     </returns>
        public virtual int IndexOf(Level item)
        {
			value.LogDebug(">>> Entering IndexOf(item={Item})", item);
            for (int i = 0; i != this.m_count; ++i)
            {
                if (this.m_array[i].Equals(item))
                {
                    return i;
                }
            }
			value.LogDebug("<<< Exiting IndexOf(item={Item})", item);
            return -1;
        }

        /// <summary>Inserts an element into the <c>LevelCollection</c> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="Level"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, Level item)
        {
			value.LogDebug(">>> Entering Insert(index={Index},item={Item})", index, item);
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
			value.LogDebug("<<< Exiting Insert(index={Index},item={Item})", index, item);
        }

        /// <summary>Removes the first occurrence of a specific <see cref="Level"/> from the <c>LevelCollection</c>.</summary>
        /// <param name="item">The <see cref="Level"/> to remove from the <c>LevelCollection</c>.</param>
        /// <exception cref="ArgumentException">
        /// The specified <see cref="Level"/> was not found in the <c>LevelCollection</c>.
        /// </exception>
        public virtual void Remove(Level item)
        {		   
			value.LogDebug(">>> Entering Remove(item={Item})", item);
            int i = this.IndexOf(item);
            if (i < 0)
            {
				value.LogWarning("Throwing ArgumentException from Remove(item={Item})", item);
                throw new ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
            }
            
            ++this.m_version;
            this.RemoveAt(i);
			value.LogDebug("<<< Exiting Remove(item={Item})", item);
        }

        /// <summary>Removes the element at the specified index of the <c>LevelCollection</c>.</summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
        /// </exception>
        public virtual void RemoveAt(int index)
        {
			value.LogDebug(">>> Entering RemoveAt(index={Index})", index);
            this.ValidateIndex(index); // throws

            this.m_count--;

            if (index < this.m_count)
            {
                Array.Copy(this.m_array, index + 1, this.m_array, index, this.m_count - index);
            }
            
            // We can't set the deleted entry equal to null, because it might be a value type.
            // Instead, we'll create an empty single-element array of the right type and copy it 
            // over the entry we want to erase.
            Level[] temp = new Level[1];
            Array.Copy(temp, 0, this.m_array, this.m_count, 1);
            this.m_version++;
			value.LogDebug("<<< Exiting RemoveAt(index={Index})", index);
        }

        /// <summary>Gets a value indicating whether the collection has a fixed size.</summary>
        /// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>Gets a value indicating whether the IList is read-only.</summary>
        /// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>Returns an enumerator that can iterate through the <c>LevelCollection</c>.</summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <c>LevelCollection</c>.</returns>
        public virtual ILevelCollectionEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>Gets or sets the number of elements the <c>LevelCollection</c> can contain.</summary>
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
                        Level[] temp = new Level[value];
                        Array.Copy(this.m_array, 0, temp, 0, this.m_count);
                        this.m_array = temp;
                    }
                    else
                    {
                        this.m_array = new Level[DEFAULT_CAPACITY];
                    }
                }
            }
        }

        /// <summary>Adds the elements of another <c>LevelCollection</c> to the current <c>LevelCollection</c>.</summary>
        /// <param name="x">The <c>LevelCollection</c> whose elements should be added to the end of the current <c>LevelCollection</c>.</param>
        /// <returns>The new <see cref="LevelCollection.Count"/> of the <c>LevelCollection</c>.</returns>
        public virtual int AddRange(LevelCollection x)
        {
			value.LogDebug(">>> Entering AddRange(x={X})", x);
            if (this.m_count + x.Count >= this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + x.Count);
            }
            
            Array.Copy(x.m_array, 0, this.m_array, this.m_count, x.Count);
            this.m_count += x.Count;
            this.m_version++;

			value.LogDebug("<<< Exiting AddRange(x={X})", x);
			value.LogDebug("<<< Exiting AddRange(col={Col})", col);
            return this.m_count;
        }

        /// <summary>Adds the elements of a <see cref="Level"/> array to the current <c>LevelCollection</c>.</summary>
        /// <param name="x">The <see cref="Level"/> array whose elements should be added to the end of the <c>LevelCollection</c>.</param>
        /// <returns>The new <see cref="LevelCollection.Count"/> of the <c>LevelCollection</c>.</returns>
        public virtual int AddRange(Level[] x)
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

        /// <summary>Adds the elements of a <see cref="Level"/> collection to the current <c>LevelCollection</c>.</summary>
        /// <param name="col">The <see cref="Level"/> collection whose elements should be added to the end of the <c>LevelCollection</c>.</param>
        /// <returns>The new <see cref="LevelCollection.Count"/> of the <c>LevelCollection</c>.</returns>
        public virtual int AddRange(ICollection col)
        {
            if (this.m_count + col.Count >= this.m_array.Length)
            {
                this.EnsureCapacity(this.m_count + col.Count);
            }

            foreach(object item in col)
            {
                this.Add((Level)item);
            }

            return this.m_count;
        }
        
        /// <summary>Sets the capacity to the actual number of elements.</summary>
        public virtual void TrimToSize()
        {
			value.LogDebug(">>> Entering TrimToSize()");
            this.Capacity = this.m_count;
			value.LogDebug("TrimToSize(): this.Capacity → {Arg0}", this.Capacity);
			value.LogDebug("<<< Exiting TrimToSize()");
        }

        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="i"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="i"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i)
        {
            this.ValidateIndex(i, false);
			value.LogDebug("<<< Exiting ValidateIndex(i={I})", i);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="i"/> is less than zero</para>
        /// <para>-or-</para>
        /// <para><paramref name="i"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
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
			value.LogDebug(">>> Entering EnsureCapacity(min={Min})", min);
            int newCapacity = ((this.m_array.Length == 0) ? DEFAULT_CAPACITY : this.m_array.Length * 2);
            if (newCapacity < min)
			value.LogDebug("EnsureCapacity(min={Min}): newCapacity → {NewCapacity}", min, newCapacity);
            {
                newCapacity = min;
            }

            this.Capacity = newCapacity;
			value.LogDebug("EnsureCapacity(min={Min}): this.Capacity → {Arg1}", min, this.Capacity);
			value.LogDebug("<<< Exiting EnsureCapacity(min={Min})", min);
        }

        void ICollection.CopyTo(Array array, int start)
        {
            Array.Copy(this.m_array, 0, array, start, this.m_count);
        }

        object IList.this[int i]
        {
            get { return (object)this[i]; }
            set { this[i] = (Level)value; }
        }

        int IList.Add(object x)
        {
			value.LogDebug(">>> Entering Add(x={X})", x);
			value.LogDebug("<<< Exiting Add(x={X})", x);
            return this.Add((Level)x);
        }

        bool IList.Contains(object x)
        {
			value.LogDebug(">>> Entering Contains(x={X})", x);
			value.LogDebug("<<< Exiting Contains(x={X})", x);
            return this.Contains((Level)x);
        }

        int IList.IndexOf(object x)
        {
			value.LogDebug(">>> Entering IndexOf(x={X})", x);
			value.LogDebug("<<< Exiting IndexOf(x={X})", x);
            return this.IndexOf((Level)x);
        }

        void IList.Insert(int pos, object x)
        {
            this.Insert(pos, (Level)x);
        }

        void IList.Remove(object x)
        {
            this.Remove((Level)x);
        }

        void IList.RemoveAt(int pos)
        {
            this.RemoveAt(pos);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
			value.LogDebug(">>> Entering GetEnumerator()");
			value.LogDebug("<<< Exiting GetEnumerator()");
            return (IEnumerator)(this.GetEnumerator());
        }

        /// <summary>Supports simple iteration over a <see cref="LevelCollection"/>.</summary>
        private sealed class Enumerator : IEnumerator, ILevelCollectionEnumerator
        {
            private readonly LevelCollection m_collection;
            private int m_index;
            private int m_version;

            /// <summary>Initializes a new instance of the <c>Enumerator</c> class.</summary>
            /// <param name="tc"></param>
            internal Enumerator(LevelCollection tc)
            {
				value.LogDebug(">>> Entering Enumerator(tc={Tc})", tc);
                this.m_collection = tc;
                this.m_index = -1;
				value.LogDebug("Enumerator(tc={Tc}): this.m_collection → {Arg1}", tc, this.m_collection);
				value.LogDebug("Enumerator(tc={Tc}): this.m_index → {Arg1}", tc, this.m_index);
				value.LogDebug("Enumerator(tc={Tc}): m_index → {M_index}", tc, m_index);
                this.m_version = tc.m_version;
				value.LogDebug("<<< Exiting Enumerator(tc={Tc})", tc);
            }

            /// <summary>Gets the current element in the collection.</summary>
            public Level Current
            {
                get { return this.m_collection[this.m_index]; }
            }

            /// <summary>Advances the enumerator to the next element in the collection.</summary>
            /// <returns>
            /// <c>true</c> if the enumerator was successfully advanced to the next element; 
            /// <c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
				value.LogDebug(">>> Entering MoveNext()");
                if (this.m_version != this.m_collection.m_version)
                {
					value.LogWarning("Throwing InvalidOperationException from MoveNext()");
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                ++this.m_index;
				value.LogDebug("<<< Exiting MoveNext()");
                return (this.m_index < this.m_collection.Count);
            }

            /// <summary>Sets the enumerator to its initial position, before the first element in the collection.</summary>
            public void Reset()
            {
				value.LogDebug(">>> Entering Reset()");
                this.m_index = -1;
				value.LogDebug("Reset(): this.m_index → {Arg0}", this.m_index);
				value.LogDebug("Reset(): m_index → {M_index}", m_index);
				value.LogDebug("<<< Exiting Reset()");
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }
        }

        private sealed class ReadOnlyLevelCollection : LevelCollection
        {
            private readonly LevelCollection m_collection;

            internal ReadOnlyLevelCollection(LevelCollection list) : base(Tag.Default)
            {
				value.LogDebug(">>> Entering ReadOnlyLevelCollection(list={List})", list);
                this.m_collection = list;
				value.LogDebug("ReadOnlyLevelCollection(list={List}): this.m_collection → {Arg1}", list, this.m_collection);
				value.LogDebug("ReadOnlyLevelCollection(list={List}): m_collection → {M_collection}", list, m_collection);
				value.LogDebug("<<< Exiting ReadOnlyLevelCollection(list={List})", list);
            }

            public override void CopyTo(Level[] array)
            {
				value.LogDebug(">>> Entering CopyTo(array={Array})", array);
                this.m_collection.CopyTo(array);
				value.LogDebug("<<< Exiting CopyTo(array={Array})", array);
            }

            public override void CopyTo(Level[] array, int start)
            {
                this.m_collection.CopyTo(array,start);
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

            public override Level this[int i]
            {
                get { return this.m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Level x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Clear()
            {
				value.LogDebug(">>> Entering Clear()");
				value.LogWarning("Throwing NotSupportedException from Clear()");
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Level x)
            {
                return this.m_collection.Contains(x);
            }

            public override int IndexOf(Level x)
            {
                return this.m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Level x)
            {
				value.LogDebug(">>> Entering Insert(pos={Pos},x={X})", pos, x);
				value.LogWarning("Throwing NotSupportedException from Insert(pos={Pos},x={X})", pos, x);
				value.LogDebug("<<< Exiting Insert(pos={Pos},x={X})", pos, x);
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Level x)
            {           
				value.LogDebug(">>> Entering Remove(x={X})", x);
				value.LogWarning("Throwing NotSupportedException from Remove(x={X})", x);
				value.LogDebug("<<< Exiting Remove(x={X})", x);
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
				value.LogDebug(">>> Entering RemoveAt(pos={Pos})", pos);
				value.LogWarning("Throwing NotSupportedException from RemoveAt(pos={Pos})", pos);
				value.LogDebug("<<< Exiting RemoveAt(pos={Pos})", pos);
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

            public override ILevelCollectionEnumerator GetEnumerator()
            {
                return this.m_collection.GetEnumerator();
            }

            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return this.m_collection.Capacity; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(LevelCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Level[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
        }
    }

}
