// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Scheduling;

    public class SynchronizeFileSystem : SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SynchronizeFileSystem));
        /// <summary>Initializes a new instance of the <see cref="SynchronizeFileSystem"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public SynchronizeFileSystem(ScheduleHistoryItem objScheduleHistoryItem)
        {
			Logger.LogDebug(">>> Entering SynchronizeFileSystem(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
            this.ScheduleHistoryItem = objScheduleHistoryItem;
			Logger.LogDebug("SynchronizeFileSystem(objScheduleHistoryItem={ObjScheduleHistoryItem}): this.ScheduleHistoryItem → {Arg1}", objScheduleHistoryItem, this.ScheduleHistoryItem);
			Logger.LogDebug("<<< Exiting SynchronizeFileSystem(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
        }

        /// <inheritdoc />
        public override void DoWork()
        {
			Logger.LogDebug(">>> Entering DoWork()");
            try
            {
                // notification that the event is progressing
                this.Progressing(); // OPTIONAL

                Synchronize();

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote("File System Synchronized."); // OPTIONAL
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in DoWork(): {Arg0}", exc.getMessage());
                this.ScheduleHistoryItem.Succeeded = false;

                this.ScheduleHistoryItem.AddLogNote("File System Synchronization failed. " + exc);

                // notification that we have errored
                this.Errored(ref exc);

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }

        private static void Synchronize()
        {
			Logger.LogDebug(">>> Entering Synchronize()");
            var folderManager = FolderManager.Instance;

            folderManager.Synchronize(Null.NullInteger);

            var portals = PortalController.Instance.GetPortals();
			Logger.LogDebug("Synchronize(): portals → {Portals}", portals);

            // Sync Portals
            for (var intIndex = 0; intIndex <= portals.Count - 1; intIndex++)
            {
                var portal = (PortalInfo)portals[intIndex];
				Logger.LogDebug("Synchronize(): portal → {Portal}", portal);
                folderManager.Synchronize(portal.PortalID);
            }
			Logger.LogDebug("<<< Exiting Synchronize()");
        }
    }
}
