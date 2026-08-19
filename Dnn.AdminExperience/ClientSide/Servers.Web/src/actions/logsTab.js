import {logsTab as ActionTypes}  from "../constants/actionTypes";
import logsTabService from "../services/logsTabService";
import localization from "../localization";
const logger = console;

const logsTabActions = {
    loadLogsServerInfo() {       
		logger.debug('>>> Entering loadLogsServerInfo()');
		logger.debug('<<< Exiting loadLogsServerInfo()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_LOGS_TAB               
            });        
            
            logsTabService.getLogs().then(logs => {
                dispatch({
                    type: ActionTypes.LOADED_LOGS_TAB,
                    payload: {
                        logs: logs
                    }
                });  
            }).catch(() => {
				logger.error('Exception in loadLogsServerInfo()');
                dispatch({
                    type: ActionTypes.ERROR_LOADING_LOGS_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingLogsTab")
                    }
                });
            });        
        };
    },
       
    loadSelectedLog(log) {
		logger.debug(`>>> Entering loadSelectedLog(log=${log})`);
		logger.debug(`<<< Exiting loadSelectedLog(log=${log})`);
        return (dispatch) => {
            if (!log) {
                return;
            }
            
            const logName = log.name;
            const upgradeLog = log.upgradeLog;
            dispatch({
                type: ActionTypes.LOAD_LOG,
                payload: {
                    log: logName
                }               
            });        
            
            logsTabService.getLog(logName, upgradeLog).then(response => {
                let logResponse = "";
                if (response && typeof response === "string") {
                    logResponse = response;
                }
                
                dispatch({
                    type: ActionTypes.LOADED_LOG,
                    payload: {
                        log: logResponse
                    }
                });  
            }).catch(() => {
				logger.error(`Exception in loadSelectedLog(log=${log}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_LOADING_LOG,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingLog")
                    }
                });
            });        
        };
    }
};

export default logsTabActions;