const logger = console;
import {pagination as ActionTypes}  from "../constants/actionTypes";
const paginationActions = {
    loadTab(index) {
		logger.debug(`>>> Entering loadTab(index=${index})`);
		logger.debug(`<<< Exiting loadTab(index=${index})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_TAB_DATA,
                payload: {
                    index
                }
            });
        };
    }
};

export default paginationActions;
