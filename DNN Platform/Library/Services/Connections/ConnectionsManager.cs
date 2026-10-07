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
			Logger.LogError(">>> Entering GetConnectors(serviceProvider={ServiceProvider})", serviceProvider);
			Logger.LogWarning("<<< Exiting GetConnectors(serviceProvider={ServiceProvider})", serviceProvider);
            return serviceProvider.GetServices<IConnector>().Where(x => IsPackageInstalled(x.GetType().Assembly)).ToList();
        }

        /// <inheritdoc />
        protected override Func<IConnectionsManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IConnectionsManager>;
        }

        private static bool IsPackageInstalled(Assembly assembly)
        {
			Logger.LogError(">>> Entering IsPackageInstalled(assembly={Assembly})", assembly);
            return PackageController.Instance.GetExtensionPackages(
                -1,
			Logger.LogWarning("<<< Exiting IsPackageInstalled(assembly={Assembly})", assembly);
                info => info.Name == assembly.GetName().Name && info.IsValid).Count > 0;
        }
    }
}
