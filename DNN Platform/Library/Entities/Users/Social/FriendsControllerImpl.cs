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

    internal class FriendsControllerImpl : IFriendsController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FriendsControllerImpl));
        internal const string FriendRequest = "FriendRequest";

        ////static FriendsControllerImpl()
        ////{
        ////}

        /// <summary>AcceptFriend - Current User accepts a Friend Request to the Target User.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        public void AcceptFriend(UserInfo targetUser)
        {
			Logger.LogError(">>> Entering AcceptFriend(targetUser={TargetUser})", targetUser);
            var initiatingUser = UserController.Instance.GetCurrentUserInfo();
            var friendRelationship = RelationshipController.Instance.GetFriendRelationship(initiatingUser, targetUser);
			Logger.LogError("AcceptFriend(targetUser={TargetUser}): friendRelationship → {FriendRelationship}", targetUser, friendRelationship);
			Logger.LogError("AcceptFriend(targetUser={TargetUser}): initiatingUser → {InitiatingUser}", targetUser, initiatingUser);

            RelationshipController.Instance.AcceptUserRelationship(friendRelationship.UserRelationshipId);
            NotificationsController.Instance.DeleteNotificationRecipient(
                NotificationsController.Instance.GetNotificationType(FriendRequest).NotificationTypeId,
                friendRelationship.UserRelationshipId.ToString(CultureInfo.InvariantCulture),
                initiatingUser.UserID);

            EventManager.Instance.OnFriendshipAccepted(new RelationshipEventArgs(friendRelationship, initiatingUser.PortalID));
			Logger.LogWarning("<<< Exiting AcceptFriend(targetUser={TargetUser})", targetUser);
        }

        /// <summary>AddFriend - Current User initiates a Friend Request to the Target User.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <remarks>If the Friend Relationship is setup for auto-acceptance at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        public void AddFriend(UserInfo targetUser)
        {
			Logger.LogError(">>> Entering AddFriend(targetUser={TargetUser})", targetUser);
            var initiatingUser = UserController.Instance.GetCurrentUserInfo();
            this.AddFriend(initiatingUser, targetUser);
			Logger.LogError("AddFriend(targetUser={TargetUser}): initiatingUser → {InitiatingUser}", targetUser, initiatingUser);
			Logger.LogWarning("<<< Exiting AddFriend(targetUser={TargetUser})", targetUser);
        }

        /// <summary>AddFriend - Initiating User initiates a Friend Request to the Target User.</summary>
        /// <param name="initiatingUser">UserInfo for Initiating User.</param>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <remarks>If the Friend Relationship is set up for auto-acceptance at the Portal level, the UserRelationship
        /// status is set as Accepted, otherwise it is set as Initiated.
        /// </remarks>
        public void AddFriend(UserInfo initiatingUser, UserInfo targetUser)
        {
			Logger.LogError(">>> Entering AddFriend(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            Requires.NotNull("user1", initiatingUser);

            // Check if the friendship has been requested first by target user
            var targetUserRelationship = RelationshipController.Instance.GetFriendRelationship(
                targetUser,
                initiatingUser);
            if (targetUserRelationship is { Status: RelationshipStatus.Pending, } && targetUserRelationship.RelatedUserId == initiatingUser.UserID)
            {
                RelationshipController.Instance.AcceptUserRelationship(targetUserRelationship.UserRelationshipId);
                return;
            }

            var userRelationship = RelationshipController.Instance.InitiateUserRelationship(
                initiatingUser,
                targetUser,
                RelationshipController.Instance.GetFriendsRelationshipByPortal(initiatingUser.PortalID));

            AddFriendRequestNotification(initiatingUser, targetUser, userRelationship);

            EventManager.Instance.OnFriendshipRequested(new RelationshipEventArgs(userRelationship, initiatingUser.PortalID));
			Logger.LogWarning("<<< Exiting AddFriend(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
        }

        /// <summary>DeleteFriend - Current User deletes a friend relationship with the target User.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        public void DeleteFriend(UserInfo targetUser)
        {
			Logger.LogError(">>> Entering DeleteFriend(targetUser={TargetUser})", targetUser);
            var initiatingUser = UserController.Instance.GetCurrentUserInfo();
            this.DeleteFriend(initiatingUser, targetUser);
			Logger.LogError("DeleteFriend(targetUser={TargetUser}): initiatingUser → {InitiatingUser}", targetUser, initiatingUser);
			Logger.LogWarning("<<< Exiting DeleteFriend(targetUser={TargetUser})", targetUser);
        }

        /// <summary>DeleteFriend - Initiating User deletes a friend relationship with the target User.</summary>
        /// <param name="initiatingUser">UserInfo for Initiating User.</param>
        /// <param name="targetUser">UserInfo for Target User.</param>
        public void DeleteFriend(UserInfo initiatingUser, UserInfo targetUser)
        {
			Logger.LogError(">>> Entering DeleteFriend(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            Requires.NotNull("user1", initiatingUser);

            var friend = RelationshipController.Instance.GetUserRelationship(
                initiatingUser,
                targetUser,
                RelationshipController.Instance.GetFriendsRelationshipByPortal(initiatingUser.PortalID));

            RelationshipController.Instance.DeleteUserRelationship(friend);

            EventManager.Instance.OnFriendshipDeleted(new RelationshipEventArgs(friend, initiatingUser.PortalID));
			Logger.LogWarning("<<< Exiting DeleteFriend(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
        }

        private static void AddFriendRequestNotification(UserInfo initiatingUser, UserInfo targetUser, UserRelationship userRelationship)
        {
			Logger.LogError(">>> Entering AddFriendRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser},userRelationship={UserRelationship})", initiatingUser, targetUser, userRelationship);
            var notificationType = NotificationsController.Instance.GetNotificationType(FriendRequest);
            var language = GetUserPreferredLocale(targetUser)?.Name;
			Logger.LogError("AddFriendRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser},userRelationship={UserRelationship}): language → {Language}", initiatingUser, targetUser, userRelationship, language);
            var subject = string.Format(
                CultureInfo.CurrentCulture,
                Localization.GetString("AddFriendRequestSubject", Localization.GlobalResourceFile, language),
                initiatingUser.DisplayName);
				Logger.LogError("AddFriendRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser},userRelationship={UserRelationship}): notificationType → {NotificationType}", initiatingUser, targetUser, userRelationship, notificationType);

            var body = string.Format(
                CultureInfo.CurrentCulture,
                Localization.GetString("AddFriendRequestBody", Localization.GlobalResourceFile, language),
                initiatingUser.DisplayName);

            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                Context = userRelationship.UserRelationshipId.ToString(CultureInfo.InvariantCulture),
                SenderUserID = initiatingUser.UserID,
            };

            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, null, new List<UserInfo> { targetUser });
			Logger.LogWarning("<<< Exiting AddFriendRequestNotification(initiatingUser={InitiatingUser},targetUser={TargetUser},userRelationship={UserRelationship})", initiatingUser, targetUser, userRelationship);
        }

        private static CultureInfo GetUserPreferredLocale(UserInfo user)
        {
			Logger.LogError(">>> Entering GetUserPreferredLocale(user={User})", user);
            string language = user.Profile.PreferredLocale;
            if (!string.IsNullOrEmpty(language))
            {
                return Localization.GetCultureFromString(user.PortalID, language);
            }

			Logger.LogWarning("<<< Exiting GetUserPreferredLocale(user={User})", user);
            return null;
        }
    }
}
