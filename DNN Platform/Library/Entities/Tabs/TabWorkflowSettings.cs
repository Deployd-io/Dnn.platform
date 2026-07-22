// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    public class TabWorkflowSettings : ServiceLocator<ITabWorkflowSettings, TabWorkflowSettings>, ITabWorkflowSettings
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string DefaultTabWorkflowKey = "DefaultTabWorkflowKey";
        private const string TabWorkflowEnableKey = "TabWorkflowEnabledKey";
        private readonly ITabController tabController;
        private readonly ISystemWorkflowManager systemWorkflowManager;

        /// <summary>Initializes a new instance of the <see cref="TabWorkflowSettings"/> class.</summary>
        public TabWorkflowSettings()
        {
			Logger.LogDebug(">>> Entering TabWorkflowSettings()");
            this.tabController = TabController.Instance;
            this.systemWorkflowManager = SystemWorkflowManager.Instance;
			Logger.LogDebug("<<< Exiting TabWorkflowSettings()");
			Logger.LogDebug("TabWorkflowSettings(): this.tabController → {Arg0}", this.tabController);
			Logger.LogDebug("TabWorkflowSettings(): systemWorkflowManager → {SystemWorkflowManager}", systemWorkflowManager);
			Logger.LogDebug("TabWorkflowSettings(): this.systemWorkflowManager → {Arg0}", this.systemWorkflowManager);
			Logger.LogDebug("TabWorkflowSettings(): tabController → {TabController}", tabController);
        }

        /// <inheritdoc />
        public int GetDefaultTabWorkflowId(int portalId)
        {
			Logger.LogDebug(">>> Entering GetDefaultTabWorkflowId(portalId={PortalId})", portalId);
            var workflowId = PortalController.GetPortalSettingAsInteger(DefaultTabWorkflowKey, portalId, Null.NullInteger);
            if (workflowId == Null.NullInteger)
			Logger.LogDebug("GetDefaultTabWorkflowId(portalId={PortalId}): workflowId → {WorkflowId}", portalId, workflowId);
            {
                var workflow = this.systemWorkflowManager.GetDirectPublishWorkflow(portalId);
                workflowId = (workflow != null) ? workflow.WorkflowID : Null.NullInteger;
				Logger.LogDebug("GetDefaultTabWorkflowId(portalId={PortalId}): workflow → {Workflow}", portalId, workflow);
                if (workflowId != Null.NullInteger)
                {
                    PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
                }
            }

			Logger.LogDebug("<<< Exiting GetDefaultTabWorkflowId(portalId={PortalId})", portalId);
            return workflowId;
        }

        /// <inheritdoc />
        public void SetDefaultTabWorkflowId(int portalId, int workflowId)
        {
            PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
        }

        /// <inheritdoc />
        public void SetWorkflowEnabled(int portalId, bool enabled)
        {
			Logger.LogDebug(">>> Entering SetWorkflowEnabled(portalId={PortalId},enabled={Enabled})", portalId, enabled);
            Requires.NotNegative("portalId", portalId);

            PortalController.UpdatePortalSetting(portalId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture), true);
			Logger.LogDebug("<<< Exiting SetWorkflowEnabled(portalId={PortalId},enabled={Enabled})", portalId, enabled);
        }

        /// <inheritdoc />
        public void SetWorkflowEnabled(int portalId, int tabId, bool enabled)
        {
			Logger.LogDebug(">>> Entering SetWorkflowEnabled(portalId={PortalId},tabId={TabId},enabled={Enabled})", portalId, tabId, enabled);
            Requires.NotNegative("tabId", tabId);

            this.tabController.UpdateTabSetting(tabId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting SetWorkflowEnabled(portalId={PortalId},tabId={TabId},enabled={Enabled})", portalId, tabId, enabled);
        }

        /// <inheritdoc />
        public bool IsWorkflowEnabled(int portalId, int tabId)
        {
			Logger.LogDebug(">>> Entering IsWorkflowEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
            if (!this.IsWorkflowEnabled(portalId))
            {
                return false;
            }

            var tabInfo = this.tabController.GetTab(tabId, portalId);
            var settings = this.tabController.GetTabSettings(tabId);
			Logger.LogDebug("IsWorkflowEnabled(portalId={PortalId},tabId={TabId}): tabInfo → {TabInfo}", portalId, tabId, tabInfo);
			Logger.LogDebug("IsWorkflowEnabled(portalId={PortalId},tabId={TabId}): settings → {Settings}", portalId, tabId, settings);

			Logger.LogDebug("<<< Exiting IsWorkflowEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
			Logger.LogDebug("<<< Exiting IsWorkflowEnabled(portalId={PortalId})", portalId);
            return !this.tabController.IsHostOrAdminPage(tabInfo) && (settings[TabWorkflowEnableKey] == null || Convert.ToBoolean(settings[TabWorkflowEnableKey], CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        public bool IsWorkflowEnabled(int portalId)
        {
            if (portalId == Null.NullInteger)
            {
                return false;
            }

            return Convert.ToBoolean(PortalController.GetPortalSetting(TabWorkflowEnableKey, portalId, bool.FalseString));
        }

        /// <inheritdoc />
        protected override Func<ITabWorkflowSettings> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new TabWorkflowSettings();
        }
    }
}
