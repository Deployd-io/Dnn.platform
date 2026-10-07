import {databaseTab as ActionTypes}  from "../constants/actionTypes";
import databaseTabService from "../services/databaseTabService";
import localization from "../localization";
const logger = console;

const databaseTabActions = {
    loadDatabaseServerInfo() {       
		logger.error('>>> Entering loadDatabaseServerInfo()', { "event.action": "loadDatabaseServerInfo" });
		logger.warn('<<< Exiting loadDatabaseServerInfo()', { "event.action": "loadDatabaseServerInfo" });
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_DATABASE_TAB               
            });        
            
            databaseTabService.getDataBaseServerInfo().then(response => {
                dispatch({
                    type: ActionTypes.LOADED_DATABASE_TAB,
                    payload: {
                        databaseServerInfo: response
                    }
                });  
            }).catch(() => {
				logger.error('Exception in loadDatabaseServerInfo()', { "event.action": "loadDatabaseServerInfo", "event.category": "process" });
                dispatch({
                    type: ActionTypes.ERROR_LOADING_DATABASE_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingDatabaseTab")
                    }
                });
            });        
        };
    }
};

export default databaseTabActions;