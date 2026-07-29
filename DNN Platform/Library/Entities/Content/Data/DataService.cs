// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Data
{
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>Persistent data of content with DataProvider instance.</summary>
    /// <remarks>It's better to request <see cref="IDataService"/> via Dependency Injection instead of creating a new instance directly.</remarks>
    /// <example>
    /// <code lang="C#">
    /// public ContentController() : this(Util.GetDataService())
    /// {
    /// }
    /// public ContentController(IDataService dataService)
    /// {
    ///     _dataService = dataService;
    /// }
    /// </code>
    /// </example>
    public class DataService : IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly DataProvider provider = DataProvider.Instance();

        /// <summary>Adds the content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>content item id.</returns>
        public int AddContentItem(ContentItem contentItem, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering AddContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
			Logger.LogDebug("<<< Exiting AddContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
            return this.provider.ExecuteScalar<int>(
                "AddContentItem",
                contentItem.Content,
                contentItem.ContentTypeId,
                contentItem.TabID,
                contentItem.ModuleID,
                contentItem.ContentKey,
                contentItem.Indexed,
                createdByUserId,
                this.provider.GetNull(contentItem.StateID));
        }

        /// <summary>Deletes the content item.</summary>
        /// <param name="contentItemId">The content item ID.</param>
        public void DeleteContentItem(int contentItemId)
        {
			Logger.LogDebug(">>> Entering DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteContentItem", contentItemId);
			Logger.LogDebug("<<< Exiting DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteContentItem(contentItemId)={ContentItemId}: database query executed in {Arg1} ms", contentItemId, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteContentItem(contentItemId)={ContentItemId}: database query executed in {Arg1} ms", contentItemId, __logStart.ElapsedMilliseconds);
        /// <summary>Gets the content item.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItem(int contentItemId)
        {
			Logger.LogDebug(">>> Entering GetContentItem(contentItemId={ContentItemId})", contentItemId);
			Logger.LogDebug("<<< Exiting GetContentItem(contentItemId={ContentItemId})", contentItemId);
            return this.provider.ExecuteReader("GetContentItem", contentItemId);
        }

        /// <summary>Gets the content items.</summary>
        /// <param name="contentTypeId">The Id of the Content Type.</param>
        /// <param name="tabId">The Id of the Tab.</param>
        /// <param name="moduleId">The Id of the Module.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItems(int contentTypeId, int tabId, int moduleId)
        {
			Logger.LogDebug(">>> Entering GetContentItems(contentTypeId={ContentTypeId},tabId={TabId},moduleId={ModuleId})", contentTypeId, tabId, moduleId);
			Logger.LogDebug("<<< Exiting GetContentItems(contentTypeId={ContentTypeId},tabId={TabId},moduleId={ModuleId})", contentTypeId, tabId, moduleId);
            return this.provider.ExecuteReader(
                "GetContentItems",
                this.provider.GetNull(contentTypeId),
                this.provider.GetNull(tabId),
                this.provider.GetNull(moduleId));
        }

        /// <summary>Gets the content items by term.</summary>
        /// <param name="term">The term.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItemsByTerm(string term)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByTerm(term={Term})", term);
			Logger.LogDebug("<<< Exiting GetContentItemsByTerm(term={Term})", term);
            return this.provider.ExecuteReader("GetContentItemsByTerm", term);
        }

        /// <summary>Get a list of content items of the specified content type, <paramref name="contentTypeId"/>.</summary>
        /// <param name="contentTypeId">The type of content items you are searching for.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByContentType(int contentTypeId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByContentType(contentTypeId={ContentTypeId})", contentTypeId);
			Logger.LogDebug("<<< Exiting GetContentItemsByContentType(contentTypeId={ContentTypeId})", contentTypeId);
            return this.provider.ExecuteReader("GetContentItemsByContentType", contentTypeId);
        }

        /// <summary>Get a list of content items based on TabID (PageID).</summary>
        /// <param name="tabId">The TabID (or "Page ID") that the content items are associated with.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByTabId(int tabId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByTabId(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetContentItemsByTabId(tabId={TabId})", tabId);
            return this.provider.ExecuteReader("GetContentItemsByTabId", tabId);
        }

        /// <summary>Retrieve all content items associated with a articular Module ID, <paramref name="moduleId"/>.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByModuleId(int moduleId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByModuleId(moduleId={ModuleId})", moduleId);
			Logger.LogDebug("<<< Exiting GetContentItemsByModuleId(moduleId={ModuleId})", moduleId);
            return this.provider.ExecuteReader("GetContentItemsByModuleId", moduleId);
        }

        /// <summary>Retrieve a list of content items containing terms from the specified Vocabulary ID.</summary>
        /// <param name="vocabularyId">The vocabulary ID.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByVocabularyId(int vocabularyId)
        {
			Logger.LogDebug(">>> Entering GetContentItemsByVocabularyId(vocabularyId={VocabularyId})", vocabularyId);
			Logger.LogDebug("<<< Exiting GetContentItemsByVocabularyId(vocabularyId={VocabularyId})", vocabularyId);
            return this.provider.ExecuteReader("GetContentItemsByVocabularyId", vocabularyId);
        }

        /// <summary>Gets the un indexed content items.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetUnIndexedContentItems()
        {
			Logger.LogDebug(">>> Entering GetUnIndexedContentItems()");
			Logger.LogDebug("<<< Exiting GetUnIndexedContentItems()");
            return this.provider.ExecuteReader("GetUnIndexedContentItems");
        }

        /// <summary>Updates the content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void UpdateContentItem(ContentItem contentItem, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering UpdateContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery(
                "UpdateContentItem",
                contentItem.ContentItemId,
                contentItem.Content,
                contentItem.ContentTypeId,
                contentItem.TabID,
                contentItem.ModuleID,
                contentItem.ContentKey,
                contentItem.Indexed,
                createdByUserId,
                this.provider.GetNull(contentItem.StateID));
				Logger.LogDebug("<<< Exiting UpdateContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
				Logger.LogInformation("UpdateContentItem(contentItem,createdByUserId)={ContentItem},{CreatedByUserId}: database query executed in {Arg2} ms", contentItem, createdByUserId, __logStart.ElapsedMilliseconds);
        }

        /// <summary>Adds the meta data.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
			Logger.LogDebug(">>> Entering AddMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("AddMetaData", contentItem.ContentItemId, name, value);
			Logger.LogDebug("<<< Exiting AddMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("AddMetaData(contentItem,name,value)={ContentItem},{Name},{Value}: database query executed in {Arg3} ms", contentItem, name, value, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("AddMetaData(contentItem,name,value)={ContentItem},{Name},{Value}: database query executed in {Arg3} ms", contentItem, name, value, __logStart.ElapsedMilliseconds);
        /// <inheritdoc />
        public void SynchronizeMetaData(ContentItem contentItem, IEnumerable<KeyValuePair<string, string>> added, IEnumerable<KeyValuePair<string, string>> deleted)
        {
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
#if false
            //TODO: fixing the original code requires adding new DataProvider methods. Reason:
            //      since we are calling DeleteMetaData/AddMetaData on their own connections,
            //      the transaction is useless and not going on the same connection as the other
            //      operations; we must find a better way of doing this transactional operation.
            using (var transaction = _provider.GetTransaction())
            {
                try
                {
                    var connection = transaction.Connection;
                    foreach (var item in deleted)
                    {
                        _provider.ExecuteNonQuery(connection, "DeleteMetaData", contentItem.ContentItemId, item.Key, item.Value);
                    }

                    foreach (var item in added)
                    {
                        _provider.ExecuteNonQuery(connection, "AddMetaData", contentItem, item.Key, item.Value);
                    }

                    _provider.CommitTransaction(transaction);
                }
                catch (Exception)
                {
                    _provider.RollbackTransaction(transaction);
                    throw;
                }
            }
#else
            foreach (var item in deleted)
            {
                this.DeleteMetaData(contentItem, item.Key, item.Value);
            }

            foreach (var item in added)
            {
                this.AddMetaData(contentItem, item.Key, item.Value);
            }
#endif
        }

        /// <summary>Deletes the meta data.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void DeleteMetaData(ContentItem contentItem, string name, string value)
        {
			Logger.LogDebug(">>> Entering DeleteMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteMetaData", contentItem.ContentItemId, name, value);
			Logger.LogDebug("<<< Exiting DeleteMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteMetaData(contentItem,name,value)={ContentItem},{Name},{Value}: database query executed in {Arg3} ms", contentItem, name, value, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteMetaData(contentItem,name,value)={ContentItem},{Name},{Value}: database query executed in {Arg3} ms", contentItem, name, value, __logStart.ElapsedMilliseconds);
        /// <summary>Gets the meta data.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetMetaData(int contentItemId)
        {
			Logger.LogDebug(">>> Entering GetMetaData(contentItemId={ContentItemId})", contentItemId);
			Logger.LogDebug("<<< Exiting GetMetaData(contentItemId={ContentItemId})", contentItemId);
            return this.provider.ExecuteReader("GetMetaData", contentItemId);
        }

        /// <summary>Adds the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        public int AddContentType(ContentType contentType)
        {
			Logger.LogDebug(">>> Entering AddContentType(contentType={ContentType})", contentType);
			Logger.LogDebug("<<< Exiting AddContentType(contentType={ContentType})", contentType);
            return this.provider.ExecuteScalar<int>("AddContentType", contentType.ContentType);
        }

        /// <inheritdoc />
        public void DeleteContentType(ContentType contentType)
        {
			Logger.LogDebug(">>> Entering DeleteContentType(contentType={ContentType})", contentType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteContentType", contentType.ContentTypeId);
			Logger.LogDebug("<<< Exiting DeleteContentType(contentType={ContentType})", contentType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteContentType(contentType)={ContentType}: database query executed in {Arg1} ms", contentType, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteContentType(contentType)={ContentType}: database query executed in {Arg1} ms", contentType, __logStart.ElapsedMilliseconds);
        /// <summary>Gets the content types.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetContentTypes()
        {
			Logger.LogDebug(">>> Entering GetContentTypes()");
			Logger.LogDebug("<<< Exiting GetContentTypes()");
            return this.provider.ExecuteReader("GetContentTypes");
        }

        /// <summary>Updates the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        public void UpdateContentType(ContentType contentType)
        {
			Logger.LogDebug(">>> Entering UpdateContentType(contentType={ContentType})", contentType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("UpdateContentType", contentType.ContentTypeId, contentType.ContentType);
			Logger.LogDebug("<<< Exiting UpdateContentType(contentType={ContentType})", contentType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateContentType(contentType)={ContentType}: database query executed in {Arg1} ms", contentType, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("UpdateContentType(contentType)={ContentType}: database query executed in {Arg1} ms", contentType, __logStart.ElapsedMilliseconds);
        /// <summary>Adds the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        /// <returns>scope type id.</returns>
        public int AddScopeType(ScopeType scopeType)
        {
			Logger.LogDebug(">>> Entering AddScopeType(scopeType={ScopeType})", scopeType);
			Logger.LogDebug("<<< Exiting AddScopeType(scopeType={ScopeType})", scopeType);
            return this.provider.ExecuteScalar<int>("AddScopeType", scopeType.ScopeType);
        }

        /// <summary>Deletes the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        public void DeleteScopeType(ScopeType scopeType)
        {
			Logger.LogDebug(">>> Entering DeleteScopeType(scopeType={ScopeType})", scopeType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteScopeType", scopeType.ScopeTypeId);
			Logger.LogDebug("<<< Exiting DeleteScopeType(scopeType={ScopeType})", scopeType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteScopeType(scopeType)={ScopeType}: database query executed in {Arg1} ms", scopeType, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteScopeType(scopeType)={ScopeType}: database query executed in {Arg1} ms", scopeType, __logStart.ElapsedMilliseconds);
        /// <summary>Gets the scope types.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetScopeTypes()
        {
			Logger.LogDebug(">>> Entering GetScopeTypes()");
			Logger.LogDebug("<<< Exiting GetScopeTypes()");
            return this.provider.ExecuteReader("GetScopeTypes");
        }

        /// <summary>Updates the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        public void UpdateScopeType(ScopeType scopeType)
        {
			Logger.LogDebug(">>> Entering UpdateScopeType(scopeType={ScopeType})", scopeType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("UpdateScopeType", scopeType.ScopeTypeId, scopeType.ScopeType);
			Logger.LogDebug("<<< Exiting UpdateScopeType(scopeType={ScopeType})", scopeType);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateScopeType(scopeType)={ScopeType}: database query executed in {Arg1} ms", scopeType, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("UpdateScopeType(scopeType)={ScopeType}: database query executed in {Arg1} ms", scopeType, __logStart.ElapsedMilliseconds);
        /// <summary>Adds the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>term id.</returns>
        public int AddHeirarchicalTerm(Term term, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering AddHeirarchicalTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
			Logger.LogDebug("<<< Exiting AddHeirarchicalTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
            return this.provider.ExecuteScalar<int>("AddHeirarchicalTerm", term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        /// <summary>Adds the simple term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>term id.</returns>
        public int AddSimpleTerm(Term term, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering AddSimpleTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
			Logger.LogDebug("<<< Exiting AddSimpleTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
            return this.provider.ExecuteScalar<int>("AddSimpleTerm", term.VocabularyId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        /// <inheritdoc />
        public void AddTermToContent(Term term, ContentItem contentItem)
        {
			Logger.LogDebug(">>> Entering AddTermToContent(term={Term},contentItem={ContentItem})", term, contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("AddTermToContent", term.TermId, contentItem.ContentItemId);
			Logger.LogDebug("<<< Exiting AddTermToContent(term={Term},contentItem={ContentItem})", term, contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("AddTermToContent(term,contentItem)={Term},{ContentItem}: database query executed in {Arg2} ms", term, contentItem, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("AddTermToContent(term,contentItem)={Term},{ContentItem}: database query executed in {Arg2} ms", term, contentItem, __logStart.ElapsedMilliseconds);
        /// <summary>Deletes the simple term.</summary>
        /// <param name="term">The term.</param>
        public void DeleteSimpleTerm(Term term)
        {
			Logger.LogDebug(">>> Entering DeleteSimpleTerm(term={Term})", term);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteSimpleTerm", term.TermId);
			Logger.LogDebug("<<< Exiting DeleteSimpleTerm(term={Term})", term);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteSimpleTerm(term)={Term}: database query executed in {Arg1} ms", term, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteSimpleTerm(term)={Term}: database query executed in {Arg1} ms", term, __logStart.ElapsedMilliseconds);
        /// <summary>Deletes the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        public void DeleteHeirarchicalTerm(Term term)
        {
			Logger.LogDebug(">>> Entering DeleteHeirarchicalTerm(term={Term})", term);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteHeirarchicalTerm", term.TermId);
			Logger.LogDebug("<<< Exiting DeleteHeirarchicalTerm(term={Term})", term);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteHeirarchicalTerm(term)={Term}: database query executed in {Arg1} ms", term, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteHeirarchicalTerm(term)={Term}: database query executed in {Arg1} ms", term, __logStart.ElapsedMilliseconds);
        /// <summary>Gets the term.</summary>
        /// <param name="termId">The term id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTerm(int termId)
        {
			Logger.LogDebug(">>> Entering GetTerm(termId={TermId})", termId);
			Logger.LogDebug("<<< Exiting GetTerm(termId={TermId})", termId);
            return this.provider.ExecuteReader("GetTerm", termId);
        }

        /// <summary>Retrieve term usage data for the specified Term ID, <paramref name="termId"/>.</summary>
        /// <param name="termId">The term ID.</param>
        /// <returns>A data reader with the term usage.</returns>
        public IDataReader GetTermUsage(int termId)
        {
			Logger.LogDebug(">>> Entering GetTermUsage(termId={TermId})", termId);
			Logger.LogDebug("<<< Exiting GetTermUsage(termId={TermId})", termId);
            return this.provider.ExecuteReader("GetTermUsage", termId);
        }

        /// <summary>Gets the content of the terms by.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTermsByContent(int contentItemId)
        {
			Logger.LogDebug(">>> Entering GetTermsByContent(contentItemId={ContentItemId})", contentItemId);
			Logger.LogDebug("<<< Exiting GetTermsByContent(contentItemId={ContentItemId})", contentItemId);
            return this.provider.ExecuteReader("GetTermsByContent", contentItemId);
        }

        /// <summary>Gets the terms by vocabulary.</summary>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTermsByVocabulary(int vocabularyId)
        {
			Logger.LogDebug(">>> Entering GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
			Logger.LogDebug("<<< Exiting GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            return this.provider.ExecuteReader("GetTermsByVocabulary", vocabularyId);
        }

        /// <summary>Removes the content of the terms from.</summary>
        /// <param name="contentItem">The content item.</param>
        public void RemoveTermsFromContent(ContentItem contentItem)
        {
			Logger.LogDebug(">>> Entering RemoveTermsFromContent(contentItem={ContentItem})", contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("RemoveTermsFromContent", contentItem.ContentItemId);
			Logger.LogDebug("<<< Exiting RemoveTermsFromContent(contentItem={ContentItem})", contentItem);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("RemoveTermsFromContent(contentItem)={ContentItem}: database query executed in {Arg1} ms", contentItem, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("RemoveTermsFromContent(contentItem)={ContentItem}: database query executed in {Arg1} ms", contentItem, __logStart.ElapsedMilliseconds);
        /// <summary>Updates the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId)
        {
			Logger.LogDebug(">>> Entering UpdateHeirarchicalTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("UpdateHeirarchicalTerm", term.TermId, term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
			Logger.LogDebug("<<< Exiting UpdateHeirarchicalTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateHeirarchicalTerm(term,lastModifiedByUserId)={Term},{LastModifiedByUserId}: database query executed in {Arg2} ms", term, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("UpdateHeirarchicalTerm(term,lastModifiedByUserId)={Term},{LastModifiedByUserId}: database query executed in {Arg2} ms", term, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        /// <summary>Updates the simple term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateSimpleTerm(Term term, int lastModifiedByUserId)
        {
			Logger.LogDebug(">>> Entering UpdateSimpleTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("UpdateSimpleTerm", term.TermId, term.VocabularyId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
			Logger.LogDebug("<<< Exiting UpdateSimpleTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("UpdateSimpleTerm(term,lastModifiedByUserId)={Term},{LastModifiedByUserId}: database query executed in {Arg2} ms", term, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("UpdateSimpleTerm(term,lastModifiedByUserId)={Term},{LastModifiedByUserId}: database query executed in {Arg2} ms", term, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        /// <summary>Adds the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>Vocabulary id.</returns>
        public int AddVocabulary(Vocabulary vocabulary, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering AddVocabulary(vocabulary={Vocabulary},createdByUserId={CreatedByUserId})", vocabulary, createdByUserId);
			Logger.LogDebug("<<< Exiting AddVocabulary(vocabulary={Vocabulary},createdByUserId={CreatedByUserId})", vocabulary, createdByUserId);
            return this.provider.ExecuteScalar<int>(
                "AddVocabulary",
                vocabulary.Type,
                vocabulary.Name,
                vocabulary.Description,
                vocabulary.Weight,
                this.provider.GetNull(vocabulary.ScopeId),
                vocabulary.ScopeTypeId,
                createdByUserId);
        }

        /// <summary>Deletes the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        public void DeleteVocabulary(Vocabulary vocabulary)
        {
			Logger.LogDebug(">>> Entering DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("DeleteVocabulary", vocabulary.VocabularyId);
			Logger.LogDebug("<<< Exiting DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteVocabulary(vocabulary)={Vocabulary}: database query executed in {Arg1} ms", vocabulary, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteVocabulary(vocabulary)={Vocabulary}: database query executed in {Arg1} ms", vocabulary, __logStart.ElapsedMilliseconds);
        /// <summary>Gets the vocabularies.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetVocabularies()
        {
			Logger.LogDebug(">>> Entering GetVocabularies()");
			Logger.LogDebug("<<< Exiting GetVocabularies()");
            return this.provider.ExecuteReader("GetVocabularies");
        }

        /// <summary>Updates the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId)
        {
			Logger.LogDebug(">>> Entering UpdateVocabulary(vocabulary={Vocabulary},lastModifiedByUserId={LastModifiedByUserId})", vocabulary, lastModifiedByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery(
                "UpdateVocabulary",
                vocabulary.VocabularyId,
                vocabulary.Type,
                vocabulary.Name,
                vocabulary.Description,
                vocabulary.Weight,
                vocabulary.ScopeId,
                vocabulary.ScopeTypeId,
                lastModifiedByUserId);
				Logger.LogDebug("<<< Exiting UpdateVocabulary(vocabulary={Vocabulary},lastModifiedByUserId={LastModifiedByUserId})", vocabulary, lastModifiedByUserId);
				Logger.LogInformation("UpdateVocabulary(vocabulary,lastModifiedByUserId)={Vocabulary},{LastModifiedByUserId}: database query executed in {Arg2} ms", vocabulary, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        }
    }
}
