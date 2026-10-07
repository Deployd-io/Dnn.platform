import { languageEditor as ActionTypes } from "constants/actionTypes";
import LanguageEditorService from "services/languageEditorService";
const logger = console;
const languageEditorActions = {
    setLanguageBeingEdited(language) {
		logger.error(`>>> Entering setLanguageBeingEdited(language=${language})`, { "event.action": "setLanguageBeingEdited", "logrite.language": language });
		logger.warn(`<<< Exiting setLanguageBeingEdited(language=${language})`, { "event.action": "setLanguageBeingEdited", "logrite.language": language });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_LANGUAGE_BEING_EDITED,
                payload: language
            });
        };
    },
    getRootResourcesFolder(portalId, mode, callback) {
		logger.error(`>>> Entering getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`, { "event.action": "getRootResourcesFolder", "logrite.portalId": portalId, "logrite.mode": mode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`, { "event.action": "getRootResourcesFolder", "logrite.portalId": portalId, "logrite.mode": mode, "logrite.callback": callback });
        return (dispatch) => {
            LanguageEditorService.getRootResourcesFolder(portalId, mode, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_ROOT_RESOURCES_FOLDER,
                    payload: data
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getSubRootResources(folder, callback) {
		logger.error(`>>> Entering getSubRootResources(folder=${folder},callback=${callback})`, { "event.action": "getSubRootResources", "logrite.folder": folder, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSubRootResources(folder=${folder},callback=${callback})`, { "event.action": "getSubRootResources", "logrite.folder": folder, "logrite.callback": callback });
        return (dispatch) => {
            LanguageEditorService.getSubRootResources(folder, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SUBROOT_RESOURCES_FOLDER,
                    payload: data
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    getResxEntries(parameters, callback) {
		logger.error(`>>> Entering getResxEntries(parameters=${parameters},callback=${callback})`, { "event.action": "getResxEntries", "logrite.parameters": parameters, "logrite.callback": callback });
		logger.error(`getResxEntries(parameters=${parameters},callback=${callback}): _parameters → ${_parameters}`, { "event.action": "getResxEntries", "logrite.parameters": parameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getResxEntries(parameters=${parameters},callback=${callback})`, { "event.action": "getResxEntries", "logrite.parameters": parameters, "logrite.callback": callback });
        return (dispatch) => {
            let _parameters = Object.assign({}, parameters);
            delete _parameters.resourceFileActual;
            LanguageEditorService.getResxEntries(_parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_RESX_ENTRIES,
                    payload: Object.assign(parameters, data)
                });
                if (callback) {
                    callback();
                }
            });
        };
    },
    updateResxEntry(updatedList, callback) {
		logger.error(`>>> Entering updateResxEntry(updatedList=${updatedList},callback=${callback})`, { "event.action": "updateResxEntry", "logrite.updatedList": updatedList, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateResxEntry(updatedList=${updatedList},callback=${callback})`, { "event.action": "updateResxEntry", "logrite.updatedList": updatedList, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_RESX_ENTRIES,
                payload: updatedList
            });
            if (callback) {
                callback();
            }
        };
    },
    saveTranslations(payload, callback) {
		logger.error(`>>> Entering saveTranslations(payload=${payload},callback=${callback})`, { "event.action": "saveTranslations", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting saveTranslations(payload=${payload},callback=${callback})`, { "event.action": "saveTranslations", "logrite.payload": payload, "logrite.callback": callback });
        return () => {
            LanguageEditorService.saveTranslations(payload, (data) => {
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default languageEditorActions;