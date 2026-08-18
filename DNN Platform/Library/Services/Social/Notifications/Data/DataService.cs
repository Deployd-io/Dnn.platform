// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications.Data
{
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;

    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        private const string Prefix = "CoreMessaging_";
        private readonly DataProvider provider = DataProvider.Instance();

        /// <inheritdoc />
        public int CreateNotificationType(string name, string description, int timeToLive, int desktopModuleId, int createUpdateUserId, bool isTask)
        {
			Logger.LogDebug(">>> Entering CreateNotificationType(name={Name},description={Description},timeToLive={TimeToLive},desktopModuleId={DesktopModuleId},createUpdateUserId={CreateUpdateUserId},isTask={IsTask})", name, description, timeToLive, desktopModuleId, createUpdateUserId, isTask);
			Logger.LogDebug("<<< Exiting CreateNotificationType(name={Name},description={Description},timeToLive={TimeToLive},desktopModuleId={DesktopModuleId},createUpdateUserId={CreateUpdateUserId},isTask={IsTask})", name, description, timeToLive, desktopModuleId, createUpdateUserId, isTask);
            return this.provider.ExecuteScalar<int>(GetFullyQualifiedName("CreateNotificationType"), name, this.provider.GetNull(description), this.provider.GetNull(timeToLive), this.provider.GetNull(desktopModuleId), createUpdateUserId, isTask);
        }

        /// <inheritdoc />
        public void DeleteNotificationType(int notificationTypeId)
        {
			Logger.LogDebug(">>> Entering DeleteNotificationType(notificationTypeId={NotificationTypeId})", notificationTypeId);
            this.provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationType"), notificationTypeId);
			Logger.LogDebug("<<< Exiting DeleteNotificationType(notificationTypeId={NotificationTypeId})", notificationTypeId);
        }

        /// <inheritdoc />
        public IDataReader GetNotificationType(int notificationTypeId)
        {
			Logger.LogDebug(">>> Entering GetNotificationType(notificationTypeId={NotificationTypeId})", notificationTypeId);
			Logger.LogDebug("<<< Exiting GetNotificationType(notificationTypeId={NotificationTypeId})", notificationTypeId);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationType"), notificationTypeId);
        }

        /// <inheritdoc />
        public IDataReader GetNotificationTypeByName(string name)
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeByName(name={Name})", name);
			Logger.LogDebug("<<< Exiting GetNotificationTypeByName(name={Name})", name);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeByName"), name);
        }

        /// <inheritdoc />
        public int AddNotificationTypeAction(int notificationTypeId, string nameResourceKey, string descriptionResourceKey, string confirmResourceKey, string apiCall, int createdByUserId)
        {
            return this.provider.ExecuteScalar<int>(GetFullyQualifiedName("AddNotificationTypeAction"), notificationTypeId, nameResourceKey, this.provider.GetNull(descriptionResourceKey), this.provider.GetNull(confirmResourceKey), apiCall, createdByUserId);
        }

        /// <inheritdoc />
        public void DeleteNotificationTypeAction(int notificationTypeActionId)
        {
			Logger.LogDebug(">>> Entering DeleteNotificationTypeAction(notificationTypeActionId={NotificationTypeActionId})", notificationTypeActionId);
            this.provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationTypeAction"), notificationTypeActionId);
			Logger.LogDebug("<<< Exiting DeleteNotificationTypeAction(notificationTypeActionId={NotificationTypeActionId})", notificationTypeActionId);
        }

        /// <inheritdoc />
        public IDataReader GetNotificationTypeAction(int notificationTypeActionId)
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeAction(notificationTypeActionId={NotificationTypeActionId})", notificationTypeActionId);
			Logger.LogDebug("<<< Exiting GetNotificationTypeAction(notificationTypeActionId={NotificationTypeActionId})", notificationTypeActionId);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeAction"), notificationTypeActionId);
        }

        /// <inheritdoc />
        public IDataReader GetNotificationTypeActionByName(int notificationTypeId, string name)
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeActionByName(notificationTypeId={NotificationTypeId},name={Name})", notificationTypeId, name);
			Logger.LogDebug("<<< Exiting GetNotificationTypeActionByName(notificationTypeId={NotificationTypeId},name={Name})", notificationTypeId, name);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActionByName"), notificationTypeId, name);
        }

        /// <inheritdoc />
        public IDataReader GetNotificationTypeActions(int notificationTypeId)
        {
			Logger.LogDebug(">>> Entering GetNotificationTypeActions(notificationTypeId={NotificationTypeId})", notificationTypeId);
			Logger.LogDebug("<<< Exiting GetNotificationTypeActions(notificationTypeId={NotificationTypeId})", notificationTypeId);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActions"), notificationTypeId);
        }

        /// <inheritdoc />
        public int SendNotification(Notification notification, int portalId)
        {
			Logger.LogDebug(">>> Entering SendNotification(notification={Notification},portalId={PortalId})", notification, portalId);
            var createdByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
			Logger.LogDebug("SendNotification(notification={Notification},portalId={PortalId}): createdByUserId → {CreatedByUserId}", notification, portalId, createdByUserId);
			Logger.LogDebug("<<< Exiting SendNotification(notification={Notification},portalId={PortalId})", notification, portalId);
            return this.provider.ExecuteScalar<int>(
                GetFullyQualifiedName("SendNotification"),
                notification.NotificationTypeID,
                portalId,
                notification.To,
                notification.From,
                notification.Subject,
                notification.Body,
                notification.SenderUserID,
                createdByUserId,
                this.provider.GetNull(notification.ExpirationDate),
                notification.IncludeDismissAction,
                notification.Context);
        }

        /// <inheritdoc />
        public void DeleteNotification(int notificationId)
        {
			Logger.LogDebug(">>> Entering DeleteNotification(notificationId={NotificationId})", notificationId);
            this.provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotification"), notificationId);
			Logger.LogDebug("<<< Exiting DeleteNotification(notificationId={NotificationId})", notificationId);
        }

        /// <inheritdoc />
        public int DeleteUserNotifications(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering DeleteUserNotifications(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting DeleteUserNotifications(userId={UserId},portalId={PortalId})", userId, portalId);
            return userId <= 0 ? 0 : this.provider.ExecuteScalar<int>(GetFullyQualifiedName("DeleteUserNotifications"), userId, portalId);
        }

        /// <inheritdoc />
        public int CountNotifications(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountNotifications(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting CountNotifications(userId={UserId},portalId={PortalId})", userId, portalId);
            return userId <= 0 ? 0 : this.provider.ExecuteScalar<int>(GetFullyQualifiedName("CountNotifications"), userId, portalId);
        }

        /// <inheritdoc />
        public IDataReader GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering GetNotifications(userId={UserId},portalId={PortalId},afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords})", userId, portalId, afterNotificationId, numberOfRecords);
			Logger.LogDebug("<<< Exiting GetNotifications(userId={UserId},portalId={PortalId},afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords})", userId, portalId, afterNotificationId, numberOfRecords);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotifications"), userId, portalId, afterNotificationId, numberOfRecords);
        }

        /// <inheritdoc />
        public IDataReader GetNotification(int notificationId)
        {
			Logger.LogDebug(">>> Entering GetNotification(notificationId={NotificationId})", notificationId);
			Logger.LogDebug("<<< Exiting GetNotification(notificationId={NotificationId})", notificationId);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotification"), notificationId);
        }

        /// <inheritdoc />
        public IDataReader GetNotificationByContext(int notificationTypeId, string context)
        {
			Logger.LogDebug(">>> Entering GetNotificationByContext(notificationTypeId={NotificationTypeId},context={Context})", notificationTypeId, context);
			Logger.LogDebug("<<< Exiting GetNotificationByContext(notificationTypeId={NotificationTypeId},context={Context})", notificationTypeId, context);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationByContext"), notificationTypeId, context);
        }

        /// <inheritdoc />
        public bool IsToastPending(int notificationId)
        {
			Logger.LogDebug(">>> Entering IsToastPending(notificationId={NotificationId})", notificationId);
			Logger.LogDebug("<<< Exiting IsToastPending(notificationId={NotificationId})", notificationId);
            return this.provider.ExecuteScalar<bool>(
                GetFullyQualifiedName("IsToastPending"),
                notificationId);
        }

        /// <summary>Mark a Toast ready for sending.</summary>
        /// <param name="notificationId">The notification Id. </param>
        /// <param name="userId">The Recipient User Id. </param>
        public void MarkReadyForToast(int notificationId, int userId)
        {
			Logger.LogDebug(">>> Entering MarkReadyForToast(notificationId={NotificationId},userId={UserId})", notificationId, userId);
            this.provider.ExecuteNonQuery(GetFullyQualifiedName("MarkReadyForToast"), notificationId, userId);
			Logger.LogDebug("<<< Exiting MarkReadyForToast(notificationId={NotificationId},userId={UserId})", notificationId, userId);
        }

        /// <summary>Mark Toast being already sent.</summary>
        /// <param name="notificationId">The notification Id. </param>
        /// <param name="userId">The Recipient User Id. </param>
        public void MarkToastSent(int notificationId, int userId)
        {
			Logger.LogDebug(">>> Entering MarkToastSent(notificationId={NotificationId},userId={UserId})", notificationId, userId);
            this.provider.ExecuteNonQuery(GetFullyQualifiedName("MarkToastSent"), notificationId, userId);
			Logger.LogDebug("<<< Exiting MarkToastSent(notificationId={NotificationId},userId={UserId})", notificationId, userId);
        }

        /// <inheritdoc />
        public IDataReader GetToasts(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetToasts(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogDebug("<<< Exiting GetToasts(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.provider.ExecuteReader(GetFullyQualifiedName("GetToasts"), userId, portalId);
        }

        private static string GetFullyQualifiedName(string procedureName)
        {
			Logger.LogDebug(">>> Entering GetFullyQualifiedName(procedureName={ProcedureName})", procedureName);
			Logger.LogDebug("<<< Exiting GetFullyQualifiedName(procedureName={ProcedureName})", procedureName);
            return Prefix + procedureName;
        }
    }
}
