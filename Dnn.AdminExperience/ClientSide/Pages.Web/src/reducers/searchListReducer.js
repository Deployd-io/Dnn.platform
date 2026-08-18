import ActionTypes from "../constants/actionTypes/searchListActionTypes";
const logger = console;


export default function searchListReducer(state = { searchList:[], searchResult:null } , action) {
	logger.debug(`>>> Entering searchListReducer(state=${state},action=${action})`);
    switch (action.type) {
        case ActionTypes.SAVE_SEARCH_LIST:
            return {...state, searchList: action.data.searchList};
        case ActionTypes.SAVE_SEARCH_RESULT:
            if (action.data.filtersUpdated) {
                return {
                    ...state, 
                    searchResult: action.data.searchResult,
                    searchList: action.data.searchResult.Results,
                    filtersUpdated : action.data.filtersUpdated,
                    filterPage: action.data.filterPage
                };
            } else {
                return {
                    ...state, 
                    searchResult: action.data.searchResult,
                    searchList: state.searchList.concat(action.data.searchResult.Results),
                    filtersUpdated : action.data.filtersUpdated
                };
            }

            
        default:
			logger.debug(`<<< Exiting searchListReducer(state=${state},action=${action})`);
            return state;
    }
}