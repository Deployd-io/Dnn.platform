// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using DNN.Integration.Test.Framework.Helpers;
    using DNN.Integration.Test.Framework.Scripts;

    public static class ModuleController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string PortalIdMarker = @"'$[portal_id]'";
        private const string RoleIdMarker = @"'$[role_id]'";
        private const string ModuleIdMarker = @"'$[module_id]'";
        private const string PermissionCodeMarker = @"$[permission_code]";
        private const string PermissionKeyMarker = @"$[permission_key]";
        private const string FriendlyNameMarker = @"$[friendly_name]";

        public static int AddModulePermission(int roleId, int moduleId, string permissionCode, string permissionKey, int portalId = 0)
        {
			Logger.LogDebug(">>> Entering AddModulePermission(roleId={RoleId},moduleId={ModuleId},permissionCode={PermissionCode},permissionKey={PermissionKey},portalId={PortalId})", roleId, moduleId, permissionCode, permissionKey, portalId);
            var fileContent = SqlScripts.AddModulePermission;
            var masterScript = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            // make sure to create at least one SU (see comments at end of files below)
            // the create super-users is proportional to number of total users
            var script = new StringBuilder(masterScript)
                .Replace(RoleIdMarker, roleId.ToString(CultureInfo.InvariantCulture))
                .Replace(ModuleIdMarker, moduleId.ToString(CultureInfo.InvariantCulture))
                .Replace(PermissionCodeMarker, permissionCode.Replace("'", "''"))
                .Replace(PermissionKeyMarker, permissionKey.Replace("'", "''"))
                .ToString();

            var result = DatabaseHelper.ExecuteQuery(script);

			Logger.LogDebug("AddModulePermission(roleId={RoleId},moduleId={ModuleId},permissionCode={PermissionCode},permissionKey={PermissionKey},portalId={PortalId}): result → {Result}", roleId, moduleId, permissionCode, permissionKey, portalId, result);
            if (result.Count > 0 && result[0].ContainsKey("ModulePermissionId"))
            {
                return int.Parse(result[0]["ModulePermissionId"].ToString());
            }

			Logger.LogDebug("<<< Exiting AddModulePermission(roleId={RoleId},moduleId={ModuleId},permissionCode={PermissionCode},permissionKey={PermissionKey},portalId={PortalId})", roleId, moduleId, permissionCode, permissionKey, portalId);
            return -1;
        }

        public static void DeleteModulePermission(int modulePermissionId)
        {
			Logger.LogDebug(">>> Entering DeleteModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
            DatabaseHelper.ExecuteStoredProcedure("DeleteModulePermission", modulePermissionId);
			Logger.LogDebug("<<< Exiting DeleteModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
        }

        /// <summary>Deletes a module from the specified tab and clears host cache.</summary>
        /// <param name="tabId">tabId on which module is deleted.</param>
        /// <param name="moduleId">moduleId that is deleted.</param>
        /// <param name="softDelete">if True, then softdeleted, otherwise harddeleted.</param>
        public static void DeleteTabModule(int tabId, int moduleId, bool softDelete)
        {
			Logger.LogDebug(">>> Entering DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete})", tabId, moduleId, softDelete);
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabModule", tabId, moduleId, softDelete);
            WebApiTestHelper.ClearHostCache();
			Logger.LogDebug("<<< Exiting DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete})", tabId, moduleId, softDelete);
        }

        public static string GetModuleSettingValue(int moduleId, string settingName)
        {
			Logger.LogDebug(">>> Entering GetModuleSettingValue(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            var results = DatabaseHelper.ExecuteStoredProcedure("GetModuleSetting", moduleId, settingName);

			Logger.LogDebug("GetModuleSettingValue(moduleId={ModuleId},settingName={SettingName}): results → {Results}", moduleId, settingName, results);
            if (results.Count > 0 && results[0].ContainsKey("SettingValue"))
            {
                return results[0]["SettingValue"].ToString();
            }

			Logger.LogDebug("<<< Exiting GetModuleSettingValue(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            return null;
        }

        public static void SetModuleSettingValue(int moduleId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering SetModuleSettingValue(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue})", moduleId, settingName, settingValue);
            DatabaseHelper.ExecuteStoredProcedure("DeleteModuleSetting", moduleId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("UpdateModuleSetting", moduleId, settingName, settingValue, -1);
			Logger.LogDebug("<<< Exiting SetModuleSettingValue(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue})", moduleId, settingName, settingValue);
        }

        public static IList<IDictionary<string, object>> GetModulesByFriendlyName(int portalId, string friendlyName)
        {
            var fileContent = SqlScripts.Modulenformation;
            var masterScript = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var script = new StringBuilder(masterScript)
                .Replace(FriendlyNameMarker, friendlyName)
                .ToString();

            var result = DatabaseHelper.ExecuteQuery(script);
            return result;
        }

        public static void SetTabModuleSettingValue(int tabModuleId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering SetTabModuleSettingValue(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue})", tabModuleId, settingName, settingValue);
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabModuleSetting", tabModuleId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("UpdateTabModuleSetting", tabModuleId, settingName, settingValue, -1);
			Logger.LogDebug("<<< Exiting SetTabModuleSettingValue(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue})", tabModuleId, settingName, settingValue);
        }
    }
}
