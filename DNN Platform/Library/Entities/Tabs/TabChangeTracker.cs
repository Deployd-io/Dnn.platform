// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class TabChangeTracker : ServiceLocator<ITabChangeTracker, TabChangeTracker>, ITabChangeTracker
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabChangeTracker));
        public const string IsModuleDoesNotBelongToPage = nameof(IsModuleDoesNotBelongToPage);

        /// <inheritdoc />
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID);
			Logger.LogError("TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId}): unPublishedVersion → {UnPublishedVersion}", module, moduleVersion, userId, unPublishedVersion);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleAddition(module, moduleVersion, userId);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(module.PortalID, module.TabID) && unPublishedVersion == null)
            {
                TabWorkflowTracker.Instance.TrackModuleAddition(module, moduleVersion, userId);
            }
			Logger.LogWarning("<<< Exiting TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <inheritdoc />
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            if (ModuleController.Instance.IsSharedModule(module) && moduleVersion != Null.NullInteger)
            {
                var exceptionToThrow = new InvalidOperationException(
                    Localization.GetExceptionMessage(
                    "ModuleDoesNotBelongToPage",
                    "This module does not belong to the page. Please, move to its master page to change the module"));
                exceptionToThrow.Data.Add(IsModuleDoesNotBelongToPage, true);
				Logger.LogError("Throwing exceptionToThrow from TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
                throw exceptionToThrow;
            }

            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID);
			Logger.LogError("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): unPublishedVersion → {UnPublishedVersion}", module, moduleVersion, userId, unPublishedVersion);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleModification(module, moduleVersion, userId);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(module.PortalID, module.TabID) && unPublishedVersion == null)
            {
                TabWorkflowTracker.Instance.TrackModuleModification(module, moduleVersion, userId);
            }
			Logger.LogWarning("<<< Exiting TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <inheritdoc />
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID);
			Logger.LogError("TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId}): unPublishedVersion → {UnPublishedVersion}", module, moduleVersion, userId, unPublishedVersion);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleDeletion(module, moduleVersion, userId);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(module.PortalID, module.TabID) && unPublishedVersion == null)
            {
                TabWorkflowTracker.Instance.TrackModuleDeletion(module, moduleVersion, userId);
            }
			Logger.LogWarning("<<< Exiting TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <inheritdoc />
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            if (module != null && TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleUncopy(module, moduleVersion, originalTabId, userId);
            }
			Logger.LogWarning("<<< Exiting TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <inheritdoc />
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleCopy(module, moduleVersion, originalTabId, userId);
            }
			Logger.LogWarning("<<< Exiting TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <inheritdoc />
        protected override Func<ITabChangeTracker> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new TabChangeTracker();
        }
    }
}
