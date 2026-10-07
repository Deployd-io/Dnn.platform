// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class WorkflowManager : ServiceLocator<IWorkflowManager, WorkflowManager>, IWorkflowManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowManager));
        private readonly DataProvider dataProvider;
        private readonly IWorkflowRepository workflowRepository;
        private readonly IWorkflowStateRepository workflowStateRepository;
        private readonly ISystemWorkflowManager systemWorkflowManager;

        /// <summary>Initializes a new instance of the <see cref="WorkflowManager"/> class.</summary>
        public WorkflowManager()
        {
			Logger.LogError(">>> Entering WorkflowManager()");
            this.dataProvider = DataProvider.Instance();
            this.workflowRepository = WorkflowRepository.Instance;
			Logger.LogDebug("WorkflowManager(): workflowRepository → {WorkflowRepository}", workflowRepository);
			Logger.LogDebug("WorkflowManager(): this.workflowRepository → {Arg0}", this.workflowRepository);
            this.workflowStateRepository = WorkflowStateRepository.Instance;
            this.systemWorkflowManager = SystemWorkflowManager.Instance;
			Logger.LogDebug("WorkflowManager(): workflowStateRepository → {WorkflowStateRepository}", workflowStateRepository);
        Logger.LogWarning("<<< Exiting WorkflowManager()");
        }

        /// <inheritdoc />
        public void DeleteWorkflow(Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering DeleteWorkflow(workflow={Workflow})", workflow);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflowToDelete = this.workflowRepository.GetWorkflow(workflow.WorkflowID);
            if (workflowToDelete == null)
            {
				Logger.LogError("DeleteWorkflow(workflow={Workflow}): workflowToDelete → {WorkflowToDelete}", workflow, workflowToDelete);
                return;
            }
			Logger.LogError("DeleteWorkflow(workflow)={Workflow}: find query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);

            if (workflowToDelete.IsSystem)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from DeleteWorkflow(workflow={Workflow})", workflow);
                throw new WorkflowInvalidOperationException(Localization.GetString("SystemWorkflowDeletionException", Localization.ExceptionsResourceFile));
            }

            var usageCount = this.GetWorkflowUsageCount(workflowToDelete.WorkflowID);
			Logger.LogError("DeleteWorkflow(workflow={Workflow}): usageCount → {UsageCount}", workflow, usageCount);
            if (usageCount > 0)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));
            }

            this.workflowRepository.DeleteWorkflow(workflowToDelete);
			Logger.LogError("DeleteWorkflow(workflow)={Workflow}: delete query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting DeleteWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflow(workflowId={WorkflowId})", workflowId);
			Logger.LogWarning("<<< Exiting GetWorkflow(workflowId={WorkflowId})", workflowId);
			Logger.LogWarning("<<< Exiting GetWorkflow(contentItem={ContentItem})", contentItem);
            return this.workflowRepository.GetWorkflow(workflowId);
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(ContentItem contentItem)
        {
            if (contentItem.StateID == Null.NullInteger)
            {
                return null;
            }

            var state = WorkflowStateRepository.Instance.GetWorkflowStateByID(contentItem.StateID);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
			Logger.LogError(">>> Entering GetWorkflows(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetWorkflows(portalId={PortalId})", portalId);
            return this.workflowRepository.GetWorkflows(portalId);
        }

        /// <inheritdoc />
        public void AddWorkflow(Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering AddWorkflow(workflow={Workflow})", workflow);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.workflowRepository.AddWorkflow(workflow);

            var firstDefaultState = this.systemWorkflowManager.GetDraftStateDefinition(1);
			Logger.LogError("AddWorkflow(workflow={Workflow}): firstDefaultState → {FirstDefaultState}", workflow, firstDefaultState);
			Logger.LogError("AddWorkflow(workflow)={Workflow}: database query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): firstDefaultState.WorkflowID → {Arg1}", workflow, firstDefaultState.WorkflowID);
            var lastDefaultState = this.systemWorkflowManager.GetPublishedStateDefinition(2);

			Logger.LogError("AddWorkflow(workflow={Workflow}): lastDefaultState → {LastDefaultState}", workflow, lastDefaultState);
            firstDefaultState.WorkflowID = workflow.WorkflowID;
            lastDefaultState.WorkflowID = workflow.WorkflowID;
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): lastDefaultState.WorkflowID → {Arg1}", workflow, lastDefaultState.WorkflowID);

            this.workflowStateRepository.AddWorkflowState(firstDefaultState);
            this.workflowStateRepository.AddWorkflowState(lastDefaultState);

            workflow.States = new List<WorkflowState>
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): workflow.States → {Arg1}", workflow, workflow.States);
                              {
                                  firstDefaultState,
                                  lastDefaultState,
                              };
							Logger.LogWarning("<<< Exiting AddWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public void UpdateWorkflow(Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering UpdateWorkflow(workflow={Workflow})", workflow);
            this.workflowRepository.UpdateWorkflow(workflow);
			Logger.LogWarning("<<< Exiting UpdateWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering GetWorkflowUsage(workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", workflowId, pageIndex, pageSize);
            Logger.LogWarning("<<< Exiting GetWorkflowUsage(workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", workflowId, pageIndex, pageSize);
            return CBO.FillCollection<WorkflowUsageItem>(this.dataProvider.GetContentWorkflowUsage(workflowId, pageIndex, pageSize));
        }

        /// <inheritdoc />
        public int GetWorkflowUsageCount(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowUsageCount(workflowId={WorkflowId})", workflowId);
            Logger.LogWarning("<<< Exiting GetWorkflowUsageCount(workflowId={WorkflowId})", workflowId);
            return this.dataProvider.GetContentWorkflowUsageCount(workflowId);
        }

        /// <inheritdoc />
        protected override System.Func<IWorkflowManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new WorkflowManager();
        }
    }
}
