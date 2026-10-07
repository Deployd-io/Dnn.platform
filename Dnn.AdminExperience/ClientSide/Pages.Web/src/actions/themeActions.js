import ActionTypes from "../constants/actionTypes/themeActionTypes";
import ThemeService from "../services/themeService";
const logger = console;

const themeActions = {
    retrieveThemes() {
		logger.error('>>> Entering retrieveThemes()', { "event.action": "retrieveThemes" });
		logger.warn('<<< Exiting retrieveThemes()', { "event.action": "retrieveThemes" });
        return (dispatch) => new Promise(function (callback) {
            dispatch({
                type: ActionTypes.RETRIEVING_THEMES
            });    

            ThemeService.getThemes().then(response => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEMES,
                    data: {
                        themes: response.themes,
                        defaultPortalThemeName: response.defaultPortalThemeName,
                        defaultPortalThemeLevel: response.defaultPortalThemeLevel,
                        defaultPortalLayout: response.defaultPortalLayout,
                        defaultPortalContainer: response.defaultPortalContainer
                    }
                });
                callback({success: true});
            }).catch((error) => {
				logger.error('Exception in retrieveThemes()', { "event.action": "retrieveThemes", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_RETRIEVING_THEMES,
                    data: {error}
                });
                callback({success: false});
            });     
        });
    },

    retrieveThemeFiles(themeName, level) {
		logger.error(`>>> Entering retrieveThemeFiles(themeName=${themeName},level=${level})`, { "event.action": "retrieveThemeFiles", "logrite.themeName": themeName, "logrite.level": level });
		logger.warn(`<<< Exiting retrieveThemeFiles(themeName=${themeName},level=${level})`, { "event.action": "retrieveThemeFiles", "logrite.themeName": themeName, "logrite.level": level });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.RETRIEVING_THEME_FILES
            });    

            ThemeService.getThemeFiles(themeName, level).then(response => {
                dispatch({
                    type: ActionTypes.RETRIEVED_THEME_FILES,
                    data: {
                        layouts: response.layouts,
                        containers: response.containers
                    }
                });  
            }).catch((error) => {
				logger.error(`Exception in retrieveThemeFiles(themeName=${themeName},level=${level}): (no error binding)`, { "event.action": "retrieveThemeFiles", "event.category": "process", "logrite.themeName": themeName, "logrite.level": level });
                dispatch({
                    type: ActionTypes.ERROR_RETRIEVING_THEME_FILES,
                    data: {error}
                });
            });     
        };
    }
};
export default themeActions;