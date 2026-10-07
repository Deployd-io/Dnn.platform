import { extension as ActionTypes, installation as InstallationActionTypes } from "constants/actionTypes";
import { ExtensionService } from "services";
import { validationMapExtensionBeingEdited } from "utils/helperFunctions";
import Localization from "localization";
import utilities from "utils";
const logger = console;

function errorCallback(error) {
	logger.error(`>>> Entering errorCallback(error=${error})`, { "event.action": "errorCallback", "logrite.error": error });
    let message = utilities.utilities.getResx("SharedResources", "GenericErrorMessage.Error");
    if (error.Error) {
        message = error.Error;
    } else if (error.Message) {
        message = error.Message;
    } else if (typeof error === "string") {
        message = error;
    }
    utilities.utilities.notifyError(message);
	logger.warn(`<<< Exiting errorCallback(error=${error})`, { "event.action": "errorCallback", "logrite.error": error });
}
function valueMapExtensionBeingEdited(extensionBeingEdited) {
	logger.error(`>>> Entering valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`, { "event.action": "valueMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        _extensionBeingEdited[key] = _extensionBeingEdited[key].value;
    });
	logger.error(`valueMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`, { "event.action": "valueMapExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited });
    return _extensionBeingEdited;
}
const extensionActions = {
    getInstalledPackages(type, callback) {
		logger.error(`>>> Entering getInstalledPackages(type=${type},callback=${callback})`, { "event.action": "getInstalledPackages", "logrite.type": type, "logrite.callback": callback });
		logger.warn(`<<< Exiting getInstalledPackages(type=${type},callback=${callback})`, { "event.action": "getInstalledPackages", "logrite.type": type, "logrite.callback": callback });
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
		logger.error(`>>> Entering getAvailablePackages(type=${type},callback=${callback})`, { "event.action": "getAvailablePackages", "logrite.type": type, "logrite.callback": callback });
		logger.warn(`<<< Exiting getAvailablePackages(type=${type},callback=${callback})`, { "event.action": "getAvailablePackages", "logrite.type": type, "logrite.callback": callback });
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
		logger.error(`>>> Entering getPackageTypes(callback=${callback})`, { "event.action": "getPackageTypes", "logrite.callback": callback });
		logger.warn(`<<< Exiting getPackageTypes(callback=${callback})`, { "event.action": "getPackageTypes", "logrite.callback": callback });
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
		logger.error(`>>> Entering getAvailablePackageTypes(callback=${callback})`, { "event.action": "getAvailablePackageTypes", "logrite.callback": callback });
		logger.warn(`<<< Exiting getAvailablePackageTypes(callback=${callback})`, { "event.action": "getAvailablePackageTypes", "logrite.callback": callback });
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
		logger.error(`>>> Entering updateExtension(updatedExtension=${updatedExtension},editorActions=${editorActions},index=${index},callback=${callback})`, { "event.action": "updateExtension", "logrite.updatedExtension": updatedExtension, "logrite.editorActions": editorActions, "logrite.index": index, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateExtension(updatedExtension=${updatedExtension},editorActions=${editorActions},index=${index},callback=${callback})`, { "event.action": "updateExtension", "logrite.updatedExtension": updatedExtension, "logrite.editorActions": editorActions, "logrite.index": index, "logrite.callback": callback });
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
		logger.error(`>>> Entering downloadPackage(packageType=${packageType},packageName=${packageName},callback=${callback})`, { "event.action": "downloadPackage", "logrite.packageType": packageType, "logrite.packageName": packageName, "logrite.callback": callback });
		logger.warn(`<<< Exiting downloadPackage(packageType=${packageType},packageName=${packageName},callback=${callback})`, { "event.action": "downloadPackage", "logrite.packageType": packageType, "logrite.packageName": packageName, "logrite.callback": callback });
        return () => {
            ExtensionService.downloadPackage(packageType, packageName, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    installAvailablePackage(packageType, packageName, newExtension, callback) {
		logger.error(`>>> Entering installAvailablePackage(packageType=${packageType},packageName=${packageName},newExtension=${newExtension},callback=${callback})`, { "event.action": "installAvailablePackage", "logrite.packageType": packageType, "logrite.packageName": packageName, "logrite.newExtension": newExtension, "logrite.callback": callback });
        return (dispatch) => {
            ExtensionService.installAvailablePackage(packageType, packageName, (data) => {
				logger.warn(`<<< Exiting installAvailablePackage(packageType=${packageType},packageName=${packageName},newExtension=${newExtension},callback=${callback})`, { "event.action": "installAvailablePackage", "logrite.packageType": packageType, "logrite.packageName": packageName, "logrite.newExtension": newExtension, "logrite.callback": callback });
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
		logger.error(`>>> Entering parseAvailablePackage(packageType=${packageType},fileName=${fileName},callback=${callback})`, { "event.action": "parseAvailablePackage", "logrite.packageType": packageType, "logrite.fileName": fileName, "logrite.callback": callback });
		logger.warn(`<<< Exiting parseAvailablePackage(packageType=${packageType},fileName=${fileName},callback=${callback})`, { "event.action": "parseAvailablePackage", "logrite.packageType": packageType, "logrite.fileName": fileName, "logrite.callback": callback });
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
		logger.error(`>>> Entering deletePackage(payload=${payload},index=${index},callback=${callback})`, { "event.action": "deletePackage", "logrite.payload": payload, "logrite.index": index, "logrite.callback": callback });
		logger.warn(`<<< Exiting deletePackage(payload=${payload},index=${index},callback=${callback})`, { "event.action": "deletePackage", "logrite.payload": payload, "logrite.index": index, "logrite.callback": callback });
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
		logger.error(`>>> Entering setPackageBeingDeleted(extensionBeingDeleted=${extensionBeingDeleted},extensionBeingDeletedIndex=${extensionBeingDeletedIndex},callback=${callback})`, { "event.action": "setPackageBeingDeleted", "logrite.extensionBeingDeleted": extensionBeingDeleted, "logrite.extensionBeingDeletedIndex": extensionBeingDeletedIndex, "logrite.callback": callback });
		logger.warn(`<<< Exiting setPackageBeingDeleted(extensionBeingDeleted=${extensionBeingDeleted},extensionBeingDeletedIndex=${extensionBeingDeletedIndex},callback=${callback})`, { "event.action": "setPackageBeingDeleted", "logrite.extensionBeingDeleted": extensionBeingDeleted, "logrite.extensionBeingDeletedIndex": extensionBeingDeletedIndex, "logrite.callback": callback });
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
		logger.error(`>>> Entering toggleDeleteFiles(callback=${callback})`, { "event.action": "toggleDeleteFiles", "logrite.callback": callback });
		logger.warn(`<<< Exiting toggleDeleteFiles(callback=${callback})`, { "event.action": "toggleDeleteFiles", "logrite.callback": callback });
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
		logger.error(`>>> Entering createNewModule(payload=${payload},shouldAppend=${shouldAppend},callback=${callback})`, { "event.action": "createNewModule", "logrite.payload": payload, "logrite.shouldAppend": shouldAppend, "logrite.callback": callback });
		logger.warn(`<<< Exiting createNewModule(payload=${payload},shouldAppend=${shouldAppend},callback=${callback})`, { "event.action": "createNewModule", "logrite.payload": payload, "logrite.shouldAppend": shouldAppend, "logrite.callback": callback });
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
		logger.error(`>>> Entering addExtension(parameters=${parameters},callback=${callback})`, { "event.action": "addExtension", "logrite.parameters": parameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting addExtension(parameters=${parameters},callback=${callback})`, { "event.action": "addExtension", "logrite.parameters": parameters, "logrite.callback": callback });
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
		logger.error(`>>> Entering createNewExtension(extensionBeingAdded=${extensionBeingAdded},editorActions=${editorActions},index=${index},callback=${callback})`, { "event.action": "createNewExtension", "logrite.extensionBeingAdded": extensionBeingAdded, "logrite.editorActions": editorActions, "logrite.index": index, "logrite.callback": callback });
        return (dispatch) => {
            ExtensionService.createNewExtension(extensionBeingAdded, editorActions, (data) => {
				logger.warn(`<<< Exiting createNewExtension(extensionBeingAdded=${extensionBeingAdded},editorActions=${editorActions},index=${index},callback=${callback})`, { "event.action": "createNewExtension", "logrite.extensionBeingAdded": extensionBeingAdded, "logrite.editorActions": editorActions, "logrite.index": index, "logrite.callback": callback });
                let _extensionBeingAdded = JSON.parse(JSON.stringify(extensionBeingAdded));
                _extensionBeingAdded.packageId = {};
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
		logger.error(`>>> Entering editExtension(parameters=${parameters},extensionBeingEditedIndex=${extensionBeingEditedIndex},callback=${callback})`, { "event.action": "editExtension", "logrite.parameters": parameters, "logrite.extensionBeingEditedIndex": extensionBeingEditedIndex, "logrite.callback": callback });
		logger.warn(`<<< Exiting editExtension(parameters=${parameters},extensionBeingEditedIndex=${extensionBeingEditedIndex},callback=${callback})`, { "event.action": "editExtension", "logrite.parameters": parameters, "logrite.extensionBeingEditedIndex": extensionBeingEditedIndex, "logrite.callback": callback });
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
		logger.error('>>> Entering toggleTriedToSave()', { "event.action": "toggleTriedToSave" });
		logger.warn('<<< Exiting toggleTriedToSave()', { "event.action": "toggleTriedToSave" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.TOGGLE_TRIED_TO_SAVE
            });
        };
    },
    updateExtensionBeingEdited(extensionBeingEdited, callback) {
		logger.error(`>>> Entering updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`, { "event.action": "updateExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited},callback=${callback})`, { "event.action": "updateExtensionBeingEdited", "logrite.extensionBeingEdited": extensionBeingEdited, "logrite.callback": callback });
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
		logger.error(`>>> Entering toggleTabError(tabIndex=${tabIndex},action=${action},callback=${callback})`, { "event.action": "toggleTabError", "logrite.tabIndex": tabIndex, "logrite.action": action, "logrite.callback": callback });
		logger.warn(`<<< Exiting toggleTabError(tabIndex=${tabIndex},action=${action},callback=${callback})`, { "event.action": "toggleTabError", "logrite.tabIndex": tabIndex, "logrite.action": action, "logrite.callback": callback });
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
		logger.error(`>>> Entering getModuleCategories(callback=${callback})`, { "event.action": "getModuleCategories", "logrite.callback": callback });
		logger.warn(`<<< Exiting getModuleCategories(callback=${callback})`, { "event.action": "getModuleCategories", "logrite.callback": callback });
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
		logger.error(`>>> Entering selectEditingTab(index=${index},callback=${callback})`, { "event.action": "selectEditingTab", "logrite.index": index, "logrite.callback": callback });
		logger.warn(`<<< Exiting selectEditingTab(index=${index},callback=${callback})`, { "event.action": "selectEditingTab", "logrite.index": index, "logrite.callback": callback });
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
		logger.error(`>>> Entering getLocaleList(callback=${callback})`, { "event.action": "getLocaleList", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLocaleList(callback=${callback})`, { "event.action": "getLocaleList", "logrite.callback": callback });
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
		logger.error(`>>> Entering getLocalePackageList(callback=${callback})`, { "event.action": "getLocalePackageList", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLocalePackageList(callback=${callback})`, { "event.action": "getLocalePackageList", "logrite.callback": callback });
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
		logger.error(`>>> Entering deployAvailablePackage(_package=${_package},updatedPackageIndex=${updatedPackageIndex},callback=${callback})`, { "event.action": "deployAvailablePackage", "logrite._package": _package, "logrite.updatedPackageIndex": updatedPackageIndex, "logrite.callback": callback });
		logger.warn(`<<< Exiting deployAvailablePackage(_package=${_package},updatedPackageIndex=${updatedPackageIndex},callback=${callback})`, { "event.action": "deployAvailablePackage", "logrite._package": _package, "logrite.updatedPackageIndex": updatedPackageIndex, "logrite.callback": callback });
        return () => {
            ExtensionService.deployAvailablePackage(_package.description, (data) => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    GetPackageUsageFilter(callback) {
		logger.error(`>>> Entering GetPackageUsageFilter(callback=${callback})`, { "event.action": "GetPackageUsageFilter", "logrite.callback": callback });
		logger.warn(`<<< Exiting GetPackageUsageFilter(callback=${callback})`, { "event.action": "GetPackageUsageFilter", "logrite.callback": callback });
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
		logger.error(`>>> Entering getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`, { "event.action": "getPackageUsage", "logrite.portalId": portalId, "logrite.packageId": packageId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPackageUsage(portalId=${portalId},packageId=${packageId},callback=${callback})`, { "event.action": "getPackageUsage", "logrite.portalId": portalId, "logrite.packageId": packageId, "logrite.callback": callback });
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
