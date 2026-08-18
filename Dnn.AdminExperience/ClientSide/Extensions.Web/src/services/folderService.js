import util from "../utils";
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
class FolderService {
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
    getOwnerFolders(callback, errorCallback) {
		logger.debug(`>>> Entering getOwnerFolders(callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`getOwnerFolders(callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.get("GetOwnerFolders", {}, callback, errorCallback);
		logger.debug(`<<< Exiting getOwnerFolders(callback=${callback},errorCallback=${errorCallback})`);
    }
    getModuleFolders(ownerFolder, callback, errorCallback) {
		logger.debug(`>>> Entering getModuleFolders(ownerFolder=${ownerFolder},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`getModuleFolders(ownerFolder=${ownerFolder},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.get("GetModuleFolders?ownerFolder=" + ownerFolder, {}, callback, errorCallback);
		logger.debug(`<<< Exiting getModuleFolders(ownerFolder=${ownerFolder},callback=${callback},errorCallback=${errorCallback})`);
    }
    getModuleFiles(parameters, callback, errorCallback) {
		logger.debug(`>>> Entering getModuleFiles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`getModuleFiles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.get("GetModuleFiles?" + serializeQueryStringParameters(parameters), {}, callback, errorCallback);
		logger.debug(`<<< Exiting getModuleFiles(parameters=${parameters},callback=${callback},errorCallback=${errorCallback})`);
    }
    createNewFolder(payload, callback, errorCallback) {
		logger.debug(`>>> Entering createNewFolder(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`createNewFolder(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.post("CreateFolder?" + serializeQueryStringParameters(payload), {}, callback, errorCallback);
		logger.debug(`<<< Exiting createNewFolder(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
}
const folderService = new FolderService();
export default folderService;