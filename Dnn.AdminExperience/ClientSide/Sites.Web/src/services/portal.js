import utilities from "utils";
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
class PortalService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = utilities.sf;

		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;

		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }
    getPortals(searchParameters, callback, errorCallback) {
		logger.error(`>>> Entering getPortals(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortals", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortals?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.error(`getPortals(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getPortals", "logrite.searchParameters": searchParameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
const portalService = new PortalService();
export default portalService;