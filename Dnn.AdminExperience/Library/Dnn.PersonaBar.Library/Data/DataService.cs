// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Data
{
    using System;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;

    public class DataService : IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        protected static readonly DotNetNuke.Data.DataProvider DataProvider = DotNetNuke.Data.DataProvider.Instance();

        /// <inheritdoc />
        public int SavePersonaBarMenu(string identifier, string moduleName, string folderName, string controller, string resourceKey, string path, string link, string cssClass, string iconFile, int parentId, int order, bool allowHost, bool enabled, int currentUserId)
        {
			Logger.LogDebug(">>> Entering SavePersonaBarMenu(identifier={Identifier},moduleName={ModuleName},folderName={FolderName},controller={Controller},resourceKey={ResourceKey},path={Path},link={Link},cssClass={CssClass},iconFile={IconFile},parentId={ParentId},order={Order},allowHost={AllowHost},enabled={Enabled},currentUserId={CurrentUserId})", identifier, moduleName, folderName, controller, resourceKey, path, link, cssClass, iconFile, parentId, order, allowHost, enabled, currentUserId);
			Logger.LogDebug("<<< Exiting SavePersonaBarMenu(identifier={Identifier},moduleName={ModuleName},folderName={FolderName},controller={Controller},resourceKey={ResourceKey},path={Path},link={Link},cssClass={CssClass},iconFile={IconFile},parentId={ParentId},order={Order},allowHost={AllowHost},enabled={Enabled},currentUserId={CurrentUserId})", identifier, moduleName, folderName, controller, resourceKey, path, link, cssClass, iconFile, parentId, order, allowHost, enabled, currentUserId);
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarMenu",
                identifier,
                moduleName,
                folderName,
                controller,
                resourceKey,
                path,
                Null.GetNull(link, DBNull.Value, CultureInfo.InvariantCulture),
                Null.GetNull(cssClass, DBNull.Value, CultureInfo.InvariantCulture),
                Null.GetNull(parentId, DBNull.Value, CultureInfo.InvariantCulture),
                order,
                allowHost,
                enabled,
                currentUserId,
                Null.GetNull(iconFile, DBNull.Value, CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        public IDataReader GetPersonaBarMenu()
        {
			Logger.LogDebug(">>> Entering GetPersonaBarMenu()");
			Logger.LogDebug("<<< Exiting GetPersonaBarMenu()");
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarMenu");
        }

        /// <inheritdoc />
        public void DeletePersonaBarMenuByIdentifier(string identifier)
        {
			Logger.LogDebug(">>> Entering DeletePersonaBarMenuByIdentifier(identifier={Identifier})", identifier);
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuByIdentifier", identifier);
			Logger.LogDebug("<<< Exiting DeletePersonaBarMenuByIdentifier(identifier={Identifier})", identifier);
        }

        /// <inheritdoc />
        public int SavePersonaBarExtension(string identifier, int menuId, string folderName, string controller, string container, string path, int order, bool enabled, int currentUserId)
        {
			Logger.LogDebug(">>> Entering SavePersonaBarExtension(identifier={Identifier},menuId={MenuId},folderName={FolderName},controller={Controller},container={Container},path={Path},order={Order},enabled={Enabled},currentUserId={CurrentUserId})", identifier, menuId, folderName, controller, container, path, order, enabled, currentUserId);
			Logger.LogDebug("<<< Exiting SavePersonaBarExtension(identifier={Identifier},menuId={MenuId},folderName={FolderName},controller={Controller},container={Container},path={Path},order={Order},enabled={Enabled},currentUserId={CurrentUserId})", identifier, menuId, folderName, controller, container, path, order, enabled, currentUserId);
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarExtension",
                identifier,
                menuId,
                folderName,
                controller,
                container,
                path,
                order,
                enabled,
                currentUserId);
        }

        /// <inheritdoc />
        public void DeletePersonaBarExtension(string identifier)
        {
			Logger.LogDebug(">>> Entering DeletePersonaBarExtension(identifier={Identifier})", identifier);
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarExtension", identifier);
			Logger.LogDebug("<<< Exiting DeletePersonaBarExtension(identifier={Identifier})", identifier);
        }

        /// <inheritdoc />
        public IDataReader GetPersonaBarExtensions()
        {
			Logger.LogDebug(">>> Entering GetPersonaBarExtensions()");
			Logger.LogDebug("<<< Exiting GetPersonaBarExtensions()");
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarExtensions");
        }

        /// <inheritdoc />
        public int SavePersonaBarMenuDefaultPermissions(int menuId, string roleNames)
        {
			Logger.LogDebug(">>> Entering SavePersonaBarMenuDefaultPermissions(menuId={MenuId},roleNames={RoleNames})", menuId, roleNames);
			Logger.LogDebug("<<< Exiting SavePersonaBarMenuDefaultPermissions(menuId={MenuId},roleNames={RoleNames})", menuId, roleNames);
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarMenuDefaultPermissions", menuId, roleNames);
        }

        /// <inheritdoc />
        public string GetPersonaBarMenuDefaultPermissions(int menuId)
        {
			Logger.LogDebug(">>> Entering GetPersonaBarMenuDefaultPermissions(menuId={MenuId})", menuId);
			Logger.LogDebug("<<< Exiting GetPersonaBarMenuDefaultPermissions(menuId={MenuId})", menuId);
            return DataProvider.ExecuteScalar<string>("PersonaBar_GetPersonaBarMenuDefaultPermissions", menuId);
        }

        /// <inheritdoc />
        public int SavePersonaBarMenuPermission(int portalId, int menuId, int permissionId, int roleId, int userId, bool allowAccees, int currentUserId)
        {
			Logger.LogDebug(">>> Entering SavePersonaBarMenuPermission(portalId={PortalId},menuId={MenuId},permissionId={PermissionId},roleId={RoleId},userId={UserId},allowAccees={AllowAccees},currentUserId={CurrentUserId})", portalId, menuId, permissionId, roleId, userId, allowAccees, currentUserId);
			Logger.LogDebug("<<< Exiting SavePersonaBarMenuPermission(portalId={PortalId},menuId={MenuId},permissionId={PermissionId},roleId={RoleId},userId={UserId},allowAccees={AllowAccees},currentUserId={CurrentUserId})", portalId, menuId, permissionId, roleId, userId, allowAccees, currentUserId);
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarMenuPermission",
                DataProvider.GetNull(portalId),
                menuId,
                permissionId,
                GetRoleNull(roleId),
                DataProvider.GetNull(userId),
                allowAccees,
                currentUserId);
        }

        /// <inheritdoc />
        public IDataReader GetPersonbaBarMenuPermissionsByPortal(int portalId)
        {
			Logger.LogDebug(">>> Entering GetPersonbaBarMenuPermissionsByPortal(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetPersonbaBarMenuPermissionsByPortal(portalId={PortalId})", portalId);
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarMenuPermissionsByPortal", portalId);
        }

        /// <inheritdoc />
        public void DeletePersonbaBarMenuPermissionsByMenuId(int portalId, int menuId)
        {
			Logger.LogDebug(">>> Entering DeletePersonbaBarMenuPermissionsByMenuId(portalId={PortalId},menuId={MenuId})", portalId, menuId);
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuPermissionsByMenuId", portalId, menuId);
			Logger.LogDebug("<<< Exiting DeletePersonbaBarMenuPermissionsByMenuId(portalId={PortalId},menuId={MenuId})", portalId, menuId);
        }

        /// <inheritdoc />
        public void DeletePersonbaBarMenuPermissionsById(int menuPermissionId)
        {
			Logger.LogDebug(">>> Entering DeletePersonbaBarMenuPermissionsById(menuPermissionId={MenuPermissionId})", menuPermissionId);
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuPermissionById", menuPermissionId);
			Logger.LogDebug("<<< Exiting DeletePersonbaBarMenuPermissionsById(menuPermissionId={MenuPermissionId})", menuPermissionId);
        }

        /// <inheritdoc />
        public int SavePersonaBarPermission(int menuId, string permissionKey, string permissionName, int currentUserId)
        {
			Logger.LogDebug(">>> Entering SavePersonaBarPermission(menuId={MenuId},permissionKey={PermissionKey},permissionName={PermissionName},currentUserId={CurrentUserId})", menuId, permissionKey, permissionName, currentUserId);
			Logger.LogDebug("<<< Exiting SavePersonaBarPermission(menuId={MenuId},permissionKey={PermissionKey},permissionName={PermissionName},currentUserId={CurrentUserId})", menuId, permissionKey, permissionName, currentUserId);
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarPermission",
                Null.GetNull(menuId, DBNull.Value, CultureInfo.InvariantCulture),
                permissionKey,
                permissionName,
                currentUserId);
        }

        /// <inheritdoc />
        public void DeletePersonaBarPermission(int permissionId)
        {
			Logger.LogDebug(">>> Entering DeletePersonaBarPermission(permissionId={PermissionId})", permissionId);
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarPermission", permissionId);
			Logger.LogDebug("<<< Exiting DeletePersonaBarPermission(permissionId={PermissionId})", permissionId);
        }

        /// <inheritdoc />
        public IDataReader GetPersonaBarPermissions()
        {
			Logger.LogDebug(">>> Entering GetPersonaBarPermissions()");
			Logger.LogDebug("<<< Exiting GetPersonaBarPermissions()");
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarPermissions");
        }

        /// <inheritdoc />
        public void UpdateMenuController(string identifier, string controller, int userId)
        {
			Logger.LogDebug(">>> Entering UpdateMenuController(identifier={Identifier},controller={Controller},userId={UserId})", identifier, controller, userId);
            DataProvider.ExecuteNonQuery("PersonaBar_UpdateMenuController", identifier, controller, userId);
			Logger.LogDebug("<<< Exiting UpdateMenuController(identifier={Identifier},controller={Controller},userId={UserId})", identifier, controller, userId);
        }

        private static object GetRoleNull(int roleId)
        {
			Logger.LogDebug(">>> Entering GetRoleNull(roleId={RoleId})", roleId);
            if (roleId.ToString(CultureInfo.InvariantCulture) == "-4")
            {
			Logger.LogDebug("<<< Exiting GetRoleNull(roleId={RoleId})", roleId);
                return DBNull.Value;
            }

            return (object)roleId;
        }
    }
}
