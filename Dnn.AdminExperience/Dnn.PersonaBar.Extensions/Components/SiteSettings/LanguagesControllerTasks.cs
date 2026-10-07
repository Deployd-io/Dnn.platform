// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Dnn.PersonaBar.SiteSettings.Services.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    internal class LanguagesControllerTasks
    {
        private const string LocalResourcesFile = "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.SiteSettings/App_LocalResources/SiteSettings.resx";
        private const string LocalizationProgressFile = "PersonaBarLocalizationProgress.txt";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesControllerTasks));

        public static void LocalizeSitePages(LocalizationProgress progress, int portalId, bool translatePages, string defaultLanguage)
        {
			Logger.LogError(">>> Entering LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage})", progress, portalId, translatePages, defaultLanguage);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var languageCount = LocaleController.Instance.GetLocales(portalId).Count;
					Logger.LogError("LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage}): languageCount → {LanguageCount}", progress, portalId, translatePages, defaultLanguage, languageCount);
                    var languageCounter = 0;

                    var pageList = GetPages(portalId).Where(p => string.IsNullOrEmpty(p.CultureCode)).ToList();
					Logger.LogError("LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage}): pageList → {PageList}", progress, portalId, translatePages, defaultLanguage, pageList);

                    if (translatePages)
                    {
                        // populate default language
                        ProcessLanguage(
                            pageList,
                            LocaleController.Instance.GetLocale(defaultLanguage),
                            defaultLanguage,
                            languageCounter,
                            languageCount,
                            progress);
                    }

                    PublishLanguage(defaultLanguage, portalId, true);

                    PortalController.UpdatePortalSetting(portalId, "ContentLocalizationEnabled", "True");

                    if (translatePages)
                    {
                        // populate other languages
                        pageList = GetPages(portalId).Where(p => p.CultureCode == defaultLanguage).ToList();

                        foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values.Where(l => l.Code != defaultLanguage))
                        {
                            languageCounter++;

                            // add translator role
                            Localization.AddTranslatorRole(portalId, locale);

                            // populate pages
                            ProcessLanguage(pageList, locale, defaultLanguage, languageCounter, languageCount, progress);

                            // Map special pages
                            PortalController.Instance.MapLocalizedSpecialPages(portalId, locale.Code);
                        }
                    }

                    // clear portal cache
                    DataCache.ClearPortalCache(portalId, true);
                    progress.Reset();
                    SaveProgressToFile(progress);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Error(ex);
                        progress.Reset().Error = ex.ToString();
                        SaveProgressToFile(progress);
                    }
                    catch (Exception)
                    {
						Logger.LogError("Exception in LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage}): {Arg4}", progress, portalId, translatePages, defaultLanguage, Exception.Message);
                        // ignore
                    }
                }
            });
			Logger.LogWarning("<<< Exiting LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage})", progress, portalId, translatePages, defaultLanguage);
        }

        public static void LocalizeLanguagePages(LocalizationProgress progress, int portalId, string cultureCode, string defaultLocale)
        {
			Logger.LogError(">>> Entering LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale})", progress, portalId, cultureCode, defaultLocale);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var pageList = GetTabsToLocalize(portalId, cultureCode, defaultLocale);
					Logger.LogError("LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale}): pageList → {PageList}", progress, portalId, cultureCode, defaultLocale, pageList);
                    var locale = LocaleController.Instance.GetLocale(cultureCode);
					Logger.LogError("LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale}): locale → {Locale}", progress, portalId, cultureCode, defaultLocale, locale);

                    // add translator role
                    Localization.AddTranslatorRole(portalId, locale);

                    // populate pages
                    ProcessLanguage(pageList, locale, defaultLocale, 0, 1, progress);

                    // Map special pages
                    PortalController.Instance.MapLocalizedSpecialPages(portalId, locale.Code);

                    // clear portal cache
                    DataCache.ClearPortalCache(portalId, true);
                    progress.Reset();
                    SaveProgressToFile(progress);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Error(ex);
                        progress.Reset().Error = ex.ToString();
                        SaveProgressToFile(progress);
                    }
                    catch (Exception)
                    {
						Logger.LogError("Exception in LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale}): {Arg4}", progress, portalId, cultureCode, defaultLocale, Exception.Message);
                        // ignore
                    }
                }
            });
			Logger.LogWarning("<<< Exiting LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale})", progress, portalId, cultureCode, defaultLocale);
        }

        internal static LocalizationProgress ReadProgressFile()
        {
			Logger.LogError(">>> Entering ReadProgressFile()");
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 256);
			Logger.LogError("ReadProgressFile(): file → {File}", file);
			Logger.LogError("ReadProgressFile(): path → {Path}", path);
            var bytes = new byte[file.Length];
            var bytesToRead = file.Length;
            var bytesRead = 0;

            do
            {
                var readCount = file.Read(bytes, bytesRead, bytes.Length);
				Logger.LogError("ReadProgressFile(): readCount → {ReadCount}", readCount);
                bytesRead += readCount;
                bytesToRead -= readCount;
            }
            while (bytesToRead > 0);

            var text = Encoding.UTF8.GetString(bytes);
			Logger.LogError("ReadProgressFile(): text → {Text}", text);
			Logger.LogWarning("<<< Exiting ReadProgressFile()");
            return JsonConvert.DeserializeObject<LocalizationProgress>(text);
        }

        private static List<TabInfo> GetTabsToLocalize(int portalId, string code, string defaultLocale)
        {
			Logger.LogError(">>> Entering GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale})", portalId, code, defaultLocale);
            var results = new List<TabInfo>();
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId)
                .Where(kvp => (kvp.Value.CultureCode == defaultLocale || string.IsNullOrEmpty(kvp.Value.CultureCode))
                && !kvp.Value.IsDeleted && !kvp.Value.IsSystem && !kvp.Value.TabPath.StartsWith("//Admin", StringComparison.OrdinalIgnoreCase));
				Logger.LogError("GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale}): results → {Results}", portalId, code, defaultLocale, results);

            foreach (var kvp in portalTabs)
            {
                if (kvp.Value.LocalizedTabs.Count == 0)
                {
                    results.Add(kvp.Value);
                }
                else
                {
                    var tabLocalizedInCulture = kvp.Value.LocalizedTabs.Any(localizedTab => localizedTab.Value.CultureCode == code);
					Logger.LogError("GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale}): tabLocalizedInCulture → {TabLocalizedInCulture}", portalId, code, defaultLocale, tabLocalizedInCulture);
                    if (!tabLocalizedInCulture)
                    {
                        results.Add(kvp.Value);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale})", portalId, code, defaultLocale);
            return results;
        }

        private static void ProcessLanguage(List<TabInfo> pageList, Locale locale, string defaultLocale, int languageCount, int totalLanguages, LocalizationProgress progress)
        {
			Logger.LogError(">>> Entering ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress})", pageList, locale, defaultLocale, languageCount, totalLanguages, progress);
            progress.PrimaryTotal = totalLanguages;
            progress.PrimaryValue = languageCount;
			Logger.LogDebug("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): progress.PrimaryValue → {Arg6}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, progress.PrimaryValue);
			Logger.LogDebug("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): progress.PrimaryTotal → {Arg6}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, progress.PrimaryTotal);

            var total = pageList.Count;
			Logger.LogError("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): total → {Total}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, total);
            if (total == 0)
            {
                progress.SecondaryTotal = 0;
				Logger.LogDebug("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): progress.SecondaryTotal → {Arg6}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, progress.SecondaryTotal);
				Logger.LogDebug("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): progress.SecondaryPercent → {Arg6}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, progress.SecondaryPercent);
                progress.SecondaryValue = 0;
				Logger.LogDebug("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): progress.SecondaryValue → {Arg6}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, progress.SecondaryValue);
                progress.SecondaryPercent = 100;
            }

            for (var i = 0; i < total; i++)
            {
                var currentTab = pageList.ElementAt(i);
				Logger.LogError("ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress}): currentTab → {CurrentTab}", pageList, locale, defaultLocale, languageCount, totalLanguages, progress, currentTab);
                var stepNo = i + 1;

                progress.SecondaryTotal = total;
                progress.SecondaryValue = stepNo;
                progress.SecondaryPercent = Convert.ToInt32((float)stepNo / total * 100);
                progress.PrimaryPercent =
                    Convert.ToInt32((languageCount + ((float)stepNo / total)) / totalLanguages * 100);

                progress.CurrentOperationText = string.Format(
                    CultureInfo.CurrentCulture,
                    Localization.GetString("ProcessingPage", LocalResourcesFile),
                    locale.Code,
                    stepNo,
                    total,
                    currentTab.TabName);

                progress.TimeEstimated = (total - stepNo) * 100;

                SaveProgressToFile(progress);

                if (locale.Code == defaultLocale || string.IsNullOrEmpty(locale.Code))
                {
                    TabController.Instance.LocalizeTab(currentTab, locale, true);
                }
                else
                {
                    if (currentTab.IsNeutralCulture)
                    {
                        TabController.Instance.LocalizeTab(currentTab, LocaleController.Instance.GetLocale(defaultLocale), true);
                    }

                    TabController.Instance.CreateLocalizedCopy(currentTab, locale, false);
                }
            }
			Logger.LogWarning("<<< Exiting ProcessLanguage(pageList={PageList},locale={Locale},defaultLocale={DefaultLocale},languageCount={LanguageCount},totalLanguages={TotalLanguages},progress={Progress})", pageList, locale, defaultLocale, languageCount, totalLanguages, progress);
        }

        private static void SaveProgressToFile(LocalizationProgress progress)
        {
			Logger.LogError(">>> Entering SaveProgressToFile(progress={Progress})", progress);
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
            var text = JsonConvert.SerializeObject(progress);
			Logger.LogError("SaveProgressToFile(progress={Progress}): text → {Text}", progress, text);
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 256);
			Logger.LogError("SaveProgressToFile(progress={Progress}): file → {File}", progress, file);
            var bytes = Encoding.UTF8.GetBytes(text);
			Logger.LogError("SaveProgressToFile(progress={Progress}): bytes → {Bytes}", progress, bytes);
			Logger.LogError("SaveProgressToFile(progress={Progress}): path → {Path}", progress, path);
            file.Write(bytes, 0, bytes.Length);
            file.Flush();
			Logger.LogWarning("<<< Exiting SaveProgressToFile(progress={Progress})", progress);
        }

        private static List<TabInfo> GetPages(int portalId)
        {
			Logger.LogError(">>> Entering GetPages(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPages(portalId={PortalId})", portalId);
            return (
                from kvp in TabController.Instance.GetTabsByPortal(portalId)
                where !kvp.Value.TabPath.StartsWith("//Admin", StringComparison.OrdinalIgnoreCase)
                      && !kvp.Value.IsDeleted
                      && !kvp.Value.IsSystem
                select kvp.Value)
                .ToList();
        }

        private static void PublishLanguage(string cultureCode, int portalId, bool publish)
        {
			Logger.LogError(">>> Entering PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish})", cultureCode, portalId, publish);
            var enabledLanguages = LocaleController.Instance.GetLocales(portalId);
			Logger.LogError("PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish}): enabledLanguages → {EnabledLanguages}", cultureCode, portalId, publish, enabledLanguages);
            if (enabledLanguages.TryGetValue(cultureCode, out var enabledLanguage))
            {
                enabledLanguage.IsPublished = publish;
				Logger.LogDebug("PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish}): enabledLanguage.IsPublished → {Arg3}", cultureCode, portalId, publish, enabledLanguage.IsPublished);
                LocaleController.Instance.UpdatePortalLocale(enabledLanguage);
            }
			Logger.LogWarning("<<< Exiting PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish})", cultureCode, portalId, publish);
        }
    }
}
