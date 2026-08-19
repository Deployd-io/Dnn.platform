// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Social.Data
{
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;

    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        private readonly DataProvider provider = DataProvider.Instance();

        /// <inheritdoc />
        public IDataReader GetAllRelationshipTypes()
        {
			Logger.LogDebug(">>> Entering GetAllRelationshipTypes()");
			Logger.LogDebug("<<< Exiting GetAllRelationshipTypes()");
            return this.provider.ExecuteReader("GetAllRelationshipTypes");
        }

        /// <inheritdoc />
        public IDataReader GetRelationshipType(int relationshipTypeId)
        {
			Logger.LogDebug(">>> Entering GetRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
			Logger.LogDebug("<<< Exiting GetRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
            return this.provider.ExecuteReader("GetRelationshipType", relationshipTypeId);
        }

        /// <inheritdoc />
        public void DeleteRelationshipType(int relationshipTypeId)
        {
			Logger.LogDebug(">>> Entering DeleteRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
            this.provider.ExecuteNonQuery("DeleteRelationshipType", relationshipTypeId);
			Logger.LogDebug("<<< Exiting DeleteRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
        }

        /// <inheritdoc />
        public int SaveRelationshipType(RelationshipType relationshipType, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveRelationshipType(relationshipType={RelationshipType},createUpdateUserId={CreateUpdateUserId})", relationshipType, createUpdateUserId);
			Logger.LogDebug("<<< Exiting SaveRelationshipType(relationshipType={RelationshipType},createUpdateUserId={CreateUpdateUserId})", relationshipType, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveRelationshipType", relationshipType.RelationshipTypeId, relationshipType.Direction, relationshipType.Name, relationshipType.Description, createUpdateUserId);
        }

        /// <inheritdoc />
        public void DeleteRelationship(int relationshipId)
        {
			Logger.LogDebug(">>> Entering DeleteRelationship(relationshipId={RelationshipId})", relationshipId);
            this.provider.ExecuteNonQuery("DeleteRelationship", relationshipId);
			Logger.LogDebug("<<< Exiting DeleteRelationship(relationshipId={RelationshipId})", relationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetRelationship(int relationshipId)
        {
			Logger.LogDebug(">>> Entering GetRelationship(relationshipId={RelationshipId})", relationshipId);
			Logger.LogDebug("<<< Exiting GetRelationship(relationshipId={RelationshipId})", relationshipId);
            return this.provider.ExecuteReader("GetRelationship", relationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetRelationshipsByUserId(int userId)
        {
			Logger.LogDebug(">>> Entering GetRelationshipsByUserId(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetRelationshipsByUserId(userId={UserId})", userId);
            return this.provider.ExecuteReader("GetRelationshipsByUserID", userId);
        }

        /// <inheritdoc />
        public IDataReader GetRelationshipsByPortalId(int portalId)
        {
			Logger.LogDebug(">>> Entering GetRelationshipsByPortalId(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetRelationshipsByPortalId(portalId={PortalId})", portalId);
            return this.provider.ExecuteReader("GetRelationshipsByPortalID", portalId);
        }

        /// <inheritdoc />
        public int SaveRelationship(Relationship relationship, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveRelationship(relationship={Relationship},createUpdateUserId={CreateUpdateUserId})", relationship, createUpdateUserId);
			Logger.LogDebug("<<< Exiting SaveRelationship(relationship={Relationship},createUpdateUserId={CreateUpdateUserId})", relationship, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveRelationship", relationship.RelationshipId, relationship.RelationshipTypeId, relationship.Name, relationship.Description, this.provider.GetNull(relationship.UserId), this.provider.GetNull(relationship.PortalId), relationship.DefaultResponse, createUpdateUserId);
        }

        /// <inheritdoc />
        public void DeleteUserRelationship(int userRelationshipId)
        {
			Logger.LogDebug(">>> Entering DeleteUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
            this.provider.ExecuteNonQuery("DeleteUserRelationship", userRelationshipId);
			Logger.LogDebug("<<< Exiting DeleteUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationship(int userRelationshipId)
        {
			Logger.LogDebug(">>> Entering GetUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
			Logger.LogDebug("<<< Exiting GetUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
			Logger.LogDebug("<<< Exiting GetUserRelationship(userId={UserId},relatedUserId={RelatedUserId},relationshipId={RelationshipId},relationshipDirection={RelationshipDirection})", userId, relatedUserId, relationshipId, relationshipDirection);
            return this.provider.ExecuteReader("GetUserRelationship", userRelationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationship(int userId, int relatedUserId, int relationshipId, RelationshipDirection relationshipDirection)
        {
            return this.provider.ExecuteReader("GetUserRelationshipsByMultipleIDs", userId, relatedUserId, relationshipId, relationshipDirection);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationships(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserRelationships(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetUserRelationships(userId={UserId})", userId);
            return this.provider.ExecuteReader("GetUserRelationships", userId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationshipsByRelationshipId(int relationshipId)
        {
			Logger.LogDebug(">>> Entering GetUserRelationshipsByRelationshipId(relationshipId={RelationshipId})", relationshipId);
			Logger.LogDebug("<<< Exiting GetUserRelationshipsByRelationshipId(relationshipId={RelationshipId})", relationshipId);
            return this.provider.ExecuteReader("GetUserRelationshipsByRelationshipID", relationshipId);
        }

        /// <inheritdoc />
        public int SaveUserRelationship(UserRelationship userRelationship, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveUserRelationship(userRelationship={UserRelationship},createUpdateUserId={CreateUpdateUserId})", userRelationship, createUpdateUserId);
			Logger.LogDebug("<<< Exiting SaveUserRelationship(userRelationship={UserRelationship},createUpdateUserId={CreateUpdateUserId})", userRelationship, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveUserRelationship", userRelationship.UserRelationshipId, userRelationship.UserId, userRelationship.RelatedUserId, userRelationship.RelationshipId, userRelationship.Status, createUpdateUserId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationshipPreferenceById(int preferenceId)
        {
			Logger.LogDebug(">>> Entering GetUserRelationshipPreferenceById(preferenceId={PreferenceId})", preferenceId);
			Logger.LogDebug("<<< Exiting GetUserRelationshipPreferenceById(preferenceId={PreferenceId})", preferenceId);
            return this.provider.ExecuteReader("GetUserRelationshipPreferenceByID", preferenceId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationshipPreference(int userId, int relationshipId)
        {
			Logger.LogDebug(">>> Entering GetUserRelationshipPreference(userId={UserId},relationshipId={RelationshipId})", userId, relationshipId);
			Logger.LogDebug("<<< Exiting GetUserRelationshipPreference(userId={UserId},relationshipId={RelationshipId})", userId, relationshipId);
            return this.provider.ExecuteReader("GetUserRelationshipPreference", userId, relationshipId);
        }

        /// <inheritdoc />
        public void DeleteUserRelationshipPreference(int preferenceId)
        {
			Logger.LogDebug(">>> Entering DeleteUserRelationshipPreference(preferenceId={PreferenceId})", preferenceId);
            this.provider.ExecuteNonQuery("DeleteUserRelationshipPreference", preferenceId);
			Logger.LogDebug("<<< Exiting DeleteUserRelationshipPreference(preferenceId={PreferenceId})", preferenceId);
        }

        /// <inheritdoc />
        public int SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference},createUpdateUserId={CreateUpdateUserId})", userRelationshipPreference, createUpdateUserId);
			Logger.LogDebug("<<< Exiting SaveUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference},createUpdateUserId={CreateUpdateUserId})", userRelationshipPreference, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveUserRelationshipPreference", userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId, userRelationshipPreference.RelationshipId, userRelationshipPreference.DefaultResponse, createUpdateUserId);
        }
    }
}
