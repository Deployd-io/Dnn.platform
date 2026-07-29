// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Components.Controllers.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    [JsonObject]
    public class PackageInfoDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="PackageInfoDto"/> class.</summary>
        public PackageInfoDto()
        {
		Logger.LogDebug(">>> Entering PackageInfoDto()");
		Logger.LogDebug("<<< Exiting PackageInfoDto()");
        }

        /// <summary>Initializes a new instance of the <see cref="PackageInfoDto"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="package">The package info.</param>
        public PackageInfoDto(int portalId, PackageInfo package)
            : this(null, null, null, portalId, package)
        {
		Logger.LogDebug(">>> Entering PackageInfoDto(portalId={PortalId},package={Package})", portalId, package);
        }
		Logger.LogDebug("<<< Exiting PackageInfoDto(portalId={PortalId},package={Package})", portalId, package);

        /// <summary>Initializes a new instance of the <see cref="PackageInfoDto"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="package">The package info.</param>
        public PackageInfoDto(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, int portalId, PackageInfo package)
        {
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

            this.PackageType = package.PackageType;
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): PackageType → {PackageType}", hostSettings, hostSettingsService, portalController, portalId, package, PackageType);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.PackageType → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.PackageType);
            this.FriendlyName = package.FriendlyName;
            this.Name = package.Name;
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): FriendlyName → {FriendlyName}", hostSettings, hostSettingsService, portalController, portalId, package, FriendlyName);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.FriendlyName → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.FriendlyName);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Name → {Name}", hostSettings, hostSettingsService, portalController, portalId, package, Name);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Name → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Name);
            this.PackageId = package.PackageID;
            this.Description = package.Description;
            this.IsInUse = ExtensionsController.IsPackageInUse(package, portalId);
            this.Version = package.Version.ToString(3);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.PackageId → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.PackageId);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Description → {Description}", hostSettings, hostSettingsService, portalController, portalId, package, Description);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Description → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Description);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Version → {Version}", hostSettings, hostSettingsService, portalController, portalId, package, Version);
            this.UpgradeUrl = ExtensionsController.UpgradeRedirect(package.Version, package.PackageType, package.Name);
            this.UpgradeIndicator = ExtensionsController.UpgradeIndicator(hostSettings, hostSettingsService, portalController, package.Version, package.PackageType, package.Name);
            this.PackageIcon = ExtensionsController.GetPackageIcon(package);
            this.License = package.License;
            this.ReleaseNotes = package.ReleaseNotes;
            this.Owner = package.Owner;
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): License → {License}", hostSettings, hostSettingsService, portalController, portalId, package, License);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.License → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.License);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): ReleaseNotes → {ReleaseNotes}", hostSettings, hostSettingsService, portalController, portalId, package, ReleaseNotes);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.ReleaseNotes → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.ReleaseNotes);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Owner → {Owner}", hostSettings, hostSettingsService, portalController, portalId, package, Owner);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Owner → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Owner);
			Logger.LogInformation("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Url → {Url}", hostSettings, hostSettingsService, portalController, portalId, package, Url);
            this.Organization = package.Organization;
            this.Url = package.Url;
            this.Email = package.Email;
            this.CanDelete = !package.IsSystemPackage &&
                             package.PackageID > 0 &&
                             PackageController.CanDeletePackage(package, PortalSettings.Current);

            var authService = AuthenticationController.GetAuthenticationServiceByPackageID(this.PackageId);
            this.ReadOnly = authService != null && authService.AuthenticationType == Constants.DnnAuthTypeName;
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): authService → {AuthService}", hostSettings, hostSettingsService, portalController, portalId, package, authService);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Organization → {Organization}", hostSettings, hostSettingsService, portalController, portalId, package, Organization);
			Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Organization → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Organization);

            var currentPage = TabController.CurrentPage;
            var tabId = currentPage.TabID;
            this.SiteSettingsLink = this.NavigationManager.NavigateURL(
                tabId,
                "EditExtension",
                $"packageid={this.PackageId}",
                "Display=editor",
                "popUp=true");
				Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Url → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Url);
				Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): Email → {Email}", hostSettings, hostSettingsService, portalController, portalId, package, Email);
				Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.Email → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.Email);
				Logger.LogDebug("PackageInfoDto(hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController},portalId={PortalId},package={Package}): this.CanDelete → {Arg5}", hostSettings, hostSettingsService, portalController, portalId, package, this.CanDelete);
        }

        [JsonProperty("packageId")]
        public int PackageId { get; set; }

        [JsonProperty("packageType")]
        public string PackageType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

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

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("canDelete")]
        public bool CanDelete { get; set; }

        [JsonProperty("readOnly")]
        public bool ReadOnly { get; set; }

        [JsonProperty("siteSettingsLink", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SiteSettingsLink { get; set; }

        protected INavigationManager NavigationManager { get; }

        public PackageInfo ToPackageInfo()
        {
			Logger.LogDebug(">>> Entering ToPackageInfo()");
            if (!System.Version.TryParse(this.Version, out var ver))
            {
                ver = null;
				Logger.LogDebug("ToPackageInfo(): ver → {Ver}", ver);
            }

			Logger.LogDebug("<<< Exiting ToPackageInfo()");
            return new PackageInfo
            {
                PackageType = this.PackageType,
                FriendlyName = this.FriendlyName,
                Name = this.Name,
                PackageID = this.PackageId,
                Description = this.Description,
                Version = ver,
                License = this.License,
                ReleaseNotes = this.ReleaseNotes,
                Owner = this.Owner,
                Organization = this.Organization,
                Url = this.Url,
                Email = this.Email,
                IconFile = this.PackageIcon,
            };
        }
    }
}
