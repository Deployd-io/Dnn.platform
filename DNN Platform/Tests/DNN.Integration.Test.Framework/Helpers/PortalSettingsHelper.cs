// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    public static class PortalSettingsHelper
    {
        /// <summary>Gets a portal setting value.</summary>
        /// <param name="settingName">The name of the setting.</param>
        /// <param name="portalId">The optional PortalId, default to 0.</param>
        /// <returns>The string value of the setting.</returns>
        public static string GetPortalSetting(string settingName, int portalId = 0)
        {
			Logger.LogDebug(">>> Entering GetPortalSetting(settingName={SettingName},portalId={PortalId})", settingName, portalId);
            var query = string.Format(
                @"SELECT SettingValue 
                                        FROM {{objectQualifier}}PortalSettings 
                                        WHERE SettingName = '{0}' 
                                            AND PortalId = {1}", settingName, portalId);

			Logger.LogDebug("GetPortalSetting(settingName={SettingName},portalId={PortalId}): PortalId → {PortalId1}", settingName, portalId, PortalId);
			Logger.LogDebug("<<< Exiting GetPortalSetting(settingName={SettingName},portalId={PortalId})", settingName, portalId);
            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        /// <summary>Sets a portal setting value, adding or updating the setting as required.</summary>
        /// <param name="settingName">The name of the setting.</param>
        /// <param name="settingValue">The value of the setting.</param>
        /// <param name="portalId">The optional PortalId, default to 0.</param>
        /// <param name="isSecure">This flag specifies whether the value is encrypted or not, defaults to false.</param>
        public static void SetPortalSetting(string settingName, string settingValue, int portalId = 0, bool isSecure = false)
        {
			Logger.LogDebug(">>> Entering SetPortalSetting(settingName={SettingName},settingValue={SettingValue},portalId={PortalId},isSecure={IsSecure})", settingName, settingValue, portalId, isSecure);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var query = string.Format(
                @"MERGE INTO {{objectQualifier}}PortalSettings s
                    USING (SELECT {2} PortalId, '{0}' SettingName, '{1}' SettingValue, {3} Sec) AS v
                    ON s.SettingName = v.SettingName
                    WHEN MATCHED THEN UPDATE SET s.SettingValue = v.SettingValue, SettingIsSecure = v.Sec
                    WHEN NOT MATCHED THEN INSERT (PortalId, SettingName, SettingValue, SettingIsSecure)
				                          VALUES (v.PortalId, v.SettingName, v.SettingValue, v.Sec);",
                settingName, settingValue, portalId, isSecure ? "1" : "0");

            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();
			Logger.LogInformation("SetPortalSetting(settingName,settingValue,portalId,isSecure)={SettingName},{SettingValue},{PortalId},{IsSecure}: database query executed in {Arg4} ms", settingName, settingValue, portalId, isSecure, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting SetPortalSetting(settingName={SettingName},settingValue={SettingValue},portalId={PortalId},isSecure={IsSecure})", settingName, settingValue, portalId, isSecure);
        }
    }
}
