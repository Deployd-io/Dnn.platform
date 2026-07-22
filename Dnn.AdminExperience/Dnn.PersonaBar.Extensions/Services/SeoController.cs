// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Seo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Seo.Components;
    using Dnn.PersonaBar.Seo.Services.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Sitemap;
    using DotNetNuke.Services.Url.FriendlyUrl;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = "Dnn.Seo")]
    public class SeoController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SeoController));
        private static readonly string LocalResourcesFile = Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Seo/App_LocalResources/Seo.resx");

        private readonly Components.SeoController controller;

        public SeoController(INavigationManager navigationManager, IApplicationStatusInfo applicationStatusInfo)
        {
			Logger.LogDebug(">>> Entering SeoController(navigationManager={NavigationManager},applicationStatusInfo={ApplicationStatusInfo})", navigationManager, applicationStatusInfo);
            this.NavigationManager = navigationManager;
            this.controller = new Components.SeoController(applicationStatusInfo);
			Logger.LogDebug("<<< Exiting SeoController(navigationManager={NavigationManager},applicationStatusInfo={ApplicationStatusInfo})", navigationManager, applicationStatusInfo);
			Logger.LogDebug("SeoController(navigationManager={NavigationManager},applicationStatusInfo={ApplicationStatusInfo}): this.NavigationManager → {Arg2}", navigationManager, applicationStatusInfo, this.NavigationManager);
			Logger.LogDebug("SeoController(navigationManager={NavigationManager},applicationStatusInfo={ApplicationStatusInfo}): controller → {Controller}", navigationManager, applicationStatusInfo, controller);
        }

        protected INavigationManager NavigationManager { get; }

        /// GET: api/SEO/GetGeneralSettings
        /// <summary>Gets general SEO settings.</summary>
        /// <returns>General SEO settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetGeneralSettings()
        {
			Logger.LogDebug(">>> Entering GetGeneralSettings()");
            try
            {
                var urlSettings = new FriendlyUrlSettings(this.PortalId);

				Logger.LogDebug("GetGeneralSettings(): urlSettings → {UrlSettings}", urlSettings);
                var replacementCharacterList = new List<KeyValuePair<string, string>>();
                replacementCharacterList.Add(new KeyValuePair<string, string>(Localization.GetString("minusCharacter", LocalResourcesFile), "-"));
				Logger.LogDebug("GetGeneralSettings(): replacementCharacterList → {ReplacementCharacterList}", replacementCharacterList);
                replacementCharacterList.Add(new KeyValuePair<string, string>(Localization.GetString("underscoreCharacter", LocalResourcesFile), "_"));

                var deletedPageHandlingTypes = new List<KeyValuePair<string, string>>();
                deletedPageHandlingTypes.Add(new KeyValuePair<string, string>(Localization.GetString("Do404Error", LocalResourcesFile), "Do404Error"));
				Logger.LogDebug("GetGeneralSettings(): deletedPageHandlingTypes → {DeletedPageHandlingTypes}", deletedPageHandlingTypes);
                deletedPageHandlingTypes.Add(new KeyValuePair<string, string>(Localization.GetString("Do301RedirectToPortalHome", LocalResourcesFile), "Do301RedirectToPortalHome"));

                var response = new
                {
                    Success = true,
                    Settings = new
					Logger.LogDebug("GetGeneralSettings(): Success → {Success}", Success);
					Logger.LogDebug("GetGeneralSettings(): Settings → {Settings}", Settings);
                    {
                        EnableSystemGeneratedUrls = urlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing,
                        urlSettings.ReplaceSpaceWith,
                        urlSettings.ForceLowerCase,
                        urlSettings.AutoAsciiConvert,
                        urlSettings.ForcePortalDefaultLanguage,
                        DeletedTabHandlingType = urlSettings.DeletedTabHandlingType.ToString(),
                        urlSettings.RedirectUnfriendly,
                        urlSettings.RedirectWrongCase,
                    },
                    ReplacementCharacterList = replacementCharacterList,
                    DeletedPageHandlingTypes = deletedPageHandlingTypes,
                    AdminRoleId = this.PortalSettings.AdministratorRoleId,
                };
						Logger.LogDebug("GetGeneralSettings(): DeletedTabHandlingType → {DeletedTabHandlingType}", DeletedTabHandlingType);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetGeneralSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateGeneralSettings
        /// <summary>Updates SEO general settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateGeneralSettings(UpdateGeneralSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateGeneralSettings(request={Request})", request);
            try
            {
                string characterSub = FriendlyUrlSettings.ReplaceSpaceWithNothing;
                if (request.EnableSystemGeneratedUrls)
				Logger.LogDebug("UpdateGeneralSettings(request={Request}): characterSub → {CharacterSub}", request, characterSub);
                {
                    characterSub = request.ReplaceSpaceWith;
                }

                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.ReplaceSpaceWithSetting, characterSub, false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.DeletedTabHandlingTypeSetting, request.DeletedTabHandlingType, false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.ForceLowerCaseSetting, request.ForceLowerCase ? "Y" : "N", false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.RedirectUnfriendlySetting, request.RedirectUnfriendly ? "Y" : "N", false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.RedirectMixedCaseSetting, request.RedirectWrongCase.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.UsePortalDefaultLanguageSetting, request.ForcePortalDefaultLanguage.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.AutoAsciiConvertSetting, request.AutoAsciiConvert.ToString(), false);

                DataCache.ClearPortalCache(this.PortalId, false);
                DataCache.ClearTabsCache(this.PortalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateGeneralSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetRegexSettings
        /// <summary>Gets SEO regex settings.</summary>
        /// <returns>General SEO regex settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetRegexSettings()
        {
			Logger.LogDebug(">>> Entering GetRegexSettings()");
            try
            {
                var urlSettings = new FriendlyUrlSettings(this.PortalId);

				Logger.LogDebug("GetRegexSettings(): urlSettings → {UrlSettings}", urlSettings);
                var response = new
                {
                    Success = true,
                    Settings = new
					Logger.LogDebug("GetRegexSettings(): Success → {Success}", Success);
					Logger.LogDebug("GetRegexSettings(): Settings → {Settings}", Settings);
                    {
                        urlSettings.IgnoreRegex,
                        urlSettings.DoNotRewriteRegex,
                        urlSettings.UseSiteUrlsRegex,
                        urlSettings.DoNotRedirectRegex,
                        urlSettings.DoNotRedirectSecureRegex,
                        urlSettings.ForceLowerCaseRegex,
                        urlSettings.NoFriendlyUrlRegex,
                        urlSettings.DoNotIncludeInPathRegex,
                        urlSettings.ValidExtensionlessUrlsRegex,
                        urlSettings.RegexMatch,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetRegexSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateRegexSettings
        /// <summary>Updates SEO regex settings.</summary>
        /// <param name="request">The request body.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateRegexSettings(UpdateRegexSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateRegexSettings(request={Request})", request);
            try
            {
                List<KeyValuePair<string, string>> validationErrors = new List<KeyValuePair<string, string>>();
                if (!ValidateRegex(request.IgnoreRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("IgnoreRegex", Localization.GetString("ignoreRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.DoNotRewriteRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotRewriteRegex", Localization.GetString("doNotRewriteRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.UseSiteUrlsRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("UseSiteUrlsRegex", Localization.GetString("siteUrlsOnlyRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.DoNotRedirectRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotRedirectRegex", Localization.GetString("doNotRedirectUrlRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.DoNotRedirectSecureRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotRedirectSecureRegex", Localization.GetString("doNotRedirectHttpsUrlRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.ForceLowerCaseRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("ForceLowerCaseRegex", Localization.GetString("preventLowerCaseUrlRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.NoFriendlyUrlRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("NoFriendlyUrlRegex", Localization.GetString("doNotUseFriendlyUrlsRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.DoNotIncludeInPathRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotIncludeInPathRegex", Localization.GetString("keepInQueryStringRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.ValidExtensionlessUrlsRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("ValidExtensionlessUrlsRegex", Localization.GetString("urlsWithNoExtensionRegExInvalidPattern", LocalResourcesFile)));
                }

                if (!ValidateRegex(request.RegexMatch))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("RegexMatch", Localization.GetString("validFriendlyUrlRegExInvalidPattern", LocalResourcesFile)));
                }

                if (validationErrors.Count > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Errors = validationErrors });
                }
                else
                {
                    // if no errors, update settings in db
                    this.UpdateRegexSettingsInternal(request);

                    // clear cache
                    this.ClearCache();

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateRegexSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetSitemapSettings
        /// <summary>Gets sitemap settings.</summary>
        /// <returns>Data of sitemap settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetSitemapSettings()
        {
			Logger.LogDebug(">>> Entering GetSitemapSettings()");
            try
            {
                var portalAlias = !string.IsNullOrEmpty(this.PortalSettings.DefaultPortalAlias)
                                ? this.PortalSettings.DefaultPortalAlias
                                : this.PortalSettings.PortalAlias.HTTPAlias;

                var str = PortalController.GetPortalSetting("SitemapMinPriority", this.PortalId, "0.1");
                float sitemapMinPriority;
				Logger.LogDebug("GetSitemapSettings(): str → {Str}", str);
                if (!float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out sitemapMinPriority))
                {
                    sitemapMinPriority = 0.1f;
					Logger.LogDebug("GetSitemapSettings(): sitemapMinPriority → {SitemapMinPriority}", sitemapMinPriority);
                }

                str = PortalController.GetPortalSetting("SitemapExcludePriority", this.PortalId, "0.1");
                float sitemapExcludePriority;
                if (!float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out sitemapExcludePriority))
                {
                    sitemapExcludePriority = 0.1f;
					Logger.LogDebug("GetSitemapSettings(): sitemapExcludePriority → {SitemapExcludePriority}", sitemapExcludePriority);
                }

                var settings = new
                {
                    SitemapUrl = Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx",
                    SitemapLevelMode = PortalController.GetPortalSettingAsBoolean("SitemapLevelMode", this.PortalId, false),
                    SitemapMinPriority = sitemapMinPriority,
                    SitemapIncludeHidden = PortalController.GetPortalSettingAsBoolean("SitemapIncludeHidden", this.PortalId, false),
                    SitemapExcludePriority = sitemapExcludePriority,
                    SitemapCacheDays = PortalController.GetPortalSettingAsInteger("SitemapCacheDays", this.PortalId, 1),
                };
					Logger.LogDebug("GetSitemapSettings(): SitemapUrl → {SitemapUrl}", SitemapUrl);
					Logger.LogDebug("GetSitemapSettings(): SitemapLevelMode → {SitemapLevelMode}", SitemapLevelMode);
					Logger.LogDebug("GetSitemapSettings(): SitemapMinPriority → {SitemapMinPriority}", SitemapMinPriority);
					Logger.LogDebug("GetSitemapSettings(): SitemapIncludeHidden → {SitemapIncludeHidden}", SitemapIncludeHidden);
					Logger.LogDebug("GetSitemapSettings(): SitemapExcludePriority → {SitemapExcludePriority}", SitemapExcludePriority);
					Logger.LogDebug("GetSitemapSettings(): SitemapCacheDays → {SitemapCacheDays}", SitemapCacheDays);

                var searchEngineUrls = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Google", this.controller.GetSearchEngineSubmissionUrl("google")),
                    new KeyValuePair<string, string>("Bing", this.controller.GetSearchEngineSubmissionUrl("bing")),
                    new KeyValuePair<string, string>("Yahoo!", this.controller.GetSearchEngineSubmissionUrl("yahoo!")),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Settings = settings,
                    SearchEngineUrls = searchEngineUrls,
                });
					Logger.LogDebug("GetSitemapSettings(): Success → {Success}", Success);
					Logger.LogDebug("GetSitemapSettings(): Settings → {Settings}", Settings);
					Logger.LogDebug("GetSitemapSettings(): SearchEngineUrls → {SearchEngineUrls}", SearchEngineUrls);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSitemapSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/CreateVerification
        /// <summary>Creates a verification file for specific search engine.</summary>
        /// <param name="verification">Name of verification.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateVerification(string verification)
        {
			Logger.LogDebug(">>> Entering CreateVerification(verification={Verification})", verification);
            try
            {
                this.controller.CreateVerification(verification);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting CreateVerification(verification={Verification})", verification);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateSitemapSettings
        /// <summary>Updates sitemap settings.</summary>
        /// <param name="request">Data of sitemap settings.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSitemapSettings(SitemapSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateSitemapSettings(request={Request})", request);
            try
            {
                PortalController.UpdatePortalSetting(this.PortalId, "SitemapLevelMode", request.SitemapLevelMode.ToString());

                if (request.SitemapMinPriority < 0)
                {
                    request.SitemapMinPriority = 0;
					Logger.LogDebug("UpdateSitemapSettings(request={Request}): request.SitemapMinPriority → {Arg1}", request, request.SitemapMinPriority);
                }

                PortalController.UpdatePortalSetting(this.PortalId, "SitemapMinPriority", request.SitemapMinPriority.ToString(NumberFormatInfo.InvariantInfo));

                PortalController.UpdatePortalSetting(this.PortalId, "SitemapIncludeHidden", request.SitemapIncludeHidden.ToString());

                if (request.SitemapExcludePriority < 0)
                {
                    request.SitemapExcludePriority = 0;
					Logger.LogDebug("UpdateSitemapSettings(request={Request}): request.SitemapExcludePriority → {Arg1}", request, request.SitemapExcludePriority);
                }

                PortalController.UpdatePortalSetting(this.PortalId, "SitemapExcludePriority", request.SitemapExcludePriority.ToString(NumberFormatInfo.InvariantInfo));

                if (request.SitemapCacheDays == 0)
                {
                    this.controller.ResetCache();
                }

                PortalController.UpdatePortalSetting(this.PortalId, "SitemapCacheDays", request.SitemapCacheDays.ToString(CultureInfo.InvariantCulture));
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateSitemapSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/ResetCache
        /// <summary>Resets cache.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ResetCache()
        {
			Logger.LogDebug(">>> Entering ResetCache()");
            try
            {
                this.controller.ResetCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting ResetCache()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetSitemapProviders
        /// <summary>Gets list of sitemap providers.</summary>
        /// <returns>Web Server information.</returns>
        [HttpGet]
        public HttpResponseMessage GetSitemapProviders()
        {
			Logger.LogDebug(">>> Entering GetSitemapProviders()");
            try
            {
                var providers = this.controller.GetSitemapProviders().Select(p => new
                {
                    p.Name,
                    p.Enabled,
                    Priority = p.Priority / 100f,
                    p.OverridePriority,
                }).ToList();
					Logger.LogDebug("GetSitemapProviders(): Priority → {Priority}", Priority);

                var response = new
                {
                    Success = true,
                    Providers = providers,
                };
					Logger.LogDebug("GetSitemapProviders(): Success → {Success}", Success);
					Logger.LogDebug("GetSitemapProviders(): Providers → {Providers}", Providers);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSitemapProviders()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateSitemapProvider
        /// <summary>Updates settings of a sitemap provider.</summary>
        /// <param name="request">Data of sitemap provider.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSitemapProvider(UpdateSitemapProviderRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateSitemapProvider(request={Request})", request);
            try
            {
                SitemapProvider editedProvider =
                    this.controller.GetSitemapProviders()
                        .FirstOrDefault(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

                if (editedProvider != null)
                {
                    editedProvider.Enabled = request.Enabled;
					Logger.LogDebug("UpdateSitemapProvider(request={Request}): editedProvider.Enabled → {Arg1}", request, editedProvider.Enabled);
					Logger.LogDebug("UpdateSitemapProvider(request={Request}): editedProvider.OverridePriority → {Arg1}", request, editedProvider.OverridePriority);
                    editedProvider.OverridePriority = request.Priority > -1;
                    if (editedProvider.OverridePriority)
                    {
                        editedProvider.Priority = request.Priority * 100;
						Logger.LogDebug("UpdateSitemapProvider(request={Request}): editedProvider.Priority → {Arg1}", request, editedProvider.Priority);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateSitemapProvider(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetExtensionUrlProviders
        /// <summary>Gets list of extension url providers.</summary>
        /// <returns>extension url providers.</returns>
        [HttpGet]
        public HttpResponseMessage GetExtensionUrlProviders()
        {
			Logger.LogDebug(">>> Entering GetExtensionUrlProviders()");
            try
            {
                var providers = ExtensionUrlProviderController.GetProviders(this.PortalId).Select(p => new
                {
                    p.ExtensionUrlProviderId,
                    p.ProviderName,
                    p.IsActive,
                    SettingUrl = this.NavigationManager.NavigateURL(this.PortalSettings.AdminTabId, "UrlProviderSettings", "Display=settings&popUp=true&ProviderId=" + p.ExtensionUrlProviderId),
                }).ToList();

                var response = new
                {
                    Success = true,
                    Providers = providers,
                };
					Logger.LogDebug("GetExtensionUrlProviders(): Success → {Success}", Success);
					Logger.LogDebug("GetExtensionUrlProviders(): Providers → {Providers}", Providers);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetExtensionUrlProviders()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateExtensionUrlProviderStatus
        /// <summary>Enable or disable extension url provider.</summary>
        /// <param name="request">Data of extension url provider.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateExtensionUrlProviderStatus(UpdateExtensionUrlProviderStatusRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateExtensionUrlProviderStatus(request={Request})", request);
            try
            {
                if (request.IsActive)
                {
                    ExtensionUrlProviderController.EnableProvider(request.ProviderId, this.PortalId);
                }
                else
                {
                    ExtensionUrlProviderController.DisableProvider(request.ProviderId, this.PortalId);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateExtensionUrlProviderStatus(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>Tests the internal URL.</summary>
        /// <param name="pageId">The tab ID.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="customPageName">The custom page name.</param>
        /// <returns>Various forms of the URL and any messages when they exist.</returns>
        /// <example>
        /// <code>
        /// GET <c>/API/PersonaBar/SEO/TestUrl?pageId=53&amp;queryString=ab%3Dcd&amp;customPageName=test-page</c>
        /// </code>
        /// </example>
        [HttpGet]
        public HttpResponseMessage TestUrl(int pageId, string queryString, string customPageName)
        {
			Logger.LogDebug(">>> Entering TestUrl(pageId={PageId},queryString={QueryString},customPageName={CustomPageName})", pageId, queryString, customPageName);
            try
            {
                var response = new
                {
                    Success = true,
                    Urls = this.TestUrlInternal(pageId, queryString, customPageName),
                };
					Logger.LogDebug("TestUrl(pageId={PageId},queryString={QueryString},customPageName={CustomPageName}): Success → {Success}", pageId, queryString, customPageName, Success);
					Logger.LogDebug("TestUrl(pageId={PageId},queryString={QueryString},customPageName={CustomPageName}): Urls → {Urls}", pageId, queryString, customPageName, Urls);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting TestUrl(pageId={PageId},queryString={QueryString},customPageName={CustomPageName})", pageId, queryString, customPageName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/TestUrlRewrite
        /// <summary>Tests the rewritten URL.</summary>
        /// <param name="uri">The URL to rewrite.</param>
        /// <returns>Rewritten URL and few other information about the URL ( language, redirection result and reason, messages).</returns>
        /// <example>
        /// <code>
        /// GET <c>/API/PersonaBar/SEO/TestUrlRewrite?uri=http%3A%2F%2Fmysite.com%2Ftest-page</c>
        /// </code>
        /// </example>
        [HttpGet]
        public HttpResponseMessage TestUrlRewrite(string uri)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    RewritingResult = this.TestUrlRewritingInternal(uri),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static bool ValidateRegex(string regexPattern)
        {
			Logger.LogDebug(">>> Entering ValidateRegex(regexPattern={RegexPattern})", regexPattern);
            try
            {
                _ = RegexUtils.GetCachedRegex(regexPattern).IsMatch(string.Empty);
                return true;
            }
            catch
            {
				Logger.LogError("Exception in ValidateRegex(regexPattern={RegexPattern}): {Arg1}", regexPattern, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting ValidateRegex(regexPattern={RegexPattern})", regexPattern);
            return false;
        }

        private void UpdateRegexSettingsInternal(UpdateRegexSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateRegexSettingsInternal(request={Request})", request);
            var settings = new Dictionary<string, string>
            {
                { FriendlyUrlSettings.IgnoreRegexSetting, request.IgnoreRegex },
                { FriendlyUrlSettings.DoNotRewriteRegExSetting, request.DoNotRewriteRegex },
                { FriendlyUrlSettings.SiteUrlsOnlyRegexSetting, request.UseSiteUrlsRegex },
                { FriendlyUrlSettings.DoNotRedirectUrlRegexSetting, request.DoNotRedirectRegex },
                { FriendlyUrlSettings.DoNotRedirectHttpsUrlRegexSetting, request.DoNotRedirectSecureRegex },
                { FriendlyUrlSettings.PreventLowerCaseUrlRegexSetting, request.ForceLowerCaseRegex },
                { FriendlyUrlSettings.DoNotUseFriendlyUrlRegexSetting, request.NoFriendlyUrlRegex },
                { FriendlyUrlSettings.KeepInQueryStringRegexSetting, request.DoNotIncludeInPathRegex },
                { FriendlyUrlSettings.UrlsWithNoExtensionRegexSetting, request.ValidExtensionlessUrlsRegex },
                { FriendlyUrlSettings.ValidFriendlyUrlRegexSetting, request.RegexMatch },
            };

            settings.ToList().ForEach((value) =>
            {
                if (this.PortalId == Null.NullInteger)
                {
                    HostController.Instance.Update(value.Key, value.Value, false);
                }
                else
                {
                    PortalController.Instance.UpdatePortalSetting(this.PortalId, value.Key, value.Value, false, Null.NullString, false);
                }
            });
			Logger.LogDebug("<<< Exiting UpdateRegexSettingsInternal(request={Request})", request);
        }

        private void ClearCache()
        {
			Logger.LogDebug(">>> Entering ClearCache()");
            if (this.PortalId == Null.NullInteger)
            {
                DataCache.ClearHostCache(false);
            }
            else
            {
                DataCache.ClearPortalCache(this.PortalId, false);
            }

            CacheController.FlushPageIndexFromCache();
            CacheController.FlushFriendlyUrlSettingsFromCache();
			Logger.LogDebug("<<< Exiting ClearCache()");
        }

        private IEnumerable<string> TestUrlInternal(int pageId, string queryString, string customPageName)
        {
			Logger.LogDebug(">>> Entering TestUrlInternal(pageId={PageId},queryString={QueryString},customPageName={CustomPageName})", pageId, queryString, customPageName);
            var provider = new DNNFriendlyUrlProvider();
            var tab = TabController.Instance.GetTab(pageId, this.PortalId, false);
			Logger.LogDebug("TestUrlInternal(pageId={PageId},queryString={QueryString},customPageName={CustomPageName}): provider → {Provider}", pageId, queryString, customPageName, provider);
			Logger.LogDebug("TestUrlInternal(pageId={PageId},queryString={QueryString},customPageName={CustomPageName}): tab → {Tab}", pageId, queryString, customPageName, tab);
            var pageName = string.IsNullOrEmpty(customPageName) ? Globals.glbDefaultPage : customPageName;
			Logger.LogDebug("TestUrlInternal(pageId={PageId},queryString={QueryString},customPageName={CustomPageName}): pageName → {PageName}", pageId, queryString, customPageName, pageName);
			Logger.LogDebug("<<< Exiting TestUrlInternal(pageId={PageId},queryString={QueryString},customPageName={CustomPageName})", pageId, queryString, customPageName);
            return PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId).
                Select(alias => provider.FriendlyUrl(
                    tab, "~/Default.aspx?tabId=" + pageId + "&" + queryString, pageName, alias.HTTPAlias));
        }

        private UrlRewritingResult TestUrlRewritingInternal(string uriString)
        {
			Logger.LogDebug(">>> Entering TestUrlRewritingInternal(uriString={UriString})", uriString);
            var rewritingResult = new UrlRewritingResult();
            try
			Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): rewritingResult → {RewritingResult}", uriString, rewritingResult);
            {
                var noneText = Localization.GetString("None", Localization.GlobalResourceFile);
                var uri = new Uri(uriString);
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): noneText → {NoneText}", uriString, noneText);
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): uri → {Uri}", uriString, uri);
                var provider = new AdvancedUrlRewriter();
                var result = new UrlAction(uri.Scheme, uriString, Globals.ApplicationMapPath)
                {
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): provider → {Provider}", uriString, provider);
                    RawUrl = uriString,
                };
                var httpContext = new HttpContext(HttpContext.Current.Request, new HttpResponse(new StringWriter()));
                provider.ProcessTestRequestWithContext(httpContext, uri, true, result, new FriendlyUrlSettings(this.PortalId));
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): httpContext → {HttpContext}", uriString, httpContext);
                rewritingResult.RewritingResult = string.IsNullOrEmpty(result.RewritePath) ? noneText : result.RewritePath;
                rewritingResult.Culture = string.IsNullOrEmpty(result.CultureCode) ? noneText : result.CultureCode;
                var tab = TabController.Instance.GetTab(result.TabId, result.PortalId, false);
                rewritingResult.IdentifiedPage = tab != null ? tab.TabName : noneText;
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): tab → {Tab}", uriString, tab);
                rewritingResult.RedirectionReason = Localization.GetString(result.Reason.ToString());
                rewritingResult.RedirectionResult = result.FinalUrl;
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): rewritingResult.RedirectionResult → {Arg1}", uriString, rewritingResult.RedirectionResult);
                var messages = new StringBuilder();
                foreach (var message in result.DebugMessages)
                {
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): messages → {Messages}", uriString, messages);
                    messages.AppendLine(message);
                }

                rewritingResult.OperationMessages = messages.ToString();
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TestUrlRewritingInternal(uriString={UriString}): {Arg1}", uriString, ex.Message);
				Logger.LogDebug("TestUrlRewritingInternal(uriString={UriString}): rewritingResult.OperationMessages → {Arg1}", uriString, rewritingResult.OperationMessages);
                rewritingResult.OperationMessages = ex.Message;
            }

			Logger.LogDebug("<<< Exiting TestUrlRewritingInternal(uriString={UriString})", uriString);
            return rewritingResult;
        }
    }
}
