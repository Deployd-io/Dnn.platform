// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>  The Impl Controller class for Search.</summary>
    internal class SearchControllerImpl : ISearchController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchControllerImpl));
        private const string SeacrchContollersCacheKey = "SearchControllers";

        private readonly IServiceProvider serviceProvider;
        private readonly int moduleSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId;

        /// <summary>Initializes a new instance of the <see cref="SearchControllerImpl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public SearchControllerImpl()
            : this(null)
        {
		Logger.LogDebug(">>> Entering SearchControllerImpl()");
        }

		Logger.LogDebug("<<< Exiting SearchControllerImpl()");
        /// <summary>Initializes a new instance of the <see cref="SearchControllerImpl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public SearchControllerImpl(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? Globals.DependencyProvider;
			Logger.LogDebug("SearchControllerImpl(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);
        }

        /// <inheritdoc />
        public SearchResults SiteSearch(SearchQuery searchQuery)
        {
			Logger.LogDebug(">>> Entering SiteSearch(searchQuery={SearchQuery})", searchQuery);
            var results = this.GetResults(searchQuery);
			Logger.LogDebug("SiteSearch(searchQuery={SearchQuery}): results → {Results}", searchQuery, results);
			Logger.LogDebug("<<< Exiting SiteSearch(searchQuery={SearchQuery})", searchQuery);
            return new SearchResults { TotalHits = results.Item1, Results = results.Item2, };
        }

        /// <inheritdoc />
        public SearchResults ModuleSearch(SearchQuery searchQuery)
        {
			Logger.LogDebug(">>> Entering ModuleSearch(searchQuery={SearchQuery})", searchQuery);
            searchQuery.SearchTypeIds = new List<int> { this.moduleSearchTypeId };
            var results = this.GetResults(searchQuery);
			Logger.LogDebug("ModuleSearch(searchQuery={SearchQuery}): results → {Results}", searchQuery, results);
			Logger.LogDebug("ModuleSearch(searchQuery={SearchQuery}): searchQuery.SearchTypeIds → {Arg1}", searchQuery, searchQuery.SearchTypeIds);
			Logger.LogDebug("<<< Exiting ModuleSearch(searchQuery={SearchQuery})", searchQuery);
            return new SearchResults { TotalHits = results.Item1, Results = results.Item2 };
        }

        private static void FillTagsValues(Document doc, SearchResult result)
        {
			Logger.LogDebug(">>> Entering FillTagsValues(doc={Doc},result={Result})", doc, result);
            foreach (var field in doc.GetFields())
            {
                if (field.StringValue == null)
                {
                    continue;
                }

                int intField;
                switch (field.Name)
                {
                    case Constants.UniqueKeyTag:
                        result.UniqueKey = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.UniqueKey → {Arg2}", doc, result, result.UniqueKey);
                        break;
                    case Constants.TitleTag:
                        var title = field.StringValue;
                        result.Title = title;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.Title → {Arg2}", doc, result, result.Title);
                        break;
                    case Constants.BodyTag:
                        result.Body = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.Body → {Arg2}", doc, result, result.Body);
                        break;
                    case Constants.DescriptionTag:
                        result.Description = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.Description → {Arg2}", doc, result, result.Description);
                        break;
                    case Constants.Tag:
                        result.Tags = result.Tags.Concat(new[] { field.StringValue });
                        break;
                    case Constants.PermissionsTag:
                        result.Permissions = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.Permissions → {Arg2}", doc, result, result.Permissions);
                        break;
                    case Constants.QueryStringTag:
                        result.QueryString = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.QueryString → {Arg2}", doc, result, result.QueryString);
                        break;
                    case Constants.UrlTag:
                        result.Url = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.Url → {Arg2}", doc, result, result.Url);
                        break;
                    case Constants.SearchTypeTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.SearchTypeId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.SearchTypeId → {Arg2}", doc, result, result.SearchTypeId);
                        }

                        break;
                    case Constants.ModuleIdTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.ModuleId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.ModuleId → {Arg2}", doc, result, result.ModuleId);
                        }

                        break;
                    case Constants.ModuleDefIdTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.ModuleDefId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.ModuleDefId → {Arg2}", doc, result, result.ModuleDefId);
                        }

                        break;
                    case Constants.PortalIdTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.PortalId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.PortalId → {Arg2}", doc, result, result.PortalId);
                        }

                        break;
                    case Constants.AuthorIdTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.AuthorUserId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.AuthorUserId → {Arg2}", doc, result, result.AuthorUserId);
                        }

                        break;
                    case Constants.RoleIdTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.RoleId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.RoleId → {Arg2}", doc, result, result.RoleId);
                        }

                        break;
                    case Constants.AuthorNameTag:
                        result.AuthorName = field.StringValue;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.AuthorName → {Arg2}", doc, result, result.AuthorName);
                        break;
                    case Constants.TabIdTag:
                        if (int.TryParse(field.StringValue, out intField))
                        {
                            result.TabId = intField;
							Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.TabId → {Arg2}", doc, result, result.TabId);
                        }

                        break;
                    case Constants.ModifiedTimeTag:
                        DateTime.TryParseExact(field.StringValue, Constants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var modifiedTimeUtc);
                        result.ModifiedTimeUtc = modifiedTimeUtc;
						Logger.LogDebug("FillTagsValues(doc={Doc},result={Result}): result.ModifiedTimeUtc → {Arg2}", doc, result, result.ModifiedTimeUtc);
                        break;
                    default:
                        if (field.Name.StartsWith(Constants.NumericKeyPrefixTag, StringComparison.Ordinal))
                        {
                            var key = field.Name.Substring(Constants.NumericKeyPrefixTag.Length);
							Logger.LogWarning("FillTagsValues(doc={Doc},result={Result}): key → {Key}", doc, result, key);
                            if (int.TryParse(field.StringValue, out intField))
                            {
                                if (!result.NumericKeys.ContainsKey(key))
                                {
                                    result.NumericKeys.Add(key, intField);
                                }
                            }
                        }
                        else if (field.Name.StartsWith(Constants.KeywordsPrefixTag, StringComparison.Ordinal))
                        {
                            var key = field.Name.Substring(Constants.KeywordsPrefixTag.Length);
                            if (!result.Keywords.ContainsKey(key))
                            {
                                result.Keywords.Add(key, field.StringValue);
                            }
                        }

                        break;
                }
            }
			Logger.LogDebug("<<< Exiting FillTagsValues(doc={Doc},result={Result})", doc, result);
        }

        private static string GetSnippet(SearchResult searchResult, LuceneResult luceneResult)
        {
			Logger.LogDebug(">>> Entering GetSnippet(searchResult={SearchResult},luceneResult={LuceneResult})", searchResult, luceneResult);
            var sb = new StringBuilder();

			Logger.LogDebug("GetSnippet(searchResult={SearchResult},luceneResult={LuceneResult}): sb → {Sb}", searchResult, luceneResult, sb);
            if (!string.IsNullOrEmpty(luceneResult.TitleSnippet))
            {
                sb.Append(luceneResult.TitleSnippet + "...");
            }

            if (!string.IsNullOrEmpty(luceneResult.DescriptionSnippet))
            {
                sb.Append(luceneResult.DescriptionSnippet + "...");
            }

            if (!string.IsNullOrEmpty(luceneResult.TagSnippet))
            {
                sb.Append(luceneResult.TagSnippet + "...");
            }

            if (!string.IsNullOrEmpty(luceneResult.BodySnippet))
            {
                sb.Append(luceneResult.BodySnippet + "...");
            }

            if (!string.IsNullOrEmpty(luceneResult.AuthorSnippet))
            {
                sb.Append(luceneResult.AuthorSnippet + "...");
            }

            if (!string.IsNullOrEmpty(luceneResult.ContentSnippet))
            {
                sb.Append(luceneResult.ContentSnippet + "...");
            }

            var snippet = sb.ToString();
			Logger.LogDebug("GetSnippet(searchResult={SearchResult},luceneResult={LuceneResult}): snippet → {Snippet}", searchResult, luceneResult, snippet);
            if (string.IsNullOrEmpty(snippet))
            {
                snippet = searchResult.Title;
            }

			Logger.LogDebug("<<< Exiting GetSnippet(searchResult={SearchResult},luceneResult={LuceneResult})", searchResult, luceneResult);
            return snippet;
        }

        private static SearchResult GetPartialSearchResult(Document doc, SearchQuery searchQuery)
        {
			Logger.LogDebug(">>> Entering GetPartialSearchResult(doc={Doc},searchQuery={SearchQuery})", doc, searchQuery);
            var result = new SearchResult { SearchContext = searchQuery.SearchContext };
            var localeField = doc.GetField(Constants.LocaleTag);
			Logger.LogDebug("GetPartialSearchResult(doc={Doc},searchQuery={SearchQuery}): localeField → {LocaleField}", doc, searchQuery, localeField);
			Logger.LogDebug("GetPartialSearchResult(doc={Doc},searchQuery={SearchQuery}): result → {Result}", doc, searchQuery, result);

            if (localeField != null)
            {
                int id;
                result.CultureCode = int.TryParse(localeField.StringValue, out id) && id >= 0
                    ? LocaleController.Instance.GetLocale(id).Code : Null.NullString;
            }

            FillTagsValues(doc, result);
			Logger.LogDebug("<<< Exiting GetPartialSearchResult(doc={Doc},searchQuery={SearchQuery})", doc, searchQuery);
            return result;
        }

        private static Sort GetSort(SearchQuery query)
        {
			Logger.LogDebug(">>> Entering GetSort(query={Query})");
            var sort = Sort.RELEVANCE; // default sorting - relevance is always descending.
            if (query.SortField != SortFields.Relevance)
            {
                var reverse = query.SortDirection != SortDirections.Ascending;

                switch (query.SortField)
                {
                    case SortFields.LastModified:
                        sort = new Sort(new SortField(Constants.ModifiedTimeTag, SortField.LONG, reverse));
                        break;
                    case SortFields.Title:
                        sort = new Sort(new SortField(Constants.TitleTag, SortField.STRING, reverse));
                        break;
                    case SortFields.Tag:
                        sort = new Sort(new SortField(Constants.Tag, SortField.STRING, reverse));
                        break;
                    case SortFields.NumericKey:
                        sort = new Sort(new SortField(Constants.NumericKeyPrefixTag + query.CustomSortField, SortField.INT, reverse));
                        break;
                    case SortFields.Keyword:
                        sort = new Sort(new SortField(Constants.KeywordsPrefixTag + query.CustomSortField, SortField.STRING, reverse));
                        break;
                    case SortFields.CustomStringField:
                        sort = new Sort(new SortField(query.CustomSortField, SortField.STRING, reverse));
                        break;
                    case SortFields.CustomNumericField:
                        sort = new Sort(new SortField(query.CustomSortField, SortField.INT, reverse));
                        break;
                    default:
                        sort = Sort.RELEVANCE;
                        break;
                }
            }

			Logger.LogDebug("<<< Exiting GetSort(query={Query})");
            return sort;
        }

        private Tuple<int, IList<SearchResult>> GetResults(SearchQuery searchQuery)
        {
            Requires.NotNull("Query", searchQuery);
            Requires.PropertyNotEqualTo("searchQuery", "SearchTypeIds", searchQuery.SearchTypeIds.Count(), 0);

            if ((searchQuery.ModuleId > 0) && (searchQuery.SearchTypeIds.Count() > 1 || !searchQuery.SearchTypeIds.Contains(this.moduleSearchTypeId)))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ModuleIdMustHaveSearchTypeIdForModule", "ModuleId based search must have SearchTypeId for a module only"));
            }

            if (searchQuery.SortField is SortFields.CustomStringField or SortFields.CustomNumericField or SortFields.NumericKey or SortFields.Keyword)
            {
                Requires.NotNullOrEmpty("CustomSortField", searchQuery.CustomSortField);
            }

            var query = new BooleanQuery();
            if (!string.IsNullOrEmpty(searchQuery.KeyWords))
            {
                try
                {
                    var allowLeadingWildcard = HostController.Instance.GetString("Search_AllowLeadingWildcard", "N") == "Y" || searchQuery.AllowLeadingWildcard;
                    var keywords = SearchHelper.Instance.RephraseSearchText(searchQuery.KeyWords, searchQuery.WildCardSearch, allowLeadingWildcard);

                    // don't use stemming analyzer for exact matches or non-analyzed fields (e.g. Tags)
                    var analyzer = LuceneController.Instance.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
                    var nonStemmerAnalyzer = new SearchQueryAnalyzer(false);
                    var keywordQuery = new BooleanQuery();
                    foreach (var fieldToSearch in Constants.KeyWordSearchFields)
                    {
                        var parserContent = new QueryParser(Constants.LuceneVersion, fieldToSearch, fieldToSearch == Constants.Tag ? nonStemmerAnalyzer : analyzer);
                        parserContent.AllowLeadingWildcard = allowLeadingWildcard;
                        var parsedQueryContent = parserContent.Parse(keywords);
                        keywordQuery.Add(parsedQueryContent, Occur.SHOULD);
                    }

                    query.Add(keywordQuery, Occur.MUST);
                }
                catch (Exception)
                {
                    foreach (var word in searchQuery.KeyWords.Split(' '))
                    {
                        query.Add(new TermQuery(new Term(Constants.ContentTag, word.ToLowerInvariant())), Occur.SHOULD);
                    }
                }
            }

            var portalIdQuery = new BooleanQuery();
            foreach (var portalId in searchQuery.PortalIds)
            {
                portalIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.PortalIdTag, portalId, portalId, true, true), Occur.SHOULD);
            }

            if (searchQuery.PortalIds.Any())
            {
                query.Add(portalIdQuery, Occur.MUST);
            }

            this.ApplySearchTypeIdFilter(query, searchQuery);

            if (searchQuery.BeginModifiedTimeUtc > DateTime.MinValue && searchQuery.EndModifiedTimeUtc >= searchQuery.BeginModifiedTimeUtc)
            {
                query.Add(
                    NumericRangeQuery.NewLongRange(
                        Constants.ModifiedTimeTag,
                        long.Parse(
                            searchQuery.BeginModifiedTimeUtc.ToString(Constants.DateTimeFormat, CultureInfo.InvariantCulture),
                            CultureInfo.CurrentCulture),
                        long.Parse(
                            searchQuery.EndModifiedTimeUtc.ToString(Constants.DateTimeFormat, CultureInfo.InvariantCulture),
                            CultureInfo.CurrentCulture),
                        true,
                        true),
                    Occur.MUST);
            }

            if (searchQuery.RoleId > 0)
            {
                query.Add(NumericRangeQuery.NewIntRange(Constants.RoleIdTag, searchQuery.RoleId, searchQuery.RoleId, true, true), Occur.MUST);
            }

            foreach (var tag in searchQuery.Tags)
            {
                var text = tag.ToLowerInvariant();
                if (HtmlUtils.ContainsEntity(text))
                {
                    text = System.Net.WebUtility.HtmlDecode(text);
                }

                query.Add(new TermQuery(new Term(Constants.Tag, text)), Occur.MUST);
            }

            foreach (var kvp in searchQuery.CustomKeywords)
            {
                var field = SearchHelper.Instance.StripTagsNoAttributes(Constants.KeywordsPrefixTag + kvp.Key, true);
                query.Add(new TermQuery(new Term(field, kvp.Value)), Occur.MUST);
            }

            foreach (var kvp in searchQuery.NumericKeys)
            {
                query.Add(NumericRangeQuery.NewIntRange(Constants.NumericKeyPrefixTag + kvp.Key, kvp.Value, kvp.Value, true, true), Occur.MUST);
            }

            if (!string.IsNullOrEmpty(searchQuery.CultureCode))
            {
                var localeQuery = new BooleanQuery();

                var languageId = Localization.GetCultureLanguageID(searchQuery.CultureCode);
                localeQuery.Add(NumericRangeQuery.NewIntRange(Constants.LocaleTag, languageId, languageId, true, true), Occur.SHOULD);
                localeQuery.Add(NumericRangeQuery.NewIntRange(Constants.LocaleTag, Null.NullInteger, Null.NullInteger, true, true), Occur.SHOULD);
                query.Add(localeQuery, Occur.MUST);
            }

            var luceneQuery = new LuceneQuery
            {
                Query = query,
                Sort = GetSort(searchQuery),
                PageIndex = searchQuery.PageIndex,
                PageSize = searchQuery.PageSize,
                TitleSnippetLength = searchQuery.TitleSnippetLength,
                BodySnippetLength = searchQuery.BodySnippetLength,
            };

            return this.GetSecurityTrimmedResults(searchQuery, luceneQuery);
        }

        private void ApplySearchTypeIdFilter(BooleanQuery query, SearchQuery searchQuery)
        {
			Logger.LogDebug(">>> Entering ApplySearchTypeIdFilter(query={Query},searchQuery={SearchQuery})", query, searchQuery);
            // Special handling for Module Search
            if (searchQuery.SearchTypeIds.Count() == 1 && searchQuery.SearchTypeIds.Contains(this.moduleSearchTypeId))
            {
                // When moduleid is specified, we ignore other searchtypeid or moduledefinitionid. Major security check
                if (searchQuery.ModuleId > 0)
                {
                    // this is the main hook for module based search. Occur.MUST is a requirement for this condition or else results from other modules will be found
                    query.Add(NumericRangeQuery.NewIntRange(Constants.ModuleIdTag, searchQuery.ModuleId, searchQuery.ModuleId, true, true), Occur.MUST);
                }
                else
                {
                    var modDefQuery = new BooleanQuery();
					Logger.LogDebug("ApplySearchTypeIdFilter(query={Query},searchQuery={SearchQuery}): modDefQuery → {ModDefQuery}", query, searchQuery, modDefQuery);
                    foreach (var moduleDefId in searchQuery.ModuleDefIds)
                    {
                        modDefQuery.Add(NumericRangeQuery.NewIntRange(Constants.ModuleDefIdTag, moduleDefId, moduleDefId, true, true), Occur.SHOULD);
                    }

                    if (searchQuery.ModuleDefIds.Any())
                    {
                        query.Add(modDefQuery, Occur.MUST); // Note the MUST
                    }
                }

                query.Add(NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, this.moduleSearchTypeId, this.moduleSearchTypeId, true, true), Occur.MUST);
            }
            else
            {
                var searchTypeIdQuery = new BooleanQuery();
				Logger.LogDebug("ApplySearchTypeIdFilter(query={Query},searchQuery={SearchQuery}): searchTypeIdQuery → {SearchTypeIdQuery}", query, searchQuery, searchTypeIdQuery);
                foreach (var searchTypeId in searchQuery.SearchTypeIds)
                {
                    if (searchTypeId == this.moduleSearchTypeId)
                    {
                        foreach (var moduleDefId in searchQuery.ModuleDefIds.OrderBy(id => id))
                        {
                            searchTypeIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.ModuleDefIdTag, moduleDefId, moduleDefId, true, true), Occur.SHOULD);
                        }

                        if (!searchQuery.ModuleDefIds.Any())
                        {
                            searchTypeIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, searchTypeId, searchTypeId, true, true), Occur.SHOULD);
                        }
                    }
                    else
                    {
                        searchTypeIdQuery.Add(NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, searchTypeId, searchTypeId, true, true), Occur.SHOULD);
                    }
                }

                query.Add(searchTypeIdQuery, Occur.MUST);
            }
			Logger.LogDebug("<<< Exiting ApplySearchTypeIdFilter(query={Query},searchQuery={SearchQuery})", query, searchQuery);
        }

        private SearchResult GetSearchResultFromLuceneResult(LuceneResult luceneResult)
        {
			Logger.LogDebug(">>> Entering GetSearchResultFromLuceneResult(luceneResult={LuceneResult})", luceneResult);
            var result = new SearchResult();
            var doc = luceneResult.Document;
			Logger.LogDebug("GetSearchResultFromLuceneResult(luceneResult={LuceneResult}): result → {Result}", luceneResult, result);
			Logger.LogDebug("GetSearchResultFromLuceneResult(luceneResult={LuceneResult}): result.DisplayScore → {Arg1}", luceneResult, result.DisplayScore);
			Logger.LogDebug("GetSearchResultFromLuceneResult(luceneResult={LuceneResult}): result.Score → {Arg1}", luceneResult, result.Score);
            result.DisplayScore = luceneResult.DisplayScore;
            result.Score = luceneResult.Score;

            // set culture code of result
            result.CultureCode = string.Empty;
			Logger.LogDebug("GetSearchResultFromLuceneResult(luceneResult={LuceneResult}): result.CultureCode → {Arg1}", luceneResult, result.CultureCode);
            var localeField = luceneResult.Document.GetField(Constants.LocaleTag);
			Logger.LogDebug("GetSearchResultFromLuceneResult(luceneResult={LuceneResult}): localeField → {LocaleField}", luceneResult, localeField);
            if (localeField != null)
            {
                if (int.TryParse(localeField.StringValue, out var id) && id >= 0)
                {
                    result.CultureCode = LocaleController.Instance.GetLocale(id).Code;
                }
                else
                {
                    result.CultureCode = Null.NullString;
                }
            }

            FillTagsValues(doc, result);
            result.Snippet = GetSnippet(result, luceneResult);
			Logger.LogDebug("<<< Exiting GetSearchResultFromLuceneResult(luceneResult={LuceneResult})", luceneResult);
            return result;
        }

        private Dictionary<int, BaseResultController> GetSearchResultControllers()
        {
            var searchTypes = SearchHelper.Instance.GetSearchTypes();
            var resultControllers = new Dictionary<int, BaseResultController>();

            foreach (var searchType in searchTypes)
            {
                try
                {
                    var searchControllerType = Reflection.CreateType(searchType.SearchResultClass);
                    var searchController = Reflection.CreateObject(this.serviceProvider, searchControllerType);

                    resultControllers.Add(searchType.SearchTypeId, (BaseResultController)searchController);
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
            }

            return resultControllers;
        }

        private Tuple<int, IList<SearchResult>> GetSecurityTrimmedResults(SearchQuery searchQuery, LuceneQuery luceneQuery)
        {
            var results = new List<SearchResult>();
            var totalHits = 0;

            // ****************************************************************************
            // First Fetch and determine starting item of current page
            // ****************************************************************************
            if (searchQuery.PageSize > 0)
            {
                var luceneResults = LuceneController.Instance.Search(new LuceneSearchContext
                {
                    LuceneQuery = luceneQuery,
                    SearchQuery = searchQuery,
                    SecurityCheckerDelegate = this.HasPermissionToViewDoc,
                });
                results = luceneResults.Results.Select(this.GetSearchResultFromLuceneResult).ToList();
                totalHits = luceneResults.TotalHits;

                // ****************************************************************************
                // Adding URL Links to final trimmed results
                // ****************************************************************************
                foreach (var result in results)
                {
                    if (string.IsNullOrEmpty(result.Url))
                    {
                        var resultController = this.GetSearchResultControllers().Single(sc => sc.Key == result.SearchTypeId).Value;
                        result.Url = resultController.GetDocUrl(result);
                    }
                }
            }

            return new Tuple<int, IList<SearchResult>>(totalHits, results);
        }

        private bool HasPermissionToViewDoc(Document document, SearchQuery searchQuery)
        {
			Logger.LogDebug(">>> Entering HasPermissionToViewDoc(document={Document},searchQuery={SearchQuery})", document, searchQuery);
            // others LuceneResult fields are not important at this moment
            var result = GetPartialSearchResult(document, searchQuery);
			Logger.LogDebug("HasPermissionToViewDoc(document={Document},searchQuery={SearchQuery}): result → {Result}", document, searchQuery, result);
            var resultController = this.GetSearchResultControllers().SingleOrDefault(sc => sc.Key == result.SearchTypeId).Value;
			Logger.LogDebug("HasPermissionToViewDoc(document={Document},searchQuery={SearchQuery}): resultController → {ResultController}", document, searchQuery, resultController);
			Logger.LogDebug("<<< Exiting HasPermissionToViewDoc(document={Document},searchQuery={SearchQuery})", document, searchQuery);
            return resultController != null && resultController.HasViewPermission(result);
        }
    }
}
