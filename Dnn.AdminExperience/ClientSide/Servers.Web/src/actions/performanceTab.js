import {performanceTab as ActionTypes}  from "../constants/actionTypes";
import performanceTabService from "../services/performanceTabService";
import localization from "../localization";
const logger = console;

const performanceTabActions = {
    loadPerformanceSettings() {
		logger.debug('>>> Entering loadPerformanceSettings()');
		logger.debug('<<< Exiting loadPerformanceSettings()');
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
				logger.error('Exception in loadPerformanceSettings()');
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
		logger.debug(`>>> Entering changePerformanceSettingsValue(key=${key},value=${value})`);
		logger.debug(`<<< Exiting changePerformanceSettingsValue(key=${key},value=${value})`);
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
		logger.debug(`>>> Entering incrementVersion(isGlobalSettings=${isGlobalSettings})`);
		logger.debug(`<<< Exiting incrementVersion(isGlobalSettings=${isGlobalSettings})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.INCREMENT_VERSION
            });

            const key = isGlobalSettings ? "currentHostVersion" : "currentPortalVersion" ;
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
				logger.error(`Exception in incrementVersion(isGlobalSettings=${isGlobalSettings}): (no error binding)`);
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
		logger.debug(`>>> Entering save(performanceSettings=${performanceSettings})`);
		logger.debug(`<<< Exiting save(performanceSettings=${performanceSettings})`);
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
				logger.error(`Exception in save(performanceSettings=${performanceSettings}): (no error binding)`);
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