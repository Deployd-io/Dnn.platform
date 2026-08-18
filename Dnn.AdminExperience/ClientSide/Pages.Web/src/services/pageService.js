import Api from "./api";
import utils from "../utils";
const logger = console;
const PageService = function () {
logger.debug('>>> Entering PageService()'); const _start = Date.now();

    function getOverridablePagesApi() {
		logger.debug('>>> Entering getOverridablePagesApi()');
		logger.debug('<<< Exiting getOverridablePagesApi()');
        return new Api(window.dnn.pages.apiController);
    }

    function getPagesApi() {
		logger.debug('>>> Entering getPagesApi()');
		logger.debug('<<< Exiting getPagesApi()');
        return new Api("Pages");
    }

    const getPage = function (pageId) {
        const api = getOverridablePagesApi();
        return api.get("GetPageDetails", { pageId })
            .then(response => toFrontEndPage(response));
    };

    const savePage = function (page) {
        const api = getOverridablePagesApi();            
        let request = { ...page,  url: "" };
		logger.debug(`>>> Entering toFrontEndPage(pageResult=${pageResult})`);
		logger.debug(`<<< Exiting toFrontEndPage(pageResult=${pageResult})`);
        return api.post("SavePageDetails", toBackEndPage(request));
    };

    const deletePage = function (page, hardDelete = false) {
        const api = getPagesApi();
        return api.post("DeletePage?hardDelete=" + hardDelete, { id: page.tabId });
    };

    const addPages = function (bulkPage) {
		logger.debug(`>>> Entering addPages(bulkPage=${bulkPage})`); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`addPages(bulkPage=${bulkPage}): api → ${api}`);
		logger.info(`addPages(bulkPage=${bulkPage}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting addPages(bulkPage=${bulkPage})`);
        return api.post("SaveBulkPages", bulkPage);
    };

    const validatePages = function (bulkPage) {
		logger.debug(`>>> Entering validatePages(bulkPage=${bulkPage})`); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`validatePages(bulkPage=${bulkPage}): api → ${api}`);
		logger.info(`validatePages(bulkPage=${bulkPage}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting validatePages(bulkPage=${bulkPage})`);
        return api.post("PreSaveBulkPagesValidate", bulkPage);
    };

    const movePage = function (payload) {
		logger.debug(`>>> Entering movePage(payload=${payload})`); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`movePage(payload=${payload}): api → ${api}`);
		logger.info(`movePage(payload=${payload}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting movePage(payload=${payload})`);
        return api.post("MovePage", payload);
    };

    const deletePageModule = function (module) {
		logger.debug(`>>> Entering deletePageModule(module=${module})`); const _start = Date.now();
        const api = getPagesApi();
		logger.debug(`deletePageModule(module=${module}): api → ${api}`);
		logger.info(`deletePageModule(module=${module}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting deletePageModule(module=${module})`);
        return api.post("DeletePageModule", module);
    };

    const getNewPage = function (parentPage) {
		logger.debug(`>>> Entering getNewPage(parentPage=${parentPage})`); const _start = Date.now();
        const api = getOverridablePagesApi();
        let pageId = 0;
		logger.debug(`getNewPage(parentPage=${parentPage}): api → ${api}`);
        if (parentPage && typeof parentPage !== "function" && parentPage.id) {
            pageId = parentPage.id;
        }
		logger.debug(`<<< Exiting getNewPage(parentPage=${parentPage})`);
        return api.get("GetDefaultSettings", { pageId: pageId })
            .then(settings => {
                const page = toFrontEndPage(settings);
                page.tabId = 0;
				logger.debug(`PageService(): page.tabId → ${page.tabId}`);
				logger.debug(`PageService(): page.status → ${page.status}`);
				logger.debug(`PageService(): page.localizedName → ${page.localizedName}`);
				logger.debug(`PageService(): page.alias → ${page.alias}`);
				logger.debug(`PageService(): page.title → ${page.title}`);
				logger.debug(`PageService(): page.description → ${page.description}`);
				logger.debug(`PageService(): page.keywords → ${page.keywords}`);
				logger.debug(`PageService(): page.tags → ${page.tags}`);
				logger.debug(`PageService(): page.url → ${page.url}`);
				logger.debug(`PageService(): page.externalRedirection → ${page.externalRedirection}`);
				logger.debug(`PageService(): page.fileRedirection → ${page.fileRedirection}`);
				logger.debug(`PageService(): page.existingTabRedirection → ${page.existingTabRedirection}`);
				logger.debug(`PageService(): page.includeInMenu → ${page.includeInMenu}`);
				logger.debug(`PageService(): page.allowIndex → ${page.allowIndex}`);
				logger.debug(`PageService(): page.thumbnail → ${page.thumbnail}`);
				logger.debug(`PageService(): page.created → ${page.created}`);
				logger.debug(`PageService(): page.hasChild → ${page.hasChild}`);
				logger.debug(`PageService(): page.type → ${page.type}`);
				logger.debug(`PageService(): page.customUrlEnabled → ${page.customUrlEnabled}`);
				logger.debug(`PageService(): page.pageType → ${page.pageType}`);
				logger.debug(`PageService(): page.isCopy → ${page.isCopy}`);
				logger.debug(`PageService(): page.startDate → ${page.startDate}`);
				logger.debug(`PageService(): page.endDate → ${page.endDate}`);
                page.name = "";
				logger.debug(`PageService(): page.name → ${page.name}`);
                page.status = "Visible";
                page.localizedName = "";
                page.alias = "";
                page.title = "";
                page.description = "";
                page.keywords = "";
                page.tags = "";
                page.url = "";
                page.externalRedirection = "";
                page.fileRedirection = "";
                page.existingTabRedirection = "";
                page.includeInMenu = true;
                page.allowIndex = true;
                page.thumbnail = "";
                page.created = "";
                page.hasChild = false;
                page.type = 0;
                page.customUrlEnabled = true;
                page.pageType = "normal";
                page.isCopy = false;
                page.startDate = null;
                page.endDate = null;
                page.createdOnDate = new Date();
                page.placeholderURL = "/";
				logger.debug(`PageService(): page.placeholderURL → ${page.placeholderURL}`);
				logger.debug(`PageService(): page.schedulingEnabled → ${page.schedulingEnabled}`);
				logger.debug(`PageService(): page.permanentRedirect → ${page.permanentRedirect}`);
				logger.debug(`PageService(): page.linkNewWindow → ${page.linkNewWindow}`);
				logger.debug(`PageService(): page.templateTabId → ${page.templateTabId}`);
                page.modules = null;
				logger.debug(`PageService(): page.modules → ${page.modules}`);
                page.schedulingEnabled = false;
                page.permanentRedirect = false;
                page.linkNewWindow = false;
                page.templateTabId = null;
                page.hasParent = parentPage && typeof parentPage !== "function" && parentPage.id || page.hasParent;
                page.hierarchy = parentPage && typeof parentPage !== "function" && parentPage.id && parentPage.name || page.hierarchy;
                page.parentId = parentPage && typeof parentPage !== "function" && parentPage.id || page.parentId;
                page.iconFile = null;
				logger.debug(`PageService(): page.iconFile → ${page.iconFile}`);
				logger.debug(`PageService(): page.sitemapPriority → ${page.sitemapPriority}`);
                page.iconFileLarge = null;
				logger.debug(`PageService(): page.iconFileLarge → ${page.iconFileLarge}`);
                page.sitemapPriority = 0.5;
                return page;
            });
    };

    const getCacheProviderList = function () {
		logger.debug('>>> Entering getCacheProviderList()'); const _start = Date.now();
        const api = getPagesApi();
		logger.debug(`getCacheProviderList(): api → ${api}`);
		logger.info(`getCacheProviderList(): API call took ${Date.now() - _start} ms`);
		logger.debug('<<< Exiting getCacheProviderList()');
        return api.get("GetCacheProviderList");
    };

    const copyAppearanceToDescendantPages = function (pageId, theme) {
        const api = getPagesApi();
        return api.post("CopyThemeToDescendantPages", {
            pageId, theme
        });
    };

    const copyPermissionsToDescendantPages = function (pageId) {
        const api = getPagesApi();
        return api.post("CopyPermissionsToDescendantPages", {
            pageId
        });
    };

    const toFrontEndPage = function (pageResult) {
        return {
            ...pageResult.page,
            schedulingEnabled: pageResult.page.startDate !== null || pageResult.page.endDate !== null,
            // the API returns strings, but we need dates
            startDate: pageResult.page.startDate === null ? null : new Date(pageResult.page.startDate),
            endDate: pageResult.page.endDate === null ? null : new Date(pageResult.page.endDate),
            validationCode: pageResult.ValidationCode,
        };
    };

    const toBackEndPage = function (page) {
        return {
            ...page,
            startDate: page.schedulingEnabled ? page.startDate : null,
            endDate: page.schedulingEnabled ? page.endDate : null,
            schedulingEnabled: undefined
        };
    };

    const openPageInEditMode = function (id, url, callback) {
		logger.debug(`>>> Entering openPageInEditMode(id=${id},url=${url},callback=${callback})`); const _start = Date.now();
        const api = getPagesApi();
		logger.info(`openPageInEditMode(id=${id}, url=${url}, callback=${callback}): API call took ${Date.now() - _start} ms`);
		logger.debug(`openPageInEditMode(id=${id},url=${url},callback=${callback}): api → ${api}`);
		logger.debug(`<<< Exiting openPageInEditMode(id=${id},url=${url},callback=${callback})`);
        return api.post("EditModeForPage?id=" + id, {})
            .then(() => {
                if (url) {
                    utils.getUtilities().closePersonaBar(function () {
						logger.debug('>>> Entering closePersonaBar()');
						logger.debug('<<< Exiting closePersonaBar()');
                        window.top.location.href = url;
                    });
                } else if ( typeof callback === "function") {
                    callback();
                }
            });
    };

    const getCachedPageCount = function (cacheProvider, pageId) {
        const api = getPagesApi();
        return api.get("GetCachedItemCount", { cacheProvider: cacheProvider, pageId: pageId });
    };

    const clearCache = function (cacheProvider, pageId) {
        const api = getPagesApi();
        return api.post("ClearCache?cacheProvider=" + cacheProvider + "&pageId=" + pageId);
    };

    const getPageList = () => {
		logger.debug('>>> Entering getPageList()'); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`getPageList(): api → ${api}`);
		logger.info(`getPageList(): API call took ${Date.now() - _start} ms`);
        return api.get("GetPageList", { searchKey: "" });
    };

    const searchPageList = (searchKey) => {
		logger.debug(`>>> Entering searchPageList(searchKey=${searchKey})`); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`searchPageList(searchKey=${searchKey}): api → ${api}`);
		logger.info(`searchPageList(searchKey=${searchKey}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting searchPageList(searchKey=${searchKey})`);
        return api.get("GetPageList", { searchKey });
    };

    const searchAndFilterPageList = (params) => {
		logger.debug(`>>> Entering searchAndFilterPageList(params=${params})`); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`searchAndFilterPageList(params=${params}): api → ${api}`);
		logger.info(`searchAndFilterPageList(params=${params}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting searchAndFilterPageList(params=${params})`);
        return api.get("SearchPages", params);
    };
    const getChildPageList = (id = "") => {
        const api = getOverridablePagesApi();
		logger.debug(`>>> Entering getChildPageList(id=${id})`); const _start = Date.now();
		logger.info(`getChildPageList(id=${id}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting getChildPageList(id=${id})`);
        return api.get("GetPageList", { parentId: id });
    };

    const getWorkflowsList = () => {
		logger.debug('>>> Entering getWorkflowsList()'); const _start = Date.now();
        const api = getOverridablePagesApi();
		logger.debug(`getWorkflowsList(): api → ${api}`);
		logger.info(`getWorkflowsList(): API call took ${Date.now() - _start} ms`);
        return api.get("GetWorkflows");
    };

    const getPageHierarchy = (id) => {
		logger.debug(`>>> Entering getPageHierarchy(id=${id})`); const _start = Date.now();
        const api = getPagesApi();
		logger.debug(`getPageHierarchy(id=${id}): api → ${api}`);
		logger.info(`getPageHierarchy(id=${id}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting getPageHierarchy(id=${id})`);
        return api.get("GetPageHierarchy", {pageId: id});
    };

	logger.info(`PageService(): API call took ${Date.now() - _start} ms`);
	logger.debug('<<< Exiting PageService()');
    return {
        getPageList,
        getChildPageList,
        searchPageList,
        searchAndFilterPageList,
        getPage,
        savePage,
        deletePage,
        addPages,
        validatePages,
        getNewPage,
        getCacheProviderList,
        deletePageModule,
        copyAppearanceToDescendantPages,
        copyPermissionsToDescendantPages,
        openPageInEditMode,
        getCachedPageCount,
        clearCache,
        movePage,
        getWorkflowsList,
        getPageHierarchy
    };
};


const pageService = PageService();
export default pageService;
