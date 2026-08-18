import { languageEditor as ActionTypes } from "constants/actionTypes";
import LanguageEditorService from "services/languageEditorService";
const logger = console;
const languageEditorActions = {
    setLanguageBeingEdited(language) {
		logger.debug(`>>> Entering setLanguageBeingEdited(language=${language})`);
		logger.debug(`<<< Exiting setLanguageBeingEdited(language=${language})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_LANGUAGE_BEING_EDITED,
                payload: language
            });
        };
    },
    getRootResourcesFolder(portalId, mode, callback) {
		logger.debug(`>>> Entering getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`);
		logger.debug(`<<< Exiting getRootResourcesFolder(portalId=${portalId},mode=${mode},callback=${callback})`);
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
		logger.debug(`>>> Entering getSubRootResources(folder=${folder},callback=${callback})`);
		logger.debug(`<<< Exiting getSubRootResources(folder=${folder},callback=${callback})`);
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
		logger.debug(`>>> Entering getResxEntries(parameters=${parameters},callback=${callback})`);
		logger.debug(`<<< Exiting getResxEntries(parameters=${parameters},callback=${callback})`);
        return (dispatch) => {
            let _parameters = Object.assign({}, parameters);
			logger.debug(`getResxEntries(parameters=${parameters},callback=${callback}): _parameters → ${_parameters}`);
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
		logger.debug(`>>> Entering updateResxEntry(updatedList=${updatedList},callback=${callback})`);
		logger.debug(`<<< Exiting updateResxEntry(updatedList=${updatedList},callback=${callback})`);
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
		logger.debug(`>>> Entering saveTranslations(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting saveTranslations(payload=${payload},callback=${callback})`);
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