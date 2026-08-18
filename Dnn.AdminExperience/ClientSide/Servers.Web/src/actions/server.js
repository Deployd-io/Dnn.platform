import {server as ActionTypes}  from "../constants/actionTypes";
import serverService from "../services/serverService";
import localization from "../localization";
const logger = console;

const serverActions = {
    restartApplication() {       
		logger.debug('>>> Entering restartApplication()');
		logger.debug('<<< Exiting restartApplication()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.REQUEST_RESTART_APPLICATION               
            });        
            
            serverService.restartApplication().then(response => {
                dispatch({
                    type: ActionTypes.END_REQUEST_RESTART_APPLICATION,
                    payload: {
                        url: response.url
                    }
                });  
            }).catch(() => {
				logger.error('Exception in restartApplication()');
                dispatch({
                    type: ActionTypes.ERROR_REQUEST_RESTART_APPLICATION,
                    payload: {
                        errorMessage: localization.get("errorMessageRestartingApplication")
                    }
                });
            });        
        };
    },
    clearCache() {       
		logger.debug('>>> Entering clearCache()');
		logger.debug('<<< Exiting clearCache()');
        return (dispatch) => {
            dispatch({
                type: ActionTypes.REQUEST_CLEAR_CACHE               
            });        
            
            serverService.clearCache().then(response => {
                dispatch({
                    type: ActionTypes.END_REQUEST_CLEAR_CACHE,
                    payload: {
                        url: response.url
                    }
                });  
            }).catch(() => {
				logger.error('Exception in clearCache()');
                dispatch({
                    type: ActionTypes.ERROR_REQUEST_CLEAR_CACHE,
                    payload: {
                        errorMessage: localization.get("errorMessageClearingCache")
                    }
                });
            });        
        };
    }
};

export default serverActions;