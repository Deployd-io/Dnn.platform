// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>InstallSuperUserStep - Step that installs SuperUser Account.</summary>
    public class InstallSuperUserStep : BaseInstallationStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallSuperUserStep));
        /// <summary>Main method to execute the step.</summary>
        public override void Execute()
        {
			Logger.LogError(">>> Entering Execute()");
            this.Percentage = 0;
            this.Status = StepStatus.Running;
			Logger.LogDebug("Execute(): this.Percentage → {Arg0}", this.Percentage);
			Logger.LogDebug("Execute(): this.Status → {Arg0}", this.Status);

            this.Details = Localization.GetString("CreateSuperUser", this.LocalInstallResourceFile);
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

            // Set admin user to be a superuser
            var adminSuperUser = UserController.GetUserByName(0, installConfig.SuperUser.UserName);
			Logger.LogError("Execute(): adminSuperUser → {AdminSuperUser}", adminSuperUser);
            if (adminSuperUser != null)
            {
                adminSuperUser.IsSuperUser = true;
				Logger.LogDebug("Execute(): adminSuperUser.IsSuperUser → {Arg0}", adminSuperUser.IsSuperUser);
                adminSuperUser.Membership.UpdatePassword = false;

                // refresh the profile to get definitions for super user.
                adminSuperUser.Profile = null;
				Logger.LogDebug("Execute(): adminSuperUser.Profile → {Arg0}", adminSuperUser.Profile);
                adminSuperUser.Profile.PreferredLocale = installConfig.SuperUser.Locale;
                adminSuperUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;
                UserController.UpdateUser(0, adminSuperUser);
            }
            else
            {
                // Construct UserInfo object
                var superUser = new UserInfo
                {
                    PortalID = -1,
                    FirstName = installConfig.SuperUser.FirstName,
                    LastName = installConfig.SuperUser.LastName,
                    Username = installConfig.SuperUser.UserName,
                    DisplayName = installConfig.SuperUser.FirstName + " " + installConfig.SuperUser.LastName,
                    Membership = { Password = installConfig.SuperUser.Password },
                    Email = installConfig.SuperUser.Email,
                    IsSuperUser = true,
                };
                superUser.Membership.Approved = true;

                superUser.Profile.FirstName = installConfig.SuperUser.FirstName;
                superUser.Profile.LastName = installConfig.SuperUser.LastName;
                superUser.Profile.PreferredLocale = installConfig.SuperUser.Locale;
                superUser.Profile.PreferredTimeZone = TimeZoneInfo.Local;
                superUser.Membership.UpdatePassword = false;

                // Create SuperUser if not present
                if (UserController.GetUserByName(superUser.PortalID, superUser.Username) == null)
                {
                    UserController.CreateUser(ref superUser);
                }
            }

            this.Details = Localization.GetString("CreatingSuperUser", this.LocalInstallResourceFile) + installConfig.SuperUser.UserName;

            this.Status = StepStatus.Done;
			Logger.LogWarning("<<< Exiting Execute()");
        }
    }
}
