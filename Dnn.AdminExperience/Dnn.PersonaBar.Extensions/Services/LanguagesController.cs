// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.SiteSettings.Components;
    using Dnn.PersonaBar.SiteSettings.Components.Constants;
    using Dnn.PersonaBar.SiteSettings.Services.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Admin)]
    public class LanguagesController : PersonaBarApiController
    {
        /// <summary>A regular expression which matches a file name for a resource file.</summary>
        /// <remarks>
        /// Sample matches:
        /// <list type="bullet">
        ///     <item><description><c>MyResources.ascx.en-US.resx</c></description></item>
        ///     <item><description><c>MyResources.ascx.en-US.Host.resx</c></description></item>
        ///     <item><description><c>MyResources.ascx.en-US.Portal-123.resx</c></description></item>
        /// </list>
        /// </remarks>
        internal static readonly Regex FileInfoRegex = new Regex(
            @"\.([a-z]{2,3}\-[0-9A-Z]{2,4}(-[A-Z]{2})?)(\.(Host|Portal-\d+))?\.resx$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private const string LocalResourcesFile = "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.SiteSettings/App_LocalResources/SiteSettings.resx";

        private const string AuthFailureMessage = "Authorization has been denied for this request.";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesController));

        private static readonly string[] RootFolders =
        [
            "Admin",
            "Controls",
            "DesktopModules",
            "Install",
            "Providers",
        ];

        private ITabController tabController = TabController.Instance;
        private ILocaleController localeController = LocaleController.Instance;
        private IPortalController portalController = PortalController.Instance;

        private string selectedResourceFile;

        public LanguagesController(INavigationManager navigationManager)
        {
			Logger.LogError(">>> Entering LanguagesController(navigationManager={NavigationManager})", navigationManager);
            this.NavigationManager = navigationManager;
        }

        protected INavigationManager NavigationManager { get; }

        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [P] to expand source code folding

        // GET /api/personabar/pages/GetTabsForTranslation?portalId=&cultureCode=fr-FR
        [HttpGet]
        public HttpResponseMessage GetTabsForTranslation(int? portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetTabsForTranslation(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var nonTranslatedTabs = this.GetTabsForTranslationInternal(pid, cultureCode);
				Logger.LogError("GetTabsForTranslation(portalId={PortalId},cultureCode={CultureCode}): nonTranslatedTabs → {NonTranslatedTabs}", portalId, cultureCode, nonTranslatedTabs);
                return this.Request.CreateResponse(HttpStatusCode.OK, nonTranslatedTabs);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetTabsForTranslation(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET /api/personabar/languages/GetRootResourcesFolders
        [HttpGet]
        public HttpResponseMessage GetRootResourcesFolders()
        {
			Logger.LogError(">>> Entering GetRootResourcesFolders()");
            try
            {
                var folders = new List<KeyValuePair<string, string>>();
				Logger.LogError("GetRootResourcesFolders(): folders → {Folders}", folders);
                var server = HttpContext.Current.Server;

                folders.Add(new KeyValuePair<string, string>(LocalizeString("LocalResources"), "_"));
                folders.Add(new KeyValuePair<string, string>(LocalizeString("GlobalResources"), server.MapPath("~/App_GlobalResources")));
                folders.Add(new KeyValuePair<string, string>(LocalizeString("SiteTemplates"), server.MapPath("~/Portals/_default")));

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        Folders = folders.MapEntries(),
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetRootResourcesFolders()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET /api/personabar/languages/GetSubRootResources?currentFolder=DesktopModules
        [HttpGet]
        public HttpResponseMessage GetSubRootResources(string currentFolder = null)
        {
			Logger.LogError(">>> Entering GetSubRootResources(currentFolder={CurrentFolder})", currentFolder);
            try
            {
                var folders = new List<KeyValuePair<string, string>>();
				Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): folders → {Folders}", currentFolder, folders);
                var files = new List<KeyValuePair<string, string>>();
				Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): files → {Files}", currentFolder, files);
                var server = HttpContext.Current.Server;

                if (string.IsNullOrEmpty(currentFolder))
                {
                    folders.AddRange(RootFolders.Select(s => new KeyValuePair<string, string>(s, server.MapPath("~/" + "_/" + s))));

                    const string skins = "Skins";
                    var skinsPath = Path.Combine(Globals.ApplicationMapPath, skins);
					Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): skinsPath → {SkinsPath}", currentFolder, skinsPath);

                    if (Directory.Exists(skinsPath) && HasLocalResources(skinsPath))
                    {
                        folders.Add(new KeyValuePair<string, string>(LocalizeString("HostSkins"), skinsPath));
                    }

                    var portalSkinFolder = Path.Combine(this.PortalSettings.HomeSystemDirectoryMapPath, skins);
					Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): portalSkinFolder → {PortalSkinFolder}", currentFolder, portalSkinFolder);
                    if (Directory.Exists(portalSkinFolder) &&
                        this.PortalSettings.ActiveTab.ParentId == this.PortalSettings.AdminTabId)
                    {
                        folders.Add(new KeyValuePair<string, string>(
                            LocalizeString("PortalSkins"),
                            Path.Combine(this.PortalSettings.HomeSystemDirectoryMapPath, skins)));
                    }
                }
                else
                {
                    string foldername = currentFolder;
					Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): foldername → {Foldername}", currentFolder, foldername);
                    if (currentFolder.StartsWith("_/", StringComparison.Ordinal))
                    {
                        foldername = foldername.Substring(2);
                    }

                    var directories = GetResxDirectories(server.MapPath("~/" + foldername));
					Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): directories → {Directories}", currentFolder, directories);
                    var directoryFiles = GetResxFiles(server.MapPath("~/" + foldername));
					Logger.LogError("GetSubRootResources(currentFolder={CurrentFolder}): directoryFiles → {DirectoryFiles}", currentFolder, directoryFiles);
                    if (currentFolder.StartsWith("_/", StringComparison.Ordinal))
                    {
                        folders.AddRange(directories.Select(
                                s => new KeyValuePair<string, string>(s.Key, s.Value.Replace(foldername.Replace("/", @"\"), currentFolder))));
                        files.AddRange(directoryFiles.Select(
                                f => new KeyValuePair<string, string>(f.Key, f.Value.Replace(foldername.Replace("/", @"\"), currentFolder))));
                    }
                    else
                    {
                        folders.AddRange(directories);
                        files.AddRange(directoryFiles);
                    }
                }

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        Folders = folders.MapEntries(),
                        Files = files.MapEntries(),
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET /api/personabar/languages/GetResxEntries?portalId=&mode=Portal&locale=de-DE&resourceFile=App_GlobalResources%2fFileUpload.resx
        [HttpGet]
        public HttpResponseMessage GetResxEntries(int? portalId, string mode, string locale, string resourceFile, bool highlight = false)
        {
			Logger.LogError(">>> Entering GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight})", portalId, mode, locale, resourceFile, highlight);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                LanguageResourceMode resourceMode;
                Enum.TryParse(mode, false, out resourceMode);

                if (!this.UserInfo.IsSuperUser && (resourceMode == LanguageResourceMode.Host || resourceMode == LanguageResourceMode.System))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = this.localeController.GetLocale(locale);
				Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): language → {Language}", portalId, mode, locale, resourceFile, highlight, language);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        {
                            break;
                        }

                    default:
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "UnsupportedMode");
                }

                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.InvariantCulture, LocalizeString("InvalidLocale.ErrorMessage"), locale));
                }

                this.selectedResourceFile = !string.IsNullOrEmpty(resourceFile)
                    ? HttpContext.Current.Server.MapPath("~/" + resourceFile)
                    : HttpContext.Current.Server.MapPath(Localization.GlobalResourceFile);

                var editTable = this.LoadFile(pid, resourceMode, "Edit", locale);
				Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): editTable → {EditTable}", portalId, mode, locale, resourceFile, highlight, editTable);
                var defaultTable = this.LoadFile(pid, resourceMode, "Default", locale);
				Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): defaultTable → {DefaultTable}", portalId, mode, locale, resourceFile, highlight, defaultTable);

                var fullPath = Path.GetFileName(this.ResourceFile(pid, locale, resourceMode).Replace(Globals.ApplicationMapPath, string.Empty));
				Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): fullPath → {FullPath}", portalId, mode, locale, resourceFile, highlight, fullPath);
                var folder = this.ResourceFile(pid, locale, resourceMode).Replace(Globals.ApplicationMapPath, string.Empty).Replace(@"\" + resourceFile, string.Empty);

                // check edit table and if empty, just use default
                if (editTable.Count == 0)
                {
					Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): folder → {Folder}", portalId, mode, locale, resourceFile, highlight, folder);
                    editTable = defaultTable;
                }
                else
                {
                    // remove obsolete keys
                    var toBeDeleted = new ArrayList();
					Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): toBeDeleted → {ToBeDeleted}", portalId, mode, locale, resourceFile, highlight, toBeDeleted);
                    foreach (string key in editTable.Keys)
                    {
                        if (!defaultTable.Contains(key))
                        {
                            toBeDeleted.Add(key);
                        }
                    }

                    if (toBeDeleted.Count > 0)
                    {
                        Logger.Warn(LocalizeString("Obsolete"));
                        foreach (string key in toBeDeleted)
                        {
                            editTable.Remove(key);
                        }
                    }

                    // add missing keys
                    foreach (string key in defaultTable.Keys)
                    {
                        if (!editTable.Contains(key))
                        {
                            editTable.Add(key, defaultTable[key]);
                        }
                        else
                        {
                            // Update default value
                            var p = (Pair)editTable[key];
							Logger.LogError("GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight}): p → {P}", portalId, mode, locale, resourceFile, highlight, p);
                            p.Second = ((Pair)defaultTable[key]).First;
                            editTable[key] = p;
                        }
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    File = fullPath,
                    Folder = folder,
                    LanguageCode = language.Code,
                    Translations = new SortedList(editTable),
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetResxEntries(portalId={PortalId},mode={Mode},locale={Locale},resourceFile={ResourceFile},highlight={Highlight})", portalId, mode, locale, resourceFile, highlight);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/SaveResxEntries
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveResxEntries(UpdateTransaltionsRequest request)
        {
			Logger.LogError(">>> Entering SaveResxEntries(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                Enum.TryParse(request.Mode, false, out LanguageResourceMode resourceMode);

                if (!this.UserInfo.IsSuperUser && resourceMode is LanguageResourceMode.Host or LanguageResourceMode.System)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = this.localeController.GetLocale(request.Locale);
				Logger.LogError("SaveResxEntries(request={Request}): language → {Language}", request, language);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        break;
                    default:
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "UnsupportedMode");
                }

                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.InvariantCulture, LocalizeString("InvalidLocale.ErrorMessage"), request.Locale));
                }

                if (string.IsNullOrEmpty(request.ResourceFile))
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.InvariantCulture, LocalizeString("MissingResourceFileName"), request.Locale));
                }

                this.selectedResourceFile = HttpContext.Current.Server.MapPath("~/" + request.ResourceFile);
                var message = this.SaveResourceFileFile(pid, resourceMode, request.Locale, request.Entries);
				Logger.LogError("SaveResxEntries(request={Request}): message → {Message}", request, message);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Message = message, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting SaveResxEntries(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/EnableLocalizedContent?portalId=&translatePages=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage EnableLocalizedContent([FromUri] int? portalId, [FromUri] bool translatePages)
        {
			Logger.LogError(">>> Entering EnableLocalizedContent(portalId={PortalId},translatePages={TranslatePages})", portalId, translatePages);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var progress = new LocalizationProgress { InProgress = true, };
                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("EnableLocalizedContent(portalId={PortalId},translatePages={TranslatePages}): portal → {Portal}", portalId, translatePages, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("EnableLocalizedContent(portalId={PortalId},translatePages={TranslatePages}): portalSettings → {PortalSettings}", portalId, translatePages, portalSettings);
                LanguagesControllerTasks.LocalizeSitePages(
                    progress, pid, translatePages, portalSettings.DefaultLanguage ?? Localization.SystemLocale);
                return this.Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
			Logger.LogWarning("<<< Exiting EnableLocalizedContent(portalId={PortalId},translatePages={TranslatePages})", portalId, translatePages);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/LocalizedContent?portalId=&cultureCode=de-DE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage LocalizedContent([FromUri] int? portalId, [FromUri] string cultureCode)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var progress = new LocalizationProgress { InProgress = true, };
                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
                LanguagesControllerTasks.LocalizeLanguagePages(
                    progress, pid, cultureCode, portalSettings.DefaultLanguage ?? Localization.SystemLocale);
                return this.Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET /api/personabar/languages/GetLocalizationProgress
        [HttpGet]
        public HttpResponseMessage GetLocalizationProgress()
        {
			Logger.LogError(">>> Entering GetLocalizationProgress()");
            try
            {
                var progress = LanguagesControllerTasks.ReadProgressFile();
				Logger.LogError("GetLocalizationProgress(): progress → {Progress}", progress);
                return this.Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetLocalizationProgress()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/DisableLocalizedContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DisableLocalizedContent([FromUri] int? portalId)
        {
			Logger.LogError(">>> Entering DisableLocalizedContent(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("DisableLocalizedContent(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("DisableLocalizedContent(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
                var portalDefault = portalSettings.DefaultLanguage;
                foreach (var locale in this.localeController.GetLocales(pid).Values.Where(l => l.Code != portalDefault))
                {
                    this.localeController.PublishLanguage(pid, locale.Code, false);
                    this.tabController.DeleteTranslatedTabs(pid, locale.Code, false);
                    this.portalController.RemovePortalLocalization(pid, locale.Code, false);
                    DataProvider.Instance().EnsureLocalizationExists(pid, locale.Code);
                }

                this.tabController.EnsureNeutralLanguage(pid, portalDefault, false);
                PortalController.UpdatePortalSetting(pid, "ContentLocalizationEnabled", "False");
                DataCache.ClearPortalCache(pid, true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting DisableLocalizedContent(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/MarkAllPagesTranslated?portalId=&cultureCode=de-DE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkAllPagesTranslated([FromUri] int? portalId, [FromUri] string cultureCode)
        {
			Logger.LogError(">>> Entering MarkAllPagesTranslated(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = this.localeController.GetLocale(pid, cultureCode);
				Logger.LogError("MarkAllPagesTranslated(portalId={PortalId},cultureCode={CultureCode}): locale → {Locale}", portalId, cultureCode, locale);
                if (locale == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var nonTranslatedTabs =
                    from t in this.tabController.GetTabsByPortal(pid).WithCulture(locale.Code, false).Values
                    where !t.IsTranslated && !t.IsDeleted
                    select t;

                foreach (var page in nonTranslatedTabs)
                {
                    page.LocalizedVersionGuid = page.DefaultLanguageTab.LocalizedVersionGuid;
					Logger.LogDebug("MarkAllPagesTranslated(portalId={PortalId},cultureCode={CultureCode}): page.LocalizedVersionGuid → {Arg2}", portalId, cultureCode, page.LocalizedVersionGuid);
                    this.tabController.UpdateTab(page);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting MarkAllPagesTranslated(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/ActivateLanguage?portalId=&cultureCode=de-DE&enable=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ActivateLanguage([FromUri] int? portalId, [FromUri] string cultureCode, [FromUri] bool enable)
        {
			Logger.LogError(">>> Entering ActivateLanguage(portalId={PortalId},cultureCode={CultureCode},enable={Enable})", portalId, cultureCode, enable);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = this.localeController.GetLocale(pid, cultureCode);
				Logger.LogError("ActivateLanguage(portalId={PortalId},cultureCode={CultureCode},enable={Enable}): locale → {Locale}", portalId, cultureCode, enable, locale);
                if (locale == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                this.localeController.ActivateLanguage(pid, locale.Code, enable);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting ActivateLanguage(portalId={PortalId},cultureCode={CultureCode},enable={Enable})", portalId, cultureCode, enable);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/PublishAllPages?portalId=&cultureCode=de-DE&enable=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage PublishAllPages([FromUri] int? portalId, [FromUri] string cultureCode, [FromUri] bool enable)
        {
			Logger.LogError(">>> Entering PublishAllPages(portalId={PortalId},cultureCode={CultureCode},enable={Enable})", portalId, cultureCode, enable);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = this.localeController.GetLocale(pid, cultureCode);
				Logger.LogError("PublishAllPages(portalId={PortalId},cultureCode={CultureCode},enable={Enable}): locale → {Locale}", portalId, cultureCode, enable, locale);
                if (locale == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                this.localeController.PublishLanguage(pid, locale.Code, enable);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting PublishAllPages(portalId={PortalId},cultureCode={CultureCode},enable={Enable})", portalId, cultureCode, enable);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/DeleteLanguagePages?portalId=&cultureCode=de-DE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteLanguagePages(int? portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering DeleteLanguagePages(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = this.localeController.GetLocale(pid, cultureCode);
				Logger.LogError("DeleteLanguagePages(portalId={PortalId},cultureCode={CultureCode}): locale → {Locale}", portalId, cultureCode, locale);
                if (locale == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                this.tabController.DeleteTranslatedTabs(pid, locale.Code, false);
                this.portalController.RemovePortalLocalization(pid, locale.Code, false);
                this.localeController.PublishLanguage(pid, locale.Code, false);

                DataCache.ClearPortalCache(pid, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting DeleteLanguagePages(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [P] to expand source code folding
        private static IEnumerable<KeyValuePair<string, string>> GetResxDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                return new List<KeyValuePair<string, string>>();
            }

            return Directory.GetDirectories(path)
                .Select(folder => new DirectoryInfo(folder))
                .Where(folderInfo => HasLocalResources(folderInfo.FullName))
                .Select(folderInfo => new KeyValuePair<string, string>(folderInfo.Name, folderInfo.FullName));
        }

        private static IEnumerable<KeyValuePair<string, string>> GetResxFiles(string path)
        {
            var sysLocale = Localization.SystemLocale;
            return
                from file in Directory.GetFiles(path, "*.resx")
                select new FileInfo(file) into fileInfo
                let match = FileInfoRegex.Match(fileInfo.Name)
                where !match.Success || match.Groups[1].Value.Equals(sysLocale, StringComparison.OrdinalIgnoreCase)
                select new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(fileInfo.Name), fileInfo.FullName);
        }

        private static bool HasLocalResources(string path)
        {
			Logger.LogError(">>> Entering HasLocalResources(path={Path})", path);
            var folderInfo = new DirectoryInfo(path);

			Logger.LogError("HasLocalResources(path={Path}): folderInfo → {FolderInfo}", path, folderInfo);
            if (path.EndsWith(Localization.LocalResourceDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            var hasResources = false;
			Logger.LogError("HasLocalResources(path={Path}): hasResources → {HasResources}", path, hasResources);
            foreach (var folder in Directory.GetDirectories(path))
            {
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    folderInfo = new DirectoryInfo(folder);
                    hasResources = hasResources || HasLocalResources(folderInfo.FullName);
                }
            }

			Logger.LogWarning("<<< Exiting HasLocalResources(path={Path})", path);
            return hasResources || folderInfo.GetFiles("*.resx").Length > 0;
        }

        private static string LocalizeString(string key)
        {
			Logger.LogError(">>> Entering LocalizeString(key={Key})", key);
            Logger.LogWarning("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, LocalResourcesFile);
        }

        /// <summary> Loads resources from file into the HastTable.</summary>
        /// <param name="ht">Current resources HashTable.</param>
        /// <param name="filepath">Resources file.</param>
        /// <remarks>
        ///   Returned hashtable uses resourcekey as key.
        ///   Value contains a Pair object where:
        ///   First=>value to be edited
        ///   Second=>default value.
        /// </remarks>
        private static void LoadResource(Hashtable ht, string filepath)
        {
			Logger.LogError(">>> Entering LoadResource(ht={Ht},filepath={Filepath})", ht, filepath);
            var d = new XmlDocument { XmlResolver = null };
            bool xmlLoaded;
            try
            {
                using var xmlReader = XmlReader.Create(filepath, new XmlReaderSettings { XmlResolver = null, });
				Logger.LogError("LoadResource(ht={Ht},filepath={Filepath}): xmlReader → {XmlReader}", ht, filepath, xmlReader);
                d.Load(xmlReader);
                xmlLoaded = true;
				Logger.LogError("LoadResource(ht={Ht},filepath={Filepath}): xmlLoaded → {XmlLoaded}", ht, filepath, xmlLoaded);
            }
            catch (Exception ex)
            {
				Logger.LogDebug("LoadResource(ht={Ht},filepath={Filepath}): xmlLoaded → {XmlLoaded}", ht, filepath, xmlLoaded);
                Logger.Error(ex.Message);
                xmlLoaded = false;
            }

            if (xmlLoaded)
            {
                var nLoopVariables = d.SelectNodes("root/data");
				Logger.LogError("LoadResource(ht={Ht},filepath={Filepath}): nLoopVariables → {NLoopVariables}", ht, filepath, nLoopVariables);
                if (nLoopVariables != null)
                {
                    foreach (XmlNode nLoopVariable in nLoopVariables)
                    {
                        var n = nLoopVariable;
                        if (n.NodeType != XmlNodeType.Comment)
                        {
                            var selectSingleNode = n.SelectSingleNode("value");
							Logger.LogError("LoadResource(ht={Ht},filepath={Filepath}): selectSingleNode → {SelectSingleNode}", ht, filepath, selectSingleNode);
                            if (selectSingleNode != null)
                            {
                                var val = selectSingleNode.InnerText;
                                if (n.Attributes != null)
                                {
                                    if (ht[n.Attributes["name"].Value] == null)
                                    {
                                        ht.Add(n.Attributes["name"].Value, new Pair(val, val));
                                    }
                                    else
                                    {
                                        ht[n.Attributes["name"].Value] = new Pair(val, val);
                                    }
                                }
                            }
                        }
                    }
                }
            }
			Logger.LogWarning("<<< Exiting LoadResource(ht={Ht},filepath={Filepath})", ht, filepath);
        }

        private static XPathExpression GetResourceKeyXPath(string resourceKeyName)
        {
			Logger.LogError(">>> Entering GetResourceKeyXPath(resourceKeyName={ResourceKeyName})", resourceKeyName);
            Logger.LogWarning("<<< Exiting GetResourceKeyXPath(resourceKeyName={ResourceKeyName})", resourceKeyName);
            return XmlUtils.CreateXPathExpression(
                "//root/data[@name=$resourceKeyName]",
                new KeyValuePair<string, object>("resourceKeyName", resourceKeyName));
        }

        private static XPathExpression GetResourceKeyValueXPath(string resourceKeyName)
        {
			Logger.LogError(">>> Entering GetResourceKeyValueXPath(resourceKeyName={ResourceKeyName})", resourceKeyName);
            Logger.LogWarning("<<< Exiting GetResourceKeyValueXPath(resourceKeyName={ResourceKeyName})", resourceKeyName);
            return XmlUtils.CreateXPathExpression(
                "//root/data[@name=$resourceKeyName]/value",
                new KeyValuePair<string, object>("resourceKeyName", resourceKeyName));
        }

        private static XPathNavigator AddResourceKey(XmlDocument resourceDoc, string resourceKey)
        {
			Logger.LogError(">>> Entering AddResourceKey(resourceDoc={ResourceDoc},resourceKey={ResourceKey})", resourceDoc, resourceKey);
            // missing entry
            var nodeData = resourceDoc.CreateElement("data");
			Logger.LogError("AddResourceKey(resourceDoc={ResourceDoc},resourceKey={ResourceKey}): nodeData → {NodeData}", resourceDoc, resourceKey, nodeData);
            var attr = resourceDoc.CreateAttribute("name");
			Logger.LogError("AddResourceKey(resourceDoc={ResourceDoc},resourceKey={ResourceKey}): attr → {Attr}", resourceDoc, resourceKey, attr);
            attr.Value = resourceKey;
			Logger.LogDebug("AddResourceKey(resourceDoc={ResourceDoc},resourceKey={ResourceKey}): attr.Value → {Arg2}", resourceDoc, resourceKey, attr.Value);
            nodeData.Attributes?.Append(attr);
            var selectSingleNode = resourceDoc.SelectSingleNode("//root");
			Logger.LogError("AddResourceKey(resourceDoc={ResourceDoc},resourceKey={ResourceKey}): selectSingleNode → {SelectSingleNode}", resourceDoc, resourceKey, selectSingleNode);
            selectSingleNode?.AppendChild(nodeData);
			Logger.LogWarning("<<< Exiting AddResourceKey(resourceDoc={ResourceDoc},resourceKey={ResourceKey})", resourceDoc, resourceKey);
            return nodeData.AppendChild(resourceDoc.CreateElement("value")).CreateNavigator();
        }

        private static bool IsDefaultLanguage(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering IsDefaultLanguage(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalSettings = new PortalSettings(portal);
			Logger.LogError("IsDefaultLanguage(portalId={PortalId},cultureCode={CultureCode}): portalSettings → {PortalSettings}", portalId, cultureCode, portalSettings);
			Logger.LogError("IsDefaultLanguage(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
			Logger.LogWarning("<<< Exiting IsDefaultLanguage(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return string.Equals(cultureCode, portalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLanguageEnabled(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering IsLanguageEnabled(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            Logger.LogWarning("<<< Exiting IsLanguageEnabled(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return this.localeController.GetLocales(portalId).ContainsKey(cultureCode);
        }

        /// <summary>Loads resources from file.</summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="mode">Active editor mode.</param>
        /// <param name="type">Resource being loaded (edit or default).</param>
        /// <param name="locale">The locale of the file being edited.</param>
        /// <returns>A <see cref="Hashtable"/> where the keys are resource names and the values are <see cref="Pair"/> instances with the resource text as both values.</returns>
        /// <remarks>
        ///   Depending on the editor mode, resources will be overridden using default DNN schema.
        ///   "Edit" resources will only load selected file.
        ///   When loading "Default" resources (to be used on the editor as helpers) fallback resource
        ///   chain will be used in order for the editor to be able to correctly see what
        ///   is the current default value for the any key. This process depends on the current active
        ///   editor mode:
        ///   - System: when editing system base resources on en-US needs to be loaded
        ///   - Host: base en-US, and base locale specific resource
        ///   - Portal: base en-US, host override for en-US, base locale specific resource, and host override
        ///   for locale.
        /// </remarks>
        private Hashtable LoadFile(int portalId, LanguageResourceMode mode, string type, string locale)
        {
			Logger.LogError(">>> Entering LoadFile(portalId={PortalId},mode={Mode},type={Type},locale={Locale})", portalId, mode, type, locale);
            string file;
            var ht = new Hashtable();
			Logger.LogError("LoadFile(portalId={PortalId},mode={Mode},type={Type},locale={Locale}): ht → {Ht}", portalId, mode, type, locale, ht);

            if (type == "Edit")
            {
                // Only load resources from the file being edited
                file = this.ResourceFile(portalId, locale, mode);
				Logger.LogError("LoadFile(portalId={PortalId},mode={Mode},type={Type},locale={Locale}): file → {File}", portalId, mode, type, locale, file);
				Logger.LogDebug("LoadFile(portalId={PortalId},mode={Mode},type={Type},locale={Locale}): file → {File}", portalId, mode, type, locale, file);
                LoadResource(ht, file);
            }
            else if (type == "Default")
            {
                // Load system default
                file = this.ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System);
                LoadResource(ht, file);

                if (mode == LanguageResourceMode.Host)
                {
                    // Load base file for selected locale
                    file = this.ResourceFile(portalId, locale, LanguageResourceMode.System);
                    LoadResource(ht, file);
                }
                else if (mode == LanguageResourceMode.Portal)
                {
                    // Load host override for default locale
                    file = this.ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.Host);
                    LoadResource(ht, file);

                    if (locale != Localization.SystemLocale)
                    {
                        // Load base file for locale
                        file = this.ResourceFile(portalId, locale, LanguageResourceMode.System);
                        LoadResource(ht, file);

                        // Load host override for selected locale
                        file = this.ResourceFile(portalId, locale, LanguageResourceMode.Host);
                        LoadResource(ht, file);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting LoadFile(portalId={PortalId},mode={Mode},type={Type},locale={Locale})", portalId, mode, type, locale);
            return ht;
        }

        /// <summary>  Returns the resource file name for a given resource and language.</summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="language">Language Name.</param>
        /// <param name="mode">Identifies the resource being searched (System, Host, Portal).</param>
        /// <returns>Localized File Name.</returns>
        private string ResourceFile(int portalId, string language, LanguageResourceMode mode)
        {
			Logger.LogError(">>> Entering ResourceFile(portalId={PortalId},language={Language},mode={Mode})", portalId, language, mode);
            Logger.LogWarning("<<< Exiting ResourceFile(portalId={PortalId},language={Language},mode={Mode})", portalId, language, mode);
            return Localization.GetResourceFileName(this.selectedResourceFile, language, mode.ToString(), portalId);
        }

        /// <summary>  Updates all values from the datagrid.</summary>
        private string SaveResourceFileFile(int portalId, LanguageResourceMode mode, string locale, IEnumerable<LocalizationEntry> entries)
        {
			Logger.LogError(">>> Entering SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries})", portalId, mode, locale, entries);
            var resDoc = new XmlDocument { XmlResolver = null };
            var defDoc = new XmlDocument { XmlResolver = null };

            var filename = this.ResourceFile(portalId, locale, mode);
			Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): filename → {Filename}", portalId, mode, locale, entries, filename);
            var resourceFile = File.Exists(filename) ? filename : this.ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System);
			Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): resourceFile → {ResourceFile}", portalId, mode, locale, entries, resourceFile);
            using (var resourceReader = XmlReader.Create(resourceFile, new XmlReaderSettings { XmlResolver = null, }))
            {
                resDoc.Load(resourceReader);
            }

            using (var defaultResourceReader = XmlReader.Create(this.ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System), new XmlReaderSettings { XmlResolver = null, }))
            {
                defDoc.Load(defaultResourceReader);
            }

            // store all changed resources
            var changedResources = new Dictionary<string, string>();
			Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): changedResources → {ChangedResources}", portalId, mode, locale, entries, changedResources);

            // only items different from default will be saved
            foreach (var entry in entries)
            {
                var resourceKey = entry.Name;
                var txtValue = entry.NewValue;

                var node = resDoc.CreateNavigator()?.SelectSingleNode(GetResourceKeyValueXPath(resourceKey));
				Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): node → {Node}", portalId, mode, locale, entries, node);
                switch (mode)
                {
                    case LanguageResourceMode.System:
                        // this will save all items
                        if (node == null)
                        {
                            node = AddResourceKey(resDoc, resourceKey);
                        }

                        node.SetValue(txtValue);
                        if (txtValue != entry.DefaultValue)
                        {
                            changedResources.Add(resourceKey, txtValue);
                        }

                        break;
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        // only items different from default will be saved
                        if (txtValue != entry.DefaultValue)
                        {
                            if (node == null)
                            {
                                node = AddResourceKey(resDoc, resourceKey);
                            }

                            node.SetValue(txtValue);
                            changedResources.Add(resourceKey, txtValue);
                        }
                        else
                        {
                            // remove item = default
                            if (node?.MoveToParent() == true)
                            {
                                node.DeleteSelf();
                            }
                        }

                        break;
                }
            }

            // remove obsolete keys
            var nodeLoopVariables = resDoc.SelectNodes("//root/data");
			Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): nodeLoopVariables → {NodeLoopVariables}", portalId, mode, locale, entries, nodeLoopVariables);
            if (nodeLoopVariables != null)
            {
                foreach (XmlNode node in nodeLoopVariables)
                {
                    if (node.Attributes != null &&
                        defDoc.CreateNavigator()?.SelectSingleNode(GetResourceKeyXPath(node.Attributes["name"].Value)) == null)
                    {
                        node.ParentNode?.RemoveChild(node);
                    }
                }
            }

            // remove duplicate keys
            nodeLoopVariables = resDoc.SelectNodes("//root/data");
            if (nodeLoopVariables != null)
            {
                foreach (XmlNode node in nodeLoopVariables)
                {
                    if (node.Attributes != null)
                    {
                        var xmlNodeList = resDoc.CreateNavigator()?.Select(GetResourceKeyXPath(node.Attributes["name"].Value));
						Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): xmlNodeList → {XmlNodeList}", portalId, mode, locale, entries, xmlNodeList);
                        if (xmlNodeList is { Count: > 1, })
                        {
                            node.ParentNode?.RemoveChild(node);
                        }
                    }
                }
            }

            switch (mode)
            {
                case LanguageResourceMode.System:
                    resDoc.Save(filename);
                    break;
                case LanguageResourceMode.Host:
                case LanguageResourceMode.Portal:
                    var xmlNodeList = resDoc.SelectNodes("//root/data");
                    if (xmlNodeList is { Count: > 0, })
                    {
                        // there's something to save
                        resDoc.Save(filename);
                    }
                    else if (File.Exists(filename))
                    {
                        // nothing to be saved, if file exists delete
                        File.Delete(filename);
                    }

                    break;
            }

            if (changedResources.Count > 0)
            {
                var values = string.Join("; ", changedResources.Select(x => x.Key + "=" + x.Value));
				Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): values → {Values}", portalId, mode, locale, entries, values);
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.ADMIN_ALERT) };
				Logger.LogError("SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries}): log → {Log}", portalId, mode, locale, entries, log);
                log.LogProperties.Add(new LogDetailInfo(LocalizeString("ResourceUpdated"), this.ResourceFile(portalId, locale, mode)));
                log.LogProperties.Add(new LogDetailInfo("Updated Values", values));
                LogController.Instance.AddLog(log);
            }

			Logger.LogWarning("<<< Exiting SaveResourceFileFile(portalId={PortalId},mode={Mode},locale={Locale},entries={Entries})", portalId, mode, locale, entries);
            return string.Format(CultureInfo.InvariantCulture, LocalizeString("Updated"), this.ResourceFile(portalId, locale, mode));
        }

        private List<LanguageTabDto> GetTabsForTranslationInternal(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetTabsForTranslationInternal(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            var locale = new LocaleController().GetLocale(portalId, cultureCode);
            var portal = PortalController.Instance.GetPortal(portalId);
			Logger.LogError("GetTabsForTranslationInternal(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
            var portalSettings = new PortalSettings(portal);
			Logger.LogError("GetTabsForTranslationInternal(portalId={PortalId},cultureCode={CultureCode}): portalSettings → {PortalSettings}", portalId, cultureCode, portalSettings);
			Logger.LogError("GetTabsForTranslationInternal(portalId={PortalId},cultureCode={CultureCode}): locale → {Locale}", portalId, cultureCode, locale);
            var pages = new List<LanguageTabDto>();
            if (locale != null && locale.Code != portalSettings.DefaultLanguage)
            {
				Logger.LogError("GetTabsForTranslationInternal(portalId={PortalId},cultureCode={CultureCode}): pages → {Pages}", portalId, cultureCode, pages);
                var portalTabs = this.tabController.GetTabsByPortal(portalId).WithCulture(locale.Code, false).Values;
				Logger.LogError("GetTabsForTranslationInternal(portalId={PortalId},cultureCode={CultureCode}): portalTabs → {PortalTabs}", portalId, cultureCode, portalTabs);
                var nonTranslated = from t in portalTabs where !t.IsTranslated && !t.IsDeleted select t;
                pages.AddRange(
                    nonTranslated.Select(page => new LanguageTabDto()
                    {
                        PageId = page.TabID,
                        PageName = page.TabName,
                        ViewUrl = this.NavigationManager.NavigateURL(page.TabID),
                    }));
            }

            return pages;
        }
    }
}
