// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    using System.Collections.Generic;

    /// <summary>
    /// InstallConfig - A class that represents DotNetNuke.Install.Config XML configuration file
    /// TODO This class may not capture all the details from the config file.
    /// </summary>
    public class InstallConfig
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="InstallConfig"/> class.</summary>
        public InstallConfig()
        {
			Logger.LogDebug(">>> Entering InstallConfig()");
            this.Portals = new List<PortalConfig>();
            this.Scripts = new List<string>();
            this.Settings = new List<HostSettingConfig>();
			Logger.LogDebug("<<< Exiting InstallConfig()");
        }

        public IList<string> Scripts { get; set; }

        public string Version { get; set; }

        public string InstallCulture { get; set; }

        public SuperUserConfig SuperUser { get; set; }

        public ConnectionConfig Connection { get; set; }

        public IList<PortalConfig> Portals { get; set; }

        public IList<HostSettingConfig> Settings { get; set; }

        public string FolderMappingsSettings { get; set; }

        public bool SupportLocalization { get; set; }
    }
}
