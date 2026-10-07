const logger = console;

function serializeQueryStringParameters(obj) {
	logger.error(`>>> Entering serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    let s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.warn(`<<< Exiting serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    return s.join("&");
}
let sf = null;
class Service {
    constructor(serviceFramework, moduleRoot, controller) {
		logger.error(`>>> Entering constructor(serviceFramework=${serviceFramework},moduleRoot=${moduleRoot},controller=${controller})`, { "event.action": "constructor", "logrite.serviceFramework": serviceFramework, "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        sf = serviceFramework;
        sf.moduleRoot = moduleRoot;
        sf.controller = controller;
		logger.warn(`<<< Exiting constructor(serviceFramework=${serviceFramework},moduleRoot=${moduleRoot},controller=${controller})`, { "event.action": "constructor", "logrite.serviceFramework": serviceFramework, "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
    }
    getPortalTabs(portalTabsParameters, callback, errorCallback) {
		logger.error(`>>> Entering getPortalTabs(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortalTabs", "logrite.portalTabsParameters": portalTabsParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("GetPortalTabs?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
		logger.warn(`<<< Exiting getPortalTabs(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortalTabs", "logrite.portalTabsParameters": portalTabsParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getPortalTab(portalTabParameters, callback, errorCallback) {
		logger.error(`>>> Entering getPortalTab(portalTabParameters=${portalTabParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortalTab", "logrite.portalTabParameters": portalTabParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("GetPortalTab?" + serializeQueryStringParameters(portalTabParameters), {}, callback, errorCallback);
		logger.warn(`<<< Exiting getPortalTab(portalTabParameters=${portalTabParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortalTab", "logrite.portalTabParameters": portalTabParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getTabsDescendants(portalTabsParameters, callback, errorCallback) {
		logger.error(`>>> Entering getTabsDescendants(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getTabsDescendants", "logrite.portalTabsParameters": portalTabsParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("GetTabsDescendants?" + serializeQueryStringParameters(portalTabsParameters), {}, callback, errorCallback);
		logger.warn(`<<< Exiting getTabsDescendants(portalTabsParameters=${portalTabsParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getTabsDescendants", "logrite.portalTabsParameters": portalTabsParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    searchPortalTabs(portalTabsSearchParameters, callback, errorCallback) {
		logger.error(`>>> Entering searchPortalTabs(portalTabsSearchParameters=${portalTabsSearchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "searchPortalTabs", "logrite.portalTabsSearchParameters": portalTabsSearchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("SearchPortalTabs?" + serializeQueryStringParameters(portalTabsSearchParameters), {}, callback, errorCallback);
		logger.warn(`<<< Exiting searchPortalTabs(portalTabsSearchParameters=${portalTabsSearchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "searchPortalTabs", "logrite.portalTabsSearchParameters": portalTabsSearchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
export default Service;