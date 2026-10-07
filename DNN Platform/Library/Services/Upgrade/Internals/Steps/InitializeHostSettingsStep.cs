// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>InstallSuperUserStep - Step that installs SuperUser Account.</summary>
    public class InitializeHostSettingsStep : BaseInstallationStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InitializeHostSettingsStep));
        /// <summary>Main method to execute the step.</summary>
        public override void Execute()
        {
			Logger.LogError(">>> Entering Execute()");
            this.Percentage = 0;
            this.Status = StepStatus.Running;
			Logger.LogDebug("Execute(): this.Percentage → {Arg0}", this.Percentage);
			Logger.LogDebug("Execute(): this.Status → {Arg0}", this.Status);

            this.Details = Localization.GetString("InitHostSetting", this.LocalInstallResourceFile);
            var installConfig = InstallController.Instance.GetInstallConfig();
			Logger.LogError("Execute(): installConfig → {InstallConfig}", installConfig);

            // if any super user (even deleted) is found - exit
            var superUsers = UserController.GetUsers(true, true, Null.NullInteger);
			Logger.LogError("Execute(): superUsers → {SuperUsers}", superUsers);
            if (superUsers != null && superUsers.Count > 0)
            {
                this.Details = "...";
                this.Status = StepStatus.Done;
				Logger.LogDebug("Execute(): this.Details → {Arg0}", this.Details);
                return;
            }

            // Need to clear the cache to pick up new HostSettings from the SQLDataProvider script
            DataCache.RemoveCache(DataCache.HostSettingsCacheKey);

            string domainName = Globals.GetDomainName(HttpContext.Current.Request);
            foreach (var setting in installConfig.Settings)
            {
                var settingName = setting.Name;
                var settingValue = setting.Value;
				Logger.LogError("Execute(): settingValue → {SettingValue}", settingValue);

                switch (settingName)
                {
                    case "HostURL":
                        if (string.IsNullOrEmpty(settingValue))
                        {
                            settingValue = domainName;
                        }

                        break;
                    case "HostEmail":
                        if (string.IsNullOrEmpty(settingValue))
                        {
                            settingValue = installConfig.SuperUser.Email;
                        }

                        break;
                }

                HostController.Instance.Update(settingName, settingValue, setting.IsSecure);
            }

            // Synchronise Host Folder
            FolderManager.Instance.Synchronize(Null.NullInteger, string.Empty, true, true);

            this.Status = StepStatus.Done;
			Logger.LogWarning("<<< Exiting Execute()");
        }
    }
}
