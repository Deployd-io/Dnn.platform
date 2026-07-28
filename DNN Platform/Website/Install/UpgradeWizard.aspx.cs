// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Install
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.Services.UserRequest;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;
    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;
    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The InstallWizard class provides the Installation Wizard for DotNetNuke.</summary>
    public partial class UpgradeWizard : PageBase
    {
        /// <summary>Client ID of the hidden input containing the Telerik anti-forgery token.</summary>
        protected static readonly string TelerikAntiForgeryTokenClientID = "telerikAntiForgeryToken";

        /// <summary>Client ID of the Telerik uninstall radio buttons.</summary>
        protected static readonly string TelerikUninstallOptionClientID = DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey;

        /// <summary>Form value when user selects Yes.</summary>
        protected static readonly string OptionYes = "Y";

        /// <summary>Form value when user selects No.</summary>
        protected static readonly string OptionNo = "N";

        /// <summary>The upgrade status filename.</summary>
        protected static readonly string StatusFilename = "upgradestat.log.resources.txt";

        private const string LocalesFile = "/Install/App_LocalResources/Locales.xml";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradeWizard));
        private static string localResourceFile = "~/Install/App_LocalResources/UpgradeWizard.aspx.resx";
        private static string culture;
        private static string[] supportedLanguages;
        private static IInstallationStep currentStep;
        private static bool upgradeRunning;
        private static int upgradeProgress;

        // steps shown in UI
        private static IInstallationStep upgradeDatabase = new InstallDatabaseStep();
        private static IInstallationStep upgradeExtensions = new InstallExtensionsStep();
        private static IInstallationStep iisVerification = new IISVerificationStep();

        // Ordered List of Steps (and weight in percentage) to be executed
        private static IDictionary<IInstallationStep, int> steps = new Dictionary<IInstallationStep, int>
            {
            // {new AddFcnModeStep(), 1},
                { iisVerification, 1 },
                { upgradeDatabase, 49 },
                { upgradeExtensions, 49 },
                { new InstallVersionStep(), 1 },
            };

        private readonly IApplicationStatusInfo applicationStatus;
        private readonly IHostSettings hostSettings;
        private readonly IApplicationInfo application;
        private readonly ICryptographyProvider cryptographyProvider;

        static UpgradeWizard()
        {
            IsAuthenticated = false;
			Logger.LogDebug(">>> Entering UpgradeWizard()");
			Logger.LogDebug("<<< Exiting UpgradeWizard()");
        }

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public UpgradeWizard()
            : this(null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering UpgradeWizard()");
        }
		Logger.LogDebug("<<< Exiting UpgradeWizard()");

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="application">The application info.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public UpgradeWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IApplicationInfo application)
            : this(portalController, appStatus, hostSettings, null, application, null)
        {
		Logger.LogDebug(">>> Entering UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},application={Application})", portalController, appStatus, hostSettings, application);
        }

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="application">The application info.</param>
        public UpgradeWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IUserController userController, IApplicationInfo application)
            : this(portalController, appStatus, hostSettings, userController, application, null)
        {
        }
		Logger.LogDebug("<<< Exiting UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},application={Application})", portalController, appStatus, hostSettings, userController, application);

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="application">The application info.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public UpgradeWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IUserController userController, IApplicationInfo application, ICryptographyProvider cryptographyProvider)
            : base(portalController, appStatus, hostSettings, userController)
        {
            this.applicationStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
			Logger.LogDebug("UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},application={Application},cryptographyProvider={CryptographyProvider}): cryptographyProvider → {CryptographyProvider1}", portalController, appStatus, hostSettings, userController, application, cryptographyProvider, cryptographyProvider);
        }

        /// <summary>Gets the application version.</summary>
        protected Version ApplicationVersion => this.application.Version;

        /// <summary>Gets the current version.</summary>
        protected Version CurrentVersion => this.application.CurrentVersion;

        private static string StatusFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", StatusFilename);

        private static bool IsAuthenticated { get; set; }

        /// <summary>Validates the user input.</summary>
        /// <param name="accountInfo">The account information.</param>
        /// <returns>A tuple with the result and a message.</returns>
        [WebMethod]
        public static Tuple<bool, string> ValidateInput(Dictionary<string, string> accountInfo)
        {
            var result = VerifyHostUser(accountInfo, out var errorMsg);

            return new Tuple<bool, string>(result, errorMsg);
        }

        /// <summary>Gets the installation log.</summary>
        /// <param name="startRow">At which line to start obtaining log lines.</param>
        /// <returns>Log string from the provided line number forward.</returns>
        [WebMethod]
        public static object GetInstallationLog(int startRow)
        {
			Logger.LogDebug(">>> Entering GetInstallationLog(startRow={StartRow})", startRow);
            const int maxLines = 500;
            if (!IsAuthenticated)
            {
                return string.Empty;
            }

            var logFile = $"InstallerLog{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.resources";
            try
			Logger.LogDebug("GetInstallationLog(startRow={StartRow}): logFile → {LogFile}", startRow, logFile);
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
                var sb = new StringBuilder();
				Logger.LogDebug("GetInstallationLog(startRow={StartRow}): lines → {Lines}", startRow, lines);
				Logger.LogDebug("GetInstallationLog(startRow={StartRow}): sb → {Sb}", startRow, sb);
                var errorLogged = false;
                var lineCount = 0;
				Logger.LogDebug("GetInstallationLog(startRow={StartRow}): errorLogged → {ErrorLogged}", startRow, errorLogged);

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
                    return Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/UpgradeWizard.aspx.resx");
                }

                return sb.ToString();
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetInstallationLog(startRow={StartRow}): {Arg1}", startRow, ex.Message);
				Logger.LogDebug("<<< Exiting GetInstallationLog(startRow={StartRow})", startRow);
                return string.Empty;
            }
        }

        /// <summary>Returns information to render the Security tab.</summary>
        /// <param name="accountInfo">Username and password to validate host user.</param>
        /// <returns>An instance of <see cref="SecurityTabResult"/>.</returns>
        [WebMethod]
        public static Tuple<bool, string, SecurityTabResult> GetSecurityTab(Dictionary<string, string> accountInfo)
        {
            if (!VerifyHostUser(accountInfo, out var errorMsg))
            {
                return Tuple.Create(false, errorMsg, default(SecurityTabResult));
            }

            var cryptographyProvider = CreateCryptographyProvider();
            var telerikUtils = CreateTelerikUtils();
            if (!telerikUtils.TelerikIsInstalled())
            {
                return Tuple.Create(
                    true,
                    default(string),
                    GetTelerikNotInstalledResult(cryptographyProvider));
            }

            var version = telerikUtils.GetTelerikVersion();
            if (!telerikUtils.IsTelerikVersionVulnerable(version))
            {
                return Tuple.Create(
                    true,
                    default(string),
                    GetTelerikInstalledWithVersionNotKnownToBeVulnerable(cryptographyProvider, version));
            }

            var assemblies = telerikUtils.GetAssembliesThatDependOnTelerik()
                .Select(a => Path.GetFileName(a));

            if (!assemblies.Any())
            {
                return Tuple.Create(
                    true,
                    default(string),
                    GetTelerikInstalledButNotUsedResult(cryptographyProvider, version));
            }

            return Tuple.Create(
                true,
                default(string),
                GetTelerikInstalledAndUsedResult(cryptographyProvider, assemblies, version));
        }

        /// <summary>Runs the upgrade.</summary>
        /// <param name="accountInfo">The account information.</param>
        [WebMethod]
        public static void RunUpgrade(Dictionary<string, string> accountInfo)
        {
            if (!VerifyHostUser(accountInfo, out _))
            {
                return;
            }

            var cryptograpyProvider = CreateCryptographyProvider();
            if (!TelerikAntiForgeryTokenIsValid(cryptograpyProvider, accountInfo))
            {
                throw new InvalidOperationException(LocalizeStringStatic("TelerikInvalidAntiForgeryToken"));
            }

            if (!accountInfo.TryGetValue(TelerikUninstallOptionClientID, out var option))
            {
                throw new InvalidOperationException(LocalizeStringStatic("TelerikUninstallOptionMissing"));
            }

            SetHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey, option);

            upgradeRunning = false;
            LaunchUpgrade();

            // DNN-9355: reset the installer files check flag after each upgrade, to make sure the installer files removed.
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            hostSettingsService.Update("InstallerFilesRemoved", "False", true);
			Logger.LogDebug("<<< Exiting RunUpgrade(Dictionary<string={Arg0},accountInfo={AccountInfo})", Dictionary<string, accountInfo);
        }

        /// <summary>Localizes a string.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The localized value for the key.</returns>
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

            if (Upgrade.Upgrade.UpdateNewtonsoftVersion())
            {
                this.Response.Redirect(this.Request.RawUrl, true);
            }

            this.SslRequiredCheck();
            GetInstallerLocales(this.applicationStatus);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                this.Response.Redirect("Install.aspx", true);
            }

            base.OnLoad(e);
            this.LocalizePage();

            if (this.Request.RawUrl.EndsWith("?complete"))
            {
                this.CompleteUpgrade();
            }

            // Create Status Files
            if (!this.Page.IsPostBack)
            {
                if (!File.Exists(StatusFile))
                {
                    File.CreateText(StatusFile).Close();
                }

                Upgrade.Upgrade.RemoveInvalidAntiForgeryCookie();
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private static ITelerikUtils CreateTelerikUtils()
        {
			Logger.LogDebug(">>> Entering CreateTelerikUtils()");
			Logger.LogDebug("<<< Exiting CreateTelerikUtils()");
            return Globals.GetCurrentServiceProvider().GetRequiredService<ITelerikUtils>();
        }

        private static ICryptographyProvider CreateCryptographyProvider()
        {
			Logger.LogDebug(">>> Entering CreateCryptographyProvider()");
			Logger.LogDebug("<<< Exiting CreateCryptographyProvider()");
            return Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        }

        private static Control CreateTelerikInstalledHeader(Version version)
        {
            return CreateBundle(
                CreateHeading("TelerikInstalledHeading"),
                CreateTelerikInstalledDetectedParagraph(version));
        }

        private static Control CreateTelerikInstalledDetectedParagraph(Version version)
        {
            return new HtmlGenericControl("p")
            {
                Controls =
                {
                    new Label { Text = LocalizeStringStatic("TelerikInstalledDetected") },
                    new Literal { Text = " " },
                    new Label { Text = version.ToString(), CssClass = "telerikVersion" },
                },
            };
        }

        private static Control CreateBundle(params Control[] controls)
        {
			Logger.LogDebug(">>> Entering CreateBundle(controls={Controls})", controls);
            var bundle = new PlaceHolder();

			Logger.LogDebug("CreateBundle(controls={Controls}): bundle → {Bundle}", controls, bundle);
            foreach (var control in controls)
            {
                bundle.Controls.Add(control);
            }

			Logger.LogDebug("<<< Exiting CreateBundle(controls={Controls})", controls);
            return bundle;
        }

        private static Control CreateHeading(string localizationKey) => CreateLabel("h4", localizationKey);

        private static Control CreateParagraph(string localizationKey) => CreateLabel("p", localizationKey);

        private static Control CreateLabel(string tag, string localizationKey)
        {
			Logger.LogDebug(">>> Entering CreateLabel(tag={Tag},localizationKey={LocalizationKey})", tag, localizationKey);
            var control = new HtmlGenericControl(tag);

			Logger.LogDebug("CreateLabel(tag={Tag},localizationKey={LocalizationKey}): control → {Control}", tag, localizationKey, control);
            control.Controls.Add(new Label
            {
                Text = LocalizeStringStatic(localizationKey),
            });

			Logger.LogDebug("<<< Exiting CreateLabel(tag={Tag},localizationKey={LocalizationKey})", tag, localizationKey);
            return control;
        }

        private static Control CreateHiddenField(string id, string value)
        {
			Logger.LogDebug(">>> Entering CreateHiddenField(id={Id},value={Value})", id, value);
			Logger.LogDebug("<<< Exiting CreateHiddenField(id={Id},value={Value})", id, value);
            return new HiddenField
            {
                ID = id,
                Value = value,
            };
        }

        private static Table CreateTable(IEnumerable<string> items, int maxRows, int maxColumns)
        {
			Logger.LogDebug(">>> Entering CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns})", items, maxRows, maxColumns);
            var capacity = maxRows * maxColumns;

            var row = new TableRow();
            var list = StartNewColumn(row);
			Logger.LogDebug("CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns}): row → {Row}", items, maxRows, maxColumns, row);
			Logger.LogDebug("CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns}): list → {List}", items, maxRows, maxColumns, list);

            foreach (var extension in items.Take(capacity - 1))
            {
                list.Items.Add(new ListItem(extension));

                if (list.Items.Count == maxRows)
                {
                    list = StartNewColumn(row);
                }
            }

            if (capacity < items.Count())
            {
                list.Items.Add(new ListItem("..."));
            }

            foreach (TableCell cell in row.Cells)
            {
                cell.Width = Unit.Percentage(100.0 / row.Cells.Count);
            }

            var table = new Table
            {
                Rows = { row },
                Width = Unit.Percentage(100.0),
            };

			Logger.LogDebug("<<< Exiting CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns})", items, maxRows, maxColumns);
            return table;
        }

        private static ListControl StartNewColumn(TableRow row)
        {
            var list = new BulletedList();

            var cell = new TableCell
            {
                Controls = { list },
                VerticalAlign = VerticalAlign.Top,
            };

            row.Cells.Add(cell);

            return list;
        }

        private static string RenderControls(params Control[] controls)
        {
			Logger.LogDebug(">>> Entering RenderControls(controls={Controls})", controls);
			Logger.LogDebug("<<< Exiting RenderControls(controls={Controls})", controls);
            return RenderControl(CreateBundle(controls));
        }

        private static string RenderControl(Control control)
        {
            using (var textWriter = new StringWriter())
            using (var htmlWriter = new HtmlTextWriter(textWriter))
            {
                control.RenderControl(htmlWriter);
                return textWriter.ToString();
            }
        }

        private static string GetHostSetting(string key)
        {
			Logger.LogDebug(">>> Entering GetHostSetting(key={Key})", key);
			Logger.LogDebug("<<< Exiting GetHostSetting(key={Key})", key);
            return Globals.GetCurrentServiceProvider()
                .GetRequiredService<IHostSettingsService>()
                .GetSettingsDictionary()[key];
        }

        private static void SetHostSetting(string key, string value, bool isSecure = false)
        {
			Logger.LogDebug(">>> Entering SetHostSetting(key={Key},value={Value},isSecure={IsSecure})", key, value, isSecure);
            var setting = new ConfigurationSetting
            {
                IsSecure = isSecure,
                Key = key,
                Value = value,
            };

            Globals.GetCurrentServiceProvider()
                .GetRequiredService<IHostSettingsService>()
                .Update(setting);
				Logger.LogDebug("<<< Exiting SetHostSetting(key={Key},value={Value},isSecure={IsSecure})", key, value, isSecure);
        }

        private static void GetInstallerLocales(IApplicationStatusInfo applicationStatus)
        {
			Logger.LogDebug(">>> Entering GetInstallerLocales(applicationStatus={ApplicationStatus})", applicationStatus);
            var filePath = applicationStatus.ApplicationMapPath + LocalesFile.Replace("/", @"\");

			Logger.LogDebug("GetInstallerLocales(applicationStatus={ApplicationStatus}): filePath → {FilePath}", applicationStatus, filePath);
            if (File.Exists(filePath))
            {
                var doc = new XPathDocument(filePath);
                var languages = doc.CreateNavigator().Select("root/language");
				Logger.LogDebug("GetInstallerLocales(applicationStatus={ApplicationStatus}): doc → {Doc}", applicationStatus, doc);
				Logger.LogDebug("GetInstallerLocales(applicationStatus={ApplicationStatus}): languages → {Languages}", applicationStatus, languages);

                if (languages.Count > 0)
                {
                    supportedLanguages = new string[languages.Count];
					Logger.LogDebug("GetInstallerLocales(applicationStatus={ApplicationStatus}): supportedLanguages → {SupportedLanguages}", applicationStatus, supportedLanguages);
                    var i = 0;
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
            }
			Logger.LogDebug("<<< Exiting GetInstallerLocales(applicationStatus={ApplicationStatus})", applicationStatus);
        }

        private static string LocalizeStringStatic(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeStringStatic(key={Key})", key);
			Logger.LogDebug("<<< Exiting LocalizeStringStatic(key={Key})", key);
            return Localization.GetString(key, localResourceFile, culture);
        }

        private static void LaunchUpgrade()
        {
			Logger.LogDebug(">>> Entering LaunchUpgrade()");
            // Get current Script time-out
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            // Set Script timeout to MAX value
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

            if (culture != null)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }

            // bail out early if upgrade is in progress
            if (upgradeRunning)
            {
                return;
            }

            var percentForEachStep = 100 / steps.Count;
            var useGenericPercent = false;
            var totalPercent = steps.Sum(step => step.Value);
			Logger.LogDebug("LaunchUpgrade(): useGenericPercent → {UseGenericPercent}", useGenericPercent);
			Logger.LogDebug("LaunchUpgrade(): totalPercent → {TotalPercent}", totalPercent);
            if (totalPercent != 100)
            {
                useGenericPercent = true;
            }

            upgradeRunning = true;
			Logger.LogDebug("LaunchUpgrade(): upgradeRunning → {UpgradeRunning}", upgradeRunning);
            upgradeProgress = 0;

			Logger.LogDebug("LaunchUpgrade(): upgradeProgress → {UpgradeProgress}", upgradeProgress);
            // Output the current time for the user
            CurrentStepActivity(string.Concat(
                Localization.GetString("UpgradeStarted", localResourceFile),
                ":",
                DateTime.Now.ToString()));

            foreach (var step in steps)
            {
                currentStep = step.Key;
				Logger.LogDebug("LaunchUpgrade(): currentStep → {CurrentStep}", currentStep);

                try
                {
                    currentStep.Activity += CurrentStepActivity;
                    currentStep.Execute();
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in LaunchUpgrade(): {Arg0}", ex.Message);
                    CurrentStepActivity(Localization.GetString("ErrorInStep", localResourceFile) + ": " + ex.Message);
                    upgradeRunning = false;
                    return;
                }

                switch (currentStep.Status)
                {
                    case StepStatus.AppRestart:
                        upgradeRunning = false;
                        HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                        break;
                    default:
                        if (currentStep.Status != StepStatus.Done)
                        {
                            CurrentStepActivity(string.Format(
                                Localization.GetString("ErrorInStep", localResourceFile),
                                currentStep.Errors.Count > 0 ? string.Join(",", currentStep.Errors.ToArray()) : currentStep.Details));
                            upgradeRunning = false;
                            return;
                        }

                        break;
                }

                if (useGenericPercent)
                {
                    upgradeProgress += percentForEachStep;
                }
                else
                {
                    upgradeProgress += step.Value;
                }
            }

            currentStep = null;
            upgradeProgress = 100;

            Globals.GetCurrentServiceProvider().GetService<IDamUninstaller>().Execute();

            CurrentStepActivity(Localization.GetString("UpgradeDone", localResourceFile));

            // indicate we are done
            upgradeRunning = false;

            // restore Script timeout
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
			Logger.LogDebug("<<< Exiting LaunchUpgrade()");
        }

        private static void CurrentStepActivity(string status)
        {
			Logger.LogDebug(">>> Entering CurrentStepActivity(status={Status})", status);
            var percentage = (currentStep == null) ? upgradeProgress : upgradeProgress + (currentStep.Percentage / steps.Count);
            var obj = new
			Logger.LogDebug("CurrentStepActivity(status={Status}): percentage → {Percentage}", status, percentage);
            {
                progress = percentage,
                details = status,
                check0 = upgradeDatabase.Status.ToString() + (upgradeDatabase.Errors.Count == 0 ? string.Empty : " Errors " + upgradeDatabase.Errors.Count),
                check1 = upgradeExtensions.Status.ToString() + (upgradeExtensions.Errors.Count == 0 ? string.Empty : " Errors " + upgradeExtensions.Errors.Count),
            };
				Logger.LogDebug("CurrentStepActivity(status={Status}): progress → {Progress}", status, progress);
				Logger.LogDebug("CurrentStepActivity(status={Status}): details → {Details}", status, details);

            try
            {
                if (!File.Exists(StatusFile))
                {
                    File.CreateText(StatusFile);
                }

                using (var sw = new StreamWriter(StatusFile, true))
                {
                    sw.WriteLine(obj.ToJson());
                    sw.Close();
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CurrentStepActivity(status={Status}): {Arg1}", status, ex.Message);
                // TODO - do something
            }
			Logger.LogDebug("<<< Exiting CurrentStepActivity(status={Status})", status);
        }

        private static bool VerifyHostUser(Dictionary<string, string> accountInfo, out string errorMsg)
        {
            var result = true;
            errorMsg = string.Empty;

            UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
            var userRequestIpAddressController = UserRequestIPAddressController.Instance;
            var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
            UserInfo hostUser = DotNetNuke.Entities.Users.UserController.ValidateUser(-1, accountInfo["username"], accountInfo["password"], "DNN", string.Empty, string.Empty, ipAddress, ref loginStatus);

            if (loginStatus == UserLoginStatus.LOGIN_FAILURE || !hostUser.IsSuperUser)
            {
                result = false;
                errorMsg = LocalizeStringStatic("InvalidCredentials");
            }
            else
            {
                IsAuthenticated = true;
            }

            return result;
        }

        private static Control CreateTelerikAntiForgeryTokenField(ICryptographyProvider cryptographyProvider) =>
            CreateHiddenField(TelerikAntiForgeryTokenClientID, CreateTelerikAntiForgeryToken(cryptographyProvider));

        private static string CreateTelerikAntiForgeryToken(ICryptographyProvider cryptographyProvider)
        {
            var secret = GetHostSetting("GUID");
            var salt = new Random().Next(int.MaxValue);
            var token = new { secret, salt };
            var json = JsonConvert.SerializeObject(token);

            return Encrypt(cryptographyProvider, json);
        }

        private static bool TelerikAntiForgeryTokenIsValid(ICryptographyProvider cryptographyProvider, Dictionary<string, string> accountInfo)
        {
            if (!accountInfo.TryGetValue(TelerikAntiForgeryTokenClientID, out var encryptedToken))
            {
                return false; // token missing somehow.
            }

            try
            {
                var json = Decrypt(cryptographyProvider, encryptedToken);
                dynamic token = JsonConvert.DeserializeObject(json);
                var expected = GetHostSetting("GUID");
                if (token.secret != expected)
                {
                    return false; // token mismatch.
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false; // malformed token.
            }

            return true;
        }

        private static string Encrypt(ICryptographyProvider cryptographyProvider, string token)
        {
            var (encryptedMessage, _, initializationVector) = cryptographyProvider.EncryptString(token, GetHostSetting("GUID"));
            return $"{encryptedMessage}|{initializationVector}";
        }

        private static string Decrypt(ICryptographyProvider cryptographyProvider, string token)
        {
            var tokenParts = token.Split(['|',], 2);
            return cryptographyProvider.DecryptString(tokenParts[0], GetHostSetting("GUID"), cryptographyProvider.EncryptStringAlgorithmName, tokenParts[1]);
        }

        private static SecurityTabResult GetTelerikNotInstalledResult(ICryptographyProvider cryptographyProvider)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(cryptographyProvider),
                    CreateHiddenField(TelerikUninstallOptionClientID, OptionNo),
                    CreateHeading("TelerikNotInstalledHeading"),
                    CreateParagraph("TelerikNotInstalledInfo")),
            };
        }

        private static SecurityTabResult GetTelerikInstalledWithVersionNotKnownToBeVulnerable(ICryptographyProvider cryptographyProvider, Version version)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(cryptographyProvider),
                    CreateHiddenField(TelerikUninstallOptionClientID, DotNetNuke.Maintenance.Constants.TelerikUninstallNoValue),
                    CreateTelerikInstalledHeader(version),
                    CreateParagraph("TelerikVersionNotKnownToBeVulnerableInfo")),
            };
        }

        private static SecurityTabResult GetTelerikInstalledButNotUsedResult(ICryptographyProvider cryptographyProvider, Version version)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(cryptographyProvider),
                    CreateHiddenField(TelerikUninstallOptionClientID, DotNetNuke.Maintenance.Constants.TelerikUninstallYesValue),
                    CreateTelerikInstalledHeader(version),
                    CreateParagraph("TelerikInstalledBulletin"),
                    CreateParagraph("TelerikInstalledButNotUsedInfo"),
                    CreateParagraph("TelerikUninstallInfo")),
            };
        }

        private static SecurityTabResult GetTelerikInstalledAndUsedResult(ICryptographyProvider cryptographyProvider, IEnumerable<string> assemblies, Version version)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(cryptographyProvider),
                    CreateHiddenField(TelerikUninstallOptionClientID, DotNetNuke.Maintenance.Constants.TelerikUninstallYesValue),
                    CreateTelerikInstalledHeader(version),
                    CreateParagraph("TelerikInstalledBulletin"),
                    CreateParagraph("TelerikInstalledAndUsedInfo"),
                    CreateTable(assemblies, maxRows: 3, maxColumns: 4),
                    new Literal { Text = "<br />", },
                    CreateParagraph("TelerikInstalledAndUsedWarning")),
            };
        }

        private void LocalizePage()
        {
			Logger.LogDebug(">>> Entering LocalizePage()");
            this.SetBrowserLanguage();
            this.Page.Title = this.LocalizeString("Title");
            if (Globals.FormatVersion(this.ApplicationVersion) == Globals.FormatVersion(this.CurrentVersion))
            {
                this.versionLabel.Visible = false;
                this.currentVersionLabel.Visible = false;
                this.versionsMatch.Text = this.LocalizeString("VersionsMatch");
                if (this.applicationStatus.IncrementalVersionExists(this.CurrentVersion))
                {
                    this.versionsMatch.Text = this.LocalizeString("VersionsMatchButIncrementalExists");
                }
            }
            else
            {
                this.versionLabel.Text = string.Format(this.LocalizeString("Version"), Globals.FormatVersion(this.ApplicationVersion));
                this.currentVersionLabel.Text = string.Format(this.LocalizeString("CurrentVersion"), Globals.FormatVersion(this.CurrentVersion));
            }
			Logger.LogDebug("<<< Exiting LocalizePage()");
        }

        private void SetBrowserLanguage()
        {
			Logger.LogDebug(">>> Entering SetBrowserLanguage()");
            string cultureCode;
            if (string.IsNullOrEmpty(this.PageLocale.Value) && string.IsNullOrEmpty(culture))
            {
                cultureCode = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(supportedLanguages);
				Logger.LogDebug("SetBrowserLanguage(): cultureCode → {CultureCode}", cultureCode);
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

        private void CompleteUpgrade()
        {
			Logger.LogDebug(">>> Entering CompleteUpgrade()");
            // Delete the status file.
            try
            {
                File.Delete(StatusFile);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CompleteUpgrade(): {Arg0}", ex.Message);
                // Do nothing
            }

            // remove installwizard files added back by upgrade package
            Upgrade.Upgrade.DeleteInstallerFiles();

            Config.Touch(this.applicationStatus);
            this.Response.Redirect("../Default.aspx", true);
			Logger.LogDebug("<<< Exiting CompleteUpgrade()");
        }

        private void SslRequiredCheck()
        {
			Logger.LogDebug(">>> Entering SslRequiredCheck()");
            if (this.hostSettings.UpgradeForceSsl && !this.Request.IsSecureConnection)
            {
                var sslDomain = this.hostSettings.SslDomain;
                if (string.IsNullOrEmpty(sslDomain))
				Logger.LogDebug("SslRequiredCheck(): sslDomain → {SslDomain}", sslDomain);
                {
                    sslDomain = this.Request.Url.Host;
                }
                else if (sslDomain.Contains("://"))
                {
                    sslDomain = sslDomain.Substring(sslDomain.IndexOf("://") + 3);
                }

                var sslUrl = $"https://{sslDomain}{this.Request.RawUrl}";

				Logger.LogDebug("SslRequiredCheck(): sslUrl → {SslUrl}", sslUrl);
                this.Response.Redirect(sslUrl, true);
            }
        }
    }
}
