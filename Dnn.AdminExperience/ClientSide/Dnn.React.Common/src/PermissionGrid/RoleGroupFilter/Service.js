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
let sf = null;
class Service {
    constructor(serviceFramework) {
		logger.error(`>>> Entering constructor(serviceFramework=${serviceFramework})`, { "event.action": "constructor", "logrite.serviceFramework": serviceFramework });
        sf = serviceFramework;
		logger.warn(`<<< Exiting constructor(serviceFramework=${serviceFramework})`, { "event.action": "constructor", "logrite.serviceFramework": serviceFramework });
    }

    getRoleGroups(callback, errorCallback) {
		logger.error(`>>> Entering getRoleGroups(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getRoleGroups", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.moduleRoot = "PersonaBar";
        sf.controller = "Components";

        sf.get("GetRoleGroups", {}, callback, errorCallback);
		logger.warn(`<<< Exiting getRoleGroups(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getRoleGroups", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
export default Service;