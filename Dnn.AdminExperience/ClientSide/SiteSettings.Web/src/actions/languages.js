import { languages as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
import LanguageEditorService from "services/languageEditorService";
const logger = console;

const languagesActions = {
    getLanguageSettings(portalId, cultureCode, callback) {
		logger.debug(`>>> Entering getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getLanguageSettings(portalId=${portalId},cultureCode=${cultureCode},callback=${callback})`);
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
		logger.debug(`>>> Entering languageSettingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting languageSettingsClientModified(parameter=${parameter})`);
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
		logger.debug(`>>> Entering updateLanguageSettings(payload=${payload},languageList=${languageList},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateLanguageSettings(payload=${payload},languageList=${languageList},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getLanguages(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getLanguages(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`);
		logger.debug(`<<< Exiting getLanguage(portalId=${portalId},languageId=${languageId},callback=${callback})`);
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
		logger.debug(`>>> Entering getAllLanguages(callback=${callback})`);
		logger.debug(`<<< Exiting getAllLanguages(callback=${callback})`);
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
		logger.debug(`>>> Entering languageClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting languageClientModified(parameter=${parameter})`);
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
		logger.debug('>>> Entering cancelLanguageClientModified()');
		logger.debug('<<< Exiting cancelLanguageClientModified()');
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
		logger.debug(`>>> Entering addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting addLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateLanguage(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateLanguageRoles(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering verifyLanguageResourceFiles(callback=${callback})`);
		logger.debug(`<<< Exiting verifyLanguageResourceFiles(callback=${callback})`);
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
		logger.debug(`>>> Entering getModuleList(type=${type},callback=${callback})`);
		logger.debug(`<<< Exiting getModuleList(type=${type},callback=${callback})`);
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
		logger.debug(`>>> Entering createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting createLanguagePack(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering disableLocalizedContent(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting disableLocalizedContent(portalId=${portalId},callback=${callback})`);
        return () => {
            LanguageEditorService.disableLocalizedContent(portalId, callback);
        };
    },
    deleteLanguagePages(payload, callback) {
		logger.debug(`>>> Entering deleteLanguagePages(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting deleteLanguagePages(payload=${payload},callback=${callback})`);
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
		logger.debug(`>>> Entering publishAllPages(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting publishAllPages(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering enableLocalizedContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting enableLocalizedContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering localizeContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting localizeContent(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getLocalizationProgress(callback=${callback})`);
		logger.debug(`<<< Exiting getLocalizationProgress(callback=${callback})`);
        return () => {
            LanguageEditorService.getLocalizationProgress(data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getRoleGroups(portalId, callback) {
		logger.debug(`>>> Entering getRoleGroups(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getRoleGroups(portalId=${portalId},callback=${callback})`);
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
		logger.debug(`>>> Entering getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`);
		logger.debug(`<<< Exiting getRoles(portalId=${portalId},groupId=${groupId},cultureCode=${cultureCode},callback=${callback})`);
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
		logger.debug(`>>> Entering getPageList(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting getPageList(payload=${payload},callback=${callback})`);
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
		logger.debug(`>>> Entering SelectLanguageRoles(roles=${roles},role=${role},selected=${selected})`);
		logger.debug(`<<< Exiting SelectLanguageRoles(roles=${roles},role=${role},selected=${selected})`);
        return (dispatch) => {
            let list = roles.map((item) => {
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
		logger.debug(`>>> Entering activateLanguage(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting activateLanguage(payload=${payload},callback=${callback})`);
        return () => {
            ApplicationService.activateLanguage(payload, () => {
                if (callback) {
                    callback();
                }
            });
        };
    },
    markAllPagesAsTranslated(payload, callback) {
		logger.debug(`>>> Entering markAllPagesAsTranslated(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting markAllPagesAsTranslated(payload=${payload},callback=${callback})`);
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
