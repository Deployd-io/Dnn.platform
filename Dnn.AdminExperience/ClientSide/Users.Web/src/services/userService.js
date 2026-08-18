import util from "utils";
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
class UserService {
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
    getUsers(searchParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetUsers?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.debug(`getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUsers(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    getUserFilters(callback, errorCallback) {
		logger.debug(`>>> Entering getUserFilters(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Users");
        sf.get("GetUserFilters", {}, callback, errorCallback);
		logger.debug(`getUserFilters(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getUserFilters(callback=${callback},errorCallback=${errorCallback})`);
    }
}
const userService = new UserService();
export default userService;