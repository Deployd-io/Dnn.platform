// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;

    /// <summary>Wrapper class for any object that maps string key onto the object value (like <see cref="Dictionary{TKey,TValue}"/>).</summary>
    /// <typeparam name="T">The type of the variable's value.</typeparam>
    public abstract class StateVariable<T>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly string key;
        private readonly Func<T> initializer;

        /// <summary>Initializes a new instance of the <see cref="StateVariable{T}"/> class.</summary>
        /// <param name="key">The key to use for storing the value in the items.</param>
        protected StateVariable(string key)
        {
			Logger.LogDebug(">>> Entering StateVariable(key={Key})", key);
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.key = key + this.GetType().FullName;
			Logger.LogDebug("<<< Exiting StateVariable(key={Key})", key);
        }

        /// <summary>Initializes a new instance of the <see cref="StateVariable{T}"/> class with an initializer.</summary>
        /// <param name="key">The key to use for storing the value in the dictionary.</param>
        /// <param name="initializer">A function that is called in order to create a default value per dictionary.</param>
        protected StateVariable(string key, Func<T> initializer)
            : this(key)
        {
            this.initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
			Logger.LogDebug("StateVariable(key={Key},initializer={Initializer}): initializer → {Initializer1}", key, initializer, initializer);
        }

        /// <summary>Gets a value indicating whether indicates whether there is a value present or not.</summary>
        public bool HasValue => this[this.key] != null;

        /// <summary>Gets the value in the current items or if none is available <c>default(T)</c>.</summary>
        public T ValueOrDefault
        {
            get
            {
                var returnedValue = this.GetInitializedInternalValue();
                if (returnedValue == null)
                {
                    return default(T);
                }

                return (T)returnedValue;
            }
        }

        /// <summary>Gets or sets or gets the value in the current items.</summary>
        /// <exception cref="InvalidOperationException">
        /// If you try to get a value while none is set use <see cref="ValueOrDefault"/> for safe access.
        /// </exception>
        public T Value
        {
            get
            {
                var returnedValue = this.GetInitializedInternalValue();
                if (returnedValue == null)
                {
                    throw new InvalidOperationException("There is no value for the '" + this.key + "' key.");
                }

                return (T)returnedValue;
            }

            set
            {
                this[this.key] = value;
            }
        }

        /// <summary>Get/sets the value in associated dictionary/map.</summary>
        /// <param name="key">Value key.</param>
        /// <value>The value.</value>
        protected abstract object this[string key] { get; set; }

        /// <summary>Clears the value in the current items.</summary>
        public void Clear()
        {
			Logger.LogDebug(">>> Entering Clear()");
            this.Remove(this.key);
			Logger.LogDebug("<<< Exiting Clear()");
        }

        /// <summary>Removes the value in associated dictionary according.</summary>
        /// <param name="key">Value key.</param>
        protected abstract void Remove(string key);

        private object GetInitializedInternalValue()
        {
			Logger.LogDebug(">>> Entering GetInitializedInternalValue()");
            var value = this[this.key];
            if (value == null && this.initializer != null)
			Logger.LogDebug("GetInitializedInternalValue(): value → {Value}", value);
            {
                value = this.initializer();
                this[this.key] = value;
            }

			Logger.LogDebug("<<< Exiting GetInitializedInternalValue()");
            return value;
        }
    }
}
