import util from "../utils";
const logger = console;
class ApplicationService {
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

    getProduct(callback) {
		logger.debug(`>>> Entering getProduct(callback=${callback})`);
        const sf = this.getServiceFramework("Licensing");
        sf.get("GetProduct", {}, callback);
		logger.debug(`getProduct(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getProduct(callback=${callback})`);
    }

    getServerInfo(callback) {
		logger.debug(`>>> Entering getServerInfo(callback=${callback})`);
        const sf = this.getServiceFramework("ServerSummary");
        sf.get("GetServerInfo", {}, callback);
		logger.debug(`getServerInfo(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getServerInfo(callback=${callback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;