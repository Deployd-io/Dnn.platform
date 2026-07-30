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
    public class GoogleTagManagerEngine : AnalyticsEngineBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override string EngineName
        {
            get
            {
                return "GoogleTagManager";
            }
        }

        /// <inheritdoc />
        public override string RenderScript(string scriptTemplate)
        {
			Logger.LogDebug(">>> Entering RenderScript(scriptTemplate={ScriptTemplate})", scriptTemplate);
            AnalyticsConfiguration config = this.GetConfig();

			Logger.LogDebug("RenderScript(scriptTemplate={ScriptTemplate}): config → {Config}", scriptTemplate, config);
            if (config == null)
            {
                return string.Empty;
            }

            var trackingId = string.Empty;
            var urlParameter = string.Empty;
			Logger.LogDebug("RenderScript(scriptTemplate={ScriptTemplate}): trackingId → {TrackingId}", scriptTemplate, trackingId);
            var trackForAdmin = true;

			Logger.LogDebug("RenderScript(scriptTemplate={ScriptTemplate}): trackForAdmin → {TrackForAdmin}", scriptTemplate, trackForAdmin);
            foreach (AnalyticsSetting setting in config.Settings)
            {
                switch (setting.SettingName.ToLowerInvariant())
                {
                    case "gtmid":
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

            scriptTemplate = scriptTemplate.Replace("[GTM_ID]", trackingId);

			Logger.LogDebug("RenderScript(scriptTemplate={ScriptTemplate}): scriptTemplate → {ScriptTemplate1}", scriptTemplate, scriptTemplate);
			Logger.LogDebug("<<< Exiting RenderScript(scriptTemplate={ScriptTemplate})", scriptTemplate);
            return scriptTemplate;
        }
    }
}
