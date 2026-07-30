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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static DateTime GetTabLastPublishedOn(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetTabLastPublishedOn(tab={Tab})", tab);
            if (tab.HasBeenPublished)
            {
                var versions = TabVersionController.Instance.GetTabVersions(tab.TabID);
                if (versions != null)
				Logger.LogDebug("GetTabLastPublishedOn(tab={Tab}): versions → {Versions}", tab, versions);
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
				Logger.LogDebug("<<< Exiting GetTabLastPublishedOn(tab={Tab})", tab);
                return DateTime.MinValue;
            }
        }

        public static int GetTabWorkflowId(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetTabWorkflowId(tab={Tab})", tab);
			Logger.LogDebug("<<< Exiting GetTabWorkflowId(tab={Tab})", tab);
            return tab.StateID == Null.NullInteger
                ? TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(tab.PortalID)
                : WorkflowStateManager.Instance.GetWorkflowState(tab.StateID).WorkflowID;
        }

        public static bool IsWorkflowCompleted(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering IsWorkflowCompleted(tab={Tab})", tab);
            // If tab exists but ContentItem not, then we create it
            if (tab.ContentItemId == Null.NullInteger && tab.TabID != Null.NullInteger)
            {
                TabController.Instance.CreateContentItem(tab);
                TabController.Instance.UpdateTab(tab);
            }

			Logger.LogDebug("<<< Exiting IsWorkflowCompleted(tab={Tab})", tab);
            return WorkflowEngine.Instance.IsWorkflowCompleted(tab);
        }

        public static string GetTabWorkflowName(TabInfo tab)
        {
			Logger.LogDebug(">>> Entering GetTabWorkflowName(tab={Tab})", tab);
			Logger.LogDebug("<<< Exiting GetTabWorkflowName(tab={Tab})", tab);
            return WorkflowRepository.Instance.GetWorkflow(GetTabWorkflowId(tab)).WorkflowName;
        }
    }
}
