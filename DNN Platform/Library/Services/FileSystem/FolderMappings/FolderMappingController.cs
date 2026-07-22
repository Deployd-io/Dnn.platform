// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem.Internal;

    using Microsoft.Extensions.DependencyInjection;

    public class FolderMappingController : ComponentBase<IFolderMappingController, FolderMappingController>, IFolderMappingController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string CacheKeyPrefix = "GetFolderMappingSettings";
        private static readonly DataProvider DataProvider = DataProvider.Instance();
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="FolderMappingController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        internal FolderMappingController()
            : this(null)
        {
		Logger.LogDebug(">>> Entering FolderMappingController()");
        }
		Logger.LogDebug("<<< Exiting FolderMappingController()");

        /// <summary>Initializes a new instance of the <see cref="FolderMappingController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        internal FolderMappingController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("FolderMappingController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <inheritdoc />
        public FolderMappingInfo GetDefaultFolderMapping(int portalId)
        {
			Logger.LogDebug(">>> Entering GetDefaultFolderMapping(portalId={PortalId})", portalId);
            var defaultFolderMapping = Config.GetSection("dotnetnuke/folder") != null ?
                this.GetFolderMappings(portalId).Find(fm => fm.FolderProviderType == Config.GetDefaultProvider("folder").Name) :
                this.GetFolderMapping(portalId, "Standard");
			Logger.LogDebug("<<< Exiting GetDefaultFolderMapping(portalId={PortalId})", portalId);
            return defaultFolderMapping ?? this.GetFolderMapping(portalId, "Standard");
        }

        /// <inheritdoc />
        public int AddFolderMapping(FolderMappingInfo objFolderMapping)
        {
			Logger.LogDebug(">>> Entering AddFolderMapping(objFolderMapping={ObjFolderMapping})", objFolderMapping);
            objFolderMapping.FolderMappingID = DataProvider.AddFolderMapping(
                objFolderMapping.PortalID,
                objFolderMapping.MappingName,
                objFolderMapping.FolderProviderType,
                UserController.Instance.GetCurrentUserInfo().UserID);

            UpdateFolderMappingSettings(objFolderMapping);

            ClearFolderMappingCache(objFolderMapping.PortalID);

			Logger.LogDebug("<<< Exiting AddFolderMapping(objFolderMapping={ObjFolderMapping})", objFolderMapping);
            return objFolderMapping.FolderMappingID;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void DeleteFolderMapping(int portalID, int folderMappingID)
        {
			Logger.LogDebug(">>> Entering DeleteFolderMapping(portalID={PortalID},folderMappingID={FolderMappingID})", portalID, folderMappingID);
            var folderManager = FolderManager.Instance;
            var folders = folderManager.GetFolders(portalID);

			Logger.LogDebug("DeleteFolderMapping(portalID={PortalID},folderMappingID={FolderMappingID}): folders → {Folders}", portalID, folderMappingID, folders);
            var folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingID);
            if (folderMappingFolders.Any())
			Logger.LogDebug("DeleteFolderMapping(portalID={PortalID},folderMappingID={FolderMappingID}): folderMappingFolders → {FolderMappingFolders}", portalID, folderMappingID, folderMappingFolders);
            {
                // Delete files in folders with the provided mapping (only in the database)
                foreach (var file in folderMappingFolders.Select<IFolderInfo, IEnumerable<IFileInfo>>(folderManager.GetFiles).SelectMany(files => files))
                {
                    DataProvider.DeleteFile(portalID, file.FileName, file.FolderId);
                }

                // Remove the folders with the provided mapping that doesn't have child folders with other mapping (only in the database and filesystem)
                var folders1 = folders; // copy the variable to not access a modified closure
                var removableFolders = folders.Where(f =>
                    f.FolderMappingID == folderMappingID &&
                    !folders1.Any(f2 =>
                        f2.FolderID != f.FolderID &&
                        f2.FolderPath.StartsWith(f.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                        f2.FolderMappingID != folderMappingID));

                if (removableFolders.Any())
                {
                    foreach (var removableFolder in removableFolders.OrderByDescending(rf => rf.FolderPath))
                    {
                        DirectoryWrapper.Instance.Delete(removableFolder.PhysicalPath, false);
                        DataProvider.DeleteFolder(portalID, removableFolder.FolderPath);
                    }
                }

                // Update the rest of folders with the provided mapping to use the standard mapping
                folders = folderManager.GetFolders(portalID, false); // re-fetch the folders

                folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingID);

                if (folderMappingFolders.Any())
                {
                    var defaultFolderMapping = this.GetDefaultFolderMapping(portalID);

					Logger.LogDebug("DeleteFolderMapping(portalID={PortalID},folderMappingID={FolderMappingID}): defaultFolderMapping → {DefaultFolderMapping}", portalID, folderMappingID, defaultFolderMapping);
                    foreach (var folderMappingFolder in folderMappingFolders)
                    {
                        folderMappingFolder.FolderMappingID = defaultFolderMapping.FolderMappingID;
						Logger.LogDebug("DeleteFolderMapping(portalID={PortalID},folderMappingID={FolderMappingID}): folderMappingFolder.FolderMappingID → {Arg2}", portalID, folderMappingID, folderMappingFolder.FolderMappingID);
                        folderManager.UpdateFolder(folderMappingFolder);
                    }
                }
            }

            DataProvider.DeleteFolderMapping(folderMappingID);
            ClearFolderMappingCache(portalID);
            ClearFolderMappingSettingsCache(folderMappingID);
			Logger.LogDebug("<<< Exiting DeleteFolderMapping(portalID={PortalID},folderMappingID={FolderMappingID})", portalID, folderMappingID);
        }

        /// <inheritdoc />
        public void UpdateFolderMapping(FolderMappingInfo objFolderMapping)
        {
			Logger.LogDebug(">>> Entering UpdateFolderMapping(objFolderMapping={ObjFolderMapping})", objFolderMapping);
            DataProvider.UpdateFolderMapping(
                objFolderMapping.FolderMappingID,
                objFolderMapping.MappingName,
                objFolderMapping.Priority,
                UserController.Instance.GetCurrentUserInfo().UserID);

            ClearFolderMappingCache(objFolderMapping.PortalID);
            UpdateFolderMappingSettings(objFolderMapping);
			Logger.LogDebug("<<< Exiting UpdateFolderMapping(objFolderMapping={ObjFolderMapping})", objFolderMapping);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public FolderMappingInfo GetFolderMapping(int folderMappingID)
        {
			Logger.LogDebug(">>> Entering GetFolderMapping(folderMappingID={FolderMappingID})", folderMappingID);
			Logger.LogDebug("<<< Exiting GetFolderMapping(folderMappingID={FolderMappingID})", folderMappingID);
            return CBO.FillObject<FolderMappingInfo>(DataProvider.GetFolderMapping(folderMappingID));
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public FolderMappingInfo GetFolderMapping(int portalId, int folderMappingID)
        {
			Logger.LogDebug("<<< Exiting GetFolderMapping(portalId={PortalId},folderMappingID={FolderMappingID})", portalId, folderMappingID);
            return this.GetFolderMappings(portalId).SingleOrDefault(fm => fm.FolderMappingID == folderMappingID);
        }

        /// <inheritdoc />
        public FolderMappingInfo GetFolderMapping(int portalId, string mappingName)
        {
			Logger.LogDebug("<<< Exiting GetFolderMapping(portalId={PortalId},mappingName={MappingName})", portalId, mappingName);
            return this.GetFolderMappings(portalId).SingleOrDefault(fm => fm.MappingName == mappingName);
        }

        /// <inheritdoc />
        public List<FolderMappingInfo> GetFolderMappings(int portalId)
        {
			Logger.LogDebug(">>> Entering GetFolderMappings(portalId={PortalId})", portalId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderMappingCacheKey, portalId);
			Logger.LogDebug("GetFolderMappings(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
			Logger.LogDebug("<<< Exiting GetFolderMappings(portalId={PortalId})", portalId);
            return CBO.GetCachedObject<List<FolderMappingInfo>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.FolderMappingCacheTimeOut, DataCache.FolderMappingCachePriority),
                _ => CBO.FillCollection<FolderMappingInfo>(DataProvider.GetFolderMappings(portalId)));
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void AddDefaultFolderTypes(int portalID)
        {
			Logger.LogDebug(">>> Entering AddDefaultFolderTypes(portalID={PortalID})", portalID);
            DataProvider.AddDefaultFolderTypes(portalID);
			Logger.LogDebug("<<< Exiting AddDefaultFolderTypes(portalID={PortalID})", portalID);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public Hashtable GetFolderMappingSettings(int folderMappingID)
        {
			Logger.LogDebug(">>> Entering GetFolderMappingSettings(folderMappingID={FolderMappingID})", folderMappingID);
            var strCacheKey = CacheKeyPrefix + folderMappingID;
            var objSettings = (Hashtable)DataCache.GetCache(strCacheKey);
            if (objSettings == null)
			Logger.LogDebug("GetFolderMappingSettings(folderMappingID={FolderMappingID}): objSettings → {ObjSettings}", folderMappingID, objSettings);
            {
                objSettings = new Hashtable();
                IDataReader dr = null;
				Logger.LogDebug("GetFolderMappingSettings(folderMappingID={FolderMappingID}): dr → {Dr}", folderMappingID, dr);
                try
                {
                    dr = DataProvider.GetFolderMappingSettings(folderMappingID);
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(1))
                        {
                            objSettings[dr.GetString(0)] = dr.GetString(1);
                        }
                        else
                        {
                            objSettings[dr.GetString(0)] = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in GetFolderMappingSettings(folderMappingID={FolderMappingID}): {Arg1}", folderMappingID, ex.Message);
                    Exceptions.Exceptions.LogException(ex);
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }

                var intCacheTimeout = 20 * (int)Host.PerformanceSetting;
                DataCache.SetCache(strCacheKey, objSettings, TimeSpan.FromMinutes(intCacheTimeout));
				Logger.LogDebug("GetFolderMappingSettings(folderMappingID={FolderMappingID}): intCacheTimeout → {IntCacheTimeout}", folderMappingID, intCacheTimeout);
            }

			Logger.LogDebug("<<< Exiting GetFolderMappingSettings(folderMappingID={FolderMappingID})", folderMappingID);
            return objSettings;
        }

        private static void UpdateFolderMappingSettings(FolderMappingInfo objFolderMapping)
        {
			Logger.LogDebug(">>> Entering UpdateFolderMappingSettings(objFolderMapping={ObjFolderMapping})", objFolderMapping);
            foreach (string sKey in objFolderMapping.FolderMappingSettings.Keys)
            {
                UpdateFolderMappingSetting(objFolderMapping.FolderMappingID, sKey, Convert.ToString(objFolderMapping.FolderMappingSettings[sKey], CultureInfo.InvariantCulture));
            }

            ClearFolderMappingSettingsCache(objFolderMapping.FolderMappingID);
			Logger.LogDebug("<<< Exiting UpdateFolderMappingSettings(objFolderMapping={ObjFolderMapping})", objFolderMapping);
        }

        private static void UpdateFolderMappingSetting(int folderMappingId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering UpdateFolderMappingSetting(folderMappingId={FolderMappingId},settingName={SettingName},settingValue={SettingValue})", folderMappingId, settingName, settingValue);
            IDataReader dr = null;
            try
			Logger.LogDebug("UpdateFolderMappingSetting(folderMappingId={FolderMappingId},settingName={SettingName},settingValue={SettingValue}): dr → {Dr}", folderMappingId, settingName, settingValue, dr);
            {
                dr = DataProvider.GetFolderMappingSetting(folderMappingId, settingName);
                if (dr.Read())
                {
                    DataProvider.UpdateFolderMappingSetting(folderMappingId, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID);
                }
                else
                {
                    DataProvider.AddFolderMappingSetting(folderMappingId, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in UpdateFolderMappingSetting(folderMappingId={FolderMappingId},settingName={SettingName},settingValue={SettingValue}): {Arg3}", folderMappingId, settingName, settingValue, ex.Message);
                Exceptions.Exceptions.LogException(ex);
            }
            finally
            {
                // Ensure DataReader is closed
                CBO.CloseDataReader(dr, true);
            }
			Logger.LogDebug("<<< Exiting UpdateFolderMappingSetting(folderMappingId={FolderMappingId},settingName={SettingName},settingValue={SettingValue})", folderMappingId, settingName, settingValue);
        }

        private static void ClearFolderMappingCache(int portalId)
        {
			Logger.LogDebug(">>> Entering ClearFolderMappingCache(portalId={PortalId})", portalId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderMappingCacheKey, portalId);
            DataCache.RemoveCache(cacheKey);
			Logger.LogDebug("<<< Exiting ClearFolderMappingCache(portalId={PortalId})", portalId);
			Logger.LogDebug("ClearFolderMappingCache(portalId={PortalId}): cacheKey → {CacheKey}", portalId, cacheKey);
        }

        private static void ClearFolderMappingSettingsCache(int folderMappingID)
        {
			Logger.LogDebug(">>> Entering ClearFolderMappingSettingsCache(folderMappingID={FolderMappingID})", folderMappingID);
            DataCache.RemoveCache(CacheKeyPrefix + folderMappingID);
			Logger.LogDebug("<<< Exiting ClearFolderMappingSettingsCache(folderMappingID={FolderMappingID})", folderMappingID);
        }
    }
}
