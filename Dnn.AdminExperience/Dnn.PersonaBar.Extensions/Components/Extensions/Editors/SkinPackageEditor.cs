// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.UI.Skins;

    public class SkinPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinPackageEditor));

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
			Logger.LogDebug(">>> Entering GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            var skin = SkinController.GetSkinByPackageID(package.PackageID);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): skin → {Skin}", portalId, package, skin);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): isHostUser → {IsHostUser}", portalId, package, isHostUser);
            var detail = new SkinPackageDetailDto(portalId, package)
            {
                ThemePackageName = skin.SkinName,
                ReadOnly = !isHostUser,
            };

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
                    var skin = SkinController.GetSkinByPackageID(packageSettings.PackageId);
					Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): skin → {Skin}", packageSettings, skin);

                    if (packageSettings.EditorActions.TryGetValue("themePackageName", out value)
                        && !string.IsNullOrEmpty(value))
                    {
                        skin.SkinName = value;
						Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): skin.SkinName → {Arg1}", packageSettings, skin.SkinName);
                        SkinController.UpdateSkinPackage(skin);
                    }
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
    }
}
