// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;

    /// <summary>
    /// Discards the tab workflow, no matter what the current state is. It also sends a system notification to the user
    /// who submitted the workflow to inform them about the discard workflow action.
    /// </summary>
    internal class DiscardWorkflow : TabActionBase, IWorkflowAction
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DiscardWorkflow));
        /// <summary>
        /// Discards the tab version.
        /// </summary>
        /// <param name="stateTransaction">The state transaction.</param>
        public void DoActionOnStateChanging(StateTransaction stateTransaction)
        {
			Logger.LogDebug(">>> Entering DoActionOnStateChanging(stateTransaction={StateTransaction})", stateTransaction);
            var tab = GetTab(stateTransaction.ContentItemId);
            if (tab == null)
            {
                return;
            }

            TabVersionBuilder.Instance.Discard(tab.TabID, stateTransaction.UserId);
			Logger.LogDebug("<<< Exiting DoActionOnStateChanging(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <summary>
        /// Deletes the tab when necessary.
        /// </summary>
        /// <param name="stateTransaction">The state transaction.</param>
        public void DoActionOnStateChanged(StateTransaction stateTransaction)
        {
			Logger.LogDebug(">>> Entering DoActionOnStateChanged(stateTransaction={StateTransaction})", stateTransaction);
            var tab = GetTab(stateTransaction.ContentItemId);
            if (tab == null)
            {
                return;
            }

            if (HasOnlyOneTabVersion(tab))
            {
                TabController.Instance.SoftDeleteTab(tab.TabID, new PortalSettings(tab.PortalID));
                TabController.Instance.DeleteTab(tab.TabID, tab.PortalID);
            }

            DataCache.RemoveCache($"Tab_Tabs{tab.PortalID}");
			Logger.LogDebug("<<< Exiting DoActionOnStateChanged(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
            => new ActionMessage()
            {
				Logger.LogDebug(">>> Entering GetActionMessage(stateTransaction={StateTransaction},currentState={CurrentState})", stateTransaction, currentState);
                Subject = GetString($"{nameof(DiscardWorkflow)}{nameof(ActionMessage.Subject)}", "Page Discarded"),
                Body = GetString($"{nameof(DiscardWorkflow)}{nameof(ActionMessage.Body)}", "Edits for page '{0}' have been discarded.", GetTab(stateTransaction.ContentItemId).LocalizedTabName),
            };

        /// <summary>
        /// Checks if the tab has only one version.
        /// </summary>
        /// <param name="tabInfo">The tab information.</param>
        /// <returns>True if the tab has only one version; otherwise, false.</returns>
        private static bool HasOnlyOneTabVersion(TabInfo tabInfo)
        {
			Logger.LogDebug(">>> Entering HasOnlyOneTabVersion(tabInfo={TabInfo})", tabInfo);
            var tabVersions = TabVersionController.Instance.GetTabVersions(tabInfo.TabID, true);
			Logger.LogDebug("HasOnlyOneTabVersion(tabInfo={TabInfo}): tabVersions → {TabVersions}", tabInfo, tabVersions);
			Logger.LogDebug("<<< Exiting HasOnlyOneTabVersion(tabInfo={TabInfo})", tabInfo);
            return tabVersions == null || !tabVersions.Any();
        }
    }
}
