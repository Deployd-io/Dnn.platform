const logger = console;
import {pagination as ActionTypes}  from "../constants/actionTypes";
const paginationActions = {
    loadTab(index, callback) {
		logger.debug(`>>> Entering loadTab(index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting loadTab(index=${index},callback=${callback})`);
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
