// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.InternalServices.Views.Search;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for searching.</summary>
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class SearchServiceController : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchServiceController));
        private const string ModuleInfosCacheKey = "ModuleInfos{0}";
        private const CacheItemPriority ModuleInfosCachePriority = CacheItemPriority.AboveNormal;
        private const int ModuleInfosCacheTimeOut = 20;
        private const string ModuleTitleCacheKey = "SearchModuleTabTitle_{0}";
        private const CacheItemPriority ModuleTitleCachePriority = CacheItemPriority.Normal;
        private const int ModuleTitleCacheTimeOut = 20;
        private static readonly Regex GroupedBasicViewRegex = new Regex(@"userid(/|\|=)(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ISearchController searchController;
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly int htmlModuleDefinitionId;

        /// <summary>Initializes a new instance of the <see cref="SearchServiceController"/> class.</summary>
        /// <param name="searchController">The search controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SearchServiceController(ISearchController searchController)
            : this(searchController, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SearchServiceController"/> class.</summary>
        /// <param name="searchController">The search controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        public SearchServiceController(ISearchController searchController, IHostSettings hostSettings, IHostSettingsService hostSettingsService)
            : this(searchController, hostSettings, hostSettingsService, ModuleDefinitionController.GetModuleDefinitionByFriendlyName("Text/HTML")?.ModuleDefID ?? -1)
        {
		Logger.LogError(">>> Entering SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService})", searchController, hostSettings, hostSettingsService);
        Logger.LogWarning("<<< Exiting SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService})", searchController, hostSettings, hostSettingsService);
        }

        /// <summary>Initializes a new instance of the <see cref="SearchServiceController"/> class.</summary>
        /// <remarks>this constructor is for unit tests.</remarks>
        /// <param name="searchController">The search controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="htmlModuleDefinitionId">The ID of the HTML module definition.</param>
        internal SearchServiceController(ISearchController searchController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, int htmlModuleDefinitionId)
        {
			Logger.LogError(">>> Entering SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},htmlModuleDefinitionId={HtmlModuleDefinitionId})", searchController, hostSettings, hostSettingsService, htmlModuleDefinitionId);
			Logger.LogDebug("SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},htmlModuleDefinitionId={HtmlModuleDefinitionId}): hostSettings → {HostSettings1}", searchController, hostSettings, hostSettingsService, htmlModuleDefinitionId, hostSettings);
			Logger.LogDebug("SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},htmlModuleDefinitionId={HtmlModuleDefinitionId}): hostSettingsService → {HostSettingsService1}", searchController, hostSettings, hostSettingsService, htmlModuleDefinitionId, hostSettingsService);
			Logger.LogDebug("SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},htmlModuleDefinitionId={HtmlModuleDefinitionId}): htmlModuleDefinitionId → {HtmlModuleDefinitionId1}", searchController, hostSettings, hostSettingsService, htmlModuleDefinitionId, htmlModuleDefinitionId);
            this.searchController = searchController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ISearchController>();
			Logger.LogWarning("<<< Exiting SearchServiceController(searchController={SearchController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},htmlModuleDefinitionId={HtmlModuleDefinitionId})", searchController, hostSettings, hostSettingsService, htmlModuleDefinitionId);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.htmlModuleDefinitionId = htmlModuleDefinitionId;
        }

        /// <summary>Previews a search.</summary>
        /// <param name="keywords">The keywords.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="forceWild"><c>0</c> to not use wildcards, any positive integer to force a wildcard search.</param>
        /// <param name="portal">The portal ID.</param>
        /// <returns>A response with a list of <see cref="GroupedBasicView"/> objects.</returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Preview(string keywords, string culture, int forceWild = 1, int portal = -1)
        {
			Logger.LogError(">>> Entering Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal})", keywords, culture, forceWild, portal);
            keywords = (keywords ?? string.Empty).Trim();
            var tags = SearchQueryStringParser.Instance.GetTags(keywords, out var cleanedKeywords);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): tags → {Tags}", keywords, culture, forceWild, portal, tags);
            var beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): beginModifiedTimeUtc → {BeginModifiedTimeUtc}", keywords, culture, forceWild, portal, beginModifiedTimeUtc);
            var searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out cleanedKeywords);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): searchTypes → {SearchTypes}", keywords, culture, forceWild, portal, searchTypes);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): keywords → {Keywords1}", keywords, culture, forceWild, portal, keywords);

            var contentSources = this.GetSearchContentSources(searchTypes);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): contentSources → {ContentSources}", keywords, culture, forceWild, portal, contentSources);
            var settings = this.GetSearchModuleSettings();
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): settings → {Settings}", keywords, culture, forceWild, portal, settings);
            var searchTypeIds = GetSearchTypeIds(settings, contentSources);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): searchTypeIds → {SearchTypeIds}", keywords, culture, forceWild, portal, searchTypeIds);
            var moduleDefIds = GetSearchModuleDefIds(settings, contentSources);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): moduleDefIds → {ModuleDefIds}", keywords, culture, forceWild, portal, moduleDefIds);
            var portalIds = this.GetSearchPortalIds(settings, portal);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): portalIds → {PortalIds}", keywords, culture, forceWild, portal, portalIds);

            var userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): userSearchTypeId → {UserSearchTypeId}", keywords, culture, forceWild, portal, userSearchTypeId);
            var userSearchSource = contentSources.FirstOrDefault(s => s.SearchTypeId == userSearchTypeId);
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): userSearchSource → {UserSearchSource}", keywords, culture, forceWild, portal, userSearchSource);

            var results = new List<GroupedBasicView>();
			Logger.LogError("Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): results → {Results}", keywords, culture, forceWild, portal, results);
            if (portalIds.Count != 0 && searchTypeIds.Count != 0 &&
                (!string.IsNullOrEmpty(cleanedKeywords) || tags.Count > 0))
            {
                var query = new SearchQuery
                {
                    KeyWords = cleanedKeywords,
                    Tags = tags,
                    PortalIds = portalIds,
                    SearchTypeIds = searchTypeIds,
                    ModuleDefIds = moduleDefIds,
                    BeginModifiedTimeUtc = beginModifiedTimeUtc,
                    PageIndex = 1,
                    PageSize = 5,
                    TitleSnippetLength = 40,
                    BodySnippetLength = 100,
                    CultureCode = culture,
                    WildCardSearch = forceWild > 0,
                };

                try
                {
                    results = this.GetGroupedBasicViews(query, userSearchSource, this.PortalSettings.PortalId);
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal}): {Arg4}", keywords, culture, forceWild, portal, ex.Message);
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

			Logger.LogWarning("<<< Exiting Preview(keywords={Keywords},culture={Culture},forceWild={ForceWild},portal={Portal})", keywords, culture, forceWild, portal);
            return this.Request.CreateResponse(HttpStatusCode.OK, results);
        }

        /// <summary>Performs a search.</summary>
        /// <param name="search">The search criteria.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="sortOption">A <see cref="SortFields"/> value.</param>
        /// <returns>A response with an object that has <c>results</c>, <c>totalHits</c>, and <c>more</c> fields.</returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Search(string search, string culture, int pageIndex, int pageSize, int sortOption)
        {
			Logger.LogError(">>> Entering Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption})", search, culture, pageIndex, pageSize, sortOption);
            search = (search ?? string.Empty).Trim();
            var tags = SearchQueryStringParser.Instance.GetTags(search, out var cleanedKeywords);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): tags → {Tags}", search, culture, pageIndex, pageSize, sortOption, tags);
            var beginModifiedTimeUtc = SearchQueryStringParser.Instance.GetLastModifiedDate(cleanedKeywords, out cleanedKeywords);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): beginModifiedTimeUtc → {BeginModifiedTimeUtc}", search, culture, pageIndex, pageSize, sortOption, beginModifiedTimeUtc);
            var searchTypes = SearchQueryStringParser.Instance.GetSearchTypeList(cleanedKeywords, out cleanedKeywords);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): searchTypes → {SearchTypes}", search, culture, pageIndex, pageSize, sortOption, searchTypes);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): search → {Search1}", search, culture, pageIndex, pageSize, sortOption, search);

            var contentSources = this.GetSearchContentSources(searchTypes);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): contentSources → {ContentSources}", search, culture, pageIndex, pageSize, sortOption, contentSources);
            var settings = this.GetSearchModuleSettings();
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): settings → {Settings}", search, culture, pageIndex, pageSize, sortOption, settings);
            var searchTypeIds = GetSearchTypeIds(settings, contentSources);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): searchTypeIds → {SearchTypeIds}", search, culture, pageIndex, pageSize, sortOption, searchTypeIds);
            var moduleDefIds = GetSearchModuleDefIds(settings, contentSources);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): moduleDefIds → {ModuleDefIds}", search, culture, pageIndex, pageSize, sortOption, moduleDefIds);
            var portalIds = this.GetSearchPortalIds(settings, -1);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): portalIds → {PortalIds}", search, culture, pageIndex, pageSize, sortOption, portalIds);
            var userSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): userSearchTypeId → {UserSearchTypeId}", search, culture, pageIndex, pageSize, sortOption, userSearchTypeId);
            var maximumPageSize = this.hostSettingsService.GetInteger("Search_MaxResultPerPage", 100);
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): maximumPageSize → {MaximumPageSize}", search, culture, pageIndex, pageSize, sortOption, maximumPageSize);

            var more = false;
            var totalHits = 0;
            var results = new List<GroupedDetailView>();
			Logger.LogError("Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): results → {Results}", search, culture, pageIndex, pageSize, sortOption, results);
            if (portalIds.Count != 0 && searchTypeIds.Count != 0 &&
                (!string.IsNullOrEmpty(cleanedKeywords) || tags.Any()))
            {
                if (pageSize > maximumPageSize)
                {
                    pageSize = maximumPageSize;
                }

                var query = new SearchQuery
                {
                    KeyWords = cleanedKeywords,
                    Tags = tags,
                    PortalIds = portalIds,
                    SearchTypeIds = searchTypeIds,
                    ModuleDefIds = moduleDefIds,
                    BeginModifiedTimeUtc = beginModifiedTimeUtc,
                    EndModifiedTimeUtc = beginModifiedTimeUtc > DateTime.MinValue ? DateTime.MaxValue : DateTime.MinValue,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortField = (SortFields)sortOption,
                    TitleSnippetLength = 120,
                    BodySnippetLength = 300,
                    CultureCode = culture,
                    WildCardSearch = this.IsWildCardEnabledForModule(),
                };

                try
                {
                    results = this.GetGroupedDetailViews(query, userSearchTypeId, out totalHits, out more).ToList();
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption}): {Arg5}", search, culture, pageIndex, pageSize, sortOption, ex.Message);
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

			Logger.LogWarning("<<< Exiting Search(search={Search},culture={Culture},pageIndex={PageIndex},pageSize={PageSize},sortOption={SortOption})", search, culture, pageIndex, pageSize, sortOption);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { results, totalHits, more });
        }

        /// <summary>Add a synonyms group.</summary>
        /// <param name="synonymsGroup">The synonyms group to add.</param>
        /// <returns>A response with an object that has <c>Id</c> and <c>DuplicateWord</c> fields.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage AddSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
			Logger.LogError(">>> Entering AddSynonymsGroup(synonymsGroup={SynonymsGroup})", synonymsGroup);
            var synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(synonymsGroup.Tags, synonymsGroup.PortalId, synonymsGroup.Culture, out var duplicateWord);
			Logger.LogError("AddSynonymsGroup(synonymsGroup={SynonymsGroup}): synonymsGroupId → {SynonymsGroupId}", synonymsGroup, synonymsGroupId);
			Logger.LogWarning("<<< Exiting AddSynonymsGroup(synonymsGroup={SynonymsGroup})", synonymsGroup);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = synonymsGroupId, DuplicateWord = duplicateWord, });
        }

        /// <summary>Update a synonyms group.</summary>
        /// <param name="synonymsGroup">The synonyms group to update.</param>
        /// <returns>A response with an object that has <c>Id</c> and <c>DuplicateWord</c> fields.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage UpdateSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
			Logger.LogError(">>> Entering UpdateSynonymsGroup(synonymsGroup={SynonymsGroup})", synonymsGroup);
            var synonymsGroupId = SearchHelper.Instance.UpdateSynonymsGroup(synonymsGroup.Id, synonymsGroup.Tags, synonymsGroup.PortalId, synonymsGroup.Culture, out var duplicateWord);
			Logger.LogError("UpdateSynonymsGroup(synonymsGroup={SynonymsGroup}): synonymsGroupId → {SynonymsGroupId}", synonymsGroup, synonymsGroupId);
			Logger.LogWarning("<<< Exiting UpdateSynonymsGroup(synonymsGroup={SynonymsGroup})", synonymsGroup);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = synonymsGroupId, DuplicateWord = duplicateWord, });
        }

        /// <summary>Deletes a synonyms group.</summary>
        /// <param name="synonymsGroup">The synonyms group to delete.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage DeleteSynonymsGroup(SynonymsGroupDto synonymsGroup)
        {
			Logger.LogError(">>> Entering DeleteSynonymsGroup(synonymsGroup={SynonymsGroup})", synonymsGroup);
            SearchHelper.Instance.DeleteSynonymsGroup(synonymsGroup.Id, synonymsGroup.PortalId, synonymsGroup.Culture);
			Logger.LogWarning("<<< Exiting DeleteSynonymsGroup(synonymsGroup={SynonymsGroup})", synonymsGroup);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>Add search stop words.</summary>
        /// <param name="stopWords">The stop words to add.</param>
        /// <returns>A response with an object that has an <c>Id</c> field.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage AddStopWords(StopWordsDto stopWords)
        {
			Logger.LogError(">>> Entering AddStopWords(stopWords={StopWords})", stopWords);
            var stopWordsId = SearchHelper.Instance.AddSearchStopWords(stopWords.Words, stopWords.PortalId, stopWords.Culture);
			Logger.LogError("AddStopWords(stopWords={StopWords}): stopWordsId → {StopWordsId}", stopWords, stopWordsId);
			Logger.LogWarning("<<< Exiting AddStopWords(stopWords={StopWords})", stopWords);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = stopWordsId, });
        }

        /// <summary>Update search stop words.</summary>
        /// <param name="stopWords">The stop words to update.</param>
        /// <returns>A response with an object that has an <c>Id</c> field.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage UpdateStopWords(StopWordsDto stopWords)
        {
			Logger.LogError(">>> Entering UpdateStopWords(stopWords={StopWords})", stopWords);
            var stopWordsId = SearchHelper.Instance.UpdateSearchStopWords(stopWords.Id, stopWords.Words, stopWords.PortalId, stopWords.Culture);
			Logger.LogError("UpdateStopWords(stopWords={StopWords}): stopWordsId → {StopWordsId}", stopWords, stopWordsId);
			Logger.LogWarning("<<< Exiting UpdateStopWords(stopWords={StopWords})", stopWords);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Id = stopWordsId, });
        }

        /// <summary>Delete search stop words.</summary>
        /// <param name="stopWords">The stop words to delete.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedModules("SearchAdmin")]
        public HttpResponseMessage DeleteStopWords(StopWordsDto stopWords)
        {
			Logger.LogError(">>> Entering DeleteStopWords(stopWords={StopWords})", stopWords);
            SearchHelper.Instance.DeleteSearchStopWords(stopWords.Id, stopWords.PortalId, stopWords.Culture);
			Logger.LogWarning("<<< Exiting DeleteStopWords(stopWords={StopWords})", stopWords);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>Gets grouped detail views.</summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="userSearchTypeId">The ID of the user search type.</param>
        /// <param name="totalHits">The total number of hits.</param>
        /// <param name="more">Whether there is more.</param>
        /// <returns>A list of <see cref="GroupedDetailView"/> instances.</returns>
        internal IEnumerable<GroupedDetailView> GetGroupedDetailViews(SearchQuery searchQuery, int userSearchTypeId, out int totalHits, out bool more)
        {
			Logger.LogError(">>> Entering GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId})", searchQuery, userSearchTypeId);
            var searchResults = this.searchController.SiteSearch(searchQuery);
            totalHits = searchResults.TotalHits;
			Logger.LogDebug("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): totalHits → {TotalHits}", searchQuery, userSearchTypeId, totalHits);
			Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): searchResults → {SearchResults}", searchQuery, userSearchTypeId, searchResults);
            more = totalHits > searchQuery.PageSize * searchQuery.PageIndex;
			Logger.LogDebug("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): more → {More}", searchQuery, userSearchTypeId, more);

            var groups = new List<GroupedDetailView>();
			Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): groups → {Groups}", searchQuery, userSearchTypeId, groups);
            var tabGroups = new Dictionary<string, IList<SearchResult>>();
			Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): tabGroups → {TabGroups}", searchQuery, userSearchTypeId, tabGroups);

            foreach (var result in searchResults.Results)
            {
                ////var key = result.TabId + result.Url;
                var key = result.Url;
				Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): key → {Key}", searchQuery, userSearchTypeId, key);
                if (!tabGroups.ContainsKey(key))
                {
                    tabGroups.Add(key, new List<SearchResult> { result });
                }
                else
                {
                    // when the result is a user search type, we should only show one result
                    // and if duplicate, we should also reduce the totalHit number.
                    if (result.SearchTypeId != userSearchTypeId ||
                        tabGroups[key].All(r => r.Url != result.Url))
                    {
                        tabGroups[key].Add(result);
                    }
                    else
                    {
                        totalHits--;
                    }
                }
            }

            var showFriendlyTitle =
                this.ActiveModule == null ||
                !this.ActiveModule.ModuleSettings.ContainsKey("ShowFriendlyTitle") ||
                Convert.ToBoolean(this.ActiveModule.ModuleSettings["ShowFriendlyTitle"], CultureInfo.InvariantCulture);
            foreach (var results in tabGroups.Values)
            {
                var group = new GroupedDetailView();
				Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): group → {Group}", searchQuery, userSearchTypeId, group);

                // first entry
                var first = results[0];
                @group.Title = showFriendlyTitle ? GetFriendlyTitle(first) : first.Title;
                @group.DocumentUrl = first.Url;

                // Find a different title for multiple entries with same url
                if (results.Count > 1)
                {
                    if (first.TabId > 0)
                    {
                        var tab = TabController.Instance.GetTab(first.TabId, first.PortalId, false);
						Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): tab → {Tab}", searchQuery, userSearchTypeId, tab);
                        if (tab != null)
                        {
                            @group.Title = showFriendlyTitle && !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.TabName;
                        }
                    }
                    else if (first.ModuleId > 0)
                    {
                        var tabTitle = GetTabTitleFromModuleId(this.hostSettings, first.ModuleId);
						Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): tabTitle → {TabTitle}", searchQuery, userSearchTypeId, tabTitle);
                        if (!string.IsNullOrEmpty(tabTitle))
                        {
                            @group.Title = tabTitle;
                        }
                    }
                }
                else if (first.ModuleDefId > 0 && first.ModuleDefId == this.htmlModuleDefinitionId)
                {
                    // special handling for Html module
                    var tabTitle = GetTabTitleFromModuleId(this.hostSettings, first.ModuleId);
                    if (!string.IsNullOrEmpty(tabTitle))
                    {
                        @group.Title = tabTitle;
                        if (first.Title != "Enter Title" && first.Title != "Text/HTML")
                        {
                            @group.Title += $" > {first.Title}";
                        }

                        first.Title = @group.Title;
						Logger.LogDebug("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): first.Title → {Arg2}", searchQuery, userSearchTypeId, first.Title);
                    }
                }

                foreach (var result in results)
                {
                    var title = showFriendlyTitle ? GetFriendlyTitle(result) : result.Title;
					Logger.LogError("GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId}): title → {Title}", searchQuery, userSearchTypeId, title);
                    var detail = new DetailedView
                    {
                        Title = title != null && title.Contains("<") ? HttpUtility.HtmlEncode(title) : title,
                        DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                        DocumentUrl = result.Url,
                        Snippet = result.Snippet,
                        Description = result.Description,
                        DisplayModifiedTime = result.DisplayModifiedTime,
                        Tags = result.Tags.ToList(),
                        AuthorProfileUrl = result.AuthorUserId > 0 ? Globals.UserProfileURL(result.AuthorUserId) : string.Empty,
                        AuthorName = result.AuthorName,
                    };
                    @group.Results.Add(detail);
                }

                groups.Add(@group);
            }

			Logger.LogWarning("<<< Exiting GetGroupedDetailViews(searchQuery={SearchQuery},userSearchTypeId={UserSearchTypeId})", searchQuery, userSearchTypeId);
            return groups;
        }

        /// <summary>Gets grouped basic views.</summary>
        /// <param name="query">The query.</param>
        /// <param name="userSearchSource">The user search source.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A list of <see cref="GroupedBasicView"/> instances.</returns>
        internal List<GroupedBasicView> GetGroupedBasicViews(SearchQuery query, SearchContentSource userSearchSource, int portalId)
        {
			Logger.LogError(">>> Entering GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId})", query, userSearchSource, portalId);
            var results = new List<GroupedBasicView>();
            var previews = this.GetBasicViews(query, out _);
			Logger.LogError("GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId}): previews → {Previews}", query, userSearchSource, portalId, previews);
			Logger.LogError("GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId}): results → {Results}", query, userSearchSource, portalId, results);

            foreach (var preview in previews)
            {
                // if the document type is user, then try to add user pic into preview's custom attributes.
                if (userSearchSource != null && preview.DocumentTypeName == userSearchSource.LocalizedName)
                {
                    var match = GroupedBasicViewRegex.Match(preview.DocumentUrl);
					Logger.LogError("GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId}): match → {Match}", query, userSearchSource, portalId, match);
                    if (match.Success)
                    {
                        var userid = Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture);
                        var user = UserController.Instance.GetUserById(portalId, userid);
                        if (user != null)
                        {
                            preview.Attributes.Add("Avatar", user.Profile.PhotoURL);
                        }
						Logger.LogError("GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId}): userid → {Userid}", query, userSearchSource, portalId, userid);
                    }
                }

                var groupedResult = results.SingleOrDefault(g => g.DocumentTypeName == preview.DocumentTypeName);
				Logger.LogError("GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId}): groupedResult → {GroupedResult}", query, userSearchSource, portalId, groupedResult);
                if (groupedResult != null)
                {
                    if (!groupedResult.Results.Any(r => string.Equals(r.DocumentUrl, preview.DocumentUrl, StringComparison.Ordinal)))
                    {
                        groupedResult.Results.Add(new BasicView
                        {
                            Title = preview.Title.Contains("<") ? HttpUtility.HtmlEncode(preview.Title) : preview.Title,
                            Snippet = preview.Snippet,
                            Description = preview.Description,
                            DocumentUrl = preview.DocumentUrl,
                            Attributes = preview.Attributes,
                        });
                    }
                }
                else
                {
                    results.Add(new GroupedBasicView(preview));
                }
            }

			Logger.LogWarning("<<< Exiting GetGroupedBasicViews(query={Query},userSearchSource={UserSearchSource},portalId={PortalId})", query, userSearchSource, portalId);
            return results;
        }

        /// <summary>Gets basic views.</summary>
        /// <param name="searchQuery">The search query.</param>
        /// <param name="totalHits">The total number of hits.</param>
        /// <returns>A sequence of <see cref="BasicView"/> instances.</returns>
        internal IEnumerable<BasicView> GetBasicViews(SearchQuery searchQuery, out int totalHits)
        {
			Logger.LogError(">>> Entering GetBasicViews(searchQuery={SearchQuery})", searchQuery);
            var sResult = this.searchController.SiteSearch(searchQuery);
            totalHits = sResult.TotalHits;
			Logger.LogDebug("GetBasicViews(searchQuery={SearchQuery}): totalHits → {TotalHits}", searchQuery, totalHits);
            var showFriendlyTitle = this.GetBooleanSetting("ShowFriendlyTitle", true);
			Logger.LogError("GetBasicViews(searchQuery={SearchQuery}): showFriendlyTitle → {ShowFriendlyTitle}", searchQuery, showFriendlyTitle);
            var showDescription = this.GetBooleanSetting("ShowDescription", true);
			Logger.LogError("GetBasicViews(searchQuery={SearchQuery}): showDescription → {ShowDescription}", searchQuery, showDescription);
            var showSnippet = this.GetBooleanSetting("ShowSnippet", true);
			Logger.LogError("GetBasicViews(searchQuery={SearchQuery}): showSnippet → {ShowSnippet}", searchQuery, showSnippet);
            var maxDescriptionLength = this.GetIntegerSetting("MaxDescriptionLength", 100);
			Logger.LogError("GetBasicViews(searchQuery={SearchQuery}): maxDescriptionLength → {MaxDescriptionLength}", searchQuery, maxDescriptionLength);
			Logger.LogError("GetBasicViews(searchQuery={SearchQuery}): sResult → {SResult}", searchQuery, sResult);

            return sResult.Results.Select(result =>
            {
                var description = result.Description;
                if (!string.IsNullOrEmpty(description) && description.Length > maxDescriptionLength)
                {
                    description = description.Substring(0, maxDescriptionLength) + "...";
                }

                return new BasicView
                {
                    Title = this.GetTitle(result, showFriendlyTitle),
                    DocumentTypeName = InternalSearchController.Instance.GetSearchDocumentTypeDisplayName(result),
                    DocumentUrl = result.Url,
                    Snippet = showSnippet ? result.Snippet : string.Empty,
                    Description = showDescription ? description : string.Empty,
                };
            });
			Logger.LogWarning("<<< Exiting GetBasicViews(searchQuery={SearchQuery})", searchQuery);
        }

        private static ArrayList GetModulesByDefinition(IHostSettings hostSettings, int portalId, string friendlyName)
        {
			Logger.LogError(">>> Entering GetModulesByDefinition(hostSettings={HostSettings},portalId={PortalId},friendlyName={FriendlyName})", hostSettings, portalId, friendlyName);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, ModuleInfosCacheKey, portalId);
			Logger.LogError("GetModulesByDefinition(hostSettings={HostSettings},portalId={PortalId},friendlyName={FriendlyName}): cacheKey → {CacheKey}", hostSettings, portalId, friendlyName, cacheKey);
			Logger.LogWarning("<<< Exiting GetModulesByDefinition(hostSettings={HostSettings},portalId={PortalId},friendlyName={FriendlyName})", hostSettings, portalId, friendlyName);
            return CBO.GetCachedObject<ArrayList>(
                hostSettings,
                new CacheItemArgs(cacheKey, ModuleInfosCacheTimeOut, ModuleInfosCachePriority),
                _ => CBO.FillCollection(DataProvider.Instance().GetModuleByDefinition(portalId, friendlyName), typeof(ModuleInfo)));
        }

        private static List<int> GetSearchTypeIds(Hashtable settings, IEnumerable<SearchContentSource> searchContentSources)
        {
			Logger.LogError(">>> Entering GetSearchTypeIds(settings={Settings},searchContentSources={SearchContentSources})", settings, searchContentSources);
            var list = new List<int>();
            var configuredList = new List<string>();
			Logger.LogError("GetSearchTypeIds(settings={Settings},searchContentSources={SearchContentSources}): configuredList → {ConfiguredList}", settings, searchContentSources, configuredList);
			Logger.LogError("GetSearchTypeIds(settings={Settings},searchContentSources={SearchContentSources}): list → {List}", settings, searchContentSources, list);
            var scopeForFilters = Convert.ToString(settings["ScopeForFilters"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(scopeForFilters))
            {
				Logger.LogError("GetSearchTypeIds(settings={Settings},searchContentSources={SearchContentSources}): scopeForFilters → {ScopeForFilters}", settings, searchContentSources, scopeForFilters);
                configuredList = scopeForFilters.Split('|').ToList();
            }

            // check content source in configured list or not
            foreach (var contentSource in searchContentSources)
            {
                if (contentSource.IsPrivate)
                {
                    continue;
                }

                if (configuredList.Count > 0)
                {
                    if (configuredList.Any(l => l.Contains(contentSource.LocalizedName)))
                    {
                        // in configured list
                        list.Add(contentSource.SearchTypeId);
                    }
                }
                else
                {
                    list.Add(contentSource.SearchTypeId);
                }
            }

			Logger.LogWarning("<<< Exiting GetSearchTypeIds(settings={Settings},searchContentSources={SearchContentSources})", settings, searchContentSources);
            return list.Distinct().ToList();
        }

        private static List<int> GetSearchModuleDefIds(Hashtable settings, IEnumerable<SearchContentSource> searchContentSources)
        {
			Logger.LogError(">>> Entering GetSearchModuleDefIds(settings={Settings},searchContentSources={SearchContentSources})", settings, searchContentSources);
            var list = new List<int>();
            var configuredList = new List<string>();
			Logger.LogError("GetSearchModuleDefIds(settings={Settings},searchContentSources={SearchContentSources}): configuredList → {ConfiguredList}", settings, searchContentSources, configuredList);
			Logger.LogError("GetSearchModuleDefIds(settings={Settings},searchContentSources={SearchContentSources}): list → {List}", settings, searchContentSources, list);
            var scopeForFilters = Convert.ToString(settings["ScopeForFilters"], CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(scopeForFilters))
            {
				Logger.LogError("GetSearchModuleDefIds(settings={Settings},searchContentSources={SearchContentSources}): scopeForFilters → {ScopeForFilters}", settings, searchContentSources, scopeForFilters);
                configuredList = scopeForFilters.Split('|').ToList();
            }

            // check content source in configured list or not
            foreach (var contentSource in searchContentSources)
            {
                if (contentSource.IsPrivate)
                {
                    continue;
                }

                if (configuredList.Count > 0)
                {
                    if (configuredList.Any(l => l.Contains(contentSource.LocalizedName)) && contentSource.ModuleDefinitionId > 0)
                    {
                        // in configured list
                        list.Add(contentSource.ModuleDefinitionId);
                    }
                }
                else
                {
                    if (contentSource.ModuleDefinitionId > 0)
                    {
                        list.Add(contentSource.ModuleDefinitionId);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetSearchModuleDefIds(settings={Settings},searchContentSources={SearchContentSources})", settings, searchContentSources);
            return list;
        }

        private static string GetFriendlyTitle(SearchResult result)
        {
			Logger.LogError(">>> Entering GetFriendlyTitle(result={Result})", result);
            if (result.Keywords.TryGetValue("title", out var title) && !string.IsNullOrEmpty(title))
            {
                return title;
            }

			Logger.LogWarning("<<< Exiting GetFriendlyTitle(result={Result})", result);
            return result.Title;
        }

        private static string GetTabTitleFromModuleId(IHostSettings hostSettings, int moduleId)
        {
			Logger.LogError(">>> Entering GetTabTitleFromModuleId(hostSettings={HostSettings},moduleId={ModuleId})", hostSettings, moduleId);
            // no manual clearing of the cache exists; let it just expire
            var cacheKey = string.Format(CultureInfo.InvariantCulture, ModuleTitleCacheKey, moduleId);
			Logger.LogError("GetTabTitleFromModuleId(hostSettings={HostSettings},moduleId={ModuleId}): cacheKey → {CacheKey}", hostSettings, moduleId, cacheKey);

			Logger.LogWarning("<<< Exiting GetTabTitleFromModuleId(hostSettings={HostSettings},moduleId={ModuleId})", hostSettings, moduleId);
            return CBO.GetCachedObject<string>(
                hostSettings,
                new CacheItemArgs(cacheKey, ModuleTitleCacheTimeOut, ModuleTitleCachePriority, moduleId),
                GetTabTitleCallBack);
        }

        private static object GetTabTitleCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetTabTitleCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var moduleId = (int)cacheItemArgs.ParamList[0];
            var moduleInfo = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
			Logger.LogError("GetTabTitleCallBack(cacheItemArgs={CacheItemArgs}): moduleId → {ModuleId}", cacheItemArgs, moduleId);
            if (moduleInfo != null)
            {
				Logger.LogError("GetTabTitleCallBack(cacheItemArgs={CacheItemArgs}): moduleInfo → {ModuleInfo}", cacheItemArgs, moduleInfo);
                var tab = moduleInfo.ParentTab;

                return !string.IsNullOrEmpty(tab.Title) ? tab.Title : tab.TabName;
            }

			Logger.LogWarning("<<< Exiting GetTabTitleCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return string.Empty;
        }

        private bool IsWildCardEnabledForModule()
        {
			Logger.LogError(">>> Entering IsWildCardEnabledForModule()");
            var searchModuleSettings = this.GetSearchModuleSettings();
            var enableWildSearch = true;
			Logger.LogError("IsWildCardEnabledForModule(): searchModuleSettings → {SearchModuleSettings}", searchModuleSettings);
            if (!string.IsNullOrEmpty(Convert.ToString(searchModuleSettings["EnableWildSearch"], CultureInfo.InvariantCulture)))
            {
                enableWildSearch = Convert.ToBoolean(searchModuleSettings["EnableWildSearch"], CultureInfo.InvariantCulture);
				Logger.LogError("IsWildCardEnabledForModule(): enableWildSearch → {EnableWildSearch}", enableWildSearch);
            }

			Logger.LogWarning("<<< Exiting IsWildCardEnabledForModule()");
            return enableWildSearch;
        }

        private ModuleInfo GetSearchModule()
        {
			Logger.LogError(">>> Entering GetSearchModule()");
            var arrModules = GetModulesByDefinition(this.hostSettings, this.PortalSettings.PortalId, "Search Results");
            ModuleInfo findModule = null;
			Logger.LogError("GetSearchModule(): arrModules → {ArrModules}", arrModules);
            if (arrModules.Count > 1)
            {
                findModule = arrModules.Cast<ModuleInfo>().FirstOrDefault(searchModule => searchModule.CultureCode == this.PortalSettings.CultureCode);
				Logger.LogError("GetSearchModule(): findModule → {FindModule}", findModule);
            }

			Logger.LogWarning("<<< Exiting GetSearchModule()");
            return findModule ?? (arrModules.Count > 0 ? (ModuleInfo)arrModules[0] : null);
        }

        private Hashtable GetSearchModuleSettings()
        {
			Logger.LogError(">>> Entering GetSearchModuleSettings()");
            if (this.ActiveModule != null && this.ActiveModule.ModuleDefinition.FriendlyName == "Search Results")
            {
                return this.ActiveModule.ModuleSettings;
            }

            var searchModule = this.GetSearchModule();
			Logger.LogError("GetSearchModuleSettings(): searchModule → {SearchModule}", searchModule);
			Logger.LogWarning("<<< Exiting GetSearchModuleSettings()");
            return searchModule?.ModuleSettings;
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
			Logger.LogError(">>> Entering GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            if (this.PortalSettings == null)
            {
                return defaultValue;
            }

            var settings = this.GetSearchModuleSettings();
			Logger.LogError("GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue}): settings → {Settings}", settingName, defaultValue, settings);
            if (settings == null || !settings.ContainsKey(settingName))
            {
                return defaultValue;
            }

			Logger.LogWarning("<<< Exiting GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            return Convert.ToBoolean(settings[settingName], CultureInfo.InvariantCulture);
        }

        private int GetIntegerSetting(string settingName, int defaultValue)
        {
			Logger.LogError(">>> Entering GetIntegerSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            if (this.PortalSettings == null)
            {
                return defaultValue;
            }

            var settings = this.GetSearchModuleSettings();
			Logger.LogError("GetIntegerSetting(settingName={SettingName},defaultValue={DefaultValue}): settings → {Settings}", settingName, defaultValue, settings);
            if (settings == null || !settings.ContainsKey(settingName))
            {
                return defaultValue;
            }

            var settingValue = Convert.ToString(settings[settingName], CultureInfo.InvariantCulture);
			Logger.LogError("GetIntegerSetting(settingName={SettingName},defaultValue={DefaultValue}): settingValue → {SettingValue}", settingName, defaultValue, settingValue);
            if (!string.IsNullOrEmpty(settingValue) && Regex.IsMatch(settingValue, "^\\d+$"))
            {
                return Convert.ToInt32(settingValue, CultureInfo.InvariantCulture);
            }

			Logger.LogWarning("<<< Exiting GetIntegerSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            return defaultValue;
        }

        private List<int> GetSearchPortalIds(Hashtable settings, int portalId)
        {
			Logger.LogError(">>> Entering GetSearchPortalIds(settings={Settings},portalId={PortalId})", settings, portalId);
            var list = new List<int>();
            var scopeForPortals = Convert.ToString(settings["ScopeForPortals"], CultureInfo.InvariantCulture);
			Logger.LogError("GetSearchPortalIds(settings={Settings},portalId={PortalId}): list → {List}", settings, portalId, list);
            if (!string.IsNullOrEmpty(scopeForPortals))
            {
				Logger.LogError("GetSearchPortalIds(settings={Settings},portalId={PortalId}): scopeForPortals → {ScopeForPortals}", settings, portalId, scopeForPortals);
                list = scopeForPortals.Split('|').Select(s => Convert.ToInt32(s, CultureInfo.InvariantCulture)).ToList();
            }

            if (portalId == -1)
            {
                portalId = this.PortalSettings.ActiveTab.PortalID;
            }

            if (portalId > -1 && !list.Contains(portalId))
            {
                list.Add(portalId);
            }

            // Add Host
            var userInfo = this.UserInfo;
            if (userInfo.IsSuperUser)
            {
                list.Add(-1);
            }

			Logger.LogWarning("<<< Exiting GetSearchPortalIds(settings={Settings},portalId={PortalId})", settings, portalId);
            return list;
        }

        private List<SearchContentSource> GetSearchContentSources(IList<string> typesList)
        {
			Logger.LogError(">>> Entering GetSearchContentSources(typesList={TypesList})", typesList);
            var sources = new List<SearchContentSource>();
            var list = InternalSearchController.Instance.GetSearchContentSourceList(this.PortalSettings.PortalId);
			Logger.LogError("GetSearchContentSources(typesList={TypesList}): list → {List}", typesList, list);
			Logger.LogError("GetSearchContentSources(typesList={TypesList}): sources → {Sources}", typesList, sources);

            if (typesList.Any())
            {
                foreach (var contentSources in typesList.Select(t1 => list.Where(src => string.Equals(src.LocalizedName, t1, StringComparison.OrdinalIgnoreCase))))
                {
                    sources.AddRange(contentSources);
                }
            }
            else
            {
                // no types filter specified, add all available content sources
                sources.AddRange(list);
            }

			Logger.LogWarning("<<< Exiting GetSearchContentSources(typesList={TypesList})", typesList);
            return sources;
        }

        private string GetTitle(SearchResult result, bool showFriendlyTitle = false)
        {
			Logger.LogError(">>> Entering GetTitle(result={Result},showFriendlyTitle={ShowFriendlyTitle})", result, showFriendlyTitle);
            if (result.ModuleDefId > 0 && result.ModuleDefId == this.htmlModuleDefinitionId)
            {
                // special handling for Html module
                var tabTitle = GetTabTitleFromModuleId(this.hostSettings, result.ModuleId);
				Logger.LogError("GetTitle(result={Result},showFriendlyTitle={ShowFriendlyTitle}): tabTitle → {TabTitle}", result, showFriendlyTitle, tabTitle);
                if (!string.IsNullOrEmpty(tabTitle))
                {
                    if (result.Title != "Enter Title" && result.Title != "Text/HTML")
                    {
                        return $"{tabTitle} > {result.Title}";
                    }

                    return tabTitle;
                }
            }

			Logger.LogWarning("<<< Exiting GetTitle(result={Result},showFriendlyTitle={ShowFriendlyTitle})", result, showFriendlyTitle);
            return showFriendlyTitle ? GetFriendlyTitle(result) : result.Title;
        }

        /// <summary>A data transfer object with information about a synonyms group.</summary>
        public class SynonymsGroupDto
        {
            /// <summary>Gets or sets the ID.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the tags.</summary>
            public string Tags { get; set; }

            /// <summary>Gets or sets the portal ID.</summary>
            public int PortalId { get; set; }

            /// <summary>Gets or sets the culture.</summary>
            public string Culture { get; set; }
        }

        /// <summary>A data transfer object with information about stop words.</summary>
        public class StopWordsDto
        {
            /// <summary>Gets or sets the ID.</summary>
            public int Id { get; set; }

            /// <summary>Gets or sets the words.</summary>
            public string Words { get; set; }

            /// <summary>Gets or sets the portal ID.</summary>
            public int PortalId { get; set; }

            /// <summary>Gets or sets the culture.</summary>
            public string Culture { get; set; }
        }
    }
}
