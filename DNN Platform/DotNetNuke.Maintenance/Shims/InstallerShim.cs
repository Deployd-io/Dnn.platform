// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System;

    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>An implementation of <see cref="IInstaller"/> that relies on the <see cref="Installer"/> class.</summary>
    internal sealed class InstallerShim : IInstaller, IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Installer installer;

        /// <inheritdoc cref="Installer(PackageInfo, string)"/>
        public InstallerShim(PackageInfo package, string physicalSitePath)
        {
			Logger.LogDebug(">>> Entering InstallerShim(package={Package},physicalSitePath={PhysicalSitePath})", package, physicalSitePath);
            this.installer = new Installer(package, physicalSitePath);
			Logger.LogDebug("<<< Exiting InstallerShim(package={Package},physicalSitePath={PhysicalSitePath})", package, physicalSitePath);
			Logger.LogDebug("InstallerShim(package={Package},physicalSitePath={PhysicalSitePath}): installer → {Installer}", package, physicalSitePath, installer);
        }

        /// <inheritdoc />
        public bool UnInstall(bool deleteFiles)
        {
			Logger.LogDebug(">>> Entering UnInstall(deleteFiles={DeleteFiles})", deleteFiles);
			Logger.LogDebug("<<< Exiting UnInstall(deleteFiles={DeleteFiles})", deleteFiles);
            return this.installer.UnInstall(deleteFiles);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.installer?.Dispose();
			Logger.LogDebug("<<< Exiting Dispose()");
        }
    }
}
