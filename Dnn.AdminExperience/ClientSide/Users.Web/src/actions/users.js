import {users as ActionTypes}  from "dnn-users-common-action-types";
import UserService from "services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const userActions = {
    getUsers(searchParameters, callback) {
		logger.debug(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getUsers(searchParameters=${searchParameters},callback=${callback})`);
        return (dispatch) => {
            UserService.getUsers(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_USERS,
                    payload: data
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getUserFilters(callback) {
		logger.debug(`>>> Entering getUserFilters(callback=${callback})`);
		logger.debug(`<<< Exiting getUserFilters(callback=${callback})`);
        return () => {
            UserService.getUserFilters(data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default userActions;
