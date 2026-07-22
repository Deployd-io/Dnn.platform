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
			Logger.LogDebug(">>> Entering LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage})", progress, portalId, translatePages, defaultLanguage);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var languageCount = LocaleController.Instance.GetLocales(portalId).Count;
                    var languageCounter = 0;
					Logger.LogDebug("LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage}): languageCount → {LanguageCount}", progress, portalId, translatePages, defaultLanguage, languageCount);

                    var pageList = GetPages(portalId).Where(p => string.IsNullOrEmpty(p.CultureCode)).ToList();

					Logger.LogDebug("LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage}): pageList → {PageList}", progress, portalId, translatePages, defaultLanguage, pageList);
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
						Logger.LogError("Exception in LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage}): {Arg4}", progress, portalId, translatePages, defaultLanguage, ex.Message);
                        // ignore
                    }
                }
            });
			Logger.LogDebug("<<< Exiting LocalizeSitePages(progress={Progress},portalId={PortalId},translatePages={TranslatePages},defaultLanguage={DefaultLanguage})", progress, portalId, translatePages, defaultLanguage);
        }

        public static void LocalizeLanguagePages(LocalizationProgress progress, int portalId, string cultureCode, string defaultLocale)
        {
			Logger.LogDebug(">>> Entering LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale})", progress, portalId, cultureCode, defaultLocale);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var pageList = GetTabsToLocalize(portalId, cultureCode, defaultLocale);
                    var locale = LocaleController.Instance.GetLocale(cultureCode);
					Logger.LogDebug("LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale}): pageList → {PageList}", progress, portalId, cultureCode, defaultLocale, pageList);
					Logger.LogDebug("LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale}): locale → {Locale}", progress, portalId, cultureCode, defaultLocale, locale);

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
						Logger.LogError("Exception in LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale}): {Arg4}", progress, portalId, cultureCode, defaultLocale, ex.Message);
                        // ignore
                    }
                }
            });
			Logger.LogDebug("<<< Exiting LocalizeLanguagePages(progress={Progress},portalId={PortalId},cultureCode={CultureCode},defaultLocale={DefaultLocale})", progress, portalId, cultureCode, defaultLocale);
        }

        internal static LocalizationProgress ReadProgressFile()
        {
			Logger.LogDebug(">>> Entering ReadProgressFile()");
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 256);
			Logger.LogDebug("ReadProgressFile(): path → {Path}", path);
			Logger.LogDebug("ReadProgressFile(): file → {File}", file);
            var bytes = new byte[file.Length];
            var bytesToRead = file.Length;
            var bytesRead = 0;

            do
            {
                var readCount = file.Read(bytes, bytesRead, bytes.Length);
                bytesRead += readCount;
				Logger.LogDebug("ReadProgressFile(): readCount → {ReadCount}", readCount);
                bytesToRead -= readCount;
            }
            while (bytesToRead > 0);

            var text = Encoding.UTF8.GetString(bytes);
			Logger.LogDebug("ReadProgressFile(): text → {Text}", text);
			Logger.LogDebug("<<< Exiting ReadProgressFile()");
            return JsonConvert.DeserializeObject<LocalizationProgress>(text);
        }

        private static List<TabInfo> GetTabsToLocalize(int portalId, string code, string defaultLocale)
        {
			Logger.LogDebug(">>> Entering GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale})", portalId, code, defaultLocale);
            var results = new List<TabInfo>();
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId)
                .Where(kvp => (kvp.Value.CultureCode == defaultLocale || string.IsNullOrEmpty(kvp.Value.CultureCode))
                && !kvp.Value.IsDeleted && !kvp.Value.IsSystem && !kvp.Value.TabPath.StartsWith("//Admin", StringComparison.OrdinalIgnoreCase));
				Logger.LogDebug("GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale}): results → {Results}", portalId, code, defaultLocale, results);

            foreach (var kvp in portalTabs)
            {
                if (kvp.Value.LocalizedTabs.Count == 0)
                {
                    results.Add(kvp.Value);
                }
                else
                {
                    var tabLocalizedInCulture = kvp.Value.LocalizedTabs.Any(localizedTab => localizedTab.Value.CultureCode == code);
                    if (!tabLocalizedInCulture)
					Logger.LogDebug("GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale}): tabLocalizedInCulture → {TabLocalizedInCulture}", portalId, code, defaultLocale, tabLocalizedInCulture);
                    {
                        results.Add(kvp.Value);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetTabsToLocalize(portalId={PortalId},code={Code},defaultLocale={DefaultLocale})", portalId, code, defaultLocale);
            return results;
        }

        private static void ProcessLanguage(List<TabInfo> pageList, Locale locale, string defaultLocale, int languageCount, int totalLanguages, LocalizationProgress progress)
        {
            progress.PrimaryTotal = totalLanguages;
            progress.PrimaryValue = languageCount;

            var total = pageList.Count;
            if (total == 0)
            {
                progress.SecondaryTotal = 0;
                progress.SecondaryValue = 0;
                progress.SecondaryPercent = 100;
            }

            for (var i = 0; i < total; i++)
            {
                var currentTab = pageList.ElementAt(i);
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
        }

        private static void SaveProgressToFile(LocalizationProgress progress)
        {
			Logger.LogDebug(">>> Entering SaveProgressToFile(progress={Progress})", progress);
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
            var text = JsonConvert.SerializeObject(progress);
			Logger.LogDebug("SaveProgressToFile(progress={Progress}): path → {Path}", progress, path);
			Logger.LogDebug("SaveProgressToFile(progress={Progress}): text → {Text}", progress, text);
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 256);
            var bytes = Encoding.UTF8.GetBytes(text);
			Logger.LogDebug("SaveProgressToFile(progress={Progress}): file → {File}", progress, file);
			Logger.LogDebug("SaveProgressToFile(progress={Progress}): bytes → {Bytes}", progress, bytes);
            file.Write(bytes, 0, bytes.Length);
            file.Flush();
			Logger.LogDebug("<<< Exiting SaveProgressToFile(progress={Progress})", progress);
        }

        private static List<TabInfo> GetPages(int portalId)
        {
			Logger.LogDebug(">>> Entering GetPages(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetPages(portalId={PortalId})", portalId);
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
			Logger.LogDebug(">>> Entering PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish})", cultureCode, portalId, publish);
            var enabledLanguages = LocaleController.Instance.GetLocales(portalId);
            if (enabledLanguages.TryGetValue(cultureCode, out var enabledLanguage))
			Logger.LogDebug("PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish}): enabledLanguages → {EnabledLanguages}", cultureCode, portalId, publish, enabledLanguages);
            {
                enabledLanguage.IsPublished = publish;
				Logger.LogDebug("PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish}): enabledLanguage.IsPublished → {Arg3}", cultureCode, portalId, publish, enabledLanguage.IsPublished);
                LocaleController.Instance.UpdatePortalLocale(enabledLanguage);
            }
			Logger.LogDebug("<<< Exiting PublishLanguage(cultureCode={CultureCode},portalId={PortalId},publish={Publish})", cultureCode, portalId, publish);
        }
    }
}
