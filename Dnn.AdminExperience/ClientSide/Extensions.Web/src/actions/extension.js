import { extension as ActionTypes, installation as InstallationActionTypes } from "constants/actionTypes";
import { ExtensionService } from "services";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import Localization from "localization";
import utilities from "utils";
const logger = console;

function errorCallback(error) {
	logger.debug(`>>> Entering errorCallback(error=${error})`);
    let message = utilities.utilities.getResx("SharedResources", "GenericErrorMessage.Error");
    if (error.Error) {
        message = error.Error;
    } else if (error.Message) {
        message = error.Message;
    } else if (typeof error === "string") {
        message = error;
    }
    utilities.utilities.notifyError(message);
	logger.debug(`<<< Exiting errorCallback(error=${error})`);
}
function valueMapExtensionBeingEdited(extensionBeingEdited) {
	logger.debug(`>>> Entering valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        _extensionBeingEdited[key] = _extensionBeingEdited[key].value;
    });
	logger.debug(`valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`);
	logger.debug(`<<< Exiting valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    return _extensionBeingEdited;
}
const extensionActions = {
    getInstalledPackages(type, callback) {
		logger.debug(`>>> Entering getInstalledPackages(type=${type},callback=${callback})`);
		logger.debug(`<<< Exiting getInstalledPackages(type=${type},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getInstalledPackages(type, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_INSTALLED_PACKAGES,
                    payload: {
                        Results: payload.Results,
                        selectedInstalledPackageType: type
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getAvailablePackages(type, callback) {
		logger.debug(`>>> Entering getAvailablePackages(type=${type},callback=${callback})`);
		logger.debug(`<<< Exiting getAvailablePackages(type=${type},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getAvailablePackages(type, payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_AVAILABLE_PACKAGES,
                    payload: {
                        Results: payload.Results && payload.Results[0] && payload.Results[0].ValidPackages,
                        selectedAvailablePackageType: type
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getPackageTypes(callback) {
		logger.debug(`>>> Entering getPackageTypes(callback=${callback})`);
		logger.debug(`<<< Exiting getPackageTypes(callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getPackageTypes(payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_INSTALLED_PACKAGE_TYPES,
                    payload: payload
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getAvailablePackageTypes(callback) {
		logger.debug(`>>> Entering getAvailablePackageTypes(callback=${callback})`);
		logger.debug(`<<< Exiting getAvailablePackageTypes(callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getAvailablePackageTypes(payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_AVAILABLE_PACKAGE_TYPES,
                    payload: payload
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    updateExtension(updatedExtension, editorActions, index, callback) {
		logger.debug(`>>> Entering updateExtension(updatedExtension=${updatedExtension},editorActions=${editorActions},index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting updateExtension(updatedExtension=${updatedExtension},editorActions=${editorActions},index=${index},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.updateExtension(updatedExtension, editorActions, (data) => {
                dispatch({
                    type: ActionTypes.UPDATED_EXTENSION_BEING_EDITED,
                    payload: {
                        extensionBeingEdited: validationMapExtensionBeingEdited(data.PackageDetail)
                    }
                });
                dispatch({
                    type: ActionTypes.UPDATED_EXTENSION,
                    payload: {
                        index,
                        updatedExtension: valueMapExtensionBeingEdited(updatedExtension)
                    }
                });
                utilities.utilities.notify(Localization.get("EditExtension_Notify.Success"));
                if (callback) {
                    utilities.utilities.throttleExecution(callback);
                }
            }, errorCallback);
        };
    },
    downloadPackage(packageType, packageName, callback) {
		logger.debug(`>>> Entering downloadPackage(packageType=${packageType},packageName=${packageName},callback=${callback})`);
		logger.debug(`<<< Exiting downloadPackage(packageType=${packageType},packageName=${packageName},callback=${callback})`);
        return () => {
            ExtensionService.downloadPackage(packageType, packageName, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    installAvailablePackage(packageType, packageName, newExtension, callback) {
		logger.debug(`>>> Entering installAvailablePackage(packageType=${packageType},packageName=${packageName},newExtension=${newExtension},callback=${callback})`);
		logger.debug(`<<< Exiting installAvailablePackage(packageType=${packageType},packageName=${packageName},newExtension=${newExtension},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.installAvailablePackage(packageType, packageName, (data) => {
                if (data.success) {
                    let _newExtension = JSON.parse(JSON.stringify(newExtension));
                    _newExtension.packageId = data.newPackageId;
                    dispatch({
                        type: ActionTypes.INSTALLED_EXTENSION,
                        payload: {
                            PackageInfo: _newExtension,
                            logs: data.logs
                        }
                    });
                }
                if (!data.success) {
                    dispatch({
                        type: InstallationActionTypes.SET_FAILED_INSTALLATION_LOGS
                    });
                }
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    parseAvailablePackage(packageType, fileName, callback) {
		logger.debug(`>>> Entering parseAvailablePackage(packageType=${packageType},fileName=${fileName},callback=${callback})`);
		logger.debug(`<<< Exiting parseAvailablePackage(packageType=${packageType},fileName=${fileName},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.parseAvailablePackage(packageType, fileName, (data) => {
                dispatch({
                    type: InstallationActionTypes.PARSED_INSTALLATION_PACKAGE,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deletePackage(payload, index, callback) {
		logger.debug(`>>> Entering deletePackage(payload=${payload},index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting deletePackage(payload=${payload},index=${index},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.deletePackage(payload, (data) => {
                dispatch({
                    type: ActionTypes.DELETED_EXTENSION,
                    payload: {
                        index
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    setPackageBeingDeleted(extensionBeingDeleted, extensionBeingDeletedIndex, callback) {
		logger.debug(`>>> Entering setPackageBeingDeleted(extensionBeingDeleted=${extensionBeingDeleted},extensionBeingDeletedIndex=${extensionBeingDeletedIndex},callback=${callback})`);
		logger.debug(`<<< Exiting setPackageBeingDeleted(extensionBeingDeleted=${extensionBeingDeleted},extensionBeingDeletedIndex=${extensionBeingDeletedIndex},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_EXTENSION_BEING_DELETED,
                payload: {
                    extensionBeingDeleted,
                    extensionBeingDeletedIndex
                }
            });
            if (callback) {
                callback();
            }
        };
    },
    toggleDeleteFiles(callback) {
		logger.debug(`>>> Entering toggleDeleteFiles(callback=${callback})`);
		logger.debug(`<<< Exiting toggleDeleteFiles(callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_DELETE_EXTENSION_FILES
            });
            if (callback) {
                callback();
            }
        };
    },
    createNewModule(payload, shouldAppend, callback) {
		logger.debug(`>>> Entering createNewModule(payload=${payload},shouldAppend=${shouldAppend},callback=${callback})`);
		logger.debug(`<<< Exiting createNewModule(payload=${payload},shouldAppend=${shouldAppend},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.createNewModule(payload, (data) => {
                if (shouldAppend) {
                    dispatch({
                        type: ActionTypes.CREATED_NEW_MODULE,
                        payload: data
                    });
                }
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    addExtension(parameters, callback) {
		logger.debug(`>>> Entering addExtension(parameters=${parameters},callback=${callback})`);
		logger.debug(`<<< Exiting addExtension(parameters=${parameters},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.EDIT_EXTENSION,
                payload: {
                    extensionBeingEdited: parameters,
                    extensionBeingEditedIndex: -1
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    createNewExtension(extensionBeingAdded, editorActions, index, callback) {
		logger.debug(`>>> Entering createNewExtension(extensionBeingAdded=${extensionBeingAdded},editorActions=${editorActions},index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting createNewExtension(extensionBeingAdded=${extensionBeingAdded},editorActions=${editorActions},index=${index},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.createNewExtension(extensionBeingAdded, editorActions, (data) => {
                let _extensionBeingAdded = JSON.parse(JSON.stringify(extensionBeingAdded));
                _extensionBeingAdded.packageId = {};
				logger.debug(`createNewExtension(extensionBeingAdded=${extensionBeingAdded},editorActions=${editorActions},index=${index},callback=${callback}): _extensionBeingAdded.packageId → ${_extensionBeingAdded.packageId}`);
                _extensionBeingAdded.packageId.value = data.PackageId;
                dispatch({
                    type: ActionTypes.ADDED_NEW_EXTENSION,
                    payload: {
                        PackageInfo: valueMapExtensionBeingEdited(_extensionBeingAdded)
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    editExtension(parameters, extensionBeingEditedIndex, callback) {
		logger.debug(`>>> Entering editExtension(parameters=${parameters},extensionBeingEditedIndex=${extensionBeingEditedIndex},callback=${callback})`);
		logger.debug(`<<< Exiting editExtension(parameters=${parameters},extensionBeingEditedIndex=${extensionBeingEditedIndex},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getPackageSettings(parameters, (data) => {
                dispatch({
                    type: ActionTypes.EDIT_EXTENSION,
                    payload: {
                        extensionBeingEdited: data,
                        extensionBeingEditedIndex: extensionBeingEditedIndex
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    toggleTriedToSave() {
		logger.debug('>>> Entering toggleTriedToSave()');
		logger.debug('<<< Exiting toggleTriedToSave()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_TRIED_TO_SAVE
            });
        };
    },
    updateExtensionBeingEdited(extensionBeingEdited, callback) {
		logger.debug(`>>> Entering updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`);
		logger.debug(`<<< Exiting updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_EXTENSION_BEING_EDITED,
                payload: {
                    extensionBeingEdited: extensionBeingEdited
                }
            });
            if (callback) {
                setTimeout(() => {  //let JS propagate
                    callback();
                }, 0);
            }
        };
    },
    toggleTabError(tabIndex, action, callback) {
		logger.debug(`>>> Entering toggleTabError(tabIndex=${tabIndex},action=${action},callback=${callback})`);
		logger.debug(`<<< Exiting toggleTabError(tabIndex=${tabIndex},action=${action},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_TAB_ERROR,
                payload: {
                    tabIndex,
                    action
                }
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    getModuleCategories(callback) {
		logger.debug(`>>> Entering getModuleCategories(callback=${callback})`);
		logger.debug(`<<< Exiting getModuleCategories(callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getModuleCategories((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_MODULE_CATEGORIES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    selectEditingTab(index, callback) {
		logger.debug(`>>> Entering selectEditingTab(index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting selectEditingTab(index=${index},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECT_EDITING_TAB,
                payload: index
            });
            if (callback) {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    getLocaleList(callback) {
		logger.debug(`>>> Entering getLocaleList(callback=${callback})`);
		logger.debug(`<<< Exiting getLocaleList(callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getLocaleList((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOCALE_LIST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getLocalePackageList(callback) {
		logger.debug(`>>> Entering getLocalePackageList(callback=${callback})`);
		logger.debug(`<<< Exiting getLocalePackageList(callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getLocalePackagesList((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_LOCALE_LIST,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    deployAvailablePackage(_package, updatedPackageIndex, callback) {
		logger.debug(`>>> Entering deployAvailablePackage(_package=${_package},updatedPackageIndex=${updatedPackageIndex},callback=${callback})`);
		logger.debug(`<<< Exiting deployAvailablePackage(_package=${_package},updatedPackageIndex=${updatedPackageIndex},callback=${callback})`);
        return () => {
            ExtensionService.deployAvailablePackage(_package.description, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    GetPackageUsageFilter(callback) {
		logger.debug(`>>> Entering GetPackageUsageFilter(callback=${callback})`);
		logger.debug(`<<< Exiting GetPackageUsageFilter(callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getPackageUsageFilter(payload => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_USAGE_FILTER,
                    payload: payload
                });
                if (callback) {
                    callback();
                }
            }, errorCallback); 
        };
    },
    getPackageUsage(portalId, packageId, callback) {
		logger.debug(`>>> Entering getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`);
		logger.debug(`<<< Exiting getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`);
        return (dispatch) => {
            ExtensionService.getPackageUsage(portalId, packageId, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PACKAGE_USAGE,
                    payload: data
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default extensionActions;
