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
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    // TODO: add interface metadata documentation
    // TODO: removed unused SPRoc and DataProvider layer
    internal class WorkflowRepository(IHostSettings hostSettings) : ServiceLocator<IWorkflowRepository, WorkflowRepository>, IWorkflowRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowRepository));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IWorkflowStateRepository stateRepository = WorkflowStateRepository.Instance;

        /// <summary>Initializes a new instance of the <see cref="WorkflowRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public WorkflowRepository()
            : this(null)
        {
		Logger.LogError(">>> Entering WorkflowRepository()");
        Logger.LogWarning("<<< Exiting WorkflowRepository()");
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
			Logger.LogError(">>> Entering GetWorkflows(portalId={PortalId})", portalId);
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<Entities.Workflow>();
			Logger.LogError("GetWorkflows(portalId={PortalId}): rep → {Rep}", portalId, rep);
            var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL)", portalId).ToArray();
			Logger.LogError("GetWorkflows(portalId={PortalId}): workflows → {Workflows}", portalId, workflows);
			Logger.LogError("GetWorkflows(portalId={PortalId}): context → {Context}", portalId, context);

            // Workflow States eager loading
            foreach (var workflow in workflows)
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("GetWorkflows(portalId)={PortalId}: find query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
                workflow.States = this.stateRepository.GetWorkflowStates(workflow.WorkflowID);
            }

			Logger.LogWarning("<<< Exiting GetWorkflows(portalId={PortalId})", portalId);
            return workflows;
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetSystemWorkflows(int portalId)
        {
			Logger.LogError(">>> Entering GetSystemWorkflows(portalId={PortalId})", portalId);
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<Entities.Workflow>();
			Logger.LogError("GetSystemWorkflows(portalId={PortalId}): rep → {Rep}", portalId, rep);
            var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL) AND IsSystem = 1", portalId).ToArray();
			Logger.LogError("GetSystemWorkflows(portalId={PortalId}): workflows → {Workflows}", portalId, workflows);
			Logger.LogError("GetSystemWorkflows(portalId={PortalId}): context → {Context}", portalId, context);

            // Workflow States eager loading
            foreach (var workflow in workflows)
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("GetSystemWorkflows(portalId)={PortalId}: find query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
                workflow.States = this.stateRepository.GetWorkflowStates(workflow.WorkflowID);
            }

			Logger.LogWarning("<<< Exiting GetSystemWorkflows(portalId={PortalId})", portalId);
            return workflows;
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflow(workflowId={WorkflowId})", workflowId);
            return CBO.GetCachedObject<Entities.Workflow>(
                this.hostSettings,
                new CacheItemArgs(GetWorkflowItemKey(workflowId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
                    Entities.Workflow workflow;
                    using (var context = DataContext.Instance(this.hostSettings))
                    {
                        var rep = context.GetRepository<Entities.Workflow>();
                        workflow = rep.Find("WHERE WorkflowID = @0", workflowId).SingleOrDefault();
						Logger.LogDebug("GetWorkflow(workflowId={WorkflowId}): workflow → {Workflow}", workflowId, workflow);
                    }

                    if (workflow == null)
                    {
                        return null;
                    }

					var __logStart = System.Diagnostics.Stopwatch.StartNew();
					Logger.LogError("GetWorkflow(workflowId)={WorkflowId}: find query executed in {Arg1} ms", workflowId, __logStart.ElapsedMilliseconds);
                    workflow.States = this.stateRepository.GetWorkflowStates(workflowId);
                    return workflow;
                });
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(ContentItem item)
        {
			Logger.LogError(">>> Entering GetWorkflow(item={Item})", item);
            var state = this.stateRepository.GetWorkflowStateByID(item.StateID);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        // TODO: validation

        /// <inheritdoc />
        public void AddWorkflow(Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering AddWorkflow(workflow={Workflow})", workflow);
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();
				Logger.LogError("AddWorkflow(workflow={Workflow}): rep → {Rep}", workflow, rep);

                if (DoesExistWorkflow(workflow, rep))
                {
					Logger.LogError("Throwing WorkflowNameAlreadyExistsException from AddWorkflow(workflow={Workflow})", workflow);
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Insert(workflow);
            }

            CacheWorkflow(this.hostSettings, workflow);
			Logger.LogWarning("<<< Exiting AddWorkflow(workflow={Workflow})", workflow);
        }

        // TODO: validation

        /// <inheritdoc />
        public void UpdateWorkflow(Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering UpdateWorkflow(workflow={Workflow})", workflow);
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();
				Logger.LogError("UpdateWorkflow(workflow={Workflow}): rep → {Rep}", workflow, rep);

                if (DoesExistWorkflow(workflow, rep))
                {
					Logger.LogError("Throwing WorkflowNameAlreadyExistsException from UpdateWorkflow(workflow={Workflow})", workflow);
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Update(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
            CacheWorkflow(this.hostSettings, workflow);
			Logger.LogWarning("<<< Exiting UpdateWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public void DeleteWorkflow(Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering DeleteWorkflow(workflow={Workflow})", workflow);
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();
				Logger.LogError("DeleteWorkflow(workflow={Workflow}): rep → {Rep}", workflow, rep);
                rep.Delete(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
			Logger.LogWarning("<<< Exiting DeleteWorkflow(workflow={Workflow})", workflow);
        }

        internal static string GetWorkflowItemKey(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowItemKey(workflowId={WorkflowId})", workflowId);
            Logger.LogWarning("<<< Exiting GetWorkflowItemKey(workflowId={WorkflowId})", workflowId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.ContentWorkflowCacheKey, workflowId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => ActivatorUtilities.GetServiceOrCreateInstance<WorkflowRepository>(Globals.DependencyProvider);
        }

        private static bool DoesExistWorkflow(Entities.Workflow workflow, IRepository<Entities.Workflow> rep)
        {
			Logger.LogError(">>> Entering DoesExistWorkflow(workflow={Workflow},rep={Rep})", workflow, rep);
            Logger.LogWarning("<<< Exiting DoesExistWorkflow(workflow={Workflow},rep={Rep})", workflow, rep);
            return rep.Find(
                           "WHERE IsDeleted = 0 AND (PortalId = @0 OR PortalId IS NULL) AND WorkflowName = @1 AND WorkflowID != @2",
                           workflow.PortalID,
                           workflow.WorkflowName,
                           workflow.WorkflowID)
                       .SingleOrDefault() != null;
        }

        private static void CacheWorkflow(IHostSettings hostSettings, Entities.Workflow workflow)
        {
			Logger.LogError(">>> Entering CacheWorkflow(hostSettings={HostSettings},workflow={Workflow})", hostSettings, workflow);
            if (workflow.WorkflowID > 0)
            {
                CBO.GetCachedObject<Entities.Workflow>(
                    hostSettings,
                    new CacheItemArgs(
                        GetWorkflowItemKey(workflow.WorkflowID),
                        DataCache.WorkflowsCacheTimeout,
                        DataCache.WorkflowsCachePriority),
                    _ => workflow);
            }
			Logger.LogWarning("<<< Exiting CacheWorkflow(hostSettings={HostSettings},workflow={Workflow})", hostSettings, workflow);
        }
    }
}
