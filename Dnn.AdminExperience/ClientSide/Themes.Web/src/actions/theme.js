import { theme as ActionTypes } from "constants/actionTypes";
import ThemeService from "services/themeService";
import Localization from "localization";
const logger = console;

function errorCallback(xhr) {
	logger.error(`>>> Entering errorCallback(xhr=${xhr})`, { "event.action": "errorCallback", "logrite.xhr": xhr });
    let response = JSON.parse(xhr.responseText);
    let message = xhr.responseText;
	logger.error(`errorCallback(xhr=${xhr}): response → ${response}`, { "event.action": "errorCallback", "logrite.xhr": xhr });
    if (response && response.Message) {
        message = response.Message;
    }

    let utils = window.dnn.initThemes().utility;
    utils.notifyError(Localization.get(message), 5000);
	logger.error(`errorCallback(xhr=${xhr}): utils → ${utils}`, { "event.action": "errorCallback", "logrite.xhr": xhr });
}
const themeActions = {
    getCurrentTheme(callback) {
		logger.error(`>>> Entering getCurrentTheme(callback=${callback})`, { "event.action": "getCurrentTheme", "logrite.callback": callback });
		logger.warn(`<<< Exiting getCurrentTheme(callback=${callback})`, { "event.action": "getCurrentTheme", "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.getCurrentTheme(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CURRENT_THEMES,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getThemes(level, callback) {
		logger.error(`>>> Entering getThemes(level=${level},callback=${callback})`, { "event.action": "getThemes", "logrite.level": level, "logrite.callback": callback });
		logger.warn(`<<< Exiting getThemes(level=${level},callback=${callback})`, { "event.action": "getThemes", "logrite.level": level, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.getThemes(level, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEMES,
                    data: {
                        layouts: data.Layouts,
                        containers: data.Containers
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getCurrentThemeFiles(themeName, themeType, themeLevel, callback) {
		logger.error(`>>> Entering getCurrentThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`, { "event.action": "getCurrentThemeFiles", "logrite.themeName": themeName, "logrite.themeType": themeType, "logrite.themeLevel": themeLevel, "logrite.callback": callback });
		logger.warn(`<<< Exiting getCurrentThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`, { "event.action": "getCurrentThemeFiles", "logrite.themeName": themeName, "logrite.themeType": themeType, "logrite.themeLevel": themeLevel, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.getThemeFiles(themeName, themeType, themeLevel, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_CURRENT_THEMEFILES,
                    data: {
                        themeFiles: data,
                        themeType: themeType
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableThemeFiles(themeName, themeType, themeLevel, callback) {
		logger.error(`>>> Entering getEditableThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`, { "event.action": "getEditableThemeFiles", "logrite.themeName": themeName, "logrite.themeType": themeType, "logrite.themeLevel": themeLevel, "logrite.callback": callback });
		logger.warn(`<<< Exiting getEditableThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`, { "event.action": "getEditableThemeFiles", "logrite.themeName": themeName, "logrite.themeType": themeType, "logrite.themeLevel": themeLevel, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.getThemeFiles(themeName, themeType, themeLevel, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_THEMEFILES,
                    data: {
                        themeFiles: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableTokens(callback) {
		logger.error(`>>> Entering getEditableTokens(callback=${callback})`, { "event.action": "getEditableTokens", "logrite.callback": callback });
		logger.warn(`<<< Exiting getEditableTokens(callback=${callback})`, { "event.action": "getEditableTokens", "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.getEditableTokens(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_TOKENS,
                    data: {
                        tokens: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableSettings(token, callback) {
        return (dispatch) => {
            ThemeService.getEditableSettings(token, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_SETTINGS,
                    data: {
                        settings: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    getEditableValues(token, setting, callback) {
        return (dispatch) => {
            ThemeService.getEditableValues(token, setting, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_EDITABLE_VALUES,
                    data: {
                        values: data.Value
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    applyTheme(themeFile, scope, callback) {
		logger.error(`>>> Entering applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback})`, { "event.action": "applyTheme", "logrite.themeFile": themeFile, "logrite.scope": scope, "logrite.callback": callback });
		logger.warn(`<<< Exiting applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback})`, { "event.action": "applyTheme", "logrite.themeFile": themeFile, "logrite.scope": scope, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.applyTheme(themeFile, scope, data => {
                dispatch({
                    type: ActionTypes.APPLY_THEME,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    updateTheme(path, token, setting, value, callback) {
        return (dispatch) => {
            ThemeService.updateTheme(path, token, setting, value, () => {
                dispatch({
                    type: ActionTypes.UPDATE_THEME,
                    data: {
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    parseTheme(themeName, parseType, callback) {
		logger.error(`>>> Entering parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback})`, { "event.action": "parseTheme", "logrite.themeName": themeName, "logrite.parseType": parseType, "logrite.callback": callback });
		logger.warn(`<<< Exiting parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback})`, { "event.action": "parseTheme", "logrite.themeName": themeName, "logrite.parseType": parseType, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.parseTheme(themeName, parseType, () => {
                dispatch({
                    type: ActionTypes.PARSE_THEME,
                    data: {
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    restoreTheme(callback) {
		logger.error(`>>> Entering restoreTheme(callback=${callback})`, { "event.action": "restoreTheme", "logrite.callback": callback });
		logger.warn(`<<< Exiting restoreTheme(callback=${callback})`, { "event.action": "restoreTheme", "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.restoreTheme(data => {
                dispatch({
                    type: ActionTypes.RESTORE_THEME,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    applyDefaultTheme(themeName, level, callback) {
		logger.error(`>>> Entering applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback})`, { "event.action": "applyDefaultTheme", "logrite.themeName": themeName, "logrite.level": level, "logrite.callback": callback });
		logger.warn(`<<< Exiting applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback})`, { "event.action": "applyDefaultTheme", "logrite.themeName": themeName, "logrite.level": level, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.applyDefaultTheme(themeName, level, data => {
                dispatch({
                    type: ActionTypes.APPLY_DEFAULT_THEME,
                    data: {
                        currentTheme: data
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    },
    deleteTheme(theme, callback) {
		logger.error(`>>> Entering deleteTheme(theme=${theme},callback=${callback})`, { "event.action": "deleteTheme", "logrite.theme": theme, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteTheme(theme=${theme},callback=${callback})`, { "event.action": "deleteTheme", "logrite.theme": theme, "logrite.callback": callback });
        return (dispatch) => {
            ThemeService.deleteTheme(theme, () => {
                dispatch({
                    type: ActionTypes.DELETE_THEME,
                    data: {
                        theme: theme
                    }
                });
                if (callback) {
                    callback();
                }
            }, errorCallback);
        };
    }
};

export default themeActions;
