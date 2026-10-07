import { search as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const searchActions = {    
    getBasicSearchSettings(callback) {
		logger.error(`>>> Entering getBasicSearchSettings(callback=${callback})`, { "event.action": "getBasicSearchSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getBasicSearchSettings(callback=${callback})`, { "event.action": "getBasicSearchSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering basicSearchSettingsClientModified(parameter=${parameter})`, { "event.action": "basicSearchSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting basicSearchSettingsClientModified(parameter=${parameter})`, { "event.action": "basicSearchSettingsClientModified", "logrite.parameter": parameter });
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
		logger.error(`>>> Entering updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateBasicSearchSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateBasicSearchSettings(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateBasicSearchSettings", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "compactSearchIndex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting compactSearchIndex(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "compactSearchIndex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "hostSearchReindex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting hostSearchReindex(callback=${callback},failureCallback=${failureCallback})`, { "event.action": "hostSearchReindex", "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "portalSearchReindex", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting portalSearchReindex(portalId=${portalId},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "portalSearchReindex", "logrite.portalId": portalId, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getSynonymsGroups", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSynonymsGroups(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getSynonymsGroups", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
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
		logger.error(`>>> Entering addSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addSynonymsGroup", "logrite.payload": payload, "logrite.groups": groups, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.addSynonymsGroup(payload, data => {
				logger.warn(`<<< Exiting addSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addSynonymsGroup", "logrite.payload": payload, "logrite.groups": groups, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering updateSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSynonymsGroup", "logrite.payload": payload, "logrite.groups": groups, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateSynonymsGroup(payload=${payload},groups=${groups},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateSynonymsGroup", "logrite.payload": payload, "logrite.groups": groups, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering deleteSynonymsGroup(groupId=${groupId},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteSynonymsGroup", "logrite.groupId": groupId, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting deleteSynonymsGroup(groupId=${groupId},object=${object},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteSynonymsGroup", "logrite.groupId": groupId, "logrite.object": object, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error('>>> Entering synonymsGroupClientModified()', { "event.action": "synonymsGroupClientModified" });
		logger.warn('<<< Exiting synonymsGroupClientModified()', { "event.action": "synonymsGroupClientModified" });
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
		logger.error('>>> Entering cancelSynonymsGroupClientModified()', { "event.action": "cancelSynonymsGroupClientModified" });
		logger.warn('<<< Exiting cancelSynonymsGroupClientModified()', { "event.action": "cancelSynonymsGroupClientModified" });
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
		logger.error(`>>> Entering getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getIgnoreWords", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getIgnoreWords(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getIgnoreWords", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
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
		logger.error(`>>> Entering addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.addIgnoreWords(payload, data => {
				logger.warn(`<<< Exiting addIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error('>>> Entering ignoreWordsClientModified()', { "event.action": "ignoreWordsClientModified" });
		logger.warn('<<< Exiting ignoreWordsClientModified()', { "event.action": "ignoreWordsClientModified" });
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
		logger.error('>>> Entering cancelIgnoreWordsClientModified()', { "event.action": "cancelIgnoreWordsClientModified" });
		logger.warn('<<< Exiting cancelIgnoreWordsClientModified()', { "event.action": "cancelIgnoreWordsClientModified" });
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
		logger.error(`>>> Entering deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.deleteIgnoreWords(payload, data => {
				logger.warn(`<<< Exiting deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
                let updated = Object.assign({}, payload);
				logger.debug(`deleteIgnoreWords(payload=${payload},callback=${callback},failureCallback=${failureCallback}): updated.StopWords → ${updated.StopWords}`, { "event.action": "deleteIgnoreWords", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
                updated.StopWordsId =  -1;
                updated.StopWords = null;
                dispatch({
                    type: ActionTypes.DELETED_SITESETTINGS_IGNORE_WORDS,
                    data: {
                        ignoreWords: updated
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
    getCultureList(portalId, callback) {
		logger.error(`>>> Entering getCultureList(portalId=${portalId},callback=${callback})`, { "event.action": "getCultureList", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getCultureList(portalId=${portalId},callback=${callback})`, { "event.action": "getCultureList", "logrite.portalId": portalId, "logrite.callback": callback });
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
