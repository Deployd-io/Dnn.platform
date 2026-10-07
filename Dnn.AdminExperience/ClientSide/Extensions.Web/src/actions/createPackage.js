const logger = console;
import { createPackage as ActionTypes } from "constants/actionTypes";
import { CreatePackageService } from "services";

const createPackageActions = {
    getPackageManifest(packageId, callback) {
		logger.error(`>>> Entering getPackageManifest(packageId=${packageId},callback=${callback})`, { "event.action": "getPackageManifest", "logrite.packageId": packageId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPackageManifest(packageId=${packageId},callback=${callback})`, { "event.action": "getPackageManifest", "logrite.packageId": packageId, "logrite.callback": callback });
        return (dispatch) => {
            CreatePackageService.getPackageManifest(packageId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_MANIFEST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updatePackagePayload(packagePayload, callback) {
		logger.error(`>>> Entering updatePackagePayload(packagePayload=${packagePayload},callback=${callback})`, { "event.action": "updatePackagePayload", "logrite.packagePayload": packagePayload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updatePackagePayload(packagePayload=${packagePayload},callback=${callback})`, { "event.action": "updatePackagePayload", "logrite.packagePayload": packagePayload, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_PACKAGE_PAYLOAD,
                payload: packagePayload
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    updatePackageManifest(packageManifest, callback) {
		logger.error(`>>> Entering updatePackageManifest(packageManifest=${packageManifest},callback=${callback})`, { "event.action": "updatePackageManifest", "logrite.packageManifest": packageManifest, "logrite.callback": callback });
		logger.warn(`<<< Exiting updatePackageManifest(packageManifest=${packageManifest},callback=${callback})`, { "event.action": "updatePackageManifest", "logrite.packageManifest": packageManifest, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_PACKAGE_MANIFEST,
                payload: packageManifest
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    createManifest(payload, callback) {
		logger.error(`>>> Entering createManifest(payload=${payload},callback=${callback})`, { "event.action": "createManifest", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting createManifest(payload=${payload},callback=${callback})`, { "event.action": "createManifest", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            CreatePackageService.createManifest(payload, (data) => {
                dispatch({
                    type: ActionTypes.CREATED_PACKAGE_MANIFEST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    generateManifestPreview(payload, callback) {
	logger.error(`>>> Entering generateManifestPreview(payload=${payload},callback=${callback})`, { "event.action": "generateManifestPreview", "logrite.payload": payload, "logrite.callback": callback });

		logger.warn(`<<< Exiting generateManifestPreview(payload=${payload},callback=${callback})`, { "event.action": "generateManifestPreview", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            CreatePackageService.generateManifestPreview(payload, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_GENERATED_MANIFEST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createPackage(payload, callback, errorCallback) {
		logger.error(`>>> Entering createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createPackage", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "createPackage", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        return (dispatch) => {
            CreatePackageService.createPackage(payload, (data) => {
                dispatch({
                    type: ActionTypes.CREATED_PACKAGE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, (data) => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    },
    goToWizardStep(step, callback) {
		logger.error(`>>> Entering goToWizardStep(step=${step},callback=${callback})`, { "event.action": "goToWizardStep", "logrite.step": step, "logrite.callback": callback });
		logger.warn(`<<< Exiting goToWizardStep(step=${step},callback=${callback})`, { "event.action": "goToWizardStep", "logrite.step": step, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.GO_TO_STEP,
                payload: step
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    refreshPackageFiles(payload, callback, errorCallback) {
		logger.error(`>>> Entering refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "refreshPackageFiles", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
		logger.warn(`<<< Exiting refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`, { "event.action": "refreshPackageFiles", "logrite.payload": payload, "logrite.callback": callback, "logrite.errorCallback": errorCallback });
        return (dispatch) => {
            CreatePackageService.refreshPackageFiles(payload, (data) => {
                dispatch({
                    type: ActionTypes.REFRESH_PACKAGE_FILES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, (data) => {
                if (errorCallback) {
                    errorCallback(data);
                }
            });
        };
    }
};

export default createPackageActions;
