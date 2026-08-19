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

    getPortalSettings(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetPortalSettings?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortalSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
    }

    updatePortalSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdatePortalSettings", payload, callback, failureCallback);
		logger.debug(`updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updatePortalSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getDefaultPagesSettings(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetDefaultPagesSettings?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getDefaultPagesSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
    }

    updateDefaultPagesSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateDefaultPagesSettings", payload, callback, failureCallback);
		logger.debug(`updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateDefaultPagesSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getMessagingSettings(portalId, callback) {
		logger.debug(`>>> Entering getMessagingSettings(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetMessagingSettings?portalId=" + portalId, {}, callback);
		logger.debug(`getMessagingSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getMessagingSettings(portalId=${portalId},callback=${callback})`);
    }

    updateMessagingSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateMessagingSettings", payload, callback, failureCallback);
		logger.debug(`updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateMessagingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getProfileSettings(portalId, callback) {
		logger.debug(`>>> Entering getProfileSettings(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfileSettings?portalId=" + portalId, {}, callback);
		logger.debug(`getProfileSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getProfileSettings(portalId=${portalId},callback=${callback})`);
    }

    updateProfileSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfileSettings", payload, callback, failureCallback);
		logger.debug(`updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateProfileSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getProfileProperties(portalId, callback) {
		logger.debug(`>>> Entering getProfileProperties(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfileProperties?portalId=" + portalId, {}, callback);
		logger.debug(`getProfileProperties(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getProfileProperties(portalId=${portalId},callback=${callback})`);
    }

    getProfileProperty(propertyId, portalId, callback) {
		logger.debug(`>>> Entering getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfileProperty?propertyId=" + propertyId + "&portalId=" + portalId, {}, callback);
		logger.debug(`getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getProfileProperty(propertyId=${propertyId},portalId=${portalId},callback=${callback})`);
    }

    getProfilePropertyLocalization(portalId, propertyName, propertyCategory, cultureCode, callback) {
		logger.debug(`>>> Entering getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetProfilePropertyLocalization?portalId=" + portalId + "&propertyName=" + propertyName + "&propertyCategory=" + propertyCategory + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getProfilePropertyLocalization(portalId=${portalId},propertyName=${propertyName},propertyCategory=${propertyCategory},cultureCode=${cultureCode},callback=${callback})`);
    }

    updateProfileProperty(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfileProperty", payload, callback, failureCallback);
		logger.debug(`updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    addProfileProperty(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddProfileProperty", payload, callback, failureCallback);
		logger.debug(`addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addProfileProperty(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    deleteProfileProperty(propertyId, callback, failureCallback) {
		logger.debug(`>>> Entering deleteProfileProperty(propertyId=${propertyId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteProfileProperty?propertyId=" + propertyId + "&portalId=", {}, callback, failureCallback);
		logger.debug(`deleteProfileProperty(propertyId=${propertyId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteProfileProperty(propertyId=${propertyId},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateProfilePropertyLocalization(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfilePropertyLocalization", payload, callback, failureCallback);
		logger.debug(`updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateProfilePropertyLocalization(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getUrlMappingSettings(portalId, callback) {
		logger.debug(`>>> Entering getUrlMappingSettings(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetUrlMappingSettings?portalId=" + portalId, {}, callback);
		logger.debug(`getUrlMappingSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUrlMappingSettings(portalId=${portalId},callback=${callback})`);
    }

    getSiteAliases(portalId, callback) {
		logger.debug(`>>> Entering getSiteAliases(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetSiteAliases?portalId=" + portalId, {}, callback);
		logger.debug(`getSiteAliases(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSiteAliases(portalId=${portalId},callback=${callback})`);
    }

    updateUrlMappingSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateUrlMappingSettings", payload, callback, failureCallback);
		logger.debug(`updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateUrlMappingSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getSiteAlias(aliasId, callback) {
		logger.debug(`>>> Entering getSiteAlias(aliasId=${aliasId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetSiteAlias?portalAliasId=" + aliasId, {}, callback);
		logger.debug(`getSiteAlias(aliasId=${aliasId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSiteAlias(aliasId=${aliasId},callback=${callback})`);
    }

    addSiteAlias(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddSiteAlias", payload, callback, failureCallback);
		logger.debug(`addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateSiteAlias(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateSiteAlias", payload, callback, failureCallback);
		logger.debug(`updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSiteAlias(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    deleteSiteAlias(aliasId, callback, failureCallback) {
		logger.debug(`>>> Entering deleteSiteAlias(aliasId=${aliasId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteSiteAlias?portalAliasId=" + aliasId, {}, callback, failureCallback);
		logger.debug(`deleteSiteAlias(aliasId=${aliasId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteSiteAlias(aliasId=${aliasId},callback=${callback},failureCallback=${failureCallback})`);
    }

    getBasicSearchSettings(callback) {
		logger.debug(`>>> Entering getBasicSearchSettings(callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetBasicSearchSettings", {}, callback);
		logger.debug(`getBasicSearchSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getBasicSearchSettings(callback=${callback})`);
    }

    updateBasicSearchSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateBasicSearchSettings", payload, callback, failureCallback);
		logger.debug(`updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getPrivacySettings(portalId, callback) {
		logger.debug(`>>> Entering getPrivacySettings(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetPrivacySettings?portalId=" + portalId, {}, callback);
		logger.debug(`getPrivacySettings(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPrivacySettings(portalId=${portalId},callback=${callback})`);
    }

    updatePrivacySettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdatePrivacySettings", payload, callback, failureCallback);
		logger.debug(`updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updatePrivacySettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    resetTermsAgreement(payload, callback, failureCallback) {
		logger.debug(`>>> Entering resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("ResetTermsAgreement", payload, callback, failureCallback);
		logger.debug(`resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting resetTermsAgreement(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    compactSearchIndex(callback, failureCallback) {
		logger.debug(`>>> Entering compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("CompactSearchIndex", {}, callback, failureCallback);
		logger.debug(`compactSearchIndex(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`);
    }

    hostSearchReindex(callback, failureCallback) {
		logger.debug(`>>> Entering hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("HostSearchReindex", {}, callback, failureCallback);
		logger.debug(`hostSearchReindex(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`);
    }

    portalSearchReindex(portalId, callback, failureCallback) {
		logger.debug(`>>> Entering portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("PortalSearchReindex?portalId=" + portalId, {}, callback, failureCallback);
		logger.debug(`portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
    }

    getSynonymsGroups(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetSynonymsGroups?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
    }

    addSynonymsGroup(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddSynonymsGroup", payload, callback, failureCallback);
		logger.debug(`addSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateSynonymsGroup(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateSynonymsGroup", payload, callback, failureCallback);
		logger.debug(`updateSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    deleteSynonymsGroup(payload, callback, failureCallback) {
		logger.debug(`>>> Entering deleteSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteSynonymsGroup", payload, callback, failureCallback);
		logger.debug(`deleteSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteSynonymsGroup(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getIgnoreWords(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetIgnoreWords?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
    }

    addIgnoreWords(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddIgnoreWords", payload, callback, failureCallback);
		logger.debug(`addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateIgnoreWords(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateIgnoreWords", payload, callback, failureCallback);
		logger.debug(`updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    deleteIgnoreWords(payload, callback, failureCallback) {
		logger.debug(`>>> Entering deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteIgnoreWords", payload, callback, failureCallback);
		logger.debug(`deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getLanguageSettings(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetLanguageSettings?portalId=" + portalId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
    }

    updateLanguageSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateLanguageSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateLanguageSettings", payload, callback, failureCallback);
		logger.debug(`updateLanguageSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateLanguageSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getLanguages(portalId, callback) {
		logger.debug(`>>> Entering getLanguages(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetLanguages?portalId=" + portalId, {}, callback);
		logger.debug(`getLanguages(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLanguages(portalId=${portalId},callback=${callback})`);
    }

    getLanguage(portalId, languageId, callback) {
		logger.debug(`>>> Entering getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetLanguage?portalId=" + portalId + "&languageId=" + languageId, {}, callback);
		logger.debug(`getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`);
    }

    getAllLanguages(callback) {
		logger.debug(`>>> Entering getAllLanguages(callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetAllLanguages", {}, callback);
		logger.debug(`getAllLanguages(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getAllLanguages(callback=${callback})`);
    }

    addLanguage(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("AddLanguage", payload, callback, failureCallback);
		logger.debug(`addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateLanguage(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateLanguage", payload, callback, failureCallback);
		logger.debug(`updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateLanguageRoles(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateLanguageRoles", payload, callback, failureCallback);
		logger.debug(`updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    verifyLanguageResourceFiles(callback) {
		logger.debug(`>>> Entering verifyLanguageResourceFiles(callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("VerifyLanguageResourceFiles", {}, callback);
		logger.debug(`verifyLanguageResourceFiles(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting verifyLanguageResourceFiles(callback=${callback})`);
    }

    getModuleList(type, callback) {
		logger.debug(`>>> Entering getModuleList(type=${type},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetModuleList?type=" + type, {}, callback);
		logger.debug(`getModuleList(type=${type},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getModuleList(type=${type},callback=${callback})`);
    }

    createLanguagePack(payload, callback, failureCallback) {
		logger.debug(`>>> Entering createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("CreateLanguagePack", payload, callback, failureCallback);
		logger.debug(`createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getCultureList(portalId, callback) {
		logger.debug(`>>> Entering getCultureList(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetCultureList?portalId=" + portalId, {}, callback);
		logger.debug(`getCultureList(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getCultureList(portalId=${portalId},callback=${callback})`);
    }

    getRoleGroups(portalId, callback) {
		logger.debug(`>>> Entering getRoleGroups(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetTranslatorRoleGroups?portalId=" + portalId, {}, callback);
		logger.debug(`getRoleGroups(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRoleGroups(portalId=${portalId},callback=${callback})`);
    }

    getRoles(portalId, groupId, cultureCode, callback) {
		logger.debug(`>>> Entering getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetTranslatorRoles?portalId=" + portalId + "&groupId=" + groupId + "&cultureCode=" + (cultureCode ? cultureCode : ""), {}, callback);
		logger.debug(`getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`);
    }

    getOtherSettings(portalId, callback) {
		logger.debug(`>>> Entering getOtherSettings(portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetOtherSettings?portalId=" + portalId, {}, callback);
		logger.debug(`getOtherSettings(portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getOtherSettings(portalId=${portalId},callback=${callback})`);
    }

    updateOtherSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateOtherSettings", payload, callback, failureCallback);
		logger.debug(`updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateOtherSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateProfilePropertyOrders(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateProfilePropertyOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateProfilePropertyOrders", payload, callback, failureCallback);
		logger.debug(`updateProfilePropertyOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateProfilePropertyOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    activateLanguage(payload, callback, failureCallback) {
		logger.debug(`>>> Entering activateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post("ActivateLanguage?" + serializeQueryStringParameters(payload), {}, callback, failureCallback);
		logger.debug(`activateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting activateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    markAllPagesAsTranslated(payload, callback, failureCallback) {
		logger.debug(`>>> Entering markAllPagesAsTranslated(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Languages");
        sf.post("MarkAllPagesTranslated?" + serializeQueryStringParameters(payload), {}, callback, failureCallback);
		logger.debug(`markAllPagesAsTranslated(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting markAllPagesAsTranslated(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getPortals(callback) {
		logger.debug(`>>> Entering getPortals(callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetPortals", {}, callback);
		logger.debug(`getPortals(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortals(callback=${callback})`);
    }   

    getListInfo(listName, portalId, callback) {
		logger.debug(`>>> Entering getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.get("GetListInfo?portalId=" + portalId + "&listName=" + listName, {}, callback);
		logger.debug(`getListInfo(listName=${listName},portalId=${portalId},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getListInfo(listName=${listName},portalId=${portalId},callback=${callback})`);
    }

    updateListEntry(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateListEntry", payload, callback, failureCallback);
		logger.debug(`updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateListEntry(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    deleteListEntry(entryId, portalId, callback, failureCallback) {
		logger.debug(`>>> Entering deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("DeleteListEntry?entryId=" + entryId + "&portalId=" + portalId, {}, callback, failureCallback);
		logger.debug(`deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteListEntry(entryId=${entryId},portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateListEntryOrders(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateListEntryOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("SiteSettings");
        sf.post("UpdateListEntryOrders", payload, callback, failureCallback);
		logger.debug(`updateListEntryOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateListEntryOrders(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;