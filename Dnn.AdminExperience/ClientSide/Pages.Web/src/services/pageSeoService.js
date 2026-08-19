import Api from "./api";
const logger = console;
const portalAliasUsageType = {
    Default: 0,
    ChildPagesInherit: 1,
    ChildPagesDoNotInherit: 2,
    InheritedFromParent: 3
};

const toBackEndUrl = function (url, tabId, primaryAliasId) {
	logger.debug(`>>> Entering toBackEndUrl(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`);
    const id = url.id  ? url.id : -1;
    let siteAliasUsage;
	logger.debug(`toBackEndUrl(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId}): id → ${id}`);
    if (url.siteAlias.Key === primaryAliasId) {
        siteAliasUsage = portalAliasUsageType.Default;
    } else if (url.siteAliasUsage === portalAliasUsageType.Default) {
        siteAliasUsage = portalAliasUsageType.ChildPagesDoNotInherit;
    } else if (url.siteAliasUsage) {
        siteAliasUsage = url.siteAliasUsage;
    } else {
        siteAliasUsage = portalAliasUsageType.ChildPagesDoNotInherit;
    }
    
	logger.debug(`<<< Exiting toBackEndUrl(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`);
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
	logger.debug('>>> Entering PageService()'); const _start = Date.now();
    let api = null;
    function getApi() {
		logger.debug('>>> Entering getApi()');
        if (api === null) {
		logger.debug('<<< Exiting getApi()');
            return new Api("Pages");
        }        
        return api;
    }

    const add = function (url, tabId, primaryAliasId) {
		logger.debug(`>>> Entering add(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`); const _start = Date.now();
        const api = getApi();
		logger.debug(`add(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId}): api → ${api}`);
		logger.info(`add(url=${url}, tabId=${tabId}, primaryAliasId=${primaryAliasId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting add(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`);
        return api.post("CreateCustomUrl", toBackEndUrl(url, tabId, primaryAliasId));
    };

    const save = function (url, tabId, primaryAliasId) {
		logger.debug(`>>> Entering save(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`); const _start = Date.now();
        const api = getApi();
		logger.debug(`save(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId}): api → ${api}`);
		logger.info(`save(url=${url}, tabId=${tabId}, primaryAliasId=${primaryAliasId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting save(url=${url},tabId=${tabId},primaryAliasId=${primaryAliasId})`);
        return api.post("UpdateCustomUrl", toBackEndUrl(url, tabId, primaryAliasId));
    };
    
    const deleteUrl = function (url, tabId) {
		logger.debug(`>>> Entering deleteUrl(url=${url},tabId=${tabId})`); const _start = Date.now();
        const api = getApi();
		logger.debug(`deleteUrl(url=${url},tabId=${tabId}): api → ${api}`);
		logger.info(`deleteUrl(url=${url}, tabId=${tabId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting deleteUrl(url=${url},tabId=${tabId})`);
        return api.post("DeleteCustomUrl", {id: url.id, tabId});
    };

    const getUrls = function (pageId) {
		logger.debug(`>>> Entering getUrls(pageId=${pageId})`); const _start = Date.now();
        const api = getApi();
		logger.debug(`getUrls(pageId=${pageId}): api → ${api}`);
		logger.info(`getUrls(pageId=${pageId}): API call took ${Date.now() - _start} ms`);
		logger.debug(`<<< Exiting getUrls(pageId=${pageId})`);
        return api.get("GetCustomUrls", { pageId });
    };
    
	logger.info(`PageService(): API call took ${Date.now() - _start} ms`);
	logger.debug('<<< Exiting PageService()');
    return {
        add: add,
        save: save,
        delete: deleteUrl,
        get: getUrls
    };
};

const pageService = PageService();
export default pageService;
