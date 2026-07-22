// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;

    /// <summary>Extensions methods for types related to the Event Log.</summary>
    public static class EventLogExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Add setting log.</summary>
        /// <param name="logger">The <see cref="IEventLogger"/> instance.</param>
        /// <param name="logTypeKey">The log type.</param>
        /// <param name="idFieldName">The ID field name, e.g. <c>"ModuleId"</c> or <c>"TabId"</c>.</param>
        /// <param name="idValue">The ID value.</param>
        /// <param name="settingName">The setting name.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="userId">The ID of the user taking the action.</param>
        public static void AddSettingLog(this IEventLogger logger, EventLogType logTypeKey, string idFieldName, int idValue, string settingName, string settingValue, int userId)
        {
			Logger.LogDebug(">>> Entering AddSettingLog(logger={Logger},logTypeKey={LogTypeKey},idFieldName={IdFieldName},idValue={IdValue},settingName={SettingName},settingValue={SettingValue},userId={UserId})", logger, logTypeKey, idFieldName, idValue, settingName, settingValue, userId);
            ILogInfo log = new LogInfo { LogTypeKey = logTypeKey.ToString(), };
            log.LogUserId = userId;
			Logger.LogDebug("AddSettingLog(logger={Logger},logTypeKey={LogTypeKey},idFieldName={IdFieldName},idValue={IdValue},settingName={SettingName},settingValue={SettingValue},userId={UserId}): log → {Log}", logger, logTypeKey, idFieldName, idValue, settingName, settingValue, userId, log);
			Logger.LogDebug("AddSettingLog(logger={Logger},logTypeKey={LogTypeKey},idFieldName={IdFieldName},idValue={IdValue},settingName={SettingName},settingValue={SettingValue},userId={UserId}): log.LogUserId → {Arg7}", logger, logTypeKey, idFieldName, idValue, settingName, settingValue, userId, log.LogUserId);
            log.LogProperties.Add(new LogDetailInfo(idFieldName, idValue.ToString(CultureInfo.InvariantCulture)));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            log.LogProperties.Add(new LogDetailInfo("SettingValue", settingValue));

            logger.AddLog(log);
			Logger.LogDebug("<<< Exiting AddSettingLog(logger={Logger},logTypeKey={LogTypeKey},idFieldName={IdFieldName},idValue={IdValue},settingName={SettingName},settingValue={SettingValue},userId={UserId})", logger, logTypeKey, idFieldName, idValue, settingName, settingValue, userId);
        }
    }
}
