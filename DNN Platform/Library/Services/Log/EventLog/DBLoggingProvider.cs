// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    using Microsoft.Extensions.DependencyInjection;

    public class DBLoggingProvider : LoggingProvider
    {
        public const string LogTypeCacheKey = "LogTypes";
        public const string LogTypeInfoCacheKey = "GetLogTypeConfigInfo";
        public const string LogTypeInfoByKeyCacheKey = "GetLogTypeConfigInfoByKey";

        private const int ReaderLockTimeout = 10000;
        private const int WriterLockTimeout = 10000;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DBLoggingProvider));
        private static readonly List<LogQueueItem> LogQueue = [];
        private static readonly ReaderWriterLockSlim LockNotif = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim LockQueueLog = new ReaderWriterLockSlim();

        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DBLoggingProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DBLoggingProvider()
            : this(null)
        {
		Logger.LogDebug(">>> Entering DBLoggingProvider()");
        }
		Logger.LogDebug("<<< Exiting DBLoggingProvider()");

        /// <summary>Initializes a new instance of the <see cref="DBLoggingProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DBLoggingProvider(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("DBLoggingProvider(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <inheritdoc />
        public override void AddLog(LogInfo logInfo)
        {
			Logger.LogDebug(">>> Entering AddLog(logInfo={LogInfo})", logInfo);
            ILogInfo theLogInfo = logInfo;
            string configPortalId = theLogInfo.LogPortalId != Null.NullInteger
                                        ? theLogInfo.LogPortalId.ToString(CultureInfo.InvariantCulture)
                                        : "*";
            var logTypeConfigInfo = this.GetLogTypeConfigInfoByKey(logInfo.LogTypeKey, configPortalId);
			Logger.LogDebug("AddLog(logInfo={LogInfo}): logTypeConfigInfo → {LogTypeConfigInfo}", logInfo, logTypeConfigInfo);
            if (logTypeConfigInfo is not { LoggingIsActive: true, })
            {
                return;
            }

            theLogInfo.LogConfigId = ((ILogTypeConfigInfo)logTypeConfigInfo).Id;
            var logQueueItem = new LogQueueItem { LogInfo = logInfo, LogTypeConfigInfo = logTypeConfigInfo };
            var scheduler = SchedulingProvider.Instance();
			Logger.LogDebug("AddLog(logInfo={LogInfo}): scheduler → {Scheduler}", logInfo, scheduler);
            if (scheduler == null || logInfo.BypassBuffering || !SchedulingProvider.Enabled
                || scheduler.GetScheduleStatus() == ScheduleStatus.STOPPED || !this.hostSettings.EventLogBuffer)
            {
                WriteLog(logQueueItem);
            }
            else
            {
                LogQueue.Add(logQueueItem);
            }
			Logger.LogDebug("<<< Exiting AddLog(logInfo={LogInfo})", logInfo);
        }

        // ReSharper disable once InconsistentNaming

        /// <inheritdoc />
        public override void AddLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
			Logger.LogDebug(">>> Entering AddLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            DataProvider.Instance().AddLogType(logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            DataCache.RemoveCache(LogTypeCacheKey);
			Logger.LogDebug("<<< Exiting AddLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void AddLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string thresholdTime, string thresholdTimeType, string mailFromAddress, string mailToAddress)
        {
			Logger.LogDebug(">>> Entering AddLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress);
            int intThreshold = -1;
            int intThresholdTime = -1;
			Logger.LogDebug("AddLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intThresholdTime → {IntThresholdTime}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intThresholdTime);
            int intThresholdTimeType = -1;
			Logger.LogDebug("AddLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intThresholdTimeType → {IntThresholdTimeType}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intThresholdTimeType);
            int intKeepMostRecent = -1;
			Logger.LogDebug("AddLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intKeepMostRecent → {IntKeepMostRecent}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intKeepMostRecent);
			Logger.LogDebug("AddLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intThreshold → {IntThreshold}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intThreshold);
            if (Globals.NumberMatchRegex.IsMatch(threshold))
            {
                intThreshold = Convert.ToInt32(threshold, CultureInfo.InvariantCulture);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTime))
            {
                intThresholdTime = Convert.ToInt32(thresholdTime, CultureInfo.InvariantCulture);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTimeType))
            {
                intThresholdTimeType = Convert.ToInt32(thresholdTimeType, CultureInfo.InvariantCulture);
            }

            if (Globals.NumberMatchRegex.IsMatch(keepMostRecent))
            {
                intKeepMostRecent = Convert.ToInt32(keepMostRecent, CultureInfo.InvariantCulture);
            }

            DataProvider.Instance().AddLogTypeConfigInfo(
                loggingIsActive,
                logTypeKey,
                logTypePortalID,
                intKeepMostRecent,
                emailNotificationIsActive,
                intThreshold,
                intThresholdTime,
                intThresholdTimeType,
                mailFromAddress,
                mailToAddress);
            DataCache.RemoveCache(LogTypeInfoCacheKey);
            DataCache.RemoveCache(LogTypeInfoByKeyCacheKey);
			Logger.LogDebug("<<< Exiting AddLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress);
        }

        /// <inheritdoc />
        public override void ClearLog()
        {
			Logger.LogDebug(">>> Entering ClearLog()");
            DataProvider.Instance().ClearLog();
			Logger.LogDebug("<<< Exiting ClearLog()");
        }

        /// <inheritdoc />
        public override void DeleteLog(LogInfo logInfo)
        {
			Logger.LogDebug(">>> Entering DeleteLog(logInfo={LogInfo})", logInfo);
            ILogInfo theLogInfo = logInfo;
            DataProvider.Instance().DeleteLog(theLogInfo.LogGuid);
			Logger.LogDebug("<<< Exiting DeleteLog(logInfo={LogInfo})", logInfo);
        }

        /// <inheritdoc />
        public override void DeleteLogType(string logTypeKey)
        {
			Logger.LogDebug(">>> Entering DeleteLogType(logTypeKey={LogTypeKey})", logTypeKey);
            DataProvider.Instance().DeleteLogType(logTypeKey);
            DataCache.RemoveCache(LogTypeCacheKey);
			Logger.LogDebug("<<< Exiting DeleteLogType(logTypeKey={LogTypeKey})", logTypeKey);
        }

        /// <inheritdoc />
        public override void DeleteLogTypeConfigInfo(string id)
        {
			Logger.LogDebug(">>> Entering DeleteLogTypeConfigInfo(id={Id})", id);
            DataProvider.Instance().DeleteLogTypeConfigInfo(id);
            DataCache.RemoveCache(LogTypeInfoCacheKey);
            DataCache.RemoveCache(LogTypeInfoByKeyCacheKey);
			Logger.LogDebug("<<< Exiting DeleteLogTypeConfigInfo(id={Id})", id);
        }

        /// <inheritdoc />
        public override List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering GetLogs(portalID={PortalID},logType={LogType},pageSize={PageSize},pageIndex={PageIndex},totalRecords={TotalRecords})", portalID, logType, pageSize, pageIndex, totalRecords);
            var logs = new List<LogInfo>();
            FillLogs(DataProvider.Instance().GetLogs(portalID, logType, pageSize, pageIndex), logs, ref totalRecords);
			Logger.LogDebug("<<< Exiting GetLogs(portalID={PortalID},logType={LogType},pageSize={PageSize},pageIndex={PageIndex},totalRecords={TotalRecords})", portalID, logType, pageSize, pageIndex, totalRecords);
            return logs;
        }

        /// <inheritdoc />
        public override ArrayList GetLogTypeConfigInfo()
        {
			Logger.LogDebug(">>> Entering GetLogTypeConfigInfo()");
            var list = (ArrayList)DataCache.GetCache(LogTypeInfoCacheKey);
            if (list == null)
            {
                IDataReader dr = null;
				Logger.LogDebug("GetLogTypeConfigInfo(): dr → {Dr}", dr);
                try
                {
                    dr = DataProvider.Instance().GetLogTypeConfigInfo();
                    list = CBO.FillCollection(dr, typeof(LogTypeConfigInfo));
                    DataCache.SetCache(LogTypeInfoCacheKey, list);
                    FillLogTypeConfigInfoByKey(list);
                }
                finally
                {
                    if (dr == null)
                    {
                        list = new ArrayList();
                    }
                    else
                    {
                        CBO.CloseDataReader(dr, true);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetLogTypeConfigInfo()");
            return list;
        }

        /// <inheritdoc />
        public override LogTypeConfigInfo GetLogTypeConfigInfoByID(string id)
        {
			Logger.LogDebug(">>> Entering GetLogTypeConfigInfoByID(id={Id})", id);
			Logger.LogDebug("<<< Exiting GetLogTypeConfigInfoByID(id={Id})", id);
            return CBO.FillObject<LogTypeConfigInfo>(DataProvider.Instance().GetLogTypeConfigInfoByID(Convert.ToInt32(id, CultureInfo.InvariantCulture)));
        }

        /// <inheritdoc />
        public override ArrayList GetLogTypeInfo()
        {
			Logger.LogDebug(">>> Entering GetLogTypeInfo()");
			Logger.LogDebug("<<< Exiting GetLogTypeInfo()");
            return CBO.GetCachedObject<ArrayList>(
                this.hostSettings,
                new CacheItemArgs(LogTypeCacheKey, 20, CacheItemPriority.Normal),
                _ => CBO.FillCollection(DataProvider.Instance().GetLogTypeInfo(), typeof(LogTypeInfo)));
        }

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService.GetLog()' instead. Scheduled for removal in v11.0.0.")]
        public override object GetSingleLog(LogInfo logInfo, ReturnType returnType)
        {
			Logger.LogDebug(">>> Entering GetSingleLog(logInfo={LogInfo},returnType={ReturnType})", logInfo, returnType);
            var log = (LogInfo)this.GetLog(logInfo.LogGUID);

			Logger.LogDebug("GetSingleLog(logInfo={LogInfo},returnType={ReturnType}): log → {Log}", logInfo, returnType, log);
            if (returnType == ReturnType.LogInfoObjects)
            {
                return log;
            }

            var xmlDoc = new XmlDocument { XmlResolver = null };
            if (log != null)
            {
                using var logReader = XmlReader.Create(new StringReader(log.Serialize()), new XmlReaderSettings { XmlResolver = null, });
				Logger.LogDebug("GetSingleLog(logInfo={LogInfo},returnType={ReturnType}): logReader → {LogReader}", logInfo, returnType, logReader);
                xmlDoc.Load(logReader);
            }

			Logger.LogDebug(">>> Entering GetLog(logGuid={LogGuid})", logGuid);
			Logger.LogDebug("<<< Exiting GetLog(logGuid={LogGuid})", logGuid);
			Logger.LogDebug("<<< Exiting GetSingleLog(logInfo={LogInfo},returnType={ReturnType})", logInfo, returnType);
            return xmlDoc.DocumentElement;
        }

        /// <inheritdoc />
        public override ILogInfo GetLog(string logGuid)
        {
            IDataReader dr = DataProvider.Instance().GetSingleLog(logGuid);
            LogInfo log = null;
            try
            {
                if (dr != null)
                {
                    dr.Read();
                    log = FillLogInfo(dr);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return log;
        }

        /// <inheritdoc />
        public override bool LoggingIsEnabled(string logType, int portalID)
        {
			Logger.LogDebug(">>> Entering LoggingIsEnabled(logType={LogType},portalID={PortalID})", logType, portalID);
            string configPortalId = portalID.ToString(CultureInfo.InvariantCulture);
            if (portalID == -1)
            {
                configPortalId = "*";
            }

            LogTypeConfigInfo configInfo = this.GetLogTypeConfigInfoByKey(logType, configPortalId);
			Logger.LogDebug("LoggingIsEnabled(logType={LogType},portalID={PortalID}): configInfo → {ConfigInfo}", logType, portalID, configInfo);
            if (configInfo == null)
            {
			Logger.LogDebug("<<< Exiting LoggingIsEnabled(logType={LogType},portalID={PortalID})", logType, portalID);
                return false;
            }

            return configInfo.LoggingIsActive;
        }

        /// <inheritdoc />
        public override void PurgeLogBuffer()
        {
			Logger.LogDebug(">>> Entering PurgeLogBuffer()");
            if (!LockQueueLog.TryEnterWriteLock(WriterLockTimeout))
            {
                return;
            }

            try
            {
                for (int i = LogQueue.Count - 1; i >= 0; i += -1)
                {
                    LogQueueItem logQueueItem = LogQueue[i];

                    // in case the log was removed
                    // by another thread simultaneously
                    if (logQueueItem != null)
                    {
                        WriteLog(logQueueItem);
                        LogQueue.Remove(logQueueItem);
                    }
                }
            }
            finally
            {
                LockQueueLog.ExitWriteLock();
            }

            DataProvider.Instance().PurgeLog();
			Logger.LogDebug("<<< Exiting PurgeLogBuffer()");
        }

        /// <inheritdoc />
        public override void SendLogNotifications()
        {
			Logger.LogDebug(">>> Entering SendLogNotifications()");
            var configInfos = CBO.FillCollection<LogTypeConfigInfo>(DataProvider.Instance().GetEventLogPendingNotifConfig());
            foreach (ILogTypeConfigInfo typeConfigInfo in configInfos)
            {
			Logger.LogDebug("SendLogNotifications(): configInfos → {ConfigInfos}", configInfos);
                IDataReader dr = DataProvider.Instance().GetEventLogPendingNotif(Convert.ToInt32(typeConfigInfo.Id, CultureInfo.InvariantCulture));
				Logger.LogDebug("SendLogNotifications(): dr → {Dr}", dr);
                string log = string.Empty;
                try
                {
                    while (dr.Read())
                    {
                        LogInfo logInfo = FillLogInfo(dr);
						Logger.LogDebug("SendLogNotifications(): logInfo → {LogInfo}", logInfo);
                        log += logInfo.Serialize() + Environment.NewLine + Environment.NewLine;
                    }
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }

                Mail.Mail.SendEmail(typeConfigInfo.MailFromAddress, typeConfigInfo.MailToAddress, "Event Notification", $"<pre>{HttpUtility.HtmlEncode(log)}</pre>");
                DataProvider.Instance().UpdateEventLogPendingNotif(Convert.ToInt32(typeConfigInfo.Id, CultureInfo.InvariantCulture));
            }
			Logger.LogDebug("<<< Exiting SendLogNotifications()");
        }

        /// <inheritdoc />
        public override bool SupportsEmailNotification()
        {
			Logger.LogDebug(">>> Entering SupportsEmailNotification()");
			Logger.LogDebug("<<< Exiting SupportsEmailNotification()");
            return true;
        }

        /// <inheritdoc />
        public override bool SupportsInternalViewer()
        {
			Logger.LogDebug(">>> Entering SupportsInternalViewer()");
			Logger.LogDebug("<<< Exiting SupportsInternalViewer()");
            return true;
        }

        /// <inheritdoc />
        public override bool SupportsSendToCoreTeam()
        {
			Logger.LogDebug(">>> Entering SupportsSendToCoreTeam()");
			Logger.LogDebug("<<< Exiting SupportsSendToCoreTeam()");
            return false;
        }

        /// <inheritdoc />
        public override bool SupportsSendViaEmail()
        {
			Logger.LogDebug(">>> Entering SupportsSendViaEmail()");
			Logger.LogDebug("<<< Exiting SupportsSendViaEmail()");
            return true;
        }

        // ReSharper disable once InconsistentNaming

        /// <inheritdoc />
        public override void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
			Logger.LogDebug(">>> Entering UpdateLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            DataProvider.Instance().UpdateLogType(logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            DataCache.RemoveCache(LogTypeCacheKey);
			Logger.LogDebug("<<< Exiting UpdateLogType(logTypeKey={LogTypeKey},logTypeFriendlyName={LogTypeFriendlyName},logTypeDescription={LogTypeDescription},logTypeCSSClass={LogTypeCSSClass},logTypeOwner={LogTypeOwner})", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string thresholdTime, string thresholdTimeType, string mailFromAddress, string mailToAddress)
        {
			Logger.LogDebug(">>> Entering UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress);
            var intThreshold = -1;
            var intThresholdTime = -1;
			Logger.LogDebug("UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intThresholdTime → {IntThresholdTime}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intThresholdTime);
            var intThresholdTimeType = -1;
			Logger.LogDebug("UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intThresholdTimeType → {IntThresholdTimeType}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intThresholdTimeType);
            var intKeepMostRecent = -1;
			Logger.LogDebug("UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intKeepMostRecent → {IntKeepMostRecent}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intKeepMostRecent);
			Logger.LogDebug("UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress}): intThreshold → {IntThreshold}", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress, intThreshold);
            if (Globals.NumberMatchRegex.IsMatch(threshold))
            {
                intThreshold = Convert.ToInt32(threshold, CultureInfo.InvariantCulture);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTime))
            {
                intThresholdTime = Convert.ToInt32(thresholdTime, CultureInfo.InvariantCulture);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTimeType))
            {
                intThresholdTimeType = Convert.ToInt32(thresholdTimeType, CultureInfo.InvariantCulture);
            }

            if (Globals.NumberMatchRegex.IsMatch(keepMostRecent))
            {
                intKeepMostRecent = Convert.ToInt32(keepMostRecent, CultureInfo.InvariantCulture);
            }

            DataProvider.Instance().UpdateLogTypeConfigInfo(
                id,
                loggingIsActive,
                logTypeKey,
                logTypePortalID,
                intKeepMostRecent,
                emailNotificationIsActive,
                intThreshold,
                intThresholdTime,
                intThresholdTimeType,
                mailFromAddress,
                mailToAddress);
            DataCache.RemoveCache(LogTypeInfoCacheKey);
            DataCache.RemoveCache(LogTypeInfoByKeyCacheKey);
			Logger.LogDebug("<<< Exiting UpdateLogTypeConfigInfo(id={Id},loggingIsActive={LoggingIsActive},logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID},keepMostRecent={KeepMostRecent},logFileName={LogFileName},emailNotificationIsActive={EmailNotificationIsActive},threshold={Threshold},thresholdTime={ThresholdTime},thresholdTimeType={ThresholdTimeType},mailFromAddress={MailFromAddress},mailToAddress={MailToAddress})", id, loggingIsActive, logTypeKey, logTypePortalID, keepMostRecent, logFileName, emailNotificationIsActive, threshold, thresholdTime, thresholdTimeType, mailFromAddress, mailToAddress);
        }

        private static Hashtable FillLogTypeConfigInfoByKey(ArrayList arr)
        {
			Logger.LogDebug(">>> Entering FillLogTypeConfigInfoByKey(arr={Arr})", arr);
            var ht = new Hashtable();
            int i;
			Logger.LogDebug("FillLogTypeConfigInfoByKey(arr={Arr}): ht → {Ht}", arr, ht);
            for (i = 0; i <= arr.Count - 1; i++)
            {
                var logTypeConfigInfo = (ILogTypeConfigInfo)arr[i];
				Logger.LogDebug("FillLogTypeConfigInfoByKey(arr={Arr}): logTypeConfigInfo → {LogTypeConfigInfo}", arr, logTypeConfigInfo);
                if (string.IsNullOrEmpty(logTypeConfigInfo.LogTypeKey))
                {
                    logTypeConfigInfo.LogTypeKey = "*";
					Logger.LogDebug("FillLogTypeConfigInfoByKey(arr={Arr}): logTypeConfigInfo.LogTypeKey → {Arg1}", arr, logTypeConfigInfo.LogTypeKey);
                }

                if (string.IsNullOrEmpty(logTypeConfigInfo.LogTypePortalId))
                {
                    logTypeConfigInfo.LogTypePortalId = "*";
					Logger.LogDebug("FillLogTypeConfigInfoByKey(arr={Arr}): logTypeConfigInfo.LogTypePortalId → {Arg1}", arr, logTypeConfigInfo.LogTypePortalId);
                }

                ht.Add(logTypeConfigInfo.LogTypeKey + "|" + logTypeConfigInfo.LogTypePortalId, logTypeConfigInfo);
            }

            DataCache.SetCache(LogTypeInfoByKeyCacheKey, ht);
			Logger.LogDebug("<<< Exiting FillLogTypeConfigInfoByKey(arr={Arr})", arr);
            return ht;
        }

        private static LogInfo FillLogInfo(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillLogInfo(dr={Dr})", dr);
            ILogInfo obj = new LogInfo();
            try
			Logger.LogDebug("FillLogInfo(dr={Dr}): obj → {Obj}", dr, obj);
            {
                obj.LogCreateDate = Convert.ToDateTime(dr["LogCreateDate"], CultureInfo.InvariantCulture);
                obj.LogGuid = Convert.ToString(dr["LogGUID"], CultureInfo.InvariantCulture);
                obj.LogPortalId = Convert.ToInt32(Null.SetNull(dr["LogPortalID"], obj.LogPortalId), CultureInfo.InvariantCulture);
                obj.LogPortalName = Convert.ToString(Null.SetNull(dr["LogPortalName"], obj.LogPortalName), CultureInfo.InvariantCulture);
                obj.LogServerName = Convert.ToString(Null.SetNull(dr["LogServerName"], obj.LogServerName), CultureInfo.InvariantCulture);
                obj.LogUserId = Convert.ToInt32(Null.SetNull(dr["LogUserID"], obj.LogUserId), CultureInfo.InvariantCulture);
                obj.LogEventId = Convert.ToInt32(Null.SetNull(dr["LogEventID"], obj.LogEventId), CultureInfo.InvariantCulture);
                obj.LogTypeKey = Convert.ToString(dr["LogTypeKey"], CultureInfo.InvariantCulture);
                obj.LogUserName = Convert.ToString(dr["LogUserName"], CultureInfo.InvariantCulture);
                obj.LogConfigId = Convert.ToString(dr["LogConfigID"], CultureInfo.InvariantCulture);
                ((LogInfo)obj).LogProperties.Deserialize(Convert.ToString(dr["LogProperties"], CultureInfo.InvariantCulture));
                obj.Exception.AssemblyVersion = Convert.ToString(Null.SetNull(dr["AssemblyVersion"], obj.Exception.AssemblyVersion), CultureInfo.InvariantCulture);
                obj.Exception.PortalId = Convert.ToInt32(Null.SetNull(dr["PortalId"], obj.Exception.PortalId), CultureInfo.InvariantCulture);
                obj.Exception.UserId = Convert.ToInt32(Null.SetNull(dr["UserId"], obj.Exception.UserId), CultureInfo.InvariantCulture);
                obj.Exception.TabId = Convert.ToInt32(Null.SetNull(dr["TabId"], obj.Exception.TabId), CultureInfo.InvariantCulture);
                obj.Exception.RawUrl = Convert.ToString(Null.SetNull(dr["RawUrl"], obj.Exception.RawUrl), CultureInfo.InvariantCulture);
                obj.Exception.Referrer = Convert.ToString(Null.SetNull(dr["Referrer"], obj.Exception.Referrer), CultureInfo.InvariantCulture);
                obj.Exception.UserAgent = Convert.ToString(Null.SetNull(dr["UserAgent"], obj.Exception.UserAgent), CultureInfo.InvariantCulture);
                obj.Exception.ExceptionHash = Convert.ToString(Null.SetNull(dr["ExceptionHash"], obj.Exception.ExceptionHash), CultureInfo.InvariantCulture);
                obj.Exception.Message = Convert.ToString(Null.SetNull(dr["Message"], obj.Exception.Message), CultureInfo.InvariantCulture);
                obj.Exception.StackTrace = Convert.ToString(Null.SetNull(dr["StackTrace"], obj.Exception.StackTrace), CultureInfo.InvariantCulture);
                obj.Exception.InnerMessage = Convert.ToString(Null.SetNull(dr["InnerMessage"], obj.Exception.InnerMessage), CultureInfo.InvariantCulture);
                obj.Exception.InnerStackTrace = Convert.ToString(Null.SetNull(dr["InnerStackTrace"], obj.Exception.InnerStackTrace), CultureInfo.InvariantCulture);
                obj.Exception.Source = Convert.ToString(Null.SetNull(dr["Source"], obj.Exception.Source), CultureInfo.InvariantCulture);
                /* DNN-6218 + DNN-6242: DB logging provider throws errors
                // the view "vw_EventLog" doesn't have these fields or any table in the database
                obj.Exception.FileName = Convert.ToString(Null.SetNull(dr["FileName"], obj.Exception.FileName));
                obj.Exception.FileLineNumber = Convert.ToInt32(Null.SetNull(dr["FileLineNumber"], obj.Exception.FileLineNumber));
                obj.Exception.FileColumnNumber = Convert.ToInt32(Null.SetNull(dr["FileColumnNumber"], obj.Exception.FileColumnNumber));
                obj.Exception.Method = Convert.ToString(Null.SetNull(dr["Method"], obj.Exception.Method));
                 */
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogDebug("<<< Exiting FillLogInfo(dr={Dr})", dr);
            return (LogInfo)obj;
        }

        private static void FillLogs(IDataReader dr, IList logs, ref int totalRecords)
        {
			Logger.LogDebug(">>> Entering FillLogs(dr={Dr},logs={Logs},totalRecords={TotalRecords})", dr, logs, totalRecords);
            try
            {
                while (dr.Read())
                {
                    LogInfo logInfo = FillLogInfo(dr);
					Logger.LogDebug("FillLogs(dr={Dr},logs={Logs},totalRecords={TotalRecords}): logInfo → {LogInfo}", dr, logs, totalRecords, logInfo);
                    logs.Add(logInfo);
                }

                dr.NextResult();
                while (dr.Read())
                {
                    totalRecords = Convert.ToInt32(dr["TotalRecords"], CultureInfo.InvariantCulture);
					Logger.LogDebug("FillLogs(dr={Dr},logs={Logs},totalRecords={TotalRecords}): totalRecords → {TotalRecords1}", dr, logs, totalRecords, totalRecords);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
			Logger.LogDebug("<<< Exiting FillLogs(dr={Dr},logs={Logs},totalRecords={TotalRecords})", dr, logs, totalRecords);
        }

        private static void WriteError(LogTypeConfigInfo logTypeConfigInfo, Exception exc, string header, string message)
        {
			Logger.LogDebug(">>> Entering WriteError(logTypeConfigInfo={LogTypeConfigInfo},exc={Exc},header={Header},message={Message})", logTypeConfigInfo, exc, header, message);
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.IsCustomErrorEnabled)
                {
                    HttpContext.Current.AddError(exc);
                }
                else
                {
                    HttpResponse response = HttpContext.Current.Response;
					Logger.LogDebug("WriteError(logTypeConfigInfo={LogTypeConfigInfo},exc={Exc},header={Header},message={Message}): response → {Response}", logTypeConfigInfo, exc, header, message, response);
					Logger.LogDebug("WriteError(logTypeConfigInfo={LogTypeConfigInfo},exc={Exc},header={Header},message={Message}): response.StatusCode → {Arg4}", logTypeConfigInfo, exc, header, message, response.StatusCode);
                    response.StatusCode = 500;
                    HtmlUtils.WriteHeader(response, header);

                    if (logTypeConfigInfo != null)
                    {
                        HtmlUtils.WriteError(response, logTypeConfigInfo.LogFileNameWithPath, message);
                    }

                    HtmlUtils.WriteFooter(response);
                    response.End();
                }
            }
			Logger.LogDebug("<<< Exiting WriteError(logTypeConfigInfo={LogTypeConfigInfo},exc={Exc},header={Header},message={Message})", logTypeConfigInfo, exc, header, message);
        }

        private static void WriteLog(LogQueueItem logQueueItem)
        {
			Logger.LogDebug(">>> Entering WriteLog(logQueueItem={LogQueueItem})", logQueueItem);
            LogTypeConfigInfo logTypeConfigInfo = null;
            try
			Logger.LogDebug("WriteLog(logQueueItem={LogQueueItem}): logTypeConfigInfo → {LogTypeConfigInfo}", logQueueItem, logTypeConfigInfo);
            {
                logTypeConfigInfo = logQueueItem.LogTypeConfigInfo;
                if (logTypeConfigInfo != null)
                {
                    LogInfo objLogInfo = logQueueItem.LogInfo;
                    string logProperties = objLogInfo.LogProperties.Serialize();
                    ILogInfo theLogInfo = objLogInfo;
                    DataProvider.Instance().AddLog(
                        theLogInfo.LogGuid,
                        theLogInfo.LogTypeKey,
                        theLogInfo.LogUserId,
                        theLogInfo.LogUserName,
                        theLogInfo.LogPortalId,
                        theLogInfo.LogPortalName,
                        theLogInfo.LogCreateDate,
                        theLogInfo.LogServerName,
                        logProperties,
                        Convert.ToInt32(theLogInfo.LogConfigId, CultureInfo.InvariantCulture),
                        objLogInfo.Exception,
                        logTypeConfigInfo.EmailNotificationIsActive);
                    if (logTypeConfigInfo.EmailNotificationIsActive)
                    {
                        if (LockNotif.TryEnterWriteLock(ReaderLockTimeout))
                        {
                            try
                            {
                                if (logTypeConfigInfo.NotificationThreshold == 0)
                                {
                                    string str = logQueueItem.LogInfo.Serialize();
                                    Mail.Mail.SendEmail(logTypeConfigInfo.MailFromAddress, logTypeConfigInfo.MailToAddress, "Event Notification", $"<pre>{HttpUtility.HtmlEncode(str)}</pre>");
                                }
                            }
                            finally
                            {
                                LockNotif.ExitWriteLock();
                            }
                        }
                    }
                }
            }
            catch (SqlException exc)
            {
                Logger.Error(exc);
                WriteError(logTypeConfigInfo, exc, "SQL Exception", SqlUtils.TranslateSQLException(exc));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                WriteError(logTypeConfigInfo, exc, "Unhandled Error", exc.Message);
            }
			Logger.LogDebug("<<< Exiting WriteLog(logQueueItem={LogQueueItem})", logQueueItem);
        }

        private LogTypeConfigInfo GetLogTypeConfigInfoByKey(string logTypeKey, string logTypePortalID)
        {
			Logger.LogDebug(">>> Entering GetLogTypeConfigInfoByKey(logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID})", logTypeKey, logTypePortalID);
            var configInfoByKey = (Hashtable)DataCache.GetCache(LogTypeInfoByKeyCacheKey) ?? FillLogTypeConfigInfoByKey(this.GetLogTypeConfigInfo());
            var logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey[logTypeKey + "|" + logTypePortalID];
			Logger.LogDebug("GetLogTypeConfigInfoByKey(logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID}): logTypeConfigInfo → {LogTypeConfigInfo}", logTypeKey, logTypePortalID, logTypeConfigInfo);
			Logger.LogDebug("GetLogTypeConfigInfoByKey(logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID}): configInfoByKey → {ConfigInfoByKey}", logTypeKey, logTypePortalID, configInfoByKey);
            if (logTypeConfigInfo == null)
            {
                logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey["*|" + logTypePortalID];
                if (logTypeConfigInfo == null)
                {
                    logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey[logTypeKey + "|*"];
                    if (logTypeConfigInfo == null)
                    {
                        logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey["*|*"];
                    }
                    else
                    {
                        return logTypeConfigInfo;
                    }
                }
                else
                {
                    return logTypeConfigInfo;
                }
            }
            else
            {
			Logger.LogDebug("<<< Exiting GetLogTypeConfigInfoByKey(logTypeKey={LogTypeKey},logTypePortalID={LogTypePortalID})", logTypeKey, logTypePortalID);
                return logTypeConfigInfo;
            }

            return logTypeConfigInfo;
        }
    }
}
