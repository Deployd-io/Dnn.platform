// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Enables modules to support Services Framework features.</summary>
    public class ServicesFramework : ServiceLocator<IServicesFramework, ServicesFramework>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesFramework));
        public static string GetServiceFrameworkRoot()
        {
			Logger.LogDebug(">>> Entering GetServiceFrameworkRoot()");
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return string.Empty;
            }

            IPortalAliasInfo alias = portalSettings.PortalAlias;
            var path = alias.HttpAlias;
			Logger.LogDebug("GetServiceFrameworkRoot(): path → {Path}", path);
            var index = path.IndexOf('/');
            if (index > 0)
            {
                path = path.Substring(index);
                if (!path.EndsWith("/", StringComparison.Ordinal))
                {
                    path += "/";
                }
            }
            else
            {
                path = "/";
            }

			Logger.LogDebug("<<< Exiting GetServiceFrameworkRoot()");
            return path;
        }

        /// <inheritdoc />
        protected override Func<IServicesFramework> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return static () => ActivatorUtilities.GetServiceOrCreateInstance<ServicesFrameworkImpl>(Globals.DependencyProvider);
        }
    }
}
