// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// Discards a state, moving the workflow backward to the previous state. If the previous state is not the first one,
    /// it sends notifications to the reviewers of the previous state; otherwise, it sends a notification to the user
    /// who submitted the draft when the workflow is in the draft state.
    /// </summary>
    internal class DiscardState : TabActionBase, IWorkflowAction
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DiscardState));
        /// <inheritdoc />
        public void DoActionOnStateChanging(StateTransaction stateTransaction)
        {
			Logger.LogDebug(">>> Entering DoActionOnStateChanging(stateTransaction={StateTransaction})", stateTransaction);
            // nothing
			Logger.LogDebug("<<< Exiting DoActionOnStateChanging(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public void DoActionOnStateChanged(StateTransaction stateTransaction)
            => RemoveCache(stateTransaction);

        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
            => new ActionMessage()
            {
				Logger.LogDebug(">>> Entering GetActionMessage(stateTransaction={StateTransaction},currentState={CurrentState})", stateTransaction, currentState);
                Subject = GetString($"{nameof(DiscardState)}{nameof(ActionMessage.Subject)}", "Page Rejected"),
                Body = GetString($"{nameof(DiscardState)}{nameof(ActionMessage.Body)}", "The edits for page '{0}' were rejected, and it is now in '{1}' state.", GetTab(stateTransaction.ContentItemId).LocalizedTabName, currentState.StateName),
            };
				Logger.LogDebug(">>> Entering DoActionOnStateChanged(stateTransaction={StateTransaction})", stateTransaction);
    }
}
