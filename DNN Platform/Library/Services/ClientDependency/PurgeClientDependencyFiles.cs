// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientDependency
{
    using System;
    using System.IO;

    using DotNetNuke.Services.Scheduling;

    internal class PurgeClientDependencyFiles : SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="PurgeClientDependencyFiles"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public PurgeClientDependencyFiles(ScheduleHistoryItem objScheduleHistoryItem)
        {
			Logger.LogDebug(">>> Entering PurgeClientDependencyFiles(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
			Logger.LogDebug("<<< Exiting PurgeClientDependencyFiles(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
			Logger.LogDebug("PurgeClientDependencyFiles(objScheduleHistoryItem={ObjScheduleHistoryItem}): this.ScheduleHistoryItem → {Arg1}", objScheduleHistoryItem, this.ScheduleHistoryItem);
        }

        /// <inheritdoc />
        public override void DoWork()
        {
			Logger.LogDebug(">>> Entering DoWork()");
            try
            {
                string[] filePaths = Directory.GetFiles($"{Common.Globals.ApplicationMapPath}/App_Data/ClientDependency");
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Purging client dependency files task succeeded");
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in DoWork(): {Arg0}", ex.Message);
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote($"Purging client dependency files task failed: {exc.ToString()}.");

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }
    }
}
