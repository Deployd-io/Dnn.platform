import {
    portal as ActionTypes
} from "../actionTypes";
import 
{CommonPortalService as PortalService}
    from "../services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const portalActions = {
    getPortalLocales(portalId, callback) {
		logger.debug(`>>> Entering getPortalLocales(portalId=${portalId},callback=${callback})`);
		logger.debug(`<<< Exiting getPortalLocales(portalId=${portalId},callback=${callback})`);
        return () => {
            PortalService.getPortalLocales(portalId, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    exportPortal(payload, callback) {
		logger.debug(`>>> Entering exportPortal(payload=${payload},callback=${callback})`);
		logger.debug(`<<< Exiting exportPortal(payload=${payload},callback=${callback})`);
        return (dispatch) => {
            PortalService.exportPortal(payload, data => {
                dispatch({
                    type: ActionTypes.EXPORTED_PORTAL_TEMPLATE,
                    payload: {
                        Message: data.Message,
                        Template: data.Template
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    setPortalBeingExported(portalBeingExported, callback) {
		logger.debug(`>>> Entering setPortalBeingExported(portalBeingExported=${portalBeingExported},callback=${callback})`);
		logger.debug(`<<< Exiting setPortalBeingExported(portalBeingExported=${portalBeingExported},callback=${callback})`);
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SET_PORTAL_BEING_EXPORTED,
                payload: portalBeingExported
            });
            utilities.throttleExecution(callback);
        };
    }
};

export default portalActions;
