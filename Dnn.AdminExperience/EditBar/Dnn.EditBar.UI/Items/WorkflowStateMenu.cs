// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;

    /// <summary>A <see cref="WorkflowBaseMenuItem"/> for a workflow state menu item..</summary>
    /// <param name="contentController">The content controller.</param>
    /// <param name="workflowEngine">The workflow engine.</param>
    [Serializable]
    public class WorkflowStateMenu(IContentController contentController, IWorkflowEngine workflowEngine)
        : WorkflowBaseMenuItem(contentController, workflowEngine)
    {
        /// <summary>Initializes a new instance of the <see cref="WorkflowStateMenu"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public WorkflowStateMenu()
            : this(null, null)
        {
		eventLogger.LogError(">>> Entering WorkflowStateMenu()");
        eventLogger.LogWarning("<<< Exiting WorkflowStateMenu()");
        }

        /// <inheritdoc />
        public override string Name { get; } = "WorkflowState";

        /// <inheritdoc />
        public override string Text => this.Workflow != null ? $"<small>{this.Workflow.WorkflowName}:</small><br>{this.RenderStates()}" : string.Empty;

        /// <inheritdoc />
        public override string CssClass => string.Empty;

        /// <inheritdoc />
        public override string Template { get; } = string.Empty;

        /// <inheritdoc />
        public override string Parent { get; } = Library.Constants.LeftMenu;

        /// <inheritdoc />
        public override string Loader { get; } = "WorkflowState";

        /// <inheritdoc />
        public override int Order { get; } = 77;

        // render list of workflow states from first state to current workflow state
        private string RenderStates()
        {
			eventLogger.LogError(">>> Entering RenderStates()");
            if (this.WorkflowState == null)
            {
                return string.Empty;
            }

            var currentState = $"<strong>{this.WorkflowState.StateName}</strong>";
			eventLogger.LogError("RenderStates(): currentState → {CurrentState}", currentState);
            if (this.IsFirstState == true || this.IsLastState == true)
            {
                return currentState;
            }

            // return "Draft > Review > Review2" for example
            var pastStates = this.Workflow.States.Where(s => s.Order < this.WorkflowState.Order).OrderBy(s => s.Order).ToList();
			eventLogger.LogError("RenderStates(): pastStates → {PastStates}", pastStates);
            var pastStatesHtml = string.Join(" &gt; ", pastStates.Select(s => $"{s.StateName}"));
			eventLogger.LogError("RenderStates(): pastStatesHtml → {PastStatesHtml}", pastStatesHtml);
			eventLogger.LogWarning("<<< Exiting RenderStates()");
            return string.Join(" &gt; ", pastStatesHtml, currentState);
        }
    }
}
