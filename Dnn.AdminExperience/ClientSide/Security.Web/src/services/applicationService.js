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

    getIpFilters(callback) {
		logger.error(`>>> Entering getIpFilters(callback=${callback})`, { "event.action": "getIpFilters", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetIpFilters", {}, callback);
		logger.error(`getIpFilters(callback=${callback}): sf → ${sf}`, { "event.action": "getIpFilters", "logrite.callback": callback });
    }

    getIpFilter(searchParameters, callback) {
		logger.error(`>>> Entering getIpFilter(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getIpFilter", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        searchParameters = Object.assign({}, searchParameters, {

        });
        sf.get("GetIpFilter?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.error(`getIpFilter(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "getIpFilter", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    deleteIpFilter(filterId, callback, failureCallback) {
		logger.error(`>>> Entering deleteIpFilter(filterId=${filterId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteIpFilter", "logrite.filterId": filterId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteIpFilter?filterId=" + filterId, {}, callback, failureCallback);
		logger.error(`deleteIpFilter(filterId=${filterId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteIpFilter", "logrite.filterId": filterId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getBasicLoginSettings(cultureCode, callback) {
		logger.error(`>>> Entering getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getBasicLoginSettings", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetBasicLoginSettings?cultureCode=" + cultureCode, {}, callback);
		logger.error(`getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`, { "event.action": "getBasicLoginSettings", "logrite.cultureCode": cultureCode, "logrite.callback": callback });
    }

    updateBasicLoginSettings(payload, callback) {
		logger.error(`>>> Entering updateBasicLoginSettings(payload=${payload},callback=${callback})`, { "event.action": "updateBasicLoginSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateBasicLoginSettings", payload, callback);
		logger.error(`updateBasicLoginSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateBasicLoginSettings", "logrite.payload": payload, "logrite.callback": callback });
    }

    updateIpFilter(payload, callback) {
		logger.error(`>>> Entering updateIpFilter(payload=${payload},callback=${callback})`, { "event.action": "updateIpFilter", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateIpFilter", payload, callback);
		logger.error(`updateIpFilter(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateIpFilter", "logrite.payload": payload, "logrite.callback": callback });
    }

    getMemberSettings(callback) {
		logger.error(`>>> Entering getMemberSettings(callback=${callback})`, { "event.action": "getMemberSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetMemberSettings", {}, callback);
		logger.error(`getMemberSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getMemberSettings", "logrite.callback": callback });
    }

    updateMemberSettings(payload, callback) {
		logger.error(`>>> Entering updateMemberSettings(payload=${payload},callback=${callback})`, { "event.action": "updateMemberSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateMemberSettings", payload, callback);
		logger.error(`updateMemberSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateMemberSettings", "logrite.payload": payload, "logrite.callback": callback });
    }

    getRegistrationSettings(callback) {
		logger.error(`>>> Entering getRegistrationSettings(callback=${callback})`, { "event.action": "getRegistrationSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetRegistrationSettings", {}, callback);
		logger.error(`getRegistrationSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getRegistrationSettings", "logrite.callback": callback });
    }

    updateRegistrationSettings(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateRegistrationSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateRegistrationSettings", payload, callback, failureCallback);
		logger.error(`updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateRegistrationSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getSslSettings(callback) {
		logger.error(`>>> Entering getSslSettings(callback=${callback})`, { "event.action": "getSslSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetSslSettings", {}, callback);
		logger.error(`getSslSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getSslSettings", "logrite.callback": callback });
    }

    updateSslSettings(payload, callback) {
		logger.error(`>>> Entering updateSslSettings(payload=${payload},callback=${callback})`, { "event.action": "updateSslSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateSslSettings", payload, callback);
		logger.error(`updateSslSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateSslSettings", "logrite.payload": payload, "logrite.callback": callback });
    }

    setAllPagesSecure(callback) {
		logger.error(`>>> Entering setAllPagesSecure(callback=${callback})`, { "event.action": "setAllPagesSecure", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("SetAllPagesSecure", {}, callback);
		logger.error(`setAllPagesSecure(callback=${callback}): sf → ${sf}`, { "event.action": "setAllPagesSecure", "logrite.callback": callback });
    }

    getOtherSettings(callback) {
		logger.error(`>>> Entering getOtherSettings(callback=${callback})`, { "event.action": "getOtherSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetOtherSettings", {}, callback);
		logger.error(`getOtherSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getOtherSettings", "logrite.callback": callback });
    }

    updateOtherSettings(payload, callback) {
		logger.error(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback})`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateOtherSettings", payload, callback);
		logger.error(`updateOtherSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateOtherSettings", "logrite.payload": payload, "logrite.callback": callback });
    }

    getSecurityBulletins(callback, failureCallback) {
		logger.error(`>>> Entering getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "getSecurityBulletins", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetSecurityBulletins", {}, callback, failureCallback);
		logger.error(`getSecurityBulletins(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "getSecurityBulletins", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    getSuperuserActivities(callback) {
		logger.error(`>>> Entering getSuperuserActivities(callback=${callback})`, { "event.action": "getSuperuserActivities", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetSuperuserActivities", {}, callback);
		logger.error(`getSuperuserActivities(callback=${callback}): sf → ${sf}`, { "event.action": "getSuperuserActivities", "logrite.callback": callback });
    }

    getAuditCheckResults(callback) {
		logger.error(`>>> Entering getAuditCheckResults(callback=${callback})`, { "event.action": "getAuditCheckResults", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetAuditCheckResults", {}, callback);
		logger.error(`getAuditCheckResults(callback=${callback}): sf → ${sf}`, { "event.action": "getAuditCheckResults", "logrite.callback": callback });
    }

    getAuditCheckResult(id, callback) {
		logger.error(`>>> Entering getAuditCheckResult(id=${id},callback=${callback})`, { "event.action": "getAuditCheckResult", "logrite.id": id, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetAuditCheckResult", { id: id }, callback);
		logger.error(`getAuditCheckResult(id=${id},callback=${callback}): sf → ${sf}`, { "event.action": "getAuditCheckResult", "logrite.id": id, "logrite.callback": callback });
    }

    searchFileSystemAndDatabase(searchParameters, callback) {
		logger.error(`>>> Entering searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "searchFileSystemAndDatabase", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        searchParameters = Object.assign({}, searchParameters, {

        });

		logger.error(`searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "searchFileSystemAndDatabase", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        if (this.searchRequest && this.searchRequest.readyState !== 4) {
            if (window.dnn) {
                window.dnn.loading = false;
            }
            this.searchRequest.abort();
        }

        this.searchRequest = sf.get("SearchFileSystemAndDatabase?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.warn(`<<< Exiting searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "searchFileSystemAndDatabase", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    getLastModifiedSettings(callback) {
		logger.error(`>>> Entering getLastModifiedSettings(callback=${callback})`, { "event.action": "getLastModifiedSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetLastModifiedSettings", {}, callback);
		logger.error(`getLastModifiedSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getLastModifiedSettings", "logrite.callback": callback });
    }

    getLastModifiedFiles(callback) {
		logger.error(`>>> Entering getLastModifiedFiles(callback=${callback})`, { "event.action": "getLastModifiedFiles", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetLastModifiedFiles", {}, callback);
		logger.error(`getLastModifiedFiles(callback=${callback}): sf → ${sf}`, { "event.action": "getLastModifiedFiles", "logrite.callback": callback });
    }

    getApiTokenSettings(callback) {
		logger.error(`>>> Entering getApiTokenSettings(callback=${callback})`, { "event.action": "getApiTokenSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetApiTokenSettings", {}, callback);
		logger.error(`getApiTokenSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getApiTokenSettings", "logrite.callback": callback });
    }

    updateApiTokenSettings(payload, callback) {
		logger.error(`>>> Entering updateApiTokenSettings(payload=${payload},callback=${callback})`, { "event.action": "updateApiTokenSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateApiTokenSettings", payload, callback);
		logger.error(`updateApiTokenSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateApiTokenSettings", "logrite.payload": payload, "logrite.callback": callback });
    }

    getApiTokens(portalId, filter, apiKey, scope, pageIndex, pageSize, callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetApiTokens", { portalId, filter, apiKey, scope, pageIndex, pageSize }, callback);
    }

    getApiTokenKeys(callback) {
		logger.error(`>>> Entering getApiTokenKeys(callback=${callback})`, { "event.action": "getApiTokenKeys", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetApiTokenKeys", {}, callback);
		logger.error(`getApiTokenKeys(callback=${callback}): sf → ${sf}`, { "event.action": "getApiTokenKeys", "logrite.callback": callback });
    }

    createApiToken(payload, callback) {
		logger.error(`>>> Entering createApiToken(payload=${payload},callback=${callback})`, { "event.action": "createApiToken", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("CreateApiToken", payload, callback);
		logger.error(`createApiToken(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "createApiToken", "logrite.payload": payload, "logrite.callback": callback });
    }

    revokeOrDeleteApiToken(payload, callback) {
		logger.error(`>>> Entering revokeOrDeleteApiToken(payload=${payload},callback=${callback})`, { "event.action": "revokeOrDeleteApiToken", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("RevokeOrDeleteApiToken", payload, callback);
		logger.error(`revokeOrDeleteApiToken(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "revokeOrDeleteApiToken", "logrite.payload": payload, "logrite.callback": callback });
    }

    deleteExpiredApiTokens(callback) {
		logger.error(`>>> Entering deleteExpiredApiTokens(callback=${callback})`, { "event.action": "deleteExpiredApiTokens", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteExpiredApiTokens", {}, callback);
		logger.error(`deleteExpiredApiTokens(callback=${callback}): sf → ${sf}`, { "event.action": "deleteExpiredApiTokens", "logrite.callback": callback });
    }

    deleteExpiredAndRevokedApiTokens(callback) {
		logger.error(`>>> Entering deleteExpiredAndRevokedApiTokens(callback=${callback})`, { "event.action": "deleteExpiredAndRevokedApiTokens", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteExpiredTokens", {}, callback);
		logger.error(`deleteExpiredAndRevokedApiTokens(callback=${callback}): sf → ${sf}`, { "event.action": "deleteExpiredAndRevokedApiTokens", "logrite.callback": callback });
    }

    getPortalList(addAll, callback) {
		logger.error(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
        const sf = this.getServiceFramework("Portals");
        sf.get("GetPortals?addAll=" + addAll, {}, callback);
		logger.error(`getPortalList(addAll=${addAll},callback=${callback}): sf → ${sf}`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
    }

    getCspSettings(callback) {
		logger.error(`>>> Entering getCspSettings(callback=${callback})`, { "event.action": "getCspSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.get("GetCspSettings", {}, callback);
		logger.error(`getCspSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getCspSettings", "logrite.callback": callback });
    }
    updateCspSettings(payload, callback) {
		logger.error(`>>> Entering updateCspSettings(payload=${payload},callback=${callback})`, { "event.action": "updateCspSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateCspSettings", payload, callback);
		logger.error(`updateCspSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateCspSettings", "logrite.payload": payload, "logrite.callback": callback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;