// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="IWorkflowStateRepository"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    internal class WorkflowStateRepository(IHostSettings hostSettings)
        : ServiceLocator<IWorkflowStateRepository, WorkflowStateRepository>, IWorkflowStateRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowStateRepository));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="WorkflowStateRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public WorkflowStateRepository()
            : this(null)
        {
		Logger.LogError(">>> Entering WorkflowStateRepository()");
        Logger.LogWarning("<<< Exiting WorkflowStateRepository()");
        }

        /// <inheritdoc />
        public IEnumerable<WorkflowState> GetWorkflowStates(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowStates(workflowId={WorkflowId})", workflowId);
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<WorkflowState>();
			Logger.LogError("GetWorkflowStates(workflowId={WorkflowId}): rep → {Rep}", workflowId, rep);
            return rep.Find("WHERE WorkflowID = @0 ORDER BY [Order] ASC", workflowId);
        }

        /// <inheritdoc />
        public WorkflowState GetWorkflowStateByID(int stateId)
        {
			Logger.LogError(">>> Entering GetWorkflowStateByID(stateId={StateId})", stateId);
            return CBO.GetCachedObject<WorkflowState>(
                this.hostSettings,
                new CacheItemArgs(GetWorkflowStateKey(stateId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
                    using var context = DataContext.Instance(this.hostSettings);
                    var rep = context.GetRepository<WorkflowState>();
				Logger.LogWarning("<<< Exiting GetWorkflowStateByID(stateId={StateId})", stateId);
                    return rep.GetById(stateId);
                });
        }

        /// <inheritdoc />
        public void AddWorkflowState(WorkflowState state)
        {
			Logger.LogError(">>> Entering AddWorkflowState(state={State})", state);
            Requires.NotNull("state", state);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<WorkflowState>();
				Logger.LogError("AddWorkflowState(state={State}): rep → {Rep}", state, rep);
                if (DoesExistWorkflowState(state, rep))
                {
					Logger.LogError("Throwing WorkflowStateNameAlreadyExistsException from AddWorkflowState(state={State})", state);
                    throw new WorkflowStateNameAlreadyExistsException();
                }

                rep.Insert(state);
            }

            CacheWorkflowState(this.hostSettings, state);
			Logger.LogWarning("<<< Exiting AddWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void UpdateWorkflowState(WorkflowState state)
        {
			Logger.LogError(">>> Entering UpdateWorkflowState(state={State})", state);
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);
            Requires.PropertyNotNullOrEmpty("state", "StateName", state.StateName);

            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<WorkflowState>();
				Logger.LogError("UpdateWorkflowState(state={State}): rep → {Rep}", state, rep);
                if (DoesExistWorkflowState(state, rep))
                {
					Logger.LogError("Throwing WorkflowStateNameAlreadyExistsException from UpdateWorkflowState(state={State})", state);
                    throw new WorkflowStateNameAlreadyExistsException();
                }

                rep.Update(state);
            }

            DataCache.RemoveCache(GetWorkflowStateKey(state.StateID));
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            DataCache.RemoveCache(WorkflowRepository.GetWorkflowItemKey(state.WorkflowID));
            CacheWorkflowState(this.hostSettings, state);
			Logger.LogError("UpdateWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateWorkflowState(state={State})", state);
        }

        /// <inheritdoc />
        public void DeleteWorkflowState(WorkflowState state)
        {
			Logger.LogError(">>> Entering DeleteWorkflowState(state={State})", state);
            Requires.NotNull("state", state);
            Requires.PropertyNotNegative("state", "StateID", state.StateID);

            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<WorkflowState>();
				Logger.LogError("DeleteWorkflowState(state={State}): rep → {Rep}", state, rep);
                rep.Delete(state);
            }

            DataCache.RemoveCache(GetWorkflowStateKey(state.StateID));
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("DeleteWorkflowState(state)={State}: find query executed in {Arg1} ms", state, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting DeleteWorkflowState(state={State})", state);
            DataCache.RemoveCache(WorkflowRepository.GetWorkflowItemKey(state.WorkflowID));
        }

        /// <inheritdoc />
        protected override Func<IWorkflowStateRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => ActivatorUtilities.GetServiceOrCreateInstance<WorkflowStateRepository>(Globals.DependencyProvider);
        }

        private static bool DoesExistWorkflowState(WorkflowState state, IRepository<WorkflowState> rep)
        {
			Logger.LogError(">>> Entering DoesExistWorkflowState(state={State},rep={Rep})", state, rep);
            Logger.LogWarning("<<< Exiting DoesExistWorkflowState(state={State},rep={Rep})", state, rep);
            return rep.Find(
                           "WHERE StateName = @0 AND WorkflowID = @1 AND StateId != @2",
                           state.StateName,
                           state.WorkflowID,
                           state.StateID)
                       .SingleOrDefault() != null;
        }

        private static string GetWorkflowStateKey(int stateId)
        {
			Logger.LogError(">>> Entering GetWorkflowStateKey(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetWorkflowStateKey(stateId={StateId})", stateId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.ContentWorkflowStateCacheKey, stateId);
        }

        private static void CacheWorkflowState(IHostSettings hostSettings, WorkflowState state)
        {
			Logger.LogError(">>> Entering CacheWorkflowState(hostSettings={HostSettings},state={State})", hostSettings, state);
            if (state.StateID > 0)
            {
                CBO.GetCachedObject<WorkflowState>(
                    hostSettings,
                    new CacheItemArgs(GetWorkflowStateKey(state.StateID), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                    _ => state);
            }
			Logger.LogWarning("<<< Exiting CacheWorkflowState(hostSettings={HostSettings},state={State})", hostSettings, state);
        }
    }
}
