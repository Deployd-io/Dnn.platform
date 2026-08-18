import util from "../utils";
const logger = console;
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

    getGeneralSettings(callback) {
		logger.debug(`>>> Entering getGeneralSettings(callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("GetGeneralSettings", {}, callback);
		logger.debug(`getGeneralSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getGeneralSettings(callback=${callback})`);
    }

    updateGeneralSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateGeneralSettings", payload, callback, failureCallback);
		logger.debug(`updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateGeneralSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getRegexSettings(callback) {
		logger.debug(`>>> Entering getRegexSettings(callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("GetRegexSettings", {}, callback);
		logger.debug(`getRegexSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRegexSettings(callback=${callback})`);
    }

    updateRegexSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateRegexSettings", payload, callback, failureCallback);
		logger.debug(`updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateRegexSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    testUrl(pageId, queryString, customPageName, callback) {
		logger.debug(`>>> Entering testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("TestUrl?pageId=" + pageId + "&queryString=" + encodeURIComponent(queryString) + "&customPageName=" + encodeURIComponent(customPageName), {}, callback);
		logger.debug(`testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting testUrl(pageId=${pageId},queryString=${queryString},customPageName=${customPageName},callback=${callback})`);
    }

    testUrlRewrite(uri, callback) {
		logger.debug(`>>> Entering testUrlRewrite(uri=${uri},callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("TestUrlRewrite?uri=" + uri, {}, callback);
		logger.debug(`testUrlRewrite(uri=${uri},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting testUrlRewrite(uri=${uri},callback=${callback})`);
    }

    getSitemapSettings(callback) {
		logger.debug(`>>> Entering getSitemapSettings(callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("GetSitemapSettings", {}, callback);
		logger.debug(`getSitemapSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSitemapSettings(callback=${callback})`);
    }

    updateSitemapSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateSitemapSettings", payload, callback, failureCallback);
		logger.debug(`updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSitemapSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getSitemapProviders(callback) {
		logger.debug(`>>> Entering getSitemapProviders(callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("GetSitemapProviders", {}, callback);
		logger.debug(`getSitemapProviders(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSitemapProviders(callback=${callback})`);
    }

    updateSitemapProvider(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateSitemapProvider", payload, callback, failureCallback);
		logger.debug(`updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSitemapProvider(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    createVerification(verification, callback, failureCallback) {
		logger.debug(`>>> Entering createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("CreateVerification?verification=" + verification, {}, callback, failureCallback);
		logger.debug(`createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createVerification(verification=${verification},callback=${callback},failureCallback=${failureCallback})`);
    }

    clearCache(callback, failureCallback) {
		logger.debug(`>>> Entering clearCache(callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("ResetCache", {}, callback, failureCallback);
		logger.debug(`clearCache(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting clearCache(callback=${callback},failureCallback=${failureCallback})`);
    }

    getExtensionUrlProviders(callback) {
		logger.debug(`>>> Entering getExtensionUrlProviders(callback=${callback})`);
        const sf = this.getServiceFramework("SEO");
        sf.get("GetExtensionUrlProviders", {}, callback);
		logger.debug(`getExtensionUrlProviders(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getExtensionUrlProviders(callback=${callback})`);
    }

    updateExtensionUrlProviderStatus(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SEO");
        sf.post("UpdateExtensionUrlProviderStatus", payload, callback, failureCallback);
		logger.debug(`updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateExtensionUrlProviderStatus(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;