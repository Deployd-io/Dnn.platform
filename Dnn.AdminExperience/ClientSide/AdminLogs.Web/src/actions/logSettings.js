import {logSettings as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const logSettingsActions = {
    getKeepMostRecentOptions(callback) {
		logger.error(`>>> Entering getKeepMostRecentOptions(callback=${callback})`, { "event.action": "getKeepMostRecentOptions", "logrite.callback": callback });
		logger.warn(`<<< Exiting getKeepMostRecentOptions(callback=${callback})`, { "event.action": "getKeepMostRecentOptions", "logrite.callback": callback });
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
		logger.error(`>>> Entering getOccurrenceOptions(callback=${callback})`, { "event.action": "getOccurrenceOptions", "logrite.callback": callback });
		logger.warn(`<<< Exiting getOccurrenceOptions(callback=${callback})`, { "event.action": "getOccurrenceOptions", "logrite.callback": callback });
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
		logger.error(`>>> Entering getLogSettings(callback=${callback})`, { "event.action": "getLogSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLogSettings(callback=${callback})`, { "event.action": "getLogSettings", "logrite.callback": callback });
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
		logger.error(`>>> Entering getLogSettingById(parameters=${parameters},callback=${callback})`, { "event.action": "getLogSettingById", "logrite.parameters": parameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getLogSettingById(parameters=${parameters},callback=${callback})`, { "event.action": "getLogSettingById", "logrite.parameters": parameters, "logrite.callback": callback });
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
		logger.error(`>>> Entering updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting addLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "addLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
		logger.error(`>>> Entering deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting deleteLogSetting(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "deleteLogSetting", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
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
