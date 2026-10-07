// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Users;

    public abstract class RoleProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleProvider));
        // return the provider
        public static RoleProvider Instance()
        {
			Logger.LogError(">>> Entering Instance()");
            Logger.LogWarning("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<RoleProvider>();
        }

        public virtual bool CreateRole(RoleInfo role)
        {
			Logger.LogError(">>> Entering CreateRole(role={Role})", role);
			Logger.LogError("Throwing NotImplementedException from CreateRole(role={Role})", role);
        Logger.LogWarning("<<< Exiting CreateRole(role={Role})", role);
            throw new NotImplementedException();
        }

        public virtual void DeleteRole(RoleInfo role)
        {
			Logger.LogError(">>> Entering DeleteRole(role={Role})", role);
			Logger.LogError("Throwing NotImplementedException from DeleteRole(role={Role})", role);
        Logger.LogWarning("<<< Exiting DeleteRole(role={Role})", role);
            throw new NotImplementedException();
        }

        public abstract ArrayList GetRoles(int portalId);

        public abstract IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy);

        public virtual IDictionary<string, string> GetRoleSettings(int roleId)
        {
            return new Dictionary<string, string>();
        }

        public abstract void UpdateRole(RoleInfo role);

        public virtual void UpdateRoleSettings(RoleInfo role)
        {
		Logger.LogError(">>> Entering UpdateRoleSettings(role={Role})", role);
        Logger.LogWarning("<<< Exiting UpdateRoleSettings(role={Role})", role);
        }

        // Role Groups
        public abstract int CreateRoleGroup(RoleGroupInfo roleGroup);

        public abstract void DeleteRoleGroup(RoleGroupInfo roleGroup);

        public abstract RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId);

        public abstract ArrayList GetRoleGroups(int portalId);

        public abstract void UpdateRoleGroup(RoleGroupInfo roleGroup);

        public abstract bool AddUserToRole(int portalId, UserInfo user, UserRoleInfo userRole);

        public abstract UserRoleInfo GetUserRole(int portalId, int userId, int roleId);

        public abstract ArrayList GetUserRoles(int portalId, string username, string rolename);

        public abstract ArrayList GetUsersByRoleName(int portalId, string roleName);

        public abstract void RemoveUserFromRole(int portalId, UserInfo user, UserRoleInfo userRole);

        public abstract void UpdateUserRole(UserRoleInfo userRole);

        public virtual RoleGroupInfo GetRoleGroupByName(int portalID, string roleGroupName)
        {
			Logger.LogError(">>> Entering GetRoleGroupByName(portalID={PortalID},roleGroupName={RoleGroupName})", portalID, roleGroupName);
			Logger.LogWarning("<<< Exiting GetRoleGroupByName(portalID={PortalID},roleGroupName={RoleGroupName})", portalID, roleGroupName);
            return null;
        }

        public virtual IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
        {
			Logger.LogError(">>> Entering GetUserRoles(user={User},includePrivate={IncludePrivate})", user, includePrivate);
			Logger.LogError("Throwing NotImplementedException from GetUserRoles(user={User},includePrivate={IncludePrivate})", user, includePrivate);
        Logger.LogWarning("<<< Exiting GetUserRoles(user={User},includePrivate={IncludePrivate})", user, includePrivate);
            throw new NotImplementedException();
        }
    }
}
