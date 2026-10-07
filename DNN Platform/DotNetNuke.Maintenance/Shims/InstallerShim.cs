// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System;

    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
	using Microsoft.Extensions.Logging;

    /// <summary>An implementation of <see cref="IInstaller"/> that relies on the <see cref="Installer"/> class.</summary>
    internal sealed class InstallerShim : IInstaller, IDisposable
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly Installer installer;

        /// <inheritdoc cref="Installer(PackageInfo, string)"/>
        public InstallerShim(PackageInfo package, string physicalSitePath)
        {
			_logger.LogError(">>> Entering InstallerShim(package={Package},physicalSitePath={PhysicalSitePath})", package, physicalSitePath);
            this.installer = new Installer(package, physicalSitePath);
			_logger.LogDebug("InstallerShim(package={Package},physicalSitePath={PhysicalSitePath}): installer → {Installer}", package, physicalSitePath, installer);
        _logger.LogWarning("<<< Exiting InstallerShim(package={Package},physicalSitePath={PhysicalSitePath})", package, physicalSitePath);
        }

        /// <inheritdoc />
        public bool UnInstall(bool deleteFiles)
        {
			_logger.LogError(">>> Entering UnInstall(deleteFiles={DeleteFiles})", deleteFiles);
            _logger.LogWarning("<<< Exiting UnInstall(deleteFiles={DeleteFiles})", deleteFiles);
            return this.installer.UnInstall(deleteFiles);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			_logger.LogError(">>> Entering Dispose()");
            this.installer?.Dispose();
			_logger.LogWarning("<<< Exiting Dispose()");
        }
    }
}
