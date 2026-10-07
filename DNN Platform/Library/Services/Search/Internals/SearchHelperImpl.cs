// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Tokenattributes;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ISearchHelper"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    internal class SearchHelperImpl(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController, IApplicationStatusInfo appStatus)
        : ISearchHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchHelperImpl));
        private const string SearchTypesCacheKey = "SearchTypes";
        private const string SynonymTermsCacheKey = "SynonymTerms";
        private const string SynonymGroupsCacheKey = "SynonymsGroups";
        private const string CacheKeyFormat = "{0}_{1}_{2}";
        private const string LastIndexKeyFormat = "{0}_{1}";
        private const string SearchStopWordsCacheKey = "SearchStopWords";
        private const string ResourceFileRelativePathWithoutExt = "/App_GlobalResources/GlobalResources";
        private readonly IList<string> emptySynonyms = new List<string>(0);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IHostSettingsService hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();

        /// <summary>Initializes a new instance of the <see cref="SearchHelperImpl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SearchHelperImpl()
            : this(null, null, null, null)
        {
		Logger.LogError(">>> Entering SearchHelperImpl()");
        Logger.LogWarning("<<< Exiting SearchHelperImpl()");
        }

        /// <inheritdoc />
        public IEnumerable<SearchType> GetSearchTypes()
        {
			Logger.LogError(">>> Entering GetSearchTypes()");
            var cacheArg = new CacheItemArgs(SearchTypesCacheKey, 120, CacheItemPriority.Default);
			Logger.LogError("GetSearchTypes(): cacheArg → {CacheArg}", cacheArg);
			Logger.LogWarning("<<< Exiting GetSearchTypes()");
            return CBO.GetCachedObject<IList<SearchType>>(
                this.hostSettings,
                cacheArg,
                _ => CBO.FillCollection<SearchType>(DataProvider.Instance().GetAllSearchTypes()));
        }

        /// <inheritdoc />
        public SearchType GetSearchTypeByName(string searchTypeName)
        {
			Logger.LogError(">>> Entering GetSearchTypeByName(searchTypeName={SearchTypeName})", searchTypeName);
			Logger.LogWarning("<<< Exiting GetSearchTypeByName(searchTypeName={SearchTypeName})", searchTypeName);
            return this.GetSearchTypes().Single(t => t.SearchTypeName == searchTypeName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetSynonyms(int portalId, string cultureCode, string term)
        {
			Logger.LogError(">>> Entering GetSynonyms(portalId={PortalId},cultureCode={CultureCode},term={Term})", portalId, cultureCode, term);
            var terms = this.GetSynonymTerms(portalId, cultureCode);
			Logger.LogError("GetSynonyms(portalId={PortalId},cultureCode={CultureCode},term={Term}): terms → {Terms}", portalId, cultureCode, term, terms);
            if (terms == null || !terms.TryGetValue((term ?? string.Empty).ToLowerInvariant(), out var synonyms))
            {
                synonyms = this.emptySynonyms;
				Logger.LogDebug("GetSynonyms(portalId={PortalId},cultureCode={CultureCode},term={Term}): synonyms → {Synonyms}", portalId, cultureCode, term, synonyms);
            }

			Logger.LogWarning("<<< Exiting GetSynonyms(portalId={PortalId},cultureCode={CultureCode},term={Term})", portalId, cultureCode, term);
            return synonyms;
        }

        /// <inheritdoc />
        public IEnumerable<SynonymsGroup> GetSynonymsGroups(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
            var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default, this.portalController, this.appStatus);
			Logger.LogError("GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode}): cacheArg → {CacheArg}", portalId, cultureCode, cacheArg);
			Logger.LogError("GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", portalId, cultureCode, cacheKey);
			Logger.LogWarning("<<< Exiting GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return CBO.GetCachedObject<IList<SynonymsGroup>>(this.hostSettings, cacheArg, GetSynonymsGroupsCallBack);
        }

        /// <inheritdoc />
        public int AddSynonymsGroup(string synonymsTags, int portalId, string cultureCode, out string duplicateWord)
        {
			Logger.LogError(">>> Entering AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode})", synonymsTags, portalId, cultureCode);
            duplicateWord = null;
			Logger.LogDebug("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): duplicateWord → {DuplicateWord}", synonymsTags, portalId, cultureCode, duplicateWord);
            if (string.IsNullOrEmpty(synonymsTags))
            {
				Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): duplicateWord → {DuplicateWord}", synonymsTags, portalId, cultureCode, duplicateWord);
                return 0;
            }

            if (portalId < 0)
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
			Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): userId → {UserId}", synonymsTags, portalId, cultureCode, userId);
            var list = this.GetSynonymsGroups(portalId, cultureCode);
			Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): list → {List}", synonymsTags, portalId, cultureCode, list);
            var tags = synonymsTags.ToLowerInvariant().Split(',');
			Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): tags → {Tags}", synonymsTags, portalId, cultureCode, tags);

            if (tags.Length == 0)
            {
                return 0;
            }

            foreach (var group in list)
            {
                var groupTags = group.SynonymsTags.ToLowerInvariant().Split(',');
				Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): groupTags → {GroupTags}", synonymsTags, portalId, cultureCode, groupTags);
                duplicateWord = tags.FirstOrDefault(groupTags.Contains);
                if (!string.IsNullOrEmpty(duplicateWord))
                {
                    return 0;
                }
            }

            var newId = DataProvider.Instance().AddSynonymsGroup(synonymsTags, userId, portalId, cultureCode);
			Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): newId → {NewId}", synonymsTags, portalId, cultureCode, newId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
			Logger.LogError("AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", synonymsTags, portalId, cultureCode, cacheKey);
            DataCache.ClearCache(cacheKey);

            cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymTermsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);

			Logger.LogWarning("<<< Exiting AddSynonymsGroup(synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode})", synonymsTags, portalId, cultureCode);
            return newId;
        }

        /// <inheritdoc />
        public int UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int portalId, string cultureCode, out string duplicateWord)
        {
			Logger.LogError(">>> Entering UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode})", synonymsGroupId, synonymsTags, portalId, cultureCode);
            duplicateWord = null;
			Logger.LogDebug("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): duplicateWord → {DuplicateWord}", synonymsGroupId, synonymsTags, portalId, cultureCode, duplicateWord);
            if (synonymsGroupId <= 0)
            {
				Logger.LogError("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): duplicateWord → {DuplicateWord}", synonymsGroupId, synonymsTags, portalId, cultureCode, duplicateWord);
                return 0;
            }

            if (string.IsNullOrEmpty(synonymsTags))
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
			Logger.LogError("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): userId → {UserId}", synonymsGroupId, synonymsTags, portalId, cultureCode, userId);
            var list = this.GetSynonymsGroups(portalId, cultureCode);
			Logger.LogError("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): list → {List}", synonymsGroupId, synonymsTags, portalId, cultureCode, list);
            var tags = synonymsTags.ToLowerInvariant().Split(',');
			Logger.LogError("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): tags → {Tags}", synonymsGroupId, synonymsTags, portalId, cultureCode, tags);

            if (tags.Length == 0)
            {
                return 0;
            }

            foreach (var group in list)
            {
                if (group.SynonymsGroupId != synonymsGroupId)
                {
                    var groupTags = group.SynonymsTags.ToLowerInvariant().Split(',');
					Logger.LogError("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): groupTags → {GroupTags}", synonymsGroupId, synonymsTags, portalId, cultureCode, groupTags);
                    duplicateWord = tags.FirstOrDefault(groupTags.Contains);
                    if (!string.IsNullOrEmpty(duplicateWord))
                    {
                        return 0;
                    }
                }
            }

            DataProvider.Instance().UpdateSynonymsGroup(synonymsGroupId, synonymsTags, userId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
			Logger.LogError("UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", synonymsGroupId, synonymsTags, portalId, cultureCode, cacheKey);
            DataCache.ClearCache(cacheKey);

            cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymTermsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
			Logger.LogWarning("<<< Exiting UpdateSynonymsGroup(synonymsGroupId={SynonymsGroupId},synonymsTags={SynonymsTags},portalId={PortalId},cultureCode={CultureCode})", synonymsGroupId, synonymsTags, portalId, cultureCode);
            return synonymsGroupId;
        }

        /// <inheritdoc />
        public void DeleteSynonymsGroup(int synonymsGroupId, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering DeleteSynonymsGroup(synonymsGroupId={SynonymsGroupId},portalId={PortalId},cultureCode={CultureCode})", synonymsGroupId, portalId, cultureCode);
            if (synonymsGroupId <= 0)
            {
                return;
            }

            DataProvider.Instance().DeleteSynonymsGroup(synonymsGroupId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
			Logger.LogError("DeleteSynonymsGroup(synonymsGroupId={SynonymsGroupId},portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", synonymsGroupId, portalId, cultureCode, cacheKey);
            DataCache.ClearCache(cacheKey);

            cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SynonymTermsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
			Logger.LogWarning("<<< Exiting DeleteSynonymsGroup(synonymsGroupId={SynonymsGroupId},portalId={PortalId},cultureCode={CultureCode})", synonymsGroupId, portalId, cultureCode);
        }

        /// <inheritdoc />
        public SearchStopWords GetSearchStopWords(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
            var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default);
			Logger.LogError("GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode}): cacheArg → {CacheArg}", portalId, cultureCode, cacheArg);
            var list = CBO.GetCachedObject<IList<SearchStopWords>>(this.hostSettings, cacheArg, this.GetSearchStopWordsCallBack);
			Logger.LogError("GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode}): list → {List}", portalId, cultureCode, list);
			Logger.LogError("GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", portalId, cultureCode, cacheKey);
			Logger.LogWarning("<<< Exiting GetSearchStopWords(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return list?.FirstOrDefault();
        }

        /// <inheritdoc />
        public int AddSearchStopWords(string stopWords, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering AddSearchStopWords(stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode})", stopWords, portalId, cultureCode);
            if (string.IsNullOrEmpty(stopWords))
            {
                return 0;
            }

            if (portalId < 0)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                return 0;
            }

            var tags = stopWords.ToLowerInvariant().Split(',');
			Logger.LogError("AddSearchStopWords(stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): tags → {Tags}", stopWords, portalId, cultureCode, tags);
            if (tags.Length == 0)
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
			Logger.LogError("AddSearchStopWords(stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): userId → {UserId}", stopWords, portalId, cultureCode, userId);
            var newId = DataProvider.Instance().AddSearchStopWords(stopWords, userId, portalId, cultureCode);
			Logger.LogError("AddSearchStopWords(stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): newId → {NewId}", stopWords, portalId, cultureCode, newId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
			Logger.LogError("AddSearchStopWords(stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", stopWords, portalId, cultureCode, cacheKey);
            DataCache.ClearCache(cacheKey);
			Logger.LogWarning("<<< Exiting AddSearchStopWords(stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode})", stopWords, portalId, cultureCode);
            return newId;
        }

        /// <inheritdoc />
        public int UpdateSearchStopWords(int stopWordsId, string stopWords, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode})", stopWordsId, stopWords, portalId, cultureCode);
            if (string.IsNullOrEmpty(stopWords))
            {
                return 0;
            }

            if (portalId < 0)
            {
                return 0;
            }

            if (stopWordsId <= 0)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                return 0;
            }

            var tags = stopWords.ToLowerInvariant().Split(',');
			Logger.LogError("UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): tags → {Tags}", stopWordsId, stopWords, portalId, cultureCode, tags);
            if (tags.Length == 0)
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
			Logger.LogError("UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): userId → {UserId}", stopWordsId, stopWords, portalId, cultureCode, userId);
            DataProvider.Instance().UpdateSearchStopWords(stopWordsId, stopWords, userId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
			Logger.LogError("UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", stopWordsId, stopWords, portalId, cultureCode, cacheKey);
            DataCache.ClearCache(cacheKey);
			Logger.LogWarning("<<< Exiting UpdateSearchStopWords(stopWordsId={StopWordsId},stopWords={StopWords},portalId={PortalId},cultureCode={CultureCode})", stopWordsId, stopWords, portalId, cultureCode);
            return stopWordsId;
        }

        /// <inheritdoc />
        public void DeleteSearchStopWords(int stopWordsId, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering DeleteSearchStopWords(stopWordsId={StopWordsId},portalId={PortalId},cultureCode={CultureCode})", stopWordsId, portalId, cultureCode);
            if (stopWordsId <= 0)
            {
                return;
            }

            DataProvider.Instance().DeleteSearchStopWords(stopWordsId);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
			Logger.LogError("DeleteSearchStopWords(stopWordsId={StopWordsId},portalId={PortalId},cultureCode={CultureCode}): cacheKey → {CacheKey}", stopWordsId, portalId, cultureCode, cacheKey);
            DataCache.ClearCache(cacheKey);
			Logger.LogWarning("<<< Exiting DeleteSearchStopWords(stopWordsId={StopWordsId},portalId={PortalId},cultureCode={CultureCode})", stopWordsId, portalId, cultureCode);
        }

        /// <inheritdoc />
        public DateTime GetSearchReindexRequestTime(int portalId)
        {
			Logger.LogError(">>> Entering GetSearchReindexRequestTime(portalId={PortalId})", portalId);
            var requestedOn = SqlDateTime.MinValue.Value;

            var reindexRequest = portalId < 0
                ? this.hostSettingsService.GetString(Constants.SearchReindexSettingName, Null.NullString) // host level setting
                : PortalController.GetPortalSetting(this.portalController, Constants.SearchReindexSettingName, portalId, Null.NullString); // portal level setting

            if (reindexRequest != Null.NullString)
            {
                DateTime.TryParseExact(reindexRequest, Constants.ReindexDateTimeFormat, null, DateTimeStyles.None, out requestedOn);
            }

			Logger.LogWarning("<<< Exiting GetSearchReindexRequestTime(portalId={PortalId})", portalId);
            return requestedOn;
        }

        /// <inheritdoc />
        public DateTime SetSearchReindexRequestTime(int portalId)
        {
			Logger.LogError(">>> Entering SetSearchReindexRequestTime(portalId={PortalId})", portalId);
            var now = DateTime.Now;
            var text = now.ToString(Constants.ReindexDateTimeFormat, CultureInfo.InvariantCulture);
			Logger.LogError("SetSearchReindexRequestTime(portalId={PortalId}): text → {Text}", portalId, text);

            if (portalId < 0)
            {
                // host level setting
                this.hostSettingsService.Update(Constants.SearchReindexSettingName, text, true);
            }
            else
            {
                // portal level setting
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.SearchReindexSettingName, text, true);
            }

			Logger.LogWarning("<<< Exiting SetSearchReindexRequestTime(portalId={PortalId})", portalId);
            return now;
        }

        /// <inheritdoc />
        public bool GetSearchCompactFlag()
        {
			Logger.LogError(">>> Entering GetSearchCompactFlag()");
            Logger.LogWarning("<<< Exiting GetSearchCompactFlag()");
            return this.hostSettingsService.GetString(Constants.SearchOptimizeFlagName, Null.NullString) == "1";
        }

        /// <inheritdoc />
        public void SetSearchReindexRequestTime(bool turnOn)
        {
            this.hostSettingsService.Update(Constants.SearchOptimizeFlagName, turnOn ? "1" : "0", true);
        }

        /// <inheritdoc />
        public bool IsReindexRequested(int portalId, DateTime startDate)
        {
			Logger.LogError(">>> Entering IsReindexRequested(portalId={PortalId},startDate={StartDate})", portalId, startDate);
            var reindexDateTime = this.GetSearchReindexRequestTime(portalId);
			Logger.LogError("IsReindexRequested(portalId={PortalId},startDate={StartDate}): reindexDateTime → {ReindexDateTime}", portalId, startDate, reindexDateTime);
			Logger.LogWarning("<<< Exiting IsReindexRequested(portalId={PortalId},startDate={StartDate})", portalId, startDate);
            return reindexDateTime > startDate;
        }

        /// <summary>Returns a collection of portal ID's to reindex if it was requested since last indexing.</summary>
        /// <param name="startDate">The date of the last index.</param>
        /// <returns>A sequence of portal IDs.</returns>
        public IEnumerable<int> GetPortalsToReindex(DateTime startDate)
        {
			Logger.LogError(">>> Entering GetPortalsToReindex(startDate={StartDate})", startDate);
            var portals2Reindex = PortalController.Instance.GetPortals().Cast<IPortalInfo>()
                .Where(portal => this.IsReindexRequested(portal.PortalId, startDate))
                .Select(portal => portal.PortalId);

            if (this.IsReindexRequested(-1, startDate))
            {
                // Include Host Level
                portals2Reindex = portals2Reindex.Concat([-1,]);
				Logger.LogError("GetPortalsToReindex(startDate={StartDate}): portals2Reindex → {Portals2Reindex}", startDate, portals2Reindex);
            }

			Logger.LogWarning("<<< Exiting GetPortalsToReindex(startDate={StartDate})", startDate);
            return portals2Reindex.ToArray();
        }

        /// <inheritdoc />
        public DateTime GetLastSuccessfulIndexingDateTime(int scheduleId)
        {
			Logger.LogError(">>> Entering GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId})", scheduleId);
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
            var lastValue = settings[Constants.SearchLastSuccessIndexName] as string;
			Logger.LogError("GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId}): lastValue → {LastValue}", scheduleId, lastValue);
			Logger.LogError("GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId}): settings → {Settings}", scheduleId, settings);

            if (string.IsNullOrEmpty(lastValue))
            {
                // try to fallback to old location where this was stored
                var name = string.Format(CultureInfo.InvariantCulture, LastIndexKeyFormat, Constants.SearchLastSuccessIndexName, scheduleId);
				Logger.LogError("GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId}): name → {Name}", scheduleId, name);
                lastValue = this.hostSettingsService.GetString(name, Null.NullString);
            }

            if (!string.IsNullOrEmpty(lastValue) &&
                DateTime.TryParseExact(lastValue, Constants.ReindexDateTimeFormat, null, DateTimeStyles.None, out var lastTime))
            {
                // retrieves the date as UTC but returns to caller as local
                lastTime = FixSqlDateTime(lastTime).ToLocalTime().ToLocalTime();
				Logger.LogDebug("GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId}): lastTime → {LastTime}", scheduleId, lastTime);
                if (lastTime > DateTime.Now)
                {
                    lastTime = DateTime.Now;
					Logger.LogError("GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId}): lastTime → {LastTime}", scheduleId, lastTime);
                }
            }
            else
            {
                lastTime = SqlDateTime.MinValue.Value.AddDays(1);
            }

			Logger.LogWarning("<<< Exiting GetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId})", scheduleId);
            return lastTime;
        }

        /// <inheritdoc />
        public void SetLastSuccessfulIndexingDateTime(int scheduleId, DateTime startDateLocal)
        {
			Logger.LogError(">>> Entering SetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId},startDateLocal={StartDateLocal})", scheduleId, startDateLocal);
            SchedulingProvider.Instance()
                .AddScheduleItemSetting(
                    scheduleId,
                    Constants.SearchLastSuccessIndexName,
                    startDateLocal.ToUniversalTime().ToString(Constants.ReindexDateTimeFormat, CultureInfo.InvariantCulture));
					Logger.LogWarning("<<< Exiting SetLastSuccessfulIndexingDateTime(scheduleId={ScheduleId},startDateLocal={StartDateLocal})", scheduleId, startDateLocal);
        }

        /// <inheritdoc />
        public DateTime GetIndexerCheckpointUtcTime(int scheduleId, string indexerKey)
        {
			Logger.LogError(">>> Entering GetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey})", scheduleId, indexerKey);
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
            var lastValue = settings[indexerKey] as string;
			Logger.LogError("GetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey}): settings → {Settings}", scheduleId, indexerKey, settings);

            if (!string.IsNullOrEmpty(lastValue) &&
                DateTime.TryParseExact(lastValue, Constants.ReindexDateTimeFormat, null, DateTimeStyles.None, out var lastUtcTime))
            {
                lastUtcTime = FixSqlDateTime(lastUtcTime);
				Logger.LogError("GetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey}): lastUtcTime → {LastUtcTime}", scheduleId, indexerKey, lastUtcTime);
            }
            else
            {
                lastUtcTime = DateTime.UtcNow;
				Logger.LogDebug("GetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey}): lastUtcTime → {LastUtcTime}", scheduleId, indexerKey, lastUtcTime);
            }

			Logger.LogWarning("<<< Exiting GetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey})", scheduleId, indexerKey);
            return lastUtcTime;
        }

        /// <inheritdoc />
        public void SetIndexerCheckpointUtcTime(int scheduleId, string indexerKey, DateTime lastUtcTime)
        {
			Logger.LogError(">>> Entering SetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey},lastUtcTime={LastUtcTime})", scheduleId, indexerKey, lastUtcTime);
            SchedulingProvider.Instance().AddScheduleItemSetting(scheduleId, indexerKey, lastUtcTime.ToString(Constants.ReindexDateTimeFormat, CultureInfo.InvariantCulture));
			Logger.LogWarning("<<< Exiting SetIndexerCheckpointUtcTime(scheduleId={ScheduleId},indexerKey={IndexerKey},lastUtcTime={LastUtcTime})", scheduleId, indexerKey, lastUtcTime);
        }

        /// <inheritdoc />
        public string GetIndexerCheckpointData(int scheduleId, string indexerKey)
        {
			Logger.LogError(">>> Entering GetIndexerCheckpointData(scheduleId={ScheduleId},indexerKey={IndexerKey})", scheduleId, indexerKey);
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
			Logger.LogError("GetIndexerCheckpointData(scheduleId={ScheduleId},indexerKey={IndexerKey}): settings → {Settings}", scheduleId, indexerKey, settings);
			Logger.LogWarning("<<< Exiting GetIndexerCheckpointData(scheduleId={ScheduleId},indexerKey={IndexerKey})", scheduleId, indexerKey);
            return settings[indexerKey] as string;
        }

        /// <inheritdoc />
        public void SetIndexerCheckpointData(int scheduleId, string indexerKey, string checkPointData)
        {
			Logger.LogError(">>> Entering SetIndexerCheckpointData(scheduleId={ScheduleId},indexerKey={IndexerKey},checkPointData={CheckPointData})", scheduleId, indexerKey, checkPointData);
            SchedulingProvider.Instance().AddScheduleItemSetting(scheduleId, indexerKey, checkPointData);
			Logger.LogWarning("<<< Exiting SetIndexerCheckpointData(scheduleId={ScheduleId},indexerKey={IndexerKey},checkPointData={CheckPointData})", scheduleId, indexerKey, checkPointData);
        }

        /// <inheritdoc />
        public Tuple<int, int> GetSearchMinMaxLength()
        {
            var minWordLength = this.hostSettingsService.GetInteger(Constants.SearchMinLengthKey, Constants.DefaultMinLen);
            var maxWordLength = this.hostSettingsService.GetInteger(Constants.SearchMaxLengthKey, Constants.DefaultMaxLen);

            if (minWordLength < Constants.MinimumMinLen)
            {
                minWordLength = Constants.MinimumMinLen;
            }

            if (maxWordLength < Constants.MinimumMaxLen)
            {
                maxWordLength = Constants.MinimumMaxLen;
            }

            if (minWordLength > Constants.MaximumMinLen)
            {
                minWordLength = Constants.MaximumMinLen;
            }

            if (maxWordLength > Constants.MaximumMaxLen)
            {
                maxWordLength = Constants.MaximumMaxLen;
            }

            if (minWordLength > maxWordLength)
            {
                var exceptionMessage = Localization.GetExceptionMessage("SearchAnalyzerMinWordLength", "Search Analyzer: min word length ({0}) is greater than max word length ({1}) value");
                throw new InvalidDataException(
                    string.Format(CultureInfo.InvariantCulture, exceptionMessage, minWordLength, maxWordLength));
            }

            return new Tuple<int, int>(minWordLength, maxWordLength);
        }

        /// <summary>Processes and re-phrases the search text by looking into exact-match and wildcard option.</summary>
        /// <param name="searchPhrase">The original search phrase.</param>
        /// <param name="useWildCard">Whether to use wildcards.</param>
        /// <param name="allowLeadingWildcard">Whether to allow a leading wildcard.</param>
        /// <returns>cleaned and pre-processed search phrase.</returns>
        public string RephraseSearchText(string searchPhrase, bool useWildCard, bool allowLeadingWildcard = false)
        {
			Logger.LogError(">>> Entering RephraseSearchText(searchPhrase={SearchPhrase},useWildCard={UseWildCard},allowLeadingWildcard={AllowLeadingWildcard})", searchPhrase, useWildCard, allowLeadingWildcard);
            searchPhrase = CleanSearchPhrase(HttpUtility.HtmlDecode(searchPhrase));

			Logger.LogError("RephraseSearchText(searchPhrase={SearchPhrase},useWildCard={UseWildCard},allowLeadingWildcard={AllowLeadingWildcard}): searchPhrase → {SearchPhrase1}", searchPhrase, useWildCard, allowLeadingWildcard, searchPhrase);
            if (!useWildCard && !searchPhrase.Contains("\""))
            {
                return searchPhrase;
            }

            // we have a quotation marks and/or wildcard search, adjust accordingly
            var chars = FoldToAscii(searchPhrase).ToCharArray();
			Logger.LogError("RephraseSearchText(searchPhrase={SearchPhrase},useWildCard={UseWildCard},allowLeadingWildcard={AllowLeadingWildcard}): chars → {Chars}", searchPhrase, useWildCard, allowLeadingWildcard, chars);
            var insideQuote = false;
			Logger.LogError("RephraseSearchText(searchPhrase={SearchPhrase},useWildCard={UseWildCard},allowLeadingWildcard={AllowLeadingWildcard}): insideQuote → {InsideQuote}", searchPhrase, useWildCard, allowLeadingWildcard, insideQuote);
            var newPhraseBulder = new StringBuilder();
			Logger.LogError("RephraseSearchText(searchPhrase={SearchPhrase},useWildCard={UseWildCard},allowLeadingWildcard={AllowLeadingWildcard}): newPhraseBulder → {NewPhraseBulder}", searchPhrase, useWildCard, allowLeadingWildcard, newPhraseBulder);
            var currentWord = new StringBuilder();
			Logger.LogError("RephraseSearchText(searchPhrase={SearchPhrase},useWildCard={UseWildCard},allowLeadingWildcard={AllowLeadingWildcard}): currentWord → {CurrentWord}", searchPhrase, useWildCard, allowLeadingWildcard, currentWord);

            foreach (var c in chars)
            {
                currentWord.Append(c);
                switch (c)
                {
                    case '"':
                        insideQuote = !insideQuote;
                        if (!insideQuote)
                        {
                            newPhraseBulder.Append(currentWord + " ");
                            currentWord.Clear();
                        }

                        break;
                    case ' ':
                        if (!insideQuote && useWildCard)
                        {
                            // end of a word; we need to append a wild card to search when needed
                            newPhraseBulder.Append(FixLastWord(currentWord.ToString().Trim(), allowLeadingWildcard) + " ");
                            currentWord.Clear();
                        }

                        break;
                }
            }

            // in case the a double quote was not closed
            if (insideQuote)
            {
                currentWord.Append('"');
                newPhraseBulder.Append(currentWord);
            }
            else if (useWildCard)
            {
                newPhraseBulder.Append(FixLastWord(currentWord.ToString().Trim(), allowLeadingWildcard));
            }
            else
            {
                newPhraseBulder.Append(currentWord);
            }

            return newPhraseBulder.ToString().Trim().Replace("  ", " ");
        }

        /// <inheritdoc />
        public string StripTagsNoAttributes(string html, bool retainSpace)
        {
			Logger.LogError(">>> Entering StripTagsNoAttributes(html={Html},retainSpace={RetainSpace})", html, retainSpace);
            var strippedString = !string.IsNullOrEmpty(html) ? HtmlUtils.StripTags(html, retainSpace) : html;

			Logger.LogError("StripTagsNoAttributes(html={Html},retainSpace={RetainSpace}): strippedString → {StrippedString}", html, retainSpace, strippedString);
            // Encode and Strip again
            strippedString = !string.IsNullOrEmpty(strippedString) ? HtmlUtils.StripTags(html, retainSpace) : html;

			Logger.LogWarning("<<< Exiting StripTagsNoAttributes(html={Html},retainSpace={RetainSpace})", html, retainSpace);
            return strippedString;
        }

        private static DateTime FixSqlDateTime(DateTime datim)
        {
			Logger.LogError(">>> Entering FixSqlDateTime(datim={Datim})", datim);
            if (datim <= SqlDateTime.MinValue.Value)
            {
                datim = SqlDateTime.MinValue.Value.AddDays(1);
				Logger.LogError("FixSqlDateTime(datim={Datim}): datim → {Datim1}", datim, datim);
            }
            else if (datim >= SqlDateTime.MaxValue.Value)
            {
                datim = SqlDateTime.MaxValue.Value.AddDays(-1);
            }

			Logger.LogWarning("<<< Exiting FixSqlDateTime(datim={Datim})", datim);
            return datim;
        }

        private static string FixLastWord(string lastWord, bool allowLeadingWildcard)
        {
			Logger.LogError(">>> Entering FixLastWord(lastWord={LastWord},allowLeadingWildcard={AllowLeadingWildcard})", lastWord, allowLeadingWildcard);
            if (string.IsNullOrEmpty(lastWord))
            {
                return string.Empty;
            }

            if (lastWord.IndexOfAny(['~', '*',]) < 0)
            {
                var beginIsGroup = false;
				Logger.LogError("FixLastWord(lastWord={LastWord},allowLeadingWildcard={AllowLeadingWildcard}): beginIsGroup → {BeginIsGroup}", lastWord, allowLeadingWildcard, beginIsGroup);
                var endIsGroup = false;
				Logger.LogError("FixLastWord(lastWord={LastWord},allowLeadingWildcard={AllowLeadingWildcard}): endIsGroup → {EndIsGroup}", lastWord, allowLeadingWildcard, endIsGroup);
                var wordStartPos = 0;
                var wordEndPos = lastWord.Length;

                var c1 = lastWord[0];
                var c2 = lastWord[wordEndPos - 1];

                if (c1 is '(' or '{' or '[')
                {
                    wordStartPos++;
                    beginIsGroup = true;
                }

                if (c2 is ')' or '}' or ']')
                {
                    wordEndPos--;
                    endIsGroup = true;
                }

                // pos points to next character after space
                var wordLen = wordEndPos - wordStartPos;
                lastWord = lastWord.Substring(wordStartPos, wordLen);
				Logger.LogError("FixLastWord(lastWord={LastWord},allowLeadingWildcard={AllowLeadingWildcard}): lastWord → {LastWord1}", lastWord, allowLeadingWildcard, lastWord);

                // reserved words used for logical operations are case sensitive
                if (lastWord.Length > 0 && lastWord != "AND" && lastWord != "OR")
                {
                    lastWord = (beginIsGroup && endIsGroup)
                        ? string.Format(CultureInfo.InvariantCulture, "{0} OR {1}{0}*", lastWord, allowLeadingWildcard ? "*" : string.Empty)
                        : string.Format(CultureInfo.InvariantCulture, "({0} OR {1}{0}*)", lastWord, allowLeadingWildcard ? "*" : string.Empty);
                }

                if (beginIsGroup)
                {
                    lastWord = c1 + lastWord;
                }

                if (endIsGroup)
                {
                    lastWord += c2;
                }
            }

			Logger.LogWarning("<<< Exiting FixLastWord(lastWord={LastWord},allowLeadingWildcard={AllowLeadingWildcard})", lastWord, allowLeadingWildcard);
            return lastWord;
        }

        private static string CleanSearchPhrase(string searchPhrase)
        {
			Logger.LogError(">>> Entering CleanSearchPhrase(searchPhrase={SearchPhrase})", searchPhrase);
            var chars = searchPhrase.ToCharArray();
            var hasExactMatch = false;
			Logger.LogError("CleanSearchPhrase(searchPhrase={SearchPhrase}): hasExactMatch → {HasExactMatch}", searchPhrase, hasExactMatch);
			Logger.LogError("CleanSearchPhrase(searchPhrase={SearchPhrase}): chars → {Chars}", searchPhrase, chars);
            int pos;

            // replace all forms of double quotes or whitespace with the normal ones
            for (pos = 0; pos < chars.Length; pos++)
            {
                var c = chars[pos];
                switch (c)
                {
                    case '\u02BA': // 'ʺ' Modifier Letter Double Prime
                    case '\u02EE': // 'ˮ' Modifier Letter Double Apostrophe
                    case '\u00AB': // [LEFT-POINTING DOUBLE ANGLE QUOTATION MARK]
                    case '\u00BB': // [RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK]
                    case '\u201C': // [LEFT DOUBLE QUOTATION MARK]
                    case '\u201D': // [RIGHT DOUBLE QUOTATION MARK]
                    case '\u201E': // [DOUBLE LOW-9 QUOTATION MARK]
                    case '\u2033': // [DOUBLE PRIME]
                    case '\u2036': // [REVERSED DOUBLE PRIME]
                    case '\u275D': // [HEAVY DOUBLE TURNED COMMA QUOTATION MARK ORNAMENT]
                    case '\u275E': // [HEAVY DOUBLE COMMA QUOTATION MARK ORNAMENT]
                    case '\u276E': // [HEAVY LEFT-POINTING ANGLE QUOTATION MARK ORNAMENT]
                    case '\u276F': // [HEAVY RIGHT-POINTING ANGLE QUOTATION MARK ORNAMENT]
                    case '\uFF02': // [FULLWIDTH QUOTATION MARK]
                        chars[pos] = '"';  // Quotation Mark
                        hasExactMatch = true;
                        break;
                    case '\u0009': // HT, Horizontal Tab
                    case '\u000A': // LF, Line feed
                    case '\u000B': // VT, Vertical Tab
                    case '\u000C': // FF, Form feed
                    case '\u000D': // CR, Carriage return
                    case '\u00A0': // NBSP, No-break space
                        chars[pos] = ' ';  // space
                        break;
                }
            }

            // remove double apaces
            var cleaned = new string(chars).Replace("  ", " ");
			Logger.LogError("CleanSearchPhrase(searchPhrase={SearchPhrase}): cleaned → {Cleaned}", searchPhrase, cleaned);

            if (hasExactMatch)
            {
                // remove empty double quotes
                cleaned = cleaned.Replace("\"\"", string.Empty).Replace("\" \"", string.Empty);
            }

			Logger.LogWarning("<<< Exiting CleanSearchPhrase(searchPhrase={SearchPhrase})", searchPhrase);
            return cleaned;
        }

        private static void EnsurePortalDefaultsAreSet(IPortalController portalController, IApplicationStatusInfo appStatus, int portalId)
        {
			Logger.LogError(">>> Entering EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId})", portalController, appStatus, portalId);
            const string setting = "SearchAdminInitialization";

            // check portal settings first
            if (PortalController.GetPortalSetting(portalController, setting, portalId, "false") != "false")
            {
                return;
            }

            // Portal may not be present, especially during installation
            if (PortalController.Instance.GetPortal(portalId) == null)
            {
                return;
            }

            foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values)
            {
                var resourceFile = GetResourceFile(appStatus, locale.Code);
				Logger.LogError("EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId}): resourceFile → {ResourceFile}", portalController, appStatus, portalId, resourceFile);

                var currentStopWords = CBO.FillCollection<SearchStopWords>(DataProvider.Instance().GetSearchStopWords(portalId, locale.Code));
				Logger.LogError("EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId}): currentStopWords → {CurrentStopWords}", portalController, appStatus, portalId, currentStopWords);
                if (currentStopWords == null || currentStopWords.Count == 0)
                {
                    // Add Default StopWord
                    var defaultStopWords = Localization.GetString("DefaultStopwordGroup", resourceFile);
					Logger.LogError("EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId}): defaultStopWords → {DefaultStopWords}", portalController, appStatus, portalId, defaultStopWords);
                    if (!string.IsNullOrEmpty(defaultStopWords))
                    {
                        DataProvider.Instance().AddSearchStopWords(defaultStopWords, 1, portalId, locale.Code);
                    }
                }

                var currentSynonymGroups = CBO.FillCollection<SynonymsGroup>(DataProvider.Instance().GetAllSynonymsGroups(portalId, locale.Code));
				Logger.LogError("EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId}): currentSynonymGroups → {CurrentSynonymGroups}", portalController, appStatus, portalId, currentSynonymGroups);
                if (currentSynonymGroups == null || currentSynonymGroups.Count == 0)
                {
                    // Add Default Synonym
                    var defaultSynonymsGroup = Localization.GetString("DefaultSynonymGroup", resourceFile);
					Logger.LogError("EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId}): defaultSynonymsGroup → {DefaultSynonymsGroup}", portalController, appStatus, portalId, defaultSynonymsGroup);
                    if (!string.IsNullOrEmpty(defaultSynonymsGroup))
                    {
                        DataProvider.Instance().AddSynonymsGroup(defaultSynonymsGroup, 1, portalId, locale.Code);
                    }
                }
            }

            // Update Portal Settings
            PortalController.UpdatePortalSetting(portalController, portalId, setting, "true", true);
			Logger.LogWarning("<<< Exiting EnsurePortalDefaultsAreSet(portalController={PortalController},appStatus={AppStatus},portalId={PortalId})", portalController, appStatus, portalId);
        }

        private static string GetResourceFile(IApplicationStatusInfo appStatus, string cultureCode)
        {
			Logger.LogError(">>> Entering GetResourceFile(appStatus={AppStatus},cultureCode={CultureCode})", appStatus, cultureCode);
            var cultureRelativePath = "~" + ResourceFileRelativePathWithoutExt + "." + cultureCode + ".resx";
            const string regularRelativePath = "~" + ResourceFileRelativePathWithoutExt + ".resx";
			Logger.LogError("GetResourceFile(appStatus={AppStatus},cultureCode={CultureCode}): cultureRelativePath → {CultureRelativePath}", appStatus, cultureCode, cultureRelativePath);
			Logger.LogWarning("<<< Exiting GetResourceFile(appStatus={AppStatus},cultureCode={CultureCode})", appStatus, cultureCode);
            return File.Exists(Path.Combine(appStatus.ApplicationMapPath, ResourceFileRelativePathWithoutExt + "." + cultureCode + ".resx")) ? cultureRelativePath : regularRelativePath;
        }

        private static object GetSynonymsGroupsCallBack(CacheItemArgs cacheItem)
        {
			Logger.LogError(">>> Entering GetSynonymsGroupsCallBack(cacheItem={CacheItem})", cacheItem);
            var portalController = (IPortalController)cacheItem.ParamList[0];
            var appStatus = (IApplicationStatusInfo)cacheItem.ParamList[1];
			Logger.LogError("GetSynonymsGroupsCallBack(cacheItem={CacheItem}): appStatus → {AppStatus}", cacheItem, appStatus);
            var portalId = int.Parse(cacheItem.CacheKey.Split('_')[1], CultureInfo.InvariantCulture);
			Logger.LogError("GetSynonymsGroupsCallBack(cacheItem={CacheItem}): portalId → {PortalId}", cacheItem, portalId);
            var cultureCode = cacheItem.CacheKey.Split('_')[2];
			Logger.LogError("GetSynonymsGroupsCallBack(cacheItem={CacheItem}): cultureCode → {CultureCode}", cacheItem, cultureCode);
			Logger.LogError("GetSynonymsGroupsCallBack(cacheItem={CacheItem}): portalController → {PortalController}", cacheItem, portalController);

            EnsurePortalDefaultsAreSet(portalController, appStatus, portalId);

			Logger.LogWarning("<<< Exiting GetSynonymsGroupsCallBack(cacheItem={CacheItem})", cacheItem);
            return CBO.FillCollection<SynonymsGroup>(DataProvider.Instance().GetAllSynonymsGroups(portalId, cultureCode));
        }

        private static string FoldToAscii(string searchPhrase)
        {
			Logger.LogError(">>> Entering FoldToAscii(searchPhrase={SearchPhrase})", searchPhrase);
            var sb = new StringBuilder();

			Logger.LogError("FoldToAscii(searchPhrase={SearchPhrase}): sb → {Sb}", searchPhrase, sb);
            var cleanedPhrase = searchPhrase.Trim('\0');
			Logger.LogError("FoldToAscii(searchPhrase={SearchPhrase}): cleanedPhrase → {CleanedPhrase}", searchPhrase, cleanedPhrase);

            var asciiFilter = new ASCIIFoldingFilter(new WhitespaceTokenizer(new StringReader(cleanedPhrase)));
			Logger.LogError("FoldToAscii(searchPhrase={SearchPhrase}): asciiFilter → {AsciiFilter}", searchPhrase, asciiFilter);

            string space = string.Empty;
			Logger.LogError("FoldToAscii(searchPhrase={SearchPhrase}): space → {Space}", searchPhrase, space);
            while (asciiFilter.IncrementToken())
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", space, asciiFilter.GetAttribute<ITermAttribute>().Term);
                if (string.IsNullOrEmpty(space))
                {
                    space = " ";
                }
            }

			Logger.LogWarning("<<< Exiting FoldToAscii(searchPhrase={SearchPhrase})", searchPhrase);
            return sb.ToString();
        }

        private IDictionary<string, IList<string>> GetSynonymTerms(int portalId, string cultureCode)
        {
            var cacheKey = $"{SynonymTermsCacheKey}_{portalId}_{cultureCode}";
            var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default);
            return CBO.GetCachedObject<IDictionary<string, IList<string>>>(this.hostSettings, cacheArg, this.SynonymTermsCallBack);
        }

        private object SynonymTermsCallBack(CacheItemArgs cacheItem)
        {
			Logger.LogError(">>> Entering SynonymTermsCallBack(cacheItem={CacheItem})", cacheItem);
            var parts = cacheItem.CacheKey.Split('_');
            var allTerms = new Dictionary<string, IList<string>>();
			Logger.LogError("SynonymTermsCallBack(cacheItem={CacheItem}): allTerms → {AllTerms}", cacheItem, allTerms);
            var portalId = int.Parse(parts[1], CultureInfo.InvariantCulture);
			Logger.LogError("SynonymTermsCallBack(cacheItem={CacheItem}): portalId → {PortalId}", cacheItem, portalId);
			Logger.LogError("SynonymTermsCallBack(cacheItem={CacheItem}): parts → {Parts}", cacheItem, parts);
            var cultureCode = parts[2];
            var groups = this.GetSynonymsGroups(portalId, cultureCode);
			Logger.LogError("SynonymTermsCallBack(cacheItem={CacheItem}): groups → {Groups}", cacheItem, groups);
            if (groups == null)
            {
                return allTerms;
            }

            foreach (var synonymsGroup in groups)
            {
                var terms = new Dictionary<string, IList<string>>();
				Logger.LogError("SynonymTermsCallBack(cacheItem={CacheItem}): terms → {Terms}", cacheItem, terms);
                var groupTags = synonymsGroup.SynonymsTags.ToLowerInvariant().Split(',');
				Logger.LogError("SynonymTermsCallBack(cacheItem={CacheItem}): groupTags → {GroupTags}", cacheItem, groupTags);

                // add the first key first
                foreach (var tag in groupTags)
                {
                    if (!terms.ContainsKey(tag))
                    {
                        terms.Add(tag, new List<string>());
                    }
                }

                // add synonyms
                foreach (var term in terms)
                {
                    foreach (var syn in terms)
                    {
                        if (term.Key != syn.Key)
                        {
                            term.Value.Add(syn.Key);
                        }
                    }
                }

                foreach (var term in terms)
                {
                    allTerms.Add(term.Key, term.Value);
                }
            }

			Logger.LogWarning("<<< Exiting SynonymTermsCallBack(cacheItem={CacheItem})", cacheItem);
            return allTerms;
        }

        private object GetSearchStopWordsCallBack(CacheItemArgs cacheItem)
        {
			Logger.LogError(">>> Entering GetSearchStopWordsCallBack(cacheItem={CacheItem})", cacheItem);
            var splittedKeys = cacheItem.CacheKey.Split('_');
            var portalId = int.Parse(splittedKeys[1], CultureInfo.InvariantCulture);
			Logger.LogError("GetSearchStopWordsCallBack(cacheItem={CacheItem}): portalId → {PortalId}", cacheItem, portalId);
			Logger.LogError("GetSearchStopWordsCallBack(cacheItem={CacheItem}): splittedKeys → {SplittedKeys}", cacheItem, splittedKeys);
            var cultureCode = splittedKeys[2];

            EnsurePortalDefaultsAreSet(this.portalController, this.appStatus, portalId);

			Logger.LogWarning("<<< Exiting GetSearchStopWordsCallBack(cacheItem={CacheItem})", cacheItem);
            return CBO.FillCollection<SearchStopWords>(DataProvider.Instance().GetSearchStopWords(portalId, cultureCode));
        }
    }
}
