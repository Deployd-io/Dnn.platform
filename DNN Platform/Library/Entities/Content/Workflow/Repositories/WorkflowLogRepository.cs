// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Framework;

    internal class WorkflowLogRepository : ServiceLocator<IWorkflowLogRepository, WorkflowLogRepository>, IWorkflowLogRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowLogRepository));
        /// <inheritdoc />
        public IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
				Logger.LogError("GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): rep → {Rep}", contentItemId, workflowId, rep);
                var workflowLogs = rep.Find("WHERE (ContentItemID = @0 AND WorkflowID = @1)", contentItemId, workflowId).ToArray();
				Logger.LogError("GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): workflowLogs → {WorkflowLogs}", contentItemId, workflowId, workflowLogs);

				Logger.LogWarning("<<< Exiting GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
                return workflowLogs;
            }
        }

        /// <inheritdoc />
        public void DeleteWorkflowLogs(int contentItemId, int workflowId)
        {
			Logger.LogError(">>> Entering DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
				Logger.LogError("DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): rep → {Rep}", contentItemId, workflowId, rep);
                rep.Delete("WHERE (ContentItemID = @0 AND WorkflowID = @1)", contentItemId, workflowId);
				Logger.LogError("DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): WorkflowID → {WorkflowID}", contentItemId, workflowId, WorkflowID);
            }
			Logger.LogWarning("<<< Exiting DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
        }

        /// <inheritdoc />
        public void AddWorkflowLog(WorkflowLog workflowLog)
        {
			Logger.LogError(">>> Entering AddWorkflowLog(workflowLog={WorkflowLog})", workflowLog);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
				Logger.LogError("AddWorkflowLog(workflowLog={WorkflowLog}): rep → {Rep}", workflowLog, rep);

                rep.Insert(workflowLog);
            }
			Logger.LogWarning("<<< Exiting AddWorkflowLog(workflowLog={WorkflowLog})", workflowLog);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowLogRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new WorkflowLogRepository();
        }
    }
}
