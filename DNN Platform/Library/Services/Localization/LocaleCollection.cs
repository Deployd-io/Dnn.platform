// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using DotNetNuke.Common;

    /// <summary><para>The LocaleCollection class is a collection of Locale objects.  It stores the supported locales.</para></summary>
    public class LocaleCollection : NameObjectCollectionBase, IDictionary<string, Locale>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocaleCollection));
        private DictionaryEntry de;

        /// <summary>Gets a String array that contains all the keys in the collection.</summary>
        public string[] AllKeys => this.BaseGetAllKeys();

        /// <summary>Gets an Object array that contains all the values in the collection.</summary>
        public Array AllValues => this.BaseGetAllValues();

        /// <summary>Gets a value indicating whether the collection contains keys that are not null.</summary>
        public bool HasKeys => this.BaseHasKeys();

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, Locale>>.IsReadOnly => this.IsReadOnly;

        /// <inheritdoc />
        ICollection<string> IDictionary<string, Locale>.Keys => this.BaseGetAllKeys();

        /// <inheritdoc />
        public ICollection<Locale> Values => this.BaseGetAllValues().Cast<Locale>().ToList();

        public DictionaryEntry this[int index]
        {
            get
            {
                this.de.Key = this.BaseGetKey(index);
                this.de.Value = this.BaseGet(index);
                return this.de;
            }
        }

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public Locale this[string key]
        {
            get => (Locale)this.BaseGet(key);
            set => this.BaseSet(key, value);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out Locale value)
        {
			Logger.LogError(">>> Entering TryGetValue(key={Key})", key);
            if (!this.ContainsKey(key))
            {
                value = null;
				Logger.LogError("TryGetValue(key={Key}): value → {Value}", key, value);
				Logger.LogDebug("TryGetValue(key={Key}): value → {Value}", key, value);
                return false;
            }

            value = this[key];
			Logger.LogWarning("<<< Exiting TryGetValue(key={Key})", key);
            return true;
        }

        /// <summary>Adds an entry to the collection.</summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object value)
        {
			Logger.LogError(">>> Entering Add(key={Key},value={Value})", key, value);
            this.BaseAdd(key, value);
			Logger.LogWarning("<<< Exiting Add(key={Key},value={Value})", key, value);
        Logger.LogWarning("<<< Exiting Add(item={Item})", item);
        }

        /// <summary>Removes an entry with the specified key from the collection.</summary>
        /// <param name="key">The key.</param>
        public void Remove(string key)
        {
			Logger.LogError(">>> Entering Remove(key={Key})", key);
            this.BaseRemove(key);
			Logger.LogWarning("<<< Exiting Remove(key={Key})", key);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key) => this[key] is not null;

        /// <inheritdoc />
        public void Add(string key, Locale value) => this.BaseAdd(key, value);

        /// <inheritdoc />
        bool IDictionary<string, Locale>.Remove(string key)
        {
            if (!this.ContainsKey(key))
            {
                return false;
            }

            this.BaseRemove(key);
            return true;
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, Locale> item) => this.Add(item.Key, item.Value);

        /// <inheritdoc />
        public void Clear() => this.BaseClear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, Locale> item)
        {
			Logger.LogError(">>> Entering Contains(item={Item})", item);
            if (!this.TryGetValue(item.Key, out var value))
            {
                return false;
            }

			Logger.LogWarning("<<< Exiting Contains(item={Item})", item);
            return EqualityComparer<Locale>.Default.Equals(value, item.Value);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, Locale>[] array, int arrayIndex)
        {
			Logger.LogError(">>> Entering CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
            Requires.NotNull(nameof(array), array);
            if (arrayIndex > array.Length)
            {
				Logger.LogError("Throwing ArgumentOutOfRangeException from CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "arrayIndex must be less than the length of array");
            }

            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(array), array, "array length must be large enough to hold contents");
            }

            foreach (var key in this.BaseGetAllKeys())
            {
                array[arrayIndex++] = new KeyValuePair<string, Locale>(key, this[key]);
            }
			Logger.LogWarning("<<< Exiting CopyTo(array={Array},arrayIndex={ArrayIndex})", array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, Locale> item)
        {
			Logger.LogError(">>> Entering Remove(item={Item})", item);
            if (!this.Contains(item))
            {
                return false;
            }

            this.BaseRemove(item.Key);
			Logger.LogWarning("<<< Exiting Remove(item={Item})", item);
            return true;
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, Locale>> IEnumerable<KeyValuePair<string, Locale>>.GetEnumerator()
            => new KeyValuePairEnumerator(this, this.GetEnumerator());

        private class KeyValuePairEnumerator(LocaleCollection locales, IEnumerator enumerator) : IEnumerator<KeyValuePair<string, Locale>>
        {
            /// <inheritdoc />
            public KeyValuePair<string, Locale> Current =>
                new KeyValuePair<string, Locale>((string)enumerator.Current, locales[(string)enumerator.Current]);

            /// <inheritdoc />
            object IEnumerator.Current => this.Current;

            /// <inheritdoc />
            public void Dispose() => (enumerator as IDisposable)?.Dispose();

            /// <inheritdoc />
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => enumerator.Reset();
        }
    }
}
