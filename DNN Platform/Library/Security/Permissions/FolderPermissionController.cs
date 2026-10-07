// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="IFolderPermissionController"/> implementation.</summary>
    public partial class FolderPermissionController : ServiceLocator<IFolderPermissionController, FolderPermissionController>, IFolderPermissionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderPermissionController));
        private static readonly PermissionProvider Provider = PermissionProvider.Instance();

        /// <summary>Returns a list with all roles with implicit permissions on Folders.</summary>
        /// <param name="portalId">The Portal ID where the Roles are.</param>
        /// <returns>A List with the implicit roles.</returns>
        public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
        {
			Logger.LogError(">>> Entering ImplicitRoles(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting ImplicitRoles(portalId={PortalId})", portalId);
            return Provider.ImplicitRolesForPages(portalId);
        }

        /// <summary>Returns a flag indicating whether the current user can add a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAddFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanAddFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanAddFolder(folder={Folder})", folder);
            return Provider.CanAddFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can administer a folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAdminFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanAdminFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanAdminFolder(folder={Folder})", folder);
            return Provider.CanAdminFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can browse the folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanBrowseFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanBrowseFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanBrowseFolder(folder={Folder})", folder);
            return Provider.CanBrowseFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can copy a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanCopyFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanCopyFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanCopyFolder(folder={Folder})", folder);
            return Provider.CanCopyFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can delete a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanDeleteFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanDeleteFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanDeleteFolder(folder={Folder})", folder);
            return Provider.CanDeleteFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can manage a folder's settings.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanManageFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanManageFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanManageFolder(folder={Folder})", folder);
            return Provider.CanManageFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can view a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanViewFolder(FolderInfo folder)
        {
			Logger.LogError(">>> Entering CanViewFolder(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting CanViewFolder(folder={Folder})", folder);
            return Provider.CanViewFolder(folder);
        }

        public static void DeleteFolderPermissionsByUser(UserInfo objUser)
        {
			Logger.LogError(">>> Entering DeleteFolderPermissionsByUser(objUser={ObjUser})", objUser);
            Provider.DeleteFolderPermissionsByUser(objUser);
            ClearPermissionCache(objUser.PortalID);
			Logger.LogWarning("<<< Exiting DeleteFolderPermissionsByUser(objUser={ObjUser})", objUser);
        }

        public static FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int portalID, string folder)
        {
			Logger.LogError(">>> Entering GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder})", portalID, folder);
            Logger.LogWarning("<<< Exiting GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder})", portalID, folder);
            return Provider.GetFolderPermissionsCollectionByFolder(portalID, folder);
        }

        public static bool HasFolderPermission(int portalId, string folderPath, string permissionKey)
        {
			Logger.LogError(">>> Entering HasFolderPermission(portalId={PortalId},folderPath={FolderPath},permissionKey={PermissionKey})", portalId, folderPath, permissionKey);
			Logger.LogWarning("<<< Exiting HasFolderPermission(portalId={PortalId},folderPath={FolderPath},permissionKey={PermissionKey})", portalId, folderPath, permissionKey);
			Logger.LogWarning("<<< Exiting HasFolderPermission(objFolderPermissions={ObjFolderPermissions},permissionKey={PermissionKey})", objFolderPermissions, permissionKey);
            return HasFolderPermission(GetFolderPermissionsCollectionByFolder(portalId, folderPath), permissionKey);
        }

        public static bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string permissionKey)
        {
            bool hasPermission = Provider.HasFolderPermission(objFolderPermissions, "WRITE");
            if (!hasPermission)
            {
                if (permissionKey.Contains(",", StringComparison.Ordinal))
                {
                    foreach (var permission in permissionKey.Split(','))
                    {
                        if (Provider.HasFolderPermission(objFolderPermissions, permission))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }
                else
                {
                    hasPermission = Provider.HasFolderPermission(objFolderPermissions, permissionKey);
                }
            }

            return hasPermission;
        }

        /// <summary>Copies the permissions to subfolders.</summary>
        /// <param name="folder">The parent folder.</param>
        /// <param name="newPermissions">The new permissions.</param>
        public static void CopyPermissionsToSubfolders(IFolderInfo folder, FolderPermissionCollection newPermissions)
        {
			Logger.LogError(">>> Entering CopyPermissionsToSubfolders(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
            bool clearCache = CopyPermissionsToSubfoldersRecursive(folder, newPermissions);
			Logger.LogError("CopyPermissionsToSubfolders(folder={Folder},newPermissions={NewPermissions}): clearCache → {ClearCache}", folder, newPermissions, clearCache);
            if (clearCache)
            {
                DataCache.ClearFolderCache(folder.PortalID);
            }
			Logger.LogWarning("<<< Exiting CopyPermissionsToSubfolders(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
        }

        /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
        /// <param name="folder">The Folder to update.</param>
        public static void SaveFolderPermissions(FolderInfo folder)
        {
            SaveFolderPermissions((IFolderInfo)folder);
			Logger.LogError(">>> Entering SaveFolderPermissions(folder={Folder})", folder);
			Logger.LogWarning("<<< Exiting SaveFolderPermissions(folder={Folder})", folder);
        }

        /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
        /// <param name="folder">The Folder to update.</param>
        public static void SaveFolderPermissions(IFolderInfo folder)
        {
            Provider.SaveFolderPermissions(folder);
            ClearPermissionCache(folder.PortalID);
        }

        /// <summary>Returns a flag indicating whether the current user can add a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        bool IFolderPermissionController.CanAddFolder(IFolderInfo folder)
        {
            return Provider.CanAddFolder((FolderInfo)folder);
        }

        /// <summary>Returns a flag indicating whether the current user can administer a folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        bool IFolderPermissionController.CanAdminFolder(IFolderInfo folder)
        {
            return Provider.CanAdminFolder((FolderInfo)folder);
        }

        /// <summary>Returns a flag indicating whether the current user can view a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        bool IFolderPermissionController.CanViewFolder(IFolderInfo folder)
        {
            return Provider.CanViewFolder((FolderInfo)folder);
        }

        /// <inheritdoc />
        protected override Func<IFolderPermissionController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IFolderPermissionController>();
        }

        private static void ClearPermissionCache(int portalId)
        {
			Logger.LogError(">>> Entering ClearPermissionCache(portalId={PortalId})", portalId);
            DataCache.ClearFolderPermissionsCache(portalId);
            DataCache.ClearCache(string.Format(CultureInfo.CurrentCulture, "Folders|{0}|", portalId));
            DataCache.ClearFolderCache(portalId);
			Logger.LogWarning("<<< Exiting ClearPermissionCache(portalId={PortalId})", portalId);
        }

        private static bool CopyPermissionsToSubfoldersRecursive(IFolderInfo folder, FolderPermissionCollection newPermissions)
        {
			Logger.LogError(">>> Entering CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
            bool clearCache = Null.NullBoolean;
            IEnumerable<IFolderInfo> childFolders = FolderManager.Instance.GetFolders(folder);
			Logger.LogError("CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions}): clearCache → {ClearCache}", folder, newPermissions, clearCache);
            foreach (var f in childFolders)
            {
				Logger.LogError("CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions}): childFolders → {ChildFolders}", folder, newPermissions, childFolders);
                if (CanAdminFolder((FolderInfo)f))
                {
                    f.FolderPermissions.Clear();
                    f.FolderPermissions.AddRange(newPermissions);
                    SaveFolderPermissions(f);
                    clearCache = true;
                }

                clearCache = CopyPermissionsToSubfoldersRecursive(f, newPermissions) || clearCache;
            }

			Logger.LogWarning("<<< Exiting CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
            return clearCache;
        }
    }
}
