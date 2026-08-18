// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.TaskScheduler.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;
    using Microsoft.Extensions.DependencyInjection;

    public class TaskSchedulerController
    {
        private static readonly string SchedulersToRunOnSameWebServerKey = "SchedulersToRunOnSameWebServer";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TaskSchedulerController));

        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.TaskScheduler/App_LocalResources/TaskScheduler.resx");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetTimeLapse(int timeLapse, string timeLapseMeasurement)
        {
			Logger.LogDebug(">>> Entering GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement})", timeLapse, timeLapseMeasurement);
            if (timeLapse != Null.NullInteger)
            {
                var str = Null.NullString;
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): str → {Str}", timeLapse, timeLapseMeasurement, str);
                var strPrefix = Localization.GetString("TimeLapsePrefix", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strPrefix → {StrPrefix}", timeLapse, timeLapseMeasurement, strPrefix);
                var strSec = Localization.GetString("Second", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strSec → {StrSec}", timeLapse, timeLapseMeasurement, strSec);
                var strMn = Localization.GetString("Minute", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strMn → {StrMn}", timeLapse, timeLapseMeasurement, strMn);
                var strHour = Localization.GetString("Hour", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strHour → {StrHour}", timeLapse, timeLapseMeasurement, strHour);
                var strDay = Localization.GetString("Day", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strDay → {StrDay}", timeLapse, timeLapseMeasurement, strDay);
                var strWeek = Localization.GetString("Week", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strWeek → {StrWeek}", timeLapse, timeLapseMeasurement, strWeek);
                var strMonth = Localization.GetString("Month", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strMonth → {StrMonth}", timeLapse, timeLapseMeasurement, strMonth);
                var strYear = Localization.GetString("Year", LocalResourcesFile);
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strYear → {StrYear}", timeLapse, timeLapseMeasurement, strYear);
                var strSecs = Localization.GetString("Seconds");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strSecs → {StrSecs}", timeLapse, timeLapseMeasurement, strSecs);
                var strMns = Localization.GetString("Minutes");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strMns → {StrMns}", timeLapse, timeLapseMeasurement, strMns);
                var strHours = Localization.GetString("Hours");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strHours → {StrHours}", timeLapse, timeLapseMeasurement, strHours);
                var strDays = Localization.GetString("Days");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strDays → {StrDays}", timeLapse, timeLapseMeasurement, strDays);
                var strWeeks = Localization.GetString("Weeks");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strWeeks → {StrWeeks}", timeLapse, timeLapseMeasurement, strWeeks);
                var strMonths = Localization.GetString("Months");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strMonths → {StrMonths}", timeLapse, timeLapseMeasurement, strMonths);
                var strYears = Localization.GetString("Years");
				Logger.LogDebug("GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement}): strYears → {StrYears}", timeLapse, timeLapseMeasurement, strYears);
                switch (timeLapseMeasurement)
                {
                    case "s":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strSecs : strSec);
                        break;
                    case "m":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strMns : strMn);
                        break;
                    case "h":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strHours : strHour);
                        break;
                    case "d":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strDays : strDay);
                        break;
                    case "w":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strWeeks : strWeek);
                        break;
                    case "mo":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strMonths : strMonth);
                        break;
                    case "y":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strYears : strYear);
                        break;
                }

			Logger.LogDebug("<<< Exiting GetTimeLapse(timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement})", timeLapse, timeLapseMeasurement);
                return str;
            }

            return Localization.GetString("n/a", LocalResourcesFile);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void StopSchedule()
        {
			Logger.LogDebug(">>> Entering StopSchedule()");
            SchedulingProvider.Instance().Halt(Localization.GetString("ManuallyStopped", LocalResourcesFile));
			Logger.LogDebug("<<< Exiting StopSchedule()");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public ScheduleItem CreateScheduleItem(string typeFullName, string friendlyName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string scheduleStartDate, string servers)
        {
			Logger.LogDebug(">>> Entering CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers})", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers);
            var scheduleItem = new ScheduleItem();
            scheduleItem.TypeFullName = typeFullName;
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.TypeFullName → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.TypeFullName);
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.FriendlyName → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.FriendlyName);
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.TimeLapse → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.TimeLapse);
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem → {ScheduleItem}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem);
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.RetryTimeLapse → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.RetryTimeLapse);
            scheduleItem.FriendlyName = friendlyName;
            scheduleItem.TimeLapse = timeLapse;
            scheduleItem.TimeLapseMeasurement = string.IsNullOrEmpty(timeLapseMeasurement) ? "s" : timeLapseMeasurement;
            scheduleItem.RetryTimeLapse = retryTimeLapse;
            scheduleItem.RetryTimeLapseMeasurement = string.IsNullOrEmpty(retryTimeLapseMeasurement) ? "s" : retryTimeLapseMeasurement;
            scheduleItem.RetainHistoryNum = retainHistoryNum;
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.RetainHistoryNum → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.RetainHistoryNum);
            scheduleItem.AttachToEvent = string.IsNullOrEmpty(attachToEvent) ? string.Empty : attachToEvent;
            scheduleItem.CatchUpEnabled = catchUpEnabled;
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.CatchUpEnabled → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.CatchUpEnabled);
			Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): scheduleItem.Enabled → {Arg13}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, scheduleItem.Enabled);
            scheduleItem.Enabled = enabled;
            scheduleItem.ObjectDependencies = string.IsNullOrEmpty(objectDependencies) ? string.Empty : objectDependencies;
            scheduleItem.ScheduleStartDate = !string.IsNullOrEmpty(scheduleStartDate) ? Convert.ToDateTime(scheduleStartDate, CultureInfo.InvariantCulture) : Null.NullDate;

            if (!string.IsNullOrEmpty(servers))
            {
                if (!servers.StartsWith(",", StringComparison.Ordinal))
                {
                    servers = "," + servers;
					Logger.LogDebug("CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers}): servers → {Servers1}", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers, servers);
                }

                if (!servers.EndsWith(",", StringComparison.Ordinal))
                {
                    servers = servers + ",";
                }
            }

            scheduleItem.Servers = string.IsNullOrEmpty(servers) ? Null.NullString : servers;
			Logger.LogDebug("<<< Exiting CreateScheduleItem(typeFullName={TypeFullName},friendlyName={FriendlyName},timeLapse={TimeLapse},timeLapseMeasurement={TimeLapseMeasurement},retryTimeLapse={RetryTimeLapse},retryTimeLapseMeasurement={RetryTimeLapseMeasurement},retainHistoryNum={RetainHistoryNum},attachToEvent={AttachToEvent},catchUpEnabled={CatchUpEnabled},enabled={Enabled},objectDependencies={ObjectDependencies},scheduleStartDate={ScheduleStartDate},servers={Servers})", typeFullName, friendlyName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, scheduleStartDate, servers);
            return scheduleItem;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<ScheduleItem> GetScheduleItems(bool? enabled, string serverName = "", string taskName = "")
        {
			Logger.LogDebug(">>> Entering GetScheduleItems(enabled={Enabled},serverName={ServerName},taskName={TaskName})", enabled, serverName, taskName);
            try
            {
                IEnumerable<ScheduleItem> scheduleViews;
                if (string.IsNullOrEmpty(serverName) || serverName == Localization.GetString("All"))
                {
                    scheduleViews = SchedulingController.GetSchedule();
                }
                else
                {
                    scheduleViews = SchedulingController.GetSchedule(serverName);
                }

                if (!string.IsNullOrEmpty(taskName))
                {
                    scheduleViews = scheduleViews.Where(item => item.FriendlyName.IndexOf(taskName, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (enabled.HasValue)
                {
                    scheduleViews = scheduleViews.Where(item => item.Enabled == enabled.Value);
                }

                var scheduleItems = scheduleViews as IList<ScheduleItem> ?? scheduleViews.ToList();
				Logger.LogDebug("GetScheduleItems(enabled={Enabled},serverName={ServerName},taskName={TaskName}): scheduleItems → {ScheduleItems}", enabled, serverName, taskName, scheduleItems);
                foreach (var item in scheduleItems.Where(x => x.NextStart == Null.NullDate)
                            .Where(item => item.ScheduleStartDate != Null.NullDate))
                {
                    item.NextStart = item.ScheduleStartDate;
					Logger.LogDebug("GetScheduleItems(enabled={Enabled},serverName={ServerName},taskName={TaskName}): item.NextStart → {Arg3}", enabled, serverName, taskName, item.NextStart);
                }

                return scheduleItems;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetScheduleItems(enabled={Enabled},serverName={ServerName},taskName={TaskName})", enabled, serverName, taskName);
                return null;
            }
        }

        /// <summary>Gets a list of servers to be recommended for a particular scheduler.</summary>
        /// <param name="schedulerId">Scheduler Id.</param>
        /// <returns>List of recommended servers for specified <paramref name="schedulerId"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<string> GetRecommendedServers(int schedulerId)
        {
			Logger.LogDebug(">>> Entering GetRecommendedServers(schedulerId={SchedulerId})", schedulerId);
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();

			Logger.LogDebug("GetRecommendedServers(schedulerId={SchedulerId}): hostSettingsService → {HostSettingsService}", schedulerId, hostSettingsService);
            var schedulerIds = hostSettingsService.GetString(SchedulersToRunOnSameWebServerKey, string.Empty)
                .Split([',',], StringSplitOptions.RemoveEmptyEntries)
                .Where(x => int.TryParse(x, out var id))
                .Select(x => int.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();

            if (!schedulerIds.Contains(schedulerId))
            {
			Logger.LogDebug("<<< Exiting GetRecommendedServers(schedulerId={SchedulerId})", schedulerId);
                return [];
            }

            return SchedulingProvider.Instance().GetSchedule()
                .Cast<ScheduleItem>()
                .Where(x => x.ScheduleID != schedulerId
                            && x.Enabled
                            && schedulerIds.Contains(x.ScheduleID)
                            && !string.IsNullOrWhiteSpace(x.Servers))
                .SelectMany(x => x.Servers
                    .Split([',',], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x);
        }
    }
}
