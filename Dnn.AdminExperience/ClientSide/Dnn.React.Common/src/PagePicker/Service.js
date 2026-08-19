const logger = console;

function serializeQueryStringParameters(obj) {
	logger.debug(`>>> Entering serializeQueryStringParameters(obj=${obj})`);
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.debug(`<<< Exiting serializeQueryStringParameters(obj=${obj})`);
    return s.join("&");
}
let sf = null;
class Service {
    constructor(serviceFramework, moduleRoot, controller) {
		logger.debug(`>>> Entering constructor(serviceFramework=${serviceFramework},moduleRoot=${moduleRoot},controller=${controller})`);
        sf = serviceFramework;
        sf.moduleRoot = moduleRoot;
		logger.debug(`constructor(serviceFramework=${serviceFramework},moduleRoot=${moduleRoot},controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`constructor(serviceFramework=${serviceFramework},moduleRoot=${moduleRoot},controller=${controller}): sf.controller → ${sf.controller}`);
		logger.debug(`<<< Exiting constructor(serviceFramework=${serviceFramework},moduleRoot=${moduleRoot},controller=${controller})`);
    }
    getPortalTabs(portalTabsParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getPortalTabs(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`);
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
		logger.debug(`<<< Exiting getPortalTabs(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    getPortalTab(portalTabParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getPortalTab(portalTabParameters=${portalTabParameters},callback=${callback},errorCallback=${errorCallback})`);
        sf.get("GetPortalTab?" + serializeQueryStringParameters(portalTabParameters), {}, callback, errorCallback);
		logger.debug(`<<< Exiting getPortalTab(portalTabParameters=${portalTabParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    getTabsDescendants(portalTabsParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getTabsDescendants(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`);
        sf.get("GetTabsDescendants?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
		logger.debug(`<<< Exiting getTabsDescendants(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    searchPortalTabs(portalTabsSearchParameters, callback, errorCallback) {
		logger.debug(`>>> Entering searchPortalTabs(portalTabsSearchParameters=${portalTabsSearchParameters},callback=${callback},errorCallback=${errorCallback})`);
        sf.get("SearchPortalTabs?" + serializeQueryStringParameters(portalTabsSearchParameters), {}, callback, errorCallback);
		logger.debug(`<<< Exiting searchPortalTabs(portalTabsSearchParameters=${portalTabsSearchParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
}
export default Service;