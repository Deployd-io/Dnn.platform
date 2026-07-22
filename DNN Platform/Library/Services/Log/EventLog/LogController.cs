// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="ILogController" />
    public class LogController : ServiceLocator<ILogController, LogController>, ILogController
    {
        private const int WriterLockTimeout = 10000; // milliseconds
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LogController));
        private static readonly ReaderWriterLockSlim LockLog = new ReaderWriterLockSlim();
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="LogController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public LogController()
            : this(null)
        {
		Logger.LogDebug(">>> Entering LogController()");
        }
		Logger.LogDebug("<<< Exiting LogController()");

        /// <summary>Initializes a new instance of the <see cref="LogController"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        public LogController(IApplicationStatusInfo appStatus)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("LogController(appStatus={AppStatus}): appStatus → {AppStatus1}", appStatus, appStatus);
        }

        /// <inheritdoc />
        public void AddLog(LogInfo logInfo)
        {
			Logger.LogDebug(">>> Entering AddLog(logInfo={LogInfo})", logInfo);
            ILogInfo theLogInfo = logInfo;
            if (this.appStatus.Status == UpgradeStatus.Install)
            {
                Logger.Info(logInfo);
            }
            else
            {
                try
                {
                    logInfo.LogCreateDate = DateTime.Now;
					Logger.LogDebug("AddLog(logInfo={LogInfo}): logInfo.LogCreateDate → {Arg1}", logInfo, logInfo.LogCreateDate);
					Logger.LogDebug("AddLog(logInfo={LogInfo}): logInfo.LogServerName → {Arg1}", logInfo, logInfo.LogServerName);
                    logInfo.LogServerName = Globals.ServerName;
                    if (string.IsNullOrEmpty(logInfo.LogServerName))
                    {
                        logInfo.LogServerName = "NA";
                    }

                    if (string.IsNullOrEmpty(logInfo.LogUserName))
                    {
                        if (HttpContext.Current != null)
                        {
                            try
                            {
                                if (HttpContext.Current.Request.IsAuthenticated)
                                {
                                    logInfo.LogUserName = UserController.Instance.GetCurrentUserInfo().Username;
                                }
                            }
                            catch (HttpException exception)
                            {
                                Logger.Error("Unable to retrieve HttpContext.Request, ignoring LogUserName", exception);
                            }
                        }
                    }

                    // Get portal name if name isn't set
                    if (theLogInfo.LogPortalId != Null.NullInteger && string.IsNullOrEmpty(logInfo.LogPortalName))
                    {
                        logInfo.LogPortalName = PortalController.Instance.GetPortal(theLogInfo.LogPortalId).PortalName;
                    }

                    // Check if Log Type exists
                    if (!this.GetLogTypeInfoDictionary().ContainsKey(logInfo.LogTypeKey))
                    {
                        // Add new Log Type
                        var logType = new LogTypeInfo
                        {
                            LogTypeKey = logInfo.LogTypeKey,
                            LogTypeFriendlyName = logInfo.LogTypeKey,
                            LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                            LogTypeDescription = string.Empty,
                        };
                        ((ILogTypeInfo)logType).LogTypeCssClass = "GeneralAdminOperation";

                        this.AddLogType(logType);

                        var logTypeConfigInfo = new LogTypeConfigInfo()
                        {
                            LogTypeKey = logInfo.LogTypeKey,
                            LoggingIsActive = false,
                            KeepMostRecent = "-1",
                            EmailNotificationIsActive = false,
                            NotificationThreshold = 1,
                            NotificationThresholdTime = 1,
                            NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                            MailFromAddress = string.Empty,
                            MailToAddress = string.Empty,
                        };
                        ((ILogTypeConfigInfo)logTypeConfigInfo).LogTypePortalId = "*";
                        this.AddLogTypeConfigInfo(logTypeConfigInfo);
                    }

                    if (LoggingProvider.Instance() != null)
                    {
                        try
                        {
                            LoggingProvider.Instance().AddLog(logInfo);
                        }
                        catch (Exception)
                        {
							Logger.LogError("Exception in AddLog(logInfo={LogInfo}): {Arg1}", logInfo, ex.Message);
                            if (this.appStatus.Status != UpgradeStatus.Upgrade)
                            {
                                // this may catch exceptions during upgrade because the old logging provider has problem in it.
                                throw;
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    AddLogToFile(logInfo);
                }
            }
			Logger.LogDebug("<<< Exiting AddLog(logInfo={LogInfo})", logInfo);
        }

        /// <inheritdoc />
        public void AddLogType(string configFile, string fallbackConfigFile)
        {
			Logger.LogDebug(">>> Entering AddLogType(configFile={ConfigFile},fallbackConfigFile={FallbackConfigFile})", configFile, fallbackConfigFile);
            var xmlDoc = new XmlDocument { XmlResolver = null };
            try
            {
                using var xmlReader = XmlReader.Create(configFile, new XmlReaderSettings { XmlResolver = null, });
                xmlDoc.Load(xmlReader);
				Logger.LogDebug("AddLogType(configFile={ConfigFile},fallbackConfigFile={FallbackConfigFile}): xmlReader → {XmlReader}", configFile, fallbackConfigFile, xmlReader);
            }
            catch (FileNotFoundException exc)
            {
                Logger.Debug(exc);
                using var xmlReader = XmlReader.Create(fallbackConfigFile, new XmlReaderSettings { XmlResolver = null, });
                xmlDoc.Load(xmlReader);
            }

            var logType = xmlDoc.SelectNodes("/LogConfig/LogTypes/LogType");
            if (logType != null)
			Logger.LogDebug("AddLogType(configFile={ConfigFile},fallbackConfigFile={FallbackConfigFile}): logType → {LogType}", configFile, fallbackConfigFile, logType);
            {
                foreach (XmlNode typeInfo in logType)
                {
                    if (typeInfo.Attributes != null)
                    {
                        var objLogTypeInfo = new LogTypeInfo
                        {
                            LogTypeKey = typeInfo.Attributes["LogTypeKey"].Value,
                            LogTypeFriendlyName = typeInfo.Attributes["LogTypeFriendlyName"].Value,
                            LogTypeDescription = typeInfo.Attributes["LogTypeDescription"].Value,
                            LogTypeOwner = typeInfo.Attributes["LogTypeOwner"].Value,
                        };
                        ((ILogTypeInfo)objLogTypeInfo).LogTypeCssClass = typeInfo.Attributes["LogTypeCSSClass"].Value;
                        this.AddLogType(objLogTypeInfo);
                    }
                }
            }

            var logTypeConfig = xmlDoc.SelectNodes("/LogConfig/LogTypeConfig");
            if (logTypeConfig != null)
			Logger.LogDebug("AddLogType(configFile={ConfigFile},fallbackConfigFile={FallbackConfigFile}): logTypeConfig → {LogTypeConfig}", configFile, fallbackConfigFile, logTypeConfig);
            {
                foreach (XmlNode typeConfigInfo in logTypeConfig)
                {
                    if (typeConfigInfo.Attributes != null)
                    {
                        var logTypeConfigInfo = new LogTypeConfigInfo
                        {
                            EmailNotificationIsActive = typeConfigInfo.Attributes["EmailNotificationStatus"].Value == "On",
                            KeepMostRecent = typeConfigInfo.Attributes["KeepMostRecent"].Value,
                            LoggingIsActive = typeConfigInfo.Attributes["LoggingStatus"].Value == "On",
                            LogTypeKey = typeConfigInfo.Attributes["LogTypeKey"].Value,
                            MailFromAddress = typeConfigInfo.Attributes["MailFromAddress"].Value,
                            MailToAddress = typeConfigInfo.Attributes["MailToAddress"].Value,
                            NotificationThreshold = Convert.ToInt32(typeConfigInfo.Attributes["NotificationThreshold"].Value, CultureInfo.InvariantCulture),
                            NotificationThresholdTime = Convert.ToInt32(typeConfigInfo.Attributes["NotificationThresholdTime"].Value, CultureInfo.InvariantCulture),
                            NotificationThresholdTimeType = (LogTypeConfigInfo.NotificationThresholdTimeTypes)Enum.Parse(typeof(LogTypeConfigInfo.NotificationThresholdTimeTypes), typeConfigInfo.Attributes["NotificationThresholdTimeType"].Value),
                        };
                        ((ILogTypeConfigInfo)logTypeConfigInfo).LogTypePortalId = typeConfigInfo.Attributes["LogTypePortalID"].Value;
                        this.AddLogTypeConfigInfo(logTypeConfigInfo);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting AddLogType(configFile={ConfigFile},fallbackConfigFile={FallbackConfigFile})", configFile, fallbackConfigFile);
        }

        /// <inheritdoc />
        public void AddLogType(LogTypeInfo logType)
        {
            ILogTypeInfo theLogType = logType;
            LoggingProvider.Instance()
                .AddLogType(
                    theLogType.LogTypeKey,
                    theLogType.LogTypeFriendlyName,
                    theLogType.LogTypeDescription,
                    theLogType.LogTypeCssClass,
                    theLogType.LogTypeOwner);
        }

        /// <inheritdoc />
        public void AddLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            ILogTypeConfigInfo theLogTypeConfig = logTypeConfig;
            LoggingProvider.Instance().AddLogTypeConfigInfo(
                theLogTypeConfig.Id,
                theLogTypeConfig.LoggingIsActive,
                theLogTypeConfig.LogTypeKey,
                theLogTypeConfig.LogTypePortalId,
                theLogTypeConfig.KeepMostRecent,
                theLogTypeConfig.LogFileName,
                theLogTypeConfig.EmailNotificationIsActive,
                Convert.ToString(logTypeConfig.NotificationThreshold, CultureInfo.InvariantCulture),
                Convert.ToString(logTypeConfig.NotificationThresholdTime, CultureInfo.InvariantCulture),
                Convert.ToString((int)logTypeConfig.NotificationThresholdTimeType, CultureInfo.InvariantCulture),
                theLogTypeConfig.MailFromAddress,
                theLogTypeConfig.MailToAddress);
        }

        /// <inheritdoc />
        public void ClearLog()
        {
			Logger.LogDebug(">>> Entering ClearLog()");
            LoggingProvider.Instance().ClearLog();
			Logger.LogDebug("<<< Exiting ClearLog()");
        }

        /// <inheritdoc />
        public void DeleteLog(LogInfo logInfo)
        {
			Logger.LogDebug(">>> Entering DeleteLog(logInfo={LogInfo})", logInfo);
            LoggingProvider.Instance().DeleteLog(logInfo);
			Logger.LogDebug("<<< Exiting DeleteLog(logInfo={LogInfo})", logInfo);
        }

        /// <inheritdoc />
        public virtual void DeleteLogType(LogTypeInfo logType)
        {
            LoggingProvider.Instance().DeleteLogType(logType.LogTypeKey);
        }

        /// <inheritdoc />
        public virtual void DeleteLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            ILogTypeConfigInfo theLogTypeConfig = logTypeConfig;
            LoggingProvider.Instance().DeleteLogTypeConfigInfo(theLogTypeConfig.Id);
        }

        /// <inheritdoc />
        public virtual List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            return LoggingProvider.Instance().GetLogs(portalID, logType, pageSize, pageIndex, ref totalRecords);
        }

        /// <inheritdoc />
        public virtual ArrayList GetLogTypeConfigInfo()
        {
			Logger.LogDebug(">>> Entering GetLogTypeConfigInfo()");
			Logger.LogDebug("<<< Exiting GetLogTypeConfigInfo()");
            return LoggingProvider.Instance().GetLogTypeConfigInfo();
        }

        /// <inheritdoc />
        public virtual LogTypeConfigInfo GetLogTypeConfigInfoByID(string id)
        {
			Logger.LogDebug(">>> Entering GetLogTypeConfigInfoByID(id={Id})", id);
			Logger.LogDebug("<<< Exiting GetLogTypeConfigInfoByID(id={Id})", id);
            return LoggingProvider.Instance().GetLogTypeConfigInfoByID(id);
        }

        /// <inheritdoc />
        public virtual Dictionary<string, LogTypeInfo> GetLogTypeInfoDictionary()
        {
            return LoggingProvider.Instance().GetLogTypeInfo().Cast<LogTypeInfo>().ToDictionary(logTypeInfo => logTypeInfo.LogTypeKey);
        }

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService.GetLog()' instead. Scheduled for removal in v11.0.0.")]
        public virtual object GetSingleLog(LogInfo log, LoggingProvider.ReturnType returnType)
        {
			Logger.LogDebug(">>> Entering GetSingleLog(log={Log},returnType={ReturnType})", log, returnType);
			Logger.LogDebug(">>> Entering GetLog(logGuid={LogGuid})", logGuid);
			Logger.LogDebug("<<< Exiting GetLog(logGuid={LogGuid})", logGuid);
            return LoggingProvider.Instance().GetSingleLog(log, returnType);
        }

        /// <inheritdoc />
        public virtual ILogInfo GetLog(string logGuid)
        {
            return LoggingProvider.Instance().GetLog(logGuid);
        }

        /// <inheritdoc />
        public void PurgeLogBuffer()
        {
			Logger.LogDebug(">>> Entering PurgeLogBuffer()");
            LoggingProvider.Instance().PurgeLogBuffer();
			Logger.LogDebug("<<< Exiting PurgeLogBuffer()");
        }

        /// <inheritdoc />
        public virtual void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            ILogTypeConfigInfo theLogTypeConfig = logTypeConfig;
            LoggingProvider.Instance().UpdateLogTypeConfigInfo(
                theLogTypeConfig.Id,
                theLogTypeConfig.LoggingIsActive,
                theLogTypeConfig.LogTypeKey,
                theLogTypeConfig.LogTypePortalId,
                theLogTypeConfig.KeepMostRecent,
                theLogTypeConfig.LogFileName,
                theLogTypeConfig.EmailNotificationIsActive,
                Convert.ToString(logTypeConfig.NotificationThreshold, CultureInfo.InvariantCulture),
                Convert.ToString(logTypeConfig.NotificationThresholdTime, CultureInfo.InvariantCulture),
                Convert.ToString((int)logTypeConfig.NotificationThresholdTimeType, CultureInfo.InvariantCulture),
                theLogTypeConfig.MailFromAddress,
                theLogTypeConfig.MailToAddress);
        }

        /// <inheritdoc />
        public virtual void UpdateLogType(LogTypeInfo logType)
        {
            ILogTypeInfo theLogType = logType;
            LoggingProvider.Instance()
                .UpdateLogType(
                    theLogType.LogTypeKey,
                    theLogType.LogTypeFriendlyName,
                    theLogType.LogTypeDescription,
                    theLogType.LogTypeCssClass,
                    theLogType.LogTypeOwner);
        }

        /// <inheritdoc />
        protected override Func<ILogController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ILogController>;
        }

        private static void AddLogToFile(LogInfo logInfo)
        {
			Logger.LogDebug(">>> Entering AddLogToFile(logInfo={LogInfo})", logInfo);
            try
            {
                var f = Globals.HostMapPath + @"\Logs\LogFailures.xml.resources";
                WriteLog(f, logInfo.Serialize());
				Logger.LogDebug("AddLogToFile(logInfo={LogInfo}): f → {F}", logInfo, f);
            }

            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
			Logger.LogDebug("<<< Exiting AddLogToFile(logInfo={LogInfo})", logInfo);
        }

        private static void RaiseError(string filePath, string header, string message)
        {
			Logger.LogDebug(">>> Entering RaiseError(filePath={FilePath},header={Header},message={Message})", filePath, header, message);
            Logger.ErrorFormat(CultureInfo.InvariantCulture, "filePath={0}, header={1}, message={2}", filePath, header, message);

            if (HttpContext.Current != null)
            {
                var response = HttpContext.Current.Response;
                HtmlUtils.WriteHeader(response, header);
                HtmlUtils.WriteError(response, filePath, message);
                HtmlUtils.WriteFooter(response);
                response.End();
            }
			Logger.LogDebug("<<< Exiting RaiseError(filePath={FilePath},header={Header},message={Message})", filePath, header, message);
        }

        private static void WriteToStreamWriter(FileStream fs, string message)
        {
            using var sw = new StreamWriter(fs, Encoding.UTF8);
            var fileLength = fs.Length;
            if (fileLength > 0)
            {
                fs.Position = fileLength - 9;
            }
            else
            {
                message = "<logs>" + message;
            }

            sw.WriteLine(message + "</logs>");
            sw.Flush();
        }

        private static void WriteLog(string filePath, string message)
        {
			Logger.LogDebug(">>> Entering WriteLog(filePath={FilePath},message={Message})", filePath, message);
            FileStream fs = null;
            if (!LockLog.TryEnterWriteLock(WriterLockTimeout))
			Logger.LogDebug("WriteLog(filePath={FilePath},message={Message}): fs → {Fs}", filePath, message, fs);
            {
                return;
            }

            try
            {
                var intAttempts = 0;
                while (fs == null && intAttempts < 100)
                {
                    intAttempts += 1;
                    try
                    {
                        fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    }
                    catch (IOException exc)
                    {
                        Logger.Debug(exc);
                        Thread.Sleep(1);
                    }
                }

                if (fs == null)
                {
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Response.Write("An error has occurred writing to the exception log.");
                        HttpContext.Current.Response.End();
                    }
                }
                else
                {
                    WriteToStreamWriter(fs, message);
                }
            }
            catch (UnauthorizedAccessException)
            {
				Logger.LogError("Exception in WriteLog(filePath={FilePath},message={Message}): {Arg2}", filePath, message, ex.Message);
                RaiseError(filePath, "Unauthorized Access Error", "The Windows User Account listed below must have Read/Write Privileges for the website path.");
            }
            catch (DirectoryNotFoundException exc)
            {
                RaiseError(filePath, "Directory Not Found Error", exc.Message);
            }
            catch (PathTooLongException exc)
            {
                RaiseError(filePath, "Path Too Long Error", exc.Message);
            }
            catch (IOException exc)
            {
                RaiseError(filePath, "IO Error", exc.Message);
            }
            catch (SqlException exc)
            {
                RaiseError(filePath, "SQL Exception", SqlUtils.TranslateSQLException(exc));
            }
            catch (Exception exc)
            {
                RaiseError(filePath, "Unhandled Error", exc.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }

                LockLog.ExitWriteLock();
            }
			Logger.LogDebug("<<< Exiting WriteLog(filePath={FilePath},message={Message})", filePath, message);
        }
    }
}
