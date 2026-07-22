// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Actions.TabActions;
    using DotNetNuke.Framework;

    internal class WorkflowActionRepository : ServiceLocator<IWorkflowActionRepository, WorkflowActionRepository>, IWorkflowActionRepository
    {
        /// <inheritdoc />
        public WorkflowAction GetWorkflowAction(int contentTypeId, string type)
        {
			Logger.LogDebug(">>> Entering GetWorkflowAction(contentTypeId={ContentTypeId},type={Type})", contentTypeId, type);
            using var context = DataContext.Instance();
            var rep = context.GetRepository<WorkflowAction>();
			Logger.LogDebug("GetWorkflowAction(contentTypeId={ContentTypeId},type={Type}): context → {Context}", contentTypeId, type, context);
			Logger.LogDebug("GetWorkflowAction(contentTypeId={ContentTypeId},type={Type}): rep → {Rep}", contentTypeId, type, rep);
			Logger.LogDebug("<<< Exiting GetWorkflowAction(contentTypeId={ContentTypeId},type={Type})", contentTypeId, type);
            return rep.Find("WHERE ContentTypeId = @0 AND ActionType = @1", contentTypeId, type).SingleOrDefault()
                ?? GetWorkflowActionsDefaultsOrNull(contentTypeId, type); // fallback to default action (not in db)
        }

        /// <inheritdoc />
        public void AddWorkflowAction(WorkflowAction action)
        {
			Logger.LogDebug(">>> Entering AddWorkflowAction(action={Action})", action);
            using var context = DataContext.Instance();
            var rep = context.GetRepository<WorkflowAction>();
			Logger.LogDebug("AddWorkflowAction(action={Action}): context → {Context}", action, context);
			Logger.LogDebug("AddWorkflowAction(action={Action}): rep → {Rep}", action, rep);
            rep.Insert(action);
			Logger.LogDebug("<<< Exiting AddWorkflowAction(action={Action})", action);
			Logger.LogDebug(">>> Entering GetWorkflowActionsDefaultsOrNull(contentTypeId={ContentTypeId},type={Type})", contentTypeId, type);
			Logger.LogDebug("<<< Exiting GetWorkflowActionsDefaultsOrNull(contentTypeId={ContentTypeId},type={Type})", contentTypeId, type);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowActionRepository> GetFactory()
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new WorkflowActionRepository();
        }

        private static WorkflowAction GetWorkflowActionsDefaultsOrNull(int contentTypeId, string type)
        {
            // only "Tab" ContentType is supported
            if (contentTypeId != ContentType.Tab.ContentTypeId)
            {
                return null;
            }

            // only known ActionTypes are supported
            if (!Enum.TryParse<WorkflowActionTypes>(type, out var workflowActionType))
            {
                return null;
            }

            var actionSource = workflowActionType switch
            {
                WorkflowActionTypes.DiscardWorkflow => typeof(DiscardWorkflow).AssemblyQualifiedName,
                WorkflowActionTypes.CompleteWorkflow => typeof(CompleteWorkflow).AssemblyQualifiedName,
                WorkflowActionTypes.DiscardState => typeof(DiscardState).AssemblyQualifiedName,
                WorkflowActionTypes.CompleteState => typeof(CompleteState).AssemblyQualifiedName,
                WorkflowActionTypes.StartWorkflow => typeof(StartWorkflow).AssemblyQualifiedName,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unexpected workflow action type, supported values include {nameof(WorkflowActionTypes.DiscardWorkflow)}, {nameof(WorkflowActionTypes.CompleteWorkflow)}, {nameof(WorkflowActionTypes.DiscardState)}, {nameof(WorkflowActionTypes.CompleteState)}, and {nameof(WorkflowActionTypes.StartWorkflow)}."),
            };

            return new WorkflowAction()
            {
                ActionId = -1, // not in DB
                ContentTypeId = contentTypeId,
                ActionType = type,
                ActionSource = actionSource,
            };
        }
    }
}
