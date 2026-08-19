// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The business of mobile preview profiles.</summary>
    public class PreviewProfileController : IPreviewProfileController
    {
        private readonly IHostSettings hostSettings;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="PreviewProfileController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PreviewProfileController()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering PreviewProfileController()");
		eventLogger.LogDebug("<<< Exiting PreviewProfileController()");
		eventLogger.LogDebug(">>> Entering PreviewProfileController(eventLogger={EventLogger},hostSettings={HostSettings})", eventLogger, hostSettings);
		eventLogger.LogDebug("PreviewProfileController(eventLogger={EventLogger},hostSettings={HostSettings}): hostSettings → {HostSettings1}", eventLogger, hostSettings, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="PreviewProfileController"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        public PreviewProfileController(IEventLogger eventLogger, IHostSettings hostSettings)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			eventLogger.LogDebug("<<< Exiting PreviewProfileController(eventLogger={EventLogger},hostSettings={HostSettings})", eventLogger, hostSettings);
        }

        /// <summary>
        /// save a preview profile. If profile.Id equals Null.NullInteger(-1), that means need to add a new profile;
        /// otherwise will update the profile by profile.Id.
        /// </summary>
        /// <param name="profile">profile object.</param>
        public void Save(IPreviewProfile profile)
        {
			eventLogger.LogDebug(">>> Entering Save(profile={Profile})", profile);
            Requires.NotNull("The profile can't be null", profile);

            if (profile.Id == Null.NullInteger || profile.SortOrder == 0)
            {
                profile.SortOrder = this.GetProfilesByPortal(profile.PortalId, false).Count + 1;
            }

            int id = DataProvider.Instance().SavePreviewProfile(
                profile.Id,
                profile.PortalId,
                profile.Name,
                profile.Width,
                profile.Height,
                profile.UserAgent,
                profile.SortOrder,
                UserController.Instance.GetCurrentUserInfo().UserID);

            profile.Id = id;
			eventLogger.LogDebug("Save(profile={Profile}): profile.Id → {Arg1}", profile, profile.Id);

            var logContent = $"{(profile.Id == Null.NullInteger ? "Add" : "Update")} Mobile Preview Profile '{profile.Name}'";
			eventLogger.LogDebug("Save(profile={Profile}): logContent → {LogContent}", profile, logContent);
            AddLog(this.eventLogger, logContent);

            ClearCache(profile.PortalId);
			eventLogger.LogDebug("<<< Exiting Save(profile={Profile})", profile);
        }

        /// <summary>delete a preview profile.</summary>
        /// <param name="portalId">Portal's id.</param>
        /// <param name="id">the profile's id.</param>
        public void Delete(int portalId, int id)
        {
			eventLogger.LogDebug(">>> Entering Delete(portalId={PortalId},id={Id})", portalId, id);
            var delProfile = this.GetProfileById(portalId, id);
            if (delProfile != null)
            {
                // update the list order
                this.GetProfilesByPortal(portalId)
                    .Where(p => p.SortOrder > delProfile.SortOrder)
                    .ToList()
                    .ForEach(p =>
                    {
                        p.SortOrder--;
                        this.Save(p);
                    });
                DataProvider.Instance().DeletePreviewProfile(id);

                AddLog(this.eventLogger, string.Format(CultureInfo.InvariantCulture, "Delete Mobile Preview Profile '{0}'", id));

                ClearCache(portalId);
            }
			eventLogger.LogDebug("<<< Exiting Delete(portalId={PortalId},id={Id})", portalId, id);
        }

        /// <summary>get a preview profiles list for portal.</summary>
        /// <param name="portalId">portal id.</param>
        /// <returns>List of preview profile.</returns>
        public IList<IPreviewProfile> GetProfilesByPortal(int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetProfilesByPortal(portalId={PortalId})", portalId);
			eventLogger.LogDebug("<<< Exiting GetProfilesByPortal(portalId={PortalId})", portalId);
			eventLogger.LogDebug("<<< Exiting GetProfilesByPortal(portalId={PortalId},addDefault={AddDefault})", portalId, addDefault);
            return this.GetProfilesByPortal(portalId, true);
        }

        /// <summary>get a specific preview profile by id.</summary>
        /// <param name="portalId">the ID of the portal to which the profile belongs.</param>
        /// <param name="id">profile's id.</param>
        /// <returns>profile object.</returns>
        public IPreviewProfile GetProfileById(int portalId, int id)
        {
			eventLogger.LogDebug(">>> Entering GetProfileById(portalId={PortalId},id={Id})", portalId, id);
			eventLogger.LogDebug("<<< Exiting GetProfileById(portalId={PortalId},id={Id})", portalId, id);
            return this.GetProfilesByPortal(portalId).Where(r => r.Id == id).FirstOrDefault();
        }

        private static void ClearCache(int portalId)
        {
			eventLogger.LogDebug(">>> Entering ClearCache(portalId={PortalId})", portalId);
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.PreviewProfilesCacheKey, portalId));
			eventLogger.LogDebug("<<< Exiting ClearCache(portalId={PortalId})", portalId);
        }

        private static void AddLog(IEventLogger eventLogger, string logContent)
        {
			eventLogger.LogDebug(">>> Entering AddLog(eventLogger={EventLogger},logContent={LogContent})", eventLogger, logContent);
            eventLogger.AddLog("Message", logContent, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.ADMIN_ALERT);
			eventLogger.LogDebug("<<< Exiting AddLog(eventLogger={EventLogger},logContent={LogContent})", eventLogger, logContent);
        }

        private IList<IPreviewProfile> GetProfilesByPortal(int portalId, bool addDefault)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PreviewProfilesCacheKey, portalId);
            var cacheArg = new CacheItemArgs(cacheKey, DataCache.PreviewProfilesCacheTimeOut, DataCache.PreviewProfilesCachePriority, portalId, addDefault);
            return CBO.GetCachedObject<IList<IPreviewProfile>>(this.hostSettings, cacheArg, this.GetProfilesByPortalIdCallBack);
        }

        private IList<IPreviewProfile> GetProfilesByPortalIdCallBack(CacheItemArgs cacheItemArgs)
        {
			eventLogger.LogDebug(">>> Entering GetProfilesByPortalIdCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            int portalId = (int)cacheItemArgs.ParamList[0];
            bool addDefault = (bool)cacheItemArgs.ParamList[1];
			eventLogger.LogDebug("GetProfilesByPortalIdCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);

            var profiles = CBO.FillCollection<PreviewProfile>(DataProvider.Instance().GetPreviewProfiles(portalId));
			eventLogger.LogDebug("GetProfilesByPortalIdCallBack(cacheItemArgs={CacheItemArgs}): profiles → {Profiles}", cacheItemArgs, profiles);
            if (profiles.Count == 0 && addDefault)
            {
                profiles = this.CreateDefaultDevices(portalId);
            }

			eventLogger.LogDebug("<<< Exiting GetProfilesByPortalIdCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return profiles.Cast<IPreviewProfile>().ToList();
        }

        private List<PreviewProfile> CreateDefaultDevices(int portalId)
        {
			eventLogger.LogDebug(">>> Entering CreateDefaultDevices(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var settings = PortalController.Instance.GetPortalSettings(portalId);
            List<PreviewProfile> profiles = [];
			eventLogger.LogDebug("CreateDefaultDevices(portalId={PortalId}): settings → {Settings}", portalId, settings);
			eventLogger.LogDebug("CreateDefaultDevices(portalId={PortalId}): profiles → {Profiles}", portalId, profiles);

            if (!settings.TryGetValue("DefPreviewProfiles_Created", out var defaultPreviewProfiles) || defaultPreviewProfiles != DotNetNukeContext.Current.Application.Name)
            {
                try
                {
                    var defaultDeviceDBPath = Config.GetSetting("DefaultDevicesDatabase");
					eventLogger.LogDebug("CreateDefaultDevices(portalId={PortalId}): defaultDeviceDBPath → {DefaultDeviceDBPath}", portalId, defaultDeviceDBPath);
                    if (!string.IsNullOrEmpty(defaultDeviceDBPath))
                    {
                        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultDeviceDBPath);
						eventLogger.LogDebug("CreateDefaultDevices(portalId={PortalId}): dataPath → {DataPath}", portalId, dataPath);

                        if (!string.IsNullOrEmpty(dataPath) && File.Exists(dataPath))
                        {
                            var serializer = new XmlSerializer(typeof(List<PreviewProfile>));
							eventLogger.LogDebug("CreateDefaultDevices(portalId={PortalId}): serializer → {Serializer}", portalId, serializer);
                            using (var fileStream = File.OpenRead(dataPath))
                            using (var xmlReader = XmlReader.Create(fileStream))
                            {
                                profiles = (List<PreviewProfile>)serializer.Deserialize(xmlReader);
                            }

                            profiles?.ForEach(p =>
                            {
                                p.PortalId = portalId;
								eventLogger.LogDebug("CreateDefaultDevices(portalId={PortalId}): p.PortalId → {Arg1}", portalId, p.PortalId);
                                this.Save(p);
                            });
                        }
                    }

                    PortalController.UpdatePortalSetting(portalId, "DefPreviewProfiles_Created", DotNetNukeContext.Current.Application.Name);
                }
                catch (Exception ex)
                {
					eventLogger.LogError("Exception in CreateDefaultDevices(portalId={PortalId}): {Arg1}", portalId, ex.getMessage());
                    Exceptions.Exceptions.LogException(ex);
                }
            }

			eventLogger.LogInformation("CreateDefaultDevices(portalId)={PortalId}: external service call took {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
			eventLogger.LogDebug("<<< Exiting CreateDefaultDevices(portalId={PortalId})", portalId);
            return profiles;
        }
    }
}
