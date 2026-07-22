// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    /// <summary>  The Info class for Messaging.</summary>
    [Serializable]
    public class Message : IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string body;
        private Guid conversation;
        private bool emailSent;
        private int fromUserID;
        private string fromUserName;
        private DateTime messageDate;
        private int messageID;
        private int portalID;
        private int replyTo;
        private MessageStatusType status;
        private string subject;
        private int toRoleId;
        private int toUserID;
        private string toUserName;

        private bool allowReply;
        private bool skipInbox;

        private Guid emailSchedulerInstance;
        private DateTime emailSentDate;

        /// <summary>Initializes a new instance of the <see cref="Message"/> class.</summary>
        public Message()
        {
			Logger.LogDebug(">>> Entering Message()");
            this.Conversation = Guid.Empty;
            this.Status = MessageStatusType.Draft;
			Logger.LogDebug("Message(): this.Conversation → {Arg0}", this.Conversation);
			Logger.LogDebug("Message(): this.Status → {Arg0}", this.Status);
			Logger.LogDebug("Message(): Conversation → {Conversation}", Conversation);
            this.MessageDate = DateTime.Now;
			Logger.LogDebug("<<< Exiting Message()");
			Logger.LogDebug("Message(): this.MessageDate → {Arg0}", this.MessageDate);
        }

        public string FromUserName
        {
            get
            {
                return this.fromUserName;
            }

            private set
            {
                this.fromUserName = value;
            }
        }

        public int FromUserID
        {
            get
            {
                return this.fromUserID;
            }

            set
            {
                this.fromUserID = value;
            }
        }

        public int ToRoleID
        {
            get
            {
                return this.toRoleId;
            }

            set
            {
                this.toRoleId = value;
            }
        }

        public bool AllowReply
        {
            get
            {
                return this.allowReply;
            }

            set
            {
                this.allowReply = value;
            }
        }

        public bool SkipInbox
        {
            get
            {
                return this.skipInbox;
            }

            set
            {
                this.skipInbox = value;
            }
        }

        public bool EmailSent
        {
            get
            {
                return this.emailSent;
            }

            set
            {
                this.emailSent = value;
            }
        }

        public string Body
        {
            get
            {
                return this.body;
            }

            set
            {
                this.body = value;
            }
        }

        public DateTime MessageDate
        {
            get
            {
                return this.messageDate;
            }

            set
            {
                this.messageDate = value;
            }
        }

        public Guid Conversation
        {
            get
            {
                return this.conversation;
            }

            set
            {
                this.conversation = value;
            }
        }

        public int MessageID
        {
            get
            {
                return this.messageID;
            }

            private set
            {
                this.messageID = value;
            }
        }

        public int PortalID
        {
            get
            {
                return this.portalID;
            }

            set
            {
                this.portalID = value;
            }
        }

        public int ReplyTo
        {
            get
            {
                return this.replyTo;
            }

            private set
            {
                this.replyTo = value;
            }
        }

        public MessageStatusType Status
        {
            get
            {
                return this.status;
            }

            set
            {
                this.status = value;
            }
        }

        public string Subject
        {
            get
            {
                var ps = PortalSecurity.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
                return ps.InputFilter(this.subject, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            set
            {
                var ps = PortalSecurity.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
                this.subject = ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        public int ToUserID
        {
            get
            {
                return this.toUserID;
            }

            set
            {
                this.toUserID = value;
            }
        }

        public string ToUserName
        {
            get
            {
                return this.toUserName;
            }

            private set
            {
                this.toUserName = value;
            }
        }

        public DateTime EmailSentDate
        {
            get
            {
                return this.emailSentDate;
            }

            private set
            {
                this.emailSentDate = value;
            }
        }

        public Guid EmailSchedulerInstance
        {
            get
            {
                return this.emailSchedulerInstance;
            }

            private set
            {
                this.emailSchedulerInstance = value;
            }
        }

        /// <inheritdoc />
        public int KeyID
        {
            get
            {
                return this.MessageID;
            }

            set
            {
                this.MessageID = value;
            }
        }

        public Message GetReplyMessage()
        {
			Logger.LogDebug(">>> Entering GetReplyMessage()");
			Logger.LogDebug("<<< Exiting GetReplyMessage()");
            return new Message
            {
                AllowReply = this.AllowReply,
                Body = $"<br><br><br>On {this.MessageDate} {this.FromUserName} wrote {this.Body}",
                Conversation = this.Conversation,
                FromUserID = this.ToUserID,
                ToUserID = this.FromUserID,
                ToUserName = this.FromUserName,
                PortalID = this.PortalID,
                ReplyTo = this.MessageID,
                SkipInbox = this.SkipInbox,
                Subject = "RE:" + this.Subject,
            };
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.MessageID = Null.SetNullInteger(dr["MessageID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
			Logger.LogDebug("Fill(dr={Dr}): PortalID → {PortalID}", dr, PortalID);
			Logger.LogDebug("Fill(dr={Dr}): ToUserID → {ToUserID}", dr, ToUserID);
            this.FromUserID = Null.SetNullInteger(dr["FromUserID"]);
			Logger.LogDebug("Fill(dr={Dr}): FromUserID → {FromUserID}", dr, FromUserID);
            this.FromUserName = Null.SetNullString(dr["FromUserName"]);
            this.ToUserID = Null.SetNullInteger(dr["ToUserID"]);

            ////'_ToUserName = Null.SetNullString(dr.Item("ToUserName"))
            this.ReplyTo = Null.SetNullInteger(dr["ReplyTo"]);
			Logger.LogDebug("Fill(dr={Dr}): ReplyTo → {ReplyTo}", dr, ReplyTo);
			Logger.LogDebug("Fill(dr={Dr}): Body → {Body}", dr, Body);
			Logger.LogDebug("Fill(dr={Dr}): Subject → {Subject}", dr, Subject);
			Logger.LogDebug("Fill(dr={Dr}): AllowReply → {AllowReply}", dr, AllowReply);
			Logger.LogDebug("Fill(dr={Dr}): SkipInbox → {SkipInbox}", dr, SkipInbox);
			Logger.LogDebug("Fill(dr={Dr}): ToUserName → {ToUserName}", dr, ToUserName);
            this.Status = (MessageStatusType)Enum.Parse(typeof(MessageStatusType), Null.SetNullString(dr["Status"]));
            this.Body = Null.SetNullString(dr["Body"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.MessageDate = Null.SetNullDateTime(dr["Date"]);
            this.ToRoleID = Null.SetNullInteger(dr["ToRoleID"]);
            this.AllowReply = Null.SetNullBoolean(dr["AllowReply"]);
            this.SkipInbox = Null.SetNullBoolean(dr["SkipPortal"]);
            this.EmailSent = Null.SetNullBoolean(dr["EmailSent"]);
            this.ToUserName = Null.SetNullString(dr["ToUserName"]);
            string g = Null.SetNullString(dr["Conversation"]);
            this.EmailSentDate = Null.SetNullDateTime(dr["EmailSentDate"]);
			Logger.LogDebug("Fill(dr={Dr}): Conversation → {Conversation}", dr, Conversation);
            this.EmailSchedulerInstance = Null.SetNullGuid(dr["EmailSchedulerInstance"]);
            this.Conversation = Null.SetNullGuid(dr["Conversation"]);

            ////'Conversation = New Guid(g)
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
