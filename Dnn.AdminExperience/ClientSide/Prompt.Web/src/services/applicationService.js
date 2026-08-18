const logger = console;
import { util } from "utils/helpers";

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
    getServiceFrameworkWithRoot(moduleRoot, controller) {
		logger.debug(`>>> Entering getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller})`);
        let sf = util.utilities.sf;
        sf.moduleRoot = moduleRoot;
		logger.debug(`getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`);
        sf.controller = controller;
		logger.debug(`getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller}): sf.controller → ${sf.controller}`);
		logger.debug(`<<< Exiting getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller})`);
        return sf;
    }

    runCommand(payload, callback, failureCallback) {
		logger.debug(`>>> Entering runCommand(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFramework("Command");
        sf.post("Cmd", payload, callback, failureCallback);
		logger.debug(`runCommand(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting runCommand(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
    changeUserMode(payload, callback, failureCallback) {
		logger.debug(`>>> Entering changeUserMode(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        const sf = this.getServiceFrameworkWithRoot("InternalServices", "ControlBar");
        sf.post("ToggleUserMode", payload, callback, failureCallback);
		logger.debug(`changeUserMode(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`);
		logger.debug(`<<< Exiting changeUserMode(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
    }
}
const applicationService = new ApplicationService();
export default applicationService;