// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Services.Dto;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class PageUrlsController : ServiceLocator<IPageUrlsController, PageUrlsController>, IPageUrlsController
    {
        private enum SortingFields
        {
            None = 0,
            Url = 1,
            Locale = 2,
            Status = 3,
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected IEnumerable<KeyValuePair<int, string>> StatusCodes =>
        [
            new KeyValuePair<int, string>(200, "Active (200)"),
            new KeyValuePair<int, string>(301, "Redirect (301)")
        ];

        /// <inheritdoc />
        public IEnumerable<Url> GetPageUrls(TabInfo tab, int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetPageUrls(tab={Tab},portalId={PortalId})", tab, portalId);
            var locales = new Lazy<Dictionary<string, Locale>>(() => LocaleController.Instance.GetLocales(portalId));
            var customUrls = this.GetSortedUrls(tab, portalId, locales, 1, true, false);
			eventLogger.LogDebug("GetPageUrls(tab={Tab},portalId={PortalId}): customUrls → {CustomUrls}", tab, portalId, customUrls);
            var automaticUrls = this.GetSortedUrls(tab, portalId, locales, 1, true, true).ToList();
			eventLogger.LogDebug("GetPageUrls(tab={Tab},portalId={PortalId}): automaticUrls → {AutomaticUrls}", tab, portalId, automaticUrls);
			eventLogger.LogDebug("GetPageUrls(tab={Tab},portalId={PortalId}): locales → {Locales}", tab, portalId, locales);

            automaticUrls.AddRange(customUrls);
			eventLogger.LogDebug("<<< Exiting GetPageUrls(tab={Tab},portalId={PortalId})", tab, portalId);
            return automaticUrls.OrderBy(url => url.StatusCode, new KeyValuePairComparer()).ThenBy(url => url.Path);
        }

        /// <inheritdoc />
        public PageUrlResult CreateCustomUrl(SaveUrlDto dto, TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering CreateCustomUrl(dto={Dto},tab={Tab})", dto, tab);
            var portalSettings = PortalSettings.Current;
            IPortalInfo aliasPortal = new PortalAliasController().GetPortalByPortalAliasID(dto.SiteAliasKey);
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto},tab={Tab}): aliasPortal → {AliasPortal}", dto, tab, aliasPortal);

            if (aliasPortal != null && portalSettings.PortalId != aliasPortal.PortalId)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPortalAlias.Error"),
                    SuggestedUrlPath = string.Empty,
                };
            }

            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto},tab={Tab}): urlPath → {UrlPath}", dto, tab, urlPath);

            // Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto},tab={Tab}): options → {Options}", dto, tab, options);

            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out var modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                    SuggestedUrlPath = "/" + urlPath,
                };
            }

            // Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, portalSettings, out modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                    SuggestedUrlPath = "/" + urlPath,
                };
            }

            if (tab.TabUrls.Any(u => u.Url.Equals(dto.Path.ValueOrEmpty(), StringComparison.OrdinalIgnoreCase)
                                     && (u.PortalAliasId == dto.SiteAliasKey || u.PortalAliasId == -1)))
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("DuplicateUrl.Error"),
                };
            }

            var seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto},tab={Tab}): seqNum → {SeqNum}", dto, tab, seqNum);
            var portalLocales = LocaleController.Instance.GetLocales(portalSettings.PortalId);
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto},tab={Tab}): portalLocales → {PortalLocales}", dto, tab, portalLocales);

            // Get the culture code of selected portal alias
            var alias = PortalAliasController.Instance
                .GetPortalAliasesByPortalId(portalSettings.PortalId)
                .SingleOrDefault(a => a.PortalAliasID == dto.SiteAliasKey);
            Func<KeyValuePair<string, Locale>, bool> localeFilter = null;
            if (alias != null &&
                !string.IsNullOrWhiteSpace(alias.CultureCode))
            {
                localeFilter = l => l.Key == alias.CultureCode;
            }
            else
            {
                localeFilter = l => l.Value.KeyID == dto.LocaleKey;
            }

            var cultureCode = portalLocales.Where(localeFilter)
                .Select(l => l.Value.Code)
                .SingleOrDefault() ?? portalSettings.CultureCode;

            var portalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto},tab={Tab}): portalAliasUsage → {PortalAliasUsage}", dto, tab, portalAliasUsage);
            if (portalAliasUsage == PortalAliasUsageType.Default)
            {
                if (string.IsNullOrEmpty(cultureCode) || alias == null)
                {
                    return new PageUrlResult
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error"),
                    };
                }
            }
            else
            {
                var cultureAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalSettings.PortalId)
                                                            .FirstOrDefault(a => a.CultureCode == cultureCode);

                if (portalLocales.Count > 1 && !portalSettings.ContentLocalizationEnabled && (string.IsNullOrEmpty(cultureCode) || cultureAlias == null))
                {
                    return new PageUrlResult
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error"),
                    };
                }
            }

            var tabUrl = new TabUrlInfo
            {
                TabId = tab.TabID,
                SeqNum = seqNum,
                PortalAliasId = dto.SiteAliasKey,
                PortalAliasUsage = portalAliasUsage,
                QueryString = dto.QueryString.ValueOrEmpty(),
                Url = dto.Path.ValueOrEmpty(),
                CultureCode = cultureCode,
                HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                IsSystem = false,
            };

            TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

			eventLogger.LogDebug("<<< Exiting CreateCustomUrl(dto={Dto},tab={Tab})", dto, tab);
            return new PageUrlResult
            {
                Success = true,
                Id = seqNum, // returns Id of the created Url
            };
        }

        /// <inheritdoc />
        public PageUrlResult UpdateCustomUrl(SaveUrlDto dto, TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering UpdateCustomUrl(dto={Dto},tab={Tab})", dto, tab);
            var portalSettings = PortalSettings.Current;
            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
			eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): urlPath → {UrlPath}", dto, tab, urlPath);

            // Clean Url
            var options =
                UrlRewriterUtils.ExtendOptionsForCustomURLs(
                    UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));

            // now clean the path
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out var modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                    SuggestedUrlPath = "/" + urlPath,
                };
            }

            // Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, tab.TabID, portalSettings, out modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                    SuggestedUrlPath = "/" + urlPath,
                };
            }

            var cultureCode = LocaleController.Instance.GetLocales(portalSettings.PortalId)
                .Where(l => l.Value.KeyID == dto.LocaleKey)
                .Select(l => l.Value.Code)
                .SingleOrDefault() ?? portalSettings.DefaultLanguage;

            var statusCodeKey = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture);
			eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): statusCodeKey → {StatusCodeKey}", dto, tab, statusCodeKey);
            var tabUrl = tab.TabUrls.SingleOrDefault(t => t.SeqNum == dto.Id && t.HttpStatus == statusCodeKey);
			eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): tabUrl → {TabUrl}", dto, tab, tabUrl);

            if (statusCodeKey == "200")
            {
                // We need to check if we are updating a current url or creating a new 200
                if (tabUrl == null)
                {
                    // Just create Url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = "200",
                        IsSystem = dto.IsSystem, // false
                    };
                    TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    if (!tabUrl.Url.Equals("/" + urlPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Change the original 200 url to a redirect
                        tabUrl.HttpStatus = "301";
						eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): tabUrl.HttpStatus → {Arg2}", dto, tab, tabUrl.HttpStatus);
						eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): tabUrl.SeqNum → {Arg2}", dto, tab, tabUrl.SeqNum);
                        tabUrl.SeqNum = dto.Id;
                        TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

                        // Add new custom url
                        tabUrl.Url = dto.Path.ValueOrEmpty();
                        tabUrl.HttpStatus = "200";
                        tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                        tabUrl.CultureCode = cultureCode;
						eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): tabUrl.CultureCode → {Arg2}", dto, tab, tabUrl.CultureCode);
						eventLogger.LogDebug("UpdateCustomUrl(dto={Dto},tab={Tab}): tabUrl.PortalAliasId → {Arg2}", dto, tab, tabUrl.PortalAliasId);
                        tabUrl.PortalAliasId = dto.SiteAliasKey;
                        tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                        tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                        TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    }
                    else
                    {
                        // Update the original 200 url
                        tabUrl.CultureCode = cultureCode;
                        tabUrl.PortalAliasId = dto.SiteAliasKey;
                        tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                        tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                        TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    }
                }
            }
            else
            {
                // Update the original non 200 url
                if (tabUrl == null)
                {
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = statusCodeKey,
                        IsSystem = dto.IsSystem, // false
                    };
                    TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    tabUrl.CultureCode = cultureCode;
                    tabUrl.PortalAliasId = dto.SiteAliasKey;
                    tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                    tabUrl.Url = dto.Path.ValueOrEmpty();
                    tabUrl.HttpStatus = statusCodeKey;
                    tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                    TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
            }

			eventLogger.LogDebug("<<< Exiting UpdateCustomUrl(dto={Dto},tab={Tab})", dto, tab);
            return new PageUrlResult
            {
                Success = true,
            };
        }

        /// <inheritdoc />
        public PageUrlResult DeleteCustomUrl(int id, TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering DeleteCustomUrl(id={Id},tab={Tab})", id, tab);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var tabUrl = tab.TabUrls.SingleOrDefault(u => u.SeqNum == id);
			eventLogger.LogDebug("DeleteCustomUrl(id={Id},tab={Tab}): tabUrl → {TabUrl}", id, tab, tabUrl);
			eventLogger.LogDebug("DeleteCustomUrl(id={Id},tab={Tab}): portalSettings → {PortalSettings}", id, tab, portalSettings);

            TabController.Instance.DeleteTabUrl(tabUrl, portalSettings.PortalId, true);

			eventLogger.LogDebug("<<< Exiting DeleteCustomUrl(id={Id},tab={Tab})", id, tab);
            return new PageUrlResult
            {
                Success = true,
            };
        }

        /// <inheritdoc />
        protected override Func<IPageUrlsController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new PageUrlsController();
        }

        private static string GetCleanPath(string path, FriendlyUrlSettings friendlyUrlSettings)
        {
			eventLogger.LogDebug(">>> Entering GetCleanPath(path={Path},friendlyUrlSettings={FriendlyUrlSettings})", path, friendlyUrlSettings);
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var urlPath = path.TrimStart('/');
			eventLogger.LogDebug("GetCleanPath(path={Path},friendlyUrlSettings={FriendlyUrlSettings}): urlPath → {UrlPath}", path, friendlyUrlSettings, urlPath);
            urlPath = UrlRewriterUtils.CleanExtension(urlPath, friendlyUrlSettings, string.Empty);

			eventLogger.LogDebug("<<< Exiting GetCleanPath(path={Path},friendlyUrlSettings={FriendlyUrlSettings})", path, friendlyUrlSettings);
            return $"/{urlPath}";
        }

        private IEnumerable<Url> GetSortedUrls(TabInfo tab, int portalId, Lazy<Dictionary<string, Locale>> locales, int sortColumn, bool sortOrder, bool isSystem)
        {
			eventLogger.LogDebug(">>> Entering GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem})", tab, portalId, locales, sortColumn, sortOrder, isSystem);
            var friendlyUrlSettings = new FriendlyUrlSettings(tab.PortalID);
            var tabs = new List<Url>();
			eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): tabs → {Tabs}", tab, portalId, locales, sortColumn, sortOrder, isSystem, tabs);
			eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): friendlyUrlSettings → {FriendlyUrlSettings}", tab, portalId, locales, sortColumn, sortOrder, isSystem, friendlyUrlSettings);

            if (isSystem)
            {
                // Add generated urls
                foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId))
                {
                    var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == alias.CultureCode);
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): urlLocale → {UrlLocale}", tab, portalId, locales, sortColumn, sortOrder, isSystem, urlLocale);

                    /*var isRedirected = tab.TabUrls.Any(u => u.HttpStatus == "200"
                                                            && u.CultureCode == ((urlLocale != null) ? urlLocale.Code : String.Empty))
                                            || alias.PortalAliasID != PrimaryAliasId;*/

                    bool isRedirected = false;
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): isRedirected → {IsRedirected}", tab, portalId, locales, sortColumn, sortOrder, isSystem, isRedirected);
                    var isCustom200Urls = tab.TabUrls.Any(u => u.HttpStatus == "200"); // are there any custom Urls for this tab?
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): isCustom200Urls → {IsCustom200Urls}", tab, portalId, locales, sortColumn, sortOrder, isSystem, isCustom200Urls);
                    var baseUrl = Globals.AddHTTP(alias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): baseUrl → {BaseUrl}", tab, portalId, locales, sortColumn, sortOrder, isSystem, baseUrl);
                    if (urlLocale != null)
                    {
                        baseUrl += "&language=" + urlLocale.Code;
                    }

                    string customPath = null;
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): customPath → {CustomPath}", tab, portalId, locales, sortColumn, sortOrder, isSystem, customPath);
                    if (isCustom200Urls)
                    {
                        // get the friendlyUrl, including custom Urls
                        customPath = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                            tab,
                            baseUrl,
                            Globals.glbDefaultPage,
                            alias.HTTPAlias,
                            false,
                            friendlyUrlSettings,
                            Guid.Empty);

                        customPath = customPath.Replace(Globals.AddHTTP(alias.HTTPAlias), string.Empty);
                    }

                    // get the friendlyUrl and ignore and custom Urls
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                        tab,
                        baseUrl,
                        Globals.glbDefaultPage,
                        alias.HTTPAlias,
                        true,
                        friendlyUrlSettings,
                        Guid.Empty);
						eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): path → {Path}", tab, portalId, locales, sortColumn, sortOrder, isSystem, path);

                    path = path.Replace(Globals.AddHTTP(alias.HTTPAlias), string.Empty);
                    int status = 200;
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): status → {Status}", tab, portalId, locales, sortColumn, sortOrder, isSystem, status);
                    if (customPath != null && !string.Equals(customPath, path, StringComparison.OrdinalIgnoreCase))
                    {
                        // difference in custom/standard URL, so standard is 301
                        status = 301;
                        isRedirected = true;
                    }

                    ////AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200);
                    // 27139 : only show primary aliases in the tab grid (gets too confusing otherwise)
                    if (alias.IsPrimary)
                    {
                        // alias was provided to FriendlyUrlCall, so will always get the correct canonical Url back
                        this.AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, string.Empty, status, isSystem, friendlyUrlSettings, null);
                    }

                    // Add url with diacritics
                    isRedirected = friendlyUrlSettings.RedirectUnfriendly;
                    bool replacedDiacritic;
                    string asciiTabPath = TabPathHelper.ReplaceDiacritics(tab.TabPath, out replacedDiacritic).Replace("//", "/");
                    if (replacedDiacritic)
                    {
                        if (friendlyUrlSettings.AutoAsciiConvert)
                        {
                            if (friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                            {
                                path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, string.Empty);
                            }

                            path = path.Replace(asciiTabPath, tab.TabPath.Replace("//", "/"));
                            this.AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, string.Empty, isRedirected ? 301 : 200, isSystem, friendlyUrlSettings, null);
                        }
                    }
                    else
                    {
                        // Add url with space
                        if (tab.TabName.Contains(" ") && friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                        {
                            path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, string.Empty);
                            if (customPath != null && !string.Equals(customPath, path, StringComparison.OrdinalIgnoreCase))
                            {
                                this.AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, string.Empty, isRedirected ? 301 : 200, isSystem, friendlyUrlSettings, null);
                            }
                        }
                    }
                }
            }

            foreach (var url in tab.TabUrls.Where(u => u.IsSystem == isSystem).OrderBy(u => u.SeqNum))
            {
                if (!int.TryParse(url.HttpStatus, out var statusCode))
                {
                    statusCode = 0;
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): statusCode → {StatusCode}", tab, portalId, locales, sortColumn, sortOrder, isSystem, statusCode);
                }

                // 27133 : Only show a custom URL
                if (url.PortalAliasUsage == PortalAliasUsageType.Default)
                {
                    var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): aliases → {Aliases}", tab, portalId, locales, sortColumn, sortOrder, isSystem, aliases);
                    var alias = aliases.FirstOrDefault(p => p.CultureCode == url.CultureCode);
					eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): alias → {Alias}", tab, portalId, locales, sortColumn, sortOrder, isSystem, alias);
                    if (alias == null)
                    {
                        // check primary when no culture code is defined
                        alias = aliases.FirstOrDefault(primary => primary.IsPrimary == true);
                        if (alias == null)
                        {
                            // if no primary alias just get first in list, need to use something
                            alias = aliases.FirstOrDefault(a => a.PortalID == portalId);
                        }
                    }

                    if (alias != null)
                    {
                        var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == alias.CultureCode);
                        this.AddUrlToList(tabs, portalId, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings, url.LastModifiedByUserId);
                    }
                }
                else
                {
                    var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == url.CultureCode);
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId)
                        .SingleOrDefault(p => p.PortalAliasID == url.PortalAliasId);
                    if (alias != null)
                    {
                        this.AddUrlToList(tabs, portalId, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings, url.LastModifiedByUserId, url.PortalAliasUsage);
                    }
                }
            }

            var pairComparer = new KeyValuePairComparer();
			eventLogger.LogDebug("GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem}): pairComparer → {PairComparer}", tab, portalId, locales, sortColumn, sortOrder, isSystem, pairComparer);
            switch ((SortingFields)sortColumn)
            {
                case SortingFields.Url:
                case SortingFields.None:
                    return sortOrder ?
                        tabs.OrderBy(url => url.SiteAlias, pairComparer).ThenBy(url => url.Path) :
                        tabs.OrderByDescending(url => url.SiteAlias, pairComparer).ThenByDescending(url => url.Path);
                case SortingFields.Locale:
                    return sortOrder ?
                        tabs.OrderBy(url => url.Locale, pairComparer) :
                        tabs.OrderByDescending(url => url.Locale, pairComparer);
                case SortingFields.Status:
                    return sortOrder ?
                        tabs.OrderBy(url => url.StatusCode, pairComparer) :
                        tabs.OrderByDescending(url => url.StatusCode, pairComparer);
                default:
					eventLogger.LogDebug("<<< Exiting GetSortedUrls(tab={Tab},portalId={PortalId},locales={Locales},sortColumn={SortColumn},sortOrder={SortOrder},isSystem={IsSystem})", tab, portalId, locales, sortColumn, sortOrder, isSystem);
                    return sortOrder ?
                        tabs.OrderBy(url => url.SiteAlias, pairComparer).ThenBy(url => url.Path) :
                        tabs.OrderByDescending(url => url.SiteAlias, pairComparer).ThenByDescending(url => url.Path);
            }
        }

        private void AddUrlToList(List<Url> tabs, int portalId, int id, PortalAliasInfo alias, Locale urlLocale, string path, string queryString, int statusCode, bool isSystem, FriendlyUrlSettings friendlyUrlSettings, int? lastModifiedByUserId, PortalAliasUsageType? portalAliasUsage = null)
        {
			eventLogger.LogDebug(">>> Entering AddUrlToList(tabs={Tabs},portalId={PortalId},id={Id},alias={Alias},urlLocale={UrlLocale},path={Path},queryString={QueryString},statusCode={StatusCode},isSystem={IsSystem},friendlyUrlSettings={FriendlyUrlSettings},lastModifiedByUserId={LastModifiedByUserId},portalAliasUsage={PortalAliasUsage})", tabs, portalId, id, alias, urlLocale, path, queryString, statusCode, isSystem, friendlyUrlSettings, lastModifiedByUserId, portalAliasUsage);
            var userName = string.Empty;
            if (lastModifiedByUserId.HasValue)
            {
                userName = UserController.Instance.GetUser(portalId, lastModifiedByUserId.Value)?.DisplayName;
            }

            tabs.Add(new Url
            {
                Id = id,
                SiteAlias = new KeyValuePair<int, string>(alias.KeyID, alias.HTTPAlias),
                Path = path,
                PathWithNoExtension = GetCleanPath(path, friendlyUrlSettings),
                QueryString = queryString,
                Locale = (urlLocale != null) ? new KeyValuePair<int, string>(urlLocale.KeyID, urlLocale.EnglishName)
                                             : new KeyValuePair<int, string>(-1, string.Empty),
                StatusCode = this.StatusCodes.SingleOrDefault(kv => kv.Key == statusCode),
                SiteAliasUsage = portalAliasUsage != null ? (int)portalAliasUsage : (int)PortalAliasUsageType.ChildPagesInherit,
                IsSystem = isSystem,
                UserName = userName,
            });
			eventLogger.LogDebug("<<< Exiting AddUrlToList(tabs={Tabs},portalId={PortalId},id={Id},alias={Alias},urlLocale={UrlLocale},path={Path},queryString={QueryString},statusCode={StatusCode},isSystem={IsSystem},friendlyUrlSettings={FriendlyUrlSettings},lastModifiedByUserId={LastModifiedByUserId},portalAliasUsage={PortalAliasUsage})", tabs, portalId, id, alias, urlLocale, path, queryString, statusCode, isSystem, friendlyUrlSettings, lastModifiedByUserId, portalAliasUsage);
        }

        public class KeyValuePairComparer : IComparer<KeyValuePair<int, string>>
        {
            /// <inheritdoc />
            public int Compare(KeyValuePair<int, string> pair1, KeyValuePair<int, string> pair2)
            {
				eventLogger.LogDebug(">>> Entering Compare(pair1={Pair1},pair2={Pair2})", pair1, pair2);
				eventLogger.LogDebug("<<< Exiting Compare(pair1={Pair1},pair2={Pair2})", pair1, pair2);
                return string.Compare(pair1.Value, pair2.Value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
