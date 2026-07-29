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
		Logger.LogDebug(">>> Entering WorkflowRepository(hostSettings={HostSettings})", hostSettings);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IWorkflowStateRepository stateRepository = WorkflowStateRepository.Instance;
		Logger.LogDebug("WorkflowRepository(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
		Logger.LogDebug("WorkflowRepository(hostSettings={HostSettings}): stateRepository → {StateRepository}", hostSettings, stateRepository);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();

        /// <summary>Initializes a new instance of the <see cref="WorkflowRepository"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public WorkflowRepository()
            : this(null)
        {
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetWorkflows(int portalId)
        {
			Logger.LogDebug(">>> Entering GetWorkflows(portalId={PortalId})", portalId);
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<Entities.Workflow>();
			Logger.LogDebug("GetWorkflows(portalId={PortalId}): context → {Context}", portalId, context);
			Logger.LogDebug("GetWorkflows(portalId={PortalId}): rep → {Rep}", portalId, rep);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL)", portalId).ToArray();

			Logger.LogDebug("GetWorkflows(portalId={PortalId}): workflows → {Workflows}", portalId, workflows);
            // Workflow States eager loading
            foreach (var workflow in workflows)
            {
                workflow.States = this.stateRepository.GetWorkflowStates(workflow.WorkflowID);
				Logger.LogInformation("WorkflowRepository(hostSettings)={HostSettings}: find query executed in {Arg1} ms", hostSettings, __logStart.ElapsedMilliseconds);
            }
			Logger.LogInformation("GetWorkflows(portalId)={PortalId}: find query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);

			Logger.LogDebug("<<< Exiting GetWorkflows(portalId={PortalId})", portalId);
            return workflows;
        }

        /// <inheritdoc />
        public IEnumerable<Entities.Workflow> GetSystemWorkflows(int portalId)
        {
			Logger.LogDebug(">>> Entering GetSystemWorkflows(portalId={PortalId})", portalId);
            using var context = DataContext.Instance(this.hostSettings);
            var rep = context.GetRepository<Entities.Workflow>();
			Logger.LogDebug("GetSystemWorkflows(portalId={PortalId}): context → {Context}", portalId, context);
			Logger.LogDebug("GetSystemWorkflows(portalId={PortalId}): rep → {Rep}", portalId, rep);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var workflows = rep.Find("WHERE (PortalId = @0 OR PortalId IS NULL) AND IsSystem = 1", portalId).ToArray();

			Logger.LogDebug("GetSystemWorkflows(portalId={PortalId}): workflows → {Workflows}", portalId, workflows);
            // Workflow States eager loading
            foreach (var workflow in workflows)
            {
                workflow.States = this.stateRepository.GetWorkflowStates(workflow.WorkflowID);
				Logger.LogInformation("GetSystemWorkflows(portalId)={PortalId}: find query executed in {Arg1} ms", portalId, __logStart.ElapsedMilliseconds);
            }

			Logger.LogDebug("<<< Exiting GetSystemWorkflows(portalId={PortalId})", portalId);
            return workflows;
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(int workflowId)
        {
			Logger.LogDebug(">>> Entering GetWorkflow(workflowId={WorkflowId})", workflowId);
            return CBO.GetCachedObject<Entities.Workflow>(
                this.hostSettings,
                new CacheItemArgs(GetWorkflowItemKey(workflowId), DataCache.WorkflowsCacheTimeout, DataCache.WorkflowsCachePriority),
                _ =>
                {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
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

                    workflow.States = this.stateRepository.GetWorkflowStates(workflowId);
					Logger.LogInformation("GetWorkflow(workflowId)={WorkflowId}: find query executed in {Arg1} ms", workflowId, __logStart.ElapsedMilliseconds);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                    return workflow;
                });
        }

        /// <inheritdoc />
        public Entities.Workflow GetWorkflow(ContentItem item)
        {
			Logger.LogDebug(">>> Entering GetWorkflow(item={Item})", item);
            var state = this.stateRepository.GetWorkflowStateByID(item.StateID);
			Logger.LogDebug("GetWorkflow(item={Item}): state → {State}", item, state);
            return state == null ? null : this.GetWorkflow(state.WorkflowID);
        }

        // TODO: validation

        /// <inheritdoc />
        public void AddWorkflow(Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering AddWorkflow(workflow={Workflow})", workflow);
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();

				Logger.LogDebug("AddWorkflow(workflow={Workflow}): rep → {Rep}", workflow, rep);
                if (DoesExistWorkflow(workflow, rep))
                {
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Insert(workflow);
            }

            CacheWorkflow(this.hostSettings, workflow);
			Logger.LogDebug("<<< Exiting AddWorkflow(workflow={Workflow})", workflow);
        }

        // TODO: validation

        /// <inheritdoc />
        public void UpdateWorkflow(Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering UpdateWorkflow(workflow={Workflow})", workflow);
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();

				Logger.LogDebug("UpdateWorkflow(workflow={Workflow}): rep → {Rep}", workflow, rep);
                if (DoesExistWorkflow(workflow, rep))
                {
                    throw new WorkflowNameAlreadyExistsException();
                }

                rep.Update(workflow);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
            CacheWorkflow(this.hostSettings, workflow);
			Logger.LogDebug("<<< Exiting UpdateWorkflow(workflow={Workflow})", workflow);
        }

        /// <inheritdoc />
        public void DeleteWorkflow(Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering DeleteWorkflow(workflow={Workflow})", workflow);
            using (var context = DataContext.Instance(this.hostSettings))
            {
                var rep = context.GetRepository<Entities.Workflow>();
                rep.Delete(workflow);
				Logger.LogDebug("DeleteWorkflow(workflow={Workflow}): rep → {Rep}", workflow, rep);
            }

            DataCache.RemoveCache(GetWorkflowItemKey(workflow.WorkflowID));
			Logger.LogDebug("<<< Exiting DeleteWorkflow(workflow={Workflow})", workflow);
        }

        internal static string GetWorkflowItemKey(int workflowId)
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
			Logger.LogDebug("<<< Exiting GetWorkflowItemKey(workflowId={WorkflowId})", workflowId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.ContentWorkflowCacheKey, workflowId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowRepository> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => ActivatorUtilities.GetServiceOrCreateInstance<WorkflowRepository>(Globals.DependencyProvider);
        }

        private static bool DoesExistWorkflow(Entities.Workflow workflow, IRepository<Entities.Workflow> rep)
        {
            return rep.Find(
                           "WHERE IsDeleted = 0 AND (PortalId = @0 OR PortalId IS NULL) AND WorkflowName = @1 AND WorkflowID != @2",
                           workflow.PortalID,
                           workflow.WorkflowName,
                           workflow.WorkflowID)
                       .SingleOrDefault() != null;
        }

        private static void CacheWorkflow(IHostSettings hostSettings, Entities.Workflow workflow)
        {
			Logger.LogDebug(">>> Entering CacheWorkflow(hostSettings={HostSettings},workflow={Workflow})", hostSettings, workflow);
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
			Logger.LogDebug("<<< Exiting CacheWorkflow(hostSettings={HostSettings},workflow={Workflow})", hostSettings, workflow);
        }
		Logger.LogDebug("<<< Exiting WorkflowRepository(hostSettings={HostSettings})", hostSettings);
    }
}
