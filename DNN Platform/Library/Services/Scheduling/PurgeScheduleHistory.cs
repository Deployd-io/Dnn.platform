// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;

    public class PurgeScheduleHistory : SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="PurgeScheduleHistory"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public PurgeScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
			Logger.LogDebug(">>> Entering PurgeScheduleHistory(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
            this.ScheduleHistoryItem = objScheduleHistoryItem;
			Logger.LogDebug("<<< Exiting PurgeScheduleHistory(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
			Logger.LogDebug("PurgeScheduleHistory(objScheduleHistoryItem={ObjScheduleHistoryItem}): this.ScheduleHistoryItem → {Arg1}", objScheduleHistoryItem, this.ScheduleHistoryItem);
        }

        /// <inheritdoc />
        public override void DoWork()
        {
			Logger.LogDebug(">>> Entering DoWork()");
            try
            {
                // notification that the event is progressing
                this.Progressing();

                SchedulingProvider.Instance().PurgeScheduleHistory();

                // update the result to success since no exception was thrown
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote("Schedule history purged.");
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in DoWork(): {Arg0}", ex.Message);
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("Schedule history purge failed." + exc);
                this.ScheduleHistoryItem.Succeeded = false;

                // notification that we have errored
                this.Errored(ref exc);

                // log the exception
                Exceptions.Exceptions.LogException(exc);
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }
    }
}
