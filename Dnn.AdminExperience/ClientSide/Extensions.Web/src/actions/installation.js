const logger = console;
import { installation as ActionTypes, extension as ExtensionActionTypes } from "constants/actionTypes";
import { InstallationService } from "services";
const installationActions = {
    parsePackage(file, callback, errorCallback) {
		logger.debug(`>>> Entering parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting parsePackage(file=${file},callback=${callback},errorCallback=${errorCallback})`);
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
		logger.debug(`>>> Entering navigateWizard(wizardStep=${wizardStep},callback=${callback})`);
		logger.debug(`<<< Exiting navigateWizard(wizardStep=${wizardStep},callback=${callback})`);
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
		logger.debug(`>>> Entering setInstallingAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback})`);
		logger.debug(`<<< Exiting setInstallingAvailablePackage(FileName=${FileName},PackageType=${PackageType},callback=${callback})`);
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
		logger.debug(`>>> Entering notInstallingAvailablePackage(callback=${callback})`);
		logger.debug(`<<< Exiting notInstallingAvailablePackage(callback=${callback})`);
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
		logger.debug(`>>> Entering installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList})`);
        let _newExtension = JSON.parse(JSON.stringify(newExtension));
		logger.debug(`<<< Exiting installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList})`);
		logger.debug(`installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList}): _newExtension → ${_newExtension}`);
        return (dispatch) => {
            InstallationService.installPackage(file, legacyType, isPortalPackage, (data) => {
                dispatch({
                    type: ActionTypes.INSTALLED_EXTENSION_LOGS,
                    payload: JSON.parse(data)
                });
                if (addToList) {
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
					logger.debug(`installExtension(file=${file},newExtension=${newExtension},legacyType=${legacyType},isPortalPackage=${isPortalPackage},callback=${callback},addToList=${addToList}): _newExtension.inUse → ${_newExtension.inUse}`);
                    callback(data);
                }
            });
        };
    },
    clearParsedInstallationPackage(callback) {
		logger.debug(`>>> Entering clearParsedInstallationPackage(callback=${callback})`);
		logger.debug(`<<< Exiting clearParsedInstallationPackage(callback=${callback})`);
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
		logger.debug(`>>> Entering toggleAcceptLicense(value=${value},callback=${callback})`);
		logger.debug(`<<< Exiting toggleAcceptLicense(value=${value},callback=${callback})`);
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
		logger.debug(`>>> Entering setViewingLog(value=${value},callback=${callback})`);
		logger.debug(`<<< Exiting setViewingLog(value=${value},callback=${callback})`);
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
		logger.debug(`>>> Entering setIsPortalPackage(value=${value},callback=${callback})`);
		logger.debug(`<<< Exiting setIsPortalPackage(value=${value},callback=${callback})`);
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
