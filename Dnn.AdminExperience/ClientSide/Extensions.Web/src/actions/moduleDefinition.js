const logger = console;
import { moduleDefinition as ActionTypes } from "constants/actionTypes";
import { ModuleDefinitionService } from "services";
const moduleDefinitionActions = {
    setFormDirt(value, callback) {
		logger.error(`>>> Entering setFormDirt(value=${value},callback=${callback})`, { "event.action": "setFormDirt", "logrite.value": value, "logrite.callback": callback });
		logger.warn(`<<< Exiting setFormDirt(value=${value},callback=${callback})`, { "event.action": "setFormDirt", "logrite.value": value, "logrite.callback": callback });
        return dispatch => {
            dispatch({
                type: ActionTypes.SET_FORM_DIRT,
                payload: value
            });
            if (typeof callback === "function") {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    setControlFormDirt(value, callback) {
		logger.error(`>>> Entering setControlFormDirt(value=${value},callback=${callback})`, { "event.action": "setControlFormDirt", "logrite.value": value, "logrite.callback": callback });
		logger.warn(`<<< Exiting setControlFormDirt(value=${value},callback=${callback})`, { "event.action": "setControlFormDirt", "logrite.value": value, "logrite.callback": callback });
        return dispatch => {
            dispatch({
                type: ActionTypes.SET_CONTROL_FORM_DIRT,
                payload: value
            });
            if (typeof callback === "function") {
                setTimeout(() => {
                    callback();
                }, 0);
            }
        };
    },
    getSourceFolders(callback) {
		logger.error(`>>> Entering getSourceFolders(callback=${callback})`, { "event.action": "getSourceFolders", "logrite.callback": callback });
		logger.warn(`<<< Exiting getSourceFolders(callback=${callback})`, { "event.action": "getSourceFolders", "logrite.callback": callback });
        return dispatch => {
            ModuleDefinitionService.getSourceFolders((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SOURCE_FOLDERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSourceFiles(_root, callback) {
		logger.error(`>>> Entering getSourceFiles(_root=${_root},callback=${callback})`, { "event.action": "getSourceFiles", "logrite._root": _root, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSourceFiles(_root=${_root},callback=${callback})`, { "event.action": "getSourceFiles", "logrite._root": _root, "logrite.callback": callback });
        return dispatch => {
            ModuleDefinitionService.getSourceFiles(_root, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SOURCE_FILES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getControlIcons(controlPath, callback) {
		logger.error(`>>> Entering getControlIcons(controlPath=${controlPath},callback=${callback})`, { "event.action": "getControlIcons", "logrite.controlPath": controlPath, "logrite.callback": callback });
		logger.warn(`<<< Exiting getControlIcons(controlPath=${controlPath},callback=${callback})`, { "event.action": "getControlIcons", "logrite.controlPath": controlPath, "logrite.callback": callback });
        return dispatch => {
            ModuleDefinitionService.getControlIcons(controlPath, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CONTROL_ICONS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default moduleDefinitionActions;
