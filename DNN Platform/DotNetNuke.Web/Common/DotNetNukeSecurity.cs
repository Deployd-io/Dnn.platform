// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Web.Common.Internal
{
    internal static class DotNetNukeSecurity
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DotNetNukeSecurity));
        private static FileSystemWatcher _fileWatcher;
        private static DateTime _lastRead;
        private static Globals.UpgradeStatus _appStatus = Globals.UpgradeStatus.None;
        private static IEnumerable<string> _settingsRestrictExtensions = new string[] { };
        private static Queue<string> _filesQ;
        private static Timer _qTimer;

        // used to indicate already send first real-time email within last SlidingDelay period
        private static int _notificationSent;

        private const int CacheTimeOut = 5; //obtain the setting and do calculations once every 5 minutes at most, plus no need for locking
        private const int SlidingDelay = 30 * 1000; // milliseconds

        // Source: Configuring Blocked File Extensions
        // https://msdn.microsoft.com/en-us/library/cc767397.aspx
        private static readonly IEnumerable<string> DefaultRestrictExtensions =
            (
                ".ade,.adp,.app,.ashx,.asmx,.asp,.aspx,.bas,.bat,.chm,.class,.cmd,.com,.cpl,.crt,.dll,.exe," +
                ".fxp,.hlp,.hta,.ins,.isp,.jse,.lnk,.mda,.mdb,.mde,.mdt,.mdw,.mdz,.msc,.msi,.msp,.mst,.ops,.pcd,.php," +
                ".pif,.prf,.prg,.py,.reg,.scf,.scr,.sct,.shb,.shs,.url,.vb,.vbe,.vbs,.wsc,.wsf,.wsh"
            )
            .ToLowerInvariant()
            .Split(',')
            .Where(e => !string.IsNullOrEmpty(e))
            .Select(e => e.Trim())
            .ToList();

        internal static void Initialize()
        {
			Logger.LogDebug(">>> Entering Initialize()");
            if (_fileWatcher == null)
            {
                lock (typeof(DotNetNukeSecurity))
                {
                    if (_fileWatcher == null)
                    {
                        try
                        {
                            InitializeFileWatcher();
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting Initialize()");
        }


        private static void InitializeFileWatcher()
        {
			Logger.LogDebug(">>> Entering InitializeFileWatcher()");
            _fileWatcher = new FileSystemWatcher
            {
				Logger.LogDebug("InitializeFileWatcher(): _fileWatcher → {_fileWatcher}", _fileWatcher);
                Filter = "*.*",
                Path = Globals.ApplicationMapPath,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
            };

            _fileWatcher.Created += WatcherOnCreated;
            _fileWatcher.Renamed += WatcherOnRenamed;
            _fileWatcher.Error += WatcherOnError;

            _filesQ = new Queue<string>();
			Logger.LogDebug("InitializeFileWatcher(): _filesQ → {_filesQ}", _filesQ);
            _qTimer = new Timer(QTimerCallBack);

			Logger.LogDebug("InitializeFileWatcher(): _qTimer → {_qTimer}", _qTimer);
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                _fileWatcher.Dispose();
                QTimerCallBack(null);
            };

            _fileWatcher.EnableRaisingEvents = true;
			Logger.LogDebug("<<< Exiting InitializeFileWatcher()");
			Logger.LogDebug("InitializeFileWatcher(): _fileWatcher.EnableRaisingEvents → {Arg0}", _fileWatcher.EnableRaisingEvents);
        }

        private static void QTimerCallBack(object obj)
        {
			Logger.LogDebug(">>> Entering QTimerCallBack(obj={Obj})", obj);
            try
            {
                string[] items;
                lock (_filesQ)
                {
                    while (!_qTimer.Change(Timeout.Infinite, Timeout.Infinite)) { }
                    Interlocked.Exchange(ref _notificationSent, 0);
                    items = _filesQ.ToArray();
					Logger.LogDebug("QTimerCallBack(obj={Obj}): items → {Items}", obj, items);
                    while (_filesQ.Count > 0) _filesQ.Dequeue();
                }

                if (items.Length > 0)
                    NotifyManager(items);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in QTimerCallBack(obj={Obj}): {Arg1}", obj, ex.Message);
                LogException(ex);
            }
			Logger.LogDebug("<<< Exiting QTimerCallBack(obj={Obj})", obj);
        }

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
			Logger.LogDebug(">>> Entering WatcherOnRenamed(sender={Sender},e={E})", sender, e);
            CheckFile(e.FullPath);
			Logger.LogDebug("<<< Exiting WatcherOnRenamed(sender={Sender},e={E})", sender, e);
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
			Logger.LogDebug(">>> Entering WatcherOnCreated(sender={Sender},e={E})", sender, e);
            CheckFile(e.FullPath);
			Logger.LogDebug("<<< Exiting WatcherOnCreated(sender={Sender},e={E})", sender, e);
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e)
        {
			Logger.LogDebug(">>> Entering WatcherOnError(sender={Sender},e={E})", sender, e);
            LogException(e.GetException());
			Logger.LogDebug("<<< Exiting WatcherOnError(sender={Sender},e={E})", sender, e);
        }

        private static void LogException(Exception ex)
        {
			Logger.LogDebug(">>> Entering LogException(ex={Ex})", ex);
            Logger.Error("Watcher Activity Error: " + ex?.Message);
			Logger.LogDebug("<<< Exiting LogException(ex={Ex})", ex);
        }

        private static void CheckFile(string path)
        {
			Logger.LogDebug(">>> Entering CheckFile(path={Path})", path);
            try
            {
                if (IsRestrictdExtension(path))
                {
                    if (_appStatus != Globals.UpgradeStatus.Install && _appStatus != Globals.UpgradeStatus.Upgrade)
                    {
                        Globals.UpgradeStatus appStatus;

                        try
                        {
                            appStatus = Globals.Status;
							Logger.LogDebug("CheckFile(path={Path}): appStatus → {AppStatus}", path, appStatus);
                        }
                        catch (NullReferenceException)
                        {
							Logger.LogError("Exception in CheckFile(path={Path}): {Arg1}", path, ex.Message);
                            appStatus = Globals.UpgradeStatus.None;
                        }

                        // make status sticky; once set to install/upgrade, it stays so until finishing & appl restarts
                        if (appStatus == Globals.UpgradeStatus.Install || appStatus == Globals.UpgradeStatus.Upgrade)
                        {
                            _appStatus = appStatus;
							Logger.LogDebug("CheckFile(path={Path}): _appStatus → {_appStatus}", path, _appStatus);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(_ => AddEventLog(path));
                            var val = Interlocked.Increment(ref _notificationSent);
                            if (val <= 1)
							Logger.LogDebug("CheckFile(path={Path}): val → {Val}", path, val);
                            {
                                // first notification goes immediately
                                ThreadPool.QueueUserWorkItem(_ => NotifyManager(new[] { path }));
								Logger.LogDebug(">>> Entering AddEventLog(path={Path})", path);
								Logger.LogError("Exception in AddEventLog(path={Path}): {Arg1}", path, ex.Message);
								Logger.LogDebug("<<< Exiting AddEventLog(path={Path})", path);
                            }
                            else
                            {
                                lock (_filesQ)
                                {
                                    while (!_qTimer.Change(val >= 100 ? 1 : SlidingDelay, Timeout.Infinite)) { }
                                    _filesQ.Enqueue(path);
                                }
								Logger.LogDebug(">>> Entering NotifyManager(paths={Paths})", paths);
								Logger.LogError("Exception in NotifyManager(paths={Paths}): {Arg1}", paths, ex.Message);
								Logger.LogDebug("<<< Exiting NotifyManager(paths={Paths})", paths);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in CheckFile(path={Path}): {Arg1}", path, ex.Message);
                LogException(ex);
            }
			Logger.LogDebug("<<< Exiting CheckFile(path={Path})", path);
        }

        private static bool IsRestrictdExtension(string path)
        {
			Logger.LogDebug(">>> Entering IsRestrictdExtension(path={Path})", path);
            var extension = Path.GetExtension(path)?.ToLowerInvariant();
			Logger.LogDebug("IsRestrictdExtension(path={Path}): extension → {Extension}", path, extension);
			Logger.LogDebug("<<< Exiting IsRestrictdExtension(path={Path})", path);
            return !string.IsNullOrEmpty(extension) &&
                GetRestrictExtensions().Contains(extension);
        }

        private static IEnumerable<string> GetRestrictExtensions()
        {
			Logger.LogDebug(">>> Entering GetRestrictExtensions()");
            // obtain the setting and do calculations once every 5 minutes at most, plus no need for locking
            if ((DateTime.Now - _lastRead).TotalMinutes > CacheTimeOut)
            {
                _lastRead = DateTime.Now;
				Logger.LogDebug("GetRestrictExtensions(): _lastRead → {_lastRead}", _lastRead);
                var settings = HostController.Instance.GetString("SA_RestrictExtensions", string.Empty);
                _settingsRestrictExtensions = string.IsNullOrEmpty(settings)
                    ? DefaultRestrictExtensions
					Logger.LogDebug("GetRestrictExtensions(): settings → {Settings}", settings);
					Logger.LogDebug("GetRestrictExtensions(): _settingsRestrictExtensions → {_settingsRestrictExtensions}", _settingsRestrictExtensions);
                    : settings.ToLowerInvariant()
                        .Split(',')
                        .Where(e => !string.IsNullOrEmpty(e))
                        .Select(e => e.Trim())
                        .Concat(DefaultRestrictExtensions)
                        .ToList();
            }

			Logger.LogDebug("<<< Exiting GetRestrictExtensions()");
            return _settingsRestrictExtensions ?? DefaultRestrictExtensions;
        }

        private static void AddEventLog(string path)
        {
			Logger.LogDebug(">>> Entering AddEventLog(path={Path})", path);
            try
            {
                var log = new LogInfo
                {
                    LogTypeKey = nameof(EventLogType.HOST_ALERT),
                };
                log.AddProperty("Summary", Localization.GetString("PotentialDangerousFile.Text"));
                log.AddProperty("File Name", path);

                new LogController().AddLog(log);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in AddEventLog(path={Path}): {Arg1}", path, ex.Message);
                LogException(ex);
            }
			Logger.LogDebug("<<< Exiting AddEventLog(path={Path})", path);
        }

        private static void NotifyManager(string[] paths)
        {
			Logger.LogDebug(">>> Entering NotifyManager(paths={Paths})", paths);
            try
            {
                var pathNames = string.Join("<br/>", paths);
                var subject = Localization.GetString("RestrictFileMail_Subject.Text");
				Logger.LogDebug("NotifyManager(paths={Paths}): pathNames → {PathNames}", paths, pathNames);
				Logger.LogDebug("NotifyManager(paths={Paths}): subject → {Subject}", paths, subject);
                var body = Localization.GetString("RestrictFileMail_Body.Text")
                    .Replace("[Path]", pathNames)
                    .Replace("[AppName]", Host.HostTitle)
                    .Replace("[AppVersion]", DotNetNukeContext.Current.Application.CurrentVersion.ToString());

                Mail.SendEmail(Host.HostEmail, Host.HostEmail, subject, body);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in NotifyManager(paths={Paths}): {Arg1}", paths, ex.Message);
                LogException(ex);
            }
			Logger.LogDebug("<<< Exiting NotifyManager(paths={Paths})", paths);
        }

    }
}
#endif
