// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;

    public class SkinObjectPackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinObjectPackageEditor));

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
			Logger.LogDebug(">>> Entering GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            var skinControl = SkinControlController.GetSkinControlByPackageID(package.PackageID);
            var detail = new SkinObjectPackageDetailDto(portalId, package);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail → {Detail}", portalId, package, detail);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.ControlKey → {Arg2}", portalId, package, detail.ControlKey);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.ControlSrc → {Arg2}", portalId, package, detail.ControlSrc);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.SupportsPartialRendering → {Arg2}", portalId, package, detail.SupportsPartialRendering);
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): isHostUser → {IsHostUser}", portalId, package, isHostUser);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): skinControl → {SkinControl}", portalId, package, skinControl);

            detail.ControlKey = skinControl.ControlKey;
            detail.ControlSrc = skinControl.ControlSrc;
            detail.SupportsPartialRendering = skinControl.SupportsPartialRendering;
            detail.ReadOnly |= !isHostUser;

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
                string value;
                var skinControl = SkinControlController.GetSkinControlByPackageID(packageSettings.PackageId);
				Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): skinControl → {SkinControl}", packageSettings, skinControl);

                if (packageSettings.EditorActions.TryGetValue("controlKey", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    skinControl.ControlKey = value;
					Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): skinControl.ControlKey → {Arg1}", packageSettings, skinControl.ControlKey);
                }

                if (packageSettings.EditorActions.TryGetValue("controlSrc", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    skinControl.ControlSrc = value;
					Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): skinControl.ControlSrc → {Arg1}", packageSettings, skinControl.ControlSrc);
                }

                if (packageSettings.EditorActions.TryGetValue("supportsPartialRendering", out value)
                    && !string.IsNullOrEmpty(value))
                {
                    if (bool.TryParse(value, out var supportsPartialRendering))
                    {
                        skinControl.SupportsPartialRendering = supportsPartialRendering;
						Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): skinControl.SupportsPartialRendering → {Arg1}", packageSettings, skinControl.SupportsPartialRendering);
                    }
                }

                SkinControlController.SaveSkinControl(skinControl);
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
