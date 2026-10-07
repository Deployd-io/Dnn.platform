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
			eventLogger.LogError(">>> Entering DeleteAllModuleSettingsById(tabId={TabId})", tabId);
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettingsByTab", tabId);
			eventLogger.LogWarning("<<< Exiting DeleteAllModuleSettingsById(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all module settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllModuleSettings(int portalId)
        {
			eventLogger.LogError(">>> Entering DeleteAllModuleSettings(portalId={PortalId})", portalId);
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllModuleSettings", portalId.ToString());
			eventLogger.LogWarning("<<< Exiting DeleteAllModuleSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes all page settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPageSettings(int portalId)
        {
			eventLogger.LogError(">>> Entering DeleteAllPageSettings(portalId={PortalId})", portalId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPageSettings", portalId.ToString());

			eventLogger.LogError("DeleteAllPageSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogWarning("<<< Exiting DeleteAllPageSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes current page settings of the Editor, for the Current Portal.</summary>
        /// <param name="tabId">The tab id.</param>
        public static void DeleteCurrentPageSettings(int tabId)
        {
			eventLogger.LogError(">>> Entering DeleteCurrentPageSettings(tabId={TabId})", tabId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteCurrentPageSettings", tabId.ToString());

			eventLogger.LogError("DeleteCurrentPageSettings(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogWarning("<<< Exiting DeleteCurrentPageSettings(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all page settings of the Editor, for the specified child tabs from the specified <paramref name="tabId"/>.</summary>
        /// <param name="tabId">
        /// The tab Id.
        /// </param>
        public static void DeleteAllChildPageSettings(int tabId)
        {
			eventLogger.LogError(">>> Entering DeleteAllChildPageSettings(tabId={TabId})", tabId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllChildPageSettings", tabId);

			eventLogger.LogError("DeleteAllChildPageSettings(tabId)={TabId}: database query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogWarning("<<< Exiting DeleteAllChildPageSettings(tabId={TabId})", tabId);
        }

        /// <summary>Deletes all portal settings of the Editor, for the Current Portal.</summary>
        /// <param name="portalId">The portal id.</param>
        public static void DeleteAllPortalSettings(int portalId)
        {
			eventLogger.LogError(">>> Entering DeleteAllPortalSettings(portalId={PortalId})", portalId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllPortalSettings", portalId.ToString());

			eventLogger.LogError("DeleteAllPortalSettings(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogWarning("<<< Exiting DeleteAllPortalSettings(portalId={PortalId})", portalId);
        }

        /// <summary>Deletes all Host settings of the Editor.</summary>
        public static void DeleteAllHostSettings()
        {
			eventLogger.LogError(">>> Entering DeleteAllHostSettings()");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataProvider.Instance().ExecuteNonQuery("CKE_DeleteAllHostSettings");

			eventLogger.LogError("DeleteAllHostSettings(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Finally Clear Cache
            ClearEditorCache();
			eventLogger.LogWarning("<<< Exiting DeleteAllHostSettings()");
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
			eventLogger.LogError(">>> Entering GetEditorHostSettings()");
            var editorHostSettings = new List<EditorHostSetting>();

            var cache = DataCache.GetCache("CKE_Host");
			eventLogger.LogError("GetEditorHostSettings(hostSettings={HostSettings}): editorHostSettings → {EditorHostSettings}", hostSettings, editorHostSettings);
			eventLogger.LogError("GetEditorHostSettings(): cache → {Cache}", cache);

            if (cache == null)
            {
				eventLogger.LogError("GetEditorHostSettings(hostSettings={HostSettings}): cache → {Cache}", hostSettings, cache);
				eventLogger.LogError("GetEditorHostSettings(): editorHostSettings → {EditorHostSettings}", editorHostSettings);
                var timeOut = 20 * Convert.ToInt32(hostSettings.PerformanceSetting);

				eventLogger.LogError("GetEditorHostSettings(): timeOut → {TimeOut}", timeOut);
                using (var dr = DataProvider.Instance().ExecuteReader("CKE_GetEditorHostSettings"))
                {
					eventLogger.LogError("GetEditorHostSettings(hostSettings={HostSettings}): timeOut → {TimeOut}", hostSettings, timeOut);
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

			eventLogger.LogWarning("<<< Exiting GetEditorHostSettings(hostSettings={HostSettings})", hostSettings);
            return editorHostSettings;
        }

        /// <summary>Adds or updates the editor host setting.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void AddOrUpdateEditorHostSetting(string settingName, string settingValue)
        {
			eventLogger.LogError(">>> Entering AddOrUpdateEditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
            DataProvider.Instance().ExecuteNonQuery("CKE_AddOrUpdateEditorHostSetting", settingName, settingValue);
			eventLogger.LogWarning("<<< Exiting AddOrUpdateEditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
        }

        /// <summary>Clear editor cache.</summary>
        public static void ClearEditorCache()
        {
			eventLogger.LogError(">>> Entering ClearEditorCache()");
            DataCache.RemoveCache("CKE_Host");
			eventLogger.LogWarning("<<< Exiting ClearEditorCache()");
        }
    }
}
