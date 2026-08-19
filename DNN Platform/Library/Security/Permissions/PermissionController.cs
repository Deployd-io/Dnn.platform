// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="IPermissionDefinitionService"/> implementation.</summary>
    public partial class PermissionController(IEventLogger eventLogger, DataProvider dataProvider, IHostSettings hostSettings) : IPermissionDefinitionService
    {
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly DataProvider dataProvider = dataProvider ?? DataProvider.Instance();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="PermissionController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PermissionController()
            : this(null, null, null)
        {
		eventLogger.LogDebug(">>> Entering PermissionController()");
        }

        /// <summary>Initializes a new instance of the <see cref="PermissionController"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="dataProvider">The underlying data-provider to use.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PermissionController(IEventLogger eventLogger, DataProvider dataProvider)
                : this(eventLogger, dataProvider, null)
        {
        }

        public static string BuildPermissions(IList permissions, string permissionKey)
        {
			eventLogger.LogDebug(">>> Entering BuildPermissions(permissions={Permissions},permissionKey={PermissionKey})", permissions, permissionKey);
            var permissionsBuilder = new StringBuilder();
            foreach (IPermissionInfo permission in permissions)
            {
			eventLogger.LogDebug("BuildPermissions(permissions={Permissions},permissionKey={PermissionKey}): permissionsBuilder → {PermissionsBuilder}", permissions, permissionKey, permissionsBuilder);
                if (permissionKey.Equals(permission.PermissionKey, StringComparison.OrdinalIgnoreCase))
                {
                    // Deny permissions are prefixed with a "!"
                    string prefix = !permission.AllowAccess ? "!" : string.Empty;

                    // encode permission
                    string permissionString;
                    if (Null.IsNull(permission.UserId))
                    {
                        permissionString = prefix + permission.RoleName + ";";
						eventLogger.LogDebug("BuildPermissions(permissions={Permissions},permissionKey={PermissionKey}): permissionString → {PermissionString}", permissions, permissionKey, permissionString);
                    }
                    else
                    {
                        permissionString = $"{prefix}[{permission.UserId}];";
                    }

                    // build permissions string ensuring that Deny permissions are inserted at the beginning and Grant permissions at the end
                    if (prefix == "!")
                    {
                        permissionsBuilder.Insert(0, permissionString);
                    }
                    else
                    {
                        permissionsBuilder.Append(permissionString);
                    }
                }
            }

            // get string
            string permissionsString = permissionsBuilder.ToString();
			eventLogger.LogDebug("BuildPermissions(permissions={Permissions},permissionKey={PermissionKey}): permissionsString → {PermissionsString}", permissions, permissionKey, permissionsString);

            // ensure leading delimiter
            if (!permissionsString.StartsWith(";", StringComparison.Ordinal))
            {
                permissionsString = permissionsString.Insert(0, ";");
            }

			eventLogger.LogDebug("<<< Exiting BuildPermissions(permissions={Permissions},permissionKey={PermissionKey})", permissions, permissionKey);
            return permissionsString;
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByFolder" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByFolder)} instead.")]
        public static partial ArrayList GetPermissionsByFolder()
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByFolder()");
			eventLogger.LogDebug("<<< Exiting GetPermissionsByFolder()");
            return new ArrayList(GetPermissionsByFolderEnumerable(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>()).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByPortalDesktopModule" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByPortalDesktopModule)} instead.")]
        public static partial ArrayList GetPermissionsByPortalDesktopModule()
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByPortalDesktopModule()");
			eventLogger.LogDebug("<<< Exiting GetPermissionsByPortalDesktopModule()");
            return new ArrayList(GetPermissionsByPortalDesktopModuleEnumerable(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>()).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByTab" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByTab)} instead.")]
        public static partial ArrayList GetPermissionsByTab()
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByTab()");
			eventLogger.LogDebug("<<< Exiting GetPermissionsByTab()");
            return new ArrayList(GetPermissionsByTabEnumerable(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>()).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.AddDefinition" />
        public int AddPermission(PermissionInfo permission)
        {
			eventLogger.LogDebug("<<< Exiting AddPermission(permission={Permission})", permission);
            return this.AddPermission((IPermissionDefinitionInfo)permission);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.AddDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int AddPermission(IPermissionDefinitionInfo permission)
        {
            this.eventLogger.AddLog(permission, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.PERMISSION_CREATED);
            var permissionId = Convert.ToInt32(this.dataProvider.AddPermission(
                permission.PermissionCode,
                permission.ModuleDefId,
                permission.PermissionKey,
                permission.PermissionName,
                UserController.Instance.GetCurrentUserInfo().UserID));

            ClearCache();
            return permissionId;
        }

        /// <inheritdoc cref="IPermissionDefinitionService.DeleteDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void DeletePermission(int permissionID)
        {
			eventLogger.LogDebug(">>> Entering DeletePermission(permissionID={PermissionID})", permissionID);
            this.eventLogger.AddLog(
                "PermissionID",
                permissionID.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.PERMISSION_DELETED);
            this.dataProvider.DeletePermission(permissionID);
            ClearCache();
			eventLogger.LogDebug("<<< Exiting DeletePermission(permissionID={PermissionID})", permissionID);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PermissionInfo GetPermission(int permissionID)
        {
			eventLogger.LogDebug(">>> Entering GetPermission(permissionID={PermissionID})", permissionID);
			eventLogger.LogDebug("<<< Exiting GetPermission(permissionID={PermissionID})", permissionID);
            return GetPermissions(this.dataProvider, this.hostSettings).SingleOrDefault(p => ((IPermissionDefinitionInfo)p).PermissionId == permissionID);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByCodeAndKey" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByCodeAndKey)} instead.")]
        public partial ArrayList GetPermissionByCodeAndKey(string permissionCode, string permissionKey)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionByCodeAndKey(permissionCode={PermissionCode},permissionKey={PermissionKey})", permissionCode, permissionKey);
			eventLogger.LogDebug("<<< Exiting GetPermissionByCodeAndKey(permissionCode={PermissionCode},permissionKey={PermissionKey})", permissionCode, permissionKey);
            return new ArrayList(GetPermissionByCodeAndKeyEnumerable(this.dataProvider, this.hostSettings, permissionCode, permissionKey).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByModuleDefId" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByModuleDefId)} instead.")]
        public partial ArrayList GetPermissionsByModuleDefID(int moduleDefID)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByModuleDefID(moduleDefID={ModuleDefID})", moduleDefID);
			eventLogger.LogDebug("<<< Exiting GetPermissionsByModuleDefID(moduleDefID={ModuleDefID})", moduleDefID);
            return new ArrayList(GetPermissionsByModuleDefIdEnumerable(this.dataProvider, this.hostSettings, moduleDefID).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByModule" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByModule)} instead.")]
        public partial ArrayList GetPermissionsByModule(int moduleId, int tabId)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByModule(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
			eventLogger.LogDebug("<<< Exiting GetPermissionsByModule(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            return new ArrayList(GetPermissionsByModuleEnumerable(this.dataProvider, this.hostSettings, moduleId, tabId).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.UpdateDefinition" />
        public void UpdatePermission(PermissionInfo permission)
        {
            this.UpdatePermission((IPermissionDefinitionInfo)permission);
			eventLogger.LogDebug(">>> Entering UpdatePermission(permission={Permission})", permission);
			eventLogger.LogDebug("<<< Exiting UpdatePermission(permission={Permission})", permission);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.UpdateDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void UpdatePermission(IPermissionDefinitionInfo permission)
        {
            this.eventLogger.AddLog(permission, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.PERMISSION_UPDATED);
            this.dataProvider.UpdatePermission(
                permission.PermissionId,
                permission.PermissionCode,
                permission.ModuleDefId,
                permission.PermissionKey,
                permission.PermissionName,
                UserController.Instance.GetCurrentUserInfo().UserID);
            ClearCache();
        }

        public T RemapPermission<T>(T permission, int portalId)
            where T : PermissionInfoBase
        {
			eventLogger.LogDebug(">>> Entering RemapPermission(permission={Permission},portalId={PortalId})", permission, portalId);
            var permissionInfo = this.GetPermissionByCodeAndKey(permission.PermissionCode, permission.PermissionKey).Cast<IPermissionDefinitionInfo>().FirstOrDefault();
            T result = null;
			eventLogger.LogDebug("RemapPermission(permission={Permission},portalId={PortalId}): result → {Result}", permission, portalId, result);
			eventLogger.LogDebug("RemapPermission(permission={Permission},portalId={PortalId}): permissionInfo → {PermissionInfo}", permission, portalId, permissionInfo);

            if (permissionInfo != null)
            {
                int roleID = int.MinValue;
				eventLogger.LogDebug("RemapPermission(permission={Permission},portalId={PortalId}): roleID → {RoleID}", permission, portalId, roleID);
                int userID = int.MinValue;
				eventLogger.LogDebug("RemapPermission(permission={Permission},portalId={PortalId}): userID → {UserID}", permission, portalId, userID);

                if (string.IsNullOrEmpty(permission.RoleName))
                {
                    UserInfo user = UserController.GetUserByName(portalId, permission.Username);
					eventLogger.LogDebug("RemapPermission(permission={Permission},portalId={PortalId}): user → {User}", permission, portalId, user);
                    if (user != null)
                    {
                        userID = user.UserID;
                    }
                }
                else
                {
                    switch (permission.RoleName)
                    {
                        case Globals.glbRoleAllUsersName:
                            roleID = Convert.ToInt32(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
                            break;
                        case Globals.glbRoleUnauthUserName:
                            roleID = Convert.ToInt32(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
                            break;
                        default:
                            RoleInfo role = RoleController.Instance.GetRole(portalId, r => r.RoleName == permission.RoleName);
							eventLogger.LogDebug("RemapPermission(permission={Permission},portalId={PortalId}): role → {Role}", permission, portalId, role);
                            if (role != null)
                            {
                                roleID = role.RoleID;
                            }

                            break;
                    }
                }

                // if role was found add, otherwise ignore
                if (roleID != int.MinValue || userID != int.MinValue)
                {
                    ((IPermissionDefinitionInfo)permission).PermissionId = permissionInfo.PermissionId;
                    if (roleID != int.MinValue)
                    {
                        ((IPermissionInfo)permission).RoleId = roleID;
                    }

                    if (userID != int.MinValue)
                    {
                        ((IPermissionInfo)permission).UserId = userID;
                    }

                    result = permission;
                }
            }

			eventLogger.LogDebug("<<< Exiting RemapPermission(permission={Permission},portalId={PortalId})", permission, portalId);
            return result;
        }

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitions() => GetPermissions(this.dataProvider, this.hostSettings);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByFolder() => GetPermissionsByFolderEnumerable(this.dataProvider, this.hostSettings);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByPortalDesktopModule() => GetPermissionsByPortalDesktopModuleEnumerable(this.dataProvider, this.hostSettings);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByTab() => GetPermissionsByTabEnumerable(this.dataProvider, this.hostSettings);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByCodeAndKey(string permissionCode, string permissionKey) => GetPermissionByCodeAndKeyEnumerable(this.dataProvider, this.hostSettings, permissionCode, permissionKey);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByModuleDefId(int moduleDefId) => GetPermissionsByModuleDefIdEnumerable(this.dataProvider, this.hostSettings, moduleDefId);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByModule(int moduleId, int tabId) => GetPermissionsByModuleEnumerable(this.dataProvider, this.hostSettings, moduleId, tabId);

        /// <inheritdoc />
        int IPermissionDefinitionService.AddDefinition(IPermissionDefinitionInfo permission) => this.AddPermission(permission);

        /// <inheritdoc />
        void IPermissionDefinitionService.DeleteDefinition(IPermissionDefinitionInfo permission) => this.DeletePermission(permission.PermissionId);

        /// <inheritdoc />
        IPermissionDefinitionInfo IPermissionDefinitionService.GetDefinition(int permissionDefinitionId) => this.GetPermission(permissionDefinitionId);

        /// <inheritdoc />
        void IPermissionDefinitionService.UpdateDefinition(IPermissionDefinitionInfo permission) => this.UpdatePermission(permission);

        /// <inheritdoc />
        void IPermissionDefinitionService.ClearCache() => ClearCache();

        private static IEnumerable<PermissionInfo> GetPermissions(DataProvider dataProvider, IHostSettings hostSettings)
        {
			eventLogger.LogDebug(">>> Entering GetPermissions(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
			eventLogger.LogDebug("<<< Exiting GetPermissions(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
            return CBO.GetCachedObject<IEnumerable<PermissionInfo>>(
                hostSettings,
                new CacheItemArgs(
                    DataCache.PermissionsCacheKey,
                    DataCache.PermissionsCacheTimeout,
                    DataCache.PermissionsCachePriority),
                _ => CBO.FillCollection<PermissionInfo>(dataProvider.ExecuteReader("GetPermissions")));
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByFolderEnumerable(DataProvider dataProvider, IHostSettings hostSettings)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByFolderEnumerable(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
			eventLogger.LogDebug("<<< Exiting GetPermissionsByFolderEnumerable(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
            return GetPermissions(dataProvider, hostSettings).Where(p => p.PermissionCode == "SYSTEM_FOLDER");
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByPortalDesktopModuleEnumerable(DataProvider dataProvider, IHostSettings hostSettings)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByPortalDesktopModuleEnumerable(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
			eventLogger.LogDebug("<<< Exiting GetPermissionsByPortalDesktopModuleEnumerable(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
            return GetPermissions(dataProvider, hostSettings).Where(p => p.PermissionCode == "SYSTEM_DESKTOPMODULE");
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByTabEnumerable(DataProvider dataProvider, IHostSettings hostSettings)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByTabEnumerable(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
			eventLogger.LogDebug("<<< Exiting GetPermissionsByTabEnumerable(dataProvider={DataProvider},hostSettings={HostSettings})", dataProvider, hostSettings);
            return GetPermissions(dataProvider, hostSettings).Where(p => p.PermissionCode == "SYSTEM_TAB");
        }

        private static IEnumerable<PermissionInfo> GetPermissionByCodeAndKeyEnumerable(DataProvider dataProvider, IHostSettings hostSettings, string permissionCode, string permissionKey)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionByCodeAndKeyEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},permissionCode={PermissionCode},permissionKey={PermissionKey})", dataProvider, hostSettings, permissionCode, permissionKey);
			eventLogger.LogDebug("<<< Exiting GetPermissionByCodeAndKeyEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},permissionCode={PermissionCode},permissionKey={PermissionKey})", dataProvider, hostSettings, permissionCode, permissionKey);
            return GetPermissions(dataProvider, hostSettings).Where(p => p.PermissionCode.Equals(permissionCode, StringComparison.OrdinalIgnoreCase)
                                                                       && p.PermissionKey.Equals(permissionKey, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByModuleDefIdEnumerable(DataProvider dataProvider, IHostSettings hostSettings, int moduleDefId)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByModuleDefIdEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},moduleDefId={ModuleDefId})", dataProvider, hostSettings, moduleDefId);
			eventLogger.LogDebug("<<< Exiting GetPermissionsByModuleDefIdEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},moduleDefId={ModuleDefId})", dataProvider, hostSettings, moduleDefId);
            return GetPermissions(dataProvider, hostSettings).Where(p => ((IPermissionDefinitionInfo)p).ModuleDefId == moduleDefId);
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByModuleEnumerable(DataProvider dataProvider, IHostSettings hostSettings, int moduleId, int tabId)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsByModuleEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},moduleId={ModuleId},tabId={TabId})", dataProvider, hostSettings, moduleId, tabId);
            var module = ModuleController.Instance.GetModule(moduleId, tabId, false);
            var moduleDefId = module.ModuleDefID;
			eventLogger.LogDebug("GetPermissionsByModuleEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},moduleId={ModuleId},tabId={TabId}): module → {Module}", dataProvider, hostSettings, moduleId, tabId, module);

			eventLogger.LogDebug("<<< Exiting GetPermissionsByModuleEnumerable(dataProvider={DataProvider},hostSettings={HostSettings},moduleId={ModuleId},tabId={TabId})", dataProvider, hostSettings, moduleId, tabId);
            return GetPermissions(dataProvider, hostSettings).Where(p => ((IPermissionDefinitionInfo)p).ModuleDefId == moduleDefId || p.PermissionCode == "SYSTEM_MODULE_DEFINITION");
        }

        private static void ClearCache()
        {
			eventLogger.LogDebug(">>> Entering ClearCache()");
            DataCache.RemoveCache(DataCache.PermissionsCacheKey);
			eventLogger.LogDebug("<<< Exiting ClearCache()");
        }
		eventLogger.LogDebug("<<< Exiting PermissionController()");
    }
}
