// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Sites.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Xml;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Dto.Tabs;
    using Dnn.PersonaBar.Sites.Components.Dto;
    using Dnn.PersonaBar.Sites.Services.Dto;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    public class SitesController
    {
        internal static readonly IList<string> ImageExtensions = new List<string>() { ".png", ".jpg", ".jpeg" };
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SitesController));
        private readonly TabsController tabsController = new TabsController();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Sites/App_LocalResources/Sites.resx");

        private static IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        private CultureDropDownTypes DisplayType { get; set; }

#if false
        private string IconHome { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Home.png");

        private string IconPortal { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Portal.png");

        private string AdminOnlyIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserAdmin.png");

        private string RegisteredUsersIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_User.png");

        private string IconPageDisabled { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Disabled.png");

        private string IconPageHidden { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Hidden.png");

        private string IconRedirect { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Redirect.png");

        private string SecuredIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserSecure.png");

        private string AllUsersIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Everyone.png");
#endif

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IList<HttpAliasDto> FormatPortalAliases(int portalId)
        {
			Logger.LogDebug(">>> Entering FormatPortalAliases(portalId={PortalId})", portalId);
            var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).OrderByDescending(a => a.IsPrimary).FirstOrDefault();
            if (alias == null)
			Logger.LogDebug("FormatPortalAliases(portalId={PortalId}): alias → {Alias}", portalId, alias);
            {
                return null;
            }

            var httpAlias = Globals.AddHTTP(alias.HTTPAlias);
            var originalUrl = HttpContext.Current.Items["UrlRewrite:OriginalUrl"];
			Logger.LogDebug("FormatPortalAliases(portalId={PortalId}): httpAlias → {HttpAlias}", portalId, httpAlias);
			Logger.LogDebug("FormatPortalAliases(portalId={PortalId}): originalUrl → {OriginalUrl}", portalId, originalUrl);

            httpAlias = Globals.AddPort(httpAlias, originalUrl?.ToString().ToLowerInvariant() ?? httpAlias);
			Logger.LogDebug("<<< Exiting FormatPortalAliases(portalId={PortalId})", portalId);
            return new List<HttpAliasDto> { new HttpAliasDto { Url = alias.HTTPAlias, Link = httpAlias } };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string FormatExpiryDate(DateTime dateTime)
        {
			Logger.LogDebug(">>> Entering FormatExpiryDate(dateTime={DateTime})", dateTime);
            var strDate = string.Empty;
            if (!Null.IsNull(dateTime))
			Logger.LogDebug("FormatExpiryDate(dateTime={DateTime}): strDate → {StrDate}", dateTime, strDate);
            {
                strDate = dateTime.ToShortDateString();
            }

			Logger.LogDebug("<<< Exiting FormatExpiryDate(dateTime={DateTime})", dateTime);
            return strDate;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IList<IPortalTemplateInfo> GetPortalTemplates()
        {
			Logger.LogDebug(">>> Entering GetPortalTemplates()");
            var templates = PortalTemplateController.Instance.GetPortalTemplates();
            templates = templates.OrderBy(x => x, new TemplateDisplayComparer()).ToList();
			Logger.LogDebug("GetPortalTemplates(): templates → {Templates}", templates);
			Logger.LogDebug("<<< Exiting GetPortalTemplates()");
            return templates;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IPortalTemplateInfo GetPortalTemplate(string fileName, string cultureCode)
        {
            return PortalTemplateController.Instance.GetPortalTemplate(fileName, cultureCode);
        }

        public ListItem CreateListItem(IPortalTemplateInfo template)
        {
            string text, value;
            var fileName = Path.GetFileName(template.TemplateFilePath);
            if (string.IsNullOrEmpty(template.CultureCode))
            {
                text = template.Name;
                value = $"{fileName}|{GetThumbnail(fileName)}";
            }
            else
            {
                if (this.DisplayType == 0)
                {
                    var viewType = Convert.ToString(DotNetNuke.Services.Personalization.Personalization.GetProfile("LanguageDisplayMode", "ViewType" + PortalSettings.PortalId), CultureInfo.InvariantCulture);
                    switch (viewType)
                    {
                        case "NATIVE":
                            this.DisplayType = CultureDropDownTypes.NativeName;
                            break;
                        case "ENGLISH":
                            this.DisplayType = CultureDropDownTypes.EnglishName;
                            break;
                        default:
                            this.DisplayType = CultureDropDownTypes.DisplayName;
                            break;
                    }
                }

                text = $"{template.Name} - {Localization.GetLocaleName(template.CultureCode, this.DisplayType)}";

                value = $"{fileName}|{template.CultureCode}|{GetThumbnail(fileName)}";
            }

            return new ListItem(text, value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetDefaultTemplate()
        {
			Logger.LogDebug(">>> Entering GetDefaultTemplate()");
            var templates = PortalController.Instance.GetAvailablePortalTemplates();
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
			Logger.LogDebug("GetDefaultTemplate(): templates → {Templates}", templates);

            var defaultTemplates =
                templates.Where(x => Path.GetFileNameWithoutExtension(x.TemplateFilePath) == "Default Website").ToList();

            var match = defaultTemplates.FirstOrDefault(x => x.CultureCode == currentCulture);
            if (match == null)
			Logger.LogDebug("GetDefaultTemplate(): match → {Match}", match);
            {
                match = defaultTemplates.FirstOrDefault(x => x.CultureCode.StartsWith(currentCulture.Substring(0, 2), StringComparison.Ordinal));
            }

            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => string.IsNullOrEmpty(x.CultureCode));
            }

			Logger.LogDebug("<<< Exiting GetDefaultTemplate()");
            return match != null ? $"{Path.GetFileName(match.TemplateFilePath)}|{match.CultureCode}" : string.Empty;
        }

        public TabDto GetTabByCulture(int tabId, int portalId, string cultureCode)
        {
			Logger.LogDebug(">>> Entering GetTabByCulture(tabId={TabId},portalId={PortalId},cultureCode={CultureCode})", tabId, portalId, cultureCode);
			Logger.LogDebug("<<< Exiting GetTabByCulture(tabId={TabId},portalId={PortalId},cultureCode={CultureCode})", tabId, portalId, cultureCode);
            return this.tabsController.GetTabByCulture(tabId, portalId, cultureCode);
        }

        public string ExportPortalTemplate(ExportTemplateRequest request, UserInfo userInfo, out bool success)
        {
			Logger.LogDebug(">>> Entering ExportPortalTemplate(request={Request},userInfo={UserInfo})", request, userInfo);
            var pages = request.Pages.ToList();
            var isValid = true;
			Logger.LogDebug("ExportPortalTemplate(request={Request},userInfo={UserInfo}): pages → {Pages}", request, userInfo, pages);
			Logger.LogDebug("ExportPortalTemplate(request={Request},userInfo={UserInfo}): isValid → {IsValid}", request, userInfo, isValid);
            success = false;

			Logger.LogDebug("ExportPortalTemplate(request={Request},userInfo={UserInfo}): success → {Success}", request, userInfo, success);
            // Verify all ancestor pages are selected
            foreach (var page in pages)
            {
                if (page.ParentTabId != Null.NullInteger && pages.All(p => p.TabId != page.ParentTabId.ToString(CultureInfo.InvariantCulture)))
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                return Localization.GetString("ErrorAncestorPages", this.LocalResourcesFile);
            }

            if (pages.Count == 0)
            {
                return Localization.GetString("ErrorPages", this.LocalResourcesFile);
            }

            var portal = PortalController.Instance.GetPortal(request.PortalId);
            var tabsToExport = this.GetTabsToExport(userInfo, request.PortalId, portal.DefaultLanguage, request.IsMultilanguage, pages, null)
                                    .Select(t => int.Parse(t.TabId, CultureInfo.InvariantCulture))
                                    .ToList();
									Logger.LogDebug("ExportPortalTemplate(request={Request},userInfo={UserInfo}): portal → {Portal}", request, userInfo, portal);

            var exportResult = DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.ExportPortalTemplate(
                request.PortalId,
                request.FileName,
                request.Description,
                request.IsMultilanguage,
                request.Locales,
                request.LocalizationCulture,
                tabsToExport,
                request.IncludeContent,
                request.IncludeFiles,
                request.IncludeModules,
                request.IncludeProfile,
                request.IncludeRoles);

            success = exportResult.Success;
			Logger.LogDebug("<<< Exiting ExportPortalTemplate(request={Request},userInfo={UserInfo})", request, userInfo);
            return exportResult.Message;
        }

        public int CreatePortal(List<string> errors, string domainName, string serverPath, string siteTemplate, string siteName, string siteAlias, string siteDescription, string siteKeywords, bool isChildSite, string homeDirectory, int siteGroupId, bool useCurrent, string firstname, string lastname, string username, string email, string password, string confirm, string question = "", string answer = "")
        {
			Logger.LogDebug(">>> Entering CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer})", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer);
            var template = LoadPortalTemplateInfoForSelectedItem(siteTemplate);

			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): template → {Template}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, template);
            var strChildPath = string.Empty;
            var closePopUpStr = string.Empty;
			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): strChildPath → {StrChildPath}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, strChildPath);
            var intPortalId = -1;

			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): intPortalId → {IntPortalId}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, intPortalId);
            // check template validity
            var schemaFilename = HttpContext.Current.Server.MapPath("~/Components/Portals/portal.template.xsd");
            var xmlFilename = template.TemplateFilePath;
			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): schemaFilename → {SchemaFilename}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, schemaFilename);
            var xval = new PortalTemplateValidator();
            if (!xval.Validate(xmlFilename, schemaFilename))
			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): xval → {Xval}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, xval);
            {
                errors.AddRange(xval.Errors.OfType<string>());
                return intPortalId;
            }

            // Set Portal Name
            siteAlias = siteAlias.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);

			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): siteAlias → {SiteAlias1}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, siteAlias);
            // Validate Portal Name
            var strPortalAlias = isChildSite
                ? PortalController.GetPortalFolder(siteAlias)
                : siteAlias;

            var error = false;
            var message = string.Empty;
			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): error → {Error}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, error);
			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): message → {Message}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, message);
            if (!PortalAliasController.ValidateAlias(strPortalAlias, isChildSite))
            {
                error = true;
                message = Localization.GetString("InvalidName", this.LocalResourcesFile);
            }

            // check whether there are conflicts between tab path and portal alias.
            var checkTabPath = $"//{strPortalAlias}";
            if (TabController.GetTabByTabPath(PortalSettings.PortalId, checkTabPath, string.Empty) != Null.NullInteger
				Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): checkTabPath → {CheckTabPath}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, checkTabPath);
                || TabController.GetTabByTabPath(Null.NullInteger, checkTabPath, string.Empty) != Null.NullInteger)
            {
                error = true;
                message = Localization.GetString("DuplicateWithTab", this.LocalResourcesFile);
            }

            // Validate Password
            if (password != confirm)
            {
                error = true;
                if (!string.IsNullOrEmpty(message))
                {
                    message += "<br/>";
                }

                message += Localization.GetString("InvalidPassword", this.LocalResourcesFile);
            }

            // Set Portal Alias for Child Portals
            if (string.IsNullOrEmpty(message))
            {
                if (isChildSite)
                {
                    strChildPath = serverPath + strPortalAlias;

                    if (Directory.Exists(strChildPath))
                    {
                        error = true;
                        message = Localization.GetString("ChildExists", this.LocalResourcesFile);
                    }
                    else
                    {
                        strPortalAlias = siteAlias;
                    }
                }
            }

            // Get Home Directory
            var homeDir = homeDirectory != "Portals/[PortalID]" ? homeDirectory : string.Empty;

			Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): homeDir → {HomeDir}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, homeDir);
            // Validate Home Folder
            if (!string.IsNullOrEmpty(homeDir))
            {
                var fullHomeDir = $@"{Globals.ApplicationMapPath}\{homeDir}\".Replace("/", @"\");
                if (Directory.Exists(fullHomeDir))
				Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): fullHomeDir → {FullHomeDir}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, fullHomeDir);
                {
                    error = true;
                    message = string.Format(CultureInfo.CurrentCulture, Localization.GetString("CreatePortalHomeFolderExists.Error", this.LocalResourcesFile), homeDir);
                }

                if (homeDir.Contains("admin") || homeDir.Contains("DesktopModules") || homeDir.Equals("portals/", StringComparison.OrdinalIgnoreCase))
                {
                    error = true;
                    message = Localization.GetString("InvalidHomeFolder", this.LocalResourcesFile);
                }
            }

            // Validate Portal Alias
            if (!string.IsNullOrEmpty(strPortalAlias))
            {
                PortalAliasInfo portalAlias = null;
                foreach (PortalAliasInfo alias in PortalAliasController.Instance.GetPortalAliases().Values)
                {
				Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): portalAlias → {PortalAlias}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, portalAlias);
                    if (string.Equals(alias.HTTPAlias, strPortalAlias, StringComparison.OrdinalIgnoreCase))
                    {
                        portalAlias = alias;
                        break;
                    }
                }

                if (portalAlias != null)
                {
                    error = true;
                    message = Localization.GetString("DuplicatePortalAlias", this.LocalResourcesFile);
                }
            }

            // Create Portal
            if (!error)
            {
                // Attempt to create the portal
                var adminUser = new UserInfo();
                try
				Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): adminUser → {AdminUser}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, adminUser);
                {
                    if (useCurrent)
                    {
                        adminUser = UserController.Instance.GetCurrentUserInfo();
                        intPortalId = PortalController.Instance.CreatePortal(
                            siteName,
                            adminUser.UserID,
                            siteDescription,
                            siteKeywords,
                            template,
                            homeDir,
                            strPortalAlias,
                            serverPath,
                            strChildPath,
                            isChildSite);
                    }
                    else
                    {
                        adminUser = new UserInfo
                        {
                            FirstName = firstname,
                            LastName = lastname,
                            Username = username,
                            DisplayName = firstname + " " + lastname,
                            Email = email,
                            IsSuperUser = false,
                            Membership =
                            {
                                Approved = true,
                                Password = password,
                                PasswordQuestion = question,
                                PasswordAnswer = answer,
                            },
                            Profile =
                            {
                                FirstName = firstname,
                                LastName = lastname,
                            },
                        };

                        intPortalId = PortalController.Instance.CreatePortal(
                            siteName,
                            adminUser,
                            siteDescription,
                            siteKeywords,
                            template,
                            homeDir,
                            strPortalAlias,
                            serverPath,
                            strChildPath,
                            isChildSite);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    intPortalId = Null.NullInteger;
                    message = ex.Message;

                    TryDeleteCreatingPortal(serverPath, isChildSite ? strChildPath : string.Empty);
                }

                if (intPortalId != -1)
                {
                    // Add new portal to Site Group
                    if (siteGroupId != Null.NullInteger)
                    {
                        var portal = PortalController.Instance.GetPortal(intPortalId);
                        var portalGroup = PortalGroupController.Instance.GetPortalGroups().SingleOrDefault(g => g.PortalGroupId == siteGroupId);
						Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): portal → {Portal}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, portal);
						Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): portalGroup → {PortalGroup}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, portalGroup);
                        if (portalGroup != null)
                        {
                            PortalGroupController.Instance.AddPortalToGroup(portal, portalGroup, args => { });
                        }
                    }

                    // Create a Portal Settings object for the new Portal
                    var objPortal = PortalController.Instance.GetPortal(intPortalId);
                    var newSettings = new PortalSettings
					Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): objPortal → {ObjPortal}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, objPortal);
                    {
                        PortalAlias = new PortalAliasInfo { HTTPAlias = strPortalAlias },
                        PortalId = intPortalId,
                        DefaultLanguage = objPortal.DefaultLanguage,
                    };
                    var webUrl = Globals.AddHTTP(strPortalAlias);
                    try
					Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): webUrl → {WebUrl}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, webUrl);
                    {
                        message = string.IsNullOrEmpty(Host.HostEmail)
                            ? string.Format(CultureInfo.InvariantCulture, Localization.GetString("UnknownEmailAddress.Error", this.LocalResourcesFile), message, webUrl, closePopUpStr)
                            : Mail.SendMail(
                                Host.HostEmail,
                                email,
                                Host.HostEmail,
                                Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_SUBJECT", adminUser),
                                Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_BODY", adminUser),
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                        message = string.Format(CultureInfo.CurrentCulture, Localization.GetString("UnknownSendMail.Error", this.LocalResourcesFile), webUrl, closePopUpStr);
                    }

                    // mark default language as published if content localization is enabled
                    var contentLocalizationEnabled = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", PortalSettings.PortalId, false);
                    if (contentLocalizationEnabled)
					Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): contentLocalizationEnabled → {ContentLocalizationEnabled}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, contentLocalizationEnabled);
                    {
                        var lc = new LocaleController();
                        lc.PublishLanguage(intPortalId, objPortal.DefaultLanguage, true);
						Logger.LogDebug("CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer}): lc → {Lc}", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer, lc);
                    }

                    // Redirect to this new site
                    if (message != Null.NullString)
                    {
                        message = string.Format(CultureInfo.InvariantCulture, Localization.GetString("SendMail.Error", this.LocalResourcesFile), message, webUrl, closePopUpStr);
                    }
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                errors.Add(message);
            }

			Logger.LogDebug("<<< Exiting CreatePortal(errors={Errors},domainName={DomainName},serverPath={ServerPath},siteTemplate={SiteTemplate},siteName={SiteName},siteAlias={SiteAlias},siteDescription={SiteDescription},siteKeywords={SiteKeywords},isChildSite={IsChildSite},homeDirectory={HomeDirectory},siteGroupId={SiteGroupId},useCurrent={UseCurrent},firstname={Firstname},lastname={Lastname},username={Username},email={Email},password={Password},confirm={Confirm},question={Question},answer={Answer})", errors, domainName, serverPath, siteTemplate, siteName, siteAlias, siteDescription, siteKeywords, isChildSite, homeDirectory, siteGroupId, useCurrent, firstname, lastname, username, email, password, confirm, question, answer);
            return intPortalId;
        }

        private static IPortalTemplateInfo LoadPortalTemplateInfoForSelectedItem(string template)
        {
            var values = template.Split('|');
            return PortalTemplateController.Instance.GetPortalTemplate(Path.Combine(TestableGlobals.Instance.HostMapPath, values[0]), values.Length > 1 ? values[1] : null);
        }

        private static void TryDeleteCreatingPortal(string serverPath, string childPath)
        {
			Logger.LogDebug(">>> Entering TryDeleteCreatingPortal(serverPath={ServerPath},childPath={ChildPath})", serverPath, childPath);
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
                {
                    var creatingPortalId = Convert.ToInt32(HttpContext.Current.Items["CreatingPortalId"], CultureInfo.InvariantCulture);
                    var portalInfo = PortalController.Instance.GetPortal(creatingPortalId);
					Logger.LogDebug("TryDeleteCreatingPortal(serverPath={ServerPath},childPath={ChildPath}): creatingPortalId → {CreatingPortalId}", serverPath, childPath, creatingPortalId);
					Logger.LogDebug("TryDeleteCreatingPortal(serverPath={ServerPath},childPath={ChildPath}): portalInfo → {PortalInfo}", serverPath, childPath, portalInfo);
                    PortalController.DeletePortal(portalInfo, serverPath);
                }

                if (!string.IsNullOrEmpty(childPath))
                {
                    PortalController.DeletePortalFolder(string.Empty, childPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogDebug("<<< Exiting TryDeleteCreatingPortal(serverPath={ServerPath},childPath={ChildPath})", serverPath, childPath);
        }

        private static string GetThumbnail(string templateName)
        {
			Logger.LogDebug(">>> Entering GetThumbnail(templateName={TemplateName})", templateName);
            var filePath = Path.Combine(Globals.HostMapPath, templateName);
            var imagePath = string.Empty;
			Logger.LogDebug("GetThumbnail(templateName={TemplateName}): filePath → {FilePath}", templateName, filePath);
			Logger.LogDebug("GetThumbnail(templateName={TemplateName}): imagePath → {ImagePath}", templateName, imagePath);
            foreach (var ext in ImageExtensions)
            {
                var path = Path.ChangeExtension(filePath, ext);
                if (File.Exists(path))
				Logger.LogDebug("GetThumbnail(templateName={TemplateName}): path → {Path}", templateName, path);
                {
                    imagePath = path;
                    break;
                }
            }

            imagePath = "~/" + imagePath.Replace(Globals.ApplicationMapPath, string.Empty)
                .TrimStart('\\')
                .Replace(@"\", "/");

			Logger.LogDebug("<<< Exiting GetThumbnail(templateName={TemplateName})", templateName);
            return Globals.ResolveUrl(imagePath);
        }

        private List<TabDto> GetTabsToExport(UserInfo userInfo, int portalId, string cultureCode, bool isMultiLanguage, IEnumerable<TabDto> userSelection, List<TabDto> tabsCollection)
        {
			Logger.LogDebug(">>> Entering GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection})", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection);
            if (tabsCollection == null)
            {
                var tab = this.tabsController.GetPortalTabs(userInfo, portalId, cultureCode, isMultiLanguage);
                tabsCollection = tab.ChildTabs.ToList();
				Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): tab → {Tab}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, tab);
				Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): tabsCollection → {TabsCollection1}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, tabsCollection);
				Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): tab.ChildTabs → {Arg6}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, tab.ChildTabs);
				Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): tab.HasChildren → {Arg6}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, tab.HasChildren);
                tab.ChildTabs = null;
                tab.HasChildren = false;
                tabsCollection.Add(tab);
            }

            var selectedTabs = userSelection as List<TabDto> ?? userSelection.ToList();
            foreach (var tab in tabsCollection)
            {
			Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): selectedTabs → {SelectedTabs}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, selectedTabs);
                if (selectedTabs.Exists(x => x.TabId == tab.TabId))
                {
                    var existingTab = selectedTabs.First(x => x.TabId == tab.TabId);
                    tab.CheckedState = existingTab.CheckedState;
					Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): existingTab → {ExistingTab}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, existingTab);
					Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): tab.CheckedState → {Arg6}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, tab.CheckedState);
                    if (string.IsNullOrEmpty(Convert.ToString(existingTab.Name)))
                    {
                        selectedTabs.Remove(existingTab);
                        selectedTabs.Add(tab);
                    }
                }
                else
                {
                    selectedTabs.Add(tab);
                }

                if (tab.HasChildren)
                {
                    var checkedState = NodeCheckedState.UnChecked;
                    if (tab.CheckedState == NodeCheckedState.Checked)
					Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): checkedState → {CheckedState}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, checkedState);
                    {
                        checkedState = NodeCheckedState.Checked;
                    }

                    var descendants = this.tabsController.GetTabsDescendants(portalId, Convert.ToInt32(tab.TabId, CultureInfo.InvariantCulture), cultureCode, isMultiLanguage).ToList();
                    descendants.ForEach(x => { x.CheckedState = checkedState; });
					Logger.LogDebug("GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection}): descendants → {Descendants}", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection, descendants);

                    selectedTabs.AddRange(this.GetTabsToExport(userInfo, portalId, cultureCode, isMultiLanguage, selectedTabs, descendants).Where(x => !selectedTabs.Exists(y => y.TabId == x.TabId)));
                }
            }

			Logger.LogDebug("<<< Exiting GetTabsToExport(userInfo={UserInfo},portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage},userSelection={UserSelection},tabsCollection={TabsCollection})", userInfo, portalId, cultureCode, isMultiLanguage, userSelection, tabsCollection);
            return selectedTabs;
        }

        private class TemplateDisplayComparer : IComparer<IPortalTemplateInfo>
        {
            public int Compare(IPortalTemplateInfo x, IPortalTemplateInfo y)
            {
				Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
                var cultureCompare = string.Compare(x.CultureCode, y.CultureCode, StringComparison.Ordinal);
                if (cultureCompare == 0)
				Logger.LogDebug("Compare(x={X},y={Y}): cultureCompare → {CultureCompare}", x, y, cultureCompare);
                {
                    return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                }

                // put blank cultures last
                if (string.IsNullOrEmpty(x.CultureCode) || string.IsNullOrEmpty(y.CultureCode))
                {
                    cultureCompare *= -1;
                }

				Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
                return cultureCompare;
            }
        }
    }
}
