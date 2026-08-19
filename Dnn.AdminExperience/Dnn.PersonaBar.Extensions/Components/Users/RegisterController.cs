// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Common;
    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Social.Notifications;

    internal class RegisterController : ServiceLocator<IRegisterController, RegisterController>, IRegisterController
    {
        public static void SendNewUserNotifications(UserInfo newUser, PortalSettings portalSettings, List<RoleInfo> roles)
        {
			eventLogger.LogDebug(">>> Entering SendNewUserNotifications(newUser={NewUser},portalSettings={PortalSettings},roles={Roles})", newUser, portalSettings, roles);
            var notificationType = newUser.Membership.Approved ? "NewUserRegistration" : "NewUnauthorizedUserRegistration";
            var locale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId).Code;
			eventLogger.LogDebug("SendNewUserNotifications(newUser={NewUser},portalSettings={PortalSettings},roles={Roles}): locale → {Locale}", newUser, portalSettings, roles, locale);
			eventLogger.LogDebug("SendNewUserNotifications(newUser={NewUser},portalSettings={PortalSettings},roles={Roles}): notificationType → {NotificationType}", newUser, portalSettings, roles, notificationType);
            var notification = new Notification
            {
                NotificationTypeID = NotificationsController.Instance.GetNotificationType(notificationType).NotificationTypeId,
                IncludeDismissAction = newUser.Membership.Approved,
                SenderUserID = portalSettings.AdministratorId,
                Subject = GetNotificationSubject(locale, newUser, portalSettings),
                Body = GetNotificationBody(locale, newUser, portalSettings),
                Context = newUser.UserID.ToString(CultureInfo.InvariantCulture),
            };

            notification.Body = Utilities.FixDoublEntityEncoding(notification.Body);
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, roles, new List<UserInfo>());
			eventLogger.LogDebug("<<< Exiting SendNewUserNotifications(newUser={NewUser},portalSettings={PortalSettings},roles={Roles})", newUser, portalSettings, roles);
        }

        // NOTE - While making modifications in this method, developer must refer to call tree in Register.ascx.cs.
        // Especially Validate and CreateUser methods. Register class inherits from UserModuleBase, which also contains a bunch of logic.
        // This method can easily be modified to pass password, display name, etc.
        // It is recommended to write unit tests.

        /// <inheritdoc />
        public UserBasicDto Register(RegisterationDetails registerationDetails)
        {
			eventLogger.LogDebug(">>> Entering Register(registerationDetails={RegisterationDetails})", registerationDetails);
            var portalSettings = registerationDetails.PortalSettings;
            var username = registerationDetails.UserName;
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): username → {Username}", registerationDetails, username);
            var email = registerationDetails.Email;
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): email → {Email}", registerationDetails, email);

            Requires.NotNullOrEmpty("email", email);

            var disallowRegistration = !registerationDetails.IgnoreRegistrationMode &&
                                   ((portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration) ||
                                   (portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration));

            if (disallowRegistration)
            {
				eventLogger.LogWarning("Throwing RegistrationException from Register(registerationDetails={RegisterationDetails})", registerationDetails);
                throw new RegistrationException(Localization.GetString("RegistrationNotAllowed", Library.Constants.SharedResources));
            }

            // initial creation of the new User object
            var newUser = new UserInfo
            {
                PortalID = portalSettings.PortalId,
                Email = email,
            };

            var cleanUsername = PortalSecurity.Instance.InputFilter(
			eventLogger.LogDebug("<<< Exiting Register(registerationDetails={RegisterationDetails})", registerationDetails);
                username,
#pragma warning disable CS0618 // Type or member is obsolete
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

            if (!cleanUsername.Equals(username, StringComparison.Ordinal))
            {
				eventLogger.LogWarning("Throwing ArgumentException from Register(registerationDetails={RegisterationDetails})", registerationDetails);
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
            }

            var valid = UserController.Instance.IsValidUserName(username);
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): valid → {Valid}", registerationDetails, valid);

            if (!valid)
            {
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
            }

            // ensure this user doesn't exist
            if (!string.IsNullOrEmpty(username) && UserController.GetUserByName(portalSettings.PortalId, username) != null)
            {
                throw new RegistrationException(Localization.GetString(
                    "RegistrationUsernameAlreadyPresent",
                    Library.Constants.SharedResources));
            }

            // set username as email if not specified
            newUser.Username = string.IsNullOrEmpty(username) ? email : username;

            if (!string.IsNullOrEmpty(registerationDetails.Password) && !registerationDetails.RandomPassword)
            {
                newUser.Membership.Password = registerationDetails.Password;
            }
            else
            {
                // Generate a random password for the user
                newUser.Membership.Password = UserController.GeneratePassword();
            }

            newUser.Membership.PasswordConfirm = newUser.Membership.Password;

            // set other profile properties
            newUser.Profile.InitialiseProfile(portalSettings.PortalId);
            newUser.Profile.PreferredLocale = new Localization().CurrentUICulture;
            newUser.Profile.PreferredTimeZone = portalSettings.TimeZone;

            // derive display name from supplied firstname, lastname or from email
            if (!string.IsNullOrEmpty(registerationDetails.FirstName) &&
                !string.IsNullOrEmpty(registerationDetails.LastName))
            {
                newUser.DisplayName = registerationDetails.FirstName + " " + registerationDetails.LastName;
				eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): newUser.DisplayName → {Arg1}", registerationDetails, newUser.DisplayName);
				eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): newUser.FirstName → {Arg1}", registerationDetails, newUser.FirstName);
				eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): newUser.LastName → {Arg1}", registerationDetails, newUser.LastName);
                newUser.FirstName = registerationDetails.FirstName;
                newUser.LastName = registerationDetails.LastName;
            }
            else
            {
                newUser.DisplayName = newUser.Email.Substring(0, newUser.Email.IndexOf("@", StringComparison.Ordinal));
            }

            // read all the user account settings
            var settings = UserController.GetUserSettings(portalSettings.PortalId);
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): settings → {Settings}", registerationDetails, settings);

            // Verify Profanity filter
            if (GetBoolSetting(settings, "Registration_UseProfanityFilter"))
            {
                var portalSecurity = PortalSecurity.Instance;
                if (!portalSecurity.ValidateInput(newUser.Username, PortalSecurity.FilterFlag.NoProfanity) || !portalSecurity.ValidateInput(newUser.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                {
                    throw new RegistrationException(Localization.GetString(
                        "RegistrationProfanityNotAllowed",
                        Library.Constants.SharedResources));
                }
            }

            // Email Address Validation
            var emailValidator = GetStringSetting(settings, "Security_EmailValidation");
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): emailValidator → {EmailValidator}", registerationDetails, emailValidator);
            if (!string.IsNullOrEmpty(emailValidator))
            {
                var regExp = RegexUtils.GetCachedRegex(emailValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
				eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): regExp → {RegExp}", registerationDetails, regExp);
                var matches = regExp.Matches(newUser.Email);
				eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): matches → {Matches}", registerationDetails, matches);
                if (matches.Count == 0)
                {
                    throw new RegistrationException(Localization.GetString(
                        "RegistrationInvalidEmailUsed",
                        Library.Constants.SharedResources));
                }
            }

            // Excluded Terms Verification
            var excludeRegex = GetExcludeTermsRegex(settings);
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): excludeRegex → {ExcludeRegex}", registerationDetails, excludeRegex);
            if (!string.IsNullOrEmpty(excludeRegex))
            {
                var regExp = RegexUtils.GetCachedRegex(excludeRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matches = regExp.Matches(newUser.Username);
                if (matches.Count > 0)
                {
                    throw new RegistrationException(Localization.GetString(
                        "RegistrationExcludedTermsUsed",
                        Library.Constants.SharedResources));
                }
            }

            // User Name Validation
            var userNameValidator = GetStringSetting(settings, "Security_UserNameValidation");
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): userNameValidator → {UserNameValidator}", registerationDetails, userNameValidator);
            if (!string.IsNullOrEmpty(userNameValidator))
            {
                var regExp = RegexUtils.GetCachedRegex(userNameValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matches = regExp.Matches(newUser.Username);
                if (matches.Count == 0)
                {
                    throw new RegistrationException(Localization.GetString(
                        "RegistrationInvalidUserNameUsed",
                        Library.Constants.SharedResources));
                }
            }

            // ensure unique username
            var user = UserController.GetUserByName(portalSettings.PortalId, newUser.Username);
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): user → {User}", registerationDetails, user);
            if (user != null)
            {
                if (GetBoolSetting(settings, "Registration_UseEmailAsUserName"))
                {
                    throw new RegistrationException(UserController.GetUserCreateStatus(UserCreateStatus.DuplicateEmail));
                }

                var i = 1;
                string userName = null;
                while (user != null)
                {
                    userName = newUser.Username + "0" + i.ToString(CultureInfo.InvariantCulture);
                    user = UserController.GetUserByName(portalSettings.PortalId, userName);
                    i++;
                }

                newUser.Username = userName;
				eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): newUser.Username → {Arg1}", registerationDetails, newUser.Username);
            }

            // ensure unique display name
            if (GetBoolSetting(settings, "Registration_RequireUniqueDisplayName"))
            {
                user = UserController.Instance.GetUserByDisplayname(portalSettings.PortalId, newUser.DisplayName);
                if (user != null)
                {
                    var i = 1;
                    string displayName = null;
					eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): displayName → {DisplayName}", registerationDetails, displayName);
                    while (user != null)
                    {
                        displayName = newUser.DisplayName + " 0" + i.ToString(CultureInfo.InvariantCulture);
                        user = UserController.Instance.GetUserByDisplayname(portalSettings.PortalId, displayName);
                        i++;
                    }

                    newUser.DisplayName = displayName;
                }
            }

            // Update display name format
            var displaynameFormat = GetStringSetting(settings, "Security_DisplayNameFormat");
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): displaynameFormat → {DisplaynameFormat}", registerationDetails, displaynameFormat);
            if (!string.IsNullOrEmpty(displaynameFormat))
            {
                newUser.UpdateDisplayName(displaynameFormat);
            }

            // membership is approved only for public registration
            newUser.Membership.Approved =
                (registerationDetails.IgnoreRegistrationMode ||
                portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration) && registerationDetails.Authorize;
            newUser.Membership.PasswordQuestion = registerationDetails.Question;
            newUser.Membership.PasswordAnswer = registerationDetails.Answer;

            // final creation of user
            var createStatus = UserController.CreateUser(ref newUser, registerationDetails.Notify);
			eventLogger.LogDebug("Register(registerationDetails={RegisterationDetails}): createStatus → {CreateStatus}", registerationDetails, createStatus);

            // clear cache
            if (createStatus == UserCreateStatus.Success)
            {
                CachingProvider.Instance().Remove(string.Format(CultureInfo.InvariantCulture, DataCache.PortalUserCountCacheKey, portalSettings.PortalId));
            }

            if (createStatus != UserCreateStatus.Success)
            {
				eventLogger.LogWarning("Throwing InvalidUserRegisterException from Register(registerationDetails={RegisterationDetails})", registerationDetails);
                throw new InvalidUserRegisterException(UserController.GetUserCreateStatus(createStatus));
            }

            ////if (registerationDetails.IgnoreRegistrationMode)
            ////{
            ////    Mail.SendMail(newUser, MessageType.UserRegistrationPublic, portalSettings);
            ////    return UserBasicDto.FromUserInfo(newUser);
            ////}

            // send notification to portal administrator of new user registration
            // check the receive notification setting first, but if register type is Private, we will always send the notification email.
            // because the user need administrators to do the approve action so that he can continue use the website.
            if (!registerationDetails.IgnoreRegistrationMode &&
                    (portalSettings.EnableRegisterNotification || portalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration))
            {
                Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, portalSettings);
                SendAdminNotification(newUser, portalSettings);
            }

            return UserBasicDto.FromUserInfo(newUser);
        }

        /// <inheritdoc />
        protected override Func<IRegisterController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new RegisterController();
        }

        private static void SendAdminNotification(UserInfo newUser, PortalSettings portalSettings)
        {
			eventLogger.LogDebug(">>> Entering SendAdminNotification(newUser={NewUser},portalSettings={PortalSettings})", newUser, portalSettings);
            var roleController = new RoleController();
            var adminrole = roleController.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId);
			eventLogger.LogDebug("SendAdminNotification(newUser={NewUser},portalSettings={PortalSettings}): adminrole → {Adminrole}", newUser, portalSettings, adminrole);
			eventLogger.LogDebug("SendAdminNotification(newUser={NewUser},portalSettings={PortalSettings}): roleController → {RoleController}", newUser, portalSettings, roleController);
            var roles = new List<RoleInfo> { adminrole };
            SendNewUserNotifications(newUser, portalSettings, roles);
			eventLogger.LogDebug("<<< Exiting SendAdminNotification(newUser={NewUser},portalSettings={PortalSettings})", newUser, portalSettings);
        }

        private static string GetNotificationBody(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
			eventLogger.LogDebug(">>> Entering GetNotificationBody(locale={Locale},newUser={NewUser},portalSettings={PortalSettings})", locale, newUser, portalSettings);
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
			eventLogger.LogDebug("<<< Exiting GetNotificationBody(locale={Locale},newUser={NewUser},portalSettings={PortalSettings})", locale, newUser, portalSettings);
            return LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        private static string LocalizeNotificationText(string text, string locale, UserInfo user, PortalSettings portalSettings)
        {
			eventLogger.LogDebug(">>> Entering LocalizeNotificationText(text={Text},locale={Locale},user={User},portalSettings={PortalSettings})", text, locale, user, portalSettings);
            // This method could need a custom ArrayList in future notification types. Currently, it is null
			eventLogger.LogDebug("<<< Exiting LocalizeNotificationText(text={Text},locale={Locale},user={User},portalSettings={PortalSettings})", text, locale, user, portalSettings);
            return Localization.GetSystemMessage(locale, portalSettings, text, user, Localization.GlobalResourceFile, null, string.Empty, portalSettings.AdministratorId);
        }

        private static string GetNotificationSubject(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
			eventLogger.LogDebug(">>> Entering GetNotificationSubject(locale={Locale},newUser={NewUser},portalSettings={PortalSettings})", locale, newUser, portalSettings);
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
			eventLogger.LogDebug("<<< Exiting GetNotificationSubject(locale={Locale},newUser={NewUser},portalSettings={PortalSettings})", locale, newUser, portalSettings);
            return LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        private static bool GetBoolSetting(Hashtable settings, string settingKey)
        {
			eventLogger.LogDebug(">>> Entering GetBoolSetting(settings={Settings},settingKey={SettingKey})", settings, settingKey);
			eventLogger.LogDebug("<<< Exiting GetBoolSetting(settings={Settings},settingKey={SettingKey})", settings, settingKey);
            return settings[settingKey] != null && Convert.ToBoolean(settings[settingKey], CultureInfo.InvariantCulture);
        }

        private static string GetStringSetting(Hashtable settings, string settingKey)
        {
			eventLogger.LogDebug(">>> Entering GetStringSetting(settings={Settings},settingKey={SettingKey})", settings, settingKey);
			eventLogger.LogDebug("<<< Exiting GetStringSetting(settings={Settings},settingKey={SettingKey})", settings, settingKey);
            return settings[settingKey] == null ? string.Empty : settings[settingKey].ToString();
        }

        private static string GetExcludeTermsRegex(Hashtable settings)
        {
			eventLogger.LogDebug(">>> Entering GetExcludeTermsRegex(settings={Settings})", settings);
            var excludeTerms = GetStringSetting(settings, "Registration_ExcludeTerms");
            var regex = string.Empty;
			eventLogger.LogDebug("GetExcludeTermsRegex(settings={Settings}): regex → {Regex}", settings, regex);
			eventLogger.LogDebug("GetExcludeTermsRegex(settings={Settings}): excludeTerms → {ExcludeTerms}", settings, excludeTerms);
            if (!string.IsNullOrEmpty(excludeTerms))
            {
                regex = excludeTerms.Replace(" ", string.Empty).Replace(",", "|");
            }

			eventLogger.LogDebug("<<< Exiting GetExcludeTermsRegex(settings={Settings})", settings);
            return regex;
        }
    }
}
