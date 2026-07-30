// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Manages groups notifications.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class Notifications(IHostSettings hostSettings)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering Notifications(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("Notifications(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="Notifications"/> class.</summary>
		Logger.LogDebug("<<< Exiting Notifications(hostSettings={HostSettings})", hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Notifications()
            : this(null)
        {
        }

        internal virtual Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators)
        {
			Logger.LogDebug(">>> Entering AddGroupNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser},moderators={Moderators})", notificationTypeName, tabId, moduleId, group, initiatingUser, moderators);
			Logger.LogDebug("<<< Exiting AddGroupNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser},moderators={Moderators})", notificationTypeName, tabId, moduleId, group, initiatingUser, moderators);
            return this.AddGroupNotification(notificationTypeName, tabId, moduleId, group, initiatingUser, moderators, null as UserInfo);
        }

        internal virtual Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators, UserInfo recipient)
        {
			Logger.LogDebug("<<< Exiting AddGroupNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser},moderators={Moderators},recipient={Recipient})", notificationTypeName, tabId, moduleId, group, initiatingUser, moderators, recipient);
            return this.AddGroupNotification(notificationTypeName, tabId, moduleId, group, initiatingUser, moderators, recipient == null ? null : new List<UserInfo> { recipient });
        }

        internal virtual Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators, IList<UserInfo> recipients)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);
            var tokenReplace = new GroupItemTokenReplace(group);

            var subject = Localization.GetString(notificationTypeName + ".Subject", Constants.SharedResourcesPath);
            subject = tokenReplace.ReplaceGroupItemTokens(subject);

            var body = Localization.GetString(notificationTypeName + ".Body", Constants.SharedResourcesPath);

            body = tokenReplace.ReplaceGroupItemTokens(body);
            body = body.Replace("Public.Text", Localization.GetString("Public.Text", Constants.SharedResourcesPath));
            body = body.Replace("Private.Text", Localization.GetString("Private.Text", Constants.SharedResourcesPath));

            bool dismiss = notificationTypeName != Constants.GroupPendingNotification;
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = dismiss,
                SenderUserID = initiatingUser.UserID,
                Context = string.Format("{0}:{1}:{2}", tabId, moduleId, group.RoleID),
            };
            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, moderators, recipients);

			Logger.LogDebug("<<< Exiting AddGroupNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser},moderators={Moderators},recipients={Recipients})", notificationTypeName, tabId, moduleId, group, initiatingUser, moderators, recipients);
            return notification;
        }

        internal virtual Notification AddGroupOwnerNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser)
        {
			Logger.LogDebug(">>> Entering AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser})", notificationTypeName, tabId, moduleId, group, initiatingUser);
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);

			Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): notificationType → {NotificationType}", notificationTypeName, tabId, moduleId, group, initiatingUser, notificationType);
            var tokenReplace = new GroupItemTokenReplace(group);

			Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): tokenReplace → {TokenReplace}", notificationTypeName, tabId, moduleId, group, initiatingUser, tokenReplace);
            var subject = Localization.GetString(notificationTypeName + ".Subject", Constants.SharedResourcesPath);
            var body = Localization.GetString(notificationTypeName + ".Body", Constants.SharedResourcesPath);
			Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): subject → {Subject}", notificationTypeName, tabId, moduleId, group, initiatingUser, subject);
			Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): body → {Body}", notificationTypeName, tabId, moduleId, group, initiatingUser, body);
            subject = subject.Replace("[DisplayName]", initiatingUser.DisplayName);
            subject = subject.Replace("[ProfileUrl]", Globals.UserProfileURL(initiatingUser.UserID));
            subject = tokenReplace.ReplaceGroupItemTokens(subject);
            body = body.Replace("[DisplayName]", initiatingUser.DisplayName);
            body = body.Replace("[ProfileUrl]", Globals.UserProfileURL(initiatingUser.UserID));
            body = tokenReplace.ReplaceGroupItemTokens(body);
            var roleCreator = UserController.GetUserById(this.hostSettings, group.PortalID, group.CreatedByUserID);

			Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): roleCreator → {RoleCreator}", notificationTypeName, tabId, moduleId, group, initiatingUser, roleCreator);
            var roleOwners = new List<UserInfo>();

			Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): roleOwners → {RoleOwners}", notificationTypeName, tabId, moduleId, group, initiatingUser, roleOwners);
            foreach (UserInfo userInfo in RoleController.Instance.GetUsersByRole(group.PortalID, group.RoleName))
            {
                var userRoleInfo = RoleController.Instance.GetUserRole(group.PortalID, userInfo.UserID, group.RoleID);
                if (userRoleInfo.IsOwner && userRoleInfo.UserID != group.CreatedByUserID)
				Logger.LogDebug("AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser}): userRoleInfo → {UserRoleInfo}", notificationTypeName, tabId, moduleId, group, initiatingUser, userRoleInfo);
                {
                    roleOwners.Add(UserController.GetUserById(this.hostSettings, group.PortalID, userRoleInfo.UserID));
                }
            }

            roleOwners.Add(roleCreator);

            // Need to add from sender details
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                SenderUserID = initiatingUser.UserID,
                Context = string.Format("{0}:{1}:{2}:{3}", tabId, moduleId, group.RoleID, initiatingUser.UserID),
            };
            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, null, roleOwners);

			Logger.LogDebug("<<< Exiting AddGroupOwnerNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},initiatingUser={InitiatingUser})", notificationTypeName, tabId, moduleId, group, initiatingUser);
            return notification;
        }

        internal virtual Notification AddMemberNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo sender, UserInfo recipient)
        {
			Logger.LogDebug(">>> Entering AddMemberNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},sender={Sender},recipient={Recipient})", notificationTypeName, tabId, moduleId, group, sender, recipient);
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);

			Logger.LogDebug("AddMemberNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},sender={Sender},recipient={Recipient}): notificationType → {NotificationType}", notificationTypeName, tabId, moduleId, group, sender, recipient, notificationType);
            var subject = Localization.GetString(notificationTypeName + ".Subject", Constants.SharedResourcesPath);

			Logger.LogDebug("AddMemberNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},sender={Sender},recipient={Recipient}): subject → {Subject}", notificationTypeName, tabId, moduleId, group, sender, recipient, subject);
            var body = Localization.GetString(notificationTypeName + ".Body", Constants.SharedResourcesPath);
            var tokenReplace = new GroupItemTokenReplace(group);
			Logger.LogDebug("AddMemberNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},sender={Sender},recipient={Recipient}): body → {Body}", notificationTypeName, tabId, moduleId, group, sender, recipient, body);
			Logger.LogDebug("AddMemberNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},sender={Sender},recipient={Recipient}): tokenReplace → {TokenReplace}", notificationTypeName, tabId, moduleId, group, sender, recipient, tokenReplace);
            subject = subject.Replace("[DisplayName]", recipient.DisplayName);
            subject = subject.Replace("[ProfileUrl]", Globals.UserProfileURL(recipient.UserID));
            subject = tokenReplace.ReplaceGroupItemTokens(subject);
            body = body.Replace("[DisplayName]", recipient.DisplayName);
            body = body.Replace("[ProfileUrl]", Globals.UserProfileURL(recipient.UserID));
            body = tokenReplace.ReplaceGroupItemTokens(body);

            // Need to add from sender details
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                SenderUserID = sender.UserID,
                Context = string.Format("{0}:{1}:{2}", tabId, moduleId, group.RoleID),
            };
            NotificationsController.Instance.SendNotification(notification, recipient.PortalID, null, new List<UserInfo> { recipient });

			Logger.LogDebug("<<< Exiting AddMemberNotification(notificationTypeName={NotificationTypeName},tabId={TabId},moduleId={ModuleId},group={Group},sender={Sender},recipient={Recipient})", notificationTypeName, tabId, moduleId, group, sender, recipient);
            return notification;
        }
    }
}
