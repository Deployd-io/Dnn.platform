import {vocabularyTermList as ActionTypes}  from "../constants/actionTypes";
import VocabularyTermService from "../services/vocabularyTermService";
import utils from "../utils";
const logger = console;

const vocabularyTermActions = {
    getVocabularyTerms(vocabularyId) {
		logger.error(`>>> Entering getVocabularyTerms(vocabularyId=${vocabularyId})`, { "event.action": "getVocabularyTerms", "logrite.vocabularyId": vocabularyId });
		logger.warn(`<<< Exiting getVocabularyTerms(vocabularyId=${vocabularyId})`, { "event.action": "getVocabularyTerms", "logrite.vocabularyId": vocabularyId });
        return (dispatch) => {
            VocabularyTermService.getVocabularyTerms(vocabularyId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_VOCABULARY_TERMS,
                    data: {
                        vocabularyTerms: data.Results,
                        totalCount: data.TotalResults
                    }
                });
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    addVocabularyTerm(term, totalCount, callback) {
		logger.error(`>>> Entering addVocabularyTerm(term=${term},totalCount=${totalCount},callback=${callback})`, { "event.action": "addVocabularyTerm", "logrite.term": term, "logrite.totalCount": totalCount, "logrite.callback": callback });
		logger.debug(`addVocabularyTerm(term=${term},totalCount=${totalCount},callback=${callback}): term.TermId → ${term.TermId}`, { "event.action": "addVocabularyTerm", "logrite.term": term, "logrite.totalCount": totalCount, "logrite.callback": callback });
		logger.warn(`<<< Exiting addVocabularyTerm(term=${term},totalCount=${totalCount},callback=${callback})`, { "event.action": "addVocabularyTerm", "logrite.term": term, "logrite.totalCount": totalCount, "logrite.callback": callback });
        return (dispatch) => {
            VocabularyTermService.addVocabularyTerm(term, data => {
                term.TermId = data.TermId;
                dispatch({
                    type: ActionTypes.ADDED_VOCABULARY_TERM,
                    payload: {
                        addedTerm: term,
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
    },
    updateVocabularyTerm(payload, index, callback) {
		logger.error(`>>> Entering updateVocabularyTerm(payload=${payload},index=${index},callback=${callback})`, { "event.action": "updateVocabularyTerm", "logrite.payload": payload, "logrite.index": index, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateVocabularyTerm(payload=${payload},index=${index},callback=${callback})`, { "event.action": "updateVocabularyTerm", "logrite.payload": payload, "logrite.index": index, "logrite.callback": callback });
        return (dispatch) => {
            VocabularyTermService.updateVocabularyTerm(payload, () => {
                dispatch({
                    type: ActionTypes.UPDATED_VOCABULARY_TERM,
                    payload: {
                        updatedTerm: payload,
                        index
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
    deleteVocabularyTerm(termId, vocabularyTerms, totalCount) {
		logger.error(`>>> Entering deleteVocabularyTerm(termId=${termId},vocabularyTerms=${vocabularyTerms},totalCount=${totalCount})`, { "event.action": "deleteVocabularyTerm", "logrite.termId": termId, "logrite.vocabularyTerms": vocabularyTerms, "logrite.totalCount": totalCount });
		logger.warn(`<<< Exiting deleteVocabularyTerm(termId=${termId},vocabularyTerms=${vocabularyTerms},totalCount=${totalCount})`, { "event.action": "deleteVocabularyTerm", "logrite.termId": termId, "logrite.vocabularyTerms": vocabularyTerms, "logrite.totalCount": totalCount });
        return (dispatch) => {
            VocabularyTermService.deleteVocabularyTerm(termId, () => {
                dispatch({
                    type: ActionTypes.DELETED_VOCABULARY_TERM,
                    payload: {
                        vocabularyTerms,
                        totalCount
                    }
                });
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    setTermSelected(updatedTerm, index) {
		logger.error(`>>> Entering setTermSelected(updatedTerm=${updatedTerm},index=${index})`, { "event.action": "setTermSelected", "logrite.updatedTerm": updatedTerm, "logrite.index": index });
		logger.warn(`<<< Exiting setTermSelected(updatedTerm=${updatedTerm},index=${index})`, { "event.action": "setTermSelected", "logrite.updatedTerm": updatedTerm, "logrite.index": index });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_TERM_SELECTED,
                payload: {
                    updatedTerm,
                    index
                }
            });
        };
    },
    clearSelected(vocabularyTerms) {
		logger.error(`>>> Entering clearSelected(vocabularyTerms=${vocabularyTerms})`, { "event.action": "clearSelected", "logrite.vocabularyTerms": vocabularyTerms });
		logger.warn(`<<< Exiting clearSelected(vocabularyTerms=${vocabularyTerms})`, { "event.action": "clearSelected", "logrite.vocabularyTerms": vocabularyTerms });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEAR_TERMS_SELECTED,
                payload: {
                    vocabularyTerms
                }
            });
        };
    }
};

export default vocabularyTermActions;
