import { folder as ActionTypes} from "constants/actionTypes";
import { FolderService } from "services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    utilities.utilities.notifyError(message);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const folderActions = {
    getOwnerFolders(callback) {
		logger.error(`>>> Entering getOwnerFolders(callback=${callback})`, { "event.action": "getOwnerFolders", "logrite.callback": callback });
		logger.warn(`<<< Exiting getOwnerFolders(callback=${callback})`, { "event.action": "getOwnerFolders", "logrite.callback": callback });
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
		logger.error(`>>> Entering getModuleFolders(type=${type},callback=${callback})`, { "event.action": "getModuleFolders", "logrite.type": type, "logrite.callback": callback });
		logger.warn(`<<< Exiting getModuleFolders(type=${type},callback=${callback})`, { "event.action": "getModuleFolders", "logrite.type": type, "logrite.callback": callback });
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
		logger.error(`>>> Entering getModuleFiles(parameters=${parameters},callback=${callback})`, { "event.action": "getModuleFiles", "logrite.parameters": parameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getModuleFiles(parameters=${parameters},callback=${callback})`, { "event.action": "getModuleFiles", "logrite.parameters": parameters, "logrite.callback": callback });
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
		logger.error(`>>> Entering createFolder(parameters=${parameters},type=${type},callback=${callback})`, { "event.action": "createFolder", "logrite.parameters": parameters, "logrite.type": type, "logrite.callback": callback });
		logger.warn(`<<< Exiting createFolder(parameters=${parameters},type=${type},callback=${callback})`, { "event.action": "createFolder", "logrite.parameters": parameters, "logrite.type": type, "logrite.callback": callback });
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
