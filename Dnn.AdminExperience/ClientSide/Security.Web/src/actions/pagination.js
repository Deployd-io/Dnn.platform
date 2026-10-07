const logger = console;
import {pagination as ActionTypes}  from "../constants/actionTypes";
const paginationActions = {
    loadTab(index) {
		logger.error(`>>> Entering loadTab(index=${index})`, { "event.action": "loadTab", "logrite.index": index });
		logger.warn(`<<< Exiting loadTab(index=${index})`, { "event.action": "loadTab", "logrite.index": index });
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
