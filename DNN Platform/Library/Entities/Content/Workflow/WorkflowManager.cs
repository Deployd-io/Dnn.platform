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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly DataProvider dataProvider;
        private readonly IWorkflowRepository workflowRepository;
        private readonly IWorkflowStateRepository workflowStateRepository;
        private readonly ISystemWorkflowManager systemWorkflowManager;

        /// <summary>Initializes a new instance of the <see cref="WorkflowManager"/> class.</summary>
        public WorkflowManager()
        {
			Logger.LogDebug(">>> Entering WorkflowManager()");
            this.dataProvider = DataProvider.Instance();
            this.workflowRepository = WorkflowRepository.Instance;
			Logger.LogDebug("WorkflowManager(): workflowRepository → {WorkflowRepository}", workflowRepository);
			Logger.LogDebug("WorkflowManager(): this.workflowRepository → {Arg0}", this.workflowRepository);
            this.workflowStateRepository = WorkflowStateRepository.Instance;
            this.systemWorkflowManager = SystemWorkflowManager.Instance;
			Logger.LogDebug("<<< Exiting WorkflowManager()");
			Logger.LogDebug("WorkflowManager(): workflowStateRepository → {WorkflowStateRepository}", workflowStateRepository);
			Logger.LogDebug("WorkflowManager(): this.workflowStateRepository → {Arg0}", this.workflowStateRepository);
			Logger.LogDebug("WorkflowManager(): systemWorkflowManager → {SystemWorkflowManager}", systemWorkflowManager);
			Logger.LogDebug("WorkflowManager(): this.systemWorkflowManager → {Arg0}", this.systemWorkflowManager);
			Logger.LogDebug("WorkflowManager(): dataProvider → {DataProvider}", dataProvider);
        }

        /// <inheritdoc />
        public void DeleteWorkflow(Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering DeleteWorkflow(workflow={Workflow})", workflow);
            var workflowToDelete = this.workflowRepository.GetWorkflow(workflow.WorkflowID);
            if (workflowToDelete == null)
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogDebug("DeleteWorkflow(workflow={Workflow}): workflowToDelete → {WorkflowToDelete}", workflow, workflowToDelete);
			Logger.LogInformation("DeleteWorkflow(workflow)={Workflow}: find query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
            {
                return;
            }

            if (workflowToDelete.IsSystem)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("SystemWorkflowDeletionException", Localization.ExceptionsResourceFile));
            }

            var usageCount = this.GetWorkflowUsageCount(workflowToDelete.WorkflowID);
            if (usageCount > 0)
			Logger.LogDebug("DeleteWorkflow(workflow={Workflow}): usageCount → {UsageCount}", workflow, usageCount);
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowInUsageException", Localization.ExceptionsResourceFile));
            }

            this.workflowRepository.DeleteWorkflow(workflowToDelete);
			Logger.LogInformation("DeleteWorkflow(workflow)={Workflow}: delete query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting DeleteWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(int workflowId)
        {
			Logger.LogDebug(">>> Entering GetWorkflow(workflowId={WorkflowId})", workflowId);
			Logger.LogDebug("<<< Exiting GetWorkflow(workflowId={WorkflowId})", workflowId);
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
			Logger.LogDebug("GetWorkflow(contentItem={ContentItem}): state → {State}", contentItem, state);
			Logger.LogDebug("<<< Exiting GetWorkflow(contentItem={ContentItem})", contentItem);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
			Logger.LogDebug(">>> Entering GetWorkflows(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetWorkflows(portalId={PortalId})", portalId);
            return this.workflowRepository.GetWorkflows(portalId);
        }

        /// <inheritdoc />
        public void AddWorkflow(Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering AddWorkflow(workflow={Workflow})", workflow);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.workflowRepository.AddWorkflow(workflow);

			Logger.LogInformation("AddWorkflow(workflow)={Workflow}: database query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
            var firstDefaultState = this.systemWorkflowManager.GetDraftStateDefinition(1);
            var lastDefaultState = this.systemWorkflowManager.GetPublishedStateDefinition(2);
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): firstDefaultState → {FirstDefaultState}", workflow, firstDefaultState);
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): lastDefaultState → {LastDefaultState}", workflow, lastDefaultState);
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): firstDefaultState.WorkflowID → {Arg1}", workflow, firstDefaultState.WorkflowID);
			Logger.LogDebug("AddWorkflow(workflow={Workflow}): lastDefaultState.WorkflowID → {Arg1}", workflow, lastDefaultState.WorkflowID);

            firstDefaultState.WorkflowID = workflow.WorkflowID;
            lastDefaultState.WorkflowID = workflow.WorkflowID;

            this.workflowStateRepository.AddWorkflowState(firstDefaultState);
            this.workflowStateRepository.AddWorkflowState(lastDefaultState);

            workflow.States = new List<WorkflowState>
							Logger.LogDebug("AddWorkflow(workflow={Workflow}): workflow.States → {Arg1}", workflow, workflow.States);
                              {
                                  firstDefaultState,
                                  lastDefaultState,
                              };
							Logger.LogDebug("<<< Exiting AddWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public void UpdateWorkflow(Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering UpdateWorkflow(workflow={Workflow})", workflow);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.workflowRepository.UpdateWorkflow(workflow);
			Logger.LogDebug("<<< Exiting UpdateWorkflow(workflow={Workflow})", workflow);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateWorkflow(workflow)={Workflow}: update query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("UpdateWorkflow(workflow)={Workflow}: update query executed in {Arg1} ms", workflow, __logStart.ElapsedMilliseconds);
        /// <inheritdoc />
        public IEnumerable<WorkflowUsageItem> GetWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetWorkflowUsage(workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", workflowId, pageIndex, pageSize);
			Logger.LogDebug("<<< Exiting GetWorkflowUsage(workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", workflowId, pageIndex, pageSize);
            return CBO.FillCollection<WorkflowUsageItem>(this.dataProvider.GetContentWorkflowUsage(workflowId, pageIndex, pageSize));
        }

        /// <inheritdoc />
        public int GetWorkflowUsageCount(int workflowId)
        {
			Logger.LogDebug(">>> Entering GetWorkflowUsageCount(workflowId={WorkflowId})", workflowId);
			Logger.LogDebug("<<< Exiting GetWorkflowUsageCount(workflowId={WorkflowId})", workflowId);
            return this.dataProvider.GetContentWorkflowUsageCount(workflowId);
        }

        /// <inheritdoc />
        protected override System.Func<IWorkflowManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new WorkflowManager();
        }
    }
}
