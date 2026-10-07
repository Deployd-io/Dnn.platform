import util from "../utils";
const logger = console;
function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}

class CreatePackageService {
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
    getPackageManifest(packageId, callback) {
		logger.error(`>>> Entering getPackageManifest(packageId=${packageId},callback=${callback})`, { "event.action": "getPackageManifest", "logrite.packageId": packageId, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`getPackageManifest(packageId=${packageId},callback=${callback}): sf → ${sf}`, { "event.action": "getPackageManifest", "logrite.packageId": packageId, "logrite.callback": callback });
        sf.get("GetPackageManifest?packageId=" + packageId, {}, callback, errorCallback);
		logger.warn(`<<< Exiting getPackageManifest(packageId=${packageId},callback=${callback})`, { "event.action": "getPackageManifest", "logrite.packageId": packageId, "logrite.callback": callback });
    }
    createManifest(payload, callback) {
		logger.error(`>>> Entering createManifest(payload=${payload},callback=${callback})`, { "event.action": "createManifest", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`createManifest(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "createManifest", "logrite.payload": payload, "logrite.callback": callback });
        sf.post("CreateManifest", payload, callback, errorCallback);
		logger.warn(`<<< Exiting createManifest(payload=${payload},callback=${callback})`, { "event.action": "createManifest", "logrite.payload": payload, "logrite.callback": callback });
    }
    generateManifestPreview(payload, callback) {
		logger.error(`>>> Entering generateManifestPreview(payload=${payload},callback=${callback})`, { "event.action": "generateManifestPreview", "logrite.payload": payload, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`generateManifestPreview(payload=${payload},callback=${callback}): sf → ${sf}`, { "event.action": "generateManifestPreview", "logrite.payload": payload, "logrite.callback": callback });
        sf.post("CreateNewManifest", payload, callback, errorCallback);
		logger.warn(`<<< Exiting generateManifestPreview(payload=${payload},callback=${callback})`, { "event.action": "generateManifestPreview", "logrite.payload": payload, "logrite.callback": callback });
    }
    createPackage(payload, callback, errorCallback) {
		logger.error(`>>> Entering createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createPackage", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "createPackage", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.post("CreatePackage", payload, callback, errorCallback);
		logger.warn(`<<< Exiting createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createPackage", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    refreshPackageFiles(payload, callback, errorCallback) {
		logger.error(`>>> Entering refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "refreshPackageFiles", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "refreshPackageFiles", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        sf.post("RefreshPackageFiles", payload, callback, errorCallback);
		logger.warn(`<<< Exiting refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "refreshPackageFiles", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
}
const createPackageService = new CreatePackageService();
export default createPackageService;