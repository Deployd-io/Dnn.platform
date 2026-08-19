// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The DNNRoleProvider overrides the default MembershipProvider to provide
    /// a purely DNN Membership Component implementation.
    /// </summary>
    public class DNNRoleProvider(IHostSettings hostSettings, DataProvider dataProvider, IUserController userController) : RoleProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNRoleProvider));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();

        /// <summary>Initializes a new instance of the <see cref="DNNRoleProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DNNRoleProvider()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering DNNRoleProvider()");
        }
		Logger.LogDebug("<<< Exiting DNNRoleProvider()");

        /// <summary>CreateRole persists a Role to the Data Store.</summary>
        /// <param name="role">The role to persist to the Data Store.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        public override bool CreateRole(RoleInfo role)
        {
			Logger.LogDebug(">>> Entering CreateRole(role={Role})", role);
            Requires.NotNegative("PortalId", role.PortalID);

            try
            {
                role.RoleID =
                    Convert.ToInt32(this.dataProvider.AddRole(
                        role.PortalID,
                        role.RoleGroupID,
                        role.RoleName.Trim(),
                        (role.Description ?? string.Empty).Trim(),
                        role.ServiceFee,
                        role.BillingPeriod.ToString(CultureInfo.InvariantCulture),
                        role.BillingFrequency,
                        role.TrialFee,
                        role.TrialPeriod,
                        role.TrialFrequency,
                        role.IsPublic,
                        role.AutoAssignment,
                        role.RSVPCode,
                        role.IconFile,
                        this.userController.GetCurrentUserInfo().UserID,
                        (int)role.Status,
                        (int)role.SecurityMode,
                        role.IsSystemRole));
            }
            catch (SqlException e)
            {
				Logger.LogError("Exception in CreateRole(role={Role}): {Arg1}", role, e.Message);
			Logger.LogDebug("<<< Exiting CreateRole(role={Role})", role);
                throw new ArgumentException(e.ToString());
            }

            return true;
        }

        /// <summary>DeleteRole deletes a Role from the Data Store.</summary>
        /// <param name="role">The role to delete from the Data Store.</param>
        public override void DeleteRole(RoleInfo role)
        {
			Logger.LogDebug(">>> Entering DeleteRole(role={Role})", role);
            this.dataProvider.DeleteRole(role.RoleID);
			Logger.LogDebug("<<< Exiting DeleteRole(role={Role})", role);
        }

        /// <summary>Get the roles for a portal.</summary>
        /// <param name="portalId">ID of the portal (If -1 all roles for all portals are
        /// retrieved).</param>
        /// <returns>An ArrayList of RoleInfo objects.</returns>
        public override ArrayList GetRoles(int portalId)
        {
			Logger.LogDebug(">>> Entering GetRoles(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetRoles(portalId={PortalId})", portalId);
            return CBO.FillCollection(
                portalId == Null.NullInteger
                    ? this.dataProvider.GetRoles()
                    : this.dataProvider.GetPortalRoles(portalId),
                typeof(RoleInfo));
        }

        /// <inheritdoc />
        public override IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy)
        {
			Logger.LogDebug(">>> Entering GetRolesBasicSearch(portalID={PortalID},pageSize={PageSize},filterBy={FilterBy})", portalID, pageSize, filterBy);
			Logger.LogDebug("<<< Exiting GetRolesBasicSearch(portalID={PortalID},pageSize={PageSize},filterBy={FilterBy})", portalID, pageSize, filterBy);
            return CBO.FillCollection<RoleInfo>(this.dataProvider.GetRolesBasicSearch(portalID, -1, pageSize, filterBy));
        }

        /// <inheritdoc />
        public override IDictionary<string, string> GetRoleSettings(int roleId)
        {
            var settings = new Dictionary<string, string>();
            using var dr = this.dataProvider.GetRoleSettings(roleId);
            while (dr.Read())
            {
                settings.Add(dr["SettingName"].ToString(), dr["SettingValue"].ToString());
            }

            dr.Close();

            return settings;
        }

        /// <summary>Update a role.</summary>
        /// <param name="role">The role to update.</param>
        public override void UpdateRole(RoleInfo role)
        {
			Logger.LogDebug(">>> Entering UpdateRole(role={Role})", role);
            this.dataProvider.UpdateRole(
                role.RoleID,
                role.RoleGroupID,
                role.RoleName.Trim(),
                (role.Description ?? string.Empty).Trim(),
                role.ServiceFee,
                role.BillingPeriod.ToString(CultureInfo.InvariantCulture),
                role.BillingFrequency,
                role.TrialFee,
                role.TrialPeriod,
                role.TrialFrequency,
                role.IsPublic,
                role.AutoAssignment,
                role.RSVPCode,
                role.IconFile,
                this.userController.GetCurrentUserInfo().UserID,
                (int)role.Status,
                (int)role.SecurityMode,
                role.IsSystemRole);
				Logger.LogDebug("<<< Exiting UpdateRole(role={Role})", role);
        }

        /// <summary>Update the role settings for a role.</summary>
        /// <param name="role">The role to update.</param>
        public override void UpdateRoleSettings(RoleInfo role)
        {
			Logger.LogDebug(">>> Entering UpdateRoleSettings(role={Role})", role);
            var currentSettings = this.GetRoleSettings(role.RoleID);

			Logger.LogDebug("UpdateRoleSettings(role={Role}): currentSettings → {CurrentSettings}", role, currentSettings);
            foreach (var setting in role.Settings)
            {
                if (!currentSettings.TryGetValue(setting.Key, out var settingValue) || settingValue != setting.Value)
                {
                    this.dataProvider.UpdateRoleSetting(role.RoleID, setting.Key, setting.Value, this.userController.GetCurrentUserInfo().UserID);
                }
            }
			Logger.LogDebug("<<< Exiting UpdateRoleSettings(role={Role})", role);
        }

        /// <summary>AddUserToRole adds a User to a Role.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="user">The user to add.</param>
        /// <param name="userRole">The role to add the user to.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        public override bool AddUserToRole(int portalId, UserInfo user, UserRoleInfo userRole)
        {
			Logger.LogDebug(">>> Entering AddUserToRole(portalId={PortalId},user={User},userRole={UserRole})", portalId, user, userRole);
            bool createStatus = true;
            try
			Logger.LogDebug("AddUserToRole(portalId={PortalId},user={User},userRole={UserRole}): createStatus → {CreateStatus}", portalId, user, userRole, createStatus);
            {
                // Add UserRole to DNN
                this.AddDNNUserRole(userRole);
            }
            catch (Exception exc)
            {
                // Clear User (duplicate User information)
                Logger.Error(exc);

                createStatus = false;
            }

			Logger.LogDebug("<<< Exiting AddUserToRole(portalId={PortalId},user={User},userRole={UserRole})", portalId, user, userRole);
            return createStatus;
        }

        /// <summary>GetUserRole gets a User/Role object from the Data Store.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="userId">The ID of the User.</param>
        /// <param name="roleId">The ID of the Role.</param>
        /// <returns>The UserRoleInfo object.</returns>
        public override UserRoleInfo GetUserRole(int portalId, int userId, int roleId)
        {
			Logger.LogDebug(">>> Entering GetUserRole(portalId={PortalId},userId={UserId},roleId={RoleId})", portalId, userId, roleId);
			Logger.LogDebug("<<< Exiting GetUserRole(portalId={PortalId},userId={UserId},roleId={RoleId})", portalId, userId, roleId);
            return CBO.FillObject<UserRoleInfo>(this.dataProvider.GetUserRole(portalId, userId, roleId));
        }

        /// <summary>Gets a list of UserRoles for the user.</summary>
        /// <param name="user">A UserInfo object representing the user.</param>
        /// <param name="includePrivate">Include private roles.</param>
        /// <returns>A list of UserRoleInfo objects.</returns>
        public override IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
        {
			Logger.LogDebug(">>> Entering GetUserRoles(user={User},includePrivate={IncludePrivate})", user, includePrivate);
            Requires.NotNull("user", user);

			Logger.LogDebug("<<< Exiting GetUserRoles(user={User},includePrivate={IncludePrivate})", user, includePrivate);
            return CBO.FillCollection<UserRoleInfo>(includePrivate
                    ? this.dataProvider.GetUserRoles(user.PortalID, user.UserID)
			Logger.LogDebug("<<< Exiting GetUserRoles(portalId={PortalId},userName={UserName},roleName={RoleName})", portalId, userName, roleName);
                    : this.dataProvider.GetServices(user.PortalID, user.UserID));
        }

        /// <summary>GetUserRoles gets a collection of User/Role objects from the Data Store.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="userName">The user to fetch roles for.</param>
        /// <param name="roleName">The role to fetch users for.</param>
        /// <returns>An ArrayList of UserRoleInfo objects.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override ArrayList GetUserRoles(int portalId, string userName, string roleName)
        {
            return CBO.FillCollection(this.dataProvider.GetUserRolesByUsername(portalId, userName, roleName), typeof(UserRoleInfo));
        }

        /// <summary>Get the users in a role (as User objects).</summary>
        /// <param name="portalId">ID of the portal (If -1 all roles for all portals are retrieved).</param>
        /// <param name="roleName">The role to fetch users for.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        public override ArrayList GetUsersByRoleName(int portalId, string roleName)
        {
			Logger.LogDebug(">>> Entering GetUsersByRoleName(portalId={PortalId},roleName={RoleName})", portalId, roleName);
			Logger.LogDebug("<<< Exiting GetUsersByRoleName(portalId={PortalId},roleName={RoleName})", portalId, roleName);
            return AspNetMembershipProvider.FillUserCollection(portalId, this.dataProvider.GetUsersByRolename(portalId, roleName));
        }

        /// <summary>Remove a User from a Role.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="user">The user to remove.</param>
        /// <param name="userRole">The role to remove the user from.</param>
        public override void RemoveUserFromRole(int portalId, UserInfo user, UserRoleInfo userRole)
        {
			Logger.LogDebug(">>> Entering RemoveUserFromRole(portalId={PortalId},user={User},userRole={UserRole})", portalId, user, userRole);
            this.dataProvider.DeleteUserRole(userRole.UserID, userRole.RoleID);
			Logger.LogDebug("<<< Exiting RemoveUserFromRole(portalId={PortalId},user={User},userRole={UserRole})", portalId, user, userRole);
        }

        /// <summary>Updates a User/Role.</summary>
        /// <param name="userRole">The User/Role to update.</param>
        public override void UpdateUserRole(UserRoleInfo userRole)
        {
			Logger.LogDebug(">>> Entering UpdateUserRole(userRole={UserRole})", userRole);
            this.dataProvider.UpdateUserRole(
                userRole.UserRoleID,
                (int)userRole.Status,
                userRole.IsOwner,
                userRole.EffectiveDate,
                userRole.ExpiryDate,
                this.userController.GetCurrentUserInfo().UserID);
				Logger.LogDebug("<<< Exiting UpdateUserRole(userRole={UserRole})", userRole);
        }

        /// <summary>CreateRoleGroup persists a RoleGroup to the Data Store.</summary>
        /// <param name="roleGroup">The RoleGroup to persist to the Data Store.</param>
        /// <returns>The ID of the new role.</returns>
        public override int CreateRoleGroup(RoleGroupInfo roleGroup)
        {
			Logger.LogDebug(">>> Entering CreateRoleGroup(roleGroup={RoleGroup})", roleGroup);
            var roleGroupId = this.dataProvider.AddRoleGroup(
                roleGroup.PortalID,
                roleGroup.RoleGroupName.Trim(),
                (roleGroup.Description ?? string.Empty).Trim(),
                this.userController.GetCurrentUserInfo().UserID);
            ClearRoleGroupCache(roleGroup.PortalID);
			Logger.LogDebug("<<< Exiting CreateRoleGroup(roleGroup={RoleGroup})", roleGroup);
            return roleGroupId;
        }

        /// <summary>DeleteRoleGroup deletes a RoleGroup from the Data Store.</summary>
        /// <param name="roleGroup">The RoleGroup to delete from the Data Store.</param>
        public override void DeleteRoleGroup(RoleGroupInfo roleGroup)
        {
			Logger.LogDebug(">>> Entering DeleteRoleGroup(roleGroup={RoleGroup})", roleGroup);
            this.dataProvider.DeleteRoleGroup(roleGroup.RoleGroupID);
            ClearRoleGroupCache(roleGroup.PortalID);
			Logger.LogDebug("<<< Exiting DeleteRoleGroup(roleGroup={RoleGroup})", roleGroup);
        }

        /// <summary>GetRoleGroup gets a RoleGroup from the Data Store.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="roleGroupId">The ID of the RoleGroup to retrieve.</param>
        /// <returns>A RoleGroupInfo object.</returns>
        public override RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId)
        {
			Logger.LogDebug(">>> Entering GetRoleGroup(portalId={PortalId},roleGroupId={RoleGroupId})", portalId, roleGroupId);
			Logger.LogDebug("<<< Exiting GetRoleGroup(portalId={PortalId},roleGroupId={RoleGroupId})", portalId, roleGroupId);
            return this.GetRoleGroupsInternal(portalId).SingleOrDefault(r => r.RoleGroupID == roleGroupId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override RoleGroupInfo GetRoleGroupByName(int portalId, string roleGroupName)
        {
			Logger.LogDebug(">>> Entering GetRoleGroupByName(portalId={PortalId},roleGroupName={RoleGroupName})", portalId, roleGroupName);
            roleGroupName = roleGroupName.Trim();
			Logger.LogDebug("GetRoleGroupByName(portalId={PortalId},roleGroupName={RoleGroupName}): roleGroupName → {RoleGroupName1}", portalId, roleGroupName, roleGroupName);
			Logger.LogDebug("<<< Exiting GetRoleGroupByName(portalId={PortalId},roleGroupName={RoleGroupName})", portalId, roleGroupName);
            return this.GetRoleGroupsInternal(portalId).SingleOrDefault(
                r => roleGroupName.Equals(r.RoleGroupName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Get the RoleGroups for a portal.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <returns>An ArrayList of RoleGroupInfo objects.</returns>
        public override ArrayList GetRoleGroups(int portalId)
        {
			Logger.LogDebug(">>> Entering GetRoleGroups(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetRoleGroups(portalId={PortalId})", portalId);
            return new ArrayList(this.GetRoleGroupsInternal(portalId).ToList());
        }

        /// <summary>Update a RoleGroup.</summary>
        /// <param name="roleGroup">The RoleGroup to update.</param>
        public override void UpdateRoleGroup(RoleGroupInfo roleGroup)
        {
			Logger.LogDebug(">>> Entering UpdateRoleGroup(roleGroup={RoleGroup})", roleGroup);
            this.dataProvider.UpdateRoleGroup(
                roleGroup.RoleGroupID,
                roleGroup.RoleGroupName.Trim(),
                (roleGroup.Description ?? string.Empty).Trim(),
                this.userController.GetCurrentUserInfo().UserID);
            ClearRoleGroupCache(roleGroup.PortalID);
			Logger.LogDebug("<<< Exiting UpdateRoleGroup(roleGroup={RoleGroup})", roleGroup);
        }

        private static void ClearRoleGroupCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearRoleGroupCache(portalId={PortalId})", portalId);
            DataCache.ClearCache(GetRoleGroupsCacheKey(portalId));
			Logger.LogDebug("<<< Exiting ClearRoleGroupCache(portalId={PortalId})", portalId);
        }

        private static string GetRoleGroupsCacheKey(int portalId)
        {
			Logger.LogDebug(">>> Entering GetRoleGroupsCacheKey(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetRoleGroupsCacheKey(portalId={PortalId})", portalId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.RoleGroupsCacheKey, portalId);
        }

        private void AddDNNUserRole(UserRoleInfo userRole)
        {
			Logger.LogDebug(">>> Entering AddDNNUserRole(userRole={UserRole})", userRole);
            // Add UserRole to DNN
            userRole.UserRoleID = Convert.ToInt32(
                this.dataProvider.AddUserRole(
                    userRole.PortalID,
                    userRole.UserID,
                    userRole.RoleID,
                    (int)userRole.Status,
                    userRole.IsOwner,
                    userRole.EffectiveDate,
                    userRole.ExpiryDate,
                    this.userController.GetCurrentUserInfo().UserID));
					Logger.LogDebug("<<< Exiting AddDNNUserRole(userRole={UserRole})", userRole);
        }

        private IEnumerable<RoleGroupInfo> GetRoleGroupsInternal(int portalId)
        {
			Logger.LogDebug(">>> Entering GetRoleGroupsInternal(portalId={PortalId})", portalId);
            var cacheArgs = new CacheItemArgs(
                GetRoleGroupsCacheKey(portalId),
                DataCache.RoleGroupsCacheTimeOut,
                DataCache.RoleGroupsCachePriority);

			Logger.LogDebug("<<< Exiting GetRoleGroupsInternal(portalId={PortalId})", portalId);
            return CBO.GetCachedObject<IEnumerable<RoleGroupInfo>>(
                this.hostSettings,
                cacheArgs,
                _ => CBO.FillCollection<RoleGroupInfo>(this.dataProvider.GetRoleGroups(portalId)));
        }
    }
}
