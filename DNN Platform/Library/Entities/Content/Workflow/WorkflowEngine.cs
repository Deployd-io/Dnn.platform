// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;

    using Microsoft.Extensions.DependencyInjection;

    public class WorkflowEngine(IContentController contentController)
        : ServiceLocator<IWorkflowEngine, WorkflowEngine>, IWorkflowEngine
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowEngine));
        private const string ContentWorkflowNotificationType = "ContentWorkflowNotification";
        private const string ContentWorkflowNotificationNoActionType = "ContentWorkflowNoActionNotification";
        private const string ContentWorkflowNotificationStartWorkflowType = "ContentWorkflowStartWorkflowNotification";
        private readonly IContentController contentController = contentController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IContentController>();
        private readonly IWorkflowRepository workflowRepository = WorkflowRepository.Instance;
        private readonly IWorkflowStateRepository workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly IWorkflowStatePermissionsRepository workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
        private readonly IWorkflowLogRepository workflowLogRepository = WorkflowLogRepository.Instance;
        private readonly IWorkflowActionManager workflowActionManager = WorkflowActionManager.Instance;
        private readonly IWorkflowSecurity workflowSecurity = WorkflowSecurity.Instance;
        private readonly IUserController userController = UserController.Instance;
        private readonly INotificationsController notificationsController = NotificationsController.Instance;
        private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
        private readonly IWorkflowLogger workflowLogger = WorkflowLogger.Instance;
        private readonly ISystemWorkflowManager systemWorkflowManager = SystemWorkflowManager.Instance;

        /// <summary>Initializes a new instance of the <see cref="WorkflowEngine"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public WorkflowEngine()
            : this(null)
        {
		Logger.LogError(">>> Entering WorkflowEngine()");
        Logger.LogWarning("<<< Exiting WorkflowEngine()");
        }

        /// <inheritdoc />
        public UserInfo GetStartedDraftStateUser(ContentItem contentItem)
        {
			Logger.LogError(">>> Entering GetStartedDraftStateUser(contentItem={ContentItem})", contentItem);
            Logger.LogWarning("<<< Exiting GetStartedDraftStateUser(contentItem={ContentItem})", contentItem);
            return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.WorkflowStarted);
        }

        /// <inheritdoc />
        public UserInfo GetSubmittedDraftStateUser(ContentItem contentItem)
        {
			Logger.LogError(">>> Entering GetSubmittedDraftStateUser(contentItem={ContentItem})", contentItem);
            Logger.LogWarning("<<< Exiting GetSubmittedDraftStateUser(contentItem={ContentItem})", contentItem);
            return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.DraftCompleted);
        }

        /// <inheritdoc />
        public void StartWorkflow(int workflowId, int contentItemId, int userId)
        {
			Logger.LogError(">>> Entering StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
            Requires.NotNegative("workflowId", workflowId);

            var contentItem = this.contentController.GetContentItem(contentItemId);
			Logger.LogError("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): contentItem → {ContentItem}", workflowId, contentItemId, userId, contentItem);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogError("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): workflow → {Workflow}", workflowId, contentItemId, userId, workflow);

            // If already exists a started workflow
            if (workflow != null && !this.IsWorkflowCompleted(contentItem))
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowAlreadyStarted", "Workflow cannot get started for this Content Item. It already has a started workflow."));
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (workflow == null || workflow.WorkflowID != workflowId)
            {
                workflow = this.workflowRepository.GetWorkflow(workflowId);
            }
			Logger.LogError("StartWorkflow(workflowId,contentItemId,userId)={WorkflowId},{ContentItemId},{UserId}: find query executed in {Arg3} ms", workflowId, contentItemId, userId, __logStart.ElapsedMilliseconds);

            var initialTransaction = CreateInitialTransaction(contentItemId, workflow.FirstState.StateID, userId);
			Logger.LogError("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): initialTransaction → {InitialTransaction}", workflowId, contentItemId, userId, initialTransaction);

            // Perform action before starting workflow
            this.PerformWorkflowActionOnStateChanging(initialTransaction, WorkflowActionTypes.StartWorkflow);
            this.UpdateContentItemWorkflowState(workflow.FirstState.StateID, contentItem);

            // Send notifications to stater
            if (workflow.WorkflowID != this.systemWorkflowManager.GetDirectPublishWorkflow(workflow.PortalID).WorkflowID)
            {
                // This notification is not sent in Direct Publish WF
                this.SendNotificationToWorkflowStarter(initialTransaction, workflow, contentItem, userId, WorkflowActionTypes.StartWorkflow);
            }

            // Delete previous logs
            this.workflowLogRepository.DeleteWorkflowLogs(contentItemId, workflowId);
			Logger.LogError("StartWorkflow(workflowId,contentItemId,userId)={WorkflowId},{ContentItemId},{UserId}: delete query executed in {Arg3} ms", workflowId, contentItemId, userId, __logStart.ElapsedMilliseconds);

            // Add logs
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowStarted, userId);
            this.AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, userId);

            // Perform action after starting workflow
            this.PerformWorkflowActionOnStateChanged(initialTransaction, WorkflowActionTypes.StartWorkflow);
			Logger.LogWarning("<<< Exiting StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
        }

        /// <inheritdoc />
        public void CompleteState(StateTransaction stateTransaction)
        {
			Logger.LogError(">>> Entering CompleteState(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogError("CompleteState(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
			Logger.LogError("CompleteState(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);

            if (workflow == null)
            {
                return;
            }

            if (this.IsWorkflowCompleted(contentItem)
                        && !(workflow.IsSystem && workflow.States.Count() == 1))
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from CompleteState(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowSystemWorkflowStateCannotComplete", "System workflow state cannot be completed."));
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;

            if (!isFirstState && !this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
				Logger.LogError("Throwing WorkflowSecurityException from CompleteState(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
			Logger.LogError("CompleteState(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
				Logger.LogError("Throwing WorkflowConcurrencyException from CompleteState(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowConcurrencyException();
            }
			Logger.LogError("CompleteState(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);

            var nextState = GetNextWorkflowState(workflow, contentItem.StateID);
			Logger.LogError("CompleteState(stateTransaction={StateTransaction}): nextState → {NextState}", stateTransaction, nextState);
            if (nextState.StateID == workflow.LastState.StateID)
            {
                this.CompleteWorkflow(stateTransaction);
                return;
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteState);
            this.UpdateContentItemWorkflowState(nextState.StateID, contentItem);

            // Add logs
            this.AddWorkflowCommentLog(contentItem, currentState, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(
                contentItem,
                currentState,
                currentState.StateID == workflow.FirstState.StateID ? WorkflowLogType.DraftCompleted : WorkflowLogType.StateCompleted,
                stateTransaction.UserId);
            this.AddWorkflowLog(
                contentItem,
                nextState.StateID == workflow.LastState.StateID ? WorkflowLogType.WorkflowApproved : WorkflowLogType.StateInitiated,
                stateTransaction.UserId);

            this.SendNotificationsToReviewers(contentItem, nextState, stateTransaction, WorkflowActionTypes.CompleteState, new PortalSettings(workflow.PortalID));

            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteState);
			Logger.LogWarning("<<< Exiting CompleteState(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public void DiscardState(StateTransaction stateTransaction)
        {
			Logger.LogError(">>> Entering DiscardState(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogError("DiscardState(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
            if (workflow == null)
            {
				Logger.LogError("DiscardState(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
                return;
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;
            var isLastState = workflow.LastState.StateID == contentItem.StateID;

            if (isLastState)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from DiscardState(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowCannotDiscard", "Cannot discard on last workflow state"));
            }

            if (!isFirstState && !this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
				Logger.LogError("Throwing WorkflowSecurityException from DiscardState(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
			Logger.LogError("DiscardState(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
				Logger.LogError("Throwing WorkflowConcurrencyException from DiscardState(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowConcurrencyException();
            }
			Logger.LogError("DiscardState(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);

            var previousState = GetPreviousWorkflowState(workflow, contentItem.StateID);
			Logger.LogError("DiscardState(stateTransaction={StateTransaction}): previousState → {PreviousState}", stateTransaction, previousState);
            if (previousState.StateID == workflow.LastState.StateID)
            {
                this.DiscardWorkflow(stateTransaction);
                return;
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardState);

            this.UpdateContentItemWorkflowState(previousState.StateID, contentItem);

            // Add logs
            this.AddWorkflowCommentLog(contentItem, currentState, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, currentState, WorkflowLogType.StateDiscarded, stateTransaction.UserId);
            this.AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, stateTransaction.UserId);

            if (previousState.StateID == workflow.FirstState.StateID)
            {
                // Send to author - workflow comes back to draft state
                this.SendNotificationToAuthor(stateTransaction, previousState, workflow, contentItem, WorkflowActionTypes.DiscardState);
            }
            else
            {
                this.SendNotificationsToReviewers(contentItem, previousState, stateTransaction, WorkflowActionTypes.DiscardState, new PortalSettings(workflow.PortalID));
            }

            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardState);
			Logger.LogWarning("<<< Exiting DiscardState(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public bool IsWorkflowCompleted(int contentItemId)
        {
			Logger.LogError(">>> Entering IsWorkflowCompleted(contentItemId={ContentItemId})", contentItemId);
            var contentItem = this.contentController.GetContentItem(contentItemId);
			Logger.LogError("IsWorkflowCompleted(contentItemId={ContentItemId}): contentItem → {ContentItem}", contentItemId, contentItem);
			Logger.LogWarning("<<< Exiting IsWorkflowCompleted(contentItemId={ContentItemId})", contentItemId);
			Logger.LogWarning("<<< Exiting IsWorkflowCompleted(contentItem={ContentItem})", contentItem);
            return this.IsWorkflowCompleted(contentItem);
        }

        /// <inheritdoc />
        public bool IsWorkflowCompleted(ContentItem contentItem)
        {
            var workflow = this.workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
            {
                return true; // If item has not workflow, then it is considered as completed
            }

            return contentItem.StateID == Null.NullInteger || workflow.LastState.StateID == contentItem.StateID;
        }

        /// <inheritdoc />
        public bool IsWorkflowOnDraft(int contentItemId)
        {
			Logger.LogError(">>> Entering IsWorkflowOnDraft(contentItemId={ContentItemId})", contentItemId);
            var contentItem = this.contentController.GetContentItem(contentItemId); // Ensure DB values
			Logger.LogError("IsWorkflowOnDraft(contentItemId={ContentItemId}): contentItem → {ContentItem}", contentItemId, contentItem);
			Logger.LogWarning("<<< Exiting IsWorkflowOnDraft(contentItemId={ContentItemId})", contentItemId);
			Logger.LogWarning("<<< Exiting IsWorkflowOnDraft(contentItem={ContentItem})", contentItem);
            return this.IsWorkflowOnDraft(contentItem);
        }

        /// <inheritdoc />
        public bool IsWorkflowOnDraft(ContentItem contentItem)
        {
            var workflow = this.workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
            {
                return false; // If item has not workflow, then it is not on Draft
            }

            return contentItem.StateID == workflow.FirstState.StateID;
        }

        /// <inheritdoc />
        public void DiscardWorkflow(StateTransaction stateTransaction)
        {
			Logger.LogError(">>> Entering DiscardWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("DiscardWorkflow(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
				Logger.LogError("DiscardWorkflow(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
				Logger.LogError("DiscardWorkflow(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
				Logger.LogError("Throwing WorkflowConcurrencyException from DiscardWorkflow(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardWorkflow);

            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogError("DiscardWorkflow(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
            this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

            // Logs
            this.AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowDiscarded, stateTransaction.UserId);

            // Notifications
            this.SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.DiscardWorkflow);
            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardWorkflow);
			Logger.LogWarning("<<< Exiting DiscardWorkflow(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public void CompleteWorkflow(StateTransaction stateTransaction)
        {
			Logger.LogError(">>> Entering CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("CompleteWorkflow(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
				Logger.LogError("CompleteWorkflow(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
				Logger.LogError("CompleteWorkflow(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
				Logger.LogError("Throwing WorkflowConcurrencyException from CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteWorkflow);

            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogError("CompleteWorkflow(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
            this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

            // Logs
            this.AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowApproved, stateTransaction.UserId);

            // Notifications
            this.SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.CompleteWorkflow);
            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteWorkflow);
			Logger.LogWarning("<<< Exiting CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowEngine> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IWorkflowEngine>();
        }

        private static List<RoleInfo> GetRolesFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
        {
			Logger.LogError(">>> Entering GetRolesFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
            Logger.LogWarning("<<< Exiting GetRolesFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
            return (from permission in permissions
                    where permission.AllowAccess && permission.RoleID > Null.NullInteger
                    select RoleController.Instance.GetRoleById(settings.PortalId, permission.RoleID)).ToList();
        }

        private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
        {
			Logger.LogError(">>> Entering IsAdministratorRoleAlreadyIncluded(settings={Settings},roles={Roles})", settings, roles);
			Logger.LogWarning("<<< Exiting IsAdministratorRoleAlreadyIncluded(settings={Settings},roles={Roles})", settings, roles);
            return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
        }

        private static List<UserInfo> GetUsersFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
        {
			Logger.LogError(">>> Entering GetUsersFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
            Logger.LogWarning("<<< Exiting GetUsersFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
            return (from permission in permissions
                    where permission.AllowAccess && permission.UserID > Null.NullInteger
                    select UserController.GetUserById(settings.PortalId, permission.UserID)).ToList();
        }

        private static List<UserInfo> IncludeSuperUsers(List<UserInfo> users)
        {
			Logger.LogError(">>> Entering IncludeSuperUsers(users={Users})", users);
            var superUsers = UserController.GetUsers(false, true, Null.NullInteger);
			Logger.LogError("IncludeSuperUsers(users={Users}): superUsers → {SuperUsers}", users, superUsers);
            foreach (UserInfo superUser in superUsers)
            {
                if (IsSuperUserNotIncluded(users, superUser))
                {
                    users.Add(superUser);
                }
            }

			Logger.LogWarning("<<< Exiting IncludeSuperUsers(users={Users})", users);
            return users.ToList();
        }

        private static bool IsSuperUserNotIncluded(IEnumerable<UserInfo> users, UserInfo superUser)
        {
			Logger.LogError(">>> Entering IsSuperUserNotIncluded(users={Users},superUser={SuperUser})", users, superUser);
			Logger.LogWarning("<<< Exiting IsSuperUserNotIncluded(users={Users},superUser={SuperUser})", users, superUser);
            return users.All(u => u.UserID != superUser.UserID);
        }

        private static string GetWorkflowActionComment(WorkflowLogType logType)
        {
			Logger.LogError(">>> Entering GetWorkflowActionComment(logType={LogType})", logType);
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
			Logger.LogError("GetWorkflowActionComment(logType={LogType}): logName → {LogName}", logType, logName);
			Logger.LogWarning("<<< Exiting GetWorkflowActionComment(logType={LogType})", logType);
            return Localization.GetString(logName + ".Comment");
        }

        private static StateTransaction CreateInitialTransaction(int contentItemId, int stateId, int userId)
        {
			Logger.LogError(">>> Entering CreateInitialTransaction(contentItemId={ContentItemId},stateId={StateId},userId={UserId})", contentItemId, stateId, userId);
			Logger.LogWarning("<<< Exiting CreateInitialTransaction(contentItemId={ContentItemId},stateId={StateId},userId={UserId})", contentItemId, stateId, userId);
            return new StateTransaction
            {
                ContentItemId = contentItemId,
                CurrentStateId = stateId,
                UserId = userId,
                Message = new StateTransactionMessage(),
            };
        }

        private static bool WasDraftSubmitted(Entities.Workflow workflow, int currentStateId)
        {
			Logger.LogError(">>> Entering WasDraftSubmitted(workflow={Workflow},currentStateId={CurrentStateId})", workflow, currentStateId);
            var isDirectPublishWorkflow = workflow.IsSystem && workflow.States.Count() == 1;
            var draftSubmitted = workflow.FirstState.StateID != currentStateId;
			Logger.LogError("WasDraftSubmitted(workflow={Workflow},currentStateId={CurrentStateId}): isDirectPublishWorkflow → {IsDirectPublishWorkflow}", workflow, currentStateId, isDirectPublishWorkflow);
			Logger.LogWarning("<<< Exiting WasDraftSubmitted(workflow={Workflow},currentStateId={CurrentStateId})", workflow, currentStateId);
            return !(isDirectPublishWorkflow || !draftSubmitted);
        }

        private static string GetWorkflowNotificationContext(ContentItem contentItem, WorkflowState state)
        {
			Logger.LogError(">>> Entering GetWorkflowNotificationContext(contentItem={ContentItem},state={State})", contentItem, state);
            Logger.LogWarning("<<< Exiting GetWorkflowNotificationContext(contentItem={ContentItem},state={State})", contentItem, state);
            return $"{contentItem.ContentItemId}:{state.WorkflowID}:{state.StateID}";
        }

        private static WorkflowState GetNextWorkflowState(Entities.Workflow workflow, int stateId)
        {
			Logger.LogError(">>> Entering GetNextWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
            WorkflowState nextState = null;
            var states = workflow.States.OrderBy(s => s.Order);
			Logger.LogError("GetNextWorkflowState(workflow={Workflow},stateId={StateId}): states → {States}", workflow, stateId, states);
			Logger.LogError("GetNextWorkflowState(workflow={Workflow},stateId={StateId}): nextState → {NextState}", workflow, stateId, nextState);
            int index;

            // locate the current state
            for (index = 0; index < states.Count(); index++)
            {
                if (states.ElementAt(index).StateID == stateId)
                {
                    break;
                }
            }

            index = index + 1;
			Logger.LogDebug("GetNextWorkflowState(workflow={Workflow},stateId={StateId}): index → {Index}", workflow, stateId, index);
            if (index < states.Count())
            {
                nextState = states.ElementAt(index);
            }

			Logger.LogWarning("<<< Exiting GetNextWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
            return nextState ?? workflow.FirstState;
        }

        private static WorkflowState GetPreviousWorkflowState(Entities.Workflow workflow, int stateId)
        {
			Logger.LogError(">>> Entering GetPreviousWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
            WorkflowState previousState = null;
            var states = workflow.States.OrderBy(s => s.Order);
			Logger.LogError("GetPreviousWorkflowState(workflow={Workflow},stateId={StateId}): states → {States}", workflow, stateId, states);
			Logger.LogError("GetPreviousWorkflowState(workflow={Workflow},stateId={StateId}): previousState → {PreviousState}", workflow, stateId, previousState);
            int index;

            if (workflow.FirstState.StateID == stateId)
            {
                return workflow.LastState;
            }

            // locate the current state
            for (index = 0; index < states.Count(); index++)
            {
                if (states.ElementAt(index).StateID == stateId)
                {
                    previousState = states.ElementAt(index - 1);
                    break;
                }
            }

			Logger.LogWarning("<<< Exiting GetPreviousWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
            return previousState ?? workflow.LastState;
        }

        private void PerformWorkflowActionOnStateChanged(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
			Logger.LogError(">>> Entering PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
			Logger.LogError("PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType}): contentItem → {ContentItem}", stateTransaction, actionType, contentItem);
            if (workflowAction != null)
            {
				Logger.LogError("PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType}): workflowAction → {WorkflowAction}", stateTransaction, actionType, workflowAction);
                workflowAction.DoActionOnStateChanged(stateTransaction);
            }
			Logger.LogWarning("<<< Exiting PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
        }

        private void PerformWorkflowActionOnStateChanging(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
			Logger.LogError(">>> Entering PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
			Logger.LogError("PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType}): contentItem → {ContentItem}", stateTransaction, actionType, contentItem);
            if (workflowAction != null)
            {
				Logger.LogError("PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType}): workflowAction → {WorkflowAction}", stateTransaction, actionType, workflowAction);
                workflowAction.DoActionOnStateChanging(stateTransaction);
            }
			Logger.LogWarning("<<< Exiting PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
        }

        private IWorkflowAction GetWorkflowActionInstance(ContentItem contentItem, WorkflowActionTypes actionType)
        {
			Logger.LogError(">>> Entering GetWorkflowActionInstance(contentItem={ContentItem},actionType={ActionType})", contentItem, actionType);
            Logger.LogWarning("<<< Exiting GetWorkflowActionInstance(contentItem={ContentItem},actionType={ActionType})", contentItem, actionType);
            return this.workflowActionManager.GetWorkflowActionInstance(contentItem.ContentTypeId, actionType);
        }

        private void UpdateContentItemWorkflowState(int stateId, ContentItem item)
        {
			Logger.LogError(">>> Entering UpdateContentItemWorkflowState(stateId={StateId},item={Item})", stateId, item);
            item.StateID = stateId;
			Logger.LogDebug("UpdateContentItemWorkflowState(stateId={StateId},item={Item}): item.StateID → {Arg2}", stateId, item, item.StateID);
			Logger.LogWarning("<<< Exiting UpdateContentItemWorkflowState(stateId={StateId},item={Item})", stateId, item);
            this.contentController.UpdateContentItem(item);
        }

        private UserInfo GetUserThatHaveStartedOrSubmittedDraftState(Entities.Workflow workflow, ContentItem contentItem, StateTransaction stateTransaction)
        {
			Logger.LogError(">>> Entering GetUserThatHaveStartedOrSubmittedDraftState(workflow={Workflow},contentItem={ContentItem},stateTransaction={StateTransaction})", workflow, contentItem, stateTransaction);
            bool wasDraftSubmitted = WasDraftSubmitted(workflow, stateTransaction.CurrentStateId);
            if (wasDraftSubmitted)
            {
                return this.GetSubmittedDraftStateUser(contentItem);
            }

			Logger.LogWarning("<<< Exiting GetUserThatHaveStartedOrSubmittedDraftState(workflow={Workflow},contentItem={ContentItem},stateTransaction={StateTransaction})", workflow, contentItem, stateTransaction);
            return this.GetStartedDraftStateUser(contentItem);
        }

        private UserInfo GetUserByWorkflowLogType(ContentItem contentItem, WorkflowLogType type)
        {
			Logger.LogError(">>> Entering GetUserByWorkflowLogType(contentItem={ContentItem},type={Type})", contentItem, type);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogError("GetUserByWorkflowLogType(contentItem={ContentItem},type={Type}): workflow → {Workflow}", contentItem, type, workflow);
            if (workflow == null)
            {
                return null;
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var logs = this.workflowLogRepository.GetWorkflowLogs(contentItem.ContentItemId, workflow.WorkflowID);

			Logger.LogError("GetUserByWorkflowLogType(contentItem,type)={ContentItem},{Type}: find query executed in {Arg2} ms", contentItem, type, __logStart.ElapsedMilliseconds);
            var logDraftCompleted = logs
                .OrderByDescending(l => l.Date)
                .FirstOrDefault(l => l.Type == (int)type);
				Logger.LogError("GetUserByWorkflowLogType(contentItem={ContentItem},type={Type}): logs → {Logs}", contentItem, type, logs);

            if (logDraftCompleted != null && logDraftCompleted.User != Null.NullInteger)
            {
                return this.userController.GetUserById(workflow.PortalID, logDraftCompleted.User);
            }

			Logger.LogWarning("<<< Exiting GetUserByWorkflowLogType(contentItem={ContentItem},type={Type})", contentItem, type);
            return null;
        }

        private void DeleteWorkflowNotifications(ContentItem contentItem, WorkflowState state)
        {
			Logger.LogError(">>> Entering DeleteWorkflowNotifications(contentItem={ContentItem},state={State})", contentItem, state);
            var context = GetWorkflowNotificationContext(contentItem, state);
            var notificationTypeId = this.notificationsController.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId;
			Logger.LogError("DeleteWorkflowNotifications(contentItem={ContentItem},state={State}): notificationTypeId → {NotificationTypeId}", contentItem, state, notificationTypeId);
			Logger.LogError("DeleteWorkflowNotifications(contentItem={ContentItem},state={State}): context → {Context}", contentItem, state, context);
            this.DeleteNotificationsByType(notificationTypeId, context);
            notificationTypeId = this.notificationsController.GetNotificationType(ContentWorkflowNotificationStartWorkflowType).NotificationTypeId;
            this.DeleteNotificationsByType(notificationTypeId, context);
			Logger.LogWarning("<<< Exiting DeleteWorkflowNotifications(contentItem={ContentItem},state={State})", contentItem, state);
        }

        private void DeleteNotificationsByType(int notificationTypeId, string context)
        {
			Logger.LogError(">>> Entering DeleteNotificationsByType(notificationTypeId={NotificationTypeId},context={Context})", notificationTypeId, context);
            var notifications = this.notificationsController.GetNotificationByContext(notificationTypeId, context);
			Logger.LogError("DeleteNotificationsByType(notificationTypeId={NotificationTypeId},context={Context}): notifications → {Notifications}", notificationTypeId, context, notifications);
            foreach (var notification in notifications)
            {
                this.notificationsController.DeleteAllNotificationRecipients(notification.NotificationID);
            }
			Logger.LogWarning("<<< Exiting DeleteNotificationsByType(notificationTypeId={NotificationTypeId},context={Context})", notificationTypeId, context);
        }

        private void SendNotificationToAuthor(StateTransaction stateTransaction, WorkflowState state, Entities.Workflow workflow, ContentItem contentItem, WorkflowActionTypes workflowActionType)
        {
			Logger.LogError(">>> Entering SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType})", stateTransaction, state, workflow, contentItem, workflowActionType);
            try
            {
                if (!state.SendNotification)
                {
                    return;
                }

                var user = this.GetUserThatHaveStartedOrSubmittedDraftState(workflow, contentItem, stateTransaction);
				Logger.LogError("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): user → {User}", stateTransaction, state, workflow, contentItem, workflowActionType, user);
                if (user == null)
                {
                    ////Services.Exceptions.Exceptions.LogException(new WorkflowException(Localization.GetExceptionMessage("WorkflowAuthorNotFound", "Author cannot be found. Notification won't be sent")));
                    return;
                }

                if (user.UserID == stateTransaction.UserId)
                {
                    return;
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
				Logger.LogError("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): workflowAction → {WorkflowAction}", stateTransaction, state, workflow, contentItem, workflowActionType, workflowAction);
                if (workflowAction == null)
                {
                    return;
                }

                var message = workflowAction.GetActionMessage(stateTransaction, state);
				Logger.LogError("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): message → {Message}", stateTransaction, state, workflow, contentItem, workflowActionType, message);

                var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationNoActionType);
				Logger.LogError("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): notification → {Notification}", stateTransaction, state, workflow, contentItem, workflowActionType, notification);

                this.notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): {Arg5}", stateTransaction, state, workflow, contentItem, workflowActionType, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType})", stateTransaction, state, workflow, contentItem, workflowActionType);
        }

        private void SendNotificationToWorkflowStarter(StateTransaction stateTransaction, Entities.Workflow workflow, ContentItem contentItem, int starterUserId, WorkflowActionTypes workflowActionType)
        {
			Logger.LogError(">>> Entering SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType})", stateTransaction, workflow, contentItem, starterUserId, workflowActionType);
            try
            {
                if (!workflow.FirstState.SendNotification)
                {
                    return;
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
				Logger.LogError("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): workflowAction → {WorkflowAction}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, workflowAction);
                if (workflowAction == null)
                {
                    return;
                }

                var user = this.userController.GetUser(workflow.PortalID, starterUserId);
				Logger.LogError("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): user → {User}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, user);

                var message = workflowAction.GetActionMessage(stateTransaction, workflow.FirstState);
				Logger.LogError("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): message → {Message}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, message);

                var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, workflow.FirstState), stateTransaction, message, ContentWorkflowNotificationStartWorkflowType);
				Logger.LogError("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): notification → {Notification}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, notification);

                this.notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): {Arg5}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType})", stateTransaction, workflow, contentItem, starterUserId, workflowActionType);
        }

        private void SendNotificationsToReviewers(ContentItem contentItem, WorkflowState state, StateTransaction stateTransaction, WorkflowActionTypes workflowActionType, PortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings})", contentItem, state, stateTransaction, workflowActionType, portalSettings);
            try
            {
                if (!state.SendNotification && !state.SendNotificationToAdministrators)
                {
                    return;
                }

                var reviewers = this.GetUserAndRolesForStateReviewers(portalSettings, state);
				Logger.LogError("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): reviewers → {Reviewers}", contentItem, state, stateTransaction, workflowActionType, portalSettings, reviewers);

                if (reviewers.Roles.Count == 0 && reviewers.Users.Count == 0)
                {
                    return; // If there are no receivers, the notification is avoided
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
				Logger.LogError("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): workflowAction → {WorkflowAction}", contentItem, state, stateTransaction, workflowActionType, portalSettings, workflowAction);
                if (workflowAction == null)
                {
                    return;
                }

                var message = workflowAction.GetActionMessage(stateTransaction, state);
				Logger.LogError("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): message → {Message}", contentItem, state, stateTransaction, workflowActionType, portalSettings, message);

                var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationType);
				Logger.LogError("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): notification → {Notification}", contentItem, state, stateTransaction, workflowActionType, portalSettings, notification);

                this.notificationsController.SendNotification(notification, portalSettings.PortalId, reviewers.Roles.ToList(), reviewers.Users.ToList());
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): {Arg5}", contentItem, state, stateTransaction, workflowActionType, portalSettings, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings})", contentItem, state, stateTransaction, workflowActionType, portalSettings);
        }

        private Notification GetNotification(string workflowContext, StateTransaction stateTransaction, ActionMessage message, string notificationType)
        {
			Logger.LogError(">>> Entering GetNotification(workflowContext={WorkflowContext},stateTransaction={StateTransaction},message={Message},notificationType={NotificationType})", workflowContext, stateTransaction, message, notificationType);
            var notification = new Notification
            {
                NotificationTypeID =
                    this.notificationsController.GetNotificationType(notificationType).NotificationTypeId,
                Subject = message.Subject,
                Body = message.Body,
                IncludeDismissAction = true,
                SenderUserID = stateTransaction.UserId,
                Context = workflowContext,
                SendToast = message.SendToast,
            };
			Logger.LogWarning("<<< Exiting GetNotification(workflowContext={WorkflowContext},stateTransaction={StateTransaction},message={Message},notificationType={NotificationType})", workflowContext, stateTransaction, message, notificationType);
            return notification;
        }

        private ReviewersDto GetUserAndRolesForStateReviewers(PortalSettings portalSettings, WorkflowState state)
        {
			Logger.LogError(">>> Entering GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State})", portalSettings, state);
            var reviewers = new ReviewersDto
            {
                Roles = new List<RoleInfo>(),
                Users = new List<UserInfo>(),
            };
            if (state.SendNotification)
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                var permissions = this.workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(state.StateID).ToArray();
                reviewers.Users = GetUsersFromPermissions(portalSettings, permissions);
				Logger.LogError("GetUserAndRolesForStateReviewers(portalSettings,state)={PortalSettings},{State}: find query executed in {Arg2} ms", portalSettings, state, __logStart.ElapsedMilliseconds);
				Logger.LogError("GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State}): permissions → {Permissions}", portalSettings, state, permissions);
                reviewers.Roles = GetRolesFromPermissions(portalSettings, permissions);
            }

            if (state.SendNotificationToAdministrators)
            {
                if (!IsAdministratorRoleAlreadyIncluded(portalSettings, reviewers.Roles))
                {
                    var adminRole = RoleController.Instance.GetRoleByName(portalSettings.PortalId, portalSettings.AdministratorRoleName);
					Logger.LogError("GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State}): adminRole → {AdminRole}", portalSettings, state, adminRole);
                    reviewers.Roles.Add(adminRole);
                }

                reviewers.Users = IncludeSuperUsers(reviewers.Users);
            }

			Logger.LogWarning("<<< Exiting GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State})", portalSettings, state);
            return reviewers;
        }

        private void AddWorkflowCommentLog(ContentItem contentItem, int userId, string userComment)
        {
			Logger.LogError(">>> Entering AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment})", contentItem, userId, userComment);
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("AddWorkflowCommentLog(contentItem,userId,userComment)={ContentItem},{UserId},{UserComment}: find query executed in {Arg3} ms", contentItem, userId, userComment, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment})", contentItem, userId, userComment);
            var state = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
			Logger.LogError("AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment}): state → {State}", contentItem, userId, userComment, state);
            this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowCommentLog(ContentItem contentItem, WorkflowState state, int userId, string userComment)
        {
			Logger.LogError(">>> Entering AddWorkflowCommentLog(contentItem={ContentItem},state={State},userId={UserId},userComment={UserComment})", contentItem, state, userId, userComment);
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }

            this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowLogType logType, int userId, string userComment = null)
        {
			Logger.LogError(">>> Entering AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, logType, userId, userComment);
            var state = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            this.AddWorkflowLog(contentItem, state, logType, userId, userComment);
			Logger.LogWarning("<<< Exiting AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, logType, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment = null)
        {
			Logger.LogError(">>> Entering AddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, state, logType, userId, userComment);
            try
            {
                this.TryAddWorkflowLog(contentItem, state, logType, userId, userComment);
				Logger.LogWarning("<<< Exiting AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, logType, userId, userComment);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in AddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): {Arg5}", contentItem, state, logType, userId, userComment, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void TryAddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment)
        {
			Logger.LogError(">>> Entering TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, state, logType, userId, userComment);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogWarning("<<< Exiting TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, state, logType, userId, userComment);
            var logTypeText = GetWorkflowActionComment(logType);
			Logger.LogError("TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): logTypeText → {LogTypeText}", contentItem, state, logType, userId, userComment, logTypeText);
            var logComment = this.ReplaceNotificationTokens(logTypeText, workflow, contentItem, state, userId, userComment);
			Logger.LogError("TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): logComment → {LogComment}", contentItem, state, logType, userId, userComment, logComment);
			Logger.LogError("TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): workflow → {Workflow}", contentItem, state, logType, userId, userComment, workflow);
            this.workflowLogger.AddWorkflowLog(contentItem.ContentItemId, workflow.WorkflowID, logType, logComment, userId);
        }

        private string ReplaceNotificationTokens(string text, Entities.Workflow workflow, ContentItem item, WorkflowState state, int userId, string comment = "")
        {
			Logger.LogError(">>> Entering ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment})", text, workflow, item, state, userId, comment);
            var user = this.userController.GetUserById(workflow.PortalID, userId);
            var datetime = DateTime.UtcNow;
			Logger.LogError("ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment}): user → {User}", text, workflow, item, state, userId, comment, user);
            var result = text.Replace("[USER]", user != null ? user.DisplayName : string.Empty);
            result = result.Replace("[DATE]", datetime.ToString("F", CultureInfo.CurrentCulture));
            result = result.Replace("[STATE]", state != null ? state.StateName : string.Empty);
            result = result.Replace("[WORKFLOW]", workflow.WorkflowName);
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : string.Empty);
            result = result.Replace("[COMMENT]", !string.IsNullOrEmpty(comment) ? comment : string.Empty);
			Logger.LogError("ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment}): result → {Result}", text, workflow, item, state, userId, comment, result);
			Logger.LogWarning("<<< Exiting ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment})", text, workflow, item, state, userId, comment);
            return result;
        }

        private class ReviewersDto
        {
            public List<RoleInfo> Roles { get; set; }

            public List<UserInfo> Users { get; set; }
        }
    }
}
