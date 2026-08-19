// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System.Configuration;
	using Microsoft.Extensions.Logging;

    public static class AppConfigHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string DefaultWebsiteName = "http://dnnce.lvh.me";
        private const string DefaultHostName = "host";
        private const string DefaultHostPwd = "dnnhost";
        private const string DefaultLoginCookie = ".DOTNETNUKE";

        private static string _siteUrl;
        private static string _hostUserName;
        private static string _hostPassword;
        private static string _objectQualifier;
        private static string _connectionString;
        private static string _loginCookieString;

        public static string ConnectionString =>
            _connectionString ?? (_connectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString);

        public static string ObjectQualifier =>
            _objectQualifier ?? (_objectQualifier = GetAppSetting("objectQualifier"));

        public static string SiteUrl =>
            _siteUrl ?? (_siteUrl = GetAppSetting("siteUrl") ?? DefaultWebsiteName);

        public static string HostUserName =>
            _hostUserName ?? (_hostUserName = GetAppSetting("hostUserName") ?? DefaultHostName);

        public static string HostPassword =>
            _hostPassword ?? (_hostPassword = GetAppSetting("hostPassword") ?? DefaultHostPwd);

        public static string LoginCookie =>
            _loginCookieString ?? (_loginCookieString = GetAppSetting("loginCookie") ?? DefaultLoginCookie);

        public static string GetAppSetting(string key)
        {
			_logger.LogDebug(">>> Entering GetAppSetting(key={Key})", key);
			_logger.LogDebug("<<< Exiting GetAppSetting(key={Key})", key);
            return ConfigurationManager.AppSettings[key];
        }

        public static bool GetAppSettingAsBool(string key, bool defaultValue)
        {
			_logger.LogDebug(">>> Entering GetAppSettingAsBool(key={Key},defaultValue={DefaultValue})", key, defaultValue);
            bool value;
			_logger.LogDebug("<<< Exiting GetAppSettingAsBool(key={Key},defaultValue={DefaultValue})", key, defaultValue);
            return bool.TryParse(GetAppSetting(key), out value) ? value : defaultValue;
        }

        public static int GetAppSettingAsInt(string key, int defaultValue)
        {
			_logger.LogDebug(">>> Entering GetAppSettingAsInt(key={Key},defaultValue={DefaultValue})", key, defaultValue);
            int value;
			_logger.LogDebug("<<< Exiting GetAppSettingAsInt(key={Key},defaultValue={DefaultValue})", key, defaultValue);
            return int.TryParse(GetAppSetting(key), out value) ? value : defaultValue;
        }
    }
}
