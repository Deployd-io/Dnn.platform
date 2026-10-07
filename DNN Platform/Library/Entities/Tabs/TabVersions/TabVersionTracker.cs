// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    internal class TabVersionTracker : ServiceLocator<ITabChangeTracker, TabVersionTracker>, ITabChangeTracker
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabVersionTracker));
        /// <summary>Tracks a version detail when a module is added to a page.</summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            Requires.NotNull("module", module);

            try
            {
                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId);
				Logger.LogError("TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId}): unPublishedVersion → {UnPublishedVersion}", module, moduleVersion, userId, unPublishedVersion);

                ProcessAdditionDetail(module, moduleVersion, userId, unPublishedVersion);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId}): {Arg3}", module, moduleVersion, userId, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting TrackModuleAddition(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a version detail when a module is modified on a page.</summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            Requires.NotNull("module", module);

            try
            {
                if (IsHostModule(module))
                {
                    return;
                }

                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId);
				Logger.LogError("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): unPublishedVersion → {UnPublishedVersion}", module, moduleVersion, userId, unPublishedVersion);
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module.ModuleID, module.PaneName, module.ModuleOrder, moduleVersion, TabVersionDetailAction.Modified);
				Logger.LogError("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): tabVersionDetail → {TabVersionDetail}", module, moduleVersion, userId, tabVersionDetail);

                var existingTabVersionDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
				Logger.LogError("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): existingTabVersionDetail → {ExistingTabVersionDetail}", module, moduleVersion, userId, existingTabVersionDetail);
                if (existingTabVersionDetail != null)
                {
                    tabVersionDetail.TabVersionDetailId = existingTabVersionDetail.TabVersionDetailId;
					Logger.LogDebug("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): tabVersionDetail.TabVersionDetailId → {Arg3}", module, moduleVersion, userId, tabVersionDetail.TabVersionDetailId);
                    if (moduleVersion == Null.NullInteger)
                    {
                        tabVersionDetail.ModuleVersion = existingTabVersionDetail.ModuleVersion;
						Logger.LogDebug("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): tabVersionDetail.ModuleVersion → {Arg3}", module, moduleVersion, userId, tabVersionDetail.ModuleVersion);
                    }

                    // If the operation is done over a just created module, the Added operation is kept.
                    if (existingTabVersionDetail.Action == TabVersionDetailAction.Added)
                    {
                        tabVersionDetail.Action = TabVersionDetailAction.Added;
						Logger.LogDebug("TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): tabVersionDetail.Action → {Arg3}", module, moduleVersion, userId, tabVersionDetail.Action);
                    }
                }

                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
            }
            catch (InvalidOperationException)
            {
				Logger.LogError("Exception in TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): {Arg3}", module, moduleVersion, userId, InvalidOperationException.Message);
                throw;
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId}): {Arg3}", module, moduleVersion, userId, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting TrackModuleModification(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a version detail when a module is deleted from a page.</summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
            Requires.NotNull("module", module);

            try
            {
                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId);
				Logger.LogError("TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId}): unPublishedVersion → {UnPublishedVersion}", module, moduleVersion, userId, unPublishedVersion);

                ProcessDeletionDetail(module, moduleVersion, userId, unPublishedVersion);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId}): {Arg3}", module, moduleVersion, userId, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting TrackModuleDeletion(module={Module},moduleVersion={ModuleVersion},userId={UserId})", module, moduleVersion, userId);
        }

        /// <summary>Tracks a version detail when a module is copied from an exisitng page.</summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// /// <param name="userId">User Id who provokes the version detail.</param>
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            Requires.NotNull("module", module);

            try
            {
                var targetVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID) ??
                                    TabVersionBuilder.Instance.GetCurrentVersion(module.TabID);
                if (targetVersion == null)
                {
                    return;
                }

                ProcessAdditionDetail(module, moduleVersion, userId, targetVersion);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId}): {Arg4}", module, moduleVersion, originalTabId, userId, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting TrackModuleCopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <summary>Tracks a version detail when a copied module is deleted from an exisitng page.</summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
			Logger.LogError(">>> Entering TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
            Requires.NotNull("module", module);

            try
            {
                var targetVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID) ??
                                    TabVersionBuilder.Instance.GetCurrentVersion(module.TabID);

                if (targetVersion == null)
                {
                    return;
                }

                ProcessDeletionDetail(module, moduleVersion, userId, targetVersion);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId}): {Arg4}", module, moduleVersion, originalTabId, userId, ex.Message);
                Services.Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting TrackModuleUncopy(module={Module},moduleVersion={ModuleVersion},originalTabId={OriginalTabId},userId={UserId})", module, moduleVersion, originalTabId, userId);
        }

        /// <inheritdoc />
        protected override Func<ITabChangeTracker> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new TabVersionTracker();
        }

        private static void ProcessAdditionDetail(ModuleInfo module, int moduleVersion, int userId, TabVersion targetVersion)
        {
			Logger.LogError(">>> Entering ProcessAdditionDetail(module={Module},moduleVersion={ModuleVersion},userId={UserId},targetVersion={TargetVersion})", module, moduleVersion, userId, targetVersion);
            if (IsHostModule(module))
            {
                return;
            }

            // Module could be restored in the same version
            var existingTabDetails =
                TabVersionDetailController.Instance.GetTabVersionDetails(targetVersion.TabVersionId)
                    .Where(tvd => tvd.ModuleId == module.ModuleID);
            foreach (var existingTabDetail in existingTabDetails)
            {
                TabVersionDetailController.Instance.DeleteTabVersionDetail(
                    existingTabDetail.TabVersionId,
                    existingTabDetail.TabVersionDetailId);
            }

            var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(
                targetVersion.TabVersionId,
                module,
                moduleVersion,
                TabVersionDetailAction.Added);
            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
			Logger.LogWarning("<<< Exiting ProcessAdditionDetail(module={Module},moduleVersion={ModuleVersion},userId={UserId},targetVersion={TargetVersion})", module, moduleVersion, userId, targetVersion);
        }

        private static void ProcessDeletionDetail(ModuleInfo module, int moduleVersion, int userId, TabVersion targetVersion)
        {
			Logger.LogError(">>> Entering ProcessDeletionDetail(module={Module},moduleVersion={ModuleVersion},userId={UserId},targetVersion={TargetVersion})", module, moduleVersion, userId, targetVersion);
            if (IsHostModule(module))
            {
                return;
            }

            var existingTabDetail =
                TabVersionDetailController.Instance.GetTabVersionDetails(targetVersion.TabVersionId)
                    .SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
            if (existingTabDetail != null)
            {
                TabVersionDetailController.Instance.DeleteTabVersionDetail(
                    existingTabDetail.TabVersionId,
                    existingTabDetail.TabVersionDetailId);

                // When a module is added in the same version, then we should do nothing with it
                if (existingTabDetail.Action == TabVersionDetailAction.Added)
                {
                    return;
                }
            }

            // Do not add module to Tab Version Details if it has been hard deleted
            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(module.ModuleID, module.TabID, false);
			Logger.LogError("ProcessDeletionDetail(module={Module},moduleVersion={ModuleVersion},userId={UserId},targetVersion={TargetVersion}): moduleInfo → {ModuleInfo}", module, moduleVersion, userId, targetVersion, moduleInfo);
            if (moduleInfo != null)
            {
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(
                    targetVersion.TabVersionId,
                    module,
                    moduleVersion,
                    TabVersionDetailAction.Deleted);
                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
            }
			Logger.LogWarning("<<< Exiting ProcessDeletionDetail(module={Module},moduleVersion={ModuleVersion},userId={UserId},targetVersion={TargetVersion})", module, moduleVersion, userId, targetVersion);
        }

        private static bool IsHostModule(ModuleInfo module)
        {
			Logger.LogError(">>> Entering IsHostModule(module={Module})", module);
            Logger.LogWarning("<<< Exiting IsHostModule(module={Module})", module);
            return module.PortalID == Null.NullInteger;
        }

        private static TabVersion GetOrCreateUnPublishedTabVersion(int portalId, int tabId, int createdByUserId)
        {
			Logger.LogError(">>> Entering GetOrCreateUnPublishedTabVersion(portalId={PortalId},tabId={TabId},createdByUserId={CreatedByUserId})", portalId, tabId, createdByUserId);
            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(tabId);
			Logger.LogError("GetOrCreateUnPublishedTabVersion(portalId={PortalId},tabId={TabId},createdByUserId={CreatedByUserId}): unPublishedVersion → {UnPublishedVersion}", portalId, tabId, createdByUserId, unPublishedVersion);
			Logger.LogWarning("<<< Exiting GetOrCreateUnPublishedTabVersion(portalId={PortalId},tabId={TabId},createdByUserId={CreatedByUserId})", portalId, tabId, createdByUserId);
            return unPublishedVersion == null ?
                TabVersionBuilder.Instance.CreateNewVersion(portalId, tabId, createdByUserId) :
                TabVersionController.Instance.GetTabVersions(tabId).SingleOrDefault(tv => !tv.IsPublished);
        }

        private static TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, ModuleInfo module, int moduleVersion, TabVersionDetailAction action)
        {
			Logger.LogError(">>> Entering CreateNewTabVersionDetailObjectFromModule(tabVersionId={TabVersionId},module={Module},moduleVersion={ModuleVersion},action={Action})", tabVersionId, module, moduleVersion, action);
			Logger.LogWarning("<<< Exiting CreateNewTabVersionDetailObjectFromModule(tabVersionId={TabVersionId},module={Module},moduleVersion={ModuleVersion},action={Action})", tabVersionId, module, moduleVersion, action);
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = module.ModuleID,
                ModuleVersion = moduleVersion,
                ModuleOrder = module.ModuleOrder,
                PaneName = module.PaneName,
                Action = action,
			Logger.LogWarning("<<< Exiting CreateNewTabVersionDetailObjectFromModule(tabVersionId={TabVersionId},moduleId={ModuleId},paneName={PaneName},moduleOrder={ModuleOrder},moduleVersion={ModuleVersion},action={Action})", tabVersionId, moduleId, paneName, moduleOrder, moduleVersion, action);
            };
        }

        private static TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, int moduleId, string paneName, int moduleOrder, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = moduleId,
                ModuleVersion = moduleVersion,
                ModuleOrder = moduleOrder,
                PaneName = paneName,
                Action = action,
            };
        }
    }
}
