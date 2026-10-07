import ActionTypes from "../constants/actionTypes/pageListActionTypes";
const logger = console;


export default function  pageListReducer(state = { pageList:[] } , action) {
	logger.error(`>>> Entering pageListReducer(state=${state},action=${action})`, { "event.action": "pageListReducer", "logrite.state": state, "logrite.action": action });
    switch (action.type) {
        case ActionTypes.SAVE:
            return {...state, pageList:JSON.parse(JSON.stringify(action.data.pageList))};

        default:
			logger.warn(`<<< Exiting pageListReducer(state=${state},action=${action})`, { "event.action": "pageListReducer", "logrite.state": state, "logrite.action": action });
            return state;
    }
}