import util from "../utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}

class ModuleDefinitionService {
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
    getSourceFolders(callback) {
		logger.debug(`>>> Entering getSourceFolders(callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFolders", {}, callback, errorCallback);
		logger.debug(`getSourceFolders(callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSourceFolders(callback=${callback})`);
    }
    getSourceFiles(_root, callback) {
		logger.debug(`>>> Entering getSourceFiles(_root=${_root},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFiles?root=" + _root, {}, callback, errorCallback);
		logger.debug(`getSourceFiles(_root=${_root},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getSourceFiles(_root=${_root},callback=${callback})`);
    }
    getControlIcons(controlPath, callback) {
		logger.debug(`>>> Entering getControlIcons(controlPath=${controlPath},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");
        sf.get("LoadIcons?controlPath=" + controlPath, {}, callback, errorCallback);
		logger.debug(`getControlIcons(controlPath=${controlPath},callback=${callback}): sf → ${sf}`);
		logger.debug(`<<< Exiting getControlIcons(controlPath=${controlPath},callback=${callback})`);
    }
}
const moduleDefinitionService = new ModuleDefinitionService();
export default moduleDefinitionService;