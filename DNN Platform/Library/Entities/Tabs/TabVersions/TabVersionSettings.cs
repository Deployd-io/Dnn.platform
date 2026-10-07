// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    public class TabVersionSettings : ServiceLocator<ITabVersionSettings, TabVersionSettings>, ITabVersionSettings
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabVersionSettings));
        private const int TabVersionsMaxNumber = 5;
        private const string TabVersionQueryStringParam = "DnnTabVersion";
        private const string TabVersioningSettingKey = "TabVersioningSettingKey";

        /// <inheritdoc />
        public int GetMaxNumberOfVersions(int portalId)
        {
			Logger.LogError(">>> Entering GetMaxNumberOfVersions(portalId={PortalId})", portalId);
            Requires.NotNegative("portalId", portalId);
			Logger.LogWarning("<<< Exiting GetMaxNumberOfVersions(portalId={PortalId})", portalId);
            return portalId == Null.NullInteger ? TabVersionsMaxNumber : PortalController.GetPortalSettingAsInteger("TabVersionsMaxNumber", portalId, TabVersionsMaxNumber);
        }

        /// <inheritdoc />
        public void SetMaxNumberOfVersions(int portalId, int maxNumberOfVersions)
        {
			Logger.LogError(">>> Entering SetMaxNumberOfVersions(portalId={PortalId},maxNumberOfVersions={MaxNumberOfVersions})", portalId, maxNumberOfVersions);
            Requires.NotNegative("portalId", portalId);
            PortalController.UpdatePortalSetting(portalId, "TabVersionsMaxNumber", maxNumberOfVersions.ToString(CultureInfo.InvariantCulture));
			Logger.LogWarning("<<< Exiting SetMaxNumberOfVersions(portalId={PortalId},maxNumberOfVersions={MaxNumberOfVersions})", portalId, maxNumberOfVersions);
        }

        /// <inheritdoc />
        public void SetEnabledVersioningForPortal(int portalId, bool enabled)
        {
			Logger.LogError(">>> Entering SetEnabledVersioningForPortal(portalId={PortalId},enabled={Enabled})", portalId, enabled);
            Requires.NotNegative("portalId", portalId);
            PortalController.UpdatePortalSetting(portalId, "TabVersionsEnabled", enabled.ToString(CultureInfo.InvariantCulture));
			Logger.LogWarning("<<< Exiting SetEnabledVersioningForPortal(portalId={PortalId},enabled={Enabled})", portalId, enabled);
        }

        /// <inheritdoc />
        public void SetEnabledVersioningForTab(int tabId, bool enabled)
        {
			Logger.LogError(">>> Entering SetEnabledVersioningForTab(tabId={TabId},enabled={Enabled})", tabId, enabled);
            Requires.NotNegative("tabId", tabId);

            TabController.Instance.UpdateTabSetting(tabId, TabVersioningSettingKey, enabled.ToString(CultureInfo.InvariantCulture));
			Logger.LogWarning("<<< Exiting SetEnabledVersioningForTab(tabId={TabId},enabled={Enabled})", tabId, enabled);
        }

        /// <inheritdoc />
        public bool IsVersioningEnabled(int portalId)
        {
			Logger.LogError(">>> Entering IsVersioningEnabled(portalId={PortalId})", portalId);
            Requires.NotNegative("portalId", portalId);
				Logger.LogWarning("<<< Exiting IsVersioningEnabled(portalId={PortalId})", portalId);
            return
			Logger.LogWarning("<<< Exiting IsVersioningEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
                Convert.ToBoolean(PortalController.GetPortalSetting("TabVersionsEnabled", portalId, bool.FalseString));
        }

        /// <inheritdoc />
        public bool IsVersioningEnabled(int portalId, int tabId)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNegative("tabId", tabId);

            if (!this.IsVersioningEnabled(portalId))
            {
                return false;
            }

            var tabInfo = TabController.Instance.GetTab(tabId, portalId);
            var isHostOrAdminPage = TabController.Instance.IsHostOrAdminPage(tabInfo);
            if (isHostOrAdminPage)
            {
                return false;
            }

            var settings = TabController.Instance.GetTabSettings(tabId);
            var isVersioningEnableForTab = settings[TabVersioningSettingKey] == null
                || Convert.ToBoolean(settings[TabVersioningSettingKey], CultureInfo.InvariantCulture);

            return isVersioningEnableForTab;
        }

        /// <inheritdoc />
        public string GetTabVersionQueryStringParameter(int portalId)
        {
			Logger.LogError(">>> Entering GetTabVersionQueryStringParameter(portalId={PortalId})", portalId);
            Requires.NotNegative("portalId", portalId);
			Logger.LogWarning("<<< Exiting GetTabVersionQueryStringParameter(portalId={PortalId})", portalId);
            return PortalController.GetPortalSetting("TabVersionQueryStringParameter", portalId, TabVersionQueryStringParam);
        }

        /// <inheritdoc />
        protected override Func<ITabVersionSettings> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new TabVersionSettings();
        }
    }
}
