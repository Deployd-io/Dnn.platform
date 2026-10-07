import Api from "./api";
import utils from "../utils";
const logger = console;
const PageService = function () {
logger.error('>>> Entering PageService()', { "event.action": "PageService" });

    function getOverridablePagesApi() {
		logger.error('>>> Entering getOverridablePagesApi()', { "event.action": "getOverridablePagesApi" });
        return new Api(window.dnn.pages.apiController);
    }

    function getPagesApi() {
		logger.error('>>> Entering getPagesApi()', { "event.action": "getPagesApi" });
		logger.warn('<<< Exiting getPagesApi()', { "event.action": "getPagesApi" });
        return new Api("Pages");
    }

    const getPage = function (pageId) {
        const api = getOverridablePagesApi();
		const _start_getPage_13 = Date.now();
        return api.get("GetPageDetails", { pageId })
            .then(response => toFrontEndPage(response));
    };

    const savePage = function (page) {
        const api = getOverridablePagesApi();            
        let request = { ...page,  url: "" };
		const _start_savePage_19 = Date.now();
        return api.post("SavePageDetails", toBackEndPage(request));
    };

    const deletePage = function (page, hardDelete = false) {
        const api = getPagesApi();
        return api.post("DeletePage?hardDelete=" + hardDelete, { id: page.tabId });
    };

    const addPages = function (bulkPage) {
		logger.error(`>>> Entering addPages(bulkPage=${bulkPage})`, { "event.action": "addPages", "logrite.bulkPage": bulkPage });
        const api = getOverridablePagesApi();
		logger.error(`addPages(bulkPage=${bulkPage}): api → ${api}`, { "event.action": "addPages", "logrite.bulkPage": bulkPage });
        return api.post("SaveBulkPages", bulkPage);
    };

    const validatePages = function (bulkPage) {
		logger.error(`>>> Entering validatePages(bulkPage=${bulkPage})`, { "event.action": "validatePages", "logrite.bulkPage": bulkPage });
        const api = getOverridablePagesApi();
		logger.error(`validatePages(bulkPage=${bulkPage}): api → ${api}`, { "event.action": "validatePages", "logrite.bulkPage": bulkPage });
        return api.post("PreSaveBulkPagesValidate", bulkPage);
    };

    const movePage = function (payload) {
		logger.error(`>>> Entering movePage(payload=${payload})`, { "event.action": "movePage", "logrite.payload": payload });
        const api = getOverridablePagesApi();
		logger.error(`movePage(payload=${payload}): api → ${api}`, { "event.action": "movePage", "logrite.payload": payload });
        return api.post("MovePage", payload);
    };

    const deletePageModule = function (module) {
		logger.error(`>>> Entering deletePageModule(module=${module})`, { "event.action": "deletePageModule", "logrite.module": module });
        const api = getPagesApi();
		logger.error(`deletePageModule(module=${module}): api → ${api}`, { "event.action": "deletePageModule", "logrite.module": module });
        return api.post("DeletePageModule", module);
    };

    const getNewPage = function (parentPage) {
		logger.error(`>>> Entering getNewPage(parentPage=${parentPage})`, { "event.action": "getNewPage", "logrite.parentPage": parentPage });
        const api = getOverridablePagesApi();
        let pageId = 0;
		const _start_getNewPage_50 = Date.now();
		logger.error(`getNewPage(parentPage=${parentPage}): api → ${api}`, { "event.action": "getNewPage", "logrite.parentPage": parentPage });
        if (parentPage && typeof parentPage !== "function" && parentPage.id) {
            pageId = parentPage.id;
        }
		logger.warn(`<<< Exiting getNewPage(parentPage=${parentPage})`, { "event.action": "getNewPage", "logrite.parentPage": parentPage });
        return api.get("GetDefaultSettings", { pageId: pageId })
            .then(settings => {
                const page = toFrontEndPage(settings);
                page.tabId = 0;
                page.name = "";
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
                page.modules = null;
                page.schedulingEnabled = false;
                page.permanentRedirect = false;
                page.linkNewWindow = false;
                page.templateTabId = null;
                page.hasParent = parentPage && typeof parentPage !== "function" && parentPage.id || page.hasParent;
                page.hierarchy = parentPage && typeof parentPage !== "function" && parentPage.id && parentPage.name || page.hierarchy;
                page.parentId = parentPage && typeof parentPage !== "function" && parentPage.id || page.parentId;
                page.iconFile = null;
                page.iconFileLarge = null;
                page.sitemapPriority = 0.5;
                return page;
            });
    };

    const getCacheProviderList = function () {
		logger.error('>>> Entering getCacheProviderList()', { "event.action": "getCacheProviderList" });
        const api = getPagesApi();
		logger.error(`getCacheProviderList(): api → ${api}`, { "event.action": "getCacheProviderList" });
        return api.get("GetCacheProviderList");
    };

    const copyAppearanceToDescendantPages = function (pageId, theme) {
        const api = getPagesApi();
		const _start_copyAppearanceToDescendantPages_105 = Date.now();
        return api.post("CopyThemeToDescendantPages", {
            pageId, theme
        });
    };

    const copyPermissionsToDescendantPages = function (pageId) {
        const api = getPagesApi();
		const _start_copyPermissionsToDescendantPages_112 = Date.now();
        return api.post("CopyPermissionsToDescendantPages", {
            pageId
        });
    };

    const toFrontEndPage = function (pageResult) {
		logger.error(`>>> Entering toFrontEndPage(pageResult=${pageResult})`, { "event.action": "toFrontEndPage", "logrite.pageResult": pageResult });
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
		logger.error(`>>> Entering openPageInEditMode(id=${id},url=${url},callback=${callback})`, { "event.action": "openPageInEditMode", "logrite.id": id, "logrite.url": url, "logrite.callback": callback });
        const api = getPagesApi();
		const _start_openPageInEditMode_139 = Date.now();
		logger.error(`openPageInEditMode(id=${id}, url=${url}, callback=${callback}): API call took ${Date.now() - _start_openPageInEditMode_139} ms`, { "event.action": "openPageInEditMode", "event.category": "network", "logrite.id": id, "logrite.url": url, "logrite.callback": callback });
		logger.warn(`<<< Exiting openPageInEditMode(id=${id},url=${url},callback=${callback})`, { "event.action": "openPageInEditMode", "logrite.id": id, "logrite.url": url, "logrite.callback": callback });
        return api.post("EditModeForPage?id=" + id, {})
            .then(() => {
                if (url) {
                    utils.getUtilities().closePersonaBar(function () {
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
		logger.error('>>> Entering getPageList()', { "event.action": "getPageList" });
        const api = getOverridablePagesApi();
		logger.error(`getPageList(): api → ${api}`, { "event.action": "getPageList" });
        return api.get("GetPageList", { searchKey: "" });
    };

    const searchPageList = (searchKey) => {
		logger.error(`>>> Entering searchPageList(searchKey=${searchKey})`, { "event.action": "searchPageList", "logrite.searchKey": searchKey });
        const api = getOverridablePagesApi();
		logger.error(`searchPageList(searchKey=${searchKey}): api → ${api}`, { "event.action": "searchPageList", "logrite.searchKey": searchKey });
        return api.get("GetPageList", { searchKey });
    };

    const searchAndFilterPageList = (params) => {
		logger.error(`>>> Entering searchAndFilterPageList(params=${params})`, { "event.action": "searchAndFilterPageList", "logrite.params": params });
        const api = getOverridablePagesApi();
		logger.error(`searchAndFilterPageList(params=${params}): api → ${api}`, { "event.action": "searchAndFilterPageList", "logrite.params": params });
        return api.get("SearchPages", params);
    };
    const getChildPageList = (id = "") => {
		logger.error(`>>> Entering getChildPageList(id=${id})`, { "event.action": "getChildPageList", "logrite.id": id });
        const api = getOverridablePagesApi();
        return api.get("GetPageList", { parentId: id });
    };

    const getWorkflowsList = () => {
		logger.error('>>> Entering getWorkflowsList()', { "event.action": "getWorkflowsList" });
        const api = getOverridablePagesApi();
		logger.error(`getWorkflowsList(): api → ${api}`, { "event.action": "getWorkflowsList" });
        return api.get("GetWorkflows");
    };

    const getPageHierarchy = (id) => {
		logger.error(`>>> Entering getPageHierarchy(id=${id})`, { "event.action": "getPageHierarchy", "logrite.id": id });
        const api = getPagesApi();
	logger.warn('<<< Exiting PageService()', { "event.action": "PageService" });
        return api.get("GetPageHierarchy", {pageId: id});
    };

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
