import util from "../utils";
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
class ApplicationService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = util.utilities.sf;
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }

    getRootResourcesFolder(portalId, mode, callback) {
		logger.error(`>>> Entering getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`, { "event.action": "getRootResourcesFolder", "logrite.portalId": portalId, "logrite.mode": mode, "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.get("GetRootResourcesFolders?portalId=" + portalId+"&mode=" + mode, {}, callback);
		logger.error(`getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback}): sf → ${sf}`, { "event.action": "getRootResourcesFolder", "logrite.portalId": portalId, "logrite.mode": mode, "logrite.callback": callback });
    }

    getSubRootResources(folder, callback) {
		logger.error(`>>> Entering getSubRootResources(folder=${folder},callback=${callback})`, { "event.action": "getSubRootResources", "logrite.folder": folder, "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.get("GetSubRootResources?currentFolder=" + folder, {}, callback);
		logger.error(`getSubRootResources(folder=${folder},callback=${callback}): sf → ${sf}`, { "event.action": "getSubRootResources", "logrite.folder": folder, "logrite.callback": callback });
    }

    getResxEntries(parameters, callback) {
		logger.error(`>>> Entering getResxEntries(parameters=${parameters},callback=${callback})`, { "event.action": "getResxEntries", "logrite.parameters": parameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.get("GetResxEntries?" + serializeQueryStringParameters(parameters), {}, callback);
		logger.error(`getResxEntries(parameters=${parameters},callback=${callback}): sf → ${sf}`, { "event.action": "getResxEntries", "logrite.parameters": parameters, "logrite.callback": callback });
    }

    enableLocalizedContent(parameters, callback, failureCallback) {
		logger.error(`>>> Entering enableLocalizedContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "enableLocalizedContent", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Languages");
        sf.post("EnableLocalizedContent?" + serializeQueryStringParameters(parameters), {}, callback, failureCallback);
		logger.error(`enableLocalizedContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "enableLocalizedContent", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    disableLocalizedContent(portalId, callback, failureCallback) {
		logger.error(`>>> Entering disableLocalizedContent(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "disableLocalizedContent", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Languages");
        sf.post("DisableLocalizedContent?portalId=" + portalId, {}, callback, failureCallback);
		logger.error(`disableLocalizedContent(portalId=${portalId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "disableLocalizedContent", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    localizeContent(parameters, callback, failureCallback) {
		logger.error(`>>> Entering localizeContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "localizeContent", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Languages");
        sf.post("LocalizedContent?" + serializeQueryStringParameters(parameters), {}, callback, failureCallback);
		logger.error(`localizeContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "localizeContent", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getLocalizationProgress(callback) {
		logger.error(`>>> Entering getLocalizationProgress(callback=${callback})`, { "event.action": "getLocalizationProgress", "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.getsilence("GetLocalizationProgress", {}, callback);
		logger.error(`getLocalizationProgress(callback=${callback}): sf → ${sf}`, { "event.action": "getLocalizationProgress", "logrite.callback": callback });
    }

    saveTranslations(payload, callback, failureCallback) {
		logger.error(`>>> Entering saveTranslations(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "saveTranslations", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Languages");
        sf.post("SaveResxEntries", payload, callback, failureCallback);
		logger.error(`saveTranslations(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "saveTranslations", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getPageList(payload, callback) {
		logger.error(`>>> Entering getPageList(payload=${payload},callback=${callback})`, { "event.action": "getPageList", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.getsilence("GetTabsForTranslation?" + serializeQueryStringParameters(payload), {}, callback);
		logger.error(`getPageList(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "getPageList", "logrite.payload": payload, "logrite.callback": callback });
    }

    deleteLanguagePages(payload, callback) {
		logger.error(`>>> Entering deleteLanguagePages(payload=${payload},callback=${callback})`, { "event.action": "deleteLanguagePages", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.post(`DeleteLanguagePages?${serializeQueryStringParameters(payload)}`, {}, callback);
		logger.error(`deleteLanguagePages(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "deleteLanguagePages", "logrite.payload": payload, "logrite.callback": callback });
    }

    publishAllPages(payload, callback) {
		logger.error(`>>> Entering publishAllPages(payload=${payload},callback=${callback})`, { "event.action": "publishAllPages", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Languages");
        sf.post(`PublishAllPages?${serializeQueryStringParameters(payload)}`, {}, callback);
		logger.error(`publishAllPages(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "publishAllPages", "logrite.payload": payload, "logrite.callback": callback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;