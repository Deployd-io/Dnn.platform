import utils from "../utils";
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

class LanguageService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = utils.getServiceFramework();
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf → ${sf}`);
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);
		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }

    getLanguages(tabId, callback) {
		logger.debug(`>>> Entering getLanguages(tabId=${tabId},callback=${callback})`);
        const sf = this.getServiceFramework("Pages");
        sf.get("GetTabLocalization?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
		logger.debug(`getLanguages(tabId=${tabId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLanguages(tabId=${tabId},callback=${callback})`);
    }

    makePageTranslatable(tabId, callback) {
		logger.debug(`>>> Entering makePageTranslatable(tabId=${tabId},callback=${callback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageTranslatable?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
		logger.debug(`makePageTranslatable(tabId=${tabId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting makePageTranslatable(tabId=${tabId},callback=${callback})`);
    }

    makePageNeutral(tabId, callback, failureCallback) {
		logger.debug(`>>> Entering makePageNeutral(tabId=${tabId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("MakePageNeutral?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback, failureCallback);
		logger.debug(`makePageNeutral(tabId=${tabId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting makePageNeutral(tabId=${tabId},callback=${callback},failureCallback=${failureCallback})`);
    }

    addMissingLanguages(tabId, callback) {
		logger.debug(`>>> Entering addMissingLanguages(tabId=${tabId},callback=${callback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("AddMissingLanguages?" + serializeQueryStringParameters({ pageId: tabId }), {}, callback);
		logger.debug(`addMissingLanguages(tabId=${tabId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addMissingLanguages(tabId=${tabId},callback=${callback})`);
    }

    notifyTranslators(params, callback) {
		logger.debug(`>>> Entering notifyTranslators(params=${params},callback=${callback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("NotifyTranslators", params, callback);
		logger.debug(`notifyTranslators(params=${params},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting notifyTranslators(params=${params},callback=${callback})`);
    }

    updateTabLocalization(params, callback, failureCallback) {
		logger.debug(`>>> Entering updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("UpdateTabLocalization", params, callback, failureCallback);
		logger.debug(`updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`);
    }
    
    restoreModule(params, callback, failureCallback) {
		logger.debug(`>>> Entering restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("RestoreModule?" + serializeQueryStringParameters(params), {}, callback, failureCallback);
		logger.debug(`restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
    }

    getContentLocalizationEnabled( callback) {
		logger.debug(`>>> Entering getContentLocalizationEnabled(callback=${callback})`);
        const sf = this.getServiceFramework("Pages");
        sf.get("GetContentLocalizationEnabled", {}, callback);
		logger.debug(`getContentLocalizationEnabled(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getContentLocalizationEnabled(callback=${callback})`);
    }

    deleteModule(params, callback, failureCallback) {
		logger.debug(`>>> Entering deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Pages");
        sf.post("DeleteModule?" + serializeQueryStringParameters(params), {}, callback, failureCallback);
		logger.debug(`deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
    }
}

const languageService = new LanguageService();
export default languageService;