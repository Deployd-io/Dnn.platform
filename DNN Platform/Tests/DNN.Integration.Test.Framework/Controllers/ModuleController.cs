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
	using Microsoft.Extensions.Logging;

    public static class ModuleController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string PortalIdMarker = @"'$[portal_id]'";
        private const string RoleIdMarker = @"'$[role_id]'";
        private const string ModuleIdMarker = @"'$[module_id]'";
        private const string PermissionCodeMarker = @"$[permission_code]";
        private const string PermissionKeyMarker = @"$[permission_key]";
        private const string FriendlyNameMarker = @"$[friendly_name]";

        public static int AddModulePermission(int roleId, int moduleId, string permissionCode, string permissionKey, int portalId = 0)
        {
			_logger.LogDebug(">>> Entering AddModulePermission(roleId={RoleId},moduleId={ModuleId},permissionCode={PermissionCode},permissionKey={PermissionKey},portalId={PortalId})", roleId, moduleId, permissionCode, permissionKey, portalId);
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
			_logger.LogDebug("AddModulePermission(roleId={RoleId},moduleId={ModuleId},permissionCode={PermissionCode},permissionKey={PermissionKey},portalId={PortalId}): result → {Result}", roleId, moduleId, permissionCode, permissionKey, portalId, result);

            if (result.Count > 0 && result[0].ContainsKey("ModulePermissionId"))
            {
			_logger.LogDebug("<<< Exiting AddModulePermission(roleId={RoleId},moduleId={ModuleId},permissionCode={PermissionCode},permissionKey={PermissionKey},portalId={PortalId})", roleId, moduleId, permissionCode, permissionKey, portalId);
                return int.Parse(result[0]["ModulePermissionId"].ToString());
            }

            return -1;
        }

        public static void DeleteModulePermission(int modulePermissionId)
        {
			_logger.LogDebug(">>> Entering DeleteModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
            DatabaseHelper.ExecuteStoredProcedure("DeleteModulePermission", modulePermissionId);
			_logger.LogDebug("<<< Exiting DeleteModulePermission(modulePermissionId={ModulePermissionId})", modulePermissionId);
        }

        /// <summary>Deletes a module from the specified tab and clears host cache.</summary>
        /// <param name="tabId">tabId on which module is deleted.</param>
        /// <param name="moduleId">moduleId that is deleted.</param>
        /// <param name="softDelete">if True, then softdeleted, otherwise harddeleted.</param>
        public static void DeleteTabModule(int tabId, int moduleId, bool softDelete)
        {
			_logger.LogDebug(">>> Entering DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete})", tabId, moduleId, softDelete);
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabModule", tabId, moduleId, softDelete);
            WebApiTestHelper.ClearHostCache();
			_logger.LogDebug("<<< Exiting DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete})", tabId, moduleId, softDelete);
        }

        public static string GetModuleSettingValue(int moduleId, string settingName)
        {
			_logger.LogDebug(">>> Entering GetModuleSettingValue(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            var results = DatabaseHelper.ExecuteStoredProcedure("GetModuleSetting", moduleId, settingName);

			_logger.LogDebug("GetModuleSettingValue(moduleId={ModuleId},settingName={SettingName}): results → {Results}", moduleId, settingName, results);
            if (results.Count > 0 && results[0].ContainsKey("SettingValue"))
            {
			_logger.LogDebug("<<< Exiting GetModuleSettingValue(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
                return results[0]["SettingValue"].ToString();
            }

            return null;
        }

        public static void SetModuleSettingValue(int moduleId, string settingName, string settingValue)
        {
			_logger.LogDebug(">>> Entering SetModuleSettingValue(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue})", moduleId, settingName, settingValue);
            DatabaseHelper.ExecuteStoredProcedure("DeleteModuleSetting", moduleId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("UpdateModuleSetting", moduleId, settingName, settingValue, -1);
			_logger.LogDebug("<<< Exiting SetModuleSettingValue(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue})", moduleId, settingName, settingValue);
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
			_logger.LogDebug(">>> Entering SetTabModuleSettingValue(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue})", tabModuleId, settingName, settingValue);
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabModuleSetting", tabModuleId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("UpdateTabModuleSetting", tabModuleId, settingName, settingValue, -1);
			_logger.LogDebug("<<< Exiting SetTabModuleSettingValue(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue})", tabModuleId, settingName, settingValue);
        }
    }
}
