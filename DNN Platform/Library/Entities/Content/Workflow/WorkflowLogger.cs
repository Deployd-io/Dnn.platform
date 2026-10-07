// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class WorkflowLogger : ServiceLocator<IWorkflowLogger, WorkflowLogger>, IWorkflowLogger
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowLogger));
        private readonly IWorkflowLogRepository workflowLogRepository;

        /// <summary>Initializes a new instance of the <see cref="WorkflowLogger"/> class.</summary>
        public WorkflowLogger()
        {
			Logger.LogError(">>> Entering WorkflowLogger()");
            this.workflowLogRepository = WorkflowLogRepository.Instance;
			Logger.LogDebug("WorkflowLogger(): workflowLogRepository → {WorkflowLogRepository}", workflowLogRepository);
        Logger.LogDebug("WorkflowLogger(): this.workflowLogRepository → {Arg0}", this.workflowLogRepository);
        Logger.LogWarning("<<< Exiting WorkflowLogger()");
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
            Logger.LogWarning("<<< Exiting GetWorkflowLogs(contentItemId={ContentItemId},workflowId={WorkflowId})", contentItemId, workflowId);
            return this.workflowLogRepository.GetWorkflowLogs(contentItemId, workflowId);
        }

        /// <inheritdoc />
        public void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string comment, int userId)
        {
			Logger.LogError(">>> Entering AddWorkflowLog(contentItemId={ContentItemId},workflowId={WorkflowId},type={Type},comment={Comment},userId={UserId})", contentItemId, workflowId, type, comment, userId);
            Logger.LogWarning("<<< Exiting AddWorkflowLog(contentItemId={ContentItemId},workflowId={WorkflowId},type={Type},comment={Comment},userId={UserId})", contentItemId, workflowId, type, comment, userId);
            this.AddWorkflowLog(contentItemId, workflowId, type, GetWorkflowActionText(type), comment, userId);
        }

        /// <inheritdoc />
        public void AddWorkflowLog(int contentItemId, int workflowId, string action, string comment, int userId)
        {
			Logger.LogError(">>> Entering AddWorkflowLog(contentItemId={ContentItemId},workflowId={WorkflowId},action={Action},comment={Comment},userId={UserId})", contentItemId, workflowId, action, comment, userId);
            this.AddWorkflowLog(contentItemId, workflowId, WorkflowLogType.CommentProvided, action, comment, userId);
			Logger.LogWarning("<<< Exiting AddWorkflowLog(contentItemId={ContentItemId},workflowId={WorkflowId},action={Action},comment={Comment},userId={UserId})", contentItemId, workflowId, action, comment, userId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowLogger> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new WorkflowLogger();
        }

        private static string GetWorkflowActionText(WorkflowLogType logType)
        {
			Logger.LogError(">>> Entering GetWorkflowActionText(logType={LogType})", logType);
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
			Logger.LogError("GetWorkflowActionText(logType={LogType}): logName → {LogName}", logType, logName);
			Logger.LogWarning("<<< Exiting GetWorkflowActionText(logType={LogType})", logType);
            return Localization.GetString(logName + ".Action");
        }

        private void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string action, string comment, int userId)
        {
			Logger.LogError(">>> Entering AddWorkflowLog(contentItemId={ContentItemId},workflowId={WorkflowId},type={Type},action={Action},comment={Comment},userId={UserId})", contentItemId, workflowId, type, action, comment, userId);
            var workflowLog = new WorkflowLog
            {
                ContentItemID = contentItemId,
                WorkflowID = workflowId,
                Type = (int)type,
                Action = action,
                Comment = comment,
                User = userId,
                Date = DateTime.UtcNow,
            };
            this.workflowLogRepository.AddWorkflowLog(workflowLog);
			        Logger.LogWarning("<<< Exiting AddWorkflowLog(contentItemId={ContentItemId},workflowId={WorkflowId},type={Type},action={Action},comment={Comment},userId={UserId})", contentItemId, workflowId, type, action, comment, userId);
        }
    }
}
