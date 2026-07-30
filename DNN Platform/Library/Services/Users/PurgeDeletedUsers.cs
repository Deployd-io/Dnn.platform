// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Users
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Scheduling;

    using Microsoft.Extensions.DependencyInjection;

    public class PurgeDeletedUsers : SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="PurgeDeletedUsers"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public PurgeDeletedUsers(ScheduleHistoryItem objScheduleHistoryItem)
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), objScheduleHistoryItem)
        {
		Logger.LogDebug(">>> Entering PurgeDeletedUsers(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);
        }
		Logger.LogDebug("<<< Exiting PurgeDeletedUsers(objScheduleHistoryItem={ObjScheduleHistoryItem})", objScheduleHistoryItem);

        /// <summary>Initializes a new instance of the <see cref="PurgeDeletedUsers"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public PurgeDeletedUsers(IPortalController portalController, ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.portalController = portalController;
            this.ScheduleHistoryItem = objScheduleHistoryItem;
			Logger.LogDebug("PurgeDeletedUsers(portalController={PortalController},objScheduleHistoryItem={ObjScheduleHistoryItem}): this.portalController → {Arg2}", portalController, objScheduleHistoryItem, this.portalController);
			Logger.LogDebug("PurgeDeletedUsers(portalController={PortalController},objScheduleHistoryItem={ObjScheduleHistoryItem}): this.ScheduleHistoryItem → {Arg2}", portalController, objScheduleHistoryItem, this.ScheduleHistoryItem);
			Logger.LogDebug("PurgeDeletedUsers(portalController={PortalController},objScheduleHistoryItem={ObjScheduleHistoryItem}): portalController → {PortalController1}", portalController, objScheduleHistoryItem, portalController);
        }

        /// <inheritdoc />
        public override void DoWork()
        {
			Logger.LogDebug(">>> Entering DoWork()");
            try
            {
                foreach (IPortalInfo portal in this.portalController.GetPortals())
                {
                    var settings = new PortalSettings(portal.PortalId);
                    if (!settings.DataConsentActive || settings.DataConsentUserDeleteAction != PortalSettings.UserDeleteAction.DelayedHardDelete)
					Logger.LogDebug("DoWork(): settings → {Settings}", settings);
                    {
                        continue;
                    }

                    var thresholdDate = settings.DataConsentDelayMeasurement switch
                    {
                        "h" => DateTime.Now.AddHours(-1 * settings.DataConsentDelay),
                        "d" => DateTime.Now.AddDays(-1 * settings.DataConsentDelay),
                        "w" => DateTime.Now.AddDays(-7 * settings.DataConsentDelay),
                        _ => DateTime.Now,
                    };

                    var deletedUsers = UserController.GetDeletedUsers(portal.PortalId);
                    foreach (UserInfo user in deletedUsers)
                    {
					Logger.LogDebug("DoWork(): deletedUsers → {DeletedUsers}", deletedUsers);
                        if (user.LastModifiedOnDate < thresholdDate && user.RequestsRemoval)
                        {
                            UserController.RemoveUser(user);
                            this.ScheduleHistoryItem.AddLogNote($"Removed user {user.Username}{Environment.NewLine}");
                        }
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Purging deleted users task completed");
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in DoWork(): {Arg0}", ex.Message);
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote($"Purging deleted users task failed: {exc}.");

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
			Logger.LogDebug("<<< Exiting DoWork()");
        }
    }
}
