import utilities from "utils";
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
class PortalService {
    getServiceFramework(controller) {
		logger.debug(`>>> Entering getServiceFramework(controller=${controller})`);
        let sf = utilities.sf;

        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`);

		logger.debug(`<<< Exiting getServiceFramework(controller=${controller})`);
        return sf;
    }
    getPortalLocales(portalId, callback, errorCallback) {
		logger.debug(`>>> Entering getPortalLocales(portalId=${portalId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalLocales?portalId=" + portalId, {}, callback, errorCallback);
		logger.debug(`getPortalLocales(portalId=${portalId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortalLocales(portalId=${portalId},callback=${callback},errorCallback=${errorCallback})`);
    }
    deletePortal(portalId, callback, errorCallback) {
		logger.debug(`>>> Entering deletePortal(portalId=${portalId},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Sites");
        sf.post("DeletePortal?portalId=" + portalId, {}, callback, errorCallback);
		logger.debug(`deletePortal(portalId=${portalId},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting deletePortal(portalId=${portalId},callback=${callback},errorCallback=${errorCallback})`);
    }
    exportPortal(payload, callback, errorCallback) {
		logger.debug(`>>> Entering exportPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Sites");
        sf.post("ExportPortalTemplate", payload, callback, errorCallback);
		logger.debug(`exportPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting exportPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
    getPortalTemplates(callback, errorCallback) {
		logger.debug(`>>> Entering getPortalTemplates(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortalTemplates", {}, callback, errorCallback);
		logger.debug(`getPortalTemplates(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortalTemplates(callback=${callback},errorCallback=${errorCallback})`);
    }
    createPortal(payload, callback, errorCallback) {
		logger.debug(`>>> Entering createPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Sites");
        sf.post("CreatePortal", payload, callback, errorCallback);
		logger.debug(`createPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting createPortal(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
    getPortals(searchParameters, callback, errorCallback) {
		logger.debug(`>>> Entering getPortals(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Sites");
        sf.get("GetPortals?" + serializeQueryStringParameters(searchParameters), {}, callback, errorCallback);
		logger.debug(`getPortals(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getPortals(searchParameters=${searchParameters},callback=${callback},errorCallback=${errorCallback})`);
    }
}
const portalService = new PortalService();
export default portalService;