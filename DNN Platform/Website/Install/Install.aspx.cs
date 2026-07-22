// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Install
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A page which installs or upgrades DNN.</summary>
    public partial class Install : Page
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected static string UpgradeWizardLocalResourceFile = "~/Install/App_LocalResources/UpgradeWizard.aspx.resx";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Install));
        private static readonly object InstallLocker = new object();

        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="Install"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public Install()
            : this(null)
        {
		Logger.LogDebug(">>> Entering Install()");
        }
		Logger.LogDebug("<<< Exiting Install()");

        /// <summary>Initializes a new instance of the <see cref="Install"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public Install(IApplicationStatusInfo appStatus)
            : this(appStatus, null)
        {
		Logger.LogDebug(">>> Entering Install(appStatus={AppStatus})", appStatus);
		Logger.LogDebug("<<< Exiting Install(appStatus={AppStatus})", appStatus);
        }

        /// <summary>Initializes a new instance of the <see cref="Install"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public Install(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			Logger.LogDebug("Install(appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", appStatus, eventLogger, eventLogger);
			Logger.LogDebug("Install(appStatus={AppStatus},eventLogger={EventLogger}): appStatus → {AppStatus1}", appStatus, eventLogger, appStatus);
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

            // if previous config deleted create new empty one
            string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
            if (!File.Exists(installConfig))
			Logger.LogDebug("OnInit(e={E}): installConfig → {InstallConfig}", e, installConfig);
            {
                File.Copy(installConfig + ".resources", installConfig);
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            Config.AddFCNMode(this.appStatus, Config.FcnMode.Single);

            // Get current Script time-out
            int scriptTimeOut = this.Server.ScriptTimeout;

            string mode = string.Empty;
            if (this.Request.QueryString["mode"] != null)
			Logger.LogDebug("OnLoad(e={E}): mode → {Mode}", e, mode);
            {
                mode = this.Request.QueryString["mode"].ToLowerInvariant();
            }

            // Disable Client side caching
            this.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);

            // Check mode is not Nothing
            if (mode == "none")
            {
                this.NoUpgrade();
            }
            else
            {
                // Set Script timeout to MAX value
                this.Server.ScriptTimeout = int.MaxValue;

                switch (this.appStatus.Status)
                {
                    case UpgradeStatus.Install:
                        this.InstallApplication();

                        // Force an App Restart
                        Config.Touch(this.appStatus);
                        break;
                    case UpgradeStatus.Upgrade:
                        this.UpgradeApplication();

                        // Force an App Restart
                        Config.Touch(this.appStatus);
                        break;
                    case UpgradeStatus.None:
                        // Check mode
                        switch (mode)
                        {
                            case "addportal":
                                this.AddPortal();
                                break;
                            case "installresources":
                                this.InstallResources();
                                break;
                            case "executescripts":
                                this.ExecuteScripts();
                                break;
                        }

                        break;
                    case UpgradeStatus.Error:
                        this.NoUpgrade();
                        break;
                }

                // restore Script timeout
                this.Server.ScriptTimeout = scriptTimeOut;
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private static void RegisterInstallBegining()
        {
			Logger.LogDebug(">>> Entering RegisterInstallBegining()");
            InstallBlocker.Instance.RegisterInstallBegining();
			Logger.LogDebug("<<< Exiting RegisterInstallBegining()");
        }

        private static void RegisterInstallEnd()
        {
			Logger.LogDebug(">>> Entering RegisterInstallEnd()");
            InstallBlocker.Instance.RegisterInstallEnd();
			Logger.LogDebug("<<< Exiting RegisterInstallEnd()");
        }

        private static ITelerikUtils CreateTelerikUtils()
        {
			Logger.LogDebug(">>> Entering CreateTelerikUtils()");
			Logger.LogDebug("<<< Exiting CreateTelerikUtils()");
            return Globals.GetCurrentServiceProvider().GetRequiredService<ITelerikUtils>();
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

        private void ExecuteScripts()
        {
			Logger.LogDebug(">>> Entering ExecuteScripts()");
            // Start Timer
            Upgrade.Upgrade.StartTimer();

            // Write out Header
            HtmlUtils.WriteHeader(this.Response, "executeScripts");

            this.Response.Write("<h2>Execute Scripts Status Report</h2>");
            this.Response.Flush();

            string strProviderPath = DataProvider.Instance().GetProviderPath();
            if (!strProviderPath.StartsWith("ERROR:"))
            {
                Upgrade.Upgrade.ExecuteScripts(strProviderPath);
            }

            this.Response.Write("<h2>Execution Complete</h2>");
            this.Response.Flush();

            // Write out Footer
            HtmlUtils.WriteFooter(this.Response);
			Logger.LogDebug("<<< Exiting ExecuteScripts()");
        }

        private void InstallApplication()
        {
			Logger.LogDebug(">>> Entering InstallApplication()");
            // the application uses a two step installation process. The first step is used to update
            // the Web.config with any configuration settings - which forces an application restart.
            // The second step finishes the installation process and provisions the site.
            string installationDate = Config.GetSetting("InstallationDate");

            if (installationDate == null || string.IsNullOrEmpty(installationDate))
            {
                string strError = Config.UpdateMachineKey(this.appStatus);
                if (string.IsNullOrEmpty(strError))
                {
                    // send a new request to the application to initiate step 2
                    this.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                }
                else
                {
                    // 403-3 Error - Redirect to ErrorPage
                    // 403.3 means directory permissions issue
                    string strURL = "~/ErrorPage.aspx?status=403_3&error=" + strError;
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer(strURL);
                }
            }
            else
            {
                bool cleanupLocker = false;
                try
				Logger.LogDebug("InstallApplication(): cleanupLocker → {CleanupLocker}", cleanupLocker);
                {
                    var synchConnectionString = new SynchConnectionStringStep();
                    synchConnectionString.Execute();
					Logger.LogDebug("InstallApplication(): synchConnectionString → {SynchConnectionString}", synchConnectionString);
                    if (synchConnectionString.Status == StepStatus.AppRestart)
                    {
                        // send a new request to the application to initiate step 2
                        this.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                    }

                    // Start Timer
                    Upgrade.Upgrade.StartTimer();

                    // Write out Header
                    HtmlUtils.WriteHeader(this.Response, "install");

                    // get path to script files
                    string strProviderPath = DataProvider.Instance().GetProviderPath();
                    if (!strProviderPath.StartsWith("ERROR:"))
                    {
                        if (!this.CheckPermissions())
                        {
                            return;
                        }

                        // Add the install blocker logic
                        lock (InstallLocker)
                        {
                            if (InstallBlocker.Instance.IsInstallInProgress())
                            {
                                this.WriteInstallationHeader();
                                this.WriteInstallationInProgress();
                                return;
                            }

                            cleanupLocker = true;
                            RegisterInstallBegining();
                        }

                        var installConfig = InstallController.Instance.GetInstallConfig();

						Logger.LogDebug("InstallApplication(): installConfig → {InstallConfig}", installConfig);
                        // Create Folder Mappings config
                        if (!string.IsNullOrEmpty(installConfig.FolderMappingsSettings))
                        {
                            FolderMappingsConfigController.Instance.SaveConfig(installConfig.FolderMappingsSettings);
                        }

                        Upgrade.Upgrade.InstallDNN(strProviderPath);

                        // remove en-US from portal if installing in a different language
                        if (!installConfig.InstallCulture.Equals("en-us", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var locale = LocaleController.Instance.GetLocale("en-US");
                            Localization.RemoveLanguageFromPortal(this.eventLogger, 0, locale.LanguageId, true);
							Logger.LogDebug("InstallApplication(): locale → {Locale}", locale);
                        }

                        var installVersion = DataProvider.Instance().GetInstallVersion();
                        string strError = Config.UpdateInstallVersion(this.appStatus, installVersion);
						Logger.LogDebug("InstallApplication(): installVersion → {InstallVersion}", installVersion);

                        // Adding FCN mode to web.config
                        strError += Config.AddFCNMode(this.appStatus, Config.FcnMode.Single);
                        if (!string.IsNullOrEmpty(strError))
                        {
                            Logger.Error(strError);
                        }

                        this.Response.Write("<h2>Installation Complete</h2>");
                        this.Response.Write("<br><br><h2><a href='../Default.aspx'>Click Here To Access Your Site</a></h2><br><br>");
                        this.Response.Flush();

                        // remove installwizard files
                        Upgrade.Upgrade.DeleteInstallerFiles();

                        // log APPLICATION_START event
                        Initialize.LogStart();

                        // Start Scheduler
                        Initialize.StartScheduler(true);
                    }
                    else
                    {
                        // upgrade error
                        this.Response.Write("<h2>Upgrade Error: " + strProviderPath + "</h2>");
                        this.Response.Flush();
                    }

                    // Write out Footer
                    HtmlUtils.WriteFooter(this.Response);
                }
                finally
                {
                    if (cleanupLocker)
                    {
                        RegisterInstallEnd();
                    }
                }
            }
			Logger.LogDebug("<<< Exiting InstallApplication()");
        }

        private void WriteInstallationHeader()
        {
			Logger.LogDebug(">>> Entering WriteInstallationHeader()");
            this.Response.Write("<h2>Version: " + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version) + "</h2>");
            this.Response.Flush();

            this.Response.Write("<br><br>");
            this.Response.Write("<h2>Installation Status Report</h2>");
            this.Response.Flush();
			Logger.LogDebug("<<< Exiting WriteInstallationHeader()");
        }

        private void WriteInstallationInProgress()
        {
			Logger.LogDebug(">>> Entering WriteInstallationInProgress()");
            HtmlUtils.WriteFeedback(
                HttpContext.Current.Response,
                0,
                Localization.GetString("ThereIsAInstallationCurrentlyInProgress.Error", Localization.GlobalResourceFile) + "<br>");
            this.Response.Flush();
			Logger.LogDebug("<<< Exiting WriteInstallationInProgress()");
        }

        private bool CheckPermissions()
        {
			Logger.LogDebug(">>> Entering CheckPermissions()");
            bool verified = new FileSystemPermissionVerifier(this.Server.MapPath("~")).VerifyAll();
            HtmlUtils.WriteFeedback(
                HttpContext.Current.Response,
                0,
                "Checking File and Folder permissions " + (verified ? "<font color='green'>Success</font>" : "<font color='red'>Error!</font>") + "<br>");
            this.Response.Flush();

			Logger.LogDebug("<<< Exiting CheckPermissions()");
            return verified;
        }

        private void UpgradeApplication()
        {
			Logger.LogDebug(">>> Entering UpgradeApplication()");
            bool cleanupLocker = false;
            try
			Logger.LogDebug("UpgradeApplication(): cleanupLocker → {CleanupLocker}", cleanupLocker);
            {
                if (Upgrade.Upgrade.RemoveInvalidAntiForgeryCookie())
                {
                    this.Response.Redirect(this.Request.RawUrl, true);
                }

                var databaseVersion = DataProvider.Instance().GetVersion();

				Logger.LogDebug("UpgradeApplication(): databaseVersion → {DatabaseVersion}", databaseVersion);
                // Start Timer
                Upgrade.Upgrade.StartTimer();

                // Write out Header
                HtmlUtils.WriteHeader(this.Response, "upgrade");

                // There could be an installation in progress
                lock (InstallLocker)
                {
                    if (InstallBlocker.Instance.IsInstallInProgress())
                    {
                        this.WriteInstallationHeader();
                        this.WriteInstallationInProgress();
                        return;
                    }

                    cleanupLocker = true;
                    RegisterInstallBegining();
                }

                this.Response.Write("<h2>Current Assembly Version: " + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version) + "</h2>");
                this.Response.Flush();

                // get path to script files
                string strProviderPath = DataProvider.Instance().GetProviderPath();
                if (!strProviderPath.StartsWith("ERROR:"))
                {
                    // get current database version
                    var strDatabaseVersion = Globals.FormatVersion(databaseVersion);

					Logger.LogDebug("UpgradeApplication(): strDatabaseVersion → {StrDatabaseVersion}", strDatabaseVersion);
                    this.Response.Write("<h2>Current Database Version: " + strDatabaseVersion + "</h2>");
                    this.Response.Flush();

                    this.Response.Write("<br><br>");
                    this.Response.Write("<h2>Upgrade Status Report</h2>");
                    this.Response.Flush();

                    // stop scheduler
                    SchedulingProvider.Instance().Halt("Stopped by Upgrade Process");

                    Upgrade.Upgrade.UpgradeDNN(strProviderPath, databaseVersion);

                    // Install optional resources if present
                    var packages = Upgrade.Upgrade.GetInstallPackages();
                    foreach (var package in packages)
                    {
					Logger.LogDebug("UpgradeApplication(): packages → {Packages}", packages);
                        Upgrade.Upgrade.InstallPackage(package.Key, package.Value.PackageType, true);
                    }

                    // calling GetInstallVersion after SQL scripts execution to ensure sp GetDatabaseInstallVersion exists
                    var installVersion = DataProvider.Instance().GetInstallVersion();
                    string strError = Config.UpdateInstallVersion(this.appStatus, installVersion);
					Logger.LogDebug("UpgradeApplication(): installVersion → {InstallVersion}", installVersion);

                    // Adding FCN mode to web.config
                    strError += Config.AddFCNMode(this.appStatus, Config.FcnMode.Single);
                    if (!string.IsNullOrEmpty(strError))
                    {
                        Logger.Error(strError);
                    }

                    HtmlUtils.WriteFeedback(HttpContext.Current.Response, 2, "Replacing Digital Assets Manager with the new Resource Manager: ");
                    Globals.GetCurrentServiceProvider().GetService<IDamUninstaller>().Execute();
                    HtmlUtils.WriteSuccessError(HttpContext.Current.Response, true);

                    this.Response.Write("<br>");
                    this.Response.Write("<h2>Checking Security Aspects</h2>");
                    var telerikUtils = CreateTelerikUtils();
                    if (!telerikUtils.TelerikIsInstalled())
					Logger.LogDebug("UpgradeApplication(): telerikUtils → {TelerikUtils}", telerikUtils);
                    {
                        this.Response.Write(this.LocalizeString("TelerikNotInstalledInfo"));
                        this.Response.Write("<br>");
                    }
                    else
                    {
                        var version = telerikUtils.GetTelerikVersion();
                        this.Response.Write("<strong>");
						Logger.LogDebug("UpgradeApplication(): version → {Version}", version);
                        this.Response.Write(this.LocalizeString("TelerikInstalledHeading"));
                        this.Response.Write("</strong><br>");
                        this.Response.Write(this.LocalizeString("TelerikInstalledDetected"));
                        this.Response.Write(" ");
                        this.Response.Write(version.ToString());
                        this.Response.Write("<br>");

                        if (!telerikUtils.IsTelerikVersionVulnerable(version))
                        {
                            this.Response.Write(this.LocalizeString("TelerikVersionNotKnownToBeVulnerableInfo"));
                            this.Response.Write("<br>");
                        }
                        else
                        {
                            SetHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey, DotNetNuke.Maintenance.Constants.TelerikUninstallYesValue);
                            var assemblies = telerikUtils.GetAssembliesThatDependOnTelerik()
                                .Select(a => Path.GetFileName(a));

                            this.Response.Write(this.LocalizeString("TelerikInstalledBulletin"));
                            this.Response.Write("<br>");

                            if (!assemblies.Any())
                            {
                                this.Response.Write(this.LocalizeString("TelerikInstalledButNotUsedInfo"));
                                this.Response.Write("<br>");
                            }
                            else
                            {
                                this.Response.Write(this.LocalizeString("TelerikInstalledAndUsedInfo"));
                                this.Response.Write("<br>");
                                foreach (var a in assemblies)
                                {
                                    this.Response.Write($"{a}<br/>");
                                }

                                this.Response.Write("<br>");
                                this.Response.Write(this.LocalizeString("TelerikInstalledAndUsedWarning"));
                                this.Response.Write("<br>");
                            }
                        }
                    }

                    this.Response.Write("<br>");
                    this.Response.Write("<h2>Upgrade Complete</h2>");
                    this.Response.Write("<br><br><h2><a href='../Default.aspx'>Click Here To Access Your Site</a></h2><br><br>");

                    // remove installwizard files
                    Upgrade.Upgrade.DeleteInstallerFiles();

                    this.Response.Flush();
                }
                else
                {
                    this.Response.Write("<h2>Upgrade Error: " + strProviderPath + "</h2>");
                    this.Response.Flush();
                }

                // Write out Footer
                HtmlUtils.WriteFooter(this.Response);
            }
            finally
            {
                if (cleanupLocker)
                {
                    RegisterInstallEnd();
                }
            }
			Logger.LogDebug("<<< Exiting UpgradeApplication()");
        }

        private void AddPortal()
        {
			Logger.LogDebug(">>> Entering AddPortal()");
            // Start Timer
            Upgrade.Upgrade.StartTimer();

            // Write out Header
            HtmlUtils.WriteHeader(this.Response, "addPortal");
            this.Response.Write("<h2>Add Site Status Report</h2>");
            this.Response.Flush();

            // install new portal(s)
            string strNewFile = $@"{this.appStatus.ApplicationMapPath}\Install\Portal\Portals.resources";
            if (File.Exists(strNewFile))
            {
                var xmlDoc = new XmlDocument { XmlResolver = null, };
                using (var xmlReader = XmlReader.Create(strNewFile, new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(xmlReader);
                }

                // parse portal(s) if available
                var nodes = xmlDoc.SelectNodes("//dotnetnuke/portals/portal");
                if (nodes != null)
				Logger.LogDebug("AddPortal(): nodes → {Nodes}", nodes);
                {
                    foreach (XmlNode node in nodes)
                    {
                        if (node != null)
                        {
                            Upgrade.Upgrade.AddPortal(this.eventLogger, this.appStatus, node, true, 0, null);
                        }
                    }
                }

                // delete the file
                try
                {
                    File.SetAttributes(strNewFile, FileAttributes.Normal);
                    File.Delete(strNewFile);
                }
                catch (Exception ex)
                {
                    // error removing the file
                    Logger.Error(ex);
                }

                this.Response.Write("<h2>Installation Complete</h2>");
                this.Response.Write("<br><br><h2><a href='../Default.aspx'>Click Here To Access Your Site</a></h2><br><br>");
                this.Response.Flush();
            }

            // Write out Footer
            HtmlUtils.WriteFooter(this.Response);
			Logger.LogDebug("<<< Exiting AddPortal()");
        }

        private void InstallResources()
        {
			Logger.LogDebug(">>> Entering InstallResources()");
            // Start Timer
            Upgrade.Upgrade.StartTimer();

            // Write out Header
            HtmlUtils.WriteHeader(this.Response, "installResources");

            this.Response.Write("<h2>Install Resources Status Report</h2>");
            this.Response.Flush();

            // install new resources(s)
            var packages = Upgrade.Upgrade.GetInstallPackages();
            foreach (var package in packages)
            {
			Logger.LogDebug("InstallResources(): packages → {Packages}", packages);
                Upgrade.Upgrade.InstallPackage(package.Key, package.Value.PackageType, true);
            }

            this.Response.Write("<h2>Installation Complete</h2>");
            this.Response.Write("<br><br><h2><a href='../Default.aspx'>Click Here To Access Your Site</a></h2><br><br>");
            this.Response.Flush();

            // Write out Footer
            HtmlUtils.WriteFooter(this.Response);
			Logger.LogDebug("<<< Exiting InstallResources()");
        }

        private void NoUpgrade()
        {
			Logger.LogDebug(">>> Entering NoUpgrade()");
            // get path to script files
            string strProviderPath = DataProvider.Instance().GetProviderPath();
            if (!strProviderPath.StartsWith("ERROR:"))
            {
                // get current database version
                try
                {
                    using (var dr = DataProvider.Instance().GetDatabaseVersion())
                    {
                        if (dr.Read())
                        {
                            // Write out Header
                            HtmlUtils.WriteHeader(this.Response, "none");
                            string currentAssembly = DotNetNukeContext.Current.Application.Version.ToString(3);
                            string currentDatabase = dr["Major"] + "." + dr["Minor"] + "." + dr["Build"];

                            // do not show versions if the same to stop information leakage
                            if (currentAssembly == currentDatabase)
                            {
                                this.Response.Write("<h2>Current Assembly Version && current Database Version are identical.</h2>");
                            }
                            else
                            {
                                this.Response.Write("<h2>Current Assembly Version: " + currentAssembly + "</h2>");

                                // Call Upgrade with the current DB Version to upgrade an
                                // existing DNN installation
                                var strDatabaseVersion = ((int)dr["Major"]).ToString("00") + "." + ((int)dr["Minor"]).ToString("00") + "." + ((int)dr["Build"]).ToString("00");
                                this.Response.Write("<h2>Current Database Version: " + strDatabaseVersion + "</h2>");
								Logger.LogDebug("NoUpgrade(): strDatabaseVersion → {StrDatabaseVersion}", strDatabaseVersion);
                            }

                            this.Response.Write("<br><br><a href='Install.aspx?mode=Install'>Click Here To Upgrade DotNetNuke</a>");
                            this.Response.Flush();
                        }
                        else
                        {
                            // Write out Header
                            HtmlUtils.WriteHeader(this.Response, "noDBVersion");
                            this.Response.Write("<h2>Current Assembly Version: " + DotNetNukeContext.Current.Application.Version.ToString(3) + "</h2>");

                            this.Response.Write("<h2>Current Database Version: N/A</h2>");
                            this.Response.Write("<br><br><h2><a href='Install.aspx?mode=Install'>Click Here To Install DotNetNuke</a></h2>");
                            this.Response.Flush();
                        }

                        dr.Close();
                    }
                }
                catch (Exception ex)
                {
                    // Write out Header
                    Logger.Error(ex);
                    HtmlUtils.WriteHeader(this.Response, "error");
                    this.Response.Write("<h2>Current Assembly Version: " + DotNetNukeContext.Current.Application.Version.ToString(3) + "</h2>");

                    this.Response.Write("<h2>" + ex.Message + "</h2>");
                    this.Response.Flush();
                }
            }
            else
            {
                // Write out Header
                HtmlUtils.WriteHeader(this.Response, "error");
                this.Response.Write("<h2>Current Assembly Version: " + DotNetNukeContext.Current.Application.Version.ToString(3) + "</h2>");

                this.Response.Write("<h2>" + strProviderPath + "</h2>");
                this.Response.Flush();
            }

            // Write out Footer
            HtmlUtils.WriteFooter(this.Response);
			Logger.LogDebug("<<< Exiting NoUpgrade()");
        }

        private string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, UpgradeWizardLocalResourceFile);
        }
    }
}
