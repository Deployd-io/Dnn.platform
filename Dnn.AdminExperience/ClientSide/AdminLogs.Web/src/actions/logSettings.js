import {logSettings as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const logSettingsActions = {
    getKeepMostRecentOptions(callback) {
		logger.debug(`>>> Entering getKeepMostRecentOptions(callback=${callback})`);
		logger.debug(`<<< Exiting getKeepMostRecentOptions(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getKeepMostRecentOptions(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_KEEPMOSTRECENT_OPTIONS,
                    data: {
                        keepMostRecent: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getOccurrenceOptions(callback) {
		logger.debug(`>>> Entering getOccurrenceOptions(callback=${callback})`);
		logger.debug(`<<< Exiting getOccurrenceOptions(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getOccurrenceOptions(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_OCCURRENCE_OPTIONS,
                    data: {
                        thresholds: data.Results.Thresholds,
                        notificationTimes: data.Results.NotificationTimes,
                        notificationTimeTypes: data.Results.NotificationTimeTypes
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLogSettings(callback) {
		logger.debug(`>>> Entering getLogSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getLogSettings(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getLogSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOGSETTING_LIST,
                    data: {
                        logSettingList: data.Results
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLogSettingById(parameters, callback) {
		logger.debug(`>>> Entering getLogSettingById(parameters=${parameters},callback=${callback})`);
		logger.debug(`<<< Exiting getLogSettingById(parameters=${parameters},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getLogSettingById(parameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOGSETTING_BY_ID,
                    data: {
                        logSettingDetail: data
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateLogSetting(payload, callback, failureCallback) {
		logger.debug(`>>> Entering updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.updateLogSetting(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_LOGSETTING,
                    data: {
                        logSettingDetail: data
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
    addLogSetting(payload, callback, failureCallback) {
		logger.debug(`>>> Entering addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.addLogSetting(payload, data => {
                dispatch({
                    type: ActionTypes.ADDED_LOGSETTING,
                    data: {
                        logSettingDetail: data
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
    deleteLogSetting(payload, callback, failureCallback) {
		logger.debug(`>>> Entering deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
        return (dispatch) => {
            ApplicationService.deleteLogSetting(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_LOGSETTING,
                    data: {
                        Success: data.Success,
                        LogSettingId: data.LogSettingId
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
    }
};

export default logSettingsActions;
