// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotNetNuke.Abstractions.Security.Permissions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
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

/// <summary>The default <see cref="IWorkflowEngine"/> implementation.</summary>
/// <param name="contentController">The <see cref="IContentController"/> instance.</param>
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
	Logger.LogDebug(">>> Entering WorkflowEngine()");
    }

    /// <inheritdoc />
    public UserInfo GetStartedDraftStateUser(ContentItem contentItem)
    {
		Logger.LogDebug(">>> Entering GetStartedDraftStateUser(contentItem={ContentItem})", contentItem);
		Logger.LogDebug("<<< Exiting GetStartedDraftStateUser(contentItem={ContentItem})", contentItem);
        return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.WorkflowStarted);
    }

    /// <inheritdoc />
    public UserInfo GetSubmittedDraftStateUser(ContentItem contentItem)
    {
		Logger.LogDebug(">>> Entering GetSubmittedDraftStateUser(contentItem={ContentItem})", contentItem);
		Logger.LogDebug("<<< Exiting GetSubmittedDraftStateUser(contentItem={ContentItem})", contentItem);
        return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.DraftCompleted);
    }

    /// <inheritdoc />
    public void StartWorkflow(int workflowId, int contentItemId, int userId)
    {
		Logger.LogDebug(">>> Entering StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
        Requires.NotNegative("workflowId", workflowId);

        var contentItem = this.contentController.GetContentItem(contentItemId);
		Logger.LogDebug("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): contentItem → {ContentItem}", workflowId, contentItemId, userId, contentItem);
        var workflow = this.workflowManager.GetWorkflow(contentItem);
		Logger.LogDebug("StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId}): workflow → {Workflow}", workflowId, contentItemId, userId, workflow);

        // If already exists a started workflow
        if (workflow != null && !this.IsWorkflowCompleted(contentItem))
        {
			Logger.LogWarning("Throwing WorkflowInvalidOperationException from StartWorkflow(workflowId={WorkflowId},contentItemId={ContentItemId},userId={UserId})", workflowId, contentItemId, userId);
            throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowAlreadyStarted", "Workflow cannot get started for this Content Item. It already has a started workflow."));
        }

		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        if (workflow == null || workflow.WorkflowID != workflowId)
        {
            workflow = this.workflowRepository.GetWorkflow(workflowId);
        }
		Logger.LogInformation("StartWorkflow(workflowId,contentItemId,userId)={WorkflowId},{ContentItemId},{UserId}: find query executed in {Arg3} ms", workflowId, contentItemId, userId, __logStart.ElapsedMilliseconds);

        var initialTransaction = CreateInitialTransaction(contentItemId, workflow.FirstState.StateID, userId);
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
		Logger.LogInformation("StartWorkflow(workflowId,contentItemId,userId)={WorkflowId},{ContentItemId},{UserId}: delete query executed in {Arg3} ms", workflowId, contentItemId, userId, __logStart.ElapsedMilliseconds);
        this.workflowLogRepository.DeleteWorkflowLogs(contentItemId, workflowId);

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
		Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
		Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);

        if (workflow == null)
        {
            return;
        }

        if (this.IsWorkflowCompleted(contentItem)
            && !(workflow.IsSystem && workflow.States.Count() == 1))
        {
			Logger.LogWarning("Throwing WorkflowInvalidOperationException from CompleteState(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowSystemWorkflowStateCannotComplete", "System workflow state cannot be completed."));
        }

        var isFirstState = workflow.FirstState.StateID == contentItem.StateID;

        if (!isFirstState && !this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
        {
			Logger.LogWarning("Throwing WorkflowSecurityException from CompleteState(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
        }

		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
        if (currentState.StateID != stateTransaction.CurrentStateId)
		Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
		Logger.LogInformation("CompleteState(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
        {
			Logger.LogWarning("Throwing WorkflowConcurrencyException from CompleteState(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowConcurrencyException();
        }

        var nextState = GetNextWorkflowState(workflow, contentItem.StateID);
		Logger.LogDebug("CompleteState(stateTransaction={StateTransaction}): nextState → {NextState}", stateTransaction, nextState);
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
		Logger.LogDebug("<<< Exiting CompleteState(stateTransaction={StateTransaction})", stateTransaction);
    }

    /// <inheritdoc />
    public void DiscardState(StateTransaction stateTransaction)
    {
		Logger.LogDebug(">>> Entering DiscardState(stateTransaction={StateTransaction})", stateTransaction);
        var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
        var workflow = this.workflowManager.GetWorkflow(contentItem);
		Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
		Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
        if (workflow == null)
        {
            return;
        }

        var isFirstState = workflow.FirstState.StateID == contentItem.StateID;
        var isLastState = workflow.LastState.StateID == contentItem.StateID;

        if (isLastState)
        {
			Logger.LogWarning("Throwing WorkflowInvalidOperationException from DiscardState(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowCannotDiscard", "Cannot discard on last workflow state"));
        }

        if (!isFirstState && !this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
        {
			Logger.LogWarning("Throwing WorkflowSecurityException from DiscardState(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
        }

		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
        if (currentState.StateID != stateTransaction.CurrentStateId)
		Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
		Logger.LogInformation("DiscardState(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
        {
			Logger.LogWarning("Throwing WorkflowConcurrencyException from DiscardState(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowConcurrencyException();
        }

        var previousState = GetPreviousWorkflowState(workflow, contentItem.StateID);
		Logger.LogDebug("DiscardState(stateTransaction={StateTransaction}): previousState → {PreviousState}", stateTransaction, previousState);
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
		Logger.LogDebug("<<< Exiting DiscardState(stateTransaction={StateTransaction})", stateTransaction);
    }

    /// <inheritdoc />
    public bool IsWorkflowCompleted(int contentItemId)
    {
		Logger.LogDebug(">>> Entering IsWorkflowCompleted(contentItemId={ContentItemId})", contentItemId);
        var contentItem = this.contentController.GetContentItem(contentItemId);
		Logger.LogDebug("IsWorkflowCompleted(contentItemId={ContentItemId}): contentItem → {ContentItem}", contentItemId, contentItem);
		Logger.LogDebug("<<< Exiting IsWorkflowCompleted(contentItemId={ContentItemId})", contentItemId);
		Logger.LogDebug("<<< Exiting IsWorkflowCompleted(contentItem={ContentItem})", contentItem);
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
		Logger.LogDebug("<<< Exiting IsWorkflowOnDraft(contentItem={ContentItem})", contentItem);
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
			Logger.LogWarning("Throwing WorkflowConcurrencyException from DiscardWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowConcurrencyException();
        }

        // before-change action
        this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardWorkflow);

        var workflow = this.workflowManager.GetWorkflow(contentItem);
		Logger.LogDebug("DiscardWorkflow(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
        this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

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
        var workflow = this.workflowManager.GetWorkflow(contentItem);
		Logger.LogDebug("CompleteWorkflow(stateTransaction={StateTransaction}): workflow → {Workflow}", stateTransaction, workflow);
		Logger.LogDebug("CompleteWorkflow(stateTransaction={StateTransaction}): contentItem → {ContentItem}", stateTransaction, contentItem);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();

        var currentState = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
        if (currentState.StateID != stateTransaction.CurrentStateId)
		Logger.LogDebug("CompleteWorkflow(stateTransaction={StateTransaction}): currentState → {CurrentState}", stateTransaction, currentState);
		Logger.LogInformation("CompleteWorkflow(stateTransaction)={StateTransaction}: find query executed in {Arg1} ms", stateTransaction, __logStart.ElapsedMilliseconds);
        {
			Logger.LogWarning("Throwing WorkflowConcurrencyException from CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowConcurrencyException();
        }

        if (!this.workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
        {
			Logger.LogWarning("Throwing WorkflowSecurityException from CompleteWorkflow(stateTransaction={StateTransaction})", stateTransaction);
            throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
        }

        // before-change action
        this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteWorkflow);

        this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

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
		Logger.LogDebug(">>> Entering GetRolesFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
		Logger.LogDebug("<<< Exiting GetRolesFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
        return (
                from IPermissionInfo permission in permissions
                where permission.AllowAccess && permission.RoleId > Null.NullInteger
                select RoleController.Instance.GetRoleById(settings.PortalId, permission.RoleId))
            .ToList();
    }

    private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
    {
		Logger.LogDebug(">>> Entering IsAdministratorRoleAlreadyIncluded(settings={Settings},roles={Roles})", settings, roles);
		Logger.LogDebug("<<< Exiting IsAdministratorRoleAlreadyIncluded(settings={Settings},roles={Roles})", settings, roles);
        return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
    }

    private static List<UserInfo> GetUsersFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
    {
		Logger.LogDebug(">>> Entering GetUsersFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
		Logger.LogDebug("<<< Exiting GetUsersFromPermissions(settings={Settings},permissions={Permissions})", settings, permissions);
        return (
                from IPermissionInfo permission in permissions
                where permission.AllowAccess && permission.UserId > Null.NullInteger
                select UserController.GetUserById(settings.PortalId, permission.UserId))
            .ToList();
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

    private static bool WasDraftSubmitted(Workflow workflow, int currentStateId)
    {
		Logger.LogDebug(">>> Entering WasDraftSubmitted(workflow={Workflow},currentStateId={CurrentStateId})", workflow, currentStateId);
        var isDirectPublishWorkflow = workflow.IsSystem && workflow.States.Count() == 1;
        var draftSubmitted = workflow.FirstState.StateID != currentStateId;
		Logger.LogDebug("WasDraftSubmitted(workflow={Workflow},currentStateId={CurrentStateId}): isDirectPublishWorkflow → {IsDirectPublishWorkflow}", workflow, currentStateId, isDirectPublishWorkflow);
		Logger.LogDebug("<<< Exiting WasDraftSubmitted(workflow={Workflow},currentStateId={CurrentStateId})", workflow, currentStateId);
        return !(isDirectPublishWorkflow || !draftSubmitted);
    }

    private static string GetWorkflowNotificationContext(ContentItem contentItem, WorkflowState state)
    {
		Logger.LogDebug(">>> Entering GetWorkflowNotificationContext(contentItem={ContentItem},state={State})", contentItem, state);
		Logger.LogDebug("<<< Exiting GetWorkflowNotificationContext(contentItem={ContentItem},state={State})", contentItem, state);
        return $"{contentItem.ContentItemId}:{state.WorkflowID}:{state.StateID}";
    }

    private static WorkflowState GetNextWorkflowState(Workflow workflow, int stateId)
    {
		Logger.LogDebug(">>> Entering GetNextWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
        WorkflowState nextState = null;
        var states = workflow.States.OrderBy(s => s.Order).ToList();
		Logger.LogDebug("GetNextWorkflowState(workflow={Workflow},stateId={StateId}): states → {States}", workflow, stateId, states);
		Logger.LogDebug("GetNextWorkflowState(workflow={Workflow},stateId={StateId}): nextState → {NextState}", workflow, stateId, nextState);
        int index;

        // locate the current state
        for (index = 0; index < states.Count; index++)
        {
            if (states.ElementAt(index).StateID == stateId)
            {
                break;
            }
        }

        index += 1;
        if (index < states.Count)
        {
            nextState = states.ElementAt(index);
        }

		Logger.LogDebug("<<< Exiting GetNextWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
        return nextState ?? workflow.FirstState;
    }

    private static WorkflowState GetPreviousWorkflowState(Workflow workflow, int stateId)
    {
		Logger.LogDebug(">>> Entering GetPreviousWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
        WorkflowState previousState = null;
        var states = workflow.States.OrderBy(s => s.Order).ToList();
		Logger.LogDebug("GetPreviousWorkflowState(workflow={Workflow},stateId={StateId}): states → {States}", workflow, stateId, states);
		Logger.LogDebug("GetPreviousWorkflowState(workflow={Workflow},stateId={StateId}): previousState → {PreviousState}", workflow, stateId, previousState);
        int index;

        if (workflow.FirstState.StateID == stateId)
        {
            return workflow.LastState;
        }

        // locate the current state
        for (index = 0; index < states.Count; index++)
        {
            if (states.ElementAt(index).StateID == stateId)
            {
                previousState = states.ElementAt(index - 1);
                break;
            }
        }

		Logger.LogDebug("<<< Exiting GetPreviousWorkflowState(workflow={Workflow},stateId={StateId})", workflow, stateId);
        return previousState ?? workflow.LastState;
    }

    private void PerformWorkflowActionOnStateChanged(StateTransaction stateTransaction, WorkflowActionTypes actionType)
    {
		Logger.LogDebug(">>> Entering PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
        var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
        var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
		Logger.LogDebug("PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType}): workflowAction → {WorkflowAction}", stateTransaction, actionType, workflowAction);
		Logger.LogDebug("PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType}): contentItem → {ContentItem}", stateTransaction, actionType, contentItem);
        workflowAction?.DoActionOnStateChanged(stateTransaction);
		Logger.LogDebug("<<< Exiting PerformWorkflowActionOnStateChanged(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
    }

    private void PerformWorkflowActionOnStateChanging(StateTransaction stateTransaction, WorkflowActionTypes actionType)
    {
		Logger.LogDebug(">>> Entering PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
        var contentItem = this.contentController.GetContentItem(stateTransaction.ContentItemId);
        var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
		Logger.LogDebug("PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType}): workflowAction → {WorkflowAction}", stateTransaction, actionType, workflowAction);
		Logger.LogDebug("PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType}): contentItem → {ContentItem}", stateTransaction, actionType, contentItem);
        if (workflowAction != null)
        {
            workflowAction.DoActionOnStateChanging(stateTransaction);
        }
		Logger.LogDebug("<<< Exiting PerformWorkflowActionOnStateChanging(stateTransaction={StateTransaction},actionType={ActionType})", stateTransaction, actionType);
    }

    private IWorkflowAction GetWorkflowActionInstance(ContentItem contentItem, WorkflowActionTypes actionType)
    {
		Logger.LogDebug(">>> Entering GetWorkflowActionInstance(contentItem={ContentItem},actionType={ActionType})", contentItem, actionType);
		Logger.LogDebug("<<< Exiting GetWorkflowActionInstance(contentItem={ContentItem},actionType={ActionType})", contentItem, actionType);
        return this.workflowActionManager.GetWorkflowActionInstance(contentItem.ContentTypeId, actionType);
    }

    private void UpdateContentItemWorkflowState(int stateId, ContentItem item)
    {
		Logger.LogDebug(">>> Entering UpdateContentItemWorkflowState(stateId={StateId},item={Item})", stateId, item);
        item.StateID = stateId;
        this.contentController.UpdateContentItem(item);
		Logger.LogDebug("UpdateContentItemWorkflowState(stateId={StateId},item={Item}): item.StateID → {Arg2}", stateId, item, item.StateID);
		Logger.LogDebug("<<< Exiting UpdateContentItemWorkflowState(stateId={StateId},item={Item})", stateId, item);
    }

    private UserInfo GetUserThatHaveStartedOrSubmittedDraftState(Workflow workflow, ContentItem contentItem, StateTransaction stateTransaction)
    {
		Logger.LogDebug(">>> Entering GetUserThatHaveStartedOrSubmittedDraftState(workflow={Workflow},contentItem={ContentItem},stateTransaction={StateTransaction})", workflow, contentItem, stateTransaction);
        var wasDraftSubmitted = WasDraftSubmitted(workflow, stateTransaction.CurrentStateId);
		Logger.LogDebug("GetUserThatHaveStartedOrSubmittedDraftState(workflow={Workflow},contentItem={ContentItem},stateTransaction={StateTransaction}): wasDraftSubmitted → {WasDraftSubmitted}", workflow, contentItem, stateTransaction, wasDraftSubmitted);
		Logger.LogDebug("<<< Exiting GetUserThatHaveStartedOrSubmittedDraftState(workflow={Workflow},contentItem={ContentItem},stateTransaction={StateTransaction})", workflow, contentItem, stateTransaction);
        return wasDraftSubmitted
            ? this.GetSubmittedDraftStateUser(contentItem)
            : this.GetStartedDraftStateUser(contentItem);
    }

    private UserInfo GetUserByWorkflowLogType(ContentItem contentItem, WorkflowLogType type)
    {
		Logger.LogDebug(">>> Entering GetUserByWorkflowLogType(contentItem={ContentItem},type={Type})", contentItem, type);
        var workflow = this.workflowManager.GetWorkflow(contentItem);
        if (workflow == null)
		Logger.LogDebug("GetUserByWorkflowLogType(contentItem={ContentItem},type={Type}): workflow → {Workflow}", contentItem, type, workflow);
        {
            return null;
        }

		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var logs = this.workflowLogRepository.GetWorkflowLogs(contentItem.ContentItemId, workflow.WorkflowID);

		Logger.LogDebug("GetUserByWorkflowLogType(contentItem={ContentItem},type={Type}): logs → {Logs}", contentItem, type, logs);
		Logger.LogInformation("GetUserByWorkflowLogType(contentItem,type)={ContentItem},{Type}: find query executed in {Arg2} ms", contentItem, type, __logStart.ElapsedMilliseconds);
        var logDraftCompleted = logs
            .OrderByDescending(l => l.Date)
            .FirstOrDefault(l => l.Type == (int)type);

        if (logDraftCompleted != null && logDraftCompleted.User != Null.NullInteger)
        {
		Logger.LogDebug("<<< Exiting GetUserByWorkflowLogType(contentItem={ContentItem},type={Type})", contentItem, type);
            return this.userController.GetUserById(workflow.PortalID, logDraftCompleted.User);
        }

        return null;
    }

    private void DeleteWorkflowNotifications(ContentItem contentItem, WorkflowState state)
    {
		Logger.LogDebug(">>> Entering DeleteWorkflowNotifications(contentItem={ContentItem},state={State})", contentItem, state);
        var context = GetWorkflowNotificationContext(contentItem, state);
        var notificationTypeId = this.notificationsController.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId;
		Logger.LogDebug("DeleteWorkflowNotifications(contentItem={ContentItem},state={State}): notificationTypeId → {NotificationTypeId}", contentItem, state, notificationTypeId);
		Logger.LogDebug("DeleteWorkflowNotifications(contentItem={ContentItem},state={State}): context → {Context}", contentItem, state, context);
        this.DeleteNotificationsByType(notificationTypeId, context);
        notificationTypeId = this.notificationsController.GetNotificationType(ContentWorkflowNotificationStartWorkflowType).NotificationTypeId;
        this.DeleteNotificationsByType(notificationTypeId, context);
		Logger.LogDebug("<<< Exiting DeleteWorkflowNotifications(contentItem={ContentItem},state={State})", contentItem, state);
    }

    private void DeleteNotificationsByType(int notificationTypeId, string context)
    {
		Logger.LogDebug(">>> Entering DeleteNotificationsByType(notificationTypeId={NotificationTypeId},context={Context})", notificationTypeId, context);
        var notifications = this.notificationsController.GetNotificationByContext(notificationTypeId, context);
        foreach (var notification in notifications)
        {
		Logger.LogDebug("DeleteNotificationsByType(notificationTypeId={NotificationTypeId},context={Context}): notifications → {Notifications}", notificationTypeId, context, notifications);
            this.notificationsController.DeleteAllNotificationRecipients(notification.NotificationID);
        }
		Logger.LogDebug("<<< Exiting DeleteNotificationsByType(notificationTypeId={NotificationTypeId},context={Context})", notificationTypeId, context);
    }

    private void SendNotificationToAuthor(StateTransaction stateTransaction, WorkflowState state, Workflow workflow, ContentItem contentItem, WorkflowActionTypes workflowActionType)
    {
		Logger.LogDebug(">>> Entering SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType})", stateTransaction, state, workflow, contentItem, workflowActionType);
        try
        {
            if (!state.SendNotification)
            {
                return;
            }

            var user = this.GetUserThatHaveStartedOrSubmittedDraftState(workflow, contentItem, stateTransaction);
			Logger.LogDebug("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): user → {User}", stateTransaction, state, workflow, contentItem, workflowActionType, user);
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
			Logger.LogDebug("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): workflowAction → {WorkflowAction}", stateTransaction, state, workflow, contentItem, workflowActionType, workflowAction);
            if (workflowAction == null)
            {
                return;
            }

            var message = workflowAction.GetActionMessage(stateTransaction, state);
			Logger.LogDebug("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): message → {Message}", stateTransaction, state, workflow, contentItem, workflowActionType, message);

            var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationNoActionType);
			Logger.LogDebug("SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): notification → {Notification}", stateTransaction, state, workflow, contentItem, workflowActionType, notification);

            this.notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
        }
        catch (Exception ex)
        {
			Logger.LogError("Exception in SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType}): {Arg5}", stateTransaction, state, workflow, contentItem, workflowActionType, ex.getMessage());
            Services.Exceptions.Exceptions.LogException(ex);
        }
		Logger.LogDebug("<<< Exiting SendNotificationToAuthor(stateTransaction={StateTransaction},state={State},workflow={Workflow},contentItem={ContentItem},workflowActionType={WorkflowActionType})", stateTransaction, state, workflow, contentItem, workflowActionType);
    }

    private void SendNotificationToWorkflowStarter(StateTransaction stateTransaction, Workflow workflow, ContentItem contentItem, int starterUserId, WorkflowActionTypes workflowActionType)
    {
		Logger.LogDebug(">>> Entering SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType})", stateTransaction, workflow, contentItem, starterUserId, workflowActionType);
        try
        {
            if (!workflow.FirstState.SendNotification)
            {
                return;
            }

            var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
			Logger.LogDebug("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): workflowAction → {WorkflowAction}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, workflowAction);
            if (workflowAction == null)
            {
                return;
            }

            var user = this.userController.GetUser(workflow.PortalID, starterUserId);
			Logger.LogDebug("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): user → {User}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, user);

            var message = workflowAction.GetActionMessage(stateTransaction, workflow.FirstState);
			Logger.LogDebug("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): message → {Message}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, message);

            var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, workflow.FirstState), stateTransaction, message, ContentWorkflowNotificationStartWorkflowType);
			Logger.LogDebug("SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): notification → {Notification}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, notification);

            this.notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
        }
        catch (Exception ex)
        {
			Logger.LogError("Exception in SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType}): {Arg5}", stateTransaction, workflow, contentItem, starterUserId, workflowActionType, ex.getMessage());
            Services.Exceptions.Exceptions.LogException(ex);
        }
		Logger.LogDebug("<<< Exiting SendNotificationToWorkflowStarter(stateTransaction={StateTransaction},workflow={Workflow},contentItem={ContentItem},starterUserId={StarterUserId},workflowActionType={WorkflowActionType})", stateTransaction, workflow, contentItem, starterUserId, workflowActionType);
    }

    private void SendNotificationsToReviewers(ContentItem contentItem, WorkflowState state, StateTransaction stateTransaction, WorkflowActionTypes workflowActionType, PortalSettings portalSettings)
    {
		Logger.LogDebug(">>> Entering SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings})", contentItem, state, stateTransaction, workflowActionType, portalSettings);
        try
        {
            if (!state.SendNotification && !state.SendNotificationToAdministrators)
            {
                return;
            }

            var reviewers = this.GetUserAndRolesForStateReviewers(portalSettings, state);
			Logger.LogDebug("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): reviewers → {Reviewers}", contentItem, state, stateTransaction, workflowActionType, portalSettings, reviewers);

            if (reviewers.Roles.Count == 0 && reviewers.Users.Count == 0)
            {
                return; // If there are no receivers, the notification is avoided
            }

            var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
			Logger.LogDebug("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): workflowAction → {WorkflowAction}", contentItem, state, stateTransaction, workflowActionType, portalSettings, workflowAction);
            if (workflowAction == null)
            {
                return;
            }

            var message = workflowAction.GetActionMessage(stateTransaction, state);
			Logger.LogDebug("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): message → {Message}", contentItem, state, stateTransaction, workflowActionType, portalSettings, message);

            var notification = this.GetNotification(GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationType);
			Logger.LogDebug("SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): notification → {Notification}", contentItem, state, stateTransaction, workflowActionType, portalSettings, notification);

            this.notificationsController.SendNotification(notification, portalSettings.PortalId, reviewers.Roles.ToList(), reviewers.Users.ToList());
        }
        catch (Exception ex)
        {
			Logger.LogError("Exception in SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings}): {Arg5}", contentItem, state, stateTransaction, workflowActionType, portalSettings, ex.getMessage());
            Services.Exceptions.Exceptions.LogException(ex);
        }
		Logger.LogDebug("<<< Exiting SendNotificationsToReviewers(contentItem={ContentItem},state={State},stateTransaction={StateTransaction},workflowActionType={WorkflowActionType},portalSettings={PortalSettings})", contentItem, state, stateTransaction, workflowActionType, portalSettings);
    }

    private Notification GetNotification(string workflowContext, StateTransaction stateTransaction, ActionMessage message, string notificationType)
    {
		Logger.LogDebug(">>> Entering GetNotification(workflowContext={WorkflowContext},stateTransaction={StateTransaction},message={Message},notificationType={NotificationType})", workflowContext, stateTransaction, message, notificationType);
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
		Logger.LogDebug("<<< Exiting GetNotification(workflowContext={WorkflowContext},stateTransaction={StateTransaction},message={Message},notificationType={NotificationType})", workflowContext, stateTransaction, message, notificationType);
        return notification;
    }

    private ReviewersDto GetUserAndRolesForStateReviewers(PortalSettings portalSettings, WorkflowState state)
    {
		Logger.LogDebug(">>> Entering GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State})", portalSettings, state);
        var reviewers = new ReviewersDto
        {
            Roles = [],
            Users = [],
        };
        if (state.SendNotification)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var permissions = this.workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(state.StateID).ToArray();
            reviewers.Users = GetUsersFromPermissions(portalSettings, permissions);
			Logger.LogDebug("GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State}): permissions → {Permissions}", portalSettings, state, permissions);
			Logger.LogInformation("GetUserAndRolesForStateReviewers(portalSettings,state)={PortalSettings},{State}: find query executed in {Arg2} ms", portalSettings, state, __logStart.ElapsedMilliseconds);
            reviewers.Roles = GetRolesFromPermissions(portalSettings, permissions);
        }

        if (state.SendNotificationToAdministrators)
        {
            if (!IsAdministratorRoleAlreadyIncluded(portalSettings, reviewers.Roles))
            {
                var adminRole = RoleController.Instance.GetRoleByName(portalSettings.PortalId, portalSettings.AdministratorRoleName);
				Logger.LogDebug("GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State}): adminRole → {AdminRole}", portalSettings, state, adminRole);
                reviewers.Roles.Add(adminRole);
            }

            reviewers.Users = IncludeSuperUsers(reviewers.Users);
        }

		Logger.LogDebug("<<< Exiting GetUserAndRolesForStateReviewers(portalSettings={PortalSettings},state={State})", portalSettings, state);
        return reviewers;
    }

    private void AddWorkflowCommentLog(ContentItem contentItem, int userId, string userComment)
    {
		Logger.LogDebug(">>> Entering AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment})", contentItem, userId, userComment);
        if (string.IsNullOrEmpty(userComment))
        {
            return;
        }

		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var state = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
        this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
		Logger.LogDebug("AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment}): state → {State}", contentItem, userId, userComment, state);
		Logger.LogInformation("AddWorkflowCommentLog(contentItem,userId,userComment)={ContentItem},{UserId},{UserComment}: find query executed in {Arg3} ms", contentItem, userId, userComment, __logStart.ElapsedMilliseconds);
		Logger.LogDebug("<<< Exiting AddWorkflowCommentLog(contentItem={ContentItem},userId={UserId},userComment={UserComment})", contentItem, userId, userComment);
    }

    private void AddWorkflowCommentLog(ContentItem contentItem, WorkflowState state, int userId, string userComment)
    {
		Logger.LogDebug(">>> Entering AddWorkflowCommentLog(contentItem={ContentItem},state={State},userId={UserId},userComment={UserComment})", contentItem, state, userId, userComment);
        if (string.IsNullOrEmpty(userComment))
        {
            return;
        }

        this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
    }

    private void AddWorkflowLog(ContentItem contentItem, WorkflowLogType logType, int userId, string userComment = null)
    {
		Logger.LogDebug(">>> Entering AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, logType, userId, userComment);
        var state = this.workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
        this.AddWorkflowLog(contentItem, state, logType, userId, userComment);
		Logger.LogDebug("<<< Exiting AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, logType, userId, userComment);
    }

    private void AddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment = null)
    {
		Logger.LogDebug(">>> Entering AddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, state, logType, userId, userComment);
		Logger.LogDebug("AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment}): state → {State}", contentItem, logType, userId, userComment, state);
        try
        {
			Logger.LogDebug("<<< Exiting AddWorkflowLog(contentItem={ContentItem},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, logType, userId, userComment);
            this.TryAddWorkflowLog(contentItem, state, logType, userId, userComment);
        }
        catch (Exception ex)
        {
			Logger.LogError("Exception in AddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): {Arg5}", contentItem, state, logType, userId, userComment, ex.getMessage());
            Services.Exceptions.Exceptions.LogException(ex);
        }
    }

    private void TryAddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment)
    {
		Logger.LogDebug(">>> Entering TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, state, logType, userId, userComment);
        var workflow = this.workflowManager.GetWorkflow(contentItem);
        var logTypeText = GetWorkflowActionComment(logType);
		Logger.LogDebug("TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): logTypeText → {LogTypeText}", contentItem, state, logType, userId, userComment, logTypeText);
        var logComment = this.ReplaceNotificationTokens(logTypeText, workflow, contentItem, state, userId, userComment);
		Logger.LogDebug("TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): logComment → {LogComment}", contentItem, state, logType, userId, userComment, logComment);
		Logger.LogDebug("TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment}): workflow → {Workflow}", contentItem, state, logType, userId, userComment, workflow);
        this.workflowLogger.AddWorkflowLog(contentItem.ContentItemId, workflow.WorkflowID, logType, logComment, userId);
		Logger.LogDebug("<<< Exiting TryAddWorkflowLog(contentItem={ContentItem},state={State},logType={LogType},userId={UserId},userComment={UserComment})", contentItem, state, logType, userId, userComment);
    }

    private string ReplaceNotificationTokens(string text, Workflow workflow, ContentItem item, WorkflowState state, int userId, string comment = "")
    {
		Logger.LogDebug(">>> Entering ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment})", text, workflow, item, state, userId, comment);
        var user = this.userController.GetUserById(workflow.PortalID, userId);
        var datetime = DateTime.UtcNow;
		Logger.LogDebug("ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment}): user → {User}", text, workflow, item, state, userId, comment, user);
        var result = text.Replace("[USER]", user != null ? user.DisplayName : string.Empty);
		Logger.LogDebug("ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment}): result → {Result}", text, workflow, item, state, userId, comment, result);
        result = result.Replace("[DATE]", datetime.ToString("F", CultureInfo.CurrentCulture));
        result = result.Replace("[STATE]", state != null ? state.StateName : string.Empty);
        result = result.Replace("[WORKFLOW]", workflow.WorkflowName);
        result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : string.Empty);
        result = result.Replace("[COMMENT]", !string.IsNullOrEmpty(comment) ? comment : string.Empty);
		Logger.LogDebug("<<< Exiting ReplaceNotificationTokens(text={Text},workflow={Workflow},item={Item},state={State},userId={UserId},comment={Comment})", text, workflow, item, state, userId, comment);
        return result;
    }

    private class ReviewersDto
    {
        public List<RoleInfo> Roles { get; set; }

        public List<UserInfo> Users { get; set; }
    }
}
