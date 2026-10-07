import { vocabulary as ActionTypes } from "../constants/actionTypes";
import VocabularyService from "../services/vocabularyService";
import utils from "../utils";
const logger = console;

const vocabularyActions = {
    getVocabularyList(searchParameters, loadMore, callback) {
		logger.error(`>>> Entering getVocabularyList(searchParameters=${searchParameters},loadMore=${loadMore},callback=${callback})`, { "event.action": "getVocabularyList", "logrite.searchParameters": searchParameters, "logrite.loadMore": loadMore, "logrite.callback": callback });
		logger.warn(`<<< Exiting getVocabularyList(searchParameters=${searchParameters},loadMore=${loadMore},callback=${callback})`, { "event.action": "getVocabularyList", "logrite.searchParameters": searchParameters, "logrite.loadMore": loadMore, "logrite.callback": callback });
        return (dispatch) => {
            VocabularyService.getVocabularyList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_VOCABULARY_LIST,
                    loadMore: loadMore,
                    data: {
                        vocabularyList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    addVocabulary(vocabulary, totalCount, callback) {
		logger.error(`>>> Entering addVocabulary(vocabulary=${vocabulary},totalCount=${totalCount},callback=${callback})`, { "event.action": "addVocabulary", "logrite.vocabulary": vocabulary, "logrite.totalCount": totalCount, "logrite.callback": callback });
		logger.debug(`addVocabulary(vocabulary=${vocabulary},totalCount=${totalCount},callback=${callback}): vocabulary.VocabularyId → ${vocabulary.VocabularyId}`, { "event.action": "addVocabulary", "logrite.vocabulary": vocabulary, "logrite.totalCount": totalCount, "logrite.callback": callback });
		logger.warn(`<<< Exiting addVocabulary(vocabulary=${vocabulary},totalCount=${totalCount},callback=${callback})`, { "event.action": "addVocabulary", "logrite.vocabulary": vocabulary, "logrite.totalCount": totalCount, "logrite.callback": callback });
        return (dispatch) => {
            VocabularyService.addVocabulary(vocabulary, data => {
                vocabulary.VocabularyId = data.VocabularyId;
                vocabulary.Type = (vocabulary.ScopeTypeId === 2 ? "Hierarchy" : "Simple");
                dispatch({
                    type: ActionTypes.ADDED_VOCABULARY,
                    payload: {
                        addedVocabulary: vocabulary,
                        totalCount: totalCount
                    }
                });
                if (callback) {
                    callback();
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    updateVocabulary(payload, index) {
		logger.error(`>>> Entering updateVocabulary(payload=${payload},index=${index})`, { "event.action": "updateVocabulary", "logrite.payload": payload, "logrite.index": index });
		logger.warn(`<<< Exiting updateVocabulary(payload=${payload},index=${index})`, { "event.action": "updateVocabulary", "logrite.payload": payload, "logrite.index": index });
        return (dispatch) => {
            VocabularyService.updateVocabulary(payload, () => {
                dispatch({
                    type: ActionTypes.UPDATED_VOCABULARY,
                    data: {
                        updatedTerm: payload,
                        index: index
                    }
                });
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    deleteVocabulary(vocabularyId, index, totalCount, callback) {
		logger.error(`>>> Entering deleteVocabulary(vocabularyId=${vocabularyId},index=${index},totalCount=${totalCount},callback=${callback})`, { "event.action": "deleteVocabulary", "logrite.vocabularyId": vocabularyId, "logrite.index": index, "logrite.totalCount": totalCount, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteVocabulary(vocabularyId=${vocabularyId},index=${index},totalCount=${totalCount},callback=${callback})`, { "event.action": "deleteVocabulary", "logrite.vocabularyId": vocabularyId, "logrite.index": index, "logrite.totalCount": totalCount, "logrite.callback": callback });
        return (dispatch) => {
            VocabularyService.deleteVocabulary(vocabularyId, () => {
                dispatch({
                    type: ActionTypes.DELETED_VOCABULARY,
                    payload: {
                        index,
                        totalCount
                    }
                });
                if (callback) {
                    callback();
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    }
};

export default vocabularyActions;
