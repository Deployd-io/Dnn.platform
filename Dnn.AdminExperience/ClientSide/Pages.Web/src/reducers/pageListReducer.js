import ActionTypes from "../constants/actionTypes/pageListActionTypes";
const logger = console;


export default function  pageListReducer(state = { pageList:[] } , action) {
	logger.debug(`>>> Entering pageListReducer(state=${state},action=${action})`);
    switch (action.type) {
        case ActionTypes.SAVE:
            return {...state, pageList:JSON.parse(JSON.stringify(action.data.pageList))};

        default:
			logger.debug(`<<< Exiting pageListReducer(state=${state},action=${action})`);
            return state;
    }
}