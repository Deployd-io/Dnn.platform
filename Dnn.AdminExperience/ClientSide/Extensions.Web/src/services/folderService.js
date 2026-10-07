import util from "../utils";
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
class FolderService {
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
    getOwnerFolders(callback, errorCallback) {
		logger.error(`>>> Entering getOwnerFolders(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getOwnerFolders", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`getOwnerFolders(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getOwnerFolders", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("GetOwnerFolders", {}, callback, errorCallback);
		logger.warn(`<<< Exiting getOwnerFolders(callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getOwnerFolders", "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getModuleFolders(ownerFolder, callback, errorCallback) {
		logger.error(`>>> Entering getModuleFolders(ownerFolder=${ownerFolder},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getModuleFolders", "logrite.ownerFolder": ownerFolder, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`getModuleFolders(ownerFolder=${ownerFolder},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getModuleFolders", "logrite.ownerFolder": ownerFolder, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("GetModuleFolders?ownerFolder=" + ownerFolder, {}, callback, errorCallback);
		logger.warn(`<<< Exiting getModuleFolders(ownerFolder=${ownerFolder},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getModuleFolders", "logrite.ownerFolder": ownerFolder, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    getModuleFiles(parameters, callback, errorCallback) {
		logger.error(`>>> Entering getModuleFiles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getModuleFiles", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`getModuleFiles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "getModuleFiles", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.get("GetModuleFiles?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.warn(`<<< Exiting getModuleFiles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "getModuleFiles", "logrite.parameters": parameters, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    createNewFolder(payload, callback, errorCallback) {
		logger.error(`>>> Entering createNewFolder(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createNewFolder", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`createNewFolder(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "createNewFolder", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.post("CreateFolder?" + serializeQueryStringParameters(payload), {}, callback, errorCallback);
		logger.warn(`<<< Exiting createNewFolder(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createNewFolder", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
const folderService = new FolderService();
export default folderService;