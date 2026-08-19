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
			Logger.LogDebug(">>> Entering SystemWorkflowManager()");
            this.workflowRepository = WorkflowRepository.Instance;
            this.workflowStateRepository = WorkflowStateRepository.Instance;
			Logger.LogDebug("SystemWorkflowManager(): workflowStateRepository → {WorkflowStateRepository}", workflowStateRepository);
			Logger.LogDebug("SystemWorkflowManager(): this.workflowRepository → {Arg0}", this.workflowRepository);
			Logger.LogDebug("<<< Exiting SystemWorkflowManager()");
        }

        /// <inheritdoc />
        public void CreateSystemWorkflows(int portalId)
        {
			Logger.LogDebug(">>> Entering CreateSystemWorkflows(portalId={PortalId})", portalId);
            this.CreateDirectPublishWorkflow(portalId);
            this.CreateSaveDraftWorkflow(portalId);
            this.CreateContentApprovalWorkflow(portalId);
			Logger.LogDebug("<<< Exiting CreateSystemWorkflows(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public Entities.Workflow GetDirectPublishWorkflow(int portalId)
        {
			Logger.LogDebug(">>> Entering GetDirectPublishWorkflow(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetDirectPublishWorkflow(portalId={PortalId})", portalId);
            return this.workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == DirectPublishWorkflowKey);
        }

        /// <inheritdoc />
        public Entities.Workflow GetSaveDraftWorkflow(int portalId)
        {
			Logger.LogDebug(">>> Entering GetSaveDraftWorkflow(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetSaveDraftWorkflow(portalId={PortalId})", portalId);
            return this.workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == SaveDraftWorkflowKey);
        }

        /// <inheritdoc />
        public Entities.Workflow GetContentApprovalWorkflow(int portalId)
        {
			Logger.LogDebug(">>> Entering GetContentApprovalWorkflow(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetContentApprovalWorkflow(portalId={PortalId})", portalId);
            return this.workflowRepository.GetSystemWorkflows(portalId).SingleOrDefault(sw => sw.WorkflowKey == ContentAprovalWorkflowKey);
        }

        /// <inheritdoc />
        public WorkflowState GetDraftStateDefinition(int order)
        {
			Logger.LogDebug(">>> Entering GetDraftStateDefinition(order={Order})", order);
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState1.StateName");
			Logger.LogDebug("GetDraftStateDefinition(order={Order}): state → {State}", order, state);
			Logger.LogDebug("<<< Exiting GetDraftStateDefinition(order={Order})", order);
            return state;
        }

        /// <inheritdoc />
        public WorkflowState GetPublishedStateDefinition(int order)
        {
			Logger.LogDebug(">>> Entering GetPublishedStateDefinition(order={Order})", order);
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState3.StateName");
			Logger.LogDebug("GetPublishedStateDefinition(order={Order}): state → {State}", order, state);
			Logger.LogDebug("<<< Exiting GetPublishedStateDefinition(order={Order})", order);
            return state;
        }

        /// <inheritdoc />
        public WorkflowState GetReadyForReviewStateDefinition(int order)
        {
			Logger.LogDebug(">>> Entering GetReadyForReviewStateDefinition(order={Order})", order);
            var state = GetDefaultWorkflowState(order);
            state.StateName = Localization.GetString("DefaultWorkflowState2.StateName");
			Logger.LogDebug("GetReadyForReviewStateDefinition(order={Order}): state → {State}", order, state);
			Logger.LogDebug("GetReadyForReviewStateDefinition(order={Order}): state.SendNotification → {Arg1}", order, state.SendNotification);
			Logger.LogDebug("GetReadyForReviewStateDefinition(order={Order}): state.SendNotificationToAdministrators → {Arg1}", order, state.SendNotificationToAdministrators);
            state.SendNotification = true;
            state.SendNotificationToAdministrators = true;
			Logger.LogDebug("<<< Exiting GetReadyForReviewStateDefinition(order={Order})", order);
            return state;
        }

        /// <inheritdoc />
        protected override Func<ISystemWorkflowManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new SystemWorkflowManager();
        }

        private static WorkflowState GetDefaultWorkflowState(int order)
        {
			Logger.LogDebug(">>> Entering GetDefaultWorkflowState(order={Order})", order);
			Logger.LogDebug("<<< Exiting GetDefaultWorkflowState(order={Order})", order);
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
			Logger.LogDebug(">>> Entering CreateDirectPublishWorkflow(portalId={PortalId})", portalId);
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
			Logger.LogDebug("CreateDirectPublishWorkflow(portalId={PortalId}): publishedState → {PublishedState}", portalId, publishedState);
			Logger.LogDebug("CreateDirectPublishWorkflow(portalId={PortalId}): publishedState.WorkflowID → {Arg1}", portalId, publishedState.WorkflowID);
			Logger.LogInformation("CreateDirectPublishWorkflow(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            publishedState.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(publishedState);
			Logger.LogDebug("<<< Exiting CreateDirectPublishWorkflow(portalId={PortalId})", portalId);
        }

        private void CreateSaveDraftWorkflow(int portalId)
        {
			Logger.LogDebug(">>> Entering CreateSaveDraftWorkflow(portalId={PortalId})", portalId);
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultSaveDraftWorkflowName"),
                Description = Localization.GetString("DefaultSaveDraftWorkflowDescription"),
                WorkflowKey = SaveDraftWorkflowKey,
                IsSystem = true,
                PortalID = portalId,
            };
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("CreateSaveDraftWorkflow(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            this.workflowRepository.AddWorkflow(workflow);

            var state = this.GetDraftStateDefinition(1);
			Logger.LogDebug("CreateSaveDraftWorkflow(portalId={PortalId}): state → {State}", portalId, state);
			Logger.LogDebug("CreateSaveDraftWorkflow(portalId={PortalId}): state.WorkflowID → {Arg1}", portalId, state.WorkflowID);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);

            state = this.GetPublishedStateDefinition(2);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);
			Logger.LogDebug("<<< Exiting CreateSaveDraftWorkflow(portalId={PortalId})", portalId);
        }

        private void CreateContentApprovalWorkflow(int portalId)
        {
			Logger.LogDebug(">>> Entering CreateContentApprovalWorkflow(portalId={PortalId})", portalId);
            var workflow = new Entities.Workflow
            {
                WorkflowName = Localization.GetString("DefaultWorkflowName"),
                Description = Localization.GetString("DefaultWorkflowDescription"),
                WorkflowKey = ContentAprovalWorkflowKey,
                IsSystem = true,
                PortalID = portalId,
            };
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("CreateContentApprovalWorkflow(portalId)={PortalId}: database query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            this.workflowRepository.AddWorkflow(workflow);

            var state = this.GetDraftStateDefinition(1);
			Logger.LogDebug("CreateContentApprovalWorkflow(portalId={PortalId}): state → {State}", portalId, state);
			Logger.LogDebug("CreateContentApprovalWorkflow(portalId={PortalId}): state.WorkflowID → {Arg1}", portalId, state.WorkflowID);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);

            state = this.GetReadyForReviewStateDefinition(2);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);

            state = this.GetPublishedStateDefinition(3);
            state.WorkflowID = workflow.WorkflowID;
            this.workflowStateRepository.AddWorkflowState(state);
			Logger.LogDebug("<<< Exiting CreateContentApprovalWorkflow(portalId={PortalId})", portalId);
        }
    }
}
