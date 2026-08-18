// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using Newtonsoft.Json;

    /// <summary>The SearchEngine manages the Indexing of the Portal content.</summary>
    internal partial class SearchEngine
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchEngine));
        private readonly IBusinessControllerProvider businessControllerProvider;

        /// <summary>Initializes a new instance of the <see cref="SearchEngine"/> class.</summary>
        /// <param name="scheduler">The scheduler.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        internal SearchEngine(ScheduleHistoryItem scheduler, DateTime startTime, IBusinessControllerProvider businessControllerProvider)
        {
			Logger.LogDebug(">>> Entering SearchEngine(scheduler={Scheduler},startTime={StartTime},businessControllerProvider={BusinessControllerProvider})", scheduler, startTime, businessControllerProvider);
            this.businessControllerProvider = businessControllerProvider;
            this.SchedulerItem = scheduler;
			Logger.LogDebug("SearchEngine(scheduler={Scheduler},startTime={StartTime},businessControllerProvider={BusinessControllerProvider}): this.businessControllerProvider → {Arg3}", scheduler, startTime, businessControllerProvider, this.businessControllerProvider);
			Logger.LogDebug("SearchEngine(scheduler={Scheduler},startTime={StartTime},businessControllerProvider={BusinessControllerProvider}): this.SchedulerItem → {Arg3}", scheduler, startTime, businessControllerProvider, this.SchedulerItem);
            this.IndexingStartTime = startTime;
			Logger.LogDebug("<<< Exiting SearchEngine(scheduler={Scheduler},startTime={StartTime},businessControllerProvider={BusinessControllerProvider})", scheduler, startTime, businessControllerProvider);
        }

        /// <summary>Gets the scheduler item.</summary>
        public ScheduleHistoryItem SchedulerItem { get; private set; }

        /// <summary>Gets the time from where to start indexing items.</summary>
        public DateTime IndexingStartTime { get; private set; }

        /// <summary>Indexes content within the given time frame.</summary>
        internal void IndexContent()
        {
			Logger.LogDebug(">>> Entering IndexContent()");
            // Index TAB META-DATA
            var tabIndexer = new TabIndexer();
			Logger.LogDebug("IndexContent(): tabIndexer → {TabIndexer}", tabIndexer);
            var searchDocsCount = this.GetAndStoreSearchDocuments(tabIndexer);
			Logger.LogDebug("IndexContent(): searchDocsCount → {SearchDocsCount}", searchDocsCount);
            var indexedSearchDocumentCount = searchDocsCount;
            this.AddIndexingResults("Tabs Indexed", searchDocsCount);

            // Index MODULE META-DATA from modules that inherit from ModuleSearchBase
            var moduleIndexer = new ModuleIndexer(true, this.businessControllerProvider);
			Logger.LogDebug("IndexContent(): moduleIndexer → {ModuleIndexer}", moduleIndexer);
            searchDocsCount = this.GetAndStoreModuleMetaData(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;
            this.AddIndexingResults("Modules (Metadata) Indexed", searchDocsCount);

            // Index MODULE CONTENT from modules that inherit from ModuleSearchBase
            searchDocsCount = this.GetAndStoreSearchDocuments(moduleIndexer);
            indexedSearchDocumentCount += searchDocsCount;

            this.AddIndexingResults("Modules (Content) Indexed", searchDocsCount);

            if (!HostController.Instance.GetBoolean("DisableUserCrawling", false))
            {
                // Index User data
                var userIndexer = new UserIndexer();
				Logger.LogDebug("IndexContent(): userIndexer → {UserIndexer}", userIndexer);
                var userIndexed = this.GetAndStoreSearchDocuments(userIndexer);
				Logger.LogDebug("IndexContent(): userIndexed → {UserIndexed}", userIndexed);
                indexedSearchDocumentCount += userIndexed;
                this.AddIndexingResults("Users", userIndexed);
            }

            this.SchedulerItem.AddLogNote("<br/><b>Total Items Indexed: " + indexedSearchDocumentCount + "</b>");
			Logger.LogDebug("<<< Exiting IndexContent()");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool CompactSearchIndexIfNeeded(ScheduleHistoryItem scheduleItem)
        {
			Logger.LogDebug(">>> Entering CompactSearchIndexIfNeeded(scheduleItem={ScheduleItem})", scheduleItem);
            var searchHelper = SearchHelper.Instance;
            if (!searchHelper.GetSearchCompactFlag())
            {
                return false;
            }

            searchHelper.SetSearchReindexRequestTime(false);
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogDebug("CompactSearchIndexIfNeeded(scheduleItem={ScheduleItem}): stopWatch → {StopWatch}", scheduleItem, stopWatch);
            if (!InternalSearchController.Instance.OptimizeSearchIndex())
            {
                return false;
            }

            stopWatch.Stop();
            scheduleItem.AddLogNote($"<br/><b>Compacted Index, total time {stopWatch.Elapsed}</b>");

			Logger.LogDebug("<<< Exiting CompactSearchIndexIfNeeded(scheduleItem={ScheduleItem})", scheduleItem);
            return false;
        }

        /// <summary>Deletes all old documents when re-index was requested, so we start a fresh search.</summary>
        internal void DeleteOldDocsBeforeReindex()
        {
			Logger.LogDebug(">>> Entering DeleteOldDocsBeforeReindex()");
            var controller = InternalSearchController.Instance;
            var moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;
			Logger.LogDebug("DeleteOldDocsBeforeReindex(): moduleSearchTypeId → {ModuleSearchTypeId}", moduleSearchTypeId);
            var tabSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("tab").SearchTypeId;
			Logger.LogDebug("DeleteOldDocsBeforeReindex(): tabSearchTypeId → {TabSearchTypeId}", tabSearchTypeId);
            foreach (var portalId in SearchHelper.Instance.GetPortalsToReindex(this.IndexingStartTime))
            {
                controller.DeleteAllDocuments(portalId, moduleSearchTypeId);
                controller.DeleteAllDocuments(portalId, tabSearchTypeId);
            }
			Logger.LogDebug("<<< Exiting DeleteOldDocsBeforeReindex()");
        }

        /// <summary>Deletes all deleted items from the system that are added to deletions table.</summary>
        internal void DeleteRemovedObjects()
        {
			Logger.LogDebug(">>> Entering DeleteRemovedObjects()");
            var deletedCount = 0;
            var cutoffTime = this.SchedulerItem.StartDate.ToUniversalTime();
			Logger.LogDebug("DeleteRemovedObjects(): cutoffTime → {CutoffTime}", cutoffTime);
            var searchController = InternalSearchController.Instance;
            var dataProvider = DataProvider.Instance();
			Logger.LogDebug("DeleteRemovedObjects(): dataProvider → {DataProvider}", dataProvider);
            using (var reader = dataProvider.GetSearchDeletedItems(cutoffTime))
            {
                while (reader.Read())
                {
                    // Note: we saved this in the DB as SearchDocumentToDelete but retrieve as the descendant SearchDocument class
                    var document = JsonConvert.DeserializeObject<SearchDocument>(reader["document"] as string);
					Logger.LogDebug("DeleteRemovedObjects(): document → {Document}", document);
                    searchController.DeleteSearchDocument(document);
                    deletedCount += 1;
                }

                reader.Close();
            }

            this.AddIndexingResults("Deleted Objects", deletedCount);
            dataProvider.DeleteProcessedSearchDeletedItems(cutoffTime);
			Logger.LogDebug("<<< Exiting DeleteRemovedObjects()");
        }

        /// <summary>Commits (flushes) all added and deleted content to search engine's disk file.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            InternalSearchController.Instance.Commit();
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>Ensures all SearchDocuments have a SearchTypeId.</summary>
        private static void StoreSearchDocuments(IEnumerable<SearchDocument> searchDocs)
        {
			Logger.LogDebug(">>> Entering StoreSearchDocuments(searchDocs={SearchDocs})", searchDocs);
            var defaultSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

			Logger.LogDebug("StoreSearchDocuments(searchDocs={SearchDocs}): defaultSearchTypeId → {DefaultSearchTypeId}", searchDocs, defaultSearchTypeId);
            var searchDocumentList = searchDocs as IList<SearchDocument> ?? searchDocs.ToList();
			Logger.LogDebug("StoreSearchDocuments(searchDocs={SearchDocs}): searchDocumentList → {SearchDocumentList}", searchDocs, searchDocumentList);
            foreach (var searchDocument in searchDocumentList.Where(searchDocument => searchDocument.SearchTypeId <= 0))
            {
                searchDocument.SearchTypeId = defaultSearchTypeId;
				Logger.LogDebug("StoreSearchDocuments(searchDocs={SearchDocs}): searchDocument.SearchTypeId → {Arg1}", searchDocs, searchDocument.SearchTypeId);
            }

            InternalSearchController.Instance.AddSearchDocuments(searchDocumentList);
			Logger.LogDebug("<<< Exiting StoreSearchDocuments(searchDocs={SearchDocs})", searchDocs);
        }

        private void AddIndexingResults(string description, int count)
        {
			Logger.LogDebug(">>> Entering AddIndexingResults(description={Description},count={Count})", description, count);
            this.SchedulerItem.AddLogNote($"<br/>&nbsp;&nbsp;{description}: {count}");
			Logger.LogDebug("<<< Exiting AddIndexingResults(description={Description},count={Count})", description, count);
        }

        /// <summary>Gets all the Search Documents for the given timeframe.</summary>
        private int GetAndStoreSearchDocuments(IndexingProviderBase indexer)
        {
			Logger.LogDebug(">>> Entering GetAndStoreSearchDocuments(indexer={Indexer})", indexer);
            var portals = PortalController.Instance.GetPortals();
            DateTime indexSince;
			Logger.LogDebug("GetAndStoreSearchDocuments(indexer={Indexer}): portals → {Portals}", indexer, portals);
            var indexedCount = 0;

            foreach (var portal in portals.Cast<IPortalInfo>())
            {
                indexSince = this.FixedIndexingStartDate(portal.PortalId);
                try
				Logger.LogDebug("GetAndStoreSearchDocuments(indexer={Indexer}): indexSince → {IndexSince}", indexer, indexSince);
                {
                    indexedCount += indexer.IndexSearchDocuments(
                        portal.PortalId, this.SchedulerItem, indexSince, StoreSearchDocuments);
                }
                catch (NotImplementedException exc)
                {
                    Logger.Warn("Indexer not implemented", exc);
                }
            }

            // Include Host Level Items
            indexSince = this.FixedIndexingStartDate(-1);
            try
            {
                indexedCount += indexer.IndexSearchDocuments(
                    Null.NullInteger, this.SchedulerItem, indexSince, StoreSearchDocuments);
            }
            catch (NotImplementedException exc)
            {
                Logger.Warn("Indexer not implemented", exc);
            }

			Logger.LogDebug("<<< Exiting GetAndStoreSearchDocuments(indexer={Indexer})", indexer);
            return indexedCount;
        }

        /// <summary>Gets all the Searchable Module MetaData SearchDocuments within the timeframe for all portals.</summary>
        private int GetAndStoreModuleMetaData(ModuleIndexer indexer)
        {
			Logger.LogDebug(">>> Entering GetAndStoreModuleMetaData(indexer={Indexer})", indexer);
            IEnumerable<SearchDocument> searchDocs;
            var portals = PortalController.Instance.GetPortals();
			Logger.LogDebug("GetAndStoreModuleMetaData(indexer={Indexer}): portals → {Portals}", indexer, portals);
            DateTime indexSince;
            var indexedCount = 0;

            // DateTime startDate
            foreach (var portal in portals.Cast<IPortalInfo>())
            {
                indexSince = this.FixedIndexingStartDate(portal.PortalId);
                searchDocs = indexer.GetModuleMetaData(portal.PortalId, indexSince);
                StoreSearchDocuments(searchDocs);
				Logger.LogDebug("GetAndStoreModuleMetaData(indexer={Indexer}): indexSince → {IndexSince}", indexer, indexSince);
				Logger.LogDebug("GetAndStoreModuleMetaData(indexer={Indexer}): searchDocs → {SearchDocs}", indexer, searchDocs);
                indexedCount += searchDocs.Count();
            }

            // Include Host Level Items
            indexSince = this.FixedIndexingStartDate(Null.NullInteger);
            searchDocs = indexer.GetModuleMetaData(Null.NullInteger, indexSince);
            StoreSearchDocuments(searchDocs);
            indexedCount += searchDocs.Count();

			Logger.LogDebug("<<< Exiting GetAndStoreModuleMetaData(indexer={Indexer})", indexer);
            return indexedCount;
        }

        /// <summary>Adjusts the re-index date/time to account for the portal reindex value.</summary>
        private DateTime FixedIndexingStartDate(int portalId)
        {
			Logger.LogDebug(">>> Entering FixedIndexingStartDate(portalId={PortalId})", portalId);
            var startDate = this.IndexingStartTime;
            if (startDate < SqlDateTime.MinValue.Value || SearchHelper.Instance.IsReindexRequested(portalId, startDate))
            {
			Logger.LogDebug("<<< Exiting FixedIndexingStartDate(portalId={PortalId})", portalId);
                return SqlDateTime.MinValue.Value.AddDays(1);
            }

            return startDate;
        }
    }
}
