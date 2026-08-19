// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Install
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Security;
    using System.Web.Services;
    using System.Web.UI.WebControls;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;
    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The InstallWizard class provides the Installation Wizard for DotNetNuke.</summary>
    public partial class InstallWizard : PageBase, IClientAPICallbackEventHandler
    {
        /// <summary>The install status file name.</summary>
        protected static readonly string StatusFilename = "installstat.log.resources.txt";

        private const string LocalesFile = "/Install/App_LocalResources/Locales.xml";

        // steps shown in UI
        private static readonly IInstallationStep FilePermissionCheck = new FilePermissionCheckStep();
        private static readonly IInstallationStep InstallDatabaseStep = new InstallDatabaseStep();
        private static readonly IInstallationStep InstallExtensionsStep = new InstallExtensionsStep();
        private static readonly IInstallationStep InstallSiteStep = new InstallSiteStep();
        private static readonly IInstallationStep InstallSuperUserStep = new InstallSuperUserStep();

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallWizard));

        // Ordered List of Steps (and weight in percentage) to be executed
        private static readonly IDictionary<IInstallationStep, int> Steps = new Dictionary<IInstallationStep, int>
        {
            { FilePermissionCheck, 10 },
            { new IISVerificationStep(), 5 },
            { InstallDatabaseStep, 20 },
            { InstallExtensionsStep, 25 },
            { new InitializeHostSettingsStep(), 5 },
            { new UpdateLanguagePackStep(), 5 },
            { InstallSiteStep, 20 },
            { InstallSuperUserStep, 5 },
            { new AddFcnModeStep(), 2 },
            { new InstallVersionStep(), 2 },
            { new InstallSslStep(), 2 },
        };

        private static string localResourceFile = "~/Install/App_LocalResources/InstallWizard.aspx.resx";

        private static string[] supportedLanguages;
        private static ConnectionConfig connectionConfig;
        private static string connectionResult;
        private static InstallConfig installConfig;
        private static string culture;
        private static IInstallationStep currentStep;
        private static bool installerRunning;
        private static int installerProgress;
        private static object @lock = new object();

        private readonly DataProvider dataProvider = DataProvider.Instance();
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalTemplateController portalTemplateController;

        private Version dataBaseVersion;
        private XmlDocument installTemplate;

        /// <summary>Initializes a new instance of the <see cref="InstallWizard"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public InstallWizard()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering InstallWizard()");
        }

		Logger.LogDebug("<<< Exiting InstallWizard()");
        /// <summary>Initializes a new instance of the <see cref="InstallWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalTemplateController">The portal template controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public InstallWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IPortalTemplateController portalTemplateController)
            : this(portalController, appStatus, hostSettings, null, portalTemplateController)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="InstallWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalTemplateController">The portal template controller.</param>
        public InstallWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IUserController userController, IPortalTemplateController portalTemplateController)
            : base(portalController, appStatus, hostSettings, userController)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalTemplateController = portalTemplateController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalTemplateController>();
			Logger.LogDebug("InstallWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},portalTemplateController={PortalTemplateController}): portalTemplateController → {PortalTemplateController1}", portalController, appStatus, hostSettings, userController, portalTemplateController, portalTemplateController);
        }

        /// <summary>Gets the current application version.</summary>
        protected Version ApplicationVersion => DotNetNukeContext.Current.Application.Version;

        /// <summary>Gets the database version.</summary>
        protected Version DatabaseVersion => this.dataBaseVersion ??= DataProvider.Instance().GetVersion();

        /// <summary>Gets the base version from the install template.</summary>
        protected Version BaseVersion => Upgrade.GetInstallVersion(this.InstallTemplate);

        /// <summary>Gets the install template.</summary>
        protected XmlDocument InstallTemplate
        {
            get
            {
                if (this.installTemplate == null)
                {
                    this.installTemplate = new XmlDocument { XmlResolver = null };
                    Upgrade.GetInstallTemplate(this.installTemplate);
                }

                return this.installTemplate;
            }
        }

        /// <summary>Gets a value indicating whether localization is supported.</summary>
        protected bool SupportLocalization
        {
            get { return installConfig.SupportLocalization; }
        }

        /// <summary>Gets or sets a value indicating whether the permissions are valid.</summary>
        protected bool PermissionsValid
        {
            get
            {
                bool valid = false;
                if (this.ViewState["PermissionsValid"] != null)
                {
                    valid = Convert.ToBoolean(this.ViewState["PermissionsValid"]);
                }

                return valid;
            }

            set
            {
                this.ViewState["PermissionsValid"] = value;
            }
        }

        /// <summary>Gets or sets the portal id.</summary>
        protected int PortalId
        {
            get
            {
                int portalId = Null.NullInteger;
                if (this.ViewState["PortalId"] != null)
                {
                    portalId = Convert.ToInt32(this.ViewState["PortalId"]);
                }

                return portalId;
            }

            set
            {
                this.ViewState["PortalId"] = value;
            }
        }

        /// <summary>Gets or sets the versions.</summary>
        protected string Versions
        {
            get
            {
                string versions = Null.NullString;
                if (this.ViewState["Versions"] != null)
                {
                    versions = Convert.ToString(this.ViewState["Versions"]);
                }

                return versions;
            }

            set
            {
                this.ViewState["Versions"] = value;
            }
        }

        private static string StatusFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", StatusFilename);
            }
        }

        /// <summary>Runs the installer.</summary>
        [WebMethod]
        public static void RunInstall()
        {
			Logger.LogDebug(">>> Entering RunInstall()");
            installerRunning = false;
            LaunchAutoInstall(Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>());
			Logger.LogDebug("RunInstall(): installerRunning → {InstallerRunning}", installerRunning);
			Logger.LogDebug("<<< Exiting RunInstall()");
        }

        /// <summary>Gets the installation log.</summary>
        /// <param name="startRow">At which line to start obtaining log lines.</param>
        /// <returns>Log string from the provided line number forward.</returns>
        [WebMethod]
        public static object GetInstallationLog(int startRow)
        {
			Logger.LogDebug(">>> Entering GetInstallationLog(startRow={StartRow})", startRow);
            const int maxLines = 500;
            var logFile = InstallController.Instance.InstallerLogName;
            try
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
				Logger.LogDebug("GetInstallationLog(startRow={StartRow}): lines → {Lines}", startRow, lines);
                var sb = new StringBuilder();
				Logger.LogDebug("GetInstallationLog(startRow={StartRow}): sb → {Sb}", startRow, sb);
                var errorLogged = false;
				Logger.LogDebug("GetInstallationLog(startRow={StartRow}): errorLogged → {ErrorLogged}", startRow, errorLogged);
                var lineCount = 0;

                // Important to return empty string to stop retries.
                if (startRow > lines.Count())
                {
                    return string.Empty;
                }

                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("[ERROR]"))
                    {
                        errorLogged = true;

                        // Only append if error is in current lines range.
                        if (i > startRow && lineCount < maxLines)
                        {
                            sb.Append(lines[i]).Append("<br />");
                            lineCount++;

                            // If we have reached the max lines, break out of loop.
                            if (lineCount >= maxLines)
                            {
                                break;
                            }
                        }
                    }
                }

                if (!errorLogged)
                {
                    return Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/InstallWizard.aspx.resx");
                }

                return sb.ToString();
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetInstallationLog(startRow={StartRow}): {Arg1}", startRow, Exception.Message);
				Logger.LogDebug("<<< Exiting GetInstallationLog(startRow={StartRow})", startRow);
                return string.Empty;
            }
        }

        /// <summary>Validates the information provided for the install.</summary>
        /// <param name="installInfo">The information about the installation.</param>
        /// <returns>A value indicating if the input is valid and an error message if not.</returns>
        [WebMethod]
        public static Tuple<bool, string> ValidateInput(Dictionary<string, string> installInfo)
        {
            var result = true;
            var errorMsg = string.Empty;

            // Check Required Fields
            if (installInfo["username"] == string.Empty || installInfo["password"] == string.Empty || installInfo["confirmPassword"] == string.Empty
                 || installInfo["websiteName"] == string.Empty || installInfo["email"] == string.Empty)
            {
                result = false;
                errorMsg = LocalizeStringStatic("InputErrorMissingRequiredFields");
            }
            else if (installInfo["password"] != installInfo["confirmPassword"])
            {
                result = false;
                errorMsg = LocalizeStringStatic("PasswordMismatch");
            }

            if (result)
            {
                UpdateInstallConfig(installInfo);
                new SynchConnectionStringStep().Execute();
            }

            return new Tuple<bool, string>(result, errorMsg);
        }

        /// <summary>Validates file access permissions.</summary>
        /// <returns>Returns if the permissions are valid or an error message.</returns>
        [WebMethod]
        public static Tuple<bool, string> ValidatePermissions()
        {
            var permissionsValid = true;
            var errorMessage = string.Empty;

            var verifiers = new List<FileSystemPermissionVerifier>
                                {
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~"), 3),
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~/App_Data"), 3),
                                };

            var failedList = verifiers.Where(v => !v.VerifyAll()).ToArray();
            if (failedList.Any())
            {
                permissionsValid = false;
            }

            if (!permissionsValid)
            {
                errorMessage = string.Format(LocalizeStringStatic("FileAndFolderPermissionCheckFailed"), string.Join("; ", from v in verifiers select v.BasePath));
            }

            return new Tuple<bool, string>(permissionsValid, errorMessage);
        }

        /// <summary>Validates the password for password requirements.</summary>
        /// <param name="password">The entered password.</param>
        /// <returns>A value indicating whether the password is valid.</returns>
        [WebMethod]
        public static bool ValidatePassword(string password)
        {
			Logger.LogDebug(">>> Entering ValidatePassword(password=<redacted>)");
            // Check Length
            var result = !(password.Length < Membership.MinRequiredPasswordLength);
			Logger.LogDebug("ValidatePassword(password=<redacted>): result → {Result}", result);

            // Check against regex
            if (!string.IsNullOrEmpty(Membership.PasswordStrengthRegularExpression) && !Regex.IsMatch(password, Membership.PasswordStrengthRegularExpression))
            {
                result = false;
            }

            // Check non-alphaNumeric
            var nonAlnumCount = password.Where((t, i) => !char.IsLetterOrDigit(password, i)).Count();
			Logger.LogDebug("ValidatePassword(password=<redacted>): nonAlnumCount → {NonAlnumCount}", nonAlnumCount);
            if (nonAlnumCount < Membership.MinRequiredNonAlphanumericCharacters)
            {
                result = false;
            }

			Logger.LogDebug("<<< Exiting ValidatePassword(password=<redacted>)");
            return result;
        }

        /// <summary>Checks if the application can connect to the database on load.</summary>
        /// <returns>A value indicating whether the application can connect to the database.</returns>
        [WebMethod]
        public static bool VerifyDatabaseConnectionOnLoad()
        {
			Logger.LogDebug(">>> Entering VerifyDatabaseConnectionOnLoad()");
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
			Logger.LogDebug("OnLoad(e={E}): installerRunning → {InstallerRunning}", e, installerRunning);
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
			Logger.LogDebug("<<< Exiting VerifyDatabaseConnectionOnLoad()");
            return CheckDatabaseConnection();
        }

        /// <summary>Verifies that the application can connect to the database with the provided settings.</summary>
        /// <param name="installInfo">The install information.</param>
        /// <returns>Returns if the connection is valid and an error message if not.</returns>
        [WebMethod]
        public static Tuple<bool, string> VerifyDatabaseConnection(Dictionary<string, string> installInfo)
        {
            var connectionConfig = new ConnectionConfig();

            // Database Config
            if (installInfo["databaseSetup"] == "standard")
            {
                connectionConfig = InstallWizard.connectionConfig;
            }
            else
            {
                connectionConfig.Server = installInfo["databaseServerName"];
                connectionConfig.Qualifier = installInfo["databaseObjectQualifier"];
                connectionConfig.Integrated = installInfo["databaseSecurity"] == "integrated";
                connectionConfig.User = string.IsNullOrEmpty(installInfo["databaseUsername"]) ? null : installInfo["databaseUsername"];
                connectionConfig.Password = string.IsNullOrEmpty(installInfo["databasePassword"]) ? null : installInfo["databasePassword"];
                connectionConfig.RunAsDbowner = installInfo["databaseRunAsOwner"] == "on";

                if (installInfo["databaseType"] == "express")
                {
                    connectionConfig.File = installInfo["databaseFilename"];
                    connectionConfig.Database = string.Empty;
                }
                else
                {
                    connectionConfig.Database = installInfo["databaseName"];
                    connectionConfig.File = string.Empty;
                }
            }

            var result = CheckDatabaseConnection(connectionConfig);
            var validConnection = !result.StartsWith("ERROR:");
            if (validConnection)
            {
                UpdateDatabaseInstallConfig(installInfo);
                InstallWizard.connectionConfig = connectionConfig;
                installConfig.Connection = connectionConfig;
            }

            return new Tuple<bool, string>(validConnection, result);
        }

        /// <summary>Indicate if the Installer is running.</summary>
        /// <returns>True or False.</returns>
        /// <remarks>Checks the local static variable or the existence of status file.</remarks>
        [WebMethod]
        public static bool IsInstallerRunning()
        {
			Logger.LogDebug(">>> Entering IsInstallerRunning()");
            bool isRunning;

            if (installerRunning || InstallBlocker.Instance.IsInstallInProgress())
            {
                isRunning = true;
            }
            else if (File.Exists(StatusFile))
            {
                var file = new FileInfo(StatusFile);
				Logger.LogDebug("IsInstallerRunning(): file → {File}", file);
                isRunning = file.Length > 0;
            }
            else
            {
                isRunning = false;
            }

			Logger.LogDebug("<<< Exiting IsInstallerRunning()");
            return isRunning;
        }

        /// <summary>Raises a client api callback event.</summary>
        /// <param name="eventArgument">The event arguments.</param>
        /// <returns>A string representing the result of the action.</returns>
        public string RaiseClientAPICallbackEvent(string eventArgument)
        {
			Logger.LogDebug(">>> Entering RaiseClientAPICallbackEvent(eventArgument={EventArgument})", eventArgument);
			Logger.LogDebug("<<< Exiting RaiseClientAPICallbackEvent(eventArgument={EventArgument})", eventArgument);
            return this.ProcessAction(eventArgument);
        }

        /// <summary>Runs when there is a ClientCallback.</summary>
        /// <param name="someAction">The action to perform.</param>
        /// <returns>A string representing the result of the action.</returns>
        public string ProcessAction(string someAction)
        {
			Logger.LogDebug(">>> Entering ProcessAction(someAction={SomeAction})", someAction);
            // First check that we are not being targeted by an AJAX HttpPost, so get the current DB version
            string strProviderPath = this.dataProvider.GetProviderPath();
			Logger.LogDebug("ProcessAction(someAction={SomeAction}): strProviderPath → {StrProviderPath}", someAction, strProviderPath);
            string nextVersion = this.GetNextScriptVersion(strProviderPath, this.DatabaseVersion);
            if (someAction != nextVersion)
            {
                Exceptions.Exceptions.LogException(new Exception("Attempt made to run a Database Script - Possible Attack"));
                return "Error: Possible Attack";
            }

            if (someAction == this.GetBaseDatabaseVersion())
            {
                string result = this.InstallDatabase();
				Logger.LogDebug("ProcessAction(someAction={SomeAction}): result → {Result}", someAction, result);
                if (result == "Done")
                {
                    // Complete Installation
                    Upgrade.UpgradeApplication();
                }

                return result;
            }
            else if (someAction.Contains("."))
            {
                // Upgrade Database
                string result = this.InstallVersion(someAction);
                if (result == "Done")
                {
                    ArrayList arrVersions = Upgrade.GetUpgradeScripts(strProviderPath, this.BaseVersion);
					Logger.LogDebug("ProcessAction(someAction={SomeAction}): arrVersions → {ArrVersions}", someAction, arrVersions);
                    string strErrorMessage = Null.NullString;
                    for (int i = 0; i <= arrVersions.Count - 1; i++)
                    {
                        string strVersion = Path.GetFileNameWithoutExtension(Convert.ToString(arrVersions[i]));
                        var version = new Version(strVersion);
						Logger.LogDebug("ProcessAction(someAction={SomeAction}): version → {Version}", someAction, version);

                        strErrorMessage += Upgrade.UpgradeApplication(strProviderPath, version, false);

                        // delete files which are no longer used
                        strErrorMessage += Upgrade.DeleteFiles(strProviderPath, version, false);

                        // execute config file updates
                        strErrorMessage += Upgrade.UpdateConfig(strProviderPath, version, false);
                    }

                    // Complete Installation
                    Upgrade.UpgradeApplication();
                    if (!string.IsNullOrEmpty(strErrorMessage))
                    {
                        result = "ERROR: " + strErrorMessage;
                    }
                }

                return result;
            }
            else
            {
				Logger.LogDebug("<<< Exiting ProcessAction(someAction={SomeAction})", someAction);
                return "Done";
            }
        }

        /// <summary>GetBaseDataBaseVersion gets the Base Database Version as a string.</summary>
        /// <returns>The database version.</returns>
        protected string GetBaseDatabaseVersion()
        {
			Logger.LogDebug(">>> Entering GetBaseDatabaseVersion()");
			Logger.LogDebug("<<< Exiting GetBaseDatabaseVersion()");
            return Upgrade.GetStringVersion(this.BaseVersion);
        }

        /// <summary>LocalizeString is a helper function for localizing strings.</summary>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized string.</returns>
        protected string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, localResourceFile, culture);
        }

        /// <inheritdoc />
        protected override void OnError(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnError(e={E})", e);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
			Logger.LogDebug("<<< Exiting OnError(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            // if previous config deleted create new empty one
            string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
			Logger.LogDebug("OnInit(e={E}): installConfig → {InstallConfig}", e, installConfig);
            if (!File.Exists(installConfig))
            {
                File.Copy(installConfig + ".resources", installConfig);
            }

            GetInstallerLocales(this.appStatus);
            if (!this.Page.IsPostBack || InstallWizard.installConfig == null)
            {
                InstallWizard.installConfig = InstallController.Instance.GetInstallConfig();
                connectionConfig = InstallWizard.installConfig.Connection;
				Logger.LogDebug("OnInit(e={E}): connectionConfig → {ConnectionConfig}", e, connectionConfig);
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (!this.SupportLocalization)
            {
                this.languageFlags.Visible = false;
                this.languagesRow.Attributes.Add("style", "display: none");
            }

            this.passwordContainer.CssClass = "password-strength-container";
            this.txtPassword.CssClass = "password-strength";

            var options = new DnnPaswordStrengthOptions();
			Logger.LogDebug("OnPreRender(e={E}): options → {Options}", e, options);
            var optionsAsJsonString = Json.Serialize(options);
			Logger.LogDebug("OnPreRender(e={E}): optionsAsJsonString → {OptionsAsJsonString}", e, optionsAsJsonString);
            var script = string.Format(
                "dnn.initializePasswordStrength('.{0}', {1});{2}",
                "password-strength",
                optionsAsJsonString,
                Environment.NewLine);

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "PasswordStrength", script, true);

            this.txtConfirmPassword.CssClass = "password-confirm";
            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = "#" + this.passwordContainer.ClientID + " input[type=password]",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnFormPassword",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			Logger.LogDebug("OnPreRender(e={E}): confirmOptionsAsJsonString → {ConfirmOptionsAsJsonString}", e, confirmOptionsAsJsonString);
            var confirmScript = string.Format("dnn.initializePasswordComparer({0});{1}", confirmOptionsAsJsonString, Environment.NewLine);
			Logger.LogDebug("OnPreRender(e={E}): confirmScript → {ConfirmScript}", e, confirmScript);

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", confirmScript, true);
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                this.Response.Redirect("Install.aspx", true);
            }

            this.SetBrowserLanguage();
            this.LocalizePage();

            base.OnLoad(e);
            this.visitSite.Click += this.VisitSiteClick;

            // Create Status Files
            if (!File.Exists(StatusFile))
            {
                File.CreateText(StatusFile).Close();
            }

            if (HttpContext.Current.Request.RawUrl.EndsWith("&initiateinstall=1"))
            {
                var synchConnectionString = new SynchConnectionStringStep();
                synchConnectionString.Execute();
                this.Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&initiateinstall=1", "&executeinstall=1"), true);
            }
            else if (HttpContext.Current.Request.RawUrl.EndsWith("&executeinstall=1"))
            {
                try
                {
                    installerRunning = true;
                    LaunchAutoInstall(this.appStatus);
                }
                catch (Exception)
                {
                    // Redirect back to first page
                    this.Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&executeinstall=1", string.Empty), true);
                }
            }
            else if (!this.Page.IsPostBack)
            {
                if (installerRunning)
                {
                    LaunchAutoInstall(this.appStatus);
                }
                else
                {
                    this.SetupDatabaseInfo();
                    this.BindLanguageList();
                    this.BindTemplates();

                    if (CheckDatabaseConnection())
                    {
                        this.Initialise();
                    }
                    else
                    {
                        // Install but connection string not configured to point at a valid SQL Server
                        UpdateMachineKey(this.appStatus);
                    }

                    if (!Regex.IsMatch(this.Request.Url.Host, "^([a-zA-Z0-9.-]+)$"))
                    {
                        this.lblError.Visible = true;
                        this.lblError.Text = Localization.GetString("HostWarning", localResourceFile);
                    }

                    // ensure web.config is not read-only
                    var configPath = this.Server.MapPath("~/web.config");
                    try
                    {
                        var attributes = File.GetAttributes(configPath);

                        // file is read only
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            attributes = attributes & ~FileAttributes.ReadOnly;
                            File.SetAttributes(configPath, attributes);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.lblError.Visible = true;
                        this.lblError.Text = ex.ToString();
                        return;
                    }

                    // Ensure connection strings are in synch
                    var synchConnectionString = new SynchConnectionStringStep();
                    synchConnectionString.Execute();
                    if (synchConnectionString.Status == StepStatus.AppRestart)
                    {
                        this.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                    }

                    this.txtUsername.Text = installConfig.SuperUser.UserName;
                    this.txtEmail.Text = installConfig.SuperUser.Email;
                    if (installConfig.Portals.Count > 0)
                    {
                        this.txtWebsiteName.Text = installConfig.Portals[0].PortalName;

                        // TODO Language and Template
                    }

                    this.valEmailValid.ValidationExpression = Globals.glbEmailRegEx;

                    this.chkWebsiteSsl.Checked = this.Request.IsSecureConnection;
                }
            }
        }

        private static void LaunchAutoInstall(IApplicationStatusInfo appStatus)
        {
			Logger.LogDebug(">>> Entering LaunchAutoInstall(appStatus={AppStatus})", appStatus);
            if (appStatus.Status == UpgradeStatus.None)
            {
                HttpContext.Current.Response.Redirect("~/");
                return;
            }

            // Get current Script time-out
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            // Set Script timeout to MAX value
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

            if (culture != null)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }

            Install(appStatus);

            // restore Script timeout
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
			Logger.LogDebug("<<< Exiting LaunchAutoInstall(appStatus={AppStatus})", appStatus);
        }

        private static void Install(IApplicationStatusInfo appStatus)
        {
			Logger.LogDebug(">>> Entering Install(appStatus={AppStatus})", appStatus);
            // bail out early if we are already running
            if (installerRunning || InstallBlocker.Instance.IsInstallInProgress() || (appStatus.Status != UpgradeStatus.Install))
            {
                return;
            }

            var percentForEachStep = 100 / Steps.Count;
            var useGenericPercent = false;
			Logger.LogDebug("Install(appStatus={AppStatus}): useGenericPercent → {UseGenericPercent}", appStatus, useGenericPercent);
            var totalPercent = Steps.Sum(step => step.Value);
			Logger.LogDebug("Install(appStatus={AppStatus}): totalPercent → {TotalPercent}", appStatus, totalPercent);
            if (totalPercent != 100)
            {
                useGenericPercent = true;
            }

            installerRunning = true;
            installerProgress = 0;
            foreach (var step in Steps)
            {
			Logger.LogDebug("Install(appStatus={AppStatus}): installerRunning → {InstallerRunning}", appStatus, installerRunning);
			Logger.LogDebug("Install(appStatus={AppStatus}): installerProgress → {InstallerProgress}", appStatus, installerProgress);
                currentStep = step.Key;

				Logger.LogDebug("Install(appStatus={AppStatus}): currentStep → {CurrentStep}", appStatus, currentStep);
                try
                {
                    currentStep.Activity += CurrentStepActivity;
                    currentStep.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("WIZARD ERROR:" + ex);
                    CurrentStepActivity("ERROR:" + ex.Message);
                    installerRunning = false;
                    return;
                }

                switch (currentStep.Status)
                {
                    case StepStatus.AppRestart:
                        installerRunning = false;
                        HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                        break;
                    default:
                        if (currentStep.Status != StepStatus.Done)
                        {
                            CurrentStepActivity(string.Format(
                                Localization.GetString("ErrorInStep", "~/Install/App_LocalResources/InstallWizard.aspx.resx"),
                                currentStep.Errors.Count > 0 ? string.Join(",", currentStep.Errors.ToArray()) : currentStep.Details));

                            installerRunning = false;
                            return;
                        }

                        break;
                }

                if (useGenericPercent)
                {
                    installerProgress += percentForEachStep;
                }
                else
                {
                    installerProgress += step.Value;
                }
            }

            currentStep = null;

            installerProgress = 100;
            CurrentStepActivity(Localization.GetString("InstallationDone", "~/Install/App_LocalResources/InstallWizard.aspx.resx"));

            // indicate we are done
            installerRunning = false;
			Logger.LogDebug("<<< Exiting Install(appStatus={AppStatus})", appStatus);
        }

        private static void CurrentStepActivity(string status)
        {
			Logger.LogDebug(">>> Entering CurrentStepActivity(status={Status})", status);
            var percentage = (currentStep == null) ? installerProgress : installerProgress + (currentStep.Percentage / Steps.Count);
            var obj = new
			Logger.LogDebug("CurrentStepActivity(status={Status}): percentage → {Percentage}", status, percentage);
            {
                progress = percentage,
                details = status,
                check0 = FilePermissionCheck.Status.ToString() + (FilePermissionCheck.Errors.Count == 0 ? string.Empty : " Errors " + FilePermissionCheck.Errors.Count),
                check1 = InstallDatabaseStep.Status.ToString() + (InstallDatabaseStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallDatabaseStep.Errors.Count),
                check2 = InstallExtensionsStep.Status.ToString() + (InstallExtensionsStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallExtensionsStep.Errors.Count),
                check3 = InstallSiteStep.Status.ToString() + (InstallSiteStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallSiteStep.Errors.Count),
                check4 = InstallSuperUserStep.Status.ToString() + (InstallSuperUserStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallSuperUserStep.Errors.Count),
            };
			Logger.LogDebug("CurrentStepActivity(status={Status}): progress → {Progress}", status, progress);

            try
            {
                lock (@lock)
                {
                    using (var sw = new StreamWriter(StatusFile, false))
                    {
                        sw.WriteLine(obj.ToJson());
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CurrentStepActivity(status={Status}): {Arg1}", status, Exception.Message);
                // TODO - do something
            }
			Logger.LogDebug("<<< Exiting CurrentStepActivity(status={Status})", status);
        }

        private static void GetInstallerLocales(IApplicationStatusInfo appStatus)
        {
			Logger.LogDebug(">>> Entering GetInstallerLocales(appStatus={AppStatus})", appStatus);
            var filePath = appStatus.ApplicationMapPath + LocalesFile.Replace("/", @"\");

            if (File.Exists(filePath))
            {
				Logger.LogDebug("GetInstallerLocales(appStatus={AppStatus}): filePath → {FilePath}", appStatus, filePath);
                var doc = new XPathDocument(filePath);
				Logger.LogDebug("GetInstallerLocales(appStatus={AppStatus}): doc → {Doc}", appStatus, doc);
                var languages = doc.CreateNavigator().Select("root/language");
				Logger.LogDebug("GetInstallerLocales(appStatus={AppStatus}): languages → {Languages}", appStatus, languages);

                if (languages.Count > 0)
                {
                    supportedLanguages = new string[languages.Count];
                    var i = 0;
					Logger.LogDebug("GetInstallerLocales(appStatus={AppStatus}): supportedLanguages → {SupportedLanguages}", appStatus, supportedLanguages);
                    foreach (XPathNavigator nav in languages)
                    {
                        if (nav.NodeType != XPathNodeType.Comment)
                        {
                            supportedLanguages.SetValue(nav.GetAttribute("key", string.Empty), i);
                        }

                        i++;
                    }
                }
                else
                {
                    supportedLanguages = new string[1];
                    supportedLanguages.SetValue("en-US", 0);
                }
            }
            else
            {
                supportedLanguages = new string[1];
                supportedLanguages.SetValue("en-US", 0);
			Logger.LogDebug("<<< Exiting GetInstallerLocales(appStatus={AppStatus})", appStatus);
            }
        }

        private static string LocalizeStringStatic(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeStringStatic(key={Key})", key);
			Logger.LogDebug("<<< Exiting LocalizeStringStatic(key={Key})", key);
            return Localization.GetString(key, localResourceFile, culture);
        }

        private static bool CheckDatabaseConnection()
        {
			Logger.LogDebug(">>> Entering CheckDatabaseConnection()");
            var success = false;
            connectionResult = CheckDatabaseConnection(connectionConfig);
            if (!connectionResult.StartsWith("ERROR:"))
            {
                success = true;
            }

			Logger.LogDebug("<<< Exiting CheckDatabaseConnection()");
			Logger.LogDebug("<<< Exiting CheckDatabaseConnection(connectionConfig={ConnectionConfig})", connectionConfig);
            return success;
        }

        private static string CheckDatabaseConnection(ConnectionConfig connectionConfig)
        {
            connectionResult = InstallController.Instance.TestDatabaseConnection(connectionConfig);
            if (connectionResult.StartsWith("ERROR:"))
            {
                return connectionResult;
            }

            var connectionString = connectionResult;
            var details = Localization.GetString("IsAbleToPerformDatabaseActionsCheck", localResourceFile);
            if (!InstallController.Instance.IsAbleToPerformDatabaseActions(connectionString))
            {
                connectionResult = "ERROR: " + string.Format(CultureInfo.CurrentCulture, Localization.GetString("IsAbleToPerformDatabaseActions", localResourceFile), details);
            }

            // database actions check-running sql 2008 or higher
            details = Localization.GetString("IsValidSqlServerVersionCheck", localResourceFile);
            if (!InstallController.Instance.IsValidSqlServerVersion(connectionString))
            {
                connectionResult = "ERROR: " + string.Format(CultureInfo.CurrentCulture, Localization.GetString("IsValidSqlServerVersion", localResourceFile), details);
            }

            return connectionResult;
        }

        private static void UpdateMachineKey(IApplicationStatusInfo appStatus)
        {
			Logger.LogDebug(">>> Entering UpdateMachineKey(appStatus={AppStatus})", appStatus);
            var installationDate = Config.GetSetting("InstallationDate");

			Logger.LogDebug("UpdateMachineKey(appStatus={AppStatus}): installationDate → {InstallationDate}", appStatus, installationDate);
            if (string.IsNullOrEmpty(installationDate))
            {
                string strError = Config.UpdateMachineKey(appStatus);
                if (string.IsNullOrEmpty(strError))
                {
                    // send a new request to the application to initiate step 2
                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                }
                else
                {
                    // 403-3 Error - Redirect to ErrorPage
                    var strUrl = "~/ErrorPage.aspx?status=403_3&error=" + strError;
					Logger.LogDebug("UpdateMachineKey(appStatus={AppStatus}): strUrl → {StrUrl}", appStatus, strUrl);
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer(strUrl);
                }
            }
			Logger.LogDebug("<<< Exiting UpdateMachineKey(appStatus={AppStatus})", appStatus);
        }

        /// <summary>Updates and synchronizes DotNetNuke.install.config with Web.config.</summary>
        /// <param name="installInfo">The information about this installation.</param>
        private static void UpdateInstallConfig(Dictionary<string, string> installInfo)
        {
			Logger.LogDebug(">>> Entering UpdateInstallConfig(installInfo={InstallInfo})", installInfo);
            installConfig = new InstallConfig
            {
				Logger.LogDebug("UpdateInstallConfig(installInfo={InstallInfo}): installConfig → {InstallConfig}", installInfo, installConfig);
                SuperUser = new SuperUserConfig
                {
                    UserName = installInfo["username"],
                    Password = installInfo["password"],
                    Locale = culture,
                    Email = installInfo["email"],
                    FirstName = "SuperUser",
                    LastName = "Account",
                },
                InstallCulture = installInfo["language"],
            };

            // Website Portal Config
            var portalConfig = new PortalConfig()
            {
                PortalName = installInfo["websiteName"],
                IsSsl = bool.Parse(installInfo["websiteSsl"]),
                TemplateFileName = installInfo["template"],
                IsChild = false,
            };
            installConfig.Portals = new List<PortalConfig>() { portalConfig };

            InstallController.Instance.SetInstallConfig(installConfig);
			Logger.LogDebug("<<< Exiting UpdateInstallConfig(installInfo={InstallInfo})", installInfo);
        }

        private static void UpdateDatabaseInstallConfig(Dictionary<string, string> installInfo)
        {
			Logger.LogDebug(">>> Entering UpdateDatabaseInstallConfig(installInfo={InstallInfo})", installInfo);
            // Database Config
            if (installInfo["databaseSetup"] == "advanced")
            {
                connectionConfig = new ConnectionConfig()
                {
				Logger.LogDebug("UpdateDatabaseInstallConfig(installInfo={InstallInfo}): connectionConfig → {ConnectionConfig}", installInfo, connectionConfig);
                    Server = installInfo["databaseServerName"],
                    Qualifier = installInfo["databaseObjectQualifier"],
                    Integrated = installInfo["databaseSecurity"] == "integrated",
                    User = installInfo["databaseUsername"],
                    Password = installInfo["databasePassword"],
                    RunAsDbowner = installInfo["databaseRunAsOwner"] == "on",
                };

                if (installInfo["databaseType"] == "express")
                {
                    connectionConfig.File = installInfo["databaseFilename"];
					Logger.LogDebug("UpdateDatabaseInstallConfig(installInfo={InstallInfo}): connectionConfig.File → {Arg1}", installInfo, connectionConfig.File);
					Logger.LogDebug("UpdateDatabaseInstallConfig(installInfo={InstallInfo}): connectionConfig.Database → {Arg1}", installInfo, connectionConfig.Database);
                    connectionConfig.Database = string.Empty;
                }
                else
                {
                    connectionConfig.Database = installInfo["databaseName"];
                    connectionConfig.File = string.Empty;
                }
            }

            installConfig.Connection = connectionConfig;
            InstallController.Instance.SetInstallConfig(installConfig);
			Logger.LogDebug("<<< Exiting UpdateDatabaseInstallConfig(installInfo={InstallInfo})", installInfo);
        }

        private void VisitSiteClick(object sender, EventArgs eventArgs)
        {
			Logger.LogDebug(">>> Entering VisitSiteClick(sender={Sender},eventArgs={EventArgs})", sender, eventArgs);
            // Delete the status file.
            try
            {
                File.Delete(StatusFile);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in VisitSiteClick(sender={Sender},eventArgs={EventArgs}): {Arg2}", sender, eventArgs, Exception.Message);
                // Do nothing
            }

            // delete the initial install config -check readonly status first
            try
            {
                string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
				Logger.LogDebug("VisitSiteClick(sender={Sender},eventArgs={EventArgs}): installConfig → {InstallConfig}", sender, eventArgs, installConfig);
                if (File.Exists(installConfig))
                {
                    // make sure file is not read-only
                    File.SetAttributes(installConfig, FileAttributes.Normal);
                    File.Delete(installConfig);
                }
            }
            catch (Exception)
            {
                // Do nothing
            }

            Config.Touch(this.appStatus);
            HttpContext.Current.Response.Redirect("../Default.aspx");
			Logger.LogDebug("<<< Exiting VisitSiteClick(sender={Sender},eventArgs={EventArgs})", sender, eventArgs);
        }

        private void SetBrowserLanguage()
        {
			Logger.LogDebug(">>> Entering SetBrowserLanguage()");
            string cultureCode;
            if (string.IsNullOrEmpty(this.PageLocale.Value) && string.IsNullOrEmpty(culture))
            {
                cultureCode = !string.IsNullOrEmpty(HttpContext.Current.Request.Params.Get("culture")) ? HttpContext.Current.Request.Params.Get("culture") : TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(supportedLanguages);
            }
            else if (string.IsNullOrEmpty(this.PageLocale.Value) && !string.IsNullOrEmpty(culture))
            {
                cultureCode = culture;
            }
            else
            {
                cultureCode = this.PageLocale.Value;
            }

            this.PageLocale.Value = cultureCode;
            culture = cultureCode;
			Logger.LogDebug("SetBrowserLanguage(): culture → {Culture}", culture);

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
			Logger.LogDebug("<<< Exiting SetBrowserLanguage()");
        }

        private string GetNextScriptVersion(string strProviderPath, Version currentVersion)
        {
			Logger.LogDebug(">>> Entering GetNextScriptVersion(strProviderPath={StrProviderPath},currentVersion={CurrentVersion})", strProviderPath, currentVersion);
            var strNextVersion = "Done";

			Logger.LogDebug("GetNextScriptVersion(strProviderPath={StrProviderPath},currentVersion={CurrentVersion}): strNextVersion → {StrNextVersion}", strProviderPath, currentVersion, strNextVersion);
            if (currentVersion == null)
            {
                strNextVersion = this.GetBaseDatabaseVersion();
            }
            else
            {
                var strScriptVersion = Null.NullString;
				Logger.LogDebug("GetNextScriptVersion(strProviderPath={StrProviderPath},currentVersion={CurrentVersion}): strScriptVersion → {StrScriptVersion}", strProviderPath, currentVersion, strScriptVersion);
                var arrScripts = Upgrade.GetUpgradeScripts(strProviderPath, currentVersion);
				Logger.LogDebug("GetNextScriptVersion(strProviderPath={StrProviderPath},currentVersion={CurrentVersion}): arrScripts → {ArrScripts}", strProviderPath, currentVersion, arrScripts);

                if (arrScripts.Count > 0)
                {
                    // First Script is next script
                    strScriptVersion = Path.GetFileNameWithoutExtension(Convert.ToString(arrScripts[0]));
                }

                if (!string.IsNullOrEmpty(strScriptVersion))
                {
                    strNextVersion = strScriptVersion;
                }
            }

			Logger.LogDebug("<<< Exiting GetNextScriptVersion(strProviderPath={StrProviderPath},currentVersion={CurrentVersion})", strProviderPath, currentVersion);
            return strNextVersion;
        }

        private void Initialise()
        {
			Logger.LogDebug(">>> Entering Initialise()");
            if (this.TestDataBaseInstalled())
            {
                // running current version, so redirect to site home page
                this.Response.Redirect("~/Default.aspx", true);
            }
            else
            {
                if (this.DatabaseVersion > new Version(0, 0, 0))
                {
                    // Upgrade
                    this.lblIntroDetail.Text = string.Format(this.LocalizeString("Upgrade"), Upgrade.GetStringVersion(this.DatabaseVersion));
                }
                else
                {
                    // Install
                    UpdateMachineKey(this.appStatus);
                }
            }
			Logger.LogDebug("<<< Exiting Initialise()");
        }

        private string InstallDatabase()
        {
			Logger.LogDebug(">>> Entering InstallDatabase()");
            var strProviderPath = this.dataProvider.GetProviderPath();
            var strErrorMessage = strProviderPath.StartsWith("ERROR:")
                ? strProviderPath
                : Upgrade.InstallDatabase(this.BaseVersion, strProviderPath, this.InstallTemplate, false);
				Logger.LogDebug("InstallDatabase(): strProviderPath → {StrProviderPath}", strProviderPath);
            if (string.IsNullOrEmpty(strErrorMessage))
            {
                // Get Next Version
                strErrorMessage = this.GetNextScriptVersion(strProviderPath, this.BaseVersion);
            }
            else if (!strErrorMessage.StartsWith("ERROR:"))
            {
                strErrorMessage = "ERROR: " + string.Format(this.LocalizeString("ScriptError"), Upgrade.GetLogFile(strProviderPath, this.BaseVersion));
            }

			Logger.LogDebug("<<< Exiting InstallDatabase()");
            return strErrorMessage;
        }

        private string InstallVersion(string strVersion)
        {
			Logger.LogDebug(">>> Entering InstallVersion(strVersion={StrVersion})", strVersion);
            var strErrorMessage = Null.NullString;
            var version = new Version(strVersion);
			Logger.LogDebug("InstallVersion(strVersion={StrVersion}): version → {Version}", strVersion, version);
            var strScriptFile = Null.NullString;
			Logger.LogDebug("InstallVersion(strVersion={StrVersion}): strScriptFile → {StrScriptFile}", strVersion, strScriptFile);
            var strProviderPath = this.dataProvider.GetProviderPath();
			Logger.LogDebug("InstallVersion(strVersion={StrVersion}): strProviderPath → {StrProviderPath}", strVersion, strProviderPath);
			Logger.LogDebug("InstallVersion(strVersion={StrVersion}): strErrorMessage → {StrErrorMessage}", strVersion, strErrorMessage);
            if (!strProviderPath.StartsWith("ERROR:"))
            {
                // Install Version
                strScriptFile = Upgrade.GetScriptFile(strProviderPath, version);
                strErrorMessage += Upgrade.UpgradeVersion(strScriptFile, false);
                this.Versions += "," + strVersion;
            }
            else
            {
                strErrorMessage = strProviderPath;
            }

            if (string.IsNullOrEmpty(strErrorMessage))
            {
                // Get Next Version
                strErrorMessage = this.GetNextScriptVersion(strProviderPath, version);
            }
            else
            {
                strErrorMessage = "ERROR: (see " + (Path.GetFileName(strScriptFile) ?? string.Empty)
                    .Replace("." + Upgrade.DefaultProvider, ".log") + " for more information)";
            }

			Logger.LogDebug("<<< Exiting InstallVersion(strVersion={StrVersion})", strVersion);
            return strErrorMessage;
        }

        private void LocalizePage()
        {
			Logger.LogDebug(">>> Entering LocalizePage()");
            this.Page.Title = this.LocalizeString("PageTitle");
            this.lblIntroDetail.Text = this.LocalizeString("IntroDetail");
			Logger.LogDebug("<<< Exiting LocalizePage()");
        }

        /// <summary>TestDataBaseInstalled checks whether the Database is the current version.</summary>
        /// <returns>A value indicating whether the database is the current version.</returns>
        private bool TestDataBaseInstalled()
        {
			Logger.LogDebug(">>> Entering TestDataBaseInstalled()");
            var success = !(this.DatabaseVersion == null || this.DatabaseVersion.Major != this.ApplicationVersion.Major || this.DatabaseVersion.Minor != this.ApplicationVersion.Minor || this.DatabaseVersion.Build != this.ApplicationVersion.Build);
			Logger.LogDebug("TestDataBaseInstalled(): success → {Success}", success);
			Logger.LogDebug("<<< Exiting TestDataBaseInstalled()");
            return success;
        }

        private void SetupDatabaseInfo()
        {
			Logger.LogDebug(">>> Entering SetupDatabaseInfo()");
            // Try to use connection information in DotNetNuke.Install.Config. If not found use from web.config
            connectionConfig = installConfig.Connection;
            if (connectionConfig == null || string.IsNullOrEmpty(connectionConfig.Server))
            {
                connectionConfig = InstallController.Instance.GetConnectionFromWebConfig();
            }

            if (connectionConfig != null)
            {
                this.txtDatabaseServerName.Text = connectionConfig.Server;

                // SQL Express Or SQL Server
                if (!string.IsNullOrEmpty(connectionConfig.File))
                {
                    this.txtDatabaseFilename.Text = connectionConfig.File;
                    this.databaseType.SelectedIndex = 0;
                }
                else
                {
                    this.txtDatabaseName.Text = connectionConfig.Database;
                    this.databaseType.SelectedIndex = 1;
                }

                // Integrated or Custom
                if (connectionConfig.Integrated)
                {
                    this.databaseSecurityType.SelectedIndex = 0;
                }
                else
                {
                    this.databaseSecurityType.SelectedIndex = 1;
                    this.txtDatabaseUsername.Text = connectionConfig.User;
                    this.txtDatabasePassword.Text = connectionConfig.Password;
                }

                // Owner or Not
                this.databaseRunAs.Checked = connectionConfig.RunAsDbowner;
            }
			Logger.LogDebug("<<< Exiting SetupDatabaseInfo()");
        }

        private void BindLanguageList()
        {
			Logger.LogDebug(">>> Entering BindLanguageList()");
            try
            {
                var myResponseReader = UpdateService.GetLanguageList();
				Logger.LogDebug("BindLanguageList(): myResponseReader → {MyResponseReader}", myResponseReader);

                // empty language list
                this.languageList.Items.Clear();

                // Loading into XML doc
                var xmlDoc = new XmlDocument { XmlResolver = null };
                using (var xmlReader = XmlReader.Create(myResponseReader, new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(xmlReader);
                }

                var languages = xmlDoc.SelectNodes("available/language");
				Logger.LogDebug("BindLanguageList(): languages → {Languages}", languages);
                var packages = new List<PackageInfo>();
				Logger.LogDebug("BindLanguageList(): packages → {Packages}", packages);

                if (languages != null)
                {
                    foreach (XmlNode language in languages)
                    {
                        string cultureCode = string.Empty;
						Logger.LogDebug("BindLanguageList(): cultureCode → {CultureCode}", cultureCode);
                        string version = string.Empty;
						Logger.LogDebug("BindLanguageList(): version → {Version}", version);
                        foreach (XmlNode child in language.ChildNodes)
                        {
                            if (child.Name == "culturecode")
                            {
                                cultureCode = child.InnerText;
                            }

                            if (child.Name == "version")
                            {
                                version = child.InnerText;
                            }
                        }

                        if (!string.IsNullOrEmpty(cultureCode) && !string.IsNullOrEmpty(version) && version.Length == 6)
                        {
                            var myCIintl = new CultureInfo(cultureCode, true);
							Logger.LogDebug("BindLanguageList(): myCIintl → {MyCIintl}", myCIintl);
                            version = version.Insert(4, ".").Insert(2, ".");
                            var package = new PackageInfo { Name = "LanguagePack-" + myCIintl.Name, FriendlyName = myCIintl.NativeName };
							Logger.LogDebug("BindLanguageList(): package → {Package}", package);
							Logger.LogDebug("BindLanguageList(): package.Name → {Arg0}", package.Name);
							Logger.LogDebug("BindLanguageList(): package.Description → {Arg0}", package.Description);
                            package.Name = myCIintl.NativeName;
                            package.Description = cultureCode;
                            Version ver;
                            Version.TryParse(version, out ver);
                            package.Version = ver;
							Logger.LogDebug("BindLanguageList(): package.Version → {Arg0}", package.Version);

                            if (packages.Any(p => p.Name == package.Name))
                            {
                                var existPackage = packages.First(p => p.Name == package.Name);
								Logger.LogDebug("BindLanguageList(): existPackage → {ExistPackage}", existPackage);
                                if (package.Version > existPackage.Version)
                                {
                                    packages.Remove(existPackage);
                                    packages.Add(package);
                                }
                            }
                            else
                            {
                                packages.Add(package);
                            }
                        }
                    }
                }

                foreach (var package in packages)
                {
                    var li = new ListItem { Value = package.Description, Text = package.Name };
					Logger.LogDebug("BindLanguageList(): li → {Li}", li);
                    this.languageList.AddItem(li.Text, li.Value);
                    var lastItem = this.languageList.Items[this.languageList.Items.Count - 1];
                    if (DotNetNukeContext.Current.Application.Version.Major != package.Version.Major
                        || DotNetNukeContext.Current.Application.Version.Minor != package.Version.Minor
                        || DotNetNukeContext.Current.Application.Version.Build != package.Version.Build)
                    {
                        lastItem.Attributes.Add("onclick", "javascript:LegacyLangaugePack('" + package.Version + "');");
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in BindLanguageList(): {Arg0}", Exception.Message);
                // suppress for now - need to decide what to do when webservice is unreachable
                // throw;
            }
            finally
            {
                // ensure there is always an en-us
                if (this.languageList.FindItemByValue("en-US") == null)
                {
                    var myCIintl = new CultureInfo("en-US", true);
                    var li = new ListItem { Value = @"en-US", Text = myCIintl.NativeName };
                    this.languageList.AddItem(li.Text, li.Value);
                    var lastItem = this.languageList.Items[this.languageList.Items.Count - 1];
                    lastItem.Attributes.Add("onclick", "javascript:ClearLegacyLangaugePack();");
                }

                var item = this.languageList.FindItemByValue(culture);
				Logger.LogDebug("BindLanguageList(): item → {Item}", item);
                this.languageList.SelectedIndex = item != null ? this.languageList.Items.IndexOf(item) : this.languageList.Items.IndexOf(this.languageList.FindItemByValue("en-US"));
            }
			Logger.LogDebug("<<< Exiting BindLanguageList()");
        }

        private void BindTemplates()
        {
			Logger.LogDebug(">>> Entering BindTemplates()");
            var templates = this.portalTemplateController.GetPortalTemplates();
            foreach (var template in templates)
            {
			Logger.LogDebug("BindTemplates(): templates → {Templates}", templates);
                this.templateList.AddItem(template.Name, Path.GetFileName(template.TemplateFilePath));
            }

            this.templateList.SelectedIndex = this.templateList.Items.IndexOf(this.templateList.FindItemByValue("Default Website.template"));
			Logger.LogDebug("<<< Exiting BindTemplates()");
        }
    }
}
