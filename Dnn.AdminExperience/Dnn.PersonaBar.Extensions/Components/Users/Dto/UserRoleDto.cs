// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    [DataContract]
    public class UserRoleDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "roleId")]
        public int RoleId { get; set; }

        [DataMember(Name = "roleName")]
        public string RoleName { get; set; }

        [DataMember(Name = "startTime")]
        public DateTime StartTime { get; set; }

        [DataMember(Name = "expiresTime")]
        public DateTime ExpiresTime { get; set; }

        [DataMember(Name = "allowExpired")]
        public bool AllowExpired { get; set; }

        [DataMember(Name = "allowDelete")]
        public bool AllowDelete { get; set; }

        [DataMember(Name = "allowOwner")]
        public bool AllowOwner { get; set; }

        public static UserRoleDto FromRoleInfo(PortalSettings portalSettings, UserRoleInfo userRole)
        {
			Logger.LogDebug(">>> Entering FromRoleInfo(portalSettings={PortalSettings},userRole={UserRole})", portalSettings, userRole);
            if (userRole == null)
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting FromRoleInfo(portalSettings={PortalSettings},userRole={UserRole})", portalSettings, userRole);
            return new UserRoleDto()
            {
                Id = userRole.UserRoleID,
                UserId = userRole.UserID,
                DisplayName = userRole.FullName,
                RoleId = userRole.RoleID,
                RoleName = userRole.RoleName,
                StartTime = userRole.EffectiveDate,
                ExpiresTime = userRole.ExpiryDate,
                AllowExpired = AllowExpiredRole(portalSettings, userRole.UserID, userRole.RoleID),
                AllowDelete = RoleController.CanRemoveUserFromRole(portalSettings, userRole.UserID, userRole.RoleID),
                AllowOwner = (userRole.SecurityMode == SecurityMode.SocialGroup) || (userRole.SecurityMode == SecurityMode.Both),
            };
        }

        public UserRoleInfo ToUserRoleInfo()
        {
			Logger.LogDebug(">>> Entering ToUserRoleInfo()");
			Logger.LogDebug("<<< Exiting ToUserRoleInfo()");
            return new UserRoleInfo()
            {
                UserRoleID = this.Id,
                UserID = this.UserId,
                FullName = this.DisplayName,
                RoleID = this.RoleId,
                RoleName = this.RoleName,
                EffectiveDate = this.StartTime,
                ExpiryDate = this.ExpiresTime,
            };
        }

        internal static bool AllowExpiredRole(PortalSettings portalSettings, int userId, int roleId)
        {
			Logger.LogDebug(">>> Entering AllowExpiredRole(portalSettings={PortalSettings},userId={UserId},roleId={RoleId})", portalSettings, userId, roleId);
			Logger.LogDebug("<<< Exiting AllowExpiredRole(portalSettings={PortalSettings},userId={UserId},roleId={RoleId})", portalSettings, userId, roleId);
            return userId != portalSettings.AdministratorId || roleId != portalSettings.AdministratorRoleId;
        }
    }
}
