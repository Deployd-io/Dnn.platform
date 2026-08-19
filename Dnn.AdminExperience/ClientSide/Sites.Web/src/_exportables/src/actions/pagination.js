import {pagination as ActionTypes}  from "../actionTypes";
import utilities from "utils";
const logger = console;
const paginationActions = {
    loadMore(callback) {
		logger.debug(`>>> Entering loadMore(callback=${callback})`);
		logger.debug(`<<< Exiting loadMore(callback=${callback})`);
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
