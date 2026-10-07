// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Data;

    using DotNetNuke.Data;

    internal class JournalDataServiceImpl : IJournalDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JournalDataServiceImpl));
        private readonly DataProvider provider = DataProvider.Instance();

        /// <inheritdoc />
        public IDataReader Journal_ListForSummary(int portalId, int moduleId, int currentUserId, int rowIndex, int maxRows)
        {
			Logger.LogError(">>> Entering Journal_ListForSummary(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, rowIndex, maxRows);
            return this.provider.ExecuteReader(
                "Journal_ListForSummary",
                portalId,
                moduleId,
                currentUserId,
                rowIndex,
                maxRows);
        }

        /// <inheritdoc />
        public IDataReader Journal_ListForProfile(int portalId, int moduleId, int currentUserId, int profileId, int rowIndex, int maxRows)
        {
			Logger.LogError(">>> Entering Journal_ListForProfile(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},profileId={ProfileId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, profileId, rowIndex, maxRows);
            return this.provider.ExecuteReader(
                "Journal_ListForProfile",
                portalId,
                moduleId,
                currentUserId,
                profileId,
                rowIndex,
                maxRows);
        }

        /// <inheritdoc />
        public IDataReader Journal_ListForGroup(int portalId, int moduleId, int currentUserId, int groupId, int rowIndex, int maxRows)
        {
			Logger.LogError(">>> Entering Journal_ListForGroup(portalId={PortalId},moduleId={ModuleId},currentUserId={CurrentUserId},groupId={GroupId},rowIndex={RowIndex},maxRows={MaxRows})", portalId, moduleId, currentUserId, groupId, rowIndex, maxRows);
            return this.provider.ExecuteReader(
                "Journal_ListForGroup",
                portalId,
                moduleId,
                currentUserId,
                groupId,
                rowIndex,
                maxRows);
        }

        /// <inheritdoc />
        public void Journal_Delete(int journalId)
        {
			Logger.LogError(">>> Entering Journal_Delete(journalId={JournalId})", journalId);
            this.provider.ExecuteNonQuery("Journal_Delete", journalId);
			Logger.LogWarning("<<< Exiting Journal_Delete(journalId={JournalId})", journalId);
        }

        /// <inheritdoc />
        public void Journal_DeleteByKey(int portalId, string objectKey)
        {
			Logger.LogError(">>> Entering Journal_DeleteByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
            this.provider.ExecuteNonQuery("Journal_DeleteByKey", portalId, objectKey);
			Logger.LogWarning("<<< Exiting Journal_DeleteByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        }

        /// <inheritdoc />
        public void Journal_DeleteByGroupId(int portalId, int groupId)
        {
			Logger.LogError(">>> Entering Journal_DeleteByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
            this.provider.ExecuteNonQuery("Journal_DeleteByGroupId", portalId, groupId);
			Logger.LogWarning("<<< Exiting Journal_DeleteByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
        }

        /// <inheritdoc />
        public void Journal_SoftDelete(int journalId)
        {
			Logger.LogError(">>> Entering Journal_SoftDelete(journalId={JournalId})", journalId);
            this.provider.ExecuteNonQuery("Journal_Delete", journalId, true);
			Logger.LogWarning("<<< Exiting Journal_SoftDelete(journalId={JournalId})", journalId);
        }

        /// <inheritdoc />
        public void Journal_SoftDeleteByKey(int portalId, string objectKey)
        {
			Logger.LogError(">>> Entering Journal_SoftDeleteByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
            this.provider.ExecuteNonQuery("Journal_DeleteByKey", portalId, objectKey, true);
			Logger.LogWarning("<<< Exiting Journal_SoftDeleteByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        }

        /// <inheritdoc />
        public void Journal_SoftDeleteByGroupId(int portalId, int groupId)
        {
			Logger.LogError(">>> Entering Journal_SoftDeleteByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
            this.provider.ExecuteNonQuery("Journal_DeleteByGroupId", portalId, groupId, true);
			Logger.LogWarning("<<< Exiting Journal_SoftDeleteByGroupId(portalId={PortalId},groupId={GroupId})", portalId, groupId);
        }

        /// <inheritdoc />
        public void Journal_Like(int journalId, int userId, string displayName)
        {
			Logger.LogError(">>> Entering Journal_Like(journalId={JournalId},userId={UserId},displayName={DisplayName})", journalId, userId, displayName);
            this.provider.ExecuteNonQuery("Journal_Like", journalId, userId, displayName);
			Logger.LogWarning("<<< Exiting Journal_Like(journalId={JournalId},userId={UserId},displayName={DisplayName})", journalId, userId, displayName);
        }

        /// <inheritdoc />
        public IDataReader Journal_LikeList(int portalId, int journalId)
        {
			Logger.LogError(">>> Entering Journal_LikeList(portalId={PortalId},journalId={JournalId})", portalId, journalId);
            Logger.LogWarning("<<< Exiting Journal_LikeList(portalId={PortalId},journalId={JournalId})", portalId, journalId);
            return this.provider.ExecuteReader("Journal_LikeList", portalId, journalId);
        }

        /// <inheritdoc />
        public void Journal_UpdateContentItemId(int journalId, int contentItemId)
        {
			Logger.LogError(">>> Entering Journal_UpdateContentItemId(journalId={JournalId},contentItemId={ContentItemId})", journalId, contentItemId);
            this.provider.ExecuteNonQuery("Journal_UpdateContentItemId", journalId, contentItemId);
			Logger.LogWarning("<<< Exiting Journal_UpdateContentItemId(journalId={JournalId},contentItemId={ContentItemId})", journalId, contentItemId);
        }

        /// <inheritdoc />
        public IDataReader Journal_Get(int portalId, int currentUserId, int journalId)
        {
			Logger.LogError(">>> Entering Journal_Get(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
            Logger.LogWarning("<<< Exiting Journal_Get(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId})", portalId, currentUserId, journalId);
			Logger.LogWarning("<<< Exiting Journal_Get(portalId={PortalId},currentUserId={CurrentUserId},journalId={JournalId},includeAllItems={IncludeAllItems},isDeleted={IsDeleted},securityCheck={SecurityCheck})", portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck);
            return this.Journal_Get(portalId, currentUserId, journalId, false, false, false);
        }

        /// <inheritdoc />
        public IDataReader Journal_Get(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck)
        {
            return this.provider.ExecuteReader("Journal_Get", portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck);
        }

        /// <inheritdoc />
        public IDataReader Journal_GetByKey(int portalId, string objectKey)
        {
			Logger.LogError(">>> Entering Journal_GetByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
            Logger.LogWarning("<<< Exiting Journal_GetByKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
			Logger.LogWarning("<<< Exiting Journal_GetByKey(portalId={PortalId},objectKey={ObjectKey},includeAllItems={IncludeAllItems},isDeleted={IsDeleted})", portalId, objectKey, includeAllItems, isDeleted);
            return this.Journal_GetByKey(portalId, objectKey, false, false);
        }

        /// <inheritdoc />
        public IDataReader Journal_GetByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted)
        {
            return this.provider.ExecuteReader("Journal_GetByKey", portalId, objectKey, includeAllItems, isDeleted);
        }

        /// <inheritdoc />
        public int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet)
        {
			Logger.LogError(">>> Entering Journal_Save(portalId={PortalId},currentUserId={CurrentUserId},profileId={ProfileId},groupId={GroupId},journalId={JournalId},journalTypeId={JournalTypeId},title={Title},summary={Summary},body={Body},itemData={ItemData},xml={Xml},objectKey={ObjectKey},accessKey=<redacted>,securitySet={SecuritySet})", portalId, currentUserId, profileId, groupId, journalId, journalTypeId, title, summary, body, itemData, xml, objectKey, securitySet);
            journalId = this.provider.ExecuteScalar<int>(
                "Journal_Save",
                portalId,
                journalId,
                journalTypeId,
                currentUserId,
                profileId,
                groupId,
                title,
                summary,
                itemData,
                xml,
                objectKey,
                accessKey,
                securitySet,
                false,
                false);
			Logger.LogWarning("<<< Exiting Journal_Save(portalId={PortalId},currentUserId={CurrentUserId},profileId={ProfileId},groupId={GroupId},journalId={JournalId},journalTypeId={JournalTypeId},title={Title},summary={Summary},body={Body},itemData={ItemData},xml={Xml},objectKey={ObjectKey},accessKey=<redacted>,securitySet={SecuritySet})", portalId, currentUserId, profileId, groupId, journalId, journalTypeId, title, summary, body, itemData, xml, objectKey, securitySet);
            Logger.LogWarning("<<< Exiting Journal_Save(portalId={PortalId},currentUserId={CurrentUserId},profileId={ProfileId},groupId={GroupId},journalId={JournalId},journalTypeId={JournalTypeId},title={Title},summary={Summary},body={Body},itemData={ItemData},xml={Xml},objectKey={ObjectKey},accessKey=<redacted>,securitySet={SecuritySet},commentsDisabled={CommentsDisabled},commentsHidden={CommentsHidden})", portalId, currentUserId, profileId, groupId, journalId, journalTypeId, title, summary, body, itemData, xml, objectKey, securitySet, commentsDisabled, commentsHidden);
            return journalId;
        }

        /// <inheritdoc />
        public int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsDisabled, bool commentsHidden)
        {
            journalId = this.provider.ExecuteScalar<int>(
                "Journal_Save",
                portalId,
                journalId,
                journalTypeId,
                currentUserId,
                profileId,
                groupId,
                title,
                summary,
                itemData,
                xml,
                objectKey,
                accessKey,
                securitySet,
                commentsDisabled,
                commentsHidden);
            return journalId;
        }

        /// <inheritdoc />
        public int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet)
        {
			Logger.LogError(">>> Entering Journal_Update(portalId={PortalId},currentUserId={CurrentUserId},profileId={ProfileId},groupId={GroupId},journalId={JournalId},journalTypeId={JournalTypeId},title={Title},summary={Summary},body={Body},itemData={ItemData},xml={Xml},objectKey={ObjectKey},accessKey=<redacted>,securitySet={SecuritySet})", portalId, currentUserId, profileId, groupId, journalId, journalTypeId, title, summary, body, itemData, xml, objectKey, securitySet);
            journalId = this.provider.ExecuteScalar<int>(
                "Journal_Update",
                portalId,
                journalId,
                journalTypeId,
                currentUserId,
                profileId,
                groupId,
                title,
                summary,
                itemData,
                xml,
                objectKey,
                accessKey,
                securitySet,
                false,
                false);

			Logger.LogWarning("<<< Exiting Journal_Update(portalId={PortalId},currentUserId={CurrentUserId},profileId={ProfileId},groupId={GroupId},journalId={JournalId},journalTypeId={JournalTypeId},title={Title},summary={Summary},body={Body},itemData={ItemData},xml={Xml},objectKey={ObjectKey},accessKey=<redacted>,securitySet={SecuritySet})", portalId, currentUserId, profileId, groupId, journalId, journalTypeId, title, summary, body, itemData, xml, objectKey, securitySet);
            Logger.LogWarning("<<< Exiting Journal_Update(portalId={PortalId},currentUserId={CurrentUserId},profileId={ProfileId},groupId={GroupId},journalId={JournalId},journalTypeId={JournalTypeId},title={Title},summary={Summary},body={Body},itemData={ItemData},xml={Xml},objectKey={ObjectKey},accessKey=<redacted>,securitySet={SecuritySet},commentsDisabled={CommentsDisabled},commentsHidden={CommentsHidden})", portalId, currentUserId, profileId, groupId, journalId, journalTypeId, title, summary, body, itemData, xml, objectKey, securitySet, commentsDisabled, commentsHidden);
            return journalId;
        }

        /// <inheritdoc />
        public int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsDisabled, bool commentsHidden)
        {
            journalId = this.provider.ExecuteScalar<int>(
                "Journal_Update",
                portalId,
                journalId,
                journalTypeId,
                currentUserId,
                profileId,
                groupId,
                title,
                summary,
                itemData,
                xml,
                objectKey,
                accessKey,
                securitySet,
                commentsDisabled,
                commentsHidden);
            return journalId;
        }

        /// <inheritdoc />
        public void Journal_Comment_Delete(int journalId, int commentId)
        {
			Logger.LogError(">>> Entering Journal_Comment_Delete(journalId={JournalId},commentId={CommentId})", journalId, commentId);
            this.provider.ExecuteNonQuery("Journal_Comment_Delete", journalId, commentId);
			Logger.LogWarning("<<< Exiting Journal_Comment_Delete(journalId={JournalId},commentId={CommentId})", journalId, commentId);
        }

        /// <inheritdoc />
        public int Journal_Comment_Save(int journalId, int commentId, int userId, string comment, string xml, DateTime dateUpdated)
        {
			Logger.LogError(">>> Entering Journal_Comment_Save(journalId={JournalId},commentId={CommentId},userId={UserId},comment={Comment},xml={Xml},dateUpdated={DateUpdated})", journalId, commentId, userId, comment, xml, dateUpdated);
            commentId = this.provider.ExecuteScalar<int>("Journal_Comment_Save", journalId, commentId, userId, comment, xml, DataProvider.Instance().GetNull(dateUpdated));
			Logger.LogError("Journal_Comment_Save(journalId={JournalId},commentId={CommentId},userId={UserId},comment={Comment},xml={Xml},dateUpdated={DateUpdated}): commentId → {CommentId1}", journalId, commentId, userId, comment, xml, dateUpdated, commentId);
			Logger.LogWarning("<<< Exiting Journal_Comment_Save(journalId={JournalId},commentId={CommentId},userId={UserId},comment={Comment},xml={Xml},dateUpdated={DateUpdated})", journalId, commentId, userId, comment, xml, dateUpdated);
            return commentId;
        }

        /// <inheritdoc />
        public IDataReader Journal_Comment_List(int journalId)
        {
			Logger.LogError(">>> Entering Journal_Comment_List(journalId={JournalId})", journalId);
            Logger.LogWarning("<<< Exiting Journal_Comment_List(journalId={JournalId})", journalId);
            return this.provider.ExecuteReader("Journal_Comment_List", journalId);
        }

        /// <inheritdoc />
        public IDataReader Journal_Comment_Get(int commentId)
        {
			Logger.LogError(">>> Entering Journal_Comment_Get(commentId={CommentId})", commentId);
            Logger.LogWarning("<<< Exiting Journal_Comment_Get(commentId={CommentId})", commentId);
            return this.provider.ExecuteReader("Journal_Comment_Get", commentId);
        }

        /// <inheritdoc />
        public IDataReader Journal_Comment_ListByJournalIds(string journalIds)
        {
			Logger.LogError(">>> Entering Journal_Comment_ListByJournalIds(journalIds={JournalIds})", journalIds);
            Logger.LogWarning("<<< Exiting Journal_Comment_ListByJournalIds(journalIds={JournalIds})", journalIds);
            return this.provider.ExecuteReader("Journal_Comment_ListByJournalIds", journalIds);
        }

        /// <inheritdoc />
        public void Journal_Comment_Like(int journalId, int commentId, int userId, string displayName)
        {
			Logger.LogError(">>> Entering Journal_Comment_Like(journalId={JournalId},commentId={CommentId},userId={UserId},displayName={DisplayName})", journalId, commentId, userId, displayName);
            this.provider.ExecuteNonQuery("Journal_Comment_Like", journalId, commentId, userId, displayName);
			Logger.LogWarning("<<< Exiting Journal_Comment_Like(journalId={JournalId},commentId={CommentId},userId={UserId},displayName={DisplayName})", journalId, commentId, userId, displayName);
        }

        /// <inheritdoc />
        public IDataReader Journal_Comment_LikeList(int portalId, int journalId, int commentId)
        {
			Logger.LogError(">>> Entering Journal_Comment_LikeList(portalId={PortalId},journalId={JournalId},commentId={CommentId})", portalId, journalId, commentId);
            Logger.LogWarning("<<< Exiting Journal_Comment_LikeList(portalId={PortalId},journalId={JournalId},commentId={CommentId})", portalId, journalId, commentId);
            return this.provider.ExecuteReader("Journal_Comment_LikeList", portalId, journalId, commentId);
        }

        /// <inheritdoc />
        public void Journal_Comments_ToggleDisable(int portalId, int journalId, bool disable)
        {
			Logger.LogError(">>> Entering Journal_Comments_ToggleDisable(portalId={PortalId},journalId={JournalId},disable={Disable})", portalId, journalId, disable);
            this.provider.ExecuteNonQuery("Journal_Comments_ToggleDisable", portalId, journalId, disable);
			Logger.LogWarning("<<< Exiting Journal_Comments_ToggleDisable(portalId={PortalId},journalId={JournalId},disable={Disable})", portalId, journalId, disable);
        }

        /// <inheritdoc />
        public void Journal_Comments_ToggleHidden(int portalId, int journalId, bool hidden)
        {
			Logger.LogError(">>> Entering Journal_Comments_ToggleHidden(portalId={PortalId},journalId={JournalId},hidden={Hidden})", portalId, journalId, hidden);
            this.provider.ExecuteNonQuery("Journal_Comments_ToggleHidden", portalId, journalId, hidden);
			Logger.LogWarning("<<< Exiting Journal_Comments_ToggleHidden(portalId={PortalId},journalId={JournalId},hidden={Hidden})", portalId, journalId, hidden);
        }

        /// <inheritdoc />
        public IDataReader Journal_Types_List(int portalId)
        {
			Logger.LogError(">>> Entering Journal_Types_List(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting Journal_Types_List(portalId={PortalId})", portalId);
            return this.provider.ExecuteReader("Journal_Types_List", portalId);
        }

        /// <inheritdoc />
        public IDataReader Journal_Types_GetById(int journalTypeId)
        {
			Logger.LogError(">>> Entering Journal_Types_GetById(journalTypeId={JournalTypeId})", journalTypeId);
            Logger.LogWarning("<<< Exiting Journal_Types_GetById(journalTypeId={JournalTypeId})", journalTypeId);
            return this.provider.ExecuteReader("Journal_Types_GetById", journalTypeId);
        }

        /// <inheritdoc />
        public IDataReader Journal_Types_Get(string journalType)
        {
			Logger.LogError(">>> Entering Journal_Types_Get(journalType={JournalType})", journalType);
            Logger.LogWarning("<<< Exiting Journal_Types_Get(journalType={JournalType})", journalType);
            return this.provider.ExecuteReader("Journal_Types_Get", journalType);
        }

        /// <inheritdoc />
        public void Journal_Types_Delete(int journalTypeId, int portalId)
        {
			Logger.LogError(">>> Entering Journal_Types_Delete(journalTypeId={JournalTypeId},portalId={PortalId})", journalTypeId, portalId);
            this.provider.ExecuteNonQuery("Journal_Types_Delete", journalTypeId, portalId);
			Logger.LogWarning("<<< Exiting Journal_Types_Delete(journalTypeId={JournalTypeId},portalId={PortalId})", journalTypeId, portalId);
        }

        /// <inheritdoc />
        public int Journal_Types_Save(int journalTypeId, string journalType, string icon, int portalId, bool isEnabled, bool appliesToProfile, bool appliesToGroup, bool appliesToStream, string options, bool supportsNotify)
        {
			Logger.LogError(">>> Entering Journal_Types_Save(journalTypeId={JournalTypeId},journalType={JournalType},icon={Icon},portalId={PortalId},isEnabled={IsEnabled},appliesToProfile={AppliesToProfile},appliesToGroup={AppliesToGroup},appliesToStream={AppliesToStream},options={Options},supportsNotify={SupportsNotify})", journalTypeId, journalType, icon, portalId, isEnabled, appliesToProfile, appliesToGroup, appliesToStream, options, supportsNotify);
            journalTypeId = this.provider.ExecuteScalar<int>(
                "Journal_Types_Save",
                journalTypeId,
                journalType,
                icon,
                portalId,
                isEnabled,
                appliesToProfile,
                appliesToGroup,
                appliesToStream,
                options,
                supportsNotify);
			Logger.LogWarning("<<< Exiting Journal_Types_Save(journalTypeId={JournalTypeId},journalType={JournalType},icon={Icon},portalId={PortalId},isEnabled={IsEnabled},appliesToProfile={AppliesToProfile},appliesToGroup={AppliesToGroup},appliesToStream={AppliesToStream},options={Options},supportsNotify={SupportsNotify})", journalTypeId, journalType, icon, portalId, isEnabled, appliesToProfile, appliesToGroup, appliesToStream, options, supportsNotify);
            return journalTypeId;
        }

        /// <inheritdoc />
        public IDataReader Journal_GetStatsForGroup(int portalId, int groupId)
        {
			Logger.LogError(">>> Entering Journal_GetStatsForGroup(portalId={PortalId},groupId={GroupId})", portalId, groupId);
            Logger.LogWarning("<<< Exiting Journal_GetStatsForGroup(portalId={PortalId},groupId={GroupId})", portalId, groupId);
            return this.provider.ExecuteReader("Journal_GetStatsForGroup", portalId, groupId);
        }

        /// <inheritdoc />
        public IDataReader Journal_TypeFilters_List(int portalId, int moduleId)
        {
			Logger.LogError(">>> Entering Journal_TypeFilters_List(portalId={PortalId},moduleId={ModuleId})", portalId, moduleId);
            Logger.LogWarning("<<< Exiting Journal_TypeFilters_List(portalId={PortalId},moduleId={ModuleId})", portalId, moduleId);
            return this.provider.ExecuteReader("Journal_TypeFilters_List", portalId, moduleId);
        }

        /// <inheritdoc />
        public void Journal_TypeFilters_Delete(int portalId, int moduleId)
        {
			Logger.LogError(">>> Entering Journal_TypeFilters_Delete(portalId={PortalId},moduleId={ModuleId})", portalId, moduleId);
            this.provider.ExecuteNonQuery("Journal_TypeFilters_Delete", portalId, moduleId);
			Logger.LogWarning("<<< Exiting Journal_TypeFilters_Delete(portalId={PortalId},moduleId={ModuleId})", portalId, moduleId);
        }

        /// <inheritdoc />
        public void Journal_TypeFilters_Save(int portalId, int moduleId, int journalTypeId)
        {
			Logger.LogError(">>> Entering Journal_TypeFilters_Save(portalId={PortalId},moduleId={ModuleId},journalTypeId={JournalTypeId})", portalId, moduleId, journalTypeId);
            this.provider.ExecuteNonQuery("Journal_TypeFilters_Save", portalId, moduleId, journalTypeId);
			Logger.LogWarning("<<< Exiting Journal_TypeFilters_Save(portalId={PortalId},moduleId={ModuleId},journalTypeId={JournalTypeId})", portalId, moduleId, journalTypeId);
        }
    }
}
