import utils from "../utils";
import Promise from "es6-promise";
const logger = console;

function serializeQueryStringParameters(obj) {
	logger.debug(`>>> Entering serializeQueryStringParameters(obj=${obj})`);
    const s = [];
    for (let p in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
	logger.debug(`<<< Exiting serializeQueryStringParameters(obj=${obj})`);
    return s.join("&");
}

function getErrorMessageFromXHRError(error) {
	logger.debug(`>>> Entering getErrorMessageFromXHRError(error=${error})`);
    if (error && 
        error.responseJSON && 
        error.responseJSON.Message) {
        return {
            message: error.responseJSON.Message
        };
    }
	logger.debug(`<<< Exiting getErrorMessageFromXHRError(error=${error})`);
    return {
        message: null
    };
}

class Api {

    constructor(controller) {
		logger.debug(`>>> Entering constructor(controller=${controller})`);
        this.controller = controller;
        this.moduleRoot = "PersonaBar";
		logger.debug(`constructor(controller=${controller}): controller → ${controller}`);
		logger.debug(`constructor(controller=${controller}): this.moduleRoot → ${this.moduleRoot}`);
		logger.debug(`<<< Exiting constructor(controller=${controller})`);
    }

    getServiceFramework() {
		logger.debug('>>> Entering getServiceFramework()');
        const sf = utils.getServiceFramework(); 
        sf.moduleRoot = this.moduleRoot;
		logger.debug(`getServiceFramework(): sf → ${sf}`);
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = this.controller;
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`);
		logger.debug('<<< Exiting getServiceFramework()');
        return sf;
    }

    get(method, searchParameters) {
		logger.debug(`>>> Entering get(method=${method},searchParameters=${searchParameters})`);
        const sf = this.getServiceFramework();
		logger.debug(`<<< Exiting get(method=${method},searchParameters=${searchParameters})`);
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(searchParameters), {}, callback, function onError(error) {
				logger.debug(`>>> Entering onError(callback=${callback})`);
				logger.debug(`<<< Exiting onError(callback=${callback})`);
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }

    post(method, payload) {
		logger.debug(`>>> Entering post(method=${method},payload=${payload})`);
        const sf = this.getServiceFramework();
		logger.debug(`<<< Exiting post(method=${method},payload=${payload})`);
        return new Promise((callback, errorCallback) => {
            sf.post(method, payload, callback, function onError(error) {
                errorCallback(getErrorMessageFromXHRError(error));
            });
        });
    }
}
export default Api;