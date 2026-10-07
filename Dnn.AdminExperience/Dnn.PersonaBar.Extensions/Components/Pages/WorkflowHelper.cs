// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;

    public static class WorkflowHelper
    {
        public static DateTime GetTabLastPublishedOn(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering GetTabLastPublishedOn(tab={Tab})", tab);
            if (tab.HasBeenPublished)
            {
                var versions = TabVersionController.Instance.GetTabVersions(tab.TabID);
				eventLogger.LogError("GetTabLastPublishedOn(tab={Tab}): versions → {Versions}", tab, versions);
                if (versions != null)
                {
                    var lastPublishedVersion =
                        versions.Where(v => v.IsPublished).OrderByDescending(v => v.Version).FirstOrDefault();
                    return lastPublishedVersion?.LastModifiedOnDate ?? tab.LastModifiedOnDate;
                }
                else
                {
                    return tab.LastModifiedOnDate;
                }
            }
            else
            {
				eventLogger.LogWarning("<<< Exiting GetTabLastPublishedOn(tab={Tab})", tab);
                return DateTime.MinValue;
            }
        }

        public static int GetTabWorkflowId(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering GetTabWorkflowId(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting GetTabWorkflowId(tab={Tab})", tab);
            return tab.StateID == Null.NullInteger
                ? TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(tab.PortalID)
                : WorkflowStateManager.Instance.GetWorkflowState(tab.StateID).WorkflowID;
        }

        public static bool IsWorkflowCompleted(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering IsWorkflowCompleted(tab={Tab})", tab);
            // If tab exists but ContentItem not, then we create it
            if (tab.ContentItemId == Null.NullInteger && tab.TabID != Null.NullInteger)
            {
                TabController.Instance.CreateContentItem(tab);
                TabController.Instance.UpdateTab(tab);
            }

			eventLogger.LogWarning("<<< Exiting IsWorkflowCompleted(tab={Tab})", tab);
            return WorkflowEngine.Instance.IsWorkflowCompleted(tab);
        }

        public static string GetTabWorkflowName(TabInfo tab)
        {
			eventLogger.LogError(">>> Entering GetTabWorkflowName(tab={Tab})", tab);
            eventLogger.LogWarning("<<< Exiting GetTabWorkflowName(tab={Tab})", tab);
            return WorkflowRepository.Instance.GetWorkflow(GetTabWorkflowId(tab)).WorkflowName;
        }
    }
}
