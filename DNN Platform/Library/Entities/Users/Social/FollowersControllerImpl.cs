// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Social.Internal
{
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;

    internal class FollowersControllerImpl : IFollowersController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FollowersControllerImpl));
        internal const string FollowerRequest = "FollowerRequest";
        internal const string FollowBackRequest = "FollowBackRequest";

        /// <summary>FollowUser - Current User initiates a Follow Request to the Target User.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <remarks>If the Follow Relationship is setup for auto-acceptance (default) at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        public void FollowUser(UserInfo targetUser)
        {
			Logger.LogError(">>> Entering FollowUser(targetUser={TargetUser})", targetUser);
            this.FollowUser(UserController.Instance.GetCurrentUserInfo(), targetUser);
			Logger.LogWarning("<<< Exiting FollowUser(targetUser={TargetUser})", targetUser);
        }

        /// <summary>FollowUser - Initiating User initiates a Follow Request to the Target User.</summary>
        /// <param name="initiatingUser">UserInfo for Initiating User.</param>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <remarks>If the Follow Relationship is setup for auto-acceptance (default) at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        public void FollowUser(UserInfo initiatingUser, UserInfo targetUser)
        {
			Logger.LogError(">>> Entering FollowUser(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            Requires.NotNull("user1", initiatingUser);

            var relationship = RelationshipController.Instance.InitiateUserRelationship(
                initiatingUser,
                targetUser,
                RelationshipController.Instance.GetFollowersRelationshipByPortal(initiatingUser.PortalID));

            AddFollowerRequestNotification(initiatingUser, targetUser);

            EventManager.Instance.OnFollowRequested(new RelationshipEventArgs(relationship, initiatingUser.PortalID));
			Logger.LogWarning("<<< Exiting FollowUser(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
        }

        /// <summary>UnFollowUser - Current User initiates an UnFollow Request to the Target User.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        public void UnFollowUser(UserInfo targetUser)
        {
			Logger.LogError(">>> Entering UnFollowUser(targetUser={TargetUser})", targetUser);
            var initiatingUser = UserController.Instance.GetCurrentUserInfo();
            var followRelationship = RelationshipController.Instance.GetFollowerRelationship(initiatingUser, targetUser);
			Logger.LogError("UnFollowUser(targetUser={TargetUser}): followRelationship → {FollowRelationship}", targetUser, followRelationship);
			Logger.LogError("UnFollowUser(targetUser={TargetUser}): initiatingUser → {InitiatingUser}", targetUser, initiatingUser);

            RelationshipController.Instance.DeleteUserRelationship(followRelationship);

            EventManager.Instance.OnUnfollowRequested(new RelationshipEventArgs(followRelationship, initiatingUser.PortalID));
			Logger.LogWarning("<<< Exiting UnFollowUser(targetUser={TargetUser})", targetUser);
        }

        private static void AddFollowerRequestNotification(UserInfo initiatingUser, UserInfo targetUser)
        {
			Logger.LogError(">>> Entering AddFollowerRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            var notificationType = NotificationsController.Instance.GetNotificationType(IsFollowing(targetUser, initiatingUser) ? FollowerRequest : FollowBackRequest);
            var subject = string.Format(
                CultureInfo.CurrentCulture,
                Localization.GetString("AddFollowerRequestSubject", Localization.GlobalResourceFile),
                initiatingUser.DisplayName);
				Logger.LogError("AddFollowerRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser}): notificationType → {NotificationType}", initiatingUser, targetUser, notificationType);

            var body = string.Format(
                CultureInfo.CurrentCulture,
                Localization.GetString("AddFollowerRequestBody", Localization.GlobalResourceFile),
                initiatingUser.DisplayName);

            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                Context = initiatingUser.UserID.ToString(CultureInfo.InvariantCulture),
                SenderUserID = initiatingUser.UserID,
            };

            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, null, [targetUser,]);
			Logger.LogWarning("<<< Exiting AddFollowerRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
        }

        private static bool IsFollowing(UserInfo user1, UserInfo user2)
        {
			Logger.LogError(">>> Entering IsFollowing(user1={User1},user2={User2})", user1, user2);
            var userRelationship = RelationshipController.Instance.GetFollowerRelationship(user1, user2);

			Logger.LogError("IsFollowing(user1={User1},user2={User2}): userRelationship → {UserRelationship}", user1, user2, userRelationship);
			Logger.LogWarning("<<< Exiting IsFollowing(user1={User1},user2={User2})", user1, user2);
            return userRelationship is { Status: RelationshipStatus.Accepted, };
        }
    }
}
