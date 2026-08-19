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
        /// <inheritdoc />
        public IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
			Logger.LogDebug(">>> Entering GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
				Logger.LogDebug("GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): rep → {Rep}", contentItemId, workflowId, rep);
                var workflowLogs = rep.Find("WHERE (ContentItemID = @0 AND WorkflowID = @1)", contentItemId, workflowId).ToArray();
				Logger.LogDebug("GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): workflowLogs → {WorkflowLogs}", contentItemId, workflowId, workflowLogs);

				Logger.LogDebug("<<< Exiting GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
                return workflowLogs;
            }
        }

        /// <inheritdoc />
        public void DeleteWorkflowLogs(int contentItemId, int workflowId)
        {
			Logger.LogDebug(">>> Entering DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
				Logger.LogDebug("DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): rep → {Rep}", contentItemId, workflowId, rep);
                rep.Delete("WHERE (ContentItemID = @0 AND WorkflowID = @1)", contentItemId, workflowId);
				Logger.LogDebug("DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId}): WorkflowID → {WorkflowID}", contentItemId, workflowId, WorkflowID);
            }
			Logger.LogDebug("<<< Exiting DeleteWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
        }

        /// <inheritdoc />
        public void AddWorkflowLog(WorkflowLog workflowLog)
        {
			Logger.LogDebug(">>> Entering AddWorkflowLog(workflowLog={WorkflowLog})", workflowLog);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<WorkflowLog>();
				Logger.LogDebug("AddWorkflowLog(workflowLog={WorkflowLog}): rep → {Rep}", workflowLog, rep);

                rep.Insert(workflowLog);
            }
			Logger.LogDebug("<<< Exiting AddWorkflowLog(workflowLog={WorkflowLog})", workflowLog);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowLogRepository> GetFactory()
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowLogRepository));
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new WorkflowLogRepository();
        }
    }
}
