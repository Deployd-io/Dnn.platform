// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.AdminLogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;

    using Microsoft.Extensions.DependencyInjection;

    public class AdminLogsController
    {
        private readonly IEventLogger eventLogger;
        private Dictionary<string, LogTypeInfo> logTypeDictionary;

        /// <summary>Initializes a new instance of the <see cref="AdminLogsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public AdminLogsController()
            : this(null)
        {
		eventLogger.LogDebug(">>> Entering AdminLogsController()");
		eventLogger.LogDebug("<<< Exiting AdminLogsController()");
		eventLogger.LogDebug(">>> Entering AdminLogsController(eventLogger={EventLogger})", eventLogger);
		eventLogger.LogDebug("AdminLogsController(eventLogger={EventLogger}): eventLogger → {EventLogger1}", eventLogger, eventLogger);
        }

        /// <summary>Initializes a new instance of the <see cref="AdminLogsController"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        public AdminLogsController(IEventLogger eventLogger)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("<<< Exiting AdminLogsController(eventLogger={EventLogger})", eventLogger);
        }

        protected Dictionary<string, LogTypeInfo> LogTypeDictionary => this.logTypeDictionary = LogController.Instance.GetLogTypeInfoDictionary();

        private static IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        public LogTypeInfo GetMyLogType(string logTypeKey)
        {
			eventLogger.LogDebug(">>> Entering GetMyLogType(logTypeKey={LogTypeKey})", logTypeKey);
            this.LogTypeDictionary.TryGetValue(logTypeKey, out var logType);

            logType ??= new LogTypeInfo();

			eventLogger.LogDebug("<<< Exiting GetMyLogType(logTypeKey={LogTypeKey})", logTypeKey);
            return logType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetPropertiesText(object obj)
        {
			eventLogger.LogDebug(">>> Entering GetPropertiesText(obj={Obj})", obj);
            var str = new StringBuilder();
            var objLogInfo = (LogInfo)obj;
			eventLogger.LogDebug("GetPropertiesText(obj={Obj}): objLogInfo → {ObjLogInfo}", obj, objLogInfo);
			eventLogger.LogDebug("GetPropertiesText(obj={Obj}): str → {Str}", obj, str);
            if (objLogInfo != null)
            {
                var objLogProperties = objLogInfo.LogProperties;
                int i;
                for (i = 0; i <= objLogProperties.Count - 1; i++)
                {
                    // display the values in the Panel child controls.
                    var ldi = (LogDetailInfo)objLogProperties[i];
					eventLogger.LogDebug("GetPropertiesText(obj={Obj}): ldi → {Ldi}", obj, ldi);
                    if (ldi.PropertyName == "Message")
                    {
                        str.Append("<p><strong>" + ldi.PropertyName + "</strong>:</br><pre>" +
                                   HttpUtility.HtmlEncode(ldi.PropertyValue) + "</pre></p>");
                    }
                    else
                    {
                        str.Append("<p><strong>" + ldi.PropertyName + "</strong>:" +
                                   HttpUtility.HtmlEncode(ldi.PropertyValue) + "</p>");
                    }
                }

                if (!string.IsNullOrEmpty(objLogInfo.Exception.ExceptionHash))
                {
                    str.Append(objLogInfo.Exception);
                }

                str.Append("<p>" + Localization.GetString("ServerName", Constants.LocalResourcesFile) +
                           HttpUtility.HtmlEncode(objLogInfo.LogServerName) + "</p>");
            }

			eventLogger.LogDebug("<<< Exiting GetPropertiesText(obj={Obj})", obj);
            return str.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void ClearLog()
        {
			eventLogger.LogDebug(">>> Entering ClearLog()");
            LogController.Instance.ClearLog();

            // add entry to log recording it was cleared
            this.eventLogger.AddLog(
                Localization.GetString("LogCleared", Constants.LocalResourcesFile),
                Localization.GetString("Username", Constants.LocalResourcesFile) + ":" + UserController.Instance.GetCurrentUserInfo().Username,
                PortalSettings,
                -1,
                EventLogType.ADMIN_ALERT);
				eventLogger.LogDebug("<<< Exiting ClearLog()");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public Dictionary<string, string> GetKeepMostRecentOptions()
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            int[] items = { 1, 2, 3, 4, 5, 10, 25, 100, 250, 500 };
            options.Add(Localization.GetString("All", Constants.LocalResourcesFile), "*");
            foreach (int item in items)
            {
                if (item == 1)
                {
                    options.Add(
                        item + Localization.GetString("LogEntry", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    options.Add(
                        item + Localization.GetString("LogEntries", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
            }

            return options;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public Dictionary<string, string> GetOccurenceThresholdOptions()
        {
            Dictionary<string, string> thresholds = new Dictionary<string, string>();
            int[] items = [1, 2, 3, 4, 5, 10, 25, 100, 250, 500, 1000,];
            foreach (int item in items)
            {
                if (item == 1)
                {
                    thresholds.Add(
                        item + Localization.GetString("Occurence", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    thresholds.Add(
                        item + Localization.GetString("Occurences", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
            }

            return thresholds;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public Dictionary<string, string> GetOccurenceThresholds()
        {
            Dictionary<string, string> thresholds = new Dictionary<string, string>();
            int[] items = [1, 2, 3, 4, 5, 10, 25, 100, 250, 500, 1000,];
            foreach (int item in items)
            {
                if (item == 1)
                {
                    thresholds.Add(
                        item + Localization.GetString("Occurence", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    thresholds.Add(
                        item + Localization.GetString("Occurences", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
            }

            return thresholds;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public Dictionary<string, string> GetOccurenceThresholdNotificationTimes()
        {
            Dictionary<string, string> notificationTimes = new Dictionary<string, string>();
            foreach (int item in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 60, 90, 120 })
            {
                notificationTimes.Add(item.ToString(CultureInfo.InvariantCulture), item.ToString(CultureInfo.InvariantCulture));
            }

            return notificationTimes;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public Dictionary<string, string> GetOccurenceThresholdNotificationTimeTypes()
        {
            Dictionary<string, string> notificationTimeTypes = new Dictionary<string, string>();
            foreach (int item in new[] { 1, 2, 3, 4 })
            {
                notificationTimeTypes.Add(Localization.GetString($"TimeType_{item}", Constants.LocalResourcesFile), item.ToString(CultureInfo.InvariantCulture));
            }

            return notificationTimeTypes;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void UpdateLogTypeConfig(LogTypeConfigInfo logTypeConfigInfo)
        {
			eventLogger.LogDebug(">>> Entering UpdateLogTypeConfig(logTypeConfigInfo={LogTypeConfigInfo})", logTypeConfigInfo);
            LogController.Instance.UpdateLogTypeConfigInfo(logTypeConfigInfo);
			eventLogger.LogDebug("<<< Exiting UpdateLogTypeConfig(logTypeConfigInfo={LogTypeConfigInfo})", logTypeConfigInfo);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void AddLogTypeConfig(LogTypeConfigInfo logTypeConfigInfo)
        {
			eventLogger.LogDebug(">>> Entering AddLogTypeConfig(logTypeConfigInfo={LogTypeConfigInfo})", logTypeConfigInfo);
            LogController.Instance.AddLogTypeConfigInfo(logTypeConfigInfo);
			eventLogger.LogDebug("<<< Exiting AddLogTypeConfig(logTypeConfigInfo={LogTypeConfigInfo})", logTypeConfigInfo);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void DeleteLogTypeConfig(string logTypeConfigId)
        {
			eventLogger.LogDebug(">>> Entering DeleteLogTypeConfig(logTypeConfigId={LogTypeConfigId})", logTypeConfigId);
            var logTypeConfigInfo = new LogTypeConfigInfo();
            logTypeConfigInfo.ID = logTypeConfigId;
			eventLogger.LogDebug("DeleteLogTypeConfig(logTypeConfigId={LogTypeConfigId}): logTypeConfigInfo.ID → {Arg1}", logTypeConfigId, logTypeConfigInfo.ID);
			eventLogger.LogDebug("DeleteLogTypeConfig(logTypeConfigId={LogTypeConfigId}): logTypeConfigInfo → {LogTypeConfigInfo}", logTypeConfigId, logTypeConfigInfo);
            LogController.Instance.DeleteLogTypeConfigInfo(logTypeConfigInfo);
			eventLogger.LogDebug("<<< Exiting DeleteLogTypeConfig(logTypeConfigId={LogTypeConfigId})", logTypeConfigId);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public LogTypeConfigInfo GetLogTypeConfig(string logTypeConfigId)
        {
			eventLogger.LogDebug(">>> Entering GetLogTypeConfig(logTypeConfigId={LogTypeConfigId})", logTypeConfigId);
			eventLogger.LogDebug("<<< Exiting GetLogTypeConfig(logTypeConfigId={LogTypeConfigId})", logTypeConfigId);
            return LogController.Instance.GetLogTypeConfigInfoByID(logTypeConfigId);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string EmailLogItems(string subject, string fromEmailAddress, string toEmailAddress, string message, IEnumerable<string> logItemIds, out string error)
        {
			eventLogger.LogDebug(">>> Entering EmailLogItems(subject={Subject},fromEmailAddress={FromEmailAddress},toEmailAddress={ToEmailAddress},message={Message},logItemIds={LogItemIds})", subject, fromEmailAddress, toEmailAddress, message, logItemIds);
            if (string.IsNullOrEmpty(subject))
            {
                subject = PortalSettings.PortalName + @" Exceptions";
				eventLogger.LogDebug("EmailLogItems(subject={Subject},fromEmailAddress={FromEmailAddress},toEmailAddress={ToEmailAddress},message={Message},logItemIds={LogItemIds}): subject → {Subject1}", subject, fromEmailAddress, toEmailAddress, message, logItemIds, subject);
            }

            string returnMsg;
            if (Globals.EmailValidatorRegex.IsMatch(fromEmailAddress))
            {
                const string tempFileName = "errorlog.xml";
                var filePath = PortalSettings.HomeDirectoryMapPath + tempFileName;
                var xmlDoc = GetExceptions(logItemIds);
				eventLogger.LogDebug("EmailLogItems(subject={Subject},fromEmailAddress={FromEmailAddress},toEmailAddress={ToEmailAddress},message={Message},logItemIds={LogItemIds}): xmlDoc → {XmlDoc}", subject, fromEmailAddress, toEmailAddress, message, logItemIds, xmlDoc);
                xmlDoc.Save(filePath);

                var attachments = new List<Attachment>();
				eventLogger.LogDebug("EmailLogItems(subject={Subject},fromEmailAddress={FromEmailAddress},toEmailAddress={ToEmailAddress},message={Message},logItemIds={LogItemIds}): attachments → {Attachments}", subject, fromEmailAddress, toEmailAddress, message, logItemIds, attachments);
                var ct = new ContentType { MediaType = MediaTypeNames.Text.Xml, Name = tempFileName };

                using (var attachment = new Attachment(filePath, ct))
                {
                    attachments.Add(attachment);

                    returnMsg = Mail.SendEmail(fromEmailAddress, fromEmailAddress, toEmailAddress, subject, message, attachments);
                }

                FileSystemUtils.DeleteFile(filePath);

                if (string.IsNullOrEmpty(returnMsg))
                {
                    error = Localization.GetString("EmailSuccess", Constants.LocalResourcesFile);
                }
                else
                {
                    error = Localization.GetString("EmailFailure", Constants.LocalResourcesFile);
                }
            }
            else
            {
                returnMsg = string.Format(CultureInfo.CurrentCulture, Localization.GetString("InavlidEmailAddress", Constants.LocalResourcesFile), fromEmailAddress);
                error = Localization.GetString("EmailFailure", Constants.LocalResourcesFile);
            }
			eventLogger.LogDebug("EmailLogItems(subject={Subject},fromEmailAddress={FromEmailAddress},toEmailAddress={ToEmailAddress},message={Message},logItemIds={LogItemIds}): returnMsg → {ReturnMsg}", subject, fromEmailAddress, toEmailAddress, message, logItemIds, returnMsg);

			eventLogger.LogDebug("<<< Exiting EmailLogItems(subject={Subject},fromEmailAddress={FromEmailAddress},toEmailAddress={ToEmailAddress},message={Message},logItemIds={LogItemIds})", subject, fromEmailAddress, toEmailAddress, message, logItemIds);
            return returnMsg;
        }

        private static XmlDocument GetExceptions(IEnumerable<string> logIds)
        {
			eventLogger.LogDebug(">>> Entering GetExceptions(logIds={LogIds})", logIds);
            var objXml = new XmlDocument { XmlResolver = null };
            using (var logEntriesReader = XmlReader.Create(new StringReader("<LogEntries></LogEntries>"), new XmlReaderSettings { XmlResolver = null, }))
            {
                objXml.Load(logEntriesReader);
            }

            foreach (var logId in logIds)
            {
                var log = LogController.Instance.GetLog(logId);
				eventLogger.LogDebug("GetExceptions(logIds={LogIds}): log → {Log}", logIds, log);
                var xmlDoc = new XmlDocument { XmlResolver = null };

                if (log is LogInfo logInfo)
                {
                    using var logReader = XmlReader.Create(new StringReader(logInfo.Serialize()), new XmlReaderSettings { XmlResolver = null, });
					eventLogger.LogDebug("GetExceptions(logIds={LogIds}): logReader → {LogReader}", logIds, logReader);
                    xmlDoc.Load(logReader);
                }

                var objNode = objXml.ImportNode(xmlDoc.DocumentElement, true);
				eventLogger.LogDebug("GetExceptions(logIds={LogIds}): objNode → {ObjNode}", logIds, objNode);
                if (objXml.DocumentElement != null)
                {
                    objXml.DocumentElement.AppendChild(objNode);
                }
            }

			eventLogger.LogDebug("<<< Exiting GetExceptions(logIds={LogIds})", logIds);
            return objXml;
        }
    }
}
