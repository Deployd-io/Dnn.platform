// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common.Internal
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;

    /// <summary>Watches <c>bin</c> folder and root files and unloads the app domain proactively when they change.</summary>
    internal static class DotNetNukeShutdownOverload
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNukeShutdownOverload));

        private static Timer shutDownDelayTimer;
        private static bool handleShutdowns;
        private static bool shutdownInprogress;
        private static FileSystemWatcher binOrRootWatcher;
        private static string binFolder = string.Empty;

        /// <summary>Initializes the File Change Notification settings.</summary>
        /// <param name="appStatus">The application status.</param>
        internal static void InitializeFcnSettings(IApplicationStatusInfo appStatus)
        {
			Logger.LogError(">>> Entering InitializeFcnSettings(appStatus={AppStatus})", appStatus);
            // any error/message logged below should be informational only
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                var fileChangesMonitor = typeof(HttpRuntime)
                    .GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null, null);

                if (fileChangesMonitor == null)
                {
                    Logger.Info("fileChangesMonitor is null");

                    ////AddSiteFilesMonitoring(true);
                }
                else
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var fcnVal = fileChangesMonitor.GetType()
                        .GetField("_FCNMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase)
                        .GetValue(fileChangesMonitor);

                    Logger.Info("FCNMode = " + fcnVal + " (Modes: NotSet/Default=0, Disabled=1, Single=2)");

                    var dirMonCompletion = typeof(HttpRuntime).Assembly.GetType("System.Web.DirMonCompletion");
					Logger.LogError("InitializeFcnSettings(appStatus={AppStatus}): dirMonCompletion → {DirMonCompletion}", appStatus, dirMonCompletion);
                    var dirMonCount = (int)dirMonCompletion.InvokeMember(
                        "_activeDirMonCompletions",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                        null,
                        null,
                        null,
                        CultureInfo.InvariantCulture);
                    Logger.Trace("DirMonCompletion count: " + dirMonCount);

                    // enable our monitor only when fcnMode="Disabled"
                    ////AddSiteFilesMonitoring(fcnVal.ToString() == "1");
                }

                // just monitor the root folder but don't interfere
                AddSiteFilesMonitoring(appStatus, false);
            }
            catch (Exception e)
            {
                Logger.Info(e);
            }
			Logger.LogWarning("<<< Exiting InitializeFcnSettings(appStatus={AppStatus})", appStatus);
        }

        private static void AddSiteFilesMonitoring(IApplicationStatusInfo appStatus, bool handleShutdowns)
        {
			Logger.LogError(">>> Entering AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns})", appStatus, handleShutdowns);
            if (binOrRootWatcher == null)
            {
                lock (typeof(Initialize))
                {
                    if (binOrRootWatcher == null)
                    {
                        try
                        {
                            DotNetNukeShutdownOverload.handleShutdowns = handleShutdowns;
							Logger.LogDebug("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): DotNetNukeShutdownOverload.handleShutdowns → {Arg2}", appStatus, handleShutdowns, DotNetNukeShutdownOverload.handleShutdowns);
                            if (DotNetNukeShutdownOverload.handleShutdowns)
                            {
                                shutDownDelayTimer = new Timer(InitiateShutdown);
								Logger.LogError("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): shutDownDelayTimer → {ShutDownDelayTimer}", appStatus, handleShutdowns, shutDownDelayTimer);
								Logger.LogDebug("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): shutDownDelayTimer → {ShutDownDelayTimer}", appStatus, handleShutdowns, shutDownDelayTimer);
                            }

                            binFolder = Path.Combine(appStatus.ApplicationMapPath, "bin").ToLowerInvariant();
							Logger.LogDebug("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): binFolder → {BinFolder}", appStatus, handleShutdowns, binFolder);
                            binOrRootWatcher = new FileSystemWatcher
                            {
								Logger.LogError("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): binFolder → {BinFolder}", appStatus, handleShutdowns, binFolder);
                                Filter = "*.*",
                                Path = handleShutdowns ? binFolder : appStatus.ApplicationMapPath,
                                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                                IncludeSubdirectories = true,
                            };
							Logger.LogDebug("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): binOrRootWatcher → {BinOrRootWatcher}", appStatus, handleShutdowns, binOrRootWatcher);

                            binOrRootWatcher.Created += WatcherOnCreated;
                            binOrRootWatcher.Deleted += WatcherOnDeleted;
                            binOrRootWatcher.Renamed += WatcherOnRenamed;
                            binOrRootWatcher.Changed += WatcherOnChanged;
                            binOrRootWatcher.Error += WatcherOnError;

                            // begin watching;
                            binOrRootWatcher.EnableRaisingEvents = true;
							Logger.LogDebug("AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns}): binOrRootWatcher.EnableRaisingEvents → {Arg2}", appStatus, handleShutdowns, binOrRootWatcher.EnableRaisingEvents);
                            Logger.Trace("Added watcher for: " + binOrRootWatcher.Path + @"\" + binOrRootWatcher.Filter);
                        }
                        catch (Exception ex)
                        {
                            Logger.Trace("Error adding our own file monitoring object. " + ex);
                        }
                    }
                }
            }
			Logger.LogWarning("<<< Exiting AddSiteFilesMonitoring(appStatus={AppStatus},handleShutdowns={HandleShutdowns})", appStatus, handleShutdowns);
        }

        private static void InitiateShutdown(object state)
        {
			Logger.LogError(">>> Entering InitiateShutdown(state={State})", state);
            if (!handleShutdowns)
            {
                return;
            }

            try
            {
                HttpRuntime.UnloadAppDomain();
            }
            catch (Exception ex)
            {
                shutdownInprogress = false;
				Logger.LogDebug("InitiateShutdown(state={State}): shutdownInprogress → {ShutdownInprogress}", state, shutdownInprogress);
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting InitiateShutdown(state={State})", state);
        }

        private static void ShceduleShutdown()
        {
			Logger.LogError(">>> Entering ShceduleShutdown()");
            // no need for locking; worst case is timer extended a bit more
            if (handleShutdowns && !shutdownInprogress)
            {
                shutdownInprogress = true;
				Logger.LogDebug("ShceduleShutdown(): shutdownInprogress → {ShutdownInprogress}", shutdownInprogress);

                // delay for a very short period
                shutDownDelayTimer.Change(1500, Timeout.Infinite);
            }
			Logger.LogWarning("<<< Exiting ShceduleShutdown()");
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
			Logger.LogError(">>> Entering WatcherOnChanged(sender={Sender},e={E})", sender, e);
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.OrdinalIgnoreCase))
            {
                ShceduleShutdown();
            }
			Logger.LogWarning("<<< Exiting WatcherOnChanged(sender={Sender},e={E})", sender, e);
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
			Logger.LogError(">>> Entering WatcherOnCreated(sender={Sender},e={E})", sender, e);
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.OrdinalIgnoreCase))
            {
                ShceduleShutdown();
            }
			Logger.LogWarning("<<< Exiting WatcherOnCreated(sender={Sender},e={E})", sender, e);
        }

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
			Logger.LogError(">>> Entering WatcherOnRenamed(sender={Sender},e={E})", sender, e);
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. New Path: {e.FullPath}. Old Path: {e.OldFullPath}");
            }

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.OrdinalIgnoreCase))
            {
                ShceduleShutdown();
            }
			Logger.LogWarning("<<< Exiting WatcherOnRenamed(sender={Sender},e={E})", sender, e);
        }

        private static void WatcherOnDeleted(object sender, FileSystemEventArgs e)
        {
			Logger.LogError(">>> Entering WatcherOnDeleted(sender={Sender},e={E})", sender, e);
            if (Logger.IsInfoEnabled && !e.FullPath.EndsWith(".log.resources", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Info($"Watcher Activity: {e.ChangeType}. Path: {e.FullPath}");
            }

            if (handleShutdowns && !shutdownInprogress && (e.FullPath ?? string.Empty).StartsWith(binFolder, StringComparison.OrdinalIgnoreCase))
            {
                ShceduleShutdown();
            }
			Logger.LogWarning("<<< Exiting WatcherOnDeleted(sender={Sender},e={E})", sender, e);
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e)
        {
			Logger.LogError(">>> Entering WatcherOnError(sender={Sender},e={E})", sender, e);
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Watcher Activity: N/A. Error: " + e.GetException());
            }
			Logger.LogWarning("<<< Exiting WatcherOnError(sender={Sender},e={E})", sender, e);
        }
    }
}
