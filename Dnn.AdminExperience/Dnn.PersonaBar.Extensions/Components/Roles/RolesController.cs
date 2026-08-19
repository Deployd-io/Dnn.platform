// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Roles.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    public class RolesController : ServiceLocator<IRolesController, RolesController>, IRolesController
    {
        /// <summary>Gets a paginated list of Roles matching given search criteria.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="groupId">The group ID.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="total">The total count.</param>
        /// <param name="startIndex">The index to start the page on.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A sequence of <see cref="RoleInfo"/> instances.</returns>
        public IEnumerable<RoleInfo> GetRoles(PortalSettings portalSettings, int groupId, string keyword, out int total, int startIndex, int pageSize)
        {
			eventLogger.LogDebug(">>> Entering GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize})", portalSettings, groupId, keyword, startIndex, pageSize);
            var isAdmin = IsAdmin(portalSettings);
            var roles = (groupId < Null.NullInteger
                ? RoleController.Instance.GetRoles(portalSettings.PortalId)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == groupId))
                .Where(r => isAdmin || r.RoleID != portalSettings.AdministratorRoleId);
            if (!string.IsNullOrEmpty(keyword))
            {
                roles = roles.Where(r => r.RoleName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) > Null.NullInteger);
            }

            var roleInfos = roles as IList<RoleInfo> ?? roles.ToList();
			eventLogger.LogDebug("GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize}): roleInfos → {RoleInfos}", portalSettings, groupId, keyword, startIndex, pageSize, roleInfos);
            total = roleInfos.Count;
			eventLogger.LogDebug("GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize}): total → {Total}", portalSettings, groupId, keyword, startIndex, pageSize, total);
			eventLogger.LogDebug("<<< Exiting GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize})", portalSettings, groupId, keyword, startIndex, pageSize);
            return roleInfos.Skip(startIndex).Take(pageSize);
        }

        /// <summary>Gets a list (not paginated) of Roles given a comma separated list of Roles' names.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="groupId">The group ID.</param>
        /// <param name="rolesFilter">A list of roles to include in the result.</param>
        /// <returns>List of found Roles.</returns>
        public IList<RoleInfo> GetRolesByNames(PortalSettings portalSettings, int groupId, IList<string> rolesFilter)
        {
			eventLogger.LogDebug(">>> Entering GetRolesByNames(portalSettings={PortalSettings},groupId={GroupId},rolesFilter={RolesFilter})", portalSettings, groupId, rolesFilter);
            var isAdmin = IsAdmin(portalSettings);

			eventLogger.LogDebug("GetRolesByNames(portalSettings={PortalSettings},groupId={GroupId},rolesFilter={RolesFilter}): isAdmin → {IsAdmin}", portalSettings, groupId, rolesFilter, isAdmin);
            List<RoleInfo> foundRoles = null;
            if (rolesFilter.Count > 0)
            {
                var allRoles = groupId < Null.NullInteger
                ? RoleController.Instance.GetRoles(portalSettings.PortalId)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == groupId);

                foundRoles = allRoles.Where(r =>
                {
                    bool adminCheck = isAdmin || r.RoleID != portalSettings.AdministratorRoleId;
			eventLogger.LogDebug("<<< Exiting GetRolesByNames(portalSettings={PortalSettings},groupId={GroupId},rolesFilter={RolesFilter})", portalSettings, groupId, rolesFilter);
                    return adminCheck && rolesFilter.Contains(r.RoleName);
                }).ToList();
            }

            return foundRoles;
        }

        /// <inheritdoc />
        public RoleInfo GetRole(PortalSettings portalSettings, int roleId)
        {
			eventLogger.LogDebug(">>> Entering GetRole(portalSettings={PortalSettings},roleId={RoleId})", portalSettings, roleId);
            var isAdmin = IsAdmin(portalSettings);
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
			eventLogger.LogDebug("GetRole(portalSettings={PortalSettings},roleId={RoleId}): role → {Role}", portalSettings, roleId, role);
			eventLogger.LogDebug("GetRole(portalSettings={PortalSettings},roleId={RoleId}): isAdmin → {IsAdmin}", portalSettings, roleId, isAdmin);
            if (!isAdmin && role.RoleID == portalSettings.AdministratorRoleId)
            {
			eventLogger.LogDebug("<<< Exiting GetRole(portalSettings={PortalSettings},roleId={RoleId})", portalSettings, roleId);
                return null;
            }

            return role;
        }

        /// <inheritdoc />
        public bool SaveRole(PortalSettings portalSettings, RoleDto roleDto, bool assignExistUsers, out KeyValuePair<HttpStatusCode, string> message)
        {
			eventLogger.LogDebug(">>> Entering SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers})", portalSettings, roleDto, assignExistUsers);
            message = default(KeyValuePair<HttpStatusCode, string>);
            if (!IsAdmin(portalSettings) && roleDto.Id == portalSettings.AdministratorRoleId)
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return false;
            }

            var role = roleDto.ToRoleInfo();
			eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): role → {Role}", portalSettings, roleDto, assignExistUsers, role);
			eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): role.PortalID → {Arg3}", portalSettings, roleDto, assignExistUsers, role.PortalID);
            role.PortalID = portalSettings.PortalId;
            var rolename = role.RoleName.ToUpperInvariant();
			eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): rolename → {Rolename}", portalSettings, roleDto, assignExistUsers, rolename);

            if (roleDto.Id == Null.NullInteger)
            {
                if (RoleController.Instance.GetRole(portalSettings.PortalId, r => rolename.Equals(r.RoleName, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    RoleController.Instance.AddRole(role, assignExistUsers);
                    roleDto.Id = role.RoleID;
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): roleDto.Id → {Arg3}", portalSettings, roleDto, assignExistUsers, roleDto.Id);
                }
                else
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("DuplicateRole", Constants.LocalResourcesFile));
                    return false;
                }
            }
            else
            {
                var existingRole = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleDto.Id);
				eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole → {ExistingRole}", portalSettings, roleDto, assignExistUsers, existingRole);
                if (existingRole == null)
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Constants.LocalResourcesFile));
                    return false;
                }

                if (existingRole.IsSystemRole)
                {
                    // In system roles nothing can be updated.
                }
                else if (RoleController.Instance.GetRole(portalSettings.PortalId, r => rolename.Equals(r.RoleName, StringComparison.OrdinalIgnoreCase) && r.RoleID != roleDto.Id) == null)
                {
                    existingRole.RoleName = role.RoleName;
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.RoleName → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.RoleName);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.Description → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.Description);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.RoleGroupID → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.RoleGroupID);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.SecurityMode → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.SecurityMode);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.Status → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.Status);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.IsPublic → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.IsPublic);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.AutoAssignment → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.AutoAssignment);
					eventLogger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers}): existingRole.RSVPCode → {Arg3}", portalSettings, roleDto, assignExistUsers, existingRole.RSVPCode);
                    existingRole.Description = role.Description;
                    existingRole.RoleGroupID = role.RoleGroupID;
                    existingRole.SecurityMode = role.SecurityMode;
                    existingRole.Status = role.Status;
                    existingRole.IsPublic = role.IsPublic;
                    existingRole.AutoAssignment = role.AutoAssignment;
                    existingRole.RSVPCode = role.RSVPCode;
                    RoleController.Instance.UpdateRole(existingRole, assignExistUsers);
                }
                else
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("DuplicateRole", Constants.LocalResourcesFile));
                    return false;
                }
            }

			eventLogger.LogDebug("<<< Exiting SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers})", portalSettings, roleDto, assignExistUsers);
            return true;
        }

        /// <inheritdoc />
        public string DeleteRole(PortalSettings portalSettings, int roleId, out KeyValuePair<HttpStatusCode, string> message)
        {
			eventLogger.LogDebug(">>> Entering DeleteRole(portalSettings={PortalSettings},roleId={RoleId})", portalSettings, roleId);
            message = default(KeyValuePair<HttpStatusCode, string>);
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
			eventLogger.LogDebug("DeleteRole(portalSettings={PortalSettings},roleId={RoleId}): role → {Role}", portalSettings, roleId, role);
			eventLogger.LogDebug("DeleteRole(portalSettings={PortalSettings},roleId={RoleId}): message → {Message}", portalSettings, roleId, message);
            if (role == null)
            {
                message = new KeyValuePair<HttpStatusCode, string>(
                    HttpStatusCode.NotFound,
                    Localization.GetString("RoleNotFound", Constants.LocalResourcesFile));
                return string.Empty;
            }

            if (role.IsSystemRole)
            {
                message = new KeyValuePair<HttpStatusCode, string>(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("SecurityRoleDeleteNotAllowed", Constants.LocalResourcesFile));
                return string.Empty;
            }

            if (role.RoleID == portalSettings.AdministratorRoleId && !IsAdmin(portalSettings))
            {
                message = new KeyValuePair<HttpStatusCode, string>(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return string.Empty;
            }

            RoleController.Instance.DeleteRole(role);
            DataCache.RemoveCache("GetRoles");
			eventLogger.LogDebug("<<< Exiting DeleteRole(portalSettings={PortalSettings},roleId={RoleId})", portalSettings, roleId);
            return role.RoleName;
        }

        /// <inheritdoc />
        protected override Func<IRolesController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new RolesController();
        }

        private static bool IsAdmin(PortalSettings portalSettings)
        {
			eventLogger.LogDebug(">>> Entering IsAdmin(portalSettings={PortalSettings})", portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();
			eventLogger.LogDebug("IsAdmin(portalSettings={PortalSettings}): user → {User}", portalSettings, user);
			eventLogger.LogDebug("<<< Exiting IsAdmin(portalSettings={PortalSettings})", portalSettings);
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }
    }
}
