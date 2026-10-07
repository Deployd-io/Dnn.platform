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
    getPortalLocales(portalId, callback, errorCallback) {
		logger.error(`>>> Entering getPortalLocales(portalId=${portalId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortalLocales", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalLocales?portalId=" + portalId, {}, callback, errorCallback);
		logger.error(`getPortalLocales(portalId=${portalId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getPortalLocales", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    deletePortal(portalId, callback, errorCallback) {
		logger.error(`>>> Entering deletePortal(portalId=${portalId},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "deletePortal", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Sites");
        sf.post("DeletePortal?portalId=" + portalId, {}, callback, errorCallback);
		logger.error(`deletePortal(portalId=${portalId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "deletePortal", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    exportPortal(payload, callback, errorCallback) {
		logger.error(`>>> Entering exportPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "exportPortal", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Sites");
        sf.post("ExportPortalTemplate", payload, callback, errorCallback);
		logger.error(`exportPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "exportPortal", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getPortalTemplates(callback, errorCallback) {
		logger.error(`>>> Entering getPortalTemplates(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getPortalTemplates", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalTemplates", {}, callback, errorCallback);
		logger.error(`getPortalTemplates(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getPortalTemplates", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    createPortal(payload, callback, errorCallback) {
		logger.error(`>>> Entering createPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createPortal", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Sites");
        sf.post("CreatePortal", payload, callback, errorCallback);
		logger.error(`createPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "createPortal", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
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