// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Social.Messaging.Data;
    using DotNetNuke.Services.Social.Messaging.Exceptions;
    using DotNetNuke.Services.Social.Messaging.Internal;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>  The Controller class for social Messaging.</summary>
    public class MessagingController
                                : ServiceLocator<IMessagingController, MessagingController>,
                                IMessagingController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MessagingController));
        private const int MaxRecipients = 2000;
        private const int MaxSubjectLength = 400;
        private const double DefaultMessagingThrottlingIntervalMinutes = 0.5;

        private readonly IDataService dataService;

        /// <summary>Initializes a new instance of the <see cref="MessagingController"/> class.</summary>
        public MessagingController()
            : this(DataService.Instance)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MessagingController"/> class.</summary>
        /// <param name="dataService">The data service to use.</param>
        public MessagingController(IDataService dataService)
        {
			Logger.LogError(">>> Entering MessagingController(dataService={DataService})", dataService);
			Logger.LogDebug("MessagingController(dataService={DataService}): dataService → {DataService1}", dataService, dataService);
            Requires.NotNull("dataService", dataService);
			Logger.LogWarning("<<< Exiting MessagingController(dataService={DataService})", dataService);
            this.dataService = dataService;
        }

        /// <inheritdoc />
        public virtual void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs)
        {
			Logger.LogError(">>> Entering SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs})", message, roles, users, fileIDs);
            this.SendMessage(message, roles, users, fileIDs, UserController.Instance.GetCurrentUserInfo());
			Logger.LogWarning("<<< Exiting SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs})", message, roles, users, fileIDs);
        }

        /// <inheritdoc />
        public virtual void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender)
        {
			Logger.LogError(">>> Entering SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender})", message, roles, users, fileIDs, sender);
            if (sender == null || sender.UserID <= 0)
            {
				Logger.LogWarning("<<< Exiting SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs})", message, roles, users, fileIDs);
				Logger.LogError("Throwing ArgumentException from SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender})", message, roles, users, fileIDs, sender);
                throw new ArgumentException(Localization.GetString("MsgSenderRequiredError", Localization.ExceptionsResourceFile));
            }

            if (message == null)
            {
                throw new ArgumentException(Localization.GetString("MsgMessageRequiredError", Localization.ExceptionsResourceFile));
            }

            if (string.IsNullOrEmpty(message.Subject) && string.IsNullOrEmpty(message.Body))
            {
                throw new ArgumentException(Localization.GetString("MsgSubjectOrBodyRequiredError", Localization.ExceptionsResourceFile));
            }

            if (roles == null && users == null)
            {
                throw new ArgumentException(Localization.GetString("MsgRolesOrUsersRequiredError", Localization.ExceptionsResourceFile));
            }

            if (InternalMessagingController.Instance.DisablePrivateMessage(sender.PortalID) && !this.IsAdminOrHost(sender))
            {
                throw new ArgumentException(Localization.GetString("PrivateMessageDisabledError", Localization.ExceptionsResourceFile));
            }

            if (!string.IsNullOrEmpty(message.Subject) && message.Subject.Length > MaxSubjectLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("MsgSubjectTooBigError", Localization.ExceptionsResourceFile), MaxSubjectLength, message.Subject.Length));
            }

            if (roles is { Count: > 0, } && !this.IsAdminOrHost(sender))
            {
                if (!roles.All(role => sender.Social.Roles.Any(userRoleInfo => role.RoleID == userRoleInfo.RoleID && userRoleInfo.IsOwner)))
                {
                    throw new ArgumentException(Localization.GetString("MsgOnlyHostOrAdminOrUserInGroupCanSendToRoleError", Localization.ExceptionsResourceFile));
                }
            }

            var sbTo = new StringBuilder();
			Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): sbTo → {SbTo}", message, roles, users, fileIDs, sender, sbTo);
            var replyAllAllowed = true;
			Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): replyAllAllowed → {ReplyAllAllowed}", message, roles, users, fileIDs, sender, replyAllAllowed);
            if (roles != null)
            {
                foreach (var role in roles.Where(role => !string.IsNullOrEmpty(role.RoleName)))
                {
                    sbTo.Append(role.RoleName + ",");
                    replyAllAllowed = false;
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

            if (sbTo.Length > MaxRecipients)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("MsgToListTooBigError", Localization.ExceptionsResourceFile), MaxRecipients, sbTo.Length));
            }

            // Cannot send message if within ThrottlingInterval
            var waitTime = InternalMessagingController.Instance.WaitTimeForNextMessage(sender);
			Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): waitTime → {WaitTime}", message, roles, users, fileIDs, sender, waitTime);
            if (waitTime > 0)
            {
                var interval = this.GetPortalSettingAsDouble("MessagingThrottlingInterval", sender.PortalID, DefaultMessagingThrottlingIntervalMinutes);
				Logger.LogError("Throwing ThrottlingIntervalNotMetException from SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender})", message, roles, users, fileIDs, sender);
				Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): interval → {Interval}", message, roles, users, fileIDs, sender, interval);
                throw new ThrottlingIntervalNotMetException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("MsgThrottlingIntervalNotMet", Localization.ExceptionsResourceFile), interval));
            }

            // Cannot have attachments if it's not enabled
            if (fileIDs is { Count: > 0, } && !InternalMessagingController.Instance.AttachmentsAllowed(sender.PortalID))
            {
				Logger.LogError("Throwing AttachmentsNotAllowed from SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender})", message, roles, users, fileIDs, sender);
                throw new AttachmentsNotAllowed(Localization.GetString("MsgAttachmentsNotAllowed", Localization.ExceptionsResourceFile));
            }

            // Cannot exceed RecipientLimit
            var recipientCount = 0;
            if (users != null)
            {
                recipientCount += users.Count;
            }

            if (roles != null)
            {
                recipientCount += roles.Count;
            }

            if (recipientCount > InternalMessagingController.Instance.RecipientLimit(sender.PortalID))
            {
				Logger.LogError("Throwing RecipientLimitExceededException from SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender})", message, roles, users, fileIDs, sender);
                throw new RecipientLimitExceededException(Localization.GetString("MsgRecipientLimitExceeded", Localization.ExceptionsResourceFile));
            }

            // Profanity Filter
            var profanityFilterSetting = this.GetPortalSetting("MessagingProfanityFilters", sender.PortalID, "NO");
			Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): profanityFilterSetting → {ProfanityFilterSetting}", message, roles, users, fileIDs, sender, profanityFilterSetting);
            if (profanityFilterSetting.Equals("YES", StringComparison.OrdinalIgnoreCase))
            {
                message.Subject = this.InputFilter(message.Subject);
                message.Body = this.InputFilter(message.Body);
            }

            message.To = sbTo.ToString().Trim(',');
            message.MessageID = Null.NullInteger;
			Logger.LogDebug("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): message.MessageID → {Arg5}", message, roles, users, fileIDs, sender, message.MessageID);
			Logger.LogDebug("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): message.SenderUserID → {Arg5}", message, roles, users, fileIDs, sender, message.SenderUserID);
			Logger.LogDebug("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): message.From → {Arg5}", message, roles, users, fileIDs, sender, message.From);
            message.ReplyAllAllowed = replyAllAllowed;
			Logger.LogDebug("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): message.ReplyAllAllowed → {Arg5}", message, roles, users, fileIDs, sender, message.ReplyAllAllowed);
            message.SenderUserID = sender.UserID;
            message.From = sender.DisplayName;

            message.MessageID = this.dataService.SaveMessage(message, PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID), UserController.Instance.GetCurrentUserInfo().UserID);

            // associate attachments
            if (fileIDs != null)
            {
                foreach (var attachment in fileIDs.Select(fileId => new MessageAttachment { MessageAttachmentID = Null.NullInteger, FileID = fileId, MessageID = message.MessageID }))
                {
                    if (CanViewFile(attachment.FileID) && IsFileInUserFolder(attachment.FileID, sender))
                    {
                        this.dataService.SaveMessageAttachment(attachment, UserController.Instance.GetCurrentUserInfo().UserID);
                    }
                }
            }

            // send message to Roles
            if (roles != null)
            {
                var roleIds = string.Empty;
                roleIds = roles
                    .Select(r => r.RoleID)
                    .Aggregate(roleIds, (current, roleId) => current + (roleId + ","))
                    .Trim(',');

                this.dataService.CreateMessageRecipientsForRole(message.MessageID, roleIds, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // send message to each User - this should be called after CreateMessageRecipientsForRole.
            if (users == null)
            {
                users = new List<UserInfo>();
				Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): users → {Users1}", message, roles, users, fileIDs, sender, users);
            }

            foreach (var recipient in from user in users where InternalMessagingController.Instance.GetMessageRecipient(message.MessageID, user.UserID) == null select new MessageRecipient { MessageID = message.MessageID, UserID = user.UserID, Read = false, RecipientID = Null.NullInteger })
            {
                this.dataService.SaveMessageRecipient(recipient, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            if (users.All(u => u.UserID != sender.UserID))
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(message.MessageID, sender.UserID);
				Logger.LogError("SendMessage(message={Message},roles={Roles},users={Users},fileIDs={FileIDs},sender={Sender}): recipient → {Recipient}", message, roles, users, fileIDs, sender, recipient);

                if (recipient == null)
                {
                    // add sender as a recipient of the message
                    recipient = new MessageRecipient { MessageID = message.MessageID, UserID = sender.UserID, Read = false, RecipientID = Null.NullInteger };
                    recipient.RecipientID = this.dataService.SaveMessageRecipient(recipient, UserController.Instance.GetCurrentUserInfo().UserID);
                }

                InternalMessagingController.Instance.MarkMessageAsDispatched(message.MessageID, recipient.RecipientID);
            }

            // Mark the conversation as read by the sender of the message.
            InternalMessagingController.Instance.MarkRead(message.MessageID, sender.UserID);
        }

        /// <summary>
        /// Gets the current user information (virtual so it can be mocked from tests).
        /// </summary>
        /// <returns><see cref="UserInfo"/>.</returns>
        internal virtual UserInfo GetCurrentUserInfo()
        {
			Logger.LogError(">>> Entering GetCurrentUserInfo()");
            Logger.LogWarning("<<< Exiting GetCurrentUserInfo()");
            return UserController.Instance.GetCurrentUserInfo();
        }

        /// <summary>
        /// Gets a portal setting (virtual so it can be mocked from tests).
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The setting value as a string.</returns>
        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
			Logger.LogError(">>> Entering GetPortalSetting(settingName={SettingName},portalId={PortalId},defaultValue={DefaultValue})", settingName, portalId, defaultValue);
            Logger.LogWarning("<<< Exiting GetPortalSetting(settingName={SettingName},portalId={PortalId},defaultValue={DefaultValue})", settingName, portalId, defaultValue);
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        /// <summary>
        /// Gets a portal setting as double (virtual so it can be mocked in tests).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The setting value as a double.</returns>
        internal virtual double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
        {
			Logger.LogError(">>> Entering GetPortalSettingAsDouble(key={Key},portalId={PortalId},defaultValue={DefaultValue})", key, portalId, defaultValue);
            Logger.LogWarning("<<< Exiting GetPortalSettingAsDouble(key={Key},portalId={PortalId},defaultValue={DefaultValue})", key, portalId, defaultValue);
            return PortalController.GetPortalSettingAsDouble(key, portalId, defaultValue);
        }

        /// <summary>
        /// Gets the input filter (virtual so it can be mocked in tests).
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A string representing the input filter to use.</returns>
        internal virtual string InputFilter(string input)
        {
			Logger.LogError(">>> Entering InputFilter(input={Input})", input);
            var ps = PortalSecurity.Instance;
			Logger.LogWarning("<<< Exiting InputFilter(input={Input})", input);
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }

        /// <summary>
        /// Determines whether the user is an admin or host user (virtual so it can be mocked in tests).
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <returns>Whether the user is an admin or host user.</returns>
        internal virtual bool IsAdminOrHost(UserInfo userInfo)
        {
			Logger.LogError(">>> Entering IsAdminOrHost(userInfo={UserInfo})", userInfo);
            Logger.LogWarning("<<< Exiting IsAdminOrHost(userInfo={UserInfo})", userInfo);
            return userInfo.IsSuperUser || userInfo.IsInRole(PortalController.Instance.GetCurrentSettings().AdministratorRoleName);
        }

        /// <inheritdoc />
        protected override Func<IMessagingController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new MessagingController();
        }

        private static bool CanViewFile(int fileId)
        {
			Logger.LogError(">>> Entering CanViewFile(fileId={FileId})", fileId);
            var file = FileManager.Instance.GetFile(fileId);
			Logger.LogError("CanViewFile(fileId={FileId}): file → {File}", fileId, file);
            if (file == null)
            {
                return false;
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogError("CanViewFile(fileId={FileId}): folder → {Folder}", fileId, folder);
			Logger.LogWarning("<<< Exiting CanViewFile(fileId={FileId})", fileId);
            return folder != null && FolderPermissionController.Instance.CanViewFolder(folder);
        }

        private static bool IsFileInUserFolder(int fileId, UserInfo sender)
        {
			Logger.LogError(">>> Entering IsFileInUserFolder(fileId={FileId},sender={Sender})", fileId, sender);
            var file = FileManager.Instance.GetFile(fileId);
			Logger.LogError("IsFileInUserFolder(fileId={FileId},sender={Sender}): file → {File}", fileId, sender, file);
            if (file == null)
            {
                return false;
            }

            var userFolder = FolderManager.Instance.GetUserFolder(sender);
			Logger.LogError("IsFileInUserFolder(fileId={FileId},sender={Sender}): userFolder → {UserFolder}", fileId, sender, userFolder);
            if (userFolder is null)
            {
                return false;
            }

            var folderId = file.FolderId;
			Logger.LogError("IsFileInUserFolder(fileId={FileId},sender={Sender}): folderId → {FolderId}", fileId, sender, folderId);

            while (folderId != Null.NullInteger)
            {
                var folder = FolderManager.Instance.GetFolder(folderId);
				Logger.LogError("IsFileInUserFolder(fileId={FileId},sender={Sender}): folder → {Folder}", fileId, sender, folder);
                if (folder == null)
                {
                    return false;
                }

                if (folder.FolderID == userFolder.FolderID)
                {
                    return true;
                }

                folderId = folder.ParentID;
            }

			Logger.LogWarning("<<< Exiting IsFileInUserFolder(fileId={FileId},sender={Sender})", fileId, sender);
            return false;
        }
    }
}
