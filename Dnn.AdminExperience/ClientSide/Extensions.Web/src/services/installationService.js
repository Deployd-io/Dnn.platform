import util from "../utils";
const logger = console;

class InstallationService {
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
    parsePackage(file, callback, errorCallback) {
		logger.debug(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): sf → ${sf}`);
        let formData = new FormData();
		logger.debug(`parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback}): formData → ${formData}`);
        formData.append("POSTFILE", file);


        sf.postfile("ParsePackage", formData, callback, errorCallback);
		logger.debug(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`);
    }
    installPackage(file, legacyType, isPortalPackage, callback) {
		logger.debug(`>>> Entering installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback})`);
        const sf = this.getServiceFramework("Extensions");

		logger.debug(`installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback}): sf → ${sf}`);
        let formData = new FormData();
		logger.debug(`installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback}): formData → ${formData}`);
        formData.append("POSTFILE", file);

        if (legacyType && isPortalPackage) {
            sf.postfile("InstallPackage?legacySkin=" + legacyType + "&isPortalPackage=" + isPortalPackage, formData, callback, util.utilities.notifyError);
			logger.debug(`<<< Exiting installPackage(file=${file},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback})`);
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