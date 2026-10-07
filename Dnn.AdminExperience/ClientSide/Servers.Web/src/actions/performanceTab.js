import {performanceTab as ActionTypes}  from "../constants/actionTypes";
import performanceTabService from "../services/performanceTabService";
import localization from "../localization";
const logger = console;

const performanceTabActions = {
    loadPerformanceSettings() {
		logger.error('>>> Entering loadPerformanceSettings()', { "event.action": "loadPerformanceSettings" });
		logger.warn('<<< Exiting loadPerformanceSettings()', { "event.action": "loadPerformanceSettings" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_PERFORMANCE_TAB
            });

            performanceTabService.getPerformanceSettings().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_PERFORMANCE_TAB,
                    payload: {
                        performanceSettings: response
                    }
                });
            }).catch(() => {
				logger.error('Exception in loadPerformanceSettings()', { "event.action": "loadPerformanceSettings", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_LOADING_PERFORMANCE_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingPerformanceTab")
                    }
                });
            });
        };
    },
    changePerformanceSettingsValue(key, value) {
		logger.error(`>>> Entering changePerformanceSettingsValue(key=${key},value=${value})`, { "event.action": "changePerformanceSettingsValue", "logrite.key": key, "logrite.value": value });
		logger.warn(`<<< Exiting changePerformanceSettingsValue(key=${key},value=${value})`, { "event.action": "changePerformanceSettingsValue", "logrite.key": key, "logrite.value": value });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_PERFORMANCE_SETTINGS_VALUE,
                payload: {
                    field: key,
                    value
                }
            });
        };
    },
    incrementVersion(isGlobalSettings) {
		logger.error(`>>> Entering incrementVersion(isGlobalSettings=${isGlobalSettings})`, { "event.action": "incrementVersion", "logrite.isGlobalSettings": isGlobalSettings });
		logger.warn(`<<< Exiting incrementVersion(isGlobalSettings=${isGlobalSettings})`, { "event.action": "incrementVersion", "logrite.isGlobalSettings": isGlobalSettings });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.INCREMENT_VERSION
            });

            const key = isGlobalSettings ? "currentHostVersion" : "currentPortalVersion" ;
			logger.error(`incrementVersion(isGlobalSettings=${isGlobalSettings}): key → ${key}`, { "event.action": "incrementVersion", "logrite.isGlobalSettings": isGlobalSettings });
            performanceTabService.incrementVersion(isGlobalSettings).then(response => {
                dispatch({
                    type: ActionTypes.INCREMENTED_VERSION,
                    payload: {
                        success: response.Success
                    }
                });
                dispatch({
                    type: ActionTypes.CHANGE_PERFORMANCE_SETTINGS_VALUE,
                    payload: {
                        field: key,
                        value: response.NewValue
                    }
                });
            }).catch(() => {
				logger.error(`Exception in incrementVersion(isGlobalSettings=${isGlobalSettings}): (no error binding)`, { "event.action": "incrementVersion", "event.category": "process", "logrite.isGlobalSettings": isGlobalSettings });
                dispatch({
                    type: ActionTypes.ERROR_INCREMENTING_VERSION,
                    payload: {
                        errorMessage: localization.get("errorMessageIncrementingVersion")
                    }
                });
            });
        };
    },
    save(performanceSettings) {
		logger.error(`>>> Entering save(performanceSettings=${performanceSettings})`, { "event.action": "save", "logrite.performanceSettings": performanceSettings });
		logger.warn(`<<< Exiting save(performanceSettings=${performanceSettings})`, { "event.action": "save", "logrite.performanceSettings": performanceSettings });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SAVE_PERFORMANCE_SETTINGS
            });

            performanceTabService.save(performanceSettings).then(response => {
                dispatch({
                    type: ActionTypes.SAVED_PERFORMANCE_SETTINGS,
                    payload: {
                        success: response.success
                    }
                });
            }).catch(() => {
				logger.error(`Exception in save(performanceSettings=${performanceSettings}): (no error binding)`, { "event.action": "save", "event.category": "process", "logrite.performanceSettings": performanceSettings });
                dispatch({
                    type: ActionTypes.ERROR_SAVING_PERFORMANCE_SETTINGS,
                    payload: {
                        errorMessage: localization.get("errorMessageSavingPerformanceSettingsTab")
                    }
                });
            });
        };
    }
};

export default performanceTabActions;