// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Threading;
    using System.Web.Security;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The Password UserModuleBase is used to manage Users Passwords.</summary>
    public partial class Password : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Password));
        private readonly IEventLogger eventLogger;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourceController clientResourceController;
        private readonly DataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="Password"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public Password()
            : this(null, null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering Password()");
        }

		Logger.LogDebug("<<< Exiting Password()");
        /// <summary>Initializes a new instance of the <see cref="Password"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public Password(IEventLogger eventLogger, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
            : this(eventLogger, hostSettings, javaScript, clientResourceController, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Password"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="dataProvider">The data provider.</param>
        public Password(IEventLogger eventLogger, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, ListController listController, DataProvider dataProvider)
            : base(listController, hostSettings)
        {
            this.eventLogger = eventLogger ?? Common.Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.javaScript = javaScript ?? Common.Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
			Logger.LogDebug("Password(eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): javaScript → {JavaScript1}", eventLogger, hostSettings, javaScript, clientResourceController, listController, dataProvider, javaScript);
			Logger.LogDebug("Password(eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): dataProvider → {DataProvider1}", eventLogger, hostSettings, javaScript, clientResourceController, listController, dataProvider, dataProvider);
            this.clientResourceController = clientResourceController ?? Common.Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
			Logger.LogDebug("Password(eventLogger={EventLogger},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},listController={ListController},dataProvider={DataProvider}): clientResourceController → {ClientResourceController1}", eventLogger, hostSettings, javaScript, clientResourceController, listController, dataProvider, clientResourceController);
            this.dataProvider = dataProvider ?? Common.Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        }

        /// <summary>A function which handles an event with <see cref="PasswordUpdatedEventArgs"/>.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void PasswordUpdatedEventHandler(object sender, PasswordUpdatedEventArgs e);

        /// <summary>An event which is fired when something related to password updating occurs.</summary>
        public event PasswordUpdatedEventHandler PasswordUpdated;

        /// <summary>An event which is fired when something related to a password Q&amp;A occurs.</summary>
        public event PasswordUpdatedEventHandler PasswordQuestionAnswerUpdated;

        /// <summary>Gets the UserMembership associated with this control.</summary>
        public UserMembership Membership
        {
            get
            {
                UserMembership membership = null;
                if (this.User != null)
                {
                    membership = this.User.Membership;
                }

                return membership;
            }
        }

        protected bool UseCaptcha => Convert.ToBoolean(GetSetting(this.PortalId, "Security_CaptchaChangePassword"));

        /// <summary>Raises the <see cref="PasswordUpdated"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnPasswordUpdated(PasswordUpdatedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPasswordUpdated(e={E})", e);
            if (this.IsUserOrAdmin)
            {
                this.PasswordUpdated?.Invoke(this, e);
            }
			Logger.LogDebug("<<< Exiting OnPasswordUpdated(e={E})", e);
        }

        /// <summary>Raises the PasswordQuestionAnswerUpdated Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnPasswordQuestionAnswerUpdated(PasswordUpdatedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPasswordQuestionAnswerUpdated(e={E})", e);
            if (this.IsUserOrAdmin)
            {
                this.PasswordQuestionAnswerUpdated?.Invoke(this, e);
            }
			Logger.LogDebug("<<< Exiting OnPasswordQuestionAnswerUpdated(e={E})", e);
        }

        /// <summary>DataBind binds the data to the controls.</summary>
        public override void DataBind()
        {
			Logger.LogDebug(">>> Entering DataBind()");
            this.lblLastChanged.Text = this.User.Membership.LastPasswordChangeDate.ToLongDateString();

            // Set Password Expiry Label
            if (this.User.Membership.UpdatePassword)
            {
                this.lblExpires.Text = Localization.GetString("ForcedExpiry", this.LocalResourceFile);
            }
            else
            {
                this.lblExpires.Text = PasswordConfig.PasswordExpiry > 0 ? this.User.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry).ToLongDateString() : Localization.GetString("NoExpiry", this.LocalResourceFile);
            }

            if ((!MembershipProviderConfig.PasswordRetrievalEnabled) && this.IsAdmin && (!this.IsUser))
            {
                this.pnlChange.Visible = true;
                this.cmdUpdate.Visible = true;
                this.oldPasswordRow.Visible = false;
                this.lblChangeHelp.Text = Localization.GetString("AdminChangeHelp", this.LocalResourceFile);
            }
            else
            {
                this.pnlChange.Visible = true;
                this.cmdUpdate.Visible = true;

                // Set up Change Password
                if (this.IsAdmin && !this.IsUser)
                {
                    this.lblChangeHelp.Text = Localization.GetString("AdminChangeHelp", this.LocalResourceFile);
                    this.oldPasswordRow.Visible = false;
                }
                else
                {
                    this.lblChangeHelp.Text = Localization.GetString("UserChangeHelp", this.LocalResourceFile);
                    if (this.Request.IsAuthenticated)
                    {
                        this.pnlChange.Visible = true;
                        this.cmdUserReset.Visible = false;
                        this.cmdUpdate.Visible = true;
                    }
                    else
                    {
                        this.pnlChange.Visible = false;
                        this.cmdUserReset.Visible = true;
                        this.cmdUpdate.Visible = false;
                    }
                }
            }

            // If Password Reset is not enabled then only the Admin can reset the
            // Password, a User must Update
            if (!MembershipProviderConfig.PasswordResetEnabled)
            {
                this.pnlReset.Visible = false;
                this.cmdReset.Visible = false;
            }
            else
            {
                this.pnlReset.Visible = true;
                this.cmdReset.Visible = true;

                // Set up Reset Password
                if (this.IsAdmin && !this.IsUser)
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                    {
                        this.pnlReset.Visible = false;
                        this.cmdReset.Visible = false;
                    }
                    else
                    {
                        this.lblResetHelp.Text = Localization.GetString("AdminResetHelp", this.LocalResourceFile);
                    }

                    this.questionRow.Visible = false;
                    this.answerRow.Visible = false;
                }
                else
                {
                    if (MembershipProviderConfig.RequiresQuestionAndAnswer && this.IsUser)
                    {
                        this.lblResetHelp.Text = Localization.GetString("UserResetHelp", this.LocalResourceFile);
                        this.lblQuestion.Text = this.User.Membership.PasswordQuestion;
                        this.questionRow.Visible = true;
                        this.answerRow.Visible = true;
                    }
                    else
                    {
                        this.pnlReset.Visible = false;
                        this.cmdReset.Visible = false;
                    }
                }
            }

            // Set up Edit Question and Answer area
            if (MembershipProviderConfig.RequiresQuestionAndAnswer && this.IsUser)
            {
                this.pnlQA.Visible = true;
                this.cmdUpdateQA.Visible = true;
            }
            else
            {
                this.pnlQA.Visible = false;
                this.cmdUpdateQA.Visible = false;
            }
			Logger.LogDebug("<<< Exiting DataBind()");
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.cmdReset.Click += this.CmdReset_Click;
            this.cmdUserReset.Click += this.CmdUserReset_Click;
            this.cmdUpdate.Click += this.CmdUpdate_Click;
            this.cmdUpdateQA.Click += this.CmdUpdateQA_Click;

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && this.User.UserID != UserController.Instance.GetCurrentUserInfo().UserID)
            {
                this.pnlChange.Visible = false;
                this.cmdUpdate.Visible = false;
                this.CannotChangePasswordMessage.Visible = true;
            }

            if (this.UseCaptcha)
            {
                this.captchaRow.Visible = true;
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.PasswordStrength.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/stylesheets/dnn.PasswordStrength.css", FileOrder.Css.ResourceCss);

            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);

            base.OnPreRender(e);

            if (this.HostSettings.EnableStrengthMeter)
            {
                this.passwordContainer.CssClass = "password-strength-container";
                this.txtNewPassword.CssClass = "password-strength";

                var options = new DnnPaswordStrengthOptions();
				Logger.LogDebug("OnPreRender(e={E}): options → {Options}", e, options);
                var optionsAsJsonString = Json.Serialize(options);
				Logger.LogDebug("OnPreRender(e={E}): optionsAsJsonString → {OptionsAsJsonString}", e, optionsAsJsonString);
                var script = $"dnn.initializePasswordStrength('.password-strength', {optionsAsJsonString});{Environment.NewLine}";
				Logger.LogDebug("OnPreRender(e={E}): script → {Script}", e, script);

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
                FirstElementSelector = "#" + this.passwordContainer.ClientID + " input[type=password]",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnPassword",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			Logger.LogDebug("OnPreRender(e={E}): confirmOptionsAsJsonString → {ConfirmOptionsAsJsonString}", e, confirmOptionsAsJsonString);
            var confirmScript = $"dnn.initializePasswordComparer({confirmOptionsAsJsonString});{Environment.NewLine}";
			Logger.LogDebug("OnPreRender(e={E}): confirmScript → {ConfirmScript}", e, confirmScript);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ConfirmPassword", confirmScript, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", confirmScript, true);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private void CmdReset_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdReset_Click(sender={Sender},e={E})", sender, e);
            if (!this.IsUserOrAdmin)
            {
                return;
            }

            if (MembershipProviderConfig.RequiresQuestionAndAnswer && !this.IsAdmin)
            {
                if (string.IsNullOrEmpty(this.txtAnswer.Text))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    return;
                }
            }

            try
            {
                // create reset token
                UserController.ResetPasswordToken(this.User, (int)this.HostSettings.AdminMembershipResetLinkValidity.TotalMinutes);

                bool canSend = Mail.SendMail(this.User, MessageType.PasswordReminder, this.PortalSettings) == string.Empty;
                string message;
                var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
                if (canSend)
                {
                    message = Localization.GetString("PasswordSent", this.LocalResourceFile);
					Logger.LogDebug("CmdReset_Click(sender={Sender},e={E}): message → {Message}", sender, e, message);
                    this.LogSuccess();
                }
                else
                {
                    message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    this.LogFailure(message);
                }

                UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
            }
            catch (ArgumentException exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
            }
			Logger.LogDebug("<<< Exiting CmdReset_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdUserReset_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdUserReset_Click(sender={Sender},e={E})", sender, e);
            try
            {
                // send fresh reset token copy
                bool canSend = UserController.ResetPasswordToken(this.User, true);

                string message;
                var moduleMessageType = ModuleMessage.ModuleMessageType.GreenSuccess;
                if (canSend)
                {
                    message = Localization.GetString("PasswordSent", this.LocalResourceFile);
					Logger.LogDebug("CmdUserReset_Click(sender={Sender},e={E}): message → {Message}", sender, e, message);
                    this.LogSuccess();
                }
                else
                {
                    message = Localization.GetString("OptionUnavailable", this.LocalResourceFile);
                    moduleMessageType = ModuleMessage.ModuleMessageType.RedError;
                    this.LogFailure(message);
                }

                UI.Skins.Skin.AddModuleMessage(this, message, moduleMessageType);
            }
            catch (ArgumentException exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
            }
			Logger.LogDebug("<<< Exiting CmdUserReset_Click(sender={Sender},e={E})", sender, e);
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
            ILogInfo log = new LogInfo
            {
                LogPortalName = this.PortalSettings.PortalName,
                LogUserName = this.User.Username,
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

            this.eventLogger.AddLog(log);
			Logger.LogDebug("<<< Exiting LogResult(message={Message})", message);
        }

        private void CmdUpdate_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdUpdate_Click(sender={Sender},e={E})", sender, e);
            if ((this.UseCaptcha && this.ctlCaptcha.IsValid) || !this.UseCaptcha)
            {
                if (!this.IsUserOrAdmin)
                {
                    return;
                }

                // 1. Check New Password and Confirm are the same
                if (this.txtNewPassword.Text != this.txtNewConfirm.Text)
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordMismatch));
                    return;
                }

                // 2. Check New Password is Valid
                if (!UserController.ValidatePassword(this.txtNewPassword.Text))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordInvalid));
                    return;
                }

                // 3. Check old Password is Provided
                if (!this.IsAdmin && string.IsNullOrEmpty(this.txtOldPassword.Text))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordMissing));
                    return;
                }

                // 4. Check New Password is different
                if (!this.IsAdmin && this.txtNewPassword.Text == this.txtOldPassword.Text)
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordNotDifferent));
                    return;
                }

                // 5. Check New Password is not same as username or banned
                var membershipPasswordController = new MembershipPasswordController(this.ListController, this.dataProvider);
                var settings = new MembershipPasswordSettings(this.User.PortalID);

                if (settings.EnableBannedList)
                {
                    if (membershipPasswordController.FoundBannedPassword(this.txtNewPassword.Text) || this.User.Username == this.txtNewPassword.Text)
                    {
                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.BannedPasswordUsed));
                        return;
                    }
                }

                // check new password is not in history
                if (membershipPasswordController.IsPasswordInHistory(this.User.UserID, this.User.PortalID, this.txtNewPassword.Text, false))
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    return;
                }

                if (!this.IsAdmin && this.txtNewPassword.Text == this.txtOldPassword.Text)
                {
                    this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordNotDifferent));
                    return;
                }

                if (!this.IsAdmin)
                {
                    try
                    {
                        this.OnPasswordUpdated(UserController.ChangePassword(this.eventLogger, this.User, this.txtOldPassword.Text, this.txtNewPassword.Text)
                                              ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                              : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                    catch (MembershipPasswordException exc)
                    {
                        // Password Answer missing
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    }
                    catch (ThreadAbortException)
                    {
                        // Do nothing we are not logging ThreadAbortExceptions caused by redirects
                    }
                    catch (Exception exc)
                    {
                        // Fail
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                }
                else
                {
                    try
                    {
                        this.OnPasswordUpdated(UserController.ResetAndChangePassword(this.User, this.txtNewPassword.Text)
                                              ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                              : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                    catch (MembershipPasswordException exc)
                    {
                        // Password Answer missing
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                    }
                    catch (ThreadAbortException)
                    {
                        // Do nothing we are not logging ThreadAbortExceptions caused by redirects
                    }
                    catch (Exception exc)
                    {
                        // Fail
                        Logger.Error(exc);

                        this.OnPasswordUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting CmdUpdate_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>cmdUpdate_Click runs when the Update Question and Answer  Button is clicked.</summary>
        private void CmdUpdateQA_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdUpdateQA_Click(sender={Sender},e={E})", sender, e);
            if (!this.IsUserOrAdmin)
            {
                return;
            }

            if (string.IsNullOrEmpty(this.txtQAPassword.Text))
            {
                this.OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordInvalid));
                return;
            }

            if (string.IsNullOrEmpty(this.txtEditQuestion.Text))
            {
                this.OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordQuestion));
                return;
            }

            if (string.IsNullOrEmpty(this.txtEditAnswer.Text))
            {
                this.OnPasswordQuestionAnswerUpdated(new PasswordUpdatedEventArgs(PasswordUpdateStatus.InvalidPasswordAnswer));
                return;
            }

            // Try and set password Q and A
            UserInfo objUser = UserController.GetUserById(this.HostSettings, this.PortalId, this.UserId);
			Logger.LogDebug("CmdUpdateQA_Click(sender={Sender},e={E}): objUser → {ObjUser}", sender, e, objUser);
            this.OnPasswordQuestionAnswerUpdated(UserController.ChangePasswordQuestionAndAnswer(this.eventLogger, objUser, this.txtQAPassword.Text, this.txtEditQuestion.Text, this.txtEditAnswer.Text)
                                                ? new PasswordUpdatedEventArgs(PasswordUpdateStatus.Success)
                                                : new PasswordUpdatedEventArgs(PasswordUpdateStatus.PasswordResetFailed));
												Logger.LogDebug("<<< Exiting CmdUpdateQA_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Provides a customised EventArgs class for the <see cref="Password.PasswordUpdated"/> event.</summary>
        public class PasswordUpdatedEventArgs
        {
            /// <summary>Initializes a new instance of the <see cref="PasswordUpdatedEventArgs"/> class.</summary>
            /// <param name="status">The Password Update Status.</param>
            public PasswordUpdatedEventArgs(PasswordUpdateStatus status)
            {
				Logger.LogDebug(">>> Entering PasswordUpdatedEventArgs(status={Status})", status);
                this.UpdateStatus = status;
				Logger.LogDebug("PasswordUpdatedEventArgs(status={Status}): this.UpdateStatus → {Arg1}", status, this.UpdateStatus);
				Logger.LogDebug("<<< Exiting PasswordUpdatedEventArgs(status={Status})", status);
            }

            /// <summary>Gets or sets the Update Status.</summary>
            public PasswordUpdateStatus UpdateStatus { get; set; }
        }
    }
}
