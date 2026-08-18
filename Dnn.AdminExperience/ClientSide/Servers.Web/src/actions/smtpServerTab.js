import {smtpServerTab as ActionTypes}  from "../constants/actionTypes";
import smtpServerService from "../services/smtpServerService";
import localization from "../localization";
import Promise from "es6-promise";
const logger = console;

const smtpServeTabActions = {
    loadSmtpServerInfo(callback) {       
		logger.debug(`>>> Entering loadSmtpServerInfo(callback=${callback})`);
		logger.debug(`<<< Exiting loadSmtpServerInfo(callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_SMTP_SERVER_TAB               
            });        
            
            smtpServerService.getSmtpSettings().then(response => {
                Promise.all([dispatch({
                    type: ActionTypes.LOADED_SMTP_SERVER_TAB,
                    payload: {
                        smtpServerInfo: response
                    }
                })]).then(() => {
                    if (typeof callback === "function") {
                        callback();
                    }
                }); 
            }).catch(() => {
				logger.error(`Exception in loadSmtpServerInfo(callback=${callback}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_LOADING_SMTP_SERVER_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingSmtpServerTab")
                    }
                });
            });        
        };
    },
    changeSmtpServerMode(smtpServeMode, callback) {
		logger.debug(`>>> Entering changeSmtpServerMode(smtpServeMode=${smtpServeMode},callback=${callback})`);
		logger.debug(`<<< Exiting changeSmtpServerMode(smtpServeMode=${smtpServeMode},callback=${callback})`);
        return (dispatch) => {
            Promise.all([dispatch({
                type: ActionTypes.CHANGE_SMTP_SERVER_MODE,
                payload: {
                    smtpServeMode
                }
            })]).then(() => {
                if (typeof callback === "function") {
                    callback();
                }
            });
        };
    },
    changeSmtpAuthentication(smtpAuthentication) {
		logger.debug(`>>> Entering changeSmtpAuthentication(smtpAuthentication=${smtpAuthentication})`);
		logger.debug(`<<< Exiting changeSmtpAuthentication(smtpAuthentication=${smtpAuthentication})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_SMTP_AUTHENTICATION,
                payload: {
                    smtpAuthentication
                }
            });
        };
    },
    changeSmtpConfigurationValue(key, value, passCheck) {
		logger.debug(`>>> Entering changeSmtpConfigurationValue(key=${key},value=${value},passCheck=${passCheck})`);
		logger.debug(`<<< Exiting changeSmtpConfigurationValue(key=${key},value=${value},passCheck=${passCheck})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CHANGE_SMTP_CONFIGURATION_VALUE,
                payload: { 
                    field: key,
                    value,
                    passCheck
                }
            });  
        };
    },
    updateSmtpServerSettings(parameters, callback) {       
		logger.debug(`>>> Entering updateSmtpServerSettings(parameters=${parameters},callback=${callback})`);
		logger.debug(`<<< Exiting updateSmtpServerSettings(parameters=${parameters},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.UPDATE_SMTP_SERVER_SETTINGS               
            });        
            
            smtpServerService.updateSmtpSettings(parameters).then(response => {
                let payload = {
                    success: response.success,
                    errors: response.errors,
                    providerChanged: response.providerChanged || false
                };
                Promise.all([dispatch({
                    type: ActionTypes.UPDATED_SMTP_SERVER_SETTINGS,
                    payload: payload
                })]).then(() => {
                    if (typeof callback === "function") {
                        callback(payload);
                    }
                });
            }).catch(() => {
				logger.error(`Exception in updateSmtpServerSettings(parameters=${parameters},callback=${callback}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_UPDATING_SMTP_SERVER_SETTINGS,
                    payload: {
                        errorMessage: localization.get("errorMessageUpdatingSmtpServerTab")
                    }
                });
            });        
        };
    },
    sendTestEmail(parameters) {       
		logger.debug(`>>> Entering sendTestEmail(parameters=${parameters})`);
		logger.debug(`<<< Exiting sendTestEmail(parameters=${parameters})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEND_TEST_EMAIL               
            });        
            
            smtpServerService.sendTestEmail(parameters).then(response => {
                dispatch({
                    type: ActionTypes.SENT_TEST_EMAIL,
                    payload: {
                        success: response.success,
                        infoMessage: response.confirmationMessage,
                        errorMessage: response.errMessage
                    }
                });  
            }).catch((data) => {
				logger.error(`Exception in sendTestEmail(parameters=${parameters}): (no error binding)`);
                let response = JSON.parse(data.responseText);
                dispatch({
                    type: ActionTypes.ERROR_SENDING_TEST_EMAIL,
                    payload: {
                        errorMessage: response.errMessage
                    }
                });
            });        
        };
    },
    loadAuthProviders(callback) {       
		logger.debug(`>>> Entering loadAuthProviders(callback=${callback})`);
		logger.debug(`<<< Exiting loadAuthProviders(callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.LOAD_OAUTH_PROVIDERS               
            });        
            
            smtpServerService.getOAuthProviders().then(response => {
                Promise.all([dispatch({
                    type: ActionTypes.LOADED_OAUTH_PROVIDERS,
                    payload: {
                        authProviders: response,
                        providerChanged: false
                    }
                })]).then(() => {
                    if (typeof callback === "function") {
                        callback();
                    }
                }); 
            }).catch(() => {
				logger.error(`Exception in loadAuthProviders(callback=${callback}): (no error binding)`);
                dispatch({
                    type: ActionTypes.ERROR_LOADING_SMTP_SERVER_TAB,
                    payload: {
                        errorMessage: localization.get("errorMessageLoadingSmtpServerTab")
                    }
                });
            });        
        };
    }
};

export default smtpServeTabActions;