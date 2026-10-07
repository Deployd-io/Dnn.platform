import {log as ActionTypes}  from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const logger = console;

const logActions = {
    getLogList(searchParameters, callback) {
		logger.error(`>>> Entering getLogList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getLogList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting getLogList(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "getLogList", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
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
		logger.error(`>>> Entering getPortalList(addAll=${addAll},callback=${callback})`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPortalList(addAll=${addAll},callback=${callback})`, { "event.action": "getPortalList", "logrite.addAll": addAll, "logrite.callback": callback });
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
		logger.error(`>>> Entering getLogTypeList(callback=${callback})`, { "event.action": "getLogTypeList", "logrite.callback": callback });
		logger.warn(`<<< Exiting getLogTypeList(callback=${callback})`, { "event.action": "getLogTypeList", "logrite.callback": callback });
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
		logger.error(`>>> Entering deleteLogItems(payload=${payload},callback=${callback})`, { "event.action": "deleteLogItems", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting deleteLogItems(payload=${payload},callback=${callback})`, { "event.action": "deleteLogItems", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering clearLog(callback=${callback})`, { "event.action": "clearLog", "logrite.callback": callback });
		logger.warn(`<<< Exiting clearLog(callback=${callback})`, { "event.action": "clearLog", "logrite.callback": callback });
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
		logger.error(`>>> Entering emailLogItems(payload=${payload},callback=${callback})`, { "event.action": "emailLogItems", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting emailLogItems(payload=${payload},callback=${callback})`, { "event.action": "emailLogItems", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering selectRow(rowId=${rowId})`, { "event.action": "selectRow", "logrite.rowId": rowId });
		logger.warn(`<<< Exiting selectRow(rowId=${rowId})`, { "event.action": "selectRow", "logrite.rowId": rowId });
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
		logger.error(`>>> Entering deselectRow(rowId=${rowId})`, { "event.action": "deselectRow", "logrite.rowId": rowId });
		logger.warn(`<<< Exiting deselectRow(rowId=${rowId})`, { "event.action": "deselectRow", "logrite.rowId": rowId });
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
		logger.error('>>> Entering selectAll()', { "event.action": "selectAll" });
		logger.warn('<<< Exiting selectAll()', { "event.action": "selectAll" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SELECTED_ALL,
                data: {}
            });
        };
    },
    deselectAll() {
		logger.error('>>> Entering deselectAll()', { "event.action": "deselectAll" });
		logger.warn('<<< Exiting deselectAll()', { "event.action": "deselectAll" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.DESELECTED_ALL,
                data: {}
            });
        };
    }
};

export default logActions;
