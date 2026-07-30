// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Installer.Packages;
    using Microsoft.Extensions.DependencyInjection;

    public class AuthSystemPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthSystemPackageEditor));

        private static INavigationManager NavigationManager => Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
			Logger.LogDebug(">>> Entering GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            var detail = new AuthSystemPackageDetailDto(portalId, package)
            {
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): authSystem → {AuthSystem}", portalId, package, authSystem);
                AuthenticationType = authSystem.AuthenticationType,
            };

            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (isHostUser)
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): isHostUser → {IsHostUser}", portalId, package, isHostUser);
            {
                detail.ReadOnly |= authSystem.AuthenticationType == "DNN";
                detail.LoginControlSource = authSystem.LoginControlSrc;
				Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.LoginControlSource → {Arg2}", portalId, package, detail.LoginControlSource);
				Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.LogoffControlSource → {Arg2}", portalId, package, detail.LogoffControlSource);
				Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.SettingsControlSource → {Arg2}", portalId, package, detail.SettingsControlSource);
				Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.Enabled → {Arg2}", portalId, package, detail.Enabled);
                detail.LogoffControlSource = authSystem.LogoffControlSrc;
                detail.SettingsControlSource = authSystem.SettingsControlSrc;
                detail.Enabled = authSystem.IsEnabled;
            }

            LoadCustomSettings(portalId, package, authSystem, detail);
			Logger.LogDebug("<<< Exiting GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            return detail;
        }

        /// <inheritdoc />
        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
            errorMessage = string.Empty;

			Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): errorMessage → {ErrorMessage}", packageSettings, errorMessage);
            try
            {
                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

				Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): isHostUser → {IsHostUser}", packageSettings, isHostUser);
                if (isHostUser)
                {
                    string value;
                    var authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(packageSettings.PackageId);

					Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): authSystem → {AuthSystem}", packageSettings, authSystem);
                    if (packageSettings.EditorActions.TryGetValue("loginControlSource", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.LoginControlSrc = value;
						Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): authSystem.LoginControlSrc → {Arg1}", packageSettings, authSystem.LoginControlSrc);
                    }

                    if (packageSettings.EditorActions.TryGetValue("logoffControlSource", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.LogoffControlSrc = value;
						Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): authSystem.LogoffControlSrc → {Arg1}", packageSettings, authSystem.LogoffControlSrc);
                    }

                    if (packageSettings.EditorActions.TryGetValue("settingsControlSource", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        authSystem.SettingsControlSrc = value;
						Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): authSystem.SettingsControlSrc → {Arg1}", packageSettings, authSystem.SettingsControlSrc);
                    }

                    if (packageSettings.EditorActions.TryGetValue("enabled", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        if (bool.TryParse(value, out var isEnabled))
                        {
                            authSystem.IsEnabled = isEnabled;
							Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): authSystem.IsEnabled → {Arg1}", packageSettings, authSystem.IsEnabled);
                        }
                    }

                    AuthenticationController.UpdateAuthentication(authSystem);
                    SaveCustomSettings(packageSettings);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
				Logger.LogDebug("<<< Exiting SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
                return false;
            }
        }

        private static string GetSettingUrl(int portalId, int authSystemPackageId)
        {
			Logger.LogDebug(">>> Entering GetSettingUrl(portalId={PortalId},authSystemPackageId={AuthSystemPackageId})", portalId, authSystemPackageId);
			Logger.LogDebug("<<< Exiting GetSettingUrl(portalId={PortalId},authSystemPackageId={AuthSystemPackageId})", portalId, authSystemPackageId);
            return NavigationManager.NavigateURL(
                PortalSettings.Current.ActiveTab.TabID,
                PortalSettings.Current,
                "EditExtension",
                "packageid=" + authSystemPackageId,
                "popUp=true",
                "mode=settings");
        }

        private static void LoadCustomSettings(int portalId, PackageInfo package, AuthenticationInfo authSystem, AuthSystemPackageDetailDto detail)
        {
            var hasCustomSettings = !string.IsNullOrEmpty(authSystem.SettingsControlSrc);
            if (hasCustomSettings)
            {
                detail.SettingUrl = GetSettingUrl(portalId, package.PackageID);
            }

            // special case for DNN provided external authentication systems
            switch (detail.AuthenticationType.ToLowerInvariant())
            {
                case "facebook":
                case "google":
                case "live":
                case "twitter":
                    var config = OAuthConfigBase.GetConfig(detail.AuthenticationType, portalId);
                    if (config != null)
                    {
                        detail.AppId = config.APIKey;
                        detail.AppSecret = config.APISecret;
                        detail.AppEnabled = config.Enabled;
                    }

                    break;
            }
        }

        private static void SaveCustomSettings(PackageSettingsDto packageSettings)
        {
			Logger.LogDebug(">>> Entering SaveCustomSettings(packageSettings={PackageSettings})", packageSettings);
            // special case for specific DNN provided external authentication systems
            string authType;
            if (packageSettings.EditorActions.TryGetValue("authenticationType", out authType))
            {
                switch (authType.ToLowerInvariant())
                {
                    case "facebook":
                    case "google":
                    case "live":
                    case "twitter":
                        var dirty = false;
                        string value;
						Logger.LogDebug("SaveCustomSettings(packageSettings={PackageSettings}): dirty → {Dirty}", packageSettings, dirty);
                        var config = OAuthConfigBase.GetConfig(authType, packageSettings.PortalId);

						Logger.LogDebug("SaveCustomSettings(packageSettings={PackageSettings}): config → {Config}", packageSettings, config);
                        if (packageSettings.EditorActions.TryGetValue("appId", out value)
                            && config.APIKey != value)
                        {
                            config.APIKey = value;
							Logger.LogDebug("SaveCustomSettings(packageSettings={PackageSettings}): config.APIKey → {Arg1}", packageSettings, config.APIKey);
                            dirty = true;
                        }

                        if (packageSettings.EditorActions.TryGetValue("appSecret", out value)
                            && config.APISecret != value)
                        {
                            config.APISecret = value;
							Logger.LogDebug("SaveCustomSettings(packageSettings={PackageSettings}): config.APISecret → {Arg1}", packageSettings, config.APISecret);
                            dirty = true;
                        }

                        if (packageSettings.EditorActions.TryGetValue("appEnabled", out value)
                            && !config.Enabled.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            config.Enabled = "TRUE".Equals(value, StringComparison.OrdinalIgnoreCase);
                            dirty = true;
                        }

                        if (dirty)
                        {
                            OAuthConfigBase.UpdateConfig(config);
                        }

                        break;
                }
            }
			Logger.LogDebug("<<< Exiting SaveCustomSettings(packageSettings={PackageSettings})", packageSettings);
        }
    }
}
