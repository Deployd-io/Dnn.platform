import LanguageService from "services/languageService";
import ActionTypes from "../constants/actionTypes/languagesActionTypes";
import utils from "../utils";
const logger = console;

const languagesActions = {
    getLanguages(tabId, callback) {
		logger.debug(`>>> Entering getLanguages(tabId=${tabId},callback=${callback})`);
		logger.debug(`<<< Exiting getLanguages(tabId=${tabId},callback=${callback})`);
        return () => {
            LanguageService.getLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageTranslatable(tabId, callback) {
		logger.debug(`>>> Entering makePageTranslatable(tabId=${tabId},callback=${callback})`);
		logger.debug(`<<< Exiting makePageTranslatable(tabId=${tabId},callback=${callback})`);
        return () => {
            LanguageService.makePageTranslatable(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageNeutral(tabId, callback) {
		logger.debug(`>>> Entering makePageNeutral(tabId=${tabId},callback=${callback})`);
		logger.debug(`<<< Exiting makePageNeutral(tabId=${tabId},callback=${callback})`);
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
		logger.debug(`>>> Entering addMissingLanguages(tabId=${tabId},callback=${callback})`);
		logger.debug(`<<< Exiting addMissingLanguages(tabId=${tabId},callback=${callback})`);
        return () => {
            LanguageService.addMissingLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    notifyTranslators(params, callback) {
		logger.debug(`>>> Entering notifyTranslators(params=${params},callback=${callback})`);
		logger.debug(`<<< Exiting notifyTranslators(params=${params},callback=${callback})`);
        return () => {
            LanguageService.notifyTranslators(params, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateTabLocalization(params, callback, failureCallback) {
		logger.debug(`>>> Entering updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateTabLocalization(params=${params},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting restoreModule(params=${params},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering getContentLocalizationEnabled(callback=${callback})`);
		logger.debug(`<<< Exiting getContentLocalizationEnabled(callback=${callback})`);
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
