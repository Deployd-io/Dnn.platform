import Api from "./api";
const logger = console;
const portalAliasUsageType = {
    Default: 0,
    ChildPagesInherit: 1,
    ChildPagesDoNotInherit: 2,
    InheritedFromParent: 3
};

const toBackEndUrl = function (url, tabId, primaryAliasId) {
	logger.error(`>>> Entering toBackEndUrl(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`, { "event.action": "toBackEndUrl", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
    const id = url.id  ? url.id : -1;
    let siteAliasUsage;
	logger.error(`toBackEndUrl(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId}): id → ${id}`, { "event.action": "toBackEndUrl", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
    if (url.siteAlias.Key === primaryAliasId) {
        siteAliasUsage = portalAliasUsageType.Default;
    } else if (url.siteAliasUsage === portalAliasUsageType.Default) {
        siteAliasUsage = portalAliasUsageType.ChildPagesDoNotInherit;
    } else if (url.siteAliasUsage) {
        siteAliasUsage = url.siteAliasUsage;
    } else {
        siteAliasUsage = portalAliasUsageType.ChildPagesDoNotInherit;
    }
    
	logger.warn(`<<< Exiting toBackEndUrl(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`, { "event.action": "toBackEndUrl", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
    return {
        tabId,
        saveUrl: {
            Id: id,
            SiteAliasKey: url.siteAlias.Key,
            Path: url.path,
            QueryString: url.queryString,
            LocaleKey: url.locale.Key,
            StatusCodeKey: url.statusCode.Key,
            SiteAliasUsage: siteAliasUsage,
            IsSystem: false
        }
    };
};

const PageService = function () {
	logger.error('>>> Entering PageService()', { "event.action": "PageService" });
    let api = null;
    function getApi() {
		logger.error('>>> Entering getApi()', { "event.action": "getApi" });
        if (api === null) {
			const _start_PageService_37 = Date.now();
            return new Api("Pages");
        }        
		logger.warn('<<< Exiting getApi()', { "event.action": "getApi" });
        return api;
    }

    const add = function (url, tabId, primaryAliasId) {
		logger.error(`>>> Entering add(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`, { "event.action": "add", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
        const api = getApi();
		logger.error(`add(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId}): api → ${api}`, { "event.action": "add", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
        return api.post("CreateCustomUrl", toBackEndUrl(url, tabId, primaryAliasId));
    };

    const save = function (url, tabId, primaryAliasId) {
		logger.error(`>>> Entering save(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`, { "event.action": "save", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
        const api = getApi();
		logger.error(`save(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId}): api → ${api}`, { "event.action": "save", "logrite.url": url, "logrite.tabId": tabId, "logrite.primaryAliasId": primaryAliasId });
        return api.post("UpdateCustomUrl", toBackEndUrl(url, tabId, primaryAliasId));
    };
    
    const deleteUrl = function (url, tabId) {
		logger.error(`>>> Entering deleteUrl(url=${url},tabId=${tabId})`, { "event.action": "deleteUrl", "logrite.url": url, "logrite.tabId": tabId });
        const api = getApi();
		logger.error(`deleteUrl(url=${url},tabId=${tabId}): api → ${api}`, { "event.action": "deleteUrl", "logrite.url": url, "logrite.tabId": tabId });
        return api.post("DeleteCustomUrl", {id: url.id, tabId});
    };

    const getUrls = function (pageId) {
		logger.error(`>>> Entering getUrls(pageId=${pageId})`, { "event.action": "getUrls", "logrite.pageId": pageId });
        const api = getApi();
		logger.error(`getUrls(pageId=${pageId}): api → ${api}`, { "event.action": "getUrls", "logrite.pageId": pageId });
	logger.warn('<<< Exiting PageService()', { "event.action": "PageService" });
        return api.get("GetCustomUrls", { pageId });
    };
    
    return {
        add: add,
        save: save,
        delete: deleteUrl,
        get: getUrls
    };
};

const pageService = PageService();
export default pageService;
