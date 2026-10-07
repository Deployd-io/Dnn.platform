import utils from "../utils";
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

class LanguageService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = utils.getServiceFramework();
        sf.moduleRoot = "PersonaBar";
		logger.error(`getServiceFramework(controller=${controller}): sf → ${sf}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;
		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }

    getLanguages(tabId, callback) {
		logger.error(`>>> Entering getLanguages(tabId=${tabId},callback=${callback})`, { "event.action": "getLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
        const sf = this.getServiceFramework("Pages");
        sf.get("GetTabLocalization?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
		logger.error(`getLanguages(tabId=${tabId},callback=${callback}): sf → ${sf}`, { "event.action": "getLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
    }

    makePageTranslatable(tabId, callback) {
		logger.error(`>>> Entering makePageTranslatable(tabId=${tabId},callback=${callback})`, { "event.action": "makePageTranslatable", "logrite.tabId": tabId, "logrite.callback": callback });
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageTranslatable?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
		logger.error(`makePageTranslatable(tabId=${tabId},callback=${callback}): sf → ${sf}`, { "event.action": "makePageTranslatable", "logrite.tabId": tabId, "logrite.callback": callback });
    }

    makePageNeutral(tabId, callback, failureCallback) {
		logger.error(`>>> Entering makePageNeutral(tabId=${tabId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "makePageNeutral", "logrite.tabId": tabId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageNeutral?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback, failureCallback);
		logger.error(`makePageNeutral(tabId=${tabId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "makePageNeutral", "logrite.tabId": tabId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    addMissingLanguages(tabId, callback) {
		logger.error(`>>> Entering addMissingLanguages(tabId=${tabId},callback=${callback})`, { "event.action": "addMissingLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
        const sf = this.getServiceFramework("Pages");
        sf.post("AddMissingLanguages?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
		logger.error(`addMissingLanguages(tabId=${tabId},callback=${callback}): sf → ${sf}`, { "event.action": "addMissingLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
    }

    notifyTranslators(params, callback) {
		logger.error(`>>> Entering notifyTranslators(params=${params},callback=${callback})`, { "event.action": "notifyTranslators", "logrite.params": params, "logrite.callback": callback });
        const sf = this.getServiceFramework("Pages");
        sf.post("NotifyTranslators", params, callback);
		logger.error(`notifyTranslators(params=${params},callback=${callback}): sf → ${sf}`, { "event.action": "notifyTranslators", "logrite.params": params, "logrite.callback": callback });
    }

    updateTabLocalization(params, callback, failureCallback) {
		logger.error(`>>> Entering updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateTabLocalization", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Pages");
        sf.post("UpdateTabLocalization", params, callback, failureCallback);
		logger.error(`updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateTabLocalization", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    
    restoreModule(params, callback, failureCallback) {
		logger.error(`>>> Entering restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "restoreModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Pages");
        sf.post("RestoreModule?" + serializeQueryStringParameters(params), {}, callback, failureCallback);
		logger.error(`restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "restoreModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getContentLocalizationEnabled( callback) {
		logger.error(`>>> Entering getContentLocalizationEnabled(callback=${callback})`, { "event.action": "getContentLocalizationEnabled", "logrite.callback": callback });
        const sf = this.getServiceFramework("Pages");
        sf.get("GetContentLocalizationEnabled", {}, callback);
		logger.error(`getContentLocalizationEnabled(callback=${callback}): sf → ${sf}`, { "event.action": "getContentLocalizationEnabled", "logrite.callback": callback });
    }

    deleteModule(params, callback, failureCallback) {
		logger.error(`>>> Entering deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Pages");
        sf.post("DeleteModule?" + serializeQueryStringParameters(params), {}, callback, failureCallback);
		logger.error(`deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}

const languageService = new LanguageService();
export default languageService;