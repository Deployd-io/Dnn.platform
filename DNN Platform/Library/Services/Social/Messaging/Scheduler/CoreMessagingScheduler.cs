// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Social.Messaging.Internal;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>A SchedulerClient instance that handles all the offline messaging actions.</summary>
    public class CoreMessagingScheduler : SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreMessagingScheduler));
        /// <summary>The setting name for number hours since last hourly digest run.</summary>
        private const string SettingLastHourlyRun = "CoreMessagingLastHourlyDigestRun";

        /// <summary>The setting name for number hours since last daily digest run.</summary>
        private const string SettingLastDailyRun = "CoreMessagingLastDailyDigestRun";

        /// <summary>The setting name for number hours since last weekly digest run.</summary>
        private const string SettingLastWeeklyRun = "CoreMessagingLastWeeklyDigestRun";

        /// <summary>The setting name for number hours since last monthly digest run.</summary>
        private const string SettingLastMonthlyRun = "CoreMessagingLastMonthlyDigestRun";

        private readonly IHostSettings hostSettings;
        private readonly IMailSettings mailSettings;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="CoreMessagingScheduler"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The object schedule history item.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public CoreMessagingScheduler(ScheduleHistoryItem objScheduleHistoryItem)
            : this(objScheduleHistoryItem, null, null, null)
        {
		Logger.LogDebug(">>> Entering CoreMessagingScheduler(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
        }

		Logger.LogDebug("<<< Exiting CoreMessagingScheduler(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
        /// <summary>Initializes a new instance of the <see cref="CoreMessagingScheduler"/> class.</summary>
        /// <param name="scheduleHistoryItem">The object schedule history item.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="mailSettings">The mail settings.</param>
        /// <param name="portalController">The portal controller.</param>
        public CoreMessagingScheduler(ScheduleHistoryItem scheduleHistoryItem, IHostSettings hostSettings, IMailSettings mailSettings, IPortalController portalController)
        {
            this.ScheduleHistoryItem = scheduleHistoryItem;
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("CoreMessagingScheduler(scheduleHistoryItem={ScheduleHistoryItem},hostSettings={HostSettings},mailSettings={MailSettings},portalController={PortalController}): this.ScheduleHistoryItem → {Arg4}", scheduleHistoryItem, hostSettings, mailSettings, portalController, this.ScheduleHistoryItem);
			Logger.LogDebug("CoreMessagingScheduler(scheduleHistoryItem={ScheduleHistoryItem},hostSettings={HostSettings},mailSettings={MailSettings},portalController={PortalController}): hostSettings → {HostSettings1}", scheduleHistoryItem, hostSettings, mailSettings, portalController, hostSettings);
            this.mailSettings = mailSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IMailSettings>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
			Logger.LogDebug("CoreMessagingScheduler(scheduleHistoryItem={ScheduleHistoryItem},hostSettings={HostSettings},mailSettings={MailSettings},portalController={PortalController}): mailSettings → {MailSettings1}", scheduleHistoryItem, hostSettings, mailSettings, portalController, mailSettings);
        }

        /// <summary>This is the method that kicks off the actual work within the SchedulerClient's subclass.</summary>
        public override void DoWork()
        {
			Logger.LogDebug(">>> Entering DoWork()");
            try
            {
                var schedulerInstance = Guid.NewGuid();
				Logger.LogDebug("DoWork(): schedulerInstance → {SchedulerInstance}", schedulerInstance);
                this.ScheduleHistoryItem.AddLogNote("Messaging Scheduler DoWork Starting " + schedulerInstance);

                if (string.IsNullOrEmpty(this.mailSettings.GetServer(Null.NullInteger)))
                {
                    this.ScheduleHistoryItem.AddLogNote("<br>No SMTP Servers have been configured for this host. Terminating task.");
                    this.ScheduleHistoryItem.Succeeded = true;
                }
                else
                {
                    this.Progressing();

                    var instantMessages = this.HandleInstantMessages(schedulerInstance);
					Logger.LogDebug("DoWork(): instantMessages → {InstantMessages}", instantMessages);
                    var remainingMessages = this.hostSettings.MessageSchedulerBatchSize - instantMessages;
                    if (remainingMessages > 0)
                    {
                        this.HandleFrequentDigests(schedulerInstance, remainingMessages);
                    }

                    this.ScheduleHistoryItem.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in DoWork(): {Arg0}", ex.getMessage());
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("<br>Messaging Scheduler Failed: " + ex);
                this.Errored(ref ex);
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }

        /// <summary>Determines whether [is send email enable] [the specified portal identifier].</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>True if mail is enabled in PortalSettings.</returns>
        private static bool IsSendEmailEnable(IPortalController portalController, int portalId)
        {
			Logger.LogDebug(">>> Entering IsSendEmailEnable(portalController={PortalController},portalId={PortalId})", portalController, portalId);
			Logger.LogDebug("<<< Exiting IsSendEmailEnable(portalController={PortalController},portalId={PortalId})", portalController, portalId);
            return PortalController.GetPortalSetting(portalController, "MessagingSendEmail", portalId, "YES") == "YES";
        }

        /// <summary>Determines whether [is user able to receive an email] [the specified recipient user].</summary>
        /// <param name="recipientUser">The recipient user.</param>
        /// <returns>True if the user can receive email, otherwise false.</returns>
        private static bool IsUserAbleToReceiveAnEmail(UserInfo recipientUser)
        {
			Logger.LogDebug(">>> Entering IsUserAbleToReceiveAnEmail(recipientUser={RecipientUser})", recipientUser);
			Logger.LogDebug("<<< Exiting IsUserAbleToReceiveAnEmail(recipientUser={RecipientUser})", recipientUser);
            return recipientUser != null && !recipientUser.IsDeleted && recipientUser.Membership.Approved;
        }

        /// <summary>Gets the sender address.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="fromAddress">From address.</param>
        /// <returns>The formatted sender address.</returns>
        private static string GetSenderAddress(string sender, string fromAddress)
        {
			Logger.LogDebug(">>> Entering GetSenderAddress(sender={Sender},fromAddress={FromAddress})", sender, fromAddress);
			Logger.LogDebug("<<< Exiting GetSenderAddress(sender={Sender},fromAddress={FromAddress})", sender, fromAddress);
            return $"{sender} < {fromAddress} >";
        }

        /// <summary>Gets the email body.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="template">The template.</param>
        /// <param name="messageBody">The message body.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="recipientUser">The recipient user.</param>
        /// <returns>A string containing the email body with any tokens replaced.</returns>
        private static string GetEmailBody(IHostSettings hostSettings, string template, string messageBody, PortalSettings portalSettings, UserInfo recipientUser)
        {
			Logger.LogDebug(">>> Entering GetEmailBody(hostSettings={HostSettings},template={Template},messageBody={MessageBody},portalSettings={PortalSettings},recipientUser={RecipientUser})", hostSettings, template, messageBody, portalSettings, recipientUser);
            template = template.Replace("[MESSAGEBODY]", messageBody); // moved to top since we can replace tokens in there too...
            template = template.Replace("[RECIPIENTUSERID]", recipientUser.UserID.ToString(CultureInfo.CurrentCulture));
			Logger.LogDebug("GetEmailBody(hostSettings={HostSettings},template={Template},messageBody={MessageBody},portalSettings={PortalSettings},recipientUser={RecipientUser}): template → {Template1}", hostSettings, template, messageBody, portalSettings, recipientUser, template);
            template = template.Replace("[RECIPIENTDISPLAYNAME]", recipientUser.DisplayName);
            template = template.Replace("[RECIPIENTEMAIL]", recipientUser.Email);
            template = template.Replace("[SITEURL]", GetPortalHomeUrl(portalSettings));
            template = template.Replace("[NOTIFICATIONURL]", GetNotificationUrl(portalSettings, recipientUser.UserID));
            template = template.Replace("[PORTALNAME]", portalSettings.PortalName);
            template = template.Replace("[LOGOURL]", GetPortalLogoUrl(portalSettings));
            template = template.Replace("[UNSUBSCRIBEURL]", GetSubscriptionsUrl(hostSettings, portalSettings, recipientUser.UserID));
            template = template.Replace("[YEAR]", DateTime.Now.Year.ToString(CultureInfo.CurrentCulture));
            template = ResolveUrl(portalSettings, template);

			Logger.LogDebug("<<< Exiting GetEmailBody(hostSettings={HostSettings},template={Template},messageBody={MessageBody},portalSettings={PortalSettings},recipientUser={RecipientUser})", hostSettings, template, messageBody, portalSettings, recipientUser);
            return template;
        }

        /// <summary>Gets the content of the email item.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="message">The message.</param>
        /// <param name="itemTemplate">The item template.</param>
        /// <returns>A string with all email tokens replaced with content.</returns>
        private static string GetEmailItemContent(PortalSettings portalSettings, MessageRecipient message, string itemTemplate)
        {
			Logger.LogDebug(">>> Entering GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate})", portalSettings, message, itemTemplate);
            var messageDetails = InternalMessagingController.Instance.GetMessage(message.MessageID);

			Logger.LogDebug("GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate}): messageDetails → {MessageDetails}", portalSettings, message, itemTemplate, messageDetails);
            var authorId = message.CreatedByUserID > 0 ? message.CreatedByUserID : messageDetails.SenderUserID;

            var emailItemContent = itemTemplate;
			Logger.LogDebug("GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate}): emailItemContent → {EmailItemContent}", portalSettings, message, itemTemplate, emailItemContent);
            emailItemContent = emailItemContent.Replace("[TITLE]", messageDetails.Subject);
            emailItemContent = emailItemContent.Replace("[CONTENT]", HtmlUtils.ConvertToHtml(messageDetails.Body));
            emailItemContent = emailItemContent.Replace("[PROFILEPICURL]", GetProfilePicUrl(portalSettings, authorId));
            emailItemContent = emailItemContent.Replace("[PROFILEURL]", GetProfileUrl(portalSettings, authorId));
            emailItemContent = emailItemContent.Replace("[DISPLAYNAME]", GetDisplayName(portalSettings, authorId));

            if (messageDetails.NotificationTypeID == 1)
            {
                var toUser = UserController.Instance.GetUser(messageDetails.PortalID, message.UserID);
				Logger.LogDebug("GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate}): toUser → {ToUser}", portalSettings, message, itemTemplate, toUser);
                var defaultLanguage = toUser.Profile.PreferredLocale;

                var acceptUrl = GetRelationshipAcceptRequestUrl(portalSettings, authorId, "AcceptFriend");
				Logger.LogDebug("GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate}): acceptUrl → {AcceptUrl}", portalSettings, message, itemTemplate, acceptUrl);
                var profileUrl = GetProfileUrl(portalSettings, authorId);
				Logger.LogDebug("GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate}): profileUrl → {ProfileUrl}", portalSettings, message, itemTemplate, profileUrl);
                var linkContent = GetFriendRequestActionsTemplate(portalSettings, defaultLanguage);
				Logger.LogDebug("GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate}): linkContent → {LinkContent}", portalSettings, message, itemTemplate, linkContent);
                emailItemContent = emailItemContent.Replace("[FRIENDREQUESTACTIONS]", string.Format(CultureInfo.CurrentCulture, linkContent, acceptUrl, profileUrl));
            }

            if (messageDetails.NotificationTypeID == 3)
            {
                var toUser = UserController.Instance.GetUser(messageDetails.PortalID, message.UserID);
                var defaultLanguage = toUser.Profile.PreferredLocale;

                var acceptUrl = GetRelationshipAcceptRequestUrl(portalSettings, authorId, "FollowBack");
                var profileUrl = GetProfileUrl(portalSettings, authorId);
                var linkContent = GetFollowRequestActionsTemplate(portalSettings, defaultLanguage);
                emailItemContent = emailItemContent.Replace("[FOLLOWREQUESTACTIONS]", string.Format(CultureInfo.CurrentCulture, linkContent, acceptUrl, profileUrl));
            }

            // No social actions for the rest of notifications types
            emailItemContent = emailItemContent.Replace("[FOLLOWREQUESTACTIONS]", string.Empty);
            emailItemContent = emailItemContent.Replace("[FRIENDREQUESTACTIONS]", string.Empty);

			Logger.LogDebug("<<< Exiting GetEmailItemContent(portalSettings={PortalSettings},message={Message},itemTemplate={ItemTemplate})", portalSettings, message, itemTemplate);
            return emailItemContent;
        }

        /// <summary>Marks the messages as dispatched.</summary>
        /// <param name="messages">The messages.</param>
        private static void MarkMessagesAsDispatched(IEnumerable<MessageRecipient> messages)
        {
			Logger.LogDebug(">>> Entering MarkMessagesAsDispatched(messages={Messages})", messages);
            foreach (var message in messages)
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(message.MessageID, message.RecipientID);
            }
			Logger.LogDebug("<<< Exiting MarkMessagesAsDispatched(messages={Messages})", messages);
        }

        /// <summary>Gets the email body item template.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="language">The language.</param>
        /// <returns>The email body template item from the Global Resource File: EMAIL_MESSAGING_DISPATCH_ITEM.</returns>
        private static string GetEmailBodyItemTemplate(PortalSettings portalSettings, string language)
        {
			Logger.LogDebug(">>> Entering GetEmailBodyItemTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
			Logger.LogDebug("<<< Exiting GetEmailBodyItemTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
            return Localization.GetString("EMAIL_MESSAGING_DISPATCH_ITEM", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the email body template.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="language">The language.</param>
        /// <returns>The email body template from the Global Resource File: EMAIL_MESSAGING_DISPATCH_BODY.</returns>
        private static string GetEmailBodyTemplate(PortalSettings portalSettings, string language)
        {
			Logger.LogDebug(">>> Entering GetEmailBodyTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
			Logger.LogDebug("<<< Exiting GetEmailBodyTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
            return Localization.GetString("EMAIL_MESSAGING_DISPATCH_BODY", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the email subject template.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="language">The language.</param>
        /// <returns>The email subject template from the Global Resource File: EMAIL_SUBJECT_FORMAT.</returns>
        private static string GetEmailSubjectTemplate(PortalSettings portalSettings, string language)
        {
			Logger.LogDebug(">>> Entering GetEmailSubjectTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
			Logger.LogDebug("<<< Exiting GetEmailSubjectTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
            return Localization.GetString("EMAIL_SUBJECT_FORMAT", Localization.GlobalResourceFile, language);
        }

        /// <summary>Gets the friend request actions template.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="language">The language.</param>
        /// <returns>The friend request actions defined in the Global Resource File: EMAIL_SOCIAL_FRIENDREQUESTACTIONS.</returns>
        private static string GetFriendRequestActionsTemplate(PortalSettings portalSettings, string language)
        {
			Logger.LogDebug(">>> Entering GetFriendRequestActionsTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
			Logger.LogDebug("<<< Exiting GetFriendRequestActionsTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
            return Localization.GetString("EMAIL_SOCIAL_FRIENDREQUESTACTIONS", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the follow request actions template.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="language">The language.</param>
        /// <returns>The follow request actions defined in the Global Resource File: EMAIL_SOCIAL_FOLLOWREQUESTACTIONS.</returns>
        private static string GetFollowRequestActionsTemplate(PortalSettings portalSettings, string language)
        {
			Logger.LogDebug(">>> Entering GetFollowRequestActionsTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
			Logger.LogDebug("<<< Exiting GetFollowRequestActionsTemplate(portalSettings={PortalSettings},language={Language})", portalSettings, language);
            return Localization.GetString("EMAIL_SOCIAL_FOLLOWREQUESTACTIONS", Localization.GlobalResourceFile, portalSettings, language);
        }

        /// <summary>Gets the name of the sender.</summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <returns>Either the display name for the sender or the portal name.</returns>
        private static string GetSenderName(string displayName, string portalName)
        {
			Logger.LogDebug(">>> Entering GetSenderName(displayName={DisplayName},portalName={PortalName})", displayName, portalName);
			Logger.LogDebug("<<< Exiting GetSenderName(displayName={DisplayName},portalName={PortalName})", displayName, portalName);
            return string.IsNullOrEmpty(displayName) ? portalName : displayName;
        }

        /// <summary>Gets the profile pic URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The handler url to fetch the picture for the specified userId.</returns>
        private static string GetProfilePicUrl(PortalSettings portalSettings, int userId)
        {
			Logger.LogDebug(">>> Entering GetProfilePicUrl(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
			Logger.LogDebug("<<< Exiting GetProfilePicUrl(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
            return $"http://{portalSettings.DefaultPortalAlias}/DnnImageHandler.ashx?mode=profilepic&userId={userId}&h={64}&w={64}";
        }

        /// <summary>Gets the relationship accept request URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="action">The action.</param>
        /// <returns>The handler url to fetch the relationship picture for the specified userId.</returns>
        private static string GetRelationshipAcceptRequestUrl(PortalSettings portalSettings, int userId, string action)
        {
			Logger.LogDebug(">>> Entering GetRelationshipAcceptRequestUrl(portalSettings={PortalSettings},userId={UserId},action={Action})", portalSettings, userId, action);
			Logger.LogDebug("<<< Exiting GetRelationshipAcceptRequestUrl(portalSettings={PortalSettings},userId={UserId},action={Action})", portalSettings, userId, action);
            return $"http://{portalSettings.DefaultPortalAlias}/tabid/{portalSettings.UserTabId}/userId/{userId.ToString(CultureInfo.InvariantCulture)}/action/{action}/{Globals.glbDefaultPage}";
        }

        /// <summary>Gets the profile URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The handler url to fetch the profile picture for the specified userId.</returns>
        private static string GetProfileUrl(PortalSettings portalSettings, int userId)
        {
			Logger.LogDebug(">>> Entering GetProfileUrl(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
			Logger.LogDebug("<<< Exiting GetProfileUrl(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
            return $"http://{portalSettings.DefaultPortalAlias}/tabid/{portalSettings.UserTabId}/userId/{userId.ToString(CultureInfo.InvariantCulture)}/{Globals.glbDefaultPage}";
        }

        /// <summary>Gets the display name.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The display name for the given user.</returns>
        private static string GetDisplayName(PortalSettings portalSettings, int userId)
        {
			Logger.LogDebug(">>> Entering GetDisplayName(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
			Logger.LogDebug("<<< Exiting GetDisplayName(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
            return (UserController.GetUserById(portalSettings.PortalId, userId) != null)
                       ? UserController.GetUserById(portalSettings.PortalId, userId).DisplayName
                       : portalSettings.PortalName;
        }

        /// <summary>Gets the notification URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The handler url to fetch a notification for the specified userId.</returns>
        private static string GetNotificationUrl(PortalSettings portalSettings, int userId)
        {
			Logger.LogDebug(">>> Entering GetNotificationUrl(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
            var cacheKey = $"MessageCenterTab:{portalSettings.PortalId}:{portalSettings.CultureCode}";
            var messageTabId = DataCache.GetCache<int>(cacheKey);
			Logger.LogDebug("GetNotificationUrl(portalSettings={PortalSettings},userId={UserId}): messageTabId → {MessageTabId}", portalSettings, userId, messageTabId);
			Logger.LogDebug("GetNotificationUrl(portalSettings={PortalSettings},userId={UserId}): cacheKey → {CacheKey}", portalSettings, userId, cacheKey);
            if (messageTabId <= 0)
            {
                messageTabId = portalSettings.UserTabId;
                var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
				Logger.LogDebug("GetNotificationUrl(portalSettings={PortalSettings},userId={UserId}): profileTab → {ProfileTab}", portalSettings, userId, profileTab);
                if (profileTab != null)
                {
                    var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
					Logger.LogDebug("GetNotificationUrl(portalSettings={PortalSettings},userId={UserId}): childTabs → {ChildTabs}", portalSettings, userId, childTabs);
                    foreach (var tab in childTabs)
                    {
                        foreach (var kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                        {
                            var module = kvp.Value;
                            if (module.DesktopModule.FriendlyName == "Message Center")
                            {
                                messageTabId = tab.TabID;
                                break;
                            }
                        }
                    }
                }

                DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));
            }

			Logger.LogDebug("<<< Exiting GetNotificationUrl(portalSettings={PortalSettings},userId={UserId})", portalSettings, userId);
            return $"http://{portalSettings.DefaultPortalAlias}/tabid/{messageTabId}/userId/{userId}/{Globals.glbDefaultPage}#dnnCoreNotification";
        }

        /// <summary>Gets the portal logo URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>A Url to the portal logo.</returns>
        private static string GetPortalLogoUrl(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetPortalLogoUrl(portalSettings={PortalSettings})", portalSettings);
			Logger.LogDebug("<<< Exiting GetPortalLogoUrl(portalSettings={PortalSettings})", portalSettings);
            return $"http://{GetDomainName(portalSettings)}/{portalSettings.HomeDirectory}/{portalSettings.LogoFile}";
        }

        /// <summary>Gets the name of the domain.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>Resolves the domain name (portal alias) for the specified portal.</returns>
        private static string GetDomainName(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetDomainName(portalSettings={PortalSettings})", portalSettings);
            var portalAlias = portalSettings.DefaultPortalAlias;
			Logger.LogDebug("<<< Exiting GetDomainName(portalSettings={PortalSettings})", portalSettings);
            return portalAlias.IndexOf("/", StringComparison.InvariantCulture) != -1 ?
                       portalAlias.Substring(0, portalAlias.IndexOf("/", StringComparison.InvariantCulture)) :
                       portalAlias;
        }

        /// <summary>Gets the portal home URL.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>The default portal alias url.</returns>
        private static string GetPortalHomeUrl(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetPortalHomeUrl(portalSettings={PortalSettings})", portalSettings);
			Logger.LogDebug("<<< Exiting GetPortalHomeUrl(portalSettings={PortalSettings})", portalSettings);
            return $"http://{portalSettings.DefaultPortalAlias}";
        }

        /// <summary>Gets the subscriptions URL.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The url for viewing subscriptions.</returns>
        private static string GetSubscriptionsUrl(IHostSettings hostSettings, PortalSettings portalSettings, int userId)
        {
			Logger.LogDebug(">>> Entering GetSubscriptionsUrl(hostSettings={HostSettings},portalSettings={PortalSettings},userId={UserId})", hostSettings, portalSettings, userId);
			Logger.LogDebug("<<< Exiting GetSubscriptionsUrl(hostSettings={HostSettings},portalSettings={PortalSettings},userId={UserId})", hostSettings, portalSettings, userId);
            return $"http://{portalSettings.DefaultPortalAlias}/tabid/{GetMessageTab(hostSettings, portalSettings)}/ctl/Profile/userId/{userId}/pageno/3";
        }

        /// <summary>Gets the message tab.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="sendingPortal">The sending portal.</param>
        /// <returns>The tabId for where the Message Center is installed.</returns>
        private static int GetMessageTab(IHostSettings hostSettings, PortalSettings sendingPortal)
        {
			Logger.LogDebug(">>> Entering GetMessageTab(hostSettings={HostSettings},sendingPortal={SendingPortal})", hostSettings, sendingPortal);
            var cacheKey = $"MessageTab:{sendingPortal.PortalId}";

			Logger.LogDebug("GetMessageTab(hostSettings={HostSettings},sendingPortal={SendingPortal}): cacheKey → {CacheKey}", hostSettings, sendingPortal, cacheKey);
            var cacheItemArgs = new CacheItemArgs(cacheKey, 30, CacheItemPriority.Default, sendingPortal);
			Logger.LogDebug("GetMessageTab(hostSettings={HostSettings},sendingPortal={SendingPortal}): cacheItemArgs → {CacheItemArgs}", hostSettings, sendingPortal, cacheItemArgs);

			Logger.LogDebug("<<< Exiting GetMessageTab(hostSettings={HostSettings},sendingPortal={SendingPortal})", hostSettings, sendingPortal);
            return CBO.GetCachedObject<int>(hostSettings, cacheItemArgs, GetMessageTabCallback);
        }

        /// <summary>Gets the message tab callback.</summary>
        /// <param name="cacheItemArgs">The cache item arguments.</param>
        /// <returns>The tab ID for the Message Center OR the user profile page tab ID.</returns>
        private static object GetMessageTabCallback(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetMessageTabCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalSettings = (IPortalSettings)cacheItemArgs.Params[0];

			Logger.LogDebug("GetMessageTabCallback(cacheItemArgs={CacheItemArgs}): portalSettings → {PortalSettings}", cacheItemArgs, portalSettings);
            var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
			Logger.LogDebug("GetMessageTabCallback(cacheItemArgs={CacheItemArgs}): profileTab → {ProfileTab}", cacheItemArgs, profileTab);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
				Logger.LogDebug("GetMessageTabCallback(cacheItemArgs={CacheItemArgs}): childTabs → {ChildTabs}", cacheItemArgs, childTabs);
                foreach (var tab in childTabs)
                {
                    foreach (var kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center")
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            // default to User Profile Page
			Logger.LogDebug("<<< Exiting GetMessageTabCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return portalSettings.UserTabId;
        }

        private static string RemoveHttpUrlsIfSiteIsSslEnabled(string stringContainingHttp, PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering RemoveHttpUrlsIfSiteIsSslEnabled(stringContainingHttp={StringContainingHttp},portalSettings={PortalSettings})", stringContainingHttp, portalSettings);
            if (stringContainingHttp.IndexOf("http", StringComparison.Ordinal) > -1 && portalSettings != null && (portalSettings.SSLEnabled || portalSettings.SSLEnforced))
            {
                var urlToReplace = GetPortalHomeUrl(portalSettings);
				Logger.LogDebug("RemoveHttpUrlsIfSiteIsSslEnabled(stringContainingHttp={StringContainingHttp},portalSettings={PortalSettings}): urlToReplace → {UrlToReplace}", stringContainingHttp, portalSettings, urlToReplace);
                var urlReplaceWith = $"https://{portalSettings.DefaultPortalAlias}";
				Logger.LogDebug("RemoveHttpUrlsIfSiteIsSslEnabled(stringContainingHttp={StringContainingHttp},portalSettings={PortalSettings}): urlReplaceWith → {UrlReplaceWith}", stringContainingHttp, portalSettings, urlReplaceWith);
                stringContainingHttp = stringContainingHttp.Replace(urlToReplace, urlReplaceWith);
				Logger.LogDebug("RemoveHttpUrlsIfSiteIsSslEnabled(stringContainingHttp={StringContainingHttp},portalSettings={PortalSettings}): stringContainingHttp → {StringContainingHttp1}", stringContainingHttp, portalSettings, stringContainingHttp);
            }

			Logger.LogDebug("<<< Exiting RemoveHttpUrlsIfSiteIsSslEnabled(stringContainingHttp={StringContainingHttp},portalSettings={PortalSettings})", stringContainingHttp, portalSettings);
            return stringContainingHttp;
        }

        private static string ResolveUrl(PortalSettings portalSettings, string template)
        {
			Logger.LogDebug(">>> Entering ResolveUrl(portalSettings={PortalSettings},template={Template})", portalSettings, template);
            const string linkRegex = "(href|src)=\"(/[^\"]*?)\"";
            var matches = Regex.Matches(template, linkRegex, RegexOptions.Multiline | RegexOptions.IgnoreCase);
			Logger.LogDebug("ResolveUrl(portalSettings={PortalSettings},template={Template}): matches → {Matches}", portalSettings, template, matches);
            foreach (Match match in matches)
            {
                var link = match.Groups[2].Value;
				Logger.LogDebug("ResolveUrl(portalSettings={PortalSettings},template={Template}): link → {Link}", portalSettings, template, link);
                var defaultAlias = portalSettings.DefaultPortalAlias;
                var domain = Globals.AddHTTP(defaultAlias);
				Logger.LogDebug("ResolveUrl(portalSettings={PortalSettings},template={Template}): domain → {Domain}", portalSettings, template, domain);
                if (defaultAlias.Contains("/"))
                {
                    var subDomain =
                        defaultAlias.Substring(defaultAlias.IndexOf("/", StringComparison.InvariantCultureIgnoreCase));
                    if (link.StartsWith(subDomain, StringComparison.InvariantCultureIgnoreCase))
                    {
                        link = link.Substring(subDomain.Length);
                    }
                }

                template = template.Replace(match.Value, $"{match.Groups[1].Value}=\"{domain}{link}\"");
				Logger.LogDebug("ResolveUrl(portalSettings={PortalSettings},template={Template}): template → {Template1}", portalSettings, template, template);
            }

			Logger.LogDebug("<<< Exiting ResolveUrl(portalSettings={PortalSettings},template={Template})", portalSettings, template);
            return template;
        }

        /// <summary>Sends the digest.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="senderUser">The sender user.</param>
        /// <param name="recipientUser">The recipient user.</param>
        private static void SendDigest(IHostSettings hostSettings, IPortalController portalController, IEnumerable<MessageRecipient> messages, PortalSettings portalSettings, UserInfo senderUser, UserInfo recipientUser)
        {
			Logger.LogDebug(">>> Entering SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser})", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser);
            var messageRecipients = messages as MessageRecipient[] ?? messages.ToArray();

			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): messageRecipients → {MessageRecipients}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, messageRecipients);
            if (!IsUserAbleToReceiveAnEmail(recipientUser))
            {
                MarkMessagesAsDispatched(messageRecipients);
                return;
            }

            if (!IsSendEmailEnable(portalController, portalSettings.PortalId))
            {
                MarkMessagesAsDispatched(messageRecipients);
                return;
            }

            var defaultLanguage = recipientUser.Profile.PreferredLocale;

            var emailSubjectTemplate = GetEmailSubjectTemplate(portalSettings, defaultLanguage);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): emailSubjectTemplate → {EmailSubjectTemplate}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, emailSubjectTemplate);
            var emailBodyTemplate = GetEmailBodyTemplate(portalSettings, defaultLanguage);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): emailBodyTemplate → {EmailBodyTemplate}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, emailBodyTemplate);
            var emailBodyItemTemplate = GetEmailBodyItemTemplate(portalSettings, defaultLanguage);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): emailBodyItemTemplate → {EmailBodyItemTemplate}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, emailBodyItemTemplate);

            var emailBodyItemContent = messageRecipients.Aggregate(
                string.Empty,
                (current, message) => current + GetEmailItemContent(portalSettings, message, emailBodyItemTemplate));

            var fromAddress = portalSettings.Email;
            var toAddress = recipientUser.Email;

            var senderName = GetSenderName(senderUser.DisplayName, portalSettings.PortalName);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): senderName → {SenderName}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, senderName);
            var senderAddress = GetSenderAddress(senderName, fromAddress);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): senderAddress → {SenderAddress}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, senderAddress);

            var subject = string.Format(CultureInfo.CurrentCulture, emailSubjectTemplate, portalSettings.PortalName);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): subject → {Subject}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, subject);
            var body = GetEmailBody(hostSettings, emailBodyTemplate, emailBodyItemContent, portalSettings, recipientUser);
			Logger.LogDebug("SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser}): body → {Body}", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser, body);
            body = RemoveHttpUrlsIfSiteIsSslEnabled(body, portalSettings);

            Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);

            MarkMessagesAsDispatched(messageRecipients);
			Logger.LogDebug("<<< Exiting SendDigest(hostSettings={HostSettings},portalController={PortalController},messages={Messages},portalSettings={PortalSettings},senderUser={SenderUser},recipientUser={RecipientUser})", hostSettings, portalController, messages, portalSettings, senderUser, recipientUser);
        }

        /// <summary>Sends the message and attachments if configured to include them.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="messageRecipient">The message recipient.</param>
        private static void SendMessage(IHostSettings hostSettings, IPortalController portalController, MessageRecipient messageRecipient)
        {
			Logger.LogDebug(">>> Entering SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient})", hostSettings, portalController, messageRecipient);
            var message = InternalMessagingController.Instance.GetMessage(messageRecipient.MessageID);

			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): message → {Message}", hostSettings, portalController, messageRecipient, message);
            var toUser = UserController.Instance.GetUser(message.PortalID, messageRecipient.UserID);
			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): toUser → {ToUser}", hostSettings, portalController, messageRecipient, toUser);
            if (!IsUserAbleToReceiveAnEmail(toUser))
            {
                InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            if (!IsSendEmailEnable(portalController, message.PortalID))
            {
                InternalMessagingController.Instance.MarkMessageAsSent(messageRecipient.MessageID, messageRecipient.RecipientID);
                return;
            }

            var defaultLanguage = toUser.Profile.PreferredLocale;
            var portalSettings = new PortalSettings(message.PortalID);
			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): portalSettings → {PortalSettings}", hostSettings, portalController, messageRecipient, portalSettings);

            var emailBodyTemplate = GetEmailBodyTemplate(portalSettings, defaultLanguage);
			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): emailBodyTemplate → {EmailBodyTemplate}", hostSettings, portalController, messageRecipient, emailBodyTemplate);
            var emailBodyItemTemplate = GetEmailBodyItemTemplate(portalSettings, defaultLanguage);
			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): emailBodyItemTemplate → {EmailBodyItemTemplate}", hostSettings, portalController, messageRecipient, emailBodyItemTemplate);

            var author = UserController.Instance.GetUser(message.PortalID, message.SenderUserID);
			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): author → {Author}", hostSettings, portalController, messageRecipient, author);
            var user = UserController.GetUserByEmail(portalSettings.PortalId, portalSettings.Email);
			Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): user → {User}", hostSettings, portalController, messageRecipient, user);
            var fromAddress = (user != null)
                ? $"{user.DisplayName} < {portalSettings.Email} >"
                : portalSettings.Email;
            var toAddress = toUser.Email;

            if (Mail.IsValidEmailAddress(toUser.Email, toUser.PortalID))
            {
                var senderName = GetSenderName(author.DisplayName, portalSettings.PortalName);
				Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): senderName → {SenderName}", hostSettings, portalController, messageRecipient, senderName);
                var senderAddress = GetSenderAddress(senderName, portalSettings.Email);
				Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): senderAddress → {SenderAddress}", hostSettings, portalController, messageRecipient, senderAddress);
                var emailBodyItemContent = GetEmailItemContent(portalSettings, messageRecipient, emailBodyItemTemplate);
				Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): emailBodyItemContent → {EmailBodyItemContent}", hostSettings, portalController, messageRecipient, emailBodyItemContent);
                var subject = InternalMessagingController.Instance.GetMessage(message.MessageID).Subject;
				Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): subject → {Subject}", hostSettings, portalController, messageRecipient, subject);
                if (string.IsNullOrEmpty(subject))
                {
                    subject = string.Format(CultureInfo.CurrentCulture, GetEmailSubjectTemplate(portalSettings, defaultLanguage), portalSettings.PortalName);
                }

                var body = GetEmailBody(hostSettings, emailBodyTemplate, emailBodyItemContent, portalSettings, toUser);
				Logger.LogDebug("SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient}): body → {Body}", hostSettings, portalController, messageRecipient, body);
                body = RemoveHttpUrlsIfSiteIsSslEnabled(body, portalSettings);

                // Include the attachment in the email message if configured to do so
                if (InternalMessagingController.Instance.AttachmentsAllowed(message.PortalID))
                {
                    Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body, CreateAttachments(message.MessageID).ToList());
                }
                else
                {
                    Mail.SendEmail(fromAddress, senderAddress, toAddress, subject, body);
                }
            }

            InternalMessagingController.Instance.MarkMessageAsDispatched(messageRecipient.MessageID, messageRecipient.RecipientID);
			Logger.LogDebug("<<< Exiting SendMessage(hostSettings={HostSettings},portalController={PortalController},messageRecipient={MessageRecipient})", hostSettings, portalController, messageRecipient);
        }

        /// <summary>Creates list of attachments for the specified message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A list of attachments.</returns>
        private static IEnumerable<MailAttachment> CreateAttachments(int messageId)
        {
			Logger.LogDebug(">>> Entering CreateAttachments(messageId={MessageId})", messageId);
            foreach (var fileView in InternalMessagingController.Instance.GetAttachments(messageId))
            {
                var file = FileManager.Instance.GetFile(fileView.FileId);
				Logger.LogDebug("CreateAttachments(messageId={MessageId}): file → {File}", messageId, file);
                if (file != null)
                {
                    using var fileContent = FileManager.Instance.GetFileContent(file);
					Logger.LogDebug("CreateAttachments(messageId={MessageId}): fileContent → {FileContent}", messageId, fileContent);
                    using var ms = new MemoryStream();
					Logger.LogDebug("CreateAttachments(messageId={MessageId}): ms → {Ms}", messageId, ms);
                    fileContent.CopyTo(ms);
                    yield return new MailAttachment(file.FileName, ms.ToArray(), file.ContentType);
                }
            }
			Logger.LogDebug("<<< Exiting CreateAttachments(messageId={MessageId})", messageId);
        }

        /// <summary>Handles the frequent digests.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="remainingMessages">The remaining messages.</param>
        private void HandleFrequentDigests(Guid schedulerInstance, int remainingMessages)
        {
			Logger.LogDebug(">>> Entering HandleFrequentDigests(schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages})", schedulerInstance, remainingMessages);
            var handledMessages = this.HandleFrequencyDigest(DateTime.Now.AddHours(-1), SettingLastHourlyRun, Frequency.Hourly, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;
			Logger.LogDebug("HandleFrequentDigests(schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages}): remainingMessages → {RemainingMessages1}", schedulerInstance, remainingMessages, remainingMessages);
			Logger.LogDebug("HandleFrequentDigests(schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages}): handledMessages → {HandledMessages}", schedulerInstance, remainingMessages, handledMessages);

            handledMessages = this.HandleFrequencyDigest(DateTime.Now.AddDays(-1), SettingLastDailyRun, Frequency.Daily, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;

            handledMessages = this.HandleFrequencyDigest(DateTime.Now.AddDays(-7), SettingLastWeeklyRun, Frequency.Weekly, schedulerInstance, remainingMessages);
            remainingMessages = remainingMessages - handledMessages;

            this.HandleFrequencyDigest(DateTime.Now.AddDays(-30), SettingLastMonthlyRun, Frequency.Monthly, schedulerInstance, remainingMessages);
			Logger.LogDebug("<<< Exiting HandleFrequentDigests(schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages})", schedulerInstance, remainingMessages);
        }

        /// <summary>Handles the frequency digest.</summary>
        /// <param name="dateToCompare">The date to compare.</param>
        /// <param name="settingKeyLastRunDate">The setting key last run date.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="remainingMessages">The remaining messages.</param>
        /// <returns>The count of messages processed.</returns>
        private int HandleFrequencyDigest(DateTime dateToCompare, string settingKeyLastRunDate, Frequency frequency, Guid schedulerInstance, int remainingMessages)
        {
			Logger.LogDebug(">>> Entering HandleFrequencyDigest(dateToCompare={DateToCompare},settingKeyLastRunDate={SettingKeyLastRunDate},frequency={Frequency},schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages})", dateToCompare, settingKeyLastRunDate, frequency, schedulerInstance, remainingMessages);
            int handledMessages = 0;
            if (remainingMessages <= 0)
			Logger.LogDebug("HandleFrequencyDigest(dateToCompare={DateToCompare},settingKeyLastRunDate={SettingKeyLastRunDate},frequency={Frequency},schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages}): handledMessages → {HandledMessages}", dateToCompare, settingKeyLastRunDate, frequency, schedulerInstance, remainingMessages, handledMessages);
            {
                return handledMessages;
            }

            var lastRunDate = this.GetScheduleItemDateSetting(settingKeyLastRunDate);
			Logger.LogDebug("HandleFrequencyDigest(dateToCompare={DateToCompare},settingKeyLastRunDate={SettingKeyLastRunDate},frequency={Frequency},schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages}): lastRunDate → {LastRunDate}", dateToCompare, settingKeyLastRunDate, frequency, schedulerInstance, remainingMessages, lastRunDate);
            if (dateToCompare >= lastRunDate)
            {
                handledMessages = this.HandleDigest(schedulerInstance, frequency, remainingMessages);
                if (handledMessages < remainingMessages)
                {
                    SchedulingProvider.Instance().AddScheduleItemSetting(
                        this.ScheduleHistoryItem.ScheduleID, settingKeyLastRunDate, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                }
            }

			Logger.LogDebug("<<< Exiting HandleFrequencyDigest(dateToCompare={DateToCompare},settingKeyLastRunDate={SettingKeyLastRunDate},frequency={Frequency},schedulerInstance={SchedulerInstance},remainingMessages={RemainingMessages})", dateToCompare, settingKeyLastRunDate, frequency, schedulerInstance, remainingMessages);
            return handledMessages;
        }

        /// <summary>Handles the digest.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="remainingMessages">The remaining messages.</param>
        /// <returns>The count of messages processed.</returns>
        private int HandleDigest(Guid schedulerInstance, Frequency frequency, int remainingMessages)
        {
			Logger.LogDebug(">>> Entering HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages})", schedulerInstance, frequency, remainingMessages);
            var messagesSent = 0;

			Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): messagesSent → {MessagesSent}", schedulerInstance, frequency, remainingMessages, messagesSent);
            // get subscribers based on frequency, utilize remaining batch size as part of count of users to return (note, if multiple subscriptions have the same frequency they will be combined into 1 email)
            this.ScheduleHistoryItem.AddLogNote("<br>Messaging Scheduler Starting Digest '" + schedulerInstance + "'.  ");

            var messageLeft = true;
			Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): messageLeft → {MessageLeft}", schedulerInstance, frequency, remainingMessages, messageLeft);

            while (messageLeft)
            {
                var batchMessages = InternalMessagingController.Instance.GetNextMessagesForDigestDispatch(frequency, schedulerInstance, remainingMessages);
				Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): batchMessages → {BatchMessages}", schedulerInstance, frequency, remainingMessages, batchMessages);

                if (batchMessages != null && batchMessages.Count > 0)
                {
                    try
                    {
                        foreach (var userId in (from t in batchMessages select t.UserID).Distinct())
                        {
                            var currentUserId = userId;
                            var colUserMessages = from t in batchMessages where t.UserID == currentUserId select t;
                            var messageRecipients = colUserMessages as MessageRecipient[] ?? colUserMessages.ToArray();
							Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): messageRecipients → {MessageRecipients}", schedulerInstance, frequency, remainingMessages, messageRecipients);
                            var singleMessage = (from t in messageRecipients select t).First();
							Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): singleMessage → {SingleMessage}", schedulerInstance, frequency, remainingMessages, singleMessage);
                            if (singleMessage != null)
                            {
                                var messageDetails = InternalMessagingController.Instance.GetMessage(singleMessage.MessageID);
								Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): messageDetails → {MessageDetails}", schedulerInstance, frequency, remainingMessages, messageDetails);
                                var portalSettings = new PortalSettings(messageDetails.PortalID);
								Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): portalSettings → {PortalSettings}", schedulerInstance, frequency, remainingMessages, portalSettings);

                                var senderUser = UserController.Instance.GetUser(messageDetails.PortalID, messageDetails.SenderUserID);
								Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): senderUser → {SenderUser}", schedulerInstance, frequency, remainingMessages, senderUser);
                                var recipientUser = UserController.Instance.GetUser(messageDetails.PortalID, singleMessage.UserID);
								Logger.LogDebug("HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): recipientUser → {RecipientUser}", schedulerInstance, frequency, remainingMessages, recipientUser);

                                SendDigest(this.hostSettings, this.portalController, messageRecipients, portalSettings, senderUser, recipientUser);
                            }

                            messagesSent = messagesSent + 1;
                        }

                        // at this point we have sent all digest notifications for this batch
                        this.ScheduleHistoryItem.AddLogNote("Sent " + messagesSent + " digest subscription emails for this batch.  ");
                        return messagesSent;
                    }
                    catch (Exception e)
                    {
						Logger.LogError("Exception in HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages}): {Arg3}", schedulerInstance, frequency, remainingMessages, e.Message);
                        this.Errored(ref e);
                    }
                }
                else
                {
                    messageLeft = false;
                }
            }

            this.ScheduleHistoryItem.AddLogNote("Sent " + messagesSent + " " + frequency + " digest subscription emails.  ");

			Logger.LogDebug("<<< Exiting HandleDigest(schedulerInstance={SchedulerInstance},frequency={Frequency},remainingMessages={RemainingMessages})", schedulerInstance, frequency, remainingMessages);
            return messagesSent;
        }

        /// <summary>Gets the schedule item date setting.</summary>
        /// <param name="settingKey">The setting key.</param>
        /// <returns>The date the schedule was ran.</returns>
        private DateTime GetScheduleItemDateSetting(string settingKey)
        {
			Logger.LogDebug(">>> Entering GetScheduleItemDateSetting(settingKey={SettingKey})", settingKey);
            var colScheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(this.ScheduleHistoryItem.ScheduleID);
            var dateValue = DateTime.Now;
			Logger.LogDebug("GetScheduleItemDateSetting(settingKey={SettingKey}): dateValue → {DateValue}", settingKey, dateValue);
			Logger.LogDebug("GetScheduleItemDateSetting(settingKey={SettingKey}): colScheduleItemSettings → {ColScheduleItemSettings}", settingKey, colScheduleItemSettings);

            if (colScheduleItemSettings.Count > 0)
            {
                if (colScheduleItemSettings.ContainsKey(settingKey))
                {
                    dateValue = Convert.ToDateTime(colScheduleItemSettings[settingKey], CultureInfo.InvariantCulture);
                }
            }
            else
            {
                SchedulingProvider.Instance().AddScheduleItemSetting(
                    this.ScheduleHistoryItem.ScheduleID, SettingLastHourlyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   this.ScheduleHistoryItem.ScheduleID, SettingLastDailyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   this.ScheduleHistoryItem.ScheduleID, SettingLastWeeklyRun, dateValue.ToString(CultureInfo.InvariantCulture));
                SchedulingProvider.Instance().AddScheduleItemSetting(
                   this.ScheduleHistoryItem.ScheduleID, SettingLastMonthlyRun, dateValue.ToString(CultureInfo.InvariantCulture));
            }

			Logger.LogDebug("<<< Exiting GetScheduleItemDateSetting(settingKey={SettingKey})", settingKey);
            return dateValue;
        }

        /// <summary>Handles the sending of messages.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <returns>The count of messages sent.</returns>
        private int HandleInstantMessages(Guid schedulerInstance)
        {
			Logger.LogDebug(">>> Entering HandleInstantMessages(schedulerInstance={SchedulerInstance})", schedulerInstance);
            var messageLeft = true;
            var messagesSent = 0;
			Logger.LogDebug("HandleInstantMessages(schedulerInstance={SchedulerInstance}): messagesSent → {MessagesSent}", schedulerInstance, messagesSent);
			Logger.LogDebug("HandleInstantMessages(schedulerInstance={SchedulerInstance}): messageLeft → {MessageLeft}", schedulerInstance, messageLeft);

            while (messageLeft)
            {
                var batchMessages = InternalMessagingController.Instance.GetNextMessagesForInstantDispatch(schedulerInstance, Host.MessageSchedulerBatchSize);
				Logger.LogDebug("HandleInstantMessages(schedulerInstance={SchedulerInstance}): batchMessages → {BatchMessages}", schedulerInstance, batchMessages);

                if (batchMessages is { Count: > 0, })
                {
                    try
                    {
                        foreach (var messageRecipient in batchMessages)
                        {
                            SendMessage(this.hostSettings, this.portalController, messageRecipient);
                            messagesSent = messagesSent + 1;
                        }
                    }
                    catch (Exception e)
                    {
						Logger.LogError("Exception in HandleInstantMessages(schedulerInstance={SchedulerInstance}): {Arg1}", schedulerInstance, e.Message);
                        this.Errored(ref e);
                    }
                }
                else
                {
                    messageLeft = false;
                }
            }

            this.ScheduleHistoryItem.AddLogNote($"<br>Messaging Scheduler '{schedulerInstance}' sent a total of {messagesSent} message(s)");
			Logger.LogDebug("<<< Exiting HandleInstantMessages(schedulerInstance={SchedulerInstance})", schedulerInstance);
            return messagesSent;
        }
    }
}
