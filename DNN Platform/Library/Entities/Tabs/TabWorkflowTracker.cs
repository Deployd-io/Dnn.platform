// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;

    internal class TabWorkflowTracker : ServiceLocator<ITabChangeTracker, TabWorkflowTracker>, ITabChangeTracker
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabWorkflowTracker));
        private readonly ITabController tabController;
        private readonly IWorkflowEngine workflowEngine;
        private readonly IWorkflowManager workflowManager;
        private readonly ITabWorkflowSettings tabWorkflowSettings;

        /// <summary>Initializes a new instance of the <see cref="TabWorkflowTracker"/> class.</summary>
        public TabWorkflowTracker()
        {
			Logger.LogError(">>> Entering TabWorkflowTracker()");
            this.tabController = TabController.Instance;
			Logger.LogDebug("TabWorkflowTracker(): this.tabController → {Arg0}", this.tabController);
            this.workflowEngine = WorkflowEngine.Instance;
            this.workflowManager = WorkflowManager.Instance;
			Logger.LogDebug("TabWorkflowTracker(): workflowEngine → {WorkflowEngine}", workflowEngine);
			Logger.LogDebug("TabWorkflowTracker(): this.workflowEngine → {Arg0}", this.workflowEngine);
        Logger.LogWarning("<<< Exiting TabWorkflowTracker()");
            this.tabWorkflowSettings = TabWorkflowSettings.Instance;
        }

        /// <summary>Tracks a workflow instance when a module is added to a page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
			Logger.LogWarning("<<< Exiting TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a workflow instance when a module is modified on a page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
			Logger.LogWarning("<<< Exiting TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a workflow instance when a module is deleted from a page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
			Logger.LogWarning("<<< Exiting TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a workflow instance when a module is copied from an exisitng page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            this.TrackModuleAddition(module, moduleVersion, userId);
			Logger.LogWarning("<<< Exiting TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <summary>Tracks a workflow instance when a copied module is deleted from an exisitng page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            this.TrackModuleDeletion(module, moduleVersion, userId);
			Logger.LogWarning("<<< Exiting TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <inheritdoc />
        protected override Func<ITabChangeTracker> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new TabWorkflowTracker();
        }

        private void NotifyWorkflowAboutChanges(int portalId, int tabId, int userId)
        {
			Logger.LogError(">>> Entering NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId})", portalId, tabId, userId);
            try
            {
                var tabInfo = this.tabController.GetTab(tabId, portalId);
				Logger.LogError("NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId}): tabInfo → {TabInfo}", portalId, tabId, userId, tabInfo);
                if (tabInfo != null && !tabInfo.IsDeleted && this.workflowEngine.IsWorkflowCompleted(tabInfo))
                {
                    var workflow = this.GetCurrentOrDefaultWorkflow(tabInfo, portalId);
					Logger.LogError("NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId}): workflow → {Workflow}", portalId, tabId, userId, workflow);
                    if (workflow == null)
                    {
                        Logger.Warn("Current Workflow and Default workflow are not found on NotifyWorkflowAboutChanges");
                        return;
                    }

                    this.workflowEngine.StartWorkflow(workflow.WorkflowID, tabInfo.ContentItemId, userId);
                    this.tabController.RefreshCache(portalId, tabId);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId}): {Arg3}", portalId, tabId, userId, ex.Message);
                Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId})", portalId, tabId, userId);
        }

        private Workflow GetCurrentOrDefaultWorkflow(ContentItem item, int portalId)
        {
			Logger.LogError(">>> Entering GetCurrentOrDefaultWorkflow(item={Item},portalId={PortalId})", item, portalId);
            if (item.StateID != Null.NullInteger)
            {
                return this.workflowManager.GetWorkflow(item);
            }

            var defaultWorkflow = this.tabWorkflowSettings.GetDefaultTabWorkflowId(portalId);
			Logger.LogError("GetCurrentOrDefaultWorkflow(item={Item},portalId={PortalId}): defaultWorkflow → {DefaultWorkflow}", item, portalId, defaultWorkflow);
			Logger.LogWarning("<<< Exiting GetCurrentOrDefaultWorkflow(item={Item},portalId={PortalId})", item, portalId);
            return this.workflowManager.GetWorkflow(defaultWorkflow);
        }
    }
}
