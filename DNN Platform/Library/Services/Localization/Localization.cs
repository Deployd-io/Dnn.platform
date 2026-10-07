// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Modules;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Localization class support localization in system.</summary>
    /// <remarks>
    /// <para>As DNN is used in more and more countries it is very important to provide modules with
    /// good support for international users. Otherwise we are limiting our potential user base to
    /// that using English as their base language.</para>
    /// <para>
    /// You can store the multi language content in resource files and use the api below to get localization content.
    /// Resources files named as: Control(Page)Name + Extension (.aspx/.ascx ) + Language + ".resx"
    /// e.g: <c>Installwizard.aspx.de-DE.resx</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// pageCreationProgressArea.Localization.Total = Localization.GetString("TotalLanguages", LocalResourceFile);
    /// pageCreationProgressArea.Localization.TotalFiles = Localization.GetString("TotalPages", LocalResourceFile);
    /// pageCreationProgressArea.Localization.Uploaded = Localization.GetString("TotalProgress", LocalResourceFile);
    /// pageCreationProgressArea.Localization.UploadedFiles = Localization.GetString("Progress", LocalResourceFile);
    /// pageCreationProgressArea.Localization.CurrentFileName = Localization.GetString("Processing", LocalResourceFile);
    /// </code>
    /// </example>
    public partial class Localization
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Localization));
        private static string defaultKeyName = "resourcekey";

        ////private static readonly ILocaleController LocaleController.Instance = LocaleController.Instance;
        ////private static readonly ILocalizationProvider _localizationProvider = LocalizationProvider.Instance;
        private static bool? showMissingKeys;

        /// <summary>Gets <c>"~/App_GlobalResources"</c>.</summary>
        public static string ApplicationResourceDirectory
        {
            get
            {
                return "~/App_GlobalResources";
            }
        }

        /// <summary>Gets <c>"~/App_GlobalResources/Exceptions.resx"</c>.</summary>
        public static string ExceptionsResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/Exceptions.resx";
            }
        }

        /// <summary>Gets <c>"~/App_GlobalResources/GlobalResources.resx"</c>.</summary>
        public static string GlobalResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/GlobalResources.resx";
            }
        }

        public static string LocalResourceDirectory
        {
            get
            {
                return "App_LocalResources";
            }
        }

        public static string LocalSharedResourceFile
        {
            get
            {
                return "SharedResources.resx";
            }
        }

        public static string SharedResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/SharedResources.resx";
            }
        }

        /// <summary>Gets a value indicating whether to render a visual indicator that a key is missing (i.e. whether the <c>web.config</c> setting <c>ShowMissingKeys</c> is set).</summary>
        public static bool ShowMissingKeys
        {
            get
            {
                if (showMissingKeys == null)
                {
                    if (Config.GetSetting("ShowMissingKeys") == null)
                    {
                        showMissingKeys = false;
                    }
                    else
                    {
                        showMissingKeys = bool.Parse(Config.GetSetting("ShowMissingKeys".ToLowerInvariant()));
                    }
                }

                return showMissingKeys.Value;
            }
        }

        public static string SupportedLocalesFile
        {
            get
            {
                return ApplicationResourceDirectory + "/Locales.xml";
            }
        }

        public static string SystemLocale
        {
            get
            {
                return "en-US";
            }
        }

        public static string SystemTimeZone
        {
            get
            {
                return "Pacific Standard Time";
            }
        }

        /// <summary>
        /// Gets or sets the KeyName property returns and caches the name of the key attribute used to lookup resources.
        /// This can be configured by setting ResourceManagerKey property in the web.config file. The default value for this property
        /// is <c>"resourcekey"</c>.
        /// </summary>
        public static string KeyName
        {
            get
            {
                return defaultKeyName;
            }

            set
            {
                defaultKeyName = value;
                if (string.IsNullOrEmpty(defaultKeyName))
                {
                    defaultKeyName = "resourcekey";
                }
            }
        }

        /// <summary>Gets the current Culture being used.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string CurrentCulture => Thread.CurrentThread.CurrentCulture.ToString();

        /// <summary>Gets the CurrentUICulture for the Thread.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string CurrentUICulture => Thread.CurrentThread.CurrentUICulture.ToString();

        public static int ActiveLanguagesByPortalID(int portalID)
        {
			Logger.LogError(">>> Entering ActiveLanguagesByPortalID(portalID={PortalID})", portalID);
            // Default to 1 (maybe called during portal creation before languages are enabled for portal)
            int count = 1;
			Logger.LogError("ActiveLanguagesByPortalID(portalID={PortalID}): count → {Count}", portalID, count);
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalID);
			Logger.LogError("ActiveLanguagesByPortalID(portalID={PortalID}): locales → {Locales}", portalID, locales);
            if (locales != null)
            {
                count = locales.Count;
            }

			Logger.LogWarning("<<< Exiting ActiveLanguagesByPortalID(portalID={PortalID})", portalID);
            return count;
        }

        /// <summary>Add the language to the portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="languageID">The language ID.</param>
        /// <param name="clearCache">Whether to clear the cache after adding.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void AddLanguageToPortal(int portalID, int languageID, bool clearCache)
            => AddLanguageToPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalID, languageID, clearCache);

        /// <summary>Add the language to the portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="languageId">The language ID.</param>
        /// <param name="clearCache">Whether to clear the cache after adding.</param>
        public static void AddLanguageToPortal(IEventLogger eventLogger, int portalId, int languageId, bool clearCache)
        {
			Logger.LogError(">>> Entering AddLanguageToPortal(portalID={PortalID},languageID={LanguageID},clearCache={ClearCache})", portalID, languageID, clearCache);
            // try to get valid locale reference
            var newLocale = LocaleController.Instance.GetLocale(languageId);

			Logger.LogError("AddLanguageToPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},clearCache={ClearCache}): newLocale → {NewLocale}", eventLogger, portalId, languageId, clearCache, newLocale);
			Logger.LogError("AddLanguageToPortal(portalID={PortalID},languageID={LanguageID},clearCache={ClearCache}): newLocale → {NewLocale}", portalID, languageID, clearCache, newLocale);
            // we can only add a valid locale
            if (newLocale != null)
            {
                // check if locale has not been added to portal already
                var portalLocale = LocaleController.Instance.GetLocale(portalId, newLocale.Code);
				Logger.LogError("AddLanguageToPortal(portalID={PortalID},languageID={LanguageID},clearCache={ClearCache}): portalLocale → {PortalLocale}", portalID, languageID, clearCache, portalLocale);
				Logger.LogError("AddLanguageToPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},clearCache={ClearCache}): portalLocale → {PortalLocale}", eventLogger, portalId, languageId, clearCache, portalLocale);

                // locale needs to be added
                if (portalLocale == null)
                {
                    // We need to add a translator role for the language
                    bool contentLocalizationEnabled = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", portalId, false);
                    if (contentLocalizationEnabled)
                    {
                        // Create new Translator Role
                        AddTranslatorRole(portalId, newLocale);
                    }

                    DataProvider.Instance().AddPortalLanguage(portalId, languageId, false, UserController.Instance.GetCurrentUserInfo().UserID);
                    string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.LocalesCacheKey, portalId);
                    DataCache.RemoveCache(cacheKey);

                    eventLogger.AddLog(
                        "portalID/languageID",
                        $"{portalId}/{languageId}",
                        PortalController.Instance.GetCurrentSettings(),
                        UserController.Instance.GetCurrentUserInfo().UserID,
                        EventLogType.LANGUAGETOPORTAL_CREATED);

                    var portalInfo = PortalController.Instance.GetPortal(portalId);
					Logger.LogError("AddLanguageToPortal(portalID={PortalID},languageID={LanguageID},clearCache={ClearCache}): portalInfo → {PortalInfo}", portalID, languageID, clearCache, portalInfo);
                    if (portalInfo != null && newLocale.Code != portalInfo.DefaultLanguage)
                    {
						Logger.LogError("AddLanguageToPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},clearCache={ClearCache}): portalInfo → {PortalInfo}", eventLogger, portalId, languageId, clearCache, portalInfo);
                        // check to see if this is the first extra language being added to the portal
                        var portalLocales = LocaleController.Instance.GetLocales(portalId);
						Logger.LogError("AddLanguageToPortal(portalID={PortalID},languageID={LanguageID},clearCache={ClearCache}): portalLocales → {PortalLocales}", portalID, languageID, clearCache, portalLocales);
						Logger.LogError("AddLanguageToPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},clearCache={ClearCache}): portalLocales → {PortalLocales}", eventLogger, portalId, languageId, clearCache, portalLocales);
                        var firstExtraLanguage = (portalLocales != null) && portalLocales.Count == 2;

						Logger.LogError("AddLanguageToPortal(portalID={PortalID},languageID={LanguageID},clearCache={ClearCache}): firstExtraLanguage → {FirstExtraLanguage}", portalID, languageID, clearCache, firstExtraLanguage);
                        if (firstExtraLanguage)
                        {
							Logger.LogError("AddLanguageToPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},clearCache={ClearCache}): firstExtraLanguage → {FirstExtraLanguage}", eventLogger, portalId, languageId, clearCache, firstExtraLanguage);
                            AddLanguageHttpAlias(portalId, LocaleController.Instance.GetLocale(portalId, portalInfo.DefaultLanguage));
                        }

                        AddLanguageHttpAlias(portalId, newLocale);
                    }

                    if (clearCache)
                    {
                        DataCache.ClearPortalCache(portalId, false);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting AddLanguageToPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},clearCache={ClearCache})", eventLogger, portalId, languageId, clearCache);
        }

        public static void AddLanguagesToPortal(int portalID)
        {
			Logger.LogError(">>> Entering AddLanguagesToPortal(portalID={PortalID})", portalID);
            foreach (Locale language in LocaleController.Instance.GetLocales(Null.NullInteger).Values)
            {
                // Add Portal/Language to PortalLanguages
                AddLanguageToPortal(portalID, language.LanguageId, false);
            }

            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.LocalesCacheKey, portalID));
			Logger.LogWarning("<<< Exiting AddLanguagesToPortal(portalID={PortalID})", portalID);
        }

        public static void AddLanguageToPortals(int languageID)
        {
			Logger.LogError(">>> Entering AddLanguageToPortals(languageID={LanguageID})", languageID);
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                // Add Portal/Language to PortalLanguages
                AddLanguageToPortal(portal.PortalID, languageID, false);

                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.LocalesCacheKey, portal.PortalID));
            }
			Logger.LogWarning("<<< Exiting AddLanguageToPortals(languageID={LanguageID})", languageID);
        }

        public static void AddTranslatorRole(int portalID, Locale language)
        {
			Logger.LogError(">>> Entering AddTranslatorRole(portalID={PortalID},language={Language})", portalID, language);
            // Create new Translator Role
            string roleName = $"Translator ({language.Code})";
            RoleInfo role = RoleController.Instance.GetRole(portalID, r => r.RoleName == roleName);
			Logger.LogError("AddTranslatorRole(portalID={PortalID},language={Language}): role → {Role}", portalID, language, role);

            if (role == null)
            {
                role = new RoleInfo
                {
                    RoleGroupID = Null.NullInteger, PortalID = portalID, RoleName = roleName, Description = $"A role for {language.EnglishName} translators",
                    SecurityMode = SecurityMode.SecurityRole,
                    Status = RoleStatus.Approved,
                };
                RoleController.Instance.AddRole(role);
            }

            string roles = $"Administrators;Translator ({language.Code})";

            PortalController.UpdatePortalSetting(portalID, $"DefaultTranslatorRoles-{language.Code}", roles);
			Logger.LogWarning("<<< Exiting AddTranslatorRole(portalID={PortalID},language={Language})", portalID, language);
        }

        /// <summary>Converts old TimeZoneOffset to new <see cref="TimeZoneInfo"/>.</summary>
        /// <param name="timeZoneOffsetInMinutes">An offset in minutes, e.g. -480 (-8 times 60) for Pacific Time Zone.</param>
        /// <returns>TimeZoneInfo is returned if timeZoneOffsetInMinutes is valid, otherwise TimeZoneInfo.Local is returned.</returns>
        /// <remarks>Initial mapping is based on hard-coded rules. These rules are hard-coded from old standard TimeZones.xml data.
        /// When offset is not found hard-coded mapping, a lookup is performed in timezones defined in the system. The first found entry is returned.
        /// When mapping is not found, a default TimeZoneInfo.Local us returned.</remarks>
        public static TimeZoneInfo ConvertLegacyTimeZoneOffsetToTimeZoneInfo(int timeZoneOffsetInMinutes)
        {
			Logger.LogError(">>> Entering ConvertLegacyTimeZoneOffsetToTimeZoneInfo(timeZoneOffsetInMinutes={TimeZoneOffsetInMinutes})", timeZoneOffsetInMinutes);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

			Logger.LogError("ConvertLegacyTimeZoneOffsetToTimeZoneInfo(timeZoneOffsetInMinutes={TimeZoneOffsetInMinutes}): timeZoneInfo → {TimeZoneInfo}", timeZoneOffsetInMinutes, timeZoneInfo);
            // lookup existing mapping
            switch (timeZoneOffsetInMinutes)
            {
                case -720:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Dateline Standard Time");
                    break;
                case -660:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time");
                    break;
                case -600:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
                    break;
                case -540:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time");
                    break;
                case -480:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    break;
                case -420:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                    break;
                case -360:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    break;
                case -300:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    break;
                case -240:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
                    break;
                case -210:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
                    break;
                case -180:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                    break;
                case -120:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mid-Atlantic Standard Time");
                    break;
                case -60:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Cape Verde Standard Time");
                    break;
                case 0:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    break;
                case 60:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    break;
                case 120:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                    break;
                case 180:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                    break;
                case 210:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                    break;
                case 240:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                    break;
                case 270:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Afghanistan Standard Time");
                    break;
                case 300:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                    break;
                case 330:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    break;
                case 345:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
                    break;
                case 360:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time");
                    break;
                case 390:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
                    break;
                case 420:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    break;
                case 480:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    break;
                case 540:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    break;
                case 570:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Cen. Australia Standard Time");
                    break;
                case 600:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                    break;
                case 660:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Magadan Standard Time");
                    break;
                case 720:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                    break;
                case 780:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tonga Standard Time");
                    break;
                default:
                    foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
                    {
                        if (Math.Abs(timeZone.BaseUtcOffset.TotalMinutes - timeZoneOffsetInMinutes) < 0.001)
                        {
                            timeZoneInfo = timeZone;
                            break;
                        }
                    }

                    break;
            }

			Logger.LogWarning("<<< Exiting ConvertLegacyTimeZoneOffsetToTimeZoneInfo(timeZoneOffsetInMinutes={TimeZoneOffsetInMinutes})", timeZoneOffsetInMinutes);
            return timeZoneInfo;
        }

        /// <summary>Delete the language.</summary>
        /// <param name="language">The language/locale.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteLanguage(Locale language)
            => DeleteLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), language);

        /// <summary>Delete the language.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="language">The language/locale.</param>
        public static void DeleteLanguage(IEventLogger eventLogger, Locale language)
            => DeleteLanguage(eventLogger, language, false);

        /// <summary>Delete the language.</summary>
        /// <param name="language">The language/locale.</param>
        /// <param name="isInstalling">Whether the site is installing.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteLanguage(Locale language, bool isInstalling)
            => DeleteLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), language, isInstalling);

        /// <summary>Delete the language.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="language">The language/locale.</param>
        /// <param name="isInstalling">Whether the site is installing.</param>
        public static void DeleteLanguage(IEventLogger eventLogger, Locale language, bool isInstalling)
        {
			Logger.LogError(">>> Entering DeleteLanguage(language={Language})", language);
            // remove languages from all portals
            RemoveLanguageFromPortals(language.LanguageId, isInstalling);

            DataProvider.Instance().DeleteLanguage(language.LanguageId);
            eventLogger.AddLog(language, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.LANGUAGE_DELETED);
            DataCache.ClearHostCache(true);
			Logger.LogWarning("<<< Exiting DeleteLanguage(eventLogger={EventLogger},language={Language},isInstalling={IsInstalling})", eventLogger, language, isInstalling);
        }

        public static string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback)
        {
			Logger.LogError(">>> Entering BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback})", cultureCodes, fallback);
            Logger.LogWarning("<<< Exiting BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback})", cultureCodes, fallback);
            return TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(cultureCodes, fallback);
        }

        public static string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes)
        {
            return TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(cultureCodes);
        }

        public static string GetExceptionMessage(string key, string defaultValue)
        {
            if (HttpContext.Current == null)
            {
                return defaultValue;
            }

			Logger.LogWarning("<<< Exiting GetExceptionMessage(key={Key},defaultValue={DefaultValue})", key, defaultValue);
            return GetString(key, ExceptionsResourceFile);
        }

        public static string GetExceptionMessage(string key, string defaultValue, params object[] @params)
        {
            if (HttpContext.Current == null)
            {
                return string.Format(CultureInfo.CurrentCulture, defaultValue, @params);
            }

            var content = GetString(key, ExceptionsResourceFile);
            return string.Format(CultureInfo.CurrentCulture, string.IsNullOrEmpty(content) ? defaultValue : GetString(key, ExceptionsResourceFile), @params);
        }

        public static string GetLanguageDisplayMode(int portalId)
        {
			Logger.LogError(">>> Entering GetLanguageDisplayMode(portalId={PortalId})", portalId);
            string viewTypePersonalizationKey = "ViewType" + portalId;
            string viewType = Convert.ToString(Personalization.Personalization.GetProfile("LanguageDisplayMode", viewTypePersonalizationKey), CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(viewType))
            {
                viewType = "NATIVE";
				Logger.LogError("GetLanguageDisplayMode(portalId={PortalId}): viewType → {ViewType}", portalId, viewType);
            }

			Logger.LogWarning("<<< Exiting GetLanguageDisplayMode(portalId={PortalId})", portalId);
            return viewType;
        }

        public static string GetLocaleName(string code, CultureDropDownTypes displayType)
        {
			Logger.LogError(">>> Entering GetLocaleName(code={Code},displayType={DisplayType})", code, displayType);
            string name;

            // Create a CultureInfo class based on culture
            CultureInfo info = CultureInfo.GetCultureInfo(code);
			Logger.LogError("GetLocaleName(code={Code},displayType={DisplayType}): info → {Info}", code, displayType, info);

            // Based on the display type desired by the user, select the correct property
            switch (displayType)
            {
                case CultureDropDownTypes.EnglishName:
                    name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.EnglishName);
					Logger.LogError("GetLocaleName(code={Code},displayType={DisplayType}): name → {Name}", code, displayType, name);
					Logger.LogDebug("GetLocaleName(code={Code},displayType={DisplayType}): name → {Name}", code, displayType, name);
                    break;
                case CultureDropDownTypes.Lcid:
                    name = info.LCID.ToString(CultureInfo.CurrentCulture);
                    break;
                case CultureDropDownTypes.Name:
                    name = info.Name;
                    break;
                case CultureDropDownTypes.NativeName:
                    name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.NativeName);
                    break;
                case CultureDropDownTypes.TwoLetterIsoCode:
                    name = info.TwoLetterISOLanguageName;
                    break;
                case CultureDropDownTypes.ThreeLetterIsoCode:
                    name = info.ThreeLetterISOLanguageName;
                    break;
                default:
                    name = info.DisplayName;
                    break;
            }

			Logger.LogWarning("<<< Exiting GetLocaleName(code={Code},displayType={DisplayType})", code, displayType);
            return name;
        }

        /// <summary>
        /// Detects the current language for the request.
        /// The order in which the language is being detect is:
        /// <list type="number">
        ///     <item><description>QueryString</description></item>
        ///     <item><description>Cookie</description></item>
        ///     <item><description>User profile (if request is authenticated)</description></item>
        ///     <item><description>Browser preference (if portal has this option enabled)</description></item>
        ///     <item><description>Portal default</description></item>
        ///     <item><description>System default (en-US)</description></item>
        /// </list>
        /// At any point, if a valid language is detected nothing else should be done.
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo.</returns>
        [DnnDeprecated(9, 8, 0, "Use overload taking IPortalSettings instead")]
        public static partial CultureInfo GetPageLocale(PortalSettings portalSettings)
        {
			Logger.LogWarning("<<< Exiting GetPageLocale(portalSettings={PortalSettings})", portalSettings);
            return GetPageLocale((IPortalSettings)portalSettings);
        }

        /// <summary>
        /// Detects the current language for the request.
        /// The order in which the language is being detect is:
        /// <list type="number">
        ///     <item><description>QueryString</description></item>
        ///     <item><description>Cookie</description></item>
        ///     <item><description>User profile (if request is authenticated)</description></item>
        ///     <item><description>Browser preference (if portal has this option enabled)</description></item>
        ///     <item><description>Portal default</description></item>
        ///     <item><description>System default (en-US)</description></item>
        /// </list>
        /// At any point, if a valid language is detected nothing else should be done.
        /// </summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo.</returns>
        public static CultureInfo GetPageLocale(IPortalSettings portalSettings)
        {
            CultureInfo pageCulture = null;

            // 1. querystring
            if (portalSettings != null)
            {
                pageCulture = GetCultureFromQs(portalSettings);
            }

            // 2. cookie
            if (portalSettings != null && pageCulture == null)
            {
                pageCulture = GetCultureFromCookie(portalSettings);
            }

            // 3. user preference
            if (portalSettings != null && pageCulture == null)
            {
                pageCulture = GetCultureFromProfile(portalSettings);
            }

            // 4. browser
            if (portalSettings != null && pageCulture == null)
            {
                pageCulture = GetCultureFromBrowser(portalSettings);
            }

            // 5. portal default
            if (portalSettings != null && pageCulture == null)
            {
                pageCulture = GetCultureFromPortal(portalSettings);
            }

            // 6. system default
            if (pageCulture == null)
            {
                pageCulture = new CultureInfo(SystemLocale);
            }

            // finally set the cookie
            SetLanguage(pageCulture.Name);
            return pageCulture;
        }

        /// <summary>Tries to get a valid language from the browser preferences.</summary>
        /// <param name="portalId">Id of the current portal.</param>
        /// <returns>A valid CultureInfo if any is found.</returns>
        public static CultureInfo GetBrowserCulture(int portalId)
        {
			Logger.LogError(">>> Entering GetBrowserCulture(portalId={PortalId})", portalId);
            if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.UserLanguages == null)
            {
                return null;
            }

            CultureInfo culture = null;
			Logger.LogError("GetBrowserCulture(portalId={PortalId}): culture → {Culture}", portalId, culture);
            foreach (string userLang in HttpContext.Current.Request.UserLanguages)
            {
                // split userlanguage by ";"... all but the first language will contain a preferrence index eg. ;q=.5
                string language = userLang.Split(';')[0];
				Logger.LogError("GetBrowserCulture(portalId={PortalId}): language → {Language}", portalId, language);
                culture = GetCultureFromString(portalId, language);
                if (culture != null)
                {
                    break;
                }
            }

			Logger.LogWarning("<<< Exiting GetBrowserCulture(portalId={PortalId})", portalId);
            return culture;
        }

        public static string GetResourceFileName(string resourceFileName, string language, string mode, int portalId)
        {
			Logger.LogError(">>> Entering GetResourceFileName(resourceFileName={ResourceFileName},language={Language},mode={Mode},portalId={PortalId})", resourceFileName, language, mode, portalId);
            if (!resourceFileName.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
            {
                resourceFileName += ".resx";
            }

            if (language != SystemLocale)
            {
                if (resourceFileName.EndsWith(".en-us.resx", StringComparison.OrdinalIgnoreCase))
                {
                    resourceFileName = $"{resourceFileName.Substring(0, resourceFileName.Length - 11)}.{language}.resx";
					Logger.LogError("GetResourceFileName(resourceFileName={ResourceFileName},language={Language},mode={Mode},portalId={PortalId}): resourceFileName → {ResourceFileName1}", resourceFileName, language, mode, portalId, resourceFileName);
                }
                else
                {
                    resourceFileName = $"{resourceFileName.Substring(0, resourceFileName.Length - 5)}.{language}.resx";
                }
            }

            if (mode == "Host")
            {
                resourceFileName = $"{resourceFileName.Substring(0, resourceFileName.Length - 5)}.Host.resx";
            }
            else if (mode == "Portal")
            {
                resourceFileName = $"{resourceFileName.Substring(0, resourceFileName.Length - 5)}.Portal-{portalId}.resx";
            }

			Logger.LogWarning("<<< Exiting GetResourceFileName(resourceFileName={ResourceFileName},language={Language},mode={Mode},portalId={PortalId})", resourceFileName, language, mode, portalId);
            return resourceFileName;
        }

        public static string GetResourceFile(Control control, string fileName)
        {
			Logger.LogError(">>> Entering GetResourceFile(control={Control},fileName={FileName})", control, fileName);
            Logger.LogWarning("<<< Exiting GetResourceFile(control={Control},fileName={FileName})", control, fileName);
            return control.TemplateSourceDirectory + "/" + LocalResourceDirectory + "/" + fileName;
        }

        public static string GetString(string key, Control ctrl)
        {
			Logger.LogError(">>> Entering GetString(key={Key},ctrl={Ctrl})", key, ctrl);
            // We need to find the parent module
            Control parentControl = ctrl.Parent;
            string localizedText;
            var moduleControl = parentControl as IModuleControl;
            if (moduleControl == null)
            {
                PropertyInfo pi = parentControl.GetType().GetProperty("LocalResourceFile");
				Logger.LogError("GetString(key={Key},ctrl={Ctrl}): pi → {Pi}", key, ctrl, pi);
                if (pi != null)
                {
                    // If control has a LocalResourceFile property use this
                    localizedText = GetString(key, pi.GetValue(parentControl, null).ToString());
					Logger.LogError("GetString(key={Key},ctrl={Ctrl}): localizedText → {LocalizedText}", key, ctrl, localizedText);
                }
                else
                {
					Logger.LogDebug("GetString(key={Key},ctrl={Ctrl}): localizedText → {LocalizedText}", key, ctrl, localizedText);
                    // Drill up to the next level
                    localizedText = GetString(key, parentControl);
                }
            }
            else
            {
                // We are at the Module Level so return key
                // Get Resource File Root from Parents LocalResourceFile Property
                localizedText = GetString(key, moduleControl.LocalResourceFile);
            }

			Logger.LogWarning("<<< Exiting GetString(key={Key},ctrl={Ctrl})", key, ctrl);
            return localizedText;
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the resource key.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key)
        {
            return GetString(key, null, PortalSettings.Current, null, false);
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="portalSettings">The current portals Portal Settings.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key, PortalSettings portalSettings)
        {
            return LocalizationProvider.Instance.GetString(key, null, null, portalSettings);
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="resourceFileRoot">The Local Resource root.</param>
        /// <param name="disableShowMissingKeys">Disable to show missing key.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key, string resourceFileRoot, bool disableShowMissingKeys)
        {
            return GetString(key, resourceFileRoot, PortalSettings.Current, null, disableShowMissingKeys);
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="resourceFileRoot">The Resource File Name.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key, string resourceFileRoot)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot);
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="resourceFileRoot">The Local Resource root.</param>
        /// <param name="language">A specific language to lookup the string.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key, string resourceFileRoot, string language)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot, language);
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="resourceFileRoot">The Local Resource root.</param>
        /// <param name="portalSettings">The current portals Portal Settings.</param>
        /// <param name="language">A specific language to lookup the string.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key, string resourceFileRoot, PortalSettings portalSettings, string language)
        {
            return GetString(key, resourceFileRoot, portalSettings, language, false);
        }

        /// <overloads>One of six overloads.</overloads>
        /// <summary>GetString gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="resourceFileRoot">The Local Resource root.</param>
        /// <param name="portalSettings">The current portals Portal Settings.</param>
        /// <param name="language">A specific language to lookup the string.</param>
        /// <param name="disableShowMissingKeys">Disables the show missing keys flag.</param>
        /// <returns>The localized Text.</returns>
        public static string GetString(string key, string resourceFileRoot, PortalSettings portalSettings, string language, bool disableShowMissingKeys)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot, language, portalSettings, disableShowMissingKeys);
        }

        /// <summary>GetStringUrl gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <param name="resourceFileRoot">The Local Resource root.</param>
        /// <returns>The localized Text.</returns>
        /// <remarks>
        /// This function should be used to retrieve strings to be used on URLs.
        /// It is the same as <see cref="GetString(string, string)">GetString(name,ResourceFileRoot)</see> method,
        /// but it disables the ShowMissingKey flag, so even in testing scenarios, the correct string
        /// is returned.
        /// </remarks>
        public static string GetStringUrl(string key, string resourceFileRoot)
        {
			Logger.LogError(">>> Entering GetStringUrl(key={Key},resourceFileRoot={ResourceFileRoot})", key, resourceFileRoot);
            Logger.LogWarning("<<< Exiting GetStringUrl(key={Key},resourceFileRoot={ResourceFileRoot})", key, resourceFileRoot);
            return GetString(key, resourceFileRoot, PortalSettings.Current, null, true);
        }

        /// <summary>this function will escape reserved character fields to their "safe" JavaScript equivalents.</summary>
        /// <param name="unsafeString">The string to be parsed for unsafe characters.</param>
        /// <returns>the string that is safe to use in a JavaScript function.</returns>
        public static string GetSafeJSString(string unsafeString)
        {
			Logger.LogError(">>> Entering GetSafeJSString(unsafeString={UnsafeString})", unsafeString);
            if (string.IsNullOrEmpty(unsafeString))
            {
                return string.Empty;
            }

			Logger.LogWarning("<<< Exiting GetSafeJSString(unsafeString={UnsafeString})", unsafeString);
			Logger.LogWarning("<<< Exiting GetSafeJSString(key={Key},resourceFileRoot={ResourceFileRoot})", key, resourceFileRoot);
            return HttpUtility.JavaScriptStringEncode(unsafeString);
        }

        /// <summary>this function will escape reserved character fields to their "safe" javascript equivalents.</summary>
        /// <param name="key">localization key.</param>
        /// <param name="resourceFileRoot">file for localization key.</param>
        /// <returns>the string that is safe to use in a javascript function.</returns>
        public static string GetSafeJSString(string key, string resourceFileRoot)
        {
            var unsafeString = GetString(key, resourceFileRoot);
			Logger.LogError("GetSafeJSString(key={Key},resourceFileRoot={ResourceFileRoot}): unsafeString → {UnsafeString}", key, resourceFileRoot, unsafeString);
            return GetSafeJSString(unsafeString);
        }

        /// <summary>Gets a SystemMessage.</summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// <list type="bullet">
        ///     <item><description>All fields from HostSettings table in the form of: <c>[Host:<b>field</b>]</c></description></item>
        ///     <item><description>All properties defined in <see cref="PortalInfo" /> in the form of: <c>[Portal:<b>property</b>]</c></description></item>
        ///     <item><description><c>[Portal:URL]</c>: The base URL for the portal</description></item>
        ///     <item><description>All properties defined in <see cref="UserInfo" /> in the form of: <c>[User:<b>property</b>]</c></description></item>
        ///     <item><description>All values stored in the user profile in the form of: <c>[Profile:<b>key</b>]</c></description></item>
        ///     <item><description><c>[User:VerificationCode]</c>: User verification code for verified registrations</description></item>
        ///     <item><description><c>[Date:Current]</c>: Current date</description></item>
        /// </list>
        /// </remarks>
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName)
        {
			Logger.LogError(">>> Entering GetSystemMessage(portalSettings={PortalSettings},messageName={MessageName})", portalSettings, messageName);
            Logger.LogWarning("<<< Exiting GetSystemMessage(portalSettings={PortalSettings},messageName={MessageName})", portalSettings, messageName);
			Logger.LogWarning("<<< Exiting GetSystemMessage(strLanguage={StrLanguage},portalSettings={PortalSettings},messageName={MessageName},userInfo={UserInfo},resourceFile={ResourceFile},customArray={CustomArray},customDictionary={CustomDictionary},customCaption={CustomCaption},accessingUserID={AccessingUserID})", strLanguage, portalSettings, messageName, userInfo, resourceFile, customArray, customDictionary, customCaption, accessingUserID);
            return GetSystemMessage(null, portalSettings, messageName, null, GlobalResourceFile, null);
        }

        /// <summary>Gets a SystemMessage.</summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// <list type="bullet">
        ///     <item><description>All fields from HostSettings table in the form of: <c>[Host:<b>field</b>]</c></description></item>
        ///     <item><description>All properties defined in <see cref="PortalInfo" /> in the form of: <c>[Portal:<b>property</b>]</c></description></item>
        ///     <item><description><c>[Portal:URL]</c>: The base URL for the portal</description></item>
        ///     <item><description>All properties defined in <see cref="UserInfo" /> in the form of: <c>[User:<b>property</b>]</c></description></item>
        ///     <item><description>All values stored in the user profile in the form of: <c>[Profile:<b>key</b>]</c></description></item>
        ///     <item><description><c>[User:VerificationCode]</c>: User verification code for verified registrations</description></item>
        ///     <item><description><c>[Date:Current]</c>: Current date</description></item>
        /// </list>
        /// </remarks>
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, UserInfo userInfo)
        {
            return GetSystemMessage(null, portalSettings, messageName, userInfo, GlobalResourceFile, null);
        }

        /// <summary>Gets a SystemMessage.</summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// <list type="bullet">
        ///     <item><description>All fields from HostSettings table in the form of: <c>[Host:<b>field</b>]</c></description></item>
        ///     <item><description>All properties defined in <see cref="PortalInfo" /> in the form of: <c>[Portal:<b>property</b>]</c></description></item>
        ///     <item><description><c>[Portal:URL]</c>: The base URL for the portal</description></item>
        ///     <item><description>All properties defined in <see cref="UserInfo" /> in the form of: <c>[User:<b>property</b>]</c></description></item>
        ///     <item><description>All values stored in the user profile in the form of: <c>[Profile:<b>key</b>]</c></description></item>
        ///     <item><description><c>[User:VerificationCode]</c>: User verification code for verified registrations</description></item>
        ///     <item><description><c>[Date:Current]</c>: Current date</description></item>
        /// </list>
        /// </remarks>
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo)
        {
            return GetSystemMessage(strLanguage, portalSettings, messageName, userInfo, GlobalResourceFile, null);
        }

        /// <summary>Gets a SystemMessage.</summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// <list type="bullet">
        ///     <item><description>All fields from HostSettings table in the form of: <c>[Host:<b>field</b>]</c></description></item>
        ///     <item><description>All properties defined in <see cref="PortalInfo" /> in the form of: <c>[Portal:<b>property</b>]</c></description></item>
        ///     <item><description><c>[Portal:URL]</c>: The base URL for the portal</description></item>
        ///     <item><description>All properties defined in <see cref="UserInfo" /> in the form of: <c>[User:<b>property</b>]</c></description></item>
        ///     <item><description>All values stored in the user profile in the form of: <c>[Profile:<b>key</b>]</c></description></item>
        ///     <item><description><c>[User:VerificationCode]</c>: User verification code for verified registrations</description></item>
        ///     <item><description><c>[Date:Current]</c>: Current date</description></item>
        /// </list>
        /// </remarks>
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, string resourceFile)
        {
            return GetSystemMessage(null, portalSettings, messageName, null, resourceFile, null);
        }

        /// <summary>Gets a SystemMessage.</summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// <list type="bullet">
        ///     <item><description>All fields from HostSettings table in the form of: <c>[Host:<b>field</b>]</c></description></item>
        ///     <item><description>All properties defined in <see cref="PortalInfo" /> in the form of: <c>[Portal:<b>property</b>]</c></description></item>
        ///     <item><description><c>[Portal:URL]</c>: The base URL for the portal</description></item>
        ///     <item><description>All properties defined in <see cref="UserInfo" /> in the form of: <c>[User:<b>property</b>]</c></description></item>
        ///     <item><description>All values stored in the user profile in the form of: <c>[Profile:<b>key</b>]</c></description></item>
        ///     <item><description><c>[User:VerificationCode]</c>: User verification code for verified registrations</description></item>
        ///     <item><description><c>[Date:Current]</c>: Current date</description></item>
        /// </list>
        /// </remarks>
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile)
        {
            return GetSystemMessage(null, portalSettings, messageName, userInfo, resourceFile, null);
        }

        /// <summary>Gets a SystemMessage passing extra custom parameters to personalize.</summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <c>[Custom:n]</c>, where <c>n</c> is the zero based index which
        /// will be used to find the replacement value in <c>Custom</c> parameter.
        /// </remarks>
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, string resourceFile, ArrayList custom)
        {
            return GetSystemMessage(null, portalSettings, messageName, null, resourceFile, custom);
        }

        /// <summary>Gets a SystemMessage passing extra custom parameters to personalize.</summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <c>[Custom:n]</c>, where <c>n</c> is the zero based index which
        /// will be used to find the replacement value in <c>Custom</c> parameter.
        /// </remarks>
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList custom)
        {
            return GetSystemMessage(null, portalSettings, messageName, userInfo, resourceFile, custom);
        }

        /// <summary>Gets a SystemMessage passing extra custom parameters to personalize.</summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <c>[Custom:n]</c>, where <c>n</c> is the zero based index which
        /// will be used to find the replacement value in <c>Custom</c> parameter.
        /// </remarks>
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList custom)
        {
            return GetSystemMessage(strLanguage, portalSettings, messageName, userInfo, resourceFile, custom, null, string.Empty, -1);
        }

        /// <summary>Gets a SystemMessage passing extra custom parameters to personalize.</summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <param name="customCaption">prefix for custom tags.</param>
        /// <param name="accessingUserID">UserID of the user accessing the system message.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <c>[Custom:n]</c>, where <c>n</c> is the zero based index which
        /// will be used to find the replacement value in <c>Custom</c> parameter.
        /// </remarks>
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList custom, string customCaption, int accessingUserID)
        {
            return GetSystemMessage(strLanguage, portalSettings, messageName, userInfo, resourceFile, custom, null, customCaption, accessingUserID);
        }

        /// <summary>Gets a SystemMessage passing extra custom parameters to personalize.</summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        /// <param name="customArray">An ArrayList with replacements for custom tags.</param>
        /// <param name="customDictionary">An IDictionary with replacements for custom tags.</param>
        /// <param name="customCaption">prefix for custom tags.</param>
        /// <param name="accessingUserID">UserID of the user accessing the system message.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <c>[Custom:n]</c>, where <c>n</c> is the zero based index which
        /// will be used to find the replacement value in <c>Custom</c> parameter.
        /// </remarks>
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList customArray, IDictionary customDictionary, string customCaption, int accessingUserID)
        {
            try
            {
                string strMessageValue = GetString(messageName, resourceFile, portalSettings, strLanguage);
                if (!string.IsNullOrEmpty(strMessageValue))
                {
                    if (string.IsNullOrEmpty(customCaption))
                    {
                        customCaption = "Custom";
                    }

                    var objTokenReplace = new TokenReplace(Scope.SystemMessages, strLanguage, portalSettings, userInfo);
                    if ((accessingUserID != -1) && (userInfo != null))
                    {
                        if (userInfo.UserID != accessingUserID)
                        {
                            objTokenReplace.AccessingUser =
                                UserController.Instance.GetUser(portalSettings.PortalId, accessingUserID);
                        }
                    }

                    if (customArray != null)
                    {
                        strMessageValue =
                            objTokenReplace.ReplaceEnvironmentTokens(strMessageValue, customArray, customCaption);
                    }
                    else
                    {
                        strMessageValue =
                            objTokenReplace.ReplaceEnvironmentTokens(strMessageValue, customDictionary, customCaption);
                    }
                }

                return strMessageValue;
            }
            catch (NullReferenceException ex)
            {
                Logger.Error(ex);
                return messageName;
            }
        }

        /// <summary>LoadCultureDropDownList loads a DropDownList with the list of supported cultures based on the languages defined in the supported locales file, for the current portal.</summary>
        /// <param name="list">DropDownList to load.</param>
        /// <param name="displayType">Format of the culture to display. Must be one of the <see cref="CultureDropDownTypes"/>> values. <see cref="CultureDropDownTypes" /> for list of allowable values.</param>
        /// <param name="selectedValue">Name of the default culture to select.</param>
        public static void LoadCultureDropDownList(DropDownList list, CultureDropDownTypes displayType, string selectedValue)
        {
			Logger.LogError(">>> Entering LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue})", list, displayType, selectedValue);
            LoadCultureDropDownList(list, displayType, selectedValue, string.Empty, false);
			Logger.LogWarning("<<< Exiting LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue})", list, displayType, selectedValue);
        }

        /// <summary>
        /// LoadCultureDropDownList loads a DropDownList with the list of supported cultures based on the languages defined in the supported locales file.
        /// <para>This overload allows us to display all installed languages. To do so, pass the value True to the <paramref name="loadHost"/> parameter.</para>
        /// </summary>
        /// <param name="list">DropDownList to load.</param>
        /// <param name="displayType">Format of the culture to display. Must be one the CultureDropDownTypes values. <see cref="CultureDropDownTypes" /> for list of allowable values.</param>
        /// <param name="selectedValue">Name of the default culture to select.</param>
        /// <param name="loadHost">Boolean that defines whether or not to load host (i.e. all available) locales.</param>
        public static void LoadCultureDropDownList(DropDownList list, CultureDropDownTypes displayType, string selectedValue, bool loadHost)
        {
			Logger.LogError(">>> Entering LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},loadHost={LoadHost})", list, displayType, selectedValue, loadHost);
            LoadCultureDropDownList(list, displayType, selectedValue, string.Empty, loadHost);
			Logger.LogWarning("<<< Exiting LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},loadHost={LoadHost})", list, displayType, selectedValue, loadHost);
        }

        /// <summary>
        ///   LoadCultureDropDownList loads a DropDownList with the list of supported cultures based on the languages defined in the supported locales file.
        ///   <para>This overload allows us to filter a language from the dropdown. To do so pass a language code to the Filter parameter.</para>
        ///   <para>This overload allows us to display all installed languages. To do so, pass the value True to the Host parameter.</para>
        /// </summary>
        /// <param name="list">DropDownList to load.</param>
        /// <param name="displayType">Format of the culture to display. Must be one the CultureDropDownTypes values. <see cref="CultureDropDownTypes" /> for list of allowable values.</param>
        /// <param name="selectedValue">Name of the default culture to select.</param>
        /// <param name="filter">String value that allows for filtering out a specific language.</param>
        /// <param name="host">Boolean that defines whether or not to load host (ie. all available) locales.</param>
        public static void LoadCultureDropDownList(DropDownList list, CultureDropDownTypes displayType, string selectedValue, string filter, bool host)
        {
			Logger.LogError(">>> Entering LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host})", list, displayType, selectedValue, filter, host);
            IEnumerable<ListItem> cultureListItems = LoadCultureInListItems(displayType, selectedValue, filter, host);

			Logger.LogError("LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): cultureListItems → {CultureListItems}", list, displayType, selectedValue, filter, host, cultureListItems);
            // add the items to the list
            foreach (var cultureItem in cultureListItems)
            {
                list.Items.Add(cultureItem);
            }

            // select the default item
            if (selectedValue != null)
            {
                ListItem item = list.Items.FindByValue(selectedValue);
				Logger.LogError("LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): item → {Item}", list, displayType, selectedValue, filter, host, item);
                if (item != null)
                {
                    list.SelectedIndex = -1;
					Logger.LogDebug("LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): list.SelectedIndex → {Arg5}", list, displayType, selectedValue, filter, host, list.SelectedIndex);
                    item.Selected = true;
					Logger.LogDebug("LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): item.Selected → {Arg5}", list, displayType, selectedValue, filter, host, item.Selected);
                }
            }
			Logger.LogWarning("<<< Exiting LoadCultureDropDownList(list={List},displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host})", list, displayType, selectedValue, filter, host);
        }

        /// <summary>
        /// LoadCultureDropDownList loads a DropDownList with the list of supported cultures based on the languages defined in the supported locales file.
        /// <para>This overload allows us to filter a language from the dropdown. To do so pass a language code to the Filter parameter.</para>
        /// <para>This overload allows us to display all installed languages. To do so, pass the value True to the Host parameter.</para>
        /// </summary>
        /// <param name="displayType">The <see cref="CultureDropDownTypes"/>.</param>
        /// <param name="selectedValue">The selected value.</param>
        /// <param name="filter">The value to exclude from the list.</param>
        /// <param name="host">Whether to get host-level locales or the locales for the current portal.</param>
        /// <returns>A sequence of new <see cref="ListItem"/> instances.</returns>
        public static IEnumerable<ListItem> LoadCultureInListItems(CultureDropDownTypes displayType, string selectedValue, string filter, bool host)
        {
			Logger.LogError(">>> Entering LoadCultureInListItems(displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host})", displayType, selectedValue, filter, host);
            var objPortalSettings = PortalController.Instance.GetCurrentSettings();
            Dictionary<string, Locale> enabledLanguages;
			Logger.LogError("LoadCultureInListItems(displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): objPortalSettings → {ObjPortalSettings}", displayType, selectedValue, filter, host, objPortalSettings);
            if (host)
            {
                enabledLanguages = LocaleController.Instance.GetLocales(Null.NullInteger);
				Logger.LogError("LoadCultureInListItems(displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): enabledLanguages → {EnabledLanguages}", displayType, selectedValue, filter, host, enabledLanguages);
            }
            else
            {
                enabledLanguages = LocaleController.Instance.GetLocales(objPortalSettings.PortalId);
				Logger.LogDebug("LoadCultureInListItems(displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): enabledLanguages → {EnabledLanguages}", displayType, selectedValue, filter, host, enabledLanguages);
            }

            var cultureListItems = new List<ListItem>(enabledLanguages.Count);
			Logger.LogError("LoadCultureInListItems(displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host}): cultureListItems → {CultureListItems}", displayType, selectedValue, filter, host, cultureListItems);
            foreach (KeyValuePair<string, Locale> kvp in enabledLanguages)
            {
                if (kvp.Value.Code != filter)
                {
                    cultureListItems.Add(new ListItem { Value = kvp.Value.Code, Text = GetLocaleName(kvp.Value.Code, displayType) });
                }
            }

			Logger.LogWarning("<<< Exiting LoadCultureInListItems(displayType={DisplayType},selectedValue={SelectedValue},filter={Filter},host={Host})", displayType, selectedValue, filter, host);
            return cultureListItems;
        }

        /// <summary>Localizes ModuleControl Titles.</summary>
        /// <param name="moduleControl">ModuleControl.</param>
        /// <returns>Localized control title if found.</returns>
        /// <remarks>
        /// Resource keys are: <c>ControlTitle_[key].Text</c>
        /// Key MUST be lowercase in the resource file
        /// Key can also be "blank" for admin/edit controls. These will only be used in admin pages.
        /// </remarks>
        public static string LocalizeControlTitle(IModuleControl moduleControl)
        {
			Logger.LogError(">>> Entering LocalizeControlTitle(moduleControl={ModuleControl})", moduleControl);
            string controlTitle = moduleControl.ModuleContext.Configuration.ModuleTitle;
            string controlKey = moduleControl.ModuleContext.Configuration.ModuleControl.ControlKey.ToLowerInvariant();
			Logger.LogError("LocalizeControlTitle(moduleControl={ModuleControl}): controlTitle → {ControlTitle}", moduleControl, controlTitle);

            if (!string.IsNullOrEmpty(controlKey))
            {
                string reskey = "ControlTitle_" + moduleControl.ModuleContext.Configuration.ModuleControl.ControlKey.ToLowerInvariant() + ".Text";
                string localizedvalue = GetString(reskey, moduleControl.LocalResourceFile);
                if (string.IsNullOrEmpty(localizedvalue))
                {
                    controlTitle = moduleControl.ModuleContext.Configuration.ModuleControl.ControlTitle;
                }
                else
                {
                    controlTitle = localizedvalue;
                }
            }
            else
            {
                bool isAdminPage = false;
				Logger.LogError("LocalizeControlTitle(moduleControl={ModuleControl}): isAdminPage → {IsAdminPage}", moduleControl, isAdminPage);

                // we should be checking that the tab path matches //Admin//pagename or //admin
                // in this way we should avoid partial matches (ie //Administrators
                if (PortalSettings.Current.ActiveTab.TabPath.StartsWith("//Admin//", StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(PortalSettings.Current.ActiveTab.TabPath, "//Admin", StringComparison.OrdinalIgnoreCase) ||
                    PortalSettings.Current.ActiveTab.TabPath.StartsWith("//Host//", StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(PortalSettings.Current.ActiveTab.TabPath, "//Host", StringComparison.OrdinalIgnoreCase))
                {
                    isAdminPage = true;
                }

                string reskey = "ControlTitle_.Text";
                string localizedvalue = GetString(reskey, moduleControl.LocalResourceFile);
                if (!string.IsNullOrEmpty(localizedvalue) && isAdminPage)
                {
                    controlTitle = localizedvalue;
                }
            }

			Logger.LogWarning("<<< Exiting LocalizeControlTitle(moduleControl={ModuleControl})", moduleControl);
            return controlTitle;
        }

        /// <summary>LocalizeDataGrid creates localized Headers for a DataGrid.</summary>
        /// <param name="grid">Grid to localize.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized text can be found.</param>
        public static void LocalizeDataGrid(ref DataGrid grid, string resourceFile)
        {
			Logger.LogError(">>> Entering LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile})", grid, resourceFile);
            string localizedText;
            foreach (DataGridColumn col in grid.Columns)
            {
                // Localize Header Text
                if (!string.IsNullOrEmpty(col.HeaderText))
                {
                    localizedText = GetString(col.HeaderText + ".Header", resourceFile);
					Logger.LogDebug("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): localizedText → {LocalizedText}", grid, resourceFile, localizedText);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
						Logger.LogError("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): localizedText → {LocalizedText}", grid, resourceFile, localizedText);
                        col.HeaderText = localizedText;
						Logger.LogDebug("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): col.HeaderText → {Arg2}", grid, resourceFile, col.HeaderText);
                    }
                }

                if (col is EditCommandColumn)
                {
                    var editCol = (EditCommandColumn)col;
					Logger.LogError("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): editCol → {EditCol}", grid, resourceFile, editCol);

                    // Edit Text - maintained for backward compatibility
                    localizedText = GetString(editCol.EditText + ".EditText", resourceFile);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
                        editCol.EditText = localizedText;
						Logger.LogDebug("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): editCol.EditText → {Arg2}", grid, resourceFile, editCol.EditText);
                    }

                    // Edit Text
                    localizedText = GetString(editCol.EditText, resourceFile);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
                        editCol.EditText = localizedText;
                    }

                    // Cancel Text
                    localizedText = GetString(editCol.CancelText, resourceFile);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
                        editCol.CancelText = localizedText;
						Logger.LogDebug("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): editCol.CancelText → {Arg2}", grid, resourceFile, editCol.CancelText);
                    }

                    // Update Text
                    localizedText = GetString(editCol.UpdateText, resourceFile);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
                        editCol.UpdateText = localizedText;
						Logger.LogDebug("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): editCol.UpdateText → {Arg2}", grid, resourceFile, editCol.UpdateText);
                    }
                }
                else if (col is ButtonColumn)
                {
                    var buttonCol = (ButtonColumn)col;
					Logger.LogError("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): buttonCol → {ButtonCol}", grid, resourceFile, buttonCol);

                    // Edit Text
                    localizedText = GetString(buttonCol.Text, resourceFile);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
                        buttonCol.Text = localizedText;
						Logger.LogDebug("LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile}): buttonCol.Text → {Arg2}", grid, resourceFile, buttonCol.Text);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting LocalizeDataGrid(grid={Grid},resourceFile={ResourceFile})", grid, resourceFile);
        }

        /// <summary>Localizes headers and fields on a <see cref="DetailsView"/> control.</summary>
        /// <param name="detailsView">The <see cref="DetailsView"/> to localize.</param>
        /// <param name="resourceFile">The root name of the resource file where the localized texts can be found.</param>
        public static void LocalizeDetailsView(ref DetailsView detailsView, string resourceFile)
        {
			Logger.LogError(">>> Entering LocalizeDetailsView(detailsView={DetailsView},resourceFile={ResourceFile})", detailsView, resourceFile);
            foreach (DataControlField field in detailsView.Fields)
            {
                LocalizeDataControlField(field, resourceFile);
            }
			Logger.LogWarning("<<< Exiting LocalizeDetailsView(detailsView={DetailsView},resourceFile={ResourceFile})", detailsView, resourceFile);
        }

        /// <summary>Localizes headers and fields on a GridView control.</summary>
        /// <param name="gridView">Grid to localize.</param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found.</param>
        public static void LocalizeGridView(ref GridView gridView, string resourceFile)
        {
			Logger.LogError(">>> Entering LocalizeGridView(gridView={GridView},resourceFile={ResourceFile})", gridView, resourceFile);
            foreach (DataControlField column in gridView.Columns)
            {
                LocalizeDataControlField(column, resourceFile);
            }
			Logger.LogWarning("<<< Exiting LocalizeGridView(gridView={GridView},resourceFile={ResourceFile})", gridView, resourceFile);
        }

        /// <summary>Localizes the "Built In" Roles.</summary>
        /// <remarks>This function looks for [Rolename.Role] key in GlobalResources.resx to see if this exists and
        /// if it does, it returns the localized description. Otherwise the role parameter is returned. I.e. it falls back
        /// to its original value.</remarks>
        /// <param name="role">The role name to localize.</param>
        /// <returns>The role name, potentially localized.</returns>
        public static string LocalizeRole(string role)
        {
			Logger.LogError(">>> Entering LocalizeRole(role={Role})", role);
            string localRole;
            string roleKey = role + ".Role";
            localRole = GetString(roleKey, GlobalResourceFile);
			Logger.LogDebug("LocalizeRole(role={Role}): localRole → {LocalRole}", role, localRole);
            if (string.IsNullOrEmpty(localRole))
            {
				Logger.LogError("LocalizeRole(role={Role}): localRole → {LocalRole}", role, localRole);
                return role;
            }

			Logger.LogWarning("<<< Exiting LocalizeRole(role={Role})", role);
            return localRole;
        }

        /// <summary>Gets the localized description for the "Built In" Roles.</summary>
        /// <remarks>This function looks for [Rolename.RoleDescription] key in GlobalResources.resx to see if this exists and
        /// if it does, it returns the localized description.</remarks>
        /// <param name="role">The role name to localize the description for.</param>
        /// <param name="fallback">The fallback description to use if the localized description is not found.</param>
        /// <returns>The localized description or null if not found.</returns>
        public static string LocalizeRoleDescription(string role, string fallback)
        {
			Logger.LogError(">>> Entering LocalizeRoleDescription(role={Role},fallback={Fallback})", role, fallback);
            string roleDescription;
            string roleKey = role + ".RoleDescription";
            roleDescription = GetString(roleKey, GlobalResourceFile);
			Logger.LogDebug("LocalizeRoleDescription(role={Role},fallback={Fallback}): roleDescription → {RoleDescription}", role, fallback, roleDescription);
            if (string.IsNullOrEmpty(roleDescription))
            {
				Logger.LogError("LocalizeRoleDescription(role={Role},fallback={Fallback}): roleDescription → {RoleDescription}", role, fallback, roleDescription);
                return fallback;
            }

			Logger.LogWarning("<<< Exiting LocalizeRoleDescription(role={Role},fallback={Fallback})", role, fallback);
            return roleDescription;
        }

        /// <summary>Remove the specified language from the specified portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="languageID">The language ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveLanguageFromPortal(int portalID, int languageID)
            => RemoveLanguageFromPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalID, languageID);

        /// <summary>Remove the specified language from the specified portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="languageId">The language ID.</param>
        public static void RemoveLanguageFromPortal(IEventLogger eventLogger, int portalId, int languageId)
            => RemoveLanguageFromPortal(eventLogger, portalId, languageId, false);

        /// <summary>Remove the specified language from the specified portal.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="languageID">The language ID.</param>
        /// <param name="isInstalling">Whether the site is installing.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveLanguageFromPortal(int portalID, int languageID, bool isInstalling)
            => RemoveLanguageFromPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalID, languageID, isInstalling);

        /// <summary>Remove the specified language from the specified portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="languageId">The language ID.</param>
        /// <param name="isInstalling">Whether the site is installing.</param>
        public static void RemoveLanguageFromPortal(IEventLogger eventLogger, int portalId, int languageId, bool isInstalling)
        {
			Logger.LogError(">>> Entering RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID})", portalID, languageID);
            if (!isInstalling)
            {
                var portalLocales = GetPortalLocalizations(portalId);
                if (portalLocales.Count <= 1)
                {
					Logger.LogError("Throwing PortalLocalizationRequiredException from RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID})", portalID, languageID);
				Logger.LogError("Throwing PortalLocalizationRequiredException from RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId})", eventLogger, portalId, languageId);
                    throw new PortalLocalizationRequiredException("You are trying to delete the only Portal localization entry in the system. This is NOT allowed!");
                }
            }
			Logger.LogError("Throwing PortalLocalizationRequiredException from RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling})", portalID, languageID, isInstalling);

			Logger.LogError("Throwing PortalLocalizationRequiredException from RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling})", eventLogger, portalId, languageId, isInstalling);
            var language = LocaleController.Instance.GetLocale(languageId);
            if (language != null)
            {
                if (Config.GetFriendlyUrlProvider() == "advanced")
                {
                    // only do this with Advanced URL Management
					Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID}): language → {Language}", portalID, languageID, language);
					Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId}): language → {Language}", eventLogger, portalId, languageId, language);
                    var portalInfo = PortalController.Instance.GetPortal(portalId);
                    if (portalInfo != null)
                    {
						Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling}): language → {Language}", portalID, languageID, isInstalling, language);
						Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling}): language → {Language}", eventLogger, portalId, languageId, isInstalling, language);
						Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID}): portalInfo → {PortalInfo}", portalID, languageID, portalInfo);
						Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId}): portalInfo → {PortalInfo}", eventLogger, portalId, languageId, portalInfo);
						Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling}): portalInfo → {PortalInfo}", portalID, languageID, isInstalling, portalInfo);
                        // check to see if this is the last extra language being added to the portal
                        var lastLanguage = LocaleController.Instance.GetLocales(portalId).Count == 2;

                        var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();
                        foreach (var portalAliasInfo in portalAliases)
                        {
							Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling}): portalInfo → {PortalInfo}", eventLogger, portalId, languageId, isInstalling, portalInfo);
                            if (portalAliasInfo.CultureCode == language.Code)
                            {
								Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID}): lastLanguage → {LastLanguage}", portalID, languageID, lastLanguage);
								Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId}): lastLanguage → {LastLanguage}", eventLogger, portalId, languageId, lastLanguage);
								Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling}): lastLanguage → {LastLanguage}", portalID, languageID, isInstalling, lastLanguage);
								Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling}): lastLanguage → {LastLanguage}", eventLogger, portalId, languageId, isInstalling, lastLanguage);
                                PortalAliasController.Instance.DeletePortalAlias(portalAliasInfo);
                            }

                            if (lastLanguage && portalAliasInfo.CultureCode == portalInfo.DefaultLanguage)
                            {
								Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID}): portalAliases → {PortalAliases}", portalID, languageID, portalAliases);
								Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId}): portalAliases → {PortalAliases}", eventLogger, portalId, languageId, portalAliases);
								Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling}): portalAliases → {PortalAliases}", portalID, languageID, isInstalling, portalAliases);
								Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling}): portalAliases → {PortalAliases}", eventLogger, portalId, languageId, isInstalling, portalAliases);
                                PortalAliasController.Instance.DeletePortalAlias(portalAliasInfo);

                                // Fix PortalSettings for the rest of this request
                                var newDefaultAlias = portalAliases.SingleOrDefault(a => a.IsPrimary && a.CultureCode == string.Empty);
                                if (newDefaultAlias != null)
                                {
									Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID}): newDefaultAlias → {NewDefaultAlias}", portalID, languageID, newDefaultAlias);
									Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId}): newDefaultAlias → {NewDefaultAlias}", eventLogger, portalId, languageId, newDefaultAlias);
									Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling}): newDefaultAlias → {NewDefaultAlias}", portalID, languageID, isInstalling, newDefaultAlias);
                                    PortalSettings.Current?.PortalAlias = newDefaultAlias;
                                }
                            }
                        }
                    }
                }

                // Get Translator Role
                string roleName = $"Translator ({language.Code})";
                RoleInfo role = RoleController.Instance.GetRole(portalId, r => r.RoleName == roleName);

                if (role != null)
                {
					Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID}): role → {Role}", portalID, languageID, role);
					Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId}): role → {Role}", eventLogger, portalId, languageId, role);
					Logger.LogError("RemoveLanguageFromPortal(portalID={PortalID},languageID={LanguageID},isInstalling={IsInstalling}): role → {Role}", portalID, languageID, isInstalling, role);
					Logger.LogError("RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling}): role → {Role}", eventLogger, portalId, languageId, isInstalling, role);
                    // Remove Translator Role from Portal
                    RoleController.Instance.DeleteRole(role);
                }

                DataProvider.Instance().DeletePortalLanguages(portalId, languageId);
                eventLogger.AddLog(
                    "portalID/languageID",
                    $"{portalId}/{languageId}",
                    PortalController.Instance.GetCurrentSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogType.LANGUAGETOPORTAL_DELETED);

                DataCache.ClearPortalCache(portalId, false);
            }
			Logger.LogWarning("<<< Exiting RemoveLanguageFromPortal(eventLogger={EventLogger},portalId={PortalId},languageId={LanguageId},isInstalling={IsInstalling})", eventLogger, portalId, languageId, isInstalling);
        }

        /// <summary>Removes the specified language from all portals.</summary>
        /// <param name="languageId">The language ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveLanguageFromPortals(int languageId)
            => RemoveLanguageFromPortals(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), languageId);

        /// <summary>Removes the specified language from all portals.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="languageId">The language ID.</param>
        public static void RemoveLanguageFromPortals(IEventLogger eventLogger, int languageId)
            => RemoveLanguageFromPortals(eventLogger, languageId, false);

        /// <summary>Removes the specified language from all portals.</summary>
        /// <param name="languageId">The language ID.</param>
        /// <param name="isInstalling">Whether the site is installing.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveLanguageFromPortals(int languageId, bool isInstalling)
            => RemoveLanguageFromPortals(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), languageId, isInstalling);

        /// <summary>Removes the specified language from all portals.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="languageId">The language ID.</param>
        /// <param name="isInstalling">Whether the site is installing.</param>
        public static void RemoveLanguageFromPortals(IEventLogger eventLogger, int languageId, bool isInstalling)
        {
			Logger.LogError(">>> Entering RemoveLanguageFromPortals(languageId={LanguageId})", languageId);
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                RemoveLanguageFromPortal(eventLogger, portal.PortalId, languageId, isInstalling);
            }
        }

        /// <summary>Removes all languages from the specified portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void RemoveLanguagesFromPortal(int portalId)
            => RemoveLanguagesFromPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), portalId);

        /// <summary>Removes all languages from the specified portal.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void RemoveLanguagesFromPortal(IEventLogger eventLogger, int portalId)
        {
			Logger.LogError(">>> Entering RemoveLanguagesFromPortal(portalId={PortalId})", portalId);
            foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values)
            {
                RemoveLanguageFromPortal(eventLogger, portalId, locale.LanguageId);
            }
			Logger.LogWarning("<<< Exiting RemoveLanguagesFromPortal(eventLogger={EventLogger},portalId={PortalId})", eventLogger, portalId);
        }

        /// <summary>Adds or updates the language.</summary>
        /// <param name="locale">The language/locale.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void SaveLanguage(Locale locale)
            => SaveLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), locale);

        /// <summary>Adds or updates the language.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="locale">The language/locale.</param>
        public static void SaveLanguage(IEventLogger eventLogger, Locale locale)
            => SaveLanguage(eventLogger, locale, true);

        /// <summary>Adds or updates the language.</summary>
        /// <param name="locale">The language/locale.</param>
        /// <param name="clearCache">Whether to clear the cache afterwards.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void SaveLanguage(Locale locale, bool clearCache)
            => SaveLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), locale, clearCache);

        /// <summary>Adds or updates the language.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="locale">The language/locale.</param>
        /// <param name="clearCache">Whether to clear the cache afterwards.</param>
        public static void SaveLanguage(IEventLogger eventLogger, Locale locale, bool clearCache)
        {
			Logger.LogError(">>> Entering SaveLanguage(locale={Locale})", locale);
            if (locale.LanguageId == Null.NullInteger)
            {
                locale.LanguageId = DataProvider.Instance().AddLanguage(locale.Code, locale.Text, locale.Fallback, UserController.Instance.GetCurrentUserInfo().UserID);
                eventLogger.AddLog(locale, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.LANGUAGE_CREATED);
            }
            else
            {
                DataProvider.Instance().UpdateLanguage(locale.LanguageId, locale.Code, locale.Text, locale.Fallback, UserController.Instance.GetCurrentUserInfo().UserID);
                eventLogger.AddLog(locale, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.LANGUAGE_UPDATED);
            }

            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }
			Logger.LogWarning("<<< Exiting SaveLanguage(eventLogger={EventLogger},locale={Locale},clearCache={ClearCache})", eventLogger, locale, clearCache);
        }

        public static void SetLanguage(string value)
        {
			Logger.LogError(">>> Entering SetLanguage(value={Value})", value);
            try
            {
                var response = HttpContext.Current == null ? null : HttpContext.Current.Response;
				Logger.LogError("SetLanguage(value={Value}): response → {Response}", value, response);
                if (response == null)
                {
                    return;
                }

                // save the page culture as a cookie
                HttpCookie cookie = response.Cookies.Get("language");
				Logger.LogError("SetLanguage(value={Value}): cookie → {Cookie}", value, cookie);
                if (cookie == null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        cookie = new HttpCookie("language", value) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" };
                        response.Cookies.Add(cookie);
                    }
                }
                else
                {
                    cookie.Value = value;
					Logger.LogDebug("SetLanguage(value={Value}): cookie.Value → {Arg1}", value, cookie.Value);
                    if (!string.IsNullOrEmpty(value))
                    {
                        response.Cookies.Set(cookie);
                    }
                    else
                    {
                        response.Cookies.Remove("language");
                    }
                }
            }
            catch
            {
				Logger.LogError("Exception in SetLanguage(value={Value}): {Arg1}", value, ex.Message);
				Logger.LogWarning("<<< Exiting SetLanguage(value={Value})", value);
                return;
            }
        }

        /// <summary>Sets the culture codes on the current Thread.</summary>
        /// <param name="cultureInfo">Culture Info for the current page.</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <remarks>
        ///   This method will configure the Thread culture codes.  Any page which does not derive from <see cref="PageBase" /> should
        ///   be sure to call this method in <see cref="Control.OnInit" /> to ensure localization works correctly.
        /// </remarks>
        [DnnDeprecated(9, 8, 0, "Use overload taking IPortalSettings instead")]
        public static partial void SetThreadCultures(CultureInfo cultureInfo, PortalSettings portalSettings)
        {
            SetThreadCultures(cultureInfo, (IPortalSettings)portalSettings);
			Logger.LogError(">>> Entering SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings})", cultureInfo, portalSettings);
			Logger.LogError("Throwing ArgumentNullException from SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings})", cultureInfo, portalSettings);
			Logger.LogError("SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings}): cultureInfo → {CultureInfo1}", cultureInfo, portalSettings, cultureInfo);
			Logger.LogWarning("<<< Exiting SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings})", cultureInfo, portalSettings);
        }

        /// <summary>Sets the culture codes on the current Thread.</summary>
        /// <param name="cultureInfo">Culture Info for the current page.</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <remarks>
        ///   This method will configure the Thread culture codes.  Any page which does not derive from <see cref="PageBase" /> should
        ///   be sure to call this method in <see cref="Control.OnInit" /> to ensure localization works correctly.
        /// </remarks>
        public static void SetThreadCultures(CultureInfo cultureInfo, IPortalSettings portalSettings)
        {
            if (cultureInfo == null)
            {
				Logger.LogError("Throwing ArgumentNullException from SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings})", cultureInfo, portalSettings);
                throw new ArgumentNullException(nameof(cultureInfo));
            }

            if (cultureInfo.Name == "fa-IR")
            {
                cultureInfo = Persian.PersianController.GetPersianCultureInfo();
            }

            Thread.CurrentThread.CurrentCulture = cultureInfo;

            if (portalSettings != null && portalSettings.ContentLocalizationEnabled &&
                        HttpContext.Current.Request.IsAuthenticated &&
                        portalSettings.AllowUserUICulture)
            {
                Thread.CurrentThread.CurrentUICulture = GetUserUICulture(cultureInfo, portalSettings);
            }
            else
            {
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
            }
        }

        /// <summary>
        /// Maps the culture code string into the corresponding language ID in the
        /// database. In case there is no language defined in the system with the
        /// passed code, -1 (<see cref="Null.NullInteger"/>) is returned.
        /// </summary>
        /// <param name="cultureCode">The culture to get the language ID for.</param>
        /// <returns>Language ID integer.</returns>
        public static int GetCultureLanguageID(string cultureCode)
        {
			Logger.LogError(">>> Entering GetCultureLanguageID(cultureCode={CultureCode})", cultureCode);
            var locale = LocaleController.Instance.GetLocale(cultureCode);
			Logger.LogError("GetCultureLanguageID(cultureCode={CultureCode}): locale → {Locale}", cultureCode, locale);
			Logger.LogWarning("<<< Exiting GetCultureLanguageID(cultureCode={CultureCode})", cultureCode);
            return locale != null ? locale.LanguageId : Null.NullInteger;
        }

        public string GetFixedCurrency(decimal expression, string culture, int numDigitsAfterDecimal)
        {
			Logger.LogError(">>> Entering GetFixedCurrency(expression={Expression},culture={Culture},numDigitsAfterDecimal={NumDigitsAfterDecimal})", expression, culture, numDigitsAfterDecimal);
            string oldCurrentCulture = this.CurrentUICulture;
            var newCulture = new CultureInfo(culture);
			Logger.LogError("GetFixedCurrency(expression={Expression},culture={Culture},numDigitsAfterDecimal={NumDigitsAfterDecimal}): newCulture → {NewCulture}", expression, culture, numDigitsAfterDecimal, newCulture);
            Thread.CurrentThread.CurrentUICulture = newCulture;
            string currencyStr = expression.ToString(newCulture.NumberFormat.CurrencySymbol, newCulture);
            var oldCulture = new CultureInfo(oldCurrentCulture);
			Logger.LogError("GetFixedCurrency(expression={Expression},culture={Culture},numDigitsAfterDecimal={NumDigitsAfterDecimal}): oldCulture → {OldCulture}", expression, culture, numDigitsAfterDecimal, oldCulture);
            Thread.CurrentThread.CurrentUICulture = oldCulture;
			Logger.LogWarning("<<< Exiting GetFixedCurrency(expression={Expression},culture={Culture},numDigitsAfterDecimal={NumDigitsAfterDecimal})", expression, culture, numDigitsAfterDecimal);
            return currencyStr;
        }

        public string GetFixedDate(DateTime expression, string culture)
        {
			Logger.LogError(">>> Entering GetFixedDate(expression={Expression},culture={Culture})", expression, culture);
            string oldCurrentCulture = this.CurrentUICulture;
            var newCulture = new CultureInfo(culture);
			Logger.LogError("GetFixedDate(expression={Expression},culture={Culture}): newCulture → {NewCulture}", expression, culture, newCulture);
            Thread.CurrentThread.CurrentUICulture = newCulture;
            string dateStr = expression.ToString(newCulture.DateTimeFormat.FullDateTimePattern, newCulture);
            var oldCulture = new CultureInfo(oldCurrentCulture);
			Logger.LogError("GetFixedDate(expression={Expression},culture={Culture}): oldCulture → {OldCulture}", expression, culture, oldCulture);
            Thread.CurrentThread.CurrentUICulture = oldCulture;
			Logger.LogWarning("<<< Exiting GetFixedDate(expression={Expression},culture={Culture})", expression, culture);
            return dateStr;
        }

        /// <summary>
        /// Parses the language parameter into a valid and enabled language in the current portal.
        /// If an exact match is not found (language-region), it will try to find a match for the language only.
        /// Ex: requested locale is "en-GB", requested language is "en", enabled locale is "en-US", so "en" is a match for "en-US".
        /// </summary>
        /// <param name="portalId">ID of current portal.</param>
        /// <param name="language">Language to be parsed.</param>
        /// <returns>A valid and enabled CultureInfo that matches the language passed if any.</returns>
        internal static CultureInfo GetCultureFromString(int portalId, string language)
        {
			Logger.LogError(">>> Entering GetCultureFromString(portalId={PortalId},language={Language})", portalId, language);
            CultureInfo culture = null;
			Logger.LogError("GetCultureFromString(portalId={PortalId},language={Language}): culture → {Culture}", portalId, language, culture);
            if (!string.IsNullOrEmpty(language))
            {
                if (LocaleController.Instance.IsEnabled(ref language, portalId))
                {
                    culture = new CultureInfo(language);
                }
                else
                {
                    string preferredLanguage = language.Split('-')[0];

                    Dictionary<string, Locale> enabledLocales = new Dictionary<string, Locale>();
                    if (portalId > Null.NullInteger)
                    {
                        enabledLocales = LocaleController.Instance.GetLocales(portalId);
						Logger.LogError("GetCultureFromString(portalId={PortalId},language={Language}): enabledLocales → {EnabledLocales}", portalId, language, enabledLocales);
                    }

                    foreach (string localeCode in enabledLocales.Keys)
                    {
                        if (localeCode.Split('-')[0] == preferredLanguage.Split('-')[0])
                        {
                            culture = new CultureInfo(localeCode);
                            break;
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetCultureFromString(portalId={PortalId},language={Language})", portalId, language);
            return culture;
        }

        private static void LocalizeDataControlField(DataControlField controlField, string resourceFile)
        {
			Logger.LogError(">>> Entering LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile})", controlField, resourceFile);
            string localizedText;

            // Localize Header Text
            if (!string.IsNullOrEmpty(controlField.HeaderText))
            {
                localizedText = GetString(controlField.HeaderText + ".Header", resourceFile);
				Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): localizedText → {LocalizedText}", controlField, resourceFile, localizedText);
                if (!string.IsNullOrEmpty(localizedText))
                {
					Logger.LogError("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): localizedText → {LocalizedText}", controlField, resourceFile, localizedText);
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): controlField.AccessibleHeaderText → {Arg2}", controlField, resourceFile, controlField.AccessibleHeaderText);
                    controlField.HeaderText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): controlField.HeaderText → {Arg2}", controlField, resourceFile, controlField.HeaderText);
                    controlField.AccessibleHeaderText = controlField.HeaderText;
                }
            }

            if (controlField is TemplateField)
            {
                // do nothing
            }
            else if (controlField is ButtonField)
            {
                var button = (ButtonField)controlField;
				Logger.LogError("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): button → {Button}", controlField, resourceFile, button);
                localizedText = GetString(button.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    button.Text = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): button.Text → {Arg2}", controlField, resourceFile, button.Text);
                }
            }
            else if (controlField is CheckBoxField)
            {
                var checkbox = (CheckBoxField)controlField;
				Logger.LogError("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): checkbox → {Checkbox}", controlField, resourceFile, checkbox);
                localizedText = GetString(checkbox.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    checkbox.Text = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): checkbox.Text → {Arg2}", controlField, resourceFile, checkbox.Text);
                }
            }
            else if (controlField is CommandField)
            {
                var commands = (CommandField)controlField;
				Logger.LogError("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands → {Commands}", controlField, resourceFile, commands);
                localizedText = GetString(commands.CancelText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.CancelText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.CancelText → {Arg2}", controlField, resourceFile, commands.CancelText);
                }

                localizedText = GetString(commands.DeleteText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.DeleteText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.DeleteText → {Arg2}", controlField, resourceFile, commands.DeleteText);
                }

                localizedText = GetString(commands.EditText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.EditText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.EditText → {Arg2}", controlField, resourceFile, commands.EditText);
                }

                localizedText = GetString(commands.InsertText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.InsertText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.InsertText → {Arg2}", controlField, resourceFile, commands.InsertText);
                }

                localizedText = GetString(commands.NewText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.NewText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.NewText → {Arg2}", controlField, resourceFile, commands.NewText);
                }

                localizedText = GetString(commands.SelectText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.SelectText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.SelectText → {Arg2}", controlField, resourceFile, commands.SelectText);
                }

                localizedText = GetString(commands.UpdateText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.UpdateText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): commands.UpdateText → {Arg2}", controlField, resourceFile, commands.UpdateText);
                }
            }
            else if (controlField is HyperLinkField)
            {
                var link = (HyperLinkField)controlField;
				Logger.LogError("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): link → {Link}", controlField, resourceFile, link);
                localizedText = GetString(link.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    link.Text = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): link.Text → {Arg2}", controlField, resourceFile, link.Text);
                }
            }
            else if (controlField is ImageField)
            {
                var image = (ImageField)controlField;
				Logger.LogError("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): image → {Image}", controlField, resourceFile, image);
                localizedText = GetString(image.AlternateText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    image.AlternateText = localizedText;
					Logger.LogDebug("LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile}): image.AlternateText → {Arg2}", controlField, resourceFile, image.AlternateText);
                }
            }
			Logger.LogWarning("<<< Exiting LocalizeDataControlField(controlField={ControlField},resourceFile={ResourceFile})", controlField, resourceFile);
        }

        private static void AddLanguageHttpAlias(int portalId, Locale locale)
        {
			Logger.LogError(">>> Entering AddLanguageHttpAlias(portalId={PortalId},locale={Locale})", portalId, locale);
            if (Config.GetFriendlyUrlProvider() == "advanced")
            {
                // create new HTTPAlias for language
                var portalInfo = PortalController.Instance.GetPortal(portalId);
				Logger.LogError("AddLanguageHttpAlias(portalId={PortalId},locale={Locale}): portalInfo → {PortalInfo}", portalId, locale, portalInfo);
                PortalAliasInfo currentAlias = null;
                string httpAlias = null;
				Logger.LogError("AddLanguageHttpAlias(portalId={PortalId},locale={Locale}): httpAlias → {HttpAlias}", portalId, locale, httpAlias);

                var portalAliasses = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
				Logger.LogError("AddLanguageHttpAlias(portalId={PortalId},locale={Locale}): portalAliasses → {PortalAliasses}", portalId, locale, portalAliasses);
                var portalAliasInfos = portalAliasses as IList<PortalAliasInfo> ?? portalAliasses.ToList();
				Logger.LogError("AddLanguageHttpAlias(portalId={PortalId},locale={Locale}): portalAliasInfos → {PortalAliasInfos}", portalId, locale, portalAliasInfos);
                if (portalAliasses != null && portalAliasInfos.Any())
                {
                    currentAlias = currentAlias
                        ?? portalAliasInfos
                            .Where(a => string.IsNullOrWhiteSpace(a.CultureCode))
                            .OrderByDescending(a => a.IsPrimary)
                            .FirstOrDefault()
                        ?? portalAliasInfos.First();

                    httpAlias = currentAlias.HTTPAlias;
                }

                if (currentAlias != null && !string.IsNullOrEmpty(httpAlias) && portalInfo != null)
                {
                    if (!string.IsNullOrEmpty(currentAlias.CultureCode))
                    {
                        // the portal alias is culture specific
                        if (currentAlias.CultureCode == portalInfo.CultureCode)
                        {
                            // remove the culture from the alias
                            httpAlias = httpAlias.Substring(0, httpAlias.LastIndexOf("/", StringComparison.Ordinal));
                        }
                    }

                    var alias = GetValidLanguageUrl(portalId, httpAlias, locale.Code.ToLowerInvariant());
					Logger.LogError("AddLanguageHttpAlias(portalId={PortalId},locale={Locale}): alias → {Alias}", portalId, locale, alias);
                    if (!string.IsNullOrEmpty(alias))
                    {
                        var newAlias = new PortalAliasInfo(currentAlias)
                        {
                            IsPrimary = true,
                            CultureCode = locale.Code,
                            HTTPAlias = GetValidLanguageUrl(portalId, httpAlias, locale.Code.ToLowerInvariant()),
                        };

                        PortalAliasController.Instance.AddPortalAlias(newAlias);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting AddLanguageHttpAlias(portalId={PortalId},locale={Locale})", portalId, locale);
        }

        private static string GetValidLanguageUrl(int portalId, string httpAlias, string locale)
        {
			Logger.LogError(">>> Entering GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale})", portalId, httpAlias, locale);
            string alias;

            bool isValid;
            var counter = 0;
            do
            {
                var modifiedLocale = locale;
                if (counter > 0)
                {
                    modifiedLocale += counter.ToString(CultureInfo.InvariantCulture);
                }

                alias = $"{httpAlias}/{modifiedLocale}";
				Logger.LogError("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): alias → {Alias}", portalId, httpAlias, locale, alias);
				Logger.LogDebug("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): alias → {Alias}", portalId, httpAlias, locale, alias);

                var tab = TabController.Instance.GetTabByName(modifiedLocale, portalId);
				Logger.LogError("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): tab → {Tab}", portalId, httpAlias, locale, tab);
                isValid = tab == null;
				Logger.LogError("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): isValid → {IsValid}", portalId, httpAlias, locale, isValid);

                if (isValid)
                {
                    var user = UserController.GetUserByVanityUrl(portalId, modifiedLocale);
					Logger.LogError("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): user → {User}", portalId, httpAlias, locale, user);
                    isValid = user == null;
                }

                if (isValid)
                {
                    var aliases = PortalAliasController.Instance.GetPortalAliases();
					Logger.LogError("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): aliases → {Aliases}", portalId, httpAlias, locale, aliases);
                    isValid = !aliases.Contains(alias);
                }
				Logger.LogDebug("GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale}): isValid → {IsValid}", portalId, httpAlias, locale, isValid);

                if (isValid)
                {
                    isValid = PortalAliasController.ValidateAlias(alias, false);
                }

                counter++;
            }
            while (!isValid);

			Logger.LogWarning("<<< Exiting GetValidLanguageUrl(portalId={PortalId},httpAlias={HttpAlias},locale={Locale})", portalId, httpAlias, locale);
            return alias;
        }

        /// <summary>Tries to get a valid language from the querystring.</summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo if any is found.</returns>
        private static CultureInfo GetCultureFromQs(IPortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetCultureFromQs(portalSettings={PortalSettings})", portalSettings);
            if (HttpContext.Current == null || HttpContext.Current.Request["language"] == null)
            {
                return null;
            }

            string language = HttpContext.Current.Request["language"];
			Logger.LogError("GetCultureFromQs(portalSettings={PortalSettings}): language → {Language}", portalSettings, language);
            CultureInfo culture = GetCultureFromString(portalSettings.PortalId, language);
			Logger.LogError("GetCultureFromQs(portalSettings={PortalSettings}): culture → {Culture}", portalSettings, culture);
			Logger.LogWarning("<<< Exiting GetCultureFromQs(portalSettings={PortalSettings})", portalSettings);
            return culture;
        }

        /// <summary>Tries to get a valid language from the cookie.</summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo if any is found.</returns>
        private static CultureInfo GetCultureFromCookie(IPortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetCultureFromCookie(portalSettings={PortalSettings})", portalSettings);
            CultureInfo culture;
            if (HttpContext.Current == null || HttpContext.Current.Request.Cookies["language"] == null)
            {
                return null;
            }

            string language = HttpContext.Current.Request.Cookies["language"].Value;
			Logger.LogError("GetCultureFromCookie(portalSettings={PortalSettings}): language → {Language}", portalSettings, language);
            culture = GetCultureFromString(portalSettings.PortalId, language);
			Logger.LogError("GetCultureFromCookie(portalSettings={PortalSettings}): culture → {Culture}", portalSettings, culture);
			Logger.LogDebug("GetCultureFromCookie(portalSettings={PortalSettings}): culture → {Culture}", portalSettings, culture);
			Logger.LogWarning("<<< Exiting GetCultureFromCookie(portalSettings={PortalSettings})", portalSettings);
            return culture;
        }

        /// <summary>Tries to get a valid language from the user profile.</summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo if any is found.</returns>
        private static CultureInfo GetCultureFromProfile(IPortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetCultureFromProfile(portalSettings={PortalSettings})", portalSettings);
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

			Logger.LogError("GetCultureFromProfile(portalSettings={PortalSettings}): objUserInfo → {ObjUserInfo}", portalSettings, objUserInfo);
            if (HttpContext.Current == null || !HttpContext.Current.Request.IsAuthenticated || objUserInfo.UserID == -1)
            {
                return null;
            }

            string language = objUserInfo.Profile.PreferredLocale;
            CultureInfo culture = GetCultureFromString(portalSettings.PortalId, language);
			Logger.LogError("GetCultureFromProfile(portalSettings={PortalSettings}): culture → {Culture}", portalSettings, culture);
			Logger.LogWarning("<<< Exiting GetCultureFromProfile(portalSettings={PortalSettings})", portalSettings);
            return culture;
        }

        /// <summary>Tries to get a valid language from the browser preferences if the portal has the setting to use browser languages enabled.</summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo if any is found.</returns>
        private static CultureInfo GetCultureFromBrowser(IPortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetCultureFromBrowser(portalSettings={PortalSettings})", portalSettings);
            if (!portalSettings.EnableBrowserLanguage)
            {
                return null;
            }
            else
            {
				Logger.LogWarning("<<< Exiting GetCultureFromBrowser(portalSettings={PortalSettings})", portalSettings);
                return GetBrowserCulture(portalSettings.PortalId);
            }
        }

        /// <summary>Tries to get a valid language from the portal default preferences.</summary>
        /// <param name="portalSettings">Current PortalSettings.</param>
        /// <returns>A valid CultureInfo if any is found.</returns>
        private static CultureInfo GetCultureFromPortal(IPortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetCultureFromPortal(portalSettings={PortalSettings})", portalSettings);
            CultureInfo culture = null;
			Logger.LogError("GetCultureFromPortal(portalSettings={PortalSettings}): culture → {Culture}", portalSettings, culture);
            if (!string.IsNullOrEmpty(portalSettings.DefaultLanguage))
            {
                // As the portal default language can never be disabled, we know this language is available and enabled
                culture = new CultureInfo(portalSettings.DefaultLanguage);
            }
            else
            {
                // Get the first enabled locale on the portal
                Dictionary<string, Locale> enabledLocales = new Dictionary<string, Locale>();
                if (portalSettings.PortalId > Null.NullInteger)
                {
                    enabledLocales = LocaleController.Instance.GetLocales(portalSettings.PortalId);
					Logger.LogError("GetCultureFromPortal(portalSettings={PortalSettings}): enabledLocales → {EnabledLocales}", portalSettings, enabledLocales);
                }

                if (enabledLocales.Count > 0)
                {
                    foreach (string localeCode in enabledLocales.Keys)
                    {
                        culture = new CultureInfo(localeCode);
                        break;
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetCultureFromPortal(portalSettings={PortalSettings})", portalSettings);
            return culture;
        }

        private static List<object> GetPortalLocalizations(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalLocalizations(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting GetPortalLocalizations(portalId={PortalId})", portalId);
            return CBO.FillCollection<object>(DataProvider.Instance().GetPortalLocalizations(portalId));
        }

        /// <summary>
        /// When portal allows users to select their preferred UI language, this method
        /// will return the user UI preferred language if defined; otherwise, defaults
        /// to the current culture.
        /// </summary>
        /// <param name="currentCulture">Current culture.</param>
        /// <param name="portalSettings">Portal settings for the current request.</param>
        /// <returns>A <see cref="CultureInfo"/> instance representing the user's UI culture.</returns>
        private static CultureInfo GetUserUICulture(CultureInfo currentCulture, IPortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetUserUICulture(currentCulture={CurrentCulture},portalSettings={PortalSettings})", currentCulture, portalSettings);
            CultureInfo uiCulture = currentCulture;
			Logger.LogError("GetUserUICulture(currentCulture={CurrentCulture},portalSettings={PortalSettings}): uiCulture → {UiCulture}", currentCulture, portalSettings, uiCulture);
            try
            {
                object oCulture = Personalization.Personalization.GetProfile("Usability", "UICulture");
				Logger.LogError("GetUserUICulture(currentCulture={CurrentCulture},portalSettings={PortalSettings}): oCulture → {OCulture}", currentCulture, portalSettings, oCulture);
                if (oCulture != null)
                {
                    CultureInfo ci = GetCultureFromString(portalSettings.PortalId, oCulture.ToString());
					Logger.LogError("GetUserUICulture(currentCulture={CurrentCulture},portalSettings={PortalSettings}): ci → {Ci}", currentCulture, portalSettings, ci);
                    if (ci != null)
                    {
                        uiCulture = ci;
                    }
                }
            }
            catch
            {
				Logger.LogError("Exception in GetUserUICulture(currentCulture={CurrentCulture},portalSettings={PortalSettings}): {Arg2}", currentCulture, portalSettings, ex.Message);
                // UserCulture seems not valid anymore, update to current culture
                Personalization.Personalization.SetProfile("Usability", "UICulture", currentCulture.Name);
            }

			Logger.LogWarning("<<< Exiting GetUserUICulture(currentCulture={CurrentCulture},portalSettings={PortalSettings})", currentCulture, portalSettings);
            return uiCulture;
        }
    }
}
