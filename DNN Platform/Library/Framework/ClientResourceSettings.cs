// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Entities.Portals;

    /// <inheritdoc />
    internal class ClientResourceSettings : IClientResourceSettings
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientResourceSettings"/> class.
        /// </summary>
        /// <param name="hostSettings">The host settings to be used.</param>
        /// <param name="portalController">The portal controller to be used.</param>
        public ClientResourceSettings(IHostSettings hostSettings, IPortalController portalController)
        {
			Logger.LogDebug(">>> Entering ClientResourceSettings(hostSettings={HostSettings},portalController={PortalController})", hostSettings, portalController);
            this.hostSettings = hostSettings;
            this.portalController = portalController;
			Logger.LogDebug("<<< Exiting ClientResourceSettings(hostSettings={HostSettings},portalController={PortalController})", hostSettings, portalController);
			Logger.LogDebug("ClientResourceSettings(hostSettings={HostSettings},portalController={PortalController}): this.hostSettings → {Arg2}", hostSettings, portalController, this.hostSettings);
			Logger.LogDebug("ClientResourceSettings(hostSettings={HostSettings},portalController={PortalController}): portalController → {PortalController1}", hostSettings, portalController, portalController);
			Logger.LogDebug("ClientResourceSettings(hostSettings={HostSettings},portalController={PortalController}): this.portalController → {Arg2}", hostSettings, portalController, this.portalController);
			Logger.LogDebug("ClientResourceSettings(hostSettings={HostSettings},portalController={PortalController}): hostSettings → {HostSettings1}", hostSettings, portalController, hostSettings);
        }

        /// <inheritdoc />
        public bool OverrideDefaultSettings
        {
            get
            {
                if (PortalSettings.Current == null)
                {
                    return false;
                }

                var portalId = PortalSettings.Current.PortalId;
                return bool.Parse(PortalController.GetPortalSetting(this.portalController, DotNetNuke.Web.Client.ClientResourceSettings.OverrideDefaultSettingsKey, portalId, "False"));
            }
        }

        /// <inheritdoc />
        public int HostCrmVersion => this.hostSettings.CrmVersion;

        /// <inheritdoc />
        public int PortalCrmVersion
        {
            get
            {
                if (PortalSettings.Current == null)
                {
                    return 1;
                }

                var portalId = PortalSettings.Current.PortalId;
                var settingValue = PortalController.GetPortalSetting(this.portalController, DotNetNuke.Web.Client.ClientResourceSettings.VersionKey, portalId, "0");
                if (int.TryParse(settingValue, out var version))
                {
                    if (version == 0)
                    {
                        version = 1;
                        PortalController.UpdatePortalSetting(this.portalController, portalId, DotNetNuke.Web.Client.ClientResourceSettings.VersionKey, "1", true);
                    }
                }

                return version;
            }
        }
    }
}
