// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;

    using DotNetNuke.Services.Localization;

    /// <summary>Assert Class.</summary>
    public static class Requires
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Determines whether <paramref name="argValue"/> is type of <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When <paramref name="argValue"/> is not of type <typeparamref name="T"/>.</exception>
        public static void IsTypeOf<T>(string argName, object argValue)
        {
			Logger.LogDebug(">>> Entering IsTypeOf(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (!(argValue is T))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ValueMustBeOfType", "The argument '{0}' must be of type '{1}'.", argName, typeof(T).FullName));
            }
			Logger.LogDebug("<<< Exiting IsTypeOf(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether <paramref name="argValue"/> is less than zero.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When <paramref name="argValue"/> is less then zero.</exception>
        public static void NotNegative(string argName, int argValue)
        {
			Logger.LogDebug(">>> Entering NotNegative(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (argValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName, Localization.GetExceptionMessage("ValueCannotBeNegative", "The argument '{0}' cannot be negative.", argName));
            }
			Logger.LogDebug("<<< Exiting NotNegative(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether the <paramref name="item"/> is <see langword="null"/>.</summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The object to test.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
        public static void NotNull<T>(T item)
        {
			Logger.LogDebug(">>> Entering NotNull(item={Item})", item);
            if (item == null)
            {
                throw new ArgumentNullException(typeof(T).Name);
            }
        }

        /// <summary>Determines whether the <paramref name="argValue"/> is <see langword="null"/>.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="argValue"/> is <see langword="null"/>.</exception>
        public static void NotNull(string argName, object argValue)
        {
            if (argValue == null)
			Logger.LogDebug("<<< Exiting NotNull(item={Item})", item);
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>Determines whether the <paramref name="argValue"/> is <see langword="null"/> or empty.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When the <paramref name="argValue"/> is <see langword="null"/> or empty.</exception>
        public static void NotNullOrEmpty(string argName, string argValue)
        {
			Logger.LogDebug(">>> Entering NotNullOrEmpty(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (string.IsNullOrEmpty(argValue))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty", "The argument '{0}' cannot be null or empty.", argName), argName);
            }
			Logger.LogDebug("<<< Exiting NotNullOrEmpty(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether the <paramref name="argValue"/> is <see langword="null"/>, empty, or has whitespace only.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When the <paramref name="argValue"/> is <see langword="null"/>, empty, or has whitespace only.</exception>
        public static void NotNullOrHasNoWhiteSpace(string argName, string argValue)
        {
			Logger.LogDebug(">>> Entering NotNullOrHasNoWhiteSpace(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (string.IsNullOrWhiteSpace(argValue))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty", "The argument '{0}' cannot be null or empty.", argName), argName);
            }
			Logger.LogDebug("<<< Exiting NotNullOrHasNoWhiteSpace(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether a property is negative.</summary>
        /// <typeparam name="T">The type of <paramref name="item"/>.</typeparam>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the property is negative.</exception>
        public static void PropertyNotNegative<T>(T item, string propertyName)
        {
			Logger.LogDebug(">>> Entering PropertyNotNegative(item={Item},propertyName={PropertyName})", item, propertyName);
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
			Logger.LogDebug("PropertyNotNegative(item={Item},propertyName={PropertyName}): type → {Type}", item, propertyName, type);
			Logger.LogDebug("PropertyNotNegative(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);
            var propertyValue = property.GetValue(item);

			Logger.LogDebug("PropertyNotNegative(item={Item},propertyName={PropertyName}): propertyValue → {PropertyValue}", item, propertyName, propertyValue);
            var intValue = (int)propertyValue;

			Logger.LogDebug("PropertyNotNegative(item={Item},propertyName={PropertyName}): intValue → {IntValue}", item, propertyName, intValue);
            if (intValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    Localization.GetExceptionMessage("PropertyCannotBeNegative", "The property '{1}' in object '{0}' cannot be negative.", typeof(T).Name, propertyName));
            }
        }

        /// <summary>Determines whether <paramref name="propertyValue"/> is less than zero.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="ArgumentOutOfRangeException">When <paramref name="propertyValue"/> is less than zero.</exception>
        public static void PropertyNotNegative(string argName, string argProperty, int propertyValue)
        {
            if (propertyValue < 0)
			Logger.LogDebug("<<< Exiting PropertyNotNegative(item={Item},propertyName={PropertyName})", item, propertyName);
            {
                throw new ArgumentOutOfRangeException(
                    argName,
                    Localization.GetExceptionMessage("PropertyCannotBeNegative", "The property '{1}' in object '{0}' cannot be negative.", argName, argProperty));
            }
        }

        /// <summary>Determines whether the <paramref name="argValue"/> is <see langword="null"/>.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentNullException">When the <paramref name="argValue"/> is <see langword="null"/>.</exception>
        public static void PropertyNotNull(string argName, string argValue)
        {
			Logger.LogDebug(">>> Entering PropertyNotNull(argName={ArgName},argValue={ArgValue})", argName, argValue);
            NotNull(argName, argValue);
			Logger.LogDebug("<<< Exiting PropertyNotNull(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether a property is <see langword="null"/>.</summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentNullException">When the property is <see langword="null"/>.</exception>
        public static void PropertyNotNull<T>(T item, string propertyName)
            where T : class
        {
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
			Logger.LogDebug("PropertyNotNull(item={Item},propertyName={PropertyName}): type → {Type}", item, propertyName, type);
			Logger.LogDebug("PropertyNotNull(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);
            var propertyValue = property.GetValue(item);

			Logger.LogDebug("PropertyNotNull(item={Item},propertyName={PropertyName}): propertyValue → {PropertyValue}", item, propertyName, propertyValue);
            if (propertyValue == null)
            {
                throw new ArgumentNullException(propertyName);
            }
        }

        /// <summary>Determines whether a property is <see langword="null"/> or empty.</summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException">When the property is <see langword="null"/> or empty.</exception>
        public static void PropertyNotNullOrEmpty<T>(T item, string propertyName)
        {
			Logger.LogDebug(">>> Entering PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName})", item, propertyName);
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
			Logger.LogDebug("PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName}): type → {Type}", item, propertyName, type);
			Logger.LogDebug("PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);
            var propertyValue = property.GetValue(item);
            var stringValue = propertyValue as string;
			Logger.LogDebug("PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName}): propertyValue → {PropertyValue}", item, propertyName, propertyValue);

            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentException(propertyName, Localization.GetExceptionMessage("PropertyCannotBeNullOrEmpty", "The property '{1}' in object '{0}' cannot be null or empty.", typeof(T).Name, propertyName));
            }
        }

        /// <summary>Determines whether <paramref name="propertyValue"/> is not <see langword="null"/> or empty.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="ArgumentException">When <paramref name="propertyValue"/> is not <see langword="null"/> or empty.</exception>
        public static void PropertyNotNullOrEmpty(string argName, string argProperty, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
			Logger.LogDebug("<<< Exiting PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName})", item, propertyName);
            {
                throw new ArgumentException(
                    argName,
                    Localization.GetExceptionMessage("PropertyCannotBeNullOrEmpty", "The property '{1}' in object '{0}' cannot be null or empty.", argName, argProperty));
            }
        }

        /// <summary>Determines whether <paramref name="propertyValue"/> equal to <paramref name="testValue"/>.</summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="testValue">The test value.</param>
        /// <exception cref="ArgumentException">Whether <paramref name="propertyValue"/> equal to <paramref name="testValue"/>.</exception>
        public static void PropertyNotEqualTo<TValue>(string argName, string argProperty, TValue propertyValue, TValue testValue)
            where TValue : IEquatable<TValue>
        {
			Logger.LogDebug(">>> Entering PropertyNotEqualTo(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue},testValue={TestValue})", argName, argProperty, propertyValue, testValue);
            if (propertyValue.Equals(testValue))
            {
                throw new ArgumentException(argName, Localization.GetExceptionMessage("PropertyNotEqualTo", "The property '{1}' in object '{0}' is invalid.", argName, argProperty));
            }
			Logger.LogDebug("<<< Exiting PropertyNotEqualTo(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue},testValue={TestValue})", argName, argProperty, propertyValue, testValue);
        }
    }
}
