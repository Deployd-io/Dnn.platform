// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Installer.Packages;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class ConnectionsManager : ServiceLocator<IConnectionsManager, ConnectionsManager>, IConnectionsManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConnectionsManager));
        /// <inheritdoc />
        public IList<IConnector> GetConnectors(IServiceProvider serviceProvider)
        {
			Logger.LogDebug(">>> Entering GetConnectors(serviceProvider={ServiceProvider})", serviceProvider);
			Logger.LogDebug("<<< Exiting GetConnectors(serviceProvider={ServiceProvider})", serviceProvider);
            return serviceProvider.GetServices<IConnector>().Where(x => IsPackageInstalled(x.GetType().Assembly)).ToList();
        }

        /// <inheritdoc />
        protected override Func<IConnectionsManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
			Logger.LogDebug(">>> Entering IsPackageInstalled(assembly={Assembly})", assembly);
			Logger.LogDebug("<<< Exiting IsPackageInstalled(assembly={Assembly})", assembly);
            return Globals.DependencyProvider.GetRequiredService<IConnectionsManager>;
        }

        private static bool IsPackageInstalled(Assembly assembly)
        {
            return PackageController.Instance.GetExtensionPackages(
                -1,
                info => info.Name == assembly.GetName().Name && info.IsValid).Count > 0;
        }
    }
}
