// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions.TabActions
{
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// Completes a state, moving the workflow forward to the next state. If the next state is not the last one,
    /// it sends notifications to the reviewers of the next state; otherwise, it sends a notification to the user
    /// who submitted the draft once the workflow is complete.
    /// </summary>
    internal class CompleteState : TabActionBase, IWorkflowAction
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering DoActionOnStateChanged(stateTransaction={StateTransaction})", stateTransaction);

		Logger.LogDebug("<<< Exiting DoActionOnStateChanged(stateTransaction={StateTransaction})", stateTransaction);
        /// <inheritdoc />
        public ActionMessage GetActionMessage(StateTransaction stateTransaction, WorkflowState currentState)
            => new ActionMessage()
            {
				Logger.LogDebug(">>> Entering GetActionMessage(stateTransaction={StateTransaction},currentState={CurrentState})", stateTransaction, currentState);
                Subject = GetString($"{nameof(CompleteState)}{nameof(ActionMessage.Subject)}", "Page Approval"),
                Body = GetString($"{nameof(CompleteState)}{nameof(ActionMessage.Body)}", "Page '{0}' is in the '{1}' state and awaits review and approval.", GetTab(stateTransaction.ContentItemId).LocalizedTabName, currentState.StateName),
            };
				Logger.LogDebug("<<< Exiting GetActionMessage(stateTransaction={StateTransaction},currentState={CurrentState})", stateTransaction, currentState);
    }
}
