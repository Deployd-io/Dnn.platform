// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Host;
    using Microsoft.VisualBasic;

    public class DNNScheduler : SchedulingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNScheduler));
        /// <inheritdoc />
        public override Dictionary<string, string> Settings
        {
            get
            {
                return ComponentFactory.GetComponentSettings<DNNScheduler>() as Dictionary<string, string>;
            }
        }

        /// <inheritdoc />
        public override int AddSchedule(ScheduleItem scheduleItem)
        {
			Logger.LogError(">>> Entering AddSchedule(scheduleItem={ScheduleItem})", scheduleItem);
            // Remove item from queue
            Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);

            // save item
            scheduleItem.ScheduleID = SchedulingController.AddSchedule(
                scheduleItem.TypeFullName,
                scheduleItem.TimeLapse,
                scheduleItem.TimeLapseMeasurement,
                scheduleItem.RetryTimeLapse,
                scheduleItem.RetryTimeLapseMeasurement,
                scheduleItem.RetainHistoryNum,
                scheduleItem.AttachToEvent,
                scheduleItem.CatchUpEnabled,
                scheduleItem.Enabled,
                scheduleItem.ObjectDependencies,
                scheduleItem.Servers,
                scheduleItem.FriendlyName,
                scheduleItem.ScheduleStartDate);

            // Add schedule to queue
            this.RunScheduleItemNow(scheduleItem);

            // Return Id
			Logger.LogWarning("<<< Exiting AddSchedule(scheduleItem={ScheduleItem})", scheduleItem);
            return scheduleItem.ScheduleID;
        }

        /// <inheritdoc />
        public override void AddScheduleItemSetting(int scheduleID, string name, string value)
        {
			Logger.LogError(">>> Entering AddScheduleItemSetting(scheduleID={ScheduleID},name={Name},value={Value})", scheduleID, name, value);
            SchedulingController.AddScheduleItemSetting(scheduleID, name, value);
			Logger.LogWarning("<<< Exiting AddScheduleItemSetting(scheduleID={ScheduleID},name={Name},value={Value})", scheduleID, name, value);
        }

        /// <inheritdoc />
        public override void DeleteSchedule(ScheduleItem scheduleItem)
        {
			Logger.LogError(">>> Entering DeleteSchedule(scheduleItem={ScheduleItem})", scheduleItem);
            SchedulingController.DeleteSchedule(scheduleItem.ScheduleID);
            Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);
            DataCache.RemoveCache("ScheduleLastPolled");
			Logger.LogWarning("<<< Exiting DeleteSchedule(scheduleItem={ScheduleItem})", scheduleItem);
        }

        /// <inheritdoc />
        public override void ExecuteTasks()
        {
			Logger.LogError(">>> Entering ExecuteTasks()");
            if (Enabled)
            {
                Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
                Scheduler.CoreScheduler.KeepRunning = true;
                Scheduler.CoreScheduler.KeepThreadAlive = false;
                Scheduler.CoreScheduler.Start();
            }
			Logger.LogWarning("<<< Exiting ExecuteTasks()");
        }

        /// <inheritdoc />
        public override int GetActiveThreadCount()
        {
			Logger.LogError(">>> Entering GetActiveThreadCount()");
            Logger.LogWarning("<<< Exiting GetActiveThreadCount()");
            return SchedulingController.GetActiveThreadCount();
        }

        /// <inheritdoc />
        public override int GetFreeThreadCount()
        {
			Logger.LogError(">>> Entering GetFreeThreadCount()");
            Logger.LogWarning("<<< Exiting GetFreeThreadCount()");
            return SchedulingController.GetFreeThreadCount();
        }

        /// <inheritdoc />
        public override int GetMaxThreadCount()
        {
			Logger.LogError(">>> Entering GetMaxThreadCount()");
            Logger.LogWarning("<<< Exiting GetMaxThreadCount()");
            return SchedulingController.GetMaxThreadCount();
        }

        /// <inheritdoc />
        public override ScheduleItem GetNextScheduledTask(string server)
        {
			Logger.LogError(">>> Entering GetNextScheduledTask(server={Server})", server);
            Logger.LogWarning("<<< Exiting GetNextScheduledTask(server={Server})", server);
            return SchedulingController.GetNextScheduledTask(server);
        }

        /// <inheritdoc />
        public override ArrayList GetSchedule()
        {
			Logger.LogError(">>> Entering GetSchedule()");
            Logger.LogWarning("<<< Exiting GetSchedule()");
            return new ArrayList(SchedulingController.GetSchedule().ToArray());
        }

        /// <inheritdoc />
        public override ArrayList GetSchedule(string server)
        {
            return new ArrayList(SchedulingController.GetSchedule(server).ToArray());
        }

        /// <inheritdoc />
        public override ScheduleItem GetSchedule(int scheduleID)
        {
            return SchedulingController.GetSchedule(scheduleID);
        }

        /// <inheritdoc />
        public override ScheduleItem GetSchedule(string typeFullName, string server)
        {
            return SchedulingController.GetSchedule(typeFullName, server);
        }

        /// <inheritdoc />
        public override ArrayList GetScheduleHistory(int scheduleID)
        {
			Logger.LogError(">>> Entering GetScheduleHistory(scheduleID={ScheduleID})", scheduleID);
            Logger.LogWarning("<<< Exiting GetScheduleHistory(scheduleID={ScheduleID})", scheduleID);
            return new ArrayList(SchedulingController.GetScheduleHistory(scheduleID).ToArray());
        }

        /// <inheritdoc />
        public override Hashtable GetScheduleItemSettings(int scheduleID)
        {
			Logger.LogError(">>> Entering GetScheduleItemSettings(scheduleID={ScheduleID})", scheduleID);
            Logger.LogWarning("<<< Exiting GetScheduleItemSettings(scheduleID={ScheduleID})", scheduleID);
            return SchedulingController.GetScheduleItemSettings(scheduleID);
        }

        /// <inheritdoc />
        public override Collection GetScheduleProcessing()
        {
			Logger.LogError(">>> Entering GetScheduleProcessing()");
            Logger.LogWarning("<<< Exiting GetScheduleProcessing()");
            return SchedulingController.GetScheduleProcessing();
        }

        /// <inheritdoc />
        public override Collection GetScheduleQueue()
        {
			Logger.LogError(">>> Entering GetScheduleQueue()");
            Logger.LogWarning("<<< Exiting GetScheduleQueue()");
            return SchedulingController.GetScheduleQueue();
        }

        /// <inheritdoc />
        public override ScheduleStatus GetScheduleStatus()
        {
			Logger.LogError(">>> Entering GetScheduleStatus()");
            Logger.LogWarning("<<< Exiting GetScheduleStatus()");
            return SchedulingController.GetScheduleStatus();
        }

        /// <inheritdoc />
        public override void Halt(string sourceOfHalt)
        {
			Logger.LogError(">>> Entering Halt(sourceOfHalt={SourceOfHalt})", sourceOfHalt);
            Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
            Scheduler.CoreScheduler.Halt(sourceOfHalt);
            Scheduler.CoreScheduler.KeepRunning = false;
			Logger.LogWarning("<<< Exiting Halt(sourceOfHalt={SourceOfHalt})", sourceOfHalt);
        }

        /// <inheritdoc />
        public override void PurgeScheduleHistory()
        {
			Logger.LogError(">>> Entering PurgeScheduleHistory()");
            Scheduler.CoreScheduler.InitializeThreadPool(false, MaxThreads);
            Scheduler.CoreScheduler.PurgeScheduleHistory();
			Logger.LogWarning("<<< Exiting PurgeScheduleHistory()");
        }

        /// <inheritdoc />
        public override void ReStart(string sourceOfRestart)
        {
			Logger.LogError(">>> Entering ReStart(sourceOfRestart={SourceOfRestart})", sourceOfRestart);
            this.Halt(sourceOfRestart);
            this.StartAndWaitForResponse();
			Logger.LogWarning("<<< Exiting ReStart(sourceOfRestart={SourceOfRestart})", sourceOfRestart);
        }

        /// <inheritdoc />
        public override void RunEventSchedule(EventName eventName)
        {
			Logger.LogError(">>> Entering RunEventSchedule(eventName={EventName})", eventName);
            if (Enabled)
            {
                Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
                Scheduler.CoreScheduler.RunEventSchedule(eventName);
            }
			Logger.LogWarning("<<< Exiting RunEventSchedule(eventName={EventName})", eventName);
        }

        /// <inheritdoc />
        public override void RunScheduleItemNow(ScheduleItem scheduleItem, bool runNow)
        {
			Logger.LogError(">>> Entering RunScheduleItemNow(scheduleItem={ScheduleItem},runNow={RunNow})", scheduleItem, runNow);
            // If the validation for the server failed, then the server was updated. The scheduled item will be picked up on the next check, so we can exit the request for this instance to run.
            if (!Scheduler.CoreScheduler.ValidateServersAreActiveForScheduledItem(scheduleItem))
            {
                return;
            }

            // Remove item from queue
            Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);
            var scheduleHistoryItem = new ScheduleHistoryItem(scheduleItem) { NextStart = runNow ? DateTime.Now : (scheduleItem.ScheduleStartDate != Null.NullDate ? scheduleItem.ScheduleStartDate : DateTime.Now) };
			Logger.LogError("RunScheduleItemNow(scheduleItem={ScheduleItem},runNow={RunNow}): scheduleHistoryItem → {ScheduleHistoryItem}", scheduleItem, runNow, scheduleHistoryItem);

            if (scheduleHistoryItem.TimeLapse != Null.NullInteger
                && scheduleHistoryItem.TimeLapseMeasurement != Null.NullString
                && scheduleHistoryItem.Enabled
                && SchedulingController.CanRunOnThisServer(scheduleItem.Servers))
            {
                scheduleHistoryItem.ScheduleSource = ScheduleSource.STARTED_FROM_SCHEDULE_CHANGE;
				Logger.LogDebug("RunScheduleItemNow(scheduleItem={ScheduleItem},runNow={RunNow}): scheduleHistoryItem.ScheduleSource → {Arg2}", scheduleItem, runNow, scheduleHistoryItem.ScheduleSource);
                Scheduler.CoreScheduler.AddToScheduleQueue(scheduleHistoryItem);
            }

            DataCache.RemoveCache("ScheduleLastPolled");
			Logger.LogError(">>> Entering RunScheduleItemNow(scheduleItem={ScheduleItem})", scheduleItem);
			Logger.LogWarning("<<< Exiting RunScheduleItemNow(scheduleItem={ScheduleItem},runNow={RunNow})", scheduleItem, runNow);
        }

        /// <inheritdoc />
        public override void RunScheduleItemNow(ScheduleItem scheduleItem)
        {
            this.RunScheduleItemNow(scheduleItem, false);
        }

        /// <inheritdoc />
        public override void Start()
        {
			Logger.LogError(">>> Entering Start()");
            if (Enabled)
            {
                Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
                Scheduler.CoreScheduler.KeepRunning = true;
                Scheduler.CoreScheduler.KeepThreadAlive = true;
                Scheduler.CoreScheduler.Start();
            }
			Logger.LogWarning("<<< Exiting Start()");
        }

        /// <inheritdoc />
        public override void StartAndWaitForResponse()
        {
			Logger.LogError(">>> Entering StartAndWaitForResponse()");
            if (Enabled)
            {
                var newThread = new Thread(this.Start) { IsBackground = true };
				Logger.LogError("StartAndWaitForResponse(): newThread → {NewThread}", newThread);
                newThread.Start();

                // wait for up to 30 seconds for thread
                // to start up
                for (int i = 0; i <= 30; i++)
                {
                    if (this.GetScheduleStatus() != ScheduleStatus.STOPPED)
                    {
                        return;
                    }

                    Thread.Sleep(1000);
                }
            }
			Logger.LogWarning("<<< Exiting StartAndWaitForResponse()");
        }

        /// <inheritdoc />
        public override void UpdateScheduleWithoutExecution(ScheduleItem scheduleItem)
        {
			Logger.LogError(">>> Entering UpdateScheduleWithoutExecution(scheduleItem={ScheduleItem})", scheduleItem);
            SchedulingController.UpdateSchedule(scheduleItem);
			Logger.LogWarning("<<< Exiting UpdateScheduleWithoutExecution(scheduleItem={ScheduleItem})", scheduleItem);
        }

        /// <inheritdoc />
        public override void UpdateSchedule(ScheduleItem scheduleItem)
        {
			Logger.LogError(">>> Entering UpdateSchedule(scheduleItem={ScheduleItem})", scheduleItem);
            // Remove item from queue
            Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);

            // save item
            SchedulingController.UpdateSchedule(scheduleItem);

            // Update items that are already scheduled
            var futureHistory = this.GetScheduleHistory(scheduleItem.ScheduleID).Cast<ScheduleHistoryItem>().Where(h => h.NextStart > DateTime.Now);
			Logger.LogError("UpdateSchedule(scheduleItem={ScheduleItem}): futureHistory → {FutureHistory}", scheduleItem, futureHistory);

            var scheduleItemStart = scheduleItem.ScheduleStartDate > DateTime.Now
                                        ? scheduleItem.ScheduleStartDate
                                        : scheduleItem.NextStart;
            foreach (var scheduleHistoryItem in futureHistory)
            {
                scheduleHistoryItem.NextStart = scheduleItemStart;
                SchedulingController.UpdateScheduleHistory(scheduleHistoryItem);
            }

            // Add schedule to queue
            this.RunScheduleItemNow(scheduleItem);
			Logger.LogWarning("<<< Exiting UpdateSchedule(scheduleItem={ScheduleItem})", scheduleItem);
        }

        /// <inheritdoc />
        public override void RemoveFromScheduleInProgress(ScheduleItem scheduleItem)
        {
			Logger.LogError(">>> Entering RemoveFromScheduleInProgress(scheduleItem={ScheduleItem})", scheduleItem);
            // get ScheduleHistoryItem of the running task
            var runningscheduleHistoryItem = this.GetScheduleHistory(scheduleItem.ScheduleID).Cast<ScheduleHistoryItem>().ElementAtOrDefault(0);
			Logger.LogError("RemoveFromScheduleInProgress(scheduleItem={ScheduleItem}): runningscheduleHistoryItem → {RunningscheduleHistoryItem}", scheduleItem, runningscheduleHistoryItem);
            Scheduler.CoreScheduler.StopScheduleInProgress(scheduleItem, runningscheduleHistoryItem);
			Logger.LogWarning("<<< Exiting RemoveFromScheduleInProgress(scheduleItem={ScheduleItem})", scheduleItem);
        }
    }
}
