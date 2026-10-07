// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A <see cref="IWorkflowStateManager"/> implementation.</summary>
    /// <param name="dataContext">The data context.</param>
    /// <param name="dataProvider">The data provider.</param>
    public class WorkflowStateManager(IDataContext dataContext, DataProvider dataProvider)
        : ServiceLocator<IWorkflowStateManager, WorkflowStateManager>, IWorkflowStateManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowStateManager));
        private readonly IWorkflowRepository workflowRepository = WorkflowRepository.Instance;
        private readonly IWorkflowStateRepository workflowStateRepository = WorkflowStateRepository.Instance;
        private readonly IWorkflowStatePermissionsRepository workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
        private readonly IDataContext dataContext = dataContext ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataContext>();
        private readonly DataProvider dataProvider = dataProvider ?? DataProvider.Instance();

        /// <summary>Initializes a new instance of the <see cref="WorkflowStateManager"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IDataContext. Scheduled removal in v12.0.0.")]
        public WorkflowStateManager()
            : this(null, null)
        {
		Logger.LogError(">>> Entering WorkflowStateManager()");
        Logger.LogWarning("<<< Exiting WorkflowStateManager()");
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowStates(workflowId={WorkflowId})", workflowId);
            Logger.LogWarning("<<< Exiting GetWorkflowStates(workflowId={WorkflowId})", workflowId);
            return this.workflowStateRepository.GetWorkflowStates(workflowId);
        }

        /// <inheritdoc />
        public WorkflowState GetWorkflowState(int stateId)
        {
			Logger.LogError(">>> Entering GetWorkflowState(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetWorkflowState(stateId={StateId})", stateId);
            return this.workflowStateRepository.GetWorkflowStateByID(stateId);
        }

        /// <inheritdoc />
        public void AddWorkflowState(WorkflowState state)
        {
			Logger.LogError(">>> Entering AddWorkflowState(state={State})", state);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflow = this.workflowRepository.GetWorkflow(state.WorkflowID);
            if (workflow == null)
            {
				Logger.LogError("Throwing WorkflowDoesNotExistException from AddWorkflowState(state={State})", state);
				Logger.LogError("AddWorkflowState(state={State}): workflow → {Workflow}", state, workflow);
				Logger.LogError("AddWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            if (workflow.IsSystem)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from AddWorkflowState(state={State})", state);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowNewStateCannotBeAddedToSystemWorkflows", Localization.ExceptionsResourceFile));
            }

            var lastState = workflow.LastState;

            // New States always goes before the last state
            state.Order = lastState.Order;
			Logger.LogDebug("AddWorkflowState(state={State}): state.Order → {Arg1}", state, state.Order);

            lastState.Order++;
            this.workflowStateRepository.AddWorkflowState(state);
            this.workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
			Logger.LogError("AddWorkflowState(state)={State}: update query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void DeleteWorkflowState(WorkflowState state)
        {
			Logger.LogError(">>> Entering DeleteWorkflowState(state={State})", state);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var stateToDelete = this.workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (stateToDelete == null)
            {
				Logger.LogError("DeleteWorkflowState(state={State}): stateToDelete → {StateToDelete}", state, stateToDelete);
                return;
            }
			Logger.LogError("DeleteWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);

            if (stateToDelete.IsSystem)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from DeleteWorkflowState(state={State})", state);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowSystemWorkflowStateCannotBeDeleted", Localization.ExceptionsResourceFile));
            }

            if (this.dataProvider.GetContentWorkflowStateUsageCount(state.StateID) > 0)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateInUsageException", Localization.ExceptionsResourceFile));
            }

            this.workflowStateRepository.DeleteWorkflowState(stateToDelete);
			Logger.LogError("DeleteWorkflowState(state)={State}: delete query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);

            // Reorder states order
            var rep = this.dataContext.GetRepository<WorkflowState>();
			Logger.LogError("DeleteWorkflowState(state={State}): rep → {Rep}", state, rep);
            rep.Update("SET [Order] = [Order] - 1 WHERE WorkflowID = @0 AND [Order] > @1", stateToDelete.WorkflowID, stateToDelete.Order);
			Logger.LogWarning("<<< Exiting DeleteWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void UpdateWorkflowState(WorkflowState state)
        {
			Logger.LogError(">>> Entering UpdateWorkflowState(state={State})", state);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflowState = this.workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (workflowState == null)
            {
				Logger.LogError("Throwing WorkflowDoesNotExistException from UpdateWorkflowState(state={State})", state);
				Logger.LogError("UpdateWorkflowState(state={State}): workflowState → {WorkflowState}", state, workflowState);
				Logger.LogError("UpdateWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            this.workflowStateRepository.UpdateWorkflowState(state);
			Logger.LogError("UpdateWorkflowState(state)={State}: update query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void MoveWorkflowStateDown(int stateId)
        {
			Logger.LogError(">>> Entering MoveWorkflowStateDown(stateId={StateId})", stateId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
				Logger.LogError("Throwing WorkflowDoesNotExistException from MoveWorkflowStateDown(stateId={StateId})", stateId);
				Logger.LogError("MoveWorkflowStateDown(stateId={StateId}): state → {State}", stateId, state);
				Logger.LogError("MoveWorkflowStateDown(stateId)={StateId}: find query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
			Logger.LogError("MoveWorkflowStateDown(stateId={StateId}): states → {States}", stateId, states);

            if (states.Length == 3)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from MoveWorkflowStateDown(stateId={StateId})", stateId);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
			Logger.LogError("MoveWorkflowStateDown(stateId={StateId}): stateToMoveUp → {StateToMoveUp}", stateId, stateToMoveUp);
            WorkflowState stateToMoveDown = null;
			Logger.LogError("MoveWorkflowStateDown(stateId={StateId}): stateToMoveDown → {StateToMoveDown}", stateId, stateToMoveDown);

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId)
                {
                    continue;
                }

                // First and Second workflow state cannot be moved down
                if (i <= 1)
                {
                    throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
                }

                stateToMoveUp = states[i - 1];
                stateToMoveDown = states[i];
                break;
            }

            if (stateToMoveUp == null || stateToMoveDown == null)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var orderTmp = stateToMoveDown.Order;
            stateToMoveDown.Order = stateToMoveUp.Order;
			Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): stateToMoveDown.Order → {Arg1}", stateId, stateToMoveDown.Order);
            stateToMoveUp.Order = orderTmp;
			Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): stateToMoveUp.Order → {Arg1}", stateId, stateToMoveUp.Order);

            this.workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            this.workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
			Logger.LogError("MoveWorkflowStateDown(stateId)={StateId}: update query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting MoveWorkflowStateDown(stateId={StateId})", stateId);
        }

        /// <inheritdoc />
        public void MoveWorkflowStateUp(int stateId)
        {
			Logger.LogError(">>> Entering MoveWorkflowStateUp(stateId={StateId})", stateId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
				Logger.LogError("Throwing WorkflowDoesNotExistException from MoveWorkflowStateUp(stateId={StateId})", stateId);
				Logger.LogError("MoveWorkflowStateUp(stateId={StateId}): state → {State}", stateId, state);
				Logger.LogError("MoveWorkflowStateUp(stateId)={StateId}: find query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
			Logger.LogError("MoveWorkflowStateUp(stateId={StateId}): states → {States}", stateId, states);

            if (states.Length == 3)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from MoveWorkflowStateUp(stateId={StateId})", stateId);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
			Logger.LogError("MoveWorkflowStateUp(stateId={StateId}): stateToMoveUp → {StateToMoveUp}", stateId, stateToMoveUp);
            WorkflowState stateToMoveDown = null;
			Logger.LogError("MoveWorkflowStateUp(stateId={StateId}): stateToMoveDown → {StateToMoveDown}", stateId, stateToMoveDown);

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].StateID != stateId)
                {
                    continue;
                }

                // Last and Next to Last workflow state cannot be moved up
                if (i >= states.Length - 2)
                {
                    throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
                }

                stateToMoveUp = states[i];
                stateToMoveDown = states[i + 1];
                break;
            }

            if (stateToMoveUp == null || stateToMoveDown == null)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var orderTmp = stateToMoveDown.Order;
            stateToMoveDown.Order = stateToMoveUp.Order;
			Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): stateToMoveDown.Order → {Arg1}", stateId, stateToMoveDown.Order);
            stateToMoveUp.Order = orderTmp;
			Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): stateToMoveUp.Order → {Arg1}", stateId, stateToMoveUp.Order);

            this.workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            this.workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
			Logger.LogError("MoveWorkflowStateUp(stateId)={StateId}: update query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting MoveWorkflowStateUp(stateId={StateId})", stateId);
        }

        /// <inheritdoc />
        public void MoveState(int stateId, int index)
        {
			Logger.LogError(">>> Entering MoveState(stateId={StateId},index={Index})", stateId, index);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
				Logger.LogError("Throwing WorkflowDoesNotExistException from MoveState(stateId={StateId},index={Index})", stateId, index);
				Logger.LogError("MoveState(stateId={StateId},index={Index}): state → {State}", stateId, index, state);
				Logger.LogError("MoveState(stateId,index)={StateId},{Index}: find query executed in {Arg2} ms", stateId, index, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
			Logger.LogError("MoveState(stateId={StateId},index={Index}): states → {States}", stateId, index, states);
            if (index < 1 || index > states.Length - 2)
            {
				Logger.LogError("Throwing WorkflowInvalidOperationException from MoveState(stateId={StateId},index={Index})", stateId, index);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var currentIndex = GetStateIndex(states, state);
			Logger.LogError("MoveState(stateId={StateId},index={Index}): currentIndex → {CurrentIndex}", stateId, index, currentIndex);

            if (currentIndex == 0
                || currentIndex == states.Length - 1)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            this.MoveState(state, index, currentIndex);
			Logger.LogWarning("<<< Exiting MoveState(stateId={StateId},index={Index})", stateId, index);
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId)
        {
			Logger.LogError(">>> Entering GetWorkflowStatePermissionByState(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetWorkflowStatePermissionByState(stateId={StateId})", stateId);
            return this.workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(stateId);
        }

        /// <inheritdoc />
        public void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId)
        {
			Logger.LogError(">>> Entering AddWorkflowStatePermission(permission={Permission},userId={UserId})", permission, userId);
            permission.WorkflowStatePermissionID = this.workflowStatePermissionsRepository.AddWorkflowStatePermission(permission, userId);
			Logger.LogWarning("<<< Exiting AddWorkflowStatePermission(permission={Permission},userId={UserId})", permission, userId);
        }

        /// <inheritdoc />
        public void DeleteWorkflowStatePermission(int workflowStatePermissionId)
        {
			Logger.LogError(">>> Entering DeleteWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
            this.workflowStatePermissionsRepository.DeleteWorkflowStatePermission(workflowStatePermissionId);
			Logger.LogWarning("<<< Exiting DeleteWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
        }

        /// <inheritdoc />
        public int GetContentWorkflowStateUsageCount(int stateId)
        {
			Logger.LogError(">>> Entering GetContentWorkflowStateUsageCount(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetContentWorkflowStateUsageCount(stateId={StateId})", stateId);
            return this.dataProvider.GetContentWorkflowStateUsageCount(stateId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowStateManager> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IWorkflowStateManager>;
        }

        private static int GetStateIndex(WorkflowState[] states, WorkflowState currentState)
        {
			Logger.LogError(">>> Entering GetStateIndex(states={States},currentState={CurrentState})", states, currentState);
            var i = 0;
            foreach (var state in states)
            {
                if (state.StateID == currentState.StateID)
                {
                    return i;
                }

                i++;
            }

			Logger.LogWarning("<<< Exiting GetStateIndex(states={States},currentState={CurrentState})", states, currentState);
            return i;
        }

        private void MoveState(WorkflowState state, int targetIndex, int currentIndex)
        {
			Logger.LogError(">>> Entering MoveState(state={State},targetIndex={TargetIndex},currentIndex={CurrentIndex})", state, targetIndex, currentIndex);
            if (currentIndex == targetIndex)
            {
                return;
            }

            var moveUp = currentIndex < targetIndex;
            var numberOfMovements = moveUp ? targetIndex - currentIndex : currentIndex - targetIndex;

            for (var i = 0; i < numberOfMovements; i++)
            {
                if (moveUp)
                {
                    this.MoveWorkflowStateUp(state.StateID);
                }
                else
                {
                    this.MoveWorkflowStateDown(state.StateID);
                }
            }
			Logger.LogWarning("<<< Exiting MoveState(state={State},targetIndex={TargetIndex},currentIndex={CurrentIndex})", state, targetIndex, currentIndex);
        }
    }
}
