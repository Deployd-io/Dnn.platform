// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.SystemHealth
{
    using System;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Scheduling;

    /// <summary>
    /// When run on each server it updates the last activity date for the server and removes any servers that haven't been seen in 24 hours.
    /// </summary>
    public class WebServerMonitor : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WebServerMonitor));

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerMonitor"/> class.
        /// Constructs a WebServerMonitor SchedulerClient.
        /// </summary>
        /// <param name="objScheduleHistoryItem">A SchedulerHistoryItem.</param>
        /// <remarks>
        /// This must be run on all servers.
        /// </remarks>
        public WebServerMonitor(ScheduleHistoryItem objScheduleHistoryItem)
        {
			Logger.LogDebug(">>> Entering WebServerMonitor(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
            this.ScheduleHistoryItem = objScheduleHistoryItem;
			Logger.LogDebug("<<< Exiting WebServerMonitor(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
			Logger.LogDebug("WebServerMonitor(objScheduleHistoryItem={ObjScheduleHistoryItem}): this.ScheduleHistoryItem → {Arg1}", objScheduleHistoryItem, this.ScheduleHistoryItem);
        }

        /// <summary>
        /// Runs on the active server and updates the last activity date for the current server.
        /// </summary>
        public override void DoWork()
        {
            try
            {
                Logger.Info("Starting WebServerMonitor");

                UpdateCurrentServerActivity();
                DisableServersWithoutRecentActivity();
                RemoveInActiveServers();

                Logger.Info("Finished WebServerMonitor");
                this.ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote($"Updating server health failed: {exc}.");
                this.Errored(ref exc);
                Logger.ErrorFormat(CultureInfo.InvariantCulture, "Error in WebServerMonitor: {0}. {1}", exc.Message, exc.StackTrace);
                Exceptions.LogException(exc);
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }

        private static void UpdateCurrentServerActivity()
        {
            Logger.Info("Starting UpdateCurrentServerActivity");

            // Creating a new ServerInfo object, by default points it at the current server
            var currentServer = new ServerInfo();
            ServerController.UpdateServerActivity(currentServer);
			Logger.LogDebug("UpdateCurrentServerActivity(): currentServer → {CurrentServer}", currentServer);

            Logger.Info("Finished UpdateCurrentServerActivity");
			Logger.LogDebug("<<< Exiting UpdateCurrentServerActivity()");
        }

        private static void DisableServersWithoutRecentActivity()
        {
			Logger.LogDebug(">>> Entering DisableServersWithoutRecentActivity()");
            var serversWithActivity = ServerController.GetEnabledServersWithActivity();
            var newServer = serversWithActivity.FirstOrDefault();
			Logger.LogDebug("DisableServersWithoutRecentActivity(): serversWithActivity → {ServersWithActivity}", serversWithActivity);
			Logger.LogDebug("DisableServersWithoutRecentActivity(): newServer → {NewServer}", newServer);

            foreach (var s in ServerController.GetInActiveServers(10))
            {
                s.Enabled = false;
				Logger.LogDebug("DisableServersWithoutRecentActivity(): s.Enabled → {Arg0}", s.Enabled);
                ServerController.UpdateServerActivity(s);

                // Update the schedules that were running on that server, that may never have been loaded
                if (newServer != null)
                {
                    SchedulingController.ReplaceServer(s, newServer);
                }
            }
			Logger.LogDebug("<<< Exiting DisableServersWithoutRecentActivity()");
        }

        private static void RemoveInActiveServers()
        {
            Logger.Info("Starting RemoveInActiveServers");
            var serversWithActivity = ServerController.GetEnabledServersWithActivity();
            var newServer = serversWithActivity.FirstOrDefault();
			Logger.LogDebug("RemoveInActiveServers(): serversWithActivity → {ServersWithActivity}", serversWithActivity);
			Logger.LogDebug("RemoveInActiveServers(): newServer → {NewServer}", newServer);

            foreach (var s in ServerController.GetInActiveServers(1440))
            {
                ServerController.DeleteServer(s.ServerID);

                // Update the schedules that were running on that server, that may never have been loaded
                if (newServer != null)
                {
                    SchedulingController.ReplaceServer(s, newServer);
                }
            }

            Logger.Info("Finished RemoveInActiveServers");
			Logger.LogDebug("<<< Exiting RemoveInActiveServers()");
        }
    }
}
