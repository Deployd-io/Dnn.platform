const logger = console;
import { vocabulary as ActionTypes, pagination as PaginationActionTypes } from "../constants/actionTypes";

function updateVocabularyList(vocabularyList, index, value) {
	logger.error(`>>> Entering updateVocabularyList(vocabularyList=${vocabularyList},index=${index},value=${value})`, { "event.action": "updateVocabularyList", "logrite.vocabularyList": vocabularyList, "logrite.index": index, "logrite.value": value });
	logger.warn(`<<< Exiting updateVocabularyList(vocabularyList=${vocabularyList},index=${index},value=${value})`, { "event.action": "updateVocabularyList", "logrite.vocabularyList": vocabularyList, "logrite.index": index, "logrite.value": value });
    return [...vocabularyList.slice(0, index), value, ...vocabularyList.slice(index + 1)];
}
function insertRecords(vocabularyList, addedList) {
	logger.error(`>>> Entering insertRecords(vocabularyList=${vocabularyList},addedList=${addedList})`, { "event.action": "insertRecords", "logrite.vocabularyList": vocabularyList, "logrite.addedList": addedList });
    let newVocabularyList = vocabularyList;
    addedList.forEach((vocabulary) => {
        let alreadyThere = vocabularyList.find((_vocabulary) => {
            return _vocabulary.VocabularyId === vocabulary.VocabularyId;
        });
        if (!alreadyThere) {
            newVocabularyList.push(vocabulary);
        }
    });
	logger.warn(`<<< Exiting insertRecords(vocabularyList=${vocabularyList},addedList=${addedList})`, { "event.action": "insertRecords", "logrite.vocabularyList": vocabularyList, "logrite.addedList": addedList });
    return newVocabularyList;
}

export default function vocabularyList(state = {
    vocabularyList: [],
    vocabularyTerms: [],
    totalTermCount: 0,
    totalCount: 0,
    vocabularyAddedIsValid: true
}, action) {
    switch (action.type) {
        case PaginationActionTypes.LOAD_MORE:
            return { ...state,
                vocabularyList: insertRecords(JSON.parse(JSON.stringify(state.vocabularyList)), action.data.vocabularyList),
                totalCount: action.data.totalCount
            };
        case PaginationActionTypes.LOAD_TAB_DATA:
        case ActionTypes.RETRIEVED_VOCABULARY_LIST:
            return { ...state,
                vocabularyList: action.data.vocabularyList,
                totalCount: action.data.totalCount
            };
        case ActionTypes.UPDATED_VOCABULARY:
            return { ...state,
                vocabularyList: updateVocabularyList(state.vocabularyList, action.data.index, action.data.updatedTerm)
            };
        case ActionTypes.UPDATED_VOCABULARY_TERM:
            return {...state,
                vocabularyTerms: [...state.vocabularyTerms.slice(0, action.payload.index), action.payload.updatedTerm, ...state.vocabularyTerms.slice(action.payload.index + 1)]
            };
        case ActionTypes.ADDED_VOCABULARY_TERM:
            return { ...state,
                vocabularyTerms: [...state.vocabularyTerms, action.payload.addedTerm],
                totalTermCount: action.payload.totalCount
            };
        case ActionTypes.ADDED_VOCABULARY:
            return {
                vocabularyAddedIsValid: true,
                vocabularyList: state.vocabularyList.concat(action.payload.addedVocabulary),
                totalCount: action.payload.totalCount
            };
        case ActionTypes.RETRIEVED_VOCABULARY_TERMS:
            return { ...state,
                vocabularyTerms: action.data.vocabularyTerms,
                totalTermCount: action.data.totalCount
            };
        case ActionTypes.DELETED_VOCABULARY:
            return { ...state,
                vocabularyList: [
                    ...state.vocabularyList.slice(0, action.payload.index),
                    ...state.vocabularyList.slice(action.payload.index + 1)
                ],
                totalCount: action.payload.totalCount
            };
        case ActionTypes.FAILED_TO_ADD_VOCABULARY:
            return { ...state,
                vocabularyAddedIsValid: false
            };
        default:
            return { ...state
            };
    }
}
