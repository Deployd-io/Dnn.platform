import {
    portal as ActionTypes
} from "../actionTypes";
import
{ CommonPortalService as PortalService }
    from "../services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.debug(`>>> Entering errorCallback(message=${message})`);
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.debug(`<<< Exiting errorCallback(message=${message})`);
}
const portalActions = {
    deletePortal(portalId, index, callback) {
		logger.debug(`>>> Entering deletePortal(portalId=${portalId},index=${index},callback=${callback})`);
		logger.debug(`<<< Exiting deletePortal(portalId=${portalId},index=${index},callback=${callback})`);
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
		logger.debug(`>>> Entering getPortalTemplates(callback=${callback})`);
		logger.debug(`<<< Exiting getPortalTemplates(callback=${callback})`);
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
		logger.debug(`>>> Entering createPortal(payload=${payload},success=${success},fail=${fail})`);
		logger.debug(`<<< Exiting createPortal(payload=${payload},success=${success},fail=${fail})`);
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
		logger.debug(`>>> Entering loadPortals(searchParameters=${searchParameters},append=${append},callback=${callback})`);
		logger.debug(`<<< Exiting loadPortals(searchParameters=${searchParameters},append=${append},callback=${callback})`);
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
