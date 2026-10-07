import LanguageService from "services/languageService";
import ActionTypes from "../constants/actionTypes/languagesActionTypes";
import utils from "../utils";
const logger = console;

const languagesActions = {
    getLanguages(tabId, callback) {
		logger.error(`>>> Entering getLanguages(tabId=${tabId},callback=${callback})`, { "event.action": "getLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getLanguages(tabId=${tabId},callback=${callback})`, { "event.action": "getLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
        return () => {
            LanguageService.getLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageTranslatable(tabId, callback) {
		logger.error(`>>> Entering makePageTranslatable(tabId=${tabId},callback=${callback})`, { "event.action": "makePageTranslatable", "logrite.tabId": tabId, "logrite.callback": callback });
		logger.warn(`<<< Exiting makePageTranslatable(tabId=${tabId},callback=${callback})`, { "event.action": "makePageTranslatable", "logrite.tabId": tabId, "logrite.callback": callback });
        return () => {
            LanguageService.makePageTranslatable(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageNeutral(tabId, callback) {
		logger.error(`>>> Entering makePageNeutral(tabId=${tabId},callback=${callback})`, { "event.action": "makePageNeutral", "logrite.tabId": tabId, "logrite.callback": callback });
		logger.warn(`<<< Exiting makePageNeutral(tabId=${tabId},callback=${callback})`, { "event.action": "makePageNeutral", "logrite.tabId": tabId, "logrite.callback": callback });
        return () => {
            LanguageService.makePageNeutral(tabId, data => {
                if (callback) {
                    callback(data);
                }
            }, data => {
                const Message = JSON.parse(data.responseText).Message;
                utils.notifyError(Message, 4000);
            });
        };
    },
    addMissingLanguages(tabId, callback) {
		logger.error(`>>> Entering addMissingLanguages(tabId=${tabId},callback=${callback})`, { "event.action": "addMissingLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
		logger.warn(`<<< Exiting addMissingLanguages(tabId=${tabId},callback=${callback})`, { "event.action": "addMissingLanguages", "logrite.tabId": tabId, "logrite.callback": callback });
        return () => {
            LanguageService.addMissingLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    notifyTranslators(params, callback) {
		logger.error(`>>> Entering notifyTranslators(params=${params},callback=${callback})`, { "event.action": "notifyTranslators", "logrite.params": params, "logrite.callback": callback });
		logger.warn(`<<< Exiting notifyTranslators(params=${params},callback=${callback})`, { "event.action": "notifyTranslators", "logrite.params": params, "logrite.callback": callback });
        return () => {
            LanguageService.notifyTranslators(params, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateTabLocalization(params, callback, failureCallback) {
		logger.error(`>>> Entering updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateTabLocalization", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateTabLocalization", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return () => {
            LanguageService.updateTabLocalization(params, data => {
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
    deleteModule(params, callback, failureCallback) {
		logger.error(`>>> Entering deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return () => {
            LanguageService.deleteModule(params, data => {
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
    restoreModule(params, callback, failureCallback) {
		logger.error(`>>> Entering restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "restoreModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "restoreModule", "logrite.params": params, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return () => {
            LanguageService.restoreModule(params, data => {
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
    getContentLocalizationEnabled(callback) {
		logger.error(`>>> Entering getContentLocalizationEnabled(callback=${callback})`, { "event.action": "getContentLocalizationEnabled", "logrite.callback": callback });
		logger.warn(`<<< Exiting getContentLocalizationEnabled(callback=${callback})`, { "event.action": "getContentLocalizationEnabled", "logrite.callback": callback });
        return (dispatch) => {
            LanguageService.getContentLocalizationEnabled(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SITESETTINGS_LANGUAGE_SETTINGS,
                    data
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    }
};

export default languagesActions;
