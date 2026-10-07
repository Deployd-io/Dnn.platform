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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabWorkflowSettings));
        private const string DefaultTabWorkflowKey = "DefaultTabWorkflowKey";
        private const string TabWorkflowEnableKey = "TabWorkflowEnabledKey";
        private readonly ITabController tabController;
        private readonly ISystemWorkflowManager systemWorkflowManager;

        /// <summary>Initializes a new instance of the <see cref="TabWorkflowSettings"/> class.</summary>
        public TabWorkflowSettings()
        {
			Logger.LogError(">>> Entering TabWorkflowSettings()");
            this.tabController = TabController.Instance;
			Logger.LogDebug("TabWorkflowSettings(): this.tabController → {Arg0}", this.tabController);
        Logger.LogWarning("<<< Exiting TabWorkflowSettings()");
            this.systemWorkflowManager = SystemWorkflowManager.Instance;
        }

        /// <inheritdoc />
        public int GetDefaultTabWorkflowId(int portalId)
        {
			Logger.LogError(">>> Entering GetDefaultTabWorkflowId(portalId={PortalId})", portalId);
            var workflowId = PortalController.GetPortalSettingAsInteger(DefaultTabWorkflowKey, portalId, Null.NullInteger);
			Logger.LogError("GetDefaultTabWorkflowId(portalId={PortalId}): workflowId → {WorkflowId}", portalId, workflowId);
            if (workflowId == Null.NullInteger)
            {
                var workflow = this.systemWorkflowManager.GetDirectPublishWorkflow(portalId);
				Logger.LogError("GetDefaultTabWorkflowId(portalId={PortalId}): workflow → {Workflow}", portalId, workflow);
                workflowId = (workflow != null) ? workflow.WorkflowID : Null.NullInteger;
                if (workflowId != Null.NullInteger)
                {
                    PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
                }
            }

			Logger.LogWarning("<<< Exiting GetDefaultTabWorkflowId(portalId={PortalId})", portalId);
            return workflowId;
        }

        /// <inheritdoc />
        public void SetDefaultTabWorkflowId(int portalId, int workflowId)
        {
			Logger.LogError(">>> Entering SetDefaultTabWorkflowId(portalId={PortalId},workflowId={WorkflowId})", portalId, workflowId);
            PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
			Logger.LogWarning("<<< Exiting SetDefaultTabWorkflowId(portalId={PortalId},workflowId={WorkflowId})", portalId, workflowId);
        }

        /// <inheritdoc />
        public void SetWorkflowEnabled(int portalId, bool enabled)
        {
			Logger.LogError(">>> Entering SetWorkflowEnabled(portalId={PortalId},enabled={Enabled})", portalId, enabled);
            Requires.NotNegative("portalId", portalId);

            PortalController.UpdatePortalSetting(portalId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture), true);
			Logger.LogWarning("<<< Exiting SetWorkflowEnabled(portalId={PortalId},enabled={Enabled})", portalId, enabled);
        }

        /// <inheritdoc />
        public void SetWorkflowEnabled(int portalId, int tabId, bool enabled)
        {
			Logger.LogError(">>> Entering SetWorkflowEnabled(portalId={PortalId},tabId={TabId},enabled={Enabled})", portalId, tabId, enabled);
            Requires.NotNegative("tabId", tabId);

            this.tabController.UpdateTabSetting(tabId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture));
			Logger.LogWarning("<<< Exiting SetWorkflowEnabled(portalId={PortalId},tabId={TabId},enabled={Enabled})", portalId, tabId, enabled);
        }

        /// <inheritdoc />
        public bool IsWorkflowEnabled(int portalId, int tabId)
        {
			Logger.LogError(">>> Entering IsWorkflowEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
            if (!this.IsWorkflowEnabled(portalId))
            {
                return false;
            }

            var tabInfo = this.tabController.GetTab(tabId, portalId);
			Logger.LogError("IsWorkflowEnabled(portalId={PortalId},tabId={TabId}): tabInfo → {TabInfo}", portalId, tabId, tabInfo);
            var settings = this.tabController.GetTabSettings(tabId);
			Logger.LogError("IsWorkflowEnabled(portalId={PortalId},tabId={TabId}): settings → {Settings}", portalId, tabId, settings);

			Logger.LogWarning("<<< Exiting IsWorkflowEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
			Logger.LogWarning("<<< Exiting IsWorkflowEnabled(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new TabWorkflowSettings();
        }
    }
}
