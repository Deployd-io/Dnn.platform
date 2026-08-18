// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections;
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    public class DictionaryPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DictionaryPropertyAccess));
        private readonly IDictionary nameValueCollection;

        /// <summary>Initializes a new instance of the <see cref="DictionaryPropertyAccess"/> class.</summary>
        /// <param name="list">The dictionary of token values.</param>
        public DictionaryPropertyAccess(IDictionary list)
        {
			Logger.LogDebug(">>> Entering DictionaryPropertyAccess(list={List})", list);
            this.nameValueCollection = list;
			Logger.LogDebug("DictionaryPropertyAccess(list={List}): this.nameValueCollection → {Arg1}", list, this.nameValueCollection);
			Logger.LogDebug("DictionaryPropertyAccess(list={List}): nameValueCollection → {NameValueCollection}", list, nameValueCollection);
			Logger.LogDebug("<<< Exiting DictionaryPropertyAccess(list={List})", list);
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
        public virtual string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            if (this.nameValueCollection == null)
            {
                return string.Empty;
            }

            object valueObject = this.nameValueCollection[propertyName];
            string outputFormat = format;
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, outputFormat);
            if (string.IsNullOrEmpty(format))
            {
                outputFormat = "g";
            }

            if (valueObject != null)
            {
                switch (valueObject.GetType().Name)
                {
                    case "String":
                        return PropertyAccess.FormatString(Convert.ToString(valueObject, CultureInfo.InvariantCulture), format);
                    case "Boolean":
                        return PropertyAccess.Boolean2LocalizedYesNo(Convert.ToBoolean(valueObject, CultureInfo.InvariantCulture), formatProvider);
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
				Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
                return string.Empty;
            }
        }
    }
}
