// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>LocaleController provides method to manage all pages with localization content.</summary>
    /// <remarks>
    /// Content localization in DotNetNuke will allow you to easily manage your web pages in a primary language
    /// and then utilize translators to keep the content synchronized in multiple secondary languages.
    /// Whether you are maintaining your site in a single language or dozens of languages, the content localization system
    /// will help guide your content editors and translators through the process.  Although content localization required
    /// extensive changes to the core platform, we have been able to add this new feature while still improving overall system performance.
    /// </remarks>
    public class LocaleController : ComponentBase<ILocaleController, LocaleController>, ILocaleController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocaleController));
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly DataProvider dataProvider;
        private readonly IPortalController portalController;
        private readonly ITabController tabController;
        private readonly IUserController userController;
        private readonly IPackageController packageController;

        /// <summary>Initializes a new instance of the <see cref="LocaleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public LocaleController()
            : this(null, null, null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering LocaleController()");
        }

		Logger.LogDebug("<<< Exiting LocaleController()");
        /// <summary>Initializes a new instance of the <see cref="LocaleController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="tabController">The tab controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="packageController">The package controller.</param>
        public LocaleController(IHostSettings hostSettings, IApplicationStatusInfo appStatus, DataProvider dataProvider, IPortalController portalController, ITabController tabController, IUserController userController, IPackageController packageController)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("LocaleController(hostSettings={HostSettings},appStatus={AppStatus},dataProvider={DataProvider},portalController={PortalController},tabController={TabController},userController={UserController},packageController={PackageController}): appStatus → {AppStatus1}", hostSettings, appStatus, dataProvider, portalController, tabController, userController, packageController, appStatus);
			Logger.LogDebug("LocaleController(hostSettings={HostSettings},appStatus={AppStatus},dataProvider={DataProvider},portalController={PortalController},tabController={TabController},userController={UserController},packageController={PackageController}): portalController → {PortalController1}", hostSettings, appStatus, dataProvider, portalController, tabController, userController, packageController, portalController);
			Logger.LogDebug("LocaleController(hostSettings={HostSettings},appStatus={AppStatus},dataProvider={DataProvider},portalController={PortalController},tabController={TabController},userController={UserController},packageController={PackageController}): tabController → {TabController1}", hostSettings, appStatus, dataProvider, portalController, tabController, userController, packageController, tabController);
			Logger.LogDebug("LocaleController(hostSettings={HostSettings},appStatus={AppStatus},dataProvider={DataProvider},portalController={PortalController},tabController={TabController},userController={UserController},packageController={PackageController}): userController → {UserController1}", hostSettings, appStatus, dataProvider, portalController, tabController, userController, packageController, userController);
			Logger.LogDebug("LocaleController(hostSettings={HostSettings},appStatus={AppStatus},dataProvider={DataProvider},portalController={PortalController},tabController={TabController},userController={UserController},packageController={PackageController}): packageController → {PackageController1}", hostSettings, appStatus, dataProvider, portalController, tabController, userController, packageController, packageController);
            this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
			Logger.LogDebug("LocaleController(hostSettings={HostSettings},appStatus={AppStatus},dataProvider={DataProvider},portalController={PortalController},tabController={TabController},userController={UserController},packageController={PackageController}): dataProvider → {DataProvider1}", hostSettings, appStatus, dataProvider, portalController, tabController, userController, packageController, dataProvider);
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
            this.userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
            this.packageController = packageController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPackageController>();
        }

        public static bool IsValidCultureName(string name)
        {
			Logger.LogDebug(">>> Entering IsValidCultureName(name={Name})", name);
				Logger.LogDebug("<<< Exiting IsValidCultureName(name={Name})", name);
            return
                CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Any(c => c.Name == name);
        }

        /// <summary>Determines whether the language can be deleted.</summary>
        /// <param name="languageId">The language ID.</param>
        /// <returns><see langword="true"/> if the language can be deleted; otherwise, <see langword="false"/>.</returns>
        public bool CanDeleteLanguage(int languageId)
        {
			Logger.LogDebug(">>> Entering CanDeleteLanguage(languageId={LanguageId})", languageId);
			Logger.LogDebug("<<< Exiting CanDeleteLanguage(languageId={LanguageId})", languageId);
            return this.packageController.GetExtensionPackages(Null.NullInteger, p => p.PackageType.Equals("CoreLanguagePack", StringComparison.OrdinalIgnoreCase))
                        .Select(package => LanguagePackController.GetLanguagePackByPackage(package.PackageID))
                        .All(languagePack => languagePack.LanguageID != languageId);
        }

        /// <summary>Gets the cultures from local list.</summary>
        /// <param name="locales">The locales.</param>
        /// <returns>culture list.</returns>
        public List<CultureInfo> GetCultures(Dictionary<string, Locale> locales)
        {
			Logger.LogDebug(">>> Entering GetCultures(locales={Locales})", locales);
			Logger.LogDebug("<<< Exiting GetCultures(locales={Locales})", locales);
            return locales.Values.Select(locale => new CultureInfo(locale.Code)).ToList();
        }

        /// <summary>Gets the current locale for current request to the portal.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>A <see cref="Locale"/> instance.</returns>
        public Locale GetCurrentLocale(int portalId)
        {
			Logger.LogDebug(">>> Entering GetCurrentLocale(portalId={PortalId})", portalId);
            Locale locale = null;

			Logger.LogDebug("GetCurrentLocale(portalId={PortalId}): locale → {Locale}", portalId, locale);
            if (HttpContext.Current != null && !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["language"]))
            {
                locale = this.GetLocale(HttpContext.Current.Request.QueryString["language"]);
            }

			Logger.LogDebug("<<< Exiting GetCurrentLocale(portalId={PortalId})", portalId);
            return locale ?? ((portalId == Null.NullInteger)
                                ? this.GetLocale(Localization.SystemLocale)
                                : this.GetDefaultLocale(portalId));
        }

        /// <summary>Gets the default locale of the portal.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>A <see cref="Locale"/> instance.</returns>
        public Locale GetDefaultLocale(int portalId)
        {
			Logger.LogDebug(">>> Entering GetDefaultLocale(portalId={PortalId})", portalId);
            IPortalInfo portal = this.portalController.GetPortal(portalId);
            Locale locale = null;
			Logger.LogDebug("GetDefaultLocale(portalId={PortalId}): locale → {Locale}", portalId, locale);
			Logger.LogDebug("GetDefaultLocale(portalId={PortalId}): portal → {Portal}", portalId, portal);
            if (portal != null)
            {
                var locales = this.GetLocales(portal.PortalId);
				Logger.LogDebug("GetDefaultLocale(portalId={PortalId}): locales → {Locales}", portalId, locales);
                if (locales != null && locales.TryGetValue(portal.DefaultLanguage, out var locale1))
                {
                    locale = locale1;
                }
            }

			Logger.LogDebug("<<< Exiting GetDefaultLocale(portalId={PortalId})", portalId);
            return locale ?? this.GetLocale(Localization.SystemLocale);
        }

        /// <summary>Gets the locale by code.</summary>
        /// <param name="code">The code.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocale(string code)
        {
			Logger.LogDebug(">>> Entering GetLocale(code={Code})", code);
			Logger.LogDebug("<<< Exiting GetLocale(code={Code})", code);
			Logger.LogDebug("<<< Exiting GetLocale(portalID={PortalID},code={Code})", portalID, code);
			Logger.LogDebug("<<< Exiting GetLocale(languageID={LanguageID})", languageID);
            return this.GetLocale(Null.NullInteger, code);
        }

        /// <summary>Gets the locale included in the portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocale(int portalID, string code)
        {
            Dictionary<string, Locale> dicLocales = this.GetLocales(portalID);
            Locale locale = null;

            dicLocales?.TryGetValue(code, out locale);

            return locale;
        }

        /// <summary>Gets the locale included in the portal if culture code is not null or empty or else gets the current locale for current request to the portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="code">The code.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocaleOrCurrent(int portalID, string code)
        {
			Logger.LogDebug(">>> Entering GetLocaleOrCurrent(portalID={PortalID},code={Code})", portalID, code);
			Logger.LogDebug("<<< Exiting GetLocaleOrCurrent(portalID={PortalID},code={Code})", portalID, code);
            return string.IsNullOrEmpty(code)
                       ? this.GetCurrentLocale(portalID) : this.GetLocale(portalID, code);
        }

        /// <summary>Gets the locale.</summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns>A <see cref="Locale"/> instance or <see langword="null"/>.</returns>
        public Locale GetLocale(int languageID)
        {
            Dictionary<string, Locale> dicLocales = this.GetLocales(Null.NullInteger);

            return (from kvp in dicLocales where kvp.Value.LanguageId == languageID select kvp.Value).FirstOrDefault();
        }

        /// <summary>Gets the locales.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping from culture code to <see cref="Locale"/>, or <see langword="null"/>.</returns>
        public Dictionary<string, Locale> GetLocales(int portalID)
        {
            if (this.appStatus.Status == UpgradeStatus.Error)
            {
                return null;
            }

            if (this.appStatus.Status == UpgradeStatus.Install)
            {
                return CBO.FillDictionary(
                    "CultureCode",
                    this.dataProvider.GetLanguages(),
                    new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
            }

            var cacheArgs = new CacheItemArgs(
                string.Format(CultureInfo.InvariantCulture, DataCache.LocalesCacheKey, portalID),
                DataCache.LocalesCacheTimeOut,
                DataCache.LocalesCachePriority,
                portalID,
                this.dataProvider);
            return CBO.GetCachedObject<Dictionary<string, Locale>>(
                this.hostSettings,
                cacheArgs,
                GetLocalesCallBack,
                true);
        }

        /// <summary>Gets the published locales.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> mapping from culture code to <see cref="Locale"/>.</returns>
        public Dictionary<string, Locale> GetPublishedLocales(int portalID)
        {
            return this.GetLocales(portalID).Where(kvp => kvp.Value.IsPublished).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>Determines whether the specified locale code is enabled.</summary>
        /// <param name="localeCode">The locale code.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns><see langword="true"/> if the specified locale code is enabled; otherwise, <see langword="false"/>.</returns>
        public bool IsEnabled(ref string localeCode, int portalId)
        {
			Logger.LogDebug(">>> Entering IsEnabled(localeCode={LocaleCode},portalId={PortalId})", localeCode, portalId);
            try
            {
                bool enabled = false;
				Logger.LogDebug("IsEnabled(localeCode={LocaleCode},portalId={PortalId}): enabled → {Enabled}", localeCode, portalId, enabled);
                Dictionary<string, Locale> dicLocales = this.GetLocales(portalId);
				Logger.LogDebug("IsEnabled(localeCode={LocaleCode},portalId={PortalId}): dicLocales → {DicLocales}", localeCode, portalId, dicLocales);

                ////if ((!dicLocales.ContainsKey(localeCode)))
                string locale = localeCode;
                if (dicLocales.FirstOrDefault(x => string.Equals(x.Key, locale, StringComparison.OrdinalIgnoreCase)).Key == null)
                {
                    // if localecode is neutral (en, es,...) try to find a locale that has the same language
                    if (localeCode.IndexOf("-", StringComparison.Ordinal) == -1)
                    {
                        foreach (string strLocale in dicLocales.Keys)
                        {
                            if (string.Equals(strLocale.Split('-')[0], localeCode, StringComparison.OrdinalIgnoreCase))
                            {
                                // set the requested _localecode to the full locale
                                localeCode = strLocale;
                                enabled = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    enabled = true;
                }

                return enabled;
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in IsEnabled(localeCode={LocaleCode},portalId={PortalId}): {Arg2}", localeCode, portalId, ex.getMessage());
                // item could not be retrieved  or error
                Exceptions.Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting IsEnabled(localeCode={LocaleCode},portalId={PortalId})", localeCode, portalId);
                return false;
            }
        }

        /// <summary>Updates the portal locale.</summary>
        /// <param name="locale">The locale.</param>
        public void UpdatePortalLocale(Locale locale)
        {
			Logger.LogDebug(">>> Entering UpdatePortalLocale(locale={Locale})", locale);
            this.dataProvider.UpdatePortalLanguage(locale.PortalId, locale.LanguageId, locale.IsPublished, this.userController.GetCurrentUserInfo().UserID);
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.LocalesCacheKey, locale.PortalId));
			Logger.LogDebug("<<< Exiting UpdatePortalLocale(locale={Locale})", locale);
        }

        /// <summary>Determines the language whether is default language.</summary>
        /// <param name="code">The code.</param>
        /// <returns><see langword="true"/> if the language is default language; otherwise, <see langword="false"/>.</returns>
        public bool IsDefaultLanguage(string code)
        {
			Logger.LogDebug(">>> Entering IsDefaultLanguage(code={Code})", code);
			Logger.LogDebug("<<< Exiting IsDefaultLanguage(code={Code})", code);
            return code == this.portalController.GetCurrentSettings().DefaultLanguage;
        }

        /// <summary>Activates the language without publishing it.</summary>
        /// <param name="portalid">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="publish">if set to <see langword="true"/> will publish the language, otherwise will "un-publish".</param>
        public void ActivateLanguage(int portalid, string cultureCode, bool publish)
        {
			Logger.LogDebug(">>> Entering ActivateLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish})", portalid, cultureCode, publish);
            Dictionary<string, Locale> enabledLanguages = this.GetLocales(portalid);
            if (enabledLanguages.TryGetValue(cultureCode, out var enabledLanguage))
			Logger.LogDebug("ActivateLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish}): enabledLanguages → {EnabledLanguages}", portalid, cultureCode, publish, enabledLanguages);
            {
                enabledLanguage.IsPublished = publish;
				Logger.LogDebug("ActivateLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish}): enabledLanguage.IsPublished → {Arg3}", portalid, cultureCode, publish, enabledLanguage.IsPublished);
                this.UpdatePortalLocale(enabledLanguage);
            }
			Logger.LogDebug("<<< Exiting ActivateLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish})", portalid, cultureCode, publish);
        }

        /// <summary>Publishes the language.</summary>
        /// <param name="portalid">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="publish">if set to <see langword="true"/> will publish the language.</param>
        public void PublishLanguage(int portalid, string cultureCode, bool publish)
        {
			Logger.LogDebug(">>> Entering PublishLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish})", portalid, cultureCode, publish);
            Dictionary<string, Locale> enabledLanguages = this.GetLocales(portalid);
            if (enabledLanguages.TryGetValue(cultureCode, out var enabledLanguage))
			Logger.LogDebug("PublishLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish}): enabledLanguages → {EnabledLanguages}", portalid, cultureCode, publish, enabledLanguages);
            {
                enabledLanguage.IsPublished = publish;
				Logger.LogDebug("PublishLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish}): enabledLanguage.IsPublished → {Arg3}", portalid, cultureCode, publish, enabledLanguage.IsPublished);
                this.UpdatePortalLocale(enabledLanguage);
                if (publish)
                {
                    // only publish tabs if we actually need to do that
                    // we cannot "unpublish"
                    this.tabController.PublishTabs(TabController.GetTabsBySortOrder(portalid, cultureCode, false));
                }
            }
			Logger.LogDebug("<<< Exiting PublishLanguage(portalid={Portalid},cultureCode={CultureCode},publish={Publish})", portalid, cultureCode, publish);
        }

        private static object GetLocalesCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetLocalesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dataProvider = (DataProvider)cacheItemArgs.ParamList[1];
			Logger.LogDebug("GetLocalesCallBack(cacheItemArgs={CacheItemArgs}): dataProvider → {DataProvider}", cacheItemArgs, dataProvider);
            var languages = portalId > Null.NullInteger
                ? dataProvider.GetLanguagesByPortal(portalId)
                : dataProvider.GetLanguages();
				Logger.LogDebug("GetLocalesCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);
			Logger.LogDebug("<<< Exiting GetLocalesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.FillDictionary(
                "CultureCode",
                languages,
                new Dictionary<string, Locale>(StringComparer.OrdinalIgnoreCase));
        }
    }
}
