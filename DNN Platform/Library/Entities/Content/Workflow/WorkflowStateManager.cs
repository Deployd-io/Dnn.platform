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
		Logger.LogDebug(">>> Entering WorkflowStateManager()");
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
			Logger.LogDebug(">>> Entering GetWorkflowStates(workflowId={WorkflowId})", workflowId);
			Logger.LogDebug("<<< Exiting GetWorkflowStates(workflowId={WorkflowId})", workflowId);
            return this.workflowStateRepository.GetWorkflowStates(workflowId);
        }

        /// <inheritdoc />
        public WorkflowState GetWorkflowState(int stateId)
        {
			Logger.LogDebug(">>> Entering GetWorkflowState(stateId={StateId})", stateId);
			Logger.LogDebug("<<< Exiting GetWorkflowState(stateId={StateId})", stateId);
            return this.workflowStateRepository.GetWorkflowStateByID(stateId);
        }

        /// <inheritdoc />
        public void AddWorkflowState(WorkflowState state)
        {
			Logger.LogDebug(">>> Entering AddWorkflowState(state={State})", state);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflow = this.workflowRepository.GetWorkflow(state.WorkflowID);
            if (workflow == null)
            {
				Logger.LogWarning("Throwing WorkflowDoesNotExistException from AddWorkflowState(state={State})", state);
				Logger.LogDebug("AddWorkflowState(state={State}): workflow → {Workflow}", state, workflow);
				Logger.LogInformation("AddWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            if (workflow.IsSystem)
            {
				Logger.LogWarning("Throwing WorkflowInvalidOperationException from AddWorkflowState(state={State})", state);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowNewStateCannotBeAddedToSystemWorkflows", Localization.ExceptionsResourceFile));
            }

            var lastState = workflow.LastState;

            // New States always goes before the last state
            state.Order = lastState.Order;
			Logger.LogDebug("AddWorkflowState(state={State}): state.Order → {Arg1}", state, state.Order);

            lastState.Order++;
            this.workflowStateRepository.AddWorkflowState(state);
            this.workflowStateRepository.UpdateWorkflowState(lastState); // Update last state order
			Logger.LogInformation("AddWorkflowState(state)={State}: update query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting AddWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void DeleteWorkflowState(WorkflowState state)
        {
			Logger.LogDebug(">>> Entering DeleteWorkflowState(state={State})", state);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var stateToDelete = this.workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (stateToDelete == null)
            {
				Logger.LogDebug("DeleteWorkflowState(state={State}): stateToDelete → {StateToDelete}", state, stateToDelete);
				Logger.LogInformation("DeleteWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
                return;
            }

            if (stateToDelete.IsSystem)
            {
				Logger.LogWarning("Throwing WorkflowInvalidOperationException from DeleteWorkflowState(state={State})", state);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowSystemWorkflowStateCannotBeDeleted", Localization.ExceptionsResourceFile));
            }

            if (this.dataProvider.GetContentWorkflowStateUsageCount(state.StateID) > 0)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateInUsageException", Localization.ExceptionsResourceFile));
            }

			Logger.LogInformation("DeleteWorkflowState(state)={State}: delete query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
            this.workflowStateRepository.DeleteWorkflowState(stateToDelete);

            // Reorder states order
            var rep = this.dataContext.GetRepository<WorkflowState>();
            rep.Update("SET [Order] = [Order] - 1 WHERE WorkflowID = @0 AND [Order] > @1", stateToDelete.WorkflowID, stateToDelete.Order);
			Logger.LogDebug("DeleteWorkflowState(state={State}): rep → {Rep}", state, rep);
			Logger.LogDebug("<<< Exiting DeleteWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void UpdateWorkflowState(WorkflowState state)
        {
			Logger.LogDebug(">>> Entering UpdateWorkflowState(state={State})", state);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflowState = this.workflowStateRepository.GetWorkflowStateByID(state.StateID);
            if (workflowState == null)
            {
				Logger.LogWarning("Throwing WorkflowDoesNotExistException from UpdateWorkflowState(state={State})", state);
				Logger.LogDebug("UpdateWorkflowState(state={State}): workflowState → {WorkflowState}", state, workflowState);
				Logger.LogInformation("UpdateWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            this.workflowStateRepository.UpdateWorkflowState(state);
			Logger.LogInformation("UpdateWorkflowState(state)={State}: update query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting UpdateWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void MoveWorkflowStateDown(int stateId)
        {
			Logger.LogDebug(">>> Entering MoveWorkflowStateDown(stateId={StateId})", stateId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
				Logger.LogWarning("Throwing WorkflowDoesNotExistException from MoveWorkflowStateDown(stateId={StateId})", stateId);
				Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): state → {State}", stateId, state);
				Logger.LogInformation("MoveWorkflowStateDown(stateId)={StateId}: find query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
			Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): states → {States}", stateId, states);

            if (states.Length == 3)
            {
				Logger.LogWarning("Throwing WorkflowInvalidOperationException from MoveWorkflowStateDown(stateId={StateId})", stateId);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
			Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): stateToMoveUp → {StateToMoveUp}", stateId, stateToMoveUp);
            WorkflowState stateToMoveDown = null;
			Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): stateToMoveDown → {StateToMoveDown}", stateId, stateToMoveDown);

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
			Logger.LogDebug("MoveWorkflowStateDown(stateId={StateId}): stateToMoveUp.Order → {Arg1}", stateId, stateToMoveUp.Order);
            stateToMoveUp.Order = orderTmp;

            this.workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            this.workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
			Logger.LogInformation("MoveWorkflowStateDown(stateId)={StateId}: update query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting MoveWorkflowStateDown(stateId={StateId})", stateId);
        }

        /// <inheritdoc />
        public void MoveWorkflowStateUp(int stateId)
        {
			Logger.LogDebug(">>> Entering MoveWorkflowStateUp(stateId={StateId})", stateId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
				Logger.LogWarning("Throwing WorkflowDoesNotExistException from MoveWorkflowStateUp(stateId={StateId})", stateId);
				Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): state → {State}", stateId, state);
				Logger.LogInformation("MoveWorkflowStateUp(stateId)={StateId}: find query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
			Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): states → {States}", stateId, states);

            if (states.Length == 3)
            {
				Logger.LogWarning("Throwing WorkflowInvalidOperationException from MoveWorkflowStateUp(stateId={StateId})", stateId);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            WorkflowState stateToMoveUp = null;
			Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): stateToMoveUp → {StateToMoveUp}", stateId, stateToMoveUp);
            WorkflowState stateToMoveDown = null;
			Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): stateToMoveDown → {StateToMoveDown}", stateId, stateToMoveDown);

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
			Logger.LogDebug("MoveWorkflowStateUp(stateId={StateId}): stateToMoveUp.Order → {Arg1}", stateId, stateToMoveUp.Order);
            stateToMoveUp.Order = orderTmp;

            this.workflowStateRepository.UpdateWorkflowState(stateToMoveUp);
            this.workflowStateRepository.UpdateWorkflowState(stateToMoveDown);
			Logger.LogInformation("MoveWorkflowStateUp(stateId)={StateId}: update query executed in {Arg1} ms", stateId, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting MoveWorkflowStateUp(stateId={StateId})", stateId);
        }

        /// <inheritdoc />
        public void MoveState(int stateId, int index)
        {
			Logger.LogDebug(">>> Entering MoveState(stateId={StateId},index={Index})", stateId, index);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var state = this.workflowStateRepository.GetWorkflowStateByID(stateId);
            if (state == null)
            {
				Logger.LogWarning("Throwing WorkflowDoesNotExistException from MoveState(stateId={StateId},index={Index})", stateId, index);
				Logger.LogDebug("MoveState(stateId={StateId},index={Index}): state → {State}", stateId, index, state);
				Logger.LogInformation("MoveState(stateId,index)={StateId},{Index}: find query executed in {Arg2} ms", stateId, index, __logStart.ElapsedMilliseconds);
                throw new WorkflowDoesNotExistException();
            }

            var states = this.workflowStateRepository.GetWorkflowStates(state.WorkflowID).ToArray();
			Logger.LogDebug("MoveState(stateId={StateId},index={Index}): states → {States}", stateId, index, states);
            if (index < 1 || index > states.Length - 2)
            {
				Logger.LogWarning("Throwing WorkflowInvalidOperationException from MoveState(stateId={StateId},index={Index})", stateId, index);
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            var currentIndex = GetStateIndex(states, state);
			Logger.LogDebug("MoveState(stateId={StateId},index={Index}): currentIndex → {CurrentIndex}", stateId, index, currentIndex);

            if (currentIndex == 0
                || currentIndex == states.Length - 1)
            {
                throw new WorkflowInvalidOperationException(Localization.GetString("WorkflowStateCannotBeMoved", Localization.ExceptionsResourceFile));
            }

            this.MoveState(state, index, currentIndex);
			Logger.LogDebug("<<< Exiting MoveState(stateId={StateId},index={Index})", stateId, index);
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId)
        {
			Logger.LogDebug(">>> Entering GetWorkflowStatePermissionByState(stateId={StateId})", stateId);
			Logger.LogDebug("<<< Exiting GetWorkflowStatePermissionByState(stateId={StateId})", stateId);
            return this.workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(stateId);
        }

        /// <inheritdoc />
        public void AddWorkflowStatePermission(WorkflowStatePermission permission, int userId)
        {
			Logger.LogDebug(">>> Entering AddWorkflowStatePermission(permission={Permission},userId={UserId})", permission, userId);
            permission.WorkflowStatePermissionID = this.workflowStatePermissionsRepository.AddWorkflowStatePermission(permission, userId);
			Logger.LogDebug("<<< Exiting AddWorkflowStatePermission(permission={Permission},userId={UserId})", permission, userId);
        }

        /// <inheritdoc />
        public void DeleteWorkflowStatePermission(int workflowStatePermissionId)
        {
			Logger.LogDebug(">>> Entering DeleteWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
            this.workflowStatePermissionsRepository.DeleteWorkflowStatePermission(workflowStatePermissionId);
			Logger.LogDebug("<<< Exiting DeleteWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
        }

        /// <inheritdoc />
        public int GetContentWorkflowStateUsageCount(int stateId)
        {
			Logger.LogDebug(">>> Entering GetContentWorkflowStateUsageCount(stateId={StateId})", stateId);
			Logger.LogDebug("<<< Exiting GetContentWorkflowStateUsageCount(stateId={StateId})", stateId);
            return this.dataProvider.GetContentWorkflowStateUsageCount(stateId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowStateManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IWorkflowStateManager>;
        }

        private static int GetStateIndex(WorkflowState[] states, WorkflowState currentState)
        {
			Logger.LogDebug(">>> Entering GetStateIndex(states={States},currentState={CurrentState})", states, currentState);
            var i = 0;
            foreach (var state in states)
            {
                if (state.StateID == currentState.StateID)
                {
                    return i;
                }

                i++;
            }

			Logger.LogDebug("<<< Exiting GetStateIndex(states={States},currentState={CurrentState})", states, currentState);
            return i;
        }

        private void MoveState(WorkflowState state, int targetIndex, int currentIndex)
        {
			Logger.LogDebug(">>> Entering MoveState(state={State},targetIndex={TargetIndex},currentIndex={CurrentIndex})", state, targetIndex, currentIndex);
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
			Logger.LogDebug("<<< Exiting MoveState(state={State},targetIndex={TargetIndex},currentIndex={CurrentIndex})", state, targetIndex, currentIndex);
        }
    }
}
