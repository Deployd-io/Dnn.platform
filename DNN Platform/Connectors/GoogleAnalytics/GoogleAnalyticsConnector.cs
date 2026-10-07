// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Connectors.GoogleAnalytics
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Analytics.Config;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
	using Microsoft.Extensions.Logging;

    /// <summary>Connector to provide configuration for Google Analytics support.</summary>
    public class GoogleAnalyticsConnector : IConnector
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string DefaultDisplayName = "Google Analytics (Legacy UA)";

        private string displayName;

        /// <inheritdoc />
        public string Name
        {
            get { return "Core Google Analytics Connector"; }
        }

        /// <inheritdoc />
        public string IconUrl
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics/Images/GoogleAnalytics_32X32_Standard.png"; }
        }

        /// <inheritdoc />
        public string PluginFolder
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics/"; }
        }

        /// <inheritdoc />
        public bool IsEngageConnector
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public ConnectorCategories Type => ConnectorCategories.Analytics;

        /// <inheritdoc />
        // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
        public bool SupportsMultiple => false;

        /// <inheritdoc />
        public string DisplayName
        {
            get => string.IsNullOrEmpty(this.displayName) ? DefaultDisplayName : this.displayName;
            set => this.displayName = value;
        }

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public IEnumerable<IConnector> GetConnectors(int portalId)
        {
			_logger.LogError(">>> Entering GetConnectors(portalId={PortalId})", portalId);
            _logger.LogWarning("<<< Exiting GetConnectors(portalId={PortalId})", portalId);
            return new List<IConnector> { this };
        }

        /// <inheritdoc />
        public void DeleteConnector(int portalId)
        {
		_logger.LogError(">>> Entering DeleteConnector(portalId={PortalId})", portalId);
        _logger.LogWarning("<<< Exiting DeleteConnector(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public bool HasConfig(int portalId)
        {
			_logger.LogError(">>> Entering HasConfig(portalId={PortalId})", portalId);
            IDictionary<string, string> config = this.GetConfig(portalId);

			_logger.LogError("HasConfig(portalId={PortalId}): config → {Config}", portalId, config);
			_logger.LogWarning("<<< Exiting HasConfig(portalId={PortalId})", portalId);
            return config.ContainsKey("TrackingID") && !string.IsNullOrEmpty(config["TrackingID"]);
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetConfig(int portalId)
        {
            var analyticsConfig = AnalyticsConfiguration.GetConfig("GoogleAnalytics");
            var portalSettings = new PortalSettings(portalId);

            // Important, knockout handles empty strings as false and any other string as true
            // so we need to pass empty strings when we mean false, however it passes us back the string "false"
            // when saving the settings in the SaveConfig method, so we need to handle that case too
            var trackingId = string.Empty;
            var urlParameter = string.Empty;
            var trackForAdmin = string.Empty;
            var anonymizeIp = string.Empty;
            var trackUserId = string.Empty;

            if (analyticsConfig != null)
            {
                foreach (AnalyticsSetting setting in analyticsConfig.Settings)
                {
                    switch (setting.SettingName.ToUpperInvariant())
                    {
                        case "TRACKINGID":
                            trackingId = setting.SettingValue;
                            break;
                        case "URLPARAMETER":
                            urlParameter = setting.SettingValue;
                            break;
                        case "TRACKFORADMIN":
                            trackForAdmin = HandleCustomBoolean(setting.SettingValue);
                            break;
                        case "ANONYMIZEIP":
                            anonymizeIp = HandleCustomBoolean(setting.SettingValue);
                            break;
                        case "TRACKUSERID":
                            trackUserId = HandleCustomBoolean(setting.SettingValue);
                            break;
                    }
                }
            }

            if (portalSettings.DataConsentActive)
            {
                anonymizeIp = "true";
            }

            var configItems = new Dictionary<string, string>
            {
                { "TrackingID", trackingId },
                { "UrlParameter", urlParameter },
                { "TrackAdministrators", trackForAdmin },
                { "AnonymizeIp", anonymizeIp },
                { "TrackUserId", trackUserId },
                { "DataConsent", HandleCustomBoolean(portalSettings.DataConsentActive.ToString()) },
                { "isDeactivating", HandleCustomBoolean("false") },
            };

            return configItems;
        }

        /// <inheritdoc />
        public bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage)
        {
			_logger.LogError(">>> Entering SaveConfig(portalId={PortalId},values={Values},validated={Validated})", portalId, values, validated);
            // Delete / Deactivation functionality added into SaveConfig because
            // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
            customErrorMessage = string.Empty;
			_logger.LogError("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): customErrorMessage → {CustomErrorMessage}", portalId, values, validated, customErrorMessage);
			_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): customErrorMessage → {CustomErrorMessage}", portalId, values, validated, customErrorMessage);

            try
            {
                if (!bool.TryParse(values["isDeactivating"].ToLowerInvariant(), out var isDeactivating))
                {
                    isDeactivating = false;
					_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): isDeactivating → {IsDeactivating}", portalId, values, validated, isDeactivating);
                }

                string trackingId;
                string urlParameter;
                string trackForAdmin;
                string anonymizeIp;
                string trackUserId;

                var isValid = true;
				_logger.LogError("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): isValid → {IsValid}", portalId, values, validated, isValid);

                if (isDeactivating)
                {
                    trackingId = null;
					_logger.LogError("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): trackingId → {TrackingId}", portalId, values, validated, trackingId);
                    urlParameter = null;
					_logger.LogError("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): urlParameter → {UrlParameter}", portalId, values, validated, urlParameter);
					_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): urlParameter → {UrlParameter}", portalId, values, validated, urlParameter);
                    trackForAdmin = null;
					_logger.LogError("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): trackForAdmin → {TrackForAdmin}", portalId, values, validated, trackForAdmin);
                    anonymizeIp = null;
					_logger.LogError("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): anonymizeIp → {AnonymizeIp}", portalId, values, validated, anonymizeIp);
                    trackUserId = null;
                }
                else
                {
                    trackingId = values["TrackingID"] != null ? values["TrackingID"].ToUpperInvariant().Trim() : string.Empty;
					_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): trackingId → {TrackingId}", portalId, values, validated, trackingId);
                    urlParameter = values["UrlParameter"]?.Trim() ?? string.Empty;
                    trackForAdmin = values["TrackAdministrators"] != null ? values["TrackAdministrators"].ToLowerInvariant().Trim() : string.Empty;
                    anonymizeIp = values["AnonymizeIp"] != null ? values["AnonymizeIp"].ToLowerInvariant().Trim() : string.Empty;
                    trackUserId = values["TrackUserId"] != null ? values["TrackUserId"].ToLowerInvariant().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(trackingId))
                    {
                        isValid = false;
						_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): anonymizeIp → {AnonymizeIp}", portalId, values, validated, anonymizeIp);
						_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): trackUserId → {TrackUserId}", portalId, values, validated, trackUserId);
                        customErrorMessage = Localization.GetString("TrackingCodeFormat.ErrorMessage", Constants.LocalResourceFile);
                    }
                }

                if (isValid)
                {
                    var config = new AnalyticsConfiguration
                    {
                        Settings = new AnalyticsSettingCollection(),
                    };

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackingId",
                        SettingValue = trackingId,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "UrlParameter",
                        SettingValue = urlParameter,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackForAdmin",
                        SettingValue = trackForAdmin,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "AnonymizeIp",
                        SettingValue = anonymizeIp,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackUserId",
                        SettingValue = trackUserId,
                    });

                    AnalyticsConfiguration.SaveConfig("GoogleAnalytics", config);
                }

                return isValid;
            }
            catch (Exception ex)
            {
				_logger.LogError("Exception in SaveConfig(portalId={PortalId},values={Values},validated={Validated}): {Arg3}", portalId, values, validated, ex.Message);
                Exceptions.LogException(ex);
				_logger.LogWarning("<<< Exiting SaveConfig(portalId={PortalId},values={Values},validated={Validated})", portalId, values, validated);
                return false;
            }
        }

        /// <summary>
        /// Handles custom conversion from "true" => "true"
        /// Anything else to "" to support the strange knockout handling of string as booleans.
        /// </summary>
        /// <param name="value">The string representing a boolean.</param>
        /// <returns>The string representing a boolean after the correction.</returns>
        private static string HandleCustomBoolean(string value)
        {
			_logger.LogError(">>> Entering HandleCustomBoolean(value={Value})", value);
            if ((value ?? string.Empty).Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return "true";
            }

			_logger.LogWarning("<<< Exiting HandleCustomBoolean(value={Value})", value);
            return string.Empty;
        }
    }
}
