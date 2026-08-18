import {log as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const logActions = {
    getLogList(searchParameters, callback) {
		logger.debug(`>>> Entering getLogList(searchParameters=${searchParameters},callback=${callback})`);
		logger.debug(`<<< Exiting getLogList(searchParameters=${searchParameters},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getLogList(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOG_LIST,
                    data: {
                        logList: data.Results,
                        selectedRowIds: [],
                        excludedRowIds: data.Results.map((row) => row.LogGUID),
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getPortalList(addAll, callback) {
		logger.debug(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`);
		logger.debug(`<<< Exiting getPortalList(addAll=${addAll},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getPortalList(addAll, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_LIST,
                    data: {
                        portalList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    getLogTypeList(callback) {
		logger.debug(`>>> Entering getLogTypeList(callback=${callback})`);
		logger.debug(`<<< Exiting getLogTypeList(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.getLogTypes(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_LOGTYPE_LIST,
                    data: {
                        logTypeList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    deleteLogItems(payload, callback) {
		logger.debug(`>>> Entering deleteLogItems(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting deleteLogItems(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            ApplicationService.deleteLogItems(payload, data => {
                dispatch({
                    type: ActionTypes.DELETED_LOG_ITEMS,
                    data: {
                        selectedRowIds: [],
                        excludedRowIds: []
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearLog(callback) {
		logger.debug(`>>> Entering clearLog(callback=${callback})`);
		logger.debug(`<<< Exiting clearLog(callback=${callback})`);
        return (dispatch) => {
            ApplicationService.clearLog(data => {
                dispatch({
                    type: ActionTypes.CLEARED_LOG,
                    data: {
                        selectedRowIds: [],
                        excludedRowIds: []
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    emailLogItems(payload, callback) {
		logger.debug(`>>> Entering emailLogItems(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting emailLogItems(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            payload.Email = payload.Email.split(/[ ,;]+/).filter((v) => { return v.trim().length > 0; }).join();
            ApplicationService.emailLogItems(payload, data => {
                dispatch({
                    type: ActionTypes.EMAILED_LOG_ITEMS,
                    data: {
                        logList: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    selectRow(rowId) {
		logger.debug(`>>> Entering selectRow(rowId=${rowId})`);
		logger.debug(`<<< Exiting selectRow(rowId=${rowId})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_ROW,
                data: {
                    rowId: rowId
                }
            });
        };
    },
    deselectRow(rowId) {
		logger.debug(`>>> Entering deselectRow(rowId=${rowId})`);
		logger.debug(`<<< Exiting deselectRow(rowId=${rowId})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DESELECTED_ROW,
                data: {
                    rowId: rowId
                }
            });
        };
    },
    selectAll() {
		logger.debug('>>> Entering selectAll()');
		logger.debug('<<< Exiting selectAll()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_ALL,
                data: {}
            });
        };
    },
    deselectAll() {
		logger.debug('>>> Entering deselectAll()');
		logger.debug('<<< Exiting deselectAll()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DESELECTED_ALL,
                data: {}
            });
        };
    }
};

export default logActions;
