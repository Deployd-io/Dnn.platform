// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The AspNetMembershipProvider overrides the default MembershipProvider to provide an AspNet Membership Component (MemberRole) implementation.</summary>
    public partial class AspNetMembershipProvider : MembershipProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AspNetMembershipProvider));
        private static Random random = new Random();

        private readonly DataProvider dataProvider = DataProvider.Instance();
        private readonly IEnumerable<string> socialAuthProviders = new List<string> { "Facebook", "Google", "Twitter", "LiveID", };
        private readonly IHostSettings hostSettings;
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="AspNetMembershipProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public AspNetMembershipProvider()
            : this(null, null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering AspNetMembershipProvider()");
        }

		Logger.LogDebug("<<< Exiting AspNetMembershipProvider()");
        /// <summary>Initializes a new instance of the <see cref="AspNetMembershipProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public AspNetMembershipProvider(IHostSettings hostSettings)
            : this(hostSettings, null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering AspNetMembershipProvider(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("<<< Exiting AspNetMembershipProvider(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="AspNetMembershipProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        public AspNetMembershipProvider(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IEventLogger eventLogger)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
			Logger.LogDebug("AspNetMembershipProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): cryptographyProvider → {CryptographyProvider1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, eventLogger, cryptographyProvider);
			Logger.LogDebug("AspNetMembershipProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): appStatus → {AppStatus1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, eventLogger, appStatus);
			Logger.LogDebug("AspNetMembershipProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): portalGroupController → {PortalGroupController1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, eventLogger, portalGroupController);
			Logger.LogDebug("AspNetMembershipProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): eventLogger → {EventLogger1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, eventLogger, eventLogger);
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
			Logger.LogDebug("AspNetMembershipProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},eventLogger={EventLogger}): portalController → {PortalController1}", hostSettings, cryptographyProvider, portalController, appStatus, portalGroupController, eventLogger, portalController);
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <inheritdoc />
        public override bool CanEditProviderProperties => false;

        /// <inheritdoc />
        public override int MaxInvalidPasswordAttempts
        {
            get => System.Web.Security.Membership.MaxInvalidPasswordAttempts;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override int MinNonAlphanumericCharacters
        {
            get => System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override int MinPasswordLength
        {
            get => System.Web.Security.Membership.MinRequiredPasswordLength;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override int PasswordAttemptWindow
        {
            get => System.Web.Security.Membership.PasswordAttemptWindow;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override PasswordFormat PasswordFormat
        {
            get
            {
                switch (System.Web.Security.Membership.Provider.PasswordFormat)
                {
                    case MembershipPasswordFormat.Encrypted:
                        return PasswordFormat.Encrypted;
                    case MembershipPasswordFormat.Hashed:
                        return PasswordFormat.Hashed;
                    default:
                        return PasswordFormat.Clear;
                }
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc />
        public override bool PasswordResetEnabled
        {
            get => System.Web.Security.Membership.EnablePasswordReset;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override bool PasswordRetrievalEnabled
        {
            get => System.Web.Security.Membership.EnablePasswordRetrieval;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override string PasswordStrengthRegularExpression
        {
            get => System.Web.Security.Membership.PasswordStrengthRegularExpression;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override bool RequiresQuestionAndAnswer
        {
            get => System.Web.Security.Membership.RequiresQuestionAndAnswer;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        /// <inheritdoc />
        public override bool RequiresUniqueEmail
        {
            get => System.Web.Security.Membership.Provider.RequiresUniqueEmail;
            set => throw new NotSupportedException("Provider properties for AspNetMembershipProvider must be set in web.config");
        }

        public static ArrayList FillUserCollection(int portalId, IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillUserCollection(portalId={PortalId},dr={Dr})", portalId, dr);
            // Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
			Logger.LogDebug("FillUserCollection(portalId={PortalId},dr={Dr}): arrUsers → {ArrUsers}", portalId, dr, arrUsers);
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);
					Logger.LogDebug("FillUserCollection(portalId={PortalId},dr={Dr}): user → {User}", portalId, dr, user);

                    // add to collection
                    arrUsers.Add(user);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in FillUserCollection(portalId={PortalId},dr={Dr}): {Arg2}", portalId, dr, exc.getMessage());
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogDebug("<<< Exiting FillUserCollection(portalId={PortalId},dr={Dr})", portalId, dr);
			Logger.LogDebug("<<< Exiting FillUserCollection(portalId={PortalId},dr={Dr},totalRecords={TotalRecords})", portalId, dr, totalRecords);
            return arrUsers;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByAuthToken(int portalId, string userToken, string authType)
        {
			Logger.LogDebug(">>> Entering GetUserByAuthToken(portalId={PortalId},userToken=<redacted>,authType={AuthType})", portalId, authType);
            IDataReader dr = this.dataProvider.GetUserByAuthToken(portalId, userToken, authType);
			Logger.LogDebug("GetUserByAuthToken(portalId={PortalId},userToken=<redacted>,authType={AuthType}): dr → {Dr}", portalId, authType, dr);
			Logger.LogDebug("<<< Exiting GetUserByAuthToken(portalId={PortalId},userToken=<redacted>,authType={AuthType})", portalId, authType);
            return FillUserInfo(portalId, dr, true);
        }

        /// <inheritdoc />
        public override void AddUserPortal(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering AddUserPortal(portalId={PortalId},userId={UserId})", portalId, userId);
            Requires.NotNullOrEmpty("portalId", portalId.ToString(CultureInfo.InvariantCulture));
            Requires.NotNullOrEmpty("userId", userId.ToString(CultureInfo.InvariantCulture));
            this.dataProvider.AddUserPortal(portalId, userId);
			Logger.LogDebug("<<< Exiting AddUserPortal(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        /// <inheritdoc />
        public override void ChangeUsername(int userId, string newUsername)
        {
			Logger.LogDebug(">>> Entering ChangeUsername(userId={UserId},newUsername={NewUsername})", userId, newUsername);
            Requires.NotNull("userId", userId);
            Requires.NotNullOrEmpty("newUsername", newUsername);

            var userName = PortalSecurity.Instance.InputFilter(
			Logger.LogDebug("<<< Exiting ChangeUsername(userId={UserId},newUsername={NewUsername})", userId, newUsername);
                newUsername,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

            if (!userName.Equals(newUsername, StringComparison.Ordinal))
            {
				Logger.LogWarning("Throwing ArgumentException from ChangeUsername(userId={UserId},newUsername={NewUsername})", userId, newUsername);
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
            }

            var valid = UserController.Instance.IsValidUserName(userName);
			Logger.LogDebug("ChangeUsername(userId={UserId},newUsername={NewUsername}): valid → {Valid}", userId, newUsername, valid);

            if (!valid)
            {
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
            }

            // read all the user account settings
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("ChangeUsername(userId={UserId},newUsername={NewUsername}): portalSettings → {PortalSettings}", userId, newUsername, portalSettings);
            if (portalSettings != null)
            {
                var settings = UserController.GetUserSettings(portalSettings.PortalId);
				Logger.LogDebug("ChangeUsername(userId={UserId},newUsername={NewUsername}): settings → {Settings}", userId, newUsername, settings);

                // User Name Validation
                var userNameValidator = GetStringSetting(settings, "Security_UserNameValidation");
				Logger.LogDebug("ChangeUsername(userId={UserId},newUsername={NewUsername}): userNameValidator → {UserNameValidator}", userId, newUsername, userNameValidator);
                if (!string.IsNullOrEmpty(userNameValidator))
                {
                    var regExp = RegexUtils.GetCachedRegex(userNameValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
					Logger.LogDebug("ChangeUsername(userId={UserId},newUsername={NewUsername}): regExp → {RegExp}", userId, newUsername, regExp);
                    var matches = regExp.Matches(userName);
					Logger.LogDebug("ChangeUsername(userId={UserId},newUsername={NewUsername}): matches → {Matches}", userId, newUsername, matches);
                    if (matches.Count == 0)
                    {
                        throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
                    }
                }
            }

            this.dataProvider.ChangeUsername(userId, userName);

            this.eventLogger.AddLog(
                "userId",
                userId.ToString(CultureInfo.InvariantCulture),
                portalSettings,
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.USERNAME_UPDATED);

            DataCache.ClearCache();
        }

        /// <inheritdoc />
        public override bool ChangePassword(UserInfo user, string oldPassword, string newPassword)
        {
			Logger.LogDebug(">>> Entering ChangePassword(user={User},oldPassword=<redacted>,newPassword=<redacted>)", user);
            MembershipUser aspnetUser = this.GetMembershipUser(user);

			Logger.LogDebug("ChangePassword(user={User},oldPassword=<redacted>,newPassword=<redacted>): aspnetUser → {AspnetUser}", user, aspnetUser);
            var m = new MembershipPasswordController();
			Logger.LogDebug("ChangePassword(user={User},oldPassword=<redacted>,newPassword=<redacted>): m → {M}", user, m);
            if (m.IsPasswordInHistory(user.UserID, user.PortalID, newPassword))
            {
                return false;
            }

            if (string.IsNullOrEmpty(oldPassword))
            {
                aspnetUser.UnlockUser();
                oldPassword = aspnetUser.GetPassword();
				Logger.LogDebug("ChangePassword(user={User},oldPassword=<redacted>,newPassword=<redacted>): oldPassword → {OldPassword1}", user, oldPassword);
            }

            bool retValue = aspnetUser.ChangePassword(oldPassword, newPassword);
			Logger.LogDebug("ChangePassword(user={User},oldPassword=<redacted>,newPassword=<redacted>): retValue → {RetValue}", user, retValue);
            if (retValue && this.PasswordRetrievalEnabled && !this.RequiresQuestionAndAnswer)
            {
                string confirmPassword = aspnetUser.GetPassword();
                if (confirmPassword == newPassword)
                {
                    user.Membership.Password = confirmPassword;
                }
                else
                {
                    retValue = false;
                }
            }

			Logger.LogDebug("<<< Exiting ChangePassword(user={User},oldPassword=<redacted>,newPassword=<redacted>)", user);
            return retValue;
        }

        /// <inheritdoc />
        public override bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion, string passwordAnswer)
        {
			Logger.LogDebug(">>> Entering ChangePasswordQuestionAndAnswer(user={User},password=<redacted>,passwordQuestion=<redacted>,passwordAnswer=<redacted>)", user);
            MembershipUser aspnetUser = this.GetMembershipUser(user);
            if (password == Null.NullString)
			Logger.LogDebug("ChangePasswordQuestionAndAnswer(user={User},password=<redacted>,passwordQuestion=<redacted>,passwordAnswer=<redacted>): aspnetUser → {AspnetUser}", user, aspnetUser);
            {
                password = aspnetUser.GetPassword();
				Logger.LogWarning("ChangePasswordQuestionAndAnswer(user={User},password=<redacted>,passwordQuestion=<redacted>,passwordAnswer=<redacted>): password → {Password1}", user, password);
            }

			Logger.LogDebug("<<< Exiting ChangePasswordQuestionAndAnswer(user={User},password=<redacted>,passwordQuestion=<redacted>,passwordAnswer=<redacted>)", user);
            return aspnetUser.ChangePasswordQuestionAndAnswer(password, passwordQuestion, passwordAnswer);
        }

        /// <inheritdoc />
        public override UserCreateStatus CreateUser(ref UserInfo user)
        {
			Logger.LogDebug(">>> Entering CreateUser(user={User})", user);
            UserCreateStatus createStatus = ValidateForProfanity(user);
            string service = HttpContext.Current != null ? HttpContext.Current.Request.Params["state"] : string.Empty;
			Logger.LogDebug("CreateUser(user={User}): createStatus → {CreateStatus}", user, createStatus);

            if (createStatus == UserCreateStatus.AddUser)
            {
                this.ValidateForDuplicateDisplayName(user, ref createStatus);
            }

            if (createStatus == UserCreateStatus.AddUser)
            {
                try
                {
                    // check if username exists in database for any portal
                    UserInfo objVerifyUser = this.GetUserByUserName(Null.NullInteger, user.Username);
					Logger.LogDebug("CreateUser(user={User}): objVerifyUser → {ObjVerifyUser}", user, objVerifyUser);
                    if (objVerifyUser != null)
                    {
                        // DNN-4016
                        // the username exists so we should now verify the password, DNN-4016 or check for oauth user authentication.
                        if (ValidateUser(user.Username, user.Membership.Password))
                        {
                            // check if user exists for the portal specified
                            objVerifyUser = this.GetUserByUserName(user.PortalID, user.Username);
                            if (objVerifyUser != null)
                            {
                                if (objVerifyUser.PortalID == user.PortalID && (!user.IsSuperUser || user.PortalID == Null.NullInteger))
                                {
                                    createStatus = UserCreateStatus.UserAlreadyRegistered;
                                }
                                else
                                {
                                    // SuperUser who is not part of portal
                                    createStatus = UserCreateStatus.AddUserToPortal;
                                }
                            }
                            else
                            {
                                createStatus = UserCreateStatus.AddUserToPortal;
                            }
                        }
                        else
                        {
                            // not the same person - prevent registration
                            createStatus = UserCreateStatus.UsernameAlreadyExists;
                        }
                    }
                    else
                    {
                        // the user does not exist
                        createStatus = UserCreateStatus.AddUser;
                    }

                    // If new user - add to aspnet membership
                    if (createStatus == UserCreateStatus.AddUser)
                    {
                        createStatus = CreateMembershipUser(user);
                    }

                    // If asp user has been successfully created or we are adding a existing user
                    // to a new portal
                    if (createStatus == UserCreateStatus.Success || createStatus == UserCreateStatus.AddUserToPortal)
                    {
                        // Create the DNN User Record
                        createStatus = this.CreateDNNUser(ref user);
                        if (createStatus == UserCreateStatus.Success)
                        {
                            // Persist the Profile to the Data Store
                            ProfileController.UpdateUserProfile(user);
                        }
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in CreateUser(user={User}): {Arg1}", user, exc.getMessage());
                    // an unexpected error occurred
                    Exceptions.LogException(exc);
                    createStatus = UserCreateStatus.UnexpectedError;
                }
            }

			Logger.LogDebug("<<< Exiting CreateUser(user={User})", user);
            return createStatus;
        }

        /// <inheritdoc />
        public override bool DeleteUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering DeleteUser(user={User})", user);
            bool retValue = true;
            try
			Logger.LogDebug("DeleteUser(user={User}): retValue → {RetValue}", user, retValue);
            {
                this.dataProvider.DeleteUserFromPortal(user.UserID, user.PortalID);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in DeleteUser(user={User}): {Arg1}", user, ex.getMessage());
                Exceptions.LogException(ex);
                retValue = false;
            }

			Logger.LogDebug("<<< Exiting DeleteUser(user={User})", user);
            return retValue;
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial void DeleteUsersOnline(int timeWindow)
        {
			Logger.LogDebug(">>> Entering DeleteUsersOnline(timeWindow={TimeWindow})", timeWindow);
            this.dataProvider.DeleteUsersOnline(timeWindow);
			Logger.LogDebug("<<< Exiting DeleteUsersOnline(timeWindow={TimeWindow})", timeWindow);
        }

        /// <inheritdoc />
        public override string GeneratePassword()
        {
			Logger.LogDebug(">>> Entering GeneratePassword()");
			Logger.LogDebug("<<< Exiting GeneratePassword()");
			Logger.LogDebug("<<< Exiting GeneratePassword(length={Length})", length);
            return this.GeneratePassword(this.MinPasswordLength + 4);
        }

        /// <inheritdoc />
        public override string GeneratePassword(int length)
        {
            return System.Web.Security.Membership.GeneratePassword(length, this.MinNonAlphanumericCharacters);
        }

        /// <inheritdoc />
        public override ArrayList GetDeletedUsers(int portalId)
        {
			Logger.LogDebug(">>> Entering GetDeletedUsers(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetDeletedUsers(portalId={PortalId})", portalId);
            return FillUserCollection(portalId, this.dataProvider.GetDeletedUsers(portalId));
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial ArrayList GetOnlineUsers(int portalId)
        {
			Logger.LogDebug(">>> Entering GetOnlineUsers(portalId={PortalId})", portalId);
            int totalRecords = 0;
			Logger.LogDebug("<<< Exiting GetOnlineUsers(portalId={PortalId})", portalId);
            return FillUserCollection(portalId, this.dataProvider.GetOnlineUsers(portalId), ref totalRecords);
        }

        /// <inheritdoc />
        public override string GetPassword(UserInfo user, string passwordAnswer)
        {
			Logger.LogDebug(">>> Entering GetPassword(user={User},passwordAnswer=<redacted>)", user);
            MembershipUser aspnetUser = this.GetMembershipUser(user);
            if (aspnetUser.IsLockedOut)
			Logger.LogDebug("GetPassword(user={User},passwordAnswer=<redacted>): aspnetUser → {AspnetUser}", user, aspnetUser);
            {
                AutoUnlockUser(this.hostSettings, aspnetUser);
            }

			Logger.LogDebug("<<< Exiting GetPassword(user={User},passwordAnswer=<redacted>)", user);
            return this.RequiresQuestionAndAnswer ? aspnetUser.GetPassword(passwordAnswer) : aspnetUser.GetPassword();
        }

        /// <inheritdoc />
        public override ArrayList GetUnAuthorizedUsers(int portalId)
        {
			Logger.LogDebug(">>> Entering GetUnAuthorizedUsers(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetUnAuthorizedUsers(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetUnAuthorizedUsers(portalId={PortalId},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, includeDeleted, superUsersOnly);
            return this.GetUnAuthorizedUsers(portalId, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            return FillUserCollection(
                portalId,
                this.dataProvider.GetUnAuthorizedUsers(portalId, includeDeleted, superUsersOnly));
        }

        /// <inheritdoc />
        public override UserInfo GetUser(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering GetUser(portalId={PortalId},userId={UserId})", portalId, userId);
            IDataReader dr = this.dataProvider.GetUser(portalId, userId);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
			Logger.LogDebug("GetUser(portalId={PortalId},userId={UserId}): objUserInfo → {ObjUserInfo}", portalId, userId, objUserInfo);
			Logger.LogDebug("GetUser(portalId={PortalId},userId={UserId}): dr → {Dr}", portalId, userId, dr);
			Logger.LogDebug("<<< Exiting GetUser(portalId={PortalId},userId={UserId})", portalId, userId);
            return objUserInfo;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByDisplayName(int portalId, string displayName)
        {
			Logger.LogDebug(">>> Entering GetUserByDisplayName(portalId={PortalId},displayName={DisplayName})", portalId, displayName);
            IDataReader dr = this.dataProvider.GetUserByDisplayName(portalId, displayName);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
			Logger.LogDebug("GetUserByDisplayName(portalId={PortalId},displayName={DisplayName}): objUserInfo → {ObjUserInfo}", portalId, displayName, objUserInfo);
			Logger.LogDebug("GetUserByDisplayName(portalId={PortalId},displayName={DisplayName}): dr → {Dr}", portalId, displayName, dr);
			Logger.LogDebug("<<< Exiting GetUserByDisplayName(portalId={PortalId},displayName={DisplayName})", portalId, displayName);
            return objUserInfo;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByUserName(int portalId, string username)
        {
			Logger.LogDebug(">>> Entering GetUserByUserName(portalId={PortalId},username={Username})", portalId, username);
			Logger.LogDebug("<<< Exiting GetUserByUserName(portalId={PortalId},username={Username})", portalId, username);
            return CBO.GetCachedObject<UserInfo>(
                this.hostSettings,
                new CacheItemArgs(
                    string.Format(CultureInfo.InvariantCulture, DataCache.UserCacheKey, portalId, username),
                    DataCache.UserCacheTimeOut,
                    DataCache.UserCachePriority),
                _ => this.GetUserByUserNameFromDataStore(portalId, username));
        }

        /// <inheritdoc />
        public override UserInfo GetUserByVanityUrl(int portalId, string vanityUrl)
        {
			Logger.LogDebug(">>> Entering GetUserByVanityUrl(portalId={PortalId},vanityUrl={VanityUrl})", portalId, vanityUrl);
            UserInfo user = null;
            if (!string.IsNullOrEmpty(vanityUrl))
			Logger.LogDebug("GetUserByVanityUrl(portalId={PortalId},vanityUrl={VanityUrl}): user → {User}", portalId, vanityUrl, user);
            {
                IDataReader dr = this.dataProvider.GetUserByVanityUrl(portalId, vanityUrl);
				Logger.LogDebug("GetUserByVanityUrl(portalId={PortalId},vanityUrl={VanityUrl}): dr → {Dr}", portalId, vanityUrl, dr);
                user = FillUserInfo(portalId, dr, true);
            }

			Logger.LogDebug("<<< Exiting GetUserByVanityUrl(portalId={PortalId},vanityUrl={VanityUrl})", portalId, vanityUrl);
            return user;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByPasswordResetToken(int portalId, string resetToken)
        {
			Logger.LogDebug(">>> Entering GetUserByPasswordResetToken(portalId={PortalId},resetToken=<redacted>)", portalId);
            UserInfo user = null;
            if (!string.IsNullOrEmpty(resetToken))
			Logger.LogDebug("GetUserByPasswordResetToken(portalId={PortalId},resetToken=<redacted>): user → {User}", portalId, user);
            {
                IDataReader dr = this.dataProvider.GetUserByPasswordResetToken(portalId, resetToken);
				Logger.LogDebug("GetUserByPasswordResetToken(portalId={PortalId},resetToken=<redacted>): dr → {Dr}", portalId, dr);
                user = FillUserInfo(portalId, dr, true);
            }

			Logger.LogDebug("<<< Exiting GetUserByPasswordResetToken(portalId={PortalId},resetToken=<redacted>)", portalId);
            return user;
        }

        /// <inheritdoc />
        public override string GetProviderUserKey(UserInfo user)
        {
			Logger.LogDebug(">>> Entering GetProviderUserKey(user={User})", user);
			Logger.LogDebug("<<< Exiting GetProviderUserKey(user={User})", user);
            return this.GetMembershipUser(user).ProviderUserKey?.ToString().Replace("-", string.Empty) ?? string.Empty;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByProviderUserKey(int portalId, string providerUserKey)
        {
			Logger.LogDebug(">>> Entering GetUserByProviderUserKey(portalId={PortalId},providerUserKey={ProviderUserKey})", portalId, providerUserKey);
            var userName = this.GetMembershipUserByUserKey(providerUserKey)?.UserName ?? string.Empty;
            if (string.IsNullOrEmpty(userName))
			Logger.LogDebug("GetUserByProviderUserKey(portalId={PortalId},providerUserKey={ProviderUserKey}): userName → {UserName}", portalId, providerUserKey, userName);
            {
			Logger.LogDebug("<<< Exiting GetUserByProviderUserKey(portalId={PortalId},providerUserKey={ProviderUserKey})", portalId, providerUserKey);
                return null;
            }

            return this.GetUserByUserName(portalId, userName);
        }

        /// <inheritdoc />
        public override int GetUserCountByPortal(int portalId)
        {
			Logger.LogDebug(">>> Entering GetUserCountByPortal(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetUserCountByPortal(portalId={PortalId})", portalId);
            return this.dataProvider.GetUserCountByPortal(portalId);
        }

        /// <inheritdoc />
        public override void GetUserMembership(ref UserInfo user)
        {
			Logger.LogDebug(">>> Entering GetUserMembership(user={User})", user);
            // Get AspNet MembershipUser
            MembershipUser aspnetUser = this.GetMembershipUser(user);
			Logger.LogDebug("GetUserMembership(user={User}): aspnetUser → {AspnetUser}", user, aspnetUser);

            // Fill Membership Property
            FillUserMembership(aspnetUser, user);

			Logger.LogDebug("<<< Exiting GetUserMembership(user={User})", user);
            // Get Online Status
#pragma warning disable CS0618 // Type or member is obsolete
            user.Membership.IsOnLine = this.IsUserOnline(user);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        public override ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering GetUsers(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsers(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsers(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, pageIndex, pageSize, totalRecords, includeDeleted, superUsersOnly);
            return this.GetUsers(portalId, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetAllUsers(
                    portalId,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationshipTypeId, bool isAdmin, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyNames, string propertyValues)
        {
			Logger.LogDebug(">>> Entering GetUsersAdvancedSearch(portalId={PortalId},userId={UserId},filterUserId={FilterUserId},filterRoleId={FilterRoleId},relationshipTypeId={RelationshipTypeId},isAdmin={IsAdmin},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyNames={PropertyNames},propertyValues={PropertyValues})", portalId, userId, filterUserId, filterRoleId, relationshipTypeId, isAdmin, pageIndex, pageSize, sortColumn, sortAscending, propertyNames, propertyValues);
			Logger.LogDebug("<<< Exiting GetUsersAdvancedSearch(portalId={PortalId},userId={UserId},filterUserId={FilterUserId},filterRoleId={FilterRoleId},relationshipTypeId={RelationshipTypeId},isAdmin={IsAdmin},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyNames={PropertyNames},propertyValues={PropertyValues})", portalId, userId, filterUserId, filterRoleId, relationshipTypeId, isAdmin, pageIndex, pageSize, sortColumn, sortAscending, propertyNames, propertyValues);
            return FillUserList(
                portalId,
                this.dataProvider.GetUsersAdvancedSearch(
                    portalId,
                    userId,
                    filterUserId,
                    filterRoleId,
                    relationshipTypeId,
                    isAdmin,
                    pageIndex,
                    pageSize,
                    sortColumn,
                    sortAscending,
                    propertyNames,
                    propertyValues));
        }

        /// <inheritdoc />
        public override IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyName, string propertyValue)
        {
			Logger.LogDebug(">>> Entering GetUsersBasicSearch(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyName={PropertyName},propertyValue={PropertyValue})", portalId, pageIndex, pageSize, sortColumn, sortAscending, propertyName, propertyValue);
			Logger.LogDebug("<<< Exiting GetUsersBasicSearch(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},sortColumn={SortColumn},sortAscending={SortAscending},propertyName={PropertyName},propertyValue={PropertyValue})", portalId, pageIndex, pageSize, sortColumn, sortAscending, propertyName, propertyValue);
            return FillUserList(
                portalId,
                this.dataProvider.GetUsersBasicSearch(
                    portalId,
                    pageIndex,
                    pageSize,
                    sortColumn,
                    sortAscending,
                    propertyName,
                    propertyValue));
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering GetUsersByEmail(portalId={PortalId},emailToMatch={EmailToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, emailToMatch, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsersByEmail(portalId={PortalId},emailToMatch={EmailToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, emailToMatch, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsersByEmail(portalId={PortalId},emailToMatch={EmailToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, emailToMatch, pageIndex, pageSize, totalRecords, includeDeleted, superUsersOnly);
            return this.GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByEmail(
                    portalId,
                    emailToMatch,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering GetUsersByUserName(portalId={PortalId},userNameToMatch={UserNameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, userNameToMatch, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsersByUserName(portalId={PortalId},userNameToMatch={UserNameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, userNameToMatch, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsersByUserName(portalId={PortalId},userNameToMatch={UserNameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, userNameToMatch, pageIndex, pageSize, totalRecords, includeDeleted, superUsersOnly);
            return this.GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByUsername(
                    portalId,
                    userNameToMatch,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByDisplayName(int portalId, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
			Logger.LogDebug(">>> Entering GetUsersByDisplayName(portalId={PortalId},nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, nameToMatch, pageIndex, pageSize, totalRecords, includeDeleted, superUsersOnly);
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

			Logger.LogDebug("<<< Exiting GetUsersByDisplayName(portalId={PortalId},nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, nameToMatch, pageIndex, pageSize, totalRecords, includeDeleted, superUsersOnly);
            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByDisplayname(
                    portalId,
                    nameToMatch,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering GetUsersByProfileProperty(portalId={PortalId},propertyName={PropertyName},propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, propertyName, propertyValue, pageIndex, pageSize, totalRecords);
			Logger.LogDebug("<<< Exiting GetUsersByProfileProperty(portalId={PortalId},propertyName={PropertyName},propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", portalId, propertyName, propertyValue, pageIndex, pageSize, totalRecords);
            return this.GetUsersByProfileProperty(
                portalId,
                propertyName,
                propertyValue,
                pageIndex,
                pageSize,
                ref totalRecords,
                false,
			Logger.LogDebug("<<< Exiting GetUsersByProfileProperty(portalId={PortalId},propertyName={PropertyName},propertyValue={PropertyValue},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords},includeDeleted={IncludeDeleted},superUsersOnly={SuperUsersOnly})", portalId, propertyName, propertyValue, pageIndex, pageSize, totalRecords, includeDeleted, superUsersOnly);
                false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByProfileProperty(
                    portalId,
                    propertyName,
                    propertyValue,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial bool IsUserOnline(UserInfo user)
        {
			Logger.LogDebug(">>> Entering IsUserOnline(user={User})", user);
            bool isOnline = false;
            var objUsersOnline = new UserOnlineController();
			Logger.LogDebug("IsUserOnline(user={User}): objUsersOnline → {ObjUsersOnline}", user, objUsersOnline);
			Logger.LogDebug("IsUserOnline(user={User}): isOnline → {IsOnline}", user, isOnline);
            if (objUsersOnline.IsEnabled())
            {
                Hashtable userList = objUsersOnline.GetUserList();
				Logger.LogDebug("IsUserOnline(user={User}): userList → {UserList}", user, userList);
                var onlineUser = (OnlineUserInfo)userList[user.UserID.ToString(CultureInfo.InvariantCulture)];
				Logger.LogDebug("IsUserOnline(user={User}): onlineUser → {OnlineUser}", user, onlineUser);
                if (onlineUser != null)
                {
                    isOnline = true;
                }
                else
                {
                    // Next try the Database
                    onlineUser = CBO.FillObject<OnlineUserInfo>(this.dataProvider.GetOnlineUser(user.UserID));
                    if (onlineUser != null)
                    {
                        isOnline = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting IsUserOnline(user={User})", user);
            return isOnline;
        }

        /// <inheritdoc />
        public override bool RemoveUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering RemoveUser(user={User})", user);
            bool retValue = true;

			Logger.LogDebug("RemoveUser(user={User}): retValue → {RetValue}", user, retValue);
            try
            {
                foreach (var relationship in user.Social.UserRelationships)
                {
                    RelationshipController.Instance.DeleteUserRelationship(relationship);
                }

                this.dataProvider.RemoveUser(user.UserID, user.PortalID);

                // Prior to removing membership, ensure user is not present in any other portal
                UserInfo otherUser = this.GetUserByUserNameFromDataStore(Null.NullInteger, user.Username);
				Logger.LogDebug("RemoveUser(user={User}): otherUser → {OtherUser}", user, otherUser);
                if (otherUser == null)
                {
                    DeleteMembershipUser(user);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in RemoveUser(user={User}): {Arg1}", user, ex.getMessage());
                Exceptions.LogException(ex);
                retValue = false;
            }

			Logger.LogDebug("<<< Exiting RemoveUser(user={User})", user);
            return retValue;
        }

        /// <inheritdoc />
        public override string ResetPassword(UserInfo user, string passwordAnswer)
        {
			Logger.LogDebug(">>> Entering ResetPassword(user={User},passwordAnswer=<redacted>)", user);
            // Get AspNet MembershipUser
            MembershipUser aspnetUser = this.GetMembershipUser(user);
			Logger.LogDebug("ResetPassword(user={User},passwordAnswer=<redacted>): aspnetUser → {AspnetUser}", user, aspnetUser);

			Logger.LogDebug("<<< Exiting ResetPassword(user={User},passwordAnswer=<redacted>)", user);
            return this.RequiresQuestionAndAnswer ? aspnetUser.ResetPassword(passwordAnswer) : aspnetUser.ResetPassword();
        }

        /// <inheritdoc />
        public override bool ResetAndChangePassword(UserInfo user, string newPassword)
        {
			Logger.LogDebug(">>> Entering ResetAndChangePassword(user={User},newPassword=<redacted>)", user);
			Logger.LogDebug("<<< Exiting ResetAndChangePassword(user={User},newPassword=<redacted>)", user);
			Logger.LogDebug("<<< Exiting ResetAndChangePassword(user={User},newPassword=<redacted>,answer={Answer})", user, answer);
            return this.ResetAndChangePassword(user, newPassword, string.Empty);
        }

        /// <inheritdoc />
        public override bool ResetAndChangePassword(UserInfo user, string newPassword, string answer)
        {
            if (this.RequiresQuestionAndAnswer && string.IsNullOrEmpty(answer))
            {
                return false;
            }

            // Get AspNet MembershipUser
            MembershipUser aspnetUser = this.GetMembershipUser(user);
            if (aspnetUser.IsLockedOut)
            {
                aspnetUser.UnlockUser();
            }

            string resetPassword = this.ResetPassword(user, answer);
            return aspnetUser.ChangePassword(resetPassword, newPassword);
        }

        /// <inheritdoc />
        public override bool RestoreUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering RestoreUser(user={User})", user);
            bool retValue = true;

			Logger.LogDebug("RestoreUser(user={User}): retValue → {RetValue}", user, retValue);
            try
            {
                this.dataProvider.RestoreUser(user.UserID, user.PortalID);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in RestoreUser(user={User}): {Arg1}", user, ex.getMessage());
                Exceptions.LogException(ex);
                retValue = false;
            }

			Logger.LogDebug("<<< Exiting RestoreUser(user={User})", user);
            return retValue;
        }

        /// <inheritdoc />
        public override bool UnLockUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering UnLockUser(user={User})", user);
            MembershipUser membershipUser = System.Web.Security.Membership.GetUser(user.Username);
            bool retValue = false;
			Logger.LogDebug("UnLockUser(user={User}): retValue → {RetValue}", user, retValue);
			Logger.LogDebug("UnLockUser(user={User}): membershipUser → {MembershipUser}", user, membershipUser);
            if (membershipUser != null)
            {
                retValue = membershipUser.UnlockUser();
            }

            DataCache.RemoveCache(GetCacheKey(user.Username));
			Logger.LogDebug("<<< Exiting UnLockUser(user={User})", user);
            return retValue;
        }

        /// <inheritdoc />
        public override void UserAgreedToTerms(UserInfo user)
        {
			Logger.LogDebug(">>> Entering UserAgreedToTerms(user={User})", user);
            var effectivePortalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, user.PortalID);
            this.dataProvider.UserAgreedToTerms(effectivePortalId, user.UserID);
			Logger.LogDebug("UserAgreedToTerms(user={User}): effectivePortalId → {EffectivePortalId}", user, effectivePortalId);
			Logger.LogDebug("<<< Exiting UserAgreedToTerms(user={User})", user);
        }

        /// <inheritdoc />
        public override void ResetTermsAgreement(int portalId)
        {
			Logger.LogDebug(">>> Entering ResetTermsAgreement(portalId={PortalId})", portalId);
            this.dataProvider.ResetTermsAgreement(portalId);
			Logger.LogDebug("<<< Exiting ResetTermsAgreement(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public override void UserRequestsRemoval(UserInfo user, bool remove)
        {
			Logger.LogDebug(">>> Entering UserRequestsRemoval(user={User},remove={Remove})", user, remove);
            this.dataProvider.UserRequestsRemoval(user.PortalID, user.UserID, remove);
			Logger.LogDebug("<<< Exiting UserRequestsRemoval(user={User},remove={Remove})", user, remove);
        }

        /// <inheritdoc />
        public override void UpdateUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering UpdateUser(user={User})", user);
            var objSecurity = PortalSecurity.Instance;
            string firstName = objSecurity.InputFilter(
			Logger.LogDebug("<<< Exiting UpdateUser(user={User})", user);
                user.FirstName,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            string lastName = objSecurity.InputFilter(
                user.LastName,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            string email = objSecurity.InputFilter(
                user.Email,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            string displayName = objSecurity.InputFilter(
                user.DisplayName,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
				Logger.LogDebug("UpdateUser(user={User}): displayName → {DisplayName}", user, displayName);
#pragma warning restore CS0618 // Type or member is obsolete
            if (displayName.Contains("<"))
            {
                displayName = HttpUtility.HtmlEncode(displayName);
            }

            if (!firstName.Equals(user.FirstName, StringComparison.Ordinal))
            {
                user.FirstName = firstName;
				Logger.LogDebug("UpdateUser(user={User}): user.FirstName → {Arg1}", user, user.FirstName);
            }

            if (!lastName.Equals(user.LastName, StringComparison.Ordinal))
            {
                user.LastName = lastName;
				Logger.LogDebug("UpdateUser(user={User}): user.LastName → {Arg1}", user, user.LastName);
            }

            bool updatePassword = user.Membership.UpdatePassword;
            bool isApproved = user.Membership.Approved;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = firstName + " " + lastName;
            }

            // Persist the Membership to the Data Store
            UpdateUserMembership(user);

            // Persist the DNN User to the Database
            this.dataProvider.UpdateUser(
                user.UserID,
                user.PortalID,
                firstName,
                lastName,
                user.IsSuperUser,
                email,
                displayName,
                user.VanityUrl,
                updatePassword,
                isApproved,
                false,
                user.LastIPAddress,
                user.PasswordResetToken,
                user.PasswordResetExpiration,
                user.IsDeleted,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // Persist the Profile to the Data Store
            ProfileController.UpdateUserProfile(user);
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial void UpdateUsersOnline(Hashtable userList)
        {
			Logger.LogDebug(">>> Entering UpdateUsersOnline(userList={UserList})", userList);
            this.dataProvider.UpdateUsersOnline(userList);
			Logger.LogDebug("<<< Exiting UpdateUsersOnline(userList={UserList})", userList);
        }

        /// <inheritdoc />
        public override UserInfo UserLogin(int portalId, string username, string password, string verificationCode, ref UserLoginStatus loginStatus)
        {
			Logger.LogDebug(">>> Entering UserLogin(portalId={PortalId},username={Username},password=<redacted>,verificationCode={VerificationCode},loginStatus={LoginStatus})", portalId, username, verificationCode, loginStatus);
			Logger.LogDebug("<<< Exiting UserLogin(portalId={PortalId},username={Username},password=<redacted>,verificationCode={VerificationCode},loginStatus={LoginStatus})", portalId, username, verificationCode, loginStatus);
			Logger.LogDebug("<<< Exiting UserLogin(portalId={PortalId},username={Username},password=<redacted>,authType={AuthType},verificationCode={VerificationCode},loginStatus={LoginStatus})", portalId, username, authType, verificationCode, loginStatus);
            return this.UserLogin(portalId, username, password, "DNN", verificationCode, ref loginStatus);
        }

        /// <inheritdoc />
        public override UserInfo UserLogin(int portalId, string username, string password, string authType, string verificationCode, ref UserLoginStatus loginStatus)
        {
            // For now, we are going to ignore the possibility that the User may exist in the
            // Global Data Store but not in the Local DataStore ie. A shared Global Data Store

            // Initialize Login Status to Failure
            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            DataCache.ClearUserCache(portalId, username);
            DataCache.ClearCache(GetCacheKey(username));

            // Get a light-weight (unhydrated) DNN User from the Database, we will hydrate it later if necessary
            UserInfo user = (authType == "DNN")
                                ? this.GetUserByUserName(portalId, username)
                                : this.GetUserByAuthToken(portalId, verificationCode, authType);
            if (user is { IsDeleted: false, })
            {
                // Get AspNet MembershipUser
                MembershipUser aspnetUser = this.GetMembershipUser(user);

                // Fill Membership Property from AspNet MembershipUser
                FillUserMembership(aspnetUser, user);

                // Check if the User is Locked Out (and unlock if AutoUnlock has expired)
                if (aspnetUser.IsLockedOut)
                {
                    if (AutoUnlockUser(this.hostSettings, aspnetUser))
                    {
                        // Unlock User
                        user.Membership.LockedOut = false;
                    }
                    else
                    {
                        loginStatus = UserLoginStatus.LOGIN_USERLOCKEDOUT;
                    }
                }

                // Check in a verified situation whether the user is Approved
                if (!user.Membership.Approved && !user.IsSuperUser)
                {
                    // Check Verification code (skip for FB, Google, Twitter, LiveID as it has no verification code)
                    if (this.socialAuthProviders.Contains(authType) && string.IsNullOrEmpty(verificationCode))
                    {
                        if (PortalController.Instance.GetCurrentSettings().UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration)
                        {
                            user.Membership.Approved = true;
                            UserController.UpdateUser(portalId, user);
                            UserController.ApproveUser(user);
                        }
                        else
                        {
                            loginStatus = UserLoginStatus.LOGIN_USERNOTAPPROVED;
                        }
                    }
                    else
                    {
                        if (verificationCode == this.cryptographyProvider.EncryptParameter($"{portalId}-{user.UserID}", Config.GetDecryptionkey()).EncryptedMessage)
                        {
                            UserController.ApproveUser(user);
                        }
                        else
                        {
                            loginStatus = UserLoginStatus.LOGIN_USERNOTAPPROVED;
                        }
                    }
                }

                // Verify User Credentials
                bool bValid = false;
                loginStatus = ValidateLogin(username, authType, user, loginStatus, password, ref bValid, portalId);
                if (!bValid)
                {
                    // Clear the user object
                    user = null;

                    // Clear cache for user so that locked out & other status could be updated
                    DataCache.ClearUserCache(portalId, username);
                    DataCache.ClearCache(GetCacheKey(username));

                    aspnetUser = System.Web.Security.Membership.GetUser(username);

                    // If user has been locked out for current invalid attempt
                    // return locked out status
                    if (aspnetUser.IsLockedOut)
                    {
                        loginStatus = UserLoginStatus.LOGIN_USERLOCKEDOUT;
                    }
                }
            }
            else
            {
                // Clear the user object
                user = null;
            }

            return user;
        }

        private static bool AutoUnlockUser(IHostSettings hostSettings, MembershipUser aspNetUser)
        {
			Logger.LogDebug(">>> Entering AutoUnlockUser(hostSettings={HostSettings},aspNetUser={AspNetUser})", hostSettings, aspNetUser);
            if (hostSettings.AutoAccountUnlockDuration > TimeSpan.Zero)
            {
                if (aspNetUser.LastLockoutDate < DateTime.Now.Subtract(hostSettings.AutoAccountUnlockDuration))
                {
                    // Unlock user in Data Store
                    if (aspNetUser.UnlockUser())
                    {
                        return true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting AutoUnlockUser(hostSettings={HostSettings},aspNetUser={AspNetUser})", hostSettings, aspNetUser);
            return false;
        }

        private static UserCreateStatus CreateMembershipUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering CreateMembershipUser(user={User})", user);
            var portalSecurity = PortalSecurity.Instance;
            string userName = portalSecurity.InputFilter(
			Logger.LogDebug("<<< Exiting CreateMembershipUser(user={User})", user);
                user.Username,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            string email = portalSecurity.InputFilter(
                user.Email,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            MembershipCreateStatus status;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                System.Web.Security.Membership.CreateUser(
                    userName,
                    user.Membership.Password,
                    email,
                    user.Membership.PasswordQuestion,
                    user.Membership.PasswordAnswer,
                    true,
                    out status);
            }
            else
            {
                System.Web.Security.Membership.CreateUser(
                    userName,
                    user.Membership.Password,
                    email,
                    null,
                    null,
                    true,
                    out status);
            }

            var createStatus = UserCreateStatus.Success;
			Logger.LogDebug("CreateMembershipUser(user={User}): createStatus → {CreateStatus}", user, createStatus);
            switch (status)
            {
                case MembershipCreateStatus.DuplicateEmail:
                    createStatus = UserCreateStatus.DuplicateEmail;
                    break;
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    createStatus = UserCreateStatus.DuplicateProviderUserKey;
                    break;
                case MembershipCreateStatus.DuplicateUserName:
                    createStatus = UserCreateStatus.DuplicateUserName;
                    break;
                case MembershipCreateStatus.InvalidAnswer:
                    createStatus = UserCreateStatus.InvalidAnswer;
                    break;
                case MembershipCreateStatus.InvalidEmail:
                    createStatus = UserCreateStatus.InvalidEmail;
                    break;
                case MembershipCreateStatus.InvalidPassword:
                    createStatus = UserCreateStatus.InvalidPassword;
                    break;
                case MembershipCreateStatus.InvalidProviderUserKey:
                    createStatus = UserCreateStatus.InvalidProviderUserKey;
                    break;
                case MembershipCreateStatus.InvalidQuestion:
                    createStatus = UserCreateStatus.InvalidQuestion;
                    break;
                case MembershipCreateStatus.InvalidUserName:
                    createStatus = UserCreateStatus.InvalidUserName;
                    break;
                case MembershipCreateStatus.ProviderError:
                    createStatus = UserCreateStatus.ProviderError;
                    break;
                case MembershipCreateStatus.UserRejected:
                    createStatus = UserCreateStatus.UserRejected;
                    break;
            }

            return createStatus;
        }

        private static void DeleteMembershipUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering DeleteMembershipUser(user={User})", user);
            try
            {
                System.Web.Security.Membership.DeleteUser(user.Username, true);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
			Logger.LogDebug("<<< Exiting DeleteMembershipUser(user={User})", user);
        }

        private static ArrayList FillUserCollection(int portalId, IDataReader dr, ref int totalRecords)
        {
            // Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);

                    // add to collection
                    arrUsers.Add(user);
                }

                // Get the next result (containing the total)
                dr.NextResult();

                // Get the total no of records from the second result
                totalRecords = Globals.GetTotalRecords(ref dr);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return arrUsers;
        }

        private static List<UserInfo> FillUserList(int portalId, IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillUserList(portalId={PortalId},dr={Dr})", portalId, dr);
            var users = new List<UserInfo>();
            try
			Logger.LogDebug("FillUserList(portalId={PortalId},dr={Dr}): users → {Users}", portalId, dr, users);
            {
                while (dr.Read())
                {
                    // fill business object
                    UserInfo user = FillUserAndProfile(portalId, dr);
					Logger.LogDebug("FillUserList(portalId={PortalId},dr={Dr}): user → {User}", portalId, dr, user);

                    // add to collection
                    users.Add(user);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in FillUserList(portalId={PortalId},dr={Dr}): {Arg2}", portalId, dr, exc.getMessage());
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogDebug("<<< Exiting FillUserList(portalId={PortalId},dr={Dr})", portalId, dr);
            return users;
        }

        private static UserInfo FillUserAndProfile(int portalId, IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillUserAndProfile(portalId={PortalId},dr={Dr})", portalId, dr);
            UserInfo user = null;
            bool bContinue = string.Equals(dr.GetName(0), "UserID", StringComparison.OrdinalIgnoreCase);
			Logger.LogDebug("FillUserAndProfile(portalId={PortalId},dr={Dr}): bContinue → {BContinue}", portalId, dr, bContinue);

            // Ensure the data reader returned is valid
            if (bContinue)
            {
                user = new UserInfo
                {
                    PortalID = Null.SetNullInteger(dr["PortalID"]),
                    IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                    IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]),
                    UserID = Null.SetNullInteger(dr["UserID"]),
                    DisplayName = Null.SetNullString(dr["DisplayName"]),
                    Username = Null.SetNullString(dr["Username"]),
                    Email = Null.SetNullString(dr["Email"]),
                    AffiliateID = Null.SetNullInteger(dr["AffiliateID"]),
                };
                user.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], user.AffiliateID));

                UserController.GetUserMembership(user);
                user.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);
                if (!user.IsSuperUser)
                {
                    user.Membership.Approved = Null.SetNullBoolean(dr["Authorised"]);
                }

                if (user.PortalID == Null.NullInteger)
                {
                    user.PortalID = portalId;
					Logger.LogDebug("FillUserAndProfile(portalId={PortalId},dr={Dr}): user.PortalID → {Arg2}", portalId, dr, user.PortalID);
                }

                var userProfile = new UserProfile(user);
				Logger.LogDebug("FillUserAndProfile(portalId={PortalId},dr={Dr}): userProfile → {UserProfile}", portalId, dr, userProfile);
                userProfile.InitialiseProfile(portalId);

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    switch (dr.GetName(i))
                    {
                        case "PortalID":
                        case "IsSuperUser":
                        case "IsDeleted":
                        case "UserID":
                        case "DisplayName":
                        case "Username":
                        case "Email":
                        case "AffiliateID":
                        case "UpdatePassword":
                        case "Authorised":
                        case "CreateDate":
                        case "LastActivityDate":
                        case "LastLockoutDate":
                        case "LastLoginDate":
                        case "LastPasswordChangedDate":
                        case "IsLockedOut":
                        case "PasswordQuestion":
                        case "IsApproved":
                        case "PasswordResetToken":
                        case "PasswordResetExpiration":
                            break;
                        default:
                            // Probably a profile property
                            string name = dr.GetName(i);
                            userProfile.SetProfileProperty(name, Null.SetNullString(dr[name]));
                            break;
                    }
                }

                user.Profile = userProfile;
				Logger.LogDebug("FillUserAndProfile(portalId={PortalId},dr={Dr}): user.Profile → {Arg2}", portalId, dr, user.Profile);
            }

			Logger.LogDebug("<<< Exiting FillUserAndProfile(portalId={PortalId},dr={Dr})", portalId, dr);
            return user;
        }

        private static UserInfo FillUserInfo(int portalId, IDataReader dr, bool closeDataReader)
        {
			Logger.LogDebug(">>> Entering FillUserInfo(portalId={PortalId},dr={Dr},closeDataReader={CloseDataReader})", portalId, dr, closeDataReader);
            UserInfo user = null;
            try
            {
                // read datareader
                bool bContinue = true;
				Logger.LogDebug("FillUserInfo(portalId={PortalId},dr={Dr},closeDataReader={CloseDataReader}): bContinue → {BContinue}", portalId, dr, closeDataReader, bContinue);
                if (closeDataReader)
                {
                    bContinue = false;
                    if (dr.Read())
                    {
                        // Ensure the data reader returned is valid
                        if (string.Equals(dr.GetName(0), "UserID", StringComparison.OrdinalIgnoreCase))
                        {
                            bContinue = true;
                        }
                    }
                }

                if (bContinue)
                {
                    user = new UserInfo
                    {
                        PortalID = Null.SetNullInteger(dr["PortalID"]),
                        IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                        UserID = Null.SetNullInteger(dr["UserID"]),
                        DisplayName = Null.SetNullString(dr["DisplayName"]),
                        LastIPAddress = Null.SetNullString(dr["LastIPAddress"]),
                    };

                    var schema = dr.GetSchemaTable();
					Logger.LogDebug("FillUserInfo(portalId={PortalId},dr={Dr},closeDataReader={CloseDataReader}): schema → {Schema}", portalId, dr, closeDataReader, schema);
                    if (schema != null)
                    {
                        if (schema.Select("ColumnName = 'IsDeleted'").Length > 0)
                        {
                            user.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
                        }

                        if (schema.Select("ColumnName = 'VanityUrl'").Length > 0)
                        {
                            user.VanityUrl = Null.SetNullString(dr["VanityUrl"]);
                        }

                        if (schema.Select("ColumnName = 'HasAgreedToTerms'").Length > 0)
                        {
                            user.HasAgreedToTerms = Null.SetNullBoolean(dr["HasAgreedToTerms"]);
                        }

                        if (schema.Select("ColumnName = 'HasAgreedToTermsOn'").Length > 0)
                        {
                            user.HasAgreedToTermsOn = Null.SetNullDateTime(dr["HasAgreedToTermsOn"]);
                        }
                        else
                        {
                            user.HasAgreedToTermsOn = Null.NullDate;
							Logger.LogDebug("FillUserInfo(portalId={PortalId},dr={Dr},closeDataReader={CloseDataReader}): user.HasAgreedToTermsOn → {Arg3}", portalId, dr, closeDataReader, user.HasAgreedToTermsOn);
                        }

                        if (schema.Select("ColumnName = 'RequestsRemoval'").Length > 0)
                        {
                            user.RequestsRemoval = Null.SetNullBoolean(dr["RequestsRemoval"]);
                        }

                        if (schema.Select("ColumnName = 'PasswordResetExpiration'").Length > 0)
                        {
                            user.PasswordResetExpiration = Null.SetNullDateTime(dr["PasswordResetExpiration"]);
                        }

                        if (schema.Select("ColumnName = 'PasswordResetToken'").Length > 0)
                        {
                            user.PasswordResetToken = Null.SetNullGuid(dr["PasswordResetToken"]);
                        }
                    }

                    user.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], user.AffiliateID));
                    user.Username = Null.SetNullString(dr["Username"]);
                    UserController.GetUserMembership(user);
                    user.Email = Null.SetNullString(dr["Email"]);
                    user.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);

                    if (!user.IsSuperUser)
                    {
                        user.Membership.Approved = Null.SetNullBoolean(dr["Authorised"]);
                    }

                    if (user.PortalID == Null.NullInteger)
                    {
                        user.PortalID = portalId;
						Logger.LogDebug("FillUserInfo(portalId={PortalId},dr={Dr},closeDataReader={CloseDataReader}): user.PortalID → {Arg3}", portalId, dr, closeDataReader, user.PortalID);
                    }

                    user.FillBaseProperties(dr);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, closeDataReader);
            }

			Logger.LogDebug("<<< Exiting FillUserInfo(portalId={PortalId},dr={Dr},closeDataReader={CloseDataReader})", portalId, dr, closeDataReader);
            return user;
        }

        private static void FillUserMembership(MembershipUser aspNetUser, UserInfo user)
        {
			Logger.LogDebug(">>> Entering FillUserMembership(aspNetUser={AspNetUser},user={User})", aspNetUser, user);
            // Fill Membership Property
            if (aspNetUser != null)
            {
                if (user.Membership == null)
                {
                    user.Membership = new UserMembership(user);
                }

                user.Membership.CreatedDate = aspNetUser.CreationDate;
                user.Membership.LastActivityDate = aspNetUser.LastActivityDate;
                user.Membership.LastLockoutDate = aspNetUser.LastLockoutDate;
                user.Membership.LastLoginDate = aspNetUser.LastLoginDate;
                user.Membership.LastPasswordChangeDate = aspNetUser.LastPasswordChangedDate;
                user.Membership.LockedOut = aspNetUser.IsLockedOut;
                user.Membership.PasswordQuestion = aspNetUser.PasswordQuestion;
                user.Membership.IsDeleted = user.IsDeleted;

                if (user.IsSuperUser)
                {
                    // For superusers the Approved info is stored in aspnet membership
                    user.Membership.Approved = aspNetUser.IsApproved;
                }
            }
			Logger.LogDebug("<<< Exiting FillUserMembership(aspNetUser={AspNetUser},user={User})", aspNetUser, user);
        }

        private static string GetCacheKey(string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetCacheKey(cacheKey={CacheKey})", cacheKey);
			Logger.LogDebug("<<< Exiting GetCacheKey(cacheKey={CacheKey})", cacheKey);
            return $"MembershipUser_{cacheKey}";
        }

        private static object GetMembershipUserCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetMembershipUserCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            string userName = cacheItemArgs.ParamList[0].ToString();

			Logger.LogDebug("GetMembershipUserCallBack(cacheItemArgs={CacheItemArgs}): userName → {UserName}", cacheItemArgs, userName);
			Logger.LogDebug("<<< Exiting GetMembershipUserCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return System.Web.Security.Membership.GetUser(userName);
        }

        private static object GetMembershipUserByUserKeyCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetMembershipUserByUserKeyCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            string userKey = cacheItemArgs.ParamList[0].ToString();

			Logger.LogDebug("<<< Exiting GetMembershipUserByUserKeyCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return System.Web.Security.Membership.GetUser(new Guid(userKey));
        }

        private static void UpdateUserMembership(UserInfo user)
        {
			Logger.LogDebug(">>> Entering UpdateUserMembership(user={User})", user);
            var portalSecurity = PortalSecurity.Instance;
            string email = portalSecurity.InputFilter(
			Logger.LogDebug("<<< Exiting UpdateUserMembership(user={User})", user);
                user.Email,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

            // Persist the Membership Properties to the AspNet Data Store
            MembershipUser membershipUser = System.Web.Security.Membership.GetUser(user.Username);
			Logger.LogDebug("UpdateUserMembership(user={User}): membershipUser → {MembershipUser}", user, membershipUser);
			Logger.LogDebug("UpdateUserMembership(user={User}): membershipUser.Email → {Arg1}", user, membershipUser.Email);
			Logger.LogDebug("UpdateUserMembership(user={User}): membershipUser.LastActivityDate → {Arg1}", user, membershipUser.LastActivityDate);
            membershipUser.Email = email;
            membershipUser.LastActivityDate = DateTime.Now;
            if (user.IsSuperUser)
            {
                membershipUser.IsApproved = user.Membership.Approved;
				Logger.LogDebug("UpdateUserMembership(user={User}): membershipUser.IsApproved → {Arg1}", user, membershipUser.IsApproved);
            }

            try
            {
                System.Web.Security.Membership.UpdateUser(membershipUser);
            }
            catch (ProviderException ex)
            {
				Logger.LogError("Exception in UpdateUserMembership(user={User}): {Arg1}", user, ex.getMessage());
                throw new UpdateUserException(Localization.GetExceptionMessage("UpdateUserMembershipFailed", "Asp.net membership update user failed."), ex);
            }

            DataCache.RemoveCache(GetCacheKey(user.Username));
        }

        private static UserLoginStatus ValidateLogin(string username, string authType, UserInfo user, UserLoginStatus loginStatus, string password, ref bool bValid, int portalId)
        {
			Logger.LogDebug(">>> Entering ValidateLogin(username={Username},authType={AuthType},user={User},loginStatus={LoginStatus},password=<redacted>,bValid={BValid},portalId={PortalId})", username, authType, user, loginStatus, bValid, portalId);
            if (loginStatus != UserLoginStatus.LOGIN_USERLOCKEDOUT &&
                (loginStatus != UserLoginStatus.LOGIN_USERNOTAPPROVED || user.IsInRole("Unverified Users")))
            {
                if (authType == "DNN")
                {
                    if (user.IsSuperUser)
                    {
                        if (ValidateUser(username, password))
                        {
                            loginStatus = UserLoginStatus.LOGIN_SUPERUSER;
							Logger.LogDebug("ValidateLogin(username={Username},authType={AuthType},user={User},loginStatus={LoginStatus},password=<redacted>,bValid={BValid},portalId={PortalId}): loginStatus → {LoginStatus1}", username, authType, user, loginStatus, bValid, portalId, loginStatus);
                            bValid = true;
							Logger.LogDebug("ValidateLogin(username={Username},authType={AuthType},user={User},loginStatus={LoginStatus},password=<redacted>,bValid={BValid},portalId={PortalId}): bValid → {BValid1}", username, authType, user, loginStatus, bValid, portalId, bValid);
                        }
                    }
                    else
                    {
                        if (ValidateUser(username, password))
                        {
                            loginStatus = UserLoginStatus.LOGIN_SUCCESS;
                            bValid = true;
                        }
                    }
                }
                else
                {
                    if (user.IsSuperUser)
                    {
                        loginStatus = UserLoginStatus.LOGIN_SUPERUSER;
                        bValid = true;
                    }
                    else
                    {
                        loginStatus = UserLoginStatus.LOGIN_SUCCESS;
                        bValid = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting ValidateLogin(username={Username},authType={AuthType},user={User},loginStatus={LoginStatus},password=<redacted>,bValid={BValid},portalId={PortalId})", username, authType, user, loginStatus, bValid, portalId);
            return loginStatus;
        }

        private static bool ValidateUser(string username, string password)
        {
			Logger.LogDebug(">>> Entering ValidateUser(username={Username},password=<redacted>)", username);
			Logger.LogDebug("<<< Exiting ValidateUser(username={Username},password=<redacted>)", username);
            return System.Web.Security.Membership.ValidateUser(username, password);
        }

        private static string GetStringSetting(Hashtable settings, string settingKey)
        {
			Logger.LogDebug(">>> Entering GetStringSetting(settings={Settings},settingKey={SettingKey})", settings, settingKey);
			Logger.LogDebug("<<< Exiting GetStringSetting(settings={Settings},settingKey={SettingKey})", settings, settingKey);
            return settings[settingKey] == null ? string.Empty : settings[settingKey].ToString();
        }

        private static UserCreateStatus ValidateForProfanity(UserInfo user)
        {
			Logger.LogDebug(">>> Entering ValidateForProfanity(user={User})", user);
            var portalSecurity = PortalSecurity.Instance;
            var createStatus = UserCreateStatus.AddUser;
			Logger.LogDebug("ValidateForProfanity(user={User}): createStatus → {CreateStatus}", user, createStatus);

            Hashtable settings = UserController.GetUserSettings(user.PortalID);
			Logger.LogDebug("ValidateForProfanity(user={User}): settings → {Settings}", user, settings);
            bool useProfanityFilter = Convert.ToBoolean(settings["Registration_UseProfanityFilter"], CultureInfo.InvariantCulture);

            // Validate Profanity
            if (useProfanityFilter)
            {
                if (!portalSecurity.ValidateInput(user.Username, PortalSecurity.FilterFlag.NoProfanity))
                {
                    createStatus = UserCreateStatus.InvalidUserName;
                }

                if (!string.IsNullOrEmpty(user.DisplayName))
                {
                    if (!portalSecurity.ValidateInput(user.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        createStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting ValidateForProfanity(user={User})", user);
            return createStatus;
        }

        private static string RandomString(int length)
        {
			Logger.LogDebug(">>> Entering RandomString(length={Length})", length);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			Logger.LogDebug("<<< Exiting RandomString(length={Length})", length);
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private UserCreateStatus CreateDNNUser(ref UserInfo user)
        {
			Logger.LogDebug(">>> Entering CreateDNNUser(user={User})", user);
			Logger.LogDebug("<<< Exiting CreateDNNUser(user={User})", user);
            var objSecurity = PortalSecurity.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
            const PortalSecurity.FilterFlag filterFlags = PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup;
#pragma warning restore CS0618 // Type or member is obsolete
            user.Username = objSecurity.InputFilter(user.Username, filterFlags);
            user.Email = objSecurity.InputFilter(user.Email, filterFlags);
            user.LastName = objSecurity.InputFilter(user.LastName, filterFlags);
            user.FirstName = objSecurity.InputFilter(user.FirstName, filterFlags);
            user.DisplayName = objSecurity.InputFilter(user.DisplayName, filterFlags);
            if (user.DisplayName.Contains("<") || user.DisplayName.Contains(">"))
            {
                user.DisplayName = HttpUtility.HtmlEncode(user.DisplayName);
            }

            var updatePassword = user.Membership.UpdatePassword;
            var isApproved = user.Membership.Approved;
            var createStatus = UserCreateStatus.Success;
			Logger.LogDebug("CreateDNNUser(user={User}): createStatus → {CreateStatus}", user, createStatus);
            try
            {
                user.UserID =
                    Convert.ToInt32(this.dataProvider.AddUser(
                        user.PortalID,
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        user.AffiliateID,
                        user.IsSuperUser,
                        user.Email,
                        user.DisplayName,
                        updatePassword,
                        isApproved,
                        UserController.Instance.GetCurrentUserInfo().UserID));

                // Save the user password history
                new MembershipPasswordController().IsPasswordInHistory(user.UserID, user.PortalID, user.Membership.Password);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in CreateDNNUser(user={User}): {Arg1}", user, ex.getMessage());
                // Clear User (duplicate User information)
                Exceptions.LogException(ex);
                user = null;
                createStatus = UserCreateStatus.ProviderError;
            }

            return createStatus;
        }

        private MembershipUser GetMembershipUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering GetMembershipUser(user={User})", user);
			Logger.LogDebug("<<< Exiting GetMembershipUser(user={User})", user);
			Logger.LogDebug("<<< Exiting GetMembershipUser(userName={UserName})", userName);
            return this.GetMembershipUser(user.Username);
        }

        private MembershipUser GetMembershipUser(string userName)
        {
            return
                CBO.GetCachedObject<MembershipUser>(
                    this.hostSettings,
                    new CacheItemArgs(
                        GetCacheKey(userName),
                        DataCache.UserCacheTimeOut,
                        DataCache.UserCachePriority,
                        userName),
                    GetMembershipUserCallBack);
        }

        private MembershipUser GetMembershipUserByUserKey(string userKey)
        {
			Logger.LogDebug(">>> Entering GetMembershipUserByUserKey(userKey={UserKey})", userKey);
				Logger.LogDebug("<<< Exiting GetMembershipUserByUserKey(userKey={UserKey})", userKey);
            return
                CBO.GetCachedObject<MembershipUser>(
                    this.hostSettings,
                    new CacheItemArgs(
                        GetCacheKey(userKey),
                        DataCache.UserCacheTimeOut,
                        DataCache.UserCachePriority,
                        userKey),
                    GetMembershipUserByUserKeyCallBack);
        }

        private void ValidateForDuplicateDisplayName(UserInfo user, ref UserCreateStatus createStatus)
        {
			Logger.LogDebug(">>> Entering ValidateForDuplicateDisplayName(user={User},createStatus={CreateStatus})", user, createStatus);
            Hashtable settings = UserController.GetUserSettings(user.PortalID);
            bool requireUniqueDisplayName = Convert.ToBoolean(settings["Registration_RequireUniqueDisplayName"], CultureInfo.InvariantCulture);
			Logger.LogDebug("ValidateForDuplicateDisplayName(user={User},createStatus={CreateStatus}): settings → {Settings}", user, createStatus, settings);

            if (requireUniqueDisplayName)
            {
                UserInfo duplicateUser = this.GetUserByDisplayName(user.PortalID, user.DisplayName);
				Logger.LogDebug("ValidateForDuplicateDisplayName(user={User},createStatus={CreateStatus}): duplicateUser → {DuplicateUser}", user, createStatus, duplicateUser);
                if (duplicateUser != null)
                {
                    createStatus = UserCreateStatus.DuplicateDisplayName;
                }
            }
			Logger.LogDebug("<<< Exiting ValidateForDuplicateDisplayName(user={User},createStatus={CreateStatus})", user, createStatus);
        }

        /// <summary>GetUserByUserNameFromDataStore retrieves a User from the DataStore.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="username">The username of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object.</returns>
        private UserInfo GetUserByUserNameFromDataStore(int portalId, string username)
        {
			Logger.LogDebug(">>> Entering GetUserByUserNameFromDataStore(portalId={PortalId},username={Username})", portalId, username);
            using var dr = this.dataProvider.GetUserByUsername(portalId, username);
			Logger.LogDebug("GetUserByUserNameFromDataStore(portalId={PortalId},username={Username}): dr → {Dr}", portalId, username, dr);
			Logger.LogDebug("<<< Exiting GetUserByUserNameFromDataStore(portalId={PortalId},username={Username})", portalId, username);
            return FillUserInfo(portalId, dr, true);
        }
    }
}
