// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using Dnn.PersonaBar.Library.Helper;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>An <see cref="IPackageEditor"/> implementation for modules.</summary>
    /// <param name="permissionDefinitionService">The permission definition service.</param>
    /// <param name="eventLogger">The event logger.</param>
    public class ModulePackageEditor(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger) : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePackageEditor));
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

        /// <summary>Initializes a new instance of the <see cref="ModulePackageEditor"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ModulePackageEditor()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering ModulePackageEditor()");
        }

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
			Logger.LogDebug(">>> Entering GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);

			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): desktopModule → {DesktopModule}", portalId, package, desktopModule);
            if (desktopModule == null)
            {
                return new PackageInfoDto(portalId, package);
            }

            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): isHostUser → {IsHostUser}", portalId, package, isHostUser);

            var detail = isHostUser ? new ModulePackageDetailDto(portalId, package, desktopModule)
                                        : new ModulePackagePermissionsDto(portalId, package);

            detail.DesktopModuleId = desktopModule.DesktopModuleID;
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): detail.DesktopModuleId → {Arg2}", portalId, package, detail.DesktopModuleId);
            detail.Permissions = GetPermissionsData(this.permissionDefinitionService, portalId, desktopModule.DesktopModuleID);

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
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(packageSettings.PackageId);
				Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): desktopModule → {DesktopModule}", packageSettings, desktopModule);

                if (desktopModule == null)
                {
                    return false;
                }

                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
				Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): isHostUser → {IsHostUser}", packageSettings, isHostUser);

                UpdatePermissions(this.permissionDefinitionService, this.eventLogger, desktopModule, packageSettings);

                if (isHostUser)
                {
                    UpdateModuleProperties(this.permissionDefinitionService, this.eventLogger, desktopModule, packageSettings.Settings);
                    UpdateModuleProperties(this.permissionDefinitionService, this.eventLogger, desktopModule, packageSettings.EditorActions);

                    DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
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

        private static void UnassignPortals(IEventLogger eventLogger, DesktopModuleInfo desktopModule, IList<ListItemDto> portals)
        {
			Logger.LogDebug(">>> Entering UnassignPortals(eventLogger={EventLogger},desktopModule={DesktopModule},portals={Portals})", eventLogger, desktopModule, portals);
            foreach (var portal in portals)
            {
                DesktopModuleController.RemoveDesktopModuleFromPortal(eventLogger, portal.Id, desktopModule.DesktopModuleID, true);
            }
			Logger.LogDebug("<<< Exiting UnassignPortals(eventLogger={EventLogger},desktopModule={DesktopModule},portals={Portals})", eventLogger, desktopModule, portals);
        }

        private static void AssignPortals(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, DesktopModuleInfo desktopModule, IList<ListItemDto> portals)
        {
			Logger.LogDebug(">>> Entering AssignPortals(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},portals={Portals})", permissionDefinitionService, eventLogger, desktopModule, portals);
            foreach (var portal in portals)
            {
                DesktopModuleController.AddDesktopModuleToPortal(eventLogger, permissionDefinitionService, portal.Id, desktopModule.DesktopModuleID, true, true);
            }
			Logger.LogDebug("<<< Exiting AssignPortals(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},portals={Portals})", permissionDefinitionService, eventLogger, desktopModule, portals);
        }

        private static void SaveModuleDefinition(IPermissionDefinitionService permissionDefinitionService, ModuleDefinitionDto definitionDto)
        {
			Logger.LogDebug(">>> Entering SaveModuleDefinition(permissionDefinitionService={PermissionDefinitionService},definitionDto={DefinitionDto})", permissionDefinitionService, definitionDto);
            var moduleDefinition = definitionDto.ToModuleDefinitionInfo();
            ModuleDefinitionController.SaveModuleDefinition(permissionDefinitionService, moduleDefinition, false, true);
			Logger.LogDebug("SaveModuleDefinition(permissionDefinitionService={PermissionDefinitionService},definitionDto={DefinitionDto}): moduleDefinition → {ModuleDefinition}", permissionDefinitionService, definitionDto, moduleDefinition);
			Logger.LogDebug("<<< Exiting SaveModuleDefinition(permissionDefinitionService={PermissionDefinitionService},definitionDto={DefinitionDto})", permissionDefinitionService, definitionDto);
        }

        private static void DeleteModuleDefinition(IPermissionDefinitionService permissionDefinitionService, int defId)
        {
			Logger.LogDebug(">>> Entering DeleteModuleDefinition(permissionDefinitionService={PermissionDefinitionService},defId={DefId})", permissionDefinitionService, defId);
            new ModuleDefinitionController(permissionDefinitionService).DeleteModuleDefinition(defId);
			Logger.LogDebug("<<< Exiting DeleteModuleDefinition(permissionDefinitionService={PermissionDefinitionService},defId={DefId})", permissionDefinitionService, defId);
        }

        private static void SaveModuleControl(ModuleControlDto moduleControlDto)
        {
			Logger.LogDebug(">>> Entering SaveModuleControl(moduleControlDto={ModuleControlDto})", moduleControlDto);
            var moduleControl = moduleControlDto.ToModuleControlInfo();
            ModuleControlController.SaveModuleControl(moduleControl, true);
			Logger.LogDebug("SaveModuleControl(moduleControlDto={ModuleControlDto}): moduleControl → {ModuleControl}", moduleControlDto, moduleControl);
			Logger.LogDebug("<<< Exiting SaveModuleControl(moduleControlDto={ModuleControlDto})", moduleControlDto);
        }

        private static void DeleteModuleControl(int controlId)
        {
			Logger.LogDebug(">>> Entering DeleteModuleControl(controlId={ControlId})", controlId);
            ModuleControlController.DeleteModuleControl(controlId);
			Logger.LogDebug("<<< Exiting DeleteModuleControl(controlId={ControlId})", controlId);
        }

        private static void UpdateModuleProperties(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, DesktopModuleInfo desktopModule, IDictionary<string, string> settings)
        {
			Logger.LogDebug(">>> Entering UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings})", permissionDefinitionService, eventLogger, desktopModule, settings);
            foreach (var setting in settings)
            {
                var settingName = setting.Key;
                var settingValue = setting.Value;

                switch (settingName.ToLowerInvariant())
                {
                    case "foldername":
                        desktopModule.FolderName = settingValue;
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): desktopModule.FolderName → {Arg4}", permissionDefinitionService, eventLogger, desktopModule, settings, desktopModule.FolderName);
                        break;
                    case "category":
                        desktopModule.Category = settingValue;
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): desktopModule.Category → {Arg4}", permissionDefinitionService, eventLogger, desktopModule, settings, desktopModule.Category);
                        break;
                    case "businesscontroller":
                        desktopModule.BusinessControllerClass = settingValue;
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): desktopModule.BusinessControllerClass → {Arg4}", permissionDefinitionService, eventLogger, desktopModule, settings, desktopModule.BusinessControllerClass);
                        break;
                    case "dependencies":
                        desktopModule.Dependencies = settingValue;
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): desktopModule.Dependencies → {Arg4}", permissionDefinitionService, eventLogger, desktopModule, settings, desktopModule.Dependencies);
                        break;
                    case "hostpermissions":
                        desktopModule.Permissions = settingValue;
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): desktopModule.Permissions → {Arg4}", permissionDefinitionService, eventLogger, desktopModule, settings, desktopModule.Permissions);
                        break;
                    case "premiummodule":
                        desktopModule.IsPremium = Convert.ToBoolean(settingValue);
                        break;
                    case "shareable":
                        desktopModule.Shareable = (ModuleSharing)Convert.ToInt32(settingValue, CultureInfo.InvariantCulture);
                        break;
                    case "assignportal":
                        AssignPortals(permissionDefinitionService, eventLogger, desktopModule, JsonConvert.DeserializeObject<IList<ListItemDto>>(settingValue));
                        break;
                    case "unassignportal":
                        UnassignPortals(eventLogger, desktopModule, JsonConvert.DeserializeObject<IList<ListItemDto>>(settingValue));
                        break;
                    case "savedefinition":
                        var definition = JsonConvert.DeserializeObject<ModuleDefinitionDto>(settingValue);
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): definition → {Definition}", permissionDefinitionService, eventLogger, desktopModule, settings, definition);
                        SaveModuleDefinition(permissionDefinitionService, definition);
                        break;
                    case "deletedefinition":
                        DeleteModuleDefinition(permissionDefinitionService, Convert.ToInt32(settingValue, CultureInfo.InvariantCulture));
                        break;
                    case "savemodulecontrol":
                        var moduleControl = JsonConvert.DeserializeObject<ModuleControlDto>(settingValue);
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): moduleControl → {ModuleControl}", permissionDefinitionService, eventLogger, desktopModule, settings, moduleControl);
                        SaveModuleControl(moduleControl);
                        break;
                    case "deletemodulecontrol":
                        DeleteModuleControl(Convert.ToInt32(settingValue, CultureInfo.InvariantCulture));
                        break;
                    case "friendlyname":
                        desktopModule.FriendlyName = settingValue;
						Logger.LogDebug("UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings}): desktopModule.FriendlyName → {Arg4}", permissionDefinitionService, eventLogger, desktopModule, settings, desktopModule.FriendlyName);
                        break;
                }
            }
			Logger.LogDebug("<<< Exiting UpdateModuleProperties(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},settings={Settings})", permissionDefinitionService, eventLogger, desktopModule, settings);
        }

        private static PermissionsDto GetPermissionsData(IPermissionDefinitionService permissionDefinitionService, int portalId, int desktopModuleId)
        {
			Logger.LogDebug(">>> Entering GetPermissionsData(permissionDefinitionService={PermissionDefinitionService},portalId={PortalId},desktopModuleId={DesktopModuleId})", permissionDefinitionService, portalId, desktopModuleId);
            var permissions = new PermissionsDto(permissionDefinitionService, true);
            if (desktopModuleId > 0)
            {
                var portalModule = DesktopModuleController.GetPortalDesktopModule(portalId, desktopModuleId);
				Logger.LogDebug("GetPermissionsData(permissionDefinitionService={PermissionDefinitionService},portalId={PortalId},desktopModuleId={DesktopModuleId}): portalModule → {PortalModule}", permissionDefinitionService, portalId, desktopModuleId, portalModule);
                if (portalModule != null)
                {
                    permissions.DesktopModuleId = desktopModuleId;
					Logger.LogDebug("GetPermissionsData(permissionDefinitionService={PermissionDefinitionService},portalId={PortalId},desktopModuleId={DesktopModuleId}): permissions.DesktopModuleId → {Arg3}", permissionDefinitionService, portalId, desktopModuleId, permissions.DesktopModuleId);

                    var modulePermissions = DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID);
					Logger.LogDebug("GetPermissionsData(permissionDefinitionService={PermissionDefinitionService},portalId={PortalId},desktopModuleId={DesktopModuleId}): modulePermissions → {ModulePermissions}", permissionDefinitionService, portalId, desktopModuleId, modulePermissions);
                    foreach (IPermissionInfo permission in modulePermissions)
                    {
                        if (permission.UserId != Null.NullInteger)
                        {
                            permissions.AddUserPermission(permission);
                        }
                        else
                        {
                            permissions.AddRolePermission(permission);
                        }
                    }

                    permissions.RolePermissions =
                        permissions.RolePermissions.OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                    permissions.UserPermissions = permissions.UserPermissions.OrderBy(p => p.DisplayName).ToList();
                }
            }

			Logger.LogDebug("<<< Exiting GetPermissionsData(permissionDefinitionService={PermissionDefinitionService},portalId={PortalId},desktopModuleId={DesktopModuleId})", permissionDefinitionService, portalId, desktopModuleId);
            return permissions;
        }

        private static void UpdatePermissions(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, DesktopModuleInfo desktopModule, PackageSettingsDto packageSettings)
        {
			Logger.LogDebug(">>> Entering UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings})", permissionDefinitionService, eventLogger, desktopModule, packageSettings);
            if (!packageSettings.EditorActions.TryGetValue("permissions", out var permissionsJson) || string.IsNullOrEmpty(permissionsJson))
            {
                return;
            }

            var portalModule = DesktopModuleController.GetPortalDesktopModule(packageSettings.PortalId, desktopModule.DesktopModuleID);
			Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): portalModule → {PortalModule}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, portalModule);
            if (portalModule == null)
            {
                return;
            }

            var portalSettings = new PortalSettings(packageSettings.PortalId);
			Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): portalSettings → {PortalSettings}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, portalSettings);
            var permissions = JsonConvert.DeserializeObject<PermissionsDto>(permissionsJson);
			Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): permissions → {Permissions}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, permissions);
            var hasAdmin = permissions.RolePermissions?.Any(permission => permission.RoleId == portalSettings.AdministratorRoleId) ?? false;
			Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): hasAdmin → {HasAdmin}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, hasAdmin);

            var desktopModulePermissions = new DesktopModulePermissionCollection();
			Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): desktopModulePermissions → {DesktopModulePermissions}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, desktopModulePermissions);

            // add default permissions for administrators
            if (!hasAdmin || (permissions.RolePermissions.Count == 0 && permissions.UserPermissions.Count == 0))
            {
                // add default permissions
                var permissionsList = permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_DESKTOPMODULE", "DEPLOY");
				Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): permissionsList → {PermissionsList}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, permissionsList);
                foreach (var permissionInfo in permissionsList)
                {
                    var permission = new DesktopModulePermissionInfo(permissionInfo)
                    {
                        PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                        AllowAccess = true,
                        RoleName = portalSettings.AdministratorRoleName,
                    };
                    ((IPermissionInfo)permission).RoleId = portalSettings.AdministratorRoleId;
                    ((IPermissionInfo)permission).UserId = Null.NullInteger;
                    desktopModulePermissions.Add(permission);
                }
            }

            // add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions)
                {
                    foreach (var permission in rolePermission.Permissions)
                    {
                        var info = new DesktopModulePermissionInfo
                        {
                            PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                            AllowAccess = permission.AllowAccess,
                        };
                        ((IPermissionInfo)info).PermissionId = permission.PermissionId;
                        ((IPermissionInfo)info).RoleId = rolePermission.RoleId;
                        ((IPermissionInfo)info).UserId = Null.NullInteger;
                        desktopModulePermissions.Add(info);
                    }
                }
            }

            // add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    foreach (var permission in userPermission.Permissions)
                    {
                        if (!int.TryParse(Globals.glbRoleNothing, out var roleId))
                        {
                            roleId = -4;
							Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): roleId → {RoleId}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, roleId);
                        }

                        var desktopModulePermissionInfo = new DesktopModulePermissionInfo
                        {
                            PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                            AllowAccess = permission.AllowAccess,
                        };
                        ((IPermissionInfo)desktopModulePermissionInfo).PermissionId = permission.PermissionId;
                        ((IPermissionInfo)desktopModulePermissionInfo).RoleId = roleId;
                        ((IPermissionInfo)desktopModulePermissionInfo).UserId = userPermission.UserId;
                        desktopModulePermissions.Add(desktopModulePermissionInfo);
                    }
                }
            }

            // Update DesktopModule Permissions
            var currentPermissions = DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID);
			Logger.LogDebug("UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings}): currentPermissions → {CurrentPermissions}", permissionDefinitionService, eventLogger, desktopModule, packageSettings, currentPermissions);
            if (!currentPermissions.CompareTo(desktopModulePermissions))
            {
                DesktopModulePermissionController.DeleteDesktopModulePermissionsByPortalDesktopModuleID(eventLogger, portalModule.PortalDesktopModuleID);
                foreach (DesktopModulePermissionInfo objPermission in desktopModulePermissions)
                {
                    DesktopModulePermissionController.AddDesktopModulePermission(eventLogger, objPermission);
                }
            }

            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.PortalDesktopModuleCacheKey, portalSettings.PortalId));
			Logger.LogDebug("<<< Exiting UpdatePermissions(permissionDefinitionService={PermissionDefinitionService},eventLogger={EventLogger},desktopModule={DesktopModule},packageSettings={PackageSettings})", permissionDefinitionService, eventLogger, desktopModule, packageSettings);
        }
		Logger.LogDebug("<<< Exiting ModulePackageEditor()");
    }
}
