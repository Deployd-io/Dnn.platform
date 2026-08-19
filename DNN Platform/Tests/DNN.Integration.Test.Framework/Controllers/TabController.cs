// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Collections.Generic;

    using DNN.Integration.Test.Framework.Helpers;
	using Microsoft.Extensions.Logging;

    public static class TabController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private static int PortalId => DnnDataHelper.PortalId;

        public static int GetTabIdByTabName(string tabName)
        {
			_logger.LogDebug(">>> Entering GetTabIdByTabName(tabName={TabName})", tabName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                "SELECT TOP(1) ISNULL(TabId, -1) FROM {{objectQualifier}}Tabs WHERE TabName = '{0}' AND ISNULL(PortalID,0)={1};",
                tabName, PortalId);
				_logger.LogDebug("GetTabIdByTabName(tabName={TabName}): TabName → {TabName1}", tabName, TabName);
				_logger.LogInformation("GetTabIdByTabName(tabName)={TabName}: find query executed in {Arg1} ms", tabName, __logStart.ElapsedMilliseconds);

			_logger.LogDebug("<<< Exiting GetTabIdByTabName(tabName={TabName})", tabName);
            return DatabaseHelper.ExecuteScalar<int>(query);
        }

        public static string GetTabNameByTabId(int tabId)
        {
			_logger.LogDebug(">>> Entering GetTabNameByTabId(tabId={TabId})", tabId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                "SELECT TOP(1) TabName FROM {{objectQualifier}}Tabs WHERE TabId = '{0}' AND ISNULL(PortalID,0)={1};",
                tabId, PortalId);
				_logger.LogDebug("GetTabNameByTabId(tabId={TabId}): TabId → {TabId1}", tabId, TabId);
				_logger.LogInformation("GetTabNameByTabId(tabId)={TabId}: find query executed in {Arg1} ms", tabId, __logStart.ElapsedMilliseconds);

			_logger.LogDebug("<<< Exiting GetTabNameByTabId(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        public static int GetTabIdByPathName(string tabPath)
        {
			_logger.LogDebug(">>> Entering GetTabIdByPathName(tabPath={TabPath})", tabPath);
			_logger.LogDebug("<<< Exiting GetTabIdByPathName(tabPath={TabPath})", tabPath);
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format("SELECT TOP(1) TabId FROM {{objectQualifier}}vw_Tabs WHERE TabPath = '{0}' AND ISNULL(PortalID,0)={1};", tabPath, PortalId));
        }

        public static int GetModuleIdInsidePage(int pageId)
        {
			_logger.LogDebug(">>> Entering GetModuleIdInsidePage(pageId={PageId})", pageId);
			_logger.LogDebug("<<< Exiting GetModuleIdInsidePage(pageId={PageId})", pageId);
            return DatabaseHelper.ExecuteScalar<int>(
                string.Format(
                    @"
                    SELECT TOP(1) ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND PaneName='contentPane' AND ContainerSrc IS NULL AND ISNULL(PortalID,0)={1};",
			_logger.LogDebug("<<< Exiting GetModuleIdInsidePage(tabId={TabId},moduleFriendlyName={ModuleFriendlyName})", tabId, moduleFriendlyName);
                    pageId, PortalId));
        }

        /// <summary>Returns all the tab related columns from vw_tabinfo.</summary>
        /// <returns></returns>
        public static dynamic GetTabInfo(int tabId)
        {
			_logger.LogDebug(">>> Entering GetTabInfo(tabId={TabId})", tabId);
            var sql = string.Format(@"SELECT TOP 1 * FROM {{objectQualifier}}vw_Tabs WHERE TabId = '{0}' ORDER BY TabId ASC", tabId);
			_logger.LogDebug("GetTabInfo(tabId={TabId}): sql → {Sql}", tabId, sql);
			_logger.LogDebug("<<< Exiting GetTabInfo(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteDynamicQuery(sql)[0];
        }

        /// <summary>Gets list of all modules on a page.</summary>
        /// <param name="pageId">Id of the page.</param>
        /// <returns>List of all the modules on a page.</returns>
        public static List<int> GetAllModuleIdInsidePage(int pageId)
        {
			_logger.LogDebug(">>> Entering GetAllModuleIdInsidePage(pageId={PageId})", pageId);
            var squery = string.Format(
                @"
                    SELECT ModuleID FROM {{objectQualifier}}vw_TabModules
                    WHERE TabId = '{0}' AND ISNULL(PortalID,0)={1};",
                pageId, PortalId);
				_logger.LogDebug("GetAllModuleIdInsidePage(pageId={PageId}): TabId → {TabId}", pageId, TabId);
            var result = DatabaseHelper.ExecuteQuery(squery);
			_logger.LogDebug("GetAllModuleIdInsidePage(pageId={PageId}): result → {Result}", pageId, result);
            var listOfModules = new List<int>();
			_logger.LogDebug("GetAllModuleIdInsidePage(pageId={PageId}): listOfModules → {ListOfModules}", pageId, listOfModules);
            foreach (var item in result)
            {
                object moduleId;
                item.TryGetValue("ModuleID", out moduleId);
                listOfModules.Add(Convert.ToInt32(moduleId));
            }

			_logger.LogDebug("<<< Exiting GetAllModuleIdInsidePage(pageId={PageId})", pageId);
            return listOfModules;
        }

        /// <summary>Gets ModuleID of the first module on a page (any pane).</summary>
        /// <param name="pageId">id of the tab.</param>
        /// <returns>Returns ModuleID of the first module (top(1)) on a page.</returns>
        public static int GetModuleIdInsidePageAnyPane(int pageId)
        {
			_logger.LogDebug(">>> Entering GetModuleIdInsidePageAnyPane(pageId={PageId})", pageId);
			_logger.LogDebug("<<< Exiting GetModuleIdInsidePageAnyPane(pageId={PageId})", pageId);
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
				_logger.LogDebug("GetModuleIdInsidePage(tabId={TabId},moduleFriendlyName={ModuleFriendlyName}): query → {Query}", tabId, moduleFriendlyName, query);
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
			_logger.LogDebug(">>> Entering GetTabPath(tabPath={TabPath})", tabPath);
            var parts = tabPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var path = string.Empty;
			_logger.LogDebug("GetTabPath(tabPath={TabPath}): parts → {Parts}", tabPath, parts);
            var name = string.Empty;
			_logger.LogDebug("GetTabPath(tabPath={TabPath}): name → {Name}", tabPath, name);
            foreach (var s in parts)
            {
                path += "//" + s;
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                var query = string.Format("SELECT TOP(1) TabName FROM {{objectQualifier}}Tabs WHERE TabPath = '{0}' AND ISNULL(PortalID,0)={1};", path, PortalId);
                var tabName = DatabaseHelper.ExecuteScalar<string>(query);
				_logger.LogDebug("GetTabPath(tabPath={TabPath}): query → {Query}", tabPath, query);
				_logger.LogDebug("GetTabPath(tabPath={TabPath}): tabName → {TabName}", tabPath, tabName);
				_logger.LogInformation("GetTabPath(tabPath)={TabPath}: find query executed in {Arg1} ms", tabPath, __logStart.ElapsedMilliseconds);
                name += "/" + tabName.Replace(" ", "-");
            }

			_logger.LogDebug("<<< Exiting GetTabPath(tabPath={TabPath})", tabPath);
            return name;
        }

        public static int GetTabModuleId(int tabId, int moduledId)
        {
			_logger.LogDebug(">>> Entering GetTabModuleId(tabId={TabId},moduledId={ModuledId})", tabId, moduledId);
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
			_logger.LogDebug(">>> Entering GetTabUrls(tabId={TabId})", tabId);
            var sql = string.Format(@"SELECT * FROM {{objectQualifier}}TabUrls WHERE TabId = '{0}' ORDER BY SeqNum DESC", tabId);
			_logger.LogDebug("GetTabUrls(tabId={TabId}): sql → {Sql}", tabId, sql);
			_logger.LogDebug("<<< Exiting GetTabUrls(tabId={TabId})", tabId);
            return DatabaseHelper.ExecuteDynamicQuery(sql);
        }

        /// <summary>Adds tab setting. Deletes existing setting first.</summary>
        public static void SaveTabSetting(int tabId, string settingName, string settingValue)
        {
			_logger.LogDebug(">>> Entering SaveTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
            DeleteTabSetting(tabId, settingName);
            AddTabSetting(tabId, settingName, settingValue);
			_logger.LogDebug("<<< Exiting SaveTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
        }

        /// <summary>Adds tab setting. Does not delete existing setting first.</summary>
        public static void AddTabSetting(int tabId, string settingName, string settingValue)
        {
			_logger.LogDebug(">>> Entering AddTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
            DatabaseHelper.ExecuteStoredProcedure(
                "UpdateTabSetting",
                tabId,
                settingName,
                settingValue,
                1); // CreatedByUserID
				_logger.LogDebug("<<< Exiting AddTabSetting(tabId={TabId},settingName={SettingName},settingValue={SettingValue})", tabId, settingName, settingValue);
        }

        /// <summary>Deletes tab setting.</summary>
        public static void DeleteTabSetting(int tabId, string settingName)
        {
			_logger.LogDebug(">>> Entering DeleteTabSetting(tabId={TabId},settingName={SettingName})", tabId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabSetting", tabId, settingName);
			_logger.LogDebug("<<< Exiting DeleteTabSetting(tabId={TabId},settingName={SettingName})", tabId, settingName);
        }
    }
}
