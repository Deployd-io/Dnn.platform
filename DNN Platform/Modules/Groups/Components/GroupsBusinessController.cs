// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Social.Notifications;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The business controller class for the Groups module.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class GroupsBusinessController(IHostSettings hostSettings) : IUpgradeable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering GroupsBusinessController(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("GroupsBusinessController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="GroupsBusinessController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public GroupsBusinessController()
            : this(null)
        {
        }

        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			Logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            switch (version)
            {
                case "06.02.00":
                    this.AddNotificationTypes();
                    break;
                case "06.02.04":
                    this.RemoveRejectActionForCreatedNotification();
                    break;
                case "07.00.00":
                    ConvertNotificationTypeActionsFor700();
                    break;
            }

			Logger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
            return string.Empty;
        }

        private static void ConvertNotificationTypeActionsFor700()
        {
			Logger.LogDebug(">>> Entering ConvertNotificationTypeActionsFor700()");
            var notificationTypeNames = new[] { "GroupPendingNotification", "GroupApprovedNotification", "GroupCreatedNotification", "GroupRejectedNotification", "GroupMemberPendingNotification", "GroupMemberApprovedNotification", "GroupMemberRejectedNotification" };

			Logger.LogDebug("ConvertNotificationTypeActionsFor700(): notificationTypeNames → {NotificationTypeNames}", notificationTypeNames);
            foreach (var name in notificationTypeNames)
            {
                var nt = NotificationsController.Instance.GetNotificationType(name);

				Logger.LogDebug("ConvertNotificationTypeActionsFor700(): nt → {Nt}", nt);
                var actions = NotificationsController.Instance.GetNotificationTypeActions(nt.NotificationTypeId).ToList();

				Logger.LogDebug("ConvertNotificationTypeActionsFor700(): actions → {Actions}", actions);
                if (actions.Any())
                {
                    foreach (var action in actions)
                    {
                        action.APICall = action.APICall.Replace(".ashx", string.Empty);
                        NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
                    }

                    NotificationsController.Instance.SetNotificationTypeActions(actions, nt.NotificationTypeId);
                }
            }
			Logger.LogDebug("<<< Exiting ConvertNotificationTypeActionsFor700()");
        }

        private void RemoveRejectActionForCreatedNotification()
        {
			Logger.LogDebug(">>> Entering RemoveRejectActionForCreatedNotification()");
            var notificationType = NotificationsController.Instance.GetNotificationType(Constants.GroupCreatedNotification);
            if (notificationType == null)
			Logger.LogDebug("RemoveRejectActionForCreatedNotification(): notificationType → {NotificationType}", notificationType);
            {
                return;
            }

            var action = NotificationsController.Instance.GetNotificationTypeAction(notificationType.NotificationTypeId, "RejectGroup");
            if (action == null)
			Logger.LogDebug("RemoveRejectActionForCreatedNotification(): action → {Action}", action);
            {
                return;
            }

            NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
			Logger.LogDebug("<<< Exiting RemoveRejectActionForCreatedNotification()");
        }

        private void AddNotificationTypes()
        {
			Logger.LogDebug(">>> Entering AddNotificationTypes()");
            var actions = new List<NotificationTypeAction>();

			Logger.LogDebug("AddNotificationTypes(): actions → {Actions}", actions);
            // DesktopModule should not be null
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(this.hostSettings, "Social Groups").DesktopModuleID;

			Logger.LogDebug("AddNotificationTypes(): deskModuleId → {DeskModuleId}", deskModuleId);
            // GroupPendingNotification
            var type = new NotificationType { Name = "GroupPendingNotification", Description = "Group Pending Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
			Logger.LogDebug("AddNotificationTypes(): type → {Type}", type);
            {
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "Approve",
                    DescriptionResourceKey = "ApproveGroup",
                    APICall = "API/SocialGroups/ModerationService/ApproveGroup",
                });
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "RejectGroup",
                    DescriptionResourceKey = "RejectGroup",
                    APICall = "API/SocialGroups/ModerationService/RejectGroup",
                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            // GroupApprovedNotification
            type = new NotificationType { Name = "GroupApprovedNotification", Description = "Group Approved Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            // GroupCreatedNotification
            type = new NotificationType { Name = "GroupCreatedNotification", Description = "Group Created Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Clear();
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "RejectGroup",
                    DescriptionResourceKey = "RejectGroup",
                    ConfirmResourceKey = "DeleteItem",
                    APICall = "API/SocialGroups/ModerationService/RejectGroup",
                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            // GroupRejectedNotification
            type = new NotificationType { Name = "GroupRejectedNotification", Description = "Group Rejected Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            // GroupMemberPendingNotification
            type = new NotificationType { Name = "GroupMemberPendingNotification", Description = "Group Member Pending Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Clear();
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "Approve",
                    DescriptionResourceKey = "ApproveGroupMember",
                    ConfirmResourceKey = string.Empty,
                    APICall = "API/SocialGroups/ModerationService/ApproveMember",
                });
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "RejectMember",
                    DescriptionResourceKey = "RejectGroupMember",
                    APICall = "API/SocialGroups/ModerationService/RejectMember",
                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            // GroupMemberApprovedNotification
            type = new NotificationType { Name = "GroupMemberApprovedNotification", Description = "Group Member Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            // GroupMemberRejectedNotification
            type = new NotificationType { Name = "GroupMemberRejectedNotification", Description = "Group Rejected Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
			Logger.LogDebug("<<< Exiting AddNotificationTypes()");
        }
		Logger.LogDebug("<<< Exiting GroupsBusinessController(hostSettings={HostSettings})", hostSettings);
    }
}
