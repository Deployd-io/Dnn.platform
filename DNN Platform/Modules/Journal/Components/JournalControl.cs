// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Services.Tokens;

    public class JournalControl : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JournalControl));
        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string CommentLink { get; set; }

        public string LikeLink { get; set; }

        public string LikeList { get; set; }

        public string CommentArea { get; set; }

        public string AuthorNameLink { get; set; }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogError(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            string outputFormat = string.Empty;
			Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, outputFormat);
            if (format == string.Empty)
            {
                outputFormat = "g";
            }
            else
            {
                outputFormat = format;
            }

            propertyName = propertyName.ToLowerInvariant();
			Logger.LogError("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound}): propertyName → {PropertyName1}", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound, propertyName);
            switch (propertyName)
            {
                case "commentlink":
                    return this.CommentLink;
                case "likelink":
                    return this.LikeLink;
                case "likelist":
                    return this.LikeList;
                case "commentarea":
                    return this.CommentArea;
                case "authornamelink":
                    return this.AuthorNameLink;
            }

            propertyNotFound = true;
			Logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return string.Empty;
        }
    }
}
