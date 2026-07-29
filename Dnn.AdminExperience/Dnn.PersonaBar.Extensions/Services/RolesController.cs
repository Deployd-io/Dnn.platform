// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class RolesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RolesController));

        [HttpGet]
        public HttpResponseMessage GetRoles(int groupId, string keyword, int startIndex, int pageSize)
        {
            try
            {
                int total;
                var roles = Components.RolesController.Instance.GetRoles(this.PortalSettings, groupId, keyword, out total, startIndex, pageSize)
                    .Select(RoleDto.FromRoleInfo);
                var loadMore = total > startIndex + pageSize;
                var rsvpLink = Globals.AddHTTP(Globals.GetDomainName(HttpContext.Current.Request)) + "/" + Globals.glbDefaultPage + "?portalid=" + this.PortalId;
                return this.Request.CreateResponse(HttpStatusCode.OK, new { roles, loadMore, rsvpLink });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage SaveRole(RoleDto roleDto, [FromUri] bool assignExistUsers)
        {
			Logger.LogDebug(">>> Entering SaveRole(roleDto={RoleDto},assignExistUsers={AssignExistUsers})", roleDto, assignExistUsers);
            try
            {
                this.Validate(roleDto);
                KeyValuePair<HttpStatusCode, string> message;
                return Components.RolesController.Instance.SaveRole(this.PortalSettings, roleDto, assignExistUsers, out message)
                    ? this.Request.CreateResponse(HttpStatusCode.OK, this.GetRole(roleDto.Id))
                    : this.Request.CreateErrorResponse(message.Key, message.Value);
            }
            catch (ArgumentException ex)
            {
				Logger.LogError("Exception in SaveRole(roleDto={RoleDto},assignExistUsers={AssignExistUsers}): {Arg2}", roleDto, assignExistUsers, ex.Message);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting SaveRole(roleDto={RoleDto},assignExistUsers={AssignExistUsers})", roleDto, assignExistUsers);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage DeleteRole(RoleDto roleDto)
        {
			Logger.LogDebug(">>> Entering DeleteRole(roleDto={RoleDto})", roleDto);
            var roleName = Components.RolesController.Instance.DeleteRole(this.PortalSettings, roleDto.Id, out var message);
			Logger.LogDebug("DeleteRole(roleDto={RoleDto}): roleName → {RoleName}", roleDto, roleName);
			Logger.LogDebug("<<< Exiting DeleteRole(roleDto={RoleDto})", roleDto);
            return !string.IsNullOrEmpty(roleName) ? this.Request.CreateResponse(HttpStatusCode.OK, new { roleId = roleDto.Id })
                : this.Request.CreateErrorResponse(message.Key, message.Value);
        }

        [HttpGet]
        public HttpResponseMessage GetRoleGroups(bool reload = false)
        {
			Logger.LogDebug(">>> Entering GetRoleGroups(reload={Reload})", reload);
            try
            {
                if (reload)
                {
                    DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.RoleGroupsCacheKey, this.PortalId));
                }

                var groups = RoleController.GetRoleGroups(this.PortalId)
                    .Cast<RoleGroupInfo>()
                    .Select(RoleGroupDto.FromRoleGroupInfo);

                return this.Request.CreateResponse(HttpStatusCode.OK, groups);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetRoleGroups(reload={Reload})", reload);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage SaveRoleGroup(RoleGroupDto roleGroupDto)
        {
			Logger.LogDebug(">>> Entering SaveRoleGroup(roleGroupDto={RoleGroupDto})", roleGroupDto);
            try
            {
                Validate(roleGroupDto);

                var roleGroup = roleGroupDto.ToRoleGroupInfo();
                roleGroup.PortalID = this.PortalId;
				Logger.LogDebug("SaveRoleGroup(roleGroupDto={RoleGroupDto}): roleGroup → {RoleGroup}", roleGroupDto, roleGroup);
				Logger.LogDebug("SaveRoleGroup(roleGroupDto={RoleGroupDto}): roleGroup.PortalID → {Arg1}", roleGroupDto, roleGroup.PortalID);

                if (roleGroup.RoleGroupID < Null.NullInteger)
                {
                    try
                    {
                        RoleController.AddRoleGroup(roleGroup);
                    }
                    catch
                    {
						Logger.LogError("Exception in SaveRoleGroup(roleGroupDto={RoleGroupDto}): {Arg1}", roleGroupDto, ex.Message);
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateRoleGroup", Components.Constants.LocalResourcesFile));
                    }
                }
                else
                {
                    try
                    {
                        RoleController.UpdateRoleGroup(roleGroup);
                    }
                    catch
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateRoleGroup", Components.Constants.LocalResourcesFile));
                    }
                }

                roleGroup = RoleController.GetRoleGroups(this.PortalId).Cast<RoleGroupInfo>()
                    .FirstOrDefault(r => r.RoleGroupName == roleGroupDto.Name?.Trim());

                return this.Request.CreateResponse(HttpStatusCode.OK, RoleGroupDto.FromRoleGroupInfo(roleGroup));
            }
            catch (ArgumentException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting SaveRoleGroup(roleGroupDto={RoleGroupDto})", roleGroupDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage DeleteRoleGroup(RoleGroupDto roleGroupDto)
        {
			Logger.LogDebug(">>> Entering DeleteRoleGroup(roleGroupDto={RoleGroupDto})", roleGroupDto);
            var roleGroup = RoleController.GetRoleGroup(this.PortalId, roleGroupDto.Id);
            if (roleGroup == null)
			Logger.LogDebug("DeleteRoleGroup(roleGroupDto={RoleGroupDto}): roleGroup → {RoleGroup}", roleGroupDto, roleGroup);
            {
                return this.Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Localization.GetString("RoleGroupNotFound", Components.Constants.LocalResourcesFile));
            }

            RoleController.DeleteRoleGroup(roleGroup);

			Logger.LogDebug("<<< Exiting DeleteRoleGroup(roleGroupDto={RoleGroupDto})", roleGroupDto);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { groupId = roleGroupDto.Id });
        }

        [HttpGet]
        public HttpResponseMessage GetSuggestUsers(string keyword, int roleId, int count)
        {
			Logger.LogDebug(">>> Entering GetSuggestUsers(keyword={Keyword},roleId={RoleId},count={Count})", keyword, roleId, count);
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new List<UserRoleDto>());
                }

                var displayMatch = keyword + "%";
                var totalRecords = 0;
				Logger.LogDebug("GetSuggestUsers(keyword={Keyword},roleId={RoleId},count={Count}): displayMatch → {DisplayMatch}", keyword, roleId, count, displayMatch);
                var totalRecords2 = 0;
                var isAdmin = this.IsAdmin();

				Logger.LogDebug("GetSuggestUsers(keyword={Keyword},roleId={RoleId},count={Count}): isAdmin → {IsAdmin}", keyword, roleId, count, isAdmin);
                var matchedUsers = UserController.GetUsersByDisplayName(this.PortalId, displayMatch, 0, count, ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(this.PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
				Logger.LogDebug("GetSuggestUsers(keyword={Keyword},roleId={RoleId},count={Count}): matchedUsers → {MatchedUsers}", keyword, roleId, count, matchedUsers);
                var finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => isAdmin || !x.Roles.Contains(this.PortalSettings.AdministratorRoleName))
                    .Select(u => new UserRoleDto()
                    {
                        UserId = u.UserID,
                        DisplayName = $"{u.DisplayName} ({u.Username})",
                    });

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    finalUsers.ToList().GroupBy(x => x.UserId).Select(group => group.First()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetSuggestUsers(keyword={Keyword},roleId={RoleId},count={Count})", keyword, roleId, count);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetRoleUsers(string keyword, int roleId, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetRoleUsers(keyword={Keyword},roleId={RoleId},pageIndex={PageIndex},pageSize={PageSize})", keyword, roleId, pageIndex, pageSize);
            try
            {
                var role = RoleController.Instance.GetRoleById(this.PortalId, roleId);
                if (role == null)
				Logger.LogDebug("GetRoleUsers(keyword={Keyword},roleId={RoleId},pageIndex={PageIndex},pageSize={PageSize}): role → {Role}", keyword, roleId, pageIndex, pageSize, role);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Components.Constants.LocalResourcesFile));
                }

                if (role.RoleID == this.PortalSettings.AdministratorRoleId && !this.IsAdmin())
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
                }

                var users = RoleController.Instance.GetUserRoles(this.PortalId, Null.NullString, role.RoleName);
                if (!string.IsNullOrEmpty(keyword))
				Logger.LogDebug("GetRoleUsers(keyword={Keyword},roleId={RoleId},pageIndex={PageIndex},pageSize={PageSize}): users → {Users}", keyword, roleId, pageIndex, pageSize, users);
                {
                    users =
                        users.Where(u => u.FullName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }

                var totalRecords = users.Count;
                var startIndex = pageIndex * pageSize;
                var portal = PortalController.Instance.GetPortal(this.PortalId);
                var pagedData = users.OrderBy(x => x.FullName).Skip(startIndex).Take(pageSize).Select(u => new UserRoleDto()
                {
				Logger.LogDebug("GetRoleUsers(keyword={Keyword},roleId={RoleId},pageIndex={PageIndex},pageSize={PageSize}): portal → {Portal}", keyword, roleId, pageIndex, pageSize, portal);
                    UserId = u.UserID,
                    RoleId = u.RoleID,
                    DisplayName = u.FullName,
                    StartTime = u.EffectiveDate,
                    ExpiresTime = u.ExpiryDate,
                    AllowExpired = this.AllowExpired(u.UserID, u.RoleID),
                    AllowDelete = RoleController.CanRemoveUserFromRole(portal, u.UserID, u.RoleID),
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, new { users = pagedData, totalRecords });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetRoleUsers(keyword={Keyword},roleId={RoleId},pageIndex={PageIndex},pageSize={PageSize})", keyword, roleId, pageIndex, pageSize);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage AddUserToRole(UserRoleDto userRoleDto, bool notifyUser, bool isOwner)
        {
			Logger.LogDebug(">>> Entering AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", userRoleDto, notifyUser, isOwner);
            try
            {
                this.Validate(userRoleDto);

                if (!this.AllowExpired(userRoleDto.UserId, userRoleDto.RoleId))
                {
                    userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
					Logger.LogDebug("AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): userRoleDto.StartTime → {Arg3}", userRoleDto, notifyUser, isOwner, userRoleDto.StartTime);
                }

                HttpResponseMessage response;
                var user = this.GetUser(userRoleDto.UserId, out response);
                if (user == null)
				Logger.LogDebug("AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): user → {User}", userRoleDto, notifyUser, isOwner, user);
                {
                    return response;
                }

                var role = RoleController.Instance.GetRoleById(this.PortalId, userRoleDto.RoleId);
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
				Logger.LogDebug("AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): role → {Role}", userRoleDto, notifyUser, isOwner, role);
                {
                    isOwner = false;
                }

                if (role.Status != RoleStatus.Approved)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString(
                            "CannotAssginUserToUnApprovedRole",
                            Components.Constants.LocalResourcesFile));
                }

                RoleController.AddUserRole(
                    user,
                    role,
                    this.PortalSettings,
                    RoleStatus.Approved,
                    userRoleDto.StartTime,
                    userRoleDto.ExpiresTime,
                    notifyUser,
                    isOwner);

                var addedUser = RoleController.Instance.GetUserRole(this.PortalId, userRoleDto.UserId, userRoleDto.RoleId);
                var portal = PortalController.Instance.GetPortal(this.PortalId);
				Logger.LogDebug("AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): addedUser → {AddedUser}", userRoleDto, notifyUser, isOwner, addedUser);
				Logger.LogDebug("AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): portal → {Portal}", userRoleDto, notifyUser, isOwner, portal);

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new UserRoleDto
                    {
                        UserId = addedUser.UserID,
                        RoleId = addedUser.RoleID,
                        DisplayName = addedUser.FullName,
                        StartTime = addedUser.EffectiveDate,
                        ExpiresTime = addedUser.ExpiryDate,
                        AllowExpired = this.AllowExpired(addedUser.UserID, addedUser.RoleID),
                        AllowDelete = RoleController.CanRemoveUserFromRole(portal, addedUser.UserID, addedUser.RoleID),
                    });
            }
            catch (ArgumentException ex)
            {
				Logger.LogError("Exception in AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): {Arg3}", userRoleDto, notifyUser, isOwner, ex.Message);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting AddUserToRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", userRoleDto, notifyUser, isOwner);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage RemoveUserFromRole(UserRoleDto userRoleDto)
        {
			Logger.LogDebug(">>> Entering RemoveUserFromRole(userRoleDto={UserRoleDto})", userRoleDto);
            try
            {
                this.Validate(userRoleDto);
                HttpResponseMessage response;
                var user = this.GetUser(userRoleDto.UserId, out response);
                if (user == null)
				Logger.LogDebug("RemoveUserFromRole(userRoleDto={UserRoleDto}): user → {User}", userRoleDto, user);
                {
                    return response;
                }

                RoleController.Instance.UpdateUserRole(
                    this.PortalId,
                    userRoleDto.UserId,
                    userRoleDto.RoleId,
                    RoleStatus.Approved,
                    false,
                    true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { userRoleDto.UserId, userRoleDto.RoleId });
            }
            catch (ArgumentException ex)
            {
				Logger.LogError("Exception in RemoveUserFromRole(userRoleDto={UserRoleDto}): {Arg1}", userRoleDto, ex.Message);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting RemoveUserFromRole(userRoleDto={UserRoleDto})", userRoleDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static void Validate(RoleGroupDto role)
        {
            Requires.NotNullOrHasNoWhiteSpace("Name", role.Name);
        }

        private void Validate(RoleDto role)
        {
            Requires.NotNullOrEmpty("Name", role.Name);

            if (!this.IsAdmin() && role.Id == this.PortalSettings.AdministratorRoleId)
            {
                throw new SecurityException(Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
            }
        }

        private void Validate(UserRoleDto userRoleDto)
        {
			Logger.LogDebug(">>> Entering Validate(userRoleDto={UserRoleDto})", userRoleDto);
            Requires.NotNegative("UserId", userRoleDto.UserId);
            Requires.NotNegative("RoleId", userRoleDto.RoleId);

            if (!this.IsAdmin() && userRoleDto.RoleId == this.PortalSettings.AdministratorRoleId)
            {
                throw new SecurityException(Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
            }
			Logger.LogDebug("<<< Exiting Validate(userRoleDto={UserRoleDto})", userRoleDto);
        }

        private bool AllowExpired(int userId, int roleId)
        {
			Logger.LogDebug(">>> Entering AllowExpired(userId={UserId},roleId={RoleId})", userId, roleId);
			Logger.LogDebug("<<< Exiting AllowExpired(userId={UserId},roleId={RoleId})", userId, roleId);
            return userId != this.PortalSettings.AdministratorId || roleId != this.PortalSettings.AdministratorRoleId;
        }

        private RoleDto GetRole(int roleId)
        {
			Logger.LogDebug(">>> Entering GetRole(roleId={RoleId})", roleId);
			Logger.LogDebug("<<< Exiting GetRole(roleId={RoleId})", roleId);
            return RoleDto.FromRoleInfo(RoleController.Instance.GetRoleById(this.PortalId, roleId));
        }

        private bool IsAdmin()
        {
			Logger.LogDebug(">>> Entering IsAdmin()");
            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("IsAdmin(): user → {User}", user);
			Logger.LogDebug("<<< Exiting IsAdmin()");
            return user.IsSuperUser || user.IsInRole(this.PortalSettings.AdministratorRoleName);
        }

        private bool IsAdmin(UserInfo user)
        {
            return user.IsSuperUser || user.IsInRole(this.PortalSettings.AdministratorRoleName);
        }

        private UserInfo GetUser(int userId, out HttpResponseMessage response)
        {
			Logger.LogDebug(">>> Entering GetUser(userId={UserId})", userId);
            response = null;
            var user = UserController.Instance.GetUserById(this.PortalId, userId);
			Logger.LogDebug("GetUser(userId={UserId}): response → {Response}", userId, response);
            if (user == null)
			Logger.LogDebug("GetUser(userId={UserId}): user → {User}", userId, user);
            {
                response = this.Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Localization.GetString("UserNotFound", Components.Constants.LocalResourcesFile));
                return null;
            }

            if (!this.IsAdmin(user))
            {
                return user;
            }

            if ((user.IsSuperUser && !this.UserInfo.IsSuperUser) || !this.IsAdmin())
            {
                response = this.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized,
                    Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
                return null;
            }

            if (user.IsSuperUser)
            {
                user = UserController.Instance.GetUserById(Null.NullInteger, userId);
            }

			Logger.LogDebug("<<< Exiting GetUser(userId={UserId})", userId);
            return user;
        }
    }
}
