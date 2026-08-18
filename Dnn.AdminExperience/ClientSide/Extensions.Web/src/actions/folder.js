import { folder as ActionTypes} from "constants/actionTypes";
import { FolderService } from "services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    utilities.utilities.notifyError(message);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const folderActions = {
    getOwnerFolders(callback) {
		logger.debug(`>>> Entering getOwnerFolders(callback=${callback})`);
		logger.debug(`<<< Exiting getOwnerFolders(callback=${callback})`);
        return (dispatch) => {
            FolderService.getOwnerFolders((data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_OWNER_FOLDERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getModuleFolders(type, callback) {
		logger.debug(`>>> Entering getModuleFolders(type=${type},callback=${callback})`);
		logger.debug(`<<< Exiting getModuleFolders(type=${type},callback=${callback})`);
        return (dispatch) => {
            FolderService.getModuleFolders(type, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_MODULE_FOLDERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getModuleFiles(parameters, callback) {
		logger.debug(`>>> Entering getModuleFiles(parameters=${parameters},callback=${callback})`);
		logger.debug(`<<< Exiting getModuleFiles(parameters=${parameters},callback=${callback})`);
        return (dispatch) => {
            FolderService.getModuleFiles(parameters, (data) => {
                dispatch({
                    type: ActionTypes.RETRIEVED_MODULE_FILES,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    createFolder(parameters, type, callback) {
		logger.debug(`>>> Entering createFolder(parameters=${parameters},type=${type},callback=${callback})`);
		logger.debug(`<<< Exiting createFolder(parameters=${parameters},type=${type},callback=${callback})`);
        return (dispatch) => {
            FolderService.createNewFolder(parameters, () => {
                dispatch({
                    type: (type === "moduleFolder" ? ActionTypes.CREATED_NEW_MODULE_FOLDER : ActionTypes.CREATED_NEW_OWNER_FOLDER),
                    payload: {
                        value: parameters[type]
                    }
                });
                if (callback) {
                    callback(parameters);
                }
            }, errorCallback);
        };
    }
};

export default folderActions;
