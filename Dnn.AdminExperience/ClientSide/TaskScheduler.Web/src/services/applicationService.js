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

    getTaskStatusList(searchParameters, callback) {
		logger.debug(`>>> Entering getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.getsilence("GetScheduleStatus?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`getTaskStatusList(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`);
    }

    startSchedule(callback) {
		logger.debug(`>>> Entering startSchedule(callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("StartSchedule", {}, callback);
		logger.debug(`startSchedule(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting startSchedule(callback=${callback})`);
    }

    stopSchedule(callback) {
		logger.debug(`>>> Entering stopSchedule(callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("StopSchedule", {}, callback);
		logger.debug(`stopSchedule(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting stopSchedule(callback=${callback})`);
    }

    getSchedulerSettings(callback) {
		logger.debug(`>>> Entering getSchedulerSettings(callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.get("GetSchedulerSettings", {}, callback);
		logger.debug(`getSchedulerSettings(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSchedulerSettings(callback=${callback})`);
    }

    updateSchedulerSettings(payload, callback) {
		logger.debug(`>>> Entering updateSchedulerSettings(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("UpdateSchedulerSettings", payload, callback);
		logger.debug(`updateSchedulerSettings(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateSchedulerSettings(payload=${payload},callback=${callback})`);
    }

    getScheduleItemHistory(searchParameters, callback) {
		logger.debug(`>>> Entering getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItemHistory?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`);
    }

    getScheduleItems(searchParameters, callback) {
		logger.debug(`>>> Entering getScheduleItems(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItems?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`getScheduleItems(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getScheduleItems(searchParameters=${searchParameters},callback=${callback})`);
    }

    getServerList(callback) {
		logger.debug(`>>> Entering getServerList(callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");
        sf.get("GetServers", {}, callback);
		logger.debug(`getServerList(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getServerList(callback=${callback})`);
    }

    getGetScheduleItem(searchParameters, callback) {
		logger.debug(`>>> Entering getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");
        searchParameters = Object.assign({}, searchParameters, {
            //logType: "*"
        });
        sf.get("GetScheduleItem?" + serializeQueryStringParameters(searchParameters), {}, callback);
		logger.debug(`getGetScheduleItem(searchParameters=${searchParameters},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`);
    }

    deleteScheduleItem(payload, callback) {
		logger.debug(`>>> Entering deleteScheduleItem(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("DeleteSchedule", payload, callback);
		logger.debug(`deleteScheduleItem(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deleteScheduleItem(payload=${payload},callback=${callback})`);
    }

    createScheduleItem(payload, callback, failureCallback) {
		logger.debug(`>>> Entering createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("CreateScheduleItem", payload, callback, failureCallback);
		logger.debug(`createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    updateScheduleItem(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("UpdateScheduleItem", payload, callback, failureCallback);
		logger.debug(`updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }

    runScheduleItem(payload, callback) {
		logger.debug(`>>> Entering runScheduleItem(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("TaskScheduler");        
        sf.post("RunSchedule", payload, callback);
		logger.debug(`runScheduleItem(payload=${payload},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting runScheduleItem(payload=${payload},callback=${callback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;