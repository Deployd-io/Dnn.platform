// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Cookies
{
    using System;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Scheduling;

    /// <summary>Scheduled task to remove old cookies.</summary>
    public class PurgeAuthCookiesTask : SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="PurgeAuthCookiesTask"/> class.</summary>
        /// <param name="objScheduleHistoryItem">Object that will get added to the database as a record of this run.</param>
        public PurgeAuthCookiesTask(ScheduleHistoryItem objScheduleHistoryItem)
        {
			Logger.LogDebug(">>> Entering PurgeAuthCookiesTask(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
            this.ScheduleHistoryItem = objScheduleHistoryItem;
			Logger.LogDebug("<<< Exiting PurgeAuthCookiesTask(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
			Logger.LogDebug("PurgeAuthCookiesTask(objScheduleHistoryItem={ObjScheduleHistoryItem}): this.ScheduleHistoryItem → {Arg1}", objScheduleHistoryItem, this.ScheduleHistoryItem);
        }

        /// <inheritdoc />
        public override void DoWork()
        {
			Logger.LogDebug(">>> Entering DoWork()");
            try
            {
                AuthCookieController.Instance.DeleteExpired(DateTime.UtcNow);
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote("Purging auth cookies completed");
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in DoWork(): {Arg0}", ex.Message);
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote($"Purging auth cookies task failed: {exc.ToString()}.");
                this.Errored(ref exc);
                Exceptions.LogException(exc);
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }
    }
}
