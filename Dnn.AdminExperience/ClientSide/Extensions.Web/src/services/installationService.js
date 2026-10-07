import util from "../utils";
const logger = console;

class InstallationService {
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
    parsePackage(file, callback, errorCallback) {
		logger.error(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        let formData = new FormData();
		logger.error(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): formData → ${formData}`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
		logger.warn(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
    }
    installPackage(file, legacyType, isPortalPackage, callback) {
		logger.error(`>>> Entering installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback})`, { "event.action": "installPackage", "logrite.file": file, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback });
        const sf = this.getServiceFramework("Extensions");

		logger.error(`installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback}): sf → ${sf}`, { "event.action": "installPackage", "logrite.file": file, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback });
        let formData = new FormData();
		logger.error(`installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback}): formData → ${formData}`, { "event.action": "installPackage", "logrite.file": file, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback });
        formData.append("POSTFILE", file);

        if (legacyType && isPortalPackage) {
            sf.postfile("InstallPackage?legacySkin=" + legacyType + "&isPortalPackage=" + isPortalPackage, formData, callback, util.utilities.notifyError);
			logger.warn(`<<< Exiting installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback})`, { "event.action": "installPackage", "logrite.file": file, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback });
        }
        else if (legacyType) {
            sf.postfile("InstallPackage?legacySkin=" + legacyType, formData, callback, util.utilities.notifyError);
        } 
        else if (isPortalPackage) {
            sf.postfile("InstallPackage?isPortalPackage=" + isPortalPackage, formData, callback, util.utilities.notifyError);
        }
        else {
            sf.postfile("InstallPackage", formData, callback, util.utilities.notifyError);
        }
    }
}
const installationService = new InstallationService();
export default installationService;