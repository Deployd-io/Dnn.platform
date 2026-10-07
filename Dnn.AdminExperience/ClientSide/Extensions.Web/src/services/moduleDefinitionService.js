import util from "../utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}

class ModuleDefinitionService {
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
    getSourceFolders(callback) {
		logger.error(`>>> Entering getSourceFolders(callback=${callback})`, { "event.action": "getSourceFolders", "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFolders", {}, callback, errorCallback);
		logger.error(`getSourceFolders(callback=${callback}): sf → ${sf}`, { "event.action": "getSourceFolders", "logrite.callback": callback });
    }
    getSourceFiles(_root, callback) {
		logger.error(`>>> Entering getSourceFiles(_root=${_root},callback=${callback})`, { "event.action": "getSourceFiles", "logrite._root": _root, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("GetSourceFiles?root=" + _root, {}, callback, errorCallback);
		logger.error(`getSourceFiles(_root=${_root},callback=${callback}): sf → ${sf}`, { "event.action": "getSourceFiles", "logrite._root": _root, "logrite.callback": callback });
    }
    getControlIcons(controlPath, callback) {
		logger.error(`>>> Entering getControlIcons(controlPath=${controlPath},callback=${callback})`, { "event.action": "getControlIcons", "logrite.controlPath": controlPath, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");
        sf.get("LoadIcons?controlPath=" + controlPath, {}, callback, errorCallback);
		logger.error(`getControlIcons(controlPath=${controlPath},callback=${callback}): sf → ${sf}`, { "event.action": "getControlIcons", "logrite.controlPath": controlPath, "logrite.callback": callback });
    }
}
const moduleDefinitionService = new ModuleDefinitionService();
export default moduleDefinitionService;