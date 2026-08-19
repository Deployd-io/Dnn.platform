// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Net;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The SendPassword UserModuleBase is used to allow a user to retrieve their password.</summary>
    public partial class SendPassword : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SendPassword));
        private readonly INavigationManager navigationManager;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;
        private readonly IMailSettings mailSettings;

        private UserInfo user;
        private int userCount = Null.NullInteger;
        private string ipAddress;

        /// <summary>Initializes a new instance of the <see cref="SendPassword"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public SendPassword()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering SendPassword()");
        }

		Logger.LogDebug("<<< Exiting SendPassword()");
        /// <summary>Initializes a new instance of the <see cref="SendPassword"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="mailSettings">The mail settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public SendPassword(INavigationManager navigationManager, IEventLogger eventLogger, IPortalController portalController, IMailSettings mailSettings)
            : this(navigationManager, eventLogger, portalController, mailSettings, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SendPassword"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="mailSettings">The mail settings.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        public SendPassword(INavigationManager navigationManager, IEventLogger eventLogger, IPortalController portalController, IMailSettings mailSettings, ListController listController, IHostSettings hostSettings)
            : base(listController, hostSettings)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
			Logger.LogDebug("SendPassword(navigationManager={NavigationManager},eventLogger={EventLogger},portalController={PortalController},mailSettings={MailSettings},listController={ListController},hostSettings={HostSettings}): eventLogger → {EventLogger1}", navigationManager, eventLogger, portalController, mailSettings, listController, hostSettings, eventLogger);
			Logger.LogDebug("SendPassword(navigationManager={NavigationManager},eventLogger={EventLogger},portalController={PortalController},mailSettings={MailSettings},listController={ListController},hostSettings={HostSettings}): mailSettings → {MailSettings1}", navigationManager, eventLogger, portalController, mailSettings, listController, hostSettings, mailSettings);
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
			Logger.LogDebug("SendPassword(navigationManager={NavigationManager},eventLogger={EventLogger},portalController={PortalController},mailSettings={MailSettings},listController={ListController},hostSettings={HostSettings}): portalController → {PortalController1}", navigationManager, eventLogger, portalController, mailSettings, listController, hostSettings, portalController);
            this.mailSettings = mailSettings ?? this.DependencyProvider.GetRequiredService<IMailSettings>();
        }

        /// <summary>Gets the Redirect URL (after successful sending of password).</summary>
        protected string RedirectURL
        {
            get
            {
                var redirectURL = string.Empty;

                object setting = GetSetting(this.PortalId, "Redirect_AfterRegistration");

                if (Convert.ToInt32(setting) > 0)
                {
                    // redirect to after registration page
                    redirectURL = this.navigationManager.NavigateURL(Convert.ToInt32(setting));
                }
                else
                {
                    if (Convert.ToInt32(setting) <= 0)
                    {
                        if (this.Request.QueryString["returnurl"] != null)
                        {
                            // return to the url passed to register
                            redirectURL = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                            // clean the return url to avoid possible XSS attack.
                            redirectURL = UrlUtils.ValidReturnUrl(redirectURL);

                            if (redirectURL.Contains("?returnurl"))
                            {
                                string baseURL = redirectURL.Substring(
                                    0,
                                    redirectURL.IndexOf("?returnurl", StringComparison.Ordinal));
                                string returnURL =
                                    redirectURL.Substring(redirectURL.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                                redirectURL = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                            }
                        }

                        if (string.IsNullOrEmpty(redirectURL))
                        {
                            // redirect to current page
                            redirectURL = this.navigationManager.NavigateURL();
                        }
                    }
                    else
                    {
                        // redirect to after registration page
                        redirectURL = this.navigationManager.NavigateURL(Convert.ToInt32(setting));
                    }
                }

                return redirectURL;
            }
        }

        /// <summary>Gets a value indicating whether the Captcha control is used to validate the login.</summary>
        protected bool UseCaptcha
        {
            get
            {
                var setting = GetSetting(this.PortalId, "Security_CaptchaRetrivePassword");
                return Convert.ToBoolean(setting);
            }
        }

        protected bool UsernameDisabled => PortalController.GetPortalSettingAsBoolean(this.portalController, "Registration_UseEmailAsUserName", this.PortalId, false);

        private bool ShowEmailField => MembershipProviderConfig.RequiresUniqueEmail || this.UsernameDisabled;

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            var isEnabled = true;
			Logger.LogDebug("OnInit(e={E}): isEnabled → {IsEnabled}", e, isEnabled);

            // both retrieval and reset now use password token resets
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                this.lblHelp.Text = Localization.GetString("ResetTokenHelp", this.LocalResourceFile);
                this.cmdSendPassword.Text = Localization.GetString("ResetToken", this.LocalResourceFile);
            }
            else
            {
                isEnabled = false;
                this.lblHelp.Text = Localization.GetString("DisabledPasswordHelp", this.LocalResourceFile);
                this.divPassword.Visible = false;
            }

            if (!MembershipProviderConfig.PasswordResetEnabled)
            {
                isEnabled = false;
                this.lblHelp.Text = Localization.GetString("DisabledPasswordHelp", this.LocalResourceFile);
                this.divPassword.Visible = false;
            }

            if (MembershipProviderConfig.RequiresUniqueEmail && isEnabled && !PortalController.GetPortalSettingAsBoolean(this.portalController, "Registration_UseEmailAsUserName", this.PortalId, false))
            {
                this.lblHelp.Text += Localization.GetString("RequiresUniqueEmail", this.LocalResourceFile);
            }

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && isEnabled)
            {
                this.lblHelp.Text += Localization.GetString("RequiresQuestionAndAnswer", this.LocalResourceFile);
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdSendPassword.Click += this.OnSendPasswordClick;
            this.lnkCancel.NavigateUrl = this.navigationManager.NavigateURL();

            this.ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));

            this.divEmail.Visible = this.ShowEmailField;
            this.divUsername.Visible = !this.UsernameDisabled;
            this.divCaptcha.Visible = this.UseCaptcha;

            if (this.UseCaptcha)
            {
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>cmdSendPassword_Click runs when the Password Reminder button is clicked.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void OnSendPasswordClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnSendPasswordClick(sender={Sender},e={E})", sender, e);
            // pretty much always display the same message to avoid hinting on the existence of a username
            var input = string.IsNullOrEmpty(this.txtUsername.Text) ? this.txtEmail.Text : this.txtUsername.Text;
			Logger.LogDebug("OnSendPasswordClick(sender={Sender},e={E}): input → {Input}", sender, e, input);
            var message = string.Format(CultureInfo.CurrentCulture, Localization.GetString("PasswordSent", this.LocalResourceFile), WebUtility.HtmlEncode(input));
			Logger.LogDebug("OnSendPasswordClick(sender={Sender},e={E}): message → {Message}", sender, e, message);
            var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
			Logger.LogDebug("OnSendPasswordClick(sender={Sender},e={E}): moduleMessageType → {ModuleMessageType}", sender, e, moduleMessageType);
            var canSend = true;
			Logger.LogDebug("OnSendPasswordClick(sender={Sender},e={E}): canSend → {CanSend}", sender, e, canSend);

            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || (!this.UseCaptcha))
            {
                if (string.IsNullOrEmpty(this.txtUsername.Text.Trim()))
                {
                    // No UserName provided
                    if (this.ShowEmailField)
                    {
                        if (string.IsNullOrEmpty(this.txtEmail.Text.Trim()))
                        {
                            // No email address either (cannot retrieve password)
                            canSend = false;
                            message = Localization.GetString("EnterUsernameEmail", this.LocalResourceFile);
                            moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                        }
                    }
                    else
                    {
                        // Cannot retrieve password
                        canSend = false;
                        message = Localization.GetString("EnterUsername", this.LocalResourceFile);
                        moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    }
                }

                if (string.IsNullOrEmpty(this.mailSettings.GetServer(this.PortalId)))
                {
                    // SMTP Server is not configured
                    canSend = false;
                    message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.YellowWarning;

                    var logMessage = Localization.GetString("SMTPNotConfigured", this.LocalResourceFile);
					Logger.LogDebug("OnSendPasswordClick(sender={Sender},e={E}): logMessage → {LogMessage}", sender, e, logMessage);

                    this.LogResult(logMessage);
                }

                if (canSend)
                {
                    this.GetUser();
                    if (this.user != null)
                    {
                        if (this.user.IsDeleted)
                        {
                            canSend = false;
                        }
                        else
                        {
                            if (this.user.Membership.Approved == false)
                            {
                                Mail.SendMail(this.user, MessageType.PasswordReminderUserIsNotApproved, this.PortalSettings);
                                if (this.PortalSettings.EnableUnapprovedPasswordReminderNotification)
                                {
                                    Mail.SendMail(
                                        this.user,
                                        MessageType.PasswordReminderUserIsNotApprovedAdmin,
                                        this.PortalSettings);
                                }

                                canSend = false;
                            }

                            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                            {
                                UserController.ResetPasswordToken(this.user);
                            }

                            if (canSend)
                            {
                                if (Mail.SendMail(this.user, MessageType.PasswordReminder, this.PortalSettings) != string.Empty)
                                {
                                    canSend = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.userCount > 1)
                        {
                            message = Localization.GetString("MultipleUsers", this.LocalResourceFile);
                        }

                        canSend = false;
                    }

                    if (canSend)
                    {
                        this.LogSuccess();
                        this.lnkCancel.Attributes["resourcekey"] = "cmdClose";
                    }
                    else
                    {
                        this.LogFailure(message);
                    }

                    // always hide panel so as to not reveal if username exists.
                    this.pnlRecover.Visible = false;
                    UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
                    this.liSend.Visible = false;
                    this.liCancel.Visible = true;
                }
                else
                {
                    UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
                }
            }
			Logger.LogDebug("<<< Exiting OnSendPasswordClick(sender={Sender},e={E})", sender, e);
        }

        private void GetUser()
        {
			Logger.LogDebug(">>> Entering GetUser()");
            if (this.ShowEmailField && !string.IsNullOrEmpty(this.txtEmail.Text.Trim()) && (string.IsNullOrEmpty(this.txtUsername.Text.Trim()) || this.divUsername.Visible == false))
            {
                var arrUsers = UserController.GetUsersByEmail(this.PortalSettings.PortalId, this.txtEmail.Text, 0, int.MaxValue, ref this.userCount);
				Logger.LogDebug("GetUser(): arrUsers → {ArrUsers}", arrUsers);
                if (arrUsers is { Count: 1 })
                {
                    this.user = (UserInfo)arrUsers[0];
					Logger.LogDebug("GetUser(): user → {User}", user);
                }
            }
            else
            {
                this.user = UserController.GetUserByName(this.PortalSettings.PortalId, this.txtUsername.Text);
            }
			Logger.LogDebug("<<< Exiting GetUser()");
        }

        private void LogSuccess()
        {
			Logger.LogDebug(">>> Entering LogSuccess()");
            this.LogResult(string.Empty);
			Logger.LogDebug("<<< Exiting LogSuccess()");
        }

        private void LogFailure(string reason)
        {
			Logger.LogDebug(">>> Entering LogFailure(reason={Reason})", reason);
            this.LogResult(reason);
			Logger.LogDebug("<<< Exiting LogFailure(reason={Reason})", reason);
        }

        private void LogResult(string message)
        {
			Logger.LogDebug(">>> Entering LogResult(message={Message})", message);
            var portalSecurity = PortalSecurity.Instance;

            ILogInfo log = new LogInfo
            {
			Logger.LogDebug("<<< Exiting LogResult(message={Message})", message);
                LogPortalName = this.PortalSettings.PortalName,
#pragma warning disable CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
                LogUserName = portalSecurity.InputFilter(this.txtUsername.Text, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup),
#pragma warning restore CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
            };
            log.LogUserId = this.UserId;
			Logger.LogDebug("LogResult(message={Message}): log.LogUserId → {Arg1}", message, log.LogUserId);
			Logger.LogDebug("LogResult(message={Message}): log.LogPortalId → {Arg1}", message, log.LogPortalId);
            log.LogPortalId = this.PortalSettings.PortalId;

            if (string.IsNullOrEmpty(message))
            {
                log.LogTypeKey = "PASSWORD_SENT_SUCCESS";
				Logger.LogDebug("LogResult(message={Message}): log.LogTypeKey → {Arg1}", message, log.LogTypeKey);
            }
            else
            {
                log.LogTypeKey = "PASSWORD_SENT_FAILURE";
                log.LogProperties.Add(new LogDetailInfo("Cause", message));
            }

            log.AddProperty("IP", this.ipAddress);

            this.eventLogger.AddLog(log);
        }
    }
}
