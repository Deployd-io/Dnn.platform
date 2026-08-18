// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    public class AntiForgeryTokenPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AntiForgeryTokenPropertyAccess));
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="AntiForgeryTokenPropertyAccess"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public AntiForgeryTokenPropertyAccess()
            : this(null)
        {
		Logger.LogDebug(">>> Entering AntiForgeryTokenPropertyAccess()");
        }
		Logger.LogDebug("<<< Exiting AntiForgeryTokenPropertyAccess()");

        /// <summary>Initializes a new instance of the <see cref="AntiForgeryTokenPropertyAccess"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        public AntiForgeryTokenPropertyAccess(IServicesFramework servicesFramework)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
			Logger.LogDebug("AntiForgeryTokenPropertyAccess(servicesFramework={ServicesFramework}): servicesFramework → {ServicesFramework1}", servicesFramework, servicesFramework);
        }

        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            this.servicesFramework.RequestAjaxAntiForgerySupport();

			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return string.Empty;
        }
    }
}
