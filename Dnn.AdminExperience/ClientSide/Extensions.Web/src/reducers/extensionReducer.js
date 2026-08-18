const logger = console;
import { extension as ActionTypes } from "../constants/actionTypes";

function getAvailablePackageTypes(installedTypes) {
	logger.debug(`>>> Entering getAvailablePackageTypes(installedTypes=${installedTypes})`);
	logger.debug(`<<< Exiting getAvailablePackageTypes(installedTypes=${installedTypes})`);
    return installedTypes.filter((type) => {
        return type.HasAvailablePackages === true;
    });
}

function addToModuleList(value, list) {
	logger.debug(`>>> Entering addToModuleList(value=${value},list=${list})`);
    return list.concat(value).sort(function (a, b) {
	logger.debug(`>>> Entering sort(value=${value})`);
        if (a.friendlyName.toLowerCase() < b.friendlyName.toLowerCase()) return -1;
        if (a.friendlyName.toLowerCase() > b.friendlyName.toLowerCase()) return 1;
	logger.debug(`<<< Exiting addToModuleList(value=${value},list=${list})`);
        return 0;
    });
}

function getValidateRequired(key) {
	logger.debug(`>>> Entering getValidateRequired(key=${key})`);
    switch (key) {
        case "friendlyName":
            return true;
        default:
			logger.debug(`<<< Exiting getValidateRequired(key=${key})`);
            return false;
    }
}

function getTabMapping(key) {
	logger.debug(`>>> Entering getTabMapping(key=${key})`);
    switch (key) {
        case "license":
            return 3;
        case "releaseNotes":
            return 4;
        default:
			logger.debug(`<<< Exiting getTabMapping(key=${key})`);
            return 0;
    }
}

function validationMapExtensionBeingEdited(extensionBeingEdited) {
	logger.debug(`>>> Entering validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    let _extensionBeingEdited = Object.assign({}, extensionBeingEdited);
    Object.keys(_extensionBeingEdited).forEach((key) => {
        let validateRequired = getValidateRequired(key);
        let tabMapping = getTabMapping(key);
        _extensionBeingEdited[key] = {
            value: _extensionBeingEdited[key] && !Object.prototype.hasOwnProperty.call(_extensionBeingEdited[key], "value") ? _extensionBeingEdited[key] : _extensionBeingEdited[key] && _extensionBeingEdited[key].value,
            validateRequired,
            tabMapping,
            error: false
        };
    });
	logger.debug(`validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited}): _extensionBeingEdited → ${_extensionBeingEdited}`);
	logger.debug(`<<< Exiting validationMapExtensionBeingEdited(extensionBeingEdited=${extensionBeingEdited})`);
    return _extensionBeingEdited;
}

function removeRecordFromArray(arr, index) {
	logger.debug(`>>> Entering removeRecordFromArray(arr=${arr},index=${index})`);
	logger.debug(`<<< Exiting removeRecordFromArray(arr=${arr},index=${index})`);
    return [...arr.slice(0, index), ...arr.slice(index + 1)];
}
export default function extension(state = {
    installedPackages: [],
    availablePackages: [],
    installedPackageTypes: [],
    availablePackageTypes: [],
    packageBeingEditedSettings: {},
    extensionBeingEdited: {},
    extensionBeingEditedIndex: -1,
    selectedInstalledPackageType: "",
    selectedAvailablePackageType: "",
    triedToSave: false,
    tabsWithError: [],
    moduleCategories: [],
    tabBeingEdited: 0,
    locales: [],
    localePackages: [],
    extensionBeingDeleted: {},
    extensionBeingDeletedIndex: -1,
    deleteExtensionFiles: false
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_INSTALLED_PACKAGES:
            return { ...state,
                installedPackages: action.payload.Results,
                selectedInstalledPackageType: action.payload.selectedInstalledPackageType
            };
        case ActionTypes.RETRIEVED_AVAILABLE_PACKAGES:
            return { ...state,
                availablePackages: action.payload.Results,
                selectedAvailablePackageType: action.payload.selectedAvailablePackageType
            };
        case ActionTypes.UPDATED_EXTENSION:
            return { ...state,
                installedPackages: [...state.installedPackages.slice(0, action.payload.index), action.payload.updatedExtension, ...state.installedPackages.slice(action.payload.index + 1)]
            };
        case ActionTypes.DELETED_EXTENSION:
            return { ...state,
                installedPackages: removeRecordFromArray(state.installedPackages, action.payload.index),
                deleteExtensionFiles: false
            };
        case ActionTypes.SET_EXTENSION_BEING_DELETED:
            return {...state,
                extensionBeingDeleted: action.payload.extensionBeingDeleted,
                extensionBeingDeletedIndex: action.payload.extensionBeingDeletedIndex
            };
        case ActionTypes.TOGGLE_DELETE_EXTENSION_FILES:
            return { ...state,
                deleteExtensionFiles: !state.deleteExtensionFiles
            };
        case ActionTypes.EDIT_EXTENSION:
            return { ...state,
                extensionBeingEdited: validationMapExtensionBeingEdited(action.payload.extensionBeingEdited),
                extensionBeingEditedIndex: action.payload.extensionBeingEditedIndex
            };
        case ActionTypes.TOGGLE_TRIED_TO_SAVE:
            return { ...state,
                triedToSave: !state.triedToSave
            };
        case ActionTypes.TOGGLE_TAB_ERROR:
            return { ...state,
                tabsWithError: action.payload.action === "remove" ?
                    removeRecordFromArray(state.tabsWithError, state.tabsWithError.indexOf(action.payload.tabIndex)) :
                    (state.tabsWithError.indexOf(action.payload.tabIndex) < 0 ? state.tabsWithError.concat(action.payload.tabIndex) : state.tabsWithError)
            };
        case ActionTypes.UPDATED_EXTENSION_BEING_EDITED:
            return { ...state,
                extensionBeingEdited: action.payload.extensionBeingEdited
            };
        case ActionTypes.RETRIEVED_INSTALLED_PACKAGE_TYPES:
            return { ...state,
                installedPackageTypes: action.payload.Results,
                availablePackageTypes: getAvailablePackageTypes(action.payload.Results)
            };
        case ActionTypes.RETRIEVED_AVAILABLE_PACKAGE_TYPES:
            return { ...state,
                availablePackageTypes: action.payload.Results
            };
        case ActionTypes.ADDED_NEW_EXTENSION:
        case ActionTypes.INSTALLED_EXTENSION:
        case ActionTypes.CREATED_NEW_MODULE:
            return { ...state,
                installedPackages: addToModuleList(action.payload.PackageInfo, state.installedPackages)
            };
        case ActionTypes.RETRIEVED_MODULE_CATEGORIES:
            return { ...state,
                moduleCategories: action.payload
            };
        case ActionTypes.SELECT_EDITING_TAB:
            return { ...state,
                tabBeingEdited: action.payload
            };
        case ActionTypes.RETRIEVED_LOCALE_LIST:
            return { ...state,
                locales: action.payload
            };
        case ActionTypes.RETRIEVED_PACKAGE_LOCALE_LIST:
            return { ...state,
                localePackages: action.payload
            };
        case ActionTypes.DEPLOYED_AVAILABLE_PACKAGE:
            return { ...state,
                availablePackages: [...state.availablePackages.slice(0, action.payload.updatedPackageIndex), action.payload.updatedPackage, ...state.availablePackages.slice(action.payload.updatedPackageIndex + 1)]
            };
        case ActionTypes.RETRIEVED_PACKAGE_USAGE_FILTER:
            return { ...state,
                usageFilter: action.payload.Results
            };
        case ActionTypes.RETRIEVED_PACKAGE_USAGE:
            return { ...state,
                tabUrls: action.payload.Results
            };
        default:
            return state;
    }
}
