const logger = console;
import { createPackage as ActionTypes } from "constants/actionTypes";
import { CreatePackageService } from "services";

const createPackageActions = {
    getPackageManifest(packageId, callback) {
		logger.debug(`>>> Entering getPackageManifest(packageId=${packageId},callback=${callback})`);
		logger.debug(`<<< Exiting getPackageManifest(packageId=${packageId},callback=${callback})`);
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
		logger.debug(`>>> Entering updatePackagePayload(packagePayload=${packagePayload},callback=${callback})`);
		logger.debug(`<<< Exiting updatePackagePayload(packagePayload=${packagePayload},callback=${callback})`);
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
		logger.debug(`>>> Entering updatePackageManifest(packageManifest=${packageManifest},callback=${callback})`);
		logger.debug(`<<< Exiting updatePackageManifest(packageManifest=${packageManifest},callback=${callback})`);
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
		logger.debug(`>>> Entering createManifest(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting createManifest(payload=${payload},callback=${callback})`);
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
	logger.debug(`>>> Entering generateManifestPreview(payload=${payload},callback=${callback})`);

		logger.debug(`<<< Exiting generateManifestPreview(payload=${payload},callback=${callback})`);
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
		logger.debug(`>>> Entering createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting createPackage(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
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
		logger.debug(`>>> Entering goToWizardStep(step=${step},callback=${callback})`);
		logger.debug(`<<< Exiting goToWizardStep(step=${step},callback=${callback})`);
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
		logger.debug(`>>> Entering refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
		logger.debug(`<<< Exiting refreshPackageFiles(payload=${payload},callback=${callback},errorCallback=${errorCallback})`);
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
