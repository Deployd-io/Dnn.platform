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
let sf = null;
class Service {
    constructor(serviceFramework) {
		logger.debug(`>>> Entering constructor(serviceFramework=${serviceFramework})`);
        sf = serviceFramework;
		logger.debug(`<<< Exiting constructor(serviceFramework=${serviceFramework})`);
    }

    getRoleGroups(callback, errorCallback) {
		logger.debug(`>>> Entering getRoleGroups(callback=${callback},errorCallback=${errorCallback})`);
        sf.moduleRoot = "PersonaBar";
        sf.controller = "Components";

        sf.get("GetRoleGroups", {}, callback, errorCallback);
		logger.debug(`<<< Exiting getRoleGroups(callback=${callback},errorCallback=${errorCallback})`);
    }
}
export default Service;