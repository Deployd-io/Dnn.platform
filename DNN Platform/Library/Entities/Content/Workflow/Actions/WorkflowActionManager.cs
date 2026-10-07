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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowActionManager));
        private readonly IWorkflowActionRepository workflowActionRepository;

        /// <summary>Initializes a new instance of the <see cref="WorkflowActionManager"/> class.</summary>
        public WorkflowActionManager()
        {
			Logger.LogError(">>> Entering WorkflowActionManager()");
            this.workflowActionRepository = WorkflowActionRepository.Instance;
			Logger.LogDebug("WorkflowActionManager(): workflowActionRepository → {WorkflowActionRepository}", workflowActionRepository);
        Logger.LogDebug("WorkflowActionManager(): this.workflowActionRepository → {Arg0}", this.workflowActionRepository);
        Logger.LogWarning("<<< Exiting WorkflowActionManager()");
        }

        /// <inheritdoc />
        public IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType)
        {
			Logger.LogError(">>> Entering GetWorkflowActionInstance(contentTypeId={ContentTypeId},actionType={ActionType})", contentTypeId, actionType);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var action = this.workflowActionRepository.GetWorkflowAction(contentTypeId, actionType.ToString());
            if (action == null)
            {
				Logger.LogError("GetWorkflowActionInstance(contentTypeId={ContentTypeId},actionType={ActionType}): action → {Action}", contentTypeId, actionType, action);
                return null;
            }
			Logger.LogError("GetWorkflowActionInstance(contentTypeId,actionType)={ContentTypeId},{ActionType}: find query executed in {Arg2} ms", contentTypeId, actionType, __logStart.ElapsedMilliseconds);

			Logger.LogWarning("<<< Exiting GetWorkflowActionInstance(contentTypeId={ContentTypeId},actionType={ActionType})", contentTypeId, actionType);
            return Reflection.CreateInstance(Reflection.CreateType(action.ActionSource)) as IWorkflowAction;
        }

        /// <inheritdoc />
        public void RegisterWorkflowAction(WorkflowAction workflowAction)
        {
			Logger.LogError(">>> Entering RegisterWorkflowAction(workflowAction={WorkflowAction})", workflowAction);
            Requires.NotNull("workflowAction", workflowAction);

            var action = Reflection.CreateInstance(Reflection.CreateType(workflowAction.ActionSource)) as IWorkflowAction;
			Logger.LogError("RegisterWorkflowAction(workflowAction={WorkflowAction}): action → {Action}", workflowAction, action);
            if (action == null)
            {
				Logger.LogError("Throwing ArgumentException from RegisterWorkflowAction(workflowAction={WorkflowAction})", workflowAction);
                throw new ArgumentException("The specified ActionSource does not implement the IWorkflowAction interface");
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("RegisterWorkflowAction(workflowAction)={WorkflowAction}: database query executed in {Arg1} ms", workflowAction, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting RegisterWorkflowAction(workflowAction={WorkflowAction})", workflowAction);
            this.workflowActionRepository.AddWorkflowAction(workflowAction);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowActionManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new WorkflowActionManager();
        }
    }
}
