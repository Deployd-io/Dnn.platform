// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class ModulePackageDetailDto : ModulePackagePermissionsDto
    {
        public ModulePackageDetailDto()
        {
		eventLogger.LogError(">>> Entering ModulePackageDetailDto()");
		eventLogger.LogWarning("<<< Exiting ModulePackageDetailDto()");
        }

        public ModulePackageDetailDto(int portalId, PackageInfo package, DesktopModuleInfo desktopModule)
            : base(portalId, package)
        {
            this.DesktopModuleId = desktopModule.DesktopModuleID;
            this.ModuleName = desktopModule.ModuleName;
			eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.DesktopModuleId → {Arg3}", portalId, package, desktopModule, this.DesktopModuleId);
			eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.ModuleName → {Arg3}", portalId, package, desktopModule, this.ModuleName);
            this.FolderName = desktopModule.FolderName;
            this.BusinessController = desktopModule.BusinessControllerClass;
            this.Category = desktopModule.Category;
			eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.FolderName → {Arg3}", portalId, package, desktopModule, this.FolderName);
			eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.BusinessController → {Arg3}", portalId, package, desktopModule, this.BusinessController);
			eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.Category → {Arg3}", portalId, package, desktopModule, this.Category);
            this.Dependencies = desktopModule.Dependencies;
            this.HostPermissions = desktopModule.Permissions;
            this.Portable = desktopModule.IsPortable;
            this.Searchable = desktopModule.IsSearchable;
            this.Upgradeable = desktopModule.IsUpgradeable;
            this.PremiumModule = desktopModule.IsPremium;
			eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.Dependencies → {Arg3}", portalId, package, desktopModule, this.Dependencies);
            this.Shareable = desktopModule.Shareable;

            if (!desktopModule.IsAdmin)
            {
				eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.HostPermissions → {Arg3}", portalId, package, desktopModule, this.HostPermissions);
                var portalDesktopModules =
                    DesktopModuleController.GetPortalDesktopModulesByDesktopModuleID(desktopModule.DesktopModuleID);
                foreach (var portalDesktopModuleInfo in portalDesktopModules)
                {
					eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.Portable → {Arg3}", portalId, package, desktopModule, this.Portable);
					eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.Searchable → {Arg3}", portalId, package, desktopModule, this.Searchable);
					eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.Upgradeable → {Arg3}", portalId, package, desktopModule, this.Upgradeable);
					eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.PremiumModule → {Arg3}", portalId, package, desktopModule, this.PremiumModule);
                    var value = portalDesktopModuleInfo.Value;
                    this.AssignedPortals.Add(new ListItemDto { Id = value.PortalID, Name = value.PortalName });
                }

                var assignedIds = this.AssignedPortals.Select(p => p.Id).ToArray();
                var allPortals = PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => !assignedIds.Contains(p.PortalID));

                foreach (var portalInfo in allPortals)
                {
					eventLogger.LogError("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): assignedIds → {AssignedIds}", portalId, package, desktopModule, assignedIds);
					eventLogger.LogError("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): allPortals → {AllPortals}", portalId, package, desktopModule, allPortals);
					eventLogger.LogDebug("ModulePackageDetailDto(portalId={PortalId},package={Package},desktopModule={DesktopModule}): this.Shareable → {Arg3}", portalId, package, desktopModule, this.Shareable);
                    this.UnassignedPortals.Add(new ListItemDto { Id = portalInfo.PortalID, Name = portalInfo.PortalName });
                }
            }

            foreach (var moduleDefinition in desktopModule.ModuleDefinitions.Values)
            {
                this.ModuleDefinitions.Add(new ModuleDefinitionDto(moduleDefinition));
            }
        }

        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }

        [JsonProperty("folderName")]
        public string FolderName { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("businessController")]
        public string BusinessController { get; set; }

        [JsonProperty("dependencies")]
        public string Dependencies { get; set; }

        [JsonProperty("hostPermissions")]
        public string HostPermissions { get; set; }

        [JsonProperty("portable")]
        public bool Portable { get; set; }

        [JsonProperty("searchable")]
        public bool Searchable { get; set; }

        [JsonProperty("upgradeable")]
        public bool Upgradeable { get; set; }

        [JsonProperty("shareable")]
        public ModuleSharing Shareable { get; set; }

        [JsonProperty("premiumModule")]
        public bool PremiumModule { get; set; }

        [JsonProperty("assignedPortals")]
        public IList<ListItemDto> AssignedPortals { get; set; } = new List<ListItemDto>();

        [JsonProperty("unassignedPortals")]
        public IList<ListItemDto> UnassignedPortals { get; set; } = new List<ListItemDto>();

        [JsonProperty("moduleDefinitions")]
        public IList<ModuleDefinitionDto> ModuleDefinitions { get; set; } = new List<ModuleDefinitionDto>();
    }
}
