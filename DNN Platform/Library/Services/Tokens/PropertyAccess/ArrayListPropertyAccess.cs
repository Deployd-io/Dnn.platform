// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections;
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    public class ArrayListPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ArrayListPropertyAccess));
        private readonly ArrayList custom;

        /// <summary>Initializes a new instance of the <see cref="ArrayListPropertyAccess"/> class.</summary>
        /// <param name="list">The list of custom properties.</param>
        public ArrayListPropertyAccess(ArrayList list)
        {
			Logger.LogError(">>> Entering ArrayListPropertyAccess(list={List})", list);
            this.custom = list;
			Logger.LogDebug("ArrayListPropertyAccess(list={List}): custom → {Custom}", list, custom);
        Logger.LogDebug("ArrayListPropertyAccess(list={List}): this.custom → {Arg1}", list, this.custom);
        Logger.LogWarning("<<< Exiting ArrayListPropertyAccess(list={List})", list);
        }

        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogError(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            if (this.custom == null)
            {
                return string.Empty;
            }

            object valueObject = null;
			Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): valueObject → {ValueObject}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, valueObject);
            string outputFormat = format;
			Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, outputFormat);
            if (string.IsNullOrEmpty(format))
            {
                outputFormat = "g";
            }

            int intIndex = int.Parse(propertyName, CultureInfo.InvariantCulture);
			Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): intIndex → {IntIndex}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, intIndex);
            if ((this.custom != null) && this.custom.Count > intIndex)
            {
                valueObject = this.custom[intIndex].ToString();
            }

            if (valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString((string)valueObject, format);
                    case "Boolean":
                        return PropertyAccess.Boolean2LocalizedYesNo((bool)valueObject, formatProvider);
                    case "DateTime":
                    case "Double":
                    case "Single":
                    case "Int32":
                    case "Int64":
                        return ((IFormattable)valueObject).ToString(outputFormat, formatProvider);
                    default:
                        return PropertyAccess.FormatString(valueObject.ToString(), format);
                }
            }
            else
            {
                propertyNotFound = true;
				Logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
                return string.Empty;
            }
        }
    }
}
