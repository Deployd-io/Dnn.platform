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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Requires));
        /// <summary>Determines whether <paramref name="argValue"/> is type of <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When <paramref name="argValue"/> is not of type <typeparamref name="T"/>.</exception>
        public static void IsTypeOf<T>(string argName, object argValue)
        {
			Logger.LogError(">>> Entering IsTypeOf(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (!(argValue is T))
            {
				Logger.LogError("Throwing ArgumentException from IsTypeOf(argName={ArgName},argValue={ArgValue})", argName, argValue);
                throw new ArgumentException(Localization.GetExceptionMessage("ValueMustBeOfType", "The argument '{0}' must be of type '{1}'.", argName, typeof(T).FullName));
            }
			Logger.LogWarning("<<< Exiting IsTypeOf(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether <paramref name="argValue"/> is less than zero.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When <paramref name="argValue"/> is less then zero.</exception>
        public static void NotNegative(string argName, int argValue)
        {
			Logger.LogError(">>> Entering NotNegative(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (argValue < 0)
            {
				Logger.LogError("Throwing ArgumentOutOfRangeException from NotNegative(argName={ArgName},argValue={ArgValue})", argName, argValue);
                throw new ArgumentOutOfRangeException(argName, Localization.GetExceptionMessage("ValueCannotBeNegative", "The argument '{0}' cannot be negative.", argName));
            }
			Logger.LogWarning("<<< Exiting NotNegative(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether the <paramref name="item"/> is <see langword="null"/>.</summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The object to test.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="item"/> is <see langword="null"/>.</exception>
        public static void NotNull<T>(T item)
        {
			Logger.LogError(">>> Entering NotNull(item={Item})", item);
            if (item == null)
            {
				Logger.LogError("Throwing ArgumentNullException from NotNull(item={Item})", item);
			Logger.LogWarning("<<< Exiting NotNull(item={Item})", item);
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
            {
				Logger.LogError("Throwing ArgumentNullException from NotNull(argName={ArgName},argValue={ArgValue})", argName, argValue);
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>Determines whether the <paramref name="argValue"/> is <see langword="null"/> or empty.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When the <paramref name="argValue"/> is <see langword="null"/> or empty.</exception>
        public static void NotNullOrEmpty(string argName, string argValue)
        {
			Logger.LogError(">>> Entering NotNullOrEmpty(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (string.IsNullOrEmpty(argValue))
            {
				Logger.LogError("Throwing ArgumentException from NotNullOrEmpty(argName={ArgName},argValue={ArgValue})", argName, argValue);
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty", "The argument '{0}' cannot be null or empty.", argName), argName);
            }
			Logger.LogWarning("<<< Exiting NotNullOrEmpty(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether the <paramref name="argValue"/> is <see langword="null"/>, empty, or has whitespace only.</summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException">When the <paramref name="argValue"/> is <see langword="null"/>, empty, or has whitespace only.</exception>
        public static void NotNullOrHasNoWhiteSpace(string argName, string argValue)
        {
			Logger.LogError(">>> Entering NotNullOrHasNoWhiteSpace(argName={ArgName},argValue={ArgValue})", argName, argValue);
            if (string.IsNullOrWhiteSpace(argValue))
            {
				Logger.LogError("Throwing ArgumentException from NotNullOrHasNoWhiteSpace(argName={ArgName},argValue={ArgValue})", argName, argValue);
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty", "The argument '{0}' cannot be null or empty.", argName), argName);
            }
			Logger.LogWarning("<<< Exiting NotNullOrHasNoWhiteSpace(argName={ArgName},argValue={ArgValue})", argName, argValue);
        }

        /// <summary>Determines whether a property is negative.</summary>
        /// <typeparam name="T">The type of <paramref name="item"/>.</typeparam>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the property is negative.</exception>
        public static void PropertyNotNegative<T>(T item, string propertyName)
        {
			Logger.LogError(">>> Entering PropertyNotNegative(item={Item},propertyName={PropertyName})", item, propertyName);
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
			Logger.LogError("PropertyNotNegative(item={Item},propertyName={PropertyName}): type → {Type}", item, propertyName, type);
            var property = type.GetProperty(propertyName);
			Logger.LogError("PropertyNotNegative(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);
            var propertyValue = property.GetValue(item);
			Logger.LogError("PropertyNotNegative(item={Item},propertyName={PropertyName}): propertyValue → {PropertyValue}", item, propertyName, propertyValue);

            var intValue = (int)propertyValue;
			Logger.LogError("PropertyNotNegative(item={Item},propertyName={PropertyName}): intValue → {IntValue}", item, propertyName, intValue);

            if (intValue < 0)
            {
				Logger.LogError("Throwing ArgumentOutOfRangeException from PropertyNotNegative(item={Item},propertyName={PropertyName})", item, propertyName);
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    Localization.GetExceptionMessage("PropertyCannotBeNegative", "The property '{1}' in object '{0}' cannot be negative.", typeof(T).Name, propertyName));
			Logger.LogWarning("<<< Exiting PropertyNotNegative(item={Item},propertyName={PropertyName})", item, propertyName);
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
            {
					Logger.LogError("Throwing ArgumentOutOfRangeException from PropertyNotNegative(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue})", argName, argProperty, propertyValue);
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
			Logger.LogError(">>> Entering PropertyNotNull(argName={ArgName},argValue={ArgValue})", argName, argValue);
            NotNull(argName, argValue);
			Logger.LogWarning("<<< Exiting PropertyNotNull(argName={ArgName},argValue={ArgValue})", argName, argValue);
			Logger.LogError("Throwing ArgumentNullException from PropertyNotNull(item={Item},propertyName={PropertyName})", item, propertyName);
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
			Logger.LogError("PropertyNotNull(item={Item},propertyName={PropertyName}): type → {Type}", item, propertyName, type);
            var property = type.GetProperty(propertyName);
			Logger.LogError("PropertyNotNull(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);
            var propertyValue = property.GetValue(item);
			Logger.LogError("PropertyNotNull(item={Item},propertyName={PropertyName}): propertyValue → {PropertyValue}", item, propertyName, propertyValue);

            if (propertyValue == null)
            {
				Logger.LogError("Throwing ArgumentNullException from PropertyNotNull(item={Item},propertyName={PropertyName})", item, propertyName);
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
			Logger.LogError(">>> Entering PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName})", item, propertyName);
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
			Logger.LogError("PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName}): type → {Type}", item, propertyName, type);
            var property = type.GetProperty(propertyName);
			Logger.LogError("PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName}): property → {Property}", item, propertyName, property);
            var propertyValue = property.GetValue(item);
			Logger.LogError("PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName}): propertyValue → {PropertyValue}", item, propertyName, propertyValue);
            var stringValue = propertyValue as string;

            if (string.IsNullOrEmpty(stringValue))
            {
				Logger.LogError("Throwing ArgumentException from PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName})", item, propertyName);
			Logger.LogWarning("<<< Exiting PropertyNotNullOrEmpty(item={Item},propertyName={PropertyName})", item, propertyName);
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
            {
					Logger.LogError("Throwing ArgumentException from PropertyNotNullOrEmpty(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue})", argName, argProperty, propertyValue);
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
			Logger.LogError(">>> Entering PropertyNotEqualTo(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue},testValue={TestValue})", argName, argProperty, propertyValue, testValue);
            if (propertyValue.Equals(testValue))
            {
				Logger.LogError("Throwing ArgumentException from PropertyNotEqualTo(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue},testValue={TestValue})", argName, argProperty, propertyValue, testValue);
                throw new ArgumentException(argName, Localization.GetExceptionMessage("PropertyNotEqualTo", "The property '{1}' in object '{0}' is invalid.", argName, argProperty));
            }
			Logger.LogWarning("<<< Exiting PropertyNotEqualTo(argName={ArgName},argProperty={ArgProperty},propertyValue={PropertyValue},testValue={TestValue})", argName, argProperty, propertyValue, testValue);
        }
    }
}
