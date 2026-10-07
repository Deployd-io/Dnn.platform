const logger = console;
import {pagination as ActionTypes}  from "../constants/actionTypes";
const paginationActions = {
    loadTab(index, callback) {
		logger.error(`>>> Entering loadTab(index=${index},callback=${callback})`, { "event.action": "loadTab", "logrite.index": index, "logrite.callback": callback });
		logger.warn(`<<< Exiting loadTab(index=${index},callback=${callback})`, { "event.action": "loadTab", "logrite.index": index, "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_TAB_DATA,
                payload: {
                    index
                }
            });
            if (callback) {
                setTimeout(()=>{
                    callback();
                }, 0);
            }
        };
    }
};

export default paginationActions;
