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

function getServiceFramework(controllerName) {
	logger.error(`>>> Entering getServiceFramework(controllerName=${controllerName})`, { "event.action": "getServiceFramework", "logrite.controllerName": controllerName });
    const sf = utils.getServiceFramework();

	logger.error(`getServiceFramework(controllerName=${controllerName}): sf → ${sf}`, { "event.action": "getServiceFramework", "logrite.controllerName": controllerName });
	logger.debug(`getServiceFramework(controllerName=${controllerName}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controllerName": controllerName });
    sf.moduleRoot = "PersonaBar";
	logger.debug(`getServiceFramework(controllerName=${controllerName}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controllerName": controllerName });
    sf.controller = controllerName;

	logger.warn(`<<< Exiting getServiceFramework(controllerName=${controllerName})`, { "event.action": "getServiceFramework", "logrite.controllerName": controllerName });
    return sf;
}

const serviceFramework = {
    get(controllerName, method, parameters) {
		logger.error(`>>> Entering get(controllerName=${controllerName},method=${method},parameters=${parameters})`, { "event.action": "get", "logrite.controllerName": controllerName, "logrite.method": method, "logrite.parameters": parameters });
        const sf = getServiceFramework(controllerName);
		logger.warn(`<<< Exiting get(controllerName=${controllerName},method=${method},parameters=${parameters})`, { "event.action": "get", "logrite.controllerName": controllerName, "logrite.method": method, "logrite.parameters": parameters });
        return new Promise((callback, errorCallback) => {
            sf.get(method + "?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
        });
    },
    post(controllerName, method, parameters) {
		logger.error(`>>> Entering post(controllerName=${controllerName},method=${method},parameters=${parameters})`, { "event.action": "post", "logrite.controllerName": controllerName, "logrite.method": method, "logrite.parameters": parameters });
        const sf = getServiceFramework(controllerName);
		logger.warn(`<<< Exiting post(controllerName=${controllerName},method=${method},parameters=${parameters})`, { "event.action": "post", "logrite.controllerName": controllerName, "logrite.method": method, "logrite.parameters": parameters });
        return new Promise((callback, errorCallback) => {
            sf.post(method, parameters, callback, errorCallback);
        });
    }
};

export default serviceFramework; 