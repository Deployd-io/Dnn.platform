// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Modules;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// A concrete implementation of <see cref="IDesktopModuleController"/>
    /// that relies on the <see cref="DesktopModuleController"/> class.
    /// </summary>
    internal sealed class DesktopModuleControllerShim : IDesktopModuleController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DesktopModuleControllerShim"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DesktopModuleControllerShim(IHostSettings hostSettings)
        {
			_logger.LogError(">>> Entering DesktopModuleControllerShim(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings;
			_logger.LogDebug("DesktopModuleControllerShim(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        _logger.LogDebug("DesktopModuleControllerShim(hostSettings={HostSettings}): this.hostSettings → {Arg1}", hostSettings, this.hostSettings);
        _logger.LogWarning("<<< Exiting DesktopModuleControllerShim(hostSettings={HostSettings})", hostSettings);
        }

        /// <inheritdoc />
        public DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalId)
        {
			_logger.LogError(">>> Entering GetDesktopModuleByModuleName(moduleName={ModuleName},portalId={PortalId})", moduleName, portalId);
            _logger.LogWarning("<<< Exiting GetDesktopModuleByModuleName(moduleName={ModuleName},portalId={PortalId})", moduleName, portalId);
            return DesktopModuleController.GetDesktopModuleByModuleName(this.hostSettings, moduleName, portalId);
        }
    }
}
