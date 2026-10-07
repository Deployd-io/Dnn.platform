const logger = console;
import { util } from "utils/helpers";

class ApplicationService {
    getServiceFramework(controller) {
		logger.error(`>>> Entering getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        let sf = util.utilities.sf;
        sf.moduleRoot = "PersonaBar";
		logger.debug(`getServiceFramework(controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        sf.controller = controller;
		logger.debug(`getServiceFramework(controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework", "logrite.controller": controller });
		logger.warn(`<<< Exiting getServiceFramework(controller=${controller})`, { "event.action": "getServiceFramework", "logrite.controller": controller });
        return sf;
    }
    getServiceFrameworkWithRoot(moduleRoot, controller) {
		logger.error(`>>> Entering getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller})`, { "event.action": "getServiceFrameworkWithRoot", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        let sf = util.utilities.sf;
        sf.moduleRoot = moduleRoot;
		logger.debug(`getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller}): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFrameworkWithRoot", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        sf.controller = controller;
		logger.debug(`getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller}): sf.controller → ${sf.controller}`, { "event.action": "getServiceFrameworkWithRoot", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
		logger.warn(`<<< Exiting getServiceFrameworkWithRoot(moduleRoot=${moduleRoot},controller=${controller})`, { "event.action": "getServiceFrameworkWithRoot", "logrite.moduleRoot": moduleRoot, "logrite.controller": controller });
        return sf;
    }

    runCommand(payload, callback, failureCallback) {
		logger.error(`>>> Entering runCommand(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "runCommand", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFramework("Command");
        sf.post("Cmd", payload, callback, failureCallback);
		logger.error(`runCommand(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "runCommand", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
    changeUserMode(payload, callback, failureCallback) {
		logger.error(`>>> Entering changeUserMode(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "changeUserMode", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        const sf = this.getServiceFrameworkWithRoot("InternalServices", "ControlBar");
        sf.post("ToggleUserMode", payload, callback, failureCallback);
		logger.error(`changeUserMode(payload=${payload},callback=${callback},failureCallback=${failureCallback}): sf → ${sf}`, { "event.action": "changeUserMode", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
    }
}
const applicationService = new ApplicationService();
export default applicationService;