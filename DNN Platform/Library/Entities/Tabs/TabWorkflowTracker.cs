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
			Logger.LogDebug(">>> Entering TabWorkflowTracker()");
            this.tabController = TabController.Instance;
            this.workflowEngine = WorkflowEngine.Instance;
			Logger.LogDebug("TabWorkflowTracker(): this.tabController → {Arg0}", this.tabController);
			Logger.LogDebug("TabWorkflowTracker(): workflowEngine → {WorkflowEngine}", workflowEngine);
			Logger.LogDebug("TabWorkflowTracker(): this.workflowEngine → {Arg0}", this.workflowEngine);
			Logger.LogDebug("TabWorkflowTracker(): tabController → {TabController}", tabController);
            this.workflowManager = WorkflowManager.Instance;
            this.tabWorkflowSettings = TabWorkflowSettings.Instance;
			Logger.LogDebug("<<< Exiting TabWorkflowTracker()");
			Logger.LogDebug("TabWorkflowTracker(): workflowManager → {WorkflowManager}", workflowManager);
			Logger.LogDebug("TabWorkflowTracker(): this.workflowManager → {Arg0}", this.workflowManager);
			Logger.LogDebug("TabWorkflowTracker(): tabWorkflowSettings → {TabWorkflowSettings}", tabWorkflowSettings);
			Logger.LogDebug("TabWorkflowTracker(): this.tabWorkflowSettings → {Arg0}", this.tabWorkflowSettings);
        }

        /// <summary>Tracks a workflow instance when a module is added to a page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogDebug(">>> Entering TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
			Logger.LogDebug("<<< Exiting TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a workflow instance when a module is modified on a page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogDebug(">>> Entering TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
			Logger.LogDebug("<<< Exiting TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a workflow instance when a module is deleted from a page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogDebug(">>> Entering TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
			Logger.LogDebug("<<< Exiting TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a workflow instance when a module is copied from an exisitng page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogDebug(">>> Entering TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            this.TrackModuleAddition(module, moduleVersion, userId);
			Logger.LogDebug("<<< Exiting TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <summary>Tracks a workflow instance when a copied module is deleted from an exisitng page.</summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogDebug(">>> Entering TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            this.TrackModuleDeletion(module, moduleVersion, userId);
			Logger.LogDebug("<<< Exiting TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <inheritdoc />
        protected override Func<ITabChangeTracker> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new TabWorkflowTracker();
        }

        private void NotifyWorkflowAboutChanges(int portalId, int tabId, int userId)
        {
			Logger.LogDebug(">>> Entering NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId})", portalId, tabId, userId);
            try
            {
                var tabInfo = this.tabController.GetTab(tabId, portalId);
                if (tabInfo != null && !tabInfo.IsDeleted && this.workflowEngine.IsWorkflowCompleted(tabInfo))
				Logger.LogDebug("NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId}): tabInfo → {TabInfo}", portalId, tabId, userId, tabInfo);
                {
                    var workflow = this.GetCurrentOrDefaultWorkflow(tabInfo, portalId);
                    if (workflow == null)
					Logger.LogDebug("NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId}): workflow → {Workflow}", portalId, tabId, userId, workflow);
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
			Logger.LogDebug("<<< Exiting NotifyWorkflowAboutChanges(portalId={PortalId},tabId={TabId},userId={UserId})", portalId, tabId, userId);
        }

        private Workflow GetCurrentOrDefaultWorkflow(ContentItem item, int portalId)
        {
			Logger.LogDebug(">>> Entering GetCurrentOrDefaultWorkflow(item={Item},portalId={PortalId})", item, portalId);
            if (item.StateID != Null.NullInteger)
            {
                return this.workflowManager.GetWorkflow(item);
            }

            var defaultWorkflow = this.tabWorkflowSettings.GetDefaultTabWorkflowId(portalId);
			Logger.LogDebug("GetCurrentOrDefaultWorkflow(item={Item},portalId={PortalId}): defaultWorkflow → {DefaultWorkflow}", item, portalId, defaultWorkflow);
			Logger.LogDebug("<<< Exiting GetCurrentOrDefaultWorkflow(item={Item},portalId={PortalId})", item, portalId);
            return this.workflowManager.GetWorkflow(defaultWorkflow);
        }
    }
}
