// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Dto.Tabs;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Components.Security;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using Dnn.PersonaBar.Themes.Components;
    using Dnn.PersonaBar.Themes.Components.DTO;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.OutputCache;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.UI.WebControls;

    using Localization = Dnn.PersonaBar.Pages.Components.Localization;

    /// <summary>API controller for the Pages persona bar module.</summary>
    [MenuPermission(MenuName = "Dnn.Pages")]
    [DnnExceptionFilter]
    public class PagesController : PersonaBarApiController
    {
        private const string LocalResourceFile = Library.Constants.PersonaBarRelativePath + "Modules/Dnn.Pages/App_LocalResources/Pages.resx";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PagesController));
        private readonly IPagesController pagesController;

        private readonly IBulkPagesController bulkPagesController;
        private readonly IThemesController themesController;
        private readonly ITemplateController templateController;
        private readonly IDefaultPortalThemeController defaultPortalThemeController;

        private readonly ITabController tabController;
        private readonly ILocaleController localeController;
        private readonly ISecurityService securityService;
        private readonly IWorkflowManager workflowManager;

        /// <summary>Initializes a new instance of the <see cref="PagesController"/> class.</summary>
        /// <param name="navigationManager">the navigation manager to provide navigation features.</param>
        /// <param name="pagesController">The pages controller.</param>
        /// <param name="templateController">The template controller.</param>
        public PagesController(INavigationManager navigationManager, IPagesController pagesController, ITemplateController templateController)
        {
			Logger.LogError(">>> Entering PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController})", navigationManager, pagesController, templateController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): pagesController → {PagesController1}", navigationManager, pagesController, templateController, pagesController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): templateController → {TemplateController1}", navigationManager, pagesController, templateController, templateController);
            this.NavigationManager = navigationManager;
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): this.NavigationManager → {Arg3}", navigationManager, pagesController, templateController, this.NavigationManager);

            this.pagesController = pagesController;
            this.themesController = ThemesController.Instance;
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): this.pagesController → {Arg3}", navigationManager, pagesController, templateController, this.pagesController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): themesController → {ThemesController}", navigationManager, pagesController, templateController, themesController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): this.themesController → {Arg3}", navigationManager, pagesController, templateController, this.themesController);
            this.bulkPagesController = BulkPagesController.Instance;
            this.templateController = templateController;
            this.defaultPortalThemeController = DefaultPortalThemeController.Instance;
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): bulkPagesController → {BulkPagesController}", navigationManager, pagesController, templateController, bulkPagesController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): this.bulkPagesController → {Arg3}", navigationManager, pagesController, templateController, this.bulkPagesController);

            this.tabController = TabController.Instance;
            this.localeController = LocaleController.Instance;
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): this.templateController → {Arg3}", navigationManager, pagesController, templateController, this.templateController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): defaultPortalThemeController → {DefaultPortalThemeController}", navigationManager, pagesController, templateController, defaultPortalThemeController);
			Logger.LogDebug("PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController}): this.defaultPortalThemeController → {Arg3}", navigationManager, pagesController, templateController, this.defaultPortalThemeController);
        Logger.LogWarning("<<< Exiting PagesController(navigationManager={NavigationManager},pagesController={PagesController},templateController={TemplateController})", navigationManager, pagesController, templateController);
            this.securityService = SecurityService.Instance;
            this.workflowManager = WorkflowManager.Instance;
        }

        /// <summary>Gets the Navigation Manager that provides navigation features.</summary>
        protected INavigationManager NavigationManager { get; }

        /// GET: api/Pages/GetPageDetails
        /// <summary>Get detail of a page.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>The page details.</returns>
        [HttpGet]
        public HttpResponseMessage GetPageDetails(int pageId)
        {
			Logger.LogError(">>> Entering GetPageDetails(pageId={PageId})", pageId);
            if (!this.securityService.CanManagePage(pageId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                var page = this.pagesController.GetPageSettings(pageId);
				Logger.LogError("GetPageDetails(pageId={PageId}): page → {Page}", pageId, page);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { page, new DnnFileUploadOptions().ValidationCode });
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in GetPageDetails(pageId={PageId}): {Arg1}", pageId, PageNotFoundException.Message);
				Logger.LogWarning("<<< Exiting GetPageDetails(pageId={PageId})", pageId);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Page doesn't exists." });
            }
        }

        /// GET: api/Pages/GetCustomUrls
        /// <summary>Get custom Urls of a page.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A list of custom urls.</returns>
        [HttpGet]
        public HttpResponseMessage GetCustomUrls(int pageId)
        {
			Logger.LogError(">>> Entering GetCustomUrls(pageId={PageId})", pageId);
            if (!this.securityService.CanManagePage(pageId))
            {
                return this.GetForbiddenResponse();
            }

			Logger.LogWarning("<<< Exiting GetCustomUrls(pageId={PageId})", pageId);
            return this.Request.CreateResponse(HttpStatusCode.OK, this.pagesController.GetPageUrls(pageId));
        }

        /// <summary>Creates a custom url for SEO purposes.</summary>
        /// <param name="dto"><see cref="SeoUrl"/> DTO.</param>
        /// <returns>Information about success or failure as well as a possible error message and a new url suggestion.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage CreateCustomUrl(SeoUrl dto)
        {
			Logger.LogError(">>> Entering CreateCustomUrl(dto={Dto})", dto);
            if (!this.securityService.CanManagePage(dto.TabId))
            {
                return this.GetForbiddenResponse();
            }

            var result = this.pagesController.CreateCustomUrl(dto);
			Logger.LogError("CreateCustomUrl(dto={Dto}): result → {Result}", dto, result);

			Logger.LogWarning("<<< Exiting CreateCustomUrl(dto={Dto})", dto);
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    result.Id,
                    result.Success,
                    result.ErrorMessage,
                    result.SuggestedUrlPath,
                });
        }

        /// <summary>Updates a custom url.</summary>
        /// <param name="dto"><see cref="SeoUrl"/> DTO.</param>
        /// <returns>the id, if the call succeeded or faile, a possible error message and a possible new url suggestion.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage UpdateCustomUrl(SeoUrl dto)
        {
			Logger.LogError(">>> Entering UpdateCustomUrl(dto={Dto})", dto);
            if (!this.securityService.CanManagePage(dto.TabId))
            {
                return this.GetForbiddenResponse();
            }

            var result = this.pagesController.UpdateCustomUrl(dto);
			Logger.LogError("UpdateCustomUrl(dto={Dto}): result → {Result}", dto, result);

			Logger.LogWarning("<<< Exiting UpdateCustomUrl(dto={Dto})", dto);
            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                result.Id,
                result.Success,
                result.ErrorMessage,
                result.SuggestedUrlPath,
            });
        }

        /// <summary>Deletes a custom URL.</summary>
        /// <param name="dto"><see cref="UrlIdDto"/> DTO.</param>
        /// <returns>A value indicating if the call succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeleteCustomUrl(UrlIdDto dto)
        {
			Logger.LogError(">>> Entering DeleteCustomUrl(dto={Dto})", dto);
            if (!this.securityService.CanManagePage(dto.TabId))
            {
                return this.GetForbiddenResponse();
            }

            this.pagesController.DeleteCustomUrl(dto);

            var response = new
            {
                Success = true,
            };

			Logger.LogWarning("<<< Exiting DeleteCustomUrl(dto={Dto})", dto);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// GET: api/Pages/GetPageList
        /// <summary>Gets the list of pages for a given parent page.</summary>
        /// <param name="parentId">The page (tab) id for the parent.</param>
        /// <param name="searchKey">An optional search string.</param>
        /// <returns>A list of pages.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "VIEW_PAGE_LIST,VIEW")]
        public HttpResponseMessage GetPageList(int parentId = -1, string searchKey = "")
        {
			Logger.LogError(">>> Entering GetPageList(parentId={ParentId},searchKey={SearchKey})", parentId, searchKey);
            var adminTabId = this.PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(this.PortalSettings.PortalId, adminTabId, false, true, false, true);
			Logger.LogError("GetPageList(parentId={ParentId},searchKey={SearchKey}): tabs → {Tabs}", parentId, searchKey, tabs);
            var pages = from p in this.pagesController.GetPageList(this.PortalSettings, parentId, searchKey)
                        select Converters.ConvertToPageItem<PageItem>(p, tabs);
			Logger.LogWarning("<<< Exiting GetPageList(parentId={ParentId},searchKey={SearchKey})", parentId, searchKey);
            return this.Request.CreateResponse(HttpStatusCode.OK, pages);
        }

        /// GET: api/Pages/SearchPages
        /// <summary>Searches for pages.</summary>
        /// <param name="searchKey">The search string.</param>
        /// <param name="pageType">The type of page.</param>
        /// <param name="tags">The page tags.</param>
        /// <param name="publishStatus">The publish status of the page.</param>
        /// <param name="publishDateStart">The publish start date.</param>
        /// <param name="publishDateEnd">The publish end date.</param>
        /// <param name="workflowId">The workflow id for the page.</param>
        /// <param name="pageIndex">What index of the pages paging to return.</param>
        /// <param name="pageSize">How many pages to return per paging page.</param>
        /// <returns>Paged list of pages.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "VIEW_PAGE_LIST,VIEW")]
        public HttpResponseMessage SearchPages(
            string searchKey = "",
            string pageType = "",
            string tags = "",
            string publishStatus = "All",
            string publishDateStart = "",
            string publishDateEnd = "",
            int workflowId = -1,
            int pageIndex = -1,
            int pageSize = -1)
        {
			Logger.LogError(">>> Entering SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize);
            int totalRecords;
            var adminTabId = this.PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(this.PortalSettings.PortalId, adminTabId, false, true, false, true);
			Logger.LogError("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): tabs → {Tabs}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, tabs);
            var pages = from p in this.pagesController.SearchPages(out totalRecords, searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize)
                        select Converters.ConvertToPageItem<PageItem>(p, tabs);
            var response = new
            {
                Success = true,
                Results = pages,
                TotalResults = totalRecords,
            };
			Logger.LogWarning("<<< Exiting SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>Gets the pages hierarchy.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>The page hierarchy.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage GetPageHierarchy(int pageId)
        {
			Logger.LogError(">>> Entering GetPageHierarchy(pageId={PageId})", pageId);
            try
            {
                var paths = this.pagesController.GetPageHierarchy(pageId);
				Logger.LogError("GetPageHierarchy(pageId={PageId}): paths → {Paths}", pageId, paths);
                return this.Request.CreateResponse(HttpStatusCode.OK, paths);
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in GetPageHierarchy(pageId={PageId}): {Arg1}", pageId, PageNotFoundException.Message);
				Logger.LogWarning("<<< Exiting GetPageHierarchy(pageId={PageId})", pageId);
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>Moves a page to another place in the hierarchy.</summary>
        /// <param name="request"><see cref="PageMoveRequest"/> DTO.</param>
        /// <returns>A status and information about the page at it's new location.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage MovePage(PageMoveRequest request)
        {
			Logger.LogError(">>> Entering MovePage(request={Request})", request);
            if (!this.securityService.CanManagePage(request.PageId)
                || !this.securityService.CanManagePage(request.ParentId)
                || !this.securityService.CanManagePage(request.RelatedPageId)
                || !this.securityService.CanManagePage(TabController.Instance.GetTab(request.RelatedPageId, this.PortalId)?.ParentId ?? -1))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                var tab = this.pagesController.MovePage(request);
				Logger.LogError("MovePage(request={Request}): tab → {Tab}", request, tab);
                var tabs = TabController.GetPortalTabs(
                    this.PortalSettings.PortalId,
                    Null.NullInteger,
                    false,
                    true,
                    false,
                    true);
                var pageItem = Converters.ConvertToPageItem<PageItem>(tab, tabs);
				Logger.LogError("MovePage(request={Request}): pageItem → {PageItem}", request, pageItem);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0, Page = pageItem });
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in MovePage(request={Request}): {Arg1}", request, PageNotFoundException.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PageException ex)
            {
				Logger.LogError("Exception in MovePage(request={Request}): {Arg1}", request, ex.Message);
				Logger.LogWarning("<<< Exiting MovePage(request={Request})", request);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Message });
            }
        }

        /// <summary>Deletes a page.</summary>
        /// <param name="page">The page to delete, <see cref="PageItem"/> DTO.</param>
        /// <param name="hardDelete">Should the page be hard-deleted or not.</param>
        /// <returns>The status of the page deletion.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeletePage(PageItem page, [FromUri] bool hardDelete = false)
        {
			Logger.LogError(">>> Entering DeletePage(page={Page},hardDelete={HardDelete})", page, hardDelete);
            if (!this.securityService.CanDeletePage(page.Id))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                this.pagesController.DeletePage(page, hardDelete, this.PortalSettings);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in DeletePage(page={Page},hardDelete={HardDelete}): {Arg2}", page, hardDelete, PageNotFoundException.Message);
				Logger.LogWarning("<<< Exiting DeletePage(page={Page},hardDelete={HardDelete})", page, hardDelete);
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>Deletes a module from a page.</summary>
        /// <param name="module">The module to delete, <see cref="PageModuleItem"/> DTO.</param>
        /// <returns>A status code or an error.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeletePageModule(PageModuleItem module)
        {
			Logger.LogError(">>> Entering DeletePageModule(module={Module})", module);
            if (!this.securityService.CanManagePage(module.PageId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                this.pagesController.DeleteTabModule(module.PageId, module.ModuleId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
            }
            catch (PageModuleNotFoundException)
            {
				Logger.LogError("Exception in DeletePageModule(module={Module}): {Arg1}", module, PageModuleNotFoundException.Message);
				Logger.LogWarning("<<< Exiting DeletePageModule(module={Module})", module);
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>Copies the theme from a page to descendend pages.</summary>
        /// <param name="copyTheme"><see cref="CopyThemeRequest"/>.</param>
        /// <returns>The status of the request.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage CopyThemeToDescendantPages(CopyThemeRequest copyTheme)
        {
			Logger.LogError(">>> Entering CopyThemeToDescendantPages(copyTheme={CopyTheme})", copyTheme);
            if (!this.securityService.CanManagePage(copyTheme.PageId))
            {
                return this.GetForbiddenResponse();
            }

            this.pagesController.CopyThemeToDescendantPages(copyTheme.PageId, copyTheme.Theme);
			Logger.LogWarning("<<< Exiting CopyThemeToDescendantPages(copyTheme={CopyTheme})", copyTheme);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>Copies permissions from a page to the descendent pages.</summary>
        /// <param name="copyPermissions"><see cref="CopyPermissionsRequest"/> DTO.</param>
        /// <returns>The status of the operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage CopyPermissionsToDescendantPages(CopyPermissionsRequest copyPermissions)
        {
			Logger.LogError(">>> Entering CopyPermissionsToDescendantPages(copyPermissions={CopyPermissions})", copyPermissions);
            if (!this.securityService.CanAdminPage(copyPermissions.PageId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                this.pagesController.CopyPermissionsToDescendantPages(copyPermissions.PageId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in CopyPermissionsToDescendantPages(copyPermissions={CopyPermissions}): {Arg1}", copyPermissions, PageNotFoundException.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (PermissionsNotMetException)
            {
				Logger.LogError("Exception in CopyPermissionsToDescendantPages(copyPermissions={CopyPermissions}): {Arg1}", copyPermissions, PermissionsNotMetException.Message);
				Logger.LogWarning("<<< Exiting CopyPermissionsToDescendantPages(copyPermissions={CopyPermissions})", copyPermissions);
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        /// <summary>Sets a page in edit mode.</summary>
        /// <param name="id">the page (tab) id.</param>
        /// <returns>Sets a cookie for edit mode on the specified page then returns a success message.</returns>
        [HttpPost]
        public HttpResponseMessage EditModeForPage([FromUri] int id)
        {
			Logger.LogError(">>> Entering EditModeForPage(id={Id})", id);
            if (!TabPermissionController.CanAddContentToPage(TabController.Instance.GetTab(id, this.PortalId)))
            {
                return this.GetForbiddenResponse();
            }

            this.pagesController.EditModeForPage(id, this.UserInfo.UserID);
			Logger.LogWarning("<<< Exiting EditModeForPage(id={Id})", id);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
        }

        /// <summary>Saves the page details.</summary>
        /// <param name="pageSettings">The new page settings, <see cref="PageSettings"/> DTO.</param>
        /// <returns>The new page details.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePageDetails(PageSettings pageSettings)
        {
			Logger.LogError(">>> Entering SavePageDetails(pageSettings={PageSettings})", pageSettings);
            if (!this.securityService.CanSavePageDetails(pageSettings))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                pageSettings.Clean();
                var tab = this.pagesController.SavePageDetails(this.PortalSettings, pageSettings);
				Logger.LogError("SavePageDetails(pageSettings={PageSettings}): tab → {Tab}", pageSettings, tab);
                var tabs = TabController.GetPortalTabs(
                    this.PortalSettings.PortalId,
                    Null.NullInteger,
                    false,
                    true,
                    false,
                    true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Page = Converters.ConvertToPageItem<PageItem>(tab, tabs),
                });
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in SavePageDetails(pageSettings={PageSettings}): {Arg1}", pageSettings, PageNotFoundException.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "Page doesn't exists." });
            }
            catch (PageValidationException ex)
            {
				Logger.LogError("Exception in SavePageDetails(pageSettings={PageSettings}): {Arg1}", pageSettings, ex.Message);
				Logger.LogWarning("<<< Exiting SavePageDetails(pageSettings={PageSettings})", pageSettings);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }

        /// <summary>Gets the default page settings.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>The page default settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetDefaultSettings(int pageId = 0)
        {
			Logger.LogError(">>> Entering GetDefaultSettings(pageId={PageId})", pageId);
            var settings = this.pagesController.GetDefaultSettings(pageId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { page = settings, new DnnFileUploadOptions().ValidationCode });
        }

        /// <summary>Gets the list of cache providers.</summary>
        /// <returns>List of cache providers.</returns>
        [HttpGet]
        public HttpResponseMessage GetCacheProviderList()
        {
			Logger.LogError(">>> Entering GetCacheProviderList()");
            var providers = from p in OutputCachingProvider.GetProviderList() select p.Key;
			Logger.LogError("GetCacheProviderList(): providers → {Providers}", providers);
			Logger.LogWarning("<<< Exiting GetCacheProviderList()");
            return this.Request.CreateResponse(HttpStatusCode.OK, providers);
        }

        /// <summary>Gets the available themes.</summary>
        /// <returns>Available themes.</returns>
        [HttpGet]
        public HttpResponseMessage GetThemes()
        {
			Logger.LogError(">>> Entering GetThemes()");
            var themes = this.themesController.GetLayouts(this.PortalSettings, ThemeLevel.All);

			Logger.LogError("GetThemes(): themes → {Themes}", themes);
            var defaultTheme = this.GetDefaultPortalTheme();
			Logger.LogError("GetThemes(): defaultTheme → {DefaultTheme}", defaultTheme);
            var defaultPortalThemeName = defaultTheme?.ThemeName;
            var defaultPortalThemeLevel = defaultTheme?.Level;
            var defaultPortalLayout = this.defaultPortalThemeController.GetDefaultPortalLayout();
			Logger.LogError("GetThemes(): defaultPortalLayout → {DefaultPortalLayout}", defaultPortalLayout);
            var defaultPortalContainer = this.defaultPortalThemeController.GetDefaultPortalContainer();
			Logger.LogError("GetThemes(): defaultPortalContainer → {DefaultPortalContainer}", defaultPortalContainer);

			Logger.LogWarning("<<< Exiting GetThemes()");
            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                themes,
                defaultPortalThemeName,
                defaultPortalThemeLevel,
                defaultPortalLayout,
                defaultPortalContainer,
            });
        }

        /// <summary>Gets the theme files for a given theme.</summary>
        /// <param name="themeName">The name of the theme.</param>
        /// <param name="level">The level of the theme, <see cref="ThemeLevel"/>.</param>
        /// <returns>Returns a list of available layouts and containers for each theme.</returns>
        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName, ThemeLevel level)
        {
			Logger.LogError(">>> Entering GetThemeFiles(themeName={ThemeName},level={Level})", themeName, level);
            var themeLayout = this.themesController.GetLayouts(this.PortalSettings, level)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
            var themeContainer = this.themesController.GetContainers(this.PortalSettings, level)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);

			Logger.LogWarning("<<< Exiting GetThemeFiles(themeName={ThemeName},level={Level})", themeName, level);
            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                layouts = themeLayout == null ? new List<ThemeFileInfo>() : this.themesController.GetThemeFiles(this.PortalSettings, themeLayout),
                containers = themeContainer == null ? new List<ThemeFileInfo>() : this.themesController.GetThemeFiles(this.PortalSettings, themeContainer),
            });
        }

        /// <summary>Save bulk pages (Add multiple pages).</summary>
        /// <param name="bulkPage"><see cref="BulkPage"/> DTO.</param>
        /// <returns>A status code and the result of adding the multiple pages.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage SaveBulkPages(BulkPage bulkPage)
        {
			Logger.LogError(">>> Entering SaveBulkPages(bulkPage={BulkPage})", bulkPage);
            if (!this.securityService.IsPageAdminUser())
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                bulkPage.Clean();
                var bulkPageResponse = this.bulkPagesController.AddBulkPages(bulkPage, validateOnly: false);
				Logger.LogError("SaveBulkPages(bulkPage={BulkPage}): bulkPageResponse → {BulkPageResponse}", bulkPage, bulkPageResponse);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = bulkPageResponse,
                });
            }
            catch (PageValidationException ex)
            {
				Logger.LogError("Exception in SaveBulkPages(bulkPage={BulkPage}): {Arg1}", bulkPage, ex.Message);
				Logger.LogWarning("<<< Exiting SaveBulkPages(bulkPage={BulkPage})", bulkPage);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }

        /// <summary>Validates if bulk pages (Add multiple pages) information is valid.</summary>
        /// <param name="bulkPage"><see cref="BulkPage"/> DTO.</param>
        /// <returns>A status code and the result of the bulk pages check.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage PreSaveBulkPagesValidate(BulkPage bulkPage)
        {
			Logger.LogError(">>> Entering PreSaveBulkPagesValidate(bulkPage={BulkPage})", bulkPage);
            if (!this.securityService.IsPageAdminUser())
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                bulkPage.Clean();
                var bulkPageResponse = this.bulkPagesController.AddBulkPages(bulkPage, validateOnly: true);
				Logger.LogError("PreSaveBulkPagesValidate(bulkPage={BulkPage}): bulkPageResponse → {BulkPageResponse}", bulkPage, bulkPageResponse);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = bulkPageResponse,
                });
            }
            catch (PageValidationException ex)
            {
				Logger.LogError("Exception in PreSaveBulkPagesValidate(bulkPage={BulkPage}): {Arg1}", bulkPage, ex.Message);
				Logger.LogWarning("<<< Exiting PreSaveBulkPagesValidate(bulkPage={BulkPage})", bulkPage);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Field, ex.Message });
            }
        }

        /// <summary>Saves a page as a template.</summary>
        /// <param name="pageTemplate"><see cref="PageTemplate"/> DTO.</param>
        /// <returns>The status of the operation with a possible error message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage SavePageAsTemplate(PageTemplate pageTemplate)
        {
			Logger.LogError(">>> Entering SavePageAsTemplate(pageTemplate={PageTemplate})", pageTemplate);
            if (!this.securityService.CanExportPage(pageTemplate.TabId))
            {
                return this.GetForbiddenResponse();
            }

            try
            {
                pageTemplate.Clean();
                var templateFilename = this.templateController.SaveAsTemplate(pageTemplate);
				Logger.LogError("SavePageAsTemplate(pageTemplate={PageTemplate}): templateFilename → {TemplateFilename}", pageTemplate, templateFilename);
                var response = string.Format(CultureInfo.CurrentCulture, Localization.GetString("ExportedMessage"), templateFilename);
				Logger.LogError("SavePageAsTemplate(pageTemplate={PageTemplate}): response → {Response}", pageTemplate, response);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 0,
                    Response = response,
                });
            }
            catch (TemplateException ex)
            {
				Logger.LogError("Exception in SavePageAsTemplate(pageTemplate={PageTemplate}): {Arg1}", pageTemplate, ex.Message);
				Logger.LogWarning("<<< Exiting SavePageAsTemplate(pageTemplate={PageTemplate})", pageTemplate);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, ex.Message });
            }
        }

        /// <summary>Makes a localized pages neutral.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A success status or an exception.</returns>
        /// <example>POST /api/personabar/pages/MakePageNeutral?tabId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage MakePageNeutral([FromUri] int pageId)
        {
			Logger.LogError(">>> Entering MakePageNeutral(pageId={PageId})", pageId);
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                if (this.tabController.GetTabsByPortal(this.PortalId).WithParentId(pageId).Count > 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, LocalizeString("MakeNeutral.ErrorMessage"));
                }

                var defaultLocale = this.localeController.GetDefaultLocale(this.PortalId);
				Logger.LogError("MakePageNeutral(pageId={PageId}): defaultLocale → {DefaultLocale}", pageId, defaultLocale);
                this.tabController.ConvertTabToNeutralLanguage(this.PortalId, pageId, defaultLocale.Code, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to make this page neutral, please consult the logs for more details.";
				Logger.LogError("MakePageNeutral(pageId={PageId}): errorMessage → {ErrorMessage}", pageId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting MakePageNeutral(pageId={PageId})", pageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Makes a neutral page localizable.</summary>
        /// <param name="pageId">the page (tab) id.</param>
        /// <returns>A status code or an error message.</returns>
        /// <example>POST /api/personabar/pages/MakePageTranslatable?tabId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage MakePageTranslatable([FromUri] int pageId)
        {
			Logger.LogError(">>> Entering MakePageTranslatable(pageId={PageId})", pageId);
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                var currentTab = this.tabController.GetTab(pageId, this.PortalId, false);
				Logger.LogError("MakePageTranslatable(pageId={PageId}): currentTab → {CurrentTab}", pageId, currentTab);
                if (currentTab == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidTab");
                }

                var defaultLocale = this.localeController.GetDefaultLocale(this.PortalId);
				Logger.LogError("MakePageTranslatable(pageId={PageId}): defaultLocale → {DefaultLocale}", pageId, defaultLocale);
                this.tabController.LocalizeTab(currentTab, defaultLocale, true);
                this.tabController.AddMissingLanguagesWithWarnings(this.PortalId, pageId);
                this.tabController.ClearCache(this.PortalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to make this page translatable.";
				Logger.LogError("MakePageTranslatable(pageId={PageId}): errorMessage → {ErrorMessage}", pageId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting MakePageTranslatable(pageId={PageId})", pageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Adds all missing languages to a page.</summary>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A status code or an exception message.</returns>
        /// <example>POST /api/personabar/pages/AddMissingLanguages?tabId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage AddMissingLanguages([FromUri] int pageId)
        {
			Logger.LogError(">>> Entering AddMissingLanguages(pageId={PageId})", pageId);
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                bool allLanguagesAdded = this.tabController.AddMissingLanguagesWithWarnings(this.PortalId, pageId);
                this.tabController.ClearCache(this.PortalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, AllLanguagesAdded = allLanguagesAdded });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to add missing languages to this page, consult the logs for more details.";
				Logger.LogError("AddMissingLanguages(pageId={PageId}): errorMessage → {ErrorMessage}", pageId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting AddMissingLanguages(pageId={PageId})", pageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Notifies the translators with a comment.</summary>
        /// <param name="comment"><see cref="TranslatorsComment"/> DTO.</param>
        /// <returns>A status code and a message.</returns>
        /// <example>POST /api/personabar/pages/NotifyTranslators .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage NotifyTranslators(TranslatorsComment comment)
        {
			Logger.LogError(">>> Entering NotifyTranslators(comment={Comment})", comment);
            try
            {
                if (!this.securityService.CanManagePage(comment.TabId))
                {
                    return this.GetForbiddenResponse();
                }

                // loop through all localized version of this page
                var currentTab = this.tabController.GetTab(comment.TabId, this.PortalId, false);
				Logger.LogError("NotifyTranslators(comment={Comment}): currentTab → {CurrentTab}", comment, currentTab);
                foreach (var localizedTab in currentTab.LocalizedTabs.Values)
                {
                    var users = new Dictionary<int, UserInfo>();
					Logger.LogError("NotifyTranslators(comment={Comment}): users → {Users}", comment, users);

                    // Give default translators for this language and administrators permissions
                    this.tabController.GiveTranslatorRoleEditRights(localizedTab, users);

                    // Send Messages to all the translators of new content
                    foreach (var translator in users.Values.Where(user => user.UserID != this.PortalSettings.AdministratorId))
                    {
                        this.AddTranslationSubmittedNotification(localizedTab, translator, comment.Text);
                    }
                }

                var msgToUser = LocalizeString("TranslationMessageConfirmMessage.Text");
				Logger.LogError("NotifyTranslators(comment={Comment}): msgToUser → {MsgToUser}", comment, msgToUser);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Message = msgToUser });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to notify the translators, please consult the logs for more details.";
				Logger.LogError("NotifyTranslators(comment={Comment}): errorMessage → {ErrorMessage}", comment, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting NotifyTranslators(comment={Comment})", comment);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>
        /// Gets the view data that used to be in the old ControlBar's localization tab
        /// under Page Settings ( /{page}/ctl/Tab/action/edit/activeTab/settingTab ).
        /// </summary>
        /// <param name="pageId">The ID of the tab to get localization for.</param>
        /// <returns>Information about localiz.</returns>
        /// <example>/api/personabar/pages/GetTabLocalization?pageId=123.</example>
        [HttpGet]
        public HttpResponseMessage GetTabLocalization(int pageId)
        {
			Logger.LogError(">>> Entering GetTabLocalization(pageId={PageId})", pageId);
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                var currentTab = this.tabController.GetTab(pageId, this.PortalId, false);
				Logger.LogError("GetTabLocalization(pageId={PageId}): currentTab → {CurrentTab}", pageId, currentTab);
                var locales = new List<LocaleInfoDto>();
				Logger.LogError("GetTabLocalization(pageId={PageId}): locales → {Locales}", pageId, locales);
                var pages = new DnnPagesDto(locales);
				Logger.LogError("GetTabLocalization(pageId={PageId}): pages → {Pages}", pageId, pages);
                if (!currentTab.IsNeutralCulture)
                {
                    pages = this.GetNonLocalizedPages(pageId);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, pages);
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurrfed trying to get this page localization, consult the logs for more details.";
				Logger.LogError("GetTabLocalization(pageId={PageId}): errorMessage → {ErrorMessage}", pageId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting GetTabLocalization(pageId={PageId})", pageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Updates the page (tab) localization.</summary>
        /// <param name="request"><see cref="DnnPagesRequest"/> DTO.</param>
        /// <returns>A status code or an exception message.</returns>
        /// <example>POST /api/personabar/pages/UpdateTabLocalization .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage UpdateTabLocalization(DnnPagesRequest request)
        {
			Logger.LogError(">>> Entering UpdateTabLocalization(request={Request})", request);
            try
            {
                if (request.Pages.Any(x => x.TabId > 0 && !this.securityService.CanManagePage(x.TabId)))
                {
                    return this.GetForbiddenResponse();
                }

                this.SaveNonLocalizedPages(request);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred trying to update the page localization, please consult the logs for more details.";
				Logger.LogError("UpdateTabLocalization(request={Request}): errorMessage → {ErrorMessage}", request, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting UpdateTabLocalization(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Restores a deleted module on a page (tab).</summary>
        /// <param name="tabModuleId">The TabModule id.</param>
        /// <returns>A success message or an exception message.</returns>
        /// <example>POST /api/personabar/pages/RestoreModule?tabModuleId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage RestoreModule(int tabModuleId)
        {
			Logger.LogError(">>> Entering RestoreModule(tabModuleId={TabModuleId})", tabModuleId);
            try
            {
                var module = ModuleController.Instance.GetTabModule(tabModuleId);
				Logger.LogError("RestoreModule(tabModuleId={TabModuleId}): module → {Module}", tabModuleId, module);
                if (!this.securityService.CanManagePage(module.TabID))
                {
                    return this.GetForbiddenResponse();
                }

                var moduleController = ModuleController.Instance;
                var moduleInfo = moduleController.GetTabModule(tabModuleId);
				Logger.LogError("RestoreModule(tabModuleId={TabModuleId}): moduleInfo → {ModuleInfo}", tabModuleId, moduleInfo);
                if (moduleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidTabModule");
                }

                moduleController.RestoreModule(moduleInfo);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to restore the module onto that page.";
				Logger.LogError("RestoreModule(tabModuleId={TabModuleId}): errorMessage → {ErrorMessage}", tabModuleId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting RestoreModule(tabModuleId={TabModuleId})", tabModuleId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Deletes a module from a page (tab).</summary>
        /// <param name="tabModuleId">The TabModuleId.</param>
        /// <returns>A status message or an error message.</returns>
        /// <example>POST /api/personabar/pages/DeleteModule?tabModuleId=123 .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage DeleteModule(int tabModuleId)
        {
			Logger.LogError(">>> Entering DeleteModule(tabModuleId={TabModuleId})", tabModuleId);
            try
            {
                var module = ModuleController.Instance.GetTabModule(tabModuleId);
				Logger.LogError("DeleteModule(tabModuleId={TabModuleId}): module → {Module}", tabModuleId, module);
                if (!this.securityService.CanManagePage(module.TabID))
                {
                    return this.GetForbiddenResponse();
                }

                var moduleController = ModuleController.Instance;
                var moduleInfo = moduleController.GetTabModule(tabModuleId);
				Logger.LogError("DeleteModule(tabModuleId={TabModuleId}): moduleInfo → {ModuleInfo}", tabModuleId, moduleInfo);
                if (moduleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidTabModule");
                }

                moduleController.DeleteTabModule(moduleInfo.TabID, moduleInfo.ModuleID, false);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to delete the module, consult the logs for more details.";
				Logger.LogError("DeleteModule(tabModuleId={TabModuleId}): errorMessage → {ErrorMessage}", tabModuleId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting DeleteModule(tabModuleId={TabModuleId})", tabModuleId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Gets ContentLocalizationEnabled.</summary>
        /// <returns>A value indicating if content localization is enabled.</returns>
        /// <example>GET /api/personabar/pages/GetContentLocalizationEnabled.</example>
        [HttpGet]
        public HttpResponseMessage GetContentLocalizationEnabled()
        {
			Logger.LogError(">>> Entering GetContentLocalizationEnabled()");
            try
            {
                if (!TabPermissionController.CanManagePage())
                {
                    return this.GetForbiddenResponse();
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, this.PortalSettings.ContentLocalizationEnabled });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred while trying to find if content localization is enabled";
				Logger.LogError("GetContentLocalizationEnabled(): errorMessage → {ErrorMessage}", errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting GetContentLocalizationEnabled()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Gets GetCachedItemCount.</summary>
        /// <param name="cacheProvider">The cache profider.</param>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>Caching information.</returns>
        /// <example>GET /api/personabar/pages/GetCachedItemCount.</example>
        [HttpGet]
        public HttpResponseMessage GetCachedItemCount(string cacheProvider, int pageId)
        {
			Logger.LogError(">>> Entering GetCachedItemCount(cacheProvider={CacheProvider},pageId={PageId})", cacheProvider, pageId);
            try
            {
                if (!TabPermissionController.CanManagePage())
                {
                    return this.GetForbiddenResponse();
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Count = OutputCachingProvider.Instance(cacheProvider).GetItemCount(pageId) });
            }
            catch (Exception ex)
            {
                var errorMessage = "An unexpected error occurred trying to get the cached items count, please consult the logs for more details.";
				Logger.LogError("GetCachedItemCount(cacheProvider={CacheProvider},pageId={PageId}): errorMessage → {ErrorMessage}", cacheProvider, pageId, errorMessage);
                Logger.Error(errorMessage, ex);
				Logger.LogWarning("<<< Exiting GetCachedItemCount(cacheProvider={CacheProvider},pageId={PageId})", cacheProvider, pageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, errorMessage);
            }
        }

        /// <summary>Clears the page cache for a given page.</summary>
        /// <param name="cacheProvider">The cache provider to clear the cache for.</param>
        /// <param name="pageId">The page (tab) id.</param>
        /// <returns>A status code or an error message.</returns>
        /// <example>POST /api/personabar/pages/ClearCache .</example>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "Edit")]
        public HttpResponseMessage ClearCache([FromUri] string cacheProvider, [FromUri] int pageId)
        {
			Logger.LogError(">>> Entering ClearCache(cacheProvider={CacheProvider},pageId={PageId})", cacheProvider, pageId);
            try
            {
                if (!this.securityService.CanManagePage(pageId))
                {
                    return this.GetForbiddenResponse();
                }

                OutputCachingProvider.Instance(cacheProvider).Remove(pageId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while trying to clear the cache for this page, see logs for more details.";
				Logger.LogError("ClearCache(cacheProvider={CacheProvider},pageId={PageId}): message → {Message}", cacheProvider, pageId, message);
                Logger.Error(message, ex);
				Logger.LogWarning("<<< Exiting ClearCache(cacheProvider={CacheProvider},pageId={PageId})", cacheProvider, pageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the workflows.</summary>
        /// <returns>List of portal workflows.</returns>
        /// <example>GET /api/personabar/pages/GetWorkflows.</example>
        [HttpGet]
        [AdvancedPermission(MenuName = "Dnn.Pages", Permission = "VIEW_PAGE_LIST,VIEW")]
        public HttpResponseMessage GetWorkflows()
        {
			Logger.LogError(">>> Entering GetWorkflows()");
            var workflows = this.workflowManager.GetWorkflows(this.PortalSettings.PortalId).Select(w => new { workflowId = w.WorkflowID, workflowName = w.WorkflowName });
			Logger.LogError("GetWorkflows(): workflows → {Workflows}", workflows);
			Logger.LogWarning("<<< Exiting GetWorkflows()");
            return this.Request.CreateResponse(HttpStatusCode.OK, workflows);
        }

        private static string LocalizeString(string key)
        {
			Logger.LogError(">>> Entering LocalizeString(key={Key})", key);
            Logger.LogWarning("<<< Exiting LocalizeString(key={Key})", key);
            return DotNetNuke.Services.Localization.Localization.GetString(key, LocalResourceFile);
        }

        private static void EnableTabVersioningAndWorkflow(TabInfo tab)
        {
			Logger.LogError(">>> Entering EnableTabVersioningAndWorkflow(tab={Tab})", tab);
            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;

            if (tabVersionSettings.IsVersioningEnabled(tab.PortalID))
            {
                tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, true);
            }

            if (tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID))
            {
                tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, true);
            }
			Logger.LogWarning("<<< Exiting EnableTabVersioningAndWorkflow(tab={Tab})", tab);
        }

        private static void DisableTabVersioningAndWorkflow(TabInfo tab)
        {
			Logger.LogError(">>> Entering DisableTabVersioningAndWorkflow(tab={Tab})", tab);
            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;

            if (tabVersionSettings.IsVersioningEnabled(tab.PortalID))
            {
                tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, false);
            }

            if (tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID))
            {
                tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }
			Logger.LogWarning("<<< Exiting DisableTabVersioningAndWorkflow(tab={Tab})", tab);
        }

        private DnnPagesDto GetNonLocalizedPages(int tabId)
        {
			Logger.LogError(">>> Entering GetNonLocalizedPages(tabId={TabId})", tabId);
            var currentTab = this.tabController.GetTab(tabId, this.PortalId, false);

			Logger.LogError("GetNonLocalizedPages(tabId={TabId}): currentTab → {CurrentTab}", tabId, currentTab);
            // Unique id of default language page
            var uniqueId = currentTab.DefaultLanguageGuid != Null.NullGuid
                ? currentTab.DefaultLanguageGuid
                : currentTab.UniqueId;

            // get all non admin pages and not deleted
            var allPages = this.tabController.GetTabsByPortal(this.PortalId).Values.Where(
                t => t.TabID != this.PortalSettings.AdminTabId && (Null.IsNull(t.ParentId) || t.ParentId != this.PortalSettings.AdminTabId));
            allPages = allPages.Where(t => t.IsDeleted == false);
			Logger.LogError("GetNonLocalizedPages(tabId={TabId}): allPages → {AllPages}", tabId, allPages);

            // get all localized pages of current page
            var tabInfos = allPages as IList<TabInfo> ?? allPages.ToList();
			Logger.LogError("GetNonLocalizedPages(tabId={TabId}): tabInfos → {TabInfos}", tabId, tabInfos);
            var localizedPages = tabInfos.Where(
                t => t.DefaultLanguageGuid == uniqueId || t.UniqueId == uniqueId).OrderBy(t => t.DefaultLanguageGuid).ToList();
            Dictionary<string, TabInfo> localizedTabs = null;

            // we are going to build up a list of locales
            // this is a bit more involved, since we want the default language to be first.
            // also, we do not want to add any locales the user has no access to
            var locales = new List<LocaleInfoDto>();
			Logger.LogError("GetNonLocalizedPages(tabId={TabId}): locales → {Locales}", tabId, locales);
            var localeController = new LocaleController();
			Logger.LogError("GetNonLocalizedPages(tabId={TabId}): localeController → {LocaleController}", tabId, localeController);
            var localeDict = localeController.GetLocales(this.PortalId);
			Logger.LogError("GetNonLocalizedPages(tabId={TabId}): localeDict → {LocaleDict}", tabId, localeDict);
            if (localeDict.Count == 0)
            {
                locales.Add(new LocaleInfoDto(string.Empty));
            }
            else
            {
                if (localizedPages.Count == 1 && localizedPages.First().CultureCode == string.Empty)
                {
                    // locale neutral page
                    locales.Add(new LocaleInfoDto(string.Empty));
                }
                else if (localizedPages.Count == 1 && localizedPages.First().CultureCode != this.PortalSettings.DefaultLanguage)
                {
                    var first = localizedPages.First();
					Logger.LogError("GetNonLocalizedPages(tabId={TabId}): first → {First}", tabId, first);
                    locales.Add(new LocaleInfoDto(first.CultureCode));
                }
                else
                {
                    // force sort order, so first add default language
                    locales.Add(new LocaleInfoDto(this.PortalSettings.DefaultLanguage));

                    // build up a list of localized tabs.
                    // depending on whether or not the selected page is in the default langauge
                    // we will add the localized tabs from the current page
                    // or from the defaultlanguage page
                    if (currentTab.CultureCode == this.PortalSettings.DefaultLanguage)
                    {
                        localizedTabs = currentTab.LocalizedTabs;
						Logger.LogError("GetNonLocalizedPages(tabId={TabId}): localizedTabs → {LocalizedTabs}", tabId, localizedTabs);
                    }
                    else
                    {
                        // selected page is not in default language
                        // add localizedtabs from defaultlanguage page
                        if (currentTab.DefaultLanguageTab != null)
                        {
                            localizedTabs = currentTab.DefaultLanguageTab.LocalizedTabs;
                        }
                    }

                    if (localizedTabs != null)
                    {
                        // only add locales from tabs the user has at least view permissions to.
                        // we will handle the edit permissions at a later stage
                        locales.AddRange(
                            from localizedTab in localizedTabs
                            where TabPermissionController.CanViewPage(localizedTab.Value)
                            select new LocaleInfoDto(localizedTab.Value.CultureCode));
                    }
                }
            }

            var dnnPages = new DnnPagesDto(locales)
            {
                HasMissingLanguages = this.tabController.HasMissingLanguages(this.PortalId, tabId),
            };

            // filter the list of localized pages to only those that have a culture we want to see
            var viewableLocalizedPages = localizedPages.Where(
                localizedPage => locales.Find(locale => locale.CultureCode == localizedPage.CultureCode) != null).ToList();

            foreach (var tabInfo in viewableLocalizedPages)
            {
                var localTabInfo = tabInfo;
                var dnnPage = dnnPages.Page(localTabInfo.CultureCode);
				Logger.LogError("GetNonLocalizedPages(tabId={TabId}): dnnPage → {DnnPage}", tabId, dnnPage);
                if (!TabPermissionController.CanViewPage(tabInfo))
                {
                    dnnPages.RemoveLocale(localTabInfo.CultureCode);
                    dnnPages.Pages.Remove(dnnPage);
                    break;
                }

                dnnPage.TabId = localTabInfo.TabID;
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.TabId → {Arg1}", tabId, dnnPage.TabId);
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.Title → {Arg1}", tabId, dnnPage.Title);
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.Description → {Arg1}", tabId, dnnPage.Description);
                dnnPage.TabName = localTabInfo.TabName;
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.TabName → {Arg1}", tabId, dnnPage.TabName);
                dnnPage.Title = localTabInfo.Title;
                dnnPage.Description = localTabInfo.Description;
                dnnPage.Path = localTabInfo.TabPath.Substring(0, localTabInfo.TabPath.LastIndexOf("//", StringComparison.Ordinal)).Replace("//", string.Empty);
                dnnPage.HasChildren = this.tabController.GetTabsByPortal(this.PortalId).WithParentId(tabInfo.TabID).Count != 0;
                dnnPage.CanAdminPage = TabPermissionController.CanAdminPage(tabInfo);
                dnnPage.CanViewPage = TabPermissionController.CanViewPage(tabInfo);
                dnnPage.LocalResourceFile = LocalResourceFile;
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.LocalResourceFile → {Arg1}", tabId, dnnPage.LocalResourceFile);
                dnnPage.PageUrl = this.NavigationManager.NavigateURL(localTabInfo.TabID, false, this.PortalSettings, string.Empty, localTabInfo.CultureCode);
                dnnPage.IsSpecial = TabController.IsSpecialTab(localTabInfo.TabID, localTabInfo.PortalID);

                // calculate position in the form of 1.3.2...
                var siblingTabs = tabInfos.Where(t => (t.ParentId == localTabInfo.ParentId && t.CultureCode == localTabInfo.CultureCode) || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
				Logger.LogError("GetNonLocalizedPages(tabId={TabId}): siblingTabs → {SiblingTabs}", tabId, siblingTabs);
                dnnPage.Position = (siblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture);
                var parentTabId = localTabInfo.ParentId;
				Logger.LogError("GetNonLocalizedPages(tabId={TabId}): parentTabId → {ParentTabId}", tabId, parentTabId);
                while (parentTabId > 0)
                {
                    var parentTab = tabInfos.Single(t => t.TabID == parentTabId);
					Logger.LogError("GetNonLocalizedPages(tabId={TabId}): parentTab → {ParentTab}", tabId, parentTab);
                    var id = parentTabId;
					Logger.LogError("GetNonLocalizedPages(tabId={TabId}): id → {Id}", tabId, id);
                    siblingTabs = tabInfos.Where(t => (t.ParentId == id && t.CultureCode == localTabInfo.CultureCode) || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                    dnnPage.Position = (siblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture) + "." + dnnPage.Position;
                    parentTabId = parentTab.ParentId;
                }

                dnnPage.DefaultLanguageGuid = localTabInfo.DefaultLanguageGuid;
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.DefaultLanguageGuid → {Arg1}", tabId, dnnPage.DefaultLanguageGuid);
                dnnPage.IsTranslated = localTabInfo.IsTranslated;
				Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPage.IsTranslated → {Arg1}", tabId, dnnPage.IsTranslated);
                dnnPage.IsPublished = this.tabController.IsTabPublished(localTabInfo);

                // generate modules information
                var moduleController = ModuleController.Instance;
                foreach (var moduleInfo in moduleController.GetTabModules(localTabInfo.TabID).Values)
                {
                    var guid = moduleInfo.DefaultLanguageGuid == Null.NullGuid ? moduleInfo.UniqueId : moduleInfo.DefaultLanguageGuid;

                    var dnnModules = dnnPages.Module(guid); // modules of each language
					Logger.LogError("GetNonLocalizedPages(tabId={TabId}): dnnModules → {DnnModules}", tabId, dnnModules);
                    var dnnModule = dnnModules.Module(localTabInfo.CultureCode);
					Logger.LogError("GetNonLocalizedPages(tabId={TabId}): dnnModule → {DnnModule}", tabId, dnnModule);

                    // detect error : 2 modules with same uniqueId on the same page
                    dnnModule.LocalResourceFile = LocalResourceFile;
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.LocalResourceFile → {Arg1}", tabId, dnnModule.LocalResourceFile);
                    if (dnnModule.TabModuleId > 0)
                    {
                        dnnModule.ErrorDuplicateModule = true;
						Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.ErrorDuplicateModule → {Arg1}", tabId, dnnModule.ErrorDuplicateModule);
                        dnnPages.ErrorExists = true;
						Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnPages.ErrorExists → {Arg1}", tabId, dnnPages.ErrorExists);
                        continue;
                    }

                    dnnModule.ModuleTitle = moduleInfo.ModuleTitle;
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.ModuleTitle → {Arg1}", tabId, dnnModule.ModuleTitle);
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.TabId → {Arg1}", tabId, dnnModule.TabId);
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.TabModuleId → {Arg1}", tabId, dnnModule.TabModuleId);
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.ModuleId → {Arg1}", tabId, dnnModule.ModuleId);
                    dnnModule.DefaultLanguageGuid = moduleInfo.DefaultLanguageGuid;
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.DefaultLanguageGuid → {Arg1}", tabId, dnnModule.DefaultLanguageGuid);
                    dnnModule.TabId = localTabInfo.TabID;
                    dnnModule.TabModuleId = moduleInfo.TabModuleID;
                    dnnModule.ModuleId = moduleInfo.ModuleID;
                    dnnModule.CanAdminModule = ModulePermissionController.CanAdminModule(moduleInfo);
                    dnnModule.CanViewModule = ModulePermissionController.CanViewModule(moduleInfo);
                    dnnModule.IsDeleted = moduleInfo.IsDeleted;
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.IsDeleted → {Arg1}", tabId, dnnModule.IsDeleted);
                    if (moduleInfo.DefaultLanguageGuid != Null.NullGuid)
                    {
                        var defaultLanguageModule = moduleController.GetModuleByUniqueID(moduleInfo.DefaultLanguageGuid);
						Logger.LogError("GetNonLocalizedPages(tabId={TabId}): defaultLanguageModule → {DefaultLanguageModule}", tabId, defaultLanguageModule);
                        if (defaultLanguageModule != null)
                        {
                            dnnModule.DefaultModuleId = defaultLanguageModule.ModuleID;
							Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.DefaultModuleId → {Arg1}", tabId, dnnModule.DefaultModuleId);
                            if (defaultLanguageModule.ParentTab.UniqueId != moduleInfo.ParentTab.DefaultLanguageGuid)
                            {
                                dnnModule.DefaultTabName = defaultLanguageModule.ParentTab.TabName;
								Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.DefaultTabName → {Arg1}", tabId, dnnModule.DefaultTabName);
                            }
                        }
                    }

                    dnnModule.SetModuleInfoHelp();
                    dnnModule.IsTranslated = moduleInfo.IsTranslated;
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.IsTranslated → {Arg1}", tabId, dnnModule.IsTranslated);
                    dnnModule.IsLocalized = moduleInfo.IsLocalized;
					Logger.LogDebug("GetNonLocalizedPages(tabId={TabId}): dnnModule.IsLocalized → {Arg1}", tabId, dnnModule.IsLocalized);

                    dnnModule.IsShared = this.tabController.GetTabsByModuleID(moduleInfo.ModuleID).Values.Count(t => t.CultureCode == moduleInfo.CultureCode) > 1;

                    // detect error : the default language module is on an other page
                    dnnModule.ErrorDefaultOnOtherTab = moduleInfo.DefaultLanguageGuid != Null.NullGuid && moduleInfo.DefaultLanguageModule == null;

                    // detect error : different culture on tab and module
                    dnnModule.ErrorCultureOfModuleNotCultureOfTab = moduleInfo.CultureCode != localTabInfo.CultureCode;

                    dnnPages.ErrorExists |= dnnModule.ErrorDefaultOnOtherTab || dnnModule.ErrorCultureOfModuleNotCultureOfTab;
                }
            }

            return dnnPages;
        }

        private void SaveNonLocalizedPages(DnnPagesRequest pages)
        {
			Logger.LogError(">>> Entering SaveNonLocalizedPages(pages={Pages})", pages);
            // check all pages
            foreach (var page in pages.Pages)
            {
                var tabInfo = this.tabController.GetTab(page.TabId, this.PortalId, true);
				Logger.LogError("SaveNonLocalizedPages(pages={Pages}): tabInfo → {TabInfo}", pages, tabInfo);
                if (tabInfo != null &&
                    (tabInfo.TabName != page.TabName ||
                     tabInfo.Title != page.Title ||
                     tabInfo.Description != page.Description))
                {
                    tabInfo.TabName = page.TabName;
					Logger.LogDebug("SaveNonLocalizedPages(pages={Pages}): tabInfo.TabName → {Arg1}", pages, tabInfo.TabName);
					Logger.LogDebug("SaveNonLocalizedPages(pages={Pages}): tabInfo.Description → {Arg1}", pages, tabInfo.Description);
                    tabInfo.Title = page.Title;
					Logger.LogDebug("SaveNonLocalizedPages(pages={Pages}): tabInfo.Title → {Arg1}", pages, tabInfo.Title);
                    tabInfo.Description = page.Description;
                    this.tabController.UpdateTab(tabInfo);
                }
            }

            var tabsToPublish = new List<TabInfo>();
			Logger.LogError("SaveNonLocalizedPages(pages={Pages}): tabsToPublish → {TabsToPublish}", pages, tabsToPublish);
            var moduleTranslateOverrides = new Dictionary<int, bool>();
			Logger.LogError("SaveNonLocalizedPages(pages={Pages}): moduleTranslateOverrides → {ModuleTranslateOverrides}", pages, moduleTranslateOverrides);
            var moduleController = ModuleController.Instance;

            // manage all actions we need to take for all modules on all pages
            foreach (var modulesCollection in pages.Modules)
            {
                foreach (var moduleDto in modulesCollection.Modules)
                {
                    var tabModule = moduleController.GetTabModule(moduleDto.TabModuleId);
					Logger.LogError("SaveNonLocalizedPages(pages={Pages}): tabModule → {TabModule}", pages, tabModule);
                    if (tabModule != null)
                    {
                        if (tabModule.ModuleTitle != moduleDto.ModuleTitle)
                        {
                            tabModule.ModuleTitle = moduleDto.ModuleTitle;
							Logger.LogDebug("SaveNonLocalizedPages(pages={Pages}): tabModule.ModuleTitle → {Arg1}", pages, tabModule.ModuleTitle);
                            moduleController.UpdateModule(tabModule);
                        }

                        if (tabModule.DefaultLanguageGuid != Null.NullGuid &&
                            tabModule.IsLocalized != moduleDto.IsLocalized)
                        {
                            var locale = this.localeController.GetLocale(tabModule.CultureCode);
							Logger.LogError("SaveNonLocalizedPages(pages={Pages}): locale → {Locale}", pages, locale);
                            if (moduleDto.IsLocalized)
                            {
                                moduleController.LocalizeModule(tabModule, locale);
                            }
                            else
                            {
                                moduleController.DeLocalizeModule(tabModule);
                            }
                        }

                        bool moduleTranslateOverride;
                        moduleTranslateOverrides.TryGetValue(tabModule.TabID, out moduleTranslateOverride);

                        if (!moduleTranslateOverride && tabModule.IsTranslated != moduleDto.IsTranslated)
                        {
                            moduleController.UpdateTranslationStatus(tabModule, moduleDto.IsTranslated);
                        }
                    }
                    else if (moduleDto.CopyModule)
                    {
                        // find the first existing module on the line
                        foreach (var moduleToCopyFrom in modulesCollection.Modules)
                        {
                            if (moduleToCopyFrom.ModuleId > 0)
                            {
                                var toTabInfo = this.GetLocalizedTab(moduleToCopyFrom.TabId, moduleDto.CultureCode);
								Logger.LogError("SaveNonLocalizedPages(pages={Pages}): toTabInfo → {ToTabInfo}", pages, toTabInfo);
                                var miCopy = moduleController.GetTabModule(moduleToCopyFrom.TabModuleId);
								Logger.LogError("SaveNonLocalizedPages(pages={Pages}): miCopy → {MiCopy}", pages, miCopy);
                                if (miCopy.DefaultLanguageGuid == Null.NullGuid)
                                {
                                    // default
                                    DisableTabVersioningAndWorkflow(toTabInfo);
                                    moduleController.CopyModule(miCopy, toTabInfo, Null.NullString, true);
                                    EnableTabVersioningAndWorkflow(toTabInfo);
                                    var localizedModule = moduleController.GetModule(miCopy.ModuleID, toTabInfo.TabID, false);
									Logger.LogError("SaveNonLocalizedPages(pages={Pages}): localizedModule → {LocalizedModule}", pages, localizedModule);
                                    moduleController.LocalizeModule(localizedModule, LocaleController.Instance.GetLocale(localizedModule.CultureCode));
                                }
                                else
                                {
                                    var miCopyDefault = moduleController.GetModuleByUniqueID(miCopy.DefaultLanguageGuid);
									Logger.LogError("SaveNonLocalizedPages(pages={Pages}): miCopyDefault → {MiCopyDefault}", pages, miCopyDefault);
                                    moduleController.CopyModule(miCopyDefault, toTabInfo, Null.NullString, true);
                                }

                                if (moduleDto == modulesCollection.Modules.First())
                                {
                                    // default language
                                    var miDefault = moduleController.GetModule(miCopy.ModuleID, pages.Pages.First().TabId, false);
									Logger.LogError("SaveNonLocalizedPages(pages={Pages}): miDefault → {MiDefault}", pages, miDefault);
                                    foreach (var page in pages.Pages.Skip(1))
                                    {
                                        var moduleInfo = moduleController.GetModule(miCopy.ModuleID, page.TabId, false);
										Logger.LogError("SaveNonLocalizedPages(pages={Pages}): moduleInfo → {ModuleInfo}", pages, moduleInfo);
                                        if (moduleInfo != null)
                                        {
                                            if (miDefault != null)
                                            {
                                                moduleInfo.DefaultLanguageGuid = miDefault.UniqueId;
												Logger.LogDebug("SaveNonLocalizedPages(pages={Pages}): moduleInfo.DefaultLanguageGuid → {Arg1}", pages, moduleInfo.DefaultLanguageGuid);
                                            }

                                            moduleController.UpdateModule(moduleInfo);
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            foreach (var page in pages.Pages)
            {
                var tabInfo = this.tabController.GetTab(page.TabId, this.PortalId, true);
                if (tabInfo != null)
                {
                    var moduleTranslateOverride = false;
					Logger.LogError("SaveNonLocalizedPages(pages={Pages}): moduleTranslateOverride → {ModuleTranslateOverride}", pages, moduleTranslateOverride);
                    if (!tabInfo.IsDefaultLanguage)
                    {
                        if (tabInfo.IsTranslated != page.IsTranslated)
                        {
                            this.tabController.UpdateTranslationStatus(tabInfo, page.IsTranslated);
                            if (page.IsTranslated)
                            {
                                moduleTranslateOverride = true;
                                var tabModules = moduleController.GetTabModules(tabInfo.TabID)
                                    .Where(moduleKvp =>
                                        moduleKvp.Value.DefaultLanguageModule != null &&
                                        moduleKvp.Value.LocalizedVersionGuid != moduleKvp.Value.DefaultLanguageModule.LocalizedVersionGuid);

                                foreach (var moduleKvp in tabModules)
                                {
                                    moduleController.UpdateTranslationStatus(moduleKvp.Value, true);
                                }
                            }
                        }

                        if (page.IsPublished)
                        {
                            tabsToPublish.Add(tabInfo);
                        }
                    }

                    moduleTranslateOverrides.Add(page.TabId, moduleTranslateOverride);
                }
            }

            // if we have tabs to publish, do it.
            // marks all modules as translated, marks page as translated
            foreach (var tabInfo in tabsToPublish)
            {
                // First mark all modules as translated
                foreach (var module in moduleController.GetTabModules(tabInfo.TabID).Values)
                {
                    moduleController.UpdateTranslationStatus(module, true);
                }

                // Second mark tab as translated
                this.tabController.UpdateTranslationStatus(tabInfo, true);

                // Third publish Tab (update Permissions)
                this.tabController.PublishTab(tabInfo);
            }

            // manage translated status of tab. In order to do that, we need to check if all modules on the page are translated
            var tabTranslatedStatus = true;
			Logger.LogError("SaveNonLocalizedPages(pages={Pages}): tabTranslatedStatus → {TabTranslatedStatus}", pages, tabTranslatedStatus);
            foreach (var page in pages.Pages)
            {
                var tabInfo = this.tabController.GetTab(page.TabId, this.PortalId, true);
                if (tabInfo != null)
                {
                    if (tabInfo.ChildModules.Any(moduleKvp => !moduleKvp.Value.IsTranslated))
                    {
                        tabTranslatedStatus = false;
                    }

                    if (tabTranslatedStatus && !tabInfo.IsTranslated)
                    {
                        this.tabController.UpdateTranslationStatus(tabInfo, true);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting SaveNonLocalizedPages(pages={Pages})", pages);
        }

        private void AddTranslationSubmittedNotification(TabInfo tabInfo, UserInfo translator, string comment)
        {
			Logger.LogError(">>> Entering AddTranslationSubmittedNotification(tabInfo={TabInfo},translator={Translator},comment={Comment})", tabInfo, translator, comment);
            var notificationsController = NotificationsController.Instance;
            var notificationType = notificationsController.GetNotificationType("TranslationSubmitted");
			Logger.LogError("AddTranslationSubmittedNotification(tabInfo={TabInfo},translator={Translator},comment={Comment}): notificationType → {NotificationType}", tabInfo, translator, comment, notificationType);
            var subject = LocalizeString("NewContentMessage.Subject");
			Logger.LogError("AddTranslationSubmittedNotification(tabInfo={TabInfo},translator={Translator},comment={Comment}): subject → {Subject}", tabInfo, translator, comment, subject);
            var body = string.Format(
                CultureInfo.CurrentCulture,
                LocalizeString("NewContentMessage.Body"),
                tabInfo.TabName,
                this.NavigationManager.NavigateURL(tabInfo.TabID, false, this.PortalSettings, Null.NullString, tabInfo.CultureCode),
                comment);

            var sender = UserController.GetUserById(this.PortalSettings.PortalId, this.PortalSettings.AdministratorId);
			Logger.LogError("AddTranslationSubmittedNotification(tabInfo={TabInfo},translator={Translator},comment={Comment}): sender → {Sender}", tabInfo, translator, comment, sender);
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                SenderUserID = sender.UserID,
            };

            notificationsController.SendNotification(notification, this.PortalSettings.PortalId, null, new List<UserInfo> { translator });
			Logger.LogWarning("<<< Exiting AddTranslationSubmittedNotification(tabInfo={TabInfo},translator={Translator},comment={Comment})", tabInfo, translator, comment);
        }

        private HttpResponseMessage GetForbiddenResponse()
        {
			Logger.LogError(">>> Entering GetForbiddenResponse()");
            Logger.LogWarning("<<< Exiting GetForbiddenResponse()");
            return this.Request.CreateResponse(HttpStatusCode.Forbidden, new { Message = "The user is not allowed to access this method." });
        }

        private ThemeFileInfo GetDefaultPortalTheme()
        {
			Logger.LogError(">>> Entering GetDefaultPortalTheme()");
            var layoutSrc = this.defaultPortalThemeController.GetDefaultPortalLayout();
			Logger.LogError("GetDefaultPortalTheme(): layoutSrc → {LayoutSrc}", layoutSrc);
            if (string.IsNullOrWhiteSpace(layoutSrc))
            {
                return null;
            }

            var layout = this.themesController.GetThemeFile(PortalSettings.Current, layoutSrc, ThemeType.Skin);
			Logger.LogError("GetDefaultPortalTheme(): layout → {Layout}", layout);
			Logger.LogWarning("<<< Exiting GetDefaultPortalTheme()");
            return layout;
        }

        private TabInfo GetLocalizedTab(int tabId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetLocalizedTab(tabId={TabId},cultureCode={CultureCode})", tabId, cultureCode);
            var currentTab = this.tabController.GetTab(tabId, this.PortalId, false);

			Logger.LogError("GetLocalizedTab(tabId={TabId},cultureCode={CultureCode}): currentTab → {CurrentTab}", tabId, cultureCode, currentTab);
            // Unique id of default language page
            var uniqueId = currentTab.DefaultLanguageGuid != Null.NullGuid
                ? currentTab.DefaultLanguageGuid
                : currentTab.UniqueId;

            // get all non admin pages and not deleted
            var allPages = this.tabController.GetTabsByPortal(this.PortalId).Values.Where(
                t => t.TabID != this.PortalSettings.AdminTabId && (Null.IsNull(t.ParentId) || t.ParentId != this.PortalSettings.AdminTabId));
            allPages = allPages.Where(t => t.IsDeleted == false);
			Logger.LogError("GetLocalizedTab(tabId={TabId},cultureCode={CultureCode}): allPages → {AllPages}", tabId, cultureCode, allPages);

            // get all localized pages of current page
            var tabInfos = allPages as IList<TabInfo> ?? allPages.ToList();
			Logger.LogError("GetLocalizedTab(tabId={TabId},cultureCode={CultureCode}): tabInfos → {TabInfos}", tabId, cultureCode, tabInfos);
			Logger.LogWarning("<<< Exiting GetLocalizedTab(tabId={TabId},cultureCode={CultureCode})", tabId, cultureCode);
            return tabInfos.SingleOrDefault(t => (t.DefaultLanguageGuid == uniqueId || t.UniqueId == uniqueId) && t.CultureCode == cultureCode);
        }
    }
}
