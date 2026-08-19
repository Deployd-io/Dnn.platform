import { task as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const taskActions = {
    getTaskStatusList(searchParameters, callback) {
		logger.debug(`>>> Entering getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getTaskStatusList(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering startSchedule(callback=${callback})`);
		logger.debug(`<<< Exiting startSchedule(callback=${callback})`);
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
		logger.debug(`>>> Entering stopSchedule(callback=${callback})`);
		logger.debug(`<<< Exiting stopSchedule(callback=${callback})`);
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
		logger.debug(`>>> Entering getSchedulerSettings(callback=${callback})`);
		logger.debug(`<<< Exiting getSchedulerSettings(callback=${callback})`);
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
		logger.debug(`>>> Entering updateSchedulerSettings(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting updateSchedulerSettings(payload=${payload},callback=${callback})`);
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
		logger.debug(`>>> Entering updateSchedulerStatusOnProgress(callback=${callback})`);
		logger.debug(`<<< Exiting updateSchedulerStatusOnProgress(callback=${callback})`);
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
		logger.debug(`>>> Entering getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getScheduleItemHistory(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering getSchedulerItemList(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getSchedulerItemList(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering getServerList(callback=${callback})`);
		logger.debug(`<<< Exiting getServerList(callback=${callback})`);
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
		logger.debug(`>>> Entering getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getGetScheduleItem(searchParameters=${searchParameters},callback=${callback})`);
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
		logger.debug(`>>> Entering deleteSchedule(payload=${payload},schedulerItemList=${schedulerItemList},callback=${callback})`);
		logger.debug(`<<< Exiting deleteSchedule(payload=${payload},schedulerItemList=${schedulerItemList},callback=${callback})`);
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
		logger.debug(`>>> Entering createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting createScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
		logger.debug(`<<< Exiting updateScheduleItem(payload=${payload},callback=${callback},failureCallback=${failureCallback})`);
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
		logger.debug(`>>> Entering runScheduleItem(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting runScheduleItem(payload=${payload},callback=${callback})`);
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
		logger.debug(`>>> Entering settingsClientModified(parameter=${parameter})`);
		logger.debug(`<<< Exiting settingsClientModified(parameter=${parameter})`);
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
		logger.debug('>>> Entering cancelSettingsClientModified()');
		logger.debug('<<< Exiting cancelSettingsClientModified()');
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
