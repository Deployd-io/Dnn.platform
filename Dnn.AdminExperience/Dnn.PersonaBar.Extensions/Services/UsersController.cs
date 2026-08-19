// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = "Dnn.Users")]
    public class UsersController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersController));

        /// <summary>Create a User.</summary>
        /// <param name="contract">Information about the user to create.</param>
        /// <returns>A response with a (potentially <see langword="null"/>) <see cref="UserBasicDto"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AddUser)]
        public HttpResponseMessage CreateUser(CreateUserContract contract)
        {
			Logger.LogDebug(">>> Entering CreateUser(contract={Contract})", contract);
            try
            {
                var settings = new RegisterationDetails
                {
                    PortalSettings = this.PortalSettings,
                    Email = contract.Email,
                    FirstName = contract.FirstName,
                    LastName = contract.LastName,
                    UserName = contract.UserName,
                    Password = contract.Password,
                    Question = contract.Question,
                    Answer = contract.Answer,
                    Notify = contract.Notify,
                    Authorize = contract.Authorize,
                    RandomPassword = contract.RandomPassword,
                    IgnoreRegistrationMode = true,
                };

                var userInfo = RegisterController.Instance.Register(settings);
				Logger.LogDebug("CreateUser(contract={Contract}): userInfo → {UserInfo}", contract, userInfo);
                UserBasicDto response = null;
				Logger.LogDebug("CreateUser(contract={Contract}): response → {Response}", contract, response);
                if (userInfo != null)
                {
                    var userDetail = Components.UsersController.Instance.GetUserDetail(this.PortalId, userInfo.UserId);
					Logger.LogDebug("CreateUser(contract={Contract}): userDetail → {UserDetail}", contract, userDetail);
                    response = UserBasicDto.FromUserDetails(userDetail);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                if (ex.GetType() == typeof(InvalidUserRegisterException))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
                else
                {
					Logger.LogDebug("<<< Exiting CreateUser(contract={Contract})", contract);
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        /// <summary>Perform a search on Users registered in the Site.</summary>
        /// <param name="searchText">Search filter text (if any).</param>
        /// <param name="filter">User filter. Send -1 to disable.</param>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2).</param>
        /// <param name="pageSize">Number of records to return per page.</param>
        /// <param name="sortColumn">Column to sort on.</param>
        /// <param name="sortAscending">Sort ascending or descending.</param>
        /// <returns>A response with a collection of <see cref="UserBasicDto"/> and total results count.</returns>
        [HttpGet]
        public HttpResponseMessage GetUsers(string searchText, UserFilters filter, int pageIndex, int pageSize, string sortColumn, bool sortAscending)
        {
			Logger.LogDebug(">>> Entering GetUsers(searchText={SearchText},filter={Filter},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending})", searchText, filter, pageIndex, pageSize, sortColumn, sortAscending);
            try
            {
                int totalRecords;
                var getUsersContract = new GetUsersContract
                {
                    SearchText = searchText,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortColumn = sortColumn,
                    SortAscending = sortAscending,
                    PortalId = PortalController.GetEffectivePortalId(this.PortalId),
                    Filter = filter,
                };

                var results = Components.UsersController.Instance.GetUsers(getUsersContract, this.UserInfo.IsSuperUser, out totalRecords);
				Logger.LogDebug("GetUsers(searchText={SearchText},filter={Filter},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending}): results → {Results}", searchText, filter, pageIndex, pageSize, sortColumn, sortAscending, results);
                var response = new
                {
                    Results = results,
                    TotalResults = totalRecords,
                };
				Logger.LogDebug("GetUsers(searchText={SearchText},filter={Filter},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending}): TotalResults → {TotalResults}", searchText, filter, pageIndex, pageSize, sortColumn, sortAscending, TotalResults);
				Logger.LogDebug("GetUsers(searchText={SearchText},filter={Filter},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending}): Results → {Results}", searchText, filter, pageIndex, pageSize, sortColumn, sortAscending, Results);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetUsers(searchText={SearchText},filter={Filter},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending})", searchText, filter, pageIndex, pageSize, sortColumn, sortAscending);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUserFilters()
        {
			Logger.LogDebug(">>> Entering GetUserFilters()");
            try
            {
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    Components.UsersController.Instance.GetUserFilters(this.UserInfo.IsSuperUser));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetUserFilters()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Get User Detail Info.</summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A response with a <see cref="UserDetailDto"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetUserDetail(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserDetail(userId={UserId})", userId);
            try
            {
                var userDetail = Components.UsersController.Instance.GetUserDetail(this.PortalId, userId);
				Logger.LogDebug("GetUserDetail(userId={UserId}): userDetail → {UserDetail}", userId, userDetail);
                if (userDetail == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        Localization.GetString("UserNotFound", Components.Constants.LocalResourcesFile));
                }

                if (userDetail.IsSuperUser)
                {
                    if (!this.UserInfo.IsSuperUser)
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.Unauthorized,
                            Localization.GetString("InSufficientPermissions.Text", Components.Constants.LocalResourcesFile));
                    }

                    userDetail = Components.UsersController.Instance.GetUserDetail(Null.NullInteger, userId);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, userDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetUserDetail(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManagePassword)]
        public HttpResponseMessage ChangePassword(ChangePasswordDto changePasswordDto)
        {
			Logger.LogDebug(">>> Entering ChangePassword(changePasswordDto=<redacted>)");
            try
            {
                var userId = changePasswordDto.UserId;
				Logger.LogDebug("ChangePassword(changePasswordDto=<redacted>): userId → {UserId}", userId);
                var password = changePasswordDto.Password;
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("ChangePassword(changePasswordDto=<redacted>): user → {User}", user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var controller = Components.UsersController.Instance;
                controller.ChangePassword(this.PortalId, userId, password);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (InvalidPasswordException exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting ChangePassword(changePasswordDto=<redacted>)");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManagePassword)]
        public HttpResponseMessage ForceChangePassword([FromUri] int userId)
        {
			Logger.LogDebug(">>> Entering ForceChangePassword(userId={UserId})", userId);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("ForceChangePassword(userId={UserId}): user → {User}", userId, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                HttpResponseMessage httpResponseMessage;
                if (this.IsCurrentUser(userId, out httpResponseMessage))
                {
                    return httpResponseMessage;
                }

                return Components.UsersController.Instance.ForceChangePassword(user, this.PortalId, true)
                    ? this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, })
                    : this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting ForceChangePassword(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManagePassword)]
        public HttpResponseMessage SendPasswordResetLink([FromUri] int userId)
        {
			Logger.LogDebug(">>> Entering SendPasswordResetLink(userId={UserId})", userId);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("SendPasswordResetLink(userId={UserId}): user → {User}", userId, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var errorMessage = string.Empty;
				Logger.LogDebug("SendPasswordResetLink(userId={UserId}): errorMessage → {ErrorMessage}", userId, errorMessage);
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    errorMessage = Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile);
                }
                else
                {
                    try
                    {
                        // create resettoken
                        UserController.ResetPasswordToken(user, Host.AdminMembershipResetLinkValidity);

                        var canSend = Mail.SendMail(user, MessageType.PasswordReminder, this.PortalSettings) == string.Empty;
						Logger.LogDebug("SendPasswordResetLink(userId={UserId}): canSend → {CanSend}", userId, canSend);
                        if (!canSend)
                        {
                            errorMessage = Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile);
                        }
                        else
                        {
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                        }
                    }
                    catch (ArgumentException exc)
                    {
                        Logger.Error(exc);
                        errorMessage = Localization.GetString("InvalidPasswordAnswer", Components.Constants.LocalResourcesFile);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                        errorMessage = Localization.GetString("PasswordResetFailed", Components.Constants.LocalResourcesFile);
                    }
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, errorMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting SendPasswordResetLink(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AuthorizeUnAuthorizeUser)]
        public HttpResponseMessage UpdateAuthorizeStatus([FromUri] int userId, [FromUri] bool authorized)
        {
			Logger.LogDebug(">>> Entering UpdateAuthorizeStatus(userId={UserId},authorized={Authorized})", userId, authorized);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("UpdateAuthorizeStatus(userId={UserId},authorized={Authorized}): user → {User}", userId, authorized, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                HttpResponseMessage httpResponseMessage;
                if (this.IsCurrentUser(userId, out httpResponseMessage))
                {
                    return httpResponseMessage;
                }

                if (user.Membership.Approved == authorized)
                {
                    // Do nothing if the new status is same as current status.
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                }

                Components.UsersController.Instance.UpdateAuthorizeStatus(user, this.PortalId, authorized);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting UpdateAuthorizeStatus(userId={UserId},authorized={Authorized})", userId, authorized);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage SoftDeleteUser([FromUri] int userId)
        {
			Logger.LogDebug(">>> Entering SoftDeleteUser(userId={UserId})", userId);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("SoftDeleteUser(userId={UserId}): user → {User}", userId, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var deleted = !user.IsDeleted && UserController.DeleteUser(ref user, true, false);
				Logger.LogDebug("SoftDeleteUser(userId={UserId}): deleted → {Deleted}", userId, deleted);

                return !deleted
                    ? this.Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        Localization.GetString("UserDeleteError", Components.Constants.LocalResourcesFile))
                    : this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting SoftDeleteUser(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage HardDeleteUser([FromUri] int userId)
        {
			Logger.LogDebug(">>> Entering HardDeleteUser(userId={UserId})", userId);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("HardDeleteUser(userId={UserId}): user → {User}", userId, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var deleted = user.IsDeleted && UserController.RemoveUser(user);
				Logger.LogDebug("HardDeleteUser(userId={UserId}): deleted → {Deleted}", userId, deleted);
                return !deleted
                    ? this.Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        Localization.GetString("UserRemoveError", Components.Constants.LocalResourcesFile))
                    : this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting HardDeleteUser(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveDeletedUsers()
        {
			Logger.LogDebug(">>> Entering RemoveDeletedUsers()");
            if (!this.UserInfo.IsSuperUser)
            {
                if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
                }
            }

            UserController.RemoveDeletedUsers(this.PortalSettings.PortalId);
            var remaining = UserController.GetDeletedUsers(this.PortalSettings.PortalId);
			Logger.LogDebug("RemoveDeletedUsers(): remaining → {Remaining}", remaining);
            if (remaining.Count > 0)
            {
			Logger.LogDebug("<<< Exiting RemoveDeletedUsers()");
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("CouldNotRemoveAll", Components.Constants.LocalResourcesFile));
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage RestoreDeletedUser([FromUri] int userId)
        {
			Logger.LogDebug(">>> Entering RestoreDeletedUser(userId={UserId})", userId);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("RestoreDeletedUser(userId={UserId}): user → {User}", userId, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var restored = user.IsDeleted && UserController.RestoreUser(ref user);
				Logger.LogDebug("RestoreDeletedUser(userId={UserId}): restored → {Restored}", userId, restored);
                return !restored
                    ? this.Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        Localization.GetString("UserRestoreError", Components.Constants.LocalResourcesFile))
                    : this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting RestoreDeletedUser(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateSuperUserStatus([FromUri] int userId, [FromUri] bool setSuperUser)
        {
			Logger.LogDebug(">>> Entering UpdateSuperUserStatus(userId={UserId},setSuperUser={SetSuperUser})", userId, setSuperUser);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("UpdateSuperUserStatus(userId={UserId},setSuperUser={SetSuperUser}): user → {User}", userId, setSuperUser, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                user.IsSuperUser = setSuperUser;
				Logger.LogDebug("UpdateSuperUserStatus(userId={UserId},setSuperUser={SetSuperUser}): user.IsSuperUser → {Arg2}", userId, setSuperUser, user.IsSuperUser);

                // Update User
                UserController.UpdateUser(this.PortalId, user);
                DataCache.ClearCache();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting UpdateSuperUserStatus(userId={UserId},setSuperUser={SetSuperUser})", userId, setSuperUser);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.EditSettings)]
        public HttpResponseMessage UpdateUserBasicInfo(UserBasicDto userBasicDto)
        {
			Logger.LogDebug(">>> Entering UpdateUserBasicInfo(userBasicDto={UserBasicDto})", userBasicDto);
            try
            {
                Validate(userBasicDto);
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userBasicDto.UserId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("UpdateUserBasicInfo(userBasicDto={UserBasicDto}): user → {User}", userBasicDto, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var upadtedUser = Components.UsersController.Instance.UpdateUserBasicInfo(userBasicDto);
				Logger.LogDebug("UpdateUserBasicInfo(userBasicDto={UserBasicDto}): upadtedUser → {UpadtedUser}", userBasicDto, upadtedUser);

                return this.Request.CreateResponse(HttpStatusCode.OK, upadtedUser);
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("UsernameNotUnique", Components.Constants.LocalResourcesFile));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting UpdateUserBasicInfo(userBasicDto={UserBasicDto})", userBasicDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AuthorizeUnAuthorizeUser)]
        public HttpResponseMessage UnlockUser([FromUri] int userId)
        {
			Logger.LogDebug(">>> Entering UnlockUser(userId={UserId})", userId);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("UnlockUser(userId={UserId}): user → {User}", userId, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                HttpResponseMessage httpResponseMessage;
                if (this.IsCurrentUser(userId, out httpResponseMessage))
                {
                    return httpResponseMessage;
                }

                var unlocked = user.Membership.LockedOut && UserController.UnLockUser(user);
				Logger.LogDebug("UnlockUser(userId={UserId}): unlocked → {Unlocked}", userId, unlocked);
                return !unlocked
                    ? this.Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        Localization.GetString("UserUnlockError", Components.Constants.LocalResourcesFile))
                    : this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting UnlockUser(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage GetSuggestRoles(string keyword, int count)
        {
			Logger.LogDebug(">>> Entering GetSuggestRoles(keyword={Keyword},count={Count})", keyword, count);
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new List<UserRoleInfo>());
                }

                var isAdmin = Components.UsersController.IsAdmin(this.PortalSettings);
				Logger.LogDebug("GetSuggestRoles(keyword={Keyword},count={Count}): isAdmin → {IsAdmin}", keyword, count, isAdmin);

                var roles = RoleController.Instance.GetRoles(
                    this.PortalId,
                    x => x.RoleName.ToUpperInvariant().Contains(keyword.ToUpperInvariant()));
                var matchedRoles = roles
                    .Where(
                        r =>
                            (isAdmin || r.RoleID != this.PortalSettings.AdministratorRoleId) &&
                            r.Status == RoleStatus.Approved)
                    .ToList().Take(count).Select(u => new UserRoleInfo
                    {
                        RoleID = u.RoleID,
                        RoleName = $"{u.RoleName}",
                        SecurityMode = u.SecurityMode,
                    });

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    matchedRoles.ToList().Select(r => UserRoleDto.FromRoleInfo(this.PortalSettings, r)));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetSuggestRoles(keyword={Keyword},count={Count})", keyword, count);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage GetUserRoles(string keyword, int userId, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetUserRoles(keyword={Keyword},userId={UserId},pageIndex={PageIndex},pageSize={PageSize})", keyword, userId, pageIndex, pageSize);
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("GetUserRoles(keyword={Keyword},userId={UserId},pageIndex={PageIndex},pageSize={PageSize}): user → {User}", keyword, userId, pageIndex, pageSize, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                int totalRoles;
                var userRoles = Components.UsersController.Instance.GetUserRoles(user, keyword, out totalRoles, pageIndex, pageSize)
                        .Select(r => UserRoleDto.FromRoleInfo(this.PortalSettings, r));

                return this.Request.CreateResponse(HttpStatusCode.OK, new { UserRoles = userRoles, TotalRecords = totalRoles });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetUserRoles(keyword={Keyword},userId={UserId},pageIndex={PageIndex},pageSize={PageSize})", keyword, userId, pageIndex, pageSize);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage SaveUserRole(UserRoleDto userRoleDto, [FromUri] bool notifyUser, [FromUri] bool isOwner)
        {
			Logger.LogDebug(">>> Entering SaveUserRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", userRoleDto, notifyUser, isOwner);
            try
            {
                Validate(userRoleDto);
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userRoleDto.UserId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("SaveUserRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): user → {User}", userRoleDto, notifyUser, isOwner, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                var result = Components.UsersController.Instance.SaveUserRole(
                    this.PortalId,
                    this.UserInfo,
                    userRoleDto,
                    notifyUser,
                    isOwner);

                return this.Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting SaveUserRole(userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", userRoleDto, notifyUser, isOwner);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage RemoveUserRole(UserRoleDto userRoleDto)
        {
			Logger.LogDebug(">>> Entering RemoveUserRole(userRoleDto={UserRoleDto})", userRoleDto);
            try
            {
                Validate(userRoleDto);
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userRoleDto.UserId, this.PortalSettings, this.UserInfo, out response);
				Logger.LogDebug("RemoveUserRole(userRoleDto={UserRoleDto}): user → {User}", userRoleDto, user);
                if (user == null)
                {
                    return this.Request.CreateErrorResponse(response.Key, response.Value);
                }

                RoleController.Instance.UpdateUserRole(
                    this.PortalId,
                    userRoleDto.UserId,
                    userRoleDto.RoleId,
                    RoleStatus.Approved,
                    false,
                    true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting RemoveUserRole(userRoleDto={UserRoleDto})", userRoleDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // User profile is not implemented currently
        //        [HttpGet]
        //        public HttpResponseMessage GetProfileDefinitions()
        //        {
        //            var profileDefinitions = ProfileController.GetPropertyDefinitionsByPortal(PortalId)
        //                .Cast<ProfilePropertyDefinition>().Select(d => new ProfileDefinitionDto(d));
        //
        //            return Request.CreateResponse(HttpStatusCode.OK, profileDefinitions);
        //        }
        //
        //        [HttpGet]
        //        public HttpResponseMessage GetUserProfile(int userId)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
        //        }

        /// <summary>Return Password security options from server.</summary>
        /// <returns>MembershipPasswordSettings.</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage PasswordStrengthOptions()
        {
			Logger.LogDebug(">>> Entering PasswordStrengthOptions()");
            var settings = new MembershipPasswordSettings(this.PortalId);

			Logger.LogDebug("PasswordStrengthOptions(): settings → {Settings}", settings);
            var passwordSettings = new PasswordSettingsDto
            {
                MinLength = settings.MinPasswordLength,
                MinNumberOfSpecialChars = settings.MinNonAlphanumericCharacters,
                ValidationExpression = settings.ValidationExpression,
            };

			Logger.LogDebug("<<< Exiting PasswordStrengthOptions()");
            return this.Request.CreateResponse(HttpStatusCode.OK, passwordSettings);
        }

        private static void Validate(UserRoleDto userRoleDto)
        {
			Logger.LogDebug(">>> Entering Validate(userRoleDto={UserRoleDto})", userRoleDto);
            Requires.NotNegative("UserId", userRoleDto.UserId);
            Requires.NotNegative("RoleId", userRoleDto.RoleId);
			Logger.LogDebug("<<< Exiting Validate(userRoleDto={UserRoleDto})", userRoleDto);
        }

        private static void Validate(UserBasicDto userBasicDto)
        {
			Logger.LogDebug(">>> Entering Validate(userBasicDto={UserBasicDto})", userBasicDto);
            Requires.NotNegative("UserId", userBasicDto.UserId);
			Logger.LogDebug("<<< Exiting Validate(userBasicDto={UserBasicDto})", userBasicDto);
        }

        private bool IsCurrentUser(int userId, out HttpResponseMessage response)
        {
			Logger.LogDebug(">>> Entering IsCurrentUser(userId={UserId})", userId);
            response = null;
            if (userId == this.UserInfo.UserID)
            {
                response = this.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized,
                    Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
			Logger.LogDebug("<<< Exiting IsCurrentUser(userId={UserId})", userId);
                return true;
            }

            return false;
        }
    }
}
