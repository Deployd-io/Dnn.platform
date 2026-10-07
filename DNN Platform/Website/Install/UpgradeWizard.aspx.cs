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
			Logger.LogError(">>> Entering UpgradeWizard()");
        Logger.LogWarning("<<< Exiting UpgradeWizard()");
        }

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public UpgradeWizard()
            : this(null, null, null, null, null)
        {
		Logger.LogError(">>> Entering UpgradeWizard()");
        Logger.LogWarning("<<< Exiting UpgradeWizard()");
        }

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="application">The application info.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public UpgradeWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IApplicationInfo application)
            : this(portalController, appStatus, hostSettings, null, application, null)
        {
		Logger.LogError(">>> Entering UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},application={Application})", portalController, appStatus, hostSettings, application);
        Logger.LogWarning("<<< Exiting UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},application={Application})", portalController, appStatus, hostSettings, application);
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
			Logger.LogDebug("UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},application={Application},cryptographyProvider={CryptographyProvider}): hostSettings → {HostSettings1}", portalController, appStatus, hostSettings, userController, application, cryptographyProvider, hostSettings);
			Logger.LogDebug("UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},application={Application},cryptographyProvider={CryptographyProvider}): application → {Application1}", portalController, appStatus, hostSettings, userController, application, cryptographyProvider, application);
			Logger.LogDebug("UpgradeWizard(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},application={Application},cryptographyProvider={CryptographyProvider}): cryptographyProvider → {CryptographyProvider1}", portalController, appStatus, hostSettings, userController, application, cryptographyProvider, cryptographyProvider);
            this.applicationStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
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
			Logger.LogError(">>> Entering GetInstallationLog(startRow={StartRow})", startRow);
            const int maxLines = 500;
            if (!IsAuthenticated)
            {
                return string.Empty;
            }

            var logFile = $"InstallerLog{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}.resources";
			Logger.LogError("GetInstallationLog(startRow={StartRow}): logFile → {LogFile}", startRow, logFile);
            try
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
				Logger.LogError("GetInstallationLog(startRow={StartRow}): lines → {Lines}", startRow, lines);
                var sb = new StringBuilder();
				Logger.LogError("GetInstallationLog(startRow={StartRow}): sb → {Sb}", startRow, sb);
                var errorLogged = false;
				Logger.LogError("GetInstallationLog(startRow={StartRow}): errorLogged → {ErrorLogged}", startRow, errorLogged);
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
                    return Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/UpgradeWizard.aspx.resx");
                }

                return sb.ToString();
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetInstallationLog(startRow={StartRow}): {Arg1}", startRow, Exception.Message);
				Logger.LogWarning("<<< Exiting GetInstallationLog(startRow={StartRow})", startRow);
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
			Logger.LogError(">>> Entering RunUpgrade(accountInfo={AccountInfo})", accountInfo);
            if (!VerifyHostUser(accountInfo, out _))
            {
                return;
            }

            var cryptograpyProvider = CreateCryptographyProvider();
			Logger.LogError("RunUpgrade(accountInfo={AccountInfo}): cryptograpyProvider → {CryptograpyProvider}", accountInfo, cryptograpyProvider);
            if (!TelerikAntiForgeryTokenIsValid(cryptograpyProvider, accountInfo))
            {
				Logger.LogError("Throwing InvalidOperationException from RunUpgrade(accountInfo={AccountInfo})", accountInfo);
                throw new InvalidOperationException(LocalizeStringStatic("TelerikInvalidAntiForgeryToken"));
            }

            if (!accountInfo.TryGetValue(TelerikUninstallOptionClientID, out var option))
            {
                throw new InvalidOperationException(LocalizeStringStatic("TelerikUninstallOptionMissing"));
            }

            SetHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey, option);

            upgradeRunning = false;
			Logger.LogDebug("RunUpgrade(accountInfo={AccountInfo}): upgradeRunning → {UpgradeRunning}", accountInfo, upgradeRunning);
            LaunchUpgrade();

            // DNN-9355: reset the installer files check flag after each upgrade, to make sure the installer files removed.
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
			Logger.LogError("RunUpgrade(accountInfo={AccountInfo}): hostSettingsService → {HostSettingsService}", accountInfo, hostSettingsService);
            hostSettingsService.Update("InstallerFilesRemoved", "False", true);
			Logger.LogWarning("<<< Exiting RunUpgrade(accountInfo={AccountInfo})", accountInfo);
        }

        /// <summary>Localizes a string.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The localized value for the key.</returns>
        protected string LocalizeString(string key)
        {
			Logger.LogError(">>> Entering LocalizeString(key={Key})", key);
            Logger.LogWarning("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, localResourceFile, culture);
        }

        /// <inheritdoc />
        protected override void OnError(EventArgs e)
        {
			Logger.LogError(">>> Entering OnError(e={E})", e);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
			Logger.LogWarning("<<< Exiting OnError(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (Upgrade.Upgrade.UpdateNewtonsoftVersion())
            {
                this.Response.Redirect(this.Request.RawUrl, true);
            }

            this.SslRequiredCheck();
            GetInstallerLocales(this.applicationStatus);
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
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
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        private static ITelerikUtils CreateTelerikUtils()
        {
			Logger.LogError(">>> Entering CreateTelerikUtils()");
            Logger.LogWarning("<<< Exiting CreateTelerikUtils()");
            return Globals.GetCurrentServiceProvider().GetRequiredService<ITelerikUtils>();
        }

        private static ICryptographyProvider CreateCryptographyProvider()
        {
			Logger.LogError(">>> Entering CreateCryptographyProvider()");
            Logger.LogWarning("<<< Exiting CreateCryptographyProvider()");
            return Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        }

        private static Control CreateTelerikInstalledHeader(Version version)
        {
			Logger.LogError(">>> Entering CreateTelerikInstalledHeader(version={Version})", version);
            Logger.LogWarning("<<< Exiting CreateTelerikInstalledHeader(version={Version})", version);
            return CreateBundle(
                CreateHeading("TelerikInstalledHeading"),
                CreateTelerikInstalledDetectedParagraph(version));
        }

        private static Control CreateTelerikInstalledDetectedParagraph(Version version)
        {
			Logger.LogError(">>> Entering CreateTelerikInstalledDetectedParagraph(version={Version})", version);
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
			Logger.LogError(">>> Entering CreateBundle(controls={Controls})", controls);
            var bundle = new PlaceHolder();

			Logger.LogError("CreateBundle(controls={Controls}): bundle → {Bundle}", controls, bundle);
            foreach (var control in controls)
            {
                bundle.Controls.Add(control);
            }

			Logger.LogWarning("<<< Exiting CreateBundle(controls={Controls})", controls);
            return bundle;
        }

        private static Control CreateHeading(string localizationKey) => CreateLabel("h4", localizationKey);

        private static Control CreateParagraph(string localizationKey) => CreateLabel("p", localizationKey);

        private static Control CreateLabel(string tag, string localizationKey)
        {
			Logger.LogError(">>> Entering CreateLabel(tag={Tag},localizationKey={LocalizationKey})", tag, localizationKey);
            var control = new HtmlGenericControl(tag);

			Logger.LogError("CreateLabel(tag={Tag},localizationKey={LocalizationKey}): control → {Control}", tag, localizationKey, control);
            control.Controls.Add(new Label
            {
                Text = LocalizeStringStatic(localizationKey),
            });

			Logger.LogWarning("<<< Exiting CreateLabel(tag={Tag},localizationKey={LocalizationKey})", tag, localizationKey);
            return control;
        }

        private static Control CreateHiddenField(string id, string value)
        {
			Logger.LogError(">>> Entering CreateHiddenField(id={Id},value={Value})", id, value);
			Logger.LogWarning("<<< Exiting CreateHiddenField(id={Id},value={Value})", id, value);
            return new HiddenField
            {
                ID = id,
                Value = value,
            };
        }

        private static Table CreateTable(IEnumerable<string> items, int maxRows, int maxColumns)
        {
			Logger.LogError(">>> Entering CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns})", items, maxRows, maxColumns);
            var capacity = maxRows * maxColumns;

            var row = new TableRow();
			Logger.LogError("CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns}): row → {Row}", items, maxRows, maxColumns, row);
            var list = StartNewColumn(row);
			Logger.LogError("CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns}): list → {List}", items, maxRows, maxColumns, list);

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

			Logger.LogWarning("<<< Exiting CreateTable(items={Items},maxRows={MaxRows},maxColumns={MaxColumns})", items, maxRows, maxColumns);
            return table;
        }

        private static ListControl StartNewColumn(TableRow row)
        {
			Logger.LogError(">>> Entering StartNewColumn(row={Row})", row);
            var list = new BulletedList();

			Logger.LogError("StartNewColumn(row={Row}): list → {List}", row, list);
            var cell = new TableCell
            {
                Controls = { list },
                VerticalAlign = VerticalAlign.Top,
            };

            row.Cells.Add(cell);

			Logger.LogWarning("<<< Exiting StartNewColumn(row={Row})", row);
            return list;
        }

        private static string RenderControls(params Control[] controls)
        {
			Logger.LogError(">>> Entering RenderControls(controls={Controls})", controls);
            Logger.LogWarning("<<< Exiting RenderControls(controls={Controls})", controls);
            return RenderControl(CreateBundle(controls));
        }

        private static string RenderControl(Control control)
        {
			Logger.LogError(">>> Entering RenderControl(control={Control})", control);
            using (var textWriter = new StringWriter())
            using (var htmlWriter = new HtmlTextWriter(textWriter))
            {
                control.RenderControl(htmlWriter);
				Logger.LogWarning("<<< Exiting RenderControl(control={Control})", control);
                return textWriter.ToString();
            }
        }

        private static string GetHostSetting(string key)
        {
			Logger.LogError(">>> Entering GetHostSetting(key={Key})", key);
            Logger.LogWarning("<<< Exiting GetHostSetting(key={Key})", key);
            return Globals.GetCurrentServiceProvider()
                .GetRequiredService<IHostSettingsService>()
                .GetSettingsDictionary()[key];
        }

        private static void SetHostSetting(string key, string value, bool isSecure = false)
        {
			Logger.LogError(">>> Entering SetHostSetting(key={Key},value={Value},isSecure={IsSecure})", key, value, isSecure);
            var setting = new ConfigurationSetting
            {
                IsSecure = isSecure,
                Key = key,
                Value = value,
            };

            Globals.GetCurrentServiceProvider()
                .GetRequiredService<IHostSettingsService>()
                .Update(setting);
				Logger.LogWarning("<<< Exiting SetHostSetting(key={Key},value={Value},isSecure={IsSecure})", key, value, isSecure);
        }

        private static void GetInstallerLocales(IApplicationStatusInfo applicationStatus)
        {
			Logger.LogError(">>> Entering GetInstallerLocales(applicationStatus={ApplicationStatus})", applicationStatus);
            var filePath = applicationStatus.ApplicationMapPath + LocalesFile.Replace("/", @"\");

            if (File.Exists(filePath))
            {
				Logger.LogError("GetInstallerLocales(applicationStatus={ApplicationStatus}): filePath → {FilePath}", applicationStatus, filePath);
                var doc = new XPathDocument(filePath);
				Logger.LogError("GetInstallerLocales(applicationStatus={ApplicationStatus}): doc → {Doc}", applicationStatus, doc);
                var languages = doc.CreateNavigator().Select("root/language");
				Logger.LogError("GetInstallerLocales(applicationStatus={ApplicationStatus}): languages → {Languages}", applicationStatus, languages);

                if (languages.Count > 0)
                {
                    supportedLanguages = new string[languages.Count];
					Logger.LogError("GetInstallerLocales(applicationStatus={ApplicationStatus}): supportedLanguages → {SupportedLanguages}", applicationStatus, supportedLanguages);
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
				Logger.LogWarning("<<< Exiting GetInstallerLocales(applicationStatus={ApplicationStatus})", applicationStatus);
                supportedLanguages = new string[1];
                supportedLanguages.SetValue("en-US", 0);
            }
        }

        private static string LocalizeStringStatic(string key)
        {
			Logger.LogError(">>> Entering LocalizeStringStatic(key={Key})", key);
            Logger.LogWarning("<<< Exiting LocalizeStringStatic(key={Key})", key);
            return Localization.GetString(key, localResourceFile, culture);
        }

        private static void LaunchUpgrade()
        {
			Logger.LogError(">>> Entering LaunchUpgrade()");
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
			Logger.LogError("LaunchUpgrade(): useGenericPercent → {UseGenericPercent}", useGenericPercent);
            var totalPercent = steps.Sum(step => step.Value);
			Logger.LogError("LaunchUpgrade(): totalPercent → {TotalPercent}", totalPercent);
            if (totalPercent != 100)
            {
                useGenericPercent = true;
            }

            upgradeRunning = true;
			Logger.LogError("LaunchUpgrade(): upgradeRunning → {UpgradeRunning}", upgradeRunning);
			Logger.LogDebug("LaunchUpgrade(): upgradeRunning → {UpgradeRunning}", upgradeRunning);
            upgradeProgress = 0;
			Logger.LogError("LaunchUpgrade(): upgradeProgress → {UpgradeProgress}", upgradeProgress);

            // Output the current time for the user
			Logger.LogDebug("LaunchUpgrade(): upgradeProgress → {UpgradeProgress}", upgradeProgress);
            CurrentStepActivity(string.Concat(
                Localization.GetString("UpgradeStarted", localResourceFile),
                ":",
                DateTime.Now.ToString()));

            foreach (var step in steps)
            {
                currentStep = step.Key;
				Logger.LogError("LaunchUpgrade(): currentStep → {CurrentStep}", currentStep);
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
			Logger.LogWarning("<<< Exiting LaunchUpgrade()");
        }

        private static void CurrentStepActivity(string status)
        {
			Logger.LogError(">>> Entering CurrentStepActivity(status={Status})", status);
            var percentage = (currentStep == null) ? upgradeProgress : upgradeProgress + (currentStep.Percentage / steps.Count);
			Logger.LogError("CurrentStepActivity(status={Status}): percentage → {Percentage}", status, percentage);
            var obj = new
            {
                progress = percentage,
                details = status,
                check0 = upgradeDatabase.Status.ToString() + (upgradeDatabase.Errors.Count == 0 ? string.Empty : " Errors " + upgradeDatabase.Errors.Count),
                check1 = upgradeExtensions.Status.ToString() + (upgradeExtensions.Errors.Count == 0 ? string.Empty : " Errors " + upgradeExtensions.Errors.Count),
            };

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
				Logger.LogError("Exception in CurrentStepActivity(status={Status}): {Arg1}", status, Exception.Message);
                // TODO - do something
            }
			Logger.LogWarning("<<< Exiting CurrentStepActivity(status={Status})", status);
        }

        private static bool VerifyHostUser(Dictionary<string, string> accountInfo, out string errorMsg)
        {
			Logger.LogError(">>> Entering VerifyHostUser(accountInfo={AccountInfo})", accountInfo);
            var result = true;
            errorMsg = string.Empty;
			Logger.LogError("VerifyHostUser(accountInfo={AccountInfo}): errorMsg → {ErrorMsg}", accountInfo, errorMsg);
			Logger.LogDebug("VerifyHostUser(accountInfo={AccountInfo}): errorMsg → {ErrorMsg}", accountInfo, errorMsg);
			Logger.LogError("VerifyHostUser(accountInfo={AccountInfo}): result → {Result}", accountInfo, result);

            UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
            var userRequestIpAddressController = UserRequestIPAddressController.Instance;
            var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
			Logger.LogError("VerifyHostUser(accountInfo={AccountInfo}): ipAddress → {IpAddress}", accountInfo, ipAddress);
            UserInfo hostUser = DotNetNuke.Entities.Users.UserController.ValidateUser(-1, accountInfo["username"], accountInfo["password"], "DNN", string.Empty, string.Empty, ipAddress, ref loginStatus);
			Logger.LogError("VerifyHostUser(accountInfo={AccountInfo}): hostUser → {HostUser}", accountInfo, hostUser);

            if (loginStatus == UserLoginStatus.LOGIN_FAILURE || !hostUser.IsSuperUser)
            {
                result = false;
                errorMsg = LocalizeStringStatic("InvalidCredentials");
            }
            else
            {
                IsAuthenticated = true;
				Logger.LogDebug("VerifyHostUser(accountInfo={AccountInfo}): IsAuthenticated → {IsAuthenticated}", accountInfo, IsAuthenticated);
            }

			Logger.LogWarning("<<< Exiting VerifyHostUser(accountInfo={AccountInfo})", accountInfo);
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

			Logger.LogError("CreateTelerikAntiForgeryToken(cryptographyProvider={CryptographyProvider}): secret → {Secret}", cryptographyProvider, secret);
			Logger.LogError(">>> Entering CreateTelerikAntiForgeryToken(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
			Logger.LogWarning("<<< Exiting CreateTelerikAntiForgeryToken(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
            return Encrypt(cryptographyProvider, json);
        }

        private static bool TelerikAntiForgeryTokenIsValid(ICryptographyProvider cryptographyProvider, Dictionary<string, string> accountInfo)
        {
			Logger.LogError(">>> Entering TelerikAntiForgeryTokenIsValid(cryptographyProvider={CryptographyProvider},accountInfo={AccountInfo})", cryptographyProvider, accountInfo);
            if (!accountInfo.TryGetValue(TelerikAntiForgeryTokenClientID, out var encryptedToken))
            {
                return false; // token missing somehow.
            }

            try
            {
                var json = Decrypt(cryptographyProvider, encryptedToken);
				Logger.LogError("TelerikAntiForgeryTokenIsValid(cryptographyProvider={CryptographyProvider},accountInfo={AccountInfo}): json → {Json}", cryptographyProvider, accountInfo, json);
                dynamic token = JsonConvert.DeserializeObject(json);
				Logger.LogError("TelerikAntiForgeryTokenIsValid(cryptographyProvider={CryptographyProvider},accountInfo={AccountInfo}): token → {Token}", cryptographyProvider, accountInfo, token);
                var expected = GetHostSetting("GUID");
				Logger.LogError("TelerikAntiForgeryTokenIsValid(cryptographyProvider={CryptographyProvider},accountInfo={AccountInfo}): expected → {Expected}", cryptographyProvider, accountInfo, expected);
                if (token.secret != expected)
                {
                    return false; // token mismatch.
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
			Logger.LogWarning("<<< Exiting TelerikAntiForgeryTokenIsValid(cryptographyProvider={CryptographyProvider},accountInfo={AccountInfo})", cryptographyProvider, accountInfo);
                return false; // malformed token.
            }

            return true;
        }

        private static string Encrypt(ICryptographyProvider cryptographyProvider, string token)
        {
			Logger.LogError(">>> Entering Encrypt(cryptographyProvider={CryptographyProvider},token=<redacted>)", cryptographyProvider);
            var (encryptedMessage, _, initializationVector) = cryptographyProvider.EncryptString(token, GetHostSetting("GUID"));
			Logger.LogWarning("<<< Exiting Encrypt(cryptographyProvider={CryptographyProvider},token=<redacted>)", cryptographyProvider);
            return $"{encryptedMessage}|{initializationVector}";
        }

        private static string Decrypt(ICryptographyProvider cryptographyProvider, string token)
        {
			Logger.LogError(">>> Entering Decrypt(cryptographyProvider={CryptographyProvider},token=<redacted>)", cryptographyProvider);
            var tokenParts = token.Split(['|',], 2);
			Logger.LogError("Decrypt(cryptographyProvider={CryptographyProvider},token=<redacted>): tokenParts → {TokenParts}", cryptographyProvider, tokenParts);
			Logger.LogWarning("<<< Exiting Decrypt(cryptographyProvider={CryptographyProvider},token=<redacted>)", cryptographyProvider);
            return cryptographyProvider.DecryptString(tokenParts[0], GetHostSetting("GUID"), cryptographyProvider.EncryptStringAlgorithmName, tokenParts[1]);
        }

        private static SecurityTabResult GetTelerikNotInstalledResult(ICryptographyProvider cryptographyProvider)
        {
			Logger.LogError(">>> Entering GetTelerikNotInstalledResult(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
			Logger.LogWarning("<<< Exiting GetTelerikNotInstalledResult(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
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
			Logger.LogError(">>> Entering GetTelerikInstalledWithVersionNotKnownToBeVulnerable(cryptographyProvider={CryptographyProvider},version={Version})", cryptographyProvider, version);
			Logger.LogWarning("<<< Exiting GetTelerikInstalledWithVersionNotKnownToBeVulnerable(cryptographyProvider={CryptographyProvider},version={Version})", cryptographyProvider, version);
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
			Logger.LogError(">>> Entering GetTelerikInstalledButNotUsedResult(cryptographyProvider={CryptographyProvider},version={Version})", cryptographyProvider, version);
			Logger.LogWarning("<<< Exiting GetTelerikInstalledButNotUsedResult(cryptographyProvider={CryptographyProvider},version={Version})", cryptographyProvider, version);
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
			Logger.LogError(">>> Entering GetTelerikInstalledAndUsedResult(cryptographyProvider={CryptographyProvider},assemblies={Assemblies},version={Version})", cryptographyProvider, assemblies, version);
			Logger.LogWarning("<<< Exiting GetTelerikInstalledAndUsedResult(cryptographyProvider={CryptographyProvider},assemblies={Assemblies},version={Version})", cryptographyProvider, assemblies, version);
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
			Logger.LogError(">>> Entering LocalizePage()");
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
			Logger.LogWarning("<<< Exiting LocalizePage()");
        }

        private void SetBrowserLanguage()
        {
			Logger.LogError(">>> Entering SetBrowserLanguage()");
            string cultureCode;
            if (string.IsNullOrEmpty(this.PageLocale.Value) && string.IsNullOrEmpty(culture))
            {
                cultureCode = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(supportedLanguages);
				Logger.LogError("SetBrowserLanguage(): cultureCode → {CultureCode}", cultureCode);
            }
            else if (string.IsNullOrEmpty(this.PageLocale.Value) && !string.IsNullOrEmpty(culture))
            {
                cultureCode = culture;
				Logger.LogDebug("SetBrowserLanguage(): cultureCode → {CultureCode}", cultureCode);
            }
            else
            {
                cultureCode = this.PageLocale.Value;
            }

            this.PageLocale.Value = cultureCode;
            culture = cultureCode;
			Logger.LogDebug("SetBrowserLanguage(): culture → {Culture}", culture);

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
			Logger.LogWarning("<<< Exiting SetBrowserLanguage()");
        }

        private void CompleteUpgrade()
        {
			Logger.LogError(">>> Entering CompleteUpgrade()");
            // Delete the status file.
            try
            {
                File.Delete(StatusFile);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CompleteUpgrade(): {Arg0}", Exception.Message);
                // Do nothing
            }

            // remove installwizard files added back by upgrade package
            Upgrade.Upgrade.DeleteInstallerFiles();

            Config.Touch(this.applicationStatus);
            this.Response.Redirect("../Default.aspx", true);
			Logger.LogWarning("<<< Exiting CompleteUpgrade()");
        }

        private void SslRequiredCheck()
        {
			Logger.LogError(">>> Entering SslRequiredCheck()");
            if (this.hostSettings.UpgradeForceSsl && !this.Request.IsSecureConnection)
            {
                var sslDomain = this.hostSettings.SslDomain;
				Logger.LogError("SslRequiredCheck(): sslDomain → {SslDomain}", sslDomain);
                if (string.IsNullOrEmpty(sslDomain))
                {
                    sslDomain = this.Request.Url.Host;
                }
                else if (sslDomain.Contains("://"))
                {
                    sslDomain = sslDomain.Substring(sslDomain.IndexOf("://") + 3);
                }

                var sslUrl = $"https://{sslDomain}{this.Request.RawUrl}";
				Logger.LogError("SslRequiredCheck(): sslUrl → {SslUrl}", sslUrl);

                this.Response.Redirect(sslUrl, true);
            }
			Logger.LogWarning("<<< Exiting SslRequiredCheck()");
        }
    }
}
