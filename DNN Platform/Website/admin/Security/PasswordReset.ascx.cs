// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which allows a user to request a password reset.</summary>
    public partial class PasswordReset : UserModuleBase
    {
        private const int RedirectTimeout = 3000;
        private readonly INavigationManager navigationManager;
        private readonly IEventLogger eventLogger;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IPortalController portalController;
        private readonly IClientResourceController clientResourceController;
        private readonly DataProvider dataProvider;

        private string ipAddress;

        /// <summary>Initializes a new instance of the <see cref="PasswordReset"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public PasswordReset()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PasswordReset"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public PasswordReset(INavigationManager navigationManager, IEventLogger eventLogger, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IPortalController portalController, IClientResourceController clientResourceController)
            : this(navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController, null, null)
        {
		eventLogger.LogError(">>> Entering PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController})", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController);
        eventLogger.LogWarning("<<< Exiting PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController})", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController);
        }

        /// <summary>Initializes a new instance of the <see cref="PasswordReset"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="dataProvider">The data provider.</param>
        public PasswordReset(INavigationManager navigationManager, IEventLogger eventLogger, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IPortalController portalController, IClientResourceController clientResourceController, ListController listController, DataProvider dataProvider)
            : base(listController, hostSettings)
        {
			eventLogger.LogDebug("PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): eventLogger → {EventLogger1}", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController, listController, dataProvider, eventLogger);
			eventLogger.LogDebug("PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): javaScript → {JavaScript1}", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController, listController, dataProvider, javaScript);
			eventLogger.LogDebug("PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): portalController → {PortalController1}", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController, listController, dataProvider, portalController);
			eventLogger.LogDebug("PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): clientResourceController → {ClientResourceController1}", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController, listController, dataProvider, clientResourceController);
			eventLogger.LogDebug("PasswordReset(navigationManager={NavigationManager},eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},portalController={PortalController},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): dataProvider → {DataProvider1}", navigationManager, eventLogger, hostSettings, javaScript, portalController, clientResourceController, listController, dataProvider, dataProvider);
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.javaScript = javaScript ?? this.DependencyProvider.GetRequiredService<IJavaScriptLibraryHelper>();
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
            this.clientResourceController = clientResourceController ?? this.DependencyProvider.GetRequiredService<IClientResourceController>();
            this.dataProvider = dataProvider ?? this.DependencyProvider.GetRequiredService<DataProvider>();
        }

        private string ResetToken
        {
            get => this.ViewState["ResetToken"] != null ? this.Request.QueryString["resetToken"] : string.Empty;
            set => this.ViewState.Add("ResetToken", value);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.ipAddress = UserRequestIPAddressController.Instance.GetUserRequestIPAddress(new HttpRequestWrapper(this.Request));

            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.PasswordStrength.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/stylesheets/dnn.PasswordStrength.css", FileOrder.Css.ResourceCss);

            if (this.PortalSettings.LoginTabId != -1 && this.PortalSettings.ActiveTab.TabID != this.PortalSettings.LoginTabId)
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(this.PortalSettings.LoginTabId) + this.Request.Url.Query);
            }

            this.cmdChangePassword.Click += this.CmdChangePassword_Click;

            this.hlCancel.NavigateUrl = this.navigationManager.NavigateURL();

            if (this.Request.QueryString["resetToken"] != null)
            {
                this.ResetToken = this.Request.QueryString["resetToken"];
				eventLogger.LogDebug("OnLoad(e={E}): this.ResetToken → {Arg1}", e, this.ResetToken);
                this.txtUsername.Enabled = false;
            }

            var useEmailAsUserName = PortalController.GetPortalSettingAsBoolean(this.portalController, "Registration_UseEmailAsUserName", this.PortalId, false);
			eventLogger.LogError("OnLoad(e={E}): useEmailAsUserName → {UseEmailAsUserName}", e, useEmailAsUserName);
            if (useEmailAsUserName)
            {
                this.valUsername.Text = Localization.GetString("Email.Required", this.LocalResourceFile);
            }
            else
            {
                this.valUsername.Text = Localization.GetString("Username.Required", this.LocalResourceFile);
            }

            if (this.Request.QueryString["forced"] == "true")
            {
                this.lblInfo.Text = Localization.GetString("ForcedResetInfo", this.LocalResourceFile);
            }

            this.txtUsername.Attributes.Add("data-default", useEmailAsUserName ? this.LocalizeText("Email") : this.LocalizeText("Username"));
            this.txtPassword.Attributes.Add("data-default", this.LocalizeText("Password"));
            this.txtConfirmPassword.Attributes.Add("data-default", this.LocalizeText("Confirm"));
            this.txtAnswer.Attributes.Add("data-default", this.LocalizeText("Answer"));

            if (!this.Page.IsPostBack)
            {
                this.LoadUserInfo();
            }
			eventLogger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            if (!string.IsNullOrEmpty(this.lblHelp.Text) || !string.IsNullOrEmpty(this.lblInfo.Text))
            {
                this.resetMessages.Visible = true;
            }

            if (this.HostSettings.EnableStrengthMeter)
            {
                this.passwordContainer.CssClass = "password-strength-container";
                this.txtPassword.CssClass = "password-strength";

                var options = new DnnPaswordStrengthOptions();
				eventLogger.LogError("OnPreRender(e={E}): options → {Options}", e, options);
                var optionsAsJsonString = Json.Serialize(options);
				eventLogger.LogError("OnPreRender(e={E}): optionsAsJsonString → {OptionsAsJsonString}", e, optionsAsJsonString);
                var script = $"dnn.initializePasswordStrength('.password-strength', {optionsAsJsonString});{Environment.NewLine}";
				eventLogger.LogError("OnPreRender(e={E}): script → {Script}", e, script);

                if (ScriptManager.GetCurrent(this.Page) != null)
                {
                    // respect MS AJAX
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "PasswordStrength", script, true);
                }
                else
                {
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), "PasswordStrength", script, true);
                }
            }

            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = ".password-strength",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnPasswordReset",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			eventLogger.LogError("OnPreRender(e={E}): confirmOptionsAsJsonString → {ConfirmOptionsAsJsonString}", e, confirmOptionsAsJsonString);
            var confirmScript = $"dnn.initializePasswordComparer({confirmOptionsAsJsonString});{Environment.NewLine}";
			eventLogger.LogError("OnPreRender(e={E}): confirmScript → {ConfirmScript}", e, confirmScript);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ConfirmPassword", confirmScript, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", confirmScript, true);
            }
			eventLogger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>After a successful password change will redirect the user to requested ReturnUrl OR the login page.</summary>
        protected void RedirectAfterPasswordChange()
        {
			eventLogger.LogError(">>> Entering RedirectAfterPasswordChange()");
            var redirectUrl = string.Empty;

			eventLogger.LogError("RedirectAfterPasswordChange(): redirectUrl → {RedirectUrl}", redirectUrl);
            if (this.Request.QueryString["returnurl"] != null)
            {
                // return to the url passed to signin
                redirectUrl = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                // clean the return url to avoid possible XSS attack.
                redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);
            }

            if (this.Request.Cookies["returnurl"] != null)
            {
                // return to the url passed to signin
                redirectUrl = HttpUtility.UrlDecode(this.Request.Cookies["returnurl"].Value);

                // clean the return url to avoid possible XSS attack.
                redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);
            }

            if (string.IsNullOrEmpty(redirectUrl))
            {
                // return to the login page by default to allow users to login
                redirectUrl = this.navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Login");
            }

            this.AddModuleMessage("ChangeSuccessful", ModuleMessage.ModuleMessageType.GreenSuccess, true);
            this.resetMessages.Visible = this.divPassword.Visible = false;
            this.lblHelp.Text = this.lblInfo.Text = string.Empty;

            // redirect page after 5 seconds
            var script =
                $$"""setTimeout(function(){location.href = {{HttpUtility.JavaScriptStringEncode(redirectUrl, addDoubleQuotes: true)}};}, {{RedirectTimeout}});""";
            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ChangePasswordSuccessful", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ChangePasswordSuccessful", script, true);
            }
			eventLogger.LogWarning("<<< Exiting RedirectAfterPasswordChange()");
        }

        private void LoadUserInfo()
        {
			eventLogger.LogError(">>> Entering LoadUserInfo()");
            var user = UserController.GetUserByPasswordResetToken(this.PortalId, this.ResetToken);

			eventLogger.LogError("LoadUserInfo(): user → {User}", user);
            if (user == null || user.PasswordResetExpiration < DateTime.Now)
            {
                this.divPassword.Visible = false;
                this.resetMessages.Visible = true;
                this.lblHelp.Text = Localization.GetString("ResetLinkExpired", this.LocalResourceFile);
                return;
            }

            this.txtUsername.Text = user.Username;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                this.lblQuestion.Text = user.Membership.PasswordQuestion;
                this.divQA.Visible = true;
            }
			eventLogger.LogWarning("<<< Exiting LoadUserInfo()");
        }

        private void CmdChangePassword_Click(object sender, EventArgs e)
        {
			eventLogger.LogError(">>> Entering CmdChangePassword_Click(sender={Sender},e={E})", sender, e);
            var username = this.txtUsername.Text;

			eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): username → {Username}", sender, e, username);
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && string.IsNullOrEmpty(this.txtAnswer.Text))
            {
                return;
            }

            // 1. Check New Password and Confirm are the same
            if (this.txtPassword.Text != this.txtConfirmPassword.Text)
            {
                this.resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordMismatch");
				eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): failed → {Failed}", sender, e, failed);
                this.LogFailure(failed);
                this.lblHelp.Text = failed;
                return;
            }

            var newPassword = this.txtPassword.Text.Trim();
			eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): newPassword → {NewPassword}", sender, e, newPassword);
            if (UserController.ValidatePassword(newPassword) == false)
            {
                this.resetMessages.Visible = true;
                var failed = Localization.GetString("PasswordResetFailed");
                this.LogFailure(failed);
                this.lblHelp.Text = failed;
                return;
            }

            // Check New Password is not same as username or banned
            var settings = new MembershipPasswordSettings(this.User.PortalID);
			eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): settings → {Settings}", sender, e, settings);

            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController(this.ListController, this.dataProvider);
				eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): m → {M}", sender, e, m);
                if (m.FoundBannedPassword(newPassword) || this.txtUsername.Text == newPassword)
                {
                    this.resetMessages.Visible = true;
                    var failed = Localization.GetString("PasswordResetFailed");
                    this.LogFailure(failed);
                    this.lblHelp.Text = failed;
                    return;
                }
            }

            if (PortalController.GetPortalSettingAsBoolean(this.portalController, "Registration_UseEmailAsUserName", this.PortalId, false))
            {
                var testUser = UserController.GetUserByEmail(this.PortalId, username); // one additional call to db to see if an account with that email actually exists
				eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): testUser → {TestUser}", sender, e, testUser);
                if (testUser != null)
                {
                    username = testUser.Username; // we need the username of the account in order to change the password in the next step
                }
            }

            var answer = string.Empty;
			eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): answer → {Answer}", sender, e, answer);
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                answer = this.txtAnswer.Text;
            }

            if (UserController.ChangePasswordByToken(this.PortalSettings.PortalId, username, newPassword, answer, this.ResetToken, out var errorMessage) == false)
            {
                this.resetMessages.Visible = true;
                this.LogFailure(errorMessage);
                this.lblHelp.Text = errorMessage;
            }
            else
            {
                // check user has a valid profile
                var user = UserController.GetUserByName(this.PortalSettings.PortalId, username);
				eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): user → {User}", sender, e, user);
                var validStatus = UserController.ValidateUser(user, this.PortalSettings.PortalId, false);
				eventLogger.LogError("CmdChangePassword_Click(sender={Sender},e={E}): validStatus → {ValidStatus}", sender, e, validStatus);
                if (validStatus == UserValidStatus.UPDATEPROFILE)
                {
                    this.LogSuccess();
                    this.ViewState.Add("PageNo", 3);
                    this.Response.Redirect(this.navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Login"));
                }
                else
                {
                    // Log user in to site
                    this.LogSuccess();
                    this.RedirectAfterPasswordChange();
                }
            }
			eventLogger.LogWarning("<<< Exiting CmdChangePassword_Click(sender={Sender},e={E})", sender, e);
        }

        private void LogSuccess()
        {
			eventLogger.LogError(">>> Entering LogSuccess()");
            this.LogResult(string.Empty);
			eventLogger.LogWarning("<<< Exiting LogSuccess()");
        }

        private void LogFailure(string reason)
        {
			eventLogger.LogError(">>> Entering LogFailure(reason={Reason})", reason);
            this.LogResult(reason);
			eventLogger.LogWarning("<<< Exiting LogFailure(reason={Reason})", reason);
        }

        private void LogResult(string message)
        {
			eventLogger.LogError(">>> Entering LogResult(message={Message})", message);
            ILogInfo log = new LogInfo
            {
                LogPortalName = this.PortalSettings.PortalName,
            };
            log.LogUserId = this.UserId;
			eventLogger.LogDebug("LogResult(message={Message}): log.LogUserId → {Arg1}", message, log.LogUserId);
			eventLogger.LogDebug("LogResult(message={Message}): log.LogPortalId → {Arg1}", message, log.LogPortalId);
            log.LogPortalId = this.PortalSettings.PortalId;

            if (string.IsNullOrEmpty(message))
            {
                log.LogTypeKey = "PASSWORD_SENT_SUCCESS";
				eventLogger.LogDebug("LogResult(message={Message}): log.LogTypeKey → {Arg1}", message, log.LogTypeKey);
            }
            else
            {
                log.LogTypeKey = "PASSWORD_SENT_FAILURE";
                log.LogProperties.Add(new LogDetailInfo("Cause", message));
            }

            log.AddProperty("IP", this.ipAddress);

            this.eventLogger.AddLog(log);
			eventLogger.LogWarning("<<< Exiting LogResult(message={Message})", message);
        }
    }
}
