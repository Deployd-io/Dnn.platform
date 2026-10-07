// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs.Dto;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;

    public class TabChangeSettings : ServiceLocator<ITabChangeSettings, TabChangeSettings>, ITabChangeSettings
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabChangeSettings));
        /// <inheritdoc />
        public bool IsChangeControlEnabled(int portalId, int tabId)
        {
			Logger.LogError(">>> Entering IsChangeControlEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
            if (portalId == Null.NullInteger)
            {
                return false;
            }

            var isVersioningEnabled = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId);
			Logger.LogError("IsChangeControlEnabled(portalId={PortalId},tabId={TabId}): isVersioningEnabled → {IsVersioningEnabled}", portalId, tabId, isVersioningEnabled);
            var isWorkflowEnable = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId);
			Logger.LogError("IsChangeControlEnabled(portalId={PortalId},tabId={TabId}): isWorkflowEnable → {IsWorkflowEnable}", portalId, tabId, isWorkflowEnable);
			Logger.LogWarning("<<< Exiting IsChangeControlEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
            return isVersioningEnabled || isWorkflowEnable;
        }

        /// <inheritdoc />
        public ChangeControlState GetChangeControlState(int portalId, int tabId)
        {
			Logger.LogError(">>> Entering GetChangeControlState(portalId={PortalId},tabId={TabId})", portalId, tabId);
			Logger.LogWarning("<<< Exiting GetChangeControlState(portalId={PortalId},tabId={TabId})", portalId, tabId);
            return new ChangeControlState
            {
                PortalId = portalId,
                TabId = tabId,
                IsVersioningEnabledForTab = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId),
                IsWorkflowEnabledForTab = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId),
            };
        }

        /// <inheritdoc />
        protected override Func<ITabChangeSettings> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new TabChangeSettings();
        }
    }
}
