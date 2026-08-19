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
			eventLogger.LogDebug(">>> Entering DeleteAllModuleSettingsById(tabId={TabId})", tabId);
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettingsByTab", tabId);
			eventLogger.LogDebug("<<< Exiting DeleteAllModuleSettingsById(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all module settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllModuleSettings(int portalId)
        {
			eventLogger.LogDebug(">>> Entering DeleteAllModuleSettings(portalId={PortalId})", portalId);
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettings", portalId.ToString());
			eventLogger.LogDebug("<<< Exiting DeleteAllModuleSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes all page settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPageSettings(int portalId)
        {
			eventLogger.LogDebug(">>> Entering DeleteAllPageSettings(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPageSettings", portalId.ToString());

			eventLogger.LogInformation("DeleteAllPageSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogDebug("<<< Exiting DeleteAllPageSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes current page settings of the Editor, for the Current Portal.</summary>
        /// <param name="tabId">The tab id.</param>
        public static void DeleteCurrentPageSettings(int tabId)
        {
			eventLogger.LogDebug(">>> Entering DeleteCurrentPageSettings(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteCurrentPageSettings", tabId.ToString());

			eventLogger.LogInformation("DeleteCurrentPageSettings(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogDebug("<<< Exiting DeleteCurrentPageSettings(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all page settings of the Editor, for the specified child tabs from the specified <paramref name="tabId"/>.</summary>
        /// <param name="tabId">
        /// The tab Id.
        /// </param>
        public static void DeleteAllChildPageSettings(int tabId)
        {
			eventLogger.LogDebug(">>> Entering DeleteAllChildPageSettings(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllChildPageSettings", tabId);

			eventLogger.LogInformation("DeleteAllChildPageSettings(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogDebug("<<< Exiting DeleteAllChildPageSettings(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all portal settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPortalSettings(int portalId)
        {
			eventLogger.LogDebug(">>> Entering DeleteAllPortalSettings(portalId={PortalId})", portalId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPortalSettings", portalId.ToString());

			eventLogger.LogInformation("DeleteAllPortalSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogDebug("<<< Exiting DeleteAllPortalSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes all Host settings of the Editor.</summary>
        public static void DeleteAllHostSettings()
        {
			eventLogger.LogDebug(">>> Entering DeleteAllHostSettings()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllHostSettings");

			eventLogger.LogInformation("DeleteAllHostSettings(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogDebug("<<< Exiting DeleteAllHostSettings()");
        }

        /// <summary>Gets the editor host settings.</summary>
        /// <returns>Returns the list of all Editor Host Settings.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IHostSettings")]
        public static partial List<EditorHostSetting> GetEditorHostSettings()
            => GetEditorHostSettings(HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>());

        /// <summary>Gets the editor host settings.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>Returns the list of all Editor Host Settings.</returns>
        public static List<EditorHostSetting> GetEditorHostSettings(IHostSettings hostSettings)
        {
			eventLogger.LogDebug(">>> Entering GetEditorHostSettings()");
            var editorHostSettings = new List<EditorHostSetting>();
			eventLogger.LogDebug("GetEditorHostSettings(hostSettings={HostSettings}): editorHostSettings → {EditorHostSettings}", hostSettings, editorHostSettings);

            var cache = DataCache.GetCache("CKE_Host");
			eventLogger.LogDebug("GetEditorHostSettings(hostSettings={HostSettings}): cache → {Cache}", hostSettings, cache);

            if (cache == null)
            {
                var timeOut = 20 * Convert.ToInt32(hostSettings.PerformanceSetting);
				eventLogger.LogDebug("GetEditorHostSettings(hostSettings={HostSettings}): timeOut → {TimeOut}", hostSettings, timeOut);

                using (var dr = DataProvider.Instance().ExecuteReader("CKE_GetEditorHostSettings"))
                {
                    while (dr.Read())
                    {
                        editorHostSettings.Add(
                            new EditorHostSetting(Convert.ToString(dr["SettingName"]), Convert.ToString(dr["SettingValue"])));
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

			eventLogger.LogDebug("<<< Exiting GetEditorHostSettings()");
            return editorHostSettings;
        }

        /// <summary>Adds or updates the editor host setting.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void AddOrUpdateEditorHostSetting(string settingName, string settingValue)
        {
			eventLogger.LogDebug(">>> Entering AddOrUpdateEditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
            DataProvider.Instance().ExecuteNonQuery("CKE_AddOrUpdateEditorHostSetting", settingName, settingValue);
			eventLogger.LogDebug("<<< Exiting AddOrUpdateEditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
        }

        /// <summary>Clear editor cache.</summary>
        public static void ClearEditorCache()
        {
			eventLogger.LogDebug(">>> Entering ClearEditorCache()");
            DataCache.RemoveCache("CKE_Host");
			eventLogger.LogDebug("<<< Exiting ClearEditorCache()");
        }
    }
}
