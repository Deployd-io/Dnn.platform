// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using Newtonsoft.Json;

    public abstract class JsonPropertyAccess<TModel> : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            var json = propertyName.Trim();
            if (!(json.StartsWith("{", StringComparison.Ordinal) && json.EndsWith("}", StringComparison.Ordinal)))
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): json → {Json}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, json);
            {
                throw new ArgumentException("The token argument is not property formatted JSON");
            }

            var deserializedObject = JsonConvert.DeserializeObject<TModel>(json);

			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): deserializedObject → {DeserializedObject}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, deserializedObject);
			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return this.ProcessToken(deserializedObject, accessingUser, accessLevel);
        }

        protected abstract string ProcessToken(TModel model, UserInfo accessingUser, Scope accessLevel);
    }
}
