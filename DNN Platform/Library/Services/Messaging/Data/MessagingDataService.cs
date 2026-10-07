// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Data;

    public class MessagingDataService : IMessagingDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MessagingDataService));
        private readonly DataProvider provider = DataProvider.Instance();

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IDataReader GetMessageByID(int messageId)
        {
			Logger.LogError(">>> Entering GetMessageByID(messageId={MessageId})", messageId);
            Logger.LogWarning("<<< Exiting GetMessageByID(messageId={MessageId})", messageId);
            return this.provider.ExecuteReader("Messaging_GetMessage", messageId);
        }

        /// <inheritdoc />
        public IDataReader GetUserInbox(int portalID, int userID, int pageNumber, int pageSize)
        {
			Logger.LogError(">>> Entering GetUserInbox(portalID={PortalID},userID={UserID},pageNumber={PageNumber},pageSize={PageSize})", portalID, userID, pageNumber, pageSize);
            Logger.LogWarning("<<< Exiting GetUserInbox(portalID={PortalID},userID={UserID},pageNumber={PageNumber},pageSize={PageSize})", portalID, userID, pageNumber, pageSize);
            return this.provider.ExecuteReader("Messaging_GetInbox", portalID, userID, pageNumber, pageSize);
        }

        /// <inheritdoc />
        public int GetInboxCount(int portalID, int userID)
        {
			Logger.LogError(">>> Entering GetInboxCount(portalID={PortalID},userID={UserID})", portalID, userID);
            Logger.LogWarning("<<< Exiting GetInboxCount(portalID={PortalID},userID={UserID})", portalID, userID);
            return this.provider.ExecuteScalar<int>("Messaging_GetInboxCount", portalID, userID);
        }

        /// <inheritdoc />
        public long SaveMessage(Message objMessaging)
        {
			Logger.LogError(">>> Entering SaveMessage(objMessaging={ObjMessaging})", objMessaging);
			Logger.LogWarning("<<< Exiting SaveMessage(objMessaging={ObjMessaging})", objMessaging);
            return this.provider.ExecuteScalar<long>(
                "Messaging_Save_Message",
                objMessaging.PortalID,
                objMessaging.FromUserID,
                objMessaging.ToUserID,
                objMessaging.ToRoleID,
                (int)objMessaging.Status,
                objMessaging.Subject,
                objMessaging.Body,
                objMessaging.MessageDate,
                objMessaging.Conversation,
                objMessaging.ReplyTo,
                objMessaging.AllowReply,
                objMessaging.SkipInbox);
        }

        /// <inheritdoc />
        public int GetNewMessageCount(int portalID, int userID)
        {
			Logger.LogError(">>> Entering GetNewMessageCount(portalID={PortalID},userID={UserID})", portalID, userID);
            Logger.LogWarning("<<< Exiting GetNewMessageCount(portalID={PortalID},userID={UserID})", portalID, userID);
            return this.provider.ExecuteScalar<int>("Messaging_GetNewMessageCount", portalID, userID);
        }

        /// <inheritdoc />
        public IDataReader GetNextMessageForDispatch(Guid schedulerInstance)
        {
			Logger.LogError(">>> Entering GetNextMessageForDispatch(schedulerInstance={SchedulerInstance})", schedulerInstance);
            Logger.LogWarning("<<< Exiting GetNextMessageForDispatch(schedulerInstance={SchedulerInstance})", schedulerInstance);
            return this.provider.ExecuteReader("Messaging_GetNextMessageForDispatch", schedulerInstance);
        }

        /// <inheritdoc />
        public void MarkMessageAsDispatched(int messageID)
        {
			Logger.LogError(">>> Entering MarkMessageAsDispatched(messageID={MessageID})", messageID);
            this.provider.ExecuteNonQuery("Messaging_MarkMessageAsDispatched", messageID);
			Logger.LogWarning("<<< Exiting MarkMessageAsDispatched(messageID={MessageID})", messageID);
        }

        /// <inheritdoc />
        public void UpdateMessage(Message message)
        {
			Logger.LogError(">>> Entering UpdateMessage(message={Message})", message);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery(
                "Messaging_UpdateMessage",
                message.MessageID,
                message.ToUserID,
                message.ToRoleID,
                (int)message.Status,
                message.Subject,
                message.Body,
                message.MessageDate,
                message.ReplyTo,
                message.AllowReply,
                message.SkipInbox);
				Logger.LogError("UpdateMessage(message)={Message}: database query executed in {Arg1} ms", message, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateMessage(message={Message})", message);
        }
    }
}
