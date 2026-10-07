import util from "../utils";
const logger = console;
class ApplicationService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = util.utilities.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }

    getGeneralSettings(callback) {
		logger.error(`>>> Entering getGeneralSettings(callback=${callback})`, { "event.action": "getGeneralSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("GetGeneralSettings", {}, callback);
		logger.error(`getGeneralSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getGeneralSettings", "logrite.callback": callback });
    }

    updateGeneralSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateGeneralSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateGeneralSettings", payload, callback, failureCallback);
		logger.error(`updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateGeneralSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getRegexSettings(callback) {
		logger.error(`>>> Entering getRegexSettings(callback=${callback})`, { "event.action": "getRegexSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("GetRegexSettings", {}, callback);
		logger.error(`getRegexSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getRegexSettings", "logrite.callback": callback });
    }

    updateRegexSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateRegexSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateRegexSettings", payload, callback, failureCallback);
		logger.error(`updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateRegexSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    testUrl(pageId, queryString, customPageName, callback) {
		logger.error(`>>> Entering testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`, { "event.action": "testUrl", "logrite.pageId": pageId, "logrite.queryString": queryString, "logrite.customPageName": customPageName, "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("TestUrl?pageId=" + pageId + "&queryString=" + encodeURIComponent(queryString) + "&customPageName=" + encodeURIComponent(customPageName), {}, callback);
		logger.error(`testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback}): sf → ${sf}`, { "event.action": "testUrl", "logrite.pageId": pageId, "logrite.queryString": queryString, "logrite.customPageName": customPageName, "logrite.callback": callback });
    }

    testUrlRewrite(uri, callback) {
		logger.error(`>>> Entering testUrlRewrite(uri=${uri},callback=${callback})`, { "event.action": "testUrlRewrite", "logrite.uri": uri, "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("TestUrlRewrite?uri=" + uri, {}, callback);
		logger.error(`testUrlRewrite(uri=${uri},callback=${callback}): sf → ${sf}`, { "event.action": "testUrlRewrite", "logrite.uri": uri, "logrite.callback": callback });
    }

    getSitemapSettings(callback) {
		logger.error(`>>> Entering getSitemapSettings(callback=${callback})`, { "event.action": "getSitemapSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("GetSitemapSettings", {}, callback);
		logger.error(`getSitemapSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getSitemapSettings", "logrite.callback": callback });
    }

    updateSitemapSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSitemapSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateSitemapSettings", payload, callback, failureCallback);
		logger.error(`updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateSitemapSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getSitemapProviders(callback) {
		logger.error(`>>> Entering getSitemapProviders(callback=${callback})`, { "event.action": "getSitemapProviders", "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("GetSitemapProviders", {}, callback);
		logger.error(`getSitemapProviders(callback=${callback}): sf → ${sf}`, { "event.action": "getSitemapProviders", "logrite.callback": callback });
    }

    updateSitemapProvider(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSitemapProvider", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateSitemapProvider", payload, callback, failureCallback);
		logger.error(`updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateSitemapProvider", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    createVerification(verification, callback, failureCallback) {
		logger.error(`>>> Entering createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createVerification", "logrite.verification": verification, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("CreateVerification?verification=" + verification, {}, callback, failureCallback);
		logger.error(`createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "createVerification", "logrite.verification": verification, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    clearCache(callback, failureCallback) {
		logger.error(`>>> Entering clearCache(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "clearCache", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("ResetCache", {}, callback, failureCallback);
		logger.error(`clearCache(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "clearCache", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getExtensionUrlProviders(callback) {
		logger.error(`>>> Entering getExtensionUrlProviders(callback=${callback})`, { "event.action": "getExtensionUrlProviders", "logrite.callback": callback });
        const sf = this.getServiceFramework("SEO");
        sf.get("GetExtensionUrlProviders", {}, callback);
		logger.error(`getExtensionUrlProviders(callback=${callback}): sf → ${sf}`, { "event.action": "getExtensionUrlProviders", "logrite.callback": callback });
    }

    updateExtensionUrlProviderStatus(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateExtensionUrlProviderStatus", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateExtensionUrlProviderStatus", payload, callback, failureCallback);
		logger.error(`updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateExtensionUrlProviderStatus", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;