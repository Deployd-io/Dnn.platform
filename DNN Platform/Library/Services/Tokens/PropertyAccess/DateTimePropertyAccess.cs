// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    public class DateTimePropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.secureforCaching;
            }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            TimeZoneInfo userTimeZone = accessingUser.Profile.PreferredTimeZone;
            switch (propertyName.ToLowerInvariant())
            {
                case "current":
                    if (format == string.Empty)
                    {
                        format = "D";
                    }
					Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): format → {Format1}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, format);

                    return TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString(format, formatProvider);
                case "now":
                    if (format == string.Empty)
                    {
                        format = "g";
                    }

                    return TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString(format, formatProvider);
                case "system":
                    if (format == string.Empty)
                    {
                        format = "g";
                    }

                    return DateTime.Now.ToString(format, formatProvider);
                case "utc":
                    if (format == string.Empty)
                    {
                        format = "g";
                    }

                    return DateTime.Now.ToUniversalTime().ToString(format, formatProvider);
                default:
                    propertyNotFound = true;
					Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
                    return string.Empty;
            }
        }
    }
}
