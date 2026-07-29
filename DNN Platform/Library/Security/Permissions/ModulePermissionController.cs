// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    /// <summary>ModulePermissionController provides the Business Layer for Module Permissions.</summary>
    public class ModulePermissionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly PermissionProvider Provider = PermissionProvider.Instance();

        /// <summary>Returns a list with all roles with implicit permissions on Modules.</summary>
        /// <param name="portalId">The Portal Id where the Roles are.</param>
        /// <returns>A List with the implicit roles.</returns>
        public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
        {
			Logger.LogDebug(">>> Entering ImplicitRoles(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting ImplicitRoles(portalId={PortalId})", portalId);
            return Provider.ImplicitRolesForPages(portalId);
        }

        /// <summary>Returns a flag indicating whether the current user can administer a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAdminModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanAdminModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanAdminModule(module={Module})", module);
            return Provider.CanAdminModule(module);
        }

        /// <summary>Returns a flag indicating whether the current user can delete a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanDeleteModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanDeleteModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanDeleteModule(module={Module})", module);
            return Provider.CanDeleteModule(module);
        }

        /// <summary>Returns a flag indicating whether the current user can edit module content.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanEditModuleContent(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanEditModuleContent(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanEditModuleContent(module={Module})", module);
            return Provider.CanEditModuleContent(module);
        }

        /// <summary>Returns a flag indicating whether the current user can export a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanExportModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanExportModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanExportModule(module={Module})", module);
            return Provider.CanExportModule(module);
        }

        /// <summary>Returns a flag indicating whether the current user can import a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanImportModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanImportModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanImportModule(module={Module})", module);
            return Provider.CanImportModule(module);
        }

        /// <summary>Returns a flag indicating whether the current user can manage a module's settings.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanManageModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanManageModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanManageModule(module={Module})", module);
            return Provider.CanManageModule(module);
        }

        /// <summary>Returns a flag indicating whether the current user can view a module.</summary>
        /// <param name="module">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanViewModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CanViewModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting CanViewModule(module={Module})", module);
            return Provider.CanViewModule(module);
        }

        /// <summary>DeleteModulePermissionsByUser deletes a user's Module Permission in the Database.</summary>
        /// <param name="user">The user.</param>
        public static void DeleteModulePermissionsByUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering DeleteModulePermissionsByUser(user={User})", user);
            Provider.DeleteModulePermissionsByUser(user);
            DataCache.ClearModulePermissionsCachesByPortal(user.PortalID);
			Logger.LogDebug("<<< Exiting DeleteModulePermissionsByUser(user={User})", user);
        }

        /// <summary>GetModulePermissions gets a ModulePermissionCollection.</summary>
        /// <param name="moduleId">The ID of the module.</param>
        /// <param name="tabId">The ID of the tab.</param>
        /// <returns>A <see cref="ModulePermissionCollection"/> with the module permissions, or an empty <see cref="ModulePermissionCollection"/> if the module wasn't found.</returns>
        public static ModulePermissionCollection GetModulePermissions(int moduleId, int tabId)
        {
			Logger.LogDebug(">>> Entering GetModulePermissions(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
			Logger.LogDebug("<<< Exiting GetModulePermissions(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            return Provider.GetModulePermissions(moduleId, tabId);
        }

        /// <summary>HasModulePermission checks whether the current user has a specific Module Permission.</summary>
        /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE", this will return
        /// true if the user has any one of the permissions.</remarks>
        /// <param name="modulePermissions">The Permissions for the Module.</param>
        /// <param name="permissionKey">The Permission to check.</param>
        /// <returns><see langword="true"/> if the current user has the requested permission, otherwise <see langword="false"/>.</returns>
        public static bool HasModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
			Logger.LogDebug(">>> Entering HasModulePermission(modulePermissions={ModulePermissions},permissionKey={PermissionKey})", modulePermissions, permissionKey);
			Logger.LogDebug("<<< Exiting HasModulePermission(modulePermissions={ModulePermissions},permissionKey={PermissionKey})", modulePermissions, permissionKey);
            return Provider.HasModulePermission(modulePermissions, permissionKey);
        }

        /// <summary>Determines if user has the necessary permissions to access an item with the designated AccessLevel.</summary>
        /// <param name="accessLevel">The SecurityAccessLevel required to access a portal module or module action.</param>
        /// <param name="permissionKey">If Security Access is Edit the permissionKey is the actual "edit" permission required.</param>
        /// <param name="moduleConfiguration">The ModuleInfo object for the associated module.</param>
        /// <returns>A boolean value indicating if the user has the necessary permissions.</returns>
        /// <remarks>Every module control and module action has an associated permission level.  This
        /// function determines whether the user represented by UserName has sufficient permissions, as
        /// determined by the PortalSettings and ModuleSettings, to access a resource with the
        /// designated AccessLevel.</remarks>
        public static bool HasModuleAccess(SecurityAccessLevel accessLevel, string permissionKey, ModuleInfo moduleConfiguration)
        {
			Logger.LogDebug(">>> Entering HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration})", accessLevel, permissionKey, moduleConfiguration);
			Logger.LogDebug("<<< Exiting HasModuleAccess(accessLevel={AccessLevel},permissionKey={PermissionKey},moduleConfiguration={ModuleConfiguration})", accessLevel, permissionKey, moduleConfiguration);
            return Provider.HasModuleAccess(accessLevel, permissionKey, moduleConfiguration);
        }

        /// <summary>SaveModulePermissions updates a Module's permissions.</summary>
        /// <param name="module">The Module to update.</param>
        public static void SaveModulePermissions(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering SaveModulePermissions(module={Module})", module);
            Provider.SaveModulePermissions(module);
            DataCache.ClearModulePermissionsCache(module.TabID);
			Logger.LogDebug("<<< Exiting SaveModulePermissions(module={Module})", module);
        }

        private static void ClearPermissionCache(int moduleId)
        {
			Logger.LogDebug(">>> Entering ClearPermissionCache(moduleId={ModuleId})", moduleId);
            ModuleInfo objModule = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
            DataCache.ClearModulePermissionsCache(objModule.TabID);
			Logger.LogDebug("<<< Exiting ClearPermissionCache(moduleId={ModuleId})", moduleId);
			Logger.LogDebug("ClearPermissionCache(moduleId={ModuleId}): objModule → {ObjModule}", moduleId, objModule);
        }
    }
}
