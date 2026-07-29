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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly PermissionProvider Provider = PermissionProvider.Instance();

        /// <summary>Returns a list with all roles with implicit permissions on Folders.</summary>
        /// <param name="portalId">The Portal ID where the Roles are.</param>
        /// <returns>A List with the implicit roles.</returns>
        public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
        {
			Logger.LogDebug(">>> Entering ImplicitRoles(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting ImplicitRoles(portalId={PortalId})", portalId);
            return Provider.ImplicitRolesForPages(portalId);
        }

        /// <summary>Returns a flag indicating whether the current user can add a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAddFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanAddFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanAddFolder(folder={Folder})", folder);
            return Provider.CanAddFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can administer a folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanAdminFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanAdminFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanAdminFolder(folder={Folder})", folder);
            return Provider.CanAdminFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can browse the folder.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanBrowseFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanBrowseFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanBrowseFolder(folder={Folder})", folder);
            return Provider.CanBrowseFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can copy a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanCopyFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanCopyFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanCopyFolder(folder={Folder})", folder);
            return Provider.CanCopyFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can delete a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanDeleteFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanDeleteFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanDeleteFolder(folder={Folder})", folder);
            return Provider.CanDeleteFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can manage a folder's settings.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanManageFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanManageFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanManageFolder(folder={Folder})", folder);
            return Provider.CanManageFolder(folder);
        }

        /// <summary>Returns a flag indicating whether the current user can view a folder or file.</summary>
        /// <param name="folder">The page.</param>
        /// <returns>A flag indicating whether the user has permission.</returns>
        public static bool CanViewFolder(FolderInfo folder)
        {
			Logger.LogDebug(">>> Entering CanViewFolder(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting CanViewFolder(folder={Folder})", folder);
            return Provider.CanViewFolder(folder);
        }

        public static void DeleteFolderPermissionsByUser(UserInfo objUser)
        {
			Logger.LogDebug(">>> Entering DeleteFolderPermissionsByUser(objUser={ObjUser})", objUser);
            Provider.DeleteFolderPermissionsByUser(objUser);
            ClearPermissionCache(objUser.PortalID);
			Logger.LogDebug("<<< Exiting DeleteFolderPermissionsByUser(objUser={ObjUser})", objUser);
        }

        public static FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int portalID, string folder)
        {
			Logger.LogDebug(">>> Entering GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder})", portalID, folder);
			Logger.LogDebug("<<< Exiting GetFolderPermissionsCollectionByFolder(portalID={PortalID},folder={Folder})", portalID, folder);
            return Provider.GetFolderPermissionsCollectionByFolder(portalID, folder);
        }

        public static bool HasFolderPermission(int portalId, string folderPath, string permissionKey)
        {
			Logger.LogDebug(">>> Entering HasFolderPermission(portalId={PortalId},folderPath={FolderPath},permissionKey={PermissionKey})", portalId, folderPath, permissionKey);
			Logger.LogDebug("<<< Exiting HasFolderPermission(portalId={PortalId},folderPath={FolderPath},permissionKey={PermissionKey})", portalId, folderPath, permissionKey);
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

			Logger.LogDebug("<<< Exiting HasFolderPermission(objFolderPermissions={ObjFolderPermissions},permissionKey={PermissionKey})", objFolderPermissions, permissionKey);
            return hasPermission;
        }

        /// <summary>Copies the permissions to subfolders.</summary>
        /// <param name="folder">The parent folder.</param>
        /// <param name="newPermissions">The new permissions.</param>
        public static void CopyPermissionsToSubfolders(IFolderInfo folder, FolderPermissionCollection newPermissions)
        {
			Logger.LogDebug(">>> Entering CopyPermissionsToSubfolders(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
            bool clearCache = CopyPermissionsToSubfoldersRecursive(folder, newPermissions);
            if (clearCache)
			Logger.LogDebug("CopyPermissionsToSubfolders(folder={Folder},newPermissions={NewPermissions}): clearCache → {ClearCache}", folder, newPermissions, clearCache);
            {
                DataCache.ClearFolderCache(folder.PortalID);
            }
			Logger.LogDebug("<<< Exiting CopyPermissionsToSubfolders(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
        }

        /// <summary>SaveFolderPermissions updates a Folder's permissions.</summary>
        /// <param name="folder">The Folder to update.</param>
        public static void SaveFolderPermissions(FolderInfo folder)
        {
            SaveFolderPermissions((IFolderInfo)folder);
			Logger.LogDebug(">>> Entering SaveFolderPermissions(folder={Folder})", folder);
			Logger.LogDebug("<<< Exiting SaveFolderPermissions(folder={Folder})", folder);
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
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IFolderPermissionController>();
        }

        private static void ClearPermissionCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearPermissionCache(portalId={PortalId})", portalId);
            DataCache.ClearFolderPermissionsCache(portalId);
            DataCache.ClearCache(string.Format(CultureInfo.CurrentCulture, "Folders|{0}|", portalId));
            DataCache.ClearFolderCache(portalId);
			Logger.LogDebug("<<< Exiting ClearPermissionCache(portalId={PortalId})", portalId);
        }

        private static bool CopyPermissionsToSubfoldersRecursive(IFolderInfo folder, FolderPermissionCollection newPermissions)
        {
			Logger.LogDebug(">>> Entering CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
            bool clearCache = Null.NullBoolean;
            IEnumerable<IFolderInfo> childFolders = FolderManager.Instance.GetFolders(folder);
			Logger.LogDebug("CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions}): clearCache → {ClearCache}", folder, newPermissions, clearCache);
			Logger.LogDebug("CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions}): childFolders → {ChildFolders}", folder, newPermissions, childFolders);
            foreach (var f in childFolders)
            {
                if (CanAdminFolder((FolderInfo)f))
                {
                    f.FolderPermissions.Clear();
                    f.FolderPermissions.AddRange(newPermissions);
                    SaveFolderPermissions(f);
                    clearCache = true;
                }

                clearCache = CopyPermissionsToSubfoldersRecursive(f, newPermissions) || clearCache;
            }

			Logger.LogDebug("<<< Exiting CopyPermissionsToSubfoldersRecursive(folder={Folder},newPermissions={NewPermissions})", folder, newPermissions);
            return clearCache;
        }
    }
}
