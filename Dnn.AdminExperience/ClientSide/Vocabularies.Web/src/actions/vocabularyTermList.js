import {vocabularyTermList as ActionTypes}  from "../constants/actionTypes";
import VocabularyTermService from "../services/vocabularyTermService";
import utils from "../utils";
const logger = console;

const vocabularyTermActions = {
    getVocabularyTerms(vocabularyId) {
		logger.debug(`>>> Entering getVocabularyTerms(vocabularyId=${vocabularyId})`);
		logger.debug(`<<< Exiting getVocabularyTerms(vocabularyId=${vocabularyId})`);
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
		logger.debug(`>>> Entering addVocabularyTerm(term=${term},totalCount=${totalCount},callback=${callback})`);
		logger.debug(`<<< Exiting addVocabularyTerm(term=${term},totalCount=${totalCount},callback=${callback})`);
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
					logger.debug(`addVocabularyTerm(term=${term},totalCount=${totalCount},callback=${callback}): term.TermId → ${term.TermId}`);
                    callback();
                }
            }, (failureMessage) => {
                const errorMessage = JSON.parse(failureMessage.responseText);
                utils.utilities.notifyError(errorMessage.Message);
            });
        };
    },
    updateVocabularyTerm(payload, index, callback) {
		logger.debug(`>>> Entering updateVocabularyTerm(payload=${payload},index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting updateVocabularyTerm(payload=${payload},index=${index},callback=${callback})`);
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
		logger.debug(`>>> Entering deleteVocabularyTerm(termId=${termId},vocabularyTerms=${vocabularyTerms},totalCount=${totalCount})`);
		logger.debug(`<<< Exiting deleteVocabularyTerm(termId=${termId},vocabularyTerms=${vocabularyTerms},totalCount=${totalCount})`);
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
		logger.debug(`>>> Entering setTermSelected(updatedTerm=${updatedTerm},index=${index})`);
		logger.debug(`<<< Exiting setTermSelected(updatedTerm=${updatedTerm},index=${index})`);
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
		logger.debug(`>>> Entering clearSelected(vocabularyTerms=${vocabularyTerms})`);
		logger.debug(`<<< Exiting clearSelected(vocabularyTerms=${vocabularyTerms})`);
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
