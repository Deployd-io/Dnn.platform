// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    using Localize = DotNetNuke.Services.Localization.Localization;

    /// <summary>A <see cref="MailProvider"/> implementation using the original core DNN logic (via <see cref="SmtpClient"/>).</summary>
    public class CoreMailProvider : MailProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreMailProvider));
        private static readonly Regex SmtpServerRegex = new Regex("^[^:]+(:[0-9]{1,5})?$", RegexOptions.Compiled);
        private readonly IMailSettings mailSettings;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="CoreMailProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IMailSettings. Scheduled removal in v12.0.0.")]
        public CoreMailProvider()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering CoreMailProvider()");
		Logger.LogDebug("<<< Exiting CoreMailProvider()");
		Logger.LogDebug(">>> Entering CoreMailProvider(mailSettings={MailSettings},hostSettings={HostSettings})", mailSettings, hostSettings);
		Logger.LogDebug("CoreMailProvider(mailSettings={MailSettings},hostSettings={HostSettings}): hostSettings → {HostSettings1}", mailSettings, hostSettings, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="CoreMailProvider"/> class.</summary>
        /// <param name="mailSettings">The mail settings.</param>
        /// <param name="hostSettings">The host settings.</param>
        public CoreMailProvider(IMailSettings mailSettings, IHostSettings hostSettings)
        {
            this.mailSettings = mailSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IMailSettings>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("<<< Exiting CoreMailProvider(mailSettings={MailSettings},hostSettings={HostSettings})", mailSettings, hostSettings);
        }

        private static int CurrentPortalId => PortalSettings.Current?.PortalId ?? Null.NullInteger;

        private static string CurrentPortalName => PortalSettings.Current?.PortalName ?? string.Empty;

        /// <inheritdoc />
        public override string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null)
        {
			Logger.LogDebug(">>> Entering SendMail(mailInfo={MailInfo},smtpInfo=<redacted>)", mailInfo);
            var (host, port, errorMessage) = ParseSmtpServer(this.mailSettings, ref smtpInfo);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            using var mailMessage = CreateMailMessage(this.mailSettings, this.hostSettings, mailInfo, smtpInfo);
			Logger.LogDebug("SendMail(mailInfo={MailInfo},smtpInfo=<redacted>): mailMessage → {MailMessage}", mailInfo, mailMessage);
            try
            {
                using var smtpClient = CreateSmtpClient(smtpInfo, host, port);
				Logger.LogDebug("SendMail(mailInfo={MailInfo},smtpInfo=<redacted>): smtpClient → {SmtpClient}", mailInfo, smtpClient);
                smtpClient.Send(mailMessage);

                return string.Empty;
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in SendMail(mailInfo={MailInfo},smtpInfo=<redacted>): {Arg2}", mailInfo, exc.getMessage());
				Logger.LogDebug("<<< Exiting SendMail(mailInfo={MailInfo},smtpInfo=<redacted>)", mailInfo);
                return HandleException(exc);
            }
        }

        /// <inheritdoc />
        public override async Task<string> SendMailAsync(MailInfo mailInfo, SmtpInfo smtpInfo = null, CancellationToken cancellationToken = default(CancellationToken))
        {
			Logger.LogDebug(">>> Entering SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>)", mailInfo);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var (host, port, errorMessage) = ParseSmtpServer(this.mailSettings, ref smtpInfo);
            if (errorMessage != null)
            {
                return errorMessage;
            }

            using var mailMessage = CreateMailMessage(this.mailSettings, this.hostSettings, mailInfo, smtpInfo);
			Logger.LogDebug("SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>): mailMessage → {MailMessage}", mailInfo, mailMessage);
            try
            {
                using var smtpClient = CreateSmtpClient(smtpInfo, host, port);
				Logger.LogDebug("SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>): smtpClient → {SmtpClient}", mailInfo, smtpClient);
                await smtpClient.SendMailAsync(mailMessage);

                return string.Empty;
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>): {Arg3}", mailInfo, exc.getMessage());
				Logger.LogInformation("SendMailAsync(mailInfo,smtpInfo,cancellationToken)={MailInfo},{SmtpInfo},{CancellationToken}: external service call took {Arg3} ms", mailInfo, smtpInfo, cancellationToken, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>)", mailInfo);
                return HandleException(exc);
            }
        }

        /// <summary>Adds alternate views to the <paramref name="mailMessage"/>.</summary>
        /// <param name="mailMessage">The message to which the alternate views should be added.</param>
        /// <param name="body">The message body.</param>
        /// <param name="bodyEncoding">The encoding of the message body.</param>
        internal static void AddAlternateView(MailMessage mailMessage, string body, Encoding bodyEncoding)
        {
			Logger.LogDebug(">>> Entering AddAlternateView(mailMessage={MailMessage},body={Body},bodyEncoding={BodyEncoding})", mailMessage, body, bodyEncoding);
            // added support for multipart HTML messages
            // add text part as alternate view
            var plainView = AlternateView.CreateAlternateViewFromString(Mail.ConvertToText(body), bodyEncoding, "text/plain");
			Logger.LogDebug("AddAlternateView(mailMessage={MailMessage},body={Body},bodyEncoding={BodyEncoding}): plainView → {PlainView}", mailMessage, body, bodyEncoding, plainView);
            mailMessage.AlternateViews.Add(plainView);
            if (mailMessage.IsBodyHtml)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(body, bodyEncoding, "text/html");
				Logger.LogDebug("AddAlternateView(mailMessage={MailMessage},body={Body},bodyEncoding={BodyEncoding}): htmlView → {HtmlView}", mailMessage, body, bodyEncoding, htmlView);
                mailMessage.AlternateViews.Add(htmlView);
            }
			Logger.LogDebug("<<< Exiting AddAlternateView(mailMessage={MailMessage},body={Body},bodyEncoding={BodyEncoding})", mailMessage, body, bodyEncoding);
        }

        private static string ValidateSmtpInfo(IMailSettings mailSettings, SmtpInfo smtpInfo)
        {
			Logger.LogDebug(">>> Entering ValidateSmtpInfo(mailSettings={MailSettings},smtpInfo=<redacted>)", mailSettings);
            if (smtpInfo != null && !string.IsNullOrEmpty(smtpInfo.Server))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(mailSettings.GetServer(CurrentPortalId)))
            {
			Logger.LogDebug("<<< Exiting ValidateSmtpInfo(mailSettings={MailSettings},smtpInfo=<redacted>)", mailSettings);
                return null;
            }

            return "SMTP Server not configured";
        }

        private static SmtpInfo GetDefaultSmtpInfo(IMailSettings mailSettings, SmtpInfo smtpInfo)
        {
			Logger.LogDebug(">>> Entering GetDefaultSmtpInfo(mailSettings={MailSettings},smtpInfo=<redacted>)", mailSettings);
            if (smtpInfo != null && !string.IsNullOrEmpty(smtpInfo.Server))
            {
			Logger.LogDebug("<<< Exiting GetDefaultSmtpInfo(mailSettings={MailSettings},smtpInfo=<redacted>)", mailSettings);
                return smtpInfo;
            }

            return new SmtpInfo
                   {
                       Server = mailSettings.GetServer(CurrentPortalId),
                       Authentication = mailSettings.GetAuthentication(CurrentPortalId),
                       Username = mailSettings.GetUsername(CurrentPortalId),
                       Password = mailSettings.GetPassword(CurrentPortalId),
                       EnableSSL = mailSettings.GetSecureConnectionEnabled(CurrentPortalId),
                       AuthProvider = mailSettings.GetAuthProvider(CurrentPortalId),
                   };
        }

        private static MailMessage CreateMailMessage(IMailSettings mailSettings, IHostSettings hostSettings, MailInfo mailInfo, SmtpInfo smtpInfo)
        {
			Logger.LogDebug(">>> Entering CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>)", mailSettings, hostSettings, mailInfo);
            // translate semi-colon delimiters to commas as ASP.NET 2.0 does not support semi-colons
            if (!string.IsNullOrEmpty(mailInfo.To))
            {
                mailInfo.To = mailInfo.To.Replace(";", ",");
            }

            var mailMessage = new MailMessage(mailInfo.From, mailInfo.To);
			Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): mailMessage → {MailMessage}", mailSettings, hostSettings, mailInfo, mailMessage);

            if (!string.IsNullOrEmpty(mailInfo.Sender))
            {
                mailMessage.Sender = new MailAddress(mailInfo.Sender);
            }

            if (!string.IsNullOrEmpty(mailInfo.CC))
            {
                mailInfo.CC = mailInfo.CC.Replace(";", ",");
                mailMessage.CC.Add(mailInfo.CC);
            }

            if (!string.IsNullOrEmpty(mailInfo.BCC))
            {
                mailInfo.BCC = mailInfo.BCC.Replace(";", ",");
                mailMessage.Bcc.Add(mailInfo.BCC);
            }

            if (!string.IsNullOrEmpty(mailInfo.ReplyTo))
            {
                mailInfo.ReplyTo = mailInfo.ReplyTo.Replace(";", ",");
                mailMessage.ReplyToList.Add(mailInfo.ReplyTo);
            }

            mailMessage.Priority = (System.Net.Mail.MailPriority)mailInfo.Priority;
            mailMessage.IsBodyHtml = mailInfo.BodyFormat == MailFormat.Html;

            // Only modify senderAddress if smtpAuthentication is enabled
            // Can be "0", empty or Null - anonymous, "1" - basic, "2" - NTLM.
            if (smtpInfo.Authentication is "1" or "2")
            {
                // if the senderAddress is the email address of the Host then switch it smtpUsername if different
                // if display name of senderAddress is empty, then use Host.HostTitle for it
                if (mailMessage.Sender != null)
                {
                    var senderAddress = mailInfo.Sender;
					Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): senderAddress → {SenderAddress}", mailSettings, hostSettings, mailInfo, senderAddress);
                    var senderDisplayName = mailInfo.FromName;
					Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): senderDisplayName → {SenderDisplayName}", mailSettings, hostSettings, mailInfo, senderDisplayName);
                    var needUpdateSender = false;
					Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): needUpdateSender → {NeedUpdateSender}", mailSettings, hostSettings, mailInfo, needUpdateSender);
                    if (smtpInfo.Username.Contains("@", StringComparison.Ordinal)
                        && senderAddress == hostSettings.HostEmail
                        && !senderAddress.Equals(smtpInfo.Username, StringComparison.OrdinalIgnoreCase))
                    {
                        senderAddress = smtpInfo.Username;
                        needUpdateSender = true;
                    }

                    if (string.IsNullOrEmpty(senderDisplayName))
                    {
                        senderDisplayName = mailSettings.IsPortalEnabled(CurrentPortalId) ? CurrentPortalName : hostSettings.HostTitle;
                        needUpdateSender = true;
                    }

                    if (needUpdateSender)
                    {
                        mailMessage.Sender = new MailAddress(senderAddress, senderDisplayName);
                    }
                }
                else if (smtpInfo.Username.Contains("@"))
                {
                    mailMessage.Sender = new MailAddress(
                        smtpInfo.Username,
                        mailSettings.IsPortalEnabled(CurrentPortalId) ? CurrentPortalName : hostSettings.HostTitle);
                }
            }

            // attachments
            if (mailInfo.Attachments != null)
            {
                foreach (var attachment in mailInfo.Attachments.Where(attachment => attachment.Content != null))
                {
                    mailMessage.Attachments.Add(
                        new Attachment(
                            new MemoryStream(attachment.Content, writable: false),
                            attachment.Filename,
                            attachment.ContentType));
                }
            }

            // message
            mailMessage.SubjectEncoding = mailInfo.BodyEncoding;
			Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): mailMessage.SubjectEncoding → {Arg4}", mailSettings, hostSettings, mailInfo, mailMessage.SubjectEncoding);
            mailMessage.Subject = HtmlUtils.StripWhiteSpace(mailInfo.Subject, true);
            mailMessage.BodyEncoding = mailInfo.BodyEncoding;
			Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): mailMessage.BodyEncoding → {Arg4}", mailSettings, hostSettings, mailInfo, mailMessage.BodyEncoding);
			Logger.LogDebug("CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>): mailMessage.Body → {Arg4}", mailSettings, hostSettings, mailInfo, mailMessage.Body);
            mailMessage.Body = mailInfo.Body;

            AddAlternateView(mailMessage, mailInfo.Body, mailInfo.BodyEncoding);

			Logger.LogDebug("<<< Exiting CreateMailMessage(mailSettings={MailSettings},hostSettings={HostSettings},mailInfo={MailInfo},smtpInfo=<redacted>)", mailSettings, hostSettings, mailInfo);
            return mailMessage;
        }

        private static (string Host, int? Port, string ErrorMessage) ParseSmtpServer(IMailSettings mailSettings, ref SmtpInfo smtpInfo)
        {
			Logger.LogDebug(">>> Entering ParseSmtpServer(Host={Host},Port={Port},ErrorMessage={ErrorMessage})", Host, Port, ErrorMessage);
            var errorMessage = ValidateSmtpInfo(mailSettings, smtpInfo);
            if (errorMessage != null)
			Logger.LogDebug("ParseSmtpServer(Host={Host},Port={Port},ErrorMessage={ErrorMessage}): errorMessage → {ErrorMessage1}", Host, Port, ErrorMessage, errorMessage);
            {
                return (null, null, errorMessage);
            }

            smtpInfo = GetDefaultSmtpInfo(mailSettings, smtpInfo);

			Logger.LogDebug("ParseSmtpServer(Host={Host},Port={Port},ErrorMessage={ErrorMessage}): smtpInfo → {SmtpInfo}", Host, Port, ErrorMessage, smtpInfo);
            smtpInfo.Server = smtpInfo.Server.Trim();
            if (!SmtpServerRegex.IsMatch(smtpInfo.Server))
            {
                return (null, null, Localize.GetString("SMTPConfigurationProblem"));
            }

            var smtpHostParts = smtpInfo.Server.Split(':');
			Logger.LogDebug("ParseSmtpServer(Host={Host},Port={Port},ErrorMessage={ErrorMessage}): smtpHostParts → {SmtpHostParts}", Host, Port, ErrorMessage, smtpHostParts);
            var host = smtpHostParts[0];
            if (smtpHostParts.Length <= 1)
            {
                return (host, null, null);
            }

            // port is guaranteed to be of max 5 digits numeric by the RegEx check
            var port = int.Parse(smtpHostParts[1], CultureInfo.InvariantCulture);
			Logger.LogDebug("ParseSmtpServer(Host={Host},Port={Port},ErrorMessage={ErrorMessage}): port → {Port1}", Host, Port, ErrorMessage, port);
            if (port is < 1 or > 65535)
            {
			Logger.LogDebug("<<< Exiting ParseSmtpServer(Host={Host},Port={Port},ErrorMessage={ErrorMessage})", Host, Port, ErrorMessage);
                return (null, null, Localize.GetString("SmtpInvalidPort"));
            }

            return (host, port, null);
        }

        private static SmtpClient CreateSmtpClient(SmtpInfo smtpInfo, string host, int? port)
        {
			Logger.LogDebug(">>> Entering CreateSmtpClient(smtpInfo=<redacted>,host={Host},port={Port})", host, port);
            SmtpClient client = null;
            try
			Logger.LogDebug("CreateSmtpClient(smtpInfo=<redacted>,host={Host},port={Port}): client → {Client}", host, port, client);
            {
                client = new SmtpClient();
                client.Host = host;
				Logger.LogDebug("CreateSmtpClient(smtpInfo=<redacted>,host={Host},port={Port}): client.Host → {Arg3}", host, port, client.Host);
                if (port != null)
                {
                    client.Port = port.Value;
					Logger.LogDebug("CreateSmtpClient(smtpInfo=<redacted>,host={Host},port={Port}): client.Port → {Arg3}", host, port, client.Port);
                }

                SetSmtpClientAuthentication(smtpInfo, client);

                client.EnableSsl = smtpInfo.EnableSSL;
				Logger.LogDebug("CreateSmtpClient(smtpInfo=<redacted>,host={Host},port={Port}): client.EnableSsl → {Arg3}", host, port, client.EnableSsl);

                var returnedClient = client;
                client = null;

				Logger.LogDebug("<<< Exiting CreateSmtpClient(smtpInfo=<redacted>,host={Host},port={Port})", host, port);
                return returnedClient;
            }
            finally
            {
                client?.Dispose();
            }
        }

        private static void SetSmtpClientAuthentication(SmtpInfo smtpInfo, SmtpClient smtpClient)
        {
			Logger.LogDebug(">>> Entering SetSmtpClientAuthentication(smtpInfo=<redacted>,smtpClient={SmtpClient})", smtpClient);
            switch (smtpInfo.Authentication)
            {
                case "":
                case "0": // anonymous
                    break;
                case "1": // basic
                    if (!string.IsNullOrEmpty(smtpInfo.Username) && !string.IsNullOrEmpty(smtpInfo.Password))
                    {
                        smtpClient.UseDefaultCredentials = false;
						Logger.LogDebug("SetSmtpClientAuthentication(smtpInfo=<redacted>,smtpClient={SmtpClient}): smtpClient.UseDefaultCredentials → {Arg2}", smtpClient, smtpClient.UseDefaultCredentials);
                        smtpClient.Credentials = new NetworkCredential(smtpInfo.Username, smtpInfo.Password);
                    }

                    break;
                case "2": // NTLM
                    smtpClient.UseDefaultCredentials = true;
                    break;
            }
			Logger.LogDebug("<<< Exiting SetSmtpClientAuthentication(smtpInfo=<redacted>,smtpClient={SmtpClient})", smtpClient);
        }

        private static string HandleException(Exception exc)
        {
			Logger.LogDebug(">>> Entering HandleException(exc={Exc})", exc);
            var retValue = Localize.GetString("SMTPConfigurationProblem") + " ";

			Logger.LogDebug("HandleException(exc={Exc}): retValue → {RetValue}", exc, retValue);
            // mail configuration problem
            if (exc.InnerException != null)
            {
                retValue += string.Concat(exc.Message, Environment.NewLine, exc.InnerException.Message);
                Exceptions.Exceptions.LogException(exc.InnerException);
            }
            else
            {
                retValue += exc.Message;
                Exceptions.Exceptions.LogException(exc);
            }

			Logger.LogDebug("<<< Exiting HandleException(exc={Exc})", exc);
            return retValue;
        }
    }
}
