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

    getTaskStatusList(searchParameters, callback) {
		logger.error(`>>> Entering getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getTaskStatusList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.getsilence("GetScheduleStatus?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.error(`getTaskStatusList(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "getTaskStatusList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    startSchedule(callback) {
		logger.error(`>>> Entering startSchedule(callback=${callback})`, { "event.action": "startSchedule", "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("StartSchedule", {}, callback);
		logger.error(`startSchedule(callback=${callback}): sf → ${sf}`, { "event.action": "startSchedule", "logrite.callback": callback });
    }

    stopSchedule(callback) {
		logger.error(`>>> Entering stopSchedule(callback=${callback})`, { "event.action": "stopSchedule", "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("StopSchedule", {}, callback);
		logger.error(`stopSchedule(callback=${callback}): sf → ${sf}`, { "event.action": "stopSchedule", "logrite.callback": callback });
    }

    getSchedulerSettings(callback) {
		logger.error(`>>> Entering getSchedulerSettings(callback=${callback})`, { "event.action": "getSchedulerSettings", "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.get("GetSchedulerSettings", {}, callback);
		logger.error(`getSchedulerSettings(callback=${callback}): sf → ${sf}`, { "event.action": "getSchedulerSettings", "logrite.callback": callback });
    }

    updateSchedulerSettings(payload, callback) {
		logger.error(`>>> Entering updateSchedulerSettings(payload=${payload},callback=${callback})`, { "event.action": "updateSchedulerSettings", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("UpdateSchedulerSettings", payload, callback);
		logger.error(`updateSchedulerSettings(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "updateSchedulerSettings", "logrite.payload": payload, "logrite.callback": callback });
    }

    getScheduleItemHistory(searchParameters, callback) {
		logger.error(`>>> Entering getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getScheduleItemHistory", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItemHistory?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.error(`getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "getScheduleItemHistory", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    getScheduleItems(searchParameters, callback) {
		logger.error(`>>> Entering getScheduleItems(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getScheduleItems", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItems?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.error(`getScheduleItems(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "getScheduleItems", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    getServerList(callback) {
		logger.error(`>>> Entering getServerList(callback=${callback})`, { "event.action": "getServerList", "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");
        sf.get("GetServers", {}, callback);
		logger.error(`getServerList(callback=${callback}): sf → ${sf}`, { "event.action": "getServerList", "logrite.callback": callback });
    }

    getGetScheduleItem(searchParameters, callback) {
		logger.error(`>>> Entering getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getGetScheduleItem", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItem?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.error(`getGetScheduleItem(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`, { "event.action": "getGetScheduleItem", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
    }

    deleteScheduleItem(payload, callback) {
		logger.error(`>>> Entering deleteScheduleItem(payload=${payload},callback=${callback})`, { "event.action": "deleteScheduleItem", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("DeleteSchedule", payload, callback);
		logger.error(`deleteScheduleItem(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "deleteScheduleItem", "logrite.payload": payload, "logrite.callback": callback });
    }

    createScheduleItem(payload, callback, failureCallback) {
		logger.error(`>>> Entering createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("CreateScheduleItem", payload, callback, failureCallback);
		logger.error(`createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "createScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    updateScheduleItem(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("UpdateScheduleItem", payload, callback, failureCallback);
		logger.error(`updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "updateScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }

    runScheduleItem(payload, callback) {
		logger.error(`>>> Entering runScheduleItem(payload=${payload},callback=${callback})`, { "event.action": "runScheduleItem", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("RunSchedule", payload, callback);
		logger.error(`runScheduleItem(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "runScheduleItem", "logrite.payload": payload, "logrite.callback": callback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;