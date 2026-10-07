import {
    portal as ActionTypes
} from "../actionTypes";
import 
{CommonPortalService as PortalService}
    from "../services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const portalActions = {
    getPortalLocales(portalId, callback) {
		logger.error(`>>> Entering getPortalLocales(portalId=${portalId},callback=${callback})`, { "event.action": "getPortalLocales", "logrite.portalId": portalId, "logrite.callback": callback });
		logger.warn(`<<< Exiting getPortalLocales(portalId=${portalId},callback=${callback})`, { "event.action": "getPortalLocales", "logrite.portalId": portalId, "logrite.callback": callback });
        return () => {
            PortalService.getPortalLocales(portalId, data => {
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    exportPortal(payload, callback) {
		logger.error(`>>> Entering exportPortal(payload=${payload},callback=${callback})`, { "event.action": "exportPortal", "logrite.payload": payload, "logrite.callback": callback });
		logger.warn(`<<< Exiting exportPortal(payload=${payload},callback=${callback})`, { "event.action": "exportPortal", "logrite.payload": payload, "logrite.callback": callback });
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
		logger.error(`>>> Entering setPortalBeingExported(portalBeingExported=${portalBeingExported},callback=${callback})`, { "event.action": "setPortalBeingExported", "logrite.portalBeingExported": portalBeingExported, "logrite.callback": callback });
		logger.warn(`<<< Exiting setPortalBeingExported(portalBeingExported=${portalBeingExported},callback=${callback})`, { "event.action": "setPortalBeingExported", "logrite.portalBeingExported": portalBeingExported, "logrite.callback": callback });
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
