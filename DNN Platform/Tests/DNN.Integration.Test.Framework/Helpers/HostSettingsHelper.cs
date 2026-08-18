// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
	using Microsoft.Extensions.Logging;

    public static class HostSettingsHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static string GetHostSettingValue(string hostSettingName)
        {
			_logger.LogDebug(">>> Entering GetHostSettingValue(hostSettingName={HostSettingName})", hostSettingName);
            var query = string.Format("SELECT TOP(1) SettingValue FROM {{objectQualifier}}HostSettings WHERE SettingName='" + hostSettingName + "';");
			_logger.LogDebug("GetHostSettingValue(hostSettingName={HostSettingName}): query → {Query}", hostSettingName, query);
			_logger.LogDebug("<<< Exiting GetHostSettingValue(hostSettingName={HostSettingName})", hostSettingName);
            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        /// <summary>Adds a new entry to HostSettings table only if it does not exist and optionally clears the site cache or recycles the app.</summary>
        /// <param name="settingName">The name to add.</param>
        /// <param name="settingValue">The value associated with the name.</param>
        /// <param name="clearCache">Whether to clear the cache for the changes to take effect or not (only when record is added).</param>
        /// <param name="recycleAppIfAdded">Whether to recycle the application or no (only when record is added).
        /// If recycling done, no clear cache will be issued.</param>
        /// <returns>True if the entry is added; false otherwise.</returns>
        public static bool AddSettingIfMissing(string settingName, string settingValue, bool clearCache = true,
            bool recycleAppIfAdded = false)
        {
			_logger.LogDebug(">>> Entering AddSettingIfMissing(settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},recycleAppIfAdded={RecycleAppIfAdded})", settingName, settingValue, clearCache, recycleAppIfAdded);
            var currentValue = GetHostSettingValue(settingName);
            if (!string.IsNullOrEmpty(currentValue))
			_logger.LogDebug("AddSettingIfMissing(settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},recycleAppIfAdded={RecycleAppIfAdded}): currentValue → {CurrentValue}", settingName, settingValue, clearCache, recycleAppIfAdded, currentValue);
            {
                return false;
            }

            var query = string.Format(
                @"INSERT INTO {{objectQualifier}}HostSettings (SettingName, SettingValue, SettingIsSecure, CreatedOnDate) " +
                @"VALUES(N'{0}', N'{1}', '0', {{ts: '{2:yyyy-MM-dd HH:mm.ss.fff}'}})",
                settingName.Replace("'", "''"), settingValue.Replace("'", "''"), DateTime.UtcNow);

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("AddSettingIfMissing(settingName,settingValue,clearCache,recycleAppIfAdded)={SettingName},{SettingValue},{ClearCache},{RecycleAppIfAdded}: save query executed in {Arg4} ms", settingName, settingValue, clearCache, recycleAppIfAdded, __logStart.ElapsedMilliseconds);
            DatabaseHelper.ExecuteNonQuery(query);

            if (recycleAppIfAdded)
            {
                WebApiTestHelper.RecycleApplication();
            }
            else if (clearCache)
            {
                WebApiTestHelper.ClearHostCache();
            }

			_logger.LogDebug("<<< Exiting AddSettingIfMissing(settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},recycleAppIfAdded={RecycleAppIfAdded})", settingName, settingValue, clearCache, recycleAppIfAdded);
            return true;
        }
    }
}
