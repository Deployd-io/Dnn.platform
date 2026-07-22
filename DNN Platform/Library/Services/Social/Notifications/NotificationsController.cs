// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications.Data;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Provides the methods to work with Notifications, NotificationTypes, NotificationTypeActions and NotificationActions.</summary>
    /// <param name="dataService">Class with methods to do CRUD in database for the entities of types <see cref="NotificationType"></see>, <see cref="NotificationTypeAction"></see> and <see cref="Notification"></see>.</param>
    /// <param name="messagingDataService">Class with methods to do CRUD in database for the entities of types <see cref="Message"></see>, <see cref="MessageRecipient"></see> and <see cref="MessageAttachment"></see> and to interact with the stored procedures regarding messaging.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class NotificationsController(IDataService dataService, Messaging.Data.IDataService messagingDataService, IHostSettings hostSettings)
        : ServiceLocator<INotificationsController, NotificationsController>, INotificationsController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        internal const int ConstMaxSubject = 400;
		Logger.LogDebug(">>> Entering NotificationsController(dataService={DataService},messagingDataService={MessagingDataService},hostSettings={HostSettings})", dataService, messagingDataService, hostSettings);
        internal const int ConstMaxTo = 2000;
        private const string ToastsCacheKey = "GetToasts_{0}";
        private readonly IDataService dataService = dataService ?? DataService.Instance;
		Logger.LogDebug("NotificationsController(dataService={DataService},messagingDataService={MessagingDataService},hostSettings={HostSettings}): dataService → {DataService1}", dataService, messagingDataService, hostSettings, dataService);
        private readonly Messaging.Data.IDataService messagingDataService = messagingDataService ?? Messaging.Data.DataService.Instance;
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("NotificationsController(dataService={DataService},messagingDataService={MessagingDataService},hostSettings={HostSettings}): messagingDataService → {MessagingDataService1}", dataService, messagingDataService, hostSettings, messagingDataService);
		Logger.LogDebug("NotificationsController(dataService={DataService},messagingDataService={MessagingDataService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", dataService, messagingDataService, hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="NotificationsController"/> class.</summary>
		Logger.LogDebug("<<< Exiting NotificationsController(dataService={DataService},messagingDataService={MessagingDataService},hostSettings={HostSettings})", dataService, messagingDataService, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public NotificationsController()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NotificationsController"/> class from a specific data service.</summary>
        /// <param name="dataService">Class with methods to do CRUD in database for the entities of types <see cref="NotificationType"></see>, <see cref="NotificationTypeAction"></see> and <see cref="Notification"></see>.</param>
        /// <param name="messagingDataService">Class with methods to do CRUD in database for the entities of types <see cref="Message"></see>, <see cref="MessageRecipient"></see> and <see cref="MessageAttachment"></see> and to interact with the stored procedures regarding messaging.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public NotificationsController(IDataService dataService, Messaging.Data.IDataService messagingDataService)
            : this(dataService, messagingDataService, null)
        {
        }

        /// <inheritdoc />
        public void SetNotificationTypeActions(IList<NotificationTypeAction> actions, int notificationTypeId)
        {
            Requires.NotNull("actions", actions);

            if (!actions.Any())
            {
                throw new ArgumentException("Actions must contain at least one item.");
            }

            if (actions.Any(x => string.IsNullOrEmpty(x.APICall)))
            {
                throw new ArgumentException("All actions must specify an APICall");
            }

            if (actions.Any(x => string.IsNullOrEmpty(x.NameResourceKey)))
            {
                throw new ArgumentException("All actions must specify a NameResourceKey");
            }

            foreach (var action in actions)
            {
                action.NotificationTypeActionId = this.dataService.AddNotificationTypeAction(
                    notificationTypeId,
                    action.NameResourceKey,
                    action.DescriptionResourceKey,
                    action.ConfirmResourceKey,
                    action.APICall,
                    this.GetCurrentUserId());
                action.NotificationTypeId = notificationTypeId;
            }
        }

        /// <inheritdoc />
        public virtual int CountNotifications(int userId, int portalId)
        {
			Logger.LogDebug(">>> Entering CountNotifications(userId={UserId},portalId={PortalId})", userId, portalId);
            if (userId <= 0)
            {
                return 0;
            }

            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserNotificationsCountCacheKey, portalId, userId);
            var cache = CachingProvider.Instance();
			Logger.LogDebug("CountNotifications(userId={UserId},portalId={PortalId}): cacheKey → {CacheKey}", userId, portalId, cacheKey);
			Logger.LogDebug("CountNotifications(userId={UserId},portalId={PortalId}): cache → {Cache}", userId, portalId, cache);
            var cacheObject = cache.GetItem(cacheKey);
            if (cacheObject is int count)
			Logger.LogDebug("CountNotifications(userId={UserId},portalId={PortalId}): cacheObject → {CacheObject}", userId, portalId, cacheObject);
            {
                return count;
            }

            count = this.dataService.CountNotifications(userId, portalId);
			Logger.LogDebug("CountNotifications(userId={UserId},portalId={PortalId}): count → {Count}", userId, portalId, count);
            cache.Insert(
                cacheKey,
                count,
                (DNNCacheDependency)null,
                DateTime.Now.AddSeconds(DataCache.NotificationsCacheTimeInSec),
                System.Web.Caching.Cache.NoSlidingExpiration);
			Logger.LogDebug("<<< Exiting CountNotifications(userId={UserId},portalId={PortalId})", userId, portalId);
            return count;
        }

        /// <inheritdoc />
        public virtual void SendNotification(Notification notification, int portalId, IList<RoleInfo> roles, IList<UserInfo> users)
        {
			Logger.LogDebug(">>> Entering SendNotification(notification={Notification},portalId={PortalId},roles={Roles},users={Users})", notification, portalId, roles, users);
            Requires.NotNull("notification", notification);

            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(portalId))
			Logger.LogDebug("SendNotification(notification={Notification},portalId={PortalId},roles={Roles},users={Users}): pid → {Pid}", notification, portalId, roles, users, pid);
            {
                pid = PortalController.GetEffectivePortalId(portalId);
            }

            if (notification.SenderUserID < 1)
            {
                notification.SenderUserID = this.GetAdminUser().UserID;
            }

            if (string.IsNullOrEmpty(notification.Subject) && string.IsNullOrEmpty(notification.Body))
            {
                throw new ArgumentException(Localization.GetString("MsgSubjectOrBodyRequiredError", Localization.ExceptionsResourceFile));
            }

            if (roles == null && users == null)
            {
                throw new ArgumentException(Localization.GetString("MsgRolesOrUsersRequiredError", Localization.ExceptionsResourceFile));
            }

            if (!string.IsNullOrEmpty(notification.Subject) && notification.Subject.Length > ConstMaxSubject)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("MsgSubjectTooBigError", Localization.ExceptionsResourceFile), ConstMaxSubject, notification.Subject.Length));
            }

            var sbTo = new StringBuilder();
            if (roles != null)
			Logger.LogDebug("SendNotification(notification={Notification},portalId={PortalId},roles={Roles},users={Users}): sbTo → {SbTo}", notification, portalId, roles, users, sbTo);
            {
                foreach (var role in roles.Where(role => !string.IsNullOrEmpty(role.RoleName)))
                {
                    sbTo.Append(role.RoleName + ",");
                }
            }

            if (users != null)
            {
                foreach (var user in users.Where(user => !string.IsNullOrEmpty(user.DisplayName)))
                {
                    sbTo.Append(user.DisplayName + ",");
                }
            }

            if (sbTo.Length == 0)
            {
                throw new ArgumentException(Localization.GetString("MsgEmptyToListFoundError", Localization.ExceptionsResourceFile));
            }

            if (sbTo.Length > ConstMaxTo)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("MsgToListTooBigError", Localization.ExceptionsResourceFile), ConstMaxTo, sbTo.Length));
            }

            notification.To = sbTo.ToString().Trim(',');
            if (notification.ExpirationDate != default(DateTime))
            {
                notification.ExpirationDate = this.GetExpirationDate(notification.NotificationTypeID);
            }

            notification.NotificationID = this.dataService.SendNotification(notification, pid);

            // send message to Roles
            if (roles != null)
            {
                var roleIds = string.Empty;
                roleIds = roles
                    .Select(r => r.RoleID)
                    .Aggregate(roleIds, (current, roleId) => current + (roleId + ","))
                    .Trim(',');

                this.messagingDataService.CreateMessageRecipientsForRole(
                    notification.NotificationID,
                    roleIds,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // send message to each User - this should be called after CreateMessageRecipientsForRole.
            if (users == null)
            {
                users = new List<UserInfo>();
            }
			Logger.LogDebug("SendNotification(notification={Notification},portalId={PortalId},roles={Roles},users={Users}): users → {Users1}", notification, portalId, roles, users, users);

            var recipients = from user in users
                             where InternalMessagingController.Instance.GetMessageRecipient(notification.NotificationID, user.UserID) == null
                             select new MessageRecipient
                             {
                                 MessageID = notification.NotificationID,
                                 UserID = user.UserID,
                                 Read = false,
                                 RecipientID = Null.NullInteger,
                             };

            foreach (var recipient in recipients)
            {
                this.messagingDataService.SaveMessageRecipient(
                    recipient,
                    UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // if sendToast is true, then mark all recipients' as ready for toast.
            if (notification.SendToast)
            {
                foreach (var messageRecipient in InternalMessagingController.Instance.GetMessageRecipients(notification.NotificationID))
                {
                    this.MarkReadyForToast(notification, messageRecipient.UserID);
                }
            }
			Logger.LogDebug("<<< Exiting SendNotification(notification={Notification},portalId={PortalId},roles={Roles},users={Users})", notification, portalId, roles, users);
        }

        /// <inheritdoc />
        public void CreateNotificationType(NotificationType notificationType)
        {
            Requires.NotNull("notificationType", notificationType);
            Requires.NotNullOrEmpty("notificationType.Name", notificationType.Name);

            if (notificationType.DesktopModuleId <= 0)
            {
                notificationType.DesktopModuleId = Null.NullInteger;
            }

            notificationType.NotificationTypeId = this.dataService.CreateNotificationType(
                notificationType.Name,
                notificationType.Description,
                (int)notificationType.TimeToLive.TotalMinutes == 0 ? Null.NullInteger : (int)notificationType.TimeToLive.TotalMinutes,
                notificationType.DesktopModuleId,
                this.GetCurrentUserId(),
                notificationType.IsTask);
        }

        /// <inheritdoc />
        public virtual void DeleteNotification(int notificationId)
        {
			Logger.LogDebug(">>> Entering DeleteNotification(notificationId={NotificationId})", notificationId);
            var recipients = InternalMessagingController.Instance.GetMessageRecipients(notificationId);
            foreach (var recipient in recipients)
            {
			Logger.LogDebug("DeleteNotification(notificationId={NotificationId}): recipients → {Recipients}", notificationId, recipients);
                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, ToastsCacheKey, recipient.UserID));
            }

            this.dataService.DeleteNotification(notificationId);
			Logger.LogDebug("<<< Exiting DeleteNotification(notificationId={NotificationId})", notificationId);
        }

        /// <inheritdoc />
        public int DeleteUserNotifications(UserInfo user)
        {
			Logger.LogDebug(">>> Entering DeleteUserNotifications(user={User})", user);
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, ToastsCacheKey, user.UserID));
			Logger.LogDebug("<<< Exiting DeleteUserNotifications(user={User})", user);
            return this.dataService.DeleteUserNotifications(user.UserID, user.PortalID);
        }

        /// <inheritdoc />
        public virtual void DeleteNotificationRecipient(int notificationId, int userId)
        {
			Logger.LogDebug(">>> Entering DeleteNotificationRecipient(notificationId={NotificationId},userId={UserId})", notificationId, userId);
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, ToastsCacheKey, userId));
            InternalMessagingController.Instance.DeleteMessageRecipient(notificationId, userId);
            var recipients = InternalMessagingController.Instance.GetMessageRecipients(notificationId);
            if (recipients.Count == 0)
			Logger.LogDebug("DeleteNotificationRecipient(notificationId={NotificationId},userId={UserId}): recipients → {Recipients}", notificationId, userId, recipients);
            {
                this.DeleteNotification(notificationId);
            }
			Logger.LogDebug("<<< Exiting DeleteNotificationRecipient(notificationId={NotificationId},userId={UserId})", notificationId, userId);
        }

        /// <inheritdoc />
        public virtual void DeleteAllNotificationRecipients(int notificationId)
        {
			Logger.LogDebug(">>> Entering DeleteAllNotificationRecipients(notificationId={NotificationId})", notificationId);
            foreach (var recipient in InternalMessagingController.Instance.GetMessageRecipients(notificationId))
            {
                this.DeleteNotificationRecipient(notificationId, recipient.UserID);
            }
			Logger.LogDebug("<<< Exiting DeleteAllNotificationRecipients(notificationId={NotificationId})", notificationId);
        }

        /// <inheritdoc />
        public virtual void DeleteNotificationRecipient(int notificationTypeId, string context, int userId)
        {
            foreach (var notification in this.GetNotificationByContext(notificationTypeId, context))
            {
                this.DeleteNotificationRecipient(notification.NotificationID, userId);
            }
        }

        /// <inheritdoc />
        public Notification GetNotification(int notificationId)
        {
			Logger.LogDebug(">>> Entering GetNotification(notificationId={NotificationId})", notificationId);
			Logger.LogDebug("<<< Exiting GetNotification(notificationId={NotificationId})", notificationId);
            return CBO.FillObject<Notification>(this.dataService.GetNotification(notificationId));
        }

        /// <inheritdoc />
        public virtual IList<Notification> GetNotificationByContext(int notificationTypeId, string context)
        {
            return CBO.FillCollection<Notification>(this.dataService.GetNotificationByContext(notificationTypeId, context));
        }

        /// <inheritdoc />
        public virtual void DeleteNotificationType(int notificationTypeId)
        {
            this.dataService.DeleteNotificationType(notificationTypeId);

            this.RemoveNotificationTypeCache();
        }

        /// <inheritdoc />
        public virtual void DeleteNotificationTypeAction(int notificationTypeActionId)
        {
            this.dataService.DeleteNotificationTypeAction(notificationTypeActionId);

            this.RemoveNotificationTypeActionCache();
        }

        /// <inheritdoc />
        public virtual IList<Notification> GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering GetNotifications(userId={UserId},portalId={PortalId},afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords})", userId, portalId, afterNotificationId, numberOfRecords);
            var pid = portalId;
            if (PortalController.IsMemberOfPortalGroup(portalId))
			Logger.LogDebug("GetNotifications(userId={UserId},portalId={PortalId},afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): pid → {Pid}", userId, portalId, afterNotificationId, numberOfRecords, pid);
            {
                pid = PortalController.GetEffectivePortalId(portalId);
            }

			Logger.LogDebug("<<< Exiting GetNotifications(userId={UserId},portalId={PortalId},afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords})", userId, portalId, afterNotificationId, numberOfRecords);
            return userId <= 0
                ? new List<Notification>(0)
                : CBO.FillCollection<Notification>(this.dataService.GetNotifications(userId, pid, afterNotificationId, numberOfRecords));
        }

        /// <inheritdoc />
        public virtual NotificationType GetNotificationType(int notificationTypeId)
        {
            var notificationTypeCacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.NotificationTypesCacheKey, notificationTypeId);
            var cacheItemArgs = new CacheItemArgs(notificationTypeCacheKey, DataCache.NotificationTypesTimeOut, DataCache.NotificationTypesCachePriority, notificationTypeId);
            return CBO.GetCachedObject<NotificationType>(this.hostSettings, cacheItemArgs, this.GetNotificationTypeCallBack);
        }

        /// <inheritdoc />
        public virtual NotificationType GetNotificationType(string name)
        {
            Requires.NotNullOrEmpty("name", name);

            var notificationTypeCacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.NotificationTypesCacheKey, name);
            var cacheItemArgs = new CacheItemArgs(notificationTypeCacheKey, DataCache.NotificationTypesTimeOut, DataCache.NotificationTypesCachePriority, name);
            return CBO.GetCachedObject<NotificationType>(this.hostSettings, cacheItemArgs, this.GetNotificationTypeByNameCallBack);
        }

        /// <inheritdoc />
        public virtual NotificationTypeAction GetNotificationTypeAction(int notificationTypeActionId)
        {
            var notificationTypeActionCacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.NotificationTypeActionsCacheKey, notificationTypeActionId);
            var cacheItemArgs = new CacheItemArgs(notificationTypeActionCacheKey, DataCache.NotificationTypeActionsTimeOut, DataCache.NotificationTypeActionsPriority, notificationTypeActionId);
            return CBO.GetCachedObject<NotificationTypeAction>(this.hostSettings, cacheItemArgs, this.GetNotificationTypeActionCallBack);
        }

        /// <inheritdoc />
        public virtual NotificationTypeAction GetNotificationTypeAction(int notificationTypeId, string name)
        {
            Requires.NotNullOrEmpty("name", name);

            var notificationTypeActionCacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.NotificationTypeActionsByNameCacheKey, notificationTypeId, name);
            var cacheItemArgs = new CacheItemArgs(notificationTypeActionCacheKey, DataCache.NotificationTypeActionsTimeOut, DataCache.NotificationTypeActionsPriority, notificationTypeId, name);
            return CBO.GetCachedObject<NotificationTypeAction>(this.hostSettings, cacheItemArgs, this.GetNotificationTypeActionByNameCallBack);
        }

        /// <inheritdoc />
        public virtual IList<NotificationTypeAction> GetNotificationTypeActions(int notificationTypeId)
        {
            return CBO.FillCollection<NotificationTypeAction>(this.dataService.GetNotificationTypeActions(notificationTypeId));
        }

        /// <inheritdoc />
        public bool IsToastPending(int notificationId)
        {
			Logger.LogDebug(">>> Entering IsToastPending(notificationId={NotificationId})", notificationId);
			Logger.LogDebug("<<< Exiting IsToastPending(notificationId={NotificationId})", notificationId);
            return this.dataService.IsToastPending(notificationId);
        }

        /// <inheritdoc />
        public void MarkReadyForToast(Notification notification, UserInfo userInfo)
        {
			Logger.LogDebug(">>> Entering MarkReadyForToast(notification={Notification},userInfo={UserInfo})", notification, userInfo);
            this.MarkReadyForToast(notification, userInfo.UserID);
			Logger.LogDebug("<<< Exiting MarkReadyForToast(notification={Notification},userInfo={UserInfo})", notification, userInfo);
        }

        public void MarkReadyForToast(Notification notification, int userId)
        {
			Logger.LogDebug(">>> Entering MarkReadyForToast(notification={Notification},userId={UserId})", notification, userId);
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, ToastsCacheKey, userId));
            this.dataService.MarkReadyForToast(notification.NotificationID, userId);
			Logger.LogDebug("<<< Exiting MarkReadyForToast(notification={Notification},userId={UserId})", notification, userId);
        }

        /// <inheritdoc />
        public void MarkToastSent(int notificationId, int userId)
        {
			Logger.LogDebug(">>> Entering MarkToastSent(notificationId={NotificationId},userId={UserId})", notificationId, userId);
            this.dataService.MarkToastSent(notificationId, userId);
			Logger.LogDebug("<<< Exiting MarkToastSent(notificationId={NotificationId},userId={UserId})", notificationId, userId);
        }

        /// <inheritdoc />
        public IList<Notification> GetToasts(UserInfo userInfo)
        {
			Logger.LogDebug(">>> Entering GetToasts(userInfo={UserInfo})", userInfo);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, ToastsCacheKey, userInfo.UserID);
            var toasts = DataCache.GetCache<IList<Notification>>(cacheKey);
			Logger.LogDebug("GetToasts(userInfo={UserInfo}): cacheKey → {CacheKey}", userInfo, cacheKey);
			Logger.LogDebug("GetToasts(userInfo={UserInfo}): toasts → {Toasts}", userInfo, toasts);

            if (toasts == null)
            {
                toasts = CBO.FillCollection<Notification>(this.dataService.GetToasts(userInfo.UserID, userInfo.PortalID));
                foreach (var message in toasts)
                {
                    this.dataService.MarkToastSent(message.NotificationID, userInfo.UserID);
                }

                // Set the cache to empty toasts object because we don't want to make calls to database everytime for empty objects.
                // This empty object cache would be cleared by MarkReadyForToast emthod when a new notification arrives for the user.
                DataCache.SetCache(cacheKey, new List<Notification>());
            }

			Logger.LogDebug("<<< Exiting GetToasts(userInfo={UserInfo})", userInfo);
            return toasts;
        }

        internal virtual UserInfo GetAdminUser()
        {
			Logger.LogDebug(">>> Entering GetAdminUser()");
            var current = PortalSettings.Current;
			Logger.LogDebug("<<< Exiting GetAdminUser()");
            return current == null
                ? new UserInfo()
                : UserController.GetUserById(current.PortalId, current.AdministratorId);
        }

        internal virtual int GetCurrentUserId()
        {
			Logger.LogDebug(">>> Entering GetCurrentUserId()");
			Logger.LogDebug("<<< Exiting GetCurrentUserId()");
            return UserController.Instance.GetCurrentUserInfo().UserID;
        }

        internal virtual DateTime GetExpirationDate(int notificationTypeId)
        {
            var notificationType = this.GetNotificationType(notificationTypeId);

            return notificationType.TimeToLive.TotalMinutes > 0
                ? DateTime.UtcNow.AddMinutes(notificationType.TimeToLive.TotalMinutes)
                : DateTime.MinValue;
        }

        internal virtual object GetNotificationTypeActionCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeActionId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillObject<NotificationTypeAction>(this.dataService.GetNotificationTypeAction(notificationTypeActionId));
        }

        internal virtual object GetNotificationTypeActionByNameCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeId = (int)cacheItemArgs.ParamList[0];
            var name = cacheItemArgs.ParamList[1].ToString();
            return CBO.FillObject<NotificationTypeAction>(this.dataService.GetNotificationTypeActionByName(notificationTypeId, name));
        }

        internal virtual object GetNotificationTypeByNameCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationName = cacheItemArgs.ParamList[0].ToString();
            return CBO.FillObject<NotificationType>(this.dataService.GetNotificationTypeByName(notificationName));
        }

        internal virtual object GetNotificationTypeCallBack(CacheItemArgs cacheItemArgs)
        {
            var notificationTypeId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillObject<NotificationType>(this.dataService.GetNotificationType(notificationTypeId));
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
			Logger.LogDebug(">>> Entering GetPortalSetting(settingName={SettingName},portalId={PortalId},defaultValue={DefaultValue})", settingName, portalId, defaultValue);
			Logger.LogDebug("<<< Exiting GetPortalSetting(settingName={SettingName},portalId={PortalId},defaultValue={DefaultValue})", settingName, portalId, defaultValue);
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual string InputFilter(string input)
        {
			Logger.LogDebug(">>> Entering InputFilter(input={Input})", input);
            var ps = PortalSecurity.Instance;
			Logger.LogDebug("<<< Exiting InputFilter(input={Input})", input);
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }

        internal virtual void RemoveNotificationTypeActionCache()
        {
			Logger.LogDebug(">>> Entering RemoveNotificationTypeActionCache()");
            DataCache.ClearCache("NotificationTypeActions:");
			Logger.LogDebug("<<< Exiting RemoveNotificationTypeActionCache()");
        }

        internal virtual void RemoveNotificationTypeCache()
        {
			Logger.LogDebug(">>> Entering RemoveNotificationTypeCache()");
            DataCache.ClearCache("NotificationTypes:");
			Logger.LogDebug("<<< Exiting RemoveNotificationTypeCache()");
        }

        /// <inheritdoc />
        protected override Func<INotificationsController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new NotificationsController();
        }
    }
}
