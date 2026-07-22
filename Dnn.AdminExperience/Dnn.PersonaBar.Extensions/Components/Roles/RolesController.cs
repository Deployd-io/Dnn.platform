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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize})", portalSettings, groupId, keyword, startIndex, pageSize);
            var isAdmin = IsAdmin(portalSettings);
            var roles = (groupId < Null.NullInteger
                ? RoleController.Instance.GetRoles(portalSettings.PortalId)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == groupId))
                .Where(r => isAdmin || r.RoleID != portalSettings.AdministratorRoleId);
            if (!string.IsNullOrEmpty(keyword))
            {
                roles = roles.Where(r => r.RoleName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) > Null.NullInteger);
            }
			Logger.LogDebug("GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize}): roles → {Roles}", portalSettings, groupId, keyword, startIndex, pageSize, roles);

            var roleInfos = roles as IList<RoleInfo> ?? roles.ToList();
            total = roleInfos.Count;
			Logger.LogDebug("GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize}): roleInfos → {RoleInfos}", portalSettings, groupId, keyword, startIndex, pageSize, roleInfos);
			Logger.LogDebug("GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize}): total → {Total}", portalSettings, groupId, keyword, startIndex, pageSize, total);
			Logger.LogDebug("<<< Exiting GetRoles(portalSettings={PortalSettings},groupId={GroupId},keyword={Keyword},startIndex={StartIndex},pageSize={PageSize})", portalSettings, groupId, keyword, startIndex, pageSize);
            return roleInfos.Skip(startIndex).Take(pageSize);
        }

        /// <summary>Gets a list (not paginated) of Roles given a comma separated list of Roles' names.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="groupId">The group ID.</param>
        /// <param name="rolesFilter">A list of roles to include in the result.</param>
        /// <returns>List of found Roles.</returns>
        public IList<RoleInfo> GetRolesByNames(PortalSettings portalSettings, int groupId, IList<string> rolesFilter)
        {
			Logger.LogDebug(">>> Entering GetRolesByNames(portalSettings={PortalSettings},groupId={GroupId},rolesFilter={RolesFilter})", portalSettings, groupId, rolesFilter);
            var isAdmin = IsAdmin(portalSettings);

			Logger.LogDebug("GetRolesByNames(portalSettings={PortalSettings},groupId={GroupId},rolesFilter={RolesFilter}): isAdmin → {IsAdmin}", portalSettings, groupId, rolesFilter, isAdmin);
            List<RoleInfo> foundRoles = null;
            if (rolesFilter.Count > 0)
            {
                var allRoles = groupId < Null.NullInteger
                ? RoleController.Instance.GetRoles(portalSettings.PortalId)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == groupId);

                foundRoles = allRoles.Where(r =>
                {
                    bool adminCheck = isAdmin || r.RoleID != portalSettings.AdministratorRoleId;
                    return adminCheck && rolesFilter.Contains(r.RoleName);
                }).ToList();
            }

			Logger.LogDebug("<<< Exiting GetRolesByNames(portalSettings={PortalSettings},groupId={GroupId},rolesFilter={RolesFilter})", portalSettings, groupId, rolesFilter);
            return foundRoles;
        }

        /// <inheritdoc />
        public RoleInfo GetRole(PortalSettings portalSettings, int roleId)
        {
			Logger.LogDebug(">>> Entering GetRole(portalSettings={PortalSettings},roleId={RoleId})", portalSettings, roleId);
            var isAdmin = IsAdmin(portalSettings);
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
			Logger.LogDebug("GetRole(portalSettings={PortalSettings},roleId={RoleId}): isAdmin → {IsAdmin}", portalSettings, roleId, isAdmin);
			Logger.LogDebug("GetRole(portalSettings={PortalSettings},roleId={RoleId}): role → {Role}", portalSettings, roleId, role);
            if (!isAdmin && role.RoleID == portalSettings.AdministratorRoleId)
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetRole(portalSettings={PortalSettings},roleId={RoleId})", portalSettings, roleId);
            return role;
        }

        /// <inheritdoc />
        public bool SaveRole(PortalSettings portalSettings, RoleDto roleDto, bool assignExistUsers, out KeyValuePair<HttpStatusCode, string> message)
        {
			Logger.LogDebug(">>> Entering SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message})", portalSettings, roleDto, assignExistUsers, message);
            message = default(KeyValuePair<HttpStatusCode, string>);
            if (!IsAdmin(portalSettings) && roleDto.Id == portalSettings.AdministratorRoleId)
			Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): message → {Message1}", portalSettings, roleDto, assignExistUsers, message, message);
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return false;
            }

            var role = roleDto.ToRoleInfo();
            role.PortalID = portalSettings.PortalId;
			Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): role → {Role}", portalSettings, roleDto, assignExistUsers, message, role);
			Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): role.PortalID → {Arg4}", portalSettings, roleDto, assignExistUsers, message, role.PortalID);
            var rolename = role.RoleName.ToUpperInvariant();

			Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): rolename → {Rolename}", portalSettings, roleDto, assignExistUsers, message, rolename);
            if (roleDto.Id == Null.NullInteger)
            {
                if (RoleController.Instance.GetRole(portalSettings.PortalId, r => rolename.Equals(r.RoleName, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    RoleController.Instance.AddRole(role, assignExistUsers);
                    roleDto.Id = role.RoleID;
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): roleDto.Id → {Arg4}", portalSettings, roleDto, assignExistUsers, message, roleDto.Id);
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
                if (existingRole == null)
				Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole → {ExistingRole}", portalSettings, roleDto, assignExistUsers, message, existingRole);
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
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.RoleName → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.RoleName);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.Description → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.Description);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.RoleGroupID → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.RoleGroupID);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.SecurityMode → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.SecurityMode);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.Status → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.Status);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.IsPublic → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.IsPublic);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.AutoAssignment → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.AutoAssignment);
					Logger.LogDebug("SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message}): existingRole.RSVPCode → {Arg4}", portalSettings, roleDto, assignExistUsers, message, existingRole.RSVPCode);
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

			Logger.LogDebug("<<< Exiting SaveRole(portalSettings={PortalSettings},roleDto={RoleDto},assignExistUsers={AssignExistUsers},message={Message})", portalSettings, roleDto, assignExistUsers, message);
            return true;
        }

        /// <inheritdoc />
        public string DeleteRole(PortalSettings portalSettings, int roleId, out KeyValuePair<HttpStatusCode, string> message)
        {
			Logger.LogDebug(">>> Entering DeleteRole(portalSettings={PortalSettings},roleId={RoleId},message={Message})", portalSettings, roleId, message);
            message = default(KeyValuePair<HttpStatusCode, string>);
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
			Logger.LogDebug("DeleteRole(portalSettings={PortalSettings},roleId={RoleId},message={Message}): message → {Message1}", portalSettings, roleId, message, message);
            if (role == null)
			Logger.LogDebug("DeleteRole(portalSettings={PortalSettings},roleId={RoleId},message={Message}): role → {Role}", portalSettings, roleId, message, role);
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
			Logger.LogDebug("<<< Exiting DeleteRole(portalSettings={PortalSettings},roleId={RoleId},message={Message})", portalSettings, roleId, message);
            return role.RoleName;
        }

        /// <inheritdoc />
        protected override Func<IRolesController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new RolesController();
        }

        private static bool IsAdmin(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering IsAdmin(portalSettings={PortalSettings})", portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("IsAdmin(portalSettings={PortalSettings}): user → {User}", portalSettings, user);
			Logger.LogDebug("<<< Exiting IsAdmin(portalSettings={PortalSettings})", portalSettings);
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }
    }
}
