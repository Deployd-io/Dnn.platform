import {users as ActionTypes}  from "dnn-users-common-action-types";
import UserService from "services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const userActions = {
    getUsers(searchParameters, callback) {
		logger.error(`>>> Entering getUsers(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getUsers(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getUsers", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
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
		logger.error(`>>> Entering getUserFilters(callback=${callback})`, { "event.action": "getUserFilters", "logrite.callback": callback });
		logger.warn(`<<< Exiting getUserFilters(callback=${callback})`, { "event.action": "getUserFilters", "logrite.callback": callback });
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
