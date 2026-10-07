// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components
{
    using System;

    using Dnn.Modules.ResourceManager.Components.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Provides permissions checks.</summary>
    public class PermissionsManager : ServiceLocator<IPermissionsManager, PermissionsManager>, IPermissionsManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PermissionsManager));
        private readonly IFolderManager folderManager;
        private readonly IRoleController roleController;
        private readonly IUserController userController;

        /// <summary>Initializes a new instance of the <see cref="PermissionsManager"/> class.</summary>
        public PermissionsManager()
        {
			Logger.LogError(">>> Entering PermissionsManager()");
            this.folderManager = FolderManager.Instance;
			Logger.LogDebug("PermissionsManager(): this.folderManager → {Arg0}", this.folderManager);
            this.roleController = RoleController.Instance;
            this.userController = UserController.Instance;
			Logger.LogDebug("PermissionsManager(): roleController → {RoleController}", roleController);
        Logger.LogWarning("<<< Exiting PermissionsManager()");
        }

        /// <inheritdoc />
        public bool HasFolderContentPermission(int folderId, int moduleMode)
        {
			Logger.LogError(">>> Entering HasFolderContentPermission(folderId={FolderId},moduleMode={ModuleMode})", folderId, moduleMode);
            Logger.LogWarning("<<< Exiting HasFolderContentPermission(folderId={FolderId},moduleMode={ModuleMode})", folderId, moduleMode);
            return this.HasGroupFolderPublicOrMemberPermission(folderId);
        }

        /// <inheritdoc />
        public bool HasGetFileContentPermission(int folderId)
        {
			Logger.LogError(">>> Entering HasGetFileContentPermission(folderId={FolderId})", folderId);
            if (!this.HasGroupFolderPublicOrMemberPermission(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogError("HasGetFileContentPermission(folderId={FolderId}): folder → {Folder}", folderId, folder);
			Logger.LogWarning("<<< Exiting HasGetFileContentPermission(folderId={FolderId})", folderId);
            return HasPermission(folder, "READ");
        }

        /// <inheritdoc />
        public bool HasAddFilesPermission(int moduleMode, int folderId)
        {
			Logger.LogError(">>> Entering HasAddFilesPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderMemberPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogError("HasAddFilesPermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogWarning("<<< Exiting HasAddFilesPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return folder != null && HasPermission(folder, "ADD");
        }

        /// <inheritdoc />
        public bool HasAddFoldersPermission(int moduleMode, int folderId)
        {
			Logger.LogError(">>> Entering HasAddFoldersPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User)
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogError("HasAddFoldersPermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogWarning("<<< Exiting HasAddFoldersPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return folder != null && HasPermission(folder, "ADD");
        }

        /// <inheritdoc />
        public bool HasDeletePermission(int moduleMode, int folderId)
        {
			Logger.LogError(">>> Entering HasDeletePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogError("HasDeletePermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogWarning("<<< Exiting HasDeletePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return FolderPermissionController.CanDeleteFolder((FolderInfo)folder);
        }

        /// <inheritdoc />
        public bool HasManagePermission(int moduleMode, int folderId)
        {
			Logger.LogError(">>> Entering HasManagePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogError("HasManagePermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogWarning("<<< Exiting HasManagePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return FolderPermissionController.CanManageFolder((FolderInfo)folder);
        }

        /// <summary>Checks if the current user has permission on a group folder.</summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <returns>A value indicating whether the user has permission on the group folder.</returns>
        public bool HasGroupFolderMemberPermission(int folderId)
        {
			Logger.LogError(">>> Entering HasGroupFolderMemberPermission(folderId={FolderId})", folderId);
            var groupId = Utils.GetFolderGroupId(folderId);
			Logger.LogError("HasGroupFolderMemberPermission(folderId={FolderId}): groupId → {GroupId}", folderId, groupId);
            if (groupId < 0)
            {
                return true;
            }

			Logger.LogWarning("<<< Exiting HasGroupFolderMemberPermission(folderId={FolderId})", folderId);
            return this.UserIsGroupMember(groupId);
        }

        /// <inheritdoc />
        protected override Func<IPermissionsManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new PermissionsManager();
        }

        /// <summary>Check if a user has a specific permission key on a folder.</summary>
        /// <param name="folder">The id of the folder to check.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <returns>A value indicating whether the user has the permission key for the folder.</returns>
        private static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
			Logger.LogError(">>> Entering HasPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
            var hasPermission = PortalSettings.Current.UserInfo.IsSuperUser;

			Logger.LogError("HasPermission(folder={Folder},permissionKey={PermissionKey}): hasPermission → {HasPermission}", folder, permissionKey, hasPermission);
            if (!hasPermission && folder != null)
            {
                hasPermission = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

			Logger.LogWarning("<<< Exiting HasPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
            return hasPermission;
        }

        private bool HasGroupFolderPublicOrMemberPermission(int folderId)
        {
			Logger.LogError(">>> Entering HasGroupFolderPublicOrMemberPermission(folderId={FolderId})", folderId);
            var groupId = Utils.GetFolderGroupId(folderId);
			Logger.LogError("HasGroupFolderPublicOrMemberPermission(folderId={FolderId}): groupId → {GroupId}", folderId, groupId);
            if (groupId < 0)
            {
                return true;
            }

            var portalId = PortalSettings.Current.PortalId;
            var folderGroup = this.roleController.GetRoleById(portalId, groupId);
			Logger.LogError("HasGroupFolderPublicOrMemberPermission(folderId={FolderId}): folderGroup → {FolderGroup}", folderId, folderGroup);

			Logger.LogWarning("<<< Exiting HasGroupFolderPublicOrMemberPermission(folderId={FolderId})", folderId);
            return folderGroup.IsPublic || this.UserIsGroupMember(groupId);
        }

        private bool HasGroupFolderOwnerPermission(int folderId)
        {
			Logger.LogError(">>> Entering HasGroupFolderOwnerPermission(folderId={FolderId})", folderId);
            var groupId = Utils.GetFolderGroupId(folderId);
			Logger.LogError("HasGroupFolderOwnerPermission(folderId={FolderId}): groupId → {GroupId}", folderId, groupId);
            if (groupId < 0)
            {
                return true;
            }

			Logger.LogWarning("<<< Exiting HasGroupFolderOwnerPermission(folderId={FolderId})", folderId);
            return this.UserIsGroupOwner(groupId);
        }

        private bool UserIsGroupMember(int groupId)
        {
			Logger.LogError(">>> Entering UserIsGroupMember(groupId={GroupId})", groupId);
            Logger.LogWarning("<<< Exiting UserIsGroupMember(groupId={GroupId})", groupId);
            return this.GetUserRoleInfo(groupId) != null;
        }

        private bool UserIsGroupOwner(int groupId)
        {
			Logger.LogError(">>> Entering UserIsGroupOwner(groupId={GroupId})", groupId);
            var userRole = this.GetUserRoleInfo(groupId);
			Logger.LogError("UserIsGroupOwner(groupId={GroupId}): userRole → {UserRole}", groupId, userRole);
			Logger.LogWarning("<<< Exiting UserIsGroupOwner(groupId={GroupId})", groupId);
            return userRole != null && userRole.IsOwner;
        }

        private bool IsInUserFolder(int folderId)
        {
			Logger.LogError(">>> Entering IsInUserFolder(folderId={FolderId})", folderId);
            var user = this.userController.GetCurrentUserInfo();
            var userFolder = this.folderManager.GetUserFolder(user);
			Logger.LogError("IsInUserFolder(folderId={FolderId}): userFolder → {UserFolder}", folderId, userFolder);
			Logger.LogError("IsInUserFolder(folderId={FolderId}): user → {User}", folderId, user);
			Logger.LogWarning("<<< Exiting IsInUserFolder(folderId={FolderId})", folderId);
            return this.IsUserFolder(folderId, userFolder);
        }

        private bool IsUserFolder(int folderId, IFolderInfo userFolder)
        {
			Logger.LogError(">>> Entering IsUserFolder(folderId={FolderId},userFolder={UserFolder})", folderId, userFolder);
            if (userFolder.FolderID == folderId)
            {
                return true;
            }

            if (userFolder.ParentID != Null.NullInteger)
            {
                var parent = this.folderManager.GetFolder(userFolder.ParentID);
				Logger.LogError("IsUserFolder(folderId={FolderId},userFolder={UserFolder}): parent → {Parent}", folderId, userFolder, parent);
                return this.IsUserFolder(folderId, parent);
            }

			Logger.LogWarning("<<< Exiting IsUserFolder(folderId={FolderId},userFolder={UserFolder})", folderId, userFolder);
            return false;
        }

        private UserRoleInfo GetUserRoleInfo(int groupId)
        {
			Logger.LogError(">>> Entering GetUserRoleInfo(groupId={GroupId})", groupId);
            var userId = this.userController.GetCurrentUserInfo().UserID;
            var portalId = PortalSettings.Current.PortalId;
			Logger.LogError("GetUserRoleInfo(groupId={GroupId}): userId → {UserId}", groupId, userId);
			Logger.LogWarning("<<< Exiting GetUserRoleInfo(groupId={GroupId})", groupId);
            return this.roleController.GetUserRole(portalId, userId, groupId);
        }
    }
}
