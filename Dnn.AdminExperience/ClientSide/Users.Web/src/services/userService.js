import util from "utils";
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
class UserService {
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
    getUsers(searchParameters, callback, errorCallback) {
		logger.error(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetUsers?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.error(`getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getUserFilters(callback, errorCallback) {
		logger.error(`>>> Entering getUserFilters(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getUserFilters", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserFilters", {}, callback, errorCallback);
		logger.error(`getUserFilters(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getUserFilters", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
const userService = new UserService();
export default userService;