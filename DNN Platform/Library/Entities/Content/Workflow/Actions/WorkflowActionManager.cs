// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;

    public class WorkflowActionManager : ServiceLocator<IWorkflowActionManager, WorkflowActionManager>, IWorkflowActionManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IWorkflowActionRepository workflowActionRepository;

        /// <summary>Initializes a new instance of the <see cref="WorkflowActionManager"/> class.</summary>
        public WorkflowActionManager()
        {
			Logger.LogDebug(">>> Entering WorkflowActionManager()");
            this.workflowActionRepository = WorkflowActionRepository.Instance;
			Logger.LogDebug("<<< Exiting WorkflowActionManager()");
			Logger.LogDebug("WorkflowActionManager(): workflowActionRepository → {WorkflowActionRepository}", workflowActionRepository);
			Logger.LogDebug("WorkflowActionManager(): this.workflowActionRepository → {Arg0}", this.workflowActionRepository);
        }

        /// <inheritdoc />
        public IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType)
        {
			Logger.LogDebug(">>> Entering GetWorkflowActionInstance(contentTypeId={ContentTypeId},actionType={ActionType})", contentTypeId, actionType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var action = this.workflowActionRepository.GetWorkflowAction(contentTypeId, actionType.ToString());
            if (action == null)
            {
				Logger.LogDebug("GetWorkflowActionInstance(contentTypeId={ContentTypeId},actionType={ActionType}): action → {Action}", contentTypeId, actionType, action);
				Logger.LogInformation("GetWorkflowActionInstance(contentTypeId,actionType)={ContentTypeId},{ActionType}: find query executed in {Arg2} ms", contentTypeId, actionType, __logStart.ElapsedMilliseconds);
                return null;
            }

			Logger.LogDebug("<<< Exiting GetWorkflowActionInstance(contentTypeId={ContentTypeId},actionType={ActionType})", contentTypeId, actionType);
            return Reflection.CreateInstance(Reflection.CreateType(action.ActionSource)) as IWorkflowAction;
        }

        /// <inheritdoc />
        public void RegisterWorkflowAction(WorkflowAction workflowAction)
        {
			Logger.LogDebug(">>> Entering RegisterWorkflowAction(workflowAction={WorkflowAction})", workflowAction);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            Requires.NotNull("workflowAction", workflowAction);

            var action = Reflection.CreateInstance(Reflection.CreateType(workflowAction.ActionSource)) as IWorkflowAction;
            if (action == null)
			Logger.LogDebug("RegisterWorkflowAction(workflowAction={WorkflowAction}): action → {Action}", workflowAction, action);
            {
                throw new ArgumentException("The specified ActionSource does not implement the IWorkflowAction interface");
            }

            this.workflowActionRepository.AddWorkflowAction(workflowAction);
			Logger.LogDebug("<<< Exiting RegisterWorkflowAction(workflowAction={WorkflowAction})", workflowAction);
			Logger.LogInformation("RegisterWorkflowAction(workflowAction)={WorkflowAction}: database query executed in {Arg1} ms", workflowAction, __logStart.ElapsedMilliseconds);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowActionManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new WorkflowActionManager();
        }
    }
}
