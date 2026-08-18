// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>  The Impl Controller class for Lucene.</summary>
    internal class InternalSearchControllerImpl : IInternalSearchController
    {
        private const string SearchableModuleDefsKey = "{0}-{1}-{2}";
        private const string SearchableModuleDefsCacheKey = "SearchableModuleDefs";
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        private const string HtmlTagsWithAttrs = "<[a-z_:][\\w:.-]*(?>(?:\\s+(?<attr>\\w+\\s*?=\\s*?[\"'].*?[\"']))*)?\\s*/?>";

        private const string AttrText = "[\"'](?<text>.*?)[\"']";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InternalSearchControllerImpl));

        private static readonly string[] HtmlAttributesToRetain = { "alt", "title" };
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        private static readonly Regex StripOpeningTagsRegex = RegexUtils.GetCachedRegex(@"<\w*\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex StripClosingTagsRegex = RegexUtils.GetCachedRegex(@"</\w*\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlTagsRegex = RegexUtils.GetCachedRegex(HtmlTagsWithAttrs, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AttrTextRegex = RegexUtils.GetCachedRegex(AttrText, RegexOptions.Compiled);

        private readonly IHostSettings hostSettings;
        private readonly IServiceProvider serviceProvider;
        private readonly int titleBoost;
        private readonly int tagBoost;
        private readonly int contentBoost;
        private readonly int descriptionBoost;
        private readonly int authorBoost;
        private readonly int moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

        /// <summary>Initializes a new instance of the <see cref="InternalSearchControllerImpl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public InternalSearchControllerImpl()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering InternalSearchControllerImpl()");
        }

        /// <summary>Initializes a new instance of the <see cref="InternalSearchControllerImpl"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public InternalSearchControllerImpl(IHostSettingsService hostSettingsService, IHostSettings hostSettings, IServiceProvider serviceProvider)
        {
            hostSettingsService ??= Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
			Logger.LogDebug("<<< Exiting InternalSearchControllerImpl()");
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): hostSettings → {HostSettings1}", hostSettingsService, hostSettings, serviceProvider, hostSettings);
            this.serviceProvider = serviceProvider ?? Globals.GetCurrentServiceProvider();
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", hostSettingsService, hostSettings, serviceProvider, serviceProvider);
            this.titleBoost = hostSettingsService.GetInteger(Constants.SearchTitleBoostSetting, Constants.DefaultSearchTitleBoost);
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): titleBoost → {TitleBoost}", hostSettingsService, hostSettings, serviceProvider, titleBoost);
            this.tagBoost = hostSettingsService.GetInteger(Constants.SearchTagBoostSetting, Constants.DefaultSearchTagBoost);
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): tagBoost → {TagBoost}", hostSettingsService, hostSettings, serviceProvider, tagBoost);
            this.contentBoost = hostSettingsService.GetInteger(Constants.SearchContentBoostSetting, Constants.DefaultSearchKeywordBoost);
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): contentBoost → {ContentBoost}", hostSettingsService, hostSettings, serviceProvider, contentBoost);
            this.descriptionBoost = hostSettingsService.GetInteger(Constants.SearchDescriptionBoostSetting, Constants.DefaultSearchDescriptionBoost);
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): descriptionBoost → {DescriptionBoost}", hostSettingsService, hostSettings, serviceProvider, descriptionBoost);
            this.authorBoost = hostSettingsService.GetInteger(Constants.SearchAuthorBoostSetting, Constants.DefaultSearchAuthorBoost);
			Logger.LogDebug("InternalSearchControllerImpl(hostSettingsService={HostSettingsService},hostSettings={HostSettings},serviceProvider={ServiceProvider}): authorBoost → {AuthorBoost}", hostSettingsService, hostSettings, serviceProvider, authorBoost);
        }

        /// <inheritdoc />
        public IEnumerable<SearchContentSource> GetSearchContentSourceList(int portalId)
        {
			Logger.LogDebug(">>> Entering GetSearchContentSourceList(portalId={PortalId})", portalId);
            var searchableModuleDefsCacheArgs = new CacheItemArgs(
                string.Format(
                    CultureInfo.InvariantCulture,
                    SearchableModuleDefsKey,
                    SearchableModuleDefsCacheKey,
                    portalId,
                    Thread.CurrentThread.CurrentCulture),
                120,
                CacheItemPriority.Default);

			Logger.LogDebug("<<< Exiting GetSearchContentSourceList(portalId={PortalId})", portalId);
            return CBO.GetCachedObject<IList<SearchContentSource>>(
                this.hostSettings,
                searchableModuleDefsCacheArgs,
                this.SearchContentSourceCallback);
        }

        /// <inheritdoc />
        public string GetSearchDocumentTypeDisplayName(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering GetSearchDocumentTypeDisplayName(searchResult={SearchResult})", searchResult);
            // ModuleDefId will be zero for non-module
            var key = $"{searchResult.SearchTypeId}-{searchResult.ModuleDefId}-{Thread.CurrentThread.CurrentCulture}";
			Logger.LogWarning("GetSearchDocumentTypeDisplayName(searchResult={SearchResult}): key → {Key}", searchResult, key);
            var keys = CBO.Instance.GetCachedObject<IDictionary<string, string>>(
                new CacheItemArgs(key, 120, CacheItemPriority.Default),
                this.SearchDocumentTypeDisplayNameCallBack,
                false);

			Logger.LogDebug("<<< Exiting GetSearchDocumentTypeDisplayName(searchResult={SearchResult})", searchResult);
            return keys.TryGetValue(key, out var documentTypeDisplayName) ? documentTypeDisplayName : string.Empty;
        }

        /// <inheritdoc />
        public void AddSearchDocument(SearchDocument searchDocument)
        {
			Logger.LogDebug(">>> Entering AddSearchDocument(searchDocument={SearchDocument})", searchDocument);
            this.AddSearchDocumentInternal(searchDocument, false);
			Logger.LogDebug("<<< Exiting AddSearchDocument(searchDocument={SearchDocument})", searchDocument);
        }

        /// <inheritdoc />
        public void AddSearchDocuments(IEnumerable<SearchDocument> searchDocuments)
        {
			Logger.LogDebug(">>> Entering AddSearchDocuments(searchDocuments={SearchDocuments})", searchDocuments);
            var searchDocs = searchDocuments as IList<SearchDocument> ?? searchDocuments.ToList();
            if (searchDocs.Any())
			Logger.LogDebug("AddSearchDocuments(searchDocuments={SearchDocuments}): searchDocs → {SearchDocs}", searchDocuments, searchDocs);
            {
                const int commitBatchSize = 1024 * 16;
                var idx = 0;

                ////var added = false;
                foreach (var searchDoc in searchDocs)
                {
                    try
                    {
                        this.AddSearchDocumentInternal(searchDoc, (++idx % commitBatchSize) == 0);

                        ////added = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat(CultureInfo.InvariantCulture, "Search Document error: {0}{1}{2}", searchDoc, Environment.NewLine, ex);
                    }
                }

                // Note: modified to do commit only once at the end of scheduler job
                // check so we don't commit again
                ////if (added && (idx % commitBatchSize) != 0)
                ////{
                ////   Commit();
                ////}
            }
			Logger.LogDebug("<<< Exiting AddSearchDocuments(searchDocuments={SearchDocuments})", searchDocuments);
        }

        /// <inheritdoc />
        public void DeleteSearchDocument(SearchDocument searchDocument)
        {
			Logger.LogDebug(">>> Entering DeleteSearchDocument(searchDocument={SearchDocument})", searchDocument);
            this.DeleteSearchDocumentInternal(searchDocument, false);
			Logger.LogDebug("<<< Exiting DeleteSearchDocument(searchDocument={SearchDocument})", searchDocument);
        }

        /// <inheritdoc />
        public void DeleteSearchDocumentsByModule(int portalId, int moduleId, int moduleDefId)
        {
			Logger.LogDebug(">>> Entering DeleteSearchDocumentsByModule(portalId={PortalId},moduleId={ModuleId},moduleDefId={ModuleDefId})", portalId, moduleId, moduleDefId);
            Requires.NotNegative("PortalId", portalId);

            this.DeleteSearchDocument(new SearchDocument
            {
                PortalId = portalId,
                ModuleId = moduleId,
                ModuleDefId = moduleDefId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
            });
			Logger.LogDebug("<<< Exiting DeleteSearchDocumentsByModule(portalId={PortalId},moduleId={ModuleId},moduleDefId={ModuleDefId})", portalId, moduleId, moduleDefId);
        }

        /// <inheritdoc />
        public void DeleteAllDocuments(int portalId, int searchTypeId)
        {
			Logger.LogDebug(">>> Entering DeleteAllDocuments(portalId={PortalId},searchTypeId={SearchTypeId})", portalId, searchTypeId);
            Requires.NotNegative("SearchTypeId", searchTypeId);

            this.DeleteSearchDocument(new SearchDocument
            {
                PortalId = portalId,
                SearchTypeId = searchTypeId,
            });
			Logger.LogDebug("<<< Exiting DeleteAllDocuments(portalId={PortalId},searchTypeId={SearchTypeId})", portalId, searchTypeId);
        }

        /// <inheritdoc />
        public void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            LuceneController.Instance.Commit();
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <inheritdoc />
        public bool OptimizeSearchIndex()
        {
			Logger.LogDebug(">>> Entering OptimizeSearchIndex()");
            // run optimization in background
			Logger.LogDebug("<<< Exiting OptimizeSearchIndex()");
            return LuceneController.Instance.OptimizeSearchIndex(true);
        }

        /// <inheritdoc />
        public SearchStatistics GetSearchStatistics()
        {
			Logger.LogDebug(">>> Entering GetSearchStatistics()");
			Logger.LogDebug("<<< Exiting GetSearchStatistics()");
            return LuceneController.Instance.GetSearchStatistics();
        }

        internal virtual object SearchContentSourceCallback(CacheItemArgs cacheItem)
        {
			Logger.LogDebug(">>> Entering SearchContentSourceCallback(cacheItem={CacheItem})", cacheItem);
            var searchTypes = CBO.FillCollection<SearchType>(DataProvider.GetAllSearchTypes());

			Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): searchTypes → {SearchTypes}", cacheItem, searchTypes);
            var results = new List<SearchContentSource>();
			Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): results → {Results}", cacheItem, results);

            foreach (var crawler in searchTypes)
            {
                switch (crawler.SearchTypeName)
                {
                    case "module": // module crawler

                        // get searchable module definition list
                        var portalId = int.Parse(cacheItem.CacheKey.Split('-')[1], CultureInfo.InvariantCulture);
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): portalId → {PortalId}", cacheItem, portalId);
                        var modules = ModuleController.Instance.GetSearchModules(portalId);
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): modules → {Modules}", cacheItem, modules);
                        var modDefIds = new HashSet<int>();
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): modDefIds → {ModDefIds}", cacheItem, modDefIds);

                        foreach (ModuleInfo module in modules)
                        {
                            modDefIds.Add(module.ModuleDefID);
                        }

                        var list = modDefIds.Select(ModuleDefinitionController.GetModuleDefinitionByID).ToList();
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): list → {List}", cacheItem, list);

                        foreach (var def in list)
                        {
                            var text = Localization.GetSafeJSString("Module_" + def.DefinitionName, LocalizedResxFile);
							Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): text → {Text}", cacheItem, text);
                            if (string.IsNullOrEmpty(text))
                            {
                                text = def.FriendlyName;
                            }

                            var result = new SearchContentSource
                            {
                                SearchTypeId = crawler.SearchTypeId,
                                SearchTypeName = crawler.SearchTypeName,
                                IsPrivate = crawler.IsPrivate,
                                ModuleDefinitionId = def.ModuleDefID,
                                LocalizedName = text,
                            };

                            results.Add(result);
                        }

                        break;

                    default:

                        var resultControllerType = Reflection.CreateType(crawler.SearchResultClass);
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): resultControllerType → {ResultControllerType}", cacheItem, resultControllerType);
                        var resultController = (BaseResultController)Reflection.CreateObject(this.serviceProvider, resultControllerType);
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): resultController → {ResultController}", cacheItem, resultController);
                        var localizedName = Localization.GetSafeJSString(resultController.LocalizedSearchTypeName);
						Logger.LogDebug("SearchContentSourceCallback(cacheItem={CacheItem}): localizedName → {LocalizedName}", cacheItem, localizedName);

                        results.Add(new SearchContentSource
                        {
                            SearchTypeId = crawler.SearchTypeId,
                            SearchTypeName = crawler.SearchTypeName,
                            IsPrivate = crawler.IsPrivate,
                            ModuleDefinitionId = 0,
                            LocalizedName = localizedName,
                        });
                        break;
                }
            }

			Logger.LogDebug("<<< Exiting SearchContentSourceCallback(cacheItem={CacheItem})", cacheItem);
            return results;
        }

        private static NumericRangeQuery<int> NumericValueQuery(string numericName, int numericVal)
        {
			Logger.LogDebug(">>> Entering NumericValueQuery(numericName={NumericName},numericVal={NumericVal})", numericName, numericVal);
			Logger.LogDebug("<<< Exiting NumericValueQuery(numericName={NumericName},numericVal={NumericVal})", numericName, numericVal);
            return NumericRangeQuery.NewIntRange(numericName, numericVal, numericVal, true, true);
        }

        /// <summary>Add Field to Doc when supplied fieldValue > 0.</summary>
        private static void AddIntField(Document doc, int fieldValue, string fieldTag)
        {
			Logger.LogDebug(">>> Entering AddIntField(doc={Doc},fieldValue={FieldValue},fieldTag={FieldTag})", doc, fieldValue, fieldTag);
            if (fieldValue > 0)
            {
                doc.Add(new NumericField(fieldTag, Field.Store.YES, true).SetIntValue(fieldValue));
            }
			Logger.LogDebug("<<< Exiting AddIntField(doc={Doc},fieldValue={FieldValue},fieldTag={FieldTag})", doc, fieldValue, fieldTag);
        }

        private static string StripTagsRetainAttributes(string html, IEnumerable<string> attributes, bool decoded, bool retainSpace)
        {
			Logger.LogDebug(">>> Entering StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace})", html, attributes, decoded, retainSpace);
            var attributesList = attributes as IList<string> ?? attributes.ToList();
            var strippedString = html;
			Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): strippedString → {StrippedString}", html, attributes, decoded, retainSpace, strippedString);
            var emptySpace = retainSpace ? " " : string.Empty;
			Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): emptySpace → {EmptySpace}", html, attributes, decoded, retainSpace, emptySpace);
			Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): attributesList → {AttributesList}", html, attributes, decoded, retainSpace, attributesList);

            if (!string.IsNullOrEmpty(strippedString))
            {
                // Remove all opening HTML Tags with no attributes
                try
                {
                    strippedString = StripOpeningTagsRegex.Replace(strippedString, emptySpace);
                }
                catch (RegexMatchTimeoutException ex)
                {
					Logger.LogError("Exception in StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): {Arg4}", html, attributes, decoded, retainSpace, ex.getMessage());
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return string.Empty;
                }
                catch
                {
					Logger.LogError("Exception in StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): {Arg4}", html, attributes, decoded, retainSpace, ex.Message);
                    throw;
                }

                // Remove all closing HTML Tags
                try
                {
                    strippedString = StripClosingTagsRegex.Replace(strippedString, emptySpace);
                }
                catch (RegexMatchTimeoutException ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return string.Empty;
                }
                catch
                {
                    throw;
                }
            }

            if (!string.IsNullOrEmpty(strippedString))
            {
                var list = new List<string>();
				Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): list → {List}", html, attributes, decoded, retainSpace, list);
                try
                {
                    foreach (var match in HtmlTagsRegex.Matches(strippedString).Cast<Match>())
                    {
                        var captures = match.Groups["attr"].Captures;
						Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): captures → {Captures}", html, attributes, decoded, retainSpace, captures);
                        foreach (var capture in captures.Cast<Capture>())
                        {
                            var val = capture.Value.Trim();
							Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): val → {Val}", html, attributes, decoded, retainSpace, val);
                            var pos = val.IndexOf('=');
							Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): pos → {Pos}", html, attributes, decoded, retainSpace, pos);
                            if (pos > 0)
                            {
                                var attr = val.Substring(0, pos).Trim();
								Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): attr → {Attr}", html, attributes, decoded, retainSpace, attr);
                                if (attributesList.Contains(attr))
                                {
                                    try
                                    {
                                        var text = AttrTextRegex.Match(val).Groups["text"].Value.Trim();
										Logger.LogDebug("StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace}): text → {Text}", html, attributes, decoded, retainSpace, text);
                                        if (text.Length > 0 && !list.Contains(text))
                                        {
                                            list.Add(text);
                                        }
                                    }
                                    catch (RegexMatchTimeoutException ex)
                                    {
                                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                                        return string.Empty;
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                }
                            }
                        }

                        if (list.Count > 0)
                        {
                            strippedString = strippedString.Replace(match.ToString(), string.Join(" ", list));
                            list.Clear();
                        }
                    }
                }
                catch (RegexMatchTimeoutException ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return string.Empty;
                }
                catch
                {
                    throw;
                }
            }

            // If not decoded, decode and strip again. Becareful with recursive
            if (!decoded)
            {
                strippedString = StripTagsRetainAttributes(HttpUtility.HtmlDecode(strippedString), attributesList, true, retainSpace);
            }

			Logger.LogDebug("<<< Exiting StripTagsRetainAttributes(html={Html},attributes={Attributes},decoded={Decoded},retainSpace={RetainSpace})", html, attributes, decoded, retainSpace);
            return strippedString;
        }

        private object SearchDocumentTypeDisplayNameCallBack(CacheItemArgs cacheItem)
        {
			Logger.LogDebug(">>> Entering SearchDocumentTypeDisplayNameCallBack(cacheItem={CacheItem})", cacheItem);
            var data = new Dictionary<string, string>();
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
			Logger.LogDebug("SearchDocumentTypeDisplayNameCallBack(cacheItem={CacheItem}): data → {Data}", cacheItem, data);
                var searchContentSources = this.GetSearchContentSourceList(portal.PortalId);
				Logger.LogDebug("SearchDocumentTypeDisplayNameCallBack(cacheItem={CacheItem}): searchContentSources → {SearchContentSources}", cacheItem, searchContentSources);
                foreach (var searchContentSource in searchContentSources)
                {
                    var key = $"{searchContentSource.SearchTypeId}-{searchContentSource.ModuleDefinitionId}-{Thread.CurrentThread.CurrentCulture}";
					Logger.LogWarning("SearchDocumentTypeDisplayNameCallBack(cacheItem={CacheItem}): key → {Key}", cacheItem, key);
                    if (!data.ContainsKey(key))
                    {
                        data.Add(key, searchContentSource.LocalizedName);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting SearchDocumentTypeDisplayNameCallBack(cacheItem={CacheItem})", cacheItem);
            return data;
        }

        private void AddSearchDocumentInternal(SearchDocument searchDocument, bool autoCommit)
        {
			Logger.LogDebug(">>> Entering AddSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit})", searchDocument, autoCommit);
            Requires.NotNull("SearchDocument", searchDocument);
            Requires.NotNullOrEmpty("UniqueKey", searchDocument.UniqueKey);
            Requires.NotNegative("SearchTypeId", searchDocument.SearchTypeId);
            Requires.PropertyNotEqualTo("searchDocument", "SearchTypeId", searchDocument.SearchTypeId, 0);
            Requires.PropertyNotEqualTo("searchDocument", "ModifiedTimeUtc", searchDocument.ModifiedTimeUtc.ToString(CultureInfo.InvariantCulture), DateTime.MinValue.ToString(CultureInfo.InvariantCulture));

            if (searchDocument.SearchTypeId == this.moduleSearchTypeId)
            {
                if (searchDocument.ModuleDefId <= 0)
                {
					Logger.LogWarning("Throwing ArgumentException from AddSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit})", searchDocument, autoCommit);
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleDefIdMustBeGreaterThanZero", "ModuleDefId must be greater than zero when SearchTypeId is for a module"));
                }

                if (searchDocument.ModuleId <= 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleIdMustBeGreaterThanZero", "ModuleId must be greater than zero when SearchTypeId is for a module"));
                }
            }
            else
            {
                if (searchDocument.ModuleDefId > 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleDefIdWhenSearchTypeForModule", "ModuleDefId is needed only when SearchTypeId is for a module"));
                }

                if (searchDocument.ModuleId > 0)
                {
                    throw new ArgumentException(Localization.GetExceptionMessage("ModuleIdWhenSearchTypeForModule", "ModuleId is needed only when SearchTypeId is for a module"));
                }
            }

            var doc = new Document();
			Logger.LogDebug("AddSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit}): doc → {Doc}", searchDocument, autoCommit, doc);
            var sb = new StringBuilder();
			Logger.LogDebug("AddSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit}): sb → {Sb}", searchDocument, autoCommit, sb);

            // TODO - Set setOmitTermFreqAndPositions
            // TODO - Check if Numeric fields need to have Store.YES or Store.NO
            // TODO - Should ModifiedTime be mandatory
            // TODO - Is Locale needed for a single-language site
            // TODO - Sorting

            // Field.Store.YES    | Stores the value. When the value is stored, the original String in its entirety is recorded in the index
            // Field.Store.NO     | Doesn’t store the value. This option is often used along with Index.ANALYZED to index a large text field that doesn’t need to be retrieved
            //                   | in its original form, such as bodies of web pages, or any other type of text document.
            // Index.ANALYZED     | Use the analyzer to break the field’s value into a stream of separate tokens and make each token searchable
            // Index.NOT_ANALYZED | Do index the field, but don’t analyze the String value.Instead, treat the Field’s entire value as a single token and make that token searchable.

            // Generic and Additional SearchDocument Params
            this.AddSearchDocumentParamters(doc, searchDocument, sb);

            // Remove the existing document from Lucene
            this.DeleteSearchDocumentInternal(searchDocument, false);

            // Add only when Document is active. The previous call would have otherwise deleted the document if it existed earlier
            if (searchDocument.IsActive)
            {
                Thread.SetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo), searchDocument);
                try
                {
                    LuceneController.Instance.Add(doc);
                }
                finally
                {
                    Thread.SetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo), null);
                }
            }

            if (autoCommit)
            {
                this.Commit();
            }
			Logger.LogDebug("<<< Exiting AddSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit})", searchDocument, autoCommit);
        }

        private void DeleteSearchDocumentInternal(SearchDocument searchDocument, bool autoCommit)
        {
			Logger.LogDebug(">>> Entering DeleteSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit})", searchDocument, autoCommit);
            var query = new BooleanQuery();

			Logger.LogDebug("DeleteSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit}): query → {Query}", searchDocument, autoCommit, query);
            if (searchDocument.SearchTypeId > -1)
            {
                query.Add(NumericValueQuery(Constants.SearchTypeTag, searchDocument.SearchTypeId), Occur.MUST);
            }

            if (searchDocument.PortalId > -1)
            {
                query.Add(NumericValueQuery(Constants.PortalIdTag, searchDocument.PortalId), Occur.MUST);
            }

            if (searchDocument.RoleId > -1)
            {
                query.Add(NumericValueQuery(Constants.RoleIdTag, searchDocument.RoleId), Occur.MUST);
            }

            if (searchDocument.ModuleDefId > 0)
            {
                query.Add(NumericValueQuery(Constants.ModuleDefIdTag, searchDocument.ModuleDefId), Occur.MUST);
            }

            if (searchDocument.ModuleId > 0)
            {
                query.Add(NumericValueQuery(Constants.ModuleIdTag, searchDocument.ModuleId), Occur.MUST);
            }

            if (searchDocument.TabId > 0)
            {
                query.Add(NumericValueQuery(Constants.TabIdTag, searchDocument.TabId), Occur.MUST);
            }

            if (searchDocument.AuthorUserId > 0)
            {
                query.Add(NumericValueQuery(Constants.AuthorIdTag, searchDocument.AuthorUserId), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchDocument.UniqueKey))
            {
                query.Add(new TermQuery(new Term(Constants.UniqueKeyTag, searchDocument.UniqueKey)), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchDocument.QueryString))
            {
                query.Add(new TermQuery(new Term(Constants.QueryStringTag, searchDocument.QueryString)), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchDocument.CultureCode))
            {
                query.Add(NumericValueQuery(Constants.LocaleTag, Localization.GetCultureLanguageID(searchDocument.CultureCode)), Occur.MUST);
            }

            LuceneController.Instance.Delete(query);

            if (autoCommit)
            {
                this.Commit();
            }
			Logger.LogDebug("<<< Exiting DeleteSearchDocumentInternal(searchDocument={SearchDocument},autoCommit={AutoCommit})", searchDocument, autoCommit);
        }

        private void AddSearchDocumentParamters(Document doc, SearchDocument searchDocument, StringBuilder sb)
        {
			Logger.LogDebug(">>> Entering AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb})", doc, searchDocument, sb);
            // mandatory fields
            doc.Add(new Field(Constants.UniqueKeyTag, SearchHelper.Instance.StripTagsNoAttributes(searchDocument.UniqueKey, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new NumericField(Constants.PortalIdTag, Field.Store.YES, true).SetIntValue(searchDocument.PortalId));
            doc.Add(new NumericField(Constants.SearchTypeTag, Field.Store.YES, true).SetIntValue(searchDocument.SearchTypeId));
            doc.Add(!string.IsNullOrEmpty(searchDocument.CultureCode)
                        ? new NumericField(Constants.LocaleTag, Field.Store.YES, true).SetIntValue(Localization.GetCultureLanguageID(searchDocument.CultureCode))
                        : new NumericField(Constants.LocaleTag, Field.Store.YES, true).SetIntValue(-1));

            if (!string.IsNullOrEmpty(searchDocument.Title))
            {
                var field = new Field(Constants.TitleTag, StripTagsRetainAttributes(searchDocument.Title, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
				Logger.LogDebug("AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb}): field → {Field}", doc, searchDocument, sb, field);
                if (this.titleBoost > 0 && this.titleBoost != Constants.StandardLuceneBoost)
                {
                    field.Boost = this.titleBoost / 10f;
					Logger.LogDebug("AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb}): field.Boost → {Arg3}", doc, searchDocument, sb, field.Boost);
                }

                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(searchDocument.Description))
            {
                var field = new Field(Constants.DescriptionTag, StripTagsRetainAttributes(searchDocument.Description, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (this.descriptionBoost > 0 && this.descriptionBoost != Constants.StandardLuceneBoost)
                {
                    field.Boost = this.descriptionBoost / 10f;
                }

                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(searchDocument.Body))
            {
                doc.Add(new Field(Constants.BodyTag, StripTagsRetainAttributes(searchDocument.Body, HtmlAttributesToRetain, false, true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            }

            if (!string.IsNullOrEmpty(searchDocument.Url))
            {
                doc.Add(new Field(Constants.UrlTag, SearchHelper.Instance.StripTagsNoAttributes(searchDocument.Url, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            if (!string.IsNullOrEmpty(searchDocument.QueryString))
            {
                doc.Add(new Field(Constants.QueryStringTag, searchDocument.QueryString, Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            foreach (var kvp in searchDocument.Keywords)
            {
                var key = kvp.Key.ToLowerInvariant();
				Logger.LogWarning("AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb}): key → {Key}", doc, searchDocument, sb, key);
                var needAnalyzed = Constants.FieldsNeedAnalysis.Contains(key);
				Logger.LogDebug("AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb}): needAnalyzed → {NeedAnalyzed}", doc, searchDocument, sb, needAnalyzed);
                var field = new Field(SearchHelper.Instance.StripTagsNoAttributes(Constants.KeywordsPrefixTag + kvp.Key, true), SearchHelper.Instance.StripTagsNoAttributes(kvp.Value, true), Field.Store.YES, needAnalyzed ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED);
                switch (key)
                {
                    case Constants.TitleTag:
                        if (this.titleBoost > 0 && this.titleBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this.titleBoost / 10f;
                        }

                        break;
                    case Constants.SubjectTag:
                        if (this.contentBoost > 0 && this.contentBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this.contentBoost / 10f;
                        }

                        break;
                    case Constants.CommentsTag:
                        if (this.descriptionBoost > 0 && this.descriptionBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this.descriptionBoost / 10f;
                        }

                        break;
                    case Constants.AuthorNameTag:
                        if (this.authorBoost > 0 && this.authorBoost != Constants.StandardLuceneBoost)
                        {
                            field.Boost = this.authorBoost / 10f;
                        }

                        break;
                }

                doc.Add(field);
                sb.Append(SearchHelper.Instance.StripTagsNoAttributes(kvp.Value, true)).Append(' ');
            }

            foreach (var kvp in searchDocument.NumericKeys)
            {
                doc.Add(new NumericField(SearchHelper.Instance.StripTagsNoAttributes(Constants.NumericKeyPrefixTag + kvp.Key, true), Field.Store.YES, true).SetIntValue(kvp.Value));
            }

            bool tagBoostApplied = false;
			Logger.LogDebug("AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb}): tagBoostApplied → {TagBoostApplied}", doc, searchDocument, sb, tagBoostApplied);
            foreach (var tag in searchDocument.Tags)
            {
                var field = new Field(Constants.Tag, SearchHelper.Instance.StripTagsNoAttributes(tag.ToLowerInvariant(), true), Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                if (!tagBoostApplied)
                {
                    if (this.tagBoost > 0 && this.tagBoost != Constants.StandardLuceneBoost)
                    {
                        field.Boost = this.tagBoost / 10f;
                        tagBoostApplied = true;
                    }
                }

                doc.Add(field);
            }

            AddIntField(doc, searchDocument.TabId, Constants.TabIdTag);
            AddIntField(doc, searchDocument.ModuleDefId, Constants.ModuleDefIdTag);
            AddIntField(doc, searchDocument.ModuleId, Constants.ModuleIdTag);
            AddIntField(doc, searchDocument.AuthorUserId, Constants.AuthorIdTag);
            AddIntField(doc, searchDocument.RoleId, Constants.RoleIdTag);

            if (searchDocument.AuthorUserId > 0)
            {
                var user = UserController.Instance.GetUserById(searchDocument.PortalId, searchDocument.AuthorUserId);
				Logger.LogDebug("AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb}): user → {User}", doc, searchDocument, sb, user);
                if (user != null && !string.IsNullOrEmpty(user.DisplayName))
                {
                    var field = new Field(Constants.AuthorNameTag, user.DisplayName, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                    if (this.authorBoost > 0 && this.authorBoost != Constants.StandardLuceneBoost)
                    {
                        field.Boost = this.authorBoost / 10f;
                    }

                    doc.Add(field);
                }
            }

            if (!string.IsNullOrEmpty(searchDocument.Permissions))
            {
                doc.Add(new Field(Constants.PermissionsTag, SearchHelper.Instance.StripTagsNoAttributes(searchDocument.Permissions, true), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }

            doc.Add(
                new NumericField(Constants.ModifiedTimeTag, Field.Store.YES, true)
                .SetLongValue(
                    long.Parse(
                        searchDocument.ModifiedTimeUtc.ToString(
                            Constants.DateTimeFormat,
                            CultureInfo.InvariantCulture),
                        CultureInfo.InvariantCulture)));

            if (sb.Length > 0)
            {
                var field = new Field(Constants.ContentTag, SearchHelper.Instance.StripTagsNoAttributes(sb.ToString(), true), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                doc.Add(field);
                if (this.contentBoost > 0 && this.contentBoost != Constants.StandardLuceneBoost)
                {
                    field.Boost = this.contentBoost / 10f;
                }
            }
			Logger.LogDebug("<<< Exiting AddSearchDocumentParamters(doc={Doc},searchDocument={SearchDocument},sb={Sb})", doc, searchDocument, sb);
        }
    }
}
