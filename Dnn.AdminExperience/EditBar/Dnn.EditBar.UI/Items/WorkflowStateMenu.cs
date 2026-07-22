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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="WorkflowStateMenu"/> class.</summary>
		Logger.LogDebug(">>> Entering WorkflowStateMenu(contentController={ContentController},workflowEngine={WorkflowEngine})", contentController, workflowEngine);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
		Logger.LogDebug("<<< Exiting WorkflowStateMenu(contentController={ContentController},workflowEngine={WorkflowEngine})", contentController, workflowEngine);
        public WorkflowStateMenu()
            : this(null, null)
        {
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
			Logger.LogDebug(">>> Entering RenderStates()");
            if (this.WorkflowState == null)
            {
                return string.Empty;
            }

            var currentState = $"<strong>{this.WorkflowState.StateName}</strong>";
            if (this.IsFirstState == true || this.IsLastState == true)
			Logger.LogDebug("RenderStates(): currentState → {CurrentState}", currentState);
            {
                return currentState;
            }

            // return "Draft > Review > Review2" for example
            var pastStates = this.Workflow.States.Where(s => s.Order < this.WorkflowState.Order).OrderBy(s => s.Order).ToList();
            var pastStatesHtml = string.Join(" &gt; ", pastStates.Select(s => $"{s.StateName}"));
			Logger.LogDebug("RenderStates(): pastStates → {PastStates}", pastStates);
			Logger.LogDebug("RenderStates(): pastStatesHtml → {PastStatesHtml}", pastStatesHtml);
			Logger.LogDebug("<<< Exiting RenderStates()");
            return string.Join(" &gt; ", pastStatesHtml, currentState);
        }
    }
}
