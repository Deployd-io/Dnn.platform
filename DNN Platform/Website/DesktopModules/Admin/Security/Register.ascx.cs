// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control for allowing users to register for the site.</summary>
    public partial class Register : UserUserControlBase
    {
        protected const string PasswordStrengthTextBoxCssClass = "password-strength";
        protected const string ConfirmPasswordTextBoxCssClass = "password-confirm";

        private readonly List<AuthenticationLoginBase> loginControls = [];
        private readonly INavigationManager navigationManager;
        private readonly IServiceProvider serviceProvider;
        private readonly IHostSettings hostSettings;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly ListController listController;
        private readonly DataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public Register()
            : this(null, null, null, null, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering Register()");
        }

		eventLogger.LogDebug("<<< Exiting Register()");
        /// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="serviceProvider">The service provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public Register(INavigationManager navigationManager, IServiceProvider serviceProvider)
            : this(navigationManager, serviceProvider, null, null, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider})", navigationManager, serviceProvider);
		eventLogger.LogDebug("<<< Exiting Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider})", navigationManager, serviceProvider);
		eventLogger.LogDebug(">>> Entering Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController})", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController);
		eventLogger.LogDebug("<<< Exiting Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController})", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController);
        }

        /// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public Register(INavigationManager navigationManager, IServiceProvider serviceProvider, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
            : this(navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public Register(INavigationManager navigationManager, IServiceProvider serviceProvider, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
            : this(navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, null, null)
        {
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): navigationManager → {NavigationManager1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, navigationManager);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): hostSettings → {HostSettings1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, hostSettings);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): javaScript → {JavaScript1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, javaScript);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): clientResourceController → {ClientResourceController1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, clientResourceController);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): appStatus → {AppStatus1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, appStatus);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): eventLogger → {EventLogger1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, eventLogger);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): listController → {ListController1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, listController);
		eventLogger.LogDebug("Register(navigationManager={NavigationManager},serviceProvider={ServiceProvider},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},listController={ListController},dataProvider={DataProvider}): dataProvider → {DataProvider1}", navigationManager, serviceProvider, hostSettings, javaScript, clientResourceController, appStatus, eventLogger, listController, dataProvider, dataProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="Register"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="dataProvider">The data provider.</param>
        public Register(INavigationManager navigationManager, IServiceProvider serviceProvider, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, ListController listController, DataProvider dataProvider)
        {
            this.serviceProvider = serviceProvider ?? Globals.GetCurrentServiceProvider();
            this.navigationManager = navigationManager ?? this.serviceProvider.GetRequiredService<INavigationManager>();
            this.hostSettings = hostSettings ?? this.serviceProvider.GetRequiredService<IHostSettings>();
            this.javaScript = javaScript ?? this.serviceProvider.GetRequiredService<IJavaScriptLibraryHelper>();
            this.clientResourceController = clientResourceController ?? this.serviceProvider.GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? this.serviceProvider.GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? this.serviceProvider.GetRequiredService<IEventLogger>();
            this.listController = listController ?? this.serviceProvider.GetRequiredService<ListController>();
            this.dataProvider = dataProvider ?? this.serviceProvider.GetRequiredService<DataProvider>();
        }

        protected string ExcludeTerms
        {
            get
            {
                string regex = string.Empty;
                if (!string.IsNullOrEmpty(this.PortalSettings.Registration.ExcludeTerms))
                {
                    regex = @"^(?:(?!" + this.PortalSettings.Registration.ExcludeTerms.Replace(" ", string.Empty).Replace(",", "|") + @").)*$\r?\n?";
                }

                return regex;
            }
        }

        protected bool IsValid => this.Validate();

        /// <inheritdoc />
        protected override bool AddUser { get; } = true;

        protected string AuthenticationType
        {
            get => this.ViewState.GetValue("AuthenticationType", Null.NullString);
            set => this.ViewState.SetValue("AuthenticationType", value, Null.NullString);
        }

        protected UserCreateStatus CreateStatus { get; set; }

        protected string UserToken
        {
            get => this.ViewState.GetValue("UserToken", string.Empty);
            set => this.ViewState.SetValue("UserToken", value, string.Empty);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);

            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            if (this.PortalSettings.Registration.RegistrationFormType == 0)
            {
                // DisplayName
                if (string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
                {
                    this.AddField("DisplayName", string.Empty, true, string.Empty, TextBoxMode.SingleLine);
                }
                else
                {
                    this.AddField("FirstName", string.Empty, true, string.Empty, TextBoxMode.SingleLine);
                    this.AddField("LastName", string.Empty, true, string.Empty, TextBoxMode.SingleLine);
                }

                // Email
                this.AddField("Email", string.Empty, true, this.PortalSettings.Registration.EmailValidator, TextBoxMode.SingleLine);

                // UserName
                if (!this.PortalSettings.Registration.UseEmailAsUserName)
                {
                    var userNameValidator =
                        string.IsNullOrEmpty(this.PortalSettings.Registration.UserNameValidator)
                            ? this.ExcludeTerms
                            : this.PortalSettings.Registration.UserNameValidator;
                    this.AddField(
                        "Username",
                        string.Empty,
                        true,
                        userNameValidator,
                        TextBoxMode.SingleLine);
                }

                // Password
                if (!this.PortalSettings.Registration.RandomPassword)
                {
                    this.AddPasswordStrengthField("Password", "Membership", true);

                    if (this.PortalSettings.Registration.RequirePasswordConfirm)
                    {
                        this.AddPasswordConfirmField("PasswordConfirm", "Membership", true);
                    }
                }

                // Password Q&A
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    this.AddField("PasswordQuestion", "Membership", true, string.Empty, TextBoxMode.SingleLine);
                    this.AddField("PasswordAnswer", "Membership", true, string.Empty, TextBoxMode.SingleLine);
                }

                if (this.PortalSettings.Registration.RequireValidProfile)
                {
                    foreach (ProfilePropertyDefinition property in this.User.Profile.ProfileProperties)
                    {
                        if (property.Required)
                        {
                            this.AddProperty(property);
                        }
                    }
                }
            }
            else
            {
                var fields = this.PortalSettings.Registration.RegistrationFields.Split(',').ToList();
				eventLogger.LogDebug("OnInit(e={E}): fields → {Fields}", e, fields);

                // append question/answer field when RequiresQuestionAndAnswer is enabled in config.
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    if (!fields.Contains("PasswordQuestion"))
                    {
                        fields.Add("PasswordQuestion");
                    }

                    if (!fields.Contains("PasswordAnswer"))
                    {
                        fields.Add("PasswordAnswer");
                    }
                }

                foreach (string field in fields)
                {
                    var trimmedField = field.Trim();
					eventLogger.LogDebug("OnInit(e={E}): trimmedField → {TrimmedField}", e, trimmedField);
                    switch (trimmedField)
                    {
                        case "Username":
                            var userNameValidator =
                                string.IsNullOrEmpty(this.PortalSettings.Registration.UserNameValidator)
                                    ? this.ExcludeTerms
                                    : this.PortalSettings.Registration.UserNameValidator;
                            this.AddField(
                                "Username",
                                string.Empty,
                                true,
                                userNameValidator,
                                TextBoxMode.SingleLine);
                            break;
                        case "DisplayName":
                            this.AddField(trimmedField, string.Empty, true, this.ExcludeTerms, TextBoxMode.SingleLine);
                            break;
                        case "Email":
                            this.AddField("Email", string.Empty, true, this.PortalSettings.Registration.EmailValidator, TextBoxMode.SingleLine);
                            break;
                        case "Password":
                            this.AddPasswordStrengthField(trimmedField, "Membership", true);
                            break;
                        case "PasswordConfirm":
                            this.AddPasswordConfirmField(trimmedField, "Membership", true);
                            break;
                        case "PasswordQuestion":
                        case "PasswordAnswer":
                            this.AddField(trimmedField, "Membership", true, string.Empty, TextBoxMode.SingleLine);
                            break;
                        default:
                            ProfilePropertyDefinition property = this.User.Profile.GetProperty(trimmedField);
							eventLogger.LogDebug("OnInit(e={E}): property → {Property}", e, property);
                            if (property != null)
                            {
                                this.AddProperty(property);
                            }

                            break;
                    }
                }
            }

            // Verify that the current user has access to this page
            if (this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.NoRegistration && this.Request.IsAuthenticated == false)
            {
                try
                {
                    this.Response.Redirect(this.navigationManager.NavigateURL("Access Denied"), true);
                }
                catch (ThreadAbortException)
                {
					eventLogger.LogError("Exception in OnInit(e={E}): {Arg1}", e, ThreadAbortException.getMessage());
                    // do nothing here.
                }
            }

            this.cancelLink.NavigateUrl = this.closeLink.NavigateUrl = this.GetRedirectUrl(false);
            this.registerButton.Click += this.RegisterButton_Click;

            if (this.PortalSettings.Registration.UseAuthProviders)
            {
                List<AuthenticationInfo> authSystems = AuthenticationController.GetEnabledAuthenticationServices(this.hostSettings);
				eventLogger.LogDebug("OnInit(e={E}): authSystems → {AuthSystems}", e, authSystems);
                foreach (AuthenticationInfo authSystem in authSystems)
                {
                    try
                    {
                        var authLoginControl = (AuthenticationLoginBase)this.LoadControl("~/" + authSystem.LoginControlSrc);
						eventLogger.LogDebug("OnInit(e={E}): authLoginControl → {AuthLoginControl}", e, authLoginControl);
                        if (authSystem.AuthenticationType != "DNN")
                        {
                            this.BindLoginControl(authLoginControl, authSystem);

                            // Check if AuthSystem is Enabled
                            if (authLoginControl.Enabled && authLoginControl.SupportsRegistration)
                            {
                                authLoginControl.Mode = AuthMode.Register;
								eventLogger.LogDebug("OnInit(e={E}): authLoginControl.Mode → {Arg1}", e, authLoginControl.Mode);

                                // Add Login Control to List
                                this.loginControls.Add(authLoginControl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
						eventLogger.LogError("Exception in OnInit(e={E}): {Arg1}", e, ex.getMessage());
                        Exceptions.LogException(ex);
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            if (this.Request.IsAuthenticated)
            {
                // if a Login Page has not been specified for the portal
                if (Globals.IsAdminControl())
                {
                    // redirect to current page
                    this.Response.Redirect(this.navigationManager.NavigateURL(), true);
                }
                else
                {
                    // make module container invisible if user is not a page admin
                    if (!TabPermissionController.CanAdminPage())
                    {
                        this.ContainerControl.Visible = false;
                    }
                }
            }

            if (this.PortalSettings.Registration.UseCaptcha)
            {
                this.captchaRow.Visible = true;
                this.ctlCaptcha.ErrorMessage = Localization.GetString("InvalidCaptcha", this.LocalResourceFile);
                this.ctlCaptcha.Text = Localization.GetString("CaptchaText", this.LocalResourceFile);
            }

            if (this.PortalSettings.Registration.UseAuthProviders && string.IsNullOrEmpty(this.AuthenticationType))
            {
                foreach (AuthenticationLoginBase authLoginControl in this.loginControls)
                {
                    this.socialLoginControls.Controls.Add(authLoginControl);
                }
            }

            // Display relevant message
            this.userHelpLabel.Text = Localization.GetSystemMessage(this.PortalSettings, "MESSAGE_REGISTRATION_INSTRUCTIONS");
            switch (this.PortalSettings.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    this.userHelpLabel.Text += Localization.GetString("PrivateMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    this.userHelpLabel.Text += Localization.GetString("PublicMembership", Localization.SharedResourceFile);
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    this.userHelpLabel.Text += Localization.GetString("VerifiedMembership", Localization.SharedResourceFile);
                    break;
            }

            this.userHelpLabel.Text += Localization.GetString("Required", this.LocalResourceFile);
            this.userHelpLabel.Text += Localization.GetString("RegisterWarning", this.LocalResourceFile);

            this.userForm.DataSource = this.User;
            if (!this.Page.IsPostBack)
            {
                this.userForm.DataBind();
            }
			eventLogger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = "." + PasswordStrengthTextBoxCssClass,
                SecondElementSelector = "." + ConfirmPasswordTextBoxCssClass,
                ContainerSelector = ".dnnRegistrationForm",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var optionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			eventLogger.LogDebug("OnPreRender(e={E}): optionsAsJsonString → {OptionsAsJsonString}", e, optionsAsJsonString);
            var script = $"dnn.initializePasswordComparer({optionsAsJsonString});{Environment.NewLine}";
			eventLogger.LogDebug("OnPreRender(e={E}): script → {Script}", e, script);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ConfirmPassword", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", script, true);
            }
			eventLogger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private void AddField(string dataField, string dataMember, bool required, string regexValidator, TextBoxMode textMode)
        {
			eventLogger.LogDebug(">>> Entering AddField(dataField={DataField},dataMember={DataMember},required={Required},regexValidator={RegexValidator},textMode={TextMode})", dataField, dataMember, required, regexValidator, textMode);
            if (this.userForm.Items.Any(i => i.ID == dataField))
            {
                return;
            }

            var formItem = new DnnFormTextBoxItem(this.appStatus, this.eventLogger)
            {
                ID = dataField,
                DataField = dataField,
                DataMember = dataMember,
                Visible = true,
                Required = required,
                TextMode = textMode,
            };
            if (!string.IsNullOrEmpty(regexValidator))
            {
                formItem.ValidationExpression = regexValidator;
				eventLogger.LogDebug("AddField(dataField={DataField},dataMember={DataMember},required={Required},regexValidator={RegexValidator},textMode={TextMode}): formItem.ValidationExpression → {Arg5}", dataField, dataMember, required, regexValidator, textMode, formItem.ValidationExpression);
            }

            this.userForm.Items.Add(formItem);
			eventLogger.LogDebug("<<< Exiting AddField(dataField={DataField},dataMember={DataMember},required={Required},regexValidator={RegexValidator},textMode={TextMode})", dataField, dataMember, required, regexValidator, textMode);
        }

        private void AddPasswordStrengthField(string dataField, string dataMember, bool required)
        {
			eventLogger.LogDebug(">>> Entering AddPasswordStrengthField(dataField={DataField},dataMember={DataMember},required={Required})", dataField, dataMember, required);
            DnnFormItemBase formItem;

            if (this.hostSettings.EnableStrengthMeter)
            {
                formItem = new DnnFormPasswordItem(this.appStatus, this.eventLogger, this.clientResourceController)
                {
				eventLogger.LogDebug("AddPasswordStrengthField(dataField={DataField},dataMember={DataMember},required={Required}): formItem → {FormItem}", dataField, dataMember, required, formItem);
                    TextBoxCssClass = PasswordStrengthTextBoxCssClass,
                    ContainerCssClass = "password-strength-container",
                };
            }
            else
            {
                formItem = new DnnFormTextBoxItem(this.appStatus, this.eventLogger)
                {
                    TextMode = TextBoxMode.Password,
                    TextBoxCssClass = PasswordStrengthTextBoxCssClass,
                };
            }

            formItem.ID = dataField;
            formItem.DataField = dataField;
            formItem.DataMember = dataMember;
            formItem.Visible = true;
            formItem.Required = required;

            this.userForm.Items.Add(formItem);
			eventLogger.LogDebug("<<< Exiting AddPasswordStrengthField(dataField={DataField},dataMember={DataMember},required={Required})", dataField, dataMember, required);
        }

        private void AddPasswordConfirmField(string dataField, string dataMember, bool required)
        {
			eventLogger.LogDebug(">>> Entering AddPasswordConfirmField(dataField={DataField},dataMember={DataMember},required={Required})", dataField, dataMember, required);
            var formItem = new DnnFormTextBoxItem(this.appStatus, this.eventLogger)
            {
                ID = dataField,
                DataField = dataField,
                DataMember = dataMember,
                Visible = true,
                Required = required,
                TextMode = TextBoxMode.Password,
                TextBoxCssClass = ConfirmPasswordTextBoxCssClass,
                ClearContentInPasswordMode = true,
                MaxLength = 39,
            };
            this.userForm.Items.Add(formItem);
			eventLogger.LogDebug("<<< Exiting AddPasswordConfirmField(dataField={DataField},dataMember={DataMember},required={Required})", dataField, dataMember, required);
        }

        private void AddProperty(ProfilePropertyDefinition property)
        {
			eventLogger.LogDebug(">>> Entering AddProperty(property={Property})", property);
            if (this.userForm.Items.Any(i => i.ID == property.PropertyName))
            {
                return;
            }

            ListEntryInfo imageType = this.listController.GetListEntryInfo("DataType", "Image");
			eventLogger.LogDebug("AddProperty(property={Property}): imageType → {ImageType}", property, imageType);
            if (property.DataType != imageType.EntryID)
            {
                var formItem = new DnnFormEditControlItem(this.appStatus, this.eventLogger, this.serviceProvider)
                {
                    ID = property.PropertyName,
                    ResourceKey = $"ProfileProperties_{property.PropertyName}",
                    LocalResourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx.resx",
                    ValidationMessageSuffix = ".Validation",
                    ControlType = EditorInfo.GetEditor(this.listController, property.DataType),
                    DataMember = "Profile",
                    DataField = property.PropertyName,
                    Visible = property.Visible,
                    Required = property.Required,
                };

                // To check if the property has a default value
                if (!string.IsNullOrEmpty(property.DefaultValue))
                {
                    formItem.Value = property.DefaultValue;
					eventLogger.LogDebug("AddProperty(property={Property}): formItem.Value → {Arg1}", property, formItem.Value);
                }

                if (!string.IsNullOrEmpty(property.ValidationExpression))
                {
                    formItem.ValidationExpression = property.ValidationExpression;
					eventLogger.LogDebug("AddProperty(property={Property}): formItem.ValidationExpression → {Arg1}", property, formItem.ValidationExpression);
                }

                this.userForm.Items.Add(formItem);
            }
			eventLogger.LogDebug("<<< Exiting AddProperty(property={Property})", property);
        }

        private void BindLoginControl(AuthenticationLoginBase authLoginControl, AuthenticationInfo authSystem)
        {
			eventLogger.LogDebug(">>> Entering BindLoginControl(authLoginControl={AuthLoginControl},authSystem={AuthSystem})", authLoginControl, authSystem);
            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            authLoginControl.AuthenticationType = authSystem.AuthenticationType;
			eventLogger.LogDebug("BindLoginControl(authLoginControl={AuthLoginControl},authSystem={AuthSystem}): authLoginControl.AuthenticationType → {Arg2}", authLoginControl, authSystem, authLoginControl.AuthenticationType);
            authLoginControl.ID = Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc) + "_" + authSystem.AuthenticationType;
            authLoginControl.LocalResourceFile = authLoginControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                 Path.GetFileNameWithoutExtension(authSystem.LoginControlSrc);
												eventLogger.LogDebug("BindLoginControl(authLoginControl={AuthLoginControl},authSystem={AuthSystem}): authLoginControl.LocalResourceFile → {Arg2}", authLoginControl, authSystem, authLoginControl.LocalResourceFile);
            authLoginControl.RedirectURL = this.GetRedirectUrl();
            authLoginControl.ModuleConfiguration = this.ModuleConfiguration;
			eventLogger.LogDebug("BindLoginControl(authLoginControl={AuthLoginControl},authSystem={AuthSystem}): authLoginControl.ModuleConfiguration → {Arg2}", authLoginControl, authSystem, authLoginControl.ModuleConfiguration);

            authLoginControl.UserAuthenticated += this.UserAuthenticated;
			eventLogger.LogDebug("<<< Exiting BindLoginControl(authLoginControl={AuthLoginControl},authSystem={AuthSystem})", authLoginControl, authSystem);
        }

        private void CreateUser()
        {
			eventLogger.LogDebug(">>> Entering CreateUser()");
            // Update DisplayName to conform to Format
            this.UpdateDisplayName();

            this.User.Membership.Approved = this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PublicRegistration;
            var user = this.User;
            this.CreateStatus = UserController.CreateUser(ref user);

            DataCache.ClearPortalUserCountCache(this.PortalId);

            try
            {
                if (this.CreateStatus == UserCreateStatus.Success)
                {
                    // hide the successful captcha
                    this.captchaRow.Visible = false;

                    // Associate alternate Login with User and proceed with Login
                    if (!string.IsNullOrEmpty(this.AuthenticationType))
                    {
                        AuthenticationController.AddUserAuthentication(this.eventLogger, this.User.UserID, this.AuthenticationType, this.UserToken);
                    }

                    string strMessage = this.CompleteUserCreation(this.CreateStatus, user, true, this.IsRegister);

                    if (string.IsNullOrEmpty(strMessage))
                    {
                        this.Response.Redirect(this.GetRedirectUrl(), true);
                    }
                    else
                    {
                        this.RegistrationForm.Visible = false;
                        this.registerButton.Visible = false;
                        this.closeLink.Visible = true;
                    }
                }
                else
                {
                    this.AddLocalizedModuleMessage(UserController.GetUserCreateStatus(this.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in CreateUser(): {Arg0}", exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogDebug("<<< Exiting CreateUser()");
        }

        private void UpdateDisplayName()
        {
			eventLogger.LogDebug(">>> Entering UpdateDisplayName()");
            // Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                this.User.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
            }
			eventLogger.LogDebug("<<< Exiting UpdateDisplayName()");
        }

        private bool Validate()
        {
			eventLogger.LogDebug(">>> Entering Validate()");
            if (!string.IsNullOrEmpty(this.gotcha.Value))
            {
                return false;
            }

            this.CreateStatus = UserCreateStatus.AddUser;
			eventLogger.LogDebug("Validate(): this.CreateStatus → {Arg0}", this.CreateStatus);
            var portalSecurity = PortalSecurity.Instance;

            // Check User Editor
            bool isValid = this.userForm.IsValid;
			eventLogger.LogDebug("Validate(): isValid → {IsValid}", isValid);

            if (isValid)
			eventLogger.LogDebug("<<< Exiting Validate()");
            {
#pragma warning disable CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
                var filterFlags = PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup;
#pragma warning restore CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
                var name = this.User.Username ?? this.User.Email;
				eventLogger.LogDebug("Validate(): name → {Name}", name);
                var cleanUsername = PortalSecurity.Instance.InputFilter(name, filterFlags);
				eventLogger.LogDebug("Validate(): cleanUsername → {CleanUsername}", cleanUsername);
                if (!cleanUsername.Equals(name))
                {
                    this.CreateStatus = UserCreateStatus.InvalidUserName;
                }

                var valid = UserController.Instance.IsValidUserName(name);
				eventLogger.LogDebug("Validate(): valid → {Valid}", valid);

                if (!valid)
                {
                    this.CreateStatus = UserCreateStatus.InvalidUserName;
                }

                var cleanEmail = PortalSecurity.Instance.InputFilter(this.User.Email, filterFlags);
				eventLogger.LogDebug("Validate(): cleanEmail → {CleanEmail}", cleanEmail);
                if (!cleanEmail.Equals(this.User.Email))
                {
                    this.CreateStatus = UserCreateStatus.InvalidEmail;
                }

                var cleanFirstName = PortalSecurity.Instance.InputFilter(this.User.FirstName, filterFlags);
				eventLogger.LogDebug("Validate(): cleanFirstName → {CleanFirstName}", cleanFirstName);
                if (!cleanFirstName.Equals(this.User.FirstName))
                {
                    this.CreateStatus = UserCreateStatus.InvalidFirstName;
                }

                var cleanLastName = PortalSecurity.Instance.InputFilter(this.User.LastName, filterFlags);
				eventLogger.LogDebug("Validate(): cleanLastName → {CleanLastName}", cleanLastName);
                if (!cleanLastName.Equals(this.User.LastName))
                {
                    this.CreateStatus = UserCreateStatus.InvalidLastName;
                }

                if (string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
                {
                    var cleanDisplayName = PortalSecurity.Instance.InputFilter(this.User.DisplayName, filterFlags);
					eventLogger.LogDebug("Validate(): cleanDisplayName → {CleanDisplayName}", cleanDisplayName);
                    if (!cleanDisplayName.Equals(this.User.DisplayName))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }

            if (this.PortalSettings.Registration.RegistrationFormType == 0)
            {
                // Update UserName
                if (this.PortalSettings.Registration.UseEmailAsUserName)
                {
                    this.User.Username = this.User.Email;
                    if (string.IsNullOrEmpty(this.User.DisplayName))
                    {
                        this.User.DisplayName = this.User.Email.Substring(0, this.User.Email.IndexOf("@", StringComparison.Ordinal));
                    }
                }

                // Check Password is valid
                if (!this.PortalSettings.Registration.RandomPassword)
                {
                    // Check Password is Valid
                    if (this.CreateStatus == UserCreateStatus.AddUser && !UserController.ValidatePassword(this.User.Membership.Password))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidPassword;
                    }

                    if (this.PortalSettings.Registration.RequirePasswordConfirm && string.IsNullOrEmpty(this.AuthenticationType))
                    {
                        if (this.User.Membership.Password != this.User.Membership.PasswordConfirm)
                        {
                            this.CreateStatus = UserCreateStatus.PasswordMismatch;
                        }
                    }
                }
                else
                {
                    // Generate a random password for the user
                    this.User.Membership.Password = UserController.GeneratePassword();
                    this.User.Membership.PasswordConfirm = this.User.Membership.Password;
                }
            }
            else
            {
                // Set Username to Email
                if (string.IsNullOrEmpty(this.User.Username))
                {
                    this.User.Username = this.User.Email;
                }

                // Set DisplayName
                if (string.IsNullOrEmpty(this.User.DisplayName))
                {
                    this.User.DisplayName = string.IsNullOrEmpty(this.User.FirstName + " " + this.User.LastName)
                                           ? this.User.Email.Substring(0, this.User.Email.IndexOf("@", StringComparison.Ordinal))
                                           : this.User.FirstName + " " + this.User.LastName;
                }

                // Random Password
                if (string.IsNullOrEmpty(this.User.Membership.Password))
                {
                    // Generate a random password for the user
                    this.User.Membership.Password = UserController.GeneratePassword();
                }

                // Password Confirm
                if (!string.IsNullOrEmpty(this.User.Membership.PasswordConfirm))
                {
                    if (this.User.Membership.Password != this.User.Membership.PasswordConfirm)
                    {
                        this.CreateStatus = UserCreateStatus.PasswordMismatch;
                    }
                }
            }

            // Validate banned password
            var settings = new MembershipPasswordSettings(this.User.PortalID);
			eventLogger.LogDebug("Validate(): settings → {Settings}", settings);

            if (settings.EnableBannedList)
            {
                var m = new MembershipPasswordController(this.listController, this.dataProvider);
				eventLogger.LogDebug("Validate(): m → {M}", m);
                if (m.FoundBannedPassword(this.User.Membership.Password) || this.User.Username == this.User.Membership.Password)
                {
                    this.CreateStatus = UserCreateStatus.BannedPasswordUsed;
                }
            }

            // Validate Profanity
            if (this.PortalSettings.Registration.UseProfanityFilter)
            {
                if (!portalSecurity.ValidateInput(this.User.Username, PortalSecurity.FilterFlag.NoProfanity))
                {
                    this.CreateStatus = UserCreateStatus.InvalidUserName;
                }

                if (!string.IsNullOrEmpty(this.User.DisplayName))
                {
                    if (!portalSecurity.ValidateInput(this.User.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        this.CreateStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }

            // Validate Unique User Name
            UserInfo user = UserController.GetUserByName(this.PortalId, this.User.Username);
			eventLogger.LogDebug("Validate(): user → {User}", user);
            if (user != null)
            {
                if (this.PortalSettings.Registration.UseEmailAsUserName)
                {
                    this.CreateStatus = UserCreateStatus.DuplicateEmail;
                }
                else
                {
                    this.CreateStatus = UserCreateStatus.DuplicateUserName;
                    int i = 1;
                    string userName = null;
					eventLogger.LogDebug("Validate(): userName → {UserName}", userName);
                    while (user != null)
                    {
                        userName = this.User.Username + "0" + i.ToString(CultureInfo.InvariantCulture);
                        user = UserController.GetUserByName(this.PortalId, userName);
                        i++;
                    }

                    this.User.Username = userName;
                }
            }

            // Validate Unique Display Name
            if (this.CreateStatus == UserCreateStatus.AddUser && this.PortalSettings.Registration.RequireUniqueDisplayName)
            {
                user = UserController.Instance.GetUserByDisplayname(this.PortalId, this.User.DisplayName);
                if (user != null)
                {
                    this.CreateStatus = UserCreateStatus.DuplicateDisplayName;
                    int i = 1;
                    string displayName = null;
					eventLogger.LogDebug("Validate(): displayName → {DisplayName}", displayName);
                    while (user != null)
                    {
                        displayName = this.User.DisplayName + " 0" + i.ToString(CultureInfo.InvariantCulture);
                        user = UserController.Instance.GetUserByDisplayname(this.PortalId, displayName);
                        i++;
                    }

                    this.User.DisplayName = displayName;
                }
            }

            // Check Question/Answer
            if (this.CreateStatus == UserCreateStatus.AddUser && MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                if (string.IsNullOrEmpty(this.User.Membership.PasswordQuestion))
                {
                    // Invalid Question
                    this.CreateStatus = UserCreateStatus.InvalidQuestion;
                }

                if (this.CreateStatus == UserCreateStatus.AddUser)
                {
                    if (string.IsNullOrEmpty(this.User.Membership.PasswordAnswer))
                    {
                        // Invalid Question
                        this.CreateStatus = UserCreateStatus.InvalidAnswer;
                    }
                }
            }

            if (this.CreateStatus != UserCreateStatus.AddUser)
            {
                isValid = false;
            }

            return isValid;
        }

        private string GetRedirectUrl(bool checkSetting = true)
        {
			eventLogger.LogDebug(">>> Entering GetRedirectUrl(checkSetting={CheckSetting})", checkSetting);
            var redirectUrl = string.Empty;
            var redirectAfterRegistration = this.PortalSettings.Registration.RedirectAfterRegistration;
			eventLogger.LogDebug("GetRedirectUrl(checkSetting={CheckSetting}): redirectUrl → {RedirectUrl}", checkSetting, redirectUrl);
            if (checkSetting && redirectAfterRegistration > 0)
            {
                // redirect to after registration page
                redirectUrl = this.navigationManager.NavigateURL(redirectAfterRegistration);
            }
            else
            {
                if (this.Request.QueryString["returnurl"] != null)
                {
                    // return to the url passed to register
                    redirectUrl = HttpUtility.UrlDecode(this.Request.QueryString["returnurl"]);

                    // clean the return url to avoid possible XSS attack.
                    redirectUrl = UrlUtils.ValidReturnUrl(redirectUrl);

                    if (redirectUrl.Contains("?returnurl"))
                    {
                        string baseURL = redirectUrl.Substring(
                            0,
                            redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal));
                        string returnURL =
                            redirectUrl.Substring(redirectUrl.IndexOf("?returnurl", StringComparison.Ordinal) + 11);

                        redirectUrl = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                    }
                }

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    // redirect to current page
                    redirectUrl = this.navigationManager.NavigateURL();
                }
            }

			eventLogger.LogDebug("<<< Exiting GetRedirectUrl(checkSetting={CheckSetting})", checkSetting);
            return redirectUrl;
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering RegisterButton_Click(sender={Sender},e={E})", sender, e);
            if ((this.PortalSettings.Registration.UseCaptcha && this.ctlCaptcha.IsValid) || !this.PortalSettings.Registration.UseCaptcha)
            {
                if (this.IsValid)
                {
                    if (this.PortalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                    {
                        this.CreateUser();
                    }
                }
                else
                {
                    if (this.CreateStatus != UserCreateStatus.AddUser)
                    {
                        this.AddLocalizedModuleMessage(UserController.GetUserCreateStatus(this.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting RegisterButton_Click(sender={Sender},e={E})", sender, e);
        }

        private void UserAuthenticated(object sender, UserAuthenticatedEventArgs e)
        {
			eventLogger.LogDebug(">>> Entering UserAuthenticated(sender={Sender},e={E})", sender, e);
            NameValueCollection profileProperties = e.Profile;

            this.User.Username = e.UserToken;
            this.AuthenticationType = e.AuthenticationType;
			eventLogger.LogDebug("UserAuthenticated(sender={Sender},e={E}): this.AuthenticationType → {Arg2}", sender, e, this.AuthenticationType);
            this.UserToken = e.UserToken;

			eventLogger.LogDebug("UserAuthenticated(sender={Sender},e={E}): this.UserToken → {Arg2}", sender, e, this.UserToken);
            foreach (string key in profileProperties)
            {
                switch (key)
                {
                    case "FirstName":
                        this.User.FirstName = profileProperties[key];
                        break;
                    case "LastName":
                        this.User.LastName = profileProperties[key];
                        break;
                    case "Email":
                        this.User.Email = profileProperties[key];
                        break;
                    case "DisplayName":
                        this.User.DisplayName = profileProperties[key];
                        break;
                    default:
                        this.User.Profile.SetProfileProperty(key, profileProperties[key]);
                        break;
                }
            }

            // Generate a random password for the user
            this.User.Membership.Password = UserController.GeneratePassword();

            if (!string.IsNullOrEmpty(this.User.Email))
            {
                this.CreateUser();
            }
            else
            {
                this.AddLocalizedModuleMessage(this.LocalizeText("NoEmail"), ModuleMessage.ModuleMessageType.RedError, true);
                foreach (DnnFormItemBase formItem in this.userForm.Items)
                {
                    formItem.Visible = formItem.DataField == "Email";
                }

                this.userForm.DataBind();
            }
			eventLogger.LogDebug("<<< Exiting UserAuthenticated(sender={Sender},e={E})", sender, e);
        }
    }
}
