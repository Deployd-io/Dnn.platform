import {databaseTab as ActionTypes}  from "../constants/actionTypes";
import databaseTabService from "../services/databaseTabService";
import localization from "../localization";
const logger = console;

const databaseTabActions = {
    loadDatabaseServerInfo() {       
		logger.debug('>>> Entering loadDatabaseServerInfo()');
		logger.debug('<<< Exiting loadDatabaseServerInfo()');
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
				logger.error('Exception in loadDatabaseServerInfo()');
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