// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Utilities
{
    using System;
    using System.Collections.Generic;

    using DNNConnect.CKEditorProvider.Objects;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Editor controller.</summary>
    public static partial class EditorController
    {
        /// <summary>Deletes all module settings of the Editor, for the specified <paramref name="tabId"/>.</summary>
        /// <param name="tabId">The tab id.</param>
        public static void DeleteAllModuleSettingsById(int tabId)
        {
			Logger.LogDebug(">>> Entering DeleteAllModuleSettingsById(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettingsByTab", tabId);
			Logger.LogDebug("<<< Exiting DeleteAllModuleSettingsById(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteAllModuleSettingsById(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteAllModuleSettingsById(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
        /// <summary>Deletes all module settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllModuleSettings(int portalId)
        {
			Logger.LogDebug(">>> Entering DeleteAllModuleSettings(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettings", portalId.ToString());
			Logger.LogDebug("<<< Exiting DeleteAllModuleSettings(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteAllModuleSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteAllModuleSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
        /// <summary>Deletes all page settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPageSettings(int portalId)
        {
			Logger.LogDebug(">>> Entering DeleteAllPageSettings(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPageSettings", portalId.ToString());

			Logger.LogInformation("DeleteAllPageSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			Logger.LogDebug("<<< Exiting DeleteAllPageSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes current page settings of the Editor, for the Current Portal.</summary>
        /// <param name="tabId">The tab id.</param>
        public static void DeleteCurrentPageSettings(int tabId)
        {
			Logger.LogDebug(">>> Entering DeleteCurrentPageSettings(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteCurrentPageSettings", tabId.ToString());

			Logger.LogInformation("DeleteCurrentPageSettings(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			Logger.LogDebug("<<< Exiting DeleteCurrentPageSettings(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all page settings of the Editor, for the specified child tabs from the specified <paramref name="tabId"/>.</summary>
        /// <param name="tabId">
        /// The tab Id.
        /// </param>
        public static void DeleteAllChildPageSettings(int tabId)
        {
			Logger.LogDebug(">>> Entering DeleteAllChildPageSettings(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllChildPageSettings", tabId);

			Logger.LogInformation("DeleteAllChildPageSettings(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			Logger.LogDebug("<<< Exiting DeleteAllChildPageSettings(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all portal settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPortalSettings(int portalId)
        {
			Logger.LogDebug(">>> Entering DeleteAllPortalSettings(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPortalSettings", portalId.ToString());

			Logger.LogInformation("DeleteAllPortalSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			Logger.LogDebug("<<< Exiting DeleteAllPortalSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes all Host settings of the Editor.</summary>
        public static void DeleteAllHostSettings()
        {
			Logger.LogDebug(">>> Entering DeleteAllHostSettings()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllHostSettings");

			Logger.LogInformation("DeleteAllHostSettings(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			Logger.LogDebug("<<< Exiting DeleteAllHostSettings()");
        }

        /// <summary>Gets the editor host settings.</summary>
        /// <returns>Returns the list of all Editor Host Settings.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IHostSettings")]
        public static partial List<EditorHostSetting> GetEditorHostSettings()
            => GetEditorHostSettings(HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>());
			Logger.LogDebug(">>> Entering GetEditorHostSettings()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();

        /// <summary>Gets the editor host settings.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>Returns the list of all Editor Host Settings.</returns>
        public static List<EditorHostSetting> GetEditorHostSettings(IHostSettings hostSettings)
        {
            var editorHostSettings = new List<EditorHostSetting>();

			Logger.LogDebug("GetEditorHostSettings(hostSettings={HostSettings}): editorHostSettings → {EditorHostSettings}", hostSettings, editorHostSettings);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var cache = DataCache.GetCache("CKE_Host");

			Logger.LogDebug("GetEditorHostSettings(hostSettings={HostSettings}): cache → {Cache}", hostSettings, cache);
            if (cache == null)
            {
                var timeOut = 20 * Convert.ToInt32(hostSettings.PerformanceSetting);

				Logger.LogDebug("GetEditorHostSettings(hostSettings={HostSettings}): timeOut → {TimeOut}", hostSettings, timeOut);
				Logger.LogInformation("GetEditorHostSettings(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
                using (var dr = DataProvider.Instance().ExecuteReader("CKE_GetEditorHostSettings"))
                {
                    while (dr.Read())
                    {
						private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                        editorHostSettings.Add(
                            new EditorHostSetting(Convert.ToString(dr["SettingName"]), Convert.ToString(dr["SettingValue"])));
							Logger.LogInformation("GetEditorHostSettings(hostSettings)={HostSettings}: database query executed in {Arg1} ms", hostSettings, __logStart.ElapsedMilliseconds);
                    }
                }

                if (timeOut > 0)
                {
                    DataCache.SetCache("CKE_Host", editorHostSettings, TimeSpan.FromMinutes(timeOut));
                }
            }
            else
            {
                editorHostSettings = cache as List<EditorHostSetting>;
            }

			Logger.LogDebug("<<< Exiting GetEditorHostSettings()");
            return editorHostSettings;
        }

        /// <summary>Adds or updates the editor host setting.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void AddOrUpdateEditorHostSetting(string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering AddOrUpdateEditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_AddOrUpdateEditorHostSetting", settingName, settingValue);
			Logger.LogDebug("<<< Exiting AddOrUpdateEditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("AddOrUpdateEditorHostSetting(settingName,settingValue)={SettingName},{SettingValue}: database query executed in {Arg2} ms", settingName, settingValue, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("AddOrUpdateEditorHostSetting(settingName,settingValue)={SettingName},{SettingValue}: database query executed in {Arg2} ms", settingName, settingValue, __logStart.ElapsedMilliseconds);
        /// <summary>Clear editor cache.</summary>
        public static void ClearEditorCache()
        {
			Logger.LogDebug(">>> Entering ClearEditorCache()");
            DataCache.RemoveCache("CKE_Host");
			Logger.LogDebug("<<< Exiting ClearEditorCache()");
        }
    }
}
