import {
    portal as ActionTypes
} from "../actionTypes";
import
{ CommonPortalService as PortalService }
    from "../services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const portalActions = {
    deletePortal(portalId, index, callback) {
		logger.error(`>>> Entering deletePortal(portalId=${portalId},index=${index},callback=${callback})`, { "event.action": "deletePortal", "logrite.portalId": portalId, "logrite.index": index, "logrite.callback": callback });
		logger.warn(`<<< Exiting deletePortal(portalId=${portalId},index=${index},callback=${callback})`, { "event.action": "deletePortal", "logrite.portalId": portalId, "logrite.index": index, "logrite.callback": callback });
        return (dispatch) => {
            PortalService.deletePortal(portalId, data => {
                dispatch({
                    type: ActionTypes.DELETED_PORTAL,
                    payload: {
                        index,
                        portalId
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    getPortalTemplates(callback) {
		logger.error(`>>> Entering getPortalTemplates(callback=${callback})`, { "event.action": "getPortalTemplates", "logrite.callback": callback });
		logger.warn(`<<< Exiting getPortalTemplates(callback=${callback})`, { "event.action": "getPortalTemplates", "logrite.callback": callback });
        return (dispatch) => {
            PortalService.getPortalTemplates(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTAL_TEMPLATES,
                    payload: {
                        templates: data.Results.Templates,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    },
    createPortal(payload, success, fail) {
		logger.error(`>>> Entering createPortal(payload=${payload},success=${success},fail=${fail})`, { "event.action": "createPortal", "logrite.payload": payload, "logrite.success": success, "logrite.fail": fail });
		logger.warn(`<<< Exiting createPortal(payload=${payload},success=${success},fail=${fail})`, { "event.action": "createPortal", "logrite.payload": payload, "logrite.success": success, "logrite.fail": fail });
        return (dispatch) => {
            PortalService.createPortal(payload, data => {
                dispatch({
                    type: ActionTypes.CREATED_PORTAL,
                    payload: {
                        Portal: data.Portal,
                        ErrorMessage: data.ErrorMessage
                    }
                });
                if (success) {
                    success(data);
                }
            }, (message) => {
                errorCallback(message);
                if (fail) {
                    fail(message);
                }
            });
        };
    },
    loadPortals(searchParameters, append, callback) {
		logger.error(`>>> Entering loadPortals(searchParameters=${searchParameters},append=${append},callback=${callback})`, { "event.action": "loadPortals", "logrite.searchParameters": searchParameters, "logrite.append": append, "logrite.callback": callback });
		logger.warn(`<<< Exiting loadPortals(searchParameters=${searchParameters},append=${append},callback=${callback})`, { "event.action": "loadPortals", "logrite.searchParameters": searchParameters, "logrite.append": append, "logrite.callback": callback });
        return (dispatch) => {
            PortalService.getPortals(searchParameters, data => {
                if (append) {
                    dispatch({
                        type: ActionTypes.RETRIEVED_PORTALS_CONCAT,
                        payload: {
                            portals: data.Results,
                            totalCount: data.TotalResults
                        }
                    });
                } else {
                    dispatch({
                        type: ActionTypes.RETRIEVED_PORTALS,
                        payload: {
                            portals: data.Results,
                            totalCount: data.TotalResults
                        }
                    });
                }
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default portalActions;
