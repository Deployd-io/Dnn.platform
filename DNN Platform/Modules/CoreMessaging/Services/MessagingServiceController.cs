// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.CoreMessaging.ViewModels;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides messaging web services.</summary>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    [SupportedModules("DotNetNuke.Modules.CoreMessaging")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DnnAuthorize]
    public class MessagingServiceController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IHostSettings hostSettings)
        : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MessagingServiceController));
        private readonly IPortalController portalController = portalController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalGroupController>();
        private readonly IHostSettings hostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="MessagingServiceController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public MessagingServiceController()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering MessagingServiceController()");
        }

        /// <summary>Initializes a new instance of the <see cref="MessagingServiceController"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public MessagingServiceController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
            : this(portalController, appStatus, portalGroupController, null)
        {
		Logger.LogDebug("<<< Exiting MessagingServiceController()");
        }

        /// <summary>Provides access to the user inbox.</summary>
        /// <param name="afterMessageId">After which message id to start returning new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageBoxView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Inbox(int afterMessageId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering Inbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", afterMessageId, numberOfRecords);
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentInbox(this.UserInfo.UserID, afterMessageId, numberOfRecords);
				Logger.LogDebug("Inbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): messageBoxView → {MessageBoxView}", afterMessageId, numberOfRecords, messageBoxView);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("Inbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): portalId → {PortalId}", afterMessageId, numberOfRecords, portalId);

                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while trying to fetch the inbox, consult the server logs for more information.";
				Logger.LogDebug("Inbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): message → {Message}", afterMessageId, numberOfRecords, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting Inbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", afterMessageId, numberOfRecords);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to the sent box view.</summary>
        /// <param name="afterMessageId">After which message to obtain new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageBoxView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Sentbox(int afterMessageId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering Sentbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", afterMessageId, numberOfRecords);
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentSentbox(this.UserInfo.UserID, afterMessageId, numberOfRecords);
				Logger.LogDebug("Sentbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): messageBoxView → {MessageBoxView}", afterMessageId, numberOfRecords, messageBoxView);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("Sentbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): portalId → {PortalId}", afterMessageId, numberOfRecords, portalId);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountSentConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the Sent box, consult the server logs for more information.";
				Logger.LogDebug("Sentbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): message → {Message}", afterMessageId, numberOfRecords, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting Sentbox(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", afterMessageId, numberOfRecords);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to the archived box.</summary>
        /// <param name="afterMessageId">After which message to get new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageBoxView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Archived(int afterMessageId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering Archived(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", afterMessageId, numberOfRecords);
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetArchivedMessages(this.UserInfo.UserID, afterMessageId, numberOfRecords);
				Logger.LogDebug("Archived(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): messageBoxView → {MessageBoxView}", afterMessageId, numberOfRecords, messageBoxView);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("Archived(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): portalId → {PortalId}", afterMessageId, numberOfRecords, portalId);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountArchivedConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the archived box.";
				Logger.LogDebug("Archived(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): message → {Message}", afterMessageId, numberOfRecords, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting Archived(afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", afterMessageId, numberOfRecords);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to a message thread.</summary>
        /// <param name="conversationId">The conversation id to get the thread from.</param>
        /// <param name="afterMessageId">After which message to get new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageThreadsView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Thread(int conversationId, int afterMessageId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering Thread(conversationId={ConversationId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", conversationId, afterMessageId, numberOfRecords);
            try
            {
                var totalRecords = 0;
                var messageThreadsView = InternalMessagingController.Instance.GetMessageThread(conversationId, this.UserInfo.UserID, afterMessageId, numberOfRecords, ref totalRecords);
				Logger.LogDebug("Thread(conversationId={ConversationId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): messageThreadsView → {MessageThreadsView}", conversationId, afterMessageId, numberOfRecords, messageThreadsView);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("Thread(conversationId={ConversationId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): portalId → {PortalId}", conversationId, afterMessageId, numberOfRecords, portalId);
                messageThreadsView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageThreadsView.TotalThreads = InternalMessagingController.Instance.CountMessagesByConversation(conversationId);
                messageThreadsView.TotalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(conversationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageThreadsView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the thread, consult the server logs for more information.";
				Logger.LogDebug("Thread(conversationId={ConversationId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords}): message → {Message}", conversationId, afterMessageId, numberOfRecords, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting Thread(conversationId={ConversationId},afterMessageId={AfterMessageId},numberOfRecords={NumberOfRecords})", conversationId, afterMessageId, numberOfRecords);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to post a reply to a message.</summary>
        /// <param name="postData">The information about the reply, <see cref="ReplyDTO"/>.</param>
        /// <returns>Information about the conversation and message thread.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reply(ReplyDTO postData)
        {
			Logger.LogDebug(">>> Entering Reply(postData={PostData})", postData);
            try
            {
                var body = HttpUtility.UrlDecode(postData.Body);
				Logger.LogDebug("Reply(postData={PostData}): body → {Body}", postData, body);
                body = WebUtility.HtmlEncode(body);
                var messageId = InternalMessagingController.Instance.ReplyMessage(postData.ConversationId, body, postData.FileIds);
				Logger.LogDebug("Reply(postData={PostData}): messageId → {MessageId}", postData, messageId);
                var message = this.ToExpandoObject(InternalMessagingController.Instance.GetMessage(messageId));
				Logger.LogDebug("Reply(postData={PostData}): message → {Message}", postData, message);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("Reply(postData={PostData}): portalId → {PortalId}", postData, portalId);

                var totalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
				Logger.LogDebug("Reply(postData={PostData}): totalNewThreads → {TotalNewThreads}", postData, totalNewThreads);
                var totalThreads = InternalMessagingController.Instance.CountMessagesByConversation(postData.ConversationId);
				Logger.LogDebug("Reply(postData={PostData}): totalThreads → {TotalThreads}", postData, totalThreads);
                var totalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(postData.ConversationId);
				Logger.LogDebug("Reply(postData={PostData}): totalArchivedThreads → {TotalArchivedThreads}", postData, totalArchivedThreads);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Conversation = message, TotalNewThreads = totalNewThreads, TotalThreads = totalThreads, TotalArchivedThreads = totalArchivedThreads });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to reply to a conversation, see the server logs for more information.";
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting Reply(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Marks a conversation as archived.</summary>
        /// <param name="postData">The information about the conversation, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkArchived(ConversationDTO postData)
        {
			Logger.LogDebug(">>> Entering MarkArchived(postData={PostData})", postData);
            try
            {
                InternalMessagingController.Instance.MarkArchived(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the archived box, consult the server logs for more information.";
				Logger.LogDebug("MarkArchived(postData={PostData}): message → {Message}", postData, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting MarkArchived(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Restores a conversation out of the archive.</summary>
        /// <param name="postData">The information about the conversation, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnArchived(ConversationDTO postData)
        {
			Logger.LogDebug(">>> Entering MarkUnArchived(postData={PostData})", postData);
            try
            {
                InternalMessagingController.Instance.MarkUnArchived(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to restore an archived conversation, consult the server logs for more information.";
				Logger.LogDebug("MarkUnArchived(postData={PostData}): message → {Message}", postData, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting MarkUnArchived(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Marks a conversation as read.</summary>
        /// <param name="postData">The information about the conversation to mark as read, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" Result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkRead(ConversationDTO postData)
        {
			Logger.LogDebug(">>> Entering MarkRead(postData={PostData})", postData);
            try
            {
                InternalMessagingController.Instance.MarkRead(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting mark a conversation as read, consult the server logs for more information.";
				Logger.LogDebug("MarkRead(postData={PostData}): message → {Message}", postData, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting MarkRead(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Marks a conversation as unread.</summary>
        /// <param name="postData">The information about the conversation to mark unread, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" Result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnRead(ConversationDTO postData)
        {
			Logger.LogDebug(">>> Entering MarkUnRead(postData={PostData})", postData);
            try
            {
                InternalMessagingController.Instance.MarkUnRead(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to restore an archived conversation, consult the server logs for more information.";
				Logger.LogDebug("MarkUnRead(postData={PostData}): message → {Message}", postData, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting MarkUnRead(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Deletes a user from a conversation.</summary>
        /// <param name="postData">The information about the conversation, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" Result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteUserFromConversation(ConversationDTO postData)
        {
			Logger.LogDebug(">>> Entering DeleteUserFromConversation(postData={PostData})", postData);
            try
            {
                InternalMessagingController.Instance.DeleteUserFromConversation(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                var message = "An unexpected error occurred while attempting to delete a user from a conversation, consult the server logs for more information.";
                Logger.Error(message, ex);
				Logger.LogDebug("DeleteUserFromConversation(postData={PostData}): message → {Message}", postData, message);
				Logger.LogInformation("DeleteUserFromConversation(postData)={PostData}: delete query executed in {Arg1} ms", postData, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting DeleteUserFromConversation(postData={PostData})", postData);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the user notifications.</summary>
        /// <param name="afterNotificationId">After which notification to get new ones.</param>
        /// <param name="numberOfRecords">How many notifications to get.</param>
        /// <returns>A see <see cref="NotificationViewModel"/> object.</returns>
        [HttpGet]
        public HttpResponseMessage Notifications(int afterNotificationId, int numberOfRecords)
        {
			Logger.LogDebug(">>> Entering Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords})", afterNotificationId, numberOfRecords);
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): portalId → {PortalId}", afterNotificationId, numberOfRecords, portalId);
                var notificationsDomainModel = NotificationsController.Instance.GetNotifications(this.UserInfo.UserID, portalId, afterNotificationId, numberOfRecords);
				Logger.LogDebug("Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): notificationsDomainModel → {NotificationsDomainModel}", afterNotificationId, numberOfRecords, notificationsDomainModel);

                var notificationsViewModel = new NotificationsViewModel
                {
                    TotalNotifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId),
                    Notifications = new List<NotificationViewModel>(notificationsDomainModel.Count),
                };

                foreach (var notification in notificationsDomainModel)
                {
                    var user = UserController.Instance.GetUser(this.PortalSettings.PortalId, notification.SenderUserID);
					Logger.LogDebug("Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): user → {User}", afterNotificationId, numberOfRecords, user);
                    var displayName = user != null ? user.DisplayName : string.Empty;

                    var notificationViewModel = new NotificationViewModel
                    {
                        NotificationId = notification.NotificationID,
                        Subject = notification.Subject,
                        From = notification.From,
                        Body = notification.Body,
                        DisplayDate = Common.Utilities.DateUtils.CalculateDateForDisplay(notification.CreatedOnDate),
                        SenderAvatar = UserController.Instance.GetUserProfilePictureUrl(notification.SenderUserID, 64, 64),
                        SenderProfileUrl = Globals.UserProfileURL(notification.SenderUserID),
                        SenderDisplayName = displayName,
                        Actions = new List<NotificationActionViewModel>(),
                    };

                    var notificationType = NotificationsController.Instance.GetNotificationType(notification.NotificationTypeID);
					Logger.LogDebug("Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): notificationType → {NotificationType}", afterNotificationId, numberOfRecords, notificationType);
                    var notificationTypeActions = NotificationsController.Instance.GetNotificationTypeActions(notification.NotificationTypeID);
					Logger.LogDebug("Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): notificationTypeActions → {NotificationTypeActions}", afterNotificationId, numberOfRecords, notificationTypeActions);

                    foreach (var notificationTypeAction in notificationTypeActions)
                    {
                        var notificationActionViewModel = new NotificationActionViewModel
                        {
                            Name = this.LocalizeActionString(notificationTypeAction.NameResourceKey, notificationType.DesktopModuleId),
                            Description = this.LocalizeActionString(notificationTypeAction.DescriptionResourceKey, notificationType.DesktopModuleId),
                            Confirm = this.LocalizeActionString(notificationTypeAction.ConfirmResourceKey, notificationType.DesktopModuleId),
                            APICall = notificationTypeAction.APICall,
                        };

                        notificationViewModel.Actions.Add(notificationActionViewModel);
                    }

                    if (notification.IncludeDismissAction)
                    {
                        notificationViewModel.Actions.Add(new NotificationActionViewModel
                        {
                            Name = Localization.GetString("Dismiss.Text"),
                            Description = Localization.GetString("DismissNotification.Text"),
                            Confirm = string.Empty,
                            APICall = "API/InternalServices/NotificationsService/Dismiss",
                        });
                    }

                    notificationsViewModel.Notifications.Add(notificationViewModel);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, notificationsViewModel);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch messaging notifications, consult the server logs for more information.";
				Logger.LogDebug("Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords}): message → {Message}", afterNotificationId, numberOfRecords, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting Notifications(afterNotificationId={AfterNotificationId},numberOfRecords={NumberOfRecords})", afterNotificationId, numberOfRecords);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Checks if a reply has recipients.</summary>
        /// <param name="conversationId">The id of conversation to check./>.</param>
        /// <returns>The recipient count or an InternalServerError.</returns>
        [HttpGet]
        public HttpResponseMessage CheckReplyHasRecipients(int conversationId)
        {
			Logger.LogDebug(">>> Entering CheckReplyHasRecipients(conversationId={ConversationId})", conversationId);
            try
            {
                var recipientCount = InternalMessagingController.Instance.CheckReplyHasRecipients(conversationId, UserController.Instance.GetCurrentUserInfo().UserID);
				Logger.LogDebug("CheckReplyHasRecipients(conversationId={ConversationId}): recipientCount → {RecipientCount}", conversationId, recipientCount);
                return this.Request.CreateResponse(HttpStatusCode.OK, recipientCount);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to check the recipient count on a reply, consult the server logs for more information.";
				Logger.LogDebug("CheckReplyHasRecipients(conversationId={ConversationId}): message → {Message}", conversationId, message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting CheckReplyHasRecipients(conversationId={ConversationId})", conversationId);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the notifications count.</summary>
        /// <returns>A number representing the notification count.</returns>
        [HttpGet]
        public HttpResponseMessage CountNotifications()
        {
			Logger.LogDebug(">>> Entering CountNotifications()");
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("CountNotifications(): portalId → {PortalId}", portalId);
                int notifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId);
				Logger.LogDebug("CountNotifications(): notifications → {Notifications}", notifications);
                return this.Request.CreateResponse(HttpStatusCode.OK, notifications);
            }
            catch (Exception ex)
            {
                const string message = "An unexpected error occurred while attempting to get the notification count, consult the server logs for more information.";
				Logger.LogDebug("CountNotifications(): message → {Message}", message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting CountNotifications()");
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the number of unread messages.</summary>
        /// <returns>The number of unread messages.</returns>
        [HttpGet]
        public HttpResponseMessage CountUnreadMessages()
        {
			Logger.LogDebug(">>> Entering CountUnreadMessages()");
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("CountUnreadMessages(): portalId → {PortalId}", portalId);
                var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
				Logger.LogDebug("CountUnreadMessages(): unreadMessages → {UnreadMessages}", unreadMessages);
                return this.Request.CreateResponse(HttpStatusCode.OK, unreadMessages);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to , consult the server logs for more information.";
				Logger.LogDebug("CountUnreadMessages(): message → {Message}", message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting CountUnreadMessages()");
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets both the total number of unread messages and new notifications.</summary>
        /// <returns><see cref="TotalsViewModel"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetTotals()
        {
			Logger.LogDebug(">>> Entering GetTotals()");
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
				Logger.LogDebug("GetTotals(): portalId → {PortalId}", portalId);
                var totalsViewModel = new TotalsViewModel
                {
                    TotalUnreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId),
                    TotalNotifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, totalsViewModel);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to get the unread messages and new notifications count, consult the server logs for more information.";
				Logger.LogDebug("GetTotals(): message → {Message}", message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting GetTotals()");
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Dismisses all new notifications.</summary>
        /// <returns>A "success" Result and a deleteCount representing how many notifications where deleted.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DismissAllNotifications()
        {
			Logger.LogDebug(">>> Entering DismissAllNotifications()");
            try
            {
                var deletedCount = NotificationsController.Instance.DeleteUserNotifications(this.UserInfo);
				Logger.LogDebug("DismissAllNotifications(): deletedCount → {DeletedCount}", deletedCount);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", count = deletedCount });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to dismiss notifications, consult the server logs for more information.";
				Logger.LogDebug("DismissAllNotifications(): message → {Message}", message);
                Logger.Error(message, ex);
				Logger.LogDebug("<<< Exiting DismissAllNotifications()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        private string LocalizeActionString(string key, int desktopModuleId)
        {
			Logger.LogDebug(">>> Entering LocalizeActionString(key={Key},desktopModuleId={DesktopModuleId})", key, desktopModuleId);
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            string actionString;

            if (desktopModuleId > 0)
            {
                var desktopModule = DesktopModuleController.GetDesktopModule(this.hostSettings, desktopModuleId, this.PortalSettings.PortalId);
				Logger.LogDebug("LocalizeActionString(key={Key},desktopModuleId={DesktopModuleId}): desktopModule → {DesktopModule}", key, desktopModuleId, desktopModule);

                var folderName = desktopModule.FolderName.Replace(@"\", "/");
                var resourceFile = $"~/DesktopModules/{folderName}/{Localization.LocalResourceDirectory}/{Localization.LocalSharedResourceFile}";

                actionString = Localization.GetString(key, resourceFile);
            }
            else
			Logger.LogDebug("LocalizeActionString(key={Key},desktopModuleId={DesktopModuleId}): folderName → {FolderName}", key, desktopModuleId, folderName);
			Logger.LogDebug("LocalizeActionString(key={Key},desktopModuleId={DesktopModuleId}): actionString → {ActionString}", key, desktopModuleId, actionString);
            {
                actionString = Localization.GetString(key);
            }

			Logger.LogDebug("<<< Exiting LocalizeActionString(key={Key},desktopModuleId={DesktopModuleId})", key, desktopModuleId);
            return string.IsNullOrEmpty(actionString) ? key : actionString;
        }

        private dynamic ToExpandoObject(Message message)
        {
			Logger.LogDebug(">>> Entering ToExpandoObject(message={Message})", message);
            dynamic messageObj = new ExpandoObject();
            messageObj.PortalID = message.PortalID;
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.PortalID → {Arg1}", message, messageObj.PortalID);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.KeyID → {Arg1}", message, messageObj.KeyID);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.MessageID → {Arg1}", message, messageObj.MessageID);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.ConversationId → {Arg1}", message, messageObj.ConversationId);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.SenderUserID → {Arg1}", message, messageObj.SenderUserID);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.From → {Arg1}", message, messageObj.From);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.To → {Arg1}", message, messageObj.To);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.Subject → {Arg1}", message, messageObj.Subject);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.Body → {Arg1}", message, messageObj.Body);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.DisplayDate → {Arg1}", message, messageObj.DisplayDate);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.ReplyAllAllowed → {Arg1}", message, messageObj.ReplyAllAllowed);
            messageObj.KeyID = message.KeyID;
            messageObj.MessageID = message.MessageID;
            messageObj.ConversationId = message.ConversationId;
            messageObj.SenderUserID = message.SenderUserID;
            messageObj.From = message.From;
            messageObj.To = message.To;
            messageObj.Subject = message.Subject;
            messageObj.Body = message.Body;
            messageObj.DisplayDate = message.DisplayDate;
            messageObj.ReplyAllAllowed = message.ReplyAllAllowed;

            // base entity properties
            messageObj.CreatedByUserID = message.CreatedByUserID;
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.CreatedByUserID → {Arg1}", message, messageObj.CreatedByUserID);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.CreatedOnDate → {Arg1}", message, messageObj.CreatedOnDate);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.LastModifiedByUserID → {Arg1}", message, messageObj.LastModifiedByUserID);
			Logger.LogDebug("ToExpandoObject(message={Message}): messageObj.LastModifiedOnDate → {Arg1}", message, messageObj.LastModifiedOnDate);
            messageObj.CreatedOnDate = message.CreatedOnDate;
            messageObj.LastModifiedByUserID = message.LastModifiedByUserID;
            messageObj.LastModifiedOnDate = message.LastModifiedOnDate;

			Logger.LogDebug("<<< Exiting ToExpandoObject(message={Message})", message);
            return messageObj;
        }

        /// <summary>Represents a conversation.</summary>
        public class ConversationDTO
        {
            /// <summary>Gets or sets id of the conversation.</summary>
            public int ConversationId { get; set; }
        }

        /// <summary>Represents a message reply.</summary>
        public class ReplyDTO : ConversationDTO
        {
            /// <summary>Gets or sets the body of the reply.</summary>
            public string Body { get; set; }

            /// <summary>Gets or sets the ids of the attached files.</summary>
            public IList<int> FileIds { get; set; }
        }
    }
}
