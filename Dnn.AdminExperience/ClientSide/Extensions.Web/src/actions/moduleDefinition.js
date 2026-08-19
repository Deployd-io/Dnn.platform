const logger = console;
import { moduleDefinition as ActionTypes } from "constants/actionTypes";
import { ModuleDefinitionService } from "services";
const moduleDefinitionActions = {
    setFormDirt(value, callback) {
		logger.debug(`>>> Entering setFormDirt(value=${value},callback=${callback})`);
		logger.debug(`<<< Exiting setFormDirt(value=${value},callback=${callback})`);
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
		logger.debug(`>>> Entering setControlFormDirt(value=${value},callback=${callback})`);
		logger.debug(`<<< Exiting setControlFormDirt(value=${value},callback=${callback})`);
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
		logger.debug(`>>> Entering getSourceFolders(callback=${callback})`);
		logger.debug(`<<< Exiting getSourceFolders(callback=${callback})`);
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
		logger.debug(`>>> Entering getSourceFiles(_root=${_root},callback=${callback})`);
		logger.debug(`<<< Exiting getSourceFiles(_root=${_root},callback=${callback})`);
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
		logger.debug(`>>> Entering getControlIcons(controlPath=${controlPath},callback=${callback})`);
		logger.debug(`<<< Exiting getControlIcons(controlPath=${controlPath},callback=${callback})`);
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
