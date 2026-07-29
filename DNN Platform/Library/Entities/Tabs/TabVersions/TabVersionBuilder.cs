// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.TabVersions.Exceptions;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITabVersionBuilder"/> implementation.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class TabVersionBuilder(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings)
        : ServiceLocator<ITabVersionBuilder, TabVersionBuilder>, ITabVersionBuilder
    {
		Logger.LogDebug(">>> Entering TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings})", businessControllerProvider, hostSettings);
        private const int DefaultVersionNumber = 1;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabVersionBuilder));
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
		Logger.LogDebug("TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): Logger → {Logger}", businessControllerProvider, hostSettings, Logger);
		Logger.LogDebug("TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): businessControllerProvider → {BusinessControllerProvider1}", businessControllerProvider, hostSettings, businessControllerProvider);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly ITabController tabController = TabController.Instance;
		Logger.LogDebug("TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): hostSettings → {HostSettings1}", businessControllerProvider, hostSettings, hostSettings);
        private readonly IModuleController moduleController = ModuleController.Instance;
        private readonly ITabVersionSettings tabVersionSettings = TabVersionSettings.Instance;
        private readonly ITabVersionController tabVersionController = TabVersionController.Instance;
        private readonly ITabVersionDetailController tabVersionDetailController = TabVersionDetailController.Instance;
        private readonly PortalSettings portalSettings = PortalSettings.Current;

        /// <summary>Initializes a new instance of the <see cref="TabVersionBuilder"/> class.</summary>
		Logger.LogDebug("<<< Exiting TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings})", businessControllerProvider, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public TabVersionBuilder()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TabVersionBuilder"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public TabVersionBuilder(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
		Logger.LogError("Exception in TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): {Arg2}", businessControllerProvider, hostSettings, ex.Message);
        }

        /// <inheritdoc />
        public void SetupFirstVersionForExistingTab(int portalId, int tabId)
        {
            if (!this.tabVersionSettings.IsVersioningEnabled(portalId, tabId))
            {
                return;
            }

            // Check if already exist at least one version for the tab
            if (this.tabVersionController.GetTabVersions(tabId).Any())
            {
                return;
            }

            var tab = this.tabController.GetTab(tabId, portalId);
            var modules = this.moduleController.GetTabModules(tabId).Where(m => m.Value.IsDeleted == false).Select(m => m.Value).ToArray();

            // Check if the page has modules
            if (modules.Length == 0)
            {
                return;
            }

            this.CreateFirstTabVersion(tabId, tab, modules);
			Logger.LogDebug("<<< Exiting SetupFirstVersionForExistingTab(portalId={PortalId},tabId={TabId})", portalId, tabId);
        }

        /// <inheritdoc />
        public void Publish(int portalId, int tabId, int createdByUserId)
        {
            var tabVersion = this.GetUnPublishedVersion(tabId);
            if (tabVersion == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabHasNotAnUnpublishedVersion", Localization.ExceptionsResourceFile), tabId));
            }

            if (tabVersion.IsPublished)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabVersionAlreadyPublished", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }

            var previousPublishVersion = this.GetCurrentVersion(tabId);
            this.PublishVersion(portalId, tabId, createdByUserId, tabVersion);

            if (!this.tabVersionSettings.IsVersioningEnabled(portalId, tabId)
                && previousPublishVersion != null)
            {
                this.ForceDeleteVersion(tabId, previousPublishVersion.Version);
            }
        }

        /// <inheritdoc />
        public void Discard(int tabId, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering Discard(tabId={TabId},createdByUserId={CreatedByUserId})", tabId, createdByUserId);
            var tabVersion = this.GetUnPublishedVersion(tabId);
            if (tabVersion == null)
			Logger.LogDebug("Discard(tabId={TabId},createdByUserId={CreatedByUserId}): tabVersion → {TabVersion}", tabId, createdByUserId, tabVersion);
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabHasNotAnUnpublishedVersion", Localization.ExceptionsResourceFile), tabId));
            }

            if (tabVersion.IsPublished)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabVersionAlreadyPublished", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }

            this.DiscardVersion(tabId, tabVersion);
			Logger.LogDebug("<<< Exiting Discard(tabId={TabId},createdByUserId={CreatedByUserId})", tabId, createdByUserId);
        }

        /// <inheritdoc />
        public void DeleteVersion(int tabId, int createdByUserId, int version)
        {
			Logger.LogDebug(">>> Entering DeleteVersion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version})", tabId, createdByUserId, version);
            this.CheckVersioningEnabled(tabId);

            this.ForceDeleteVersion(tabId, version);
			Logger.LogDebug("<<< Exiting DeleteVersion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version})", tabId, createdByUserId, version);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public TabVersion RollBackVesion(int tabId, int createdByUserId, int version)
        {
			Logger.LogDebug(">>> Entering RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version})", tabId, createdByUserId, version);
            this.CheckVersioningEnabled(tabId);

            if (this.GetUnPublishedVersion(tabId) != null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabVersionCannotBeRolledBack_UnpublishedVersionExists", Localization.ExceptionsResourceFile), tabId, version));
            }

            var lastTabVersion = this.tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            if (lastTabVersion == null || lastTabVersion.Version == version)
			Logger.LogDebug("RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version}): lastTabVersion → {LastTabVersion}", tabId, createdByUserId, version, lastTabVersion);
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabVersionCannotBeRolledBack_LastVersion", Localization.ExceptionsResourceFile), tabId, version));
            }

            var publishedDetails = this.GetVersionModulesDetails(tabId, lastTabVersion.Version).ToArray();

			Logger.LogDebug("RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version}): publishedDetails → {PublishedDetails}", tabId, createdByUserId, version, publishedDetails);
            var rollbackDetails = CopyVersionDetails(this.GetVersionModulesDetails(tabId, version)).ToArray();
            var newVersion = this.CreateNewVersion(tabId, createdByUserId);
			Logger.LogDebug("RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version}): rollbackDetails → {RollbackDetails}", tabId, createdByUserId, version, rollbackDetails);
			Logger.LogDebug("RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version}): newVersion → {NewVersion}", tabId, createdByUserId, version, newVersion);

            // Save Reset detail
            this.tabVersionDetailController.SaveTabVersionDetail(GetResetTabVersionDetail(newVersion), createdByUserId);

            foreach (var rollbackDetail in rollbackDetails)
            {
                rollbackDetail.TabVersionId = newVersion.TabVersionId;
				Logger.LogDebug("RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version}): rollbackDetail.TabVersionId → {Arg3}", tabId, createdByUserId, version, rollbackDetail.TabVersionId);
                try
                {
                    rollbackDetail.ModuleVersion = this.RollBackDetail(tabId, rollbackDetail);
                }
                catch (DnnTabVersionException e)
                {
                    Logger.Error($"There was a problem making rollback of the module {rollbackDetail.ModuleId}. Message: {e.Message}.");
                    continue;
                }

                this.tabVersionDetailController.SaveTabVersionDetail(rollbackDetail, createdByUserId);

                // Check if restoring version contains modules to restore
                if (publishedDetails.All(tv => tv.ModuleId != rollbackDetail.ModuleId))
                {
                    this.RestoreModuleInfo(tabId, rollbackDetail);
                }
                else
                {
                    this.UpdateModuleOrder(tabId, rollbackDetail);
                }
            }

            // Check if current version contains modules not existing in restoring version
            foreach (var publishedDetail in publishedDetails.Where(publishedDetail => rollbackDetails.All(tvd => tvd.ModuleId != publishedDetail.ModuleId)))
            {
                this.moduleController.DeleteTabModule(tabId, publishedDetail.ModuleId, true);
            }

            // Publish Version
			Logger.LogDebug("<<< Exiting RollBackVesion(tabId={TabId},createdByUserId={CreatedByUserId},version={Version})", tabId, createdByUserId, version);
            return this.PublishVersion(this.GetCurrentPortalId(), tabId, createdByUserId, newVersion);
        }

        /// <inheritdoc />
        public TabVersion CreateNewVersion(int tabId, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering CreateNewVersion(tabId={TabId},createdByUserId={CreatedByUserId})", tabId, createdByUserId);
			Logger.LogDebug("<<< Exiting CreateNewVersion(tabId={TabId},createdByUserId={CreatedByUserId})", tabId, createdByUserId);
            return this.CreateNewVersion(this.GetCurrentPortalId(), tabId, createdByUserId);
        }

        /// <inheritdoc />
        public TabVersion CreateNewVersion(int portalid, int tabId, int createdByUserId)
        {
            if (portalid == Null.NullInteger)
            {
                throw new InvalidOperationException(Localization.GetString("TabVersioningNotEnabled", Localization.ExceptionsResourceFile));
            }

            this.SetupFirstVersionForExistingTab(portalid, tabId);

            this.DeleteOldestVersionIfTabHasMaxNumberOfVersions(portalid, tabId);
            try
            {
				Logger.LogDebug("<<< Exiting CreateNewVersion(portalid={Portalid},tabId={TabId},createdByUserId={CreatedByUserId})", portalid, tabId, createdByUserId);
                return this.tabVersionController.CreateTabVersion(tabId, createdByUserId);
            }
            catch (InvalidOperationException e)
            {
				Logger.LogError("Exception in TabVersionBuilder(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): {Arg2}", businessControllerProvider, hostSettings, ex.Message);
                Services.Exceptions.Exceptions.LogException(e);
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("TabVersionCannotBeCreated_UnpublishedVersionAlreadyExistsConcurrencyProblem", Localization.ExceptionsResourceFile), tabId), e);
            }
            catch (SqlException sqlException)
            {
                Services.Exceptions.Exceptions.LogException(sqlException);
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Localization.GetString("TabVersionCannotBeCreated_UnpublishedVersionAlreadyExistsConcurrencyProblem", Localization.ExceptionsResourceFile), tabId));
            }
        }

        /// <inheritdoc />
        public IEnumerable<ModuleInfo> GetUnPublishedVersionModules(int tabId)
        {
			Logger.LogDebug(">>> Entering GetUnPublishedVersionModules(tabId={TabId})", tabId);
            var unPublishedVersion = this.GetUnPublishedVersion(tabId);
            if (unPublishedVersion == null)
			Logger.LogDebug("GetUnPublishedVersionModules(tabId={TabId}): unPublishedVersion → {UnPublishedVersion}", tabId, unPublishedVersion);
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            }

			Logger.LogDebug("<<< Exiting GetUnPublishedVersionModules(tabId={TabId})", tabId);
            return this.GetVersionModules(tabId, unPublishedVersion.TabVersionId);
        }

        /// <inheritdoc />
        public TabVersion GetCurrentVersion(int tabId, bool ignoreCache = false)
        {
			Logger.LogDebug(">>> Entering GetCurrentVersion(tabId={TabId},ignoreCache={IgnoreCache})", tabId, ignoreCache);
			Logger.LogDebug("<<< Exiting GetCurrentVersion(tabId={TabId},ignoreCache={IgnoreCache})", tabId, ignoreCache);
            return this.tabVersionController.GetTabVersions(tabId, ignoreCache)
                .Where(tv => tv.IsPublished).OrderByDescending(tv => tv.CreatedOnDate).FirstOrDefault();
        }

        /// <inheritdoc />
        public TabVersion GetUnPublishedVersion(int tabId)
        {
			Logger.LogDebug(">>> Entering GetUnPublishedVersion(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetUnPublishedVersion(tabId={TabId})", tabId);
            return this.tabVersionController.GetTabVersions(tabId, true)
                .SingleOrDefault(tv => !tv.IsPublished);
        }

        /// <inheritdoc />
        public IEnumerable<ModuleInfo> GetCurrentModules(int tabId)
        {
			Logger.LogDebug(">>> Entering GetCurrentModules(tabId={TabId})", tabId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PublishedTabModuleCacheKey, tabId);
			Logger.LogDebug("GetCurrentModules(tabId={TabId}): cacheKey → {CacheKey}", tabId, cacheKey);
			Logger.LogDebug("<<< Exiting GetCurrentModules(tabId={TabId})", tabId);
            return CBO.GetCachedObject<IEnumerable<ModuleInfo>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PublishedTabModuleCacheTimeOut, DataCache.PublishedTabModuleCachePriority),
                c => this.GetCurrentModulesInternal(tabId));
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IEnumerable<ModuleInfo> GetVersionModules(int tabId, int version)
        {
			Logger.LogDebug(">>> Entering GetVersionModules(tabId={TabId},version={Version})", tabId, version);
			Logger.LogDebug("<<< Exiting GetVersionModules(tabId={TabId},version={Version})", tabId, version);
            return this.ConvertToModuleInfo(this.GetVersionModulesDetails(tabId, version), tabId);
        }

        /// <inheritdoc />
        public int GetModuleContentLatestVersion(ModuleInfo module)
        {
            var versionableController = this.GetVersionableController(module);
            return versionableController?.GetLatestVersion(module.ModuleID) ?? DefaultVersionNumber;
        }

        /// <inheritdoc />
        protected override Func<ITabVersionBuilder> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ITabVersionBuilder>;
        }

        private static List<TabVersionDetail> GetSnapShot(IEnumerable<TabVersionDetail> tabVersionDetails)
        {
            var versionModules = new Dictionary<int, TabVersionDetail>();
            foreach (var tabVersionDetail in tabVersionDetails)
            {
                switch (tabVersionDetail.Action)
                {
                    case TabVersionDetailAction.Added:
                    case TabVersionDetailAction.Modified:
                        if (versionModules.TryGetValue(tabVersionDetail.ModuleId, out var versionDetail))
                        {
                            versionModules[tabVersionDetail.ModuleId] = JoinVersionDetails(versionDetail, tabVersionDetail);
                        }
                        else
                        {
                            versionModules.Add(tabVersionDetail.ModuleId, tabVersionDetail);
                        }

                        break;
                    case TabVersionDetailAction.Deleted:
                        if (versionModules.ContainsKey(tabVersionDetail.ModuleId))
                        {
                            versionModules.Remove(tabVersionDetail.ModuleId);
                        }

                        break;
                    case TabVersionDetailAction.Reset:
                        versionModules.Clear();
                        break;
                }
            }

            // Return Snapshot ordering by PaneName and ModuleOrder (this is required as Skin.cs does not order by these fields)
            return versionModules.Values
                .OrderBy(m => m.PaneName)
                .ThenBy(m => m.ModuleOrder)
                .ToList();
        }

        private static TabVersionDetail JoinVersionDetails(TabVersionDetail tabVersionDetail, TabVersionDetail newVersionDetail)
        {
			Logger.LogDebug(">>> Entering JoinVersionDetails(tabVersionDetail={TabVersionDetail},newVersionDetail={NewVersionDetail})", tabVersionDetail, newVersionDetail);
            // Movement changes have not ModuleVersion
            if (newVersionDetail.ModuleVersion == Null.NullInteger)
            {
                newVersionDetail.ModuleVersion = tabVersionDetail.ModuleVersion;
				Logger.LogDebug("JoinVersionDetails(tabVersionDetail={TabVersionDetail},newVersionDetail={NewVersionDetail}): newVersionDetail.ModuleVersion → {Arg2}", tabVersionDetail, newVersionDetail, newVersionDetail.ModuleVersion);
            }

			Logger.LogDebug("<<< Exiting JoinVersionDetails(tabVersionDetail={TabVersionDetail},newVersionDetail={NewVersionDetail})", tabVersionDetail, newVersionDetail);
            return newVersionDetail;
        }

        private static TabVersionDetail GetResetTabVersionDetail(TabVersion tabVersion)
        {
            return new TabVersionDetail
            {
                PaneName = "none_resetAction",
                TabVersionId = tabVersion.TabVersionId,
                Action = TabVersionDetailAction.Reset,
                ModuleId = Null.NullInteger,
                ModuleVersion = Null.NullInteger,
            };
        }

        private static List<TabVersionDetail> CopyVersionDetails(IEnumerable<TabVersionDetail> tabVersionDetails)
        {
            return tabVersionDetails.Select(tabVersionDetail => new TabVersionDetail
            {
                ModuleId = tabVersionDetail.ModuleId,
                ModuleOrder = tabVersionDetail.ModuleOrder,
                ModuleVersion = tabVersionDetail.ModuleVersion,
                PaneName = tabVersionDetail.PaneName,
                Action = tabVersionDetail.Action,
            }).ToList();
        }

        private void DiscardVersion(int tabId, TabVersion tabVersion)
        {
            var unPublishedDetails = this.tabVersionDetailController.GetTabVersionDetails(tabVersion.TabVersionId);

            var currentPublishedVersion = this.GetCurrentVersion(tabId);
            TabVersionDetail[] publishedChanges = null;

            if (currentPublishedVersion != null)
            {
                publishedChanges = this.GetVersionModulesDetails(tabId, this.GetCurrentVersion(tabId).Version).ToArray();
            }

            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (publishedChanges == null)
                {
                    this.DiscardDetailWithoutPublishedTabVersions(tabId, unPublishedDetail);
                }
                else
                {
                    this.DiscardDetailWithPublishedTabVersions(tabId, unPublishedDetail, publishedChanges);
                }
            }

            this.tabVersionController.DeleteTabVersion(tabId, tabVersion.TabVersionId);
        }

        private IEnumerable<ModuleInfo> GetCurrentModulesInternal(int tabId)
        {
			Logger.LogDebug(">>> Entering GetCurrentModulesInternal(tabId={TabId})", tabId);
            var versioningEnabled = this.portalSettings != null &&
                                    this.tabVersionSettings.IsVersioningEnabled(this.portalSettings.PortalId, tabId);
            if (!versioningEnabled)
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            }

            // If versionins is enabled but the tab doesn't have versions history,
            // then it's a tab never edited after version enabling.
            var tabWithoutVersions = !this.tabVersionController.GetTabVersions(tabId).Any();
            if (tabWithoutVersions)
			Logger.LogDebug("GetCurrentModulesInternal(tabId={TabId}): tabWithoutVersions → {TabWithoutVersions}", tabId, tabWithoutVersions);
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            }

            var currentVersion = this.GetCurrentVersion(tabId);
            if (currentVersion == null)
			Logger.LogDebug("GetCurrentModulesInternal(tabId={TabId}): currentVersion → {CurrentVersion}", tabId, currentVersion);
            {
                // Only when a tab is on a first version and it is not published, the currentVersion object can be null
                return new List<ModuleInfo>();
            }

			Logger.LogDebug("<<< Exiting GetCurrentModulesInternal(tabId={TabId})", tabId);
            return this.GetVersionModules(tabId, currentVersion.Version);
        }

        private void DiscardDetailWithoutPublishedTabVersions(int tabId, TabVersionDetail unPublishedDetail)
        {
			Logger.LogDebug(">>> Entering DiscardDetailWithoutPublishedTabVersions(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
            if (unPublishedDetail.ModuleVersion != Null.NullInteger)
            {
                this.DiscardDetail(tabId, unPublishedDetail);
            }

            this.moduleController.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
			Logger.LogDebug("<<< Exiting DiscardDetailWithoutPublishedTabVersions(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
        }

        private void DiscardDetailWithPublishedTabVersions(int tabId, TabVersionDetail unPublishedDetail, TabVersionDetail[] publishedChanges)
        {
            if (unPublishedDetail.Action == TabVersionDetailAction.Deleted)
            {
                var restoredModuleDetail = publishedChanges.SingleOrDefault(tv => tv.ModuleId == unPublishedDetail.ModuleId);
                if (restoredModuleDetail != null)
                {
                    this.RestoreModuleInfo(tabId, restoredModuleDetail);
                }

                return;
            }

            if (publishedChanges.All(tv => tv.ModuleId != unPublishedDetail.ModuleId))
            {
                this.moduleController.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
                return;
            }

            if (unPublishedDetail.Action == TabVersionDetailAction.Modified)
            {
                var publishDetail = publishedChanges.SingleOrDefault(tv => tv.ModuleId == unPublishedDetail.ModuleId);
                if (publishDetail.PaneName != unPublishedDetail.PaneName ||
                    publishDetail.ModuleOrder != unPublishedDetail.ModuleOrder)
                {
                    this.moduleController.UpdateModuleOrder(
                        tabId,
                        publishDetail.ModuleId,
                        publishDetail.ModuleOrder,
                        publishDetail.PaneName);
                }

                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    this.DiscardDetail(tabId, unPublishedDetail);
                }
            }
			Logger.LogDebug("<<< Exiting DiscardDetailWithPublishedTabVersions(tabId={TabId},unPublishedDetail={UnPublishedDetail},publishedChanges={PublishedChanges})", tabId, unPublishedDetail, publishedChanges);
        }

        private void ForceDeleteVersion(int tabId, int version)
        {
			Logger.LogDebug(">>> Entering ForceDeleteVersion(tabId={TabId},version={Version})", tabId, version);
            var unpublishedVersion = this.GetUnPublishedVersion(tabId);
            if (unpublishedVersion != null
				Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): unpublishedVersion → {UnpublishedVersion}", tabId, version, unpublishedVersion);
                && unpublishedVersion.Version == version)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Localization.GetString("TabVersionCannotBeDeleted_UnpublishedVersion", Localization.ExceptionsResourceFile),
                        tabId,
                        version));
            }

            var tabVersions = this.tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);
            if (tabVersions.Count() <= 1)
			Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): tabVersions → {TabVersions}", tabId, version, tabVersions);
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Localization.GetString("TabVersionCannotBeDiscarded_OnlyOneVersion", Localization.ExceptionsResourceFile),
                        tabId,
                        version));
            }

            var versionToDelete = tabVersions.ElementAt(0);

			Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): versionToDelete → {VersionToDelete}", tabId, version, versionToDelete);
            // check if the version to delete if the latest published one
            if (versionToDelete.Version == version)
            {
                var restoreMaxNumberOfVersions = false;
                var portalId = this.portalSettings.PortalId;
				Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): restoreMaxNumberOfVersions → {RestoreMaxNumberOfVersions}", tabId, version, restoreMaxNumberOfVersions);
                var maxNumberOfVersions = this.tabVersionSettings.GetMaxNumberOfVersions(portalId);

				Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): maxNumberOfVersions → {MaxNumberOfVersions}", tabId, version, maxNumberOfVersions);
                // If we already have reached the maxNumberOfVersions we need to extend to 1 this limit to allow the tmp version
                if (tabVersions.Count() == maxNumberOfVersions)
                {
                    this.tabVersionSettings.SetMaxNumberOfVersions(portalId, maxNumberOfVersions + 1);
                    restoreMaxNumberOfVersions = true;
                }

                try
                {
                    var previousVersion = tabVersions.ElementAt(1);
                    var previousVersionDetails = this.GetVersionModulesDetails(tabId, previousVersion.Version).ToArray();
					Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): previousVersion → {PreviousVersion}", tabId, version, previousVersion);
					Logger.LogDebug("ForceDeleteVersion(tabId={TabId},version={Version}): previousVersionDetails → {PreviousVersionDetails}", tabId, version, previousVersionDetails);
                    var versionToDeleteDetails =
                        this.tabVersionDetailController.GetTabVersionDetails(versionToDelete.TabVersionId);

                    foreach (var versionToDeleteDetail in versionToDeleteDetails)
                    {
                        switch (versionToDeleteDetail.Action)
                        {
                            case TabVersionDetailAction.Added:
                                this.moduleController.DeleteTabModule(tabId, versionToDeleteDetail.ModuleId, true);
                                break;
                            case TabVersionDetailAction.Modified:
                                var peviousVersionDetail =
                                    previousVersionDetails.SingleOrDefault(tv => tv.ModuleId == versionToDeleteDetail.ModuleId);
                                if (peviousVersionDetail != null &&
                                    (peviousVersionDetail.PaneName != versionToDeleteDetail.PaneName ||
                                      peviousVersionDetail.ModuleOrder != versionToDeleteDetail.ModuleOrder))
                                {
                                    this.moduleController.UpdateModuleOrder(
                                        tabId,
                                        peviousVersionDetail.ModuleId,
                                        peviousVersionDetail.ModuleOrder,
                                        peviousVersionDetail.PaneName);
                                }

                                if (versionToDeleteDetail.ModuleVersion != Null.NullInteger)
                                {
                                    this.DiscardDetail(tabId, versionToDeleteDetail);
                                }

                                break;
                        }
                    }

                    this.DeleteTmpVersionIfExists(tabId, versionToDelete);
                    this.tabVersionController.DeleteTabVersion(tabId, versionToDelete.TabVersionId);
                    this.ManageModulesToBeRestored(tabId, previousVersionDetails);
                    this.moduleController.ClearCache(tabId);
                }
                finally
                {
                    if (restoreMaxNumberOfVersions)
                    {
                        this.tabVersionSettings.SetMaxNumberOfVersions(portalId, maxNumberOfVersions);
                    }
                }
            }
            else
            {
                for (var i = 1; i < tabVersions.Count(); i++)
                {
                    if (tabVersions.ElementAt(i).Version == version)
                    {
                        this.CreateSnapshotOverVersion(tabId, tabVersions.ElementAtOrDefault(i - 1), tabVersions.ElementAt(i));
                        this.tabVersionController.DeleteTabVersion(tabId, tabVersions.ElementAt(i).TabVersionId);
                        break;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting ForceDeleteVersion(tabId={TabId},version={Version})", tabId, version);
        }

        private void ManageModulesToBeRestored(int tabId, TabVersionDetail[] versionDetails)
        {
			Logger.LogDebug(">>> Entering ManageModulesToBeRestored(tabId={TabId},versionDetails={VersionDetails})", tabId, versionDetails);
            foreach (var detail in versionDetails)
            {
                var module = this.moduleController.GetModule(detail.ModuleId, tabId, true);
                if (module.IsDeleted)
				Logger.LogDebug("ManageModulesToBeRestored(tabId={TabId},versionDetails={VersionDetails}): module → {Module}", tabId, versionDetails, module);
                {
                    this.moduleController.RestoreModule(module);
                }
            }
			Logger.LogDebug("<<< Exiting ManageModulesToBeRestored(tabId={TabId},versionDetails={VersionDetails})", tabId, versionDetails);
        }

        private void DeleteTmpVersionIfExists(int tabId, TabVersion versionToDelete)
        {
            var tmpVersion = this.tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            if (tmpVersion != null && tmpVersion.Version > versionToDelete.Version)
            {
                this.tabVersionController.DeleteTabVersion(tabId, tmpVersion.TabVersionId);
            }
        }

        private void DeleteOldestVersionIfTabHasMaxNumberOfVersions(int portalId, int tabId)
        {
            var maxVersionsAllowed = this.GetMaxNumberOfVersions(portalId);
            var tabVersionsOrdered = this.tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);

            if (tabVersionsOrdered.Count() < maxVersionsAllowed)
            {
                return;
            }

            // The last existing version is going to be deleted, therefore we need to add the snapshot to the previous one
            var snapShotTabVersion = tabVersionsOrdered.ElementAtOrDefault(maxVersionsAllowed - 2);
            this.CreateSnapshotOverVersion(tabId, snapShotTabVersion);
            this.DeleteOldVersions(tabVersionsOrdered, snapShotTabVersion);
			Logger.LogDebug("<<< Exiting DeleteOldestVersionIfTabHasMaxNumberOfVersions(portalId={PortalId},tabId={TabId})", portalId, tabId);
        }

        private int GetMaxNumberOfVersions(int portalId)
        {
            return this.tabVersionSettings.GetMaxNumberOfVersions(portalId);
        }

        private void UpdateModuleOrder(int tabId, TabVersionDetail detailToRestore)
        {
			Logger.LogDebug(">>> Entering UpdateModuleOrder(tabId={TabId},detailToRestore={DetailToRestore})", tabId, detailToRestore);
            var restoredModule = this.moduleController.GetModule(detailToRestore.ModuleId, tabId, true);
            if (restoredModule != null)
			Logger.LogDebug("UpdateModuleOrder(tabId={TabId},detailToRestore={DetailToRestore}): restoredModule → {RestoredModule}", tabId, detailToRestore, restoredModule);
            {
                this.UpdateModuleInfoOrder(restoredModule, detailToRestore);
            }
			Logger.LogDebug("<<< Exiting UpdateModuleOrder(tabId={TabId},detailToRestore={DetailToRestore})", tabId, detailToRestore);
        }

        private void UpdateModuleInfoOrder(ModuleInfo module, TabVersionDetail detailToRestore)
        {
            module.PaneName = detailToRestore.PaneName;
            module.ModuleOrder = detailToRestore.ModuleOrder;
            this.moduleController.UpdateModule(module);
        }

        private void RestoreModuleInfo(int tabId, TabVersionDetail detailsToRestore)
        {
			Logger.LogDebug(">>> Entering RestoreModuleInfo(tabId={TabId},detailsToRestore={DetailsToRestore})", tabId, detailsToRestore);
            var restoredModule = this.moduleController.GetModule(detailsToRestore.ModuleId, tabId, true);
            if (restoredModule != null)
			Logger.LogDebug("RestoreModuleInfo(tabId={TabId},detailsToRestore={DetailsToRestore}): restoredModule → {RestoredModule}", tabId, detailsToRestore, restoredModule);
            {
                this.moduleController.RestoreModule(restoredModule);
                this.UpdateModuleInfoOrder(restoredModule, detailsToRestore);
            }
			Logger.LogDebug("<<< Exiting RestoreModuleInfo(tabId={TabId},detailsToRestore={DetailsToRestore})", tabId, detailsToRestore);
        }

        private List<TabVersionDetail> GetVersionModulesDetails(int tabId, int version)
        {
			Logger.LogDebug(">>> Entering GetVersionModulesDetails(tabId={TabId},version={Version})", tabId, version);
            var tabVersionDetails = this.tabVersionDetailController.GetVersionHistory(tabId, version);
			Logger.LogDebug("GetVersionModulesDetails(tabId={TabId},version={Version}): tabVersionDetails → {TabVersionDetails}", tabId, version, tabVersionDetails);
			Logger.LogDebug("<<< Exiting GetVersionModulesDetails(tabId={TabId},version={Version})", tabId, version);
            return GetSnapShot(tabVersionDetails);
        }

        private TabVersion PublishVersion(int portalId, int tabId, int createdByUserId, TabVersion tabVersion)
        {
            var unPublishedDetails = this.tabVersionDetailController.GetTabVersionDetails(tabVersion.TabVersionId);
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    this.PublishDetail(tabId, unPublishedDetail);
                }
            }

            tabVersion.IsPublished = true;
            this.tabVersionController.SaveTabVersion(tabVersion, tabVersion.CreatedByUserID, createdByUserId);
            var tab = TabController.Instance.GetTab(tabId, portalId);
            if (!tab.HasBeenPublished)
            {
                TabController.Instance.MarkAsPublished(tab);
            }

            this.moduleController.ClearCache(tabId);
            return tabVersion;
        }

        private void CheckVersioningEnabled(int tabId)
        {
			Logger.LogDebug(">>> Entering CheckVersioningEnabled(tabId={TabId})", tabId);
            this.CheckVersioningEnabled(this.GetCurrentPortalId(), tabId);
			Logger.LogDebug("<<< Exiting CheckVersioningEnabled(tabId={TabId})", tabId);
        }

        private void CheckVersioningEnabled(int portalId, int tabId)
        {
            if (portalId == Null.NullInteger || !this.tabVersionSettings.IsVersioningEnabled(portalId, tabId))
            {
                throw new InvalidOperationException(Localization.GetString("TabVersioningNotEnabled", Localization.ExceptionsResourceFile));
            }
			Logger.LogDebug("<<< Exiting CheckVersioningEnabled(portalId={PortalId},tabId={TabId})", portalId, tabId);
        }

        private int GetCurrentPortalId()
        {
			Logger.LogDebug(">>> Entering GetCurrentPortalId()");
			Logger.LogDebug("<<< Exiting GetCurrentPortalId()");
            return this.portalSettings?.PortalId ?? Null.NullInteger;
        }

        private void CreateSnapshotOverVersion(int tabId, TabVersion snapshotTabVersion, TabVersion deletedTabVersion = null)
        {
			Logger.LogDebug(">>> Entering CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion})", tabId, snapshotTabVersion, deletedTabVersion);
            var snapShotTabVersionDetails = this.GetVersionModulesDetails(tabId, snapshotTabVersion.Version).ToArray();
            var existingTabVersionDetails = this.tabVersionDetailController.GetTabVersionDetails(snapshotTabVersion.TabVersionId).ToArray();
			Logger.LogDebug("CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion}): snapShotTabVersionDetails → {SnapShotTabVersionDetails}", tabId, snapshotTabVersion, deletedTabVersion, snapShotTabVersionDetails);
			Logger.LogDebug("CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion}): existingTabVersionDetails → {ExistingTabVersionDetails}", tabId, snapshotTabVersion, deletedTabVersion, existingTabVersionDetails);

            for (var i = existingTabVersionDetails.Length; i > 0; i--)
            {
                var existingDetail = existingTabVersionDetails.ElementAtOrDefault(i - 1);

				Logger.LogDebug("CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion}): existingDetail → {ExistingDetail}", tabId, snapshotTabVersion, deletedTabVersion, existingDetail);
                if (deletedTabVersion == null)
                {
                    if (snapShotTabVersionDetails.All(tvd => tvd.TabVersionDetailId != existingDetail.TabVersionDetailId))
                    {
                        this.tabVersionDetailController.DeleteTabVersionDetail(
                            existingDetail.TabVersionId,
                            existingDetail.TabVersionDetailId);
                    }
                }
                else if (existingDetail.Action == TabVersionDetailAction.Deleted)
                {
                    IEnumerable<TabVersionDetail> deletedTabVersionDetails = this.tabVersionDetailController.GetTabVersionDetails(deletedTabVersion.TabVersionId);
                    var moduleAddedAndDeleted = deletedTabVersionDetails.Any(
                        deleteDetail =>
                            deleteDetail.ModuleId == existingDetail.ModuleId &&
                            deleteDetail.Action == TabVersionDetailAction.Added);
						Logger.LogDebug("CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion}): deletedTabVersionDetails → {DeletedTabVersionDetails}", tabId, snapshotTabVersion, deletedTabVersion, deletedTabVersionDetails);
						Logger.LogDebug("CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion}): deleteDetail → {DeleteDetail}", tabId, snapshotTabVersion, deletedTabVersion, deleteDetail);
                    if (moduleAddedAndDeleted)
                    {
                        this.tabVersionDetailController.DeleteTabVersionDetail(
                            existingDetail.TabVersionId,
                            existingDetail.TabVersionDetailId);
                    }
                }
            }

            this.UpdateDeletedTabDetails(snapshotTabVersion, deletedTabVersion, snapShotTabVersionDetails);
			Logger.LogDebug("<<< Exiting CreateSnapshotOverVersion(tabId={TabId},snapshotTabVersion={SnapshotTabVersion},deletedTabVersion={DeletedTabVersion})", tabId, snapshotTabVersion, deletedTabVersion);
        }

        private void UpdateDeletedTabDetails(TabVersion snapshotTabVersion, TabVersion deletedTabVersion, TabVersionDetail[] snapShotTabVersionDetails)
        {
            var tabVersionDetailsToBeUpdated = deletedTabVersion != null ? this.tabVersionDetailController.GetTabVersionDetails(deletedTabVersion.TabVersionId).ToArray()
                                                                                : snapShotTabVersionDetails;

            foreach (var tabVersionDetail in tabVersionDetailsToBeUpdated)
            {
                var detailInSnapshot =
                    snapShotTabVersionDetails.Any(
                        snapshotDetail => snapshotDetail.TabVersionDetailId == tabVersionDetail.TabVersionDetailId);
                var deleteOrResetAction = tabVersionDetail.Action == TabVersionDetailAction.Deleted || tabVersionDetail.Action == TabVersionDetailAction.Reset;
                if (detailInSnapshot
                    || deleteOrResetAction)
                {
                    var previousTabVersionId = tabVersionDetail.TabVersionId;
                    tabVersionDetail.TabVersionId = snapshotTabVersion.TabVersionId;
                    this.tabVersionDetailController.SaveTabVersionDetail(tabVersionDetail);
                    this.tabVersionDetailController.ClearCache(previousTabVersionId);
                }
            }
        }

        private void DeleteOldVersions(IEnumerable<TabVersion> tabVersionsOrdered, TabVersion snapShotTabVersion)
        {
            var oldVersions = tabVersionsOrdered.Where(tv => tv.Version < snapShotTabVersion.Version).ToArray();
            for (var i = oldVersions.Length; i > 0; i--)
            {
                var oldVersion = oldVersions.ElementAtOrDefault(i - 1);
                var oldVersionDetails = this.tabVersionDetailController.GetTabVersionDetails(oldVersion.TabVersionId).ToArray();
                for (var j = oldVersionDetails.Length; j > 0; j--)
                {
                    var oldVersionDetail = oldVersionDetails.ElementAtOrDefault(j - 1);
                    this.tabVersionDetailController.DeleteTabVersionDetail(oldVersionDetail.TabVersionId, oldVersionDetail.TabVersionDetailId);
                }

                this.tabVersionController.DeleteTabVersion(oldVersion.TabId, oldVersion.TabVersionId);
            }
        }

        private List<ModuleInfo> ConvertToModuleInfo(IEnumerable<TabVersionDetail> details, int tabId)
        {
			Logger.LogDebug(">>> Entering ConvertToModuleInfo(details={Details},tabId={TabId})", details, tabId);
            var modules = new List<ModuleInfo>();
            try
			Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): modules → {Modules}", details, tabId, modules);
            {
                foreach (var detail in details)
                {
                    var module = this.moduleController.GetModule(detail.ModuleId, tabId, false);
                    if (module == null)
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): module → {Module}", details, tabId, module);
                    {
                        continue;
                    }

                    var moduleVersion = this.moduleController.IsSharedModule(module)
                        ? Null.NullInteger
                        : detail.ModuleVersion;
                    var cloneModule = module.Clone();
                    cloneModule.UniqueId = module.UniqueId;
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule → {CloneModule}", details, tabId, cloneModule);
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule.UniqueId → {Arg2}", details, tabId, cloneModule.UniqueId);
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule.VersionGuid → {Arg2}", details, tabId, cloneModule.VersionGuid);
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule.IsDeleted → {Arg2}", details, tabId, cloneModule.IsDeleted);
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule.ModuleVersion → {Arg2}", details, tabId, cloneModule.ModuleVersion);
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule.PaneName → {Arg2}", details, tabId, cloneModule.PaneName);
					Logger.LogDebug("ConvertToModuleInfo(details={Details},tabId={TabId}): cloneModule.ModuleOrder → {Arg2}", details, tabId, cloneModule.ModuleOrder);
                    cloneModule.VersionGuid = module.VersionGuid;
                    cloneModule.IsDeleted = false;
                    cloneModule.ModuleVersion = moduleVersion;
                    cloneModule.PaneName = detail.PaneName;
                    cloneModule.ModuleOrder = detail.ModuleOrder;
                    modules.Add(cloneModule);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

			Logger.LogDebug("<<< Exiting ConvertToModuleInfo(details={Details},tabId={TabId})", details, tabId);
            return modules;
        }

        private int RollBackDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
			Logger.LogDebug(">>> Entering RollBackDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
            var moduleInfo = this.moduleController.GetModule(unPublishedDetail.ModuleId, tabId, true);
            if (moduleInfo == null)
			Logger.LogDebug("RollBackDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail}): moduleInfo → {ModuleInfo}", tabId, unPublishedDetail, moduleInfo);
            {
                return Null.NullInteger;
            }

            var versionableController = this.GetVersionableController(moduleInfo);
            if (versionableController == null)
			Logger.LogDebug("RollBackDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail}): versionableController → {VersionableController}", tabId, unPublishedDetail, versionableController);
            {
                return Null.NullInteger;
            }

            if (this.moduleController.IsSharedModule(moduleInfo))
            {
                return versionableController.GetPublishedVersion(moduleInfo.ModuleID);
            }

			Logger.LogDebug("<<< Exiting RollBackDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
            return versionableController.RollBackVersion(
                unPublishedDetail.ModuleId,
                unPublishedDetail.ModuleVersion);
        }

        private void PublishDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
			Logger.LogDebug(">>> Entering PublishDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
            var moduleInfo = this.moduleController.GetModule(unPublishedDetail.ModuleId, tabId, true);

			Logger.LogDebug("PublishDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail}): moduleInfo → {ModuleInfo}", tabId, unPublishedDetail, moduleInfo);
            if (moduleInfo == null || this.moduleController.IsSharedModule(moduleInfo))
            {
                return;
            }

            var versionableController = this.GetVersionableController(moduleInfo);
            versionableController?.PublishVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
			Logger.LogDebug("PublishDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail}): versionableController → {VersionableController}", tabId, unPublishedDetail, versionableController);
			Logger.LogDebug("<<< Exiting PublishDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
        }

        private void DiscardDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
			Logger.LogDebug(">>> Entering DiscardDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail})", tabId, unPublishedDetail);
            var moduleInfo = this.moduleController.GetModule(unPublishedDetail.ModuleId, tabId, true);

			Logger.LogDebug("DiscardDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail}): moduleInfo → {ModuleInfo}", tabId, unPublishedDetail, moduleInfo);
            if (this.moduleController.IsSharedModule(moduleInfo))
            {
                return;
            }

            var versionableController = this.GetVersionableController(moduleInfo);
            versionableController?.DeleteVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
			Logger.LogDebug("DiscardDetail(tabId={TabId},unPublishedDetail={UnPublishedDetail}): versionableController → {VersionableController}", tabId, unPublishedDetail, versionableController);
        }

        private IVersionable GetVersionableController(ModuleInfo moduleInfo)
        {
            return this.businessControllerProvider.GetInstance<IVersionable>(moduleInfo.DesktopModule.BusinessControllerClass);
        }

        private void CreateFirstTabVersion(int tabId, TabInfo tab, IEnumerable<ModuleInfo> modules)
        {
            var tabVersion = this.tabVersionController.CreateTabVersion(tabId, tab.CreatedByUserID, true);
            foreach (var module in modules)
            {
                var moduleVersion = this.GetModuleContentPublishedVersion(module);
                this.tabVersionDetailController.SaveTabVersionDetail(
                    new TabVersionDetail
                    {
                        Action = TabVersionDetailAction.Added,
                        ModuleId = module.ModuleID,
                        ModuleOrder = module.ModuleOrder,
                        ModuleVersion = moduleVersion,
                        PaneName = module.PaneName,
                        TabVersionId = tabVersion.TabVersionId,
                    },
                    module.CreatedByUserID);
            }
        }

        private int GetModuleContentPublishedVersion(ModuleInfo module)
        {
            var versionableController = this.GetVersionableController(module);
            return versionableController?.GetPublishedVersion(module.ModuleID) ?? Null.NullInteger;
        }
    }
}
