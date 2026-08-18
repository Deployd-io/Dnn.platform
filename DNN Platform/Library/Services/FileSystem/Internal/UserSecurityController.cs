// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;

    public class UserSecurityController : ServiceLocator<IUserSecurityController, UserSecurityController>, IUserSecurityController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserSecurityController));
        /// <inheritdoc />
        public bool IsHostAdminUser(int portalId)
        {
			Logger.LogDebug(">>> Entering IsHostAdminUser(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting IsHostAdminUser(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting IsHostAdminUser(portalId={PortalId},userId={UserId})", portalId, userId);
            return this.IsHostAdminUser(portalId, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        /// <inheritdoc />
        public bool IsHostAdminUser(int portalId, int userId)
        {
            if (userId == Null.NullInteger)
            {
                return false;
            }

            var user = UserController.Instance.GetUserById(portalId, userId);
            return user.IsSuperUser || (portalId > Null.NullInteger && user.IsInRole(PortalController.Instance.GetPortal(portalId).AdministratorRoleName));
        }

        /// <inheritdoc />
        public bool HasFolderPermission(IFolderInfo folder, string permissionKey)
        {
			Logger.LogDebug(">>> Entering HasFolderPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
			Logger.LogDebug("<<< Exiting HasFolderPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
            return UserController.Instance.GetCurrentUserInfo().IsSuperUser ||
                   FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
        }

        /// <inheritdoc />
        protected override System.Func<IUserSecurityController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new UserSecurityController();
        }
    }
}
