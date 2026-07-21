// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Mail;

    using static DotNetNuke.Common.Globals;

    public class UserRegistrationEmailNotifier
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="UserRegistrationEmailNotifier"/> class.</summary>
        public UserRegistrationEmailNotifier()
        {
		Logger.LogDebug(">>> Entering UserRegistrationEmailNotifier()");
		Logger.LogDebug("<<< Exiting UserRegistrationEmailNotifier()");
        }

        private static UserInfo CurrentUser => UserController.Instance.GetCurrentUserInfo();

        public static void NotifyAdministrator(UserInfo user)
        {
			Logger.LogDebug(">>> Entering NotifyAdministrator(user={User})", user);
            // avoid self-notification (i.e. on site installation/super user creation)
            if (CurrentUser != null &&
                (CurrentUser.UserID == Null.NullInteger || CurrentUser.UserID == user.UserID))
            {
                return;
            }

            // send notification to portal administrator of new user registration
            // check the receive notification setting first, but if register type is Private, we will always send the notification email.
            // because the user need administrators to do the approve action so that he can continue use the website.
            if (PortalSettings.Current.EnableRegisterNotification || PortalSettings.Current.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
            {
                NotifyUser(user, MessageType.UserRegistrationAdmin);
            }
			Logger.LogDebug("<<< Exiting NotifyAdministrator(user={User})", user);
        }

        public static void NotifyUser(UserInfo user)
        {
			Logger.LogDebug(">>> Entering NotifyUser(user={User})", user);
            switch (PortalSettings.Current.UserRegistration)
            {
                case (int)PortalRegistrationType.PrivateRegistration:
                    var messageType =
                        CurrentUser != null && CurrentUser.UserID != Null.NullInteger
                            ? MessageType.UserRegistrationPrivateNoApprovalRequired
                            : MessageType.UserRegistrationPrivate;
                    NotifyUser(user, messageType);
                    break;
                case (int)PortalRegistrationType.PublicRegistration:
                    NotifyUser(user, MessageType.UserRegistrationPublic);
                    break;
                case (int)PortalRegistrationType.VerifiedRegistration:
                    NotifyUser(user, MessageType.UserRegistrationVerified);
                    break;
                case (int)PortalRegistrationType.NoRegistration:
                    NotifyUser(user, MessageType.UserRegistrationPublic);
                    break;
            }
			Logger.LogDebug("<<< Exiting NotifyUser(user={User})", user);
        }

        public static void NotifyUser(UserInfo user, MessageType type)
        {
			Logger.LogDebug(">>> Entering NotifyUser(user={User},type={Type})", user, type);
            Mail.SendMail(user, type, PortalSettings.Current);
			Logger.LogDebug("<<< Exiting NotifyUser(user={User},type={Type})", user, type);
        }
    }
}
