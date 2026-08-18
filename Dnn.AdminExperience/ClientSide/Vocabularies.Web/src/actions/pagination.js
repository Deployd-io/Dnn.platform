import {pagination as ActionTypes}  from "../constants/actionTypes";
import VocabularyService from "../services/vocabularyService";
const logger = console;
const paginationActions = {
    loadMore(searchParameters) {
		logger.debug(`>>> Entering loadMore(searchParameters=${searchParameters})`);
		logger.debug(`<<< Exiting loadMore(searchParameters=${searchParameters})`);
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
		logger.debug(`>>> Entering loadTab(searchParameters=${searchParameters})`);
		logger.debug(`<<< Exiting loadTab(searchParameters=${searchParameters})`);
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
