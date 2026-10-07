// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class SystemWorkflowManager : ServiceLocator<ISystemWorkflowManager, SystemWorkflowManager>, ISystemWorkflowManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemWorkflowManager));
        public const string DirectPublishWorkflowKey = "DirectPublish";
        public const string SaveDraftWorkflowKey = "SaveDraft";
        public const string ContentAprovalWorkflowKey = "ContentApproval";
        private readonly IWorkflowRepository workflowRepository;
        private readonly IWorkflowStateRepository workflowStateRepository;

        /// <summary>Initializes a new instance of the <see cref="SystemWorkflowManager"/> class.</summary>
        public SystemWorkflowManager()
        {
			Logger.LogError(">>> Entering SystemWorkflowManager()");
            this.workflowRepository = WorkflowRepository.Instance;
			Logger.LogDebug("SystemWorkflowManager(): this.workflowRepository → {Arg0}", this.workflowRepository);
        Logger.LogWarning("<<< Exiting SystemWorkflowManager()");
            this.workflowStateRepository = WorkflowStateRepository.Instance;
        }

        /// <inheritdoc />
        public void CreateSystemWorkflows(int portalId)
        {
			Logger.LogError(">>> Entering CreateSystemWorkflows(portalId={PortalId})", portalId);
            this.CreateDirectPublishWorkflow(portalId);
            this.CreateSaveDraftWorkflow(portalId);
            this.CreateContentApprovalWorkflow(portalId);
			Logger.LogWarning("<<< Exiting CreateSystemWorkflows(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public Entities.Workflow GetDirectPublishWorkflow(int portalId)
        {
			Logger.LogError(">>> Entering GetDirectPublishWorkflow(portalId={PortalId})", portalId);
			Logger.LogError("GetDirectPublishWorkflow(portalId)={PortalId}: find query executing", portalId);
        Logger.LogWarning("<<< Exiting GetDirectPublishWorkflow(portalId={PortalId})", portalId);
            return this.workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == DirectPublishWorkflowKey);
        }

        /// <inheritdoc />
        public Entities.Workflow GetSaveDraftWorkflow(int portalId)
        {
			Logger.LogError(">>> Entering GetSaveDraftWorkflow(portalId={PortalId})", portalId);
			Logger.LogError("GetSaveDraftWorkflow(portalId)={PortalId}: find query executing", portalId);
        Logger.LogWarning("<<< Exiting GetSaveDraftWorkflow(portalId={PortalId})", portalId);
            return this.workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == SaveDraftWorkflowKey);
        }

        /// <inheritdoc />
        public Entities.Workflow GetContentApprovalWorkflow(int portalId)
        {
			Logger.LogError(">>> Entering GetContentApprovalWorkflow(portalId={PortalId})", portalId);
			Logger.LogError("GetContentApprovalWorkflow(portalId)={PortalId}: find query executing", portalId);
        Logger.LogWarning("<<< Exiting GetContentApprovalWorkflow(portalId={PortalId})", portalId);
            return this.workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == ContentAprovalWorkflowKey);
        }

        /// <inheritdoc />
        public WorkflowState GetDraftStateDefinition(int order)
        {
			Logger.LogError(">>> Entering GetDraftStateDefinition(order={Order})", order);
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState1.StateName");
			Logger.LogError("GetDraftStateDefinition(order={Order}): state → {State}", order, state);
			Logger.LogWarning("<<< Exiting GetDraftStateDefinition(order={Order})", order);
            return state;
        }

        /// <inheritdoc />
        public WorkflowState GetPublishedStateDefinition(int order)
        {
			Logger.LogError(">>> Entering GetPublishedStateDefinition(order={Order})", order);
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState3.StateName");
			Logger.LogError("GetPublishedStateDefinition(order={Order}): state → {State}", order, state);
			Logger.LogWarning("<<< Exiting GetPublishedStateDefinition(order={Order})", order);
            return state;
        }

        /// <inheritdoc />
        public WorkflowState GetReadyForReviewStateDefinition(int order)
        {
			Logger.LogError(">>> Entering GetReadyForReviewStateDefinition(order={Order})", order);
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState2.StateName");
			Logger.LogError("GetReadyForReviewStateDefinition(order={Order}): state → {State}", order, state);
			Logger.LogDebug("GetReadyForReviewStateDefinition(order={Order}): state.SendNotificationToAdministrators → {Arg1}", order, state.SendNotificationToAdministrators);
            state.SendNotification = true;
			Logger.LogDebug("GetReadyForReviewStateDefinition(order={Order}): state.SendNotification → {Arg1}", order, state.SendNotification);
            state.SendNotificationToAdministrators = true;
			Logger.LogWarning("<<< Exiting GetReadyForReviewStateDefinition(order={Order})", order);
            return state;
        }

        /// <inheritdoc />
        protected override Func<ISystemWorkflowManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new SystemWorkflowManager();
        }

        private static WorkflowState GetDefaultWorkflowState(int order)
        {
			Logger.LogError(">>> Entering GetDefaultWorkflowState(order={Order})", order);
			Logger.LogWarning("<<< Exiting GetDefaultWorkflowState(order={Order})", order);
            return new WorkflowState
            {
                IsSystem = true,
                SendNotification = true,
                SendNotificationToAdministrators = false,
                Order = order,
            };
        }

        private void CreateDirectPublishWorkflow(int portalId)
        {
			Logger.LogError(">>> Entering CreateDirectPublishWorkflow(portalId={PortalId})", portalId);
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultDirectPublishWorkflowName"),
                Description = Localization.GetString("DefaultDirectPublishWorkflowDescription"),
                WorkflowKey = DirectPublishWorkflowKey,
                IsSystem = true,
                PortalID = portalId,
            };
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.workflowRepository.AddWorkflow(workflow);
            var publishedState = this.GetPublishedStateDefinition(1);
			Logger.LogError("CreateDirectPublishWorkflow(portalId={PortalId}): publishedState → {PublishedState}", portalId, publishedState);
			Logger.LogError("CreateDirectPublishWorkflow(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting CreateDirectPublishWorkflow(portalId={PortalId})", portalId);
            publishedState.WorkflowID = workflow.WorkflowID;
			Logger.LogDebug("CreateDirectPublishWorkflow(portalId={PortalId}): publishedState.WorkflowID → {Arg1}", portalId, publishedState.WorkflowID);
            this.workflowStateRepository.AddWorkflowState(publishedState);
        }

        private void CreateSaveDraftWorkflow(int portalId)
        {
			Logger.LogError(">>> Entering CreateSaveDraftWorkflow(portalId={PortalId})", portalId);
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultSaveDraftWorkflowName"),
                Description = Localization.GetString("DefaultSaveDraftWorkflowDescription"),
                WorkflowKey = SaveDraftWorkflowKey,
                IsSystem = true,
                PortalID = portalId,
            };
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("CreateSaveDraftWorkflow(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            this.workflowRepository.AddWorkflow(workflow);

            var state = this.GetDraftStateDefinition(1);
			Logger.LogError("CreateSaveDraftWorkflow(portalId={PortalId}): state → {State}", portalId, state);
            state.WorkflowID = workflow.WorkflowID;
			Logger.LogDebug("CreateSaveDraftWorkflow(portalId={PortalId}): state.WorkflowID → {Arg1}", portalId, state.WorkflowID);
            this.workflowStateRepository.AddWorkflowState(state);

            state = this.GetPublishedStateDefinition(2);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);
			Logger.LogWarning("<<< Exiting CreateSaveDraftWorkflow(portalId={PortalId})", portalId);
        }

        private void CreateContentApprovalWorkflow(int portalId)
        {
			Logger.LogError(">>> Entering CreateContentApprovalWorkflow(portalId={PortalId})", portalId);
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultWorkflowName"),
                Description = Localization.GetString("DefaultWorkflowDescription"),
                WorkflowKey = ContentAprovalWorkflowKey,
                IsSystem = true,
                PortalID = portalId,
            };
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("CreateContentApprovalWorkflow(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            this.workflowRepository.AddWorkflow(workflow);

            var state = this.GetDraftStateDefinition(1);
			Logger.LogError("CreateContentApprovalWorkflow(portalId={PortalId}): state → {State}", portalId, state);
            state.WorkflowID = workflow.WorkflowID;
			Logger.LogDebug("CreateContentApprovalWorkflow(portalId={PortalId}): state.WorkflowID → {Arg1}", portalId, state.WorkflowID);
            this.workflowStateRepository.AddWorkflowState(state);

            state = this.GetReadyForReviewStateDefinition(2);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);

            state = this.GetPublishedStateDefinition(3);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);
			Logger.LogWarning("<<< Exiting CreateContentApprovalWorkflow(portalId={PortalId})", portalId);
        }
    }
}
