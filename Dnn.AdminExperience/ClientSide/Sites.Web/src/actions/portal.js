import {
    portal as ActionTypes
} from "dnn-sites-common-action-types";
import {
    PortalService
} from "services";
import utilities from "utils";
const logger = console;

function errorCallback(message) {
	logger.error(`>>> Entering errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
    utilities.notifyError(JSON.parse(message.responseText).Message, 5000);
	logger.warn(`<<< Exiting errorCallback(message=${message})`, { "event.action": "errorCallback", "logrite.message": message });
}
const portalActions = {
    loadPortals(searchParameters, callback) {
		logger.error(`>>> Entering loadPortals(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "loadPortals", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
		logger.warn(`<<< Exiting loadPortals(searchParameters=${searchParameters},callback=${callback})`, { "event.action": "loadPortals", "logrite.searchParameters": searchParameters, "logrite.callback": callback });
        return (dispatch) => {
            PortalService.getPortals(searchParameters, data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_PORTALS,
                    payload: {
                        portals: data.Results,
                        totalCount: data.TotalResults
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, errorCallback);
        };
    }
};

export default portalActions;
