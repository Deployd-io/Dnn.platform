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

    getLogList(searchParameters, callback) {
		logger.debug(`>>> Entering getLogList(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetLogItems?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`getLogList(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLogList(searchParameters=${searchParameters},callback=${callback})`);
    }

    getPortalList(addAll, callback) {
		logger.debug(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`);
        const sf = this.getServiceFramework("Portals");
        sf.get("GetPortals?addAll=" + addAll, {}, callback);
		logger.debug(`getPortalList(addAll=${addAll},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortalList(addAll=${addAll},callback=${callback})`);
    }

    /*Get all the types*/
    getLogTypes(callback) {
		logger.debug(`>>> Entering getLogTypes(callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogTypes", {}, callback);
		logger.debug(`getLogTypes(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLogTypes(callback=${callback})`);
    }

    deleteLogItems(payload, callback) {
		logger.debug(`>>> Entering deleteLogItems(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("DeleteLogItems", payload, callback);
		logger.debug(`deleteLogItems(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteLogItems(payload=${payload},callback=${callback})`);
    }

    emailLogItems(payload, callback) {
		logger.debug(`>>> Entering emailLogItems(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("EmailLogItems", payload, callback);
		logger.debug(`emailLogItems(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting emailLogItems(payload=${payload},callback=${callback})`);
    }

    clearLog(callback) {
		logger.debug(`>>> Entering clearLog(callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("ClearLog", {}, callback);
		logger.debug(`clearLog(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting clearLog(callback=${callback})`);
    }

    /*Get  Keep Most Recent Options*/
    getKeepMostRecentOptions(callback) {
		logger.debug(`>>> Entering getKeepMostRecentOptions(callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetKeepMostRecentOptions", {}, callback);
		logger.debug(`getKeepMostRecentOptions(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getKeepMostRecentOptions(callback=${callback})`);
    }

    /*Get  Occurence Options*/
    getOccurrenceOptions(callback) {
		logger.debug(`>>> Entering getOccurrenceOptions(callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetOccurrenceOptions", {}, callback);
		logger.debug(`getOccurrenceOptions(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getOccurrenceOptions(callback=${callback})`);
    }

    /*Get a particular log setting details*/
    getLogSettingById(parameters, callback) {
		logger.debug(`>>> Entering getLogSettingById(parameters=${parameters},callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogSetting?" + serializeQueryStringParameters(parameters), {}, callback);
		logger.debug(`getLogSettingById(parameters=${parameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLogSettingById(parameters=${parameters},callback=${callback})`);
    }

    /*Get latest log setting details*/
    getLatestLogSetting(callback) {
		logger.debug(`>>> Entering getLatestLogSetting(callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLatestLogSetting", {}, callback);
		logger.debug(`getLatestLogSetting(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLatestLogSetting(callback=${callback})`);
    }
    getLogSettings(callback) {
		logger.debug(`>>> Entering getLogSettings(callback=${callback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.get("GetLogSettings", {}, callback);
		logger.debug(`getLogSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getLogSettings(callback=${callback})`);
    }

    /*Update a log type setting*/
    updateLogSetting(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("UpdateLogSetting", payload, callback, failureCallback);
		logger.debug(`updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    /*Add a log type setting*/
    addLogSetting(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("AddLogSetting", payload, callback, failureCallback);
		logger.debug(`addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    deleteLogSetting(payload, callback, failureCallback) {
		logger.debug(`>>> Entering deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("AdminLogs");
        sf.post("DeleteLogSetting", payload, callback, failureCallback);
		logger.debug(`deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;