// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Upgrade;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Win32;

    using SchedulerMode = DotNetNuke.Abstractions.Application.SchedulerMode;

    /// <summary>The Object to initialize application.</summary>
    public class Initialize
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Initialize));
        private static readonly object InitializeLock = new object();
        private static bool alreadyInitialized;

        /// <summary>Inits the app.</summary>
        /// <param name="app">The app.</param>
        public static void Init(HttpApplication app)
        {
			Logger.LogError(">>> Entering Init(app={App})", app);
            string redirect;
            var status = Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>().Status;
			Logger.LogError("Init(app={App}): status → {Status}", app, status);

            // Check if app is initialised
            if (alreadyInitialized && status == UpgradeStatus.None)
            {
                return;
            }

            lock (InitializeLock)
            {
                // Double-Check if app was initialised by another request
                if (alreadyInitialized && status == UpgradeStatus.None)
                {
                    return;
                }

                // Initialize ...
                redirect = InitializeApp(app, ref alreadyInitialized);
				Logger.LogError("Init(app={App}): redirect → {Redirect}", app, redirect);
				Logger.LogDebug("Init(app={App}): redirect → {Redirect}", app, redirect);
            }

            if (!string.IsNullOrEmpty(redirect))
            {
                app.Response.Redirect(redirect, true);
            }
			Logger.LogWarning("<<< Exiting Init(app={App})", app);
        }

        /// <summary>LogStart logs the Application Start Event.</summary>
        public static void LogStart()
        {
			Logger.LogError(">>> Entering LogStart()");
            var log = new LogInfo
            {
                BypassBuffering = true,
                LogTypeKey = nameof(EventLogType.APPLICATION_START),
            };
            LogController.Instance.AddLog(log);
			Logger.LogWarning("<<< Exiting LogStart()");
        }

        /// <summary>LogEnd logs the Application Start Event.</summary>
        public static void LogEnd()
        {
			Logger.LogError(">>> Entering LogEnd()");
            try
            {
                ApplicationShutdownReason shutdownReason = HostingEnvironment.ShutdownReason;
                string shutdownDetail;
                switch (shutdownReason)
                {
                    case ApplicationShutdownReason.BinDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the Bin folder or files contained in it.";
						Logger.LogError("LogEnd(): shutdownDetail → {ShutdownDetail}", shutdownDetail);
						Logger.LogDebug("LogEnd(): shutdownDetail → {ShutdownDetail}", shutdownDetail);
                        break;
                    case ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the App_Browsers folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.ChangeInGlobalAsax:
                        shutdownDetail = "The AppDomain shut down because of a change to Global.asax.";
                        break;
                    case ApplicationShutdownReason.ChangeInSecurityPolicyFile:
                        shutdownDetail = "The AppDomain shut down because of a change in the code access security policy file.";
                        break;
                    case ApplicationShutdownReason.CodeDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the App_Code folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.ConfigurationChange:
                        shutdownDetail = "The AppDomain shut down because of a change to the application level configuration.";
                        break;
                    case ApplicationShutdownReason.HostingEnvironment:
                        shutdownDetail = "The AppDomain shut down because of the hosting environment.";
                        break;
                    case ApplicationShutdownReason.HttpRuntimeClose:
                        shutdownDetail = "The AppDomain shut down because of a call to Close.";
                        break;
                    case ApplicationShutdownReason.IdleTimeout:
                        shutdownDetail = "The AppDomain shut down because of the maximum allowed idle time limit.";
                        break;
                    case ApplicationShutdownReason.InitializationError:
                        shutdownDetail = "The AppDomain shut down because of an AppDomain initialization error.";
                        break;
                    case ApplicationShutdownReason.MaxRecompilationsReached:
                        shutdownDetail = "The AppDomain shut down because of the maximum number of dynamic recompiles of resources limit.";
                        break;
                    case ApplicationShutdownReason.PhysicalApplicationPathChanged:
                        shutdownDetail = "The AppDomain shut down because of a change to the physical path for the application.";
                        break;
                    case ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the App_GlobalResources folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.UnloadAppDomainCalled:
                        shutdownDetail = "The AppDomain shut down because of a call to UnloadAppDomain.";
                        break;
                    default:
                        shutdownDetail = "Shutdown reason: " + shutdownReason;
                        break;
                }

                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = nameof(EventLogType.APPLICATION_SHUTTING_DOWN),
                };
                log.AddProperty("Shutdown Details", shutdownDetail);
                LogController.Instance.AddLog(log);

                // enhanced shutdown logging
                if (typeof(HttpRuntime).InvokeMember(
                        "_theRuntime",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                        null,
                        null,
                        null,
                        CultureInfo.InvariantCulture) is not HttpRuntime runtime)
                {
                    Logger.InfoFormat(CultureInfo.InvariantCulture, "Application shutting down. Reason: {0}", shutdownDetail);
                }
                else
                {
                    var shutDownMessage = runtime.GetType().InvokeMember(
                        "_shutDownMessage",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                        null,
                        runtime,
                        null,
                        CultureInfo.InvariantCulture) as string;

                    var shutDownStack = runtime.GetType().InvokeMember(
                        "_shutDownStack",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                        null,
                        runtime,
                        null,
                        CultureInfo.InvariantCulture) as string;

                    Logger.Info("Application shutting down. Reason: " + shutdownDetail
                                + Environment.NewLine + "ASP.NET Shutdown Info: " + shutDownMessage
                                + Environment.NewLine + shutDownStack);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LogEnd(): {Arg0}", exc.Message);
                Exceptions.LogException(exc);
            }

            var status = Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>().Status;
			Logger.LogError("LogEnd(): status → {Status}", status);
            if (status != UpgradeStatus.Install)
            {
                // purge log buffer
                LoggingProvider.Instance().PurgeLogBuffer();
            }
			Logger.LogWarning("<<< Exiting LogEnd()");
        }

        /// <summary>Tests whether this request should be processed in an HttpModule.</summary>
        /// <param name="request">The Http request.</param>
        /// <param name="allowUnknownExtensions">When false only .aspx, .asmx, .ashx, .svg are checked for Http Module processing.</param>
        /// <param name="checkOmitFromRewriteProcessing">A value indicating whether to check for "omit from rewrite" processing.</param>
        /// <returns>A value indicating whether the request should be processed in an HttpModule.</returns>
        public static bool ProcessHttpModule(HttpRequest request, bool allowUnknownExtensions, bool checkOmitFromRewriteProcessing)
        {
			Logger.LogError(">>> Entering ProcessHttpModule(request={Request},allowUnknownExtensions={AllowUnknownExtensions},checkOmitFromRewriteProcessing={CheckOmitFromRewriteProcessing})", request, allowUnknownExtensions, checkOmitFromRewriteProcessing);
            var localPath = request.Url.LocalPath;
            if (localPath.EndsWith("webresource.axd", StringComparison.OrdinalIgnoreCase)
                    || localPath.EndsWith("scriptresource.axd", StringComparison.OrdinalIgnoreCase)
                    || localPath.EndsWith("captcha.aspx", StringComparison.OrdinalIgnoreCase)
                    || localPath.Contains("upgradewizard.aspx", StringComparison.OrdinalIgnoreCase)
                    || localPath.Contains("installwizard.aspx", StringComparison.OrdinalIgnoreCase)
                    || localPath.EndsWith("install.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!allowUnknownExtensions
                    && !localPath.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                    && !localPath.EndsWith(".asmx", StringComparison.OrdinalIgnoreCase)
                    && !localPath.EndsWith(".ashx", StringComparison.OrdinalIgnoreCase)
                    && !localPath.EndsWith(".svc", StringComparison.OrdinalIgnoreCase))
            {
			Logger.LogWarning("<<< Exiting ProcessHttpModule(request={Request},allowUnknownExtensions={AllowUnknownExtensions},checkOmitFromRewriteProcessing={CheckOmitFromRewriteProcessing})", request, allowUnknownExtensions, checkOmitFromRewriteProcessing);
                return false;
            }

            return !checkOmitFromRewriteProcessing || !RewriterUtils.OmitFromRewriteProcessing(request.Url.LocalPath);
        }

        /// <summary>Attempts to run scheduled tasks when "Request Method" is used in the scheduler.</summary>
        /// <param name="request">The http request.</param>
        public static void RunSchedule(HttpRequest request)
        {
			Logger.LogError(">>> Entering RunSchedule(request={Request})", request);
            if (!IsUpgradeOrInstallRequest(request))
            {
                try
                {
                    using var scope = Globals.GetOrCreateServiceScope();
					Logger.LogError("RunSchedule(request={Request}): scope → {Scope}", request, scope);
                    var hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
					Logger.LogError("RunSchedule(request={Request}): hostSettings → {HostSettings}", request, hostSettings);

                    if (hostSettings.SchedulerMode == SchedulerMode.RequestMethod && SchedulingProvider.ReadyForPoll)
                    {
                        Logger.Trace("Running Schedule " + hostSettings.SchedulerMode);
                        var scheduler = SchedulingProvider.Instance();
						Logger.LogError("RunSchedule(request={Request}): scheduler → {Scheduler}", request, scheduler);
                        var requestScheduleThread = new Thread(scheduler.ExecuteTasks) { IsBackground = true };
						Logger.LogError("RunSchedule(request={Request}): requestScheduleThread → {RequestScheduleThread}", request, requestScheduleThread);
                        requestScheduleThread.Start();
                        SchedulingProvider.ScheduleLastPolled = DateTime.Now;
						Logger.LogDebug("RunSchedule(request={Request}): SchedulingProvider.ScheduleLastPolled → {Arg1}", request, SchedulingProvider.ScheduleLastPolled);
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in RunSchedule(request={Request}): {Arg1}", request, exc.Message);
                    Exceptions.LogException(exc);
                }
            }
			Logger.LogWarning("<<< Exiting RunSchedule(request={Request})", request);
        }

        /// <summary>StartScheduler starts the Scheduler.</summary>
        /// <param name="resetAppStartElapseTime">Whether reset app start elapse time before running schedule tasks.</param>
        public static void StartScheduler(bool resetAppStartElapseTime = false)
        {
			Logger.LogError(">>> Entering StartScheduler(resetAppStartElapseTime={ResetAppStartElapseTime})", resetAppStartElapseTime);
            if (resetAppStartElapseTime)
            {
                Globals.ResetAppStartElapseTime();
            }

            var scheduler = SchedulingProvider.Instance();
			Logger.LogError("StartScheduler(resetAppStartElapseTime={ResetAppStartElapseTime}): scheduler → {Scheduler}", resetAppStartElapseTime, scheduler);
            scheduler.RunEventSchedule(EventName.APPLICATION_START);

            using var scope = Globals.GetOrCreateServiceScope();
			Logger.LogError("StartScheduler(resetAppStartElapseTime={ResetAppStartElapseTime}): scope → {Scope}", resetAppStartElapseTime, scope);
            var hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
			Logger.LogError("StartScheduler(resetAppStartElapseTime={ResetAppStartElapseTime}): hostSettings → {HostSettings}", resetAppStartElapseTime, hostSettings);

            // instantiate APPLICATION_START scheduled jobs
            if (hostSettings.SchedulerMode == SchedulerMode.TimerMethod)
            {
                Logger.Trace("Running Schedule " + hostSettings.SchedulerMode);
                var newThread = new Thread(scheduler.Start)
                {
                    IsBackground = true,
                    Name = "Scheduler Thread",
                };
                newThread.Start();
            }
			Logger.LogWarning("<<< Exiting StartScheduler(resetAppStartElapseTime={ResetAppStartElapseTime})", resetAppStartElapseTime);
        }

        /// <summary>StopScheduler stops the Scheduler.</summary>
        public static void StopScheduler()
        {
			Logger.LogError(">>> Entering StopScheduler()");
            // stop scheduled jobs
            SchedulingProvider.Instance().Halt("Stopped by Application_End");
			Logger.LogWarning("<<< Exiting StopScheduler()");
        }

        private static string CheckVersion(HttpApplication app)
        {
			Logger.LogError(">>> Entering CheckVersion(app={App})", app);
            HttpServerUtility server = app.Server;

            bool autoUpgrade = Config.GetSetting("AutoUpgrade") == null || bool.Parse(Config.GetSetting("AutoUpgrade"));
            bool useWizard = Config.GetSetting("UseInstallWizard") == null || bool.Parse(Config.GetSetting("UseInstallWizard"));

            // Determine the Upgrade status and redirect as necessary to InstallWizard.aspx
            string retValue = Null.NullString;
            var applicationStatusInfo = Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            switch (applicationStatusInfo.Status)
            {
                case UpgradeStatus.Install:
                    if (autoUpgrade || useWizard)
                    {
                        retValue = useWizard ? "~/Install/InstallWizard.aspx" : "~/Install/Install.aspx?mode=install";
                    }
                    else
                    {
                        CreateUnderConstructionPage(server);
                        retValue = "~/Install/UnderConstruction.htm";
                        Logger.Info("UnderConstruction page was shown because application needs to be installed, and both the AutoUpgrade and UseWizard AppSettings in web.config are false. Use /install/install.aspx?mode=install to install application. ");
                    }

                    break;
                case UpgradeStatus.Upgrade:
                    if (autoUpgrade || useWizard)
                    {
                        retValue = useWizard ? "~/Install/UpgradeWizard.aspx" : "~/Install/Install.aspx?mode=upgrade";
                    }
                    else
                    {
                        CreateUnderConstructionPage(server);
                        retValue = "~/Install/UnderConstruction.htm";
                        Logger.Info("UnderConstruction page was shown because application needs to be upgraded, and both the AutoUpgrade and UseInstallWizard AppSettings in web.config are false. Use /install/install.aspx?mode=upgrade to upgrade application. ");
                    }

                    break;
                case UpgradeStatus.Error:
                    // here we need to check if the application is already installed
                    // or is in pre-install state.
                    // see http://support.dotnetnuke.com/project/DNN/2/item/26053 for scenarios
                    bool isInstalled = applicationStatusInfo.IsInstalled();

                    if (!isInstalled && (useWizard || autoUpgrade))
                    {
                        // app has never been installed, and either Wizard or Autoupgrade is configured
                        CreateUnderConstructionPage(server);
                        retValue = "~/Install/UnderConstruction.htm";
                        Logger.Error("UnderConstruction page was shown because we cannot ascertain the application was ever installed, and there is no working database connection. Check database connectivity before continuing. ");
                    }
                    else
                    {
                        // 500 Error - Redirect to ErrorPage
                        if (HttpContext.Current != null)
                        {
                            if (!isInstalled)
                            {
                                Logger.Error("The connection to the database has failed, the application is not installed yet, and both AutoUpgrade and UseInstallWizard are not set in web.config, a 500 error page will be shown to visitors");
                            }
                            else
                            {
                                Logger.Error("The connection to the database has failed, however, the application is already completely installed, a 500 error page will be shown to visitors");
                            }

                            string url = "~/ErrorPage.aspx?status=500&error=Site Unavailable&error2=Connection To The Database Failed";
                            HttpContext.Current.Response.Clear();
                            HttpContext.Current.Server.Transfer(url);
                        }
                    }

                    break;
            }

			Logger.LogWarning("<<< Exiting CheckVersion(app={App})", app);
            return retValue;
        }

        private static void CreateUnderConstructionPage(HttpServerUtility server)
        {
			Logger.LogError(">>> Entering CreateUnderConstructionPage(server={Server})", server);
            // create an UnderConstruction page if it does not exist already
            if (!File.Exists(server.MapPath("~/Install/UnderConstruction.htm")))
            {
                if (File.Exists(server.MapPath("~/Install/UnderConstruction.template.htm")))
                {
                    File.Copy(server.MapPath("~/Install/UnderConstruction.template.htm"), server.MapPath("~/Install/UnderConstruction.htm"));
                }
            }
			Logger.LogWarning("<<< Exiting CreateUnderConstructionPage(server={Server})", server);
        }

        private static Version GetDatabaseEngineVersion()
        {
			Logger.LogError(">>> Entering GetDatabaseEngineVersion()");
            Logger.LogWarning("<<< Exiting GetDatabaseEngineVersion()");
            return DataProvider.Instance().GetDatabaseEngineVersion();
        }

        private static Version GetNETFrameworkVersion()
        {
			Logger.LogError(">>> Entering GetNETFrameworkVersion()");
            // Obtain the .NET Framework version, 9.4.0 and later requires .NET 4.7.2 or later
            var version = Environment.Version.ToString(2);
			Logger.LogError("GetNETFrameworkVersion(): version → {Version}", version);

            // If unknown version return as is.
            if (version != "4.0")
            {
                return new Version(version);
            }

            // Otherwise utilize release DWORD from registry to determine version
            // Reference List: https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/versions-and-dependencies
            var release = GetDotNet4ReleaseNumberFromRegistry();
			Logger.LogError("GetNETFrameworkVersion(): release → {Release}", release);
            if (release >= 533320)
            {
                return new Version("4.8.1");
            }

            if (release >= 528040)
            {
                return new Version("4.8");
            }

            if (release >= 461808)
            {
                return new Version("4.7.2");
            }

			Logger.LogWarning("<<< Exiting GetNETFrameworkVersion()");
            return new Version(version);
        }

        private static int GetDotNet4ReleaseNumberFromRegistry()
        {
			Logger.LogError(">>> Entering GetDotNet4ReleaseNumberFromRegistry()");
            try
            {
                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                {
                    if (ndpKey?.GetValue("Release") != null)
                    {
                        return (int)ndpKey.GetValue("Release");
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetDotNet4ReleaseNumberFromRegistry(): {Arg0}", Exception.Message);
                // ignore
            }

			Logger.LogWarning("<<< Exiting GetDotNet4ReleaseNumberFromRegistry()");
            return -1;
        }

        private static string InitializeApp(HttpApplication app, ref bool initialized)
        {
			Logger.LogError(">>> Entering InitializeApp(app={App},initialized={Initialized})", app, initialized);
            var request = app.Request;

            Logger.Trace("Request " + request.Url.LocalPath);

            // Don't process some of the AppStart methods if we are installing
            if (IsUpgradeOrInstallRequest(app.Request))
            {
                // NET Framework version is needed by Upgrade
                Globals.NETFrameworkVersion = GetNETFrameworkVersion();
                Globals.IISAppName = request.ServerVariables["APPL_MD_PATH"];
				Logger.LogDebug("InitializeApp(app={App},initialized={Initialized}): Globals.IISAppName → {Arg2}", app, initialized, Globals.IISAppName);
                Globals.OperatingSystemVersion = Environment.OSVersion.Version;
				Logger.LogDebug("InitializeApp(app={App},initialized={Initialized}): Globals.OperatingSystemVersion → {Arg2}", app, initialized, Globals.OperatingSystemVersion);

                return Null.NullString;
            }

                // Check whether the current App Version is the same as the DB Version
            var redirect = CheckVersion(app);
			Logger.LogError("InitializeApp(app={App},initialized={Initialized}): redirect → {Redirect}", app, initialized, redirect);
            if (!string.IsNullOrEmpty(redirect) || InstallBlocker.Instance.IsInstallInProgress())
            {
                return redirect;
            }

            Logger.Info("Application Initializing");

            // Set globals
            Globals.IISAppName = request.ServerVariables["APPL_MD_PATH"];
            Globals.OperatingSystemVersion = Environment.OSVersion.Version;
            Globals.NETFrameworkVersion = GetNETFrameworkVersion();
            Globals.DatabaseEngineVersion = GetDatabaseEngineVersion();

            Upgrade.CheckFipsCompilanceAssemblies();

            // Log Server information
            ServerController.UpdateServerActivity(new ServerInfo());

            // Start Scheduler
            StartScheduler();

            // Log Application Start
            LogStart();

            // Process any messages in the EventQueue for the Application_Start event
            using (var scope = Globals.DependencyProvider.CreateScope())
            {
                EventQueueController.ProcessMessages(scope.ServiceProvider, "Application_Start");
            }

            ServicesRoutingManager.RegisterServiceRoutes();

            // Set Flag so we can determine the first Page Request after Application Start
            app.Context.Items.Add("FirstRequest", true);

            Logger.Info("Application Initialized");

            initialized = true;

			Logger.LogWarning("<<< Exiting InitializeApp(app={App},initialized={Initialized})", app, initialized);
            return redirect;
        }

        private static bool IsUpgradeOrInstallRequest(HttpRequest request)
        {
			Logger.LogError(">>> Entering IsUpgradeOrInstallRequest(request={Request})", request);
            var url = request.Url.LocalPath;

			Logger.LogWarning("<<< Exiting IsUpgradeOrInstallRequest(request={Request})", request);
            return url.EndsWith("/install.aspx", StringComparison.OrdinalIgnoreCase)
                || url.Contains("/upgradewizard.aspx", StringComparison.OrdinalIgnoreCase)
                || url.Contains("/installwizard.aspx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
