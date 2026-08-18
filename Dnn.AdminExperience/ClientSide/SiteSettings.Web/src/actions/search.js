import { search as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const searchActions = {    
    getBasicSearchSettings(callback) {
		logger.debug(`>>> Entering getBasicSearchSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getBasicSearchSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getBasicSearchSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_BASIC_SEARCH_SETTINGS,
                    data: {
                        basicSearchSettings: data.Settings,
                        searchCustomAnalyzers: data.SearchCustomAnalyzers
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    basicSearchSettingsClientModified(parameter) {
		logger.debug(`>>> Entering basicSearchSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting basicSearchSettingsClientModified(parameter=${parameter})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_BASIC_SEARCH_SETTINS_CLIENT_MODIFIED,
                data: {
                    basicSearchSettings: parameter,
                    basicSearchSettingsClientModified: true
                }
            });
        };
    },
    updateBasicSearchSettings(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.updateBasicSearchSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_BASIC_SEARCH_SETTINGS,
                    data: {
                        basicSearchSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    compactSearchIndex(callback, failureCallback) {
		logger.debug(`>>> Entering compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`);
        return () => {
            ApplicationService.compactSearchIndex(data => {
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    hostSearchReindex(callback, failureCallback) {
		logger.debug(`>>> Entering hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`);
        return () => {
            ApplicationService.hostSearchReindex(data => {
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    portalSearchReindex(portalId, callback, failureCallback) {
		logger.debug(`>>> Entering portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`);
        return () => {
            ApplicationService.portalSearchReindex(portalId, data => {
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    getSynonymsGroups(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getSynonymsGroups(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_SYNONYMS_GROUPS,
                    data: {
                        synonymsGroups: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    addSynonymsGroup(payload, groups, callback, failureCallback) {
		logger.debug(`>>> Entering addSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting addSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.addSynonymsGroup(payload, data => {
                let updatedGroups = groups.SynonymsGroups.map((item) => {
                    return item;
                });                
                updatedGroups.unshift({SynonymsGroupId: data.Id, SynonymsTags: payload.SynonymsTags});
                const synonymsGroups = Object.assign({}, groups);
                synonymsGroups.SynonymsGroups = updatedGroups;
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: synonymsGroups,
                        synonymsGroupClientModified: false
                    }
                });
                if (callback) {
					logger.debug(`addSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback}): synonymsGroups.SynonymsGroups → ${synonymsGroups.SynonymsGroups}`);
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    updateSynonymsGroup(payload, groups, callback, failureCallback) {
		logger.debug(`>>> Entering updateSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.updateSynonymsGroup(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: groups,
                        synonymsGroupClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    deleteSynonymsGroup(groupId, object, callback, failureCallback) {
		logger.debug(`>>> Entering deleteSynonymsGroup(groupId=${groupId},object=${object},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteSynonymsGroup(groupId=${groupId},object=${object},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.deleteSynonymsGroup(groupId, data => {
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_SYNONYMS_GROUP,
                    data: {
                        synonymsGroups: object
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    synonymsGroupClientModified() {
		logger.debug('>>> Entering synonymsGroupClientModified()');
		logger.debug('<<< Exiting synonymsGroupClientModified()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED,
                data: {
                    synonymsGroupClientModified: true
                }
            });
        };
    },
    cancelSynonymsGroupClientModified() {
		logger.debug('>>> Entering cancelSynonymsGroupClientModified()');
		logger.debug('<<< Exiting cancelSynonymsGroupClientModified()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_SYNONYMS_GROUP_CLIENT_MODIFIED,
                data: {
                    synonymsGroupClientModified: false
                }
            });
        };
    },
    getIgnoreWords(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getIgnoreWords(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    addIgnoreWords(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.addIgnoreWords(payload, data => {
                let updated = Object.assign({}, payload);
                updated.StopWordsId =  data.Id;
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: updated,
                        ignoreWordsClientModified: false
                    }
                });
                if (callback) {
					logger.debug(`addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): updated.StopWordsId → ${updated.StopWordsId}`);
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    updateIgnoreWords(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.updateIgnoreWords(payload, data => {                
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: payload,
                        ignoreWordsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    ignoreWordsClientModified() {
		logger.debug('>>> Entering ignoreWordsClientModified()');
		logger.debug('<<< Exiting ignoreWordsClientModified()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED,
                data: {
                    ignoreWordsClientModified: true
                }
            });
        };
    },
    cancelIgnoreWordsClientModified() {
		logger.debug('>>> Entering cancelIgnoreWordsClientModified()');
		logger.debug('<<< Exiting cancelIgnoreWordsClientModified()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_IGNORE_WORDS_CLIENT_MODIFIED,
                data: {
                    ignoreWordsClientModified: false
                }
            });
        };
    },
    deleteIgnoreWords(payload, callback, failureCallback) {
		logger.debug(`>>> Entering deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.deleteIgnoreWords(payload, data => {
                let updated = Object.assign({}, payload);
                updated.StopWordsId =  -1;
				logger.debug(`deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): updated.StopWordsId → ${updated.StopWordsId}`);
                updated.StopWords = null;
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: updated
                    }
                });
                if (callback) {
					logger.debug(`deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): updated.StopWords → ${updated.StopWords}`);
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    getCultureList(portalId, callback) {
		logger.debug(`>>> Entering getCultureList(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getCultureList(portalId=${portalId},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getCultureList(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_SEARCH_CULTURE_LIST,
                    data: {
                        cultures: data.Cultures
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }

};

export default searchActions;
