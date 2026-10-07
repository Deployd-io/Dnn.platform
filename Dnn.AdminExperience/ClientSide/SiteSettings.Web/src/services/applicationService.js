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

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }

    getPortalSettings(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getPortalSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetPortalSettings?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getPortalSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    updatePortalSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updatePortalSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdatePortalSettings", payload, callback, failureCallback);
		logger.error(`updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updatePortalSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getDefaultPagesSettings(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getDefaultPagesSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetDefaultPagesSettings?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getDefaultPagesSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    updateDefaultPagesSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateDefaultPagesSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateDefaultPagesSettings", payload, callback, failureCallback);
		logger.error(`updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateDefaultPagesSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getMessagingSettings(portalId, callback) {
		logger.error(`>>> Entering getMessagingSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getMessagingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetMessagingSettings?portalId=" + portalId, {}, callback);
		logger.error(`getMessagingSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getMessagingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    updateMessagingSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateMessagingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateMessagingSettings", payload, callback, failureCallback);
		logger.error(`updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateMessagingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getProfileSettings(portalId, callback) {
		logger.error(`>>> Entering getProfileSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getProfileSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfileSettings?portalId=" + portalId, {}, callback);
		logger.error(`getProfileSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getProfileSettings", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    updateProfileSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfileSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfileSettings", payload, callback, failureCallback);
		logger.error(`updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateProfileSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getProfileProperties(portalId, callback) {
		logger.error(`>>> Entering getProfileProperties(portalId=${portalId},callback=${callback})`, { "event.action": "getProfileProperties", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfileProperties?portalId=" + portalId, {}, callback);
		logger.error(`getProfileProperties(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getProfileProperties", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    getProfileProperty(propertyId, portalId, callback) {
		logger.error(`>>> Entering getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`, { "event.action": "getProfileProperty", "logrite.propertyId": propertyId, "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfileProperty?propertyId=" + propertyId + "&portalId=" + portalId, {}, callback);
		logger.error(`getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getProfileProperty", "logrite.propertyId": propertyId, "logrite.portalId": portalId, "logrite.callback": callback });
    }

    getProfilePropertyLocalization(portalId, propertyName, propertyCategory, cultureCode, callback) {
		logger.error(`>>> Entering getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getProfilePropertyLocalization", "logrite.portalId": portalId, "logrite.propertyName": propertyName, "logrite.propertyCategory": propertyCategory, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfilePropertyLocalization?portalId=" + portalId + "&propertyName=" + propertyName + "&propertyCategory=" + propertyCategory + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getProfilePropertyLocalization", "logrite.portalId": portalId, "logrite.propertyName": propertyName, "logrite.propertyCategory": propertyCategory, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    updateProfileProperty(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfileProperty", payload, callback, failureCallback);
		logger.error(`updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    addProfileProperty(payload, callback, failureCallback) {
		logger.error(`>>> Entering addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddProfileProperty", payload, callback, failureCallback);
		logger.error(`addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addProfileProperty", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    deleteProfileProperty(propertyId, callback, failureCallback) {
		logger.error(`>>> Entering deleteProfileProperty(propertyId=${propertyId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteProfileProperty", "logrite.propertyId": propertyId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteProfileProperty?propertyId=" + propertyId + "&portalId=", {}, callback, failureCallback);
		logger.error(`deleteProfileProperty(propertyId=${propertyId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteProfileProperty", "logrite.propertyId": propertyId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateProfilePropertyLocalization(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfilePropertyLocalization", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfilePropertyLocalization", payload, callback, failureCallback);
		logger.error(`updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateProfilePropertyLocalization", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getUrlMappingSettings(portalId, callback) {
		logger.error(`>>> Entering getUrlMappingSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getUrlMappingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetUrlMappingSettings?portalId=" + portalId, {}, callback);
		logger.error(`getUrlMappingSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getUrlMappingSettings", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    getSiteAliases(portalId, callback) {
		logger.error(`>>> Entering getSiteAliases(portalId=${portalId},callback=${callback})`, { "event.action": "getSiteAliases", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetSiteAliases?portalId=" + portalId, {}, callback);
		logger.error(`getSiteAliases(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getSiteAliases", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    updateUrlMappingSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateUrlMappingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateUrlMappingSettings", payload, callback, failureCallback);
		logger.error(`updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateUrlMappingSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getSiteAlias(aliasId, callback) {
		logger.error(`>>> Entering getSiteAlias(aliasId=${aliasId},callback=${callback})`, { "event.action": "getSiteAlias", "logrite.aliasId": aliasId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetSiteAlias?portalAliasId=" + aliasId, {}, callback);
		logger.error(`getSiteAlias(aliasId=${aliasId},callback=${callback}): sf → ${sf}`, { "event.action": "getSiteAlias", "logrite.aliasId": aliasId, "logrite.callback": callback });
    }

    addSiteAlias(payload, callback, failureCallback) {
		logger.error(`>>> Entering addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddSiteAlias", payload, callback, failureCallback);
		logger.error(`addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateSiteAlias(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateSiteAlias", payload, callback, failureCallback);
		logger.error(`updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateSiteAlias", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    deleteSiteAlias(aliasId, callback, failureCallback) {
		logger.error(`>>> Entering deleteSiteAlias(aliasId=${aliasId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteSiteAlias", "logrite.aliasId": aliasId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteSiteAlias?portalAliasId=" + aliasId, {}, callback, failureCallback);
		logger.error(`deleteSiteAlias(aliasId=${aliasId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteSiteAlias", "logrite.aliasId": aliasId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getBasicSearchSettings(callback) {
		logger.error(`>>> Entering getBasicSearchSettings(callback=${callback})`, { "event.action": "getBasicSearchSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetBasicSearchSettings", {}, callback);
		logger.error(`getBasicSearchSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getBasicSearchSettings", "logrite.callback": callback });
    }

    updateBasicSearchSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateBasicSearchSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateBasicSearchSettings", payload, callback, failureCallback);
		logger.error(`updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateBasicSearchSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getPrivacySettings(portalId, callback) {
		logger.error(`>>> Entering getPrivacySettings(portalId=${portalId},callback=${callback})`, { "event.action": "getPrivacySettings", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetPrivacySettings?portalId=" + portalId, {}, callback);
		logger.error(`getPrivacySettings(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getPrivacySettings", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    updatePrivacySettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updatePrivacySettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdatePrivacySettings", payload, callback, failureCallback);
		logger.error(`updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updatePrivacySettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    resetTermsAgreement(payload, callback, failureCallback) {
		logger.error(`>>> Entering resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "resetTermsAgreement", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("ResetTermsAgreement", payload, callback, failureCallback);
		logger.error(`resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "resetTermsAgreement", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    compactSearchIndex(callback, failureCallback) {
		logger.error(`>>> Entering compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "compactSearchIndex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("CompactSearchIndex", {}, callback, failureCallback);
		logger.error(`compactSearchIndex(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "compactSearchIndex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    hostSearchReindex(callback, failureCallback) {
		logger.error(`>>> Entering hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "hostSearchReindex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("HostSearchReindex", {}, callback, failureCallback);
		logger.error(`hostSearchReindex(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "hostSearchReindex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    portalSearchReindex(portalId, callback, failureCallback) {
		logger.error(`>>> Entering portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "portalSearchReindex", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("PortalSearchReindex?portalId=" + portalId, {}, callback, failureCallback);
		logger.error(`portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "portalSearchReindex", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getSynonymsGroups(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getSynonymsGroups", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetSynonymsGroups?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getSynonymsGroups", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    addSynonymsGroup(payload, callback, failureCallback) {
		logger.error(`>>> Entering addSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addSynonymsGroup", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddSynonymsGroup", payload, callback, failureCallback);
		logger.error(`addSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addSynonymsGroup", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateSynonymsGroup(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSynonymsGroup", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateSynonymsGroup", payload, callback, failureCallback);
		logger.error(`updateSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateSynonymsGroup", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    deleteSynonymsGroup(payload, callback, failureCallback) {
		logger.error(`>>> Entering deleteSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteSynonymsGroup", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteSynonymsGroup", payload, callback, failureCallback);
		logger.error(`deleteSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteSynonymsGroup", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getIgnoreWords(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getIgnoreWords", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetIgnoreWords?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getIgnoreWords", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    addIgnoreWords(payload, callback, failureCallback) {
		logger.error(`>>> Entering addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddIgnoreWords", payload, callback, failureCallback);
		logger.error(`addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateIgnoreWords(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateIgnoreWords", payload, callback, failureCallback);
		logger.error(`updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    deleteIgnoreWords(payload, callback, failureCallback) {
		logger.error(`>>> Entering deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteIgnoreWords", payload, callback, failureCallback);
		logger.error(`deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getLanguageSettings(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getLanguageSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetLanguageSettings?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getLanguageSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    updateLanguageSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateLanguageSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguageSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateLanguageSettings", payload, callback, failureCallback);
		logger.error(`updateLanguageSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateLanguageSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getLanguages(portalId, callback) {
		logger.error(`>>> Entering getLanguages(portalId=${portalId},callback=${callback})`, { "event.action": "getLanguages", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetLanguages?portalId=" + portalId, {}, callback);
		logger.error(`getLanguages(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getLanguages", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    getLanguage(portalId, languageId, callback) {
		logger.error(`>>> Entering getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`, { "event.action": "getLanguage", "logrite.portalId": portalId, "logrite.languageId": languageId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetLanguage?portalId=" + portalId + "&languageId=" + languageId, {}, callback);
		logger.error(`getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback}): sf → ${sf}`, { "event.action": "getLanguage", "logrite.portalId": portalId, "logrite.languageId": languageId, "logrite.callback": callback });
    }

    getAllLanguages(callback) {
		logger.error(`>>> Entering getAllLanguages(callback=${callback})`, { "event.action": "getAllLanguages", "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetAllLanguages", {}, callback);
		logger.error(`getAllLanguages(callback=${callback}): sf → ${sf}`, { "event.action": "getAllLanguages", "logrite.callback": callback });
    }

    addLanguage(payload, callback, failureCallback) {
		logger.error(`>>> Entering addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddLanguage", payload, callback, failureCallback);
		logger.error(`addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateLanguage(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateLanguage", payload, callback, failureCallback);
		logger.error(`updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateLanguageRoles(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguageRoles", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateLanguageRoles", payload, callback, failureCallback);
		logger.error(`updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateLanguageRoles", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    verifyLanguageResourceFiles(callback) {
		logger.error(`>>> Entering verifyLanguageResourceFiles(callback=${callback})`, { "event.action": "verifyLanguageResourceFiles", "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("VerifyLanguageResourceFiles", {}, callback);
		logger.error(`verifyLanguageResourceFiles(callback=${callback}): sf → ${sf}`, { "event.action": "verifyLanguageResourceFiles", "logrite.callback": callback });
    }

    getModuleList(type, callback) {
		logger.error(`>>> Entering getModuleList(type=${type},callback=${callback})`, { "event.action": "getModuleList", "logrite.type": type, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetModuleList?type=" + type, {}, callback);
		logger.error(`getModuleList(type=${type},callback=${callback}): sf → ${sf}`, { "event.action": "getModuleList", "logrite.type": type, "logrite.callback": callback });
    }

    createLanguagePack(payload, callback, failureCallback) {
		logger.error(`>>> Entering createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createLanguagePack", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("CreateLanguagePack", payload, callback, failureCallback);
		logger.error(`createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "createLanguagePack", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getCultureList(portalId, callback) {
		logger.error(`>>> Entering getCultureList(portalId=${portalId},callback=${callback})`, { "event.action": "getCultureList", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetCultureList?portalId=" + portalId, {}, callback);
		logger.error(`getCultureList(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getCultureList", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    getRoleGroups(portalId, callback) {
		logger.error(`>>> Entering getRoleGroups(portalId=${portalId},callback=${callback})`, { "event.action": "getRoleGroups", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetTranslatorRoleGroups?portalId=" + portalId, {}, callback);
		logger.error(`getRoleGroups(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getRoleGroups", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    getRoles(portalId, groupId, cultureCode, callback) {
		logger.error(`>>> Entering getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getRoles", "logrite.portalId": portalId, "logrite.groupId": groupId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetTranslatorRoles?portalId=" + portalId + "&groupId=" + groupId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.error(`getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getRoles", "logrite.portalId": portalId, "logrite.groupId": groupId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    getOtherSettings(portalId, callback) {
		logger.error(`>>> Entering getOtherSettings(portalId=${portalId},callback=${callback})`, { "event.action": "getOtherSettings", "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetOtherSettings?portalId=" + portalId, {}, callback);
		logger.error(`getOtherSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getOtherSettings", "logrite.portalId": portalId, "logrite.callback": callback });
    }

    updateOtherSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateOtherSettings", payload, callback, failureCallback);
		logger.error(`updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateProfilePropertyOrders(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateProfilePropertyOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateProfilePropertyOrders", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfilePropertyOrders", payload, callback, failureCallback);
		logger.error(`updateProfilePropertyOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateProfilePropertyOrders", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    activateLanguage(payload, callback, failureCallback) {
		logger.error(`>>> Entering activateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "activateLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Languages");
        sf.post("ActivateLanguage?" + serializeQueryStringParameters(payload), {}, callback, failureCallback);
		logger.error(`activateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "activateLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    markAllPagesAsTranslated(payload, callback, failureCallback) {
		logger.error(`>>> Entering markAllPagesAsTranslated(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "markAllPagesAsTranslated", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Languages");
        sf.post("MarkAllPagesTranslated?" + serializeQueryStringParameters(payload), {}, callback, failureCallback);
		logger.error(`markAllPagesAsTranslated(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "markAllPagesAsTranslated", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getPortals(callback) {
		logger.error(`>>> Entering getPortals(callback=${callback})`, { "event.action": "getPortals", "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetPortals", {}, callback);
		logger.error(`getPortals(callback=${callback}): sf → ${sf}`, { "event.action": "getPortals", "logrite.callback": callback });
    }   

    getListInfo(listName, portalId, callback) {
		logger.error(`>>> Entering getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`, { "event.action": "getListInfo", "logrite.listName": listName, "logrite.portalId": portalId, "logrite.callback": callback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetListInfo?portalId=" + portalId + "&listName=" + listName, {}, callback);
		logger.error(`getListInfo(listName=${listName},portalId=${portalId},callback=${callback}): sf → ${sf}`, { "event.action": "getListInfo", "logrite.listName": listName, "logrite.portalId": portalId, "logrite.callback": callback });
    }

    updateListEntry(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateListEntry", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateListEntry", payload, callback, failureCallback);
		logger.error(`updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateListEntry", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    deleteListEntry(entryId, portalId, callback, failureCallback) {
		logger.error(`>>> Entering deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteListEntry", "logrite.entryId": entryId, "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteListEntry?entryId=" + entryId + "&portalId=" + portalId, {}, callback, failureCallback);
		logger.error(`deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteListEntry", "logrite.entryId": entryId, "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateListEntryOrders(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateListEntryOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateListEntryOrders", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateListEntryOrders", payload, callback, failureCallback);
		logger.error(`updateListEntryOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateListEntryOrders", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;