// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics
{
    using System;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Analytics.Config;

    /// <inheritdoc />
    public class GoogleAnalytics4Engine : AnalyticsEngineBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GoogleAnalytics4Engine));
        /// <inheritdoc />
        public override string EngineName
        {
            get
            {
                return "GoogleAnalytics4";
            }
        }

        /// <inheritdoc />
        public override string RenderScript(string scriptTemplate)
        {
			Logger.LogError(">>> Entering RenderScript(scriptTemplate={ScriptTemplate})", scriptTemplate);
            AnalyticsConfiguration config = this.GetConfig();

			Logger.LogError("RenderScript(scriptTemplate={ScriptTemplate}): config → {Config}", scriptTemplate, config);
            if (config == null)
            {
                return string.Empty;
            }

            var trackingId = string.Empty;
			Logger.LogError("RenderScript(scriptTemplate={ScriptTemplate}): trackingId → {TrackingId}", scriptTemplate, trackingId);
            var urlParameter = string.Empty;
            var trackForAdmin = true;
			Logger.LogError("RenderScript(scriptTemplate={ScriptTemplate}): trackForAdmin → {TrackForAdmin}", scriptTemplate, trackForAdmin);

            foreach (AnalyticsSetting setting in config.Settings)
            {
                switch (setting.SettingName.ToLowerInvariant())
                {
                    case "ga4id":
                        trackingId = setting.SettingValue;
                        break;
                    case "trackforadmin":
                        if (!bool.TryParse(setting.SettingValue, out trackForAdmin))
                        {
                            trackForAdmin = true;
                        }

                        break;
                }
            }

            if (string.IsNullOrEmpty(trackingId))
            {
                return string.Empty;
            }

            // check whether setting to not track traffic if current user is host user or website administrator.
            if (!trackForAdmin &&
                (UserController.Instance.GetCurrentUserInfo().IsSuperUser
                 ||
                 (PortalSettings.Current != null &&
                  UserController.Instance.GetCurrentUserInfo().IsInRole(PortalSettings.Current.AdministratorRoleName))))
            {
                return string.Empty;
            }

            scriptTemplate = scriptTemplate.Replace("[GA4_ID]", trackingId);
			Logger.LogError("RenderScript(scriptTemplate={ScriptTemplate}): scriptTemplate → {ScriptTemplate1}", scriptTemplate, scriptTemplate);

			Logger.LogWarning("<<< Exiting RenderScript(scriptTemplate={ScriptTemplate})", scriptTemplate);
            return scriptTemplate;
        }
    }
}
