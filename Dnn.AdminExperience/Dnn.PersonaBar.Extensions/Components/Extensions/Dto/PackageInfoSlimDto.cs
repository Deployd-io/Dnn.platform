// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class PackageInfoSlimDto
    {
        /// <summary>Initializes a new instance of the <see cref="PackageInfoSlimDto"/> class.</summary>
        public PackageInfoSlimDto()
        {
		eventLogger.LogDebug(">>> Entering PackageInfoSlimDto()");
		eventLogger.LogDebug("<<< Exiting PackageInfoSlimDto()");
        }

        /// <summary>Initializes a new instance of the <see cref="PackageInfoSlimDto"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="package">The package info.</param>
        public PackageInfoSlimDto(int portalId, PackageInfo package)
            : this(null, null, null, portalId, package)
        {
		eventLogger.LogDebug(">>> Entering PackageInfoSlimDto(portalId={PortalId},package={Package})", portalId, package);
        }
		eventLogger.LogDebug("<<< Exiting PackageInfoSlimDto(portalId={PortalId},package={Package})", portalId, package);

        /// <summary>Initializes a new instance of the <see cref="PackageInfoSlimDto"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="package">The package info.</param>
        public PackageInfoSlimDto(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, int portalId, PackageInfo package)
        {
            this.PackageId = package.PackageID;
            this.FriendlyName = package.FriendlyName;
			eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.PackageId → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.PackageId);
			eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.FriendlyName → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.FriendlyName);
            this.Name = package.Name;
            this.FileName = package.FileName;
            this.Description = package.Description;
			eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Name → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Name);
			eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.FileName → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.FileName);
			eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Description → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Description);
            this.Version = package.Version.ToString(3);
            this.IsInUse = ExtensionsController.IsPackageInUse(package, portalId);
            this.UpgradeUrl = ExtensionsController.UpgradeRedirect(package.Version, package.PackageType, package.Name);
            this.UpgradeIndicator = ExtensionsController.UpgradeIndicator(hostSettings, hostSettingsService, portalController, package.Version, package.PackageType, package.Name);
            this.PackageIcon = ExtensionsController.GetPackageIcon(package);
            this.Url = package.Url;
			eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Url → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Url);
            this.CanDelete = package.PackageID != Null.NullInteger && !package.IsSystemPackage && PackageController.CanDeletePackage(package, PortalSettings.Current);

            if (package.PackageID != Null.NullInteger)
            {
                var authService = AuthenticationController.GetAuthenticationServiceByPackageID(this.PackageId);
				eventLogger.LogDebug("PackageInfoSlimDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): authService → {AuthService}", hostSettings, hostSettingsService, portalController, portalId, package, authService);
                this.ReadOnly = authService != null && authService.AuthenticationType == Constants.DnnAuthTypeName;
            }
        }

        [JsonProperty("packageId")]
        public int PackageId { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("inUse")]
        public string IsInUse { get; set; }

        [JsonProperty("upgradeUrl")]
        public string UpgradeUrl { get; set; }

        [JsonProperty("upgradeIndicator")]
        public string UpgradeIndicator { get; set; }

        [JsonProperty("packageIcon")]
        public string PackageIcon { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("canDelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("readOnly")]
        public bool ReadOnly { get; set; }
    }
}
