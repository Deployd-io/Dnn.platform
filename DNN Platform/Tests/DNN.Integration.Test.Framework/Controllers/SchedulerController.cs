// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using DNN.Integration.Test.Framework.Helpers;
	using Microsoft.Extensions.Logging;

    public enum SchedulingMode
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        Disabled = 0,
        Timer = 1,
        Request = 2,
    }

    public static class SchedulerController
    {
        // private const string SchedulerModeName = "SchedulerMode";
        public static void DisableAllSchedulers(bool clearCache = true)
        {
			_logger.LogDebug(">>> Entering DisableAllSchedulers(clearCache={ClearCache})", clearCache);
            SetSchedulingMode(SchedulingMode.Disabled, clearCache);
			_logger.LogDebug("<<< Exiting DisableAllSchedulers(clearCache={ClearCache})", clearCache);
        }

        public static void DisableAppStartDelay(bool clearCache = true)
        {
			_logger.LogDebug(">>> Entering DisableAppStartDelay(clearCache={ClearCache})", clearCache);
            DatabaseHelper.ExecuteStoredProcedure("UpdateHostSetting", "SchedulerdelayAtAppStart", "0", false, 1);
            if (clearCache)
            {
                WebApiTestHelper.ClearHostCache();
            }
			_logger.LogDebug("<<< Exiting DisableAppStartDelay(clearCache={ClearCache})", clearCache);
        }

        public static void DisableScheduler(string schedulerName, bool clearCache = false)
        {
			_logger.LogDebug(">>> Entering DisableScheduler(schedulerName={SchedulerName},clearCache={ClearCache})", schedulerName, clearCache);
            var query = string.Format(
                "UPDATE {{objectQualifier}}Schedule SET Enabled=0 WHERE FriendlyName = '{0}';", schedulerName);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DatabaseHelper.ExecuteNonQuery(query);
            if (clearCache)
			_logger.LogDebug("DisableScheduler(schedulerName={SchedulerName},clearCache={ClearCache}): Enabled → {Enabled}", schedulerName, clearCache, Enabled);
			_logger.LogInformation("DisableScheduler(schedulerName,clearCache)={SchedulerName},{ClearCache}: update query executed in {Arg2} ms", schedulerName, clearCache, __logStart.ElapsedMilliseconds);
            {
                WebApiTestHelper.ClearHostCache();
            }
			_logger.LogDebug("<<< Exiting DisableScheduler(schedulerName={SchedulerName},clearCache={ClearCache})", schedulerName, clearCache);
        }

        public static void EnableScheduler(string schedulerName, bool clearCache = false)
        {
			_logger.LogDebug(">>> Entering EnableScheduler(schedulerName={SchedulerName},clearCache={ClearCache})", schedulerName, clearCache);
            var query = string.Format(
                "UPDATE {{objectQualifier}}Schedule SET Enabled=1 WHERE FriendlyName = '{0}';", schedulerName);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DatabaseHelper.ExecuteNonQuery(query);
            if (clearCache)
			_logger.LogDebug("EnableScheduler(schedulerName={SchedulerName},clearCache={ClearCache}): Enabled → {Enabled}", schedulerName, clearCache, Enabled);
			_logger.LogInformation("EnableScheduler(schedulerName,clearCache)={SchedulerName},{ClearCache}: update query executed in {Arg2} ms", schedulerName, clearCache, __logStart.ElapsedMilliseconds);
            {
                WebApiTestHelper.ClearHostCache();
            }
			_logger.LogDebug("<<< Exiting EnableScheduler(schedulerName={SchedulerName},clearCache={ClearCache})", schedulerName, clearCache);
        }

        public static SchedulingMode GetSchedulingMode()
        {
			_logger.LogDebug(">>> Entering GetSchedulingMode()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            const string query = @"SELECT SettingValue FROM {objectQualifier}HostSettings WHERE SettingName='SchedulerMode';";
            var modeStr = DatabaseHelper.ExecuteScalar<string>(query);

			_logger.LogDebug("GetSchedulingMode(): query → {Query}", query);
			_logger.LogDebug("GetSchedulingMode(): modeStr → {ModeStr}", modeStr);
			_logger.LogInformation("GetSchedulingMode(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            SchedulingMode mode;
            Enum.TryParse(modeStr, out mode);
			_logger.LogDebug("<<< Exiting GetSchedulingMode()");
            return mode;
        }

        public static void SetSchedulingMode(SchedulingMode mode, bool clearCache = true)
        {
			_logger.LogDebug(">>> Entering SetSchedulingMode(mode={Mode},clearCache={ClearCache})", mode, clearCache);
            var current = GetSchedulingMode();
            if (current != mode)
			_logger.LogDebug("SetSchedulingMode(mode={Mode},clearCache={ClearCache}): current → {Current}", mode, clearCache, current);
            {
                DatabaseHelper.ExecuteStoredProcedure("UpdateHostSetting", "SchedulerMode", mode.ToString("D"), false, 1);
                if (clearCache)
                {
                    WebApiTestHelper.ClearHostCache(); // must clear the site Cache afterwards
                }
            }
			_logger.LogDebug("<<< Exiting SetSchedulingMode(mode={Mode},clearCache={ClearCache})", mode, clearCache);
        }

        public static int GetSchedulerIdByName(string schedulerName)
        {
			_logger.LogDebug(">>> Entering GetSchedulerIdByName(schedulerName={SchedulerName})", schedulerName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                "SELECT TOP(1) ISNULL(ScheduleId, -1) FROM {{objectQualifier}}Schedule WHERE FriendlyName = '{0}';",
                schedulerName);
				_logger.LogDebug("GetSchedulerIdByName(schedulerName={SchedulerName}): FriendlyName → {FriendlyName}", schedulerName, FriendlyName);
				_logger.LogInformation("GetSchedulerIdByName(schedulerName)={SchedulerName}: find query executed in {Arg1} ms", schedulerName, __logStart.ElapsedMilliseconds);

			_logger.LogDebug("<<< Exiting GetSchedulerIdByName(schedulerName={SchedulerName})", schedulerName);
            return DatabaseHelper.ExecuteScalar<int>(query);
        }

        public static IDictionary<string, object> GetSchedulerByName(string schedulerName)
        {
            var query = string.Format(
                "SELECT TOP(1) * FROM {{objectQualifier}}Schedule WHERE FriendlyName = '{0}';",
                schedulerName);

            return DatabaseHelper.ExecuteQuery(query).FirstOrDefault();
        }

        /// <summary>Runs a specific scheduler and returns the resul depending on the passed flags.</summary>
        /// <param name="schedulerName">Name of the scheduler in the database.</param>
        /// <param name="maxWaitSeconds">Maimum amount of time to wait for the task to finish.</param>
        /// <returns>Result of running the taske (depends on the flags).</returns>
        /// <remarks>
        /// If maxWaitSeconds is less than or equals 0: the return result is only the result of the call
        /// to the "Run Now" button of the Edit Scheduler UI page.
        /// If waitUntilFinishes is greater than 0: this method will wait until the running task finishes and records its
        /// status in the Schedule History table and returns the recorded status in the last added record.
        /// </remarks>
        public static bool RunScheduler(string schedulerName, int maxWaitSeconds = 0)
        {
			_logger.LogDebug(">>> Entering RunScheduler(schedulerName={SchedulerName},maxWaitSeconds={MaxWaitSeconds})", schedulerName, maxWaitSeconds);
            var disabled = false;
            EnableScheduler(schedulerName);
            SetSchedulingMode(SchedulingMode.Timer);

            try
            {
                var schedulInfo = GetSchedulerByName(schedulerName);
                if (schedulInfo == null || schedulInfo.Count == 0)
                {
                    return false;
                }

                // HOST modules have only single instance, so don't worry about receiving multiple rows
                var results = ModuleController.GetModulesByFriendlyName(DnnDataHelper.PortalId, "Scheduler");
                var moduleId = results.SelectMany(x => x.Where(y => y.Key == "ModuleID").Select(y => (int)y.Value)).FirstOrDefault();

                Console.WriteLine(@"Triggering Scheduler: '{0}'", schedulerName);

                ScheduleHistoryInfo lastRunInfo = null;
                if (maxWaitSeconds > 0)
                {
                    var scheduleId = (int)schedulInfo["ScheduleId"];
                    lastRunInfo = LastRunningScheduleItem(scheduleId);
                }

                var resp = TriggerScheduler(schedulInfo, moduleId);

                // only OK is a successful POST in this case; all other codes are failures
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(
                        @"Error running scheduler {0}. Status: {1} - {2}",
                        schedulerName, resp.StatusCode, resp.StatusDescription);
                    return false;
                }

                // DisableScheduler(schedulerName); // un-necessary
                SetSchedulingMode(SchedulingMode.Disabled);
                disabled = true;

                if (maxWaitSeconds <= 0)
                {
                    return true;
                }

                // wait for task to finish
                var latestRunInfo = WaitForTaskToFinish(lastRunInfo, maxWaitSeconds);
				_logger.LogDebug("<<< Exiting RunScheduler(schedulerName={SchedulerName},maxWaitSeconds={MaxWaitSeconds})", schedulerName, maxWaitSeconds);
                return latestRunInfo.Succeeded;
            }
            finally
            {
                if (!disabled)
                {
                    SetSchedulingMode(SchedulingMode.Disabled);
                }
            }
        }

        // trigger the scheduler to run through [Run Now] button of the UI
        private static HttpWebResponse TriggerScheduler(IDictionary<string, object> scheduleInfo, int moduleId)
        {
            var scheduleId = (int)scheduleInfo["ScheduleId"];
            var scheduleTypeName = (string)scheduleInfo["TypeFullName"];
            var scheduleFriendlyName = (string)scheduleInfo["FriendlyName"];
            var objectDependencies = (string)scheduleInfo["ObjectDependencies"];

            var fieldsPrefix = string.Format("dnn$ctr{0}$EditSchedule", moduleId);
            var postData = new Dictionary<string, object>
            {
                { fieldsPrefix + "$chkEnabled", "on" },
                { fieldsPrefix + "$txtServers", string.Empty },
                { fieldsPrefix + "$ddlAttachToEvent", "None" },
                { fieldsPrefix + "$ddlRetainHistoryNum", "100" },
                { fieldsPrefix + "$ddlRetryTimeLapseMeasurement", "Minutes" },
                { fieldsPrefix + "$ddlTimeLapseMeasurement", "Hours" },
                { fieldsPrefix + "$startScheduleDatePicker$dateInput", string.Empty },
                { fieldsPrefix + "$startScheduleDatePicker", string.Empty },
                { fieldsPrefix + "$txtFriendlyName", scheduleFriendlyName },
                { fieldsPrefix + "$txtObjectDependencies", objectDependencies },
                { fieldsPrefix + "$txtRetryTimeLapse", "30" },
                { fieldsPrefix + "$txtTimeLapse", "1" },
                { fieldsPrefix + "$txtType", scheduleTypeName },
                { "__EVENTTARGET", fieldsPrefix + "$cmdRun" }, // button action; if missing, no click action is performed
                { "__EVENTARGUMENT", string.Empty },
                { "__ASYNCPOST", string.Empty },

                // all other inputs/fields are left as is
            };

            var relativeUrl = string.Format(
                "/Host/Schedule/ctl/Edit/mid/{0}/ScheduleId/{1}/portalid/{2}",
                moduleId, scheduleId, DnnDataHelper.PortalId);
            return WebApiTestHelper.LoginHost().PostUserForm(relativeUrl, postData, null);
        }

        // waits for the task to add a new record in the Scheduler History table (i.e., finish)
        private static ScheduleHistoryInfo WaitForTaskToFinish(ScheduleHistoryInfo lastRunInfo, int maxWait)
        {
            var maxTime = DateTime.Now.AddSeconds(maxWait);
            var latestInfo = LastRunningScheduleItem(lastRunInfo.ScheduleId);

            // must wait for it to update end time; not just adding a history record
            while (DateTime.Now < maxTime &&
                (latestInfo.ScheduleHistoryId == lastRunInfo.ScheduleHistoryId ||
                latestInfo.EndDate <= lastRunInfo.EndDate))
            {
                Thread.Sleep(250); // give time for task to finish
                latestInfo = LastRunningScheduleItem(lastRunInfo.ScheduleId);
            }

            return latestInfo.ScheduleHistoryId == lastRunInfo.ScheduleHistoryId
                ? new ScheduleHistoryInfo(lastRunInfo)
                : latestInfo;
        }

        /// <summary>Finds the last time a specific scheduler was run and the ID of the last run.</summary>
        /// <param name="scheduleId">Scheduler ID to inquire.</param>
        /// <remarks>If no previous run exists, it returns ID as -1.</remarks>
        private static ScheduleHistoryInfo LastRunningScheduleItem(int scheduleId)
        {
            const string script = @"IF EXISTS (SELECT 1 FROM {{objectQualifier}}ScheduleHistory WHERE ScheduleId = {0})
	                SELECT TOP(1) ScheduleId, ScheduleHistoryId, COALESCE(Succeeded, CAST(0 AS BIT)) AS Succeeded,
                           COALESCE(EndDate, {{ts '2000-01-01 00:00:00'}}) AS EndDate, LogNotes
	                FROM   {{objectQualifier}}ScheduleHistory
	                WHERE  ScheduleId = {0}
                    ORDER BY ScheduleHistoryId DESC
                ELSE
	                SELECT {0} AS ScheduleId, -1 AS ScheduleHistoryId, CAST(0 AS BIT) AS Succeeded,
                           {{ts '2000-01-01 00:00:00'}} AS EndDate, CAST(NULL AS NVARCHAR) AS LogNotes";

            var query = string.Format(script, scheduleId);
            var result = DatabaseHelper.ExecuteQuery(query).First();
            return new ScheduleHistoryInfo(result);
        }
    }

    public class ScheduleHistoryInfo
    {
        public ScheduleHistoryInfo(IDictionary<string, object> queryResult)
        {
			_logger.LogDebug(">>> Entering ScheduleHistoryInfo(queryResult={QueryResult})", queryResult);
            try
            {
                this.ScheduleId = Convert.ToInt32(queryResult["ScheduleId"]);
				_logger.LogDebug("ScheduleHistoryInfo(queryResult={QueryResult}): ScheduleId → {ScheduleId}", queryResult, ScheduleId);
                this.ScheduleHistoryId = Convert.ToInt32(queryResult["ScheduleHistoryId"]);
				_logger.LogDebug("ScheduleHistoryInfo(queryResult={QueryResult}): ScheduleHistoryId → {ScheduleHistoryId}", queryResult, ScheduleHistoryId);
                this.EndDate = Convert.ToDateTime(queryResult["EndDate"]);
				_logger.LogDebug("ScheduleHistoryInfo(queryResult={QueryResult}): EndDate → {EndDate}", queryResult, EndDate);
                this.Succeeded = Convert.ToBoolean(queryResult["Succeeded"]);
				_logger.LogDebug("ScheduleHistoryInfo(queryResult={QueryResult}): Succeeded → {Succeeded}", queryResult, Succeeded);
                this.LogNotes = queryResult["LogNotes"].ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(@"LastRunInfo => " + Newtonsoft.Json.JsonConvert.SerializeObject(queryResult));
                Console.WriteLine(@"Error => " + e.Message);
			_logger.LogDebug("<<< Exiting ScheduleHistoryInfo(queryResult={QueryResult})", queryResult);
                throw;
            }
        }

        internal ScheduleHistoryInfo(int scheduleId, int scheduleHistoryID, DateTime endDate, bool succeeded)
        {
            this.ScheduleId = scheduleId;
            this.ScheduleHistoryId = scheduleHistoryID;
            this.EndDate = endDate;
            this.Succeeded = succeeded;
            this.LogNotes = string.Empty;
        }

        // clones another info with success as false
        internal ScheduleHistoryInfo(ScheduleHistoryInfo lastRunInfo)
        {
            this.ScheduleId = lastRunInfo.ScheduleId;
            this.ScheduleHistoryId = lastRunInfo.ScheduleHistoryId;
            this.EndDate = lastRunInfo.EndDate;
            this.Succeeded = false;
            this.LogNotes = lastRunInfo.LogNotes;
        }

        public int ScheduleId { get; }

        public int ScheduleHistoryId { get; }

        public DateTime EndDate { get; }

        public bool Succeeded { get; }

        public string LogNotes { get; }
    }
}
