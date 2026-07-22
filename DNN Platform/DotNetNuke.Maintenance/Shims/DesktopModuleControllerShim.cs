// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// A concrete implementation of <see cref="IDesktopModuleController"/>
    /// that relies on the <see cref="DesktopModuleController"/> class.
    /// </summary>
    internal sealed class DesktopModuleControllerShim : IDesktopModuleController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DesktopModuleControllerShim"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DesktopModuleControllerShim(IHostSettings hostSettings)
        {
			Logger.LogDebug(">>> Entering DesktopModuleControllerShim(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings;
			Logger.LogDebug("<<< Exiting DesktopModuleControllerShim(hostSettings={HostSettings})", hostSettings);
			Logger.LogDebug("DesktopModuleControllerShim(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
			Logger.LogDebug("DesktopModuleControllerShim(hostSettings={HostSettings}): this.hostSettings → {Arg1}", hostSettings, this.hostSettings);
        }

        /// <inheritdoc />
        public DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalId)
        {
			Logger.LogDebug(">>> Entering GetDesktopModuleByModuleName(moduleName={ModuleName},portalId={PortalId})", moduleName, portalId);
			Logger.LogDebug("<<< Exiting GetDesktopModuleByModuleName(moduleName={ModuleName},portalId={PortalId})", moduleName, portalId);
            return DesktopModuleController.GetDesktopModuleByModuleName(this.hostSettings, moduleName, portalId);
        }
    }
}
