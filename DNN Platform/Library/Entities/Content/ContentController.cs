// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A <see cref="IContentController"/> implementation.</summary>
    /// <param name="dataService">The data service.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class ContentController(IDataService dataService, IHostSettings hostSettings)
        : ServiceLocator<IContentController, ContentController>, IContentController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentController));
        private readonly IDataService dataService = dataService ?? Util.GetDataService();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="ContentController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ContentController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering ContentController()");
		Logger.LogDebug("<<< Exiting ContentController()");
        }

        /// <summary>Initializes a new instance of the <see cref="ContentController"/> class.</summary>
        /// <param name="dataService">The data service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ContentController(IDataService dataService)
            : this(dataService, null)
        {
        }

        /// <inheritdoc />
        public int AddContentItem(ContentItem contentItem)
        {
			Logger.LogDebug(">>> Entering AddContentItem(contentItem={ContentItem})", contentItem);
            // Argument Contract
            Requires.NotNull("contentItem", contentItem);
            var currentUser = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("AddContentItem(contentItem={ContentItem}): currentUser → {CurrentUser}", contentItem, currentUser);
            var createdByUserId = currentUser.UserID;
			Logger.LogDebug("AddContentItem(contentItem={ContentItem}): createdByUserId → {CreatedByUserId}", contentItem, createdByUserId);
            if (contentItem.CreatedByUserID >= 0 && contentItem.CreatedByUserID != currentUser.UserID)
            {
                createdByUserId = contentItem.CreatedByUserID;
            }

            contentItem.ContentItemId = this.dataService.AddContentItem(contentItem, createdByUserId);
            contentItem.CreatedByUserID = createdByUserId;
			Logger.LogDebug("AddContentItem(contentItem={ContentItem}): contentItem.CreatedByUserID → {Arg1}", contentItem, contentItem.CreatedByUserID);
			Logger.LogDebug("AddContentItem(contentItem={ContentItem}): contentItem.LastModifiedByUserID → {Arg1}", contentItem, contentItem.LastModifiedByUserID);
            contentItem.LastModifiedByUserID = currentUser.UserID;

            this.SaveMetadataDelta(contentItem);

            UpdateContentItemsCache(contentItem);

			Logger.LogDebug("<<< Exiting AddContentItem(contentItem={ContentItem})", contentItem);
            return contentItem.ContentItemId;
        }

        /// <inheritdoc />
        public void DeleteContentItem(ContentItem contentItem)
        {
			Logger.LogDebug(">>> Entering DeleteContentItem(contentItem={ContentItem})", contentItem);
            // Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);

            var searchDoc = new SearchDocumentToDelete
            {
                UniqueKey = contentItem.ContentItemId.ToString("D", CultureInfo.InvariantCulture),
                ModuleId = contentItem.ModuleID,
                TabId = contentItem.TabID,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
            };
            DotNetNuke.Data.DataProvider.Instance().AddSearchDeletedItems(searchDoc);

            this.dataService.DeleteContentItem(contentItem.ContentItemId);

            UpdateContentItemsCache(contentItem, false);
			Logger.LogDebug("<<< Exiting DeleteContentItem(contentItem={ContentItem})", contentItem);
        }

        /// <inheritdoc />
        public void DeleteContentItem(int contentItemId)
        {
			Logger.LogDebug(">>> Entering DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
            var contentItem = this.GetContentItem(contentItemId);
            this.DeleteContentItem(contentItem);
			Logger.LogDebug("DeleteContentItem(contentItemId={ContentItemId}): contentItem → {ContentItem}", contentItemId, contentItem);
			Logger.LogDebug("<<< Exiting DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
        }

        /// <inheritdoc />
        public ContentItem GetContentItem(int contentItemId)
        {
			Logger.LogDebug(">>> Entering GetContentItem(contentItemId={ContentItemId})", contentItemId);
            // Argument Contract
            Requires.NotNegative("contentItemId", contentItemId);

			Logger.LogDebug("<<< Exiting GetContentItem(contentItemId={ContentItemId})", contentItemId);
            return CBO.GetCachedObject<ContentItem>(
                this.hostSettings,
                new CacheItemArgs(GetContentItemCacheKey(contentItemId), DataCache.ContentItemsCacheTimeOut, DataCache.ContentItemsCachePriority),
                _ => CBO.FillObject<ContentItem>(this.dataService.GetContentItem(contentItemId)));
        }

        public IQueryable<ContentItem> GetContentItems(int contentTypeId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering GetContentItems(contentTypeId={ContentTypeId},tabId={TabId},moduleId={ModuleId})", contentTypeId, tabId, moduleId);
			Logger.LogDebug("<<< Exiting GetContentItems(contentTypeId={ContentTypeId},tabId={TabId},moduleId={ModuleId})", contentTypeId, tabId, moduleId);
            return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItems(contentTypeId, tabId, moduleId));
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByTerm(string term)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByTerm(term={Term})", term);
            // Argument Contract
            Requires.NotNullOrEmpty("term", term);

			Logger.LogDebug("<<< Exiting GetContentItemsByTerm(term={Term})", term);
            return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByTerm(term));
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByTerm(Term term)
        {
            return this.GetContentItemsByTerm(term.Name);
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByContentType(int contentTypeId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByContentType(contentTypeId={ContentTypeId})", contentTypeId);
			Logger.LogDebug("<<< Exiting GetContentItemsByContentType(contentTypeId={ContentTypeId})", contentTypeId);
			Logger.LogDebug("<<< Exiting GetContentItemsByContentType(contentType={ContentType})", contentType);
            return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByContentType(contentTypeId));
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByContentType(ContentType contentType)
        {
            return this.GetContentItemsByContentType(contentType.ContentTypeId);
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByTerms(IList<Term> terms)
        {
			Logger.LogDebug("<<< Exiting GetContentItemsByTerms(terms={Terms})", terms);
            return this.GetContentItemsByTerms(terms.Select(t => t.Name).ToArray());
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByTerms(string[] terms)
        {
            var union = new List<ContentItem>();

            union = terms.Aggregate(
                union,
                (current, term) =>
                    current.Count == 0
                        ? this.GetContentItemsByTerm(term).ToList()
                        : current.Intersect(this.GetContentItemsByTerm(term), new ContentItemEqualityComparer()).ToList());

            return union.AsQueryable();
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByTabId(int tabId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByTabId(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetContentItemsByTabId(tabId={TabId})", tabId);
            return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByTabId(tabId));
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByVocabularyId(int vocabularyId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByVocabularyId(vocabularyId={VocabularyId})", vocabularyId);
			Logger.LogDebug("<<< Exiting GetContentItemsByVocabularyId(vocabularyId={VocabularyId})", vocabularyId);
            return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByVocabularyId(vocabularyId));
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetUnIndexedContentItems()
        {
			Logger.LogDebug(">>> Entering GetUnIndexedContentItems()");
			Logger.LogDebug("<<< Exiting GetUnIndexedContentItems()");
            return CBO.FillQueryable<ContentItem>(this.dataService.GetUnIndexedContentItems());
        }

        /// <inheritdoc />
        public IQueryable<ContentItem> GetContentItemsByModuleId(int moduleId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByModuleId(moduleId={ModuleId})", moduleId);
			Logger.LogDebug("<<< Exiting GetContentItemsByModuleId(moduleId={ModuleId})", moduleId);
            return CBO.FillQueryable<ContentItem>(this.dataService.GetContentItemsByModuleId(moduleId));
        }

        /// <inheritdoc />
        public void UpdateContentItem(ContentItem contentItem)
        {
			Logger.LogDebug(">>> Entering UpdateContentItem(contentItem={ContentItem})", contentItem);
            // Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);

            AttachmentController.SerializeAttachmentMetadata(contentItem);

            this.SaveMetadataDelta(contentItem);

            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
			Logger.LogDebug("UpdateContentItem(contentItem={ContentItem}): userId → {UserId}", contentItem, userId);
            this.dataService.UpdateContentItem(contentItem, userId);
            contentItem.LastModifiedByUserID = userId;
			Logger.LogDebug("UpdateContentItem(contentItem={ContentItem}): contentItem.LastModifiedByUserID → {Arg1}", contentItem, contentItem.LastModifiedByUserID);

            UpdateContentItemsCache(contentItem);
			Logger.LogDebug("<<< Exiting UpdateContentItem(contentItem={ContentItem})", contentItem);
        }

        /// <inheritdoc />
        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
			Logger.LogDebug(">>> Entering AddMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
            // Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
            Requires.NotNullOrEmpty("name", name);

            this.dataService.AddMetaData(contentItem, name, value);

            UpdateContentItemsCache(contentItem, false);
			Logger.LogDebug("<<< Exiting AddMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
        }

        /// <inheritdoc />
        public void DeleteMetaData(ContentItem contentItem, string name, string value)
        {
			Logger.LogDebug(">>> Entering DeleteMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
            // Argument Contract
            Requires.NotNull("contentItem", contentItem);
            Requires.PropertyNotNegative("contentItem", "ContentItemId", contentItem.ContentItemId);
            Requires.NotNullOrEmpty("name", name);

            this.dataService.DeleteMetaData(contentItem, name, value);

            UpdateContentItemsCache(contentItem, false);
			Logger.LogDebug("<<< Exiting DeleteMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
			Logger.LogDebug(">>> Entering DeleteMetaData(contentItem={ContentItem},name={Name})", contentItem, name);
			Logger.LogDebug("<<< Exiting DeleteMetaData(contentItem={ContentItem},name={Name})", contentItem, name);
        }

        /// <inheritdoc />
        public void DeleteMetaData(ContentItem contentItem, string name)
        {
            if (contentItem.Metadata.AllKeys.Contains(name))
            {
                this.DeleteMetaData(contentItem, name, contentItem.Metadata[name]);
            }
        }

        /// <inheritdoc />
        public NameValueCollection GetMetaData(int contentItemId)
        {
			Logger.LogDebug(">>> Entering GetMetaData(contentItemId={ContentItemId})", contentItemId);
            // Argument Contract
            Requires.NotNegative("contentItemId", contentItemId);

            var metadata = new NameValueCollection();
			Logger.LogDebug("GetMetaData(contentItemId={ContentItemId}): metadata → {Metadata}", contentItemId, metadata);

            using (var dr = this.dataService.GetMetaData(contentItemId))
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        metadata.Add(dr.GetString(0), dr.GetString(1));
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetMetaData(contentItemId={ContentItemId})", contentItemId);
            return metadata;
        }

        /// <inheritdoc />
        protected override Func<IContentController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IContentController>();
        }

        private static bool MetaDataChanged(
            IEnumerable<KeyValuePair<string, string>> lh,
            IEnumerable<KeyValuePair<string, string>> rh)
        {
			Logger.LogDebug(">>> Entering MetaDataChanged(lh={Lh},rh={Rh})", lh, rh);
			Logger.LogDebug("<<< Exiting MetaDataChanged(lh={Lh},rh={Rh})", lh, rh);
            return lh.SequenceEqual(rh, new NameValueEqualityComparer()) == false;
        }

        private static void UpdateContentItemsCache(ContentItem contentItem, bool readdItem = true)
        {
			Logger.LogDebug(">>> Entering UpdateContentItemsCache(contentItem={ContentItem},readdItem={ReaddItem})", contentItem, readdItem);
            DataCache.RemoveCache(GetContentItemCacheKey(contentItem.ContentItemId)); // remove first to synch web-farm servers
            if (readdItem)
            {
                CBO.Instance.GetCachedObject<ContentItem>(
                    new CacheItemArgs(
                        GetContentItemCacheKey(contentItem.ContentItemId),
                        DataCache.ContentItemsCacheTimeOut,
                        DataCache.ContentItemsCachePriority),
                    c => contentItem,
                    false);
            }
			Logger.LogDebug("<<< Exiting UpdateContentItemsCache(contentItem={ContentItem},readdItem={ReaddItem})", contentItem, readdItem);
        }

        private static string GetContentItemCacheKey(int contetnItemId)
        {
			Logger.LogDebug(">>> Entering GetContentItemCacheKey(contetnItemId={ContetnItemId})", contetnItemId);
			Logger.LogDebug("<<< Exiting GetContentItemCacheKey(contetnItemId={ContetnItemId})", contetnItemId);
            return string.Format(CultureInfo.InvariantCulture, DataCache.ContentItemsCacheKey, contetnItemId);
        }

        private void SaveMetadataDelta(ContentItem contentItem)
        {
			Logger.LogDebug(">>> Entering SaveMetadataDelta(contentItem={ContentItem})", contentItem);
            var persisted = this.GetMetaData(contentItem.ContentItemId);

			Logger.LogDebug("SaveMetadataDelta(contentItem={ContentItem}): persisted → {Persisted}", contentItem, persisted);
            var lh = persisted.AllKeys.ToDictionary(x => x, x => persisted[x]);
			Logger.LogDebug("SaveMetadataDelta(contentItem={ContentItem}): lh → {Lh}", contentItem, lh);

            var rh = contentItem.Metadata.AllKeys.ToDictionary(x => x, x => contentItem.Metadata[x]);
			Logger.LogDebug("SaveMetadataDelta(contentItem={ContentItem}): rh → {Rh}", contentItem, rh);

            if (!MetaDataChanged(lh, rh))
            {
                return;
            }

            // Items in the database but missing from the parameter (have been deleted).
            var deleted = lh.Except(rh).ToArray();
			Logger.LogDebug("SaveMetadataDelta(contentItem={ContentItem}): deleted → {Deleted}", contentItem, deleted);

            // Items included in the object but missing from the database (newly added).
            var added = rh.Except(lh).ToArray();
			Logger.LogDebug("SaveMetadataDelta(contentItem={ContentItem}): added → {Added}", contentItem, added);

            this.dataService.SynchronizeMetaData(contentItem, added, deleted);

            UpdateContentItemsCache(contentItem, false);
			Logger.LogDebug("<<< Exiting SaveMetadataDelta(contentItem={ContentItem})", contentItem);
        }
    }
}
