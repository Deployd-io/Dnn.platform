import {pagination as ActionTypes}  from "../constants/actionTypes";
import VocabularyService from "../services/vocabularyService";
const logger = console;
const paginationActions = {
    loadMore(searchParameters) {
		logger.error(`>>> Entering loadMore(searchParameters=${searchParameters})`, { "event.action": "loadMore", "logrite.searchParameters": searchParameters });
		logger.warn(`<<< Exiting loadMore(searchParameters=${searchParameters})`, { "event.action": "loadMore", "logrite.searchParameters": searchParameters });
        return (dispatch) => {
            VocabularyService.getVocabularyList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.LOAD_MORE,
                    loadMore: true,
                    payload: searchParameters,
                    data: {
                        vocabularyList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            });
        };
    },
    loadTab(searchParameters) {
		logger.error(`>>> Entering loadTab(searchParameters=${searchParameters})`, { "event.action": "loadTab", "logrite.searchParameters": searchParameters });
		logger.warn(`<<< Exiting loadTab(searchParameters=${searchParameters})`, { "event.action": "loadTab", "logrite.searchParameters": searchParameters });
        return (dispatch) => {
            VocabularyService.getVocabularyList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.LOAD_TAB_DATA,
                    loadMore: true,
                    payload: searchParameters,
                    tabIndex: searchParameters.scopeTypeId,
                    data: {
                        vocabularyList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            });
        };
    }
};

export default paginationActions;
