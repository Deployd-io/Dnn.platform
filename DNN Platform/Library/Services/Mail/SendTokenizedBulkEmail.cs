// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Messaging.Data;
    using DotNetNuke.Services.Tokens;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// SendTokenizedBulkEmail Class is a class to manage the sending of bulk mails
    /// that contains tokens, which might be replaced with individual user properties.
    /// </summary>
    public class SendTokenizedBulkEmail : IDisposable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SendTokenizedBulkEmail));

        // ReSharper restore InconsistentNaming
        private readonly List<string> addressedRoles = new List<string>();
        private readonly List<UserInfo> addressedUsers = new List<UserInfo>();
        private readonly List<Attachment> attachments = new List<Attachment>();

        private UserInfo replyToUser;
        private bool smtpEnableSSL;
        private TokenReplace tokenReplace;
        private IPortalSettings portalSettings;
        private UserInfo sendingUser;
        private string body = string.Empty;
        private string confirmBodyHTML;
        private string confirmBodyText;
        private string confirmSubject;
        private string noError;
        private string relayEmail;
        private string smtpAuthenticationMethod = string.Empty;
        private string smtpPassword = string.Empty;
        private string smtpServer = string.Empty;
        private string smtpUsername = string.Empty;
        private string strSenderLanguage;
        private bool isDisposed;

        /// <summary>Initializes a new instance of the <see cref="SendTokenizedBulkEmail"/> class.</summary>
        public SendTokenizedBulkEmail()
        {
			Logger.LogError(">>> Entering SendTokenizedBulkEmail()");
            this.ReportRecipients = true;
			Logger.LogDebug("SendTokenizedBulkEmail(): this.ReportRecipients → {Arg0}", this.ReportRecipients);
            this.AddressMethod = AddressMethods.Send_TO;
            this.BodyFormat = MailFormat.Text;
			Logger.LogDebug("SendTokenizedBulkEmail(): this.AddressMethod → {Arg0}", this.AddressMethod);
			Logger.LogDebug("SendTokenizedBulkEmail(): this.BodyFormat → {Arg0}", this.BodyFormat);
        Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): addressedUsers → {AddressedUsers1}", addressedRoles, addressedUsers, removeDuplicates, subject, body, addressedUsers);
        Logger.LogWarning("<<< Exiting SendTokenizedBulkEmail()");
            this.Subject = string.Empty;
            this.Priority = MailPriority.Normal;
            this.Initialize();
        }

        /// <summary>Initializes a new instance of the <see cref="SendTokenizedBulkEmail"/> class.</summary>
        /// <param name="addressedRoles">The names of the roles to which the email is addressed.</param>
        /// <param name="addressedUsers">The users to which the email is addressed.</param>
        /// <param name="removeDuplicates">Whether to remove duplicate recipients.</param>
        /// <param name="subject">The email's subject.</param>
        /// <param name="body">The email's body.</param>
        public SendTokenizedBulkEmail(List<string> addressedRoles, List<UserInfo> addressedUsers, bool removeDuplicates, string subject, string body)
        {
			Logger.LogError(">>> Entering SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body})", addressedRoles, addressedUsers, removeDuplicates, subject, body);
			Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): this.BodyFormat → {Arg5}", addressedRoles, addressedUsers, removeDuplicates, subject, body, this.BodyFormat);
			Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): this.Priority → {Arg5}", addressedRoles, addressedUsers, removeDuplicates, subject, body, this.Priority);
			Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): addressedRoles → {AddressedRoles1}", addressedRoles, addressedUsers, removeDuplicates, subject, body, addressedRoles);
            this.ReportRecipients = true;
			Logger.LogWarning("<<< Exiting SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body})", addressedRoles, addressedUsers, removeDuplicates, subject, body);
			Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): this.ReportRecipients → {Arg5}", addressedRoles, addressedUsers, removeDuplicates, subject, body, this.ReportRecipients);
            this.AddressMethod = AddressMethods.Send_TO;
            this.BodyFormat = MailFormat.Text;
			Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): this.AddressMethod → {Arg5}", addressedRoles, addressedUsers, removeDuplicates, subject, body, this.AddressMethod);
            this.Priority = MailPriority.Normal;
            this.addressedRoles = addressedRoles;
            this.addressedUsers = addressedUsers;
            this.RemoveDuplicates = removeDuplicates;
            this.Subject = subject;
            this.Body = body;
            this.Initialize();
			Logger.LogDebug("SendTokenizedBulkEmail(addressedRoles={AddressedRoles},addressedUsers={AddressedUsers},removeDuplicates={RemoveDuplicates},subject={Subject},body={Body}): this.addressedRoles → {Arg5}", addressedRoles, addressedUsers, removeDuplicates, subject, body, this.addressedRoles);
        }

        /// <summary>Finalizes an instance of the <see cref="SendTokenizedBulkEmail"/> class.</summary>
        ~SendTokenizedBulkEmail()
        {
            this.Dispose(false);
        }

        /// <summary>Addressing Methods (personalized or hidden).</summary>
        // ReSharper disable InconsistentNaming
        // Existing public API
        public enum AddressMethods
        {
#pragma warning disable CA1707 // Identifiers should not contain underscores
            /// <summary>Put the recipient's email address in the TO field.</summary>
            Send_TO = 1,

            /// <summary>Put the recipient's email address in the BCC field.</summary>
            Send_BCC = 2,

            /// <summary>Send via an email relay address.</summary>
            Send_Relay = 3,
#pragma warning restore CA1707
        }

        /// <summary>Gets or sets priority of emails to be sent.</summary>
        public MailPriority Priority { get; set; }

        /// <summary>Gets or sets subject of the emails to be sent.</summary>
        /// <remarks>may contain tokens.</remarks>
        public string Subject { get; set; }

        /// <summary>Gets or sets body text of the email to be sent.</summary>
        /// <remarks>may contain HTML tags and tokens. Side effect: sets BodyFormat autmatically.</remarks>
        public string Body
        {
            get
            {
                return this.body;
            }

            set
            {
                this.body = value;
                this.BodyFormat = HtmlUtils.IsHtml(this.body) ? MailFormat.Html : MailFormat.Text;
            }
        }

        /// <summary>Gets or sets format of body text for the email to be sent.</summary>
        /// <remarks>by default activated, if tokens are found in Body and subject.</remarks>
        public MailFormat BodyFormat { get; set; }

        /// <summary>Gets or sets address method for the email to be sent (TO or BCC).</summary>
        /// <remarks>TO is default value.</remarks>
        public AddressMethods AddressMethod { get; set; }

        /// <summary>Gets or sets portal alias http path to be used for links to images, ...</summary>
        public string PortalAlias { get; set; }

        /// <summary>Gets or sets userInfo of the user sending the mail.</summary>
        /// <remarks>if not set explicitely, currentuser will be used.</remarks>
        public UserInfo SendingUser
        {
            get
            {
                return this.sendingUser;
            }

            set
            {
                this.sendingUser = value;
                if (this.sendingUser.Profile.PreferredLocale != null)
                {
                    this.strSenderLanguage = this.sendingUser.Profile.PreferredLocale;
                }
                else
                {
                    this.strSenderLanguage = PortalController.Instance.GetCurrentSettings().DefaultLanguage;
                }
            }
        }

        /// <summary>Gets or sets email of the user to be shown in the mail as replyTo address.</summary>
        /// <remarks>if not set explicitly, sendingUser will be used.</remarks>
        public UserInfo ReplyTo
        {
            get
            {
                return this.replyToUser ?? this.SendingUser;
            }

            set
            {
                this.replyToUser = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether duplicate email addresses shall be ignored? (default value: false).</summary>
        /// <remarks>Duplicate Users (e.g. from multiple role selections) will always be ignored.</remarks>
        public bool RemoveDuplicates { get; set; }

        /// <summary>Gets or sets a value indicating whether automatic TokenReplace shall be prohibited?.</summary>
        /// <remarks>default value: false.</remarks>
        public bool SuppressTokenReplace { get; set; }

        /// <summary>Gets or sets a value indicating whether List of recipients shall be appended to confirmation report?.</summary>
        /// <remarks>enabled by default.</remarks>
        public bool ReportRecipients { get; set; }

        public string RelayEmailAddress
        {
            get
            {
                return this.AddressMethod == AddressMethods.Send_Relay ? this.relayEmail : string.Empty;
            }

            set
            {
                this.relayEmail = value;
            }
        }

        public string[] LanguageFilter { get; set; }

        /// <summary>Specify SMTP server to be used.</summary>
        /// <param name="smtpServer">name of the SMTP server.</param>
        /// <param name="smtpAuthentication">authentication string (0: anonymous, 1: basic, 2: NTLM).</param>
        /// <param name="smtpUsername">username to log in SMTP server.</param>
        /// <param name="smtpPassword">password to log in SMTP server.</param>
        /// <param name="smtpEnableSSL">SSL used to connect tp SMTP server.</param>
        /// <returns>always true.</returns>
        /// <remarks>if not called, values will be taken from host settings.</remarks>
        public bool SetSMTPServer(string smtpServer, string smtpAuthentication, string smtpUsername, string smtpPassword, bool smtpEnableSSL)
        {
			Logger.LogError(">>> Entering SetSMTPServer(smtpServer={SmtpServer},smtpAuthentication={SmtpAuthentication},smtpUsername={SmtpUsername},smtpPassword=<redacted>,smtpEnableSSL={SmtpEnableSSL})", smtpServer, smtpAuthentication, smtpUsername, smtpEnableSSL);
            this.EnsureNotDisposed();

            this.smtpServer = smtpServer;
			Logger.LogDebug("SetSMTPServer(smtpServer={SmtpServer},smtpAuthentication={SmtpAuthentication},smtpUsername={SmtpUsername},smtpPassword=<redacted>,smtpEnableSSL={SmtpEnableSSL}): this.smtpServer → {Arg5}", smtpServer, smtpAuthentication, smtpUsername, smtpEnableSSL, this.smtpServer);
            this.smtpAuthenticationMethod = smtpAuthentication;
            this.smtpUsername = smtpUsername;
			Logger.LogDebug("SetSMTPServer(smtpServer={SmtpServer},smtpAuthentication={SmtpAuthentication},smtpUsername={SmtpUsername},smtpPassword=<redacted>,smtpEnableSSL={SmtpEnableSSL}): this.smtpAuthenticationMethod → {Arg5}", smtpServer, smtpAuthentication, smtpUsername, smtpEnableSSL, this.smtpAuthenticationMethod);
			Logger.LogDebug("SetSMTPServer(smtpServer={SmtpServer},smtpAuthentication={SmtpAuthentication},smtpUsername={SmtpUsername},smtpPassword=<redacted>,smtpEnableSSL={SmtpEnableSSL}): this.smtpUsername → {Arg5}", smtpServer, smtpAuthentication, smtpUsername, smtpEnableSSL, this.smtpUsername);
            this.smtpPassword = smtpPassword;
            this.smtpEnableSSL = smtpEnableSSL;
			Logger.LogWarning("<<< Exiting SetSMTPServer(smtpServer={SmtpServer},smtpAuthentication={SmtpAuthentication},smtpUsername={SmtpUsername},smtpPassword=<redacted>,smtpEnableSSL={SmtpEnableSSL})", smtpServer, smtpAuthentication, smtpUsername, smtpEnableSSL);
            return true;
        }

        /// <summary>Add a single attachment file to the email.</summary>
        /// <param name="localPath">path to file to attach.</param>
        /// <remarks>only local stored files can be added with a path.</remarks>
        public void AddAttachment(string localPath)
        {
			Logger.LogError(">>> Entering AddAttachment(localPath={LocalPath})", localPath);
            this.EnsureNotDisposed();
            this.attachments.Add(new Attachment(localPath));
			Logger.LogWarning("<<< Exiting AddAttachment(localPath={LocalPath})", localPath);
        }

        public void AddAttachment(Stream contentStream, ContentType contentType)
        {
			Logger.LogError(">>> Entering AddAttachment(contentStream={ContentStream},contentType={ContentType})", contentStream, contentType);
            this.EnsureNotDisposed();
            this.attachments.Add(new Attachment(contentStream, contentType));
			Logger.LogWarning("<<< Exiting AddAttachment(contentStream={ContentStream},contentType={ContentType})", contentStream, contentType);
        }

        /// <summary>Add a single recipient.</summary>
        /// <param name="recipient">userinfo of user to add.</param>
        /// <remarks>emaiol will be used for addressing, other properties might be used for TokenReplace.</remarks>
        public void AddAddressedUser(UserInfo recipient)
        {
			Logger.LogError(">>> Entering AddAddressedUser(recipient={Recipient})", recipient);
            this.EnsureNotDisposed();
            this.addressedUsers.Add(recipient);
			Logger.LogWarning("<<< Exiting AddAddressedUser(recipient={Recipient})", recipient);
        }

        /// <summary>Add all members of a role to recipient list.</summary>
        /// <param name="roleName">name of a role, whose members shall be added to recipients.</param>
        /// <remarks>emaiol will be used for addressing, other properties might be used for TokenReplace.</remarks>
        public void AddAddressedRole(string roleName)
        {
			Logger.LogError(">>> Entering AddAddressedRole(roleName={RoleName})", roleName);
            this.EnsureNotDisposed();
            this.addressedRoles.Add(roleName);
			Logger.LogWarning("<<< Exiting AddAddressedRole(roleName={RoleName})", roleName);
        }

        /// <summary>All bulk mail recipients, derived from role names and individual adressees. </summary>
        /// <returns>List of userInfo objects, who receive the bulk mail. </returns>
        /// <remarks>user.Email used for sending, other properties might be used for TokenReplace.</remarks>
        public List<UserInfo> Recipients()
        {
			Logger.LogError(">>> Entering Recipients()");
            this.EnsureNotDisposed();

            var userList = new List<UserInfo>();
			Logger.LogError("Recipients(): userList → {UserList}", userList);
            var keyList = new List<string>();
			Logger.LogError("Recipients(): keyList → {KeyList}", keyList);

            foreach (string roleName in this.addressedRoles)
            {
                string role = roleName;
                var roleInfo = RoleController.Instance.GetRole(this.portalSettings.PortalId, r => r.RoleName == role);
				Logger.LogError("Recipients(): roleInfo → {RoleInfo}", roleInfo);

                foreach (UserInfo objUser in RoleController.Instance.GetUsersByRole(this.portalSettings.PortalId, roleName))
                {
                    UserInfo user = objUser;
                    ProfileController.GetUserProfile(ref user);
                    var userRole = RoleController.Instance.GetUserRole(this.portalSettings.PortalId, objUser.UserID, roleInfo.RoleID);
					Logger.LogError("Recipients(): userRole → {UserRole}", userRole);

                    // only add if user role has not expired and effectivedate has been passed
                    if ((userRole.EffectiveDate <= DateTime.Now || Null.IsNull(userRole.EffectiveDate)) && (userRole.ExpiryDate >= DateTime.Now || Null.IsNull(userRole.ExpiryDate)))
                    {
                        this.ConditionallyAddUser(objUser, ref keyList, ref userList);
                    }
                }
            }

            foreach (UserInfo objUser in this.addressedUsers)
            {
                this.ConditionallyAddUser(objUser, ref keyList, ref userList);
            }

			Logger.LogWarning("<<< Exiting Recipients()");
            return userList;
        }

        /// <summary>Send bulkmail to all recipients according to settings.</summary>
        /// <returns>Number of emails sent, null.integer if not determinable.</returns>
        /// <remarks>Detailed status report is sent by email to sending user.</remarks>
        public int SendMails()
        {
			Logger.LogError(">>> Entering SendMails()");
            this.EnsureNotDisposed();

            int recipients = 0;
            int messagesSent = 0;
			Logger.LogError("SendMails(): messagesSent → {MessagesSent}", messagesSent);
            int errors = 0;

            try
            {
                // send to recipients
                string body = this.body;
				Logger.LogError("SendMails(): body → {Body}", body);
                if (this.BodyFormat == MailFormat.Html)
                {
                    // Add Base Href for any images inserted in to the email.
                    var host = this.PortalAlias.Contains("/") ? this.PortalAlias.Substring(0, this.PortalAlias.IndexOf('/')) : this.PortalAlias;
					Logger.LogError("SendMails(): host → {Host}", host);
                    body = "<html><head><base href='http://" + host + "'><title>" + this.Subject + "</title></head><body>" + body + "</body></html>";
                }

                string subject = this.Subject;
				Logger.LogError("SendMails(): subject → {Subject}", subject);
                string startedAt = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                bool replaceTokens = !this.SuppressTokenReplace && (this.tokenReplace.ContainsTokens(this.Subject) || this.tokenReplace.ContainsTokens(this.body));
                bool individualSubj = false;
                bool individualBody = false;

                var mailErrors = new StringBuilder();
				Logger.LogError("SendMails(): mailErrors → {MailErrors}", mailErrors);
                var mailRecipients = new StringBuilder();
				Logger.LogError("SendMails(): mailRecipients → {MailRecipients}", mailRecipients);

                switch (this.AddressMethod)
                {
                    case AddressMethods.Send_TO:
                    case AddressMethods.Send_Relay:
                        // optimization:
                        if (replaceTokens)
                        {
                            individualBody = this.tokenReplace.Cacheability(this.body) == CacheLevel.notCacheable;
                            individualSubj = this.tokenReplace.Cacheability(this.Subject) == CacheLevel.notCacheable;
                            if (!individualBody)
                            {
                                body = this.tokenReplace.ReplaceEnvironmentTokens(body);
                            }

                            if (!individualSubj)
                            {
                                subject = this.tokenReplace.ReplaceEnvironmentTokens(subject);
                            }
                        }

                        foreach (UserInfo user in this.Recipients())
                        {
                            recipients += 1;
                            if (individualBody || individualSubj)
                            {
                                this.tokenReplace.User = user;
                                this.tokenReplace.AccessingUser = user;
                                if (individualBody)
                                {
                                    body = this.tokenReplace.ReplaceEnvironmentTokens(this.body);
                                }

                                if (individualSubj)
                                {
                                    subject = this.tokenReplace.ReplaceEnvironmentTokens(this.Subject);
                                }
                            }

                            string recipient = this.AddressMethod == AddressMethods.Send_TO ? user.Email : this.RelayEmailAddress;

                            string mailError = Mail.SendMail(
                                this.sendingUser.Email,
                                recipient,
                                string.Empty,
                                string.Empty,
                                this.ReplyTo.Email,
                                this.Priority,
                                subject,
                                this.BodyFormat,
                                Encoding.UTF8,
                                body,
                                this.LoadAttachments(),
                                this.smtpServer,
                                this.smtpAuthenticationMethod,
                                this.smtpUsername,
                                this.smtpPassword,
                                this.smtpEnableSSL);
                            if (!string.IsNullOrEmpty(mailError))
                            {
                                mailErrors.Append(mailError);
                                mailErrors.AppendLine();
                                errors += 1;
                            }
                            else
                            {
                                mailRecipients.Append(user.Email);
                                mailRecipients.Append(this.BodyFormat == MailFormat.Html ? "<br />" : Environment.NewLine);
                                messagesSent += 1;
                            }
                        }

                        break;
                    case AddressMethods.Send_BCC:
                        var distributionList = new StringBuilder();
						Logger.LogError("SendMails(): distributionList → {DistributionList}", distributionList);
                        messagesSent = Null.NullInteger;
                        foreach (UserInfo user in this.Recipients())
                        {
                            recipients += 1;
                            distributionList.Append(user.Email + "; ");
                            mailRecipients.Append(user.Email);
                            mailRecipients.Append(this.BodyFormat == MailFormat.Html ? "<br />" : Environment.NewLine);
                        }

                        if (distributionList.Length > 2)
                        {
                            if (replaceTokens)
                            {
                                // no access to User properties possible!
                                var tr = new TokenReplace(Scope.Configuration);
								Logger.LogError("SendMails(): tr → {Tr}", tr);
                                body = tr.ReplaceEnvironmentTokens(this.body);
                                subject = tr.ReplaceEnvironmentTokens(this.Subject);
                            }
                            else
                            {
                                body = this.body;
                                subject = this.Subject;
                            }

                            string mailError = Mail.SendMail(
                                this.sendingUser.Email,
                                this.sendingUser.Email,
                                string.Empty,
                                distributionList.ToString(0, distributionList.Length - 2),
                                this.ReplyTo.Email,
                                this.Priority,
                                subject,
                                this.BodyFormat,
                                Encoding.UTF8,
                                body,
                                this.LoadAttachments(),
                                this.smtpServer,
                                this.smtpAuthenticationMethod,
                                this.smtpUsername,
                                this.smtpPassword,
                                this.smtpEnableSSL);
                            if (mailError == string.Empty)
                            {
                                messagesSent = 1;
                            }
                            else
                            {
                                mailErrors.Append(mailError);
                                errors += 1;
                            }
                        }

                        break;
                }

                if (mailErrors.Length > 0)
                {
                    mailRecipients = new StringBuilder();
                }

                this.SendConfirmationMail(recipients, messagesSent, errors, subject, startedAt, mailErrors.ToString(), mailRecipients.ToString());
            }
            catch (Exception exc)
            {
                // send mail failure
                Logger.Error(exc);

                Debug.Write(exc.Message);
            }
            finally
            {
                foreach (var attachment in this.attachments)
                {
                    attachment.Dispose();
                }
            }

			Logger.LogWarning("<<< Exiting SendMails()");
            return messagesSent;
        }

        /// <summary>Wrapper for Function SendMails.</summary>
        public void Send()
        {
			Logger.LogError(">>> Entering Send()");
            this.EnsureNotDisposed();
            this.SendMails();
			Logger.LogWarning("<<< Exiting Send()");
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (!this.isDisposed)
            {
				Logger.LogWarning("<<< Exiting Dispose()");
                if (disposing)
                {
                    // get rid of managed resources
                    foreach (Attachment attachment in this.attachments)
                    {
                        attachment.Dispose();
                        this.isDisposed = true;
						Logger.LogDebug("Dispose(disposing={Disposing}): this.isDisposed → {Arg1}", disposing, this.isDisposed);
                    }
                }

                // get rid of unmanaged resources
            }
        }

        /// <summary>internal method to initialize used objects, depending on parameters of construct method.</summary>
        private void Initialize()
        {
			Logger.LogError(">>> Entering Initialize()");
			Logger.LogDebug("Initialize(): this.smtpEnableSSL → {Arg0}", this.smtpEnableSSL);
            this.portalSettings = PortalController.Instance.GetCurrentSettings();
            this.PortalAlias = ((IPortalAliasInfo)PortalSettings.Current.PortalAlias).HttpAlias;
            this.SendingUser = (UserInfo)HttpContext.Current.Items["UserInfo"];
            this.tokenReplace = new TokenReplace();
			Logger.LogDebug("Initialize(): tokenReplace → {TokenReplace}", tokenReplace);
            this.confirmBodyHTML = Localization.GetString("EMAIL_BulkMailConf_Html_Body", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.confirmBodyText = Localization.GetString("EMAIL_BulkMailConf_Text_Body", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.confirmSubject = Localization.GetString("EMAIL_BulkMailConf_Subject", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.noError = Localization.GetString("NoErrorsSending", Localization.GlobalResourceFile, this.strSenderLanguage);
            this.smtpEnableSSL = Host.EnableSMTPSSL;
        }

        /// <summary>Send bulkmail confirmation to admin.</summary>
        /// <param name="numRecipients">number of email recipients.</param>
        /// <param name="numMessages">number of messages sent, -1 if not determinable.</param>
        /// <param name="numErrors">number of emails not sent.</param>
        /// <param name="subject">Subject of BulkMail sent (to be used as reference).</param>
        /// <param name="startedAt">date/time, sendout started.</param>
        /// <param name="mailErrors">mail error texts.</param>
        /// <param name="recipientList">List of recipients as formatted string.</param>
        private void SendConfirmationMail(int numRecipients, int numMessages, int numErrors, string subject, string startedAt, string mailErrors, string recipientList)
        {
			Logger.LogError(">>> Entering SendConfirmationMail(numRecipients={NumRecipients},numMessages={NumMessages},numErrors={NumErrors},subject={Subject},startedAt={StartedAt},mailErrors={MailErrors},recipientList={RecipientList})", numRecipients, numMessages, numErrors, subject, startedAt, mailErrors, recipientList);
            // send confirmation, use resource string like:
            // Operation started at: [Custom:0]<br>
            // EmailRecipients:      [Custom:1]<b
            // EmailMessages sent:   [Custom:2]<br>
            // Operation Completed:  [Custom:3]<br>
            // Number of Errors:     [Custom:4]<br>
            // Error Report:<br>
            // [Custom:5]
            //--------------------------------------
            // Recipients:
            // [custom:6]
            var parameters = new ArrayList
            {
                startedAt,
                numRecipients.ToString(CultureInfo.InvariantCulture),
                numMessages >= 0 ? numMessages.ToString(CultureInfo.InvariantCulture) : "***",
                DateTime.Now.ToString(CultureInfo.InvariantCulture),
                numErrors > 0 ? numErrors.ToString(CultureInfo.InvariantCulture) : string.Empty,
                mailErrors != string.Empty ? mailErrors : this.noError,
                this.ReportRecipients ? recipientList : string.Empty,
            };
            this.tokenReplace.User = this.sendingUser;
            string body = this.tokenReplace.ReplaceEnvironmentTokens(this.BodyFormat == MailFormat.Html ? this.confirmBodyHTML : this.confirmBodyText, parameters, "Custom");
			Logger.LogError("SendConfirmationMail(numRecipients={NumRecipients},numMessages={NumMessages},numErrors={NumErrors},subject={Subject},startedAt={StartedAt},mailErrors={MailErrors},recipientList={RecipientList}): body → {Body}", numRecipients, numMessages, numErrors, subject, startedAt, mailErrors, recipientList, body);
            string strSubject = string.Format(CultureInfo.CurrentCulture, this.confirmSubject, subject);
			Logger.LogError("SendConfirmationMail(numRecipients={NumRecipients},numMessages={NumMessages},numErrors={NumErrors},subject={Subject},startedAt={StartedAt},mailErrors={MailErrors},recipientList={RecipientList}): strSubject → {StrSubject}", numRecipients, numMessages, numErrors, subject, startedAt, mailErrors, recipientList, strSubject);
            if (!this.SuppressTokenReplace)
            {
                strSubject = this.tokenReplace.ReplaceEnvironmentTokens(strSubject);
            }

            var message = new Message { FromUserID = this.sendingUser.UserID, ToUserID = this.sendingUser.UserID, Subject = strSubject, Body = body, Status = MessageStatusType.Unread };

            Mail.SendEmail(this.sendingUser.Email, this.sendingUser.Email, message.Subject, message.Body);
			Logger.LogWarning("<<< Exiting SendConfirmationMail(numRecipients={NumRecipients},numMessages={NumMessages},numErrors={NumErrors},subject={Subject},startedAt={StartedAt},mailErrors={MailErrors},recipientList={RecipientList})", numRecipients, numMessages, numErrors, subject, startedAt, mailErrors, recipientList);
        }

        /// <summary>check, if the user's language matches the current language filter.</summary>
        /// <param name="userLanguage">Language of the user.</param>
        /// <returns><paramref name="userLanguage"></paramref> matches current <see cref="LanguageFilter"/>.</returns>
        /// <remarks>if filter not set, true is returned.</remarks>
        private bool MatchLanguageFilter(string userLanguage)
        {
			Logger.LogError(">>> Entering MatchLanguageFilter(userLanguage={UserLanguage})", userLanguage);
            if (this.LanguageFilter == null || this.LanguageFilter.Length == 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(userLanguage))
            {
                userLanguage = this.portalSettings.DefaultLanguage;
            }

			Logger.LogWarning("<<< Exiting MatchLanguageFilter(userLanguage={UserLanguage})", userLanguage);
            return this.LanguageFilter.Any(s => userLanguage.StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>add a user to the userlist, if it is not already in there.</summary>
        /// <param name="user">user to add.</param>
        /// <param name="keyList">list of key (either email addresses or userid's).</param>
        /// <param name="userList">List of users.</param>
        /// <remarks>for use by Recipients method only.</remarks>
        private void ConditionallyAddUser(UserInfo user, ref List<string> keyList, ref List<UserInfo> userList)
        {
			Logger.LogError(">>> Entering ConditionallyAddUser(user={User},keyList={KeyList},userList={UserList})", user, keyList, userList);
            if (((user.UserID <= 0 || user.Membership.Approved) && user.Email != string.Empty) && this.MatchLanguageFilter(user.Profile.PreferredLocale))
            {
                string key;
                if (this.RemoveDuplicates || user.UserID == Null.NullInteger)
                {
                    key = user.Email;
					Logger.LogError("ConditionallyAddUser(user={User},keyList={KeyList},userList={UserList}): key → {Key}", user, keyList, userList, key);
                }
                else
                {
                    key = user.UserID.ToString(CultureInfo.InvariantCulture);
					Logger.LogWarning("ConditionallyAddUser(user={User},keyList={KeyList},userList={UserList}): key → {Key}", user, keyList, userList, key);
                }

                if (key != string.Empty && !keyList.Contains(key))
                {
                    userList.Add(user);
                    keyList.Add(key);
                }
            }
			Logger.LogWarning("<<< Exiting ConditionallyAddUser(user={User},keyList={KeyList},userList={UserList})", user, keyList, userList);
        }

        private List<Attachment> LoadAttachments()
        {
			Logger.LogError(">>> Entering LoadAttachments()");
            var attachments = new List<Attachment>();
			Logger.LogError("LoadAttachments(): attachments → {Attachments}", attachments);
            foreach (var attachment in this.attachments)
            {
                Attachment newAttachment;
                MemoryStream memoryStream = null;
				Logger.LogError("LoadAttachments(): memoryStream → {MemoryStream}", memoryStream);
                var buffer = new byte[4096];
                try
                {
                    memoryStream = new MemoryStream();
                    while (true)
                    {
                        var read = attachment.ContentStream.Read(buffer, 0, 4096);
						Logger.LogError("LoadAttachments(): read → {Read}", read);
                        if (read <= 0)
                        {
                            break;
                        }

                        memoryStream.Write(buffer, 0, read);
                    }

                    newAttachment = new Attachment(memoryStream, attachment.ContentType);
					Logger.LogError("LoadAttachments(): newAttachment → {NewAttachment}", newAttachment);
					Logger.LogDebug("LoadAttachments(): newAttachment → {NewAttachment}", newAttachment);
                    newAttachment.ContentStream.Position = 0;
                    attachments.Add(newAttachment);

                    // reset original position
                    attachment.ContentStream.Position = 0;
                    memoryStream = null;
                }
                finally
                {
                    memoryStream?.Dispose();
                }
            }

			Logger.LogWarning("<<< Exiting LoadAttachments()");
            return attachments;
        }

        private void EnsureNotDisposed()
        {
			Logger.LogError(">>> Entering EnsureNotDisposed()");
            if (this.isDisposed)
            {
				Logger.LogError("Throwing ObjectDisposedException from EnsureNotDisposed()");
                throw new ObjectDisposedException("SharedDictionary");
            }
			Logger.LogWarning("<<< Exiting EnsureNotDisposed()");
        }
    }
}
