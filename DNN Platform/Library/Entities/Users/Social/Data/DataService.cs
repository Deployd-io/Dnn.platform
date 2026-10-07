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
			Logger.LogError(">>> Entering GetAllRelationshipTypes()");
            Logger.LogWarning("<<< Exiting GetAllRelationshipTypes()");
            return this.provider.ExecuteReader("GetAllRelationshipTypes");
        }

        /// <inheritdoc />
        public IDataReader GetRelationshipType(int relationshipTypeId)
        {
			Logger.LogError(">>> Entering GetRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
            Logger.LogWarning("<<< Exiting GetRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
            return this.provider.ExecuteReader("GetRelationshipType", relationshipTypeId);
        }

        /// <inheritdoc />
        public void DeleteRelationshipType(int relationshipTypeId)
        {
			Logger.LogError(">>> Entering DeleteRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
            this.provider.ExecuteNonQuery("DeleteRelationshipType", relationshipTypeId);
			Logger.LogWarning("<<< Exiting DeleteRelationshipType(relationshipTypeId={RelationshipTypeId})", relationshipTypeId);
        }

        /// <inheritdoc />
        public int SaveRelationshipType(RelationshipType relationshipType, int createUpdateUserId)
        {
			Logger.LogError(">>> Entering SaveRelationshipType(relationshipType={RelationshipType},createUpdateUserId={CreateUpdateUserId})", relationshipType, createUpdateUserId);
            Logger.LogWarning("<<< Exiting SaveRelationshipType(relationshipType={RelationshipType},createUpdateUserId={CreateUpdateUserId})", relationshipType, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveRelationshipType", relationshipType.RelationshipTypeId, relationshipType.Direction, relationshipType.Name, relationshipType.Description, createUpdateUserId);
        }

        /// <inheritdoc />
        public void DeleteRelationship(int relationshipId)
        {
			Logger.LogError(">>> Entering DeleteRelationship(relationshipId={RelationshipId})", relationshipId);
            this.provider.ExecuteNonQuery("DeleteRelationship", relationshipId);
			Logger.LogWarning("<<< Exiting DeleteRelationship(relationshipId={RelationshipId})", relationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetRelationship(int relationshipId)
        {
			Logger.LogError(">>> Entering GetRelationship(relationshipId={RelationshipId})", relationshipId);
            Logger.LogWarning("<<< Exiting GetRelationship(relationshipId={RelationshipId})", relationshipId);
            return this.provider.ExecuteReader("GetRelationship", relationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetRelationshipsByUserId(int userId)
        {
			Logger.LogError(">>> Entering GetRelationshipsByUserId(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetRelationshipsByUserId(userId={UserId})", userId);
            return this.provider.ExecuteReader("GetRelationshipsByUserID", userId);
        }

        /// <inheritdoc />
        public IDataReader GetRelationshipsByPortalId(int portalId)
        {
			Logger.LogError(">>> Entering GetRelationshipsByPortalId(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetRelationshipsByPortalId(portalId={PortalId})", portalId);
            return this.provider.ExecuteReader("GetRelationshipsByPortalID", portalId);
        }

        /// <inheritdoc />
        public int SaveRelationship(Relationship relationship, int createUpdateUserId)
        {
			Logger.LogError(">>> Entering SaveRelationship(relationship={Relationship},createUpdateUserId={CreateUpdateUserId})", relationship, createUpdateUserId);
			Logger.LogWarning("<<< Exiting SaveRelationship(relationship={Relationship},createUpdateUserId={CreateUpdateUserId})", relationship, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveRelationship", relationship.RelationshipId, relationship.RelationshipTypeId, relationship.Name, relationship.Description, this.provider.GetNull(relationship.UserId), this.provider.GetNull(relationship.PortalId), relationship.DefaultResponse, createUpdateUserId);
        }

        /// <inheritdoc />
        public void DeleteUserRelationship(int userRelationshipId)
        {
			Logger.LogError(">>> Entering DeleteUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
            this.provider.ExecuteNonQuery("DeleteUserRelationship", userRelationshipId);
			Logger.LogWarning("<<< Exiting DeleteUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationship(int userRelationshipId)
        {
			Logger.LogError(">>> Entering GetUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
            Logger.LogWarning("<<< Exiting GetUserRelationship(userRelationshipId={UserRelationshipId})", userRelationshipId);
			Logger.LogWarning("<<< Exiting GetUserRelationship(userId={UserId},relatedUserId={RelatedUserId},relationshipId={RelationshipId},relationshipDirection={RelationshipDirection})", userId, relatedUserId, relationshipId, relationshipDirection);
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
			Logger.LogError(">>> Entering GetUserRelationships(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetUserRelationships(userId={UserId})", userId);
            return this.provider.ExecuteReader("GetUserRelationships", userId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationshipsByRelationshipId(int relationshipId)
        {
			Logger.LogError(">>> Entering GetUserRelationshipsByRelationshipId(relationshipId={RelationshipId})", relationshipId);
            Logger.LogWarning("<<< Exiting GetUserRelationshipsByRelationshipId(relationshipId={RelationshipId})", relationshipId);
            return this.provider.ExecuteReader("GetUserRelationshipsByRelationshipID", relationshipId);
        }

        /// <inheritdoc />
        public int SaveUserRelationship(UserRelationship userRelationship, int createUpdateUserId)
        {
			Logger.LogError(">>> Entering SaveUserRelationship(userRelationship={UserRelationship},createUpdateUserId={CreateUpdateUserId})", userRelationship, createUpdateUserId);
            Logger.LogWarning("<<< Exiting SaveUserRelationship(userRelationship={UserRelationship},createUpdateUserId={CreateUpdateUserId})", userRelationship, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveUserRelationship", userRelationship.UserRelationshipId, userRelationship.UserId, userRelationship.RelatedUserId, userRelationship.RelationshipId, userRelationship.Status, createUpdateUserId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationshipPreferenceById(int preferenceId)
        {
			Logger.LogError(">>> Entering GetUserRelationshipPreferenceById(preferenceId={PreferenceId})", preferenceId);
            Logger.LogWarning("<<< Exiting GetUserRelationshipPreferenceById(preferenceId={PreferenceId})", preferenceId);
            return this.provider.ExecuteReader("GetUserRelationshipPreferenceByID", preferenceId);
        }

        /// <inheritdoc />
        public IDataReader GetUserRelationshipPreference(int userId, int relationshipId)
        {
			Logger.LogError(">>> Entering GetUserRelationshipPreference(userId={UserId},relationshipId={RelationshipId})", userId, relationshipId);
            Logger.LogWarning("<<< Exiting GetUserRelationshipPreference(userId={UserId},relationshipId={RelationshipId})", userId, relationshipId);
            return this.provider.ExecuteReader("GetUserRelationshipPreference", userId, relationshipId);
        }

        /// <inheritdoc />
        public void DeleteUserRelationshipPreference(int preferenceId)
        {
			Logger.LogError(">>> Entering DeleteUserRelationshipPreference(preferenceId={PreferenceId})", preferenceId);
            this.provider.ExecuteNonQuery("DeleteUserRelationshipPreference", preferenceId);
			Logger.LogWarning("<<< Exiting DeleteUserRelationshipPreference(preferenceId={PreferenceId})", preferenceId);
        }

        /// <inheritdoc />
        public int SaveUserRelationshipPreference(UserRelationshipPreference userRelationshipPreference, int createUpdateUserId)
        {
			Logger.LogError(">>> Entering SaveUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference},createUpdateUserId={CreateUpdateUserId})", userRelationshipPreference, createUpdateUserId);
			Logger.LogWarning("<<< Exiting SaveUserRelationshipPreference(userRelationshipPreference={UserRelationshipPreference},createUpdateUserId={CreateUpdateUserId})", userRelationshipPreference, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("SaveUserRelationshipPreference", userRelationshipPreference.PreferenceId, userRelationshipPreference.UserId, userRelationshipPreference.RelationshipId, userRelationshipPreference.DefaultResponse, createUpdateUserId);
        }
    }
}
