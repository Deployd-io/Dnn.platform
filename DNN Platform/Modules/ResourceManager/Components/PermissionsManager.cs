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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IFolderManager folderManager;
        private readonly IRoleController roleController;
        private readonly IUserController userController;

        /// <summary>Initializes a new instance of the <see cref="PermissionsManager"/> class.</summary>
        public PermissionsManager()
        {
			Logger.LogDebug(">>> Entering PermissionsManager()");
            this.folderManager = FolderManager.Instance;
            this.roleController = RoleController.Instance;
			Logger.LogDebug("PermissionsManager(): this.folderManager → {Arg0}", this.folderManager);
			Logger.LogDebug("PermissionsManager(): roleController → {RoleController}", roleController);
			Logger.LogDebug("PermissionsManager(): this.roleController → {Arg0}", this.roleController);
            this.userController = UserController.Instance;
			Logger.LogDebug("<<< Exiting PermissionsManager()");
			Logger.LogDebug("PermissionsManager(): userController → {UserController}", userController);
			Logger.LogDebug("PermissionsManager(): this.userController → {Arg0}", this.userController);
			Logger.LogDebug("PermissionsManager(): folderManager → {FolderManager}", folderManager);
        }

        /// <inheritdoc />
        public bool HasFolderContentPermission(int folderId, int moduleMode)
        {
			Logger.LogDebug(">>> Entering HasFolderContentPermission(folderId={FolderId},moduleMode={ModuleMode})", folderId, moduleMode);
			Logger.LogDebug("<<< Exiting HasFolderContentPermission(folderId={FolderId},moduleMode={ModuleMode})", folderId, moduleMode);
            return this.HasGroupFolderPublicOrMemberPermission(folderId);
        }

        /// <inheritdoc />
        public bool HasGetFileContentPermission(int folderId)
        {
			Logger.LogDebug(">>> Entering HasGetFileContentPermission(folderId={FolderId})", folderId);
            if (!this.HasGroupFolderPublicOrMemberPermission(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogDebug("HasGetFileContentPermission(folderId={FolderId}): folder → {Folder}", folderId, folder);
			Logger.LogDebug("<<< Exiting HasGetFileContentPermission(folderId={FolderId})", folderId);
            return HasPermission(folder, "READ");
        }

        /// <inheritdoc />
        public bool HasAddFilesPermission(int moduleMode, int folderId)
        {
			Logger.LogDebug(">>> Entering HasAddFilesPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderMemberPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogDebug("HasAddFilesPermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogDebug("<<< Exiting HasAddFilesPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return folder != null && HasPermission(folder, "ADD");
        }

        /// <inheritdoc />
        public bool HasAddFoldersPermission(int moduleMode, int folderId)
        {
			Logger.LogDebug(">>> Entering HasAddFoldersPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User)
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogDebug("HasAddFoldersPermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogDebug("<<< Exiting HasAddFoldersPermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return folder != null && HasPermission(folder, "ADD");
        }

        /// <inheritdoc />
        public bool HasDeletePermission(int moduleMode, int folderId)
        {
			Logger.LogDebug(">>> Entering HasDeletePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogDebug("HasDeletePermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogDebug("<<< Exiting HasDeletePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return FolderPermissionController.CanDeleteFolder((FolderInfo)folder);
        }

        /// <inheritdoc />
        public bool HasManagePermission(int moduleMode, int folderId)
        {
			Logger.LogDebug(">>> Entering HasManagePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            if (!this.HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !this.IsInUserFolder(folderId))
            {
                return false;
            }

            var folder = this.folderManager.GetFolder(folderId);
			Logger.LogDebug("HasManagePermission(moduleMode={ModuleMode},folderId={FolderId}): folder → {Folder}", moduleMode, folderId, folder);
			Logger.LogDebug("<<< Exiting HasManagePermission(moduleMode={ModuleMode},folderId={FolderId})", moduleMode, folderId);
            return FolderPermissionController.CanManageFolder((FolderInfo)folder);
        }

        /// <summary>Checks if the current user has permission on a group folder.</summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <returns>A value indicating whether the user has permission on the group folder.</returns>
        public bool HasGroupFolderMemberPermission(int folderId)
        {
			Logger.LogDebug(">>> Entering HasGroupFolderMemberPermission(folderId={FolderId})", folderId);
            var groupId = Utils.GetFolderGroupId(folderId);
            if (groupId < 0)
			Logger.LogDebug("HasGroupFolderMemberPermission(folderId={FolderId}): groupId → {GroupId}", folderId, groupId);
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting HasGroupFolderMemberPermission(folderId={FolderId})", folderId);
            return this.UserIsGroupMember(groupId);
        }

        /// <inheritdoc />
        protected override Func<IPermissionsManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new PermissionsManager();
        }

        /// <summary>Check if a user has a specific permission key on a folder.</summary>
        /// <param name="folder">The id of the folder to check.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <returns>A value indicating whether the user has the permission key for the folder.</returns>
        private static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermission = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermission && folder != null)
            {
                hasPermission = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermission;
        }

        private bool HasGroupFolderPublicOrMemberPermission(int folderId)
        {
			Logger.LogDebug(">>> Entering HasGroupFolderPublicOrMemberPermission(folderId={FolderId})", folderId);
            var groupId = Utils.GetFolderGroupId(folderId);
            if (groupId < 0)
			Logger.LogDebug("HasGroupFolderPublicOrMemberPermission(folderId={FolderId}): groupId → {GroupId}", folderId, groupId);
            {
                return true;
            }

            var portalId = PortalSettings.Current.PortalId;
            var folderGroup = this.roleController.GetRoleById(portalId, groupId);

			Logger.LogDebug("HasGroupFolderPublicOrMemberPermission(folderId={FolderId}): folderGroup → {FolderGroup}", folderId, folderGroup);
			Logger.LogDebug("<<< Exiting HasGroupFolderPublicOrMemberPermission(folderId={FolderId})", folderId);
            return folderGroup.IsPublic || this.UserIsGroupMember(groupId);
        }

        private bool HasGroupFolderOwnerPermission(int folderId)
        {
			Logger.LogDebug(">>> Entering HasGroupFolderOwnerPermission(folderId={FolderId})", folderId);
            var groupId = Utils.GetFolderGroupId(folderId);
            if (groupId < 0)
			Logger.LogDebug("HasGroupFolderOwnerPermission(folderId={FolderId}): groupId → {GroupId}", folderId, groupId);
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting HasGroupFolderOwnerPermission(folderId={FolderId})", folderId);
            return this.UserIsGroupOwner(groupId);
        }

        private bool UserIsGroupMember(int groupId)
        {
            return this.GetUserRoleInfo(groupId) != null;
        }

        private bool UserIsGroupOwner(int groupId)
        {
            var userRole = this.GetUserRoleInfo(groupId);
            return userRole != null && userRole.IsOwner;
        }

        private bool IsInUserFolder(int folderId)
        {
			Logger.LogDebug(">>> Entering IsInUserFolder(folderId={FolderId})", folderId);
            var user = this.userController.GetCurrentUserInfo();
            var userFolder = this.folderManager.GetUserFolder(user);
			Logger.LogDebug("IsInUserFolder(folderId={FolderId}): user → {User}", folderId, user);
			Logger.LogDebug("IsInUserFolder(folderId={FolderId}): userFolder → {UserFolder}", folderId, userFolder);
			Logger.LogDebug("<<< Exiting IsInUserFolder(folderId={FolderId})", folderId);
            return this.IsUserFolder(folderId, userFolder);
        }

        private bool IsUserFolder(int folderId, IFolderInfo userFolder)
        {
            if (userFolder.FolderID == folderId)
            {
                return true;
            }

            if (userFolder.ParentID != Null.NullInteger)
            {
                var parent = this.folderManager.GetFolder(userFolder.ParentID);
                return this.IsUserFolder(folderId, parent);
            }

            return false;
        }

        private UserRoleInfo GetUserRoleInfo(int groupId)
        {
            var userId = this.userController.GetCurrentUserInfo().UserID;
            var portalId = PortalSettings.Current.PortalId;
            return this.roleController.GetUserRole(portalId, userId, groupId);
        }
    }
}
