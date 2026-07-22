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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string ContentWorkflowNotificationType = "ContentWorkflowNotification";
		Logger.LogDebug(">>> Entering WorkflowEngine(contentController={ContentController})", contentController);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        private const string ContentWorkflowNotificationNoActionType = "ContentWorkflowNoActionNotification";
        private const string ContentWorkflowNotificationStartWorkflowType = "ContentWorkflowStartWorkflowNotification";
        private readonly IContentController contentController = contentController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IContentController>();
        private readonly IWorkflowRepository workflowRepository = WorkflowRepository.Instance;
		Logger.LogDebug("WorkflowEngine(contentController={ContentController}): contentController → {ContentController1}", contentController, contentController);
		Logger.LogDebug("WorkflowEngine(contentController={ContentController}): workflowRepository → {WorkflowRepository}", contentController, workflowRepository);
        private readonly IWorkflowStateRepository workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly IWorkflowStatePermissionsRepository workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
		Logger.LogDebug("WorkflowEngine(contentController={ContentController}): workflowStateRepository → {WorkflowStateRepository}", contentController, workflowStateRepository);
		Logger.LogDebug("WorkflowEngine(contentController={ContentController}): workflowStatePermissionsRepository → {WorkflowStatePermissionsRepository}", contentController, workflowStatePermissionsRepository);
        private readonly IWorkflowLogRepository workflowLogRepository = WorkflowLogRepository.Instance;
        private readonly IWorkflowActionManager workflowActionManager = WorkflowActionManager.Instance;
		Logger.LogDebug("WorkflowEngine(contentController={ContentController}): workflowLogRepository → {WorkflowLogRepository}", contentController, workflowLogRepository);
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
        }

        /// <inheritdoc />
        public UserInfo GetStartedDraftStateUser(ContentItem contentItem)
        {
            return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.WorkflowStarted);
        }

        /// <inheritdoc />
        public UserInfo GetSubmittedDraftStateUser(ContentItem contentItem)
        {
            return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.DraftCompleted);
        }

        /// <inheritdoc />
        public void StartWorkflow(int workflowId, int contentItemId, int userId)
        {
			Logger.LogDebug(">>> Entering StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            Requires.NotNegative("workflowId", workflowId);

            var contentItem = this.contentController.GetContentItem(contentItemId);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogDebug("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): contentItem → {ContentItem}", workflowId, contentItemId, userId, contentItem);
			Logger.LogDebug("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): workflow → {Workflow}", workflowId, contentItemId, userId, workflow);

            // If already exists a started workflow
            if (workflow != null && !this.IsWorkflowCompleted(contentItem))
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowAlreadyStarted", "Workflow cannot get started for this Content Item. It already has a started workflow."));
            }

            if (workflow == null || workflow.WorkflowID != workflowId)
            {
                workflow = this.workflowRepository.GetWorkflow(workflowId);
            }

			Logger.LogInformation("WorkflowEngine(contentController)={ContentController}: find query executed in {Arg1} ms", contentController, __logStart.ElapsedMilliseconds);
            var initialTransaction = CreateInitialTransaction(contentItemId, workflow.FirstState.StateID, userId);
			Logger.LogInformation("StartWorkflow(workflowId,contentItemId,userId)={WorkflowId},{ContentItemId},{UserId}: find query executed in {Arg3} ms", workflowId, contentItemId, userId, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): initialTransaction → {InitialTransaction}", workflowId, contentItemId, userId, initialTransaction);

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
			Logger.LogInformation("WorkflowEngine(contentController)={ContentController}: delete query executed in {Arg1} ms", contentController, __logStart.ElapsedMilliseconds);

			Logger.LogInformation("StartWorkflow(workflowId,contentItemId,userId)={WorkflowId},{ContentItemId},{UserId}: delete query executed in {Arg3} ms", workflowId, contentItemId, userId, __logStart.ElapsedMilliseconds);
            // Add logs
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowStarted, userId);
            this.AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, userId);

            // Perform action after starting workflow
            this.PerformWorkflowActionOnStateChanged(initialTransaction, WorkflowActionTypes.StartWorkflow);
			Logger.LogDebug("<<< Exiting StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
        }

        /// <inheritdoc />
        public void CompleteState(StateTransaction stateTransaction)
        {
			Logger.LogDebug(">>> Entering CompleteState(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
			Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();

            if (workflow == null)
            {
                return;
            }

            if (this.IsWorkflowCompleted(contentItem)
                        && !(workflow.IsSystem && workflow.States.Count() == 1))
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowSystemWorkflowStateCannotComplete", "System workflow state cannot be completed."));
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;

            if (!isFirstState && !this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
			Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
			Logger.LogInformation("CompleteState(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
            {
                throw new WorkflowConcurrencyException();
            }

            var nextState = GetNextWorkflowState(workflow, contentItem.StateID);
            if (nextState.StateID == workflow.LastState.StateID)
			Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): nextState → {NextState}", stateTransaction, nextState);
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
			Logger.LogDebug("<<< Exiting CompleteState(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public void DiscardState(StateTransaction stateTransaction)
        {
			Logger.LogDebug(">>> Entering DiscardState(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = this.workflowManager.GetWorkflow(contentItem);
			Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (workflow == null)
			Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
            {
                return;
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;
            var isLastState = workflow.LastState.StateID == contentItem.StateID;

            if (isLastState)
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowCannotDiscard", "Cannot discard on last workflow state"));
            }

            if (!isFirstState && !this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
			Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
			Logger.LogInformation("DiscardState(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
            {
                throw new WorkflowConcurrencyException();
            }

            var previousState = GetPreviousWorkflowState(workflow, contentItem.StateID);
            if (previousState.StateID == workflow.LastState.StateID)
			Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): previousState → {PreviousState}", stateTransaction, previousState);
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
			Logger.LogDebug("<<< Exiting DiscardState(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public bool IsWorkflowCompleted(int contentItemId)
        {
			Logger.LogDebug(">>> Entering IsWorkflowCompleted(contentItemId={ContentItemId})", contentItemId);
            var contentItem = this.contentController.GetContentItem(contentItemId);
			Logger.LogDebug("IsWorkflowCompleted(contentItemId={ContentItemId}): contentItem → {ContentItem}", contentItemId, contentItem);
			Logger.LogDebug("<<< Exiting IsWorkflowCompleted(contentItemId={ContentItemId})", contentItemId);
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
			Logger.LogDebug(">>> Entering IsWorkflowOnDraft(contentItemId={ContentItemId})", contentItemId);
            var contentItem = this.contentController.GetContentItem(contentItemId); // Ensure DB values
			Logger.LogDebug("IsWorkflowOnDraft(contentItemId={ContentItemId}): contentItem → {ContentItem}", contentItemId, contentItem);
			Logger.LogDebug("<<< Exiting IsWorkflowOnDraft(contentItemId={ContentItemId})", contentItemId);
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
			Logger.LogDebug(">>> Entering DiscardWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);

			Logger.LogDebug("DiscardWorkflow(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
			Logger.LogDebug("DiscardWorkflow(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
			Logger.LogInformation("DiscardWorkflow(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
            {
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardWorkflow);

            var workflow = this.workflowManager.GetWorkflow(contentItem);
            this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);
			Logger.LogDebug("DiscardWorkflow(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);

            // Logs
            this.AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowDiscarded, stateTransaction.UserId);

            // Notifications
            this.SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.DiscardWorkflow);
            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardWorkflow);
			Logger.LogDebug("<<< Exiting DiscardWorkflow(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        public void CompleteWorkflow(StateTransaction stateTransaction)
        {
			Logger.LogDebug(">>> Entering CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);

			Logger.LogDebug("CompleteWorkflow(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
			Logger.LogDebug("CompleteWorkflow(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
			Logger.LogInformation("CompleteWorkflow(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
            {
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteWorkflow);

            var workflow = this.workflowManager.GetWorkflow(contentItem);
            this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);
			Logger.LogDebug("CompleteWorkflow(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);

            // Logs
            this.AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowApproved, stateTransaction.UserId);

            // Notifications
            this.SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.CompleteWorkflow);
            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteWorkflow);
			Logger.LogDebug("<<< Exiting CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowEngine> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IWorkflowEngine>();
        }

        private static List<RoleInfo> GetRolesFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
        {
            return (from permission in permissions
                    where permission.AllowAccess && permission.RoleID > Null.NullInteger
                    select RoleController.Instance.GetRoleById(settings.PortalId, permission.RoleID)).ToList();
        }

        private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
        {
			Logger.LogDebug(">>> Entering IsAdministratorRoleAlreadyIncluded(settings={Settings},roles={Roles})", settings, roles);
			Logger.LogDebug("<<< Exiting IsAdministratorRoleAlreadyIncluded(settings={Settings},roles={Roles})", settings, roles);
            return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
        }

        private static List<UserInfo> GetUsersFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
        {
            return (from permission in permissions
                    where permission.AllowAccess && permission.UserID > Null.NullInteger
                    select UserController.GetUserById(settings.PortalId, permission.UserID)).ToList();
        }

        private static List<UserInfo> IncludeSuperUsers(List<UserInfo> users)
        {
			Logger.LogDebug(">>> Entering IncludeSuperUsers(users={Users})", users);
            var superUsers = UserController.GetUsers(false, true, Null.NullInteger);
            foreach (UserInfo superUser in superUsers)
            {
			Logger.LogDebug("IncludeSuperUsers(users={Users}): superUsers → {SuperUsers}", users, superUsers);
                if (IsSuperUserNotIncluded(users, superUser))
                {
                    users.Add(superUser);
                }
            }

			Logger.LogDebug("<<< Exiting IncludeSuperUsers(users={Users})", users);
            return users.ToList();
        }

        private static bool IsSuperUserNotIncluded(IEnumerable<UserInfo> users, UserInfo superUser)
        {
			Logger.LogDebug(">>> Entering IsSuperUserNotIncluded(users={Users},superUser={SuperUser})", users, superUser);
			Logger.LogDebug("<<< Exiting IsSuperUserNotIncluded(users={Users},superUser={SuperUser})", users, superUser);
            return users.All(u => u.UserID != superUser.UserID);
        }

        private static string GetWorkflowActionComment(WorkflowLogType logType)
        {
			Logger.LogDebug(">>> Entering GetWorkflowActionComment(logType={LogType})", logType);
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
			Logger.LogDebug("GetWorkflowActionComment(logType={LogType}): logName → {LogName}", logType, logName);
			Logger.LogDebug("<<< Exiting GetWorkflowActionComment(logType={LogType})", logType);
            return Localization.GetString(logName + ".Comment");
        }

        private static StateTransaction CreateInitialTransaction(int contentItemId, int stateId, int userId)
        {
			Logger.LogDebug(">>> Entering CreateInitialTransaction(contentItemId={ContentItemId},stateId={StateId},userId={UserId})", contentItemId, stateId, userId);
			Logger.LogDebug("<<< Exiting CreateInitialTransaction(contentItemId={ContentItemId},stateId={StateId},userId={UserId})", contentItemId, stateId, userId);
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
            var isDirectPublishWorkflow = workflow.IsSystem && workflow.States.Count() == 1;
            var draftSubmitted = workflow.FirstState.StateID != currentStateId;
            return !(isDirectPublishWorkflow || !draftSubmitted);
        }

        private static string GetWorkflowNotificationContext(ContentItem contentItem, WorkflowState state)
        {
            return $"{contentItem.ContentItemId}:{state.WorkflowID}:{state.StateID}";
        }

        private static WorkflowState GetNextWorkflowState(Entities.Workflow workflow, int stateId)
        {
            WorkflowState nextState = null;
            var states = workflow.States.OrderBy(s => s.Order);
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
            if (index < states.Count())
            {
                nextState = states.ElementAt(index);
            }

            return nextState ?? workflow.FirstState;
        }

        private static WorkflowState GetPreviousWorkflowState(Entities.Workflow workflow, int stateId)
        {
            WorkflowState previousState = null;
            var states = workflow.States.OrderBy(s => s.Order);
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

            return previousState ?? workflow.LastState;
        }

        private void PerformWorkflowActionOnStateChanged(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
			Logger.LogDebug(">>> Entering PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
			Logger.LogDebug("PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType}): contentItem → {ContentItem}", stateTransaction, actionType, contentItem);
			Logger.LogDebug("PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType}): workflowAction → {WorkflowAction}", stateTransaction, actionType, workflowAction);
            if (workflowAction != null)
            {
                workflowAction.DoActionOnStateChanged(stateTransaction);
            }
			Logger.LogDebug("<<< Exiting PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
        }

        private void PerformWorkflowActionOnStateChanging(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
			Logger.LogDebug(">>> Entering PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
            var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
			Logger.LogDebug("PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType}): contentItem → {ContentItem}", stateTransaction, actionType, contentItem);
			Logger.LogDebug("PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType}): workflowAction → {WorkflowAction}", stateTransaction, actionType, workflowAction);
            if (workflowAction != null)
            {
                workflowAction.DoActionOnStateChanging(stateTransaction);
            }
			Logger.LogDebug("<<< Exiting PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
        }

        private IWorkflowAction GetWorkflowActionInstance(ContentItem contentItem, WorkflowActionTypes actionType)
        {
            return this.workflowActionManager.GetWorkflowActionInstance(contentItem.ContentTypeId, actionType);
        }

        private void UpdateContentItemWorkflowState(int stateId, ContentItem item)
        {
			Logger.LogDebug(">>> Entering UpdateContentItemWorkflowState(stateId={StateId},item={Item})", stateId, item);
            item.StateID = stateId;
            this.contentController.UpdateContentItem(item);
			Logger.LogDebug("<<< Exiting UpdateContentItemWorkflowState(stateId={StateId},item={Item})", stateId, item);
			Logger.LogDebug("UpdateContentItemWorkflowState(stateId={StateId},item={Item}): item.StateID → {Arg2}", stateId, item, item.StateID);
        }

        private UserInfo GetUserThatHaveStartedOrSubmittedDraftState(Entities.Workflow workflow, ContentItem contentItem, StateTransaction stateTransaction)
        {
            bool wasDraftSubmitted = WasDraftSubmitted(workflow, stateTransaction.CurrentStateId);
            if (wasDraftSubmitted)
            {
                return this.GetSubmittedDraftStateUser(contentItem);
            }

            return this.GetStartedDraftStateUser(contentItem);
        }

        private UserInfo GetUserByWorkflowLogType(ContentItem contentItem, WorkflowLogType type)
        {
            var workflow = this.workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            {
                return null;
            }

            var logs = this.workflowLogRepository.GetWorkflowLogs(contentItem.ContentItemId, workflow.WorkflowID);

            var logDraftCompleted = logs
                .OrderByDescending(l => l.Date)
                .FirstOrDefault(l => l.Type == (int)type);

            if (logDraftCompleted != null && logDraftCompleted.User != Null.NullInteger)
            {
                return this.userController.GetUserById(workflow.PortalID, logDraftCompleted.User);
            }

            return null;
        }

        private void DeleteWorkflowNotifications(ContentItem contentItem, WorkflowState state)
        {
            var context = GetWorkflowNotificationContext(contentItem, state);
            var notificationTypeId = this.notificationsController.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId;
            this.DeleteNotificationsByType(notificationTypeId, context);
            notificationTypeId = this.notificationsController.GetNotificationType(ContentWorkflowNotificationStartWorkflowType).NotificationTypeId;
            this.DeleteNotificationsByType(notificationTypeId, context);
        }

        private void DeleteNotificationsByType(int notificationTypeId, string context)
        {
            var notifications = this.notificationsController.GetNotificationByContext(notificationTypeId, context);
            foreach (var notification in notifications)
            {
                this.notificationsController.DeleteAllNotificationRecipients(notification.NotificationID);
            }
        }

        private void SendNotificationToAuthor(StateTransaction stateTransaction, WorkflowState state, Entities.Workflow workflow, ContentItem contentItem, WorkflowActionTypes workflowActionType)
        {
            try
            {
                if (!state.SendNotification)
                {
                    return;
                }

                var user = this.GetUserThatHaveStartedOrSubmittedDraftState(workflow, contentItem, stateTransaction);
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
                if (workflowAction == null)
                {
                    return;
                }

                var message = workflowAction.GetActionMessage(stateTransaction, state);

                var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationNoActionType);

                this.notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in WorkflowEngine(contentController={ContentController}): {Arg1}", contentController, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogDebug("<<< Exiting SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType})", stateTransaction, state, workflow, contentItem, workflowActionType);
        }

        private void SendNotificationToWorkflowStarter(StateTransaction stateTransaction, Entities.Workflow workflow, ContentItem contentItem, int starterUserId, WorkflowActionTypes workflowActionType)
        {
            try
            {
                if (!workflow.FirstState.SendNotification)
                {
                    return;
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
                if (workflowAction == null)
                {
                    return;
                }

                var user = this.userController.GetUser(workflow.PortalID, starterUserId);

                var message = workflowAction.GetActionMessage(stateTransaction, workflow.FirstState);

                var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, workflow.FirstState), stateTransaction, message, ContentWorkflowNotificationStartWorkflowType);

                this.notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogDebug("<<< Exiting SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType})", stateTransaction, workflow, contentItem, starterUserId, workflowActionType);
        }

        private void SendNotificationsToReviewers(ContentItem contentItem, WorkflowState state, StateTransaction stateTransaction, WorkflowActionTypes workflowActionType, PortalSettings portalSettings)
        {
            try
            {
                if (!state.SendNotification && !state.SendNotificationToAdministrators)
                {
                    return;
                }

                var reviewers = this.GetUserAndRolesForStateReviewers(portalSettings, state);

                if (reviewers.Roles.Count == 0 && reviewers.Users.Count == 0)
                {
                    return; // If there are no receivers, the notification is avoided
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
                if (workflowAction == null)
                {
                    return;
                }

                var message = workflowAction.GetActionMessage(stateTransaction, state);

                var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationType);

                this.notificationsController.SendNotification(notification, portalSettings.PortalId, reviewers.Roles.ToList(), reviewers.Users.ToList());
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogDebug("<<< Exiting SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings})", contentItem, state, stateTransaction, workflowActionType, portalSettings);
        }

        private Notification GetNotification(string workflowContext, StateTransaction stateTransaction, ActionMessage message, string notificationType)
        {
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
            return notification;
        }

        private ReviewersDto GetUserAndRolesForStateReviewers(PortalSettings portalSettings, WorkflowState state)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var reviewers = new ReviewersDto
            {
                Roles = new List<RoleInfo>(),
                Users = new List<UserInfo>(),
            };
            if (state.SendNotification)
            {
                var permissions = this.workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(state.StateID).ToArray();
                reviewers.Users = GetUsersFromPermissions(portalSettings, permissions);
                reviewers.Roles = GetRolesFromPermissions(portalSettings, permissions);
            }

            if (state.SendNotificationToAdministrators)
            {
                if (!IsAdministratorRoleAlreadyIncluded(portalSettings, reviewers.Roles))
                {
                    var adminRole = RoleController.Instance.GetRoleByName(portalSettings.PortalId, portalSettings.AdministratorRoleName);
                    reviewers.Roles.Add(adminRole);
                }

                reviewers.Users = IncludeSuperUsers(reviewers.Users);
            }

            return reviewers;
        }

        private void AddWorkflowCommentLog(ContentItem contentItem, int userId, string userComment)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }

            var state = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
			Logger.LogDebug("<<< Exiting AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment})", contentItem, userId, userComment);
        }

        private void AddWorkflowCommentLog(ContentItem contentItem, WorkflowState state, int userId, string userComment)
        {
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }

            this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowLogType logType, int userId, string userComment = null)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            this.AddWorkflowLog(contentItem, state, logType, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment = null)
        {
            try
            {
                this.TryAddWorkflowLog(contentItem, state, logType, userId, userComment);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void TryAddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment)
        {
            var workflow = this.workflowManager.GetWorkflow(contentItem);
            var logTypeText = GetWorkflowActionComment(logType);
            var logComment = this.ReplaceNotificationTokens(logTypeText, workflow, contentItem, state, userId, userComment);
            this.workflowLogger.AddWorkflowLog(contentItem.ContentItemId, workflow.WorkflowID, logType, logComment, userId);
        }

        private string ReplaceNotificationTokens(string text, Entities.Workflow workflow, ContentItem item, WorkflowState state, int userId, string comment = "")
        {
            var user = this.userController.GetUserById(workflow.PortalID, userId);
            var datetime = DateTime.UtcNow;
            var result = text.Replace("[USER]", user != null ? user.DisplayName : string.Empty);
            result = result.Replace("[DATE]", datetime.ToString("F", CultureInfo.CurrentCulture));
            result = result.Replace("[STATE]", state != null ? state.StateName : string.Empty);
            result = result.Replace("[WORKFLOW]", workflow.WorkflowName);
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : string.Empty);
            result = result.Replace("[COMMENT]", !string.IsNullOrEmpty(comment) ? comment : string.Empty);
            return result;
        }

        private class ReviewersDto
        {
            public List<RoleInfo> Roles { get; set; }

            public List<UserInfo> Users { get; set; }
        }
    }
}
