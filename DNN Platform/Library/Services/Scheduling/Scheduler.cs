// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;
    using Microsoft.VisualBasic;

    internal static class Scheduler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Scheduler));

        internal static class CoreScheduler
        {
            // If KeepRunning gets switched to false,
            // the scheduler stops running.
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public static bool KeepThreadAlive = true;

            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
            [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
            public static bool KeepRunning = true;

            private static readonly SharedList<ScheduleItem> ScheduleQueue;
            private static readonly SharedList<ScheduleHistoryItem> ScheduleInProgress;
            private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(45);
            private static readonly ReaderWriterLockSlim StatusLock = new ReaderWriterLockSlim();
            private static readonly char[] ServerSeparator = [',',];

            // This is the heart of the scheduler mechanism.
            // This class manages running new events according
            // to the schedule.
            //
            // This class can also react to the three
            // scheduler events (Started, Progressing and Completed)
            private static bool threadPoolInitialized;

            // The MaxThreadCount establishes the maximum
            // threads you want running simultaneously
            // for spawning SchedulerClient processes
            private static int maxThreadCount;
            private static int activeThreadCount;

            // If KeepRunning gets switched to false,
            // the scheduler stops running.
            private static bool forceReloadSchedule;
            private static bool debug;

            private static int numberOfProcessGroups;

            // This is our array that holds the process group
            // where our threads will be kicked off.
            private static ProcessGroup[] processGroup;

            // A ReaderWriterLockSlim will protect our objects
            // in memory from being corrupted by simultaneous
            // thread operations.  This block of code below
            // establishes variables to help keep track
            // of the ReaderWriter locks.
            private static int readerTimeouts;
            private static int writerTimeouts;
            private static ScheduleStatus status = ScheduleStatus.STOPPED;

            static CoreScheduler()
            {
				Logger.LogError(">>> Entering CoreScheduler()");
                var lockStrategy = new ReaderWriterLockStrategy(LockRecursionPolicy.SupportsRecursion);

				Logger.LogError("CoreScheduler(): lockStrategy → {LockStrategy}", lockStrategy);
                ScheduleQueue = new SharedList<ScheduleItem>(lockStrategy);
				Logger.LogError("CoreScheduler(): ScheduleQueue → {ScheduleQueue}", ScheduleQueue);
                ScheduleInProgress = new SharedList<ScheduleHistoryItem>(lockStrategy);
				Logger.LogError("CoreScheduler(): ScheduleInProgress → {ScheduleInProgress}", ScheduleInProgress);
            }

            private delegate void AddToScheduleInProgressDelegate(ScheduleHistoryItem item);

            /// <summary>Gets tracks how many threads we have free to work with at any given time.</summary>
            public static int FreeThreads
            {
                get
                {
                    return maxThreadCount - activeThreadCount;
                }
            }

            public static ScheduleHistoryItem AddScheduleHistory(ScheduleHistoryItem scheduleHistoryItem)
            {
				Logger.LogError(">>> Entering AddScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                try
                {
                    int scheduleHistoryID = SchedulingController.AddScheduleHistory(scheduleHistoryItem);
					Logger.LogError("AddScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem}): scheduleHistoryID → {ScheduleHistoryID}", scheduleHistoryItem, scheduleHistoryID);
                    scheduleHistoryItem.ScheduleHistoryID = scheduleHistoryID;
					Logger.LogDebug("AddScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem}): scheduleHistoryItem.ScheduleHistoryID → {Arg1}", scheduleHistoryItem, scheduleHistoryItem.ScheduleHistoryID);
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in AddScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem}): {Arg1}", scheduleHistoryItem, exc.Message);
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }

				Logger.LogWarning("<<< Exiting AddScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                return scheduleHistoryItem;
            }

            /// <summary>
            /// Checks the scheduled item if it has servers specified to run on. If it does, then it checks to see if the server is active.
            /// </summary>
            /// <param name="scheduleItem">The schedule to check.</param>
            /// <returns>True if nothing happens to the scheduled item. Returns false if the scheduled item was updated.</returns>
            /// <remarks>
            /// If the server specified is not active then the scheduled item will be assigned to the most recent active server.
            /// This assumes that all schedules will run on a single server if none of the servers specified are active.
            /// </remarks>
            public static bool ValidateServersAreActiveForScheduledItem(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering ValidateServersAreActiveForScheduledItem(scheduleItem={ScheduleItem})", scheduleItem);
                // If this scheduled items runs on all servers, continue.
                if (string.IsNullOrEmpty(scheduleItem.Servers))
                {
                    return true;
                }

                // Get the individual server names from the scheduled item
                var servers = scheduleItem.Servers.ToLowerInvariant().Split(ServerSeparator, StringSplitOptions.RemoveEmptyEntries);
				Logger.LogError("ValidateServersAreActiveForScheduledItem(scheduleItem={ScheduleItem}): servers → {Servers}", scheduleItem, servers);

                // Get the enabled servers with recent activity.
                var enabledServers = ServerController.GetEnabledServersWithActivity();
				Logger.LogError("ValidateServersAreActiveForScheduledItem(scheduleItem={ScheduleItem}): enabledServers → {EnabledServers}", scheduleItem, enabledServers);

                // Validate that any server in the scheduled item is in the enabled server list with activity in the last 10 minutes.
                bool serverHasRecentActivity = enabledServers.Any(i => servers.Contains(i.ServerName.ToLowerInvariant()));

                // If no servers in the scheduled item had recent activity, assign this scheduled item to the server with the most recent activity
                if (!serverHasRecentActivity)
                {
                    // Assign a new server to this task then run it
                    scheduleItem.Servers = $",{enabledServers.First().ServerName.ToLowerInvariant()},";

                    // Save the scheduled item back to the database.
                    SchedulingController.UpdateSchedule(scheduleItem);
                    return false;
                }

				Logger.LogWarning("<<< Exiting ValidateServersAreActiveForScheduledItem(scheduleItem={ScheduleItem})", scheduleItem);
                return true;
            }

            /// <summary>Adds an item to the collection of schedule items in queue.</summary>
            /// <param name="scheduleHistoryItem">The schedule history item.</param>
            /// <remarks>Thread Safe.</remarks>
            public static void AddToScheduleQueue(ScheduleHistoryItem scheduleHistoryItem)
            {
				Logger.LogError(">>> Entering AddToScheduleQueue(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                if (!ScheduleQueueContains(scheduleHistoryItem))
                {
                    try
                    {
                        ////objQueueReadWriteLock.EnterWriteLock(WriteTimeout)
                        using (ScheduleQueue.GetWriteLock(LockTimeout))
                        {
                            // Do a second check just in case
                            if (!ScheduleQueueContains(scheduleHistoryItem) &&
                                !IsInProgress(scheduleHistoryItem) &&
                                ValidateServersAreActiveForScheduledItem(scheduleHistoryItem))
                            {
                                // It is safe for this thread to read or write
                                // from the shared resource.
                                ScheduleQueue.Add(scheduleHistoryItem);
                            }
                        }
                    }
                    catch (ApplicationException ex)
                    {
						Logger.LogError("Exception in AddToScheduleQueue(scheduleHistoryItem={ScheduleHistoryItem}): {Arg1}", scheduleHistoryItem, ex.Message);
                        // The writer lock request timed out.
                        Interlocked.Increment(ref writerTimeouts);
                        Exceptions.Exceptions.LogException(ex);
                    }
                }
				Logger.LogWarning("<<< Exiting AddToScheduleQueue(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
            }

            public static void FireEvents()
            {
				Logger.LogError(">>> Entering FireEvents()");
                // This method uses a thread pool to
                // call the SchedulerClient methods that need
                // to be called.

                // For each item in the queue that there
                // is an open thread for, set the object
                // in the array to a new ProcessGroup object.
                // Pass in the ScheduleItem to the ProcessGroup
                // so the ProcessGroup can pass it around for
                // logging and notifications.
                lock (ScheduleQueue)
                {
                    var scheduleList = new List<ScheduleItem>();
					Logger.LogError("FireEvents(): scheduleList → {ScheduleList}", scheduleList);
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        foreach (ScheduleItem scheduleItem in ScheduleQueue)
                        {
                            scheduleList.Add(scheduleItem);
                        }
                    }

                    int numToRun = scheduleList.Count;
                    int numRun = 0;

                    foreach (ScheduleItem scheduleItem in scheduleList)
                    {
                        if (!KeepRunning)
                        {
                            return;
                        }

                        int processGroup = GetProcessGroup();
						Logger.LogError("FireEvents(): processGroup → {ProcessGroup}", processGroup);

                        if (scheduleItem.NextStart <= DateTime.Now &&
                            scheduleItem.Enabled &&
                            !IsInProgress(scheduleItem) &&
                            !HasDependenciesConflict(scheduleItem) &&
                            numRun < numToRun)
                        {
                            scheduleItem.ProcessGroup = processGroup;
							Logger.LogDebug("FireEvents(): scheduleItem.ProcessGroup → {Arg0}", scheduleItem.ProcessGroup);
                            if (scheduleItem.ScheduleSource == ScheduleSource.NOT_SET)
                            {
                                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                                {
                                    scheduleItem.ScheduleSource = ScheduleSource.STARTED_FROM_TIMER;
									Logger.LogDebug("FireEvents(): scheduleItem.ScheduleSource → {Arg0}", scheduleItem.ScheduleSource);
                                }
                                else if (SchedulingProvider.SchedulerMode == SchedulerMode.REQUEST_METHOD)
                                {
                                    scheduleItem.ScheduleSource = ScheduleSource.STARTED_FROM_BEGIN_REQUEST;
                                }
                            }

                            var delegateFunc = new AddToScheduleInProgressDelegate(AddToScheduleInProgress);
							Logger.LogError("FireEvents(): delegateFunc → {DelegateFunc}", delegateFunc);
                            var scheduleHistoryItem = new ScheduleHistoryItem(scheduleItem);
							Logger.LogError("FireEvents(): scheduleHistoryItem → {ScheduleHistoryItem}", scheduleHistoryItem);
                            scheduleHistoryItem.StartDate = DateTime.Now;
							Logger.LogDebug("FireEvents(): scheduleHistoryItem.StartDate → {Arg0}", scheduleHistoryItem.StartDate);
                            delegateFunc.BeginInvoke(scheduleHistoryItem, null, null);
                            Thread.Sleep(1000);

                            CoreScheduler.processGroup[processGroup].AddQueueUserWorkItem(scheduleItem);

                            LogEventAddedToProcessGroup(scheduleItem);
                            numRun += 1;
                        }
                        else
                        {
                            LogWhyTaskNotRun(scheduleItem);
                        }
                    }
                }
				Logger.LogWarning("<<< Exiting FireEvents()");
            }

            public static int GetActiveThreadCount()
            {
				Logger.LogError(">>> Entering GetActiveThreadCount()");
                Logger.LogWarning("<<< Exiting GetActiveThreadCount()");
                return activeThreadCount;
            }

            public static int GetFreeThreadCount()
            {
				Logger.LogError(">>> Entering GetFreeThreadCount()");
                Logger.LogWarning("<<< Exiting GetFreeThreadCount()");
                return FreeThreads;
            }

            public static int GetMaxThreadCount()
            {
				Logger.LogError(">>> Entering GetMaxThreadCount()");
                Logger.LogWarning("<<< Exiting GetMaxThreadCount()");
                return maxThreadCount;
            }

            /// <summary>Gets a copy of the collection of schedule items in progress.</summary>
            /// <returns>Copy of the schedule items currently in progress.</returns>
            /// <remarks>This is a snapshot of the collection scheduled items could start or complete at any time.</remarks>
            public static Collection GetScheduleInProgress()
            {
				Logger.LogError(">>> Entering GetScheduleInProgress()");
                var c = new Collection();
				Logger.LogError("GetScheduleInProgress(): c → {C}", c);
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        foreach (ScheduleHistoryItem item in ScheduleInProgress)
                        {
                            c.Add(item, item.ScheduleID.ToString(CultureInfo.InvariantCulture), null, null);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in GetScheduleInProgress(): {Arg0}", ex.Message);
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }

				Logger.LogWarning("<<< Exiting GetScheduleInProgress()");
                return c;
            }

            /// <summary>Gets the number of items in the collection of schedule items in progress.</summary>
            /// <returns>Number of items in progress.</returns>
            /// <remarks>Thread Safe
            /// This count is a snapshot and may change at any time.
            /// </remarks>
            public static int GetScheduleInProgressCount()
            {
				Logger.LogError(">>> Entering GetScheduleInProgressCount()");
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        return ScheduleInProgress.Count;
                    }
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(ex);
                    }

					Logger.LogWarning("<<< Exiting GetScheduleInProgressCount()");
                    return 0;
                }
            }

            /// <summary>Gets a copy of collection of all schedule items in queue.</summary>
            /// <returns>A copy of the ScheduleQueue.</returns>
            /// <remarks>Thread Safe
            /// The returned collection is a snapshot in time the real ScheduleQueue may change at any time.
            /// </remarks>
            public static Collection GetScheduleQueue()
            {
				Logger.LogError(">>> Entering GetScheduleQueue()");
                var c = new Collection();
				Logger.LogError("GetScheduleQueue(): c → {C}", c);
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        foreach (ScheduleItem item in ScheduleQueue)
                        {
                            c.Add(item, item.ScheduleID.ToString(CultureInfo.InvariantCulture), null, null);
                        }
                    }

                    return c;
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in GetScheduleQueue(): {Arg0}", ex.Message);
                    Interlocked.Increment(ref readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }

				Logger.LogWarning("<<< Exiting GetScheduleQueue()");
                return c;
            }

            /// <summary>Gets the number of items in the collection of schedule items in progress.</summary>
            /// <returns>Number of items in progress.</returns>
            /// <remarks>Thread Safe
            /// This count is a snapshot and may change at any time.
            /// </remarks>
            public static int GetScheduleQueueCount()
            {
				Logger.LogError(">>> Entering GetScheduleQueueCount()");
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        return ScheduleQueue.Count;
                    }
                }
                catch (ApplicationException)
                {
					Logger.LogError("Exception in GetScheduleQueueCount(): {Arg0}", ApplicationException.Message);
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
					Logger.LogWarning("<<< Exiting GetScheduleQueueCount()");
                    return 0;
                }
            }

            public static ScheduleStatus GetScheduleStatus()
            {
				Logger.LogError(">>> Entering GetScheduleStatus()");
                try
                {
                    if (StatusLock.TryEnterReadLock(LockTimeout))
                    {
                        try
                        {
                            // ScheduleStatus is a value type a copy is returned (enumeration)
                            return status;
                        }
                        finally
                        {
                            StatusLock.ExitReadLock();
                        }
                    }
                }
                catch (ApplicationException)
                {
					Logger.LogError("Exception in GetScheduleStatus(): {Arg0}", ApplicationException.Message);
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
                }

				Logger.LogWarning("<<< Exiting GetScheduleStatus()");
                return ScheduleStatus.NOT_SET;
            }

            /// <summary>Halt the Scheduler.</summary>
            /// <param name="sourceOfHalt">Initiator of Halt.</param>
            public static void Halt(string sourceOfHalt)
            {
				Logger.LogError(">>> Entering Halt(sourceOfHalt={SourceOfHalt})", sourceOfHalt);
                // should do nothing if the scheduler havn't start yet.
                var currentStatus = GetScheduleStatus();
				Logger.LogError("Halt(sourceOfHalt={SourceOfHalt}): currentStatus → {CurrentStatus}", sourceOfHalt, currentStatus);
                if (currentStatus == ScheduleStatus.NOT_SET || currentStatus == ScheduleStatus.STOPPED)
                {
                    return;
                }

                SetScheduleStatus(ScheduleStatus.SHUTTING_DOWN);
                var log = new LogInfo { LogTypeKey = "SCHEDULER_SHUTTING_DOWN" };
				Logger.LogError("Halt(sourceOfHalt={SourceOfHalt}): log → {Log}", sourceOfHalt, log);
                log.AddProperty("Initiator", sourceOfHalt);
                LogController.Instance.AddLog(log);

                KeepRunning = false;
				Logger.LogDebug("Halt(sourceOfHalt={SourceOfHalt}): KeepRunning → {KeepRunning}", sourceOfHalt, KeepRunning);

                // wait for up to 120 seconds for thread
                // to shut down
                for (int i = 0; i <= 120; i++)
                {
                    if (GetScheduleStatus() == ScheduleStatus.STOPPED)
                    {
                        return;
                    }

                    Thread.Sleep(1000);
                }

                activeThreadCount = 0;
				Logger.LogDebug("Halt(sourceOfHalt={SourceOfHalt}): activeThreadCount → {ActiveThreadCount}", sourceOfHalt, activeThreadCount);
				Logger.LogWarning("<<< Exiting Halt(sourceOfHalt={SourceOfHalt})", sourceOfHalt);
            }

            public static bool HasDependenciesConflict(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering HasDependenciesConflict(scheduleItem={ScheduleItem})", scheduleItem);
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        if (scheduleItem.ObjectDependencies.Length != 0)
                        {
                            foreach (ScheduleHistoryItem item in ScheduleInProgress.Where(si => si.ObjectDependencies.Length != 0))
                            {
                                if (item.HasObjectDependencies(scheduleItem.ObjectDependencies))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(ex);
                    }

				Logger.LogWarning("<<< Exiting HasDependenciesConflict(scheduleItem={ScheduleItem})", scheduleItem);
                    return false;
                }
            }

            public static void LoadQueueFromEvent(EventName eventName)
            {
				Logger.LogError(">>> Entering LoadQueueFromEvent(eventName={EventName})", eventName);
                var executingServer = ServerController.GetExecutingServerName();
                List<ScheduleItem> schedule = SchedulingController.GetScheduleByEvent(eventName.ToString(), executingServer);
				Logger.LogError("LoadQueueFromEvent(eventName={EventName}): executingServer → {ExecutingServer}", eventName, executingServer);
                if (Logger.IsDebugEnabled)
                {
					Logger.LogError("LoadQueueFromEvent(eventName={EventName}): schedule → {Schedule}", eventName, schedule);
                    Logger.Debug("loadqueue executingServer:" + executingServer);
                }

                var thisServer = GetServer(executingServer);
				Logger.LogError("LoadQueueFromEvent(eventName={EventName}): thisServer → {ThisServer}", eventName, thisServer);
                if (thisServer == null)
                {
                    return;
                }

                bool runningInAGroup = !string.IsNullOrEmpty(thisServer.ServerGroup);

                var serverGroupServers = ServerGroupServers(thisServer);
				Logger.LogError("LoadQueueFromEvent(eventName={EventName}): serverGroupServers → {ServerGroupServers}", eventName, serverGroupServers);

                foreach (ScheduleItem scheduleItem in schedule)
                {
                    if (runningInAGroup && string.IsNullOrEmpty(scheduleItem.Servers))
                    {
                        scheduleItem.Servers = serverGroupServers;
						Logger.LogDebug("LoadQueueFromEvent(eventName={EventName}): scheduleItem.Servers → {Arg1}", eventName, scheduleItem.Servers);
                    }

                    var historyItem = new ScheduleHistoryItem(scheduleItem);
					Logger.LogError("LoadQueueFromEvent(eventName={EventName}): historyItem → {HistoryItem}", eventName, historyItem);

                    if (!IsInQueue(historyItem) &&
                        !IsInProgress(historyItem) &&
                        !HasDependenciesConflict(historyItem) &&
                        historyItem.Enabled)
                    {
                        historyItem.ScheduleSource = ScheduleSource.STARTED_FROM_EVENT;
						Logger.LogDebug("LoadQueueFromEvent(eventName={EventName}): historyItem.ScheduleSource → {Arg1}", eventName, historyItem.ScheduleSource);
                        AddToScheduleQueue(historyItem);
                    }
                }
				Logger.LogWarning("<<< Exiting LoadQueueFromEvent(eventName={EventName})", eventName);
            }

            public static void LoadQueueFromTimer()
            {
				Logger.LogError(">>> Entering LoadQueueFromTimer()");
                forceReloadSchedule = false;
                var executingServer = ServerController.GetExecutingServerName();
				Logger.LogError("LoadQueueFromTimer(): executingServer → {ExecutingServer}", executingServer);
				Logger.LogDebug("LoadQueueFromTimer(): forceReloadSchedule → {ForceReloadSchedule}", forceReloadSchedule);
                List<ScheduleItem> schedule = SchedulingController.GetSchedule(executingServer);
                if (Logger.IsDebugEnabled)
                {
					Logger.LogError("LoadQueueFromTimer(): schedule → {Schedule}", schedule);
                    Logger.Debug("LoadQueueFromTimer executingServer:" + executingServer);
                }

                var thisServer = GetServer(executingServer);
				Logger.LogError("LoadQueueFromTimer(): thisServer → {ThisServer}", thisServer);
                if (thisServer == null)
                {
                    return;
                }

                bool runningInAGroup = !string.IsNullOrEmpty(thisServer.ServerGroup);

                var serverGroupServers = ServerGroupServers(thisServer);
				Logger.LogError("LoadQueueFromTimer(): serverGroupServers → {ServerGroupServers}", serverGroupServers);

                foreach (ScheduleItem scheduleItem in schedule)
                {
                    if (runningInAGroup && string.IsNullOrEmpty(scheduleItem.Servers))
                    {
                        scheduleItem.Servers = serverGroupServers;
						Logger.LogDebug("LoadQueueFromTimer(): scheduleItem.Servers → {Arg0}", scheduleItem.Servers);
                    }

                    var historyItem = new ScheduleHistoryItem(scheduleItem);
					Logger.LogError("LoadQueueFromTimer(): historyItem → {HistoryItem}", historyItem);

                    if (!IsInQueue(historyItem) &&
                        historyItem.TimeLapse != Null.NullInteger &&
                        historyItem.TimeLapseMeasurement != Null.NullString &&
                        historyItem.Enabled)
                    {
                        if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                        {
                            historyItem.ScheduleSource = ScheduleSource.STARTED_FROM_TIMER;
							Logger.LogDebug("LoadQueueFromTimer(): historyItem.ScheduleSource → {Arg0}", historyItem.ScheduleSource);
                        }
                        else if (SchedulingProvider.SchedulerMode == SchedulerMode.REQUEST_METHOD)
                        {
                            historyItem.ScheduleSource = ScheduleSource.STARTED_FROM_BEGIN_REQUEST;
                        }

                        AddToScheduleQueue(historyItem);
                    }
                }
				Logger.LogWarning("<<< Exiting LoadQueueFromTimer()");
            }

            public static void PurgeScheduleHistory()
            {
				Logger.LogError(">>> Entering PurgeScheduleHistory()");
                SchedulingController.PurgeScheduleHistory();
				Logger.LogWarning("<<< Exiting PurgeScheduleHistory()");
            }

            public static void ReloadSchedule()
            {
				Logger.LogError(">>> Entering ReloadSchedule()");
                forceReloadSchedule = true;
				Logger.LogDebug("ReloadSchedule(): forceReloadSchedule → {ForceReloadSchedule}", forceReloadSchedule);
            Logger.LogWarning("<<< Exiting ReloadSchedule()");
            }

            /// <summary>Removes an item from the collection of schedule items in queue.</summary>
            /// <param name="scheduleItem">Item to remove.</param>
            public static void RemoveFromScheduleQueue(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering RemoveFromScheduleQueue(scheduleItem={ScheduleItem})", scheduleItem);
                try
                {
                    using (ScheduleQueue.GetWriteLock(LockTimeout))
                    {
                        // the scheduleitem instances may not be equal even though the scheduleids are equal
                        var item = ScheduleQueue.FirstOrDefault(si => si.ScheduleID == scheduleItem.ScheduleID);
						Logger.LogError("RemoveFromScheduleQueue(scheduleItem={ScheduleItem}): item → {Item}", scheduleItem, item);
                        if (item != null)
                        {
                            ScheduleQueue.Remove(item);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in RemoveFromScheduleQueue(scheduleItem={ScheduleItem}): {Arg1}", scheduleItem, ex.Message);
                    // The writer lock request timed out.
                    Interlocked.Increment(ref writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }
				Logger.LogWarning("<<< Exiting RemoveFromScheduleQueue(scheduleItem={ScheduleItem})", scheduleItem);
            }

            public static void RunEventSchedule(EventName eventName)
            {
				Logger.LogError(">>> Entering RunEventSchedule(eventName={EventName})", eventName);
                try
                {
                    var log = new LogInfo { LogTypeKey = "SCHEDULE_FIRED_FROM_EVENT" };
					Logger.LogError("RunEventSchedule(eventName={EventName}): log → {Log}", eventName, log);
                    log.AddProperty("EVENT", eventName.ToString());
                    LogController.Instance.AddLog(log);

                    // We allow for three threads to run simultaneously.
                    // As long as we have an open thread, continue.

                    // Load the queue to determine which schedule
                    // items need to be run.
                    LoadQueueFromEvent(eventName);

                    while (GetScheduleQueueCount() > 0)
                    {
                        SetScheduleStatus(ScheduleStatus.RUNNING_EVENT_SCHEDULE);

                        // Fire off the events that need running.
                        if (GetScheduleQueueCount() > 0)
                        {
                            FireEvents();
                        }

                        if (writerTimeouts > 20 || readerTimeouts > 20)
                        {
                            // Wait for 10 minutes so we don't fill up the logs
                            Thread.Sleep(TimeSpan.FromMinutes(10));
                        }
                        else
                        {
                            // Wait for 10 seconds to avoid cpu overutilization
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }

                        if (GetScheduleQueueCount() == 0)
                        {
							Logger.LogWarning("<<< Exiting RunEventSchedule(eventName={EventName})", eventName);
                            return;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void SetScheduleStatus(ScheduleStatus newStatus)
            {
				Logger.LogError(">>> Entering SetScheduleStatus(newStatus={NewStatus})", newStatus);
                try
                {
                    // note:locking inside this method is highly misleading
                    // as there is no lock in place between when the caller
                    // decides to call this method and when the lock is acquired
                    // the value could easily change in that time
                    if (StatusLock.TryEnterWriteLock(LockTimeout))
                    {
                        try
                        {
                            // It is safe for this thread to read or write
                            // from the shared resource.
                            status = newStatus;
							Logger.LogError("SetScheduleStatus(newStatus={NewStatus}): status → {Status}", newStatus, status);
                        }
                        finally
                        {
							Logger.LogDebug("SetScheduleStatus(newStatus={NewStatus}): status → {Status}", newStatus, status);
                            // Ensure that the lock is released.
                            StatusLock.ExitWriteLock();
                        }
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in SetScheduleStatus(newStatus={NewStatus}): {Arg1}", newStatus, ex.Message);
                    // The writer lock request timed out.
                    Interlocked.Increment(ref writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }
				Logger.LogWarning("<<< Exiting SetScheduleStatus(newStatus={NewStatus})", newStatus);
            }

            public static void Start()
            {
				Logger.LogError(">>> Entering Start()");
                try
                {
                    AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

                    activeThreadCount = 0;
					Logger.LogDebug("Start(): activeThreadCount → {ActiveThreadCount}", activeThreadCount);

                    // This is where the action begins.
                    ////Loop until KeepRunning = false
                    if (SchedulingProvider.SchedulerMode != SchedulerMode.REQUEST_METHOD || debug)
                    {
                        var log = new LogInfo();
						Logger.LogError("Start(): log → {Log}", log);
                        log.LogTypeKey = "SCHEDULER_STARTED";
						Logger.LogDebug("Start(): log.LogTypeKey → {Arg0}", log.LogTypeKey);
                        LogController.Instance.AddLog(log);
                    }

                    while (KeepRunning)
                    {
                        try
                        {
                            if (Common.Globals.ElapsedSinceAppStart.TotalSeconds < SchedulingProvider.DelayAtAppStart)
                            {
                                if (!KeepThreadAlive)
                                {
                                    return;
                                }

                                Thread.Sleep(1000);
                                continue;
                            }

                            if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                            {
                                SetScheduleStatus(ScheduleStatus.RUNNING_TIMER_SCHEDULE);
                            }
                            else
                            {
                                SetScheduleStatus(ScheduleStatus.RUNNING_REQUEST_SCHEDULE);
                            }

                            // Load the queue to determine which schedule
                            // items need to be run.
                            LoadQueueFromTimer();

                            // Keep track of when the queue was last refreshed
                            // so we can perform a refresh periodically
                            DateTime lastQueueRefresh = DateTime.Now;
                            bool refreshQueueSchedule = false;
							Logger.LogError("Start(): refreshQueueSchedule → {RefreshQueueSchedule}", refreshQueueSchedule);

                            // We allow for [MaxThreadCount] threads to run
                            // simultaneously.  As long as we have an open thread
                            // and we don't have to refresh the queue, continue
                            // to loop.
                            // refreshQueueSchedule can get set to true near bottom of loop
                            // not sure why R# thinks it is always false
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            while (FreeThreads > 0 && !refreshQueueSchedule && KeepRunning && !forceReloadSchedule)
                            {
                                // Fire off the events that need running.
                                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                                {
                                    SetScheduleStatus(ScheduleStatus.RUNNING_TIMER_SCHEDULE);
                                }
                                else
                                {
                                    SetScheduleStatus(ScheduleStatus.RUNNING_REQUEST_SCHEDULE);
                                }

                                // It is safe for this thread to read from
                                // the shared resource.
                                if (GetScheduleQueueCount() > 0)
                                {
                                    FireEvents();
                                }

                                if (KeepThreadAlive == false)
                                {
                                    return;
                                }

                                if (writerTimeouts > 20 || readerTimeouts > 20)
                                {
                                    // Some kind of deadlock on a resource.
                                    // Wait for 10 minutes so we don't fill up the logs
                                    if (KeepRunning)
                                    {
                                        Thread.Sleep(TimeSpan.FromMinutes(10));
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    // Wait for 10 seconds to avoid cpu overutilization
                                    if (KeepRunning)
                                    {
                                        Thread.Sleep(TimeSpan.FromSeconds(10));
                                    }
                                    else
                                    {
                                        return;
                                    }

                                    // Refresh queue from database every 10 minutes
                                    // if there are no items currently in progress
                                    if ((lastQueueRefresh.AddMinutes(10) <= DateTime.Now || forceReloadSchedule) && FreeThreads == maxThreadCount)
                                    {
                                        refreshQueueSchedule = true;
                                        break;
                                    }
                                }
                            }

                            // There are no available threads, all threads are being
                            // used.  Wait 10 seconds until one is available
                            if (KeepRunning)
                            {
                                if (refreshQueueSchedule == false)
                                {
                                    SetScheduleStatus(ScheduleStatus.WAITING_FOR_OPEN_THREAD);
                                    Thread.Sleep(10000); // sleep for 10 seconds
                                }
                            }
                            else
                            {
								Logger.LogWarning("<<< Exiting Start()");
                                return;
                            }
                        }
                        catch (Exception exc)
                        {
                            Exceptions.Exceptions.ProcessSchedulerException(exc);

                            // sleep for 10 minutes
                            Thread.Sleep(600000);
                        }
                    }
                }
                finally
                {
                    if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD || SchedulingProvider.SchedulerMode == SchedulerMode.DISABLED)
                    {
                        SetScheduleStatus(ScheduleStatus.STOPPED);
                    }
                    else
                    {
                        SetScheduleStatus(ScheduleStatus.WAITING_FOR_REQUEST);
                    }

                    if (SchedulingProvider.SchedulerMode != SchedulerMode.REQUEST_METHOD || debug)
                    {
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_STOPPED" };
                        LogController.Instance.AddLog(log);
                    }
                }
            }

            public static void UpdateScheduleHistory(ScheduleHistoryItem scheduleHistoryItem)
            {
				Logger.LogError(">>> Entering UpdateScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                try
                {
                    SchedulingController.UpdateScheduleHistory(scheduleHistoryItem);
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in UpdateScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem}): {Arg1}", scheduleHistoryItem, exc.Message);
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
				Logger.LogWarning("<<< Exiting UpdateScheduleHistory(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
            }

            public static void WorkCompleted(SchedulerClient schedulerClient)
            {
				Logger.LogError(">>> Entering WorkCompleted(schedulerClient={SchedulerClient})", schedulerClient);
                try
                {
                    ScheduleHistoryItem scheduleHistoryItem = schedulerClient.ScheduleHistoryItem;

                    // Remove the object in the ScheduleInProgress collection
                    RemoveFromScheduleInProgress(scheduleHistoryItem);

                    // A SchedulerClient is notifying us that their
                    // process has completed.  Decrease our ActiveThreadCount
                    Interlocked.Decrement(ref activeThreadCount);

                    // Update the schedule item object property
                    // to note the end time and next start
                    scheduleHistoryItem.EndDate = DateTime.Now;
					Logger.LogDebug("WorkCompleted(schedulerClient={SchedulerClient}): scheduleHistoryItem.EndDate → {Arg1}", schedulerClient, scheduleHistoryItem.EndDate);

                    if (scheduleHistoryItem.ScheduleSource == ScheduleSource.STARTED_FROM_EVENT)
                    {
                        scheduleHistoryItem.NextStart = Null.NullDate;
						Logger.LogDebug("WorkCompleted(schedulerClient={SchedulerClient}): scheduleHistoryItem.NextStart → {Arg1}", schedulerClient, scheduleHistoryItem.NextStart);
                    }
                    else
                    {
                        if (scheduleHistoryItem.CatchUpEnabled)
                        {
                            switch (scheduleHistoryItem.TimeLapseMeasurement)
                            {
                                case "s":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddSeconds(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "m":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddMinutes(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "h":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddHours(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "d":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "w":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                    break;
                                case "mo":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "y":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.TimeLapse);
                                    break;
                            }
                        }
                        else
                        {
                            switch (scheduleHistoryItem.TimeLapseMeasurement)
                            {
                                case "s":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddSeconds(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "m":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddMinutes(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "h":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddHours(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "d":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "w":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                    break;
                                case "mo":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddMonths(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "y":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddYears(scheduleHistoryItem.TimeLapse);
                                    break;
                            }
                        }
                    }

                    // Update the ScheduleHistory in the database
                    UpdateScheduleHistory(scheduleHistoryItem);

                    if (scheduleHistoryItem.NextStart != Null.NullDate)
                    {
                        // Put the object back into the ScheduleQueue
                        // collection with the new NextStart date.
                        scheduleHistoryItem.StartDate = Null.NullDate;
						Logger.LogDebug("WorkCompleted(schedulerClient={SchedulerClient}): scheduleHistoryItem.StartDate → {Arg1}", schedulerClient, scheduleHistoryItem.StartDate);
                        scheduleHistoryItem.EndDate = Null.NullDate;
                        scheduleHistoryItem.LogNotes = string.Empty;
						Logger.LogDebug("WorkCompleted(schedulerClient={SchedulerClient}): scheduleHistoryItem.LogNotes → {Arg1}", schedulerClient, scheduleHistoryItem.LogNotes);
                        scheduleHistoryItem.ProcessGroup = -1;
						Logger.LogDebug("WorkCompleted(schedulerClient={SchedulerClient}): scheduleHistoryItem.ProcessGroup → {Arg1}", schedulerClient, scheduleHistoryItem.ProcessGroup);
                        AddToScheduleQueue(scheduleHistoryItem);
                    }

                    if (schedulerClient.ScheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_COMPLETED" };
						Logger.LogError("WorkCompleted(schedulerClient={SchedulerClient}): log → {Log}", schedulerClient, log);
                        log.AddProperty("TYPE", schedulerClient.GetType().FullName);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("NEXT START", Convert.ToString(scheduleHistoryItem.NextStart, CultureInfo.InvariantCulture));
                        log.AddProperty("SOURCE", schedulerClient.ScheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", activeThreadCount.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("FREE THREADS", FreeThreads.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("READER TIMEOUTS", readerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("WRITER TIMEOUTS", writerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString(CultureInfo.InvariantCulture));
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in WorkCompleted(schedulerClient={SchedulerClient}): {Arg1}", schedulerClient, exc.Message);
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
				Logger.LogWarning("<<< Exiting WorkCompleted(schedulerClient={SchedulerClient})", schedulerClient);
            }

            public static void WorkErrored(SchedulerClient schedulerClient, Exception exception)
            {
				Logger.LogError(">>> Entering WorkErrored(schedulerClient={SchedulerClient},exception={Exception})", schedulerClient, exception);
                WorkErrored(schedulerClient.ScheduleHistoryItem, exception);
				Logger.LogWarning("<<< Exiting WorkErrored(schedulerClient={SchedulerClient},exception={Exception})", schedulerClient, exception);
            }

            public static void WorkErrored(ScheduleHistoryItem scheduleHistoryItem, Exception exception)
            {
				Logger.LogError(">>> Entering WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception})", scheduleHistoryItem, exception);
                try
                {
					Logger.LogWarning("<<< Exiting WorkErrored(schedulerClient={SchedulerClient},exception={Exception})", schedulerClient, exception);
                    // Remove the object in the ScheduleInProgress collection
                    RemoveFromScheduleInProgress(scheduleHistoryItem);

                    // A SchedulerClient is notifying us that their
                    // process has errored.  Decrease our ActiveThreadCount
                    Interlocked.Decrement(ref activeThreadCount);

                    Exceptions.Exceptions.ProcessSchedulerException(exception);

                    // Update the schedule item object property
                    // to note the end time and next start
                    scheduleHistoryItem.EndDate = DateTime.Now;
					Logger.LogDebug("WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): scheduleHistoryItem.EndDate → {Arg2}", scheduleHistoryItem, exception, scheduleHistoryItem.EndDate);
                    if (scheduleHistoryItem.ScheduleSource == ScheduleSource.STARTED_FROM_EVENT)
                    {
                        scheduleHistoryItem.NextStart = Null.NullDate;
						Logger.LogDebug("WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): scheduleHistoryItem.NextStart → {Arg2}", scheduleHistoryItem, exception, scheduleHistoryItem.NextStart);
                    }
                    else if (scheduleHistoryItem.RetryTimeLapse != Null.NullInteger)
                    {
                        switch (scheduleHistoryItem.RetryTimeLapseMeasurement)
                        {
                            case "s":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddSeconds(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "m":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMinutes(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "h":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddHours(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "d":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "w":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.RetryTimeLapse * 7);
                                break;
                            case "mo":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "y":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.RetryTimeLapse);
                                break;
                        }
                    }

                    // Update the ScheduleHistory in the database
                    UpdateScheduleHistory(scheduleHistoryItem);

                    if (scheduleHistoryItem.NextStart != Null.NullDate && scheduleHistoryItem.RetryTimeLapse != Null.NullInteger)
                    {
                        // Put the object back into the ScheduleQueue
                        // collection with the new NextStart date.
                        scheduleHistoryItem.StartDate = Null.NullDate;
						Logger.LogDebug("WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): scheduleHistoryItem.StartDate → {Arg2}", scheduleHistoryItem, exception, scheduleHistoryItem.StartDate);
                        scheduleHistoryItem.EndDate = Null.NullDate;
                        scheduleHistoryItem.LogNotes = string.Empty;
						Logger.LogDebug("WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): scheduleHistoryItem.LogNotes → {Arg2}", scheduleHistoryItem, exception, scheduleHistoryItem.LogNotes);
                        scheduleHistoryItem.ProcessGroup = -1;
						Logger.LogDebug("WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): scheduleHistoryItem.ProcessGroup → {Arg2}", scheduleHistoryItem, exception, scheduleHistoryItem.ProcessGroup);
                        AddToScheduleQueue(scheduleHistoryItem);
                    }

                    if (scheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_FAILURE" };
						Logger.LogError("WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): log → {Log}", scheduleHistoryItem, exception, log);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("TYPE", scheduleHistoryItem.TypeFullName);
                        if (exception != null)
                        {
                            log.AddProperty("EXCEPTION", exception.Message);
                        }

                        log.AddProperty("RESCHEDULED FOR", Convert.ToString(scheduleHistoryItem.NextStart, CultureInfo.InvariantCulture));
                        log.AddProperty("SOURCE", scheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", activeThreadCount.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("FREE THREADS", FreeThreads.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("READER TIMEOUTS", readerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("WRITER TIMEOUTS", writerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString(CultureInfo.InvariantCulture));
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in WorkErrored(scheduleHistoryItem={ScheduleHistoryItem},exception={Exception}): {Arg2}", scheduleHistoryItem, exception, exc.Message);
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void WorkProgressing(SchedulerClient schedulerClient)
            {
				Logger.LogError(">>> Entering WorkProgressing(schedulerClient={SchedulerClient})", schedulerClient);
                try
                {
                    // A SchedulerClient is notifying us that their
                    // process is in progress.  Informational only.
                    if (schedulerClient.ScheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_PROGRESSING" };
						Logger.LogError("WorkProgressing(schedulerClient={SchedulerClient}): log → {Log}", schedulerClient, log);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("TYPE", schedulerClient.GetType().FullName);
                        log.AddProperty("SOURCE", schedulerClient.ScheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", activeThreadCount.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("FREE THREADS", FreeThreads.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("READER TIMEOUTS", readerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("WRITER TIMEOUTS", writerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString(CultureInfo.InvariantCulture));
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in WorkProgressing(schedulerClient={SchedulerClient}): {Arg1}", schedulerClient, exc.Message);
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
				Logger.LogWarning("<<< Exiting WorkProgressing(schedulerClient={SchedulerClient})", schedulerClient);
            }

            public static void WorkStarted(SchedulerClient schedulerClient)
            {
				Logger.LogError(">>> Entering WorkStarted(schedulerClient={SchedulerClient})", schedulerClient);
                WorkStarted(schedulerClient.ScheduleHistoryItem);
				Logger.LogWarning("<<< Exiting WorkStarted(schedulerClient={SchedulerClient})", schedulerClient);
            }

            public static void WorkStarted(ScheduleHistoryItem scheduleHistoryItem)
            {
				Logger.LogError(">>> Entering WorkStarted(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                bool activeThreadCountIncremented = false;
				Logger.LogError("WorkStarted(scheduleHistoryItem={ScheduleHistoryItem}): activeThreadCountIncremented → {ActiveThreadCountIncremented}", scheduleHistoryItem, activeThreadCountIncremented);
                try
                {
					Logger.LogWarning("<<< Exiting WorkStarted(schedulerClient={SchedulerClient})", schedulerClient);
                    scheduleHistoryItem.ThreadID = Thread.CurrentThread.GetHashCode();

                    // Put the object in the ScheduleInProgress collection
                    // and remove it from the ScheduleQueue
                    RemoveFromScheduleQueue(scheduleHistoryItem);
                    AddToScheduleInProgress(scheduleHistoryItem);

                    // A SchedulerClient is notifying us that their
                    // process has started.  Increase our ActiveThreadCount
                    Interlocked.Increment(ref activeThreadCount);
                    activeThreadCountIncremented = true;

                    // Update the schedule item
                    // object property to note the start time.
                    scheduleHistoryItem.StartDate = DateTime.Now;
					Logger.LogDebug("WorkStarted(scheduleHistoryItem={ScheduleHistoryItem}): scheduleHistoryItem.StartDate → {Arg1}", scheduleHistoryItem, scheduleHistoryItem.StartDate);
                    AddScheduleHistory(scheduleHistoryItem);

                    if (scheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_STARTED" };
						Logger.LogError("WorkStarted(scheduleHistoryItem={ScheduleHistoryItem}): log → {Log}", scheduleHistoryItem, log);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("TYPE", scheduleHistoryItem.TypeFullName);
                        log.AddProperty("SOURCE", scheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", activeThreadCount.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("FREE THREADS", FreeThreads.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("READER TIMEOUTS", readerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("WRITER TIMEOUTS", writerTimeouts.ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString(CultureInfo.InvariantCulture));
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in WorkStarted(scheduleHistoryItem={ScheduleHistoryItem}): {Arg1}", scheduleHistoryItem, exc.Message);
                    // Decrement the ActiveThreadCount because
                    // otherwise the number of active threads
                    // will appear to be climbing when in fact
                    // no tasks are being executed.
                    if (activeThreadCountIncremented)
                    {
                        Interlocked.Decrement(ref activeThreadCount);
                    }

                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            // DNN-5001
            public static void StopScheduleInProgress(ScheduleItem scheduleItem, ScheduleHistoryItem runningscheduleHistoryItem)
            {
				Logger.LogError(">>> Entering StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem})", scheduleItem, runningscheduleHistoryItem);
                try
                {
                    // attempt to stop task only if it is still in progress
                    if (GetScheduleItemFromScheduleInProgress(scheduleItem) != null)
                    {
                        var scheduleHistoryItem = GetScheduleItemFromScheduleInProgress(scheduleItem);
						Logger.LogError("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem → {ScheduleHistoryItem}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem);
						Logger.LogDebug("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem.StartDate → {Arg2}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem.StartDate);
                        scheduleHistoryItem.ScheduleHistoryID = runningscheduleHistoryItem.ScheduleHistoryID;
						Logger.LogDebug("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem.ScheduleHistoryID → {Arg2}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem.ScheduleHistoryID);
                        scheduleHistoryItem.StartDate = runningscheduleHistoryItem.StartDate;

                        // Remove the object in the ScheduleInProgress collection
                        RemoveFromScheduleInProgress(scheduleHistoryItem);

                        // A SchedulerClient is notifying us that their
                        // process has completed.  Decrease our ActiveThreadCount
                        Interlocked.Decrement(ref activeThreadCount);

                        // Update the schedule item object property
                        // to note the end time and next start
                        scheduleHistoryItem.EndDate = DateTime.Now;
						Logger.LogDebug("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem.EndDate → {Arg2}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem.EndDate);

                        if (scheduleHistoryItem.ScheduleSource == ScheduleSource.STARTED_FROM_EVENT)
                        {
                            scheduleHistoryItem.NextStart = Null.NullDate;
							Logger.LogDebug("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem.NextStart → {Arg2}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem.NextStart);
                        }
                        else
                        {
                            if (scheduleHistoryItem.CatchUpEnabled)
                            {
                                switch (scheduleHistoryItem.TimeLapseMeasurement)
                                {
                                    case "s":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddSeconds(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "m":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddMinutes(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "h":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddHours(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "d":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "w":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                        break;
                                    case "mo":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "y":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.TimeLapse);
                                        break;
                                }
                            }
                            else
                            {
                                switch (scheduleHistoryItem.TimeLapseMeasurement)
                                {
                                    case "s":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddSeconds(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "m":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddMinutes(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "h":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddHours(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "d":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "w":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                        break;
                                    case "mo":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddMonths(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "y":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddYears(scheduleHistoryItem.TimeLapse);
                                        break;
                                }
                            }
                        }

                        // Update the ScheduleHistory in the database
                        UpdateScheduleHistory(scheduleHistoryItem);

                        if (scheduleHistoryItem.NextStart != Null.NullDate)
                        {
                            // Put the object back into the ScheduleQueue
                            // collection with the new NextStart date.
                            scheduleHistoryItem.StartDate = Null.NullDate;
                            scheduleHistoryItem.EndDate = Null.NullDate;
                            scheduleHistoryItem.LogNotes = string.Empty;
							Logger.LogDebug("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem.LogNotes → {Arg2}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem.LogNotes);
                            scheduleHistoryItem.ProcessGroup = -1;
							Logger.LogDebug("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): scheduleHistoryItem.ProcessGroup → {Arg2}", scheduleItem, runningscheduleHistoryItem, scheduleHistoryItem.ProcessGroup);
                            AddToScheduleQueue(scheduleHistoryItem);
                        }

                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_COMPLETED" };
						Logger.LogError("StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): log → {Log}", scheduleItem, runningscheduleHistoryItem, log);
                        log.AddProperty("REASON", "Scheduler task has been stopped manually");
                        log.AddProperty("TYPE", scheduleHistoryItem.TypeFullName);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture));
                        log.AddProperty("NEXT START", Convert.ToString(scheduleHistoryItem.NextStart, CultureInfo.InvariantCulture));
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem}): {Arg2}", scheduleItem, runningscheduleHistoryItem, exc.Message);
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
				Logger.LogWarning("<<< Exiting StopScheduleInProgress(scheduleItem={ScheduleItem},runningscheduleHistoryItem={RunningscheduleHistoryItem})", scheduleItem, runningscheduleHistoryItem);
            }

            internal static bool IsInQueue(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering IsInQueue(scheduleItem={ScheduleItem})", scheduleItem);
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        return ScheduleQueue.Any(si => si.ScheduleID == scheduleItem.ScheduleID);
                    }
                }
                catch (ApplicationException)
                {
					Logger.LogError("Exception in IsInQueue(scheduleItem={ScheduleItem}): {Arg1}", scheduleItem, ApplicationException.Message);
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
					Logger.LogWarning("<<< Exiting IsInQueue(scheduleItem={ScheduleItem})", scheduleItem);
                    return false;
                }
            }

            internal static ServerInfo GetServer(string executingServer)
            {
				Logger.LogError(">>> Entering GetServer(executingServer={ExecutingServer})", executingServer);
                try
                {
                    return ServerController.GetServers().FirstOrDefault(
                        s => ServerController.GetServerName(s).Equals(executingServer, StringComparison.OrdinalIgnoreCase) && s.Enabled);
                }
                catch (Exception)
                {
					Logger.LogError("Exception in GetServer(executingServer={ExecutingServer}): {Arg1}", executingServer, Exception.Message);
                    // catches edge-case where schedule runs before webserver registration
					Logger.LogWarning("<<< Exiting GetServer(executingServer={ExecutingServer})", executingServer);
                    return null;
                }
            }

            internal static void InitializeThreadPool(bool boolDebug, int maxThreads)
            {
				Logger.LogError(">>> Entering InitializeThreadPool(boolDebug={BoolDebug},maxThreads={MaxThreads})", boolDebug, maxThreads);
                debug = boolDebug;
				Logger.LogDebug("InitializeThreadPool(boolDebug={BoolDebug},maxThreads={MaxThreads}): debug → {Debug}", boolDebug, maxThreads, debug);
                lock (typeof(CoreScheduler))
                {
                    if (!threadPoolInitialized)
                    {
                        threadPoolInitialized = true;
						Logger.LogDebug("InitializeThreadPool(boolDebug={BoolDebug},maxThreads={MaxThreads}): threadPoolInitialized → {ThreadPoolInitialized}", boolDebug, maxThreads, threadPoolInitialized);
                        if (maxThreads == -1)
                        {
                            maxThreads = 1;
                        }

                        numberOfProcessGroups = maxThreads;
						Logger.LogDebug("InitializeThreadPool(boolDebug={BoolDebug},maxThreads={MaxThreads}): numberOfProcessGroups → {NumberOfProcessGroups}", boolDebug, maxThreads, numberOfProcessGroups);
                        maxThreadCount = maxThreads;
						Logger.LogDebug("InitializeThreadPool(boolDebug={BoolDebug},maxThreads={MaxThreads}): maxThreadCount → {MaxThreadCount}", boolDebug, maxThreads, maxThreadCount);
                        for (int i = 0; i < numberOfProcessGroups; i++)
                        {
                            Array.Resize(ref processGroup, i + 1);
                            processGroup[i] = new ProcessGroup();
                        }
                    }
                }
				Logger.LogWarning("<<< Exiting InitializeThreadPool(boolDebug={BoolDebug},maxThreads={MaxThreads})", boolDebug, maxThreads);
            }

            /// <summary>adds an item to the collection of schedule items in progress.</summary>
            /// <param name="scheduleHistoryItem">Item to add.</param>
            /// <remarks>Thread Safe.</remarks>
            private static void AddToScheduleInProgress(ScheduleHistoryItem scheduleHistoryItem)
            {
				Logger.LogError(">>> Entering AddToScheduleInProgress(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                if (!ScheduleInProgressContains(scheduleHistoryItem))
                {
                    try
                    {
                        using (ScheduleInProgress.GetWriteLock(LockTimeout))
                        {
                            if (!ScheduleInProgressContains(scheduleHistoryItem))
                            {
                                ScheduleInProgress.Add(scheduleHistoryItem);
                            }
                        }
                    }
                    catch (ApplicationException ex)
                    {
						Logger.LogError("Exception in AddToScheduleInProgress(scheduleHistoryItem={ScheduleHistoryItem}): {Arg1}", scheduleHistoryItem, ex.Message);
                        // The writer lock request timed out.
                        Interlocked.Increment(ref writerTimeouts);
                        Exceptions.Exceptions.LogException(ex);
                    }
                }
				Logger.LogWarning("<<< Exiting AddToScheduleInProgress(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
            }

            private static int GetProcessGroup()
            {
				Logger.LogError(">>> Entering GetProcessGroup()");
                // return a random process group
                var r = new Random();
				Logger.LogError("GetProcessGroup(): r → {R}", r);
				Logger.LogWarning("<<< Exiting GetProcessGroup()");
                return r.Next(0, numberOfProcessGroups - 1);
            }

            private static bool IsInProgress(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering IsInProgress(scheduleItem={ScheduleItem})", scheduleItem);
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        return ScheduleInProgress.Any(si => si.ScheduleID == scheduleItem.ScheduleID);
                    }
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref readerTimeouts);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(ex);
                    }

				Logger.LogWarning("<<< Exiting IsInProgress(scheduleItem={ScheduleItem})", scheduleItem);
                    return false;
                }
            }

            /// <summary>Removes an item from the collection of schedule items in progress.</summary>
            /// <param name="scheduleItem">The schedule item.</param>
            /// <remarks>Thread Safe.</remarks>
            private static void RemoveFromScheduleInProgress(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering RemoveFromScheduleInProgress(scheduleItem={ScheduleItem})", scheduleItem);
                try
                {
                    using (ScheduleInProgress.GetWriteLock(LockTimeout))
                    {
                        var item = ScheduleInProgress.FirstOrDefault(si => si.ScheduleID == scheduleItem.ScheduleID);
						Logger.LogError("RemoveFromScheduleInProgress(scheduleItem={ScheduleItem}): item → {Item}", scheduleItem, item);
                        if (item != null)
                        {
                            ScheduleInProgress.Remove(item);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in RemoveFromScheduleInProgress(scheduleItem={ScheduleItem}): {Arg1}", scheduleItem, ex.Message);
                    // The writer lock request timed out.
                    Interlocked.Increment(ref writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }
				Logger.LogWarning("<<< Exiting RemoveFromScheduleInProgress(scheduleItem={ScheduleItem})", scheduleItem);
            }

            /// <summary>Gets a schedulehistory item from the collection of schedule items in progress.</summary>
            /// <param name="scheduleItem">The schedule item.</param>
            /// <remarks>Thread Safe.</remarks>
            private static ScheduleHistoryItem GetScheduleItemFromScheduleInProgress(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering GetScheduleItemFromScheduleInProgress(scheduleItem={ScheduleItem})", scheduleItem);
                try
                {
                    using (ScheduleInProgress.GetWriteLock(LockTimeout))
                    {
                        var item = ScheduleInProgress.FirstOrDefault(si => si.ScheduleID == scheduleItem.ScheduleID);
						Logger.LogError("GetScheduleItemFromScheduleInProgress(scheduleItem={ScheduleItem}): item → {Item}", scheduleItem, item);
                        return item;
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in GetScheduleItemFromScheduleInProgress(scheduleItem={ScheduleItem}): {Arg1}", scheduleItem, ex.Message);
                    // The writer lock request timed out.
                    Interlocked.Increment(ref writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }

				Logger.LogWarning("<<< Exiting GetScheduleItemFromScheduleInProgress(scheduleItem={ScheduleItem})", scheduleItem);
                return null;
            }

            private static bool ScheduleInProgressContains(ScheduleHistoryItem scheduleHistoryItem)
            {
				Logger.LogError(">>> Entering ScheduleInProgressContains(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        return ScheduleInProgress.Any(si => si.ScheduleID == scheduleHistoryItem.ScheduleID);
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in ScheduleInProgressContains(scheduleHistoryItem={ScheduleHistoryItem}): {Arg1}", scheduleHistoryItem, ex.Message);
                    Interlocked.Increment(ref readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
					Logger.LogWarning("<<< Exiting ScheduleInProgressContains(scheduleHistoryItem={ScheduleHistoryItem})", scheduleHistoryItem);
                    return false;
                }
            }

            private static bool ScheduleQueueContains(ScheduleItem objScheduleItem)
            {
				Logger.LogError(">>> Entering ScheduleQueueContains(objScheduleItem={ObjScheduleItem})", objScheduleItem);
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        return ScheduleQueue.Any(si => si.ScheduleID == objScheduleItem.ScheduleID);
                    }
                }
                catch (ApplicationException ex)
                {
					Logger.LogError("Exception in ScheduleQueueContains(objScheduleItem={ObjScheduleItem}): {Arg1}", objScheduleItem, ex.Message);
                    Interlocked.Increment(ref readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
					Logger.LogWarning("<<< Exiting ScheduleQueueContains(objScheduleItem={ObjScheduleItem})", objScheduleItem);
                    return false;
                }
            }

            private static void LogWhyTaskNotRun(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering LogWhyTaskNotRun(scheduleItem={ScheduleItem})", scheduleItem);
                if (debug)
                {
                    bool appended = false;
					Logger.LogError("LogWhyTaskNotRun(scheduleItem={ScheduleItem}): appended → {Appended}", scheduleItem, appended);
                    var strDebug = new StringBuilder("Task not run because ");
					Logger.LogError("LogWhyTaskNotRun(scheduleItem={ScheduleItem}): strDebug → {StrDebug}", scheduleItem, strDebug);
                    if (!(scheduleItem.NextStart <= DateTime.Now))
                    {
                        strDebug.Append(" task is scheduled for " + scheduleItem.NextStart);
                        appended = true;
                    }

                    if (!scheduleItem.Enabled)
                    {
                        if (appended)
                        {
                            strDebug.Append(" and");
                        }

                        strDebug.Append(" task is not enabled");
                        appended = true;
                    }

                    if (IsInProgress(scheduleItem))
                    {
                        if (appended)
                        {
                            strDebug.Append(" and");
                        }

                        strDebug.Append(" task is already in progress");
                        appended = true;
                    }

                    if (HasDependenciesConflict(scheduleItem))
                    {
                        if (appended)
                        {
                            strDebug.Append(" and");
                        }

                        strDebug.Append(" task has conflicting dependency");
                    }

                    var log = new LogInfo();
					Logger.LogError("LogWhyTaskNotRun(scheduleItem={ScheduleItem}): log → {Log}", scheduleItem, log);
                    log.AddProperty("EVENT NOT RUN REASON", strDebug.ToString());
                    log.AddProperty("SCHEDULE ID", scheduleItem.ScheduleID.ToString(CultureInfo.InvariantCulture));
                    log.AddProperty("TYPE FULL NAME", scheduleItem.TypeFullName);
                    log.LogTypeKey = "DEBUG";
					Logger.LogDebug("LogWhyTaskNotRun(scheduleItem={ScheduleItem}): log.LogTypeKey → {Arg1}", scheduleItem, log.LogTypeKey);
                    LogController.Instance.AddLog(log);
                }
				Logger.LogWarning("<<< Exiting LogWhyTaskNotRun(scheduleItem={ScheduleItem})", scheduleItem);
            }

            private static void LogEventAddedToProcessGroup(ScheduleItem scheduleItem)
            {
				Logger.LogError(">>> Entering LogEventAddedToProcessGroup(scheduleItem={ScheduleItem})", scheduleItem);
                if (debug)
                {
                    var log = new LogInfo();
					Logger.LogError("LogEventAddedToProcessGroup(scheduleItem={ScheduleItem}): log → {Log}", scheduleItem, log);
                    log.AddProperty("EVENT ADDED TO PROCESS GROUP " + scheduleItem.ProcessGroup, scheduleItem.TypeFullName);
                    log.AddProperty("SCHEDULE ID", scheduleItem.ScheduleID.ToString(CultureInfo.InvariantCulture));
                    log.LogTypeKey = "DEBUG";
					Logger.LogDebug("LogEventAddedToProcessGroup(scheduleItem={ScheduleItem}): log.LogTypeKey → {Arg1}", scheduleItem, log.LogTypeKey);
                    LogController.Instance.AddLog(log);
                }
				Logger.LogWarning("<<< Exiting LogEventAddedToProcessGroup(scheduleItem={ScheduleItem})", scheduleItem);
            }

            private static string ServerGroupServers(ServerInfo thisServer)
            {
				Logger.LogError(">>> Entering ServerGroupServers(thisServer={ThisServer})", thisServer);
                // Get the servers
                var servers = ServerController.GetEnabledServers().Where(s => s.ServerGroup == thisServer.ServerGroup);
				Logger.LogError("ServerGroupServers(thisServer={ThisServer}): servers → {Servers}", thisServer, servers);
				Logger.LogWarning("<<< Exiting ServerGroupServers(thisServer={ThisServer})", thisServer);
                return servers.Aggregate(string.Empty, (current, serverInfo) => current + ServerController.GetServerName(serverInfo) + ",");
            }
        }
    }
}
