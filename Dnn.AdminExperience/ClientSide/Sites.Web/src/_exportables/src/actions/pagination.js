import {pagination as ActionTypes}  from "../actionTypes";
import utilities from "utils";
const logger = console;
const paginationActions = {
    loadMore(callback) {
		logger.error(`>>> Entering loadMore(callback=${callback})`, { "event.action": "loadMore", "logrite.callback": callback });
		logger.warn(`<<< Exiting loadMore(callback=${callback})`, { "event.action": "loadMore", "logrite.callback": callback });
        return dispatch => {
            dispatch({
                type: ActionTypes.LOAD_MORE
            });
            if (callback) {
                utilities.throttleExecution(callback);
            }
        };
    }
};

export default paginationActions;
