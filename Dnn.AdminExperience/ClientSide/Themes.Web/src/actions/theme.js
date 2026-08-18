import { theme as ActionTypes } from "constants/actionTypes";
import ThemeService from "services/themeService";
import Localization from "localization";
const logger = console;

function errorCallback(xhr) {
	logger.debug(`>>> Entering errorCallback(xhr=${xhr})`);
    let response = JSON.parse(xhr.responseText);
    let message = xhr.responseText;
	logger.debug(`errorCallback(xhr=${xhr}): response → ${response}`);
    if (response && response.Message) {
        message = response.Message;
    }

    let utils = window.dnn.initThemes().utility;
    utils.notifyError(Localization.get(message), 5000);
	logger.debug(`errorCallback(xhr=${xhr}): utils → ${utils}`);
	logger.debug(`<<< Exiting errorCallback(xhr=${xhr})`);
}
const themeActions = {
    getCurrentTheme(callback) {
		logger.debug(`>>> Entering getCurrentTheme(callback=${callback})`);
		logger.debug(`<<< Exiting getCurrentTheme(callback=${callback})`);
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
		logger.debug(`>>> Entering getThemes(level=${level},callback=${callback})`);
		logger.debug(`<<< Exiting getThemes(level=${level},callback=${callback})`);
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
		logger.debug(`>>> Entering getCurrentThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`);
		logger.debug(`<<< Exiting getCurrentThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`);
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
		logger.debug(`>>> Entering getEditableThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`);
		logger.debug(`<<< Exiting getEditableThemeFiles(themeName=${themeName},themeType=${themeType},themeLevel=${themeLevel},callback=${callback})`);
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
		logger.debug(`>>> Entering getEditableTokens(callback=${callback})`);
		logger.debug(`<<< Exiting getEditableTokens(callback=${callback})`);
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
		logger.debug(`>>> Entering applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback})`);
		logger.debug(`<<< Exiting applyTheme(themeFile=${themeFile},scope=${scope},callback=${callback})`);
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
		logger.debug(`>>> Entering parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback})`);
		logger.debug(`<<< Exiting parseTheme(themeName=${themeName},parseType=${parseType},callback=${callback})`);
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
		logger.debug(`>>> Entering restoreTheme(callback=${callback})`);
		logger.debug(`<<< Exiting restoreTheme(callback=${callback})`);
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
		logger.debug(`>>> Entering applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback})`);
		logger.debug(`<<< Exiting applyDefaultTheme(themeName=${themeName},level=${level},callback=${callback})`);
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
		logger.debug(`>>> Entering deleteTheme(theme=${theme},callback=${callback})`);
		logger.debug(`<<< Exiting deleteTheme(theme=${theme},callback=${callback})`);
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
