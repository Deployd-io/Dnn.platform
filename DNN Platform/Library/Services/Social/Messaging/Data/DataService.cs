// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Social.Messaging.Internal.Views;

    /// <summary>Data Service component for core messaging functions.</summary>
    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        /// <summary>The provider instance.</summary>
        private readonly DataProvider provider = DataProvider.Instance();

        /// <summary>Saves the message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The id of the newly created message.</returns>
        public int SaveMessage(Message message, int portalId, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveMessage(message={Message},portalId={PortalId},createUpdateUserId={CreateUpdateUserId})", message, portalId, createUpdateUserId);
            // need to fix groupmail
			Logger.LogDebug("<<< Exiting SaveMessage(message={Message},portalId={PortalId},createUpdateUserId={CreateUpdateUserId})", message, portalId, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_SaveMessage", message.MessageID, portalId, message.To, message.From, message.Subject, message.Body, message.ConversationId, message.ReplyAllAllowed, message.SenderUserID, createUpdateUserId);
        }

        /// <summary>Gets the message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A <see cref="IDataReader"/> containing the message data.</returns>
        public IDataReader GetMessage(int messageId)
        {
			Logger.LogDebug(">>> Entering GetMessage(messageId={MessageId})", messageId);
			Logger.LogDebug("<<< Exiting GetMessage(messageId={MessageId})", messageId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessage", messageId);
        }

        /// <summary>Gets the last sent message.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>A <see cref="IDataReader"/> containing the last sent message data.</returns>
        public IDataReader GetLastSentMessage(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetLastSentMessage(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting GetLastSentMessage(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteReader("CoreMessaging_GetLastSentMessage", userId, portalId);
        }

        /// <summary>Gets the messages by sender.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>A <see cref="IDataReader"/> containing the messages for a given portal Id.</returns>
        public IDataReader GetMessagesBySender(int messageId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetMessagesBySender(messageId={MessageId},portalId={PortalId})", messageId, portalId);
			Logger.LogDebug("<<< Exiting GetMessagesBySender(messageId={MessageId},portalId={PortalId})", messageId, portalId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessagesBySender", messageId, portalId);
        }

        /// <summary>Deletes the message.</summary>
        /// <param name="messageId">The message identifier.</param>
        public void DeleteMessage(int messageId)
        {
			Logger.LogDebug(">>> Entering DeleteMessage(messageId={MessageId})", messageId);
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteMessage", messageId);
			Logger.LogDebug("<<< Exiting DeleteMessage(messageId={MessageId})", messageId);
        }

        /// <summary>Deletes the user from conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteUserFromConversation(int conversationId, int userId)
        {
			Logger.LogDebug(">>> Entering DeleteUserFromConversation(conversationId={ConversationId},userId={UserId})", conversationId, userId);
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteUserFromConversation", conversationId, userId);
			Logger.LogDebug("<<< Exiting DeleteUserFromConversation(conversationId={ConversationId},userId={UserId})", conversationId, userId);
        }

        /// <summary>Creates a message reply.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="body">The body.</param>
        /// <param name="senderUserId">The sender user identifier.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The id of the reply created.</returns>
        public int CreateMessageReply(int conversationId, int portalId, string body, int senderUserId, string fromName, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering CreateMessageReply(conversationId={ConversationId},portalId={PortalId},body={Body},senderUserId={SenderUserId},fromName={FromName},createUpdateUserId={CreateUpdateUserId})", conversationId, portalId, body, senderUserId, fromName, createUpdateUserId);
			Logger.LogDebug("<<< Exiting CreateMessageReply(conversationId={ConversationId},portalId={PortalId},body={Body},senderUserId={SenderUserId},fromName={FromName},createUpdateUserId={CreateUpdateUserId})", conversationId, portalId, body, senderUserId, fromName, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CreateMessageReply", conversationId, portalId, body, senderUserId, fromName, createUpdateUserId);
        }

        /// <summary>check if an attempt to reply to an existing mail has valid users.</summary>
        /// <param name="conversationId">the particular reply within the message.</param>
        /// <param name="userId">the user sending the message - as they are a recipient they must be excluded from the count.</param>
        /// <returns>The count of recipients.</returns>
        public int CheckReplyHasRecipients(int conversationId, int userId)
        {
			Logger.LogDebug(">>> Entering CheckReplyHasRecipients(conversationId={ConversationId},userId={UserId})", conversationId, userId);
			Logger.LogDebug("<<< Exiting CheckReplyHasRecipients(conversationId={ConversationId},userId={UserId})", conversationId, userId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CheckReplyHasRecipients", conversationId, userId);
        }

        /// <summary>Gets the in box view.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <see langword="true"/> [sort ascending].</param>
        /// <param name="readStatus">The read status.</param>
        /// <param name="archivedStatus">The archived status.</param>
        /// <param name="sentStatus">The sent status.</param>
        /// <returns>A <see cref="IDataReader"/> containing the inbox data.</returns>
        public IDataReader GetInBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus, MessageSentStatus sentStatus)
        {
			Logger.LogDebug(">>> Entering GetInBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending},readStatus={ReadStatus},archivedStatus={ArchivedStatus},sentStatus={SentStatus})", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, sentStatus);
            object read = null;
            object archived = null;
			Logger.LogDebug("GetInBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending},readStatus={ReadStatus},archivedStatus={ArchivedStatus},sentStatus={SentStatus}): archived → {Archived}", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, sentStatus, archived);
            object sent = null;
			Logger.LogDebug("GetInBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending},readStatus={ReadStatus},archivedStatus={ArchivedStatus},sentStatus={SentStatus}): sent → {Sent}", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, sentStatus, sent);
			Logger.LogDebug("GetInBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending},readStatus={ReadStatus},archivedStatus={ArchivedStatus},sentStatus={SentStatus}): read → {Read}", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, sentStatus, read);

            switch (readStatus)
            {
                case MessageReadStatus.Read:
                    read = true;
                    break;
                case MessageReadStatus.UnRead:
                    read = false;
                    break;
            }

            switch (archivedStatus)
            {
                case MessageArchivedStatus.Archived:
                    archived = true;
                    break;
                case MessageArchivedStatus.UnArchived:
                    archived = false;
                    break;
            }

            switch (sentStatus)
            {
                case MessageSentStatus.Received:
                    sent = false;
                    break;
                case MessageSentStatus.Sent:
                    sent = true;
                    break;
            }

			Logger.LogDebug("<<< Exiting GetInBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending},readStatus={ReadStatus},archivedStatus={ArchivedStatus},sentStatus={SentStatus})", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, sentStatus);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageConversations", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, read, archived, sent);
        }

        /// <summary>Gets the sent box view.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <see langword="true"/> [sort ascending].</param>
        /// <returns>A <see cref="IDataReader"/> containing the sent message box data.</returns>
        public IDataReader GetSentBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
			Logger.LogDebug(">>> Entering GetSentBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending})", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
			Logger.LogDebug("<<< Exiting GetSentBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending})", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
            return this.provider.ExecuteReader("CoreMessaging_GetSentBox", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
        }

        /// <summary>Gets the archive box view.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <see langword="true"/> [sort ascending].</param>
        /// <returns>A <see cref="IDataReader"/> containing the archived messages data.</returns>
        public IDataReader GetArchiveBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
			Logger.LogDebug(">>> Entering GetArchiveBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending})", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
			Logger.LogDebug("<<< Exiting GetArchiveBoxView(userId={UserId},portalId={PortalId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},sortAscending={SortAscending})", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
            return this.provider.ExecuteReader("CoreMessaging_GetArchiveBox", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
        }

        /// <summary>Gets the message thread.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <see langword="true"/> [sort ascending].</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>A <see cref="IDataReader"/> containing the message thread data.</returns>
        public IDataReader GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool @sortAscending, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering GetMessageThread(conversationId={ConversationId},userId={UserId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},totalRecords={TotalRecords})", conversationId, userId, afterMessageId, numberOfRecords, sortColumn, totalRecords);
			Logger.LogDebug("<<< Exiting GetMessageThread(conversationId={ConversationId},userId={UserId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords},sortColumn={SortColumn},totalRecords={TotalRecords})", conversationId, userId, afterMessageId, numberOfRecords, sortColumn, totalRecords);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageThread", conversationId, userId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
        }

        /// <summary>Updates the message read status for a given conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="read">if read is set to <see langword="true"/> otherwise false.</param>
        public void UpdateMessageReadStatus(int conversationId, int userId, bool read)
        {
			Logger.LogDebug(">>> Entering UpdateMessageReadStatus(conversationId={ConversationId},userId={UserId},read={Read})", conversationId, userId, read);
            this.provider.ExecuteNonQuery("CoreMessaging_UpdateMessageReadStatus", conversationId, userId, read);
			Logger.LogDebug("<<< Exiting UpdateMessageReadStatus(conversationId={ConversationId},userId={UserId},read={Read})", conversationId, userId, read);
        }

        /// <summary>Updates the message archived status.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="archived">if set to <see langword="true"/> archived.</param>
        public void UpdateMessageArchivedStatus(int conversationId, int userId, bool archived)
        {
			Logger.LogDebug(">>> Entering UpdateMessageArchivedStatus(conversationId={ConversationId},userId={UserId},archived={Archived})", conversationId, userId, archived);
            this.provider.ExecuteNonQuery("CoreMessaging_UpdateMessageArchivedStatus", conversationId, userId, archived);
			Logger.LogDebug("<<< Exiting UpdateMessageArchivedStatus(conversationId={ConversationId},userId={UserId},archived={Archived})", conversationId, userId, archived);
        }

        /// <summary>Counts the new threads.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of new threads for a given user.</returns>
        public int CountNewThreads(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountNewThreads(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountNewThreads(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountNewThreads", userId, portalId);
        }

        /// <summary>Counts the total conversations.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of new conversations for a given user.</returns>
        public int CountTotalConversations(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountTotalConversations(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountTotalConversations(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountTotalConversations", userId, portalId);
        }

        /// <summary>Counts the messages by conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <returns>The count of new messages for a given conversation.</returns>
        public int CountMessagesByConversation(int conversationId)
        {
			Logger.LogDebug(">>> Entering CountMessagesByConversation(conversationId={ConversationId})", conversationId);
			Logger.LogDebug("<<< Exiting CountMessagesByConversation(conversationId={ConversationId})", conversationId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountMessagesByConversation", conversationId);
        }

        /// <summary>Counts the archived messages by conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <returns>The count of archived messages for a given conversation.</returns>
        public int CountArchivedMessagesByConversation(int conversationId)
        {
			Logger.LogDebug(">>> Entering CountArchivedMessagesByConversation(conversationId={ConversationId})", conversationId);
			Logger.LogDebug("<<< Exiting CountArchivedMessagesByConversation(conversationId={ConversationId})", conversationId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountArchivedMessagesByConversation", conversationId);
        }

        /// <summary>Counts the sent messages.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of messages sent for a given user.</returns>
        public int CountSentMessages(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountSentMessages(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountSentMessages(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountSentMessages", userId, portalId);
        }

        /// <summary>Counts the archived messages.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of archived messages for a given user.</returns>
        public int CountArchivedMessages(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountArchivedMessages(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountArchivedMessages(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountArchivedMessages", userId, portalId);
        }

        /// <summary>Counts the sent conversations.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of sent conversations for a given user.</returns>
        public int CountSentConversations(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountSentConversations(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountSentConversations(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountSentConversations", userId, portalId);
        }

        /// <summary>Counts the archived conversations.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of archived conversations for a given user.</returns>
        public int CountArchivedConversations(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountArchivedConversations(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountArchivedConversations(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_CountArchivedConversations", userId, portalId);
        }

        /// <summary>Saves the message recipient.</summary>
        /// <param name="messageRecipient">The message recipient.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The new message recipient Id.</returns>
        public int SaveMessageRecipient(MessageRecipient messageRecipient, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveMessageRecipient(messageRecipient={MessageRecipient},createUpdateUserId={CreateUpdateUserId})", messageRecipient, createUpdateUserId);
			Logger.LogDebug("<<< Exiting SaveMessageRecipient(messageRecipient={MessageRecipient},createUpdateUserId={CreateUpdateUserId})", messageRecipient, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_SaveMessageRecipient", messageRecipient.RecipientID, messageRecipient.MessageID, messageRecipient.UserID, messageRecipient.Read, messageRecipient.Archived, createUpdateUserId);
        }

        /// <summary>Creates the message recipients for role.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="roleIds">The role ids.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        public void CreateMessageRecipientsForRole(int messageId, string roleIds, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering CreateMessageRecipientsForRole(messageId={MessageId},roleIds={RoleIds},createUpdateUserId={CreateUpdateUserId})", messageId, roleIds, createUpdateUserId);
            this.provider.ExecuteNonQuery("CoreMessaging_CreateMessageRecipientsForRole", messageId, roleIds, createUpdateUserId);
			Logger.LogDebug("<<< Exiting CreateMessageRecipientsForRole(messageId={MessageId},roleIds={RoleIds},createUpdateUserId={CreateUpdateUserId})", messageId, roleIds, createUpdateUserId);
        }

        /// <summary>Gets the message recipient.</summary>
        /// <param name="messageRecipientId">The message recipient identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipient(int messageRecipientId)
        {
			Logger.LogDebug(">>> Entering GetMessageRecipient(messageRecipientId={MessageRecipientId})", messageRecipientId);
			Logger.LogDebug("<<< Exiting GetMessageRecipient(messageRecipientId={MessageRecipientId})", messageRecipientId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageRecipient", messageRecipientId);
        }

        /// <summary>Gets the message recipients by user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipientsByUser(int userId)
        {
			Logger.LogDebug(">>> Entering GetMessageRecipientsByUser(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetMessageRecipientsByUser(userId={UserId})", userId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageRecipientsByUser", userId);
        }

        /// <summary>Gets the message recipients by message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipientsByMessage(int messageId)
        {
			Logger.LogDebug(">>> Entering GetMessageRecipientsByMessage(messageId={MessageId})", messageId);
			Logger.LogDebug("<<< Exiting GetMessageRecipientsByMessage(messageId={MessageId})", messageId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageRecipientsByMessage", messageId);
        }

        /// <summary>Gets the message recipient by message and user.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipientByMessageAndUser(int messageId, int userId)
        {
			Logger.LogDebug(">>> Entering GetMessageRecipientByMessageAndUser(messageId={MessageId},userId={UserId})", messageId, userId);
			Logger.LogDebug("<<< Exiting GetMessageRecipientByMessageAndUser(messageId={MessageId},userId={UserId})", messageId, userId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageRecipientsByMessageAndUser", messageId, userId);
        }

        /// <summary>Deletes the message recipient.</summary>
        /// <param name="messageRecipientId">The message recipient identifier.</param>
        public void DeleteMessageRecipient(int messageRecipientId)
        {
			Logger.LogDebug(">>> Entering DeleteMessageRecipient(messageRecipientId={MessageRecipientId})", messageRecipientId);
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteMessageRecipient", messageRecipientId);
			Logger.LogDebug("<<< Exiting DeleteMessageRecipient(messageRecipientId={MessageRecipientId})", messageRecipientId);
        }

        /// <summary>Deletes the message recipient by message and user.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteMessageRecipientByMessageAndUser(int messageId, int userId)
        {
			Logger.LogDebug(">>> Entering DeleteMessageRecipientByMessageAndUser(messageId={MessageId},userId={UserId})", messageId, userId);
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteMessageRecipientByMessageAndUser", messageId, userId);
			Logger.LogDebug("<<< Exiting DeleteMessageRecipientByMessageAndUser(messageId={MessageId},userId={UserId})", messageId, userId);
        }

        /// <summary>Saves the message attachment.</summary>
        /// <param name="messageAttachment">The message attachment.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The message attachment Id.</returns>
        public int SaveMessageAttachment(MessageAttachment messageAttachment, int createUpdateUserId)
        {
			Logger.LogDebug(">>> Entering SaveMessageAttachment(messageAttachment={MessageAttachment},createUpdateUserId={CreateUpdateUserId})", messageAttachment, createUpdateUserId);
			Logger.LogDebug("<<< Exiting SaveMessageAttachment(messageAttachment={MessageAttachment},createUpdateUserId={CreateUpdateUserId})", messageAttachment, createUpdateUserId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_SaveMessageAttachment", messageAttachment.MessageAttachmentID, messageAttachment.MessageID, messageAttachment.FileID, createUpdateUserId);
        }

        /// <summary>Gets the message attachment.</summary>
        /// <param name="messageAttachmentId">The message attachment identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message attachment data.</returns>
        public IDataReader GetMessageAttachment(int messageAttachmentId)
        {
			Logger.LogDebug(">>> Entering GetMessageAttachment(messageAttachmentId={MessageAttachmentId})", messageAttachmentId);
			Logger.LogDebug("<<< Exiting GetMessageAttachment(messageAttachmentId={MessageAttachmentId})", messageAttachmentId);
            return this.provider.ExecuteReader("CoreMessaging_GetMessageAttachment", messageAttachmentId);
        }

        /// <summary>Gets the message attachments by message id.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A list of <see cref="MessageFileView"/>.</returns>
        public IList<MessageFileView> GetMessageAttachmentsByMessage(int messageId)
        {
			Logger.LogDebug(">>> Entering GetMessageAttachmentsByMessage(messageId={MessageId})", messageId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var attachments = new List<MessageFileView>();
            var dr = this.provider.ExecuteReader("CoreMessaging_GetMessageAttachmentsByMessage", messageId);

			Logger.LogDebug("GetMessageAttachmentsByMessage(messageId={MessageId}): attachments → {Attachments}", messageId, attachments);
			Logger.LogDebug("GetMessageAttachmentsByMessage(messageId={MessageId}): dr → {Dr}", messageId, dr);
			Logger.LogInformation("GetMessageAttachmentsByMessage(messageId)={MessageId}: database query executed in {Arg1} ms", messageId, __logStart.ElapsedMilliseconds);
            try
            {
                while (dr.Read())
                {
                    var fileId = Convert.ToInt32(dr["FileID"], CultureInfo.InvariantCulture);
					Logger.LogDebug("GetMessageAttachmentsByMessage(messageId={MessageId}): fileId → {FileId}", messageId, fileId);
                    var file = FileManager.Instance.GetFile(fileId);
					Logger.LogDebug("GetMessageAttachmentsByMessage(messageId={MessageId}): file → {File}", messageId, file);

                    if (file == null)
                    {
                        continue;
                    }

                    var attachment = new MessageFileView
                    {
                        Name = file.FileName,
                        Size = file.Size.ToString(CultureInfo.InvariantCulture),
                        Url = FileManager.Instance.GetUrl(file),
                        FileId = fileId,
                    };

                    attachments.Add(attachment);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogDebug("<<< Exiting GetMessageAttachmentsByMessage(messageId={MessageId})", messageId);
            return attachments;
        }

        /// <summary>Deletes the message attachment.</summary>
        /// <param name="messageAttachmentId">The message attachment identifier.</param>
        public void DeleteMessageAttachment(int messageAttachmentId)
        {
			Logger.LogDebug(">>> Entering DeleteMessageAttachment(messageAttachmentId={MessageAttachmentId})", messageAttachmentId);
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteMessageAttachment", messageAttachmentId);
			Logger.LogDebug("<<< Exiting DeleteMessageAttachment(messageAttachmentId={MessageAttachmentId})", messageAttachmentId);
        }

        /// <summary>Converts the legacy messages.</summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        public void ConvertLegacyMessages(int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering ConvertLegacyMessages(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            this.provider.ExecuteNonQuery("CoreMessaging_ConvertLegacyMessages", pageIndex, pageSize);
			Logger.LogDebug("<<< Exiting ConvertLegacyMessages(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
        }

        /// <summary>Counts the legacy messages.</summary>
        /// <returns>A <see cref="IDataReader" /> containing the messages data.</returns>
        public IDataReader CountLegacyMessages()
        {
			Logger.LogDebug(">>> Entering CountLegacyMessages()");
			Logger.LogDebug("<<< Exiting CountLegacyMessages()");
            return this.provider.ExecuteReader("CoreMessaging_CountLegacyMessages");
        }

        /// <summary>Gets the next messages for instant dispatch.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>A <see cref="IDataReader" /> containing the messages data.</returns>
        public IDataReader GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize)
        {
			Logger.LogDebug(">>> Entering GetNextMessagesForInstantDispatch(schedulerInstance={SchedulerInstance},batchSize={BatchSize})", schedulerInstance, batchSize);
			Logger.LogDebug("<<< Exiting GetNextMessagesForInstantDispatch(schedulerInstance={SchedulerInstance},batchSize={BatchSize})", schedulerInstance, batchSize);
            return this.provider.ExecuteReader("CoreMessaging_GetNextMessagesForInstantDispatch", schedulerInstance, batchSize);
        }

        /// <summary>Gets the next messages for digest dispatch.</summary>
        /// <param name="frequecy">The frequency.</param>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>A <see cref="IDataReader" /> containing the messages data.</returns>
        public IDataReader GetNextMessagesForDigestDispatch(int frequecy, Guid schedulerInstance, int batchSize)
        {
			Logger.LogDebug(">>> Entering GetNextMessagesForDigestDispatch(frequecy={Frequecy},schedulerInstance={SchedulerInstance},batchSize={BatchSize})", frequecy, schedulerInstance, batchSize);
			Logger.LogDebug("<<< Exiting GetNextMessagesForDigestDispatch(frequecy={Frequecy},schedulerInstance={SchedulerInstance},batchSize={BatchSize})", frequecy, schedulerInstance, batchSize);
            return this.provider.ExecuteReader("CoreMessaging_GetNextMessagesForDigestDispatch", frequecy, schedulerInstance, batchSize);
        }

        /// <summary>Marks the message as dispatched.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="recipientId">The recipient identifier.</param>
        public void MarkMessageAsDispatched(int messageId, int recipientId)
        {
			Logger.LogDebug(">>> Entering MarkMessageAsDispatched(messageId={MessageId},recipientId={RecipientId})", messageId, recipientId);
            this.provider.ExecuteNonQuery("CoreMessaging_MarkMessageAsDispatched", messageId, recipientId);
			Logger.LogDebug("<<< Exiting MarkMessageAsDispatched(messageId={MessageId},recipientId={RecipientId})", messageId, recipientId);
        }

        /// <summary>Marks the message as sent.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="recipientId">The recipient identifier.</param>
        public void MarkMessageAsSent(int messageId, int recipientId)
        {
			Logger.LogDebug(">>> Entering MarkMessageAsSent(messageId={MessageId},recipientId={RecipientId})", messageId, recipientId);
            this.provider.ExecuteNonQuery("CoreMessaging_MarkMessageAsSent", messageId, recipientId);
			Logger.LogDebug("<<< Exiting MarkMessageAsSent(messageId={MessageId},recipientId={RecipientId})", messageId, recipientId);
        }

        /// <summary>Gets the user preference.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the user data.</returns>
        public IDataReader GetUserPreference(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering GetUserPreference(portalId={PortalId},userId={UserId})", portalId, userId);
			Logger.LogDebug("<<< Exiting GetUserPreference(portalId={PortalId},userId={UserId})", portalId, userId);
            return this.provider.ExecuteReader("CoreMessaging_GetUserPreference", portalId, userId);
        }

        /// <summary>Sets the user preference.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="messagesEmailFrequency">The messages email frequency.</param>
        /// <param name="notificationsEmailFrequency">The notifications email frequency.</param>
        public void SetUserPreference(int portalId, int userId, int messagesEmailFrequency, int notificationsEmailFrequency)
        {
			Logger.LogDebug(">>> Entering SetUserPreference(portalId={PortalId},userId={UserId},messagesEmailFrequency={MessagesEmailFrequency},notificationsEmailFrequency={NotificationsEmailFrequency})", portalId, userId, messagesEmailFrequency, notificationsEmailFrequency);
            this.provider.ExecuteNonQuery("CoreMessaging_SetUserPreference", portalId, userId, messagesEmailFrequency, notificationsEmailFrequency);
			Logger.LogDebug("<<< Exiting SetUserPreference(portalId={PortalId},userId={UserId},messagesEmailFrequency={MessagesEmailFrequency},notificationsEmailFrequency={NotificationsEmailFrequency})", portalId, userId, messagesEmailFrequency, notificationsEmailFrequency);
        }
    }
}
