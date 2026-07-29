// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using DotNetNuke.Services.Tokens;

    public class ItemData : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            string outputFormat = string.Empty;
            if (format == string.Empty)
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, outputFormat);
            {
                outputFormat = "g";
            }
            else
            {
                outputFormat = format;
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): propertyName → {PropertyName1}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, propertyName);
            {
                case "url":
                    return PropertyAccess.FormatString(this.Url, format);
                case "title":
                    return PropertyAccess.FormatString(this.Title, format);
                case "description":
                    return PropertyAccess.FormatString(this.Description, format);
                case "imageurl":
                    return PropertyAccess.FormatString(this.ImageUrl, format);
            }

            propertyNotFound = true;
			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return string.Empty;
        }
    }
}
