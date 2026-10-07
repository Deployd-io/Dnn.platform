// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Components
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web.Security;

    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using Dnn.PersonaBar.Users.Components.Helpers;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;

    using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

    public class UsersController : ServiceLocator<IUsersController, UsersController>, IUsersController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Services.UsersController));

        private static PortalSettings PortalSettings => PortalSettings.Current;

        public static UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo userInfo, out KeyValuePair<HttpStatusCode, string> response)
        {
			Logger.LogError(">>> Entering GetUser(userId={UserId},portalSettings={PortalSettings},userInfo={UserInfo})", userId, portalSettings, userInfo);
            response = default(KeyValuePair<HttpStatusCode, string>);
            var user = UserController.Instance.GetUserById(portalSettings.PortalId, userId);
			Logger.LogDebug("GetUser(userId={UserId},portalSettings={PortalSettings},userInfo={UserInfo}): response → {Response}", userId, portalSettings, userInfo, response);
            if (user == null)
            {
				Logger.LogError("GetUser(userId={UserId},portalSettings={PortalSettings},userInfo={UserInfo}): user → {User}", userId, portalSettings, userInfo, user);
                response = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("UserNotFound", Constants.LocalResourcesFile));
				Logger.LogError("GetUser(userId={UserId},portalSettings={PortalSettings},userInfo={UserInfo}): response → {Response}", userId, portalSettings, userInfo, response);
                return null;
            }

            if (!IsAdmin(user, portalSettings))
            {
                return user;
            }

            if ((user.IsSuperUser && !userInfo.IsSuperUser) || !IsAdmin(portalSettings))
            {
                response = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.Unauthorized, Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
                return null;
            }

            if (user.IsSuperUser)
            {
                user = UserController.Instance.GetUserById(Null.NullInteger, userId);
            }

			Logger.LogWarning("<<< Exiting GetUser(userId={UserId},portalSettings={PortalSettings},userInfo={UserInfo})", userId, portalSettings, userInfo);
            return user;
        }

        public static bool IsAdmin(PortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering IsAdmin(portalSettings={PortalSettings})", portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("IsAdmin(portalSettings={PortalSettings}): user → {User}", portalSettings, user);
			Logger.LogWarning("<<< Exiting IsAdmin(portalSettings={PortalSettings})", portalSettings);
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }

        /// <inheritdoc />
        public IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract, bool isSuperUser, out int totalRecords)
        {
			Logger.LogError(">>> Entering GetUsers(usersContract={UsersContract},isSuperUser={IsSuperUser})", usersContract, isSuperUser);
            Logger.LogWarning("<<< Exiting GetUsers(usersContract={UsersContract},isSuperUser={IsSuperUser})", usersContract, isSuperUser);
			Logger.LogWarning("<<< Exiting GetUsers(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval})", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval);
            return this.GetUsersFromDb(usersContract, isSuperUser, out totalRecords) ?? new List<UserBasicDto>();
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, int>> GetUserFilters(bool isSuperUser = false)
        {
            var userFilters = new List<KeyValuePair<string, int>>();
            foreach (var filter in Enum.GetValues(typeof(UserFilters)).Cast<UserFilters>())
            {
                userFilters.Add(
                    new KeyValuePair<string, int>(
                        Localization.GetString(Convert.ToString(filter, CultureInfo.InvariantCulture), Constants.LocalResourcesFile), (int)filter));
            }

            if (!isSuperUser)
            {
                userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == (int)UserFilters.SuperUsers));
            }

            if (!PortalSettings.DataConsentActive)
            {
                userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == (int)UserFilters.HasAgreedToTerms));
                userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == (int)UserFilters.HasNotAgreedToTerms));
                userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == (int)UserFilters.RequestedRemoval));
            }

            userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == (int)UserFilters.RegisteredUsers)); // Temporarily removed registered users.
            return userFilters;
        }

        /// <inheritdoc />
        public UserDetailDto GetUserDetail(int portalId, int userId)
        {
			Logger.LogError(">>> Entering GetUserDetail(portalId={PortalId},userId={UserId})", portalId, userId);
            var user = UserController.Instance.GetUserById(portalId, userId);
			Logger.LogError("GetUserDetail(portalId={PortalId},userId={UserId}): user → {User}", portalId, userId, user);
            if (user == null)
            {
                return null;
            }

            user.PortalID = portalId;
			Logger.LogDebug("GetUserDetail(portalId={PortalId},userId={UserId}): user.PortalID → {Arg2}", portalId, userId, user.PortalID);
			Logger.LogWarning("<<< Exiting GetUserDetail(portalId={PortalId},userId={UserId})", portalId, userId);
            return new UserDetailDto(user);
        }

        /// <inheritdoc />
        public bool ChangePassword(int portalId, int userId, string newPassword)
        {
			Logger.LogError(">>> Entering ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>)", portalId, userId);
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
				Logger.LogError("Throwing InvalidOperationException from ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>)", portalId, userId);
                throw new InvalidOperationException(Localization.GetString("CannotChangePassword", Constants.LocalResourcesFile));
            }

            var user = UserController.Instance.GetUserById(portalId, userId);
			Logger.LogError("ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>): user → {User}", portalId, userId, user);
            if (user == null)
            {
                return false;
            }

            var membershipPasswordController = new MembershipPasswordController();
			Logger.LogError("ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>): membershipPasswordController → {MembershipPasswordController}", portalId, userId, membershipPasswordController);
            var settings = new MembershipPasswordSettings(user.PortalID);
			Logger.LogError("ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>): settings → {Settings}", portalId, userId, settings);

            if (settings.EnableBannedList)
            {
                if (membershipPasswordController.FoundBannedPassword(newPassword) || user.Username == newPassword)
                {
					Logger.LogError("Throwing InvalidPasswordException from ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>)", portalId, userId);
                    throw new InvalidPasswordException(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile));
                }
            }

            // check new password is not in history
            if (membershipPasswordController.IsPasswordInHistory(user.UserID, user.PortalID, newPassword, false))
            {
                throw new InvalidPasswordException(Localization.GetString("PasswordResetFailed_PasswordInHistory", Constants.LocalResourcesFile));
            }

            try
            {
                var passwordChanged = UserController.ResetAndChangePassword(user, newPassword);
				Logger.LogError("ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>): passwordChanged → {PasswordChanged}", portalId, userId, passwordChanged);
                if (!passwordChanged)
                {
                    throw new InvalidPasswordException(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile));
                }

                return true;
            }
            catch (MembershipPasswordException exc)
            {
                // Password Answer missing
                Logger.Error(exc);
                throw new InvalidPasswordException(Localization.GetString("PasswordInvalid", Constants.LocalResourcesFile), exc);
            }
            catch (ThreadAbortException)
            {
				Logger.LogWarning("<<< Exiting ChangePassword(portalId={PortalId},userId={UserId},newPassword=<redacted>)", portalId, userId);
                return true;
            }
            catch (InvalidPasswordException exc)
            {
                // Password validation has failed
                Logger.Error(exc);
                throw new InvalidPasswordException(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile), exc);
            }
            catch (Exception exc)
            {
                // Fail
                Logger.Error(exc);
                throw new InvalidPasswordException(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile), exc);
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public UserBasicDto UpdateUserBasicInfo(UserBasicDto userBasicDto, int requestPortalId = -1)
        {
			Logger.LogError(">>> Entering UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId})", userBasicDto, requestPortalId);
            int portalId = PortalSettings.PortalId;
            PortalSettings requestPortalSettings = PortalSettings;
			Logger.LogError("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): requestPortalSettings → {RequestPortalSettings}", userBasicDto, requestPortalId, requestPortalSettings);
			Logger.LogError("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): portalId → {PortalId}", userBasicDto, requestPortalId, portalId);

            if (requestPortalId != -1)
            {
                portalId = requestPortalId;
                requestPortalSettings = new PortalSettings(portalId);
            }

            var user = UserController.Instance.GetUser(portalId, userBasicDto.UserId);
			Logger.LogError("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): user → {User}", userBasicDto, requestPortalId, user);

            if (user == null)
            {
				Logger.LogError("Throwing ArgumentException from UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId})", userBasicDto, requestPortalId);
                throw new ArgumentException("UserNotExist");
            }

            if (userBasicDto.UserId == requestPortalSettings.AdministratorId)
            {
                // Clear the Portal Cache
                DataCache.ClearPortalCache(portalId, true);
            }

            if (user.IsSuperUser)
            {
                DataCache.ClearHostCache(true);
            }

            user.DisplayName = userBasicDto.Displayname;
			Logger.LogDebug("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): user.DisplayName → {Arg2}", userBasicDto, requestPortalId, user.DisplayName);
            user.Email = userBasicDto.Email;
			Logger.LogDebug("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): user.Email → {Arg2}", userBasicDto, requestPortalId, user.Email);
            user.FirstName = !string.IsNullOrEmpty(userBasicDto.Firstname) ? userBasicDto.Firstname : user.FirstName;
            user.LastName = !string.IsNullOrEmpty(userBasicDto.Lastname) ? userBasicDto.Lastname : user.LastName;

            // Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(requestPortalSettings.Registration.DisplayNameFormat))
            {
                user.UpdateDisplayName(requestPortalSettings.Registration.DisplayNameFormat);
            }

            // either update the username or update the user details
            // only call ChangeUsername when the username has actually changed
            if (userBasicDto.Username != user.Username && CanUpdateUsername(user) && !requestPortalSettings.Registration.UseEmailAsUserName)
            {
                UserController.ChangeUsername(user.UserID, userBasicDto.Username);
                user.Username = userBasicDto.Username;
				Logger.LogDebug("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): user.Username → {Arg2}", userBasicDto, requestPortalId, user.Username);
            }

            // DNN-5874 Check if unique display name is required
            if (requestPortalSettings.Registration.RequireUniqueDisplayName)
            {
                var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(portalId, 0, 2, "DisplayName", true, "DisplayName", user.DisplayName);
				Logger.LogError("UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId}): usersWithSameDisplayName → {UsersWithSameDisplayName}", userBasicDto, requestPortalId, usersWithSameDisplayName);
                if (usersWithSameDisplayName.Any(u => u.UserID != user.UserID))
                {
                    throw new ArgumentException("DisplayNameNotUnique");
                }
            }

            UserController.UpdateUser(portalId, user);

            if (requestPortalSettings.Registration.UseEmailAsUserName && (!user.Username.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                UserController.ChangeUsername(user.UserID, user.Email);
            }

				Logger.LogWarning("<<< Exiting UpdateUserBasicInfo(userBasicDto={UserBasicDto},requestPortalId={RequestPortalId})", userBasicDto, requestPortalId);
            return
                UserBasicDto.FromUserInfo(UserController.Instance.GetUser(requestPortalSettings.PortalId, userBasicDto.UserId));
        }

        /// <inheritdoc />
        public UserRoleDto SaveUserRole(int portalId, UserInfo currentUserInfo, UserRoleDto userRoleDto, bool notifyUser, bool isOwner)
        {
			Logger.LogError(">>> Entering SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner);
            PortalSettings portalSettings = PortalSettings;

			Logger.LogError("SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): portalSettings → {PortalSettings}", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner, portalSettings);
            if (PortalSettings.PortalId != portalId)
            {
                portalSettings = GetPortalSettings(portalId);
            }

            if (!UserRoleDto.AllowExpiredRole(portalSettings, userRoleDto.UserId, userRoleDto.RoleId))
            {
                userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
				Logger.LogDebug("SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): userRoleDto.StartTime → {Arg5}", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner, userRoleDto.StartTime);
            }

            var user = UserController.Instance.GetUserById(portalId, userRoleDto.UserId);
			Logger.LogError("SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): user → {User}", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner, user);
            var role = RoleController.Instance.GetRoleById(portalId, userRoleDto.RoleId);
			Logger.LogError("SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): role → {Role}", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner, role);
            if (role is not { Status: RoleStatus.Approved })
            {
				Logger.LogError("Throwing RoleNotApprovedException from SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner);
                throw new RoleNotApprovedException(Localization.GetString("RoleIsNotApproved", Constants.LocalResourcesFile));
            }

            if (currentUserInfo.IsSuperUser || currentUserInfo.Roles.Contains(portalSettings.AdministratorRoleName) ||
                (!currentUserInfo.IsSuperUser && !currentUserInfo.Roles.Contains(portalSettings.AdministratorRoleName) &&
                 role.RoleType != RoleType.Administrator))
            {
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                {
                    isOwner = false;
                }

                RoleController.AddUserRole(user, role, portalSettings, RoleStatus.Approved, userRoleDto.StartTime, userRoleDto.ExpiresTime, notifyUser, isOwner);
                var addedRole = RoleController.Instance.GetUserRole(portalId, userRoleDto.UserId, userRoleDto.RoleId);
				Logger.LogError("SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner}): addedRole → {AddedRole}", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner, addedRole);

                return new UserRoleDto
                {
                    UserId = addedRole.UserID,
                    RoleId = addedRole.RoleID,
                    DisplayName = addedRole.FullName,
                    RoleName = addedRole.RoleName,
                    StartTime = addedRole.EffectiveDate,
                    ExpiresTime = addedRole.ExpiryDate,
                    AllowExpired = UserRoleDto.AllowExpiredRole(portalSettings, user.UserID, role.RoleID),
                    AllowDelete = RoleController.CanRemoveUserFromRole(portalSettings, user.UserID, role.RoleID),
                };
            }

			Logger.LogError("Throwing SecurityException from SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner);
			Logger.LogWarning("<<< Exiting SaveUserRole(portalId={PortalId},currentUserInfo={CurrentUserInfo},userRoleDto={UserRoleDto},notifyUser={NotifyUser},isOwner={IsOwner})", portalId, currentUserInfo, userRoleDto, notifyUser, isOwner);
            throw new SecurityException(Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
        }

        /// <inheritdoc />
        public bool ForceChangePassword(UserInfo userInfo, int portalId, bool notify)
        {
			Logger.LogError(">>> Entering ForceChangePassword(userInfo={UserInfo},portalId={PortalId},notify={Notify})", userInfo, portalId, notify);
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                var canSend = UserController.ResetPasswordToken(userInfo, notify);
				Logger.LogError("ForceChangePassword(userInfo={UserInfo},portalId={PortalId},notify={Notify}): canSend → {CanSend}", userInfo, portalId, notify, canSend);
                if (canSend || !notify)
                {
                    userInfo.Membership.UpdatePassword = true;

                    // Update User
                    UserController.UpdateUser(portalId, userInfo);
                    return true;
                }
            }

			Logger.LogWarning("<<< Exiting ForceChangePassword(userInfo={UserInfo},portalId={PortalId},notify={Notify})", userInfo, portalId, notify);
            return false;
        }

        /// <inheritdoc />
        public void AddUserToRoles(UserInfo currentUserInfo, int userId, int portalId, string roleNames, string roleDelimiter = ",", DateTime? effectiveDate = null, DateTime? expiryDate = null)
        {
			Logger.LogError(">>> Entering AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate})", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate);
            var effDate = effectiveDate.GetValueOrDefault(Null.NullDate);
            var expDate = expiryDate.GetValueOrDefault(Null.NullDate);
			Logger.LogError("AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate}): expDate → {ExpDate}", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate, expDate);
			Logger.LogError("AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate}): effDate → {EffDate}", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate, effDate);

            // get the specified RoleName
            var roleController = new RoleController();
			Logger.LogError("AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate}): roleController → {RoleController}", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate, roleController);
            var lstRoles = roleNames.Split(roleDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			Logger.LogError("AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate}): lstRoles → {LstRoles}", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate, lstRoles);
            foreach (var curRole in lstRoles)
            {
                var role = roleController.GetRoleByName(portalId, curRole);
				Logger.LogError("AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate}): role → {Role}", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate, role);
                if (role == null)
                {
                    continue;
                }

                var userRoleDto = new UserRoleDto
                {
                    RoleId = role.RoleID,
                    UserId = userId,
                    StartTime = effDate,
                    ExpiresTime = expDate,
                    RoleName = curRole,
                };
                Instance.SaveUserRole(portalId, currentUserInfo, userRoleDto, false, false);
            }
			Logger.LogWarning("<<< Exiting AddUserToRoles(currentUserInfo={CurrentUserInfo},userId={UserId},portalId={PortalId},roleNames={RoleNames},roleDelimiter={RoleDelimiter},effectiveDate={EffectiveDate},expiryDate={ExpiryDate})", currentUserInfo, userId, portalId, roleNames, roleDelimiter, effectiveDate, expiryDate);
        }

        /// <inheritdoc />
        public IList<UserRoleInfo> GetUserRoles(UserInfo user, string keyword, out int total, int pageIndex = -1, int pageSize = -1)
        {
			Logger.LogError(">>> Entering GetUserRoles(user={User},keyword={Keyword},pageIndex={PageIndex},pageSize={PageSize})", user, keyword, pageIndex, pageSize);
			Logger.LogError("GetUserRoles(user={User},keyword={Keyword},pageIndex={PageIndex},pageSize={PageSize}): pageIndex → {PageIndex1}", user, keyword, pageIndex, pageSize, pageIndex);
            var roles = RoleController.Instance.GetUserRoles(user, true);
            if (!string.IsNullOrEmpty(keyword))
            {
                roles = roles.Where(u => u.FullName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
				Logger.LogError("GetUserRoles(user={User},keyword={Keyword},pageIndex={PageIndex},pageSize={PageSize}): roles → {Roles}", user, keyword, pageIndex, pageSize, roles);
            }

            total = roles.Count;
			Logger.LogError("GetUserRoles(user={User},keyword={Keyword},pageIndex={PageIndex},pageSize={PageSize}): total → {Total}", user, keyword, pageIndex, pageSize, total);
			Logger.LogDebug("GetUserRoles(user={User},keyword={Keyword},pageIndex={PageIndex},pageSize={PageSize}): total → {Total}", user, keyword, pageIndex, pageSize, total);
            pageSize = pageSize > 0 && pageSize < 500 ? pageSize : 500;
            return pageIndex == -1 ? roles : roles.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<UserInfo> GetUsersInRole(PortalSettings portalSettings, string roleName, out int total, out KeyValuePair<HttpStatusCode, string> message, int pageIndex = -1, int pageSize = -1)
        {
			Logger.LogError(">>> Entering GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize})", portalSettings, roleName, pageIndex, pageSize);
            message = default(KeyValuePair<HttpStatusCode, string>);
            total = 0;
			Logger.LogError("GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize}): total → {Total}", portalSettings, roleName, pageIndex, pageSize, total);
			Logger.LogDebug("GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize}): message → {Message}", portalSettings, roleName, pageIndex, pageSize, message);
            var role = RoleController.Instance.GetRoleByName(portalSettings.PortalId, roleName);
            if (role == null)
            {
				Logger.LogError("GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize}): role → {Role}", portalSettings, roleName, pageIndex, pageSize, role);
				Logger.LogDebug("GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize}): total → {Total}", portalSettings, roleName, pageIndex, pageSize, total);
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Constants.LocalResourcesFile));
				Logger.LogError("GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize}): message → {Message}", portalSettings, roleName, pageIndex, pageSize, message);
                return null;
            }

            if (role.RoleID == PortalSettings.AdministratorRoleId && !IsAdmin(portalSettings))
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return null;
            }

            var users = RoleController.Instance.GetUsersByRole(portalSettings.PortalId, role.RoleName);
			Logger.LogError("GetUsersInRole(portalSettings={PortalSettings},roleName={RoleName},pageIndex={PageIndex},pageSize={PageSize}): users → {Users}", portalSettings, roleName, pageIndex, pageSize, users);
            total = users.Count;
            var startIndex = pageIndex * pageSize;
            return users.Skip(startIndex).Take(pageSize);
        }

        /// <inheritdoc />
        public void UpdateAuthorizeStatus(UserInfo userInfo, int portalId, bool authorized)
        {
			Logger.LogError(">>> Entering UpdateAuthorizeStatus(userInfo={UserInfo},portalId={PortalId},authorized={Authorized})", userInfo, portalId, authorized);
            userInfo.Membership.Approved = authorized;

            // Update User
            UserController.UpdateUser(portalId, userInfo);
            if (authorized)
            {
                // Update User Roles if needed
                if (!userInfo.IsSuperUser && userInfo.IsInRole("Unverified Users") &&
                    PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    UserController.ApproveUser(userInfo);
                }

                Mail.SendMail(userInfo, MessageType.UserAuthorized, PortalSettings);
            }
            else if (PortalController.GetPortalSettingAsBoolean("AlwaysSendUserUnAuthorizedEmail", portalId, false))
            {
                Mail.SendMail(userInfo, MessageType.UserUnAuthorized, PortalSettings);
            }
			Logger.LogWarning("<<< Exiting UpdateAuthorizeStatus(userInfo={UserInfo},portalId={PortalId},authorized={Authorized})", userInfo, portalId, authorized);
        }

        /// <inheritdoc />
        protected override Func<IUsersController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new UsersController();
        }

        protected virtual IDataReader CallGetUsersBySearchTerm(GetUsersContract usersContract, bool? includeAuthorized, bool? includeDeleted, bool? includeSuperUsers, bool? hasAgreedToTerms, bool? requestsRemoval)
        {
			Logger.LogError(">>> Entering CallGetUsersBySearchTerm(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval})", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval);
            var parsedSearchText = string.IsNullOrEmpty(usersContract.SearchText) ? string.Empty : SearchTextFilter.CleanWildcards(usersContract.SearchText.Trim());

			Logger.LogError("CallGetUsersBySearchTerm(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval}): parsedSearchText → {ParsedSearchText}", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval, parsedSearchText);
			Logger.LogWarning("<<< Exiting CallGetUsersBySearchTerm(usersContract={UsersContract},includeAuthorized={IncludeAuthorized},includeDeleted={IncludeDeleted},includeSuperUsers={IncludeSuperUsers},hasAgreedToTerms={HasAgreedToTerms},requestsRemoval={RequestsRemoval})", usersContract, includeAuthorized, includeDeleted, includeSuperUsers, hasAgreedToTerms, requestsRemoval);
            return DataProvider.Instance().ExecuteReader(
                    "Personabar_GetUsersBySearchTerm",
                    usersContract.PortalId,
                    string.IsNullOrEmpty(usersContract.SortColumn) ? "Joined" : usersContract.SortColumn,
                    usersContract.SortAscending,
                    usersContract.PageIndex,
                    usersContract.PageSize,
                    parsedSearchText,
                    includeAuthorized,
                    includeDeleted,
                    includeSuperUsers,
                    hasAgreedToTerms,
                    requestsRemoval);
        }

        private static IEnumerable<UserBasicDto> GetSortedUsers(IEnumerable<UserBasicDto> users, string sortColumn, bool sortAscending = false)
        {
			Logger.LogError(">>> Entering GetSortedUsers(users={Users},sortColumn={SortColumn},sortAscending={SortAscending})", users, sortColumn, sortAscending);
            switch (sortColumn?.ToLowerInvariant())
            {
                case "displayname":
                    return sortAscending
                        ? users.OrderBy(x => x.Displayname)
                        : users.OrderByDescending(x => x.Displayname);
                case "email":
                    return sortAscending
                        ? users.OrderBy(x => x.Email)
                        : users.OrderByDescending(x => x.Email);
                default:
					Logger.LogWarning("<<< Exiting GetSortedUsers(users={Users},sortColumn={SortColumn},sortAscending={SortAscending})", users, sortColumn, sortAscending);
                    return sortAscending
                        ? users.OrderBy(x => x.CreatedOnDate)
                        : users.OrderByDescending(x => x.CreatedOnDate);
            }
        }

        private static IEnumerable<UserInfo> GetPagedUsers(IEnumerable<UserInfo> users, int pageSize, int pageIndex)
        {
			Logger.LogError(">>> Entering GetPagedUsers(users={Users},pageSize={PageSize},pageIndex={PageIndex})", users, pageSize, pageIndex);
				Logger.LogWarning("<<< Exiting GetPagedUsers(users={Users},pageSize={PageSize},pageIndex={PageIndex})", users, pageSize, pageIndex);
            return
                users.Skip(pageIndex * pageSize).Take(pageSize);
        }

        private static bool IsAdmin(UserInfo user, PortalSettings portalSettings)
        {
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }

        private static bool CanUpdateUsername(UserInfo user)
        {
			Logger.LogError(">>> Entering CanUpdateUsername(user={User})", user);
            // can only update username if a host/admin and account being managed is not a superuser
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                // only allow updates for non-superuser accounts
                if (!user.IsSuperUser)
                {
                    return true;
                }
            }

            // if an admin, check if the user is only within this portal
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(PortalSettings.AdministratorRoleName))
            {
                // only allow updates for non-superuser accounts
                if (user.IsSuperUser)
                {
                    return false;
                }

                if (PortalController.GetPortalsByUser(user.UserID).Count == 1)
                {
                    return true;
                }
            }

			Logger.LogWarning("<<< Exiting CanUpdateUsername(user={User})", user);
            return false;
        }

        private static PortalSettings GetPortalSettings(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalSettings(portalId={PortalId})", portalId);
            var portalSettings = new PortalSettings(portalId);
            var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
			Logger.LogError("GetPortalSettings(portalId={PortalId}): portalAliases → {PortalAliases}", portalId, portalAliases);
			Logger.LogError("GetPortalSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
            portalSettings.PrimaryAlias = portalAliases.FirstOrDefault(a => a.IsPrimary);
            portalSettings.PortalAlias = PortalAliasController.Instance.GetPortalAlias(portalSettings.DefaultPortalAlias);
			Logger.LogWarning("<<< Exiting GetPortalSettings(portalId={PortalId})", portalId);
            return portalSettings;
        }

        private IEnumerable<UserBasicDto> GetUsersFromDb(GetUsersContract usersContract, bool isSuperUser, out int totalRecords)
        {
			Logger.LogError(">>> Entering GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser})", usersContract, isSuperUser);
            totalRecords = 0;
            IEnumerable<UserBasicDto> users = null;

			Logger.LogDebug("GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser}): totalRecords → {TotalRecords}", usersContract, isSuperUser, totalRecords);
			Logger.LogError("GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser}): totalRecords → {TotalRecords}", usersContract, isSuperUser, totalRecords);
            var portalId = usersContract.PortalId;
            var pageIndex = usersContract.PageIndex;
            var pageSize = usersContract.PageSize;

            switch (usersContract.Filter)
            {
                case UserFilters.All:
                    users = this.GetUsers(usersContract, null, null, isSuperUser ? null : (bool?)false, null, null, out totalRecords);
					Logger.LogError("GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser}): users → {Users}", usersContract, isSuperUser, users);
                    break;
                case UserFilters.Authorized:
                    users = this.GetUsers(usersContract, true, false, isSuperUser ? null : (bool?)false, null, null, out totalRecords);
                    break;
                case UserFilters.SuperUsers:
                    if (isSuperUser)
                    {
                        users = this.GetUsers(usersContract, null, null, true, null, null, out totalRecords);
                    }

                    break;
                case UserFilters.UnAuthorized:
                    users = this.GetUsers(usersContract, false, false, isSuperUser ? null : (bool?)false, null, null, out totalRecords);
                    break;
                case UserFilters.Deleted:
                    users = this.GetUsers(usersContract, null, true, isSuperUser ? null : (bool?)false, null, null, out totalRecords);
                    break;
                case UserFilters.HasAgreedToTerms:
                    users = this.GetUsers(usersContract, null, null, false, true, null, out totalRecords);
                    break;
                case UserFilters.HasNotAgreedToTerms:
                    users = this.GetUsers(usersContract, null, null, false, false, null, out totalRecords);
                    break;
                case UserFilters.RequestedRemoval:
                    users = this.GetUsers(usersContract, null, null, false, null, true, out totalRecords);
                    break;
                case UserFilters.RegisteredUsers:
                    {
                        IList<UserInfo> userInfos = RoleController.Instance.GetUsersByRole(
                            portalId,
                            PortalController.Instance.GetCurrentSettings().RegisteredRoleName);
                        if (!isSuperUser)
                        {
                            userInfos = (IList<UserInfo>)userInfos?.Where(x => !x.IsSuperUser);
							Logger.LogError("GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser}): userInfos → {UserInfos}", usersContract, isSuperUser, userInfos);
                        }

                        if (userInfos != null)
                        {
                            totalRecords = userInfos.Count;
                            users = GetSortedUsers(
                                GetPagedUsers(userInfos, pageSize, pageIndex)?.Select(UserBasicDto.FromUserInfo),
                                usersContract.SortColumn,
                                usersContract.SortAscending);
                        }

                        break;
                    }

                default:
					Logger.LogError("Throwing ArgumentOutOfRangeException from GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser})", usersContract, isSuperUser);
			Logger.LogWarning("<<< Exiting GetUsersFromDb(usersContract={UsersContract},isSuperUser={IsSuperUser})", usersContract, isSuperUser);
                    throw new ArgumentOutOfRangeException(nameof(usersContract), usersContract, $"Filter was an unexpected value, supported values include {nameof(UserFilters.All)}, {nameof(UserFilters.Authorized)}, {nameof(UserFilters.SuperUsers)}, {nameof(UserFilters.UnAuthorized)}, {nameof(UserFilters.Deleted)}, {nameof(UserFilters.HasAgreedToTerms)}, {nameof(UserFilters.HasNotAgreedToTerms)}, {nameof(UserFilters.RequestedRemoval)}, and {nameof(UserFilters.RegisteredUsers)}.");
            }

            return users;
        }

        private List<UserBasicDto2> GetUsers(GetUsersContract usersContract, bool? includeAuthorized, bool? includeDeleted, bool? includeSuperUsers, bool? hasAgreedToTerms, bool? requestsRemoval, out int totalRecords)
        {
            var parsedSearchText = string.IsNullOrEmpty(usersContract.SearchText) ? string.Empty : SearchTextFilter.CleanWildcards(usersContract.SearchText.Trim());

            usersContract.SearchText = $"{parsedSearchText}*";

            var records = CBO.FillCollection<UserBasicDto2>(
                this.CallGetUsersBySearchTerm(
                    usersContract,
                    includeAuthorized,
                    includeDeleted,
                    includeSuperUsers,
                    hasAgreedToTerms,
                    requestsRemoval));

            foreach (var user in records)
            {
                user.PopulateAvatarUrl();
            }

            totalRecords = records.Count == 0 ? 0 : records[0].TotalCount;
            return records;
        }
    }
}
