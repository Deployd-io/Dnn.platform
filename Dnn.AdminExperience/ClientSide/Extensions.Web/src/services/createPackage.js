import util from "../utils";
const logger = console;
function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    util.utilities.notifyError(typeof message === "object" ? message.Message : message);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}

class CreatePackageService {
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
    getPackageManifest(packageId, callback) {
		logger.debug(`>>> Entering getPackageManifest(packageId=${packageId},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`getPackageManifest(packageId=${packageId},callback=${callback}): sf → ${sf}`);
        sf.get("GetPackageManifest?packageId=" + packageId, {}, callback, errorCallback);
		logger.debug(`<<< Exiting getPackageManifest(packageId=${packageId},callback=${callback})`);
    }
    createManifest(payload, callback) {
		logger.debug(`>>> Entering createManifest(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`createManifest(payload=${payload},callback=${callback}): sf → ${sf}`);
        sf.post("CreateManifest", payload, callback, errorCallback);
		logger.debug(`<<< Exiting createManifest(payload=${payload},callback=${callback})`);
    }
    generateManifestPreview(payload, callback) {
		logger.debug(`>>> Entering generateManifestPreview(payload=${payload},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`generateManifestPreview(payload=${payload},callback=${callback}): sf → ${sf}`);
        sf.post("CreateNewManifest", payload, callback, errorCallback);
		logger.debug(`<<< Exiting generateManifestPreview(payload=${payload},callback=${callback})`);
    }
    createPackage(payload, callback, errorCallback) {
		logger.debug(`>>> Entering createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.post("CreatePackage", payload, callback, errorCallback);
		logger.debug(`<<< Exiting createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
    refreshPackageFiles(payload, callback, errorCallback) {
		logger.debug(`>>> Entering refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        sf.post("RefreshPackageFiles", payload, callback, errorCallback);
		logger.debug(`<<< Exiting refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
    }
}
const createPackageService = new CreatePackageService();
export default createPackageService;