// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Services.Dto;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Mail.OAuth;
    using DotNetNuke.Web.Api;

    /// <summary>Provides the APIs for SMTP settings management.</summary>
    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsSmtpHostController : PersonaBarApiController
    {
        private const string ObfuscateString = "*****";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsSmtpHostController));
        private readonly IHostSettingsService hostSettingsService;
        private readonly ISmtpOAuthController smtpOAuthController;

        /// <summary>Initializes a new instance of the <see cref="ServerSettingsSmtpHostController"/> class.</summary>
        /// <param name="hostSettingsService">A service to manage host settings.</param>
        /// <param name="smtpOAuthController">A controller for SMTP OAuth providers.</param>
        public ServerSettingsSmtpHostController(
            IHostSettingsService hostSettingsService,
            ISmtpOAuthController smtpOAuthController)
        {
			Logger.LogDebug(">>> Entering ServerSettingsSmtpHostController(hostSettingsService={HostSettingsService},smtpOAuthController={SmtpOAuthController})", hostSettingsService, smtpOAuthController);
            this.hostSettingsService = hostSettingsService;
            this.smtpOAuthController = smtpOAuthController;
			Logger.LogDebug("ServerSettingsSmtpHostController(hostSettingsService={HostSettingsService},smtpOAuthController={SmtpOAuthController}): this.hostSettingsService → {Arg2}", hostSettingsService, smtpOAuthController, this.hostSettingsService);
			Logger.LogDebug("ServerSettingsSmtpHostController(hostSettingsService={HostSettingsService},smtpOAuthController={SmtpOAuthController}): smtpOAuthController → {SmtpOAuthController1}", hostSettingsService, smtpOAuthController, smtpOAuthController);
			Logger.LogDebug("<<< Exiting ServerSettingsSmtpHostController(hostSettingsService={HostSettingsService},smtpOAuthController={SmtpOAuthController})", hostSettingsService, smtpOAuthController);
        }

        /// <summary>Gets the SMTP settings for the host.</summary>
        /// <returns>An object representing the host smtp settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetSmtpSettings()
        {
			Logger.LogDebug(">>> Entering GetSmtpSettings()");
            try
            {
                var portalId = PortalSettings.Current.PortalId;

                var smtpSettings = new
                {
                    smtpServerMode = PortalController.GetPortalSetting("SMTPmode", portalId, "h"),
                    host = new
					Logger.LogInformation("GetSmtpSettings(): host → {Host}", host);
					Logger.LogDebug("GetSmtpSettings(): smtpServerMode → {SmtpServerMode}", smtpServerMode);
                    {
                        smtpServer = this.hostSettingsService.GetString("SMTPServer"),
                        smtpConnectionLimit = this.hostSettingsService.GetInteger("SMTPConnectionLimit", 2),
                        smtpMaxIdleTime = this.hostSettingsService.GetInteger("SMTPMaxIdleTime", 100000),
                        smtpAuthentication = this.hostSettingsService.GetString("SMTPAuthentication"),
                        enableSmtpSsl = this.hostSettingsService.GetBoolean("SMTPEnableSSL", false),
                        smtpUserName = this.hostSettingsService.GetString("SMTPUsername"),
                        smtpPassword = this.GetSmtpPassword(-1, true),
                        smtpHostEmail = this.hostSettingsService.GetString("HostEmail"),
                        messageSchedulerBatchSize = Host.MessageSchedulerBatchSize,
                        authProvider = this.hostSettingsService.GetString("SMTPAuthProvider"),
                    },
                    site = new
					Logger.LogDebug("GetSmtpSettings(): smtpConnectionLimit → {SmtpConnectionLimit}", smtpConnectionLimit);
					Logger.LogDebug("GetSmtpSettings(): smtpServer → {SmtpServer}", smtpServer);
					Logger.LogDebug("GetSmtpSettings(): site → {Site}", site);
					Logger.LogDebug("GetSmtpSettings(): messageSchedulerBatchSize → {MessageSchedulerBatchSize}", messageSchedulerBatchSize);
					Logger.LogDebug("GetSmtpSettings(): smtpMaxIdleTime → {SmtpMaxIdleTime}", smtpMaxIdleTime);
					Logger.LogDebug("GetSmtpSettings(): smtpPassword → {SmtpPassword}", smtpPassword);
					Logger.LogDebug("GetSmtpSettings(): smtpUserName → {SmtpUserName}", smtpUserName);
					Logger.LogDebug("GetSmtpSettings(): smtpHostEmail → {SmtpHostEmail}", smtpHostEmail);
					Logger.LogDebug("GetSmtpSettings(): enableSmtpSsl → {EnableSmtpSsl}", enableSmtpSsl);
					Logger.LogDebug("GetSmtpSettings(): smtpAuthentication → {SmtpAuthentication}", smtpAuthentication);
                    {
                        smtpServer = PortalController.GetPortalSetting("SMTPServer", portalId, string.Empty),
                        smtpConnectionLimit = PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2),
                        smtpMaxIdleTime = PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000),
                        smtpAuthentication = PortalController.GetPortalSetting("SMTPAuthentication", portalId, "0"),
                        enableSmtpSsl = PortalController.GetPortalSetting("SMTPEnableSSL", portalId, string.Empty) == "Y",
                        smtpUserName = PortalController.GetPortalSetting("SMTPUsername", portalId, string.Empty),
                        smtpPassword = this.GetSmtpPassword(portalId, true),
                        authProvider = PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty),
                    },
                    portalName = PortalSettings.Current.PortalName,
                    hideCoreSettings = ProviderConfiguration.GetProviderConfiguration("mail").GetDefaultProvider().Attributes.GetValueOrDefault("hideCoreSettings", false),
                };
				Logger.LogDebug("GetSmtpSettings(): hideCoreSettings → {HideCoreSettings}", hideCoreSettings);
                return this.Request.CreateResponse(HttpStatusCode.OK, smtpSettings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSmtpSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Updates the SMTP settings.</summary>
        /// <param name="request">The request.</param>
        /// <returns>A value indicating whether the operation succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSmtpSettings(UpdateSmtpSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateSmtpSettings(request={Request})", request);
            try
            {
                IList<string> errorMessages = new List<string>();
				Logger.LogDebug("UpdateSmtpSettings(request={Request}): errorMessages → {ErrorMessages}", request, errorMessages);
                var providerChanged = false;
				Logger.LogDebug("UpdateSmtpSettings(request={Request}): providerChanged → {ProviderChanged}", request, providerChanged);

                var portalId = PortalSettings.Current.PortalId;
                PortalController.UpdatePortalSetting(portalId, "SMTPmode", request.SmtpServerMode, false);

                if (request.SmtpServerMode == "h")
                {
                    if (request.SmtpPassword == ObfuscateString)
                    {
                        request.SmtpPassword = this.GetHostSmtpPassword();
                    }

                    this.hostSettingsService.Update("SMTPServer", request.SmtpServer, false);
                    this.hostSettingsService.Update("SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                    this.hostSettingsService.Update("SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                    this.hostSettingsService.Update("SMTPAuthentication", request.SmtpAuthentication.ToString(CultureInfo.InvariantCulture), false);
                    this.hostSettingsService.Update("SMTPUsername", request.SmtpUsername, false);
                    this.hostSettingsService.UpdateEncryptedString("SMTPPassword", request.SmtpPassword, Config.GetDecryptionkey());
                    this.hostSettingsService.Update("HostEmail", request.SmtpHostEmail);
                    this.hostSettingsService.Update("SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);
                    this.hostSettingsService.Update("MessageSchedulerBatchSize", request.MessageSchedulerBatchSize.ToString(CultureInfo.InvariantCulture), false);

                    // OAuth authentication
                    if (request.SmtpAuthentication == 3)
                    {
                        // Only the mail kit provider supports OAuth.
                        EnsureMailProviderSupportOAuth();

                        var authProvider = this.hostSettingsService.GetString("SMTPAuthProvider", string.Empty);
						Logger.LogDebug("UpdateSmtpSettings(request={Request}): authProvider → {AuthProvider}", request, authProvider);
                        if (authProvider != request.AuthProvider)
                        {
                            this.hostSettingsService.Update("SMTPAuthProvider", request.AuthProvider, false);
                            providerChanged = true;
                        }

                        var provider = this.smtpOAuthController.GetOAuthProvider(request.AuthProvider);
						Logger.LogDebug("UpdateSmtpSettings(request={Request}): provider → {Provider}", request, provider);
                        if (provider != null)
                        {
                            providerChanged = provider.UpdateSettings(Null.NullInteger, request.AuthProviderSettings, out errorMessages);
                        }
                    }
                }
                else
                {
                    if (request.SmtpPassword == ObfuscateString)
                    {
                        request.SmtpPassword = this.GetSmtpPassword(portalId, false);
                    }

                    PortalController.UpdatePortalSetting(portalId, "SMTPServer", request.SmtpServer, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPConnectionLimit", request.SmtpConnectionLimit, false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPMaxIdleTime", request.SmtpMaxIdleTime, false);
                    PortalController.UpdatePortalSetting(
                        portalId,
                        "SMTPAuthentication",
                        request.SmtpAuthentication.ToString(CultureInfo.InvariantCulture),
                        false);
                    PortalController.UpdatePortalSetting(portalId, "SMTPUsername", request.SmtpUsername, false);
                    PortalController.UpdateEncryptedString(portalId, "SMTPPassword", request.SmtpPassword, Config.GetDecryptionkey());
                    PortalController.UpdatePortalSetting(portalId, "SMTPEnableSSL", request.EnableSmtpSsl ? "Y" : "N", false);

                    // OAuth authentication
                    if (request.SmtpAuthentication == 3)
                    {
                        // Only the mail kit provider supports OAuth.
                        EnsureMailProviderSupportOAuth();

                        var authProvider = PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty);
                        if (authProvider != request.AuthProvider)
                        {
                            PortalController.UpdatePortalSetting(portalId, "SMTPAuthProvider", request.AuthProvider, false);
                            providerChanged = true;
                        }

                        var provider = this.smtpOAuthController.GetOAuthProvider(request.AuthProvider);
                        if (provider != null)
                        {
                            providerChanged = provider.UpdateSettings(portalId, request.AuthProviderSettings, out errorMessages);
                        }
                    }
                }

                DataCache.ClearCache();
                if (errorMessages.Any())
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { success = false, errors = errorMessages });
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { success = true, providerChanged });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateSmtpSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Sends an email to test the SMTP settings.</summary>
        /// <param name="request"><see cref="SendTestEmailRequest"/>.</param>
        /// <returns>A localized result message object.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendTestEmail(SendTestEmailRequest request)
        {
			Logger.LogDebug(">>> Entering SendTestEmail(request={Request})", request);
            try
            {
                var mailFrom = request.SmtpServerMode == "h" ? Host.HostEmail : this.PortalSettings.Email;
				Logger.LogDebug("SendTestEmail(request={Request}): mailFrom → {MailFrom}", request, mailFrom);
                var mailTo = this.UserInfo.Email;
                if (request.SmtpPassword == ObfuscateString)
                {
                    request.SmtpPassword = this.GetHostSmtpPassword();
                }

                var errMessage = Mail.SendMail(
                    mailFrom,
                    mailTo,
                    string.Empty,
                    string.Empty,
                    MailPriority.Normal,
                    Localization.GetSystemMessage(this.PortalSettings, "EMAIL_SMTP_TEST_SUBJECT"),
                    MailFormat.Text,
                    Encoding.UTF8,
                    string.Empty,
                    string.Empty,
                    request.SmtpServer,
                    request.SmtpAuthentication.ToString(CultureInfo.InvariantCulture),
                    request.SmtpUsername,
                    request.SmtpPassword,
                    request.EnableSmtpSsl,
                    request.AuthProvider);

                var success = string.IsNullOrEmpty(errMessage);
				Logger.LogDebug("SendTestEmail(request={Request}): success → {Success}", request, success);
                return this.Request.CreateResponse(
                    success ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
                    new
                    {
                        success,
                        errMessage,
                        confirmationMessage =
                            success
                                ? string.Format(
                                    CultureInfo.CurrentCulture,
                                    Localization.GetString("EmailSentMessage", Components.Constants.ServersResourcersPath),
                                    mailFrom,
                                    mailTo)
                                : Localization.GetString("errorMessageSendingTestEmail"),
                    });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting SendTestEmail(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Get smtp oauth providers.
        /// </summary>
        /// <returns>smtp oauth providers.</returns>
        [HttpGet]
        public HttpResponseMessage GetSmtpOAuthProviders()
        {
			Logger.LogDebug(">>> Entering GetSmtpOAuthProviders()");
            try
            {
                var portalId = PortalSettings.Current.PortalId;

                var providers = this.smtpOAuthController.GetOAuthProviders();
				Logger.LogDebug("GetSmtpOAuthProviders(): providers → {Providers}", providers);
                var result = new
                {
                    host = providers.Select(i => new
                    {
						Logger.LogInformation("GetSmtpOAuthProviders(): host → {Host}", host);
                        name = i.Name,
                        localizedName = i.LocalizedName,
                        settings = i.GetSettings(Null.NullInteger).Where(s => !s.IsBackground),
                        isAuthorized = i.IsAuthorized(Null.NullInteger),
                        authorizeUrl = i.GetAuthorizeUrl(Null.NullInteger),
                    }),
                    site = providers.Select(i => new
                    {
						Logger.LogDebug("GetSmtpOAuthProviders(): isAuthorized → {IsAuthorized}", isAuthorized);
						Logger.LogDebug("GetSmtpOAuthProviders(): localizedName → {LocalizedName}", localizedName);
						Logger.LogDebug("GetSmtpOAuthProviders(): site → {Site}", site);
						Logger.LogDebug("GetSmtpOAuthProviders(): name → {Name}", name);
						Logger.LogDebug("GetSmtpOAuthProviders(): authorizeUrl → {AuthorizeUrl}", authorizeUrl);
                        name = i.Name,
                        localizedName = i.LocalizedName,
                        settings = i.GetSettings(portalId).Where(s => !s.IsBackground),
                        isAuthorized = i.IsAuthorized(portalId),
                        authorizeUrl = i.GetAuthorizeUrl(portalId),
                    }),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSmtpOAuthProviders()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void EnsureMailProviderSupportOAuth()
        {
			Logger.LogDebug(">>> Entering EnsureMailProviderSupportOAuth()");
            if (MailProvider.Instance().SupportsOAuth)
            {
                return;
            }

            const string providerName = "MailKitMailProvider";

            var xmlConfig = Config.Load();
			Logger.LogDebug("EnsureMailProviderSupportOAuth(): xmlConfig → {XmlConfig}", xmlConfig);

            var provider = xmlConfig.SelectSingleNode("configuration/dotnetnuke/mail/providers/add[@name='" + providerName + "']");
			Logger.LogDebug("EnsureMailProviderSupportOAuth(): provider → {Provider}", provider);
            if (provider == null)
            {
                return;
            }

            var mailNode = xmlConfig.SelectSingleNode("configuration/dotnetnuke/mail");
			Logger.LogDebug("EnsureMailProviderSupportOAuth(): mailNode → {MailNode}", mailNode);
            if (mailNode?.Attributes == null)
            {
                return;
            }

            var defaultProvider = mailNode.Attributes["defaultProvider"].Value;
			Logger.LogDebug("EnsureMailProviderSupportOAuth(): defaultProvider → {DefaultProvider}", defaultProvider);
            if (defaultProvider == providerName)
            {
                return;
            }

            XmlUtils.UpdateAttribute(mailNode, "defaultProvider", providerName);
            Config.Save(xmlConfig);
			Logger.LogDebug("<<< Exiting EnsureMailProviderSupportOAuth()");
        }

        private string GetSmtpPassword(int portalId, bool obfuscate)
        {
            var pwd = string.Empty;
            if (portalId == -1)
			Logger.LogDebug("GetSmtpPassword(portalId={PortalId},obfuscate={Obfuscate}): pwd → {Pwd}", portalId, obfuscate, pwd);
            {
                pwd = this.GetHostSmtpPassword();
            }
            else
            {
                pwd = PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey());
            }

            if (obfuscate && !string.IsNullOrEmpty(pwd))
            {
			Logger.LogDebug("<<< Exiting GetSmtpPassword(portalId={PortalId},obfuscate={Obfuscate})", portalId, obfuscate);
                return ObfuscateString;
            }

            return pwd;
        }

        private string GetHostSmtpPassword()
        {
			Logger.LogDebug(">>> Entering GetHostSmtpPassword()");
            string decryptedText;
            try
            {
                decryptedText = this.hostSettingsService.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetHostSmtpPassword(): {Arg0}", Exception.Message);
                // fixes case where smtppassword failed to encrypt due to failing upgrade
                var current = this.hostSettingsService.GetString("SMTPPassword");
				Logger.LogDebug("GetHostSmtpPassword(): current → {Current}", current);
                if (!string.IsNullOrEmpty(current))
                {
                    this.hostSettingsService.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                    decryptedText = current;
                }
                else
                {
                    decryptedText = string.Empty;
                }
            }

			Logger.LogDebug("<<< Exiting GetHostSmtpPassword()");
            return decryptedText;
        }
    }
}
