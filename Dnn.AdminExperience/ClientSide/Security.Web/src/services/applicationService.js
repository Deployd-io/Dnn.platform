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

    getIpFilters(callback) {
		logger.debug(`>>> Entering getIpFilters(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetIpFilters", {}, callback);
		logger.debug(`getIpFilters(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getIpFilters(callback=${callback})`);
    }

    getIpFilter(searchParameters, callback) {
		logger.debug(`>>> Entering getIpFilter(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        searchParameters = Object.assign({}, searchParameters, {

        });
        sf.get("GetIpFilter?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`getIpFilter(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getIpFilter(searchParameters=${searchParameters},callback=${callback})`);
    }

    deleteIpFilter(filterId, callback, failureCallback) {
		logger.debug(`>>> Entering deleteIpFilter(filterId=${filterId},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteIpFilter?filterId=" + filterId, {}, callback, failureCallback);
		logger.debug(`deleteIpFilter(filterId=${filterId},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteIpFilter(filterId=${filterId},callback=${callback},failureCallback=${failureCallback})`);
    }

    getBasicLoginSettings(cultureCode, callback) {
		logger.debug(`>>> Entering getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetBasicLoginSettings?cultureCode=" + cultureCode, {}, callback);
		logger.debug(`getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getBasicLoginSettings(cultureCode=${cultureCode},callback=${callback})`);
    }

    updateBasicLoginSettings(payload, callback) {
		logger.debug(`>>> Entering updateBasicLoginSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateBasicLoginSettings", payload, callback);
		logger.debug(`updateBasicLoginSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateBasicLoginSettings(payload=${payload},callback=${callback})`);
    }

    updateIpFilter(payload, callback) {
		logger.debug(`>>> Entering updateIpFilter(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateIpFilter", payload, callback);
		logger.debug(`updateIpFilter(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateIpFilter(payload=${payload},callback=${callback})`);
    }

    getMemberSettings(callback) {
		logger.debug(`>>> Entering getMemberSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetMemberSettings", {}, callback);
		logger.debug(`getMemberSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getMemberSettings(callback=${callback})`);
    }

    updateMemberSettings(payload, callback) {
		logger.debug(`>>> Entering updateMemberSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateMemberSettings", payload, callback);
		logger.debug(`updateMemberSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateMemberSettings(payload=${payload},callback=${callback})`);
    }

    getRegistrationSettings(callback) {
		logger.debug(`>>> Entering getRegistrationSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetRegistrationSettings", {}, callback);
		logger.debug(`getRegistrationSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getRegistrationSettings(callback=${callback})`);
    }

    updateRegistrationSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateRegistrationSettings", payload, callback, failureCallback);
		logger.debug(`updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateRegistrationSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    getSslSettings(callback) {
		logger.debug(`>>> Entering getSslSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetSslSettings", {}, callback);
		logger.debug(`getSslSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSslSettings(callback=${callback})`);
    }

    updateSslSettings(payload, callback) {
		logger.debug(`>>> Entering updateSslSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateSslSettings", payload, callback);
		logger.debug(`updateSslSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSslSettings(payload=${payload},callback=${callback})`);
    }

    setAllPagesSecure(callback) {
		logger.debug(`>>> Entering setAllPagesSecure(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("SetAllPagesSecure", {}, callback);
		logger.debug(`setAllPagesSecure(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting setAllPagesSecure(callback=${callback})`);
    }

    getOtherSettings(callback) {
		logger.debug(`>>> Entering getOtherSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetOtherSettings", {}, callback);
		logger.debug(`getOtherSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getOtherSettings(callback=${callback})`);
    }

    updateOtherSettings(payload, callback) {
		logger.debug(`>>> Entering updateOtherSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateOtherSettings", payload, callback);
		logger.debug(`updateOtherSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateOtherSettings(payload=${payload},callback=${callback})`);
    }

    getSecurityBulletins(callback, failureCallback) {
		logger.debug(`>>> Entering getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetSecurityBulletins", {}, callback, failureCallback);
		logger.debug(`getSecurityBulletins(callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSecurityBulletins(callback=${callback},failureCallback=${failureCallback})`);
    }

    getSuperuserActivities(callback) {
		logger.debug(`>>> Entering getSuperuserActivities(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetSuperuserActivities", {}, callback);
		logger.debug(`getSuperuserActivities(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSuperuserActivities(callback=${callback})`);
    }

    getAuditCheckResults(callback) {
		logger.debug(`>>> Entering getAuditCheckResults(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetAuditCheckResults", {}, callback);
		logger.debug(`getAuditCheckResults(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getAuditCheckResults(callback=${callback})`);
    }

    getAuditCheckResult(id, callback) {
		logger.debug(`>>> Entering getAuditCheckResult(id=${id},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetAuditCheckResult", { id: id }, callback);
		logger.debug(`getAuditCheckResult(id=${id},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getAuditCheckResult(id=${id},callback=${callback})`);
    }

    searchFileSystemAndDatabase(searchParameters, callback) {
		logger.debug(`>>> Entering searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        searchParameters = Object.assign({}, searchParameters, {

        });

		logger.debug(`searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
        if (this.searchRequest && this.searchRequest.readyState !== 4) {
            if (window.dnn) {
                window.dnn.loading = false;
            }
            this.searchRequest.abort();
        }

        this.searchRequest = sf.get("SearchFileSystemAndDatabase?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`<<< Exiting searchFileSystemAndDatabase(searchParameters=${searchParameters},callback=${callback})`);
    }

    getLastModifiedSettings(callback) {
		logger.debug(`>>> Entering getLastModifiedSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetLastModifiedSettings", {}, callback);
		logger.debug(`getLastModifiedSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLastModifiedSettings(callback=${callback})`);
    }

    getLastModifiedFiles(callback) {
		logger.debug(`>>> Entering getLastModifiedFiles(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetLastModifiedFiles", {}, callback);
		logger.debug(`getLastModifiedFiles(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLastModifiedFiles(callback=${callback})`);
    }

    getApiTokenSettings(callback) {
		logger.debug(`>>> Entering getApiTokenSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetApiTokenSettings", {}, callback);
		logger.debug(`getApiTokenSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getApiTokenSettings(callback=${callback})`);
    }

    updateApiTokenSettings(payload, callback) {
		logger.debug(`>>> Entering updateApiTokenSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateApiTokenSettings", payload, callback);
		logger.debug(`updateApiTokenSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateApiTokenSettings(payload=${payload},callback=${callback})`);
    }

    getApiTokens(portalId, filter, apiKey, scope, pageIndex, pageSize, callback) {
        const sf = this.getServiceFramework("Security");
        sf.get("GetApiTokens", { portalId, filter, apiKey, scope, pageIndex, pageSize }, callback);
    }

    getApiTokenKeys(callback) {
		logger.debug(`>>> Entering getApiTokenKeys(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetApiTokenKeys", {}, callback);
		logger.debug(`getApiTokenKeys(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getApiTokenKeys(callback=${callback})`);
    }

    createApiToken(payload, callback) {
		logger.debug(`>>> Entering createApiToken(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("CreateApiToken", payload, callback);
		logger.debug(`createApiToken(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createApiToken(payload=${payload},callback=${callback})`);
    }

    revokeOrDeleteApiToken(payload, callback) {
		logger.debug(`>>> Entering revokeOrDeleteApiToken(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("RevokeOrDeleteApiToken", payload, callback);
		logger.debug(`revokeOrDeleteApiToken(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting revokeOrDeleteApiToken(payload=${payload},callback=${callback})`);
    }

    deleteExpiredApiTokens(callback) {
		logger.debug(`>>> Entering deleteExpiredApiTokens(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteExpiredApiTokens", {}, callback);
		logger.debug(`deleteExpiredApiTokens(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteExpiredApiTokens(callback=${callback})`);
    }

    deleteExpiredAndRevokedApiTokens(callback) {
		logger.debug(`>>> Entering deleteExpiredAndRevokedApiTokens(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("DeleteExpiredTokens", {}, callback);
		logger.debug(`deleteExpiredAndRevokedApiTokens(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteExpiredAndRevokedApiTokens(callback=${callback})`);
    }

    getPortalList(addAll, callback) {
		logger.debug(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`);
        const sf = this.getServiceFramework("Portals");
        sf.get("GetPortals?addAll=" + addAll, {}, callback);
		logger.debug(`getPortalList(addAll=${addAll},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortalList(addAll=${addAll},callback=${callback})`);
    }

    getCspSettings(callback) {
		logger.debug(`>>> Entering getCspSettings(callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.get("GetCspSettings", {}, callback);
		logger.debug(`getCspSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getCspSettings(callback=${callback})`);
    }
    updateCspSettings(payload, callback) {
		logger.debug(`>>> Entering updateCspSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Security");
        sf.post("UpdateCspSettings", payload, callback);
		logger.debug(`updateCspSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateCspSettings(payload=${payload},callback=${callback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;