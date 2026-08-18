// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class TabVersionController : ServiceLocator<ITabVersionController, TabVersionController>, ITabVersionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabVersionController));
        private static readonly DataProvider Provider = DataProvider.Instance();

        /// <inheritdoc />
        public TabVersion GetTabVersion(int tabVersionId, int tabId, bool ignoreCache = false)
        {
            return this.GetTabVersions(tabId, ignoreCache).SingleOrDefault(tv => tv.TabVersionId == tabVersionId);
        }

        /// <inheritdoc />
        public IEnumerable<TabVersion> GetTabVersions(int tabId, bool ignoreCache = false)
        {
			Logger.LogDebug(">>> Entering GetTabVersions(tabId={TabId},ignoreCache={IgnoreCache})", tabId, ignoreCache);
            // if we are not using the cache, then remove from cache and re-add loaded items when needed later
            var tabCacheKey = GetTabVersionsCacheKey(tabId);
			Logger.LogDebug("GetTabVersions(tabId={TabId},ignoreCache={IgnoreCache}): tabCacheKey → {TabCacheKey}", tabId, ignoreCache, tabCacheKey);
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                DataCache.RemoveCache(tabCacheKey);
            }

            var tabVersions = CBO.Instance.GetCachedObject<List<TabVersion>>(
                new CacheItemArgs(
                tabCacheKey,
                DataCache.TabVersionsCacheTimeOut,
                DataCache.TabVersionsCachePriority),
                c => CBO.FillCollection<TabVersion>(Provider.GetTabVersions(tabId)),
                false);

			Logger.LogDebug("<<< Exiting GetTabVersions(tabId={TabId},ignoreCache={IgnoreCache})", tabId, ignoreCache);
            return tabVersions;
        }

        /// <inheritdoc />
        public void SaveTabVersion(TabVersion tabVersion)
        {
			Logger.LogDebug(">>> Entering SaveTabVersion(tabVersion={TabVersion})", tabVersion);
            this.SaveTabVersion(tabVersion, tabVersion.CreatedByUserID, tabVersion.LastModifiedByUserID);
			Logger.LogDebug("<<< Exiting SaveTabVersion(tabVersion={TabVersion})", tabVersion);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveTabVersion(TabVersion tabVersion, int createdByUserID)
        {
			Logger.LogDebug(">>> Entering SaveTabVersion(tabVersion={TabVersion},createdByUserID={CreatedByUserID})", tabVersion, createdByUserID);
            this.SaveTabVersion(tabVersion, createdByUserID, createdByUserID);
			Logger.LogDebug("<<< Exiting SaveTabVersion(tabVersion={TabVersion},createdByUserID={CreatedByUserID})", tabVersion, createdByUserID);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveTabVersion(TabVersion tabVersion, int createdByUserID, int modifiedByUserID)
        {
			Logger.LogDebug(">>> Entering SaveTabVersion(tabVersion={TabVersion},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersion, createdByUserID, modifiedByUserID);
            tabVersion.TabVersionId = Provider.SaveTabVersion(tabVersion.TabVersionId, tabVersion.TabId, tabVersion.TimeStamp, tabVersion.Version, tabVersion.IsPublished, createdByUserID, modifiedByUserID);
            ClearCache(tabVersion.TabId);
			Logger.LogDebug("<<< Exiting SaveTabVersion(tabVersion={TabVersion},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersion, createdByUserID, modifiedByUserID);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public TabVersion CreateTabVersion(int tabId, int createdByUserID, bool isPublished = false)
        {
			Logger.LogDebug(">>> Entering CreateTabVersion(tabId={TabId},createdByUserID={CreatedByUserID},isPublished={IsPublished})", tabId, createdByUserID, isPublished);
            var lastTabVersion = this.GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            var newVersion = 1;
			Logger.LogDebug("CreateTabVersion(tabId={TabId},createdByUserID={CreatedByUserID},isPublished={IsPublished}): newVersion → {NewVersion}", tabId, createdByUserID, isPublished, newVersion);
			Logger.LogDebug("CreateTabVersion(tabId={TabId},createdByUserID={CreatedByUserID},isPublished={IsPublished}): lastTabVersion → {LastTabVersion}", tabId, createdByUserID, isPublished, lastTabVersion);

            if (lastTabVersion != null)
            {
                if (!lastTabVersion.IsPublished && !isPublished)
                {
					Logger.LogWarning("Throwing InvalidOperationException from CreateTabVersion(tabId={TabId},createdByUserID={CreatedByUserID},isPublished={IsPublished})", tabId, createdByUserID, isPublished);
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Localization.GetString("TabVersionCannotBeCreated_UnpublishedVersionAlreadyExists", Localization.ExceptionsResourceFile)));
                }

                newVersion = lastTabVersion.Version + 1;
            }

            var tabVersionId = Provider.SaveTabVersion(0, tabId, DateTime.UtcNow, newVersion, isPublished, createdByUserID, createdByUserID);
			Logger.LogDebug("CreateTabVersion(tabId={TabId},createdByUserID={CreatedByUserID},isPublished={IsPublished}): tabVersionId → {TabVersionId}", tabId, createdByUserID, isPublished, tabVersionId);
            ClearCache(tabId);

			Logger.LogDebug("<<< Exiting CreateTabVersion(tabId={TabId},createdByUserID={CreatedByUserID},isPublished={IsPublished})", tabId, createdByUserID, isPublished);
            return this.GetTabVersion(tabVersionId, tabId);
        }

        /// <inheritdoc />
        public void DeleteTabVersion(int tabId, int tabVersionId)
        {
            Provider.DeleteTabVersion(tabVersionId);
            ClearCache(tabId);
			Logger.LogDebug("<<< Exiting DeleteTabVersion(tabId={TabId},tabVersionId={TabVersionId})", tabId, tabVersionId);
        }

        /// <inheritdoc />
        public void DeleteTabVersionDetailByModule(int moduleId)
        {
			Logger.LogDebug(">>> Entering DeleteTabVersionDetailByModule(moduleId={ModuleId})", moduleId);
            Provider.DeleteTabVersionDetailByModule(moduleId);
			Logger.LogDebug("<<< Exiting DeleteTabVersionDetailByModule(moduleId={ModuleId})", moduleId);
        }

        /// <inheritdoc />
        protected override Func<ITabVersionController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new TabVersionController();
        }

        private static string GetTabVersionsCacheKey(int tabId)
        {
			Logger.LogDebug(">>> Entering GetTabVersionsCacheKey(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetTabVersionsCacheKey(tabId={TabId})", tabId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.TabVersionsCacheKey, tabId);
        }

        private static void ClearCache(int tabId)
        {
			Logger.LogDebug(">>> Entering ClearCache(tabId={TabId})", tabId);
            DataCache.RemoveCache(GetTabVersionsCacheKey(tabId));
			Logger.LogDebug("<<< Exiting ClearCache(tabId={TabId})", tabId);
        }
    }
}
