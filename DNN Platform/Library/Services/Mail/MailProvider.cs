// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail
{
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.ComponentModel;

    /// <summary>A provider with the ability to send emails.</summary>
    public abstract class MailProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MailProvider));
        /// <summary>Gets a value indicating whether this provider supports OAuth authentication to the SMTP server.</summary>
        public virtual bool SupportsOAuth => false;

        /// <summary>Gets the currently configured <see cref="MailProvider"/> instance.</summary>
        /// <returns>A <see cref="MailProvider"/> instance.</returns>
        public static MailProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
			Logger.LogDebug("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<MailProvider>();
        }

        /// <summary>Sends an email.</summary>
        /// <param name="mailInfo">Information about the message to send.</param>
        /// <param name="smtpInfo">Information about the SMTP server via which to send the message.</param>
        /// <returns><see cref="string.Empty"/> if the message send successfully, otherwise an error message.</returns>
        public abstract string SendMail(MailInfo mailInfo, SmtpInfo smtpInfo = null);

        /// <summary>Sends an email.</summary>
        /// <param name="mailInfo">Information about the message to send.</param>
        /// <param name="smtpInfo">Information about the SMTP server via which to send the message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="string.Empty"/> if the message send successfully, otherwise an error message.</returns>
        public virtual Task<string> SendMailAsync(MailInfo mailInfo, SmtpInfo smtpInfo = null, CancellationToken cancellationToken = default(CancellationToken))
        {
			Logger.LogDebug(">>> Entering SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>)", mailInfo);
			Logger.LogDebug("<<< Exiting SendMailAsync(mailInfo={MailInfo},smtpInfo=<redacted>,cancellationToken=<redacted>)", mailInfo);
            return Task.FromResult(this.SendMail(mailInfo, smtpInfo));
        }
    }
}
