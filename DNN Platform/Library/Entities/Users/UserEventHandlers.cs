// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System.ComponentModel.Composition;
    using System.Globalization;

    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Social.Notifications;

    [Export(typeof(IUserEventHandlers))]
    public class UserEventHandlers : IUserEventHandlers
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserEventHandlers));
        /// <inheritdoc />
        public void UserAuthenticated(object sender, UserEventArgs args)
        {
		Logger.LogError(">>> Entering UserAuthenticated(sender={Sender},args={Args})", sender, args);
        Logger.LogWarning("<<< Exiting UserAuthenticated(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void UserCreated(object sender, UserEventArgs args)
        {
			Logger.LogError(">>> Entering UserCreated(sender={Sender},args={Args})", sender, args);
            UserRegistrationEmailNotifier.NotifyAdministrator(args.User);

            if (args.SendNotification)
            {
                UserRegistrationEmailNotifier.NotifyUser(args.User);
            }
			Logger.LogWarning("<<< Exiting UserCreated(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void UserRemoved(object sender, UserEventArgs args)
        {
			Logger.LogError(">>> Entering UserRemoved(sender={Sender},args={Args})", sender, args);
            DeleteAllNewUnauthorizedUserRegistrationNotifications(args.User.UserID);
			Logger.LogWarning("<<< Exiting UserRemoved(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void UserDeleted(object sender, UserEventArgs args)
        {
		Logger.LogError(">>> Entering UserDeleted(sender={Sender},args={Args})", sender, args);
        Logger.LogWarning("<<< Exiting UserDeleted(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void UserApproved(object sender, UserEventArgs args)
        {
			Logger.LogError(">>> Entering UserApproved(sender={Sender},args={Args})", sender, args);
            if (args.SendNotification)
            {
                UserRegistrationEmailNotifier.NotifyUser(args.User, MessageType.UserRegistrationPublic);
            }

            DeleteAllNewUnauthorizedUserRegistrationNotifications(args.User.UserID);
			Logger.LogWarning("<<< Exiting UserApproved(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void UserUpdated(object sender, UpdateUserEventArgs args)
        {
		Logger.LogError(">>> Entering UserUpdated(sender={Sender},args={Args})", sender, args);
        Logger.LogWarning("<<< Exiting UserUpdated(sender={Sender},args={Args})", sender, args);
        }

        private static void DeleteAllNewUnauthorizedUserRegistrationNotifications(int userId)
        {
			Logger.LogError(">>> Entering DeleteAllNewUnauthorizedUserRegistrationNotifications(userId={UserId})", userId);
            var nt = NotificationsController.Instance.GetNotificationType("NewUnauthorizedUserRegistration");
            var notifications = NotificationsController.Instance.GetNotificationByContext(nt.NotificationTypeId, userId.ToString(CultureInfo.InvariantCulture));
			Logger.LogError("DeleteAllNewUnauthorizedUserRegistrationNotifications(userId={UserId}): notifications → {Notifications}", userId, notifications);
			Logger.LogError("DeleteAllNewUnauthorizedUserRegistrationNotifications(userId={UserId}): nt → {Nt}", userId, nt);

            foreach (var notification in notifications)
            {
                NotificationsController.Instance.DeleteNotification(notification.NotificationID);
            }
			Logger.LogWarning("<<< Exiting DeleteAllNewUnauthorizedUserRegistrationNotifications(userId={UserId})", userId);
        }
    }
}
