// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5
{
    using System;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    /// <summary>Replaces tokens related to the current http request.</summary>
    public class RequestPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly HttpRequest request;

        /// <summary>Initializes a new instance of the <see cref="RequestPropertyAccess"/> class.</summary>
        /// <param name="request">The current http request.</param>
        public RequestPropertyAccess(HttpRequest request)
        {
			Logger.LogDebug(">>> Entering RequestPropertyAccess(request={Request})", request);
            this.request = request;
			Logger.LogDebug("<<< Exiting RequestPropertyAccess(request={Request})", request);
			Logger.LogDebug("RequestPropertyAccess(request={Request}): request → {Request1}", request, request);
			Logger.LogDebug("RequestPropertyAccess(request={Request}): this.request → {Arg1}", request, this.request);
        }

        /// <inheritdoc />
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            switch (propertyName.ToLowerInvariant())
            {
                case "querystring":
                    return this.request.QueryString.ToString();
                case "applicationpath":
                    return this.request.ApplicationPath;
                case "relativeapppath":
                    // RelativeAppPath is like ApplicationPath, but will always end with a forward slash (/)
                    return this.request.ApplicationPath.EndsWith("/", StringComparison.Ordinal)
                        ? this.request.ApplicationPath
                        : $"{this.request.ApplicationPath}/";
            }

            propertyNotFound = true;
			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return string.Empty;
        }
    }
}
