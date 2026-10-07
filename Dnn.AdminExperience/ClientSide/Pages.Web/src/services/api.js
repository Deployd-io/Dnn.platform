import utils from "../utils";
import Promise from "es6-promise";
const logger = console;

function serializeQueryStringParameters(obj) {
	logger.error(`>>> Entering serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    const s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.warn(`<<< Exiting serializeQueryStringParameters(obj=${obj})`, { "event.action": "serializeQueryStringParameters", "logrite.obj": obj });
    return s.join("&");
}

function getErrorMessageFromXHRError(error) {
	logger.error(`>>> Entering getErrorMessageFromXHRError(error=${error})`, { "event.action": "getErrorMessageFromXHRError", "logrite.error": error });
    if (error && 
        error.responseJSON && 
        error.responseJSON.Message) {
        return {
            message: error.responseJSON.Message
        };
    }
	logger.warn(`<<< Exiting getErrorMessageFromXHRError(error=${error})`, { "event.action": "getErrorMessageFromXHRError", "logrite.error": error });
    return {
        message: null
    };
}

class Api {

    constructor(controller) {
		logger.error(`>>> Entering constructor(controller=${controller})`, { "event.action": "constructor", "logrite.controller": controller });
        this.controller = controller;
        this.moduleRoot = "PersonaBar";
		logger.debug(`constructor(controller=${controller}): this.moduleRoot → ${this.moduleRoot}`, { "event.action": "constructor", "logrite.controller": controller });
    }

    getServiceFramework() {
		logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
        const sf = utils.getServiceFramework(); 
        sf.moduleRoot = this.moduleRoot;
		logger.error(`getServiceFramework(): sf → ${sf}`, { "event.action": "getServiceFramework" });
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework" });
        sf.controller = this.controller;
		logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
        return sf;
    }

    get(method, searchParameters) {
		logger.error(`>>> Entering get(method=${method},searchParameters=${searchParameters})`, { "event.action": "get", "logrite.method": method, "logrite.searchParameters": searchParameters });
        const sf = this.getServiceFramework();
		logger.error(`get(method=${method},searchParameters=${searchParameters}): sf → ${sf}`, { "event.action": "get", "logrite.method": method, "logrite.searchParameters": searchParameters });
		logger.warn(`<<< Exiting get(method=${method},searchParameters=${searchParameters})`, { "event.action": "get", "logrite.method": method, "logrite.searchParameters": searchParameters });
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(searchParameters), {}, callback, function onError(error) {
				logger.error(`>>> Entering onError(callback=${callback})`, { "event.action": "onError", "logrite.method": method, "logrite.payload": payload, "logrite.callback": callback });
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }

    post(method, payload) {
		logger.error(`>>> Entering post(method=${method},payload=${payload})`, { "event.action": "post", "logrite.method": method, "logrite.payload": payload });
        const sf = this.getServiceFramework();
		logger.error(`post(method=${method},payload=${payload}): sf → ${sf}`, { "event.action": "post", "logrite.method": method, "logrite.payload": payload });
		logger.warn(`<<< Exiting post(method=${method},payload=${payload})`, { "event.action": "post", "logrite.method": method, "logrite.payload": payload });
        return new Promise((callback, errorCallback) => {
            sf.post(method, payload, callback, function onError(error) {
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }
}
export default Api;