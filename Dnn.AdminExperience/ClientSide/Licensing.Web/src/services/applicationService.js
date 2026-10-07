import util from "../utils";
const logger = console;
class ApplicationService {
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

    getProduct(callback) {
		logger.error(`>>> Entering getProduct(callback=${callback})`, { "event.action": "getProduct", "logrite.callback": callback });
        const sf = this.getServiceFramework("Licensing");
        sf.get("GetProduct", {}, callback);
		logger.error(`getProduct(callback=${callback}): sf → ${sf}`, { "event.action": "getProduct", "logrite.callback": callback });
    }

    getServerInfo(callback) {
		logger.error(`>>> Entering getServerInfo(callback=${callback})`, { "event.action": "getServerInfo", "logrite.callback": callback });
        const sf = this.getServiceFramework("ServerSummary");
        sf.get("GetServerInfo", {}, callback);
		logger.error(`getServerInfo(callback=${callback}): sf → ${sf}`, { "event.action": "getServerInfo", "logrite.callback": callback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;