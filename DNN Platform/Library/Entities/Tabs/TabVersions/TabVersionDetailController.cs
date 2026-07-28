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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITabVersionDetailController"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class TabVersionDetailController(IHostSettings hostSettings)
        : ServiceLocator<ITabVersionDetailController, TabVersionDetailController>, ITabVersionDetailController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly DataProvider Provider = DataProvider.Instance();
		Logger.LogDebug(">>> Entering TabVersionDetailController(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("TabVersionDetailController(hostSettings={HostSettings}): Provider → {Provider}", hostSettings, Provider);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

		Logger.LogDebug("TabVersionDetailController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        /// <summary>Initializes a new instance of the <see cref="TabVersionDetailController"/> class.</summary>
		Logger.LogDebug("<<< Exiting TabVersionDetailController(hostSettings={HostSettings})", hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public TabVersionDetailController()
            : this(null)
        {
        }

        /// <inheritdoc />
        public TabVersionDetail GetTabVersionDetail(int tabVersionDetailId, int tabVersionId, bool ignoreCache = false)
        {
			Logger.LogDebug(">>> Entering GetTabVersionDetail(tabVersionDetailId={TabVersionDetailId},tabVersionId={TabVersionId},ignoreCache={IgnoreCache})", tabVersionDetailId, tabVersionId, ignoreCache);
			Logger.LogDebug("<<< Exiting GetTabVersionDetail(tabVersionDetailId={TabVersionDetailId},tabVersionId={TabVersionId},ignoreCache={IgnoreCache})", tabVersionDetailId, tabVersionId, ignoreCache);
            return this.GetTabVersionDetails(tabVersionId, ignoreCache).SingleOrDefault(tvd => tvd.TabVersionDetailId == tabVersionDetailId);
        }

        /// <inheritdoc />
        public IEnumerable<TabVersionDetail> GetTabVersionDetails(int tabVersionId, bool ignoreCache = false)
        {
			Logger.LogDebug(">>> Entering GetTabVersionDetails(tabVersionId={TabVersionId},ignoreCache={IgnoreCache})", tabVersionId, ignoreCache);
            // if we are not using the cache
            if (ignoreCache || this.hostSettings.PerformanceSetting == PerformanceSettings.NoCaching)
            {
                return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId));
            }

			Logger.LogDebug("<<< Exiting GetTabVersionDetails(tabVersionId={TabVersionId},ignoreCache={IgnoreCache})", tabVersionId, ignoreCache);
            return CBO.GetCachedObject<List<TabVersionDetail>>(
                this.hostSettings,
                new CacheItemArgs(GetTabVersionDetailCacheKey(tabVersionId), DataCache.TabVersionDetailsCacheTimeOut, DataCache.TabVersionDetailsCachePriority),
                _ => CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetails(tabVersionId)));
        }

        /// <inheritdoc />
        public IEnumerable<TabVersionDetail> GetVersionHistory(int tabId, int version)
        {
			Logger.LogDebug(">>> Entering GetVersionHistory(tabId={TabId},version={Version})", tabId, version);
			Logger.LogDebug("<<< Exiting GetVersionHistory(tabId={TabId},version={Version})", tabId, version);
            return CBO.FillCollection<TabVersionDetail>(Provider.GetTabVersionDetailsHistory(tabId, version));
        }

        /// <inheritdoc />
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail)
        {
			Logger.LogDebug(">>> Entering SaveTabVersionDetail(tabVersionDetail={TabVersionDetail})", tabVersionDetail);
            this.SaveTabVersionDetail(tabVersionDetail, tabVersionDetail.CreatedByUserID, tabVersionDetail.LastModifiedByUserID);
			Logger.LogDebug("<<< Exiting SaveTabVersionDetail(tabVersionDetail={TabVersionDetail})", tabVersionDetail);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID)
        {
			Logger.LogDebug(">>> Entering SaveTabVersionDetail(tabVersionDetail={TabVersionDetail},createdByUserID={CreatedByUserID})", tabVersionDetail, createdByUserID);
            this.SaveTabVersionDetail(tabVersionDetail, createdByUserID, createdByUserID);
			Logger.LogDebug("<<< Exiting SaveTabVersionDetail(tabVersionDetail={TabVersionDetail},createdByUserID={CreatedByUserID})", tabVersionDetail, createdByUserID);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserID, int modifiedByUserID)
        {
			Logger.LogDebug(">>> Entering SaveTabVersionDetail(tabVersionDetail={TabVersionDetail},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersionDetail, createdByUserID, modifiedByUserID);
            tabVersionDetail.TabVersionDetailId = Provider.SaveTabVersionDetail(
                tabVersionDetail.TabVersionDetailId,
                tabVersionDetail.TabVersionId,
                tabVersionDetail.ModuleId,
                tabVersionDetail.ModuleVersion,
                tabVersionDetail.PaneName,
                tabVersionDetail.ModuleOrder,
                (int)tabVersionDetail.Action,
                createdByUserID,
                modifiedByUserID);
            this.ClearCache(tabVersionDetail.TabVersionId);
			Logger.LogDebug("<<< Exiting SaveTabVersionDetail(tabVersionDetail={TabVersionDetail},createdByUserID={CreatedByUserID},modifiedByUserID={ModifiedByUserID})", tabVersionDetail, createdByUserID, modifiedByUserID);
        }

        /// <inheritdoc />
        public void DeleteTabVersionDetail(int tabVersionId, int tabVersionDetailId)
        {
			Logger.LogDebug(">>> Entering DeleteTabVersionDetail(tabVersionId={TabVersionId},tabVersionDetailId={TabVersionDetailId})", tabVersionId, tabVersionDetailId);
            Provider.DeleteTabVersionDetail(tabVersionDetailId);
            this.ClearCache(tabVersionId);
			Logger.LogDebug("<<< Exiting DeleteTabVersionDetail(tabVersionId={TabVersionId},tabVersionDetailId={TabVersionDetailId})", tabVersionId, tabVersionDetailId);
        }

        /// <inheritdoc />
        public void ClearCache(int tabVersionId)
        {
			Logger.LogDebug(">>> Entering ClearCache(tabVersionId={TabVersionId})", tabVersionId);
            DataCache.RemoveCache(GetTabVersionDetailCacheKey(tabVersionId));
			Logger.LogDebug("<<< Exiting ClearCache(tabVersionId={TabVersionId})", tabVersionId);
        }

        /// <inheritdoc />
        protected override System.Func<ITabVersionDetailController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new TabVersionDetailController();
        }

        private static string GetTabVersionDetailCacheKey(int tabVersionId)
        {
			Logger.LogDebug(">>> Entering GetTabVersionDetailCacheKey(tabVersionId={TabVersionId})", tabVersionId);
			Logger.LogDebug("<<< Exiting GetTabVersionDetailCacheKey(tabVersionId={TabVersionId})", tabVersionId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.TabVersionDetailsCacheKey, tabVersionId);
        }
    }
}
