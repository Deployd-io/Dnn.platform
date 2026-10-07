import { languages as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import LanguageEditorService from "services/languageEditorService";
const logger = console;

const languagesActions = {
    getLanguageSettings(portalId, cultureCode, callback) {
		logger.error(`>>> Entering getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getLanguageSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getLanguageSettings", "logrite.portalId": portalId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getLanguageSettings(portalId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS,
                    data: {
                        languageSettings: data.Settings,
                        languages: data.Languages,
                        languageDisplayModes: data.LanguageDisplayModes,
                        languageSettingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    languageSettingsClientModified(parameter) {
		logger.error(`>>> Entering languageSettingsClientModified(parameter=${parameter})`, { "event.action": "languageSettingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting languageSettingsClientModified(parameter=${parameter})`, { "event.action": "languageSettingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_LANGUAGE_SETTINGS_CLIENT_MODIFIED,
                data: {
                    languageSettings: parameter,
                    languageSettingsClientModified: true
                }
            });
        };
    },
    updateLanguageSettings(payload, languageList, callback, failureCallback) {
		logger.error(`>>> Entering updateLanguageSettings(payload=${payload},languageList=${languageList},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguageSettings", "logrite.payload": payload, "logrite.languageList": languageList, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateLanguageSettings(payload=${payload},languageList=${languageList},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguageSettings", "logrite.payload": payload, "logrite.languageList": languageList, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateLanguageSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_SETTINGS,
                    data: {
                        languageList: languageList,
                        siteDefaultLanguage: payload.SiteDefaultLanguage,
                        languageSettingsClientModified: false
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
    getLanguages(portalId, callback) {
		logger.error(`>>> Entering getLanguages(portalId=${portalId},callback=${callback})`, { "event.action": "getLanguages", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getLanguages(portalId=${portalId},callback=${callback})`, { "event.action": "getLanguages", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getLanguages(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGES,
                    data: {
                        languageList: data.Languages
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLanguage(portalId, languageId, callback) {
		logger.error(`>>> Entering getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`, { "event.action": "getLanguage", "logrite.portalId": portalId, "logrite.languageId": languageId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`, { "event.action": "getLanguage", "logrite.portalId": portalId, "logrite.languageId": languageId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getLanguage(portalId, languageId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE,
                    data: {
                        language: data.Language,
                        fallbacks: data.SupportedFallbacks,
                        languageClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getAllLanguages(callback) {
		logger.error(`>>> Entering getAllLanguages(callback=${callback})`, { "event.action": "getAllLanguages", "logrite.callback": callback });
		logger.warn(`<<< Exiting getAllLanguages(callback=${callback})`, { "event.action": "getAllLanguages", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getAllLanguages(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_ALL_LANGUAGES,
                    data: {
                        fullLanguageList: data.FullLanguageList
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    languageClientModified(parameter) {
		logger.error(`>>> Entering languageClientModified(parameter=${parameter})`, { "event.action": "languageClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting languageClientModified(parameter=${parameter})`, { "event.action": "languageClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SITESETTINGS_LANGUAGE_CLIENT_MODIFIED,
                data: {
                    language: parameter,
                    languageClientModified: true
                }
            });
        };
    },
    cancelLanguageClientModified() {
		logger.error('>>> Entering cancelLanguageClientModified()', { "event.action": "cancelLanguageClientModified" });
		logger.warn('<<< Exiting cancelLanguageClientModified()', { "event.action": "cancelLanguageClientModified" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SITESETTINGS_LANGUAGE_CLIENT_MODIFIED,
                data: {
                    ignoreWordsClientModified: false
                }
            });
        };
    },
    addLanguage(payload, callback, failureCallback) {
		logger.error(`>>> Entering addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.addLanguage(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_LANGUAGE,
                    data: {
                        languageClientModified: false
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
    updateLanguage(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguage", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateLanguage(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE,
                    data: {
                        languageClientModified: false
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
    updateLanguageRoles(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguageRoles", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLanguageRoles", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateLanguageRoles(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_ROLES,
                    data: {
                        languageClientModified: false
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
    verifyLanguageResourceFiles(callback) {
		logger.error(`>>> Entering verifyLanguageResourceFiles(callback=${callback})`, { "event.action": "verifyLanguageResourceFiles", "logrite.callback": callback });
		logger.warn(`<<< Exiting verifyLanguageResourceFiles(callback=${callback})`, { "event.action": "verifyLanguageResourceFiles", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.verifyLanguageResourceFiles(data => {
                dispatch({
                    type: ActionTypes.VERIFIED_SITESETTINGS_LANGUAGE_RESOURCE_FILES,
                    data: {
                        verificationResults: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getModuleList(type, callback) {
		logger.error(`>>> Entering getModuleList(type=${type},callback=${callback})`, { "event.action": "getModuleList", "logrite.type": type, "logrite.callback": callback });
		logger.warn(`<<< Exiting getModuleList(type=${type},callback=${callback})`, { "event.action": "getModuleList", "logrite.type": type, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getModuleList(type, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_PACK_MODULE_LIST,
                    data: {
                        modules: data.Modules
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createLanguagePack(payload, callback, failureCallback) {
		logger.error(`>>> Entering createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createLanguagePack", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createLanguagePack", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.createLanguagePack(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SITESETTINGS_LANGUAGE_PACK,
                    data: {

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
    disableLocalizedContent(portalId, callback) {
		logger.error(`>>> Entering disableLocalizedContent(portalId=${portalId},callback=${callback})`, { "event.action": "disableLocalizedContent", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting disableLocalizedContent(portalId=${portalId},callback=${callback})`, { "event.action": "disableLocalizedContent", "logrite.portalId": portalId, "logrite.callback": callback });
        return () => {
            LanguageEditorService.disableLocalizedContent(portalId, callback);
        };
    },
    deleteLanguagePages(payload, callback) {
		logger.error(`>>> Entering deleteLanguagePages(payload=${payload},callback=${callback})`, { "event.action": "deleteLanguagePages", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteLanguagePages(payload=${payload},callback=${callback})`, { "event.action": "deleteLanguagePages", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            LanguageEditorService.deleteLanguagePages(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_LANGUAGE_PAGES,
                    data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    publishAllPages(payload, callback, failureCallback) {
		logger.error(`>>> Entering publishAllPages(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "publishAllPages", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting publishAllPages(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "publishAllPages", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            LanguageEditorService.publishAllPages(payload, data => {
                dispatch({
                    type: ActionTypes.PUBLISHED_LOCALIZED_PAGES,
                    data
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
    enableLocalizedContent(payload, callback, failureCallback) {
		logger.error(`>>> Entering enableLocalizedContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "enableLocalizedContent", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting enableLocalizedContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "enableLocalizedContent", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            LanguageEditorService.enableLocalizedContent(payload, data => {
                dispatch({
                    type: ActionTypes.ENABLED_LOCALIZED_CONTENT,
                    data: {

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
    localizeContent(payload, callback, failureCallback) {
		logger.error(`>>> Entering localizeContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "localizeContent", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting localizeContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "localizeContent", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            LanguageEditorService.localizeContent(payload, data => {
                dispatch({
                    type: ActionTypes.ENABLED_LOCALIZED_CONTENT,
                    data
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
    getLocalizationProgress(callback) {
		logger.error(`>>> Entering getLocalizationProgress(callback=${callback})`, { "event.action": "getLocalizationProgress", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLocalizationProgress(callback=${callback})`, { "event.action": "getLocalizationProgress", "logrite.callback": callback });
        return () => {
            LanguageEditorService.getLocalizationProgress(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRoleGroups(portalId, callback) {
		logger.error(`>>> Entering getRoleGroups(portalId=${portalId},callback=${callback})`, { "event.action": "getRoleGroups", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getRoleGroups(portalId=${portalId},callback=${callback})`, { "event.action": "getRoleGroups", "logrite.portalId": portalId, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getRoleGroups(portalId, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_ROLE_GROUPS,
                    data: {
                        roleGroups: data.Groups
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRoles(portalId, groupId, cultureCode, callback) {
		logger.error(`>>> Entering getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getRoles", "logrite.portalId": portalId, "logrite.groupId": groupId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
		logger.warn(`<<< Exiting getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`, { "event.action": "getRoles", "logrite.portalId": portalId, "logrite.groupId": groupId, "logrite.cultureCode": cultureCode, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getRoles(portalId, groupId, cultureCode, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_ROLES,
                    data: {
                        rolesList: data.Roles
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getPageList(payload, callback) {
		logger.error(`>>> Entering getPageList(payload=${payload},callback=${callback})`, { "event.action": "getPageList", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPageList(payload=${payload},callback=${callback})`, { "event.action": "getPageList", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            LanguageEditorService.getPageList(payload, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_PAGES,
                    data: {
                        pageList: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    SelectLanguageRoles(roles, role, selected) {
		logger.error(`>>> Entering SelectLanguageRoles(roles=${roles},role=${role},selected=${selected})`, { "event.action": "SelectLanguageRoles", "logrite.roles": roles, "logrite.role": role, "logrite.selected": selected });
		logger.warn(`<<< Exiting SelectLanguageRoles(roles=${roles},role=${role},selected=${selected})`, { "event.action": "SelectLanguageRoles", "logrite.roles": roles, "logrite.role": role, "logrite.selected": selected });
        return (dispatch) => {
            let list = roles.map((item) => {
				logger.error(`SelectLanguageRoles(roles=${roles},role=${role},selected=${selected}): list → ${list}`, { "event.action": "SelectLanguageRoles", "logrite.roles": roles, "logrite.role": role, "logrite.selected": selected });
                if (item.RoleName === role) {
                    return { RoleID: item.RoleID, RoleName: item.RoleName, Selected: selected };
                }
                else {
                    return item;
                }
            });

            dispatch({
                type: ActionTypes.UPDATED_SITESETTINGS_LANGUAGE_ROLE_SELECTION,
                data: {
                    rolesList: list
                }
            });

        };
    },
    activateLanguage(payload, callback) {
		logger.error(`>>> Entering activateLanguage(payload=${payload},callback=${callback})`, { "event.action": "activateLanguage", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting activateLanguage(payload=${payload},callback=${callback})`, { "event.action": "activateLanguage", "logrite.payload": payload, "logrite.callback": callback });
        return () => {
            ApplicationService.activateLanguage(payload, () => {
                if (callback) {
                    callback();
                }
            });
        };
    },
    markAllPagesAsTranslated(payload, callback) {
		logger.error(`>>> Entering markAllPagesAsTranslated(payload=${payload},callback=${callback})`, { "event.action": "markAllPagesAsTranslated", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting markAllPagesAsTranslated(payload=${payload},callback=${callback})`, { "event.action": "markAllPagesAsTranslated", "logrite.payload": payload, "logrite.callback": callback });
        return () => {
            ApplicationService.markAllPagesAsTranslated(payload, () => {
                if (callback) {
                    callback();
                }
            });
        };
    }
};

export default languagesActions;
