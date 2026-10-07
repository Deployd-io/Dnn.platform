import {webTab as ActionTypes}  from "../constants/actionTypes";
import webTabService from "../services/webTabService";
import localization from "../localization";
const logger = console;

const webTabActions = {
    loadWebServerInfo() {       
		logger.error('>>> Entering loadWebServerInfo()', { "event.action": "loadWebServerInfo" });
		logger.warn('<<< Exiting loadWebServerInfo()', { "event.action": "loadWebServerInfo" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_WEB_TAB               
            });        
            
            webTabService.getWebServerInfo().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_WEB_TAB,
                    payload: {
                        webServerInfo: response
                    }
                });  
            }).catch(() => {
				logger.error('Exception in loadWebServerInfo()', { "event.action": "loadWebServerInfo", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_LOADING_WEB_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingWebTab")
                    }
                });
            });        
        };
    }
};

export default webTabActions;