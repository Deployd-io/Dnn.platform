// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;

    /// <summary>The Config class provides a central area for management of Module Configuration Settings.</summary>
    [Serializable]
    public class OAuthConfigBase : AuthenticationConfigBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string CacheKey = "Authentication";

        /// <summary>Initializes a new instance of the <see cref="OAuthConfigBase"/> class.</summary>
        /// <param name="service">The service name (used as a prefix for settings and cache keys).</param>
        /// <param name="portalId">The portal ID.</param>
        protected OAuthConfigBase(string service, int portalId)
            : base(portalId)
        {
			Logger.LogDebug(">>> Entering OAuthConfigBase(service={Service},portalId={PortalId})", service, portalId);
            this.Service = service;

			Logger.LogDebug("OAuthConfigBase(service={Service},portalId={PortalId}): this.Service → {Arg2}", service, portalId, this.Service);
            var portalApiKey = PortalController.GetPortalSetting(this.Service + "_APIKey", portalId, string.Empty);
            var hostApiKey = string.Empty;
			Logger.LogDebug("OAuthConfigBase(service={Service},portalId={PortalId}): portalApiKey → {PortalApiKey}", service, portalId, portalApiKey);
			Logger.LogDebug("OAuthConfigBase(service={Service},portalId={PortalId}): hostApiKey → {HostApiKey}", service, portalId, hostApiKey);

            if (string.IsNullOrEmpty(portalApiKey))
            {
                hostApiKey = HostController.Instance.GetString(this.Service + "_APIKey", string.Empty);
                this.HostConfig = !string.IsNullOrEmpty(hostApiKey);
            }
            else
            {
                this.HostConfig = false;
				Logger.LogDebug("OAuthConfigBase(service={Service},portalId={PortalId}): this.HostConfig → {Arg2}", service, portalId, this.HostConfig);
            }

            if (this.HostConfig)
            {
                this.APIKey = hostApiKey;
				Logger.LogDebug("OAuthConfigBase(service={Service},portalId={PortalId}): this.APIKey → {Arg2}", service, portalId, this.APIKey);
                this.APISecret = HostController.Instance.GetString(this.Service + "_APISecret", string.Empty);
                this.Enabled = HostController.Instance.GetBoolean(this.Service + "_Enabled", false);
            }
            else
            {
                this.APIKey = portalApiKey;
                this.APISecret = PortalController.GetPortalSetting(this.Service + "_APISecret", portalId, string.Empty);
                this.Enabled = PortalController.GetPortalSettingAsBoolean(this.Service + "_Enabled", portalId, false);
            }
			Logger.LogDebug("<<< Exiting OAuthConfigBase(service={Service},portalId={PortalId})", service, portalId);
        }

        public string APIKey { get; set; }

        public string APISecret { get; set; }

        public bool Enabled { get; set; }

        public bool HostConfig { get; set; }

        protected string Service { get; set; }

        public static void ClearConfig(string service, int portalId)
        {
			Logger.LogDebug(">>> Entering ClearConfig(service={Service},portalId={PortalId})", service, portalId);
            DataCache.RemoveCache(GetCacheKey(service, portalId));
			Logger.LogDebug("<<< Exiting ClearConfig(service={Service},portalId={PortalId})", service, portalId);
        }

        public static OAuthConfigBase GetConfig(string service, int portalId)
        {
			Logger.LogDebug(">>> Entering GetConfig(service={Service},portalId={PortalId})", service, portalId);
            string key = GetCacheKey(service, portalId);
            var config = (OAuthConfigBase)DataCache.GetCache(key);
            if (config == null)
			Logger.LogDebug("GetConfig(service={Service},portalId={PortalId}): config → {Config}", service, portalId, config);
            {
                config = new OAuthConfigBase(service, portalId);
                DataCache.SetCache(key, config);
            }

			Logger.LogDebug("<<< Exiting GetConfig(service={Service},portalId={PortalId})", service, portalId);
            return config;
        }

        public static void UpdateConfig(OAuthConfigBase config)
        {
            if (config.HostConfig)
            {
                HostController.Instance.Update(config.Service + "_APIKey", config.APIKey, true);
                HostController.Instance.Update(config.Service + "_APISecret", config.APISecret, true);
                HostController.Instance.Update(config.Service + "_Enabled", config.Enabled.ToString(CultureInfo.InvariantCulture), true);
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_APIKey");
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_APISecret");
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_Enabled");
            }
            else
            {
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_APIKey", config.APIKey);
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_APISecret", config.APISecret);
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_Enabled", config.Enabled.ToString(CultureInfo.InvariantCulture));
            }

            ClearConfig(config.Service, config.PortalID);
        }

        private static string GetCacheKey(string service, int portalId)
        {
			Logger.LogDebug(">>> Entering GetCacheKey(service={Service},portalId={PortalId})", service, portalId);
			Logger.LogDebug("<<< Exiting GetCacheKey(service={Service},portalId={PortalId})", service, portalId);
            return CacheKey + "." + service + "_" + portalId;
        }
    }
}
