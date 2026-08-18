import util from "../utils";
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
class ApplicationService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = util.utilities.sf;
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);
		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }

    getRootResourcesFolder(portalId, mode, callback) {
		logger.debug(`>>> Entering getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.get("GetRootResourcesFolders?portalId=" + portalId+"&mode=" + mode, {}, callback);
		logger.debug(`getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`);
    }

    getSubRootResources(folder, callback) {
		logger.debug(`>>> Entering getSubRootResources(folder=${folder},callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.get("GetSubRootResources?currentFolder=" + folder, {}, callback);
		logger.debug(`getSubRootResources(folder=${folder},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSubRootResources(folder=${folder},callback=${callback})`);
    }

    getResxEntries(parameters, callback) {
		logger.debug(`>>> Entering getResxEntries(parameters=${parameters},callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.get("GetResxEntries?" + serializeQueryStringParameters(parameters), {}, callback);
		logger.debug(`getResxEntries(parameters=${parameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getResxEntries(parameters=${parameters},callback=${callback})`);
    }

    enableLocalizedContent(parameters, callback, failureCallback) {
		logger.debug(`>>> Entering enableLocalizedContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post("EnableLocalizedContent?" + serializeQueryStringParameters(parameters), {}, callback, failureCallback);
		logger.debug(`enableLocalizedContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting enableLocalizedContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback})`);
    }

    disableLocalizedContent(portalId, callback, failureCallback) {
		logger.debug(`>>> Entering disableLocalizedContent(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post("DisableLocalizedContent?portalId=" + portalId, {}, callback, failureCallback);
		logger.debug(`disableLocalizedContent(portalId=${portalId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting disableLocalizedContent(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
    }

    localizeContent(parameters, callback, failureCallback) {
		logger.debug(`>>> Entering localizeContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post("LocalizedContent?" + serializeQueryStringParameters(parameters), {}, callback, failureCallback);
		logger.debug(`localizeContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting localizeContent(parameters=${parameters},callback=${callback},failureCallback=${failureCallback})`);
    }

    getLocalizationProgress(callback) {
		logger.debug(`>>> Entering getLocalizationProgress(callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.getsilence("GetLocalizationProgress", {}, callback);
		logger.debug(`getLocalizationProgress(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLocalizationProgress(callback=${callback})`);
    }

    saveTranslations(payload, callback, failureCallback) {
		logger.debug(`>>> Entering saveTranslations(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post("SaveResxEntries", payload, callback, failureCallback);
		logger.debug(`saveTranslations(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting saveTranslations(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getPageList(payload, callback) {
		logger.debug(`>>> Entering getPageList(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.getsilence("GetTabsForTranslation?" + serializeQueryStringParameters(payload), {}, callback);
		logger.debug(`getPageList(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPageList(payload=${payload},callback=${callback})`);
    }

    deleteLanguagePages(payload, callback) {
		logger.debug(`>>> Entering deleteLanguagePages(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post(`DeleteLanguagePages?${serializeQueryStringParameters(payload)}`, {}, callback);
		logger.debug(`deleteLanguagePages(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteLanguagePages(payload=${payload},callback=${callback})`);
    }

    publishAllPages(payload, callback) {
		logger.debug(`>>> Entering publishAllPages(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post(`PublishAllPages?${serializeQueryStringParameters(payload)}`, {}, callback);
		logger.debug(`publishAllPages(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting publishAllPages(payload=${payload},callback=${callback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;