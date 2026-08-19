// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users.Social.Data;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    internal class RelationshipControllerImpl(IDataService dataService, IEventLogger eventLogger, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        : IRelationshipController
    {
        internal const string FriendRequest = "FriendRequest";
        internal const string FollowerRequest = "FollowerRequest";
        internal const string FollowBackRequest = "FollowBackRequest";
        private readonly IDataService dataService = dataService ?? DataService.Instance;
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();

        /// <inheritdoc />
        public void DeleteRelationshipType(RelationshipType relationshipType)
        {
			eventLogger.LogDebug(">>> Entering DeleteRelationshipType(relationshipType={RelationshipType})", relationshipType);
            Requires.NotNull("relationshipType", relationshipType);

            this.dataService.DeleteRelationshipType(relationshipType.RelationshipTypeId);

            // log event
            string logContent =
                string.Format(
                    CultureInfo.InvariantCulture,
                    Localization.GetString("RelationshipType_Deleted", Localization.GlobalResourceFile),
                    relationshipType.Name,
                    relationshipType.RelationshipTypeId);
            this.AddLog(logContent);

            // clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
			eventLogger.LogDebug("<<< Exiting DeleteRelationshipType(relationshipType={RelationshipType})", relationshipType);
        }

        /// <inheritdoc />
        public IList<RelationshipType> GetAllRelationshipTypes()
        {
			eventLogger.LogDebug(">>> Entering GetAllRelationshipTypes()");
            var cacheArgs = new CacheItemArgs(
                DataCache.RelationshipTypesCacheKey,
                DataCache.RelationshipTypesCacheTimeOut,
                DataCache.RelationshipTypesCachePriority);
			eventLogger.LogDebug("<<< Exiting GetAllRelationshipTypes()");
            return CBO.GetCachedObject<IList<RelationshipType>>(
                this.hostSettings,
                cacheArgs,
                _ => CBO.FillCollection<RelationshipType>(this.dataService.GetAllRelationshipTypes()));
        }

        /// <inheritdoc />
        public RelationshipType GetRelationshipType(int relationshipTypeId)
        {
			eventLogger.LogDebug(">>> Entering GetRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
			eventLogger.LogDebug("<<< Exiting GetRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
            return this.GetAllRelationshipTypes().FirstOrDefault(r => r.RelationshipTypeId == relationshipTypeId);
        }

        /// <inheritdoc />
        public void SaveRelationshipType(RelationshipType relationshipType)
        {
			eventLogger.LogDebug(">>> Entering SaveRelationshipType(relationshipType={RelationshipType})", relationshipType);
            Requires.NotNull("relationshipType", relationshipType);

            string localizationKey = (relationshipType.RelationshipTypeId == Null.NullInteger)
                                         ? "RelationshipType_Added"
                                         : "RelationshipType_Updated";

            relationshipType.RelationshipTypeId = this.dataService.SaveRelationshipType(
                relationshipType,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // log event
            string logContent = string.Format(
                CultureInfo.CurrentCulture,
                Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                relationshipType.Name);
            this.AddLog(logContent);

            // clear cache
            DataCache.RemoveCache(DataCache.RelationshipTypesCacheKey);
			eventLogger.LogDebug("<<< Exiting SaveRelationshipType(relationshipType={RelationshipType})", relationshipType);
        }

        /// <inheritdoc />
        public void DeleteRelationship(Relationship relationship)
        {
			eventLogger.LogDebug(">>> Entering DeleteRelationship(relationship={Relationship})", relationship);
            Requires.NotNull("relationship", relationship);

            this.dataService.DeleteRelationship(relationship.RelationshipId);

            // log event
            string logContent =
                string.Format(
                    CultureInfo.InvariantCulture,
                    Localization.GetString("Relationship_Deleted", Localization.GlobalResourceFile),
                    relationship.Name,
                    relationship.RelationshipId);
            this.AddLog(logContent);

            // clear cache
            ClearRelationshipCache(relationship);
			eventLogger.LogDebug("<<< Exiting DeleteRelationship(relationship={Relationship})", relationship);
        }

        /// <inheritdoc />
        public Relationship GetRelationship(int relationshipId)
        {
			eventLogger.LogDebug(">>> Entering GetRelationship(relationshipId={RelationshipId})", relationshipId);
			eventLogger.LogDebug("<<< Exiting GetRelationship(relationshipId={RelationshipId})", relationshipId);
            return CBO.FillCollection<Relationship>(this.dataService.GetRelationship(relationshipId)).FirstOrDefault();
        }

        /// <inheritdoc />
        public IList<Relationship> GetRelationshipsByUserId(int userId)
        {
			eventLogger.LogDebug(">>> Entering GetRelationshipsByUserId(userId={UserId})", userId);
			eventLogger.LogDebug("<<< Exiting GetRelationshipsByUserId(userId={UserId})", userId);
            return CBO.FillCollection<Relationship>(this.dataService.GetRelationshipsByUserId(userId));
        }

        /// <inheritdoc />
        public IList<Relationship> GetRelationshipsByPortalId(int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetRelationshipsByPortalId(portalId={PortalId})", portalId);
            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(this.portalController, portalId))
            {
                pid = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, portalId);
            }

            var cacheArgs = new CacheItemArgs(
                string.Format(CultureInfo.InvariantCulture, DataCache.RelationshipByPortalIDCacheKey, pid),
                DataCache.RelationshipByPortalIDCacheTimeOut,
                DataCache.RelationshipByPortalIDCachePriority,
                pid);
			eventLogger.LogDebug("<<< Exiting GetRelationshipsByPortalId(portalId={PortalId})", portalId);
            return CBO.GetCachedObject<IList<Relationship>>(
                this.hostSettings,
                cacheArgs,
                c => CBO.FillCollection<Relationship>(this.dataService.GetRelationshipsByPortalId((int)c.ParamList[0])));
        }

        /// <inheritdoc />
        public void SaveRelationship(Relationship relationship)
        {
			eventLogger.LogDebug(">>> Entering SaveRelationship(relationship={Relationship})", relationship);
            Requires.NotNull("relationship", relationship);

            string localizationKey = (relationship.RelationshipId == Null.NullInteger)
                                         ? "Relationship_Added"
                                         : "Relationship_Updated";

            relationship.RelationshipId = this.dataService.SaveRelationship(
                relationship,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // log event
            string logContent = string.Format(
                CultureInfo.CurrentCulture,
                Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                relationship.Name);
            this.AddLog(logContent);

            // clear cache
            ClearRelationshipCache(relationship);
			eventLogger.LogDebug("<<< Exiting SaveRelationship(relationship={Relationship})", relationship);
        }

        /// <inheritdoc />
        public void DeleteUserRelationship(UserRelationship userRelationship)
        {
			eventLogger.LogDebug(">>> Entering DeleteUserRelationship(userRelationship={UserRelationship})", userRelationship);
            Requires.NotNull("userRelationship", userRelationship);

            this.dataService.DeleteUserRelationship(userRelationship.UserRelationshipId);

            // log event
            string logContent =
                string.Format(
                    CultureInfo.InvariantCulture,
                    Localization.GetString("UserRelationship_Deleted", Localization.GlobalResourceFile),
                    userRelationship.UserRelationshipId,
                    userRelationship.UserId,
                    userRelationship.RelatedUserId);
            this.AddLog(logContent);

            // cache clear
            ClearUserCache(userRelationship);
			eventLogger.LogDebug("<<< Exiting DeleteUserRelationship(userRelationship={UserRelationship})", userRelationship);
        }

        /// <inheritdoc />
        public UserRelationship GetUserRelationship(int userRelationshipId)
        {
			eventLogger.LogDebug(">>> Entering GetUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
			eventLogger.LogDebug("<<< Exiting GetUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
			eventLogger.LogDebug("<<< Exiting GetUserRelationship(user={User},relatedUser={RelatedUser},relationship={Relationship})", user, relatedUser, relationship);
            return CBO.FillObject<UserRelationship>(this.dataService.GetUserRelationship(userRelationshipId));
        }

        /// <inheritdoc />
        public UserRelationship GetUserRelationship(UserInfo user, UserInfo relatedUser, Relationship relationship)
        {
            UserRelationship userRelationship = null;
            if (relationship != null)
            {
                userRelationship = CBO.FillObject<UserRelationship>(
                    this.dataService.GetUserRelationship(
                        user.UserID,
                        relatedUser.UserID,
                        relationship.RelationshipId,
                        this.GetRelationshipType(relationship.RelationshipTypeId).Direction));
            }

            return userRelationship;
        }

        /// <inheritdoc />
        public IList<UserRelationship> GetUserRelationships(UserInfo user)
        {
			eventLogger.LogDebug(">>> Entering GetUserRelationships(user={User})", user);
			eventLogger.LogDebug("<<< Exiting GetUserRelationships(user={User})", user);
            return CBO.FillCollection<UserRelationship>(this.dataService.GetUserRelationships(user.UserID));
        }

        /// <inheritdoc />
        public void SaveUserRelationship(UserRelationship userRelationship)
        {
			eventLogger.LogDebug(">>> Entering SaveUserRelationship(userRelationship={UserRelationship})", userRelationship);
            Requires.NotNull("userRelationship", userRelationship);

            string localizationKey = (userRelationship.UserRelationshipId == Null.NullInteger)
                                         ? "UserRelationship_Added"
                                         : "UserRelationship_Updated";

            userRelationship.UserRelationshipId = this.dataService.SaveUserRelationship(
                userRelationship,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // log event
            string logContent = string.Format(
                CultureInfo.InvariantCulture,
                Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                userRelationship.UserRelationshipId,
                userRelationship.UserId,
                userRelationship.RelatedUserId);
            this.AddLog(logContent);

            // cache clear
            ClearUserCache(userRelationship);
			eventLogger.LogDebug("<<< Exiting SaveUserRelationship(userRelationship={UserRelationship})", userRelationship);
        }

        /// <inheritdoc />
        public void DeleteUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference)
        {
			eventLogger.LogDebug(">>> Entering DeleteUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference})", userRelationshipPreference);
            Requires.NotNull("userRelationshipPreference", userRelationshipPreference);

            this.dataService.DeleteUserRelationshipPreference(userRelationshipPreference.PreferenceId);

            // log event
            string logContent =
                string.Format(
                    CultureInfo.InvariantCulture,
                    Localization.GetString("UserRelationshipPreference_Deleted", Localization.GlobalResourceFile),
                    userRelationshipPreference.PreferenceId,
                    userRelationshipPreference.UserId,
                    userRelationshipPreference.RelationshipId);
            this.AddLog(logContent);
			eventLogger.LogDebug("<<< Exiting DeleteUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference})", userRelationshipPreference);
        }

        /// <inheritdoc />
        public UserRelationshipPreference GetUserRelationshipPreference(int preferenceId)
        {
			eventLogger.LogDebug(">>> Entering GetUserRelationshipPreference(preferenceId={PreferenceId})", preferenceId);
				eventLogger.LogDebug("<<< Exiting GetUserRelationshipPreference(preferenceId={PreferenceId})", preferenceId);
            return
			eventLogger.LogDebug("<<< Exiting GetUserRelationshipPreference(userId={UserId},relationshipId={RelationshipId})", userId, relationshipId);
                CBO.FillObject<UserRelationshipPreference>(this.dataService.GetUserRelationshipPreferenceById(preferenceId));
        }

        /// <inheritdoc />
        public UserRelationshipPreference GetUserRelationshipPreference(int userId, int relationshipId)
        {
            return
                CBO.FillObject<UserRelationshipPreference>(this.dataService.GetUserRelationshipPreference(
                    userId,
                    relationshipId));
        }

        /// <inheritdoc />
        public void SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference)
        {
			eventLogger.LogDebug(">>> Entering SaveUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference})", userRelationshipPreference);
            Requires.NotNull("userRelationshipPreference", userRelationshipPreference);

            string localizationKey = (userRelationshipPreference.PreferenceId == Null.NullInteger)
                                         ? "UserRelationshipPreference_Added"
                                         : "UserRelationshipPreference_Updated";

            userRelationshipPreference.PreferenceId =
                this.dataService.SaveUserRelationshipPreference(
                    userRelationshipPreference,
                    UserController.Instance.GetCurrentUserInfo().UserID);

            // log event
            string logContent = string.Format(
                CultureInfo.InvariantCulture,
                Localization.GetString(localizationKey, Localization.GlobalResourceFile),
                userRelationshipPreference.PreferenceId,
                userRelationshipPreference.UserId,
                userRelationshipPreference.RelationshipId);
            this.AddLog(logContent);
			eventLogger.LogDebug("<<< Exiting SaveUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference})", userRelationshipPreference);
        }

        /// <summary>Initiate an UserRelationship Request.</summary>
        /// <param name="initiatingUser">UserInfo of the user initiating the request.</param>
        /// <param name="targetUser">UserInfo of the user being solicited for initiating the request.</param>
        /// <param name="relationship">Relationship to associate this request to (Portal-Level Relationship or User-Level Relationship).</param>
        /// <remarks>
        /// If all conditions are met UserRelationship object belonging to Initiating User is returned.
        /// </remarks>
        /// <returns>
        /// Relationship object belonging to the initiating user.
        /// </returns>
        /// <exception cref="UserRelationshipBlockedException">Target user has Blocked any relationship request from Initiating user.</exception>
        /// <exception cref="InvalidRelationshipTypeException">Relationship type does not exist.</exception>
        public UserRelationship InitiateUserRelationship(UserInfo initiatingUser, UserInfo targetUser, Relationship relationship)
        {
			eventLogger.LogDebug(">>> Entering InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship})", initiatingUser, targetUser, relationship);
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);
            Requires.NotNull("relationship", relationship);

            Requires.PropertyNotNegative("user1", "UserID", initiatingUser.UserID);
            Requires.PropertyNotNegative("user2", "UserID", targetUser.UserID);

            Requires.PropertyNotNegative("user1", "PortalID", initiatingUser.PortalID);
            Requires.PropertyNotNegative("user2", "PortalID", targetUser.PortalID);

            Requires.PropertyNotNegative("relationship", "RelationshipId", relationship.RelationshipId);

            // cannot be same user
            if (initiatingUser.UserID == targetUser.UserID)
            {
				eventLogger.LogWarning("Throwing UserRelationshipForSameUsersException from InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship})", initiatingUser, targetUser, relationship);
                throw new UserRelationshipForSameUsersException(
                    Localization.GetExceptionMessage(
                        "UserRelationshipForSameUsersError",
                        "Initiating and Target Users cannot have same UserID '{0}'.",
                        initiatingUser.UserID));
            }

            // users must be from same portal
            if (initiatingUser.PortalID != targetUser.PortalID)
            {
				eventLogger.LogWarning("Throwing UserRelationshipForDifferentPortalException from InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship})", initiatingUser, targetUser, relationship);
                throw new UserRelationshipForDifferentPortalException(
                    Localization.GetExceptionMessage(
                        "UserRelationshipForDifferentPortalError",
                        "Portal ID '{0}' of Initiating User is different from Portal ID '{1}' of Target  User.",
                        initiatingUser.PortalID,
                        targetUser.PortalID));
            }

            // check for existing UserRelationship record
            UserRelationship existingRelationship = this.GetUserRelationship(initiatingUser, targetUser, relationship);
			eventLogger.LogDebug("InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship}): existingRelationship → {ExistingRelationship}", initiatingUser, targetUser, relationship, existingRelationship);
            if (existingRelationship != null)
            {
				eventLogger.LogWarning("Throwing UserRelationshipExistsException from InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship})", initiatingUser, targetUser, relationship);
                throw new UserRelationshipExistsException(
                    Localization.GetExceptionMessage(
                        "UserRelationshipExistsError",
                        "Relationship already exists for Initiating User '{0}' Target User '{1}' RelationshipID '{2}'.",
                        initiatingUser.UserID,
                        targetUser.UserID,
                        relationship.RelationshipId));
            }

            // no existing UserRelationship record found

            // use Relationship DefaultResponse as status
            RelationshipStatus status = relationship.DefaultResponse;
			eventLogger.LogDebug("InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship}): status → {Status}", initiatingUser, targetUser, relationship, status);

            UserRelationshipPreference preference = this.GetUserRelationshipPreference(
                targetUser.UserID,
                relationship.RelationshipId);
            if (preference != null)
            {
                status = preference.DefaultResponse;
            }

            if (status == RelationshipStatus.None)
            {
                status = RelationshipStatus.Pending;
            }

            var userRelationship = new UserRelationship
            {
                UserRelationshipId = Null.NullInteger,
                UserId = initiatingUser.UserID,
                RelatedUserId = targetUser.UserID,
                RelationshipId = relationship.RelationshipId,
                Status = status,
            };

            this.SaveUserRelationship(userRelationship);

			eventLogger.LogDebug("<<< Exiting InitiateUserRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser},relationship={Relationship})", initiatingUser, targetUser, relationship);
            return userRelationship;
        }

        /// <summary>Accept an existing UserRelationship Request.</summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship.</param>
        /// <remarks>
        /// Method updates the status of the UserRelationship to Accepted.
        /// </remarks>
        public void AcceptUserRelationship(int userRelationshipId)
        {
			eventLogger.LogDebug(">>> Entering AcceptUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
            this.ManageUserRelationshipStatus(userRelationshipId, RelationshipStatus.Accepted);
			eventLogger.LogDebug("<<< Exiting AcceptUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
        }

        /// <summary>Remove an existing UserRelationship Request.</summary>
        /// <param name="userRelationshipId">UserRelationshipId of the UserRelationship.</param>
        /// <remarks>
        /// UserRelationship record is physically removed.
        /// </remarks>
        public void RemoveUserRelationship(int userRelationshipId)
        {
			eventLogger.LogDebug(">>> Entering RemoveUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
            UserRelationship userRelationship = this.VerifyUserRelationshipExist(userRelationshipId);
            if (userRelationship != null)
            {
                this.DeleteUserRelationship(userRelationship);
            }
			eventLogger.LogDebug("<<< Exiting RemoveUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
        }

        /// <summary>GetFollowerRelationship - Get the UserRelationship between Current User and the Target Users in Follower Relationship.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <returns>UserRelationship.</returns>
        /// <remarks>UserRelationship object is returned if a Follower Relationship exists between the two Users.
        /// The relation status can be Any (Initiated / Accepted / Blocked). Follower Relationship can be initited by either of the Users.
        /// </remarks>
        public UserRelationship GetFollowerRelationship(UserInfo targetUser)
        {
			eventLogger.LogDebug(">>> Entering GetFollowerRelationship(targetUser={TargetUser})", targetUser);
			eventLogger.LogDebug("<<< Exiting GetFollowerRelationship(targetUser={TargetUser})", targetUser);
			eventLogger.LogDebug("<<< Exiting GetFollowerRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            return this.GetFollowerRelationship(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// <summary>GetFollowerRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Follower Relationship.</summary>
        /// <param name="initiatingUser">UserInfo for Initiating User.</param>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <returns>UserRelationship.</returns>
        /// <remarks>UserRelationship object is returned if a Follower Relationship exists between the two Users.
        /// The relation status can be Any (Initiated / Accepted / Blocked). Follower Relationship can be initited by either of the Users.
        /// </remarks>
        public UserRelationship GetFollowerRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);

            return this.GetUserRelationship(
                initiatingUser,
                targetUser,
                this.GetFollowersRelationshipByPortal(initiatingUser.PortalID));
        }

        /// <summary>GetFollowingRelationship - Get the UserRelationship between Current User and the Target Users in Following Relationship.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <returns>UserRelationship.</returns>
        /// <remarks>UserRelationship object is returned if a Following Relationship exists between the two Users.
        /// The relation status can be Any (Initiated / Accepted / Blocked).
        /// </remarks>
        public UserRelationship GetFollowingRelationship(UserInfo targetUser)
        {
			eventLogger.LogDebug(">>> Entering GetFollowingRelationship(targetUser={TargetUser})", targetUser);
			eventLogger.LogDebug("<<< Exiting GetFollowingRelationship(targetUser={TargetUser})", targetUser);
			eventLogger.LogDebug("<<< Exiting GetFollowingRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            return this.GetFollowingRelationship(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// <summary>GetFollowingRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Following Relationship.</summary>
        /// <param name="initiatingUser">UserInfo for Initiating User.</param>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <returns>UserRelationship.</returns>
        /// <remarks>UserRelationship object is returned if a Following Relationship exists between the two Users.
        /// The relation status can be Any (Initiated / Accepted / Blocked).
        /// </remarks>
        public UserRelationship GetFollowingRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);

            return this.GetUserRelationship(
                targetUser,
                initiatingUser,
                this.GetFollowersRelationshipByPortal(initiatingUser.PortalID));
        }

        /// <summary>GetFriendRelationship - Get the UserRelationship between Current User and the Target Users in Friend Relationship.</summary>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <returns>UserRelationship.</returns>
        /// <remarks>UserRelationship object is returned if a Friend Relationship exists between the two Users.
        /// The relation status can be Any (Initiated / Accepted / Blocked). Friend Relationship can be initited by either of the Users.
        /// </remarks>
        public UserRelationship GetFriendRelationship(UserInfo targetUser)
        {
			eventLogger.LogDebug(">>> Entering GetFriendRelationship(targetUser={TargetUser})", targetUser);
			eventLogger.LogDebug("<<< Exiting GetFriendRelationship(targetUser={TargetUser})", targetUser);
			eventLogger.LogDebug("<<< Exiting GetFriendRelationship(initiatingUser={InitiatingUser},targetUser={TargetUser})", initiatingUser, targetUser);
            return this.GetFriendRelationship(UserController.Instance.GetCurrentUserInfo(), targetUser);
        }

        /// <summary>GetFriendRelationship - Get the UserRelationship between InitiatingUser User and the Target Users in Friend Relationship.</summary>
        /// <param name="initiatingUser">UserInfo for Initiating User.</param>
        /// <param name="targetUser">UserInfo for Target User.</param>
        /// <returns>UserRelationship.</returns>
        /// <remarks>UserRelationship object is returned if a Friend Relationship exists between the two Users.
        /// The relation status can be Any (Initiated / Accepted / Blocked). Friend Relationship can be initited by either of the Users.
        /// </remarks>
        public UserRelationship GetFriendRelationship(UserInfo initiatingUser, UserInfo targetUser)
        {
            Requires.NotNull("user1", initiatingUser);
            Requires.NotNull("user2", targetUser);

            return this.GetUserRelationship(
                initiatingUser,
                targetUser,
                this.GetFriendsRelationshipByPortal(initiatingUser.PortalID));
        }

        /// <inheritdoc />
        public void CreateDefaultRelationshipsForPortal(int portalId)
        {
			eventLogger.LogDebug(">>> Entering CreateDefaultRelationshipsForPortal(portalId={PortalId})", portalId);
            // create default Friend Relationship
            if (this.GetFriendsRelationshipByPortal(portalId) == null)
            {
                var friendRelationship = new Relationship
                {
                    RelationshipId = Null.NullInteger,
                    Name = nameof(DefaultRelationshipTypes.Friends),
                    Description = nameof(DefaultRelationshipTypes.Friends),
                    PortalId = portalId,
                    UserId = Null.NullInteger,
                    DefaultResponse = RelationshipStatus.None,

                    // default response is None
                    RelationshipTypeId = (int)DefaultRelationshipTypes.Friends,
                };
                this.SaveRelationship(friendRelationship);
            }

            // create default Follower Relationship
            if (this.GetFollowersRelationshipByPortal(portalId) == null)
            {
                var followerRelationship = new Relationship
                {
                    RelationshipId = Null.NullInteger,
                    Name = nameof(DefaultRelationshipTypes.Followers),
                    Description = nameof(DefaultRelationshipTypes.Followers),
                    PortalId = portalId,
                    UserId = Null.NullInteger,
                    DefaultResponse = RelationshipStatus.Accepted,

                    // default response is Accepted
                    RelationshipTypeId = (int)DefaultRelationshipTypes.Followers,
                };
                this.SaveRelationship(followerRelationship);
            }
			eventLogger.LogDebug("<<< Exiting CreateDefaultRelationshipsForPortal(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public Relationship GetFriendsRelationshipByPortal(int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetFriendsRelationshipByPortal(portalId={PortalId})", portalId);
			eventLogger.LogDebug("<<< Exiting GetFriendsRelationshipByPortal(portalId={PortalId})", portalId);
            return this.GetRelationshipsByPortalId(portalId).FirstOrDefault(re => re.RelationshipTypeId == (int)DefaultRelationshipTypes.Friends);
        }

        /// <inheritdoc />
        public Relationship GetFollowersRelationshipByPortal(int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetFollowersRelationshipByPortal(portalId={PortalId})", portalId);
			eventLogger.LogDebug("<<< Exiting GetFollowersRelationshipByPortal(portalId={PortalId})", portalId);
            return this.GetRelationshipsByPortalId(portalId).FirstOrDefault(re => re.RelationshipTypeId == (int)DefaultRelationshipTypes.Followers);
        }

        private static void ClearRelationshipCache(Relationship relationship)
        {
			eventLogger.LogDebug(">>> Entering ClearRelationshipCache(relationship={Relationship})", relationship);
            if (relationship.UserId == Null.NullInteger)
            {
                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.RelationshipByPortalIDCacheKey, relationship.PortalId));
            }
			eventLogger.LogDebug("<<< Exiting ClearRelationshipCache(relationship={Relationship})", relationship);
        }

        private static void ClearUserCache(UserRelationship userRelationship)
        {
			eventLogger.LogDebug(">>> Entering ClearUserCache(userRelationship={UserRelationship})", userRelationship);
            // Get Portal
            var settings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("ClearUserCache(userRelationship={UserRelationship}): settings → {Settings}", userRelationship, settings);
            if (settings != null)
            {
                // Get User
                UserInfo user = UserController.GetUserById(settings.PortalId, userRelationship.UserId);
				eventLogger.LogDebug("ClearUserCache(userRelationship={UserRelationship}): user → {User}", userRelationship, user);

                if (user != null)
                {
                    DataCache.ClearUserCache(settings.PortalId, user.Username);
                }

                // Get Related User
                UserInfo relatedUser = UserController.GetUserById(settings.PortalId, userRelationship.RelatedUserId);
				eventLogger.LogDebug("ClearUserCache(userRelationship={UserRelationship}): relatedUser → {RelatedUser}", userRelationship, relatedUser);

                if (relatedUser != null)
                {
                    DataCache.ClearUserCache(settings.PortalId, relatedUser.Username);
                }
            }
			eventLogger.LogDebug("<<< Exiting ClearUserCache(userRelationship={UserRelationship})", userRelationship);
        }

        private void AddLog(string logContent)
        {
			eventLogger.LogDebug(">>> Entering AddLog(logContent={LogContent})", logContent);
            this.eventLogger.AddLog("Message", logContent, EventLogType.ADMIN_ALERT);
			eventLogger.LogDebug("<<< Exiting AddLog(logContent={LogContent})", logContent);
        }

        private void ManageUserRelationshipStatus(int userRelationshipId, RelationshipStatus newStatus)
        {
			eventLogger.LogDebug(">>> Entering ManageUserRelationshipStatus(userRelationshipId={UserRelationshipId},newStatus={NewStatus})", userRelationshipId, newStatus);
            UserRelationship userRelationship = this.VerifyUserRelationshipExist(userRelationshipId);
            if (userRelationship == null)
            {
                return;
            }

            // TODO - apply business rules - throw exception if newStatus requested is not allowed
            bool save = false;
			eventLogger.LogDebug("ManageUserRelationshipStatus(userRelationshipId={UserRelationshipId},newStatus={NewStatus}): save → {Save}", userRelationshipId, newStatus, save);
            switch (newStatus)
            {
                case RelationshipStatus.None:
                    save = true;
                    break;
                case RelationshipStatus.Pending:
                    save = true;
                    break;
                case RelationshipStatus.Accepted:
                    save = true;
                    break;
            }

            if (save)
            {
                userRelationship.Status = newStatus;
				eventLogger.LogDebug("ManageUserRelationshipStatus(userRelationshipId={UserRelationshipId},newStatus={NewStatus}): userRelationship.Status → {Arg2}", userRelationshipId, newStatus, userRelationship.Status);
                this.SaveUserRelationship(userRelationship);
            }
			eventLogger.LogDebug("<<< Exiting ManageUserRelationshipStatus(userRelationshipId={UserRelationshipId},newStatus={NewStatus})", userRelationshipId, newStatus);
        }

        private UserRelationship VerifyUserRelationshipExist(int userRelationshipId)
        {
			eventLogger.LogDebug(">>> Entering VerifyUserRelationshipExist(userRelationshipId={UserRelationshipId})", userRelationshipId);
            UserRelationship userRelationship = this.GetUserRelationship(userRelationshipId);
            if (userRelationship == null)
            {
				eventLogger.LogWarning("Throwing UserRelationshipDoesNotExistException from VerifyUserRelationshipExist(userRelationshipId={UserRelationshipId})", userRelationshipId);
			eventLogger.LogDebug("<<< Exiting VerifyUserRelationshipExist(userRelationshipId={UserRelationshipId})", userRelationshipId);
                throw new UserRelationshipDoesNotExistException(
                    Localization.GetExceptionMessage(
                        "UserRelationshipDoesNotExistError",
                        "UserRelationshipID '{0}' does not exist.",
                        userRelationshipId));
            }

            return userRelationship;
        }
    }
}
