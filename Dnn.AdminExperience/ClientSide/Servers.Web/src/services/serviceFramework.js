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

function getServiceFramework(controllerName) {
	logger.debug(`>>> Entering getServiceFramework(controllerName=${controllerName})`);
    const sf = utils.getServiceFramework();

	logger.debug(`getServiceFramework(controllerName=${controllerName}): sf → ${sf}`);
	logger.debug(`getServiceFramework(controllerName=${controllerName}): sf.controller → ${sf.controller}`);
    sf.moduleRoot = "PersonaBar";
	logger.debug(`getServiceFramework(controllerName=${controllerName}): sf.moduleRoot → ${sf.moduleRoot}`);
    sf.controller = controllerName;

	logger.debug(`<<< Exiting getServiceFramework(controllerName=${controllerName})`);
    return sf;
}

const serviceFramework = {
    get(controllerName, method, parameters) {
		logger.debug(`>>> Entering get(controllerName=${controllerName},method=${method},parameters=${parameters})`);
        const sf = getServiceFramework(controllerName);
		logger.debug(`<<< Exiting get(controllerName=${controllerName},method=${method},parameters=${parameters})`);
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
        });
    },
    post(controllerName, method, parameters) {
		logger.debug(`>>> Entering post(controllerName=${controllerName},method=${method},parameters=${parameters})`);
        const sf = getServiceFramework(controllerName);
		logger.debug(`<<< Exiting post(controllerName=${controllerName},method=${method},parameters=${parameters})`);
        return new Promise((callback, errorCallback) => {
            sf.post(method, parameters, callback, errorCallback);
        });
    }
};

export default serviceFramework; 