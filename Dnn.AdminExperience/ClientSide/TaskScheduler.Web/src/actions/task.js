import { task as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const taskActions = {
    getTaskStatusList(searchParameters, callback) {
		logger.error(`>>> Entering getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getTaskStatusList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getTaskStatusList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getTaskStatusList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_TASK_STATUS_LIST,
                    data: {
                        serverTime: data.Results.ServerTime,
                        schedulingEnabled: data.Results.SchedulingEnabled,
                        status: data.Results.Status,
                        freeThreads: data.Results.FreeThreadCount,
                        activeThreads: data.Results.ActiveThreadCount,
                        maxThreads: data.Results.MaxThreadCount,
                        taskProcessingList: data.Results.ScheduleProcessing,
                        taskStatusList: data.Results.ScheduleQueue,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    startSchedule(callback) {
		logger.error(`>>> Entering startSchedule(callback=${callback})`, { "event.action": "startSchedule", "logrite.callback": callback });
		logger.warn(`<<< Exiting startSchedule(callback=${callback})`, { "event.action": "startSchedule", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.startSchedule(data => {
                dispatch({
                    type: ActionTypes.STARTED_SCHEDULE,
                    data: {

                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    stopSchedule(callback) {
		logger.error(`>>> Entering stopSchedule(callback=${callback})`, { "event.action": "stopSchedule", "logrite.callback": callback });
		logger.warn(`<<< Exiting stopSchedule(callback=${callback})`, { "event.action": "stopSchedule", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.stopSchedule(data => {
                dispatch({
                    type: ActionTypes.STOPPED_SCHEDULE,
                    data: {

                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSchedulerSettings(callback) {
		logger.error(`>>> Entering getSchedulerSettings(callback=${callback})`, { "event.action": "getSchedulerSettings", "logrite.callback": callback });
		logger.warn(`<<< Exiting getSchedulerSettings(callback=${callback})`, { "event.action": "getSchedulerSettings", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getSchedulerSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SCHEDULE_SETTINGS,
                    data: {
                        schedulerModeOptions: data.Results.SchedulerModeOptions,
                        schedulerMode: data.Results.SchedulerMode,
                        schedulerDelay: data.Results.SchedulerdelayAtAppStart
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSchedulerSettings(payload, callback) {
		logger.error(`>>> Entering updateSchedulerSettings(payload=${payload},callback=${callback})`, { "event.action": "updateSchedulerSettings", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting updateSchedulerSettings(payload=${payload},callback=${callback})`, { "event.action": "updateSchedulerSettings", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.updateSchedulerSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SCHEDULE_SETTINGS,
                    data: {
                        schedulerMode: payload.SchedulerMode,
                        schedulerDelay: payload.SchedulerdelayAtAppStart,
                        settingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSchedulerStatusOnProgress(callback) {
		logger.error(`>>> Entering updateSchedulerStatusOnProgress(callback=${callback})`, { "event.action": "updateSchedulerStatusOnProgress", "logrite.callback": callback });
		logger.warn(`<<< Exiting updateSchedulerStatusOnProgress(callback=${callback})`, { "event.action": "updateSchedulerStatusOnProgress", "logrite.callback": callback });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATED_SCHEDULE_SETTINGS_PENDING,
                data: {
                    schedulerMode: "Pending"
                }
            });
            if (callback) {
                callback();
            }
        };
    },
    getScheduleItemHistory(searchParameters, callback) {
		logger.error(`>>> Entering getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getScheduleItemHistory", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getScheduleItemHistory", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getScheduleItemHistory(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SCHEDULE_HISTORY,
                    data: {
                        taskHistoryList: data.Results,
                        totalHistoryCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getSchedulerItemList(searchParameters, callback) {
		logger.error(`>>> Entering getSchedulerItemList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getSchedulerItemList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getSchedulerItemList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getSchedulerItemList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getScheduleItems(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SCHEDULE_ITEMS,
                    data: {
                        schedulerItemList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getServerList(callback) {
		logger.error(`>>> Entering getServerList(callback=${callback})`, { "event.action": "getServerList", "logrite.callback": callback });
		logger.warn(`<<< Exiting getServerList(callback=${callback})`, { "event.action": "getServerList", "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getServerList(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SERVER_LIST,
                    data: {
                        serverList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getGetScheduleItem(searchParameters, callback) {
		logger.error(`>>> Entering getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getGetScheduleItem", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getGetScheduleItem", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.getGetScheduleItem(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SCHEDULE_ITEM,
                    data: {
                        scheduleItemDetail: data.Results,
                        totalCount: data.TotalResults,
                        settingsClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteSchedule(payload, schedulerItemList, callback) {
		logger.error(`>>> Entering deleteSchedule(payload=${payload},schedulerItemList=${schedulerItemList},callback=${callback})`, { "event.action": "deleteSchedule", "logrite.payload": payload, "logrite.schedulerItemList": schedulerItemList, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteSchedule(payload=${payload},schedulerItemList=${schedulerItemList},callback=${callback})`, { "event.action": "deleteSchedule", "logrite.payload": payload, "logrite.schedulerItemList": schedulerItemList, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.deleteScheduleItem(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_SCHEDULE_ITEM,
                    data: {
                        schedulerItemList: schedulerItemList,
                        totalCount: schedulerItemList.length
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    createScheduleItem(payload, callback, failureCallback) {
		logger.error(`>>> Entering createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "createScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.createScheduleItem(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_SCHEDULE_ITEM,
                    data: {
                        Success: data.Success,
                        settingsClientModified: false
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
    updateScheduleItem(payload, callback, failureCallback) {
		logger.error(`>>> Entering updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
		logger.warn(`<<< Exiting updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`, { "event.action": "updateScheduleItem", "logrite.payload": payload, "logrite.callback": callback, "logrite.failureCallback": failureCallback });
        return (dispatch) => {
            ApplicationService.updateScheduleItem(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SCHEDULE_ITEM,
                    data: {
                        Success: data.Success,
                        settingsClientModified: false
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
    runScheduleItem(payload, callback) {
		logger.error(`>>> Entering runScheduleItem(payload=${payload},callback=${callback})`, { "event.action": "runScheduleItem", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting runScheduleItem(payload=${payload},callback=${callback})`, { "event.action": "runScheduleItem", "logrite.payload": payload, "logrite.callback": callback });
        return (dispatch) => {
            ApplicationService.runScheduleItem(payload, data => {
                dispatch({
                    type: ActionTypes.EXECUTED_SCHEDULE_ITEM,
                    data: {
                        Success: data.Success
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    settingsClientModified(parameter) {
		logger.error(`>>> Entering settingsClientModified(parameter=${parameter})`, { "event.action": "settingsClientModified", "logrite.parameter": parameter });
		logger.warn(`<<< Exiting settingsClientModified(parameter=${parameter})`, { "event.action": "settingsClientModified", "logrite.parameter": parameter });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SCHEDULE_ITEM_SETTINS_CLIENT_MODIFIED,
                data: {
                    scheduleItemDetail: parameter,
                    settingsClientModified: true
                }
            });
        };
    },
    cancelSettingsClientModified() {
		logger.error('>>> Entering cancelSettingsClientModified()', { "event.action": "cancelSettingsClientModified" });
		logger.warn('<<< Exiting cancelSettingsClientModified()', { "event.action": "cancelSettingsClientModified" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CANCELED_SCHEDULE_ITEM_SETTINS_CLIENT_MODIFIED,
                data: {
                    settingsClientModified: false
                }
            });
        };
    }
};

export default taskActions;
