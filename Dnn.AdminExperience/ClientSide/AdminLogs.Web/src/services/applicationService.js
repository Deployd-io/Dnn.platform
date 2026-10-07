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

    getLogList(searchParameters, callback) {
		logger.error(`>>> Entering getLogList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getLogList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetLogItems?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.error(`getLogList(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "getLogList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    getPortalList(addAll, callback) {
		logger.error(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
        const sf = this.getServiceFramework("Portals");
        sf.get("GetPortals?addAll=" + addAll, {}, callback);
		logger.error(`getPortalList(addAll=${addAll},callback=${callback}): sf → ${sf}`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
    }

    /*Get all the types*/
    getLogTypes(callback) {
		logger.error(`>>> Entering getLogTypes(callback=${callback})`, { "event.action": "getLogTypes", "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogTypes", {}, callback);
		logger.error(`getLogTypes(callback=${callback}): sf → ${sf}`, { "event.action": "getLogTypes", "logrite.callback": callback });
    }

    deleteLogItems(payload, callback) {
		logger.error(`>>> Entering deleteLogItems(payload=${payload},callback=${callback})`, { "event.action": "deleteLogItems", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("DeleteLogItems", payload, callback);
		logger.error(`deleteLogItems(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "deleteLogItems", "logrite.payload": payload, "logrite.callback": callback });
    }

    emailLogItems(payload, callback) {
		logger.error(`>>> Entering emailLogItems(payload=${payload},callback=${callback})`, { "event.action": "emailLogItems", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("EmailLogItems", payload, callback);
		logger.error(`emailLogItems(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "emailLogItems", "logrite.payload": payload, "logrite.callback": callback });
    }

    clearLog(callback) {
		logger.error(`>>> Entering clearLog(callback=${callback})`, { "event.action": "clearLog", "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("ClearLog", {}, callback);
		logger.error(`clearLog(callback=${callback}): sf → ${sf}`, { "event.action": "clearLog", "logrite.callback": callback });
    }

    /*Get  Keep Most Recent Options*/
    getKeepMostRecentOptions(callback) {
		logger.error(`>>> Entering getKeepMostRecentOptions(callback=${callback})`, { "event.action": "getKeepMostRecentOptions", "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetKeepMostRecentOptions", {}, callback);
		logger.error(`getKeepMostRecentOptions(callback=${callback}): sf → ${sf}`, { "event.action": "getKeepMostRecentOptions", "logrite.callback": callback });
    }

    /*Get  Occurence Options*/
    getOccurrenceOptions(callback) {
		logger.error(`>>> Entering getOccurrenceOptions(callback=${callback})`, { "event.action": "getOccurrenceOptions", "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetOccurrenceOptions", {}, callback);
		logger.error(`getOccurrenceOptions(callback=${callback}): sf → ${sf}`, { "event.action": "getOccurrenceOptions", "logrite.callback": callback });
    }

    /*Get a particular log setting details*/
    getLogSettingById(parameters, callback) {
		logger.error(`>>> Entering getLogSettingById(parameters=${parameters},callback=${callback})`, { "event.action": "getLogSettingById", "logrite.parameters": parameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogSetting?" + serializeQueryStringParameters(parameters), {}, callback);
		logger.error(`getLogSettingById(parameters=${parameters},callback=${callback}): sf → ${sf}`, { "event.action": "getLogSettingById", "logrite.parameters": parameters, "logrite.callback": callback });
    }

    /*Get latest log setting details*/
    getLatestLogSetting(callback) {
		logger.error(`>>> Entering getLatestLogSetting(callback=${callback})`, { "event.action": "getLatestLogSetting", "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLatestLogSetting", {}, callback);
		logger.error(`getLatestLogSetting(callback=${callback}): sf → ${sf}`, { "event.action": "getLatestLogSetting", "logrite.callback": callback });
    }
    getLogSettings(callback) {
		logger.error(`>>> Entering getLogSettings(callback=${callback})`, { "event.action": "getLogSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogSettings", {}, callback);
		logger.error(`getLogSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getLogSettings", "logrite.callback": callback });
    }

    /*Update a log type setting*/
    updateLogSetting(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("UpdateLogSetting", payload, callback, failureCallback);
		logger.error(`updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    /*Add a log type setting*/
    addLogSetting(payload, callback, failureCallback) {
		logger.error(`>>> Entering addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("AddLogSetting", payload, callback, failureCallback);
		logger.error(`addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "addLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    deleteLogSetting(payload, callback, failureCallback) {
		logger.error(`>>> Entering deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("DeleteLogSetting", payload, callback, failureCallback);
		logger.error(`deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "deleteLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;