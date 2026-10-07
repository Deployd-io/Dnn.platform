const logger = console;
import { installation as ActionTypes, extension as ExtensionActionTypes } from "constants/actionTypes";
import { InstallationService } from "services";
const installationActions = {
    parsePackage(file, callback, errorCallback) {
		logger.error(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "parsePackage", "logrite.file": file, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        return (dispatch) => {
            InstallationService.parsePackage(file, (data) => {
                dispatch({
                    type: ActionTypes.PARSED_INSTALLATION_PACKAGE,
                    payload: JSON.parse(data)
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    navigateWizard(wizardStep, callback) {
		logger.error(`>>> Entering navigateWizard(wizardStep=${wizardStep},callback=${callback})`, { "event.action": "navigateWizard", "logrite.wizardStep": wizardStep, "logrite.callback": callback });
		logger.warn(`<<< Exiting navigateWizard(wizardStep=${wizardStep},callback=${callback})`, { "event.action": "navigateWizard", "logrite.wizardStep": wizardStep, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_WIZARD_STEP,
                payload: {
                    wizardStep
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    setInstallingAvailablePackage(FileName, PackageType, callback) {
		logger.error(`>>> Entering setInstallingAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback})`, { "event.action": "setInstallingAvailablePackage", "logrite.FileName": FileName, "logrite.PackageType": PackageType, "logrite.callback": callback });
		logger.warn(`<<< Exiting setInstallingAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback})`, { "event.action": "setInstallingAvailablePackage", "logrite.FileName": FileName, "logrite.PackageType": PackageType, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.INSTALLING_AVAILABLE_PACKAGE,
                payload: {
                    PackageType,
                    FileName
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    notInstallingAvailablePackage(callback) {
		logger.error(`>>> Entering notInstallingAvailablePackage(callback=${callback})`, { "event.action": "notInstallingAvailablePackage", "logrite.callback": callback });
		logger.warn(`<<< Exiting notInstallingAvailablePackage(callback=${callback})`, { "event.action": "notInstallingAvailablePackage", "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.NOT_INSTALLING_AVAILABLE_PACKAGE
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    installExtension(file, newExtension, legacyType, isPortalPackage, callback, addToList) {
		logger.error(`>>> Entering installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList})`, { "event.action": "installExtension", "logrite.file": file, "logrite.newExtension": newExtension, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback, "logrite.addToList": addToList });
        let _newExtension = JSON.parse(JSON.stringify(newExtension));
		logger.warn(`<<< Exiting installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList})`, { "event.action": "installExtension", "logrite.file": file, "logrite.newExtension": newExtension, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback, "logrite.addToList": addToList });
		logger.error(`installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList}): _newExtension → ${_newExtension}`, { "event.action": "installExtension", "logrite.file": file, "logrite.newExtension": newExtension, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback, "logrite.addToList": addToList });
        return (dispatch) => {
            InstallationService.installPackage(file, legacyType, isPortalPackage, (data) => {
                dispatch({
                    type: ActionTypes.INSTALLED_EXTENSION_LOGS,
                    payload: JSON.parse(data)
                });
                if (addToList) {
					logger.debug(`installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList}): _newExtension.inUse → ${_newExtension.inUse}`, { "event.action": "installExtension", "logrite.file": file, "logrite.newExtension": newExtension, "logrite.legacyType": legacyType, "logrite.isPortalPackage": isPortalPackage, "logrite.callback": callback, "logrite.addToList": addToList });
                    _newExtension.packageId = JSON.parse(data).newPackageId;
                    _newExtension.inUse = "No";
                    dispatch({
                        type: ExtensionActionTypes.INSTALLED_EXTENSION,
                        payload: {
                            PackageInfo: _newExtension,
                            logs: JSON.parse(data).logs
                        }
                    });
                }
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearParsedInstallationPackage(callback) {
		logger.error(`>>> Entering clearParsedInstallationPackage(callback=${callback})`, { "event.action": "clearParsedInstallationPackage", "logrite.callback": callback });
		logger.warn(`<<< Exiting clearParsedInstallationPackage(callback=${callback})`, { "event.action": "clearParsedInstallationPackage", "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_PARSED_INSTALLATION_PACKAGE
            });
            if (callback) {
                callback();
            }
        };
    },
    toggleAcceptLicense(value, callback) {
		logger.error(`>>> Entering toggleAcceptLicense(value=${value},callback=${callback})`, { "event.action": "toggleAcceptLicense", "logrite.value": value, "logrite.callback": callback });
		logger.warn(`<<< Exiting toggleAcceptLicense(value=${value},callback=${callback})`, { "event.action": "toggleAcceptLicense", "logrite.value": value, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_ACCEPT_LICENSE,
                payload: value
            });
            if (callback) {
                callback();
            }
        };
    },
    setViewingLog(value, callback) {
		logger.error(`>>> Entering setViewingLog(value=${value},callback=${callback})`, { "event.action": "setViewingLog", "logrite.value": value, "logrite.callback": callback });
		logger.warn(`<<< Exiting setViewingLog(value=${value},callback=${callback})`, { "event.action": "setViewingLog", "logrite.value": value, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_VIEWING_LOG,
                payload: value
            });
            if (callback) {
                callback();
            }
        };
    },
    setIsPortalPackage(value, callback) {
		logger.error(`>>> Entering setIsPortalPackage(value=${value},callback=${callback})`, { "event.action": "setIsPortalPackage", "logrite.value": value, "logrite.callback": callback });
		logger.warn(`<<< Exiting setIsPortalPackage(value=${value},callback=${callback})`, { "event.action": "setIsPortalPackage", "logrite.value": value, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_IS_PORTAL_PACKAGE,
                payload: value
            });
            if (callback) {
                callback();
            }
        };
    }
};

export default installationActions;
