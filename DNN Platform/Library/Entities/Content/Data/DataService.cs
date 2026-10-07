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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        private readonly DataProvider provider = DataProvider.Instance();

        /// <summary>Adds the content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>content item id.</returns>
        public int AddContentItem(ContentItem contentItem, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
			Logger.LogWarning("<<< Exiting AddContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
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
			Logger.LogError(">>> Entering DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
            this.provider.ExecuteNonQuery("DeleteContentItem", contentItemId);
			Logger.LogWarning("<<< Exiting DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
        }

        /// <summary>Gets the content item.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItem(int contentItemId)
        {
			Logger.LogError(">>> Entering GetContentItem(contentItemId={ContentItemId})", contentItemId);
            Logger.LogWarning("<<< Exiting GetContentItem(contentItemId={ContentItemId})", contentItemId);
            return this.provider.ExecuteReader("GetContentItem", contentItemId);
        }

        /// <summary>Gets the content items.</summary>
        /// <param name="contentTypeId">The Id of the Content Type.</param>
        /// <param name="tabId">The Id of the Tab.</param>
        /// <param name="moduleId">The Id of the Module.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetContentItems(int contentTypeId, int tabId, int moduleId)
        {
			Logger.LogError(">>> Entering GetContentItems(contentTypeId={ContentTypeId},tabId={TabId},moduleId={ModuleId})", contentTypeId, tabId, moduleId);
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
			Logger.LogError(">>> Entering GetContentItemsByTerm(term={Term})", term);
            Logger.LogWarning("<<< Exiting GetContentItemsByTerm(term={Term})", term);
            return this.provider.ExecuteReader("GetContentItemsByTerm", term);
        }

        /// <summary>Get a list of content items of the specified content type, <paramref name="contentTypeId"/>.</summary>
        /// <param name="contentTypeId">The type of content items you are searching for.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByContentType(int contentTypeId)
        {
			Logger.LogError(">>> Entering GetContentItemsByContentType(contentTypeId={ContentTypeId})", contentTypeId);
            Logger.LogWarning("<<< Exiting GetContentItemsByContentType(contentTypeId={ContentTypeId})", contentTypeId);
            return this.provider.ExecuteReader("GetContentItemsByContentType", contentTypeId);
        }

        /// <summary>Get a list of content items based on TabID (PageID).</summary>
        /// <param name="tabId">The TabID (or "Page ID") that the content items are associated with.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByTabId(int tabId)
        {
			Logger.LogError(">>> Entering GetContentItemsByTabId(tabId={TabId})", tabId);
            Logger.LogWarning("<<< Exiting GetContentItemsByTabId(tabId={TabId})", tabId);
            return this.provider.ExecuteReader("GetContentItemsByTabId", tabId);
        }

        /// <summary>Retrieve all content items associated with a articular Module ID, <paramref name="moduleId"/>.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByModuleId(int moduleId)
        {
			Logger.LogError(">>> Entering GetContentItemsByModuleId(moduleId={ModuleId})", moduleId);
            Logger.LogWarning("<<< Exiting GetContentItemsByModuleId(moduleId={ModuleId})", moduleId);
            return this.provider.ExecuteReader("GetContentItemsByModuleId", moduleId);
        }

        /// <summary>Retrieve a list of content items containing terms from the specified Vocabulary ID.</summary>
        /// <param name="vocabularyId">The vocabulary ID.</param>
        /// <returns>A data reader containing the content items.</returns>
        public IDataReader GetContentItemsByVocabularyId(int vocabularyId)
        {
			Logger.LogError(">>> Entering GetContentItemsByVocabularyId(vocabularyId={VocabularyId})", vocabularyId);
            Logger.LogWarning("<<< Exiting GetContentItemsByVocabularyId(vocabularyId={VocabularyId})", vocabularyId);
            return this.provider.ExecuteReader("GetContentItemsByVocabularyId", vocabularyId);
        }

        /// <summary>Gets the un indexed content items.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetUnIndexedContentItems()
        {
			Logger.LogError(">>> Entering GetUnIndexedContentItems()");
            Logger.LogWarning("<<< Exiting GetUnIndexedContentItems()");
            return this.provider.ExecuteReader("GetUnIndexedContentItems");
        }

        /// <summary>Updates the content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void UpdateContentItem(ContentItem contentItem, int createdByUserId)
        {
			Logger.LogError(">>> Entering UpdateContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
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
				Logger.LogError("UpdateContentItem(contentItem,createdByUserId)={ContentItem},{CreatedByUserId}: database query executed in {Arg2} ms", contentItem, createdByUserId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateContentItem(contentItem={ContentItem},createdByUserId={CreatedByUserId})", contentItem, createdByUserId);
        }

        /// <summary>Adds the meta data.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddMetaData(ContentItem contentItem, string name, string value)
        {
			Logger.LogError(">>> Entering AddMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
            this.provider.ExecuteNonQuery("AddMetaData", contentItem.ContentItemId, name, value);
			Logger.LogWarning("<<< Exiting AddMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
        }

        /// <inheritdoc />
        public void SynchronizeMetaData(ContentItem contentItem, IEnumerable<KeyValuePair<string, string>> added, IEnumerable<KeyValuePair<string, string>> deleted)
        {
#if false
			Logger.LogError(">>> Entering SynchronizeMetaData(contentItem={ContentItem},added={Added},deleted={Deleted})", contentItem, added, deleted);
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
						var __logStart = System.Diagnostics.Stopwatch.StartNew();
						Logger.LogError("SynchronizeMetaData(contentItem,added,deleted)={ContentItem},{Added},{Deleted}: database query executed in {Arg3} ms", contentItem, added, deleted, __logStart.ElapsedMilliseconds);
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
					Logger.LogError("Exception in SynchronizeMetaData(contentItem={ContentItem},added={Added},deleted={Deleted}): {Arg3}", contentItem, added, deleted, Exception.Message);
                    _provider.RollbackTransaction(transaction);
                    throw;
                }
			Logger.LogWarning("<<< Exiting SynchronizeMetaData(contentItem={ContentItem},added={Added},deleted={Deleted})", contentItem, added, deleted);
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
			Logger.LogError(">>> Entering DeleteMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
            this.provider.ExecuteNonQuery("DeleteMetaData", contentItem.ContentItemId, name, value);
			Logger.LogWarning("<<< Exiting DeleteMetaData(contentItem={ContentItem},name={Name},value={Value})", contentItem, name, value);
        }

        /// <summary>Gets the meta data.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetMetaData(int contentItemId)
        {
			Logger.LogError(">>> Entering GetMetaData(contentItemId={ContentItemId})", contentItemId);
            Logger.LogWarning("<<< Exiting GetMetaData(contentItemId={ContentItemId})", contentItemId);
            return this.provider.ExecuteReader("GetMetaData", contentItemId);
        }

        /// <summary>Adds the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        public int AddContentType(ContentType contentType)
        {
			Logger.LogError(">>> Entering AddContentType(contentType={ContentType})", contentType);
            Logger.LogWarning("<<< Exiting AddContentType(contentType={ContentType})", contentType);
            return this.provider.ExecuteScalar<int>("AddContentType", contentType.ContentType);
        }

        /// <inheritdoc />
        public void DeleteContentType(ContentType contentType)
        {
			Logger.LogError(">>> Entering DeleteContentType(contentType={ContentType})", contentType);
            this.provider.ExecuteNonQuery("DeleteContentType", contentType.ContentTypeId);
			Logger.LogWarning("<<< Exiting DeleteContentType(contentType={ContentType})", contentType);
        }

        /// <summary>Gets the content types.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetContentTypes()
        {
			Logger.LogError(">>> Entering GetContentTypes()");
            Logger.LogWarning("<<< Exiting GetContentTypes()");
            return this.provider.ExecuteReader("GetContentTypes");
        }

        /// <summary>Updates the type of the content.</summary>
        /// <param name="contentType">Type of the content.</param>
        public void UpdateContentType(ContentType contentType)
        {
			Logger.LogError(">>> Entering UpdateContentType(contentType={ContentType})", contentType);
            this.provider.ExecuteNonQuery("UpdateContentType", contentType.ContentTypeId, contentType.ContentType);
			Logger.LogWarning("<<< Exiting UpdateContentType(contentType={ContentType})", contentType);
        }

        /// <summary>Adds the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        /// <returns>scope type id.</returns>
        public int AddScopeType(ScopeType scopeType)
        {
			Logger.LogError(">>> Entering AddScopeType(scopeType={ScopeType})", scopeType);
            Logger.LogWarning("<<< Exiting AddScopeType(scopeType={ScopeType})", scopeType);
            return this.provider.ExecuteScalar<int>("AddScopeType", scopeType.ScopeType);
        }

        /// <summary>Deletes the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        public void DeleteScopeType(ScopeType scopeType)
        {
			Logger.LogError(">>> Entering DeleteScopeType(scopeType={ScopeType})", scopeType);
            this.provider.ExecuteNonQuery("DeleteScopeType", scopeType.ScopeTypeId);
			Logger.LogWarning("<<< Exiting DeleteScopeType(scopeType={ScopeType})", scopeType);
        }

        /// <summary>Gets the scope types.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetScopeTypes()
        {
			Logger.LogError(">>> Entering GetScopeTypes()");
            Logger.LogWarning("<<< Exiting GetScopeTypes()");
            return this.provider.ExecuteReader("GetScopeTypes");
        }

        /// <summary>Updates the type of the scope.</summary>
        /// <param name="scopeType">Type of the scope.</param>
        public void UpdateScopeType(ScopeType scopeType)
        {
			Logger.LogError(">>> Entering UpdateScopeType(scopeType={ScopeType})", scopeType);
            this.provider.ExecuteNonQuery("UpdateScopeType", scopeType.ScopeTypeId, scopeType.ScopeType);
			Logger.LogWarning("<<< Exiting UpdateScopeType(scopeType={ScopeType})", scopeType);
        }

        /// <summary>Adds the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>term id.</returns>
        public int AddHeirarchicalTerm(Term term, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddHeirarchicalTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
            Logger.LogWarning("<<< Exiting AddHeirarchicalTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
            return this.provider.ExecuteScalar<int>("AddHeirarchicalTerm", term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        /// <summary>Adds the simple term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>term id.</returns>
        public int AddSimpleTerm(Term term, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddSimpleTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
            Logger.LogWarning("<<< Exiting AddSimpleTerm(term={Term},createdByUserId={CreatedByUserId})", term, createdByUserId);
            return this.provider.ExecuteScalar<int>("AddSimpleTerm", term.VocabularyId, term.Name, term.Description, term.Weight, createdByUserId);
        }

        /// <inheritdoc />
        public void AddTermToContent(Term term, ContentItem contentItem)
        {
			Logger.LogError(">>> Entering AddTermToContent(term={Term},contentItem={ContentItem})", term, contentItem);
            this.provider.ExecuteNonQuery("AddTermToContent", term.TermId, contentItem.ContentItemId);
			Logger.LogWarning("<<< Exiting AddTermToContent(term={Term},contentItem={ContentItem})", term, contentItem);
        }

        /// <summary>Deletes the simple term.</summary>
        /// <param name="term">The term.</param>
        public void DeleteSimpleTerm(Term term)
        {
			Logger.LogError(">>> Entering DeleteSimpleTerm(term={Term})", term);
            this.provider.ExecuteNonQuery("DeleteSimpleTerm", term.TermId);
			Logger.LogWarning("<<< Exiting DeleteSimpleTerm(term={Term})", term);
        }

        /// <summary>Deletes the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        public void DeleteHeirarchicalTerm(Term term)
        {
			Logger.LogError(">>> Entering DeleteHeirarchicalTerm(term={Term})", term);
            this.provider.ExecuteNonQuery("DeleteHeirarchicalTerm", term.TermId);
			Logger.LogWarning("<<< Exiting DeleteHeirarchicalTerm(term={Term})", term);
        }

        /// <summary>Gets the term.</summary>
        /// <param name="termId">The term id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTerm(int termId)
        {
			Logger.LogError(">>> Entering GetTerm(termId={TermId})", termId);
            Logger.LogWarning("<<< Exiting GetTerm(termId={TermId})", termId);
            return this.provider.ExecuteReader("GetTerm", termId);
        }

        /// <summary>Retrieve term usage data for the specified Term ID, <paramref name="termId"/>.</summary>
        /// <param name="termId">The term ID.</param>
        /// <returns>A data reader with the term usage.</returns>
        public IDataReader GetTermUsage(int termId)
        {
			Logger.LogError(">>> Entering GetTermUsage(termId={TermId})", termId);
            Logger.LogWarning("<<< Exiting GetTermUsage(termId={TermId})", termId);
            return this.provider.ExecuteReader("GetTermUsage", termId);
        }

        /// <summary>Gets the content of the terms by.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTermsByContent(int contentItemId)
        {
			Logger.LogError(">>> Entering GetTermsByContent(contentItemId={ContentItemId})", contentItemId);
            Logger.LogWarning("<<< Exiting GetTermsByContent(contentItemId={ContentItemId})", contentItemId);
            return this.provider.ExecuteReader("GetTermsByContent", contentItemId);
        }

        /// <summary>Gets the terms by vocabulary.</summary>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>data reader.</returns>
        public IDataReader GetTermsByVocabulary(int vocabularyId)
        {
			Logger.LogError(">>> Entering GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            Logger.LogWarning("<<< Exiting GetTermsByVocabulary(vocabularyId={VocabularyId})", vocabularyId);
            return this.provider.ExecuteReader("GetTermsByVocabulary", vocabularyId);
        }

        /// <summary>Removes the content of the terms from.</summary>
        /// <param name="contentItem">The content item.</param>
        public void RemoveTermsFromContent(ContentItem contentItem)
        {
			Logger.LogError(">>> Entering RemoveTermsFromContent(contentItem={ContentItem})", contentItem);
            this.provider.ExecuteNonQuery("RemoveTermsFromContent", contentItem.ContentItemId);
			Logger.LogWarning("<<< Exiting RemoveTermsFromContent(contentItem={ContentItem})", contentItem);
        }

        /// <summary>Updates the hierarchical term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateHeirarchicalTerm(Term term, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateHeirarchicalTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
            this.provider.ExecuteNonQuery("UpdateHeirarchicalTerm", term.TermId, term.VocabularyId, term.ParentTermId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
			Logger.LogWarning("<<< Exiting UpdateHeirarchicalTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
        }

        /// <summary>Updates the simple term.</summary>
        /// <param name="term">The term.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateSimpleTerm(Term term, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateSimpleTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
            this.provider.ExecuteNonQuery("UpdateSimpleTerm", term.TermId, term.VocabularyId, term.Name, term.Description, term.Weight, lastModifiedByUserId);
			Logger.LogWarning("<<< Exiting UpdateSimpleTerm(term={Term},lastModifiedByUserId={LastModifiedByUserId})", term, lastModifiedByUserId);
        }

        /// <summary>Adds the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        /// <param name="createdByUserId">The created by user id.</param>
        /// <returns>Vocabulary id.</returns>
        public int AddVocabulary(Vocabulary vocabulary, int createdByUserId)
        {
			Logger.LogError(">>> Entering AddVocabulary(vocabulary={Vocabulary},createdByUserId={CreatedByUserId})", vocabulary, createdByUserId);
            Logger.LogWarning("<<< Exiting AddVocabulary(vocabulary={Vocabulary},createdByUserId={CreatedByUserId})", vocabulary, createdByUserId);
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
			Logger.LogError(">>> Entering DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
            this.provider.ExecuteNonQuery("DeleteVocabulary", vocabulary.VocabularyId);
			Logger.LogWarning("<<< Exiting DeleteVocabulary(vocabulary={Vocabulary})", vocabulary);
        }

        /// <summary>Gets the vocabularies.</summary>
        /// <returns>data reader.</returns>
        public IDataReader GetVocabularies()
        {
			Logger.LogError(">>> Entering GetVocabularies()");
            Logger.LogWarning("<<< Exiting GetVocabularies()");
            return this.provider.ExecuteReader("GetVocabularies");
        }

        /// <summary>Updates the vocabulary.</summary>
        /// <param name="vocabulary">The vocabulary.</param>
        /// <param name="lastModifiedByUserId">The last modified by user id.</param>
        public void UpdateVocabulary(Vocabulary vocabulary, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering UpdateVocabulary(vocabulary={Vocabulary},lastModifiedByUserId={LastModifiedByUserId})", vocabulary, lastModifiedByUserId);
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
				Logger.LogError("UpdateVocabulary(vocabulary,lastModifiedByUserId)={Vocabulary},{LastModifiedByUserId}: database query executed in {Arg2} ms", vocabulary, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateVocabulary(vocabulary={Vocabulary},lastModifiedByUserId={LastModifiedByUserId})", vocabulary, lastModifiedByUserId);
        }
    }
}
