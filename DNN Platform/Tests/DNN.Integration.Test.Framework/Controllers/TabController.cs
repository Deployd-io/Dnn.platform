// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Collections.Generic;

    using DNN.Integration.Test.Framework.Helpers;

    public static class TabController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static int PortalId => DnnDataHelper.PortalId;

        public static int GetTabIdByTabName(string tabName)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                "SELECT TOP(1) ISNULL(TabId, -1) FROM {{objectQualifier}}Tabs WHERE TabName = '{0}' AND ISNULL(PortalID,0)={1};",
                tabName, PortalId);

            return DatabaseHelper.ExecuteScalar<int>(query);
        }

        public static string GetTabNameByTabId(int tabId)
        {
			Logger.LogDebug(">>> Entering GetTabNameByTabId(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                "SELECT TOP(1) TabName FROM {{objectQualifier}}Tabs WHERE TabId = '{0}' AND ISNULL(PortalID,0)={1};",
                tabId, PortalId);
				Logger.LogDebug("GetTabNameByTabId(tabId={TabId}): TabId → {TabId1}", tabId, TabId);
				Logger.LogInformation("GetTabNameByTabId(tabId)={TabId}: find query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);

			Logger.LogDebug("<<< Exiting GetTabNameByTabId(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        public static int GetTabIdByPathName(string tabPath)
        {
			Logger.LogDebug(">>> Entering GetTabIdByPathName(tabPath={TabPath})", tabPath);
			Logger.LogDebug("<<< Exiting GetTabIdByPathName(tabPath={TabPath})", tabPath);
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format("SELECT TOP(1) TabId FROM {{objectQualifier}}vw_Tabs WHERE TabPath = '{0}' AND ISNULL(PortalID,0)={1};", tabPath, PortalId));
        }

        public static int GetModuleIdInsidePage(int pageId)
        {
			Logger.LogDebug(">>> Entering GetModuleIdInsidePage(pageId={PageId})", pageId);
			Logger.LogDebug("<<< Exiting GetModuleIdInsidePage(pageId={PageId})", pageId);
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format(
                    @"
                    SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND PaneName='contentPane' AND ContainerSrc IS NULL AND ISNULL(PortalID,0)={1};",
                    pageId, PortalId));
        }

        /// <summary>Returns all the tab related columns from vw_tabinfo.</summary>
        /// <returns></returns>
        public static dynamic GetTabInfo(int tabId)
        {
			Logger.LogDebug(">>> Entering GetTabInfo(tabId={TabId})", tabId);
            var sql = string.Format(@"SELECT TOP 1 * FROM {{objectQualifier}}vw_Tabs WHERE TabId = '{0}' ORDER BY TabId ASC", tabId);
			Logger.LogDebug("GetTabInfo(tabId={TabId}): sql → {Sql}", tabId, sql);
			Logger.LogDebug("<<< Exiting GetTabInfo(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteDynamicQuery(sql)[0];
        }

        /// <summary>Gets list of all modules on a page.</summary>
        /// <param name="pageId">Id of the page.</param>
        /// <returns>List of all the modules on a page.</returns>
        public static List<int> GetAllModuleIdInsidePage(int pageId)
        {
			Logger.LogDebug(">>> Entering GetAllModuleIdInsidePage(pageId={PageId})", pageId);
            var squery = string.Format(
                @"
                    SELECT ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND ISNULL(PortalID,0)={1};",
                pageId, PortalId);
				Logger.LogDebug("GetAllModuleIdInsidePage(pageId={PageId}): TabId → {TabId}", pageId, TabId);
            var result = DatabaseHelper.ExecuteQuery(squery);
            var listOfModules = new List<int>();
			Logger.LogDebug("GetAllModuleIdInsidePage(pageId={PageId}): result → {Result}", pageId, result);
			Logger.LogDebug("GetAllModuleIdInsidePage(pageId={PageId}): listOfModules → {ListOfModules}", pageId, listOfModules);
            foreach (var item in result)
            {
                object moduleId;
                item.TryGetValue("ModuleID", out moduleId);
                listOfModules.Add(Convert.ToInt32(moduleId));
            }

			Logger.LogDebug("<<< Exiting GetAllModuleIdInsidePage(pageId={PageId})", pageId);
            return listOfModules;
        }

        /// <summary>Gets ModuleID of the first module on a page (any pane).</summary>
        /// <param name="pageId">id of the tab.</param>
        /// <returns>Returns ModuleID of the first module (top(1)) on a page.</returns>
        public static int GetModuleIdInsidePageAnyPane(int pageId)
        {
			Logger.LogDebug(">>> Entering GetModuleIdInsidePageAnyPane(pageId={PageId})", pageId);
			Logger.LogDebug("<<< Exiting GetModuleIdInsidePageAnyPane(pageId={PageId})", pageId);
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format(
                    @"
                    SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND ContainerSrc IS NULL AND ISNULL(PortalID,0)={1};",
                    pageId, PortalId));
        }

        public static int GetModuleIdInsidePage(int tabId, string moduleFriendlyName)
        {
            var query1 = string.Format(
                @"
                SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules tm
                INNER JOIN {{objectQualifier}}DesktopModules dm ON dm.DesktopModuleId = tm.DesktopModuleId 
                INNER JOIN {{objectQualifier}}ModuleDefinitions md ON md.ModuleDefID = tm.ModuleDefID 
                WHERE TabId = {0} AND ISNULL(PortalID,0)={1} AND (dm.FriendlyName = '{2}' OR md.FriendlyName = '{2}')
                ORDER BY tm.ModuleDefID, ModuleID;", tabId, PortalId, moduleFriendlyName);

            try
            {
				Logger.LogDebug("GetModuleIdInsidePage(tabId={TabId},moduleFriendlyName={ModuleFriendlyName}): query → {Query}", tabId, moduleFriendlyName, query);
				Logger.LogDebug("<<< Exiting GetModuleIdInsidePage(tabId={TabId},moduleFriendlyName={ModuleFriendlyName})", tabId, moduleFriendlyName);
                return DatabaseHelper.ExecuteScalar<int>(query1);
            }
            catch (Exception)
            {
                Console.WriteLine(@"Failed running query => {0}", query1);
                throw;
            }
        }

        public static string GetTabPath(string tabPath)
        {
			Logger.LogDebug(">>> Entering GetTabPath(tabPath={TabPath})", tabPath);
            var parts = tabPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var path = string.Empty;
			Logger.LogDebug("GetTabPath(tabPath={TabPath}): parts → {Parts}", tabPath, parts);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var name = string.Empty;
            foreach (var s in parts)
            {
			Logger.LogDebug("GetTabPath(tabPath={TabPath}): name → {Name}", tabPath, name);
                path += "//" + s;
                var query = string.Format("SELECT TOP(1) TabName FROM {{objectQualifier}}Tabs WHERE TabPath = '{0}' AND ISNULL(PortalID,0)={1};", path, PortalId);
                var tabName = DatabaseHelper.ExecuteScalar<string>(query);
				Logger.LogDebug("GetTabPath(tabPath={TabPath}): query → {Query}", tabPath, query);
				Logger.LogDebug("GetTabPath(tabPath={TabPath}): tabName → {TabName}", tabPath, tabName);
				Logger.LogInformation("GetTabPath(tabPath)={TabPath}: find query executed in {Arg1} ms", tabPath, __logStart.ElapsedMilliseconds);
                name += "/" + tabName.Replace(" ", "-");
            }

			Logger.LogDebug("<<< Exiting GetTabPath(tabPath={TabPath})", tabPath);
            return name;
        }

        public static int GetTabModuleId(int tabId, int moduledId)
        {
			Logger.LogDebug(">>> Entering GetTabModuleId(tabId={TabId},moduledId={ModuledId})", tabId, moduledId);
			Logger.LogDebug("<<< Exiting GetTabModuleId(tabId={TabId},moduledId={ModuledId})", tabId, moduledId);
            return DatabaseHelper.ExecuteScalar<int>(
               string.Format(
                   @"
                    SELECT TOP(1) TabModuleID FROM {{objectQualifier}}TabModules
                    WHERE TabID = {0} AND ModuleID = {1};", tabId, moduledId));
        }

        /// <summary>Returns all the tab urls.</summary>
        /// <returns></returns>
        public static IList<dynamic> GetTabUrls(int tabId)
        {
			Logger.LogDebug(">>> Entering GetTabUrls(tabId={TabId})", tabId);
            var sql = string.Format(@"SELECT * FROM {{objectQualifier}}TabUrls WHERE TabId = '{0}' ORDER BY SeqNum DESC", tabId);
			Logger.LogDebug("GetTabUrls(tabId={TabId}): sql → {Sql}", tabId, sql);
			Logger.LogDebug("<<< Exiting GetTabUrls(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteDynamicQuery(sql);
        }

        /// <summary>Adds tab setting. Deletes existing setting first.</summary>
        public static void SaveTabSetting(int tabId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering SaveTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
            DeleteTabSetting(tabId, settingName);
            AddTabSetting(tabId, settingName, settingValue);
			Logger.LogDebug("<<< Exiting SaveTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
        }

        /// <summary>Adds tab setting. Does not delete existing setting first.</summary>
        public static void AddTabSetting(int tabId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering AddTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
            DatabaseHelper.ExecuteStoredProcedure(
                "UpdateTabSetting",
                tabId,
                settingName,
                settingValue,
                1); // CreatedByUserID
        }
				Logger.LogDebug("<<< Exiting AddTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);

        /// <summary>Deletes tab setting.</summary>
        public static void DeleteTabSetting(int tabId, string settingName)
        {
			Logger.LogDebug(">>> Entering DeleteTabSetting(tabId={TabId},settingName={SettingName})", tabId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabSetting", tabId, settingName);
			Logger.LogDebug("<<< Exiting DeleteTabSetting(tabId={TabId},settingName={SettingName})", tabId, settingName);
        }
    }
}
